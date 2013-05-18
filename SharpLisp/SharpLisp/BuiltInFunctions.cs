using System;

namespace SharpLisp
{
	public class BuiltInFunctions
	{
		public static object IsEmpty(object[] args) {
			if(args[0] == null) {
				return true;
			} else if (args [0] is SharpVector) {
				SharpVector vector = args [0] as SharpVector;
				return vector.Count == 0;
			} else {
				throw new Exception ("First arg to empty? is of type " + args[0].GetType());
			}
		}

		public static object Cons(object[] args) {
			object itemToInsert = args [0];
			SharpVector vector = args[1] as SharpVector;
			if (vector == null) {
				throw new Exception ("First argument to cons is not a vector: " + args[0]);
			}
			SharpVector deepCopy = new SharpVector ();
			deepCopy.Add (itemToInsert);
			foreach (var item in vector) {
				deepCopy.Add (item);
			}
			return deepCopy;
		}

		public static object Nth(object[] args) {
			SharpVector vector = args[0] as SharpVector;
			float pos = (float)args [1];
			return vector[(int)pos];
		}

		public static object First(object[] args) {
			SharpVector vector = args[0] as SharpVector;
			if (vector.Count == 0) {
				return null;
			}
			return vector[0];
		}

		public static object Rest(object[] args) {
			if (args[0] == null) {
				return null;
			}

			SharpVector vector = args[0] as SharpVector;

			if (vector.Count <= 1) {
				return null;
			}

			var deepCopy = new SharpVector ();
			for(int i = 1; i < vector.Count; i++) {
				deepCopy.Add (vector[i]);
			}
			return deepCopy;
		}

		public static object Count(object[] args) {
			if (args[0] == null) {
				return null;
			}
			AssertType<SharpVector>(args, 0);
			SharpVector vector = args[0] as SharpVector;
			return vector.Count;
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

		public static object VariadicPrint(object[] args) {
			System.Text.StringBuilder concat = new System.Text.StringBuilder ();
			foreach(var arg in args) {
				if (arg == null) {
					concat.Append("null");
				} else {
					concat.Append(arg);
				}
			}
			Console.WriteLine (concat.ToString());
			return new SharpList();
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

