/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MediaPortal.Util
{
  public class Config
  {
    private static Dictionary<Dir, string> directories;

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
      Config
    }

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

    /// <summary>
    /// Read the Directory Configuration from the Config File.
    /// First we look for the file in MyDocuments of the logged on user. If file is not there or invalid, 
    /// we use the one from the MediaPortal InstallPath.
    /// </summary>
    private static void LoadDirs(string startuppath)
    {
      Set(Dir.Base, startuppath + @"\");
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
                  // Check to see, if the location was specified with an absolute or relative path.
                  // In case of relative path, prefix it with the startuppath                 
                  if (!Path.IsPathRooted(path.InnerText))
                  {
                    strPath = Get(Dir.Base) + path.InnerText;
                  }
                  // See if we got a slash at the end. If not add one.
                  if (!strPath.EndsWith(@"\"))
                  {
                    strPath += @"\";
                  }
                  try
                  {
                    Set((Dir) Enum.Parse(typeof(Dir), dirId.InnerText), strPath);
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
      Set(Dir.Cache, Path.Combine(baseDir, @"cache\"));
      Set(Dir.Config, baseDir);
      Set(Dir.CustomInputDevice, Path.Combine(baseDir, @"InputDeviceMappings\custom\"));
      Set(Dir.Database, Path.Combine(baseDir, @"database\"));
      Set(Dir.Language, Path.Combine(baseDir, @"language\"));
      Set(Dir.Log, Path.Combine(baseDir, @"log\"));
      Set(Dir.Plugins, Path.Combine(baseDir, @"plugins\"));
      Set(Dir.Skin, Path.Combine(baseDir, @"skin\"));
      Set(Dir.Thumbs, Path.Combine(baseDir, @"thumbs\"));
      Set(Dir.Weather, Path.Combine(baseDir, @"weather\"));
    }


    //TODO: Make Get method private, when all external references are replaced by a call to GetFolder or GetFile
    /// <summary>
    /// Returns the complete path for the specified MP directory.
    /// </summary>
    /// <param name="path">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a string containing the complete path, with trailing backslash</returns>
    [Obsolete("This method is obsolete and will disappear in the future.  Use GetFolder(Dir path) or GetFile(Dir path, string fileName) instead.", false)]
    public static string Get(Dir path)
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
    /// Returns the complete path for the specified MP directory.
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a string containing the complete path, without trailing backslash</returns>
    public static string GetFolder(Dir directory)
    {
      return Path.GetDirectoryName(Get(directory));
    }

    /// <summary>
    /// Returns the complete path for the specified sub directory in the specified MP directory
    /// </summary>
    /// <param name="directory">A <see cref="Dir"/> value, indicating the directory to return</param>
    /// <returns>a string containing the complete path, without trailing backslash</returns>
    public static string GetSubFolder(Dir directory, string subDirectory)
    {
      return Path.Combine(Get(directory), subDirectory);
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
  }
}