using System;
using System.Collections.Generic;

namespace SharpLisp
{
	public class Parser
	{
		List<Token> _tokens;
		int _currentTokenNr;
		int _indent = 0;

		public Parser ()
		{
		}

		public List<object> GetRootExpressions(List<Token> pTokens) {
			_tokens = pTokens;
			_currentTokenNr = 0;

			List<object> rootExpressions = new List<object>();

			while(MoreToRead()) {
				object sexp = Read ();
				rootExpressions.Add (sexp);
			}

			return rootExpressions;
		}

		object Read() 
		{
			Token token = CurrentToken ();
			Step ();

			//Console.WriteLine ("Reading token: " + token);

			object sexp = null;

			if (token.type == TokenType.FLOAT) {
				sexp = (token as FloatToken).value;
			} else if (token.type == TokenType.STRING) {
				sexp = (token as StringToken).value;
			} else if (token.type == TokenType.BOOL) {
				sexp = (token as BoolToken).value;
			} else if (token.type == TokenType.SYMBOL) {
				sexp = token;
			} else if (token is ReservedToken) {
				sexp = token;
			} else if (token.type == TokenType.LEFT_PAREN) {
				sexp = ReadList ();
			} else if (token.type == TokenType.LEFT_BRACKET) {
				sexp = ReadVector ();
			} else {
				throw new Exception ("Can't understand token " + token);
			}

			if (sexp == null) {
				throw new Exception ("The returned s-expression is null");
			}

			return sexp;
		}

		SharpList ReadList() {
			var list = new SharpList ();
			_indent++;
			while (CurrentToken().type != TokenType.RIGHT_PAREN) {
				object child = Read ();
				list.Add (child);
				//Console.WriteLine (GetIndentSpaces() + "Added " + child.ToString() + " to list");
			}
			Step (); // skip the right parenthesis
			_indent--;
			return list;
		}

		SharpVector ReadVector() {
			var vector = new SharpVector ();
			_indent++;
			while (CurrentToken().type != TokenType.RIGHT_BRACKET) {
				object child = Read ();
				vector.Add (child);
				//Console.WriteLine (GetIndentSpaces() + "Added " + child.ToString() + " to list");
			}
			Step (); // skip the right bracket
			_indent--;
			return vector;
		}

		void Step() {
			_currentTokenNr++;
		}

		Token CurrentToken() {
			return _tokens [_currentTokenNr];
		}

		bool MoreToRead() {
			return _currentTokenNr < _tokens.Count;
		}

		string GetIndentSpaces() {
			string s = "";
			for (int i = 0; i < _indent; i++) {
				s += "\t";
			}
			return s;
		}

	}
}

