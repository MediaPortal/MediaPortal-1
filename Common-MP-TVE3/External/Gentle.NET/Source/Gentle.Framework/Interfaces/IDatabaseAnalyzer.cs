/*
 * The database schema analyzer interface for SQL engines
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: IDatabaseAnalyzer.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Collections;

namespace Gentle.Framework
{
	/// <summary>
	/// This enumeration is used to control the level of database analysis performed.
	/// </summary>
	public enum AnalyzerLevel
	{
		/// <summary>
		/// With this setting no database analysis will be performed.
		/// </summary>
		None,
		/// <summary>
		/// With this setting only tables with a corresponding object will be analyzed.
		/// </summary>
		OnDemand,
		/// <summary>
		/// With this setting all available tables will be analyzed.
		/// </summary>
		Full
	}

	/// <summary>
	/// This interface describes the methods which must be supported by a specific
	/// analyzer for a persistence engine. The analyzers are used to obtain column
	/// type, size and constraint information directly from the database
	/// </summary>
	public interface IDatabaseAnalyzer
	{
		/// <summary>
		/// This property is a Hashtable of TableMap instances containing metadata on
		/// tables obtained from the database. Client programs can iterate through this
		/// list and use the TableMap instances to generate Gentle business objects (be
		/// sure to set the AnalyzerLevel to Full in the configuration file if used for
		/// this purpose).
		/// </summary>
		Hashtable TableMaps { get; }

		/// <summary>
		/// This method obtains metadata by analyzing the database. If no table name
		/// is specified all available tables will be processed.
		/// </summary>
		/// <param name="tableName">The table name to analyze or null for all tables</param>
		void Analyze( string tableName );

		/// <summary>
		/// This method updated the given ObjectMap instance with metadata obtained
		/// from the database.
		/// </summary>
		/// <param name="map">The ObjectMap to update</param>
		void UpdateObjectMap( ObjectMap map );
	}
}