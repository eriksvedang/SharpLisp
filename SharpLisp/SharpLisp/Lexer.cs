using System;
using System.Collections.Generic;

namespace SharpLisp
{
	public class Lexer
	{
		int _readPosition;
		string _stringToRead;
		List<Token> _tokens;

		public Lexer ()
		{
		}

		public List<Token> Read(string pString)
		{
			_readPosition = 0;
			_stringToRead = pString;
			_tokens = new List<Token> ();

			while(MoreToRead()) {
				char c = ReadCurrentChar ();

				if (IsWhitespace (c)) {
					Step ();
				} else if (c == '(') {
					_tokens.Add (new Token(TokenType.LEFT_PAREN));
					Step ();
				} else if (c == ')') {
					_tokens.Add (new Token(TokenType.RIGHT_PAREN));
					Step ();
				} else if (c == '[') {
					_tokens.Add (new Token(TokenType.LEFT_BRACKET));
					Step ();
				} else if (c == ']') {
					_tokens.Add (new Token(TokenType.RIGHT_BRACKET));
					Step ();
				} else if (c == '\'') {
					_tokens.Add (new Token(TokenType.TICK));
					Step ();
				} else if (c == ';') {
					while (MoreToRead() && ReadCurrentChar() != '\n') {
						Step ();
					}
				} else if (c == '\"') {
					_tokens.Add (new StringToken(ReadString ()));
				} else if (c == '&') {
					_tokens.Add (new ReservedToken(TokenType.AMPERSAND));
					Step ();
				} else if (c == '.') {
					_tokens.Add (new SymbolToken(c.ToString()));
					Step ();
				} else if(CharCanBeginSymbol (c)) {
					string text = ReadText ();
					if(IsNumber(text)) {
						_tokens.Add (new FloatToken(Convert.ToSingle(text)));
					} else if (text == "def") {
						_tokens.Add (new ReservedToken(TokenType.DEF));
					} else if (text == "fn") {
						_tokens.Add (new ReservedToken(TokenType.FN));
					} else if (text == "let") {
						_tokens.Add (new ReservedToken(TokenType.LET));
					} else if (text == "if") {
						_tokens.Add (new ReservedToken(TokenType.IF));
					} else if (text == "defmacro") {
						_tokens.Add (new ReservedToken(TokenType.DEFMACRO));
					} else if (text == "quote") {
						_tokens.Add (new ReservedToken(TokenType.QUOTE));
					} else if (text == "set!") {
						_tokens.Add (new ReservedToken(TokenType.SET));
					} else if (text == "null") {
						_tokens.Add (new NullToken());
					} else if (text == "true") {
						_tokens.Add (new BoolToken(true));
					} else if (text == "false") {
						_tokens.Add (new BoolToken(false));
					} else {
						_tokens.Add (new SymbolToken(text));
					} 
				} 
				else {
					throw new Exception ("Can't understand char '" + c + "'");
				}
			}

			return _tokens;
		}

		char ReadCurrentChar() {
			return _stringToRead [_readPosition];
		}

		void Step() {
			_readPosition++;
			//Console.WriteLine ("Stepped to " + ReadCurrentChar());
		}

		bool MoreToRead() {
			return _readPosition < _stringToRead.Length;
		}

		bool IsNumber(string pText) {
			bool skipFirstSign = false;
			if (pText[0] == '-') {
				skipFirstSign = true;
				if (pText.Length == 1) {
					return false;
				}
			}

			bool hasHitPeriod = false;
			foreach(var c in pText) {
				if (skipFirstSign) {
					skipFirstSign = false;
					continue;
				}
				if (c == '.') {
					if(hasHitPeriod) {
						return false;
					} else {
						hasHitPeriod = true;
					}
				}
				if (!("1234567890.".Contains (c.ToString()))) {
					return false;
				}
			}
			return true;
		}

		float ReadNr() {
			string s = "";
			while (MoreToRead()) {
				char c = ReadCurrentChar ();
				if (!IsWhitespace (c) && c != ')' && c != ']') {
					s += c;
					Step ();
				} else {
					break;
				}
			}
			return Convert.ToSingle (s);
		}

		bool CharCanBeginSymbol(char c) {
			return "abcdefghijklmnopqrstuvwxyz_+-*/<>?!%=1234567890-".Contains (c.ToString().ToLower());
		}

		bool IsChar(char c) {
			return "abcdefghijklmnopqrstuvwxyz_+-*/@#&?!%=<>.,;:1234567890".Contains (c.ToString().ToLower());
		}

		string ReadText() {
			string s = "";
			while (MoreToRead()) {
				char c = ReadCurrentChar ();
				if (IsChar (c)) {
					s += c;
					Step ();
				} else {
					break;
				}
			}
			return s;
		}

		string ReadString() {
			Step ();
			string s = "";
			while (MoreToRead()) {
				char c = ReadCurrentChar ();
				if (c != '\"') {
					s += c;
					Step ();
				} else {
					break;
				}
			}
			Step ();
			return s;
		}

		bool IsWhitespace(char c) {
			return " ,\t\n".Contains (c.ToString());
		}
	}
}

