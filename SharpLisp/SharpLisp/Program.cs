using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace SharpLisp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			CultureInfo culture = new CultureInfo("");
			Thread.CurrentThread.CurrentCulture = culture;

			Environment runner = new Environment ();

			runner.LoadFile (new object[] { "core.lisp" });

			while (true) {
				Console.Write ("=> ");
				runner.ReadEval (Console.ReadLine(), true);
			}
		}
	}

	class Tester {

		public float x = -10;

		public Tester() {

		}

		public Tester(float pValue) {
			x = pValue;
		}

		public static float Foo(float x) {
			return 10 * x;
		}

		public float GetX() {
			return x;
		}
	}
}
