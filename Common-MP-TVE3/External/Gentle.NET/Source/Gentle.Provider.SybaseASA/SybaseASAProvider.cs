/*
 * Sybase ASA specifics
 * Copyright (C) 2004 Uwe Kitzmann
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SybaseASAProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System;
using System.Data;
using Gentle.Common;
using Gentle.Framework;
using iAnywhere.Data.AsaClient;

namespace Gentle.Provider.SybaseASA
{
	/// <summary>
	/// This class is an implementation of the GentleProvider class for Sybase.
	/// </summary>
	public class SybaseASAProvider : GentleProvider
	{
		private SybaseASAAnalyzer analyzer;

		/// <summary>
		/// Construct a new IGentleProvider instance for Sybase.
		/// </summary>
		/// <param name="connectionString">The connection string to use for 
		/// connecting to the database engine.</param>
		public SybaseASAProvider( string connectionString ) : base( "SybaseASA", connectionString )
		{
		}

		#region IGentleProvider
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleSqlFactory GetSqlFactory()
		{
			return new SybaseASAFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			if( analyzer == null )
			{
				analyzer = new SybaseASAAnalyzer( this );
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
			IDbCommand cmd = new AsaCommand();
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
				AsaConnection sc = new AsaConnection( ConnectionString );
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