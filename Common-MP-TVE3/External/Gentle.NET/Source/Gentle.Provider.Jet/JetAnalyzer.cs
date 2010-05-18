/*
 * Jet/Access database schema analyzer
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: JetAnalyzer.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using System.Data.OleDb;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.Jet
{
	/// <summary>
	/// This class is the schema analyzer for Jet/Access. Based on information gathered
	/// and code snippets contributed by Roger Hendriks.
	/// 
	/// Useful information concerning OLE schema analysis (additional links below):
	///   http://support.microsoft.com/default.aspx/kb/309488/EN-US/?
	///   http://support.microsoft.com/default.aspx?scid=kb;en-us;310107
	///   http://msdn.microsoft.com/library/default.asp?url=/library/en-us/oledb/htm/oledbpart5_appendixes.asp
	/// </summary>
	public class JetAnalyzer : GentleAnalyzer
	{
		public JetAnalyzer( IGentleProvider provider ) : base( provider )
		{
		}

		public override ColumnInformation AnalyzerCapability
		{
			get { return ColumnInformation.ciLocal; }
		}

		#region Private Schema Analysis Helpers
		/// <summary>
		/// http://msdn.microsoft.com/library/en-us/oledb/htm/oledbtables_rowset.asp
		/// Restriction columns: TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE ("TABLE","VIEW")
		/// Schema columns: TABLE_GUID, DESCRIPTION, TABLE_PROPID, DATE_CREATED, DATE_MODIFIED
		/// </summary>
		private DataTable GetTables( string tableName )
		{
			OleDbConnection conn = provider.GetConnection() as OleDbConnection;
			// object[] argument match restriction columns (see above)
			DataTable result = conn.GetOleDbSchemaTable( OleDbSchemaGuid.Tables,
			                                             new object[] { null, null, tableName, null } );
			conn.Close();
			return result;
		}

		/// <summary>
		/// http://msdn.microsoft.com/library/en-us/oledb/htm/oledbcolumns_rowset.asp
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/oledb/htm/oledbtype_indicators.asp
		/// Restriction columns: TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME
		/// Schema columns: DATA_TYPE, ORDINAL_POSITION, COLUMN_HASDEFAULT, COLUMN_DEFAULT, 
		///		COLUMN_FLAGS, IS_NULLABLE, NUMERIC_PRECISION, NUMERIC_SCALE, 
		///		CHARACTER_MAXIMUM_LENGTH, CHARACTER_OCTET_LENGTH
		/// </summary>
		private DataTable GetColumns( string tableName )
		{
			OleDbConnection conn = provider.GetConnection() as OleDbConnection;
			// object[] argument match restriction columns (see above)
			DataTable result = conn.GetOleDbSchemaTable( OleDbSchemaGuid.Columns,
			                                             new object[] { null, null, tableName, null } );
			conn.Close();
			return result;
		}

		/// <summary>
		/// http://msdn.microsoft.com/library/en-us/oledb/htm/oledbprimary_keys_rowset.asp
		/// Restriction columns: TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME
		/// Schema columns: COLUMN_NAME, COLUMN_GUID, COLUMN_PROPID, ORDINAL, PK_NAME
		/// </summary>
		private DataTable GetPrimaryKeys( string tableName )
		{
			OleDbConnection conn = provider.GetConnection() as OleDbConnection;
			// object[] argument match restriction columns (see above)
			DataTable result = conn.GetOleDbSchemaTable( OleDbSchemaGuid.Primary_Keys,
			                                             new object[] { null, null, tableName } );
			conn.Close();
			return result;
		}

		/// <summary>
		/// Fetch schema information on keys.
		/// </summary>
		private DataTable GetPrimaryKeyInfo( string tableName )
		{
			OleDbConnection conn = provider.GetConnection() as OleDbConnection;
			OleDbCommand cmd = new OleDbCommand( "select * from " + tableName, conn );
			OleDbDataReader dr = cmd.ExecuteReader( CommandBehavior.KeyInfo );
			DataTable result = dr.GetSchemaTable();
			conn.Close();
			return result;
		}

		/// <summary>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/oledb/htm/oledbtable_constraints_rowset.asp
		/// Restriction columns: PK_TABLE_CATALOG, PK_TABLE_SCHEMA, PK_TABLE_NAME, 
		/// FK_TABLE_CATALOG, FK_TABLE_SCHEMA, FK_TABLE_NAME
		/// Schema columns: FK_COLUMN_NAME, FK_COLUMN_GUID, FK_COLUMN_PROPID, UPDATE_RULE,
		/// DELETE_RULE, PK_NAME, FK_NAME, DEFERRABILITY 
		/// </summary>
		private DataTable GetForeignKeys( string tableName )
		{
			OleDbConnection conn = provider.GetConnection() as OleDbConnection;
			// object[] argument match restriction columns (see above)
			DataTable result = conn.GetOleDbSchemaTable( OleDbSchemaGuid.Foreign_Keys,
			                                             new object[] { null, null, null, null, null, tableName } );
			conn.Close();
			return result;
		}
		#endregion

		public override void Analyze( string tableName )
		{
			try
			{
				bool isSingleRun = tableName != null;
				DataTable dt = GetTables( tableName );
				foreach( DataRow row in dt.Rows )
				{
					try
					{
						string dbTableName = (string) row[ "TABLE_NAME" ];
						// skip Access system tables
						if( ! dbTableName.StartsWith( "MSysAccess" ) )
						{
							if( ! isSingleRun || tableName.ToLower().Equals( dbTableName.ToLower() ) )
							{
								TableMap map = GetTableMap( dbTableName );
								if( map == null )
								{
									map = new TableMap( provider, dbTableName );
									maps[ dbTableName.ToLower() ] = map;
								}
								// get column information for this table
								GetColumnData( map );
								// abort loop if analyzing single table only
								if( isSingleRun )
								{
									break;
								}
							}
						}
					}
					catch( GentleException fe )
					{
						// ignore errors caused by tables found in db but for which no map exists
						// TODO this should be a config option
						if( fe.Error != Error.NoObjectMapForTable )
						{
							throw fe;
						}
					}
				}
				// get information on primary and foreign keys (for tables found here)
				GetPrimaryKeyData();
				GetForeignKeyData();
			}
			catch( Exception e )
			{
				Check.Fail( e, Error.Unspecified, "An error occurred while analyzing the database schema." );
			}
		}

		/// <summary>
		/// This enumeration represents the bitmask values of the COLUMN_FLAGS value used below.
		/// </summary>
		[Flags]
		private enum DBCOLUMNFLAGS
		{
			ISBOOKMARK = 0x1,
			MAYDEFER = 0x2,
			WRITE = 0x4,
			WRITEUNKNOWN = 0x8,
			ISFIXEDLENGTH = 0x10,
			ISNULLABLE = 0x20,
			MAYBENULL = 0x40,
			ISLONG = 0x80,
			ISROWID = 0x100,
			ISROWVER = 0x200,
			CACHEDEFERRED = 0x1000,
			SCALEISNEGATIVE = 0x4000,
			RESERVED = 0x8000,
			ISROWURL = 0x10000,
			ISDEFAULTSTREAM = 0x20000,
			ISCOLLECTION = 0x40000,
			ISSTREAM = 0x80000,
			ISROWSET = 0x100000,
			ISROW = 0x200000,
			ROWSPECIFICCOLUMN = 0x400000
		}

		/// <summary>
		/// This method fills the TableMap with information on table columns.
		/// </summary>
		private void GetColumnData( TableMap map )
		{
			DataTable dt = GetColumns( map.TableName );
			foreach( DataRow row in dt.Rows )
			{
				// result row contains:
				// COLUMN_NAME, DATA_TYPE, ORDINAL_POSITION, COLUMN_HASDEFAULT, COLUMN_DEFAULT, 
				// COLUMN_FLAGS, IS_NULLABLE, NUMERIC_PRECISION, NUMERIC_SCALE, 
				// CHARACTER_MAXIMUM_LENGTH, CHARACTER_OCTET_LENGTH
				string columnName = (string) row[ "COLUMN_NAME" ];
				FieldMap fm = map.GetFieldMapFromColumn( columnName );
				if( fm == null )
				{
					fm = new FieldMap( map, columnName );
					map.Fields.Add( fm );
				}
				bool isNullable = Convert.ToBoolean( row[ "IS_NULLABLE" ] );
				if( fm != null )
				{
					OleDbType dbType = (OleDbType) row[ "DATA_TYPE" ];
					fm.SetDbType( (long) dbType );
					// set numeric scale for DBTYPE_DECIMAL, DBTYPE_NUMERIC, DBTYPE_VARNUMERIC
					if( dbType == OleDbType.Decimal || dbType == OleDbType.Numeric || dbType == OleDbType.VarNumeric )
					{
						fm.SetSize( Convert.ToInt32( row[ "NUMERIC_PRECISION" ] ) );
					}
					if( dbType == OleDbType.LongVarBinary || dbType == OleDbType.LongVarChar ||
					    dbType == OleDbType.LongVarWChar || dbType == OleDbType.VarBinary ||
					    dbType == OleDbType.VarChar || dbType == OleDbType.VarWChar ||
					    dbType == OleDbType.WChar || dbType == OleDbType.Char || dbType == OleDbType.BSTR ||
					    dbType == OleDbType.Binary )
					{
						fm.SetSize( Convert.ToInt32( row[ "CHARACTER_MAXIMUM_LENGTH" ] ) );
					}
					fm.SetIsNullable( isNullable );

					int columnFlags = Convert.ToInt32( row[ "COLUMN_FLAGS" ] );

					// BROKEN (expected value does not match IS_NULLABLE set above)
					// BROKEN set whether column can contain NULL values
					int flags = (int) DBCOLUMNFLAGS.ISNULLABLE + (int) DBCOLUMNFLAGS.MAYBENULL;
					bool isNullableFlag = (columnFlags & flags) != 0;
					//fm.SetIsNullable( isNullableFlag && fm.IsNullable );

					// set whether column is updatable
					flags = (int) DBCOLUMNFLAGS.WRITE + (int) DBCOLUMNFLAGS.WRITEUNKNOWN;
					bool isReadOnly = (columnFlags & flags) == 0;
					fm.IsReadOnly = isReadOnly;

					// BROKEN (expected bitmask value is never set)
					// set whether column is auto-generated
					//flags = (int) DBCOLUMNFLAGS.ISROWID;
					//bool isAutoGenerated = (columnFlags & flags) != 0;
					//fm.SetIsAutoGenerated( isAutoGenerated );				
				}
				else // raise an error if we've detected a database/type mismatch
				{
					bool hasDefault = Convert.ToBoolean( row[ "COLUMN_HASDEFAULT" ] );
					// TODO disabled due to code restructuring 
					Check.Verify( isNullable || hasDefault, Error.NoPropertyForNotNullColumn, columnName, map.TableName );
				}
			}
		}

		private void GetPrimaryKeyData()
		{
			// get PKs for all tables
			DataTable dt = GetPrimaryKeys( null );
			foreach( DataRow row in dt.Rows )
			{
				// TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME
				// COLUMN_NAME, COLUMN_GUID, COLUMN_PROPID, ORDINAL, PK_NAME
				string tableName = (string) row[ "TABLE_NAME" ];
				string columnName = (string) row[ "COLUMN_NAME" ];

				TableMap map = GetTableMap( tableName );
				if( map != null )
				{
					FieldMap fm = map.GetFieldMapFromColumn( columnName );
					if( fm != null )
					{
						fm.SetIsPrimaryKey( true );
						// determine whether PK is auto-generated
						DataTable pkInfo = GetPrimaryKeyInfo( map.QuotedTableName );
						foreach( DataRow dr in pkInfo.Rows )
						{
							string column = dr[ "ColumnName" ].ToString();
							if( column == columnName )
							{
								bool isAutoGenerated = Convert.ToBoolean( dr[ "IsAutoIncrement" ] );
								fm.SetIsAutoGenerated( isAutoGenerated );
							}
						}
					}
				}
			}
		}

		private void GetForeignKeyData()
		{
			// get FKs for all tables
			DataTable dt = GetForeignKeys( null );
			foreach( DataRow row in dt.Rows )
			{
				// PK_TABLE_CATALOG, PK_TABLE_SCHEMA, PK_TABLE_NAME, FK_TABLE_CATALOG, FK_TABLE_SCHEMA, FK_TABLE_NAME,
				// FK_COLUMN_NAME, FK_COLUMN_GUID, FK_COLUMN_PROPID, UPDATE_RULE, DELETE_RULE, 
				// PK_NAME, FK_NAME, DEFERRABILITY 
				string fkTableName = (string) row[ "FK_TABLE_NAME" ];
				string fkColumnName = (string) row[ "FK_COLUMN_NAME" ];
				string pkTableName = (string) row[ "PK_TABLE_NAME" ];
				string pkColumnName = (string) row[ "PK_COLUMN_NAME" ];

				TableMap map = GetTableMap( fkTableName );
				if( map != null )
				{
					FieldMap fm = map.GetFieldMapFromColumn( fkColumnName );
					if( fm != null )
					{
						fm.SetForeignKeyTableName( pkTableName );
						fm.SetForeignKeyColumnName( pkColumnName );
					}
				}
			}
		}
	}
}