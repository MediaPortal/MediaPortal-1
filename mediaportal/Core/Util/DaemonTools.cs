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
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Player;

namespace MediaPortal.Util
{
  /// <summary>
  /// 
  /// </summary>
  public class DaemonTools
  {
    private static string _Path;
    private static string _DriveType;
    private static string _Drive;
    private static bool _Enabled;
    private static int _DriveNo;
    private static string _MountedIsoFile = string.Empty;
    private static HashSet<string> _supportedExtensions;
    public static string VirtualCloneDrive = "vcd";

    static DaemonTools()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        _Enabled = xmlreader.GetValueAsBool("daemon", "enabled", false);
        _Path = xmlreader.GetValueAsString("daemon", "path", "");
        _Drive = xmlreader.GetValueAsString("daemon", "drive", "E:");
        _DriveNo = xmlreader.GetValueAsInt("daemon", "driveNo", 0);
        _DriveType = xmlreader.GetValueAsString("daemon", "driveType", "");
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

        if (!OSInfo.OSInfo.Win8OrLater() && _DriveType == "native")
        {
          Log.Error("Native ISO is not supported on Vista or Windows 7");
          _Enabled = false;
        }

        if (_DriveType == "native")
        {
          _supportedExtensions = new HashSet<string>();
          _supportedExtensions.Add(".iso");
        }
        else
        {
          string[] extensions =
            xmlreader.GetValueAsString("daemon", "extensions", Utils.ImageExtensionsDefault).Split(',');
          _supportedExtensions = new HashSet<string>();
          // Can't use an AddRange, as we need to trim the blanks  
          foreach (string ext in extensions)
            _supportedExtensions.Add(ext.Trim());
        }
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool Mount(string IsoFile, out string VirtualDrive)
    {
      if (g_Player.Playing)
      {
        //string file = g_Player.CurrentFile;
        //if (g_Player.CheckIfImage(file))
        {
          g_Player.Stop();
        }
      }
      VirtualDrive = string.Empty;
      if (IsoFile == null) return false;
      if (IsoFile == string.Empty) return false;
      if (!_Enabled) return false;
      if (!_DriveType.Equals("native") && !System.IO.File.Exists(_Path)) return false;
      DateTime startTime = DateTime.Now;

      UnMount();

      IsoFile = Utils.RemoveTrailingSlash(IsoFile);
      string strParams;
      System.IO.DriveInfo drive;
      int timeout = 0;

      if (_DriveType.Equals("native"))
      {
        if (!string.IsNullOrEmpty(IsoFile))
        {
          if (_DriveType.Equals("native"))
          {
            using (var ps = PowerShell.Create())
            {
              Log.Debug("Dismount-DiskImage {0}", IsoFile);
              ps.AddCommand("Dismount-DiskImage").AddParameter("ImagePath", IsoFile).Invoke();

              while (System.IO.Directory.Exists(_Drive + @"\") && (timeout < 10000))
              {
                System.Threading.Thread.Sleep(100);
                timeout += 100;
              }
            }
          }
        }

        using (var ps = PowerShell.Create())
        {
          // Set mounted ISO file to be able to unmount it if something failed to load.
          _MountedIsoFile = IsoFile;
          Log.Debug("Mount-DiskImage {0}", IsoFile);
          ps.AddCommand("Mount-DiskImage").AddParameter("ImagePath", IsoFile).AddParameter("PassThru");
          var psResult = ps.Invoke();
          Log.Debug("Mount-DiskImage Result {0}", psResult.Count);
        }

        using (var ps = PowerShell.Create())
        {
          ps.AddCommand("Get-DiskImage").AddParameter("ImagePath", IsoFile).AddCommand("Get-Volume");
          var psResult = ps.Invoke();
          Log.Debug("Mount-get drive letter Result {0}", psResult.Count);
          foreach (var driveLetter in psResult.Select(result => result.Members["DriveLetter"].Value.ToString()))
          {
            _Drive = String.Format("{0}:", driveLetter);
            Log.Debug("Mount-DiskImage DriveLetter {0}", _Drive);
          }
        }

        drive = new System.IO.DriveInfo(_Drive);

        while ((!drive.IsReady || !System.IO.Directory.Exists(_Drive + @"\")) && (timeout < 10000))
        {
          System.Threading.Thread.Sleep(100);
          timeout += 100;
        }
      }
      else
      {
        if (!_DriveType.Equals(VirtualCloneDrive))
        {
          strParams = String.Format("-mount {0}, {1},\"{2}\"", _DriveType, _DriveNo, IsoFile);
        }
        else
        {
          strParams = String.Format("-mount {0},\"{1}\"", _DriveNo, IsoFile);
        }
        Process p = Utils.StartProcess(_Path, strParams, true, true);

        drive = new System.IO.DriveInfo(_Drive);

        while ((!p.HasExited || !drive.IsReady || !System.IO.Directory.Exists(_Drive + @"\")) && (timeout < 10000))
        {
          System.Threading.Thread.Sleep(100);
          timeout += 100;
        }
      }

      if (timeout >= 10000)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
        msg.Param1 = 200093;
        msg.Param2 = 200094;
        msg.Param3 = 0;
        msg.Param4 = 0;
        msg.Object2 = true;
        GUIWindowManager.SendMessage(msg);
        if (msg.Param1 == 1)
        {
          while ((!drive.IsReady || !System.IO.Directory.Exists(_Drive + @"\")) && (timeout < 60000))
          {
            System.Threading.Thread.Sleep(100);
            timeout += 100;
          }
          if (timeout >= 60000)
          {
            Log.Error("Mounting failed after {0}s (second timeout). Check your settings.", (int) (timeout/1000));
            UnMount();
            return false;
          }
        }
        else
        {
          Log.Error("Mounting failed after {0}s (first timeout). Check your settings.", (int) (timeout/1000));
          UnMount();
          return false;
        }
      }
      RemovableDriveHelper.SetMountTime(DateTime.Now);
      VirtualDrive = _Drive;
      _MountedIsoFile = IsoFile;
      Log.Debug("Mount time: {0}s", String.Format("{0:N}", (DateTime.Now - startTime).TotalSeconds));
      return true;
    }

    public static void UnMount()
    {
      if (!_Enabled) return;
      if (!_DriveType.Equals("native") && !System.IO.File.Exists(_Path)) return;
      if (!System.IO.Directory.Exists(_Drive + @"\") && !_DriveType.Equals("native")) return;
      int timeout = 0;
      string strParams;

      if (!string.IsNullOrEmpty(_MountedIsoFile))
      {
        if (_DriveType.Equals("native"))
        {
          using (var ps = PowerShell.Create())
          {
            Log.Debug("Dismount-DiskImage {0}", _MountedIsoFile);
            ps.AddCommand("Dismount-DiskImage").AddParameter("ImagePath", _MountedIsoFile).Invoke();

            while (System.IO.Directory.Exists(_Drive + @"\") && (timeout < 10000))
            {
              System.Threading.Thread.Sleep(100);
              timeout += 100;
            }
          }
        }
        else
        {
          if (!_DriveType.Equals(VirtualCloneDrive))
          {
            strParams = String.Format("-unmount {0},{1}", _DriveType, _DriveNo);
          }
          else
          {
            strParams = String.Format("-unmount {0}", _DriveNo);
          }
          Process p = Utils.StartProcess(_Path, strParams, true, true);

          while (!p.HasExited && (timeout < 10000))
          {
            System.Threading.Thread.Sleep(100);
            timeout += 100;
          }
        }
      }
      if (timeout >= 10000)
      {
        Log.Error("Dismount failed after {0}s (second timeout). Check your settings.", (int)(timeout / 1000));
      }
      _MountedIsoFile = string.Empty;
    }

    public static string GetVirtualDrive()
    {
      if (_MountedIsoFile != string.Empty) return _Drive;
      return string.Empty;
    }

    public static string GetLastVirtualDrive()
    {
      return _Drive;
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
      if (string.IsNullOrEmpty(extension)) return false;
      
      return _supportedExtensions.Contains(extension.ToLowerInvariant());
      //extension = extension.ToLowerInvariant();
      //foreach (string ext in _supportedExtensions)
      //  if (ext.Equals(extension))
      //    return true;
      //return false;
    }

    /// <summary>
    /// Returns current mounted ISO filename with full path
    /// </summary>
    public static string MountedIsoFile
    {
      get { return _MountedIsoFile; }
    }
  }
}