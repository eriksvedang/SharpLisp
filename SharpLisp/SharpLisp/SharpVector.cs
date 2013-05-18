using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLisp
{
	public class SharpVector : List<object>
	{
		public SharpVector ()
		{
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("[");
			int i = this.Count;
			foreach (var item in this) {
				if (item == null) {
					sb.Append ("null");
				} else {
					sb.Append (item);
				}
				i--;
				if (i > 0) {
					sb.Append (" ");
				}
			}
			sb.Append ("]");
			return sb.ToString ();
		}
	}
}

