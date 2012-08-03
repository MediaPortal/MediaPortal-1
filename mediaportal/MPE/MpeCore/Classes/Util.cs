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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using System.Diagnostics;
using System.Threading;

namespace MpeCore.Classes
{
  public class Util
  {
    public static string InstallerConfigDir
    {
      get { return Config.GetSubFolder(Config.Dir.Config, "Installer"); }
    }

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
        MessageBox.Show(string.Format("Exception in plugin loading :{0}", unknownException.Message));
      }
    }

    public static bool IsPlugin(string pluginFile)
    {
      if (!File.Exists(pluginFile))
      {
        return false;
      }
      Assembly pluginAssembly = null;
      try
      {
        pluginAssembly = Assembly.LoadFrom(pluginFile);
      }
      catch (BadImageFormatException)
      {
        //return false;
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Error adding plugin depdendency for {0}.\nException message: {1}", pluginFile, ex.Message));
        return false;
      }
      if (pluginAssembly != null)
      {
        try
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
              return true;
            }
            if (type.GetInterface("MediaPortal.GUI.Library.IPlugin") != null)
            {
              return true;
            }
            if (type.GetInterface("MediaPortal.GUI.Library.IWakeable") != null)
            {
              return true;
            }
            if (type.GetInterface("TvEngine.ITvServerPlugin") != null)
            {
              return true;
            }
            if(type.IsClass && type.IsSubclassOf(typeof(GUIWindow)))
            {
              return true;
            }
          }
        }
        catch (Exception)
        {
          return false;
        }
      }
      return false;
    }


    /// <summary>
    /// Creates a relative path from one file or folder to another.
    /// </summary>
    /// <param name="fromDirectory">Contains the directory that defines the start of the relative path.</param>
    /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
    /// <returns>The relative path from the start directory to the end path.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string RelativePathTo(string fromDirectory, string toPath)
    {
      if (fromDirectory == null)

        throw new ArgumentNullException("fromDirectory");

      if (toPath == null)

        throw new ArgumentNullException("fromDirectory");

      if (System.IO.Path.IsPathRooted(fromDirectory) && System.IO.Path.IsPathRooted(toPath))
      {
        if (string.Compare(System.IO.Path.GetPathRoot(fromDirectory),
                           System.IO.Path.GetPathRoot(toPath), true) != 0)
        {
          return toPath;
        }
      }

      StringCollection relativePath = new StringCollection();

      string[] fromDirectories = fromDirectory.Split(System.IO.Path.DirectorySeparatorChar);

      string[] toDirectories = toPath.Split(System.IO.Path.DirectorySeparatorChar);

      int length = Math.Min(fromDirectories.Length, toDirectories.Length);

      int lastCommonRoot = -1;

      // find common root

      for (int x = 0; x < length; x++)
      {
        if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)

          break;

        lastCommonRoot = x;
      }

      if (lastCommonRoot == -1)
      {
        return toPath;
      }

      // add relative folders in from path

      for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)

        if (fromDirectories[x].Length > 0)

          relativePath.Add("..");

      // add to folders to path

      for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)

        relativePath.Add(toDirectories[x]);

      // create relative path

      string[] relativeParts = new string[relativePath.Count];

      relativePath.CopyTo(relativeParts, 0);

      string newPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativeParts);

      return newPath;
    }

    public static void KillAllMediaPortalProcesses()
    {
      KillProcces("Configuration");
      KillProcces("MediaPortal");
      KillProcces("SetupTv");
    }

    public static void KillProcces(string name)
    {
      Process pr = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
      if (pr != null)
      {
        MessageBox.Show(name + " must be closed in order to continue!", "Close " + name, MessageBoxButtons.OK, MessageBoxIcon.Hand);
        if (!pr.HasExited)
        {
          pr.CloseMainWindow();
          pr.Close();
          Thread.Sleep(500);
          pr = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
          if (pr != null)
          {
            try
            {
              Thread.Sleep(5000);
              pr.Kill();
            }
            catch (Exception) { }
          }
        }
      }
    }
  }
}