/*
 * Provider factory for dynamic instantiation of providers
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ProviderFactory.cs 1238 2008-04-16 20:39:11Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Provider factory for dynamic instantiation of providers.
	/// </summary>
	public sealed class ProviderFactory
	{
		private static Hashtable providers = new Hashtable();
		private static object providerLock = new object();
		private static ProviderRegistry registry;

		static ProviderFactory()
		{
			if( registry == null )
			{
				registry = new ProviderRegistry();
				Check.Verify( registry.ProviderCount > 0, Error.NoProviders );
			}
		}

		/// <summary>
		/// This method returns true if a provider was found with the specified name. The matching
		/// provider is returned in the out parameter.
		/// </summary>
		/// <param name="name">The developer assigned name of the provider.</param>
		/// <param name="provider">The matching provider or null if none was found.</param>
		/// <returns>True if a match was found and false otherwise.</returns>
		private static bool GetNamedProvider( string name, out IGentleProvider provider )
		{
			if( name != null && providers.ContainsKey( name ) )
			{
				provider = providers[ name ] as IGentleProvider;
				return true;
			}
			provider = null;
			return false;
		}

		public static IGentleProvider GetProvider( string name, string providerName, string connectionString )
		{
			return GetProvider( name, providerName, connectionString, null );
		}

		public static IGentleProvider GetProvider( string name, string providerName,
		                                           string connectionString, string schemaName )
		{
			lock( providerLock )
			{
				IGentleProvider provider;
				// first, try looking up provider using developer-assigned name
				if( name != null && GetNamedProvider( name, out provider ) )
				{
					return provider;
				}
				// next, try looking up provider using internal uniqueness-based name 
				string identity = providerName + "|" + connectionString;
				if( schemaName != null )
				{
					identity += "|" + schemaName;
				}
				if( GetNamedProvider( identity, out provider ) )
				{
					return provider;
				}
				// last, create provider and return new instance
				provider = registry.GetProvider( providerName, connectionString, schemaName );
				// insert provider using original name/identity
				providers[ name ?? identity ] = provider;
				// also insert provider using actual identity (in case null was passed for identity fields)
				string actualIdentity = provider.Name + "|" + provider.ConnectionString;
				if( schemaName != null )
				{
					actualIdentity += "|" + schemaName;
				}
				providers[ actualIdentity ] = provider;
				return provider;
			}
		}

		public static IGentleProvider GetProvider( string name, string connectionString )
		{
			return GetProvider( null, name, connectionString, null );
		}

		public static IGentleProvider GetDefaultProvider()
		{
			return GetProvider( null, null, null, null );
		}

		/// <summary>
		/// Get the provider for a specific type. This method tries to find a corresponding
		/// namespace provider to use, and if no match is found, returns the default provider.
		/// </summary>
		/// <param name="type">The type for which to find a provider.</param>
		/// <returns>A GentleProvider instance.</returns>
		public static IGentleProvider GetProvider( Type type )
		{
			NamespaceProvider np = NamespaceProviders.GetNamespaceProvider( type );
			if( np != null )
			{
				return GetProvider( null, np.ProviderName, np.ConnectionString, np.Schema );
			}
			// use the default provider
			return GetProvider( null, null, null, null );
		}

		/// <summary>
		/// Add a new namespace provider. If a namespace provider has been registered, Gentle will
		/// try to match the namespace of a type to those registered, and if a match is found will
		/// use the associated provider and connection string (instead of the default provider).
		/// Namespaces are matched using StartsWith, so using incomplete substrings is supported
		/// (the longest match will be used if multiple namespaces match).
		/// </summary>
		/// <param name="nmspace">The namespace string to use.</param>
		/// <param name="provider">The provider to use for this namespace.</param>
		/// <param name="connectionString">The connection string to use with this provider.</param>
		public static void AddNamespaceProvider( string nmspace, string provider, string connectionString )
		{
			NamespaceProviders.RegisterNamespace( nmspace, provider, connectionString );
		}

		/// <summary>
		/// Update or set the connection string to be used with the default provider. Unless
		/// you are calling this method prior to using Gentle the first time, you should call 
		/// the ResetGentle method to clear all cached data.
		/// </summary>
		/// <param name="connectionString">The connection string to use with the default provider.</param>
		public static void SetDefaultProviderConnectionString( string connectionString )
		{
			GentleSettings.DefaultProviderConnectionString = connectionString;
			registry.SetDefaultConnectionString( connectionString );
		}

		/// <summary>
		/// Clear all caches (metadata and providers). Call this method after updating the
		/// default provider connection string.
		/// </summary>
		public static void ResetGentle()
		{
			ResetGentle( true );
		}

		/// <summary>
		/// Clear all caches (metadata and providers). Call this method after updating the
		/// default provider connection string.
		/// </summary>
		/// <param name="clearObjectMapCache">True to clear the internal metadata cache (a list
		/// of ObjectMap entries stored in a static variable in the ObjectFactory class).</param>
		public static void ResetGentle( bool clearObjectMapCache )
		{
			lock( providerLock )
			{
				Broker.ClearPersistenceBroker();
				if( clearObjectMapCache )
				{
					ObjectFactory.ClearMaps();
				}
				providers.Clear();
				registry = new ProviderRegistry();
			}
		}
	}
}