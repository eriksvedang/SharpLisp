using System;

namespace SharpLisp
{
	public class SharpFunction
	{
		public delegate object FunctionDelegate(object[] args);

		public FunctionDelegate function;
		public string name;

		public SharpFunction (FunctionDelegate pFunction, string pName)
		{
			function = pFunction;
			name = pName;
		}

		public override string ToString ()
		{
			return string.Format (name);
		}
	}
}
