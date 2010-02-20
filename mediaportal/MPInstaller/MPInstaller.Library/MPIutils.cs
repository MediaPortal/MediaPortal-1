#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.MPInstaller
{
  public class MPIutils
  {
    public MPIutils() {}

    public static void LoadPlugins(string pluginFile)
    {
      if (!File.Exists(pluginFile))
      {
        MessageBox.Show("File not found " + pluginFile);
        return;
      }
      try
      {
        Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

        if (pluginAssembly != null)
        {
          Type[] exportedTypes = pluginAssembly.GetExportedTypes();

          foreach (Type type in exportedTypes)
          {
            if (type.IsAbstract)
            {
              continue;
            }
            if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
            {
              try
              {
                //
                // Create instance of the current type
                //
                object pluginObject = Activator.CreateInstance(type);
                ISetupForm pluginForm = pluginObject as ISetupForm;

                if (pluginForm != null)
                {
                  if (pluginForm.HasSetup())
                  {
                    pluginForm.ShowPlugin();
                  }
                  //ItemTag tag = new ItemTag();
                  //tag.SetupForm = pluginForm;
                  //tag.DLLName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                  //tag.windowId = pluginForm.GetWindowId();
                  //loadedPlugins.Add(tag);
                }
              }
              catch (Exception setupFormException)
              {
                MessageBox.Show(string.Format("Exception in plugin SetupForm loading : {0} ", setupFormException.Message));
              }
            }
          }
        }
      }
      catch (Exception unknownException)
      {
        MessageBox.Show("Exception in plugin loading :{0}", unknownException.Message);
      }
    }


    public static void StartApp(string file)
    {
      Process app = new Process();
      app.StartInfo.FileName = file;
      app.StartInfo.Arguments = "";
      app.Start();
    }
  }
}