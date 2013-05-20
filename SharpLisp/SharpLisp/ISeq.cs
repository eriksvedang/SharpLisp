using System;

namespace SharpLisp
{
	public interface ISeq
	{
		object First();
		object Rest();
		object Cons(object pObject);
		object Conj(object pObject);
		object Nth(int pPosition);
	}
}

