using System;

namespace SharpLisp
{
	class Tester {

		public float x = -10;

		public Tester() {

		}

		public Tester(float pValue) {
			x = pValue;
		}

		public static float Foo(float x) {
			return 10 * x;
		}

		public float GetX() {
			return x;
		}
	}
}

