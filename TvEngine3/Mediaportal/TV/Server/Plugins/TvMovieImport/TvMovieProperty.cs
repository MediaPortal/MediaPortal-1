#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#endregion

using System;
using System.IO;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TvMovieImport
{
  internal class TvMovieProperty
  {
    private static string ReadRegistryValue(string valueName)
    {
      try
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Ewe\TVGhost\Gemeinsames"))
        {
          if (key != null)
          {
            try
            {
              object value = key.GetValue(valueName);
              if (value != null && !string.IsNullOrEmpty(value.ToString()))
              {
                return value.ToString();
              }
            }
            finally
            {
              key.Close();
            }
          }
        }

        string vsKeyName = @"Software\Classes\VirtualStore\MACHINE\SOFTWARE\";
        if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
        {
          vsKeyName += @"Wow6432Node\";
        }
        vsKeyName += @"Ewe\TVGhost\Gemeinsames";

        foreach (string userKeyName in Registry.Users.GetSubKeyNames())
        {
          using (RegistryKey key = Registry.Users.OpenSubKey(String.Format(@"{0}\{1}", userKeyName, vsKeyName)))
          {
            if (key != null)
            {
              try
              {
                object value = key.GetValue(valueName);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                  return value.ToString();
                }
              }
              finally
              {
                key.Close();
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "TV Movie import: registry lookup for {0} failed", valueName);
      }
      return string.Empty;
    }

    public static string DatabasePath
    {
      get
      {
        return ReadRegistryValue("DBDatei");  // location of TVDaten.mdb
      }
    }

    public static string UpdaterPath
    {
      get
      {
        string path = ReadRegistryValue("ProgrammPath");
        if (string.IsNullOrEmpty(path))
        {
          return path;
        }
        return Path.Combine(path, "tvuptodate.exe");
      }
    }
  }
}