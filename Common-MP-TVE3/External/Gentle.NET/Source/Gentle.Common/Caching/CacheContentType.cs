/*
 * Manager class used to interface with the Cache class
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CacheContentType.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// Enumeration of possible content types in the cache. Every content type has
	/// its own HybridDictionary in order to increase performance for sequential
	/// operations (such as when invoking CacheManager.ClearByType).
	/// </summary>
	public enum CacheContentType
	{
		/// <summary>
		/// Queries and query results.
		/// </summary>
		Query,
		/// <summary>
		/// Entities (and all other types).
		/// </summary>
		Entity
	}
}