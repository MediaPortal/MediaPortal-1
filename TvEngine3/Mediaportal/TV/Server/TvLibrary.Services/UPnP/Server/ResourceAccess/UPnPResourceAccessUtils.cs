#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using MediaPortal.Common;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.TVEUPnPServer;

namespace MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.ResourceAccess
{
  public static class UPnPResourceAccessUtils
  {
    /// <summary>
    /// Base HTTP path for statuc resource access, e.g. "GetUPnPStaticResource".
    /// </summary>
    public const string RESOURCE_STATIC_ACCESS_PATH = "GetUPnPStaticResource";
    /// <summary>
    /// Base HTTP path for statuc resource access, e.g. "GetUPnPStaticResource".
    /// </summary>
    public const string RESOURCE_RECORDING_ACCESS_PATH = "GetUPnPRecordingResource";

    /// <summary>
    /// The physical root folder for all static resources in the TVE Programdata folder
    /// </summary>
    public const string RESOURCE_STATIC_DIRECTORY = "ResourceServer";

    public const string SYNTAX = RESOURCE_STATIC_ACCESS_PATH + "/[PHYSICAL_PATH]";

    /// <summary>
    /// Creates a resource URI from the physical relative path of the resource
    /// </summary>
    /// <param name="relativePath">The relative path starting from the [RESOURCE_DIRECTORY] directory within the TVE program data folder e.g. "icon/icon.png"</param>
    /// <returns>Returns URI in ther format: "http://[IP]:[PORT]/[RESOURCE_ACCESS_PATH]/[relativePath]</returns>
    public static string GetStaticResourceUrlFromRelative(string relativePath)
    {
      return Uri.EscapeUriString(String.Format("http://{0}:{1}/{2}/{3}", LocalIPAddress(), MPUPnPServer.RESOURCE_SERVER_PORT, RESOURCE_STATIC_ACCESS_PATH, relativePath));
    }

    public static bool ParseMediaItem(Uri resourceUri, out Guid mediaItemGuid)
    {
      try
      {
        var r = Regex.Match(resourceUri.PathAndQuery, RESOURCE_STATIC_ACCESS_PATH + @"\/([\w-]*)\/?");
        var mediaItem = r.Groups[1].Value;
        mediaItemGuid = new Guid(mediaItem);
      }
      catch (Exception e)
      {
        Log.Warn("ParseMediaItem: Failed with input url {0}", e, resourceUri.OriginalString);
        mediaItemGuid = Guid.Empty;
        return false;
      }

      return true;
    }

    public static string GetMimeFromRegistry(string Filename)
    {
      string mime = "application/octetstream";
      string ext = System.IO.Path.GetExtension(Filename).ToLower();
      Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
      if (rk != null && rk.GetValue("Content Type") != null)
        mime = rk.GetValue("Content Type").ToString();
      return mime;
    }

    public static string LocalIPAddress()
    {
      IPHostEntry host;
      string localIP = "";
      host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          localIP = ip.ToString();
          break;
        }
      }
      return localIP;
    }
  }
}