#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

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
