using System;
using System.Collections;

namespace System.Windows
{
	public sealed class StaticResources
	{
		public static object Find(string name)
		{
			return _resources[name];
		}

		public static bool Contains(string name)
		{
			return _resources.Contains(name);
		}

		static Hashtable			_resources = new Hashtable();
	}
}
