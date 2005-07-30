/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
namespace SQLite.NET
{
	public class SQLiteResultSet
	{

		// Fields
		private Hashtable columnIndexes;
		internal ArrayList columnNames;
		private int internalRowPointer;
		internal ArrayList rowData;
		public string LastCommand;

		// Methods
		public SQLiteResultSet()
		{
			this.columnIndexes = new Hashtable();
			this.columnNames = new ArrayList();
			this.rowData = new ArrayList();
		}
 

		public ArrayList GetColumn(int columnIndex)
		{
			ArrayList list1 = new ArrayList();
			if (this.columnNames.Count >= (columnIndex + 1))
			{
				foreach (ArrayList list2 in this.rowData)
				{
					list1.Add(list2[columnIndex]);
				}
			}
			return list1;
		}
 

		public ArrayList GetColumn(string columnName)
		{
			if (this.columnIndexes.ContainsKey(columnName))
			{
				return this.GetColumn((int) this.columnIndexes[columnName]);
			}
			return new ArrayList();
		}
 

		public string GetField(int rowIndex, int columnIndex)
		{
			if ((this.rowData.Count >= (rowIndex + 1)) && (this.ColumnNames.Count >= (columnIndex + 1)))
			{
				ArrayList list1 = this.GetRow(rowIndex);
				return list1[columnIndex].ToString();
			}
			return "";
		}
 

		public ArrayList GetRow()
		{
			return this.GetRow(this.internalRowPointer++);
		}
 

		public ArrayList GetRow(int rowIndex)
		{
			if (this.rowData.Count >= (rowIndex + 1))
			{
				return (ArrayList) this.rowData[rowIndex];
			}
			return null;
		}
 

		public Hashtable GetRowHash()
		{
			return this.GetRowHash(this.internalRowPointer++);
		}
 

		public Hashtable GetRowHash(int rowIndex)
		{
			Hashtable hashtable1 = new Hashtable(this.columnNames.Count);
			ArrayList list1 = this.GetRow(rowIndex);
			for (int num1 = 0; num1 < list1.Count; num1++)
			{
				if (list1[num1] != null)
				{
					hashtable1[(string) this.columnNames[num1]] = list1[num1];
				}
				else
				{
					hashtable1[(string) this.columnNames[num1]] = null;
				}
			}
			return hashtable1;
		}
 

		public void Reset()
		{
			this.internalRowPointer = 0;
		}
 

		public bool Seek(int index)
		{
			if (index < this.rowData.Count)
			{
				this.internalRowPointer = index;
				return true;
			}
			return false;
		}
 


		// Properties
		public ArrayList ColumnNames
		{
			get
			{
				return this.columnNames;
			}
		}
		public Hashtable ColumnIndices
		{
			get
			{
				return this.columnIndexes;
			}
		}
 

		public bool IsMoreData
		{
			get
			{
				return (this.internalRowPointer < this.rowData.Count);
			}
		}
 

		public ArrayList Rows
		{
			get
			{
				return this.rowData;
			}
		}
 

	}
 

}
