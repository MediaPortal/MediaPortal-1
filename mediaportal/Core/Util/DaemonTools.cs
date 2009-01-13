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
using System.Diagnostics;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MediaPortal.Util
{
  /// <summary>
  /// 
  /// </summary>
  public class DaemonTools
  {
    static string _Path;
    static string _Drive;
    static bool _Enabled;
    static int _DriveNo;
    static string _MountedIsoFile = string.Empty;
    static List<string> _supportedExtensions;

    static DaemonTools()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _Enabled = xmlreader.GetValueAsBool("daemon", "enabled", false);
        _Path = xmlreader.GetValueAsString("daemon", "path", "");
        _Drive = xmlreader.GetValueAsString("daemon", "drive", "E:");
        _DriveNo = xmlreader.GetValueAsInt("daemon", "driveNo", 0);
        /*
         * DAEMON Tools supports the following image files:
         * cue/bin
         * iso
         * ccd (CloneCD)
         * bwt (Blindwrite)
         * mds (Media Descriptor File)
         * cdi (Discjuggler)
         * nrg (Nero)
         * pdi (Instant CD/DVD)
         * b5t (BlindWrite 5)
        */
        string[] extensions = xmlreader.GetValueAsString("daemon", "extensions", ".cue, .bin, .iso, .ccd, .bwt, .mds,. cdi, .nrg, .pdi, .b5t, .img").Split(',');
        _supportedExtensions = new List<string>();
        // Can't use an AddRange, as we need to trim the blanks  
        foreach(string ext in extensions)
          _supportedExtensions.Add(ext.Trim());
      }
    }

    public static bool IsEnabled
    {
      get { return _Enabled; }
    }

    public static bool IsMounted(string IsoFile)
    {
      if (IsoFile == null) return false;
      if (IsoFile == string.Empty) return false;
      IsoFile = Utils.RemoveTrailingSlash(IsoFile);
      if (_MountedIsoFile.Equals(IsoFile))
      {
        if (System.IO.Directory.Exists(_Drive + @"\"))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      return false;
    }

    public static bool Mount(string IsoFile, out string VirtualDrive)
    {
      VirtualDrive = string.Empty;
      if (IsoFile == null) return false;
      if (IsoFile == string.Empty) return false;
      if (!_Enabled) return false;
      if (!System.IO.File.Exists(_Path)) return false;

      UnMount();

      IsoFile = Utils.RemoveTrailingSlash(IsoFile);
      string strParams = String.Format("-mount {0},\"{1}\"", _DriveNo, IsoFile);
      Process p = Utils.StartProcess(_Path, strParams, true, true);
      int timeout = 0;
      while (!p.HasExited && (timeout < 10000))
      {
        System.Threading.Thread.Sleep(100);
        timeout += 100;
      }
      VirtualDrive = _Drive;
      _MountedIsoFile = IsoFile;
      return true;
    }

    public static void UnMount()
    {
      if (!_Enabled) return;
      if (!System.IO.File.Exists(_Path)) return;

      string strParams = String.Format("-unmount {0}", _DriveNo);
      Process p = Utils.StartProcess(_Path, strParams, true, true);
      int timeout = 0;
      while (!p.HasExited && (timeout < 10000))
      {
        System.Threading.Thread.Sleep(100);
        timeout += 100;
      }

      _MountedIsoFile = string.Empty;
    }

    public static string GetVirtualDrive()
    {
      if (_MountedIsoFile != string.Empty) return _Drive;
      return string.Empty;
    }

    /// <summary>
    /// This method check is the given extension is a image file
    /// </summary>
    /// <param name="extension">file extension</param>
    /// <returns>
    /// true: if file is an image file (.img, .nrg, .bin, .iso, ...)
    /// false: if the file is not an image file
    /// </returns>
    public static bool IsImageFile(string extension)
    {
      if (extension == null) return false;
      if (extension == string.Empty) return false;
      extension = extension.ToLower();
      foreach (string ext in _supportedExtensions)
        if (ext.Equals(extension))
          return true;
      return false;
    }
  }
}
