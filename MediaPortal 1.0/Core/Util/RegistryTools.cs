#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Text;
using Microsoft.Win32;
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Util
{
  /// <summary>
  /// Provides some general registry lookups
  /// </summary>
  public class RegistryTools
  {
    public RegistryTools()
		{
			// 
			// TODO: Add constructor logic here
			//
		}

    /// <summary>
    /// Searches the registry to get the location of registered dlls by their name.
    /// </summary>
    /// <param name="aFilename">The filename (e.g. quartz.dll)</param>
    /// <returns>The full path the dll or an empty string</returns>
    public static List<string> GetRegisteredAssemblyPaths(string aFilename)
    {
      List<string> resultPaths = new List<string>(1);
      try
      {
        using (RegistryKey AssemblyKey = Registry.ClassesRoot.OpenSubKey("CLSID"))
        {
          string[] reggedComps = AssemblyKey.GetSubKeyNames();
          foreach (string aFilter in reggedComps)
          {
            try
            {
              using (RegistryKey key = AssemblyKey.OpenSubKey(aFilter))
              {
                if (key != null)
                {
                  using (RegistryKey defaultkey = key.OpenSubKey("InprocServer32"))
                  {
                    if (defaultkey != null)
                    {
                      string friendlyName = (string)defaultkey.GetValue(null); // Gets the (Default) value from this key            
                      if (!string.IsNullOrEmpty(friendlyName) && friendlyName.ToLower().IndexOf(aFilename.ToLower()) >= 0)
                      {
                        if (!resultPaths.Contains(friendlyName))
                          resultPaths.Add(friendlyName);
                      }
                    }
                  }
                }
              }
            }
            catch (Exception) { }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(string.Format("Error checking registry for registered Assembly: {0} - {1}", aFilename, ex.Message));
      }
      return resultPaths;
    }

    /// <summary>
    /// Checks whether a given software application / Microsoft hotfix is installed
    /// </summary>
    /// <param name="aSoftwareName">The short name to search for (e.g. KB896626)</param>
    /// <returns>True if the software is installed</returns>
    public static bool CheckRegistryForInstalledSoftware(string aSoftwareName)
    {
      bool AppFound = false;
      string componentsKeyName = @"SOFTWARE\Microsoft\Active Setup\Installed Components", friendlyName;
      try
      {
        using (RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(componentsKeyName))
        {
          string[] instComps = componentsKey.GetSubKeyNames();
          foreach (string instComp in instComps)
          {
            RegistryKey key = componentsKey.OpenSubKey(instComp);
            friendlyName = (string)key.GetValue(null); // Gets the (Default) value from this key            
            if (friendlyName != null && friendlyName.IndexOf(aSoftwareName) >= 0)
            {
              AppFound = true;
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(string.Format("Error checking registry for installed components: {0}", ex.Message));
      }
      return AppFound;
    }

  }

}
