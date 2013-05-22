using System;
using System.Reflection;
using System.Collections.Generic;

namespace SharpLisp
{
	public class BuiltInFunctions
	{
		static List<Module> s_modules;

		static void LoadModules()
		{
			s_modules = new List<Module>();

			var callingAssembly = Assembly.GetCallingAssembly ();
			s_modules.AddRange(callingAssembly.GetLoadedModules ());

//			var executingAssembly = Assembly.GetExecutingAssembly ();
//			modules.AddRange (executingAssembly.GetLoadedModules());
//
//			var entryAssembly = Assembly.GetEntryAssembly ();
//			modules.AddRange (entryAssembly.GetLoadedModules());

			AssemblyName[] names = callingAssembly.GetReferencedAssemblies ();
			//Console.WriteLine ("Referenced assemblies: " + string.Join<AssemblyName>(", ", names));

			foreach(var name in names) {
				Assembly assembly = Assembly.Load(name);
				s_modules.AddRange (assembly.GetLoadedModules());
			}

			//Console.WriteLine ("Loaded modules: " + string.Join<Module>(", ", s_modules));
		}

		public static object Throw(object[] args) {
			throw new Exception (args[0] as string);
		}

		public static object Seq(object[] args) {
			return args [0] as ISeq;
		}

		public static object New(object[] args) {
			SymbolToken typeName = (SymbolToken)args [0];

			List<object> remainingArgs = new List<object> ();
			List<Type> argTypes = new List<Type> ();

			for (int i = 1; i < args.Length; i++) {
				remainingArgs.Add (args[i]);
				argTypes.Add(args[i].GetType());
			}

			Type type = GetTypeInModules (typeName.value);

			ConstructorInfo constructor = type.GetConstructor (argTypes.ToArray());
			object newObject = constructor.Invoke (remainingArgs.ToArray());

			return newObject;
		}

		public static object InvokeMember(object[] args) {

			object o = args [0];
			SymbolToken methodName = (SymbolToken)args [1];

			List<object> remainingArgs = new List<object> ();
			for (int i = 2; i < args.Length; i++) {
				remainingArgs.Add (args[i]);
			}

			Type type = o.GetType ();

			var methodInfo = type.GetMethod (methodName.value);
			var propertyInfo = type.GetProperty (methodName.value);
			var fieldInfo = type.GetField (methodName.value);

			object result = null;

			if (methodInfo != null) {
				result = methodInfo.Invoke (o, remainingArgs.ToArray ());
			} else if (propertyInfo != null) {
				result = propertyInfo.GetValue (o, null);
			} else if (fieldInfo != null) {
				result = fieldInfo.GetValue (o);
			} else {
				throw new Exception("Can't find field, method or property " + methodName.value);
			}

			return result;
		}

		public static object InvokeStatic(object[] args) {

			SymbolToken typeName = (SymbolToken)args [0];
			SymbolToken methodName = (SymbolToken)args [1];

			List<object> remainingArgs = new List<object> ();
			for (int i = 2; i < args.Length; i++) {
				remainingArgs.Add (args[i]);
			}
	
			Type type = GetTypeInModules (typeName.value);

//			Console.WriteLine ("Methods: " + string.Join<MethodInfo>(", ", type.GetMethods()));
//			Console.WriteLine ("Fields: " + string.Join<FieldInfo>(", ", type.GetFields()));

			var methodInfo = type.GetMethod (methodName.value);
			var propertyInfo = type.GetProperty (methodName.value);
			var fieldInfo = type.GetField (methodName.value);

			object result = null;

			if (methodInfo != null) {
				result = methodInfo.Invoke (null, remainingArgs.ToArray ());
			} else if (propertyInfo != null) {
				result = propertyInfo.GetValue (null, null);
			} else if (fieldInfo != null) {
				result = fieldInfo.GetValue (null);
			} else {
				throw new Exception("Can't find method or property " + methodName.value);
			}

			return result;
		}

		public static Type GetTypeInModules(string pTypeName) {
			if (s_modules == null) {
				LoadModules ();
			}

			foreach(var module in s_modules) {
//				Console.WriteLine ("Loaded methods: " + string.Join<MethodInfo>(", ", module.GetMethods()));
//				Console.WriteLine ("Loaded types: " + string.Join<Type>(", ", module.GetTypes()));

				Type type = module.GetType (pTypeName);

				if (type != null) {
					//Console.WriteLine ("Found type " + type + " in module " + module);
					return type;
				}
			}

			throw new Exception ("Can't find type " + pTypeName); 
		}

