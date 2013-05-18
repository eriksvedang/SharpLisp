using System;

namespace SharpLisp
{
	public class SharpFunction
	{
		public Func<object[], object> function;
		public string name;

		public SharpFunction (Func<object[], object> pFunction, string pName)
		{
			function = pFunction;
			name = pName;
		}

		public override string ToString ()
		{
			return name;
		}
	}
}
