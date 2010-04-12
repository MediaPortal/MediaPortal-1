/*
 * Enumeration of possible cache lifetime management strategies
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CacheStrategy.cs 646 2005-02-21 20:28:03Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// This enumeration is used to apply a caching strategy to objects.
	/// </summary>
	public enum CacheStrategy
	{
		/// <summary>
		/// This value indicates that caching is disabled.
		/// </summary>
		Never,
		/// <summary>
		/// This value indicates that caching is enabled, and that cached objects may be
		/// collected and released at will by the garbage collector. 
		/// This is the default, but can be overridden by setting the 
		/// Options/Cache/DefaultCacheStrategy key in the configuration file.
		/// </summary>
		Temporary,
		/// <summary>
		/// This value indicates that caching is enabled, and that cached objects may not
		/// be garbage collected. The developer must manually ensure that objects are 
		/// removed from the cache when they are no longer needed.
		/// This strategy is applied to the default PersistenceBroker instance used by
		/// the Broker class in order to ensure it is never garbage collected.
		/// </summary>
		Permanent
	} ;
}