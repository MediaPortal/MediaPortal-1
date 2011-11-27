/*
 * MS Access (Jet) specifics
 * Copyright (C) 2004 Vinicius (Vinny) A. DaSilva
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: JetProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Data;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using Gentle.Common;
using Gentle.Framework;

namespace Gentle.Provider.Jet
{
	/// <summary>
	/// This class is an implementation of the IPersistenceEngine interface for the 
	/// Microsoft Jet Database Engine
	/// </summary>
	public class JetEngine : GentleProvider
	{
		private JetAnalyzer analyzer;

		public JetEngine( string connectionString ) : base( "Jet", connectionString )
		{
		}

		#region IGentleProvider
		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleSqlFactory GetSqlFactory()
		{
			return new JetFactory( this );
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="GentleProvider"/> and the
		/// <see cref="IGentleProvider"/> interface it implements for details. 
		/// </summary>
		public override GentleAnalyzer GetAnalyzer()
		{
			if( analyzer == null )
			{
				analyzer = new JetAnalyzer( this );
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
			IDbCommand cmd = new OleDbCommand();
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
				OleDbConnection sc = new OleDbConnection( ConnectionString );
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

		[DllImport( "ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false )]
		private static extern Guid CLSIDFromString( string lpsz );
	}
}