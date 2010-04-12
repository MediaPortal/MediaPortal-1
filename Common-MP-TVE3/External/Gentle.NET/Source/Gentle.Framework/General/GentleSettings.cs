/*
 * Global Gentle.NET settings (initialized from the config file)
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleSettings.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Placeholder class for Gentle.NET global configuration settings
	/// </summary>
	public class GentleSettings
	{
		// all custom options read must be nested within this key/tag prefix
		private const string OptionsPrefix = "Gentle.Framework/Options/";
		// log categories bitflags
		private static LogCategories enabledLogCategories = LogCategories.All;

		static GentleSettings()
		{
			Configurator.Configure( typeof(GentleSettings) );
			// log initial statistics 
			GentleStatistics.LogStatistics( enabledLogCategories );
		}

		#region Gentle Global Configuration Settings
		/// <summary>
		/// Option to control whether Gentle should generate SQL for concurrency control. When
		/// set to false, all columns carrying the ConcurrencyColumn will be ignored in query
		/// generation.
		/// </summary>
		[Configuration( OptionsPrefix + "ConcurrencyControl", ConfigKeyPresence.Optional )]
		public static readonly bool ConcurrencyControl;

		/// <summary>
		/// Option to control what the primary metadata source is for Gentle. Note that when
		/// the database analyzer is enabled, information obtained from schema data is assumed
		/// to be correct and thus always takes precedence.
		/// </summary>
		[Configuration( OptionsPrefix + "MasterDefinition", ConfigKeyPresence.Optional )]
		public static readonly MasterDefinition MasterDefinition = MasterDefinition.Attributes;

		/// <summary>
		/// Option to control whether the database analyzer should report an error for
		/// unused columns. The default is to ignore any such unmapped columns.
		/// </summary>
		[Configuration( OptionsPrefix + "Analyzer/Silent", ConfigKeyPresence.Optional )]
		public static readonly bool AnalyzerSilent = true;

		/// <summary>
		/// Option to control the operation of the database analyzer. When set to None the
		/// database anaylzer will be disabled (all metadata must be provided from other
		/// sources, such as attributes). When set to OnDemand, reverse engineering occurs
		/// once for every type (on first access for that type). When set to Full the entire 
		/// database is analyzed (on first access) in one operation. Full is the recommended
		/// option, but can lead to longer startup times (est. 1-10 seconds, depending on the
		/// amount of tables involved).
		/// </summary>
		[Configuration( OptionsPrefix + "Analyzer/Level", ConfigKeyPresence.Optional )]
		public static AnalyzerLevel AnalyzerLevel = AnalyzerLevel.OnDemand;

		/// <summary>
		/// The setting controls the default cache strategy used by Gentle. Please 
		/// <see cref="CacheStrategy"/> for information on the possible values.
		/// </summary>
		[Configuration( OptionsPrefix + "Cache/DefaultStrategy", ConfigKeyPresence.Optional )]
		public static CacheStrategy DefaultCacheStrategy = CacheStrategy.Temporary;

		/// <summary>
		/// This setting controls whether statements are cached by Gentle. The default is
		/// to cache CRUD statements only. You can manually cache your own statements by
		/// using the <see cref="PersistenceBroker.RegisterStatement"/> method (and
		/// retrieve cached statements using the 
		/// <see cref="PersistenceBroker.GetRegisteredStatement"/> method.
		/// </summary>
		[Configuration( OptionsPrefix + "Cache/CacheStatements", ConfigKeyPresence.Optional )]
		public static bool CacheStatements = true;

		/// <summary>
		/// This setting controls whether objects are cached by Gentle. When object caching
		/// is enabled, Gentle will filter query results and return the cached instance
		/// when available. The default is not to cache objects.
		/// </summary>
		[Configuration( OptionsPrefix + "Cache/CacheObjects", ConfigKeyPresence.Optional )]
		public static bool CacheObjects;

		/// <summary>
		/// This setting controls whether Gentle may skip query execution and return results
		/// composed of cached data. When true, Gentle maintains links between queries and
		/// associated result sets, and will only skip query execution if all objects from
		/// the previous execution are still available.
		/// </summary>
		[Configuration( OptionsPrefix + "Cache/SkipQueryExecution", ConfigKeyPresence.Optional )]
		public static bool SkipQueryExecution;

		/// <summary>
		/// This setting controls the scope within which objects are uniqued. Consult <see 
		/// cref="UniqingScope"/> for details on the available options.
		/// </summary>
		public static UniqingScope UniqingScope
		{
			get { return CacheManager.UniqingScope; }
			set { CacheManager.UniqingScope = value; }
		}

		/// <summary>
		/// The name of the default provider used by the Broker class.
		/// </summary>
		[Configuration( ProviderRegistry.DefaultProviderPrefix + "@name", ConfigKeyPresence.Optional )]
		public static string DefaultProviderName;

		/// <summary>
		/// The connection string used by the default provider.
		/// </summary>
		[Configuration( ProviderRegistry.DefaultProviderPrefix + "@connectionString", ConfigKeyPresence.Optional )]
		public static string DefaultProviderConnectionString;

		/// <summary>
		/// The default timeout value in seconds for connections created by Gentle.
		/// </summary>
		[Configuration( OptionsPrefix + "CommandTimeout", ConfigKeyPresence.Optional )]
		public static int DefaultCommandTimeout = 30;
		#endregion

		#region Logging Options
		/// <summary>
		/// The connection string used by the default provider.
		/// </summary>
		[Configuration( OptionsPrefix + "Logging/Category", ConfigKeyPresence.Optional )]
		public static void SetLogStatus( LogCategories name, bool enabled )
		{
			if( enabled )
			{
				enabledLogCategories |= name;
			}
			else
			{
				enabledLogCategories &= ~name;
			}
		}

		public static bool IsLoggingEnabled( LogCategories categories, bool matchAll )
		{
			if( matchAll )
			{
				return (enabledLogCategories & categories) == categories;
			}
			else
			{
				return (enabledLogCategories & categories) != 0;
			}
		}
		#endregion

		#region Helper Methods
		/// <summary>
		/// Get the scope delimiter used to group entries in the cache. The
		/// supplied id is used instead of the normal scope delimitor (this
		/// is useful if you wish to clear the cache for another thread or
		/// web session than your own).
		/// </summary>
		public static string GetScopeDelimiter( string id )
		{
			if( UniqingScope == UniqingScope.Thread )
			{
				return String.Format( "|tid={0}", id );
			}
			else if( UniqingScope == UniqingScope.WebSession )
			{
				return String.Format( "|sid={0}", id );
			}
			else // UniqingScope.Application
			{
				return String.Empty;
			}
		}

		/// <summary>
		/// Get the scope delimiter used to group entries in the cache. The
		/// returned value is valid for this thread or web session only.
		/// </summary>
		public static string GetScopeDelimiter()
		{
			if( UniqingScope == UniqingScope.Thread )
			{
				return String.Format( "|tid={0}", SystemSettings.ThreadIdentity );
			}
			else if( UniqingScope == UniqingScope.WebSession )
			{
				return String.Format( "|sid={0}", SystemSettings.WebSessionID );
			}
			else // UniqingScope.Application
			{
				return String.Empty;
			}
		}
		#endregion
	}
}