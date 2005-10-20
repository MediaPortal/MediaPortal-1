using System;

namespace System.Windows
{
	public interface INameScope
	{
		#region Properties
			
		object FindName(string name);

		#endregion Properties
	}
}
