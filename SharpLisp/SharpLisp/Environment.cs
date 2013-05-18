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

		Scope _globalScope;

		public Environment ()
		{
			_globalScope = new Scope ("Global", null);

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

			_globalScope.vars ["empty?"] = new SharpFunction (BuiltInFunctions.IsEmpty, "empty?");
			_globalScope.vars ["nth"] = new SharpFunction (BuiltInFunctions.Nth, "nth");
			_globalScope.vars ["first"] = new SharpFunction (BuiltInFunctions.First, "first");
			_globalScope.vars ["rest"] = new SharpFunction (BuiltInFunctions.Rest, "rest");
			_globalScope.vars ["count"] = new SharpFunction (BuiltInFunctions.Count, "count");
			_globalScope.vars ["cons"] = new SharpFunction (BuiltInFunctions.Cons, "cons");

			_globalScope.vars ["load"] = new SharpFunction (LoadFile, "load");
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
			return new SharpList ();
		}

		public object Eval(object o, Scope pCurrentScope) {
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

				if (token.type == TokenType.MACRO) {
					return Macro (pList, pCurrentScope);
				}

				if (token.type == TokenType.QUOTE) {
					return Quote (pList, pCurrentScope);
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
				foreach(var argBinding in argBindingsDeepCopy) {
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

		private object Macro(SharpList pList, Scope pCurrentScope) {
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

			return null;
		}

		private object Quote(SharpList pList, Scope pCurrentScope) {
			return pList [1];
		}
	}
}

