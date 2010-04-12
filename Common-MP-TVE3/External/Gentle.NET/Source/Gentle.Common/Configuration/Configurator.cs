/*
 * Access point into the configuration subsystem
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Configurator.cs 1234 2008-03-14 11:41:44Z mm $
 */

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;

namespace Gentle.Common
{
	/// <summary>
	/// This class serves as the access point for classes wishing to interact with
	/// the configuration subsystem.
	/// </summary>
	public class Configurator
	{
		private static object cfgLock = new object();
		// handlers in this list are used for configuring Gentle itself
		private static IList handlers = new ArrayList();
		// handlers in this list are registered by clients and can be used by any class
		private static Hashtable namedHandlers = new Hashtable();
		private static bool isInitialized;
		private static Hashtable targets = new Hashtable();
		private static bool isLoggingEnabled = true;
		private static readonly string CRLF = Environment.NewLine;
		private static StringBuilder errorLog = new StringBuilder();
		private static StringBuilder exceptionLog = new StringBuilder();

		/// <summary>
		/// Private constructor to prevent instances of static class
		/// </summary>
		private Configurator()
		{
		}

		#region Public Section Handler Registration 
		/// <summary>
		/// This method creates a configuration handler for the specified section
		/// of the standard .NET configuration file.
		/// </summary>
		/// <param name="configStoreName">The name with which to associate this handler. If null is given
		/// the handler will be used to configure Gentle settings, otherwise it will only be used when
		/// Configure is called with a matching name.</param>
		/// <param name="sectionName">The section name with which the GentleSectionHandler
		/// has been declared in the .NET configuration file.</param>
		public static void AddSectionHandler( string configStoreName, string sectionName )
		{
			// prepare error message in advance
			string errorMsg = String.Format( "Unable to create GentleSectionHandler for section " +
			                                 "named \"{0}\" in file \"{1}\".{2}",
			                                 sectionName, AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, CRLF );
			try
			{
				// try to create handler for custom section in the standard config file
#pragma warning disable 0618
				GentleSectionHandler handler = (GentleSectionHandler) ConfigurationSettings.GetConfig( sectionName );
#pragma warning restore 0618
				// add handler if valid (first valid handler will be used to get settings)
				if( handler != null && handler.IsValid )
				{
					AddHandler( configStoreName, handler, false );
				}
				else
				{
					LogMessage( errorMsg );
				}
			}
			catch( Exception e )
			{
				LogMessage( errorMsg );
				LogException( e );
			}
		}

		/// <summary>
		/// This method creates a configuration handler for the specified section
		/// of the standard .NET configuration file.
		/// </summary>
		/// <param name="sectionName">The section name with which the GentleSectionHandler
		/// has been declared in the .NET configuration file.</param>
		public static void AddSectionHandler( string sectionName )
		{
			AddSectionHandler( null, sectionName );
		}
		#endregion

		#region Public File Handler Registration 
		/// <summary>
		/// This method creates a configuration handler for the specified file name. The
		/// file name must not include path information (Gentle will search for it in specific
		/// predefined locations).
		/// </summary>
		/// <param name="configStoreName">The name with which to associate this handler. If null is given
		/// the handler will be used to configure Gentle settings, otherwise it will only be used when
		/// Configure is called with a matching name.</param>
		/// <param name="fileName">The name of the configuration file.</param>
		public static void AddFileHandler( string configStoreName, string fileName )
		{
			try
			{
				FileHandler handler = new FileHandler( fileName );
				AddHandler( configStoreName, handler, false );
			}
			catch( Exception e )
			{
				string file = fileName == null ? FileHandler.CONFIG_FILENAME : fileName;
				LogMessage( "Unable to create FileHandler for file " + file + "." );
				LogMessage( "This usually means that the file could not be found in any of " +
				            "the default search locations." + CRLF );
				LogException( e );
			}
		}

		/// <summary>
		/// This method creates a configuration handler for the specified file name. The
		/// file name must not include path information (Gentle will search for it in specific
		/// predefined locations).
		/// </summary>
		/// <param name="fileName">The name of the configuration file.</param>
		public static void AddFileHandler( string fileName )
		{
			AddFileHandler( null, fileName );
		}
		#endregion

