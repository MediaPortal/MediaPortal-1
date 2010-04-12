/*
 * The interface for database providers.
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SQLServerProviderInformation.cs 1226 2008-03-13 22:16:41Z mm $
 */
using System;
using System.Data;
using System.Data.SqlClient;
using Gentle.Framework;
using StatementType=Gentle.Framework.StatementType;

namespace Gentle.Provider.SQLServer
{
	/// <summary>
	/// The class encapsulates information about the underlying database provider,
	/// as well as the logic to extract it.
	/// </summary>
	public class SQLServerProviderInformation : IProviderInformation
	{
		private const string xp_msverProcedureName = "master..xp_msver";
		private const string VersionProperty = "ProductVersion";
		private const string NameProperty = "ProductName";

		private SQLServerProvider provider;
		private bool xp_msverInitialized;
		// provide defaults so we can run test cases with other backends
		private string name = "SQLServer";
		private Version version = new Version( 1, 0, 0 );

		public SQLServerProviderInformation( SQLServerProvider provider )
		{
			this.provider = provider;
		}

		#region IProviderInformation
		/// <summary>
		/// Please refer to the documentation of <see cref="IProviderInformation"/>
		/// for details.
		/// </summary>
		public string Name
		{
			get
			{
				InitializeXpMsVer();
				return name;
			}
		}

		/// <summary>
		/// Please refer to the documentation of <see cref="IProviderInformation"/>
		/// for details.
		/// </summary>
		public Version Version
		{
			get
			{
				InitializeXpMsVer();
				return version;
			}
		}

		private void InitializeXpMsVer()
		{
			// permit disabling of these checks
			if( xp_msverInitialized || GentleSettings.AnalyzerLevel == AnalyzerLevel.None )
			{
				return;
			}
			try
			{
				SqlCommand cmd = provider.GetCommand() as SqlCommand;
				cmd.CommandType = CommandType.StoredProcedure;
				// retrieve name
				cmd.Parameters.AddWithValue( "@optname", NameProperty );
				SqlStatement stmt = new SqlStatement( StatementType.Unknown, cmd, xp_msverProcedureName );
				SqlResult sr = provider.ExecuteStatement( stmt );
				name = sr.GetString( 0, 3 );
				// retrieve version
				cmd.Parameters[ "@optname" ].Value = VersionProperty;
				stmt = new SqlStatement( StatementType.Unknown, cmd, xp_msverProcedureName );
				sr = provider.ExecuteStatement( stmt );
				version = new Version( sr.GetString( 0, 3 ) );
			}
			finally
			{
				xp_msverInitialized = true;
			}
		}
		#endregion
	}
}