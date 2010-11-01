/*
 * Singleton used to store various performance metrics
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleStatistics.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Globalization;

namespace Gentle.Common
{
	/// <summary>
	/// Singleton used to store various statistical information (counters).
	/// </summary>
	public class GentleStatistics
	{
		private static object statsLock = new object();
		private static long cacheHits;
		private static long cacheMisses;
		private static long skippedQueries;
		private static long uniqingCount;

		private GentleStatistics()
		{
		}

		/// <summary>
		/// Return the number of successful attempts at looking up something
		/// in the cache.
		/// </summary>
		public static long CacheHits
		{
			get
			{
				lock( statsLock )
				{
					return cacheHits;
				}
			}
			set
			{
				lock( statsLock )
				{
					cacheHits = value;
				}
			}
		}

		/// <summary>
		/// Return the number of unsuccessful attempts at looking up something
		/// in the cache.
		/// </summary>
		public static long CacheMisses
		{
			get
			{
				lock( statsLock )
				{
					return cacheMisses;
				}
			}
			set
			{
				lock( statsLock )
				{
					cacheMisses = value;
				}
			}
		}

		/// <summary>
		/// Return the number of successful attempts at object uniqing. This
		/// number effectively represents the number of object constructions
		/// skipped due to a cache hit.
		/// </summary>
		public static long UniqingCount
		{
			get
			{
				lock( statsLock )
				{
					return uniqingCount;
				}
			}
			set
			{
				lock( statsLock )
				{
					uniqingCount = value;
				}
			}
		}

		/// <summary>
		/// Return the number of successful attempts at resolving a query
		/// without contacting the database.
		/// </summary>
		public static long SkippedQueries
		{
			get
			{
				lock( statsLock )
				{
					return skippedQueries;
				}
			}
			set
			{
				lock( statsLock )
				{
					skippedQueries = value;
				}
			}
		}

		/// <summary>
		/// Return the number of items currently in the cache.
		/// </summary>
		public static int CacheSize
		{
			get { return CacheManager.Count; }
		}

		/// <summary>
		/// Reset all counters to 0.
		/// </summary>
		public static void Reset( LogCategories category )
		{
			lock( statsLock )
			{
				if( (category & LogCategories.Cache) != 0 )
				{
					cacheHits = 0;
					cacheMisses = 0;
					skippedQueries = 0;
					uniqingCount = 0;
				}
			}
		}

		/// <summary>
		/// Emit current statistics for the specified categories.
		/// </summary>
		/// <param name="category">The categories to emit statistics for.</param>
		public static void LogStatistics( LogCategories category )
		{
			if( Check.IsLogEnabled( LogCategories.Cache ) )
			{
				lock( statsLock )
				{
					long accessCount = cacheHits + cacheMisses;
					double hitRatio = accessCount > 0 ? (double) cacheHits / accessCount : 0;
					// format output using the current culture (if possible; cannot use neutral cultures)
					string ratio;
					if( ! CultureInfo.CurrentCulture.IsNeutralCulture )
					{
						NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
						ratio = String.Format( nfi, "{0:p}", hitRatio );
					}
					else
					{
						ratio = String.Format( "{0:p}", hitRatio );
					}
					Check.LogInfo( LogCategories.Cache, "Cache size/hits/misses: {0}/{1}/{2} ({3} hit ratio)",
					               CacheSize, cacheHits, cacheMisses, ratio );
					Check.LogInfo( LogCategories.Cache, "Cache (objects uniqued): {0}", uniqingCount );
					Check.LogInfo( LogCategories.Cache, "Cache (queries skipped): {0}", skippedQueries );
				}
			}
		}
	}
}