#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Collections;
using System.Collections.Generic;

namespace SQLite.NET
{
  public class SQLiteResultSet
  {
    public class Row
    {
      public List<string> fields = new List<string>();
    }

    // Fields
    private Hashtable columnIndexes;
    internal List<string> columnNames;
    internal List<Row> rowData;
    public string LastCommand;

    // Methods
    public SQLiteResultSet()
    {
      this.columnIndexes = new Hashtable();
      this.columnNames = new List<string>();
      this.rowData = new List<Row>();
    }


    public ArrayList GetColumn(int columnIndex)
    {
      ArrayList list1 = new ArrayList();
      if (this.columnNames.Count >= (columnIndex + 1))
      {
        foreach (Row list2 in this.rowData)
        {
          list1.Add(list2.fields[columnIndex]);
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
        Row list1 = this.GetRow(rowIndex);
        return list1.fields[columnIndex].ToString();
      }
      return "";
    }

    public Row GetRow(int rowIndex)
    {
      if (this.rowData.Count >= (rowIndex + 1))
      {
        return this.rowData[rowIndex];
      }
      return null;
    }


    // Properties
    public List<string> ColumnNames
    {
      get { return this.columnNames; }
    }

    public Hashtable ColumnIndices
    {
      get { return this.columnIndexes; }
    }


    public ArrayList RowsList
    {
      get
      {
        ArrayList rows = new ArrayList();
        foreach (Row row in this.rowData)
        {
          ArrayList cols = new ArrayList();
          foreach (string field in row.fields)
          {
            cols.Add(field);
          }
          rows.Add(cols);
        }
        return rows;
      }
    }


    public List<Row> Rows
    {
      get { return this.rowData; }
    }
  }
}