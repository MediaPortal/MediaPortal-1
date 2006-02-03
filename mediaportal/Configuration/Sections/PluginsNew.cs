#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration.Controls;
using System.IO;
using System.Reflection;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class PluginsNew : MediaPortal.Configuration.SectionSettings
  {
    private ArrayList loadedPlugins = new ArrayList();
    private ArrayList availablePlugins = new ArrayList();
    bool isLoaded = false;

    private class ItemTag
    {
      public string DLLName;
      public ISetupForm SetupForm;
      public string strType = string.Empty;
      public int windowId = -1;
    }

    public PluginsNew()
      : this("PluginsNew")
    {
    }

    public PluginsNew(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      listViewPlugins.View = View.LargeIcon;
      listViewPlugins.AutoArrange = true;
      ImageList imageList = new ImageList();
      imageList.Images.Add(Bitmap.FromFile("plugin_raw.png"));
      imageList.ImageSize = new Size(64, 64);
      listViewPlugins.LargeImageList = imageList;

      //ListViewItem item = new ListViewItem("item1", 0);
      //listViewPlugins.Items.Add(item);


      try
      {
        
      }
      catch (Exception ex)
      {
        Log.Write("Exception: ex.Data           - {0}", ex.Data);
        Log.Write("Exception: ex.HelpLink       - {0}", ex.HelpLink);
        Log.Write("Exception: ex.InnerException - {0}", ex.InnerException);
        Log.Write("Exception: ex.Message        - {0}", ex.Message);
        Log.Write("Exception: ex.Source         - {0}", ex.Source);
        Log.Write("Exception: ex.StackTrace     - {0}", ex.StackTrace);
        Log.Write("Exception: ex.TargetSite     - {0}", ex.TargetSite);
        Log.Write("Exception: ex                - {0}", ex.ToString());
      }
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadAll();
    }

    private void LoadAll()
    {
      if (!isLoaded)
      {
        isLoaded = true;
        //
        // Enumerate available plugins
        //
        EnumeratePlugins();

        //
        // Load plugins
        //
        LoadPlugins();

        //
        // Populate our list
        //
        PopulateListView();
        LoadSettings();
      }
    }

    private void EnumeratePlugins()
    {
      EnumeratePluginDirectory(@"plugins\windows");
      EnumeratePluginDirectory(@"plugins\subtitle");
      EnumeratePluginDirectory(@"plugins\tagreaders");
      EnumeratePluginDirectory(@"plugins\externalplayers");
      EnumeratePluginDirectory(@"plugins\process");
    }

    private void EnumeratePluginDirectory(string directory)
    {
      if (Directory.Exists(directory))
      {
        //
        // Enumerate files
        //
        string[] files = Directory.GetFiles(directory, "*.dll");

        //
        // Add to list
        //
        foreach (string file in files)
        {
          availablePlugins.Add(file);
        }
      }
    }

    private void PopulateListView()
    {
      foreach (ItemTag tag in loadedPlugins)
      {
        ListViewItem item = new ListViewItem(tag.SetupForm.PluginName(), 0);
        listViewPlugins.Items.Add(item);

        //ds.Tables[0].Rows.Add(new object[] { true, true, false, tag.SetupForm.PluginName(), tag.SetupForm.Author(), tag.SetupForm.Description(), tag });
      }
    }

    private void LoadPlugins()
    {
      foreach (string pluginFile in availablePlugins)
      {
        try
        {
          Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

          if (pluginAssembly != null)
          {
            Type[] exportedTypes = pluginAssembly.GetExportedTypes();

            foreach (Type type in exportedTypes)
            {
              // an abstract class cannot be instanciated
              if (type.IsAbstract)
              {
                continue;
              }
              //
              // Try to locate the interface we're interested in
              //
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
                    ItemTag tag = new ItemTag();
                    tag.SetupForm = pluginForm;
                    tag.DLLName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                    tag.windowId = pluginForm.GetWindowId();
                    loadedPlugins.Add(tag);
                  }
                }
                catch (Exception setupFormException)
                {
                  Log.Write("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                  Log.Write("Current class is :{0}", type.FullName);
#if DEBUG
                  Log.Write(setupFormException.StackTrace);
#endif
                }
              }
            }
            foreach (Type t in exportedTypes)
            {
              try
              {
                if (t.IsClass)
                {
                  if (t.IsSubclassOf(typeof(GUIWindow)))
                  {
                    object newObj = Activator.CreateInstance(t);
                    GUIWindow win = (GUIWindow)newObj;

                    foreach (ItemTag tag in loadedPlugins)
                    {
                      if (tag.windowId == win.GetID)
                      {
                        tag.strType = win.GetType().ToString();
                        break;
                      }
                    }
                  }
                }
              }
              catch (Exception guiWindowException)
              {
                Log.Write("Exception in plugin GUIWindows loading :{0}", guiWindowException.Message);
                Log.Write("Current class is :{0}", t.FullName);
#if DEBUG
                Log.Write(guiWindowException.StackTrace);
#endif
              }
            }
          }
        }
        catch (Exception unknownException)
        {
          Log.Write("Exception in plugin loading :{0}", unknownException.Message);
#if DEBUG
          Log.Write(unknownException.StackTrace);
#endif
        }
      }
    }


    public override void LoadSettings()
    {
      //try
      //{
      //  using (Settings xmlreader = new Settings("MediaPortal.xml"))
      //  {
      //    foreach (DataRow row in ds.Tables[0].Rows)
      //    {
      //      ItemTag itemTag = row["tag"] as ItemTag;

      //      if (itemTag.SetupForm != null)
      //      {
      //        if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
      //        {
      //          row["bool1"] = xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(), itemTag.SetupForm.DefaultEnabled());
      //        }
      //        else
      //        {
      //          row["bool1"] = itemTag.SetupForm.DefaultEnabled();
      //        }

      //        bool bHome = false;
      //        bool bPlugins = false;
      //        row["bool2"] = bHome;
      //        row["bool2"] = bPlugins;
      //        string buttontxt, buttonimage, buttonimagefocus, picture;
      //        if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
      //        {
      //          if (itemTag.SetupForm.GetHome(out buttontxt, out buttonimage, out buttonimagefocus, out picture))
      //          {
      //            bHome = true;
      //            row["bool2"] = xmlreader.GetValueAsBool("home", itemTag.SetupForm.PluginName(), bHome);
      //            row["bool3"] = xmlreader.GetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), bPlugins);
      //          }
      //        }
      //      }
      //    }
      //  }
      //}
      //catch (Exception) { }
    }


    public override void SaveSettings()
    {
      //LoadAll();
      //try
      //{
      //  using (Settings xmlwriter = new Settings("MediaPortal.xml"))
      //  {
      //    foreach (DataRow row in ds.Tables[0].Rows)
      //    {
      //      ItemTag itemTag = row["tag"] as ItemTag;

      //      bool bEnabled = (bool)row["bool1"];
      //      bool bHome = (bool)row["bool2"];
      //      bool bPlugins = (bool)row["bool3"];
      //      if (itemTag.SetupForm != null)
      //      {
      //        if (itemTag.SetupForm.DefaultEnabled() && !itemTag.SetupForm.CanEnable())
      //        {
      //          bEnabled = true;
      //        }
      //      }
      //      else
      //      {
      //        bEnabled = true;
      //      }
      //      xmlwriter.SetValueAsBool("plugins", itemTag.SetupForm.PluginName(), bEnabled);
      //      xmlwriter.SetValueAsBool("home", itemTag.SetupForm.PluginName(), bHome);
      //      xmlwriter.SetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), bPlugins);
      //      xmlwriter.SetValueAsBool("pluginsdlls", itemTag.DLLName, bEnabled);
      //      if (itemTag.strType != String.Empty)
      //      {
      //        xmlwriter.SetValueAsBool("pluginswindows", itemTag.strType, bEnabled);
      //      }
      //    }
      //  }
      //}
      //catch (Exception) { }
    }




  }
}
