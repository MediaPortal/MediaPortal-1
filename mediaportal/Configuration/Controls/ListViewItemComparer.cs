using System;
using System.Collections;
using System.Windows.Forms;
namespace MediaPortal.Configuration.Controls
{
	/// <summary>
	/// Summary description for ListViewItemComparer.
	/// </summary>
	public class ListViewItemComparer : IComparer 
	{
		private int col;
		public ListViewItemComparer() 
		{
			col=0;
		}
		public ListViewItemComparer(int column) 
		{
			col=column;
		}
		public int Compare(object x, object y) 
		{
			return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
		}
	}

	public class ListViewItemComparerInt : IComparer 
	{
		private int col;
		public ListViewItemComparerInt() 
		{
			col=0;
		}
		public ListViewItemComparerInt(int column) 
		{
			col=column;
		}
		public int Compare(object x, object y) 
		{
			int item1=Int32.Parse( ((ListViewItem)x).SubItems[col].Text);
			int item2=Int32.Parse( ((ListViewItem)y).SubItems[col].Text);
			if (item1 < item2) return -1;
			if (item1 > item2) return 1;
			return 0;
		}
	}
}
