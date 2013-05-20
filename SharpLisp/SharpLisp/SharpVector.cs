using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLisp
{
	public class SharpVector : List<object>, ISeq
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

		#region ISeq implementation
		public object First ()
		{
			if (this.Count == 0) {
				return null;
			} else {
				return this[0];
			}
		}

		public object Rest ()
		{
			if (this.Count == 0) {
				return null;
			} else {
				if (Count <= 1) {
					return null;
				}
				var deepCopy = new SharpVector ();
				for(int i = 1; i < Count; i++) {
					deepCopy.Add (this[i]);
				}
				return deepCopy;
			}
		}

		public object Cons (object pObject)
		{
			var deepCopy = new SharpVector();
			deepCopy.Add (pObject);
			foreach (var item in this) {
				deepCopy.Add (item);
			}
			return deepCopy;
		}

		public object Conj (object pObject)
		{
			var deepCopy = new SharpVector();
			foreach (var item in this) {
				deepCopy.Add (item);
			}
			deepCopy.Add (pObject);
			return deepCopy;
		}

		public object Nth (int pPosition)
		{
			return this[pPosition];
		}

		#endregion
	}
}

