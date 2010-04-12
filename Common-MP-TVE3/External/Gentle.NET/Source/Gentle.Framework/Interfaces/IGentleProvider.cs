/*
 * The interface for database providers.
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IGentleProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// The high-level interface to implement when creating a new provider.
	/// </summary>
	public interface IGentleProvider : IPersistenceEngine
	{
		#region Provider Identity Properties
		/// <summary>
		/// Returns a unique name for this provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns the connection string used by this provider.
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		/// Returns the schema name (if any) used by this provider.
		/// </summary>
		string SchemaName { get; }

		/// <summary>
		/// Returns information about the actual data provider.
		/// </summary>
		IProviderInformation ProviderInformation { get; }

		/// <summary>
		/// Returns the PersistenceBroker instance for this provider.
		/// </summary>
		PersistenceBroker Broker { get; }

		/// <summary>
		/// Returns the cache key of the current instance.
		/// </summary>
		int IdentityHash { get; }
		#endregion

		#region Worker Class Getters
		/// <summary>
		/// Obtain an SQL factory for constructing SQL statements.
		/// </summary>
		/// <returns>The factory for SQL construction</returns>
		GentleSqlFactory GetSqlFactory();

		/// <summary>
		/// Obtain an SQL renderer for constructing formatted SQL from SqlQuery objects.
		/// </summary>
		/// <returns>The SQL renderer for this database</returns>
		GentleRenderer GetRenderer();

		/// <summary>
		/// Obtain an instance of the database backend analyzer. Used internally while constructing
		/// the initial ObjectMap instances for mapping types to the database.
		/// </summary>
		/// <returns>The factory for SQL construction</returns>
		GentleAnalyzer GetAnalyzer();
		#endregion
	}
}