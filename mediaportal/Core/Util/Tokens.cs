using System;
using System.Collections;

namespace MediaPortal.Util
{
	public class Tokens: IEnumerable
	{
		private string[] elements=null;

		public Tokens(string source, char[] delimiters)
		{
			if (source!=null) 
			{
				elements = source.Split(delimiters);
			}
		}

		// IEnumerable Interface Implementation:

		public TokenEnumerator GetEnumerator() // non-IEnumerable version
		{
			return new TokenEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() // IEnumerable version
		{
			return (IEnumerator) new TokenEnumerator(this);
		}

		// Inner class implements IEnumerator interface:

		public class TokenEnumerator: IEnumerator
		{
			private int position = -1;
			private Tokens t;

			public TokenEnumerator(Tokens t)
			{
				this.t = t;
			}

			public bool MoveNext()
			{
				if (null==t.elements) return false;
				if (position < t.elements.Length - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				position = -1;
			}

			public string Current // non-IEnumerator version: type-safe
			{
				get
				{
					return t.elements[position];
				}
			}

			object IEnumerator.Current // IEnumerator version: returns object
			{
				get
				{
					return t.elements[position];
				}
			}
		}
	}
}