		#region Public External Handler Registration 
		/// <summary>
		/// This method creates a configuration handler for the given XML fragment,
		/// which (if valid) will be used as configuration source.
		/// </summary>
		/// <param name="configStoreName">The name with which to associate this handler. If null is given
		/// the handler will be used to configure Gentle settings, otherwise it will only be used when
		/// Configure is called with a matching name.</param>
		/// <param name="root">The root node of the XML fragment to use as 
		/// configuration source.</param>
		public static void AddExternalHandler( string configStoreName, XmlNode root )
		{
			try
			{
				// make sure to insert manually added sources before built-in handlers
				AddHandler( configStoreName, new ConfigurationHandler( root ), true );
			}
			catch( Exception e )
			{
				LogMessage( "Unable to use supplied XML fragment as configuration source:" );
				LogMessage( root != null ? root.OuterXml : "null" );
				LogException( e );
			}
		}

		/// <summary>
		/// This method creates a configuration handler for the given XML fragment,
		/// which (if valid) will be used as configuration source.
		/// </summary>
		/// <param name="root">The root node of the XML fragment to use as 
		/// configuration source.</param>
		public static void AddExternalHandler( XmlNode root )
		{
			AddExternalHandler( null, root );
		}
		#endregion

		#region Public StreamHandler Registration 
		/// <summary>
		/// This method creates a configuration handler for the specified stream.
		/// </summary>
		/// <param name="configStoreName">The name with which to associate this handler. If null is given
		/// the handler will be used to configure Gentle settings, otherwise it will only be used when
		/// Configure is called with a matching name.</param>
		/// <param name="stream">The stream from which to read the configuration XML document/snippet.</param>
		public static void AddStreamHandler( string configStoreName, Stream stream )
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( stream );
				AddHandler( configStoreName, new ConfigurationHandler( doc ), false );
			}
			catch( Exception e )
			{
				LogMessage( "Unable to use supplied stream as a configuration source." + CRLF );
				LogException( e );
			}
		}

		/// <summary>
		/// This method creates a configuration handler for the specified stream.
		/// </summary>
		/// <param name="stream">The stream from which to read the configuration XML document/snippet.</param>
		public static void AddStreamHandler( Stream stream )
		{
			AddStreamHandler( null, stream );
		}
		#endregion

		#region Configuration Methods (clients call these to configure an object or type)
		/// <summary>
		/// Configure all targets in the specified instance.
		/// </summary>
		/// <param name="configStoreName">The name of the handler to use as configuration source.
		/// If null is given, Gentle's configuration store will be used (effectively this means
		/// one of the automatically registered handlers).</param>
		/// <param name="instance">The object to be configured</param>
		public static void Configure( string configStoreName, object instance )
		{
			InitializeHandlers();
			lock( cfgLock )
			{
				// get target descriptor
				ConfigurationMap cm = GetConfigurationMap( instance.GetType() );
				// configure instance
				if( configStoreName != null )
				{
					Check.Verify( namedHandlers.ContainsKey( configStoreName ), Error.DeveloperError,
					              "No handler has been registered with configStoreName of {0}.", configStoreName );
					IList list = new ArrayList( 1 );
					list.Add( namedHandlers[ configStoreName ] );
					cm.Configure( list, instance );
				}
				else
				{
					cm.Configure( handlers, instance );
				}
			}
		}

		/// <summary>
		/// Configure all targets in the specified instance.
		/// </summary>
		/// <param name="instance">The object to be configured</param>
		public static void Configure( object instance )
		{
			Configure( null, instance );
		}

		/// <summary>
		/// Configure all targets in the specified type (static members).
		/// </summary>
		/// <param name="configStoreName">The name of the handler to use as configuration source.
		/// If null is given, Gentle's configuration store will be used (effectively this means
		/// one of the automatically registered handlers).</param>
		/// <param name="type">The type to be configured</param>
		public static void Configure( string configStoreName, Type type )
		{
			InitializeHandlers();
			lock( cfgLock )
			{
				// get target descriptor
				ConfigurationMap cm = GetConfigurationMap( type );
				// configure type (static members)
				if( configStoreName != null )
				{
					Check.Verify( namedHandlers.ContainsKey( configStoreName ), Error.DeveloperError,
					              "No handler has been registered with configStoreName of {0}.", configStoreName );
					IList list = new ArrayList( 1 );
					list.Add( namedHandlers[ configStoreName ] );
					cm.Configure( list, type );
				}
				else
				{
					cm.Configure( handlers, type );
				}
			}
		}

		/// <summary>
		/// Configure all targets in the specified type (static members).
		/// </summary>
		/// <param name="type">The type to be configured</param>
		public static void Configure( Type type )
		{
			Configure( null, type );
		}

		/// <summary>
		/// Direct accessor for obtaining a specific value from a configuration store.
		/// </summary>
		/// <param name="configStoreName">The named configuration store to use or null for Gentle's default store.</param>
		/// <param name="configKeyPath">The XPath to the configuration value (element or attribute).</param>
		/// <returns>The value of the given key or null if nothing was found.</returns>
		public static string GetKey( string configStoreName, string configKeyPath )
		{
			InitializeHandlers();
			lock( cfgLock )
			{
				// set scope in which to look for key
				IList stores = handlers;
				if( configStoreName != null )
				{
					Check.Verify( namedHandlers.ContainsKey( configStoreName ), Error.DeveloperError,
					              "No handler has been registered with configStoreName of {0}.", configStoreName );
					IList list = new ArrayList( 1 );
					list.Add( namedHandlers[ configStoreName ] );
					stores = list;
				}
				// obtain key and return result
				foreach( ConfigurationHandler handler in stores )
				{
					try
					{
						XmlNode node = handler.GetNode( configKeyPath );
						string result = (string) TypeConverter.Get( typeof(string), node );
						return result;
					}
					catch
					{
					}
				}
				return null;
			}
		}
		#endregion

		#region Private Handler Registration Methods
		/// <summary>
		/// This method creates the default configuration handlers provided 
		/// by Gentle itself.
		/// </summary>
		private static void InitializeHandlers()
		{
			if( ! isInitialized )
			{
				isInitialized = true;
				AddSectionHandler( "gentle" );
				// null causes Gentle to search for "Gentle.config" in various preconfigured locations
				AddFileHandler( null );
			}
			// ensure that we have a valid configuration source after calling this method
			lock( cfgLock )
			{
				if( handlers.Count == 0 )
				{
					// format a nice error message for end-users
					StringBuilder sb = new StringBuilder();
					sb.Append( "FATAL ERROR: No configuration store was found!" + CRLF );
					sb.Append( "Gentle is unable to continue!" + CRLF + CRLF );
					sb.Append( "The handlers emitted the following error messages:" + CRLF );
					sb.Append( errorLog.ToString() );
					sb.Append( "The handlers threw the following exceptions:" + CRLF );
					sb.Append( exceptionLog.ToString() );
					throw new GentleException( Error.NoConfigStoreFound, sb.ToString() );
				}
				else // assume we have a valid handler - release/clear error logs
				{
					errorLog = null;
					exceptionLog = null;
				}
			}
		}

		private static void AddHandler( string configStoreName, ConfigurationHandler handler, bool isFirst )
		{
			lock( cfgLock )
			{
				if( handler != null && handler.IsValid )
				{
					handlers.Insert( isFirst ? 0 : handlers.Count, handler );
					if( configStoreName != null )
					{
						namedHandlers[ configStoreName ] = handler;
					}
				}
				else
				{
					// this block is used for logging when no exception is thrown and
					// thus complements the logging in each catch block above.
					LogMessage( "Attempt to add configuration handler failed." );
					if( handler != null )
					{
						LogMessage( "Handler: {0}  IsValid: {1}", handler.GetType(), handler.IsValid );
					}
				}
			}
		}
		#endregion

		#region Logging Helpers
		private static void LogMessage( string msg, params object[] args )
		{
			if( errorLog == null )
			{
				errorLog = new StringBuilder();
			}
			if( args != null && args.Length > 0 )
			{
				errorLog.AppendFormat( msg, args );
			}
			else
			{
				errorLog.Append( msg );
			}
			// add linefeed between messages
			errorLog.Append( CRLF );
		}

		private static void LogException( Exception e )
		{
			if( e != null )
			{
				// add empty line between exceptions
				exceptionLog.Append( e + CRLF + CRLF );
			}
		}
		#endregion

		#region ConfigurationMap Construction
		private static ConfigurationMap GetConfigurationMap( Type type )
		{
			ConfigurationMap cm;
			if( ! targets.ContainsKey( type ) )
			{
				cm = new ConfigurationMap( type );
				targets[ type ] = cm;
			}
			else
			{
				cm = (ConfigurationMap) targets[ type ];
			}
			return cm;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Accessor to turn logging on or off. This property is updated when the configuration 
		/// is being accessed.
		/// </summary>
		public static bool IsLoggingEnabled
		{
			get { return isLoggingEnabled; }
			set { isLoggingEnabled = value; }
		}

		/// <summary>
		/// Returns the number of valid registered configuration handlers. Only the first handler
		/// is used used to obtain configuration options, but if HandlerCount is 0 then Gentle
		/// has not been able to find any valid configuration source.
		/// </summary>
		public int HandlerCount
		{
			get
			{
				lock( cfgLock )
				{
					InitializeHandlers();
					return handlers.Count;
				}
			}
		}
		#endregion
	}
}