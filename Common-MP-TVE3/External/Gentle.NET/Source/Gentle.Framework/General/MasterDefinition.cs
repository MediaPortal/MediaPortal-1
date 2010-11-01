/*
 * Enumeration of possible metadata sources
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MasterDefinition.cs 646 2005-02-21 20:28:03Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// This enumeration is used to control which source of information is to be considered
	/// the master definition. This setting is only partially used at the moment. 
	/// </summary>
	public enum MasterDefinition
	{
		/// <summary>
		/// With this setting the mapping structures are defined using attributes. The definitions
		/// are verified against the database in accordance with the AnalyzerLevel setting.
		/// </summary>
		Attributes,
		/// <summary>
		/// With this setting the mapping structures are defined using definitions from XML files.
		/// This feature is not yet supported.
		/// </summary>
		XML,
		/// <summary>
		/// With this setting the mapping structures are defined by analyzing the database tables.
		/// This feature is not yet supported.
		/// </summary>
		Database
	}
}