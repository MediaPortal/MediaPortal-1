/*
 * 
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ColumnInfo.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Framework
{
	internal class ColumnInfo
	{
		private int columnComboHashCode;
		private string[] columnNames;
		private FieldList fields;
		private int columnCalculatedMask;

		public ColumnInfo( ObjectMap objectMap, string[] columnNames )
		{
			this.columnNames = columnNames;
			columnComboHashCode = ObjectConstructor.GetFieldComboHashCode( columnNames );
			fields = new FieldList();
			for( int i = 0; i < columnNames.Length; i++ )
			{
				string columnName = columnNames[ i ];
				FieldMap fm = objectMap.GetFieldMapFromColumn( columnName );
				if( fm == null )
				{
					// check for column names with table name prefixes
					int pos = columnName.IndexOf( '.' );
					if( pos > 0 )
					{
						columnName = columnName.Substring( pos + 1, columnName.Length - pos - 1 );
						fm = objectMap.GetFieldMapFromColumn( columnName );
						if( fm != null )
						{
							columnNames[ i ] = columnName;
						}
					}
					if( fm == null ) // no corresponding member could be found - assume column is calculated
					{
						columnCalculatedMask |= 1 << i;
					}
				}
				fields.Add( fm ); // intentionally add null entries to preserve column order 
			}
		}

		#region Properties
		public FieldList Fields
		{
			get { return fields; }
		}

		public string[] Names
		{
			get { return columnNames; }
		}

		public int ColumnCalculatedMask
		{
			get { return columnCalculatedMask; }
		}

		public bool IsCalculatedColumn( int i )
		{
			return (columnCalculatedMask >> i & 1) != 0;
		}
		#endregion
	}
}