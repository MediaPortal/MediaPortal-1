/*
 * Sybase ASA specifics
 * Copyright (C) 2004 Uwe Kitzmann
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SybaseProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */
//using iAnywhere.Data.AsaClient; 

using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using Mono.Data.SybaseClient;

namespace Gentle.Provider.Sybase
{
	/// <summary>
	/// This class is an implementation of the GentleProvider class for Sybase.
	/// </summary>
	public class SybaseProvider : GentleProvider
	{
		private SybaseAnalyzer analyzer;

		/// <summary>
		/// Construct a new IGentleProvider instance for Sybase.
		/// </summary>
		/// <param name="connectionString">The connection string to use for 
		/// connecting to the database engine.</param>
		public SybaseProvider( string connectionString ) : base( "Sybase", connectionString )
		{
		}

		#region IGentleProvider
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleSqlFactory GetSqlFactory()
		{
			return new SybaseFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			if( analyzer == null )
			{
				analyzer = new SybaseAnalyzer( this );
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
			IDbCommand cmd = new SybaseCommand();
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
				SybaseConnection sc = new SybaseConnection( ConnectionString );
				//new AsaConnection( connectionString );
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