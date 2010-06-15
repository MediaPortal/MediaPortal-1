/*
 * Helper class for retrieving namespace providers from an external source (i.e. the config file)
 * Copyright (C) 2004 Tom McMillen
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 */

using System;
using System.Collections;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Helper class for retrieval of namespaced providers.
	/// </summary>
	internal class NamespaceProviders
	{
		private const string NamespacePrefix = "Gentle.Framework/NamespaceProviders/Namespace";

		private static Hashtable namespaces = new Hashtable();
		// members for caching a sorted list of namespaces (used to find "deepest" match first)
		private static bool isDirty;
		private static ArrayList sortedNamespaceNames;

		static NamespaceProviders()
		{
			Configurator.Configure( typeof(NamespaceProviders) );
		}

		[Configuration( NamespacePrefix, ConfigKeyPresence.Optional )]
		public static void RegisterNamespace( string _namespace, string provider, string connectionString )
		{
			RegisterNamespace( _namespace, provider, connectionString, null );
		}

		[Configuration( NamespacePrefix, ConfigKeyPresence.Optional )]
		public static void RegisterNamespace( string _namespace, string provider, string connectionString, string schema )
		{
			try
			{
				namespaces.Add( _namespace, new NamespaceProvider( _namespace, provider, connectionString, schema ) );
				isDirty = true;
			}
			catch( Exception e )
			{
				// ignore but log errors
				Check.LogError( LogCategories.General, e );
			}
		}

		public static void RebuildSortedNamespaceList()
		{
			sortedNamespaceNames = new ArrayList();
			foreach( string name in namespaces.Keys )
			{
				sortedNamespaceNames.Add( name );
			}
			sortedNamespaceNames.Sort();
		}

		public static int Count
		{
			get { return namespaces.Count; }
		}

		public static Hashtable Namespaces
		{
			get { return namespaces; }
		}

		public static ArrayList SortedNamespaceNames
		{
			get
			{
				if( isDirty || sortedNamespaceNames == null )
				{
					RebuildSortedNamespaceList();
				}
				return sortedNamespaceNames;
			}
		}

		public static NamespaceProvider GetNamespaceProvider( string nmspace )
		{
			return namespaces[ nmspace ] as NamespaceProvider;
		}

		public static NamespaceProvider GetNamespaceProvider( Type type )
		{
			// use the .NET 2.0 default name for classes without namespace
			string ns = type.Namespace != null ? type.Namespace : "global";
			// check for exact match first (quick exit)
			if( namespaces.Contains( ns ) )
			{
				return namespaces[ ns ] as NamespaceProvider;
			}
			// get the longest matching namespace
			string match = null;
			foreach( string nmspace in namespaces.Keys )
			{
				if( type.ToString().StartsWith( nmspace ) )
				{
					if( match == null || nmspace.Length > match.Length )
					{
						match = nmspace;
					}
				}
			}
			return match != null ? GetNamespaceProvider( match ) : null;
		}
	}
}