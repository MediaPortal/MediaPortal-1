#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
// http://www.team-mediaportal.com
//
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion Copyright (C) 2005-2016 Team MediaPortal

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Win32;
using Hid = SharpLib.Hid;


namespace MediaPortal.InputDevices
{

  /// <summary>
  /// Provide utility functions to manage HID profiles.
  /// </summary>
  public class HidProfiles
  {
    private const int KXmlVersion = 1;

    /// <summary>
    ///   Get version of XML mapping file
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.Xml.XmlException
    static public int GetXmlVersion(string aFileName)
    {
      var doc = new XmlDocument();
      doc.Load(aFileName);
      return Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/HidHandler").Attributes["version"].Value);
    }

    /// <summary>
    ///   Check if XML file exists and version is current
    /// </summary>
    /// <param name="xmlPath">Path to XML file</param>
    /// Possible exceptions: System.IO.FileNotFoundException
    /// System.Xml.XmlException
    /// ApplicationException("XML version mismatch")
    static public bool CheckXmlFile(string aFileName)
    {
      if (!File.Exists(aFileName) || (GetXmlVersion(aFileName) != KXmlVersion))
      {
        Log.Error("HID: File does not exists or version mismatch {0}", aFileName);
        return false;
      }
      return true;
    }


    /// <summary>
    /// Get file names of all profiles including built-in and user ones.
    /// </summary>
    /// <returns>Array of profiles file names</returns>
    static public string[] GetExistingProfilesFileNames()
    {
      string legacyProfile = Path.Combine(InputHandler.CustomizedMappingsDirectory, "Generic-HID.xml");
      bool hasLegacyProfile = File.Exists(legacyProfile);
      string[] builtInProfiles = Directory.GetFiles(InputHandler.DefaultsDirectory, "hid.*.xml");
      string[] userProfiles = new string[0];
      //Catch errors in case that directory does not exist, that's the case when no custom mapping was ever created
      try { userProfiles = Directory.GetFiles(InputHandler.CustomizedMappingsDirectory, "hid.*.xml"); }
      catch { /*ignore*/ }
      //Workout how many profiles we have
      int profileCount = 0;
      if (hasLegacyProfile)
      {
        profileCount++;
      }
      profileCount += builtInProfiles.Length + userProfiles.Length;

      //Allocate our profile array
      string[] profiles = new string[profileCount];

      //Copy built-in and user profiles
      builtInProfiles.CopyTo(profiles, 0);
      userProfiles.CopyTo(profiles, builtInProfiles.Length);
      //Don't forget our legacy profile
      if (hasLegacyProfile)
      {
        profiles[profiles.Length - 1] = legacyProfile;
      }
      return profiles;
    }

    /// <summary>
    /// Provide a human readable name for our HID profile.
    /// </summary>
    /// <param name="aProfileFullPath">Full path to an HID profile XML</param>
    /// <returns></returns>
    static public string GetProfileName(string aProfileFullPath)
    {
      //First get the file name
      string name = Path.GetFileName(aProfileFullPath);

      //Then remove possible prefix and suffix
      string suffix = ".xml";
      string prefix = "hid.";
      if (name.EndsWith(suffix))
      {
        name = name.Substring(0, name.Length - suffix.Length);
      }

      if (name.StartsWith(prefix))
      {
        name = name.Substring(prefix.Length, name.Length - prefix.Length);
      }

      //The result is our human readable profile name
      return name;
    }

    /// <summary>
    /// Provide the full file path corresponding to the given profile name.
    /// It's either a file from our user custom profile path or from our default profiles path.
    /// Returns an empty string if a profile with this name could not be found.
    /// If a profile with the given name is found in both custom and default path the one from the custom path is provided.
    /// </summary>
    /// <param name="aProfileName">Name of an our profile</param>
    /// <returns></returns>
    static public string GetExistingProfilePath(string aProfileName)
    {
      string path = string.Empty;
      var pathDefault = GetDefaultProfilePath(aProfileName);
      var pathCustom = GetCustomProfilePath(aProfileName);

      if (File.Exists(pathCustom) && CheckXmlFile(pathCustom))
      {
        path = pathCustom;
      }
      else if (File.Exists(pathDefault) && CheckXmlFile(pathDefault))
      {
        path = pathDefault;
      }
      return path;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deviceXmlName"></param>
    /// <returns></returns>
    static public string GetDefaultProfilePath(string aProfileName)
    {
      return GetProfilePath(InputHandler.DefaultsDirectory, aProfileName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deviceXmlName"></param>
    /// <returns></returns>
    static public string GetCustomProfilePath(string aProfileName)
    {
      return GetProfilePath(InputHandler.CustomizedMappingsDirectory,aProfileName);
    }

    /// <summary>
    /// Provide profile file path from the given directory and profile name.
    /// </summary>
    /// <param name="aDirectory"></param>
    /// <param name="aProfileName"></param>
    /// <returns></returns>
    static public string GetProfilePath(string aDirectory, string aProfileName)
    {
      string path = string.Empty;
      if (!aProfileName.Equals("Generic-HID"))
      {
        //Add our prefix except for legacy profile name
        aProfileName = "hid." + aProfileName;
      }

      string pathCustom = Path.Combine(aDirectory, aProfileName + ".xml");
      return pathCustom;
    }

  }

}

