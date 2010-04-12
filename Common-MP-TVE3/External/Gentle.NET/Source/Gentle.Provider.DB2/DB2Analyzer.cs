/*
 * DB2 database schema analyzer
 * Copyright (C) 2005 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: DB2Analyzer.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.DB2
{
	public class DB2Analyzer : GentleAnalyzer
	{
		public DB2Analyzer( IGentleProvider provider ) : base( provider )
		{
		}

		private string selectTables = "TODO";

		public override ColumnInformation AnalyzerCapability
		{
			get { return ColumnInformation.ciLocal; }
		}

		public override void Analyze( string tableName )
		{
			try
			{
				bool isSingleRun = tableName != null;
				SqlStatement stmt = broker.GetStatement( selectTables );
				stmt.StatementType = StatementType.Select;
				SqlResult sr = stmt.Execute();
				for( int i = 0; i < sr.Rows.Count; i++ )
				{
					try
					{
						string dbTableName = sr.GetString( i, 0 );
						if( ! isSingleRun || tableName.ToLower().Equals( dbTableName.ToLower() ) )
						{
							TableMap map = GetTableMap( dbTableName );
							if( map == null )
							{
								map = new TableMap( provider, dbTableName );
								maps[ dbTableName.ToLower() ] = map;
							}
							// TODO: get column information for this table

							// TODO: get foreign key information

							// abort loop if analyzing single table only
							if( isSingleRun )
							{
								break;
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
			}
			catch( Exception e )
			{
				Check.Fail( e, Error.Unspecified, "An error occurred while analyzing the database schema." );
			}
		}
	}
}