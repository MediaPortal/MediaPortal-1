#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MediaPortal.Configuration
{
  public class Config
  {
    #region Enums

    public enum Dir
    {
      // Path holding Path Information
      Base,
      Log,
      Skin,
      Language,
      Database,
      Plugins,
      Thumbs,
      Cache,
      Weather,
      CustomInputDevice,
      Config,
      CustomInputDefault,
      BurnerSupport,
      Installer,
    }

    #endregion

    #region Variables

    private static Dictionary<Dir, string> directories;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Private constructor. Singleton. Do not allow any instance of this class.
    /// </summary>
    ///
    /*
private Config()
{
}
*/
    static Config()
    {
      directories = new Dictionary<Dir, string>();
      LoadDirs(AppDomain.CurrentDomain.BaseDirectory);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the complete path for the specified file in the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory where the file should be located</param>
    /// <param name="fileName">The name of the file for which to return the complete path.</param>
    /// <returns>A string containing the complete path.</returns>
    public static string GetFile(Dir directory, string fileName)
    {
      if (fileName.StartsWith(@"\") || fileName.StartsWith("/"))
      {
        throw new ArgumentException("The passed file name cannot start with a slash or backslash", "fileName");
      }
      return Path.Combine(Get(directory), fileName);
    }

    /// <summary>
    /// Returns the complete path for the specified file in the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory where the file should be located</param>
    /// <param name="fileName">The name of the file for which to return the complete path.</param>
    /// <returns>A FileInfo object containing the complete path.</returns>
    public static FileInfo GetFileInfo(Dir directory, string fileName)
    {
      return new FileInfo(GetFile(directory, fileName));
    }

    /// <summary>
    /// Returns the complete path for the specified file in the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory where the file should be located</param>
    /// <param name="subDirectory">A subdirectory</param>
    /// <param name="fileName">The name of the file for which to return the complete path.</param>
    /// <returns>A string containing the complete path.</returns>
    public static string GetFile(Dir directory, string subDirectory, string fileName)
    {
      if (fileName.StartsWith(@"\") || fileName.StartsWith("/"))
      {
        throw new ArgumentException("The passed file name cannot start with a slash or backslash", "fileName");
      }
      if (subDirectory.StartsWith(@"\") || subDirectory.StartsWith("/"))
      {
        throw new ArgumentException("The passed subDirectory cannot start with a slash or backslash", "fileName");
      }
      return GetFile(directory, Path.Combine(subDirectory, fileName));
    }

    /// <summary>
    /// Returns the complete path for the specified file in the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory where the file should be located</param>
    /// <param name="subDirectory">A subdirectory</param>
    /// <param name="fileName">The name of the file for which to return the complete path.</param>
    /// <returns>A FileInfo object containing the complete path.</returns>
    public static FileInfo GetFileInfo(Dir directory, string subDirectory, string fileName)
    {
      return new FileInfo(GetFile(directory, subDirectory, fileName));
    }

    /// <summary>
    /// Returns the complete path for the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a string containing the complete path, without trailing backslash</returns>
    public static string GetFolder(Dir directory)
    {
      return Path.GetDirectoryName(Get(directory));
    }

    /// <summary>
    /// Returns the complete path for the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a DirectoryInfo object containing the complete path, without trailing backslash</returns>
    public static DirectoryInfo GetDirectoryInfo(Dir directory)
    {
      return new DirectoryInfo(Path.GetDirectoryName(Get(directory)));
    }

    /// <summary>
    /// Returns the complete path for the specified sub directory in the specified MP directory
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <param name="subDirectory">A subdirectory</param>
    /// <returns>a string containing the complete path, without trailing backslash</returns>
    public static string GetSubFolder(Dir directory, string subDirectory)
    {
      return Path.Combine(Get(directory), subDirectory);
    }

    /// <summary>
    /// Returns the complete path for the specified sub directory in the specified MP directory
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <param name="subDirectory">A subdirectory</param>
    /// <returns>a string containing the complete path, without trailing backslash</returns>
    public static DirectoryInfo GetSubDirectoryInfo(Dir directory, string subDirectory)
    {
      return new DirectoryInfo(Path.Combine(Get(directory), subDirectory));
    }

    /// <summary>
    /// Checks if a path is set for the specified MP directory.
    /// </summary>
    /// <param name="path">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a bool indicating if the directory is set</returns>
    public static bool IsSet(Dir path)
    {
      return directories.ContainsKey(path);
    }

    /// <summary>
    /// Checks if MediaPortalDirs.xml has been updated by a new installation or SVN update
    /// in case it resides in the user's MyDocuments folder
    /// </summary>
    public static bool DirsFileUpdateDetected
    {
      get
      {
        string userFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                          @"\Team MediaPortal\MediaPortalDirs.xml";
        string baseFile = AppDomain.CurrentDomain.BaseDirectory + "MediaPortalDirs.xml";

        if (!File.Exists(userFile))
        {
          return false;
        }

        if (File.GetLastWriteTimeUtc(userFile) < File.GetLastWriteTimeUtc(baseFile))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Read the Directory Configuration from the Config File.
    /// First we look for the file in MyDocuments of the logged on user. If file is not there or invalid, 
    /// we use the one from the MediaPortal InstallPath.
    /// </summary>
    private static void LoadDirs(string startuppath)
    {
      Set(Dir.Base, startuppath + @"\");
      LoadDefaultDirs();
      if (!ReadConfig(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Team MediaPortal\"))
      {
        if (!ReadConfig(Get(Dir.Base)))
        {
          LoadDefaultDirs();
        }
      }
    }

    /// <summary>
    /// Load the Path information from the Config File into the dictionary
    /// </summary>
    /// <returns></returns>
    private static bool ReadConfig(string configDir)
    {
      // Make sure the file exists before we try to do any processing
      string strFileName = configDir + "MediaPortalDirs.xml";
      if (File.Exists(strFileName))
      {
        try
        {
          XmlDocument xml = new XmlDocument();
          xml.Load(strFileName);
          if (xml.DocumentElement == null)
          {
            return false;
          }
          XmlNodeList dirList = xml.DocumentElement.SelectNodes("/Config/Dir");
          if (dirList == null || dirList.Count == 0)
          {
            return false;
          }
          foreach (XmlNode nodeDir in dirList)
          {
            if (nodeDir.Attributes != null)
            {
              XmlNode dirId = nodeDir.Attributes.GetNamedItem("id");
              if (dirId != null && dirId.InnerText != null && dirId.InnerText.Length > 0)
              {
                XmlNode path = nodeDir.SelectSingleNode("Path");
                if (path != null)
                {
                  string strPath = path.InnerText;
                  string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                  strPath = strPath.Replace("%APPDATA%", appData);
                  strPath = strPath.Replace("%AppData%", appData);
                  string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                  strPath = strPath.Replace("%PROGRAMDATA%", commonData);
                  strPath = strPath.Replace("%ProgramData%", commonData);
                  // Check to see, if the location was specified with an absolute or relative path.
                  // In case of relative path, prefix it with the startuppath   
                  if (!Path.IsPathRooted(strPath))
                  {
                    strPath = Get(Dir.Base) + strPath;
                  }

                  // Check if e.g. a network path is accessible - otherwhise MP might not start
                  try
                  {
                    Directory.CreateDirectory(strPath);
                  }
                  catch (Exception)
                  {
                    continue;
                  }

                  // See if we got a slash at the end. If not add one.
                  if (!strPath.EndsWith(@"\"))
                  {
                    strPath += @"\";
                  }
                  try
                  {
                    Set((Dir)Enum.Parse(typeof (Dir), dirId.InnerText), strPath);
                  }
                  catch (Exception)
                  {
                    return false;
                  }
                }
              }
            }
          }
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
      return false;
    }

    private static void LoadDefaultDirs()
    {
      string baseDir = Get(Dir.Base);
      string commonAppData =
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
        + @"\Team MediaPortal\MediaPortal\";

      Set(Dir.Cache, Path.Combine(commonAppData, @"cache\"));
      Set(Dir.Config, commonAppData);
      Set(Dir.CustomInputDevice, Path.Combine(commonAppData, @"InputDeviceMappings\"));
      Set(Dir.CustomInputDefault, Path.Combine(baseDir, @"InputDeviceMappings\defaults\"));
      Set(Dir.Database, Path.Combine(commonAppData, @"database\"));
      Set(Dir.Language, Path.Combine(commonAppData, @"language\"));
      Set(Dir.Log, Path.Combine(commonAppData, @"log\"));
      Set(Dir.Plugins, Path.Combine(baseDir, @"plugins\"));
      Set(Dir.Skin, Path.Combine(commonAppData, @"skin\"));
      Set(Dir.Thumbs, Path.Combine(commonAppData, @"thumbs\"));
      Set(Dir.Weather, Path.Combine(baseDir, @"weather\"));
      Set(Dir.BurnerSupport, Path.Combine(baseDir, @"Burner\"));
      Set(Dir.Installer, Path.Combine(commonAppData, @"Installer\"));
    }


    /// <summary>
    /// Returns the complete path for the specified MP directory.
    /// </summary>
    /// <param name="path">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a string containing the complete path, with trailing backslash</returns>
    private static string Get(Dir path)
    {
      string returnVal;
      if (directories.TryGetValue(path, out returnVal))
      {
        return returnVal;
      }
      else
      {
        return "";
      }
    }

    private static void Set(Dir path, string value)
    {
      if (!Path.IsPathRooted(value) && IsSet(Dir.Base))
      {
        directories[path] = Path.Combine(Get(Dir.Base), value);
      }
      else
      {
        directories[path] = Path.GetFullPath(value);
      }
    }

    #endregion
  }
}