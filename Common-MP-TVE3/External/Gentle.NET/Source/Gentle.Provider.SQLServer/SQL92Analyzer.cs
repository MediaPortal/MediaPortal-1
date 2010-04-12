/*
 * SQL92 standard database schema analyzer
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SQL92Analyzer.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.SQLServer
{
	/// <summary>
	/// This class is a caching database analyzer. When first created it will build a cache of
	/// all found tables and populate an TableMap with as much information as is available.
	/// This class has been tested only with MS SQL Server. While SQL92 is a standard it is 
	/// likely that the contents returned by other databases (supporting the information_schema
	/// views) will return different values. Whenever a difference is encountered the appropriate
	/// methods for parsing the data should be added to the SqlFactory and descendants, keeping
	/// this class as generic as possible.
	/// </summary>
	public abstract class SQL92Analyzer : GentleAnalyzer
	{
		protected SQL92Analyzer( IGentleProvider provider ) : base( provider )
		{
		}

		private const string select =
			"select c.TABLE_NAME as TableName, c.COLUMN_NAME as ColumnName, c.DATA_TYPE as Type, " +
			" c.CHARACTER_MAXIMUM_LENGTH as Size, c.IS_NULLABLE as IsNullable, " +
			" c.COLUMN_DEFAULT as DefaultValue, ccu.CONSTRAINT_NAME as ConstraintName, " +
			" rc.UNIQUE_CONSTRAINT_NAME as ConstraintReference, " +
			" rc.UPDATE_RULE as UpdateRule, rc.DELETE_RULE as DeleteRule, " +
			" tc.CONSTRAINT_TYPE as ConstraintType, t.TABLE_TYPE as TableType " +
			"from INFORMATION_SCHEMA.COLUMNS c " +
			"inner join INFORMATION_SCHEMA.TABLES t " +
			" on c.TABLE_NAME = t.TABLE_NAME " +
			"left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu " +
			" on c.TABLE_NAME = ccu.TABLE_NAME and c.COLUMN_NAME = ccu.COLUMN_NAME " +
			"left outer join INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc " +
			" on ccu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME " +
			"left outer join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc " +
			" on ccu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME " +
			"where t.TABLE_NAME != 'dtproperties'";
		//"where t.TABLE_TYPE = 'BASE TABLE' and t.TABLE_NAME != 'dtproperties'";
		private const string selectSingle = " and t.TABLE_NAME = '{0}'";
		private const string selectReferences =
			"select c.TABLE_NAME as TableName, c.COLUMN_NAME as ColumnName " +
			"from INFORMATION_SCHEMA.COLUMNS c " +
			"inner join INFORMATION_SCHEMA.TABLES t " +
			" on c.TABLE_NAME = t.TABLE_NAME " +
			"left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu " +
			" on c.TABLE_NAME = ccu.TABLE_NAME and c.COLUMN_NAME = ccu.COLUMN_NAME " +
			"where t.TABLE_TYPE = 'BASE TABLE' and ccu.CONSTRAINT_NAME = '{0}'";
		private const string selectViewDependencies = "exec sp_depends[{0}]";

		private static bool GetBoolean( string boolean )
		{
			string[] valids = new[] { "yes", "true", "1" };
			boolean = boolean == null ? "false" : boolean.ToLower();
			bool result = false;
			foreach( string valid in valids )
			{
				result |= valid.Equals( boolean );
			}
			return result;
		}

		/// <summary>
		/// Please refer to the <see cref="GentleAnalyzer"/> class and the <see cref="IDatabaseAnalyzer"/> 
		/// interface it implements a description of this method.
		/// </summary>
		public override void Analyze( string tableName )
		{
			GentleSqlFactory sf = provider.GetSqlFactory();
			try
			{
				bool isSingleRun = tableName != null;
				// don't quote reserved words here (table name is just a string parameter)
				string sql = isSingleRun ? select + String.Format( selectSingle, tableName ) : select;
				SqlResult sr = broker.Execute( sql, null, null );
				// process result set using columns:
				// TableName, ColumnName, Type, Size, IsNullable, DefaultValue, 
				// ConstraintName, ConstraintReference, ConstraintType, 
				// UpdateRule, DeleteRule, TableType
				for( int i = 0; i < sr.Rows.Count; i++ )
				{
					try
					{
						string dbTableName = sr.GetString( i, "tablename" );
						if( ! isSingleRun || tableName.ToLower().Equals( dbTableName.ToLower() ) )
						{
							TableMap map = GetTableMap( dbTableName );
							if( map == null )
							{
								map = new TableMap( provider, dbTableName );
								maps[ dbTableName.ToLower() ] = map;
							}
							map.IsView = sr.GetString( i, "TableType" ) == "VIEW";
							// get column information for this table
							string columnName = sr.GetString( i, "ColumnName" );
							FieldMap fm = map.GetFieldMapFromColumn( columnName );
							if( fm == null )
							{
								fm = new FieldMap( map, columnName );
								map.Fields.Add( fm );
							}
							// get basic column information
							fm.SetDbType( sr.GetString( i, "Type" ), false ); // sql server is always false
							fm.SetIsNullable( GetBoolean( sr.GetString( i, "IsNullable" ) ) );
							fm.SetIsAutoGenerated( sr.GetString( i, "DefaultValue" ).Length > 0 ? true : false );
							if( sr[ i, "Size" ] != null && fm.DbType != (long) SqlDbType.Text )
							{
								fm.SetSize( sr.GetInt( i, "Size" ) );
							}
							// get column constraint infomation
							if( sr[ i, "ConstraintName" ] != null )
							{
								string type = sr.GetString( i, "ConstraintType" );
								if( type.ToLower().Equals( "primary key" ) )
								{
									fm.SetIsPrimaryKey( true );
								}
								else if( type.ToLower().Equals( "foreign key" ) )
								{
									string conref = sr.GetString( i, "ConstraintReference" );
									if( conref.StartsWith( "IDX" ) )
									{
										string fkRef = sr.GetString( i, "ConstraintName" );
										if( fkRef != null && fkRef.StartsWith( "FK" ) )
										{
											conref = fkRef;
										}
									}
									SqlResult res = broker.Execute( String.Format( selectReferences, conref ), null, null );
									if( res.ErrorCode == 0 && res.RowsContained == 1 )
									{
										fm.SetForeignKeyTableName( res.GetString( 0, "TableName" ) );
										fm.SetForeignKeyColumnName( res.GetString( 0, "ColumnName" ) );
									}
									else
									{
										if( res.RowsContained == 0 )
										{
											// see GOPF-155 for additional information
											Check.LogWarning( LogCategories.Metadata,
											                  "Unable to obtain foreign key information for column {0} of table {1}.",
											                  fm.ColumnName, map.TableName );
										}
										else
										{
											Check.LogWarning( LogCategories.Metadata, "Gentle 1.x does not support composite foreign keys." );
										}
									}
								}
							}
							if( map.IsView )
							{
								// TODO 
								// process backing table members and infer PK/identity info
								// requires that tables be processed before views!
								// 
								//string sv = String.Format( selectViewDependencies, map.TableName );
								//SqlResult res = broker.Execute( sv );
							}
						}
					}
					catch( GentleException fe )
					{
						// ignore errors caused by tables found in db but for which no map exists
						// TODO this should be a config option
						if( fe.Error != Error.NoObjectMapForTable )
						{
							throw;
						}
					}
				}
			}
			catch( Exception e )
			{
				Check.LogInfo( LogCategories.General, "Using provider {0} and connectionString {1}.",
				               provider.Name, provider.ConnectionString );
				Check.Fail( e, Error.Unspecified, "An error occurred while analyzing the database schema." );
			}
		}
	}
}