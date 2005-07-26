using System;
using System.Windows.Forms;

namespace System
{
	public class SortEventArgs : EventArgs
	{
		public SortEventArgs(SortOrder order)
		{
			this.Order = order;
		}

		public SortOrder Order;
	}
}
