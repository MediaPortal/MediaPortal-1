/*
 * Firebird specifics
 * Copyright (C) 2004 Carlos Guzmán Álvarez
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: FirebirdProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.Firebird
{
	/// <summary>
	/// This class is an implementation of the GentleProvider class for the Firebird RDBMS.
	/// </summary>
	public class FirebirdProvider : GentleProvider
	{
		// This field stores optional information about the underlying database provider (name and version).
		private IProviderInformation providerInformation;

		public FirebirdProvider( string connectionString ) : base( "Firebird", connectionString )
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
					providerInformation = new FirebirdProviderInformation( this );
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
			return new FirebirdFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			return new FirebirdAnalyzer( this );
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
			IDbCommand cmd = new FbCommand();
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
				IDbConnection dbc = new FbConnection( ConnectionString );
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