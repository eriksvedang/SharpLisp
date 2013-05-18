using System;

namespace SharpLisp
{
	public class BuiltInFunctions
	{


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
			SharpVector vector = args[0] as SharpVector;
			if (vector.Count <= 1) {
				return null;
			}
			return vector.GetRange(1, vector.Count - 1) as SharpVector;
		}

		public static object Count(object[] args) {
			AssertType<SharpVector>(args, 0);
			SharpVector vector = args[0] as SharpVector;
			return vector.Count;
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

		public static object VariadicEqual(object[] args) {
			object first = args[0];
			bool skip = true;
			foreach (var item in args) {
				if (skip) {
					skip = false;
					continue;
				}
				//Console.WriteLine ("Comparing " + item + " and " + first);
				// TODO: Remove inneffiency!
				if (item.ToString() != first.ToString()) {
					return false;
				}
			}
			return true;
		}

		public static object VariadicPrint(object[] args) {
			System.Text.StringBuilder concat = new System.Text.StringBuilder ();
			foreach(var arg in args) {
				concat.Append(arg);
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
			Type type = args [pos].GetType ();
			if(type != typeof(T)) {
				throw new Exception("Wrong type of argument " + pos + ", expected " + typeof(T) + " but it was " + type);
			}
		}
	}
}

