/*
 * MS SQL Server specifics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SQLServerProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using System.Data.SqlClient;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.SQLServer
{
	/// <summary>
	/// This class is an implementation of the IGentleProvider and IPersistenceEngine 
	/// interfaces for the Microsoft SQL Server RDBMS.
	/// </summary>
	public class SQLServerProvider : GentleProvider
	{
		private IProviderInformation providerInformation;
		protected SQLServerAnalyzer analyzer;

		/// <summary>
		/// Construct a new IGentleProvider instance for the MS SQL Server rdbms.
		/// </summary>
		/// <param name="connectionString">The connection string to use for 
		/// connecting to the database engine.</param>
		public SQLServerProvider( string connectionString ) : base( "SQLServer", connectionString )
		{
		}

		#region IGentleProvider
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override IProviderInformation ProviderInformation
		{
			get
			{
				if( providerInformation == null )
				{
					providerInformation = new SQLServerProviderInformation( this );
				}
				return providerInformation;
			}
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleSqlFactory GetSqlFactory()
		{
			return new SQLServerFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			if( analyzer == null )
			{
				analyzer = new SQLServerAnalyzer( this );
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
			IDbCommand cmd = new SqlCommand();
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
				SqlConnection sc = new SqlConnection( ConnectionString );
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
				Check.LogInfo( LogCategories.General, "Using provider {0} and connectionString {1}.", Name, ConnectionString );
				Check.Fail( e, Error.DatabaseUnavailable, Name, ConnectionString );
				throw new GentleException( Error.Unspecified, "Unreachable code" );
			}
		}
		#endregion
	}
}