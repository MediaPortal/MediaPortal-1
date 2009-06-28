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
using System.Diagnostics;
using System.IO;
using System.Threading;

using MediaPortal.Configuration;
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Util
{
  /// <summary>
  /// Provides some general registry lookups
  /// </summary>
  public class FilterChecker
  {
    private static bool fUseDvbSubtitles = false;
    private static Thread fFilterCheckThread = null;

    FilterChecker()
    {
    }

    public static void CheckInstalledVersions()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        fUseDvbSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
      }

      startFilterCheckThread();
    }

    private static bool LogRegisteredFilter(string aFilterName, bool aLogMissing, bool aDoVersionCheck)
    {
      bool found = false;
      Version aParamVersion = new Version(0, 0, 0, 0);

      // Search for aFilterName (e.g. TsReader.ax) on every installation
      List<string> filterLocations = RegistryTools.GetRegisteredAssemblyPaths(aFilterName);
      if (filterLocations.Count > 0)
      {
        foreach (string filterPath in filterLocations)
        {
          string fullPath = filterPath;
          if (!filterPath.Contains(":\\"))
            fullPath = Environment.SystemDirectory + "\\" + filterPath;
          try
          {
            // Try to get the last change date as a best approach to get the install date.
            FileInfo fi = new FileInfo(fullPath);
            found = fi.Exists;
            if (found)
            {
              Log.Info("FilterChecker: Found {0} from {1} located at {2}", aFilterName, fi.LastWriteTimeUtc.ToShortDateString(), fullPath);

              if (aDoVersionCheck)
              {
                if (CheckFileVersion(fullPath, out aParamVersion))
                  Log.Info("FilterChecker: Version of installed {0}: {1}", aFilterName, aParamVersion.ToString());
                else
                  Log.Warn("FilterChecker: Could not determine version of installed {0}", aFilterName);
              }
            }
            else
              Log.Debug("FilterChecker: Invalid path info for {0}", aFilterName);
          }
          catch (Exception) { }
        }
      }
      else
        if (aLogMissing)
          Log.Error("TVHome: *** WARNING *** Unable to detect registered filter: {0}!", aFilterName);

      return found;
    }

    /// <summary>
    /// Checks if a file has the required version
    /// </summary>
    /// <param name="aFilePath">The full path to the file to check</param>
    /// <returns>True if the file's version is equal or higher than the given minimum</returns>
    public static bool CheckFileVersion(string aFilePath, out Version aCurrentVersion)
    {
      aCurrentVersion = new Version(0, 0, 0, 0);
      try
      {
        // Getting info might fail
        FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(aFilePath);
        if (!string.IsNullOrEmpty(fileVersion.ProductVersion))
        {
          try
          {
            // The ProductVersion might contain an irregular Format
            // Replace "," with "." because of versioning localization issues
            aCurrentVersion = new System.Version(fileVersion.ProductVersion.Replace(',', '.'));
            return true;
          }
          catch (Exception) { }
        }

        if (!string.IsNullOrEmpty(fileVersion.FileVersion))
        {
          try
          {
            // FileVersions sometimes contain alphanumeric chars
            // Replace "," with "." because of versioning localization issues
            aCurrentVersion = new System.Version(fileVersion.FileVersion.Replace(',', '.'));
            return true;
          }
          catch (Exception) { }
        }

        return false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Checks if a file has the required version
    /// </summary>
    /// <param name="aFilePath">The full path to the file to check</param>
    /// <param name="aMinimumVersion">The minimum version wanted</param>
    /// <returns>True if the file's version is equal or higher than the given minimum</returns>
    public static bool CheckFileVersion(string aFilePath, string aMinimumVersion, out Version aCurrentVersion)
    {
      aCurrentVersion = new Version(0, 0, 0, 0);
      try
      {
        System.Version desiredVersion = new System.Version(aMinimumVersion);
        FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(aFilePath);
        if (!string.IsNullOrEmpty(fileVersion.ProductVersion))
        {
          // Replace "," with "." because of versioning localization issues
          aCurrentVersion = new System.Version(fileVersion.ProductVersion.Replace(',', '.'));
          return aCurrentVersion >= desiredVersion;
        }
        else
          return false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private static void FilterCheckThread()
    {
      // give MP some more time to startup
      Thread.Sleep(3333);
      if (Util.Utils.UsingTvServer)
      {
        LogRegisteredFilter("TsReader.ax", true, false);
        if (fUseDvbSubtitles)
          LogRegisteredFilter("DVBsub2.ax", true, false);
        LogRegisteredFilter("mdapifilter", false, true);
      }
      LogRegisteredFilter("quartz.dll", true, true);
    }

    private static void startFilterCheckThread()
    {
      try
      {
        if (fFilterCheckThread != null)
          if (fFilterCheckThread.IsAlive)
            return;

        Log.Debug("TVHome: Filter check started.");
        fFilterCheckThread = new Thread(FilterCheckThread);
        fFilterCheckThread.IsBackground = true;
        fFilterCheckThread.Priority = ThreadPriority.BelowNormal;
        fFilterCheckThread.Name = "FilterChecker";
        fFilterCheckThread.Start();
      }
      catch (Exception ex)
      {
        Log.Warn("TVHome: Error starting FilterChecker - {0}", ex.Message);
      }
    }
  }
}