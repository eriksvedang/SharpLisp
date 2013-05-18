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

				if (IsWhitespace(c)) {
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
				} else if (c == ';') {
					while(MoreToRead() && ReadCurrentChar() != '\n') {
						Step ();
					}
				} else if (IsNumber (c)) {
					_tokens.Add (new FloatToken(ReadNr ()));
				} else if (c == '\"') {
					_tokens.Add (new StringToken(ReadString ()));
				} else if (IsChar (c)) {
					string text = ReadText ();
					if (text == "def") {
						_tokens.Add (new ReservedToken(TokenType.DEF));
					} else if (text == "fn") {
						_tokens.Add (new ReservedToken(TokenType.FN));
					} else if (text == "let") {
						_tokens.Add (new ReservedToken(TokenType.LET));
					} else if (text == "if") {
						_tokens.Add (new ReservedToken(TokenType.IF));
					} else if (text == "conj") {
						_tokens.Add (new ReservedToken(TokenType.CONJ));
					} else if (text == "macro") {
						_tokens.Add (new ReservedToken(TokenType.MACRO));
					} else if (text == "quote") {
						_tokens.Add (new ReservedToken(TokenType.QUOTE));
					} else if (text == "null") {
						_tokens.Add (new NullToken());
					} else if (text == "true") {
						_tokens.Add (new BoolToken(true));
					} else if (text == "false") {
						_tokens.Add (new BoolToken(false));
					} else {
						_tokens.Add (new SymbolToken(text));
					}
				} else {
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

		bool IsNumber(char c) {
			return "1234567890.".Contains (c.ToString());
		}

		float ReadNr() {
			string s = "";
			while (MoreToRead()) {
				char c = ReadCurrentChar ();
				if (IsNumber (c)) {
					s += c;
					Step ();
				} else {
					break;
				}
			}
			return Convert.ToSingle (s);
		}

		bool IsChar(char c) {
			return "abcdefghijklmnopqrstuvwxyz_+-*/@#&?!%=<>".Contains (c.ToString().ToLower());
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

