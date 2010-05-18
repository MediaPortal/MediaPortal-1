/*
 * Helper class used to wrap cache entries
 * Copyright (C) 2006 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CacheEntry.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common
{
	/// <summary>
	/// This class is used to wrap entries stored in the cache.
	/// </summary>
	internal class CacheEntry
	{
		private string key;
		private WeakReference data;
		private object dataStrongRef;

		#region Construction
		public CacheEntry( string key, object value, CacheStrategy strategy )
		{
			this.key = key;
			data = new WeakReference( value );
			if( strategy == CacheStrategy.Permanent )
			{
				dataStrongRef = value;
			}
		}
		#endregion

		#region Properties
		public string Key
		{
			get { return key; }
		}
		public object Value
		{
			get { return dataStrongRef != null ? dataStrongRef : data.Target; }
			set { data.Target = value; }
		}
		public bool IsCollectable
		{
			get { return dataStrongRef == null; }
		}
		public bool IsCollected
		{
			get { return ! data.IsAlive; }
		}
		#endregion
	}
}