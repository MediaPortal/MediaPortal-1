/*
 * Interface that allows objects to return a unique hash key
 * Copyright (C) 2006 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ICacheKeyProvider.cs 1020 2006-05-26 21:30:35Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// This interface should be implemented by all types that are cached. It allows the cache
	/// to identify the key for the object which speeds up removal dramatically.
	/// </summary>
	public interface ICacheKeyProvider
	{
		/// <summary>
		/// The key used to identify the item in the cache.
		/// </summary>
		string CacheKey { get; }
	}
}