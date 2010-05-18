/*
 * Manager class used to interface with the Cache class
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CacheManager.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Collections.Specialized;

namespace Gentle.Common
{
	/// <summary>
	/// Manager class uses to interface with the underlying cache.
	/// </summary>
	public class CacheManager
	{
		#region Members
		private static int rwLockTimeOut = 5000;
		[Configuration( "Gentle.Framework/Options/Cache/UniqingScope", ConfigKeyPresence.Optional )]
		private static UniqingScope scope = UniqingScope.Application;
		private static HybridDictionary stores = new HybridDictionary();
		#endregion

		#region Constructors (private to prevent instantiation)
		private CacheManager()
		{
		}

		static CacheManager()
		{
			Configurator.Configure( typeof(CacheManager) );
		}
		#endregion

		#region CacheStore Management
		private static CacheStore GetCacheStore( CacheContentType contentType )
		{
			return GetCacheStore( scope, null, contentType );
		}

		private static CacheStore GetCacheStore( UniqingScope uniqingScope, string scopeDelimiter, CacheContentType contentType )
		{
			// make sure we always get the global cache if scope is global
			if( scope == UniqingScope.Application )
			{
				scopeDelimiter = null;
			}
			else if( scopeDelimiter == null )
			{
				scopeDelimiter = GetScopeDelimiter();
			}
			// determine which cache to use - first get array of cache stores for the specified scope
			string scopeKey = String.Format( "{0}|{1}", uniqingScope, scopeDelimiter );
			CacheStore[] cacheStores = stores[ scopeKey ] as CacheStore[];
			if( cacheStores == null )
			{
				cacheStores = new CacheStore[Enum.GetNames( typeof(CacheContentType) ).Length];
				stores[ scopeKey ] = cacheStores;
			}
			// get specific cache store by content type
			CacheStore cacheStore = cacheStores[ (int) contentType ];
			if( cacheStore == null )
			{
				cacheStore = new CacheStore();
				cacheStores[ (int) contentType ] = cacheStore;
			}
			return cacheStore;
		}

		private static CacheStore[] GetCacheStores( UniqingScope uniqingScope, string scopeDelimiter )
		{
			// make sure we always get the global cache if scope is global
			if( scope == UniqingScope.Application )
			{
				scopeDelimiter = null;
			}
			else if( scopeDelimiter == null )
			{
				scopeDelimiter = GetScopeDelimiter();
			}
			// determine which cache to use
			string scopeKey = String.Format( "{0}|{1}", uniqingScope, scopeDelimiter );
			CacheStore[] cacheStores = stores[ scopeKey ] as CacheStore[];
			if( cacheStores == null )
			{
				cacheStores = new CacheStore[Enum.GetNames( typeof(CacheContentType) ).Length];
				stores[ scopeKey ] = cacheStores;
			}
			return cacheStores;
		}
		#endregion

		#region Cache Retrieval (Get Methods)
		/// <summary>
		/// Retrieve an entry from the cache using the specified key.
		/// </summary>
		/// <param name="key">The key linked to the cache entry.</param>
		/// <returns>The cached object (or null if no entry was found).</returns>
		public static object Get( string key )
		{
			Check.VerifyNotNull( key, Error.NullParameter, "key" );
			object result = null;
			// determine which cache to use
			CacheContentType contentType = key.StartsWith( "Query|" ) ? CacheContentType.Query : CacheContentType.Entity;
			CacheStore cacheStore = GetCacheStore( contentType );
			// access cache
			cacheStore.Lock.AcquireReaderLock( rwLockTimeOut );
			try
			{
				result = cacheStore.Get( key );
			}
			finally
			{
				cacheStore.Lock.ReleaseReaderLock();
			}
			// logging
			Check.LogDebug( LogCategories.Cache, "Cache (get) using key: {0} ({1})", key, result != null ? "hit" : "miss" );
			// statistics
			if( result != null )
			{
				GentleStatistics.CacheHits += 1;
			}
			else
			{
				GentleStatistics.CacheMisses += 1;
			}
			return result;
		}
		#endregion

		#region Cache Insertion
		/// <summary>
		/// Insert an entry into the cache using <see cref="CacheStrategy.Temporary"/>.
		/// </summary>
		/// <param name="obj">The item to add to the cache.</param>
		public static void Insert( ICacheKeyProvider obj )
		{
			Insert( obj.CacheKey, obj, CacheStrategy.Temporary );
		}

		/// <summary>
		/// Insert an entry into the cache using <see cref="CacheStrategy.Temporary"/>.
		/// </summary>
		/// <param name="key">The key used to find the entry again.</param>
		/// <param name="value">The item to add to the cache.</param>
		public static void Insert( string key, object value )
		{
			Insert( key, value, CacheStrategy.Temporary );
		}

		/// <summary>
		/// Insert an entry into the cache using the specified <see cref="CacheStrategy"/>.
		/// </summary>
		/// <param name="key">The key used to find the entry again.</param>
		/// <param name="value">The item to cache.</param>
		/// <param name="strategy">The cache strategy to use for lifetime management. Possible
		/// values are Never, Temporary or Permanent.</param>
		public static void Insert( string key, object value, CacheStrategy strategy )
		{
			Check.VerifyNotNull( key, Error.NullParameter, "key" );
			Check.VerifyNotNull( value, Error.NullParameter, "value" );
			Check.LogDebug( LogCategories.Cache, "Cache (add) using key: " + key );
			CacheContentType contentType = key.StartsWith( "Query|" ) ? CacheContentType.Query : CacheContentType.Entity;
			// determine which cache to use
			CacheStore cacheStore = GetCacheStore( contentType );
			// access cache
			cacheStore.Lock.AcquireWriterLock( rwLockTimeOut );
			try
			{
				cacheStore.Insert( key, value, strategy );
			}
			finally
			{
				cacheStore.Lock.ReleaseWriterLock();
			}
		}
		#endregion

		#region Cache Removal
		/// <summary>
		/// Remove an entry from the cache.
		/// </summary>
		/// <param name="key">The key used to find the entry to remove.</param>
		public static void Remove( string key )
		{
			Check.VerifyNotNull( key, Error.NullParameter, "key" );
			// determine which cache to use
			CacheStore cacheStore = GetCacheStore( CacheContentType.Entity );
			// access cache
			cacheStore.Lock.AcquireWriterLock( rwLockTimeOut );
			try
			{
				cacheStore.Remove( key );
			}
			finally
			{
				cacheStore.Lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Forcefully remove one or more entries from the cache. Note that this
		/// is currently a O(n^2) operation (i.e. it might be slow).
		/// </summary>
		/// <param name="objects">The objects to remove from the cache.</param>
		public static void Remove( params object[] objects )
		{
			Check.VerifyNotNull( objects, Error.NullParameter, "objects" );
			// determine which cache to use
			CacheStore cacheStore = GetCacheStore( CacheContentType.Entity );
			// access cache
			cacheStore.Lock.AcquireWriterLock( rwLockTimeOut );
			try
			{
				if( objects != null && objects.Length > 0 )
				{
					foreach( object obj in objects )
					{
						if( obj is ICacheKeyProvider )
						{
							ICacheKeyProvider ckp = obj as ICacheKeyProvider;
							cacheStore.Remove( ckp.CacheKey );
						}
						else
						{
							cacheStore.RemoveObject( obj );
						}
					}
				}
			}
			finally
			{
				cacheStore.Lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Forcefully remove one or more entries from the cache. Note that this
		/// is currently a O(n^2) operation (i.e. it might be slow).
		/// </summary>
		/// <param name="objects">The list of objects to remove from the cache.</param>
		public static void Remove( IList objects )
		{
			Check.VerifyNotNull( objects, Error.NullParameter, "objects" );
			object[] objs = new object[objects.Count];
			objects.CopyTo( objs, 0 );
			Remove( objs );
		}
		#endregion

		#region Cache Clearing (Batch Removal)
		/// <summary>
		/// Clear all entries belonging to the specified scope from the cache. 
		/// </summary>
		public static void Clear( string scopeDelimiter )
		{
			foreach( CacheContentType contentType in Enum.GetValues( typeof(CacheContentType) ) )
			{
				Clear( scopeDelimiter, contentType );
			}
		}

		/// <summary>
		/// Clear all entries belonging to the specified scope from the cache. 
		/// </summary>
		public static void Clear( string scopeDelimiter, CacheContentType contentType )
		{
			// determine which cache to use
			CacheStore cacheStore = GetCacheStore( scope, scopeDelimiter, contentType );
			// access cache
			cacheStore.Lock.AcquireWriterLock( rwLockTimeOut );
			try
			{
				cacheStore.Clear();
				GentleStatistics.Reset( LogCategories.Cache );
			}
			finally
			{
				cacheStore.Lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Clear all entries in all scopes from the cache. 
		/// </summary>
		public static void Clear()
		{
			stores = new HybridDictionary();
			GentleStatistics.Reset( LogCategories.Cache );
		}

		/// <summary>
		/// Clear all entries belonging to the specified thread from the cache. The thread
		/// identity corresponds to the value of thread.GetHashCode(). Note that this method
		/// only works for threads that do not have a name.
		/// </summary>
		public static void Clear( int threadIdentity )
		{
			Clear( GetScopeDelimiter( threadIdentity.ToString() ), CacheContentType.Query );
			Clear( GetScopeDelimiter( threadIdentity.ToString() ), CacheContentType.Entity );
		}

		/// <summary>
		/// Clear information on query results for select/count queries on the specified type. 
		/// This method is used by Gentle to invalidate obsolete query results when objects 
		/// of the given type are inserted or removed.
		/// </summary>
		public static void ClearQueryResultsByType( Type type )
		{
			// this is a bit hackish but does the job for now
			string match = String.Format( "{0}|{1}|select ", CacheContentType.Query, type );
			// process all cache stores and remove query results if invalidated	
			foreach( CacheStore[] cacheStores in stores.Values )
			{
				CacheStore cacheStore = cacheStores[ (int) CacheContentType.Query ];
				if( cacheStore != null )
				{
					IList removeList = new ArrayList();
					cacheStore.Lock.AcquireWriterLock( rwLockTimeOut );
					try
					{
						// build list of items to remove
						foreach( CacheEntry entry in cacheStore )
						{
							if( entry.Key.Length >= match.Length &&
							    entry.Key.Substring( 0, match.Length ).Equals( match ) )
							{
								removeList.Add( entry.Key );
							}
						}
						// remove found items
						foreach( string key in removeList )
						{
							cacheStore.Remove( key );
						}
					}
					finally
					{
						cacheStore.Lock.ReleaseWriterLock();
					}
				}
			}
		}
		#endregion

		#region Count Accessors
		/// <summary>
		/// Return the number of items currently in the cache. The cache store to use is determined from
		/// the scope parameter (combined with the global UniqingScope setting). If null is passed this
		/// method returns the same as the Count property (current scope).
		/// </summary>
		public static int GetCount( string scopeDelimiter )
		{
			int count = 0;
			foreach( CacheContentType contentType in Enum.GetValues( typeof(CacheContentType) ) )
			{
				count += GetCount( scopeDelimiter, contentType );
			}
			return count;
		}

		/// <summary>
		/// Return the number of items currently in the cache. The cache store to use is determined from
		/// the scope parameter (combined with the global UniqingScope setting). If null is passed this
		/// method returns the same as the Count property (current scope).
		/// </summary>
		public static int GetCount( string scopeDelimiter, CacheContentType contentType )
		{
			CacheStore cacheStore = GetCacheStore( scope, scopeDelimiter, contentType );
			// we need a writer lock because computing the count also removes collected entries
			cacheStore.Lock.AcquireWriterLock( rwLockTimeOut );
			try
			{
				return cacheStore.Count;
			}
			finally
			{
				cacheStore.Lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Return the number of items currently in the cache. Only items for the current UniqingScope 
		/// will be counted.
		/// </summary>
		public static int Count
		{
			get { return GetCount( null ); }
		}
		#endregion

		#region UniqingScope Property and Helper Methods
		/// <summary>
		/// The global scope setting used to separate items in the cache.
		/// </summary>
		public static UniqingScope UniqingScope
		{
			get { return scope; }
			set { scope = value; }
		}

		/// <summary>
		/// Get the scope delimiter used to group entries in the cache. The
		/// supplied id is used instead of the normal scope delimitor (this
		/// is useful if you wish to clear the cache for another thread or
		/// web session than your own).
		/// </summary>
		public static string GetScopeDelimiter( string id )
		{
			switch( scope )
			{
				case UniqingScope.Thread:
				case UniqingScope.WebSession:
					return String.Format( "{0}", id );
				default: // UniqingScope.Application		
					return String.Empty;
			}
		}

		/// <summary>
		/// Get the scope delimiter used to group entries in the cache. The
		/// returned value is valid for this thread or web session only.
		/// </summary>
		public static string GetScopeDelimiter()
		{
			switch( scope )
			{
				case UniqingScope.Thread:
					return GetScopeDelimiter( SystemSettings.ThreadIdentity );
				case UniqingScope.WebSession:
					return GetScopeDelimiter( SystemSettings.WebSessionID );
				default: // UniqingScope.Application		
					return String.Empty;
			}
		}
		#endregion
	}
}