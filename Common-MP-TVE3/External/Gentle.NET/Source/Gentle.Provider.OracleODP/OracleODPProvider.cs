/*
 * Oracle specifics
 * Copyright (C) 2004 Andreas Seibt 
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: OracleODPProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using Oracle.DataAccess.Client;

namespace Gentle.Provider.OracleODP
{
	/// <summary>
	/// Please refer to the documentation of <see cref="GentleProvider"/> and the
	/// <see cref="IGentleProvider"/> interface it implements for details. 
	/// </summary>
	public class OracleODPProvider : GentleProvider
	{
		private OracleODPAnalyzer analyzer;

		/// <summary>
		/// Construct a new IPersistenceEngine instance for the MS SQL Server rdbms.
		/// </summary>
		/// <param name="connectionString">The connection string to use for 
		/// connecting to the database engine.</param>
		public OracleODPProvider( string connectionString ) : base( "OracleODP", connectionString )
		{
		}

		#region IGentleProvider
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleSqlFactory GetSqlFactory()
		{
			return new OracleODPFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			if( analyzer == null )
			{
				analyzer = new OracleODPAnalyzer( this );
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
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override IDbCommand GetCommand()
		{
			OracleCommand cmd = new OracleCommand();
			cmd.BindByName = true;
			cmd.CommandTimeout = GentleSettings.DefaultCommandTimeout;
			return cmd;
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override IDbConnection GetConnection()
		{
			try
			{
				OracleConnection sc = new OracleConnection( ConnectionString );
				sc.Open();
				Check.VerifyEquals( sc.State, ConnectionState.Open, Error.NoNewConnection );
				return sc;
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