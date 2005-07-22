using System;

namespace System.Windows.Serialization
{
	public interface IAddChild
	{
		void AddChild(object value);
		void AddText(string text);
	}
}
