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
using System.Xml;
using System.IO;

namespace MediaPortal.Util
{
  public class Config
  {
    static Dictionary<Dir, string> directories;

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
    private Config()
    {
    }

    static Config()
    {
      directories = new Dictionary<Dir, string>();
    }

    /// <summary>
    /// Read the Directory Configuration from the Config File.
    /// First we look for the file in MyDocuments of the logged on user. If file is not there or invalid, 
    /// we use the one from the MediaPortal InstallPath.
    /// </summary>
    static public bool LoadDirs(string startuppath)
    {
      Set(Dir.Base, startuppath + @"\");
      if (ReadConfig(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Team MediaPortal\"))
        return true;
      else if (ReadConfig(Get(Dir.Base)))
        return true;
      else 
        return false;
    }

    /// <summary>
    /// Load the Path information from the Config File into the dictionary
    /// </summary>
    /// <returns></returns>
    static private bool ReadConfig(string configDir)
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
                  // Check to see, if the location was specified with an absolute or relative oath.
                  // In case of relative path, prefix it with the startuppath                 
                  if (!Path.IsPathRooted(path.InnerText))
                  {
                    strPath = Get(Dir.Base) + path.InnerText;
                  }
                  // See if we got a slash at the end. If not add one.
                  if (!strPath.EndsWith(@"\"))
                    strPath += @"\";
                  try
                  {
                    Set((Dir)Enum.Parse(typeof(Dir), dirId.InnerText), strPath);
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

    static public string Get(Config.Dir path)
    {
      string returnVal = "";
      if (directories.TryGetValue(path, out returnVal))
      {
        return returnVal;
      }
      else
      {
        return "";
      }
    }

    static private void Set(Config.Dir path, string value)
    {
      try
      {
        directories.Add(path, value);
      }
      catch (ArgumentException)
      { }
    }
  }
}
