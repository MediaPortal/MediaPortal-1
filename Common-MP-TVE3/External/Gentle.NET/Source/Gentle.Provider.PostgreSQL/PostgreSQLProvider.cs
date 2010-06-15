/*
 * PostgreSQL specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: PostgreSQLProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using Npgsql;

namespace Gentle.Provider.PostgreSQL
{
	/// <summary>
	/// Please refer to the documentation of <see cref="GentleProvider"/> and the
	/// <see cref="IGentleProvider"/> interface it implements for details. 
	/// </summary>
	public class PostgreSQLProvider : GentleProvider
	{
		private PostgreSQLAnalyzer analyzer;

		/// <summary>
		/// Construct a new IPersistenceEngine instance for the PostgreSQL rdbms.
		/// </summary>
		/// <param name="connectionString">The connection string to use for 
		/// connecting to the database engine.</param>
		public PostgreSQLProvider( string connectionString ) : base( "PostgreSQL", connectionString )
		{
		}

		/// <summary>
		/// Construct a new IPersistenceEngine instance for the PostgreSQL rdbms.
		/// </summary>
		/// <param name="connectionString">The connection string to use for 
		/// connecting to the database engine.</param>
		/// <param name="schemaName">The schema name to use with this database provider instance.</param>
		public PostgreSQLProvider( string connectionString, string schemaName ) : base( "PostgreSQL",
		                                                                                connectionString,
		                                                                                schemaName )
		{
		}

		#region IGentleProvider
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleSqlFactory GetSqlFactory()
		{
			return new PostgreSQLFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			if( analyzer == null )
			{
				analyzer = new PostgreSQLAnalyzer( this );
			}
			return analyzer;
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleRenderer GetRenderer()
		{
			return null;
		}
		#endregion

		#region IPersistenceEngine
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IPersistenceEngine"/> interface it implements for details. 
		/// </summary>
		public override IDbCommand GetCommand()
		{
			IDbCommand cmd = new NpgsqlCommand();
			cmd.CommandTimeout = GentleSettings.DefaultCommandTimeout;
			return cmd;
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IPersistenceEngine"/> interface it implements for details. 
		/// </summary>
		public override IDbConnection GetConnection()
		{
			try
			{
				IDbConnection dbc = new NpgsqlConnection( ConnectionString );
				dbc.Open();
				Check.VerifyEquals( dbc.State, ConnectionState.Open, Error.NoNewConnection );
				return dbc;
			}
			catch( GentleException )
			{
				throw; // expose the errors raised by ourselves (i.e. the data framework) in the try block
			}
			catch( Exception e )
			{
				Check.Fail( e, Error.DatabaseUnavailable, Name, ConnectionString );
				throw new GentleException( Error.Unspecified, "Unreachable code" );
			}
		}
		#endregion
	}
}