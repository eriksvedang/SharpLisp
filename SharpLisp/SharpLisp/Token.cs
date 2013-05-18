using System;

namespace SharpLisp
{
	public enum TokenType {
		LEFT_PAREN,
		RIGHT_PAREN,
		LEFT_BRACKET,
		RIGHT_BRACKET,
		FLOAT,
		STRING,
		BOOL,
		SYMBOL,
		DEF,
		FN,
		LET,
		IF,
		CONJ,
	};

	public class Token
	{
		public TokenType type;

		public Token (TokenType pType) {
			type = pType;
		}

		public override string ToString ()
		{
			return type.ToString ();
		}
	}

	public class FloatToken : Token
	{
		public float value;

		public FloatToken(float pValue) : base(TokenType.FLOAT) {
			value = pValue;
		}

		public override string ToString ()
		{
			return value.ToString();
		}
	}

	public class StringToken : Token
	{
		public string value;

		public StringToken(string pValue) : base(TokenType.STRING) {
			value = pValue;
		}

		public override string ToString ()
		{
			return "\"" + value.ToString() + "\"";
		}
	}

	public class BoolToken : Token
	{
		public bool value;

		public BoolToken(bool pValue) : base(TokenType.BOOL) {
			value = pValue;
		}

		public override string ToString ()
		{
			return value.ToString();
		}
	}

	public class SymbolToken : Token
	{
		public string value;

		public SymbolToken(string pValue) : base(TokenType.SYMBOL) {
			value = pValue;
		}

		public override string ToString ()
		{
			//return string.Format ("Token/" + type + "(value: {0})", value);
			return value;
		}
	}

	public class ReservedToken : Token
	{
		public ReservedToken(TokenType pType) : base(pType) {

		}
	}


}