		public static object Type(object[] args) {
			return args [0].GetType ();
		}

		public static object IsEmpty(object[] args) {
			if(args[0] == null) {
				return true;
			} 
			return (int)Count (args) == 0;
		}

		public static object Cons(object[] args) {
			var seq = args [1] as ISeq;
			if(seq == null) {
				throw new Exception ("Can't cast arg 1 to ISeq");
			}
			return seq.Cons (args[0]);
		}

		public static object Conj(object[] args) {
			var seq = args [0] as ISeq;
			return seq.Conj (args[1]);
		}

		public static object List(object[] args) {
			SharpList newList = new SharpList ();
			for(int i = 0; i < args.Length; i++) {
				newList.Add(args[i]);
			}
			return newList;
		}

		public static object Nth(object[] args) {
			var seq = args [0] as ISeq;
			float pos = (float)args [1];
			return seq.Nth ((int)pos);
		}

		public static object First(object[] args) {
			var seq = args [0] as ISeq;
			return seq.First ();
		}

		public static object Rest(object[] args) {
			var seq = args [0] as ISeq;
			return seq.Rest ();
		}

		public static object Count(object[] args) {
			if (args [0] is SharpList) {
				return (args [0] as SharpList).Count;
			} else if (args [0] is SharpVector) {
				return (args [0] as SharpVector).Count;
			} else {
				throw new Exception ("Can't call count on args[0] " + args[0]);
			}
		}

		public static object Modulus(object[] args) {
			return (float)args[0] % (float)args[1];
		}

		public static object VariadicAdd(object[] args) {
			float sum = 0f;
			foreach (var item in args) {
				sum += (float)item;
			}
			return sum;
		}

		public static object VariadicMultiplication(object[] args) {
			float product = 1f;
			foreach (var item in args) {
				product *= (float)item;
			}
			return product;
		}

		public static object VariadicSubtraction(object[] args) {
			float result = (float)args[0];
			bool skip = true;
			foreach (var item in args) {
				if (skip) {
					skip = false;
					continue;
				}
				result -= (float)item;
			}
			return result;
		}

		public static object VariadicDivision(object[] args) {
			float result = (float)args[0];
			bool skip = true;
			foreach (var item in args) {
				if (skip) {
					skip = false;
					continue;
				}
				result /= (float)item;
			}
			return result;
		}

		public static Func<object[], object> VariadicCheck(Func<float, float, bool> pComparerFunction)
		{
			return args => {
				if(args.Length <= 1) {
					return true;
				}

				float prev = (float)args [0];

				for(int i = 1; i < args.Length; i++) {
					float value = (float)args[i];
					if (pComparerFunction(prev, value)) {
						prev = value;
					} else {
						return false;
					}
				}
				return true;
			};
		}

		public static object VariadicEqual(object[] args) {
			object first = args[0];
			bool skip = true;
			foreach (var item in args) {
				if (skip) {
					skip = false;
					continue;
				}

				if (item == null || first == null) {
					return item == first;
				}
				//Console.WriteLine ("Comparing " + item + " and " + first);
				// TODO: Remove inneffiency!
				else if (item.ToString() != first.ToString()) {
					return false;
				}
			}
			return true;
		}

		public static void AssertArgumentCount(int pNr, object[] args) {
			if (pNr != args.Length) {
				throw new Exception ("Wrong nr of arguments to function");
			}
		}

		public static void AssertType<T>(object[] args, int pos) {
			if (args == null) {
				throw new Exception ("args is null in call to AssertType");
			}
			if (pos < 0 || pos > args.Length - 1) {
				throw new Exception ("pos is wrong in call to AssertType: " + pos);
			}
			if (args [pos] == null) {
				throw new Exception ("arg at pos " + pos + " is null in call to AssertType: ");
			}
			Type type = args [pos].GetType ();
			if(type != typeof(T)) {
				throw new Exception("Wrong type of argument " + pos + ", expected " + typeof(T) + " but it was " + type);
			}
		}
	}
}

