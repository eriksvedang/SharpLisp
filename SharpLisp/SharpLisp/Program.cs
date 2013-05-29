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

			Environment environment = new Environment ();

			// The standard library
			environment.LoadFile (new object[] { "core.lisp" });

			// REPL
			while (true) {
				Console.Write ("=> ");
				environment.ReadEval (Console.ReadLine(), true);
			}
		}
	}
}
