/*
 * Handler for the Gentle.config file
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: FileHandler.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Gentle.Common
{
	/// <summary>
	/// This class is a handler for the Gentle.config file, responsible for locating and
	/// loading the file (by searching a list of predefined relative paths).
	/// </summary>
	public class FileHandler : ConfigurationHandler
	{
		#region Members
		/// <summary>
		/// The key used to lookup a custom config file location in the standard .NET configuration file.
		/// If this key is present the specified filename will be used before searching elsewhere.
		/// </summary>
		private static readonly string[] APPSETTINGS_FILE = { "GentleConfigFile", "ConfigFile" };
		/// <summary>
		/// The key used to lookup a custom config file location in the standard .NET configuration file.
		/// If this key is present the specified folder will be searched before any other folder.
		/// </summary>
		private static readonly string[] APPSETTINGS_FOLDER = { "GentleConfigFolder", "ConfigFolder" };
		/// <summary>
		/// Preconfigured filename that this class will look for when loading the configuration file.
		/// </summary>
		public static readonly string CONFIG_FILENAME = "Gentle.config";
		/// <summary>
		/// List of folders that Gentle will search (in addition to any custom folder specified in the
		/// regular .NET configuration file (usually App.config or Web.config). Folders are searched in
		/// order of appearance. All paths are expanded relative to the location of the Gentle assembly.
		/// </summary>
		private static readonly string[] CONFIG_FOLDERS = {
		                                                  	AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
		                                                  	Environment.CurrentDirectory,
		                                                  	"./", "./../", "./../../", "./../../../",
		                                                  	"./../Configuration/", "./../../Configuration/", "./../../../Configuration/",
		                                                  	// the following array slot is set in the constructor (its an Uri and must be transformed)
		                                                  	null
		                                                  };
		/// <summary>
		/// The full local path and file name of the configuration file. This variable is initialized
		/// once the full location and filename has been determined. Thus, it contains the local file 
		/// path (and name) of the file this handler is using.
		/// </summary>
		private string localConfigFilePath;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to use when Gentle should search for a file called Gentle.config
		/// in a number of predefined locations.
		/// </summary>
		public FileHandler() : this( null )
		{
		}

		/// <summary>
		/// Constructor to use when specifying a path and filename manually.
		/// </summary>
		/// <param name="file">The full local path and filename to the 
		/// configuration file.</param>
		public FileHandler( string file ) : base( null )
		{
			// add executing (Gentle.Common.dll) assembly paths to search locations
			CONFIG_FOLDERS[ CONFIG_FOLDERS.Length - 1 ] = GetExecutingAssemblyLocation();
			// obtain file name and/or path info of Gentle config file from application config
			if( file == null )
			{
				file = GetConfigFileInfoFromApplicationConfig();
			}
			// set file location is member variable (required for error reporting in LoadXml method)
			localConfigFilePath = GetLocalPath( file );
			// load the contents of the specified file
			if( localConfigFilePath != null )
			{
				root = LoadXml( GetTextReader( localConfigFilePath ) );
			}
			else
			{
				throw new GentleException( Error.DeveloperError, "No configuration file could be located." );
			}
		}
		#endregion

		#region Config File Location Initialization
		private string GetLocalPath( string path )
		{
			// initialize path if none was given
			if( path == null )
			{
				path = CONFIG_FILENAME;
			}
			if( FileSystemUtil.IsValidFilePath( path ) )
			{
				return path;
			}
			else if( FileSystemUtil.IsFileName( path ) )
			{
				// path is actually just a file name (contains no path information)
				return FileSystemUtil.DetermineFileLocation( path, CONFIG_FOLDERS );
			}
			else if( FileSystemUtil.IsFolder( path ) )
			{
				return FileSystemUtil.CombinePathAndFileName( path, CONFIG_FILENAME );
			}
			else
			{
				return path;
			}
		}

		private TextReader GetTextReader( string localFilePath )
		{
			//if( ! FileSystemUtil.IsValidFilePath( localFilePath ) )
			//	throw new GentleException( Error.UnsupportedConfigStoreUri, localFilePath );
			FileStream fs = new FileStream( localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read );
			return new StreamReader( fs );
		}
		#endregion

		#region Load XML 
		private void OnValidationError( object sender, ValidationEventArgs args )
		{
			Check.LogError( LogCategories.General, "Validation error loading config file: {0}.", localConfigFilePath );
			Check.LogError( LogCategories.General, args.Message );
		}

		private XmlNode LoadXml( TextReader reader )
		{
			XmlReader xr = null;
			try
			{
				// create the validating reader and specify DTD validation
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.ValidationType = ValidationType.DTD;
				settings.ValidationEventHandler += OnValidationError;
				xr = XmlReader.Create( reader, settings );
				// pass the validating reader to the XML document; if validation fails
				// due to an undefined attribute, the data is still loaded into the document.
				XmlDocument xd = new XmlDocument();
				xd.Load( xr );
				return xd;
			}
			finally
			{
				if( xr != null )
				{
					xr.Close();
				}
			}
		}
		#endregion

		#region AppSettings Helper Methods
		private string GetConfigFileInfoFromApplicationConfig()
		{
			foreach( string key in APPSETTINGS_FILE )
			{
				try
				{
					string result = ConfigurationSettings.AppSettings[ key ];
					if( result != null && result.Length > 0 )
					{
						return result;
					}
				}
				catch
				{
					// ignore missing AppSettings keys	
				}
			}
			foreach( string key in APPSETTINGS_FOLDER )
			{
				try
				{
					string result = ConfigurationSettings.AppSettings[ key ];
					if( result != null && result.Length > 0 )
					{
						return Path.Combine( result, CONFIG_FILENAME );
					}
				}
				catch
				{
					// ignore missing AppSettings keys	
				}
			}
			return null;
		}
		#endregion

		#region Uri Helpers
		private string GetExecutingAssemblyLocation()
		{
			string uriString = Assembly.GetExecutingAssembly().EscapedCodeBase;
			Uri uri = new Uri( uriString );
			string path = uri.IsFile ? Path.GetDirectoryName( uri.LocalPath ) : null;
			return path;
		}
		#endregion
	}
}