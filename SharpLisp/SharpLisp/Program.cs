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

//			SharpList code = new SharpList() {
//				print, new SharpList() { multiplication, 20.0f, new SharpList() { add, 2.0f, 3.0f }}
//			};

//			Lexer lexer = new Lexer ();
//			List<Token> tokens = lexer.Read ("(* 3 3)");
//			tokens.ForEach (Console.WriteLine);
//
//			Parser parser = new Parser ();
//			object sexp = parser.BuildAst (tokens);
//			Console.WriteLine("sexp: " + sexp.ToString());
//
			Environment runner = new Environment ();
//			Console.WriteLine("> " + runner.Eval (sexp));

			runner.LoadFile (new object[] { "core" });

			while (true) {
				Console.Write ("=> ");
				runner.ReadEval (Console.ReadLine(), true);
			}
		}
	}
}
