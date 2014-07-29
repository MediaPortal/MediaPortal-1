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
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
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

    /// <summary>
    /// Creates a base resource URI
    /// </summary>
    /// <returns></returns>
    public static string GetStaticBaseResourceUrlFromEndpoint()
    {
      return GetStaticBaseResourceUrlFromEndpoint(IPAddress.Parse(UPnPResourceAccessUtils.LocalIPAddress()));
    }

    /// <summary>
    /// Creates a base resource URI for a given endpointadress
    /// </summary>
    /// <param name="endpointadress">Endpointadress</param>
    /// <returns></returns>
    public static string GetStaticBaseResourceUrlFromEndpoint(IPAddress endpointadress)
    {
      return Uri.EscapeUriString(String.Format("http://{0}:{1}/{2}", endpointadress, MPUPnPServer.RESOURCE_SERVER_PORT, RESOURCE_STATIC_ACCESS_PATH));
    }

    public static string GetMimeFromRegistry(string Filename)
    {
      string mime = "application/octetstream";
      var extension = System.IO.Path.GetExtension(Filename);
      if (extension != null)
      {
        string ext = extension.ToLower();
        Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
        if (rk != null && rk.GetValue("Content Type") != null)
          mime = rk.GetValue("Content Type").ToString();
      }
      return mime;
    }

    public static string LocalIPAddress()
    {
      IPHostEntry host;
      string localIp = "";
      host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          localIp = ip.ToString();
          break;
        }
      }
      return localIp;
    }
  }
}