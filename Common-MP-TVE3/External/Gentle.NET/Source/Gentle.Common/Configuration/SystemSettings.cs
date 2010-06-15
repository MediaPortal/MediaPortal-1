/*
 * Placeholder class for Gentle.NET global configuration settings
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SystemSettings.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Threading;
using System.Web;

namespace Gentle.Common
{
	/// <summary>
	/// Placeholder class for Gentle.NET global configuration settings
	/// </summary>
	public class SystemSettings
	{
		private enum Info
		{
			WindowsIdentity,
			ThreadIdentity,
			ApplicationName,
			AppDomainName,
			WebPath,
			WebSessionID,
			Environment,
			MachineName
		} ;

		private static Hashtable environmentInfo;

		static SystemSettings()
		{
			// collect various useful information from the current environment
			CollectEnvironmentInfo();
		}

		#region System and Environment Information Properties
		/// <summary>
		/// A unique string identifying the environment the current application is running in. The
		/// default value is "development". This is used to support storing multiple multiple
		/// configurations for different deployment environments in the same config file.
		/// The value of the environment is itself read from the config file using the path
		/// "MachineName/WebPath" (when not in web mode, WebPath is part of the directory path
		/// from which the application was started).
		/// </summary>
		public static string Environment
		{
			get { return (string) environmentInfo[ Info.Environment ]; }
		}
		/// <summary>
		/// The Windows name of the current host.
		/// </summary>
		public static string MachineName
		{
			get { return (string) environmentInfo[ Info.MachineName ]; }
		}
		/// <summary>
		/// The web application path this application is running as. This is usually the same
		/// as the name of the IIS virtual directory. When not a web application, a fragment
		/// of the path to the current directory is used.
		/// </summary>
		public static string WebPath
		{
			get { return (string) environmentInfo[ Info.WebPath ]; }
		}
		/// <summary>
		/// The session id of the current web session, or an empty string if no web session
		/// has been established.
		/// </summary>
		public static string WebSessionID
		{
			get
			{
				string sessionID = GetWebSessionID();
				return sessionID != null ? sessionID : (string) environmentInfo[ Info.WebSessionID ];
			}
		}
		/// <summary>
		/// The name of the application.
		/// </summary>
		public static string ApplicationName
		{
			get { return (string) environmentInfo[ Info.ApplicationName ]; }
		}
		/// <summary>
		///  
		/// </summary>
		public static string ThreadIdentity
		{
			get
			{
				// get the thread name (use thread id as fallback)
				if( Thread.CurrentThread.Name != null && Thread.CurrentThread.Name != String.Empty )
				{
					return Thread.CurrentThread.Name;
				}
				else
				{
					return Thread.CurrentThread.GetHashCode().ToString();
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public static string WindowsIdentity
		{
			get { return (string) environmentInfo[ Info.WindowsIdentity ]; }
		}

		/// <summary>
		/// True if the underlying OS is a Windows environment.
		/// </summary>
		public static bool IsWindowsPlatform
		{
			get
			{
				bool result = System.Environment.OSVersion.Platform == PlatformID.Win32NT;
				result |= System.Environment.OSVersion.Platform == PlatformID.Win32S;
				result |= System.Environment.OSVersion.Platform == PlatformID.Win32Windows;
				result |= System.Environment.OSVersion.Platform == PlatformID.WinCE;
				return result;
			}
		}
		#endregion

		#region System and Environment Information Gathering
		private static void CollectEnvironmentInfo()
		{
			environmentInfo = new Hashtable();
			// get the windows user name
			try
			{
				environmentInfo[ Info.WindowsIdentity ] =
					System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			}
			catch
			{
				environmentInfo[ Info.WindowsIdentity ] = "";
			}
			// get the appdomain name
			environmentInfo[ Info.AppDomainName ] = AppDomain.CurrentDomain.FriendlyName;
			// application name currently same as appdomain
			environmentInfo[ Info.ApplicationName ] = environmentInfo[ Info.AppDomainName ];
			// get the machine name
			environmentInfo[ Info.MachineName ] = GetMachineName();
			// get the web or application path name (i.e. the bit just after the domain name)
			environmentInfo[ Info.WebPath ] = GetWebPath();
			// add a default value for session ID
			environmentInfo[ Info.WebSessionID ] = "anonymous";
			// add a default value for the environment
			environmentInfo[ Info.Environment ] = "development";
		}

		private static string GetMachineName()
		{
			if( HttpContext.Current != null &&
			    HttpContext.Current.ApplicationInstance != null &&
			    HttpContext.Current.ApplicationInstance.Server != null )
			{
				return HttpContext.Current.ApplicationInstance.Server.MachineName;
			}
			else
			{
				return System.Environment.MachineName;
			}
		}

		private static string GetSystemPath()
		{
			string result = null;
			try
			{
				result = System.Environment.CurrentDirectory;
				string[] sArray = result.Split( '\\' );
				// pick the most defining element unless that's not a option
				// assume directory structure is SomePath/ProjectName/ExeName/bin/Edition
				result = sArray[ sArray.Length > 1 ? sArray.Length - 4 : sArray.Length ];
			}
			catch
			{
				result = result == null || result.Length == 0 ? "default" : result;
			}
			return result;
		}

		private static string GetWebPath()
		{
			string result = null;
			if( HttpContext.Current != null &&
			    HttpRuntime.AppDomainAppVirtualPath != null )
			{
				result = HttpRuntime.AppDomainAppVirtualPath.Substring( 1, HttpRuntime.AppDomainAppVirtualPath.Length - 1 );
			}
			else
			{
				result = GetSystemPath();
			}
			return result == null || result.Length == 0 ? "default" : result;
		}

		private static string GetWebSessionID()
		{
			if( HttpContext.Current != null && HttpContext.Current.Session != null )
			{
				return HttpContext.Current.Session.SessionID;
			}
			else
			{
				return null;
			}
		}
		#endregion
	}
}