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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  public class SystemInformation
  {
    public static IList<BdaNetworkProvider> ListAvailableBdaNetworkProviders()
    {
      // Network provider availability depends on the TV service environment.
      // - The Microsoft specific network provider should be available on all
      //   operating systems from XP to present day.
      // - The Microsoft generic network provider is available only on XP MCE
      //   2005 + Update Rollup 2 and newer.
      // - The MediaPortal network provider has not been released at time of
      //   writing.
      List<BdaNetworkProvider> availableBdaNetworkProviders = new List<BdaNetworkProvider>(3);
      if (FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID))
      {
        availableBdaNetworkProviders.Add(BdaNetworkProvider.Generic);
      }
      if (FilterGraphTools.IsThisComObjectInstalled(typeof(DVBTNetworkProvider).GUID))
      {
        // Assume the other specific network providers are available if the
        // DVB-T one is available.
        availableBdaNetworkProviders.Add(BdaNetworkProvider.Specific);
      }
      if (File.Exists(PathManager.BuildAssemblyRelativePath("NetworkProvider.ax")))
      {
        availableBdaNetworkProviders.Add(BdaNetworkProvider.MediaPortal);
      }
      return availableBdaNetworkProviders;
    }

    public static IList<VideoEncoder> ListAvailableSoftwareEncodersVideo()
    {
      IList<VideoEncoder> compatibleEncoders = SoftwareEncoderManagement.ListAllSofwareEncodersVideo();
      IList<VideoEncoder> availableEncoders = new List<VideoEncoder>(compatibleEncoders.Count);
      foreach (VideoEncoder encoder in compatibleEncoders)
      {
        if (FilterGraphTools.IsThisComObjectInstalled(new Guid(encoder.ClassId)))
        {
          availableEncoders.Add(encoder);
        }
      }
      return availableEncoders;
    }

    public static IList<AudioEncoder> ListAvailableSoftwareEncodersAudio()
    {
      IList<AudioEncoder> compatibleEncoders = SoftwareEncoderManagement.ListAllSofwareEncodersAudio();
      IList<AudioEncoder> availableEncoders = new List<AudioEncoder>(compatibleEncoders.Count);
      foreach (AudioEncoder encoder in compatibleEncoders)
      {
        if (FilterGraphTools.IsThisComObjectInstalled(new Guid(encoder.ClassId)))
        {
          availableEncoders.Add(encoder);
        }
      }
      return availableEncoders;
    }

    public static IList<string> ListAvailableNetworkInterfaceNames()
    {
      NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
      if (interfaces == null)
      {
        return new List<string>(0);
      }
      List<string> interfaceNames = new List<string>(interfaces.Length);
      foreach (var i in interfaces)
      {
        if (i != null && !string.IsNullOrEmpty(i.Name))
        {
          interfaceNames.Add(i.Name);
        }
      }
      return interfaceNames;
    }

    public static void GetBdaFixStatus(out bool isApplicable, out bool isNeeded)
    {
      // For more information, refer to:
      // http://forum.team-mediaportal.com/threads/patch-tuner-issue-and-channel-scan-crash-windows-xp.6344/
      // In short, certain Windows XP BDA components are not stable. They
      // should be replaced with equivalents from XP MCE Roll Up 2. 
      isApplicable = false;
      isNeeded = false;   // These days XP is rarer. It is unlikely there is a real problem.
      try
      {
        // Vista or later is fine.
        if (Environment.OSVersion.Version.Major >= 6)
        {
          return;
        }

        isApplicable = true;

        // Check the version of PsisDecd.dll using the file registered for the
        // DvbSiParser class (which we assume is always available).
        using (var key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{F6B96EDA-1A94-4476-A85F-4D3DC7B39C3F}\InprocServer32"))
        {
          if (key == null)
          {
            Log.Warn("system info: DVB fix check failed, missing registry key");
            return;
          }
          string friendlyName = (string)key.GetValue(null);   // default value
          if (string.IsNullOrEmpty(friendlyName))
          {
            Log.Warn("system info: DVB fix check failed, missing registry key value");
            key.Close();
            return;
          }

          FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(friendlyName);
          if (string.IsNullOrEmpty(fileVersion.ProductVersion))
          {
            Log.Warn("system info: DVB fix check failed, version information missing from DLL");
            key.Close();
            return;
          }

          isNeeded = new System.Version(fileVersion.ProductVersion) < new System.Version("6.5.2710.2732");
          key.Close();
        }
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "system info: DVB fix check failed");
      }
    }
  }
}