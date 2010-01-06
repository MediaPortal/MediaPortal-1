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
using System;
using System.IO;

namespace TvLibrary
{
  /// <summary>
  /// This class is responsible for managing debug settings 
  /// not configured through normal TV Server configuration.
  /// Each setting's status is determined by the existence 
  /// of a similarly named file in the TV Server's data dir
  /// </summary>
  public class DebugSettings
  {
    private static string SettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                         @"\Team MediaPortal\MediaPortal TV Server\debug\";

    /// <summary>
    /// Get the path and filename of the requested debug setting
    /// </summary>
    /// <param name="setting">the name of the setting</param>
    /// <returns>the path</returns>
    public static string SettingPath(string setting)
    {
      return SettingsPath + setting + ".txt";
    }

    /// <summary>
    /// Get the value of a debug setting
    /// </summary>
    /// <param name="setting">the name of the setting</param>
    /// <returns>true if the setting is enabled, otherwise false</returns>
    public static bool GetSetting(string setting)
    {
      return File.Exists(SettingPath(setting));
    }

    /// <summary>
    /// Enables or disbables a debug setting
    /// </summary>
    /// <param name="setting">the name of the setting</param>
    /// <param name="enabled">true to enable the setting, otherwise false</param>
    public static void SetSetting(string setting, bool enabled)
    {
      string settingPath = SettingPath(setting);
      try
      {
        if (File.Exists(settingPath) == enabled)
        {
          return;
        }

        if (enabled)
        {
          if (!Directory.Exists(SettingsPath))
          {
            Directory.CreateDirectory(SettingsPath);
          }
          File.CreateText(settingPath).Close();
        }
        else
        {
          File.Delete(settingPath);
        }
      }
      catch (System.IO.IOException ex)
      {
        Log.Log.Error("Failed to write debug setting '{0}'", setting);
        Log.Log.Write(ex);
      }
    }

    /// <summary>
    /// When true, the graph should be reset after it is stopped
    /// </summary>
    public static bool ResetGraph
    {
      get { return GetSetting("ResetGraph"); }
      set { SetSetting("ResetGraph", value); }
    }

    /// <summary>
    /// When true, TSWriter should always use PAT lookup
    /// </summary>
    public static bool UsePATLookup
    {
      get { return GetSetting("UsePATLookup"); }
      set { SetSetting("UsePATLookup", value); }
    }

    /// <summary>
    /// When true, the raw timeshifted TS will be dumped to a file
    /// </summary>
    public static bool DumpRawTS
    {
      get { return GetSetting("DisableCRCCheck"); }
      set { SetSetting("DisableCRCCheck", value); }
    }

    /// <summary>
    /// When true, CRC checks should be disabled for DVB TS packets
    /// </summary>
    public static bool DisableCRCCheck
    {
      get { return GetSetting("DisableCRCCheck"); }
      set { SetSetting("DisableCRCCheck", value); }
    }
  }
}