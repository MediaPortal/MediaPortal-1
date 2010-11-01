/*
 * Static file system utility methods
 * Copyright (C) 2005 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: FileSystemUtil.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.IO;

namespace Gentle.Common
{
	/// <summary>
	/// This class is a container for useful file system commands.
	/// </summary>
	public class FileSystemUtil
	{
		#region Constructors
		// prevent instantiation of this class
		private FileSystemUtil()
		{
		}
		#endregion

		/// <summary>
		/// Check whether a file exists at the specified location.
		/// </summary>
		public static bool IsValidFilePath( string localFilePath )
		{
			if( localFilePath == null || localFilePath.Length == 0 )
			{
				return false;
			}
			try
			{
				FileInfo fileInfo = new FileInfo( localFilePath );
				return fileInfo.Exists;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Check whether the specified parameter is a file name without any path information.
		/// </summary>
		public static bool IsFileName( string fileName )
		{
			bool hasPath = fileName != null && fileName.Length > 0;
			hasPath |= fileName.IndexOf( Path.DirectorySeparatorChar ) != -1;
			hasPath |= fileName.IndexOf( Path.AltDirectorySeparatorChar ) != -1;
			return hasPath;
		}

		/// <summary>
		/// Check whether the specified parameter is a path name without any file information. 
		/// </summary>
		public static bool IsFolder( string path )
		{
			if( path == null || path.Length == 0 )
			{
				return false;
			}
			try
			{
				DirectoryInfo dirInfo = new DirectoryInfo( path );
				return dirInfo.Exists;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Check whether the supplied path is relative or absolute.
		/// </summary>
		public static bool IsRelativePath( string path )
		{
			return ! Path.IsPathRooted( path );
			/*
			// colon only permitted after drive letter so this check should be ok
			bool isFixedRoot = SystemSettings.IsWindowsPlatform ? path.IndexOf( ':' ) == 1 : true;
			// non-fixed roots and paths starting with a path separator are not considered relative
			bool isRelative = ! ( isFixedRoot || path.StartsWith( "" + Path.DirectorySeparatorChar ) );
			isRelative |= path.StartsWith( "../" ) || path.StartsWith( "..\\" );
			isRelative |= path.StartsWith( "./" ) || path.StartsWith( ".\\" );
			return isRelative;
			*/
		}

		/// <summary>
		/// Combine the supplied folder and file name into a single absolute local file path.
		/// </summary>
		public static string CombinePathAndFileName( string folder, string fileName )
		{
			folder = Path.GetFullPath( folder );
			return Path.Combine( folder, fileName );
		}

		/// <summary>
		/// Search for the specified file in the given search locations and return the first match.
		/// </summary>
		public static string DetermineFileLocation( string fileName, string[] searchLocations )
		{
			foreach( string folder in searchLocations )
			{
				if( folder != null )
				{
					string filePath = CombinePathAndFileName( folder, fileName );
					if( IsValidFilePath( filePath ) )
					{
						return filePath;
					}
				}
			}
			return null;
		}
	}
}