/*
 * Cache implementation (storage of cached objects)
 * Copyright (C) 2006 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CacheStore.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

namespace Gentle.Common
{
	internal sealed class CacheStore : IEnumerable
	{
		private HybridDictionary items = new HybridDictionary();
		private ReaderWriterLock rwLock = new ReaderWriterLock();

		#region IEnumerable
		public IEnumerator GetEnumerator()
		{
			return items.Values.GetEnumerator();
		}
		#endregion

		#region Properties & Indexers
		/// <summary>
		/// The reader-writer lock to use when accessing this CacheStore instance.
		/// </summary>
		public ReaderWriterLock Lock
		{
			get { return rwLock; }
		}

		/// <summary>
		/// Returns the number of items currently stored in the cache. Accessing this property
		/// causes a check of all items in the cache to ensure collected items are not counted.
		/// </summary>
		public int Count
		{
			get { return ClearCollected(); }
		}

		/// <summary>
		/// Indexer for accessing or adding cache entries.
		/// </summary>
		public object this[ string key ]
		{
			get { return Get( key ); }
			set { Insert( key, value ); }
		}

		/// <summary>
		/// Indexer for adding a cache item using the specified strategy.
		/// </summary>
		public object this[ string key, CacheStrategy strategy ]
		{
			set { Insert( key, value, strategy ); }
		}
		#endregion

		#region Insert Methods
		/// <summary>
		/// Insert a collectible object into the cache.
		/// </summary>
		/// <param name="key">The cache key used to reference the item.</param>
		/// <param name="value">The object to be inserted into the cache.</param>
		public void Insert( string key, object value )
		{
			Insert( key, value, CacheStrategy.Temporary );
		}

		/// <summary>
		/// Insert an object into the cache using the specified cache strategy (lifetime management).
		/// </summary>
		/// <param name="key">The cache key used to reference the item.</param>
		/// <param name="value">The object to be inserted into the cache.</param>
		/// <param name="strategy">The strategy to apply for the inserted item (use Temporary for objects 
		/// that are collectible and Permanent for objects you wish to keep forever).</param>
		public void Insert( string key, object value, CacheStrategy strategy )
		{
			Check.VerifyNotNull( key, Error.NullParameter, "key" );
			items[ key ] = new CacheEntry( key, value, strategy );
		}
		#endregion

		#region Get Methods
		/// <summary>
		/// Retrieves an entry from the cache using the given key. If the entry exists but has
		/// been collected then this method will also remove the item from the cache by temporarily
		/// upgrading the supplied lock to a writer lock.
		/// </summary>
		/// <param name="key">The cache key of the item to retrieve.</param>
		/// <returns>The retrieved cache item or null if not found.</returns>
		public object Get( object key )
		{
			Check.VerifyNotNull( key, Error.NullParameter, "key" );
			CacheEntry entry = items[ key ] as CacheEntry;
			if( entry != null )
			{
				if( ! entry.IsCollected )
				{
					return entry.Value;
				}
				else
				{
					if( rwLock != null )
					{
						LockCookie cookie = rwLock.UpgradeToWriterLock( 1000 );
						try
						{
							items.Remove( key );
						}
						finally
						{
							rwLock.DowngradeFromWriterLock( ref cookie );
						}
					}
				}
			}
			return null;
		}

		public CacheEntry GetByValue( object instance )
		{
			Check.VerifyNotNull( instance, Error.NullParameter, "instance" );
			foreach( CacheEntry entry in items.Values )
			{
				if( entry.Value == instance )
				{
					return entry;
				}
			}
			return null;
		}
		#endregion

		#region Remove Methods
		/// <summary>
		/// Removes the object associated with the given key from the cache.
		/// </summary>
		/// <param name="key">The cache key of the item to remove.</param>
		/// <returns>The item removed from the cache or null if nothing was removed.</returns>
		public object Remove( object key )
		{
			if( key != null )
			{
				CacheEntry entry = items[ key ] as CacheEntry;
				if( entry != null )
				{
					items.Remove( key );
					return entry.IsCollected ? null : entry.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// Removes the given object from the cache.
		/// </summary>
		/// <param name="instance">The object to remove.</param>
		public void RemoveObject( object instance )
		{
			CacheEntry entry = GetByValue( instance );
			if( entry != null )
			{
				Remove( entry.Key );
			}
		}
		#endregion

		#region Clear Methods
		/// <summary>
		/// Removes all entries from the cache.
		/// </summary>
		public void Clear()
		{
			items.Clear();
		}

		/// <summary>
		/// Process all items in the cache and remove entries that refer to collected items.
		/// </summary>
		/// <returns>The number of live cache entries still in the cache.</returns>
		private int ClearCollected()
		{
			IList collected = new ArrayList();
			foreach( CacheEntry entry in items.Values )
			{
				if( entry.IsCollected )
				{
					collected.Add( entry );
				}
			}
			foreach( CacheEntry entry in collected )
			{
				items.Remove( entry.Key );
			}
			return items.Count;
		}
		#endregion

		#region ToString
		/// <summary>
		/// This method returns a string with information on the cache contents (number of contained objects).
		/// </summary>
		public override string ToString()
		{
			int count = ClearCollected();
			return count > 0 ? String.Format( "Cache contains {0} live objects.", count ) : "Cache is empty.";
		}
		#endregion
	}
}