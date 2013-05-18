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

			runner.LoadFile (new object[] { "core" });

			while (true) {
				Console.Write ("=> ");
				runner.ReadEval (Console.ReadLine(), true);
			}
		}
	}

	class Tester {
		public static float Foo(float x) {
			return 10 * x;
		}
	}
}
