/*
 * 
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TableMap.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Helper class to maintain metadata on tables and columns.
	/// </summary>
	public class TableMap : BrokerLock
	{
		private IGentleProvider provider; // provider to which this map belongs
		private FieldList fields; // FieldMap instances
		private string tableName; // unquoted name
		private string quotedTableName; // quoted (if reserved word) name; tableName otherwise
		private FieldMap identityMap;
		private int tableId; // used by some analyzers to reference the table (database internal value)
		private bool isView;

		/// <summary>
		/// Construct a TableMap instance to hold information on the given table.
		/// </summary>
		/// <param name="provider">The Gentle provider to which this map relates.</param>
		/// <param name="tableName">The name of the table for which to hold information.</param>
		public TableMap( IGentleProvider provider, string tableName ) : base( new PersistenceBroker( provider ) )
		{
			this.provider = provider;
			// use property accessor to also set quoted name
			if( tableName != null )
			{
				TableName = tableName;
			}
			fields = new FieldList();
		}

		/// <summary>
		/// Construct a TableMap instance to hold information on the given table.
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance used to obtain metadata on the table.
		/// If null is passed the DefaultProvider settings will be used.</param>
		/// <param name="tableName">The name of the table for which to hold information.</param>
		public TableMap( PersistenceBroker broker, string tableName ) : base( broker )
		{
			provider = SessionBroker.Provider;
			// use property accessor to also set quoted name
			if( tableName != null )
			{
				TableName = tableName;
			}
			fields = new FieldList();
		}

		/// <summary>
		/// Construct a TableMap instance for an unspecified table.
		/// </summary>
		public TableMap( PersistenceBroker broker ) : this( broker, null )
		{
		}

		/// <summary>
		/// Obtain a <see cref="FieldMap"/> instance for the given column name.
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <returns>The FieldMap of the column</returns>
		public FieldMap GetFieldMapFromColumn( string columnName )
		{
			return fields.FindColumn( columnName );
		}

		#region Reference Lookup Methods
		/// <summary>
		/// Obtain the name of the column in in this table pointing to the given column 
		/// and table. This is used to map foreign key relations and allows the referencing
		/// column to use any name.
		/// </summary>
		/// <param name="tableName">The foreign table name</param>
		/// <param name="columnName">The foreign column name</param>
		/// <returns>The name of the local foreign key column referencing the given table column</returns>
		public FieldMap GetForeignKeyFieldMap( string tableName, string columnName )
		{
			FieldMap weakResult = null;
			foreach( FieldMap fm in fields )
			{
				if( fm.IsForeignKey &&
				    fm.ForeignKeyTableName == tableName &&
				    fm.ForeignKeyColumnName == columnName )
				{
					return fm; // exact match found - return result
				}
				else if( fm.ColumnName == columnName )
				{
					weakResult = fm;
				}
			}
			// If the following check statement is uncommented, presence of the ForeignKey attribute
			// on relations is enforced. When uncommented, the framework allows the request if the
			// column names are identical (weak referencing).
			return weakResult;
			// TODO: This behavior should be made a configuration file option.
			//Check.Fail( Error.DeveloperError, "The type {0} does not have a foreign key relation "+
			//	"to the column {1} in table {2}. Check if you need to add a ForeignKey attribute.",
			//	this.Type.Name, columnName, tableName );
		}

		/// <summary>
		/// Obtain a list of <see cref="FieldMap"/> instances that represent foreign key
		/// references to fields on the supplied parent object.
		/// </summary>
		/// <param name="parentMap">The map of the type the foreign key fields should reference
		/// if they are to be included in the result.</param>
		/// <param name="isPrimaryKeysOnly">True if only foreign keys pointing to primary keys
		/// should be included, false to include all foreign key references.</param>
		/// <returns>A list of FieldMap instances or an empty list id no fields matched.</returns>
		public Hashtable GetForeignKeyMappings( TableMap parentMap, bool isPrimaryKeysOnly )
		{
			Hashtable result = new Hashtable();
			foreach( FieldMap child in fields )
			{
				if( child.IsForeignKey )
				{
					foreach( FieldMap parent in parentMap.Fields )
					{
						bool isSameTable = String.Compare( child.ForeignKeyTableName, parentMap.TableName, true ) == 0;
						isSameTable |= String.Compare( child.ForeignKeyTableName, parentMap.QuotedTableName, true ) == 0;
						if( isSameTable && String.Compare( child.ForeignKeyColumnName, parent.ColumnName, true ) == 0 )
						{
							bool isPK = child.IsPrimaryKey || parent.IsPrimaryKey;
							if( ! isPrimaryKeysOnly || (isPrimaryKeysOnly && isPK) )
							{
								result[ child ] = parent;
							}
						}
					}
				}
			}
			Check.Verify( result.Count > 0, Error.DeveloperError,
			              String.Format( "The table {0} has no foreign keys pointing to table {1}.",
			                             tableName, parentMap.TableName ) );
			return result;
		}
		#endregion

		/// <summary>
		/// Obtain the system type of a given column name.
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <returns>The system type of the corresponding property</returns>
		public Type GetColumnType( string columnName )
		{
			FieldMap fm = fields.FindColumn( columnName );
			return fm != null ? fm.Type : null;
		}

		#region Properties
		/// <summary>
		/// Obtain the number of primary key properties/columns used by this type.
		/// </summary>
		public int PrimaryKeyCount
		{
			get
			{
				int result = 0;
				foreach( FieldMap fm in fields )
				{
					if( fm.IsPrimaryKey )
					{
						result++;
					}
				}
				return result;
			}
		}

		/// <summary>
		/// The list of fields found for the type represented.
		/// </summary>
		public FieldList Fields
		{
			get { return fields; }
		}
		/// <summary>
		/// The name of the column whose value is autogenerated on insert or null if no such column.
		/// </summary>
		public FieldMap IdentityMap
		{
			get { return identityMap; }
			set { identityMap = value; }
		}
		/// <summary>
		/// The table name to which objects are mapped.
		/// </summary>
		public string TableName
		{
			get { return tableName; }
			set
			{
				Check.Verify( tableName == null || value == tableName,
				              Error.DeveloperError, "Unable to update the table name once set." );
				tableName = value;
				// quote table name (if provider has yet been given)
				if( provider != null )
				{
					GentleSqlFactory sf = provider.GetSqlFactory();
					if( sf.IsReservedWord( value ) )
					{
						quotedTableName = sf.QuoteReservedWord( tableName );
					}
					else
					{
						quotedTableName = tableName;
					}
				}
			}
		}
		/// <summary>
		/// The table name (quoted only if reserved word) to which objects are mapped.
		/// </summary>
		public string QuotedTableName
		{
			get { return quotedTableName == null ? tableName : quotedTableName; }
		}
		/// <summary>
		/// The Gentle provider from which this map was obtained.
		/// </summary>
		public IGentleProvider Provider
		{
			get { return provider; }
			set { provider = value; }
		}
		/// <summary>
		/// Internally used value to reference the table while analyzing the database.
		/// </summary>
		public int TableId
		{
			get { return tableId; }
			set { tableId = value; }
		}
		/// <summary>
		/// Internally used value to tell whether this TableMap represents a view or a table.
		/// </summary>
		public bool IsView
		{
			get { return isView; }
			set { isView = value; }
		}
		#endregion
	}
}