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

namespace MediaPortal.Utils.Services
{
  public class Config : IConfig
  {
    Dictionary<Options, string> configOption; 

    public enum Options
    {
      // Options holding Path Information
      BasePath,
      LogPath,
      SkinPath,
      LanguagePath,
      DatabasePath,
      PluginsPath,
      ThumbsPath,
      CachePath,
      WeatherPath,
      ConfigPath
    }

    public Config(string startuppath)
    {
      configOption = new Dictionary<Options, string>();
      Set(Options.BasePath, startuppath + @"\");
    }

    /// <summary>
    /// Load the content of the Config File into the dictionary
    /// </summary>
    /// <returns></returns>
    public bool LoadConfig()
    {
      // For the beginning we asume that it is located in the MP install path. need to think further what's the best method to store it
      //
      // Make sure the file exists before we try to do any processing
      //
      string strFileName = Get(Options.BasePath) + "MediaPortalConfig.xml";
      if (File.Exists(strFileName))
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
                  strPath = Get(Options.BasePath) + path.InnerText;
                }
                // See if we got a slash at the end. If not add one.
                if (!strPath.EndsWith(@"\"))
                  strPath += @"\";
                try
                {
                  Set((Options)Enum.Parse(typeof(Options), dirId.InnerText), strPath);
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
      return false;
    }

    public string Get(Config.Options option)
    {
      string returnVal = "";
      if (configOption.TryGetValue(option, out returnVal))
      {
        return returnVal;
      }
      else
      {
        return "";
      }
    }

    private void Set(Config.Options option, string value)
    {
      try
      {
        configOption.Add(option, value);
      }
      catch (ArgumentException)
      { }
    }
  }
}
