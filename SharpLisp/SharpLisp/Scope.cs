using System;
using System.Collections.Generic;

namespace SharpLisp
{
	public class Scope
	{
		public string name;
		public Scope enclosingScope;
		public Dictionary<string, object> vars = new Dictionary<string, object>();

		public Scope (string pName, Scope pEnclosingScope)
		{
			name = pName;
			enclosingScope = pEnclosingScope;
		}

		public object TryResolveSymbol(SymbolToken pSymbolToken) {
			if (vars.ContainsKey(pSymbolToken.value)) {
				//Console.WriteLine("Resolved " + pSymbolToken + " in scope " + name);
				return vars[pSymbolToken.value];
			} else if (enclosingScope != null) {
				//Console.WriteLine("Can't find " + pSymbolToken + " in scope " + name + ", looking in enclosing scope: " + enclosingScope.name);
				return enclosingScope.TryResolveSymbol (pSymbolToken);
			} else {
				throw new Exception ("Can't resolve symbol " + pSymbolToken);
			}
		}

		public void SetVar(string pName, object pValue) {
			vars [pName] = pValue;
			//Console.WriteLine("Set " + pName + " in " + name + " to " + pValue);
		}

	}
}

