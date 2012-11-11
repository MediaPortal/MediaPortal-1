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
using System.IO;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV
{
  public class PluginLoaderSetupTv : PluginLoader
  {

    /// <summary>
    /// Loads all plugins.
    /// </summary>
    public override void Load()
    {
      try
      {
        RetrievePluginsFromServer();
        base.Load();
      }
      catch (Exception ex)
      {
        this.LogError("PluginLoaderSetupTv.Load - could not load plugins {0}", ex);
      }
    }

    private void RetrievePluginsFromServer()
    {
      bool tvserviceExists = File.Exists(@"tvservice.exe");

      if (!tvserviceExists)
      {
        IDictionary<string, byte[]> streamList = ServiceAgents.Instance.ControllerServiceAgent.GetPluginBinaries();
        string pluginsFolder = PathManager.BuildAssemblyRelativePath("plugins");
        if (!Directory.Exists(pluginsFolder))
        {
          Directory.CreateDirectory(pluginsFolder);
        }

        foreach (KeyValuePair<string, byte[]> stream in streamList)
        {
          string fileFullPath = Path.Combine(pluginsFolder, stream.Key);
          using (FileStream fileStream = File.Create(fileFullPath, stream.Value.Length))
          {
            fileStream.Write(stream.Value, 0, stream.Value.Length);
          }
        }
        IDictionary<string, byte[]> streamListCustomDevices = ServiceAgents.Instance.ControllerServiceAgent.GetPluginBinariesCustomDevices();
        string customDevicesFolder = Path.Combine(pluginsFolder, "CustomDevices");
        if (!Directory.Exists(customDevicesFolder))
        {
          Directory.CreateDirectory(customDevicesFolder);
        }
        foreach (KeyValuePair<string, byte[]> stream in streamListCustomDevices)
        {
          string fileFullPath = Path.Combine(customDevicesFolder, stream.Key);
          using (FileStream fileStream = File.Create(fileFullPath, stream.Value.Length))
          {
            fileStream.Write(stream.Value, 0, stream.Value.Length);
          }
        }
        IDictionary<string, byte[]> streamListResources = ServiceAgents.Instance.ControllerServiceAgent.GetPluginBinariesResources();
        string resourceFolder = Path.Combine(customDevicesFolder, "Resources");
        if (!Directory.Exists(resourceFolder))
        {
          Directory.CreateDirectory(resourceFolder);
        }
        foreach (KeyValuePair<string, byte[]> stream in streamListResources)
        {
          string fileFullPath = Path.Combine(resourceFolder, stream.Key);
          using (FileStream fileStream = File.Create(fileFullPath, stream.Value.Length))
          {
            fileStream.Write(stream.Value, 0, stream.Value.Length);
          }
        }
      }
    }
  }
}