//#define MACRODEBUG

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SharpLisp
{
	public class Environment
	{
		int _functionCounter = 0;
		int _closureCounter = 0;
		int _functionCallScopeCounter = 0;
		int _letScopeCounter = 0;
		int _macroScopeCounter = 0;

		Scope _globalScope;
		Dictionary<string, SharpList> _macroDefinitions = new Dictionary<string, SharpList>();

		public Environment ()
		{
			_globalScope = new Scope ("Global", null);

			_globalScope.vars ["eval"] = new SharpFunction (InternalEval, "eval");

			_globalScope.vars ["print"] = new SharpFunction (BuiltInFunctions.VariadicPrint, "print");

			_globalScope.vars ["+"] = new SharpFunction (BuiltInFunctions.VariadicAdd, "+");
			_globalScope.vars ["*"] = new SharpFunction (BuiltInFunctions.VariadicMultiplication, "*");
			_globalScope.vars ["-"] = new SharpFunction (BuiltInFunctions.VariadicSubtraction, "-");
			_globalScope.vars ["/"] = new SharpFunction (BuiltInFunctions.VariadicDivision, "/");
			_globalScope.vars ["="] = new SharpFunction (BuiltInFunctions.VariadicEqual, "=");
			_globalScope.vars [">"] = new SharpFunction (BuiltInFunctions.VariadicCheck((a, b) => a > b), ">");
			_globalScope.vars ["<"] = new SharpFunction (BuiltInFunctions.VariadicCheck((a, b) => a < b), "<");
			_globalScope.vars ["<="] = new SharpFunction (BuiltInFunctions.VariadicCheck((a, b) => a <= b), "<=");
			_globalScope.vars [">="] = new SharpFunction (BuiltInFunctions.VariadicCheck((a, b) => a >= b), ">=");
			_globalScope.vars ["mod"] = new SharpFunction (BuiltInFunctions.Modulus, "mod");
			_globalScope.vars ["."] = new SharpFunction (BuiltInFunctions.Interop, ".");

			_globalScope.vars ["empty?"] = new SharpFunction (BuiltInFunctions.IsEmpty, "empty?");
			_globalScope.vars ["nth"] = new SharpFunction (BuiltInFunctions.Nth, "nth");
			_globalScope.vars ["first"] = new SharpFunction (BuiltInFunctions.First, "first");
			_globalScope.vars ["rest"] = new SharpFunction (BuiltInFunctions.Rest, "rest");
			_globalScope.vars ["count"] = new SharpFunction (BuiltInFunctions.Count, "count");
			_globalScope.vars ["cons"] = new SharpFunction (BuiltInFunctions.Cons, "cons");
			_globalScope.vars ["list"] = new SharpFunction (BuiltInFunctions.List, "list");

			_globalScope.vars ["load"] = new SharpFunction (LoadFile, "load");
			_globalScope.vars ["type"] = new SharpFunction (BuiltInFunctions.Type, "type");
		}

		private List<object> StringToExpressions(string pString) {
			if (pString == null) {
				throw new Exception("pString is null");
			}
			Lexer lexer = new Lexer ();
			List<Token> tokens = lexer.Read (pString);
			Parser parser = new Parser ();
			return parser.GetRootExpressions (tokens);
		}

		public void ReadEval(string pString, bool pPrint)
		{
			try {
				List<object> rootExpressions = StringToExpressions(pString);

				foreach(object sexp in rootExpressions) {
					object result = Eval (sexp, _globalScope);
					if(pPrint) {
						if(result != null) {
							Console.WriteLine (result);
						} else {
							Console.WriteLine ("null");
						}
					}
				}
			}
			catch(Exception e) {
				Console.WriteLine ("Error: " + e.ToString());
			}
		}

		public object LoadFile(object[] args) {
			BuiltInFunctions.AssertType<string> (args, 0);
			StreamReader sr = File.OpenText ((string)args[0]);
			ReadEval (sr.ReadToEnd(), false);
			sr.Close ();
			return null;
		}

		object InternalEval(object[] args) {
			object lastResult = null;
			foreach(object arg in args) {
				lastResult = Eval (arg, _globalScope);
			}
			return lastResult;
		}

		public object Eval(object o, Scope pCurrentScope) {
			//Console.WriteLine ("Eval: " + o); // + " in " + pCurrentScope.name);

			object result = null;

			if (o is SharpList) {
				result = EvalList (o as SharpList, pCurrentScope);
			} else if (o is SharpVector) {
				result = EvalVector (o as SharpVector, pCurrentScope);
			} else if (o is SymbolToken) {
				result = pCurrentScope.TryResolveSymbol (o as SymbolToken);
			} else {
				result = o; // things that resolve to themselves
			}

			return result;
		}

		private object EvalList(SharpList pList, Scope pCurrentScope) {
			if (pList.Count == 0) {
				return pList;
			}

			if (pList[0] is SymbolToken) {
				//Console.WriteLine ("Found symbol token " + pList[0]);
				if (_macroDefinitions.ContainsKey (((SymbolToken)pList[0]).value)) {
					SymbolToken macroName = pList [0] as SymbolToken;
					object result = CallMacro (_macroDefinitions[macroName.value], pList, pCurrentScope);
					return result;
				}
			}

			object firstItem = GetEvaled (pList, 0, pCurrentScope);

			if (firstItem is ReservedToken) {
				ReservedToken token = firstItem as ReservedToken;

				if (token.type == TokenType.DEF) {
					return Def (pList, pCurrentScope);
				} 

				if (token.type == TokenType.FN) {
					return Fn (pList, pCurrentScope);
				} 

				if (token.type == TokenType.LET) {
					return Let (pList, pCurrentScope);
				}

				if (token.type == TokenType.IF) {
					return If (pList, pCurrentScope);
				}

				if (token.type == TokenType.CONJ) {
					return Conj (pList, pCurrentScope);
				}

				if (token.type == TokenType.DEFMACRO) {
					return DefMacro (pList, pCurrentScope);
				}

				if (token.type == TokenType.QUOTE) {
					return Quote (pList, pCurrentScope);
				}

				if (token.type == TokenType.SET) {
					return Set (pList, pCurrentScope);
				}
	
				throw new Exception("Can't understand ReservedToken: " + token);
			}

			if (firstItem is SharpFunction) {
				return FunctionCall (pList, pCurrentScope);
			} 

			throw new Exception ("Can't eval function with first item " + firstItem);
		}

		object GetEvaled(SharpList pList, int pPosition, Scope pCurrentScope) {
			return Eval(pList [pPosition], pCurrentScope);
		}

		private object EvalVector(SharpVector pVector, Scope pCurrentScope) {
			SharpVector evaledVector = new SharpVector ();

			foreach (var item in pVector) {
				evaledVector.Add (Eval (item, pCurrentScope));
			}

			return evaledVector;
		}

		private object Def(SharpList pList, Scope pCurrentScope) {
			SymbolToken symbolToken = pList [1] as SymbolToken;
			if(symbolToken == null) {
				throw new Exception("The first argument to def is not a symbol");
			}
			_globalScope.SetVar(symbolToken.value, GetEvaled(pList, 2, pCurrentScope));
			return symbolToken;
		}

		private object Let(SharpList pList, Scope pCurrentScope) {
			SharpVector bindingsVector = pList [1] as SharpVector;
			if(bindingsVector == null) {
				throw new Exception("The first argument to let is not a bindings vector");
			}

			Scope letScope = new Scope ("Let" + _letScopeCounter++, pCurrentScope);
#if LOG_SCOPES
			Console.WriteLine ("Created new let scope: " + letScope.name);
#endif

			for (int i = 0; i < bindingsVector.Count; i += 2) {
				SymbolToken symbolToken = bindingsVector [i] as SymbolToken;
				if(symbolToken == null) {
					throw new Exception("The first argument to def is not a symbol");
				}
				letScope.SetVar (symbolToken.value, Eval (bindingsVector[i + 1], pCurrentScope));
			}

			SharpList copy = new SharpList ();
			for (int i = 2; i < pList.Count; i++) {
				copy.Add(pList[i]);
			}

			object lastResult = null;
			foreach(var item in copy) {
				//Console.WriteLine("Time to eval " + item);
				lastResult = Eval (item, letScope);
			}
			return lastResult;
		}

		private object Fn(SharpList pList, Scope pCurrentScope) {
			SharpVector argBindings = pList [1] as SharpVector;
			if(argBindings == null) {
				throw new Exception("The first argument to fn is not a vector of args");
			}

			SharpVector argBindingsDeepCopy = new SharpVector ();
			foreach(var argBinding in argBindings) {
				argBindingsDeepCopy.Add (argBinding);
			}

			SharpList body = new SharpList ();
			for (int i = 2; i < pList.Count; i++) {
				body.Add(pList[i]);
			}

			Scope closure = new Scope ("Closure" + _closureCounter++, pCurrentScope);
#if LOG_SCOPES
			Console.WriteLine ("Created new closure: " + closure.name);
#endif

			string functionName = "Fn" + _functionCounter++;

			return new SharpFunction(args => {

				Scope functionCallScope = new Scope ("FunctionCallScope" + _functionCallScopeCounter++, closure);
#if LOG_SCOPES
				Console.WriteLine ("Created new function call scope: " + functionCallScope.name);
#endif

				int argPos = 0;

				for(int argBindingPos = 0; argBindingPos < argBindingsDeepCopy.Count; argBindingPos++) {
					var argBinding = argBindingsDeepCopy[argBindingPos];

					if(argBinding is ReservedToken && (argBinding as ReservedToken).type == TokenType.AMPERSAND) {
						argBindingPos++;
						SymbolToken finalSymbol = argBindingsDeepCopy[argBindingPos] as SymbolToken;

						if(finalSymbol == null) {
							throw new Exception("Final arg binding after ampersand is not a symbol: " + argBindingsDeepCopy[argBindingPos]);
						}

						var restOfArgs = new SharpList ();
						while (argPos < args.Length) {
							restOfArgs.Add(args[argPos++]);
						}
						functionCallScope.SetVar(finalSymbol.value, restOfArgs);
						break;
					}

					SymbolToken symbol = argBinding as SymbolToken;
					if(symbol == null) {
						throw new Exception("Arg binding is not a symbol: " + symbol);
					}

					functionCallScope.SetVar(symbol.value, args[argPos++]);
				}

//				Console.WriteLine(functionName + " was called with " + args.Length + " arguments:");
//				foreach(var arg in args) {
//					if(arg == null) {
//						Console.WriteLine("null");
//					} else {
//						Console.WriteLine(arg.ToString());
//					}
//				}

				object lastResult = null;
				foreach(var item in body) {
					//Console.WriteLine("Time to eval " + item);
					lastResult = Eval (item, functionCallScope);
				}
				return lastResult;
			}, functionName);
		}

		private object FunctionCall(SharpList pList, Scope pCurrentScope) {

			List<object> evaledArgs = new List<object> ();
			for (int i = 1; i < pList.Count; i++) {
				if(pList[i] != null) {
					evaledArgs.Add(GetEvaled (pList, i, pCurrentScope));
				} else {
					evaledArgs.Add (null);
				}
			}

			SharpFunction f = GetEvaled(pList, 0, pCurrentScope) as SharpFunction;
			object result = f.function (evaledArgs.ToArray());

			//Console.WriteLine ("(" + f + (evaledArgs.Count > 0 ? " " : "") + string.Join(" ", evaledArgs) + ") -> " + result);

			return result;
		}

		private object If(SharpList pList, Scope pCurrentScope) {
			object condition = GetEvaled (pList, 1, pCurrentScope);

			bool isTrue = (condition != null);

			if (condition is bool) {
				isTrue = (bool)condition;
			}

			object result = null;

			if (isTrue) {
				result = GetEvaled (pList, 2, pCurrentScope);
			} else if(pList.Count > 3) {
				result = GetEvaled (pList, 3, pCurrentScope);
			}

			return result;
		}

		private object Conj(SharpList pList, Scope pCurrentScope) {
			SharpVector vector = Eval(pList[1], pCurrentScope) as SharpVector;
			if (vector == null) {
				throw new Exception ("First argument to conj is not a vector");
			}
			SharpVector deepCopy = new SharpVector ();
			foreach (var item in vector) {
				deepCopy.Add (item);
			}
			object itemToInsert = Eval(pList[2], pCurrentScope);
			deepCopy.Add (itemToInsert);
			return deepCopy;
		}

		private object DefMacro(SharpList pList, Scope pCurrentScope) {

			SymbolToken macroNameSymbol = pList [1] as SymbolToken;
			if (macroNameSymbol == null) {
				throw new Exception("The first argument to macro is not a string");
			}

			_macroDefinitions[macroNameSymbol.value] = pList;

			return pList;
		}

		private object CallMacro(SharpList pMacroForm, SharpList pInvokingList, Scope pCurrentScope) {

			SymbolToken macroNameSymbol = pMacroForm [1] as SymbolToken;
			if (macroNameSymbol == null) {
				throw new Exception("The first argument to macro is not a string");
			}

			//Console.WriteLine ("Calling macro " + macroNameSymbol.value);

			var argBindings = pMacroForm [2] as SharpVector;
			if (argBindings == null) {
				throw new Exception("The second argument to macro is not a vector of args");
			}

			List<object> args = new List<object> ();
			for (int i = 1; i < pInvokingList.Count; i++) {
				args.Add (pInvokingList[i]);
			}

			int argPos = 0;

			Scope macroScope = new Scope ("Macro scope " + _macroScopeCounter++, pCurrentScope);

			for(int argBindingPos = 0; argBindingPos < argBindings.Count; argBindingPos++) {

				var argBinding = argBindings [argBindingPos];

				if(argBinding is ReservedToken && (argBinding as ReservedToken).type == TokenType.AMPERSAND) {
					argBindingPos++;
					SymbolToken finalSymbol = argBindings[argBindingPos] as SymbolToken;

					if(finalSymbol == null) {
						throw new Exception("Final arg binding after ampersand is not a symbol: " + argBindings[argBindingPos]);
					}

					var restOfArgs = new SharpList ();
					while (argPos < args.Count) {
						restOfArgs.Add(args[argPos++]);
					}
					macroScope.SetVar(finalSymbol.value, restOfArgs);
					break;
				}

				SymbolToken symbol = argBinding as SymbolToken;
				if (symbol == null) {
					throw new Exception ("One of the bindings to the macro is not a symbol");
				}

				macroScope.SetVar(symbol.value, args[argPos]);
#if MACRODEBUG
				Console.WriteLine ("Setting " + symbol.value + " to " + args[argPos] + " at arg pos " + argPos);
#endif

				argPos++;
			}

			List<object> compiledForms = new List<object> ();

			for(int i = 3; i < pMacroForm.Count; i++) {
				object compiledBody = CompileMacro(pMacroForm [i], macroScope);
				compiledForms.Add (compiledBody);
			}

#if MACRODEBUG
			Console.WriteLine("Compiled macro " + macroNameSymbol.value + ": " + string.Join(",", compiledForms));
#endif

			object lastResult = null;
			foreach(var form in compiledForms) {
				//Console.WriteLine ("Eval form " + form.ToString());
				lastResult = Eval (form, pCurrentScope);
			}

			return lastResult;
		}

		private object CompileMacro(object pBody, Scope pScope) {

			string pre = pBody.ToString ();
			object compiled = Eval (pBody, pScope);
			string post = compiled.ToString ();

#if MACRODEBUG
			Console.WriteLine ("Compiled " + pre + " to " + post);
#endif

			if (pre == post) {
				return compiled;
			} else {
				return CompileMacro (compiled, pScope);
			}
		}

		private object Quote(SharpList pList, Scope pCurrentScope) {
			return pList [1];
		}

		public object Set(SharpList pList, Scope pCurrentScope) {
			string symbolName = (pList [1] as SymbolToken).value;
			Scope s = pCurrentScope.TryResolveScope (symbolName);
			s.SetVar (symbolName, Eval(pList [2], pCurrentScope));
			return s.vars[symbolName];
		}
	}
}

