/*
 * Provider registry for keeping track of available providers
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ProviderRegistry.cs 1234 2008-03-14 11:41:44Z mm $
 */

using System;
using System.Collections;
using System.Reflection;
using Gentle.Common;

namespace Gentle.Framework
{
	internal sealed class ProviderRegistry
	{
		private const string ProviderPath = "Gentle.Framework/Providers/Provider";
		internal const string DefaultProviderPrefix = "Gentle.Framework/DefaultProvider/";

		private Hashtable providers = new Hashtable();

		[Configuration( DefaultProviderPrefix + "@name", ConfigKeyPresence.Optional )]
		private string defaultProviderName = null; // initialize to avoid compiler warning
		[Configuration( DefaultProviderPrefix + "@schema", ConfigKeyPresence.Optional )]
		private string defaultProviderSchemaName = null; // initialize to avoid compiler warning
		[Configuration( DefaultProviderPrefix + "@connectionString", ConfigKeyPresence.Optional )]
		private string defaultConnectionString; // initialize to avoid compiler warning

		/// <summary>
		/// Set the connection string used by the default provider. Call this method
		/// during application startup to override the connection string defined in
		/// the Gentle config file.
		/// </summary>
		/// <param name="connectionString">The default connection string</param>
		public void SetDefaultConnectionString( string connectionString )
		{
			defaultConnectionString = connectionString;
		}

		internal class ProviderInfo
		{
			public Assembly assembly;
			public Type providerType;
			public ConstructorInfo providerConstructor;
			public ConstructorInfo providerSchemaConstructor;

			public ProviderInfo( Assembly assembly, Type type )
			{
				this.assembly = assembly;
				providerType = type;
				// find normal constructor (connectionstring only)
				providerConstructor = type.GetConstructor( new[] { typeof(string) } );
				Check.VerifyNotNull( providerConstructor, Error.InvalidProviderLibrary, assembly.FullName );
				// also try to find a constructor for when schema is specified
				providerSchemaConstructor = type.GetConstructor( new[] { typeof(string), typeof(string) } );
			}

			public IGentleProvider CreateInstance( string connectionString )
			{
				object providerObj = providerConstructor.Invoke( new object[] { connectionString } );
				return ValidateProvider( providerObj, connectionString );
			}

			public IGentleProvider CreateInstance( string connectionString, string schemaName )
			{
				if( schemaName == null )
				{
					return CreateInstance( connectionString );
				}
				Check.VerifyNotNull( providerSchemaConstructor, "The provider does not have a constructor for specifying a schema.{0}" +
				                                                "You need to remove the schema attribute for this provider from the configuration file.{0}" +
				                                                "Assembly={1}", Environment.NewLine, assembly.FullName );
				object providerObj = providerSchemaConstructor.Invoke( new object[] { connectionString, schemaName } );
				return ValidateProvider( providerObj, connectionString );
			}

			private IGentleProvider ValidateProvider( object providerObj, string connectionString )
			{
				Check.VerifyNotNull( providerObj, "An error occurred while creating a provider instance.{0}" +
				                                  "Assembly={1}, ConnectionString={2}", Environment.NewLine,
				                     assembly.FullName, connectionString );
				IGentleProvider provider = providerObj as IGentleProvider;
				Check.VerifyNotNull( provider, Error.InvalidProviderLibrary, assembly.FullName );
				return provider;
			}
		}

		internal ProviderRegistry()
		{
			Configurator.Configure( this );
		}

		[Configuration( ProviderPath, ConfigKeyPresence.Mandatory )]
		public void RegisterProvider( string name, string assembly )
		{
			// add provider to list of available providers
			ProviderInfo pi = GetProviderInfo( assembly );
			// name is providerName
			if( pi != null )
			{
				providers[ name ] = pi;
			}
		}

		public IGentleProvider GetProvider( string providerName, string connectionString )
		{
			return GetProvider( providerName, connectionString, null );
		}

		public IGentleProvider GetProvider( string providerName, string connectionString, string schemaName )
		{
			if( providerName == null )
			{
				return GetProvider(); // use default provider
			}
			else
			{
				Check.Verify( providers.ContainsKey( providerName ), Error.UnknownProvider, providerName );
				ProviderInfo pi = (ProviderInfo) providers[ providerName ];
				IGentleProvider provider = pi.CreateInstance( connectionString, schemaName );
				return provider;
			}
		}

		public IGentleProvider GetProvider()
		{
			Check.Verify( providers != null && providers.Count > 0, Error.NoProviders );
			if( providers != null && providers.Count > 0 )
			{
				foreach( string providerName in providers.Keys )
				{
					if( providerName == defaultProviderName )
					{
						return GetProvider( providerName, defaultConnectionString, defaultProviderSchemaName );
					}
				}
			}
			Check.Fail( Error.NoDefaultProvider );
			return null; // compiler does not realise that Check.Fail never returns
		}

		/// <summary>
		/// Verify that the specified library contains an IGentleProvider instance, and
		/// fill out a ProviderInfo struct if the given assembly is valid.
		/// </summary>
		/// <param name="assembly">The provider assembly to verify</param>
		private ProviderInfo GetProviderInfo( Assembly assembly )
		{
			Check.VerifyNotNull( assembly, Error.NullParameter, "assembly" );
			foreach( Type type in assembly.GetTypes() )
			{
				if( type.GetInterface( "IGentleProvider", false ) != null )
				{
					return new ProviderInfo( assembly, type );
				}
			}
			Check.Fail( Error.InvalidProviderLibrary, assembly.FullName );
			return null;
		}

		/// <summary>
		/// Load the specified library/assembly dll.
		/// </summary>
		/// <param name="assemblyName">The assembly name to load</param>
		private ProviderInfo GetProviderInfo( string assemblyName )
		{
			if( assemblyName.ToLower().EndsWith( ".dll" ) )
			{
				assemblyName = assemblyName.Substring( 0, assemblyName.Length - 4 );
			}
			Assembly assembly = null;
			try
			{
#pragma warning disable 0618
				assembly = Assembly.LoadWithPartialName( assemblyName );
#pragma warning restore 0618
			}
			catch
			{
				// raise an error only if the missing assembly has been referenced.
				// this avoids unnessecary errors caused by entries in the config file
				// referencing provider libraries that are not available at run-time.
				Assembly caller = Assembly.GetEntryAssembly();
				// abort if we're unable to determine entry assembly
				if( caller == null )
				{
					Check.Fail( Error.UnknownAssembly, assemblyName );
				}
				AssemblyName[] references = caller.GetReferencedAssemblies();
				foreach( AssemblyName name in references )
				{
					if( assemblyName == name.Name )
					{
						Check.Fail( Error.UnknownAssembly, name );
					}
				}
				// library not referenced - return null to silently ignore
				return null;
			}
			Check.VerifyNotNull( assembly, Error.InvalidProviderLibrary, assemblyName );
			return GetProviderInfo( assembly );
		}

		/// <summary>
		/// Returns the number of provider libraries found.
		/// </summary>
		public int ProviderCount
		{
			get { return providers.Count; }
		}
	}
}