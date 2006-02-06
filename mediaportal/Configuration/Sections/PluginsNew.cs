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
using MediaPortal.Player;

namespace MediaPortal.Configuration.Sections
{
  public partial class PluginsNew : MediaPortal.Configuration.SectionSettings
  {
    private ArrayList loadedPlugins = new ArrayList();
    private ArrayList availablePlugins = new ArrayList();
    bool isLoaded = false;

    private class ItemTag
    {
      public string DllName;
      public ISetupForm SetupForm;
      public string Type = string.Empty;
      public int WindowId = -1;
      public bool IsProcess = false;
      public bool IsWindow = false;
      public bool IsExternalPlayer = false;
      public bool IsHome = false;
      public bool IsEnabled = false;
      public bool IsPlugins = false;
    }

    public PluginsNew()
      : this("Plugins")
    {
    }

    public PluginsNew(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

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
        loadPlugins();

        //
        // Populate our list
        //
        populateListView();
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
          availablePlugins.Add(file);
      }
    }


    private void populateListView()
    {
      foreach (ItemTag tag in loadedPlugins)
      {
        ListViewItem item = null;
        if (tag.IsProcess)
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupProcess"]);
        else if (tag.IsWindow)
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupWindow"]);
        else if (tag.IsExternalPlayer)
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupExternalPlayers"]);
        else
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupOther"]);
        item.Tag = tag;
        item.ToolTipText = string.Format("{0}", tag.SetupForm.Description());
        listViewPlugins.Items.Add(item);
        updateListViewItem(item);
      }
    }


    private void loadPlugins()
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
                  IExternalPlayer extPlayer = pluginObject as IExternalPlayer;

                  if (pluginForm != null)
                  {
                    ItemTag tag = new ItemTag();
                    tag.SetupForm = pluginForm;
                    tag.DllName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                    tag.WindowId = pluginForm.GetWindowId();
                    if (extPlayer != null)
                      tag.IsExternalPlayer = true;
                    else
                      tag.IsProcess = true;
                    loadedPlugins.Add(tag);
                  }
                }
                catch (Exception setupFormException)
                {
                  Log.Write("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                  Log.Write("Current class is :{0}", type.FullName);
                  Log.Write(setupFormException.StackTrace);
                }
              }
            }
            foreach (Type t in exportedTypes)
              try
              {
                if ((t.IsClass) && (t.IsSubclassOf(typeof(GUIWindow))))
                {
                  object newObj = Activator.CreateInstance(t);
                  GUIWindow win = (GUIWindow)newObj;

                  foreach (ItemTag tag in loadedPlugins)
                    if (tag.WindowId == win.GetID)
                    {
                      tag.Type = win.GetType().ToString();
                      tag.IsProcess = false;
                      tag.IsWindow = true;
                      break;
                    }
                }

              }
              catch (Exception guiWindowException)
              {
                Log.Write("Exception in plugin GUIWindows loading :{0}", guiWindowException.Message);
                Log.Write("Current class is :{0}", t.FullName);
                Log.Write(guiWindowException.StackTrace);
              }
          }
        }
        catch (Exception unknownException)
        {
          Log.Write("Exception in plugin loading :{0}", unknownException.Message);
          Log.Write(unknownException.StackTrace);
        }
      }
    }


    public override void LoadSettings()
    {
      try
      {
        using (Settings xmlreader = new Settings("MediaPortal.xml"))
        {
          foreach (ListViewItem item in listViewPlugins.Items)
          {
            ItemTag itemTag = (ItemTag)item.Tag;

            if (itemTag.SetupForm != null)
            {
              if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
              {
                itemTag.IsEnabled = xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(), itemTag.SetupForm.DefaultEnabled());
              }
              else
              {
                itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
              }

              if (itemTag.IsWindow)
              {
                bool bHome = false;
                bool bPlugins = false;
                itemTag.IsHome = bHome;
                itemTag.IsPlugins = bPlugins;
                string buttontxt, buttonimage, buttonimagefocus, picture;
                if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
                {
                  if (itemTag.SetupForm.GetHome(out buttontxt, out buttonimage, out buttonimagefocus, out picture))
                  {
                    bHome = true;
                    itemTag.IsHome = xmlreader.GetValueAsBool("home", itemTag.SetupForm.PluginName(), bHome);
                    itemTag.IsPlugins = xmlreader.GetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), bPlugins);
                  }
                }
              }
            }
            updateListViewItem(item);
          }
        }
      }
      catch (Exception) { }
    }


    public override void SaveSettings()
    {
      LoadAll();
      try
      {
        using (Settings xmlwriter = new Settings("MediaPortal.xml"))
        {
          foreach (ListViewItem item in listViewPlugins.Items)
          {
            ItemTag itemTag = (ItemTag)item.Tag;

            bool bEnabled = itemTag.IsEnabled;
            bool bHome = itemTag.IsHome;
            bool bPlugins = itemTag.IsPlugins;
            xmlwriter.SetValueAsBool("plugins", itemTag.SetupForm.PluginName(), bEnabled);
            xmlwriter.SetValueAsBool("pluginsdlls", itemTag.DllName, bEnabled);
            if ((bEnabled) && (!bHome && !bPlugins))
              bHome = true;
            if (itemTag.IsWindow)
            {
              xmlwriter.SetValueAsBool("home", itemTag.SetupForm.PluginName(), bHome);
              xmlwriter.SetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), bPlugins);
              xmlwriter.SetValueAsBool("pluginswindows", itemTag.Type, bEnabled);
            }
          }
        }
      }
      catch (Exception) { }
    }


    private void listViewPlugins_DoubleClick(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsEnabled = !itemTag.IsEnabled;
      if (!itemTag.SetupForm.CanEnable())
        itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();

      updateListViewItem(listViewPlugins.FocusedItem);
    }


    private void updateListViewItem(ListViewItem item)
    {
      if (((ItemTag)item.Tag).IsWindow)
        if (((ItemTag)item.Tag).IsEnabled)
          item.ImageIndex = 2;
        else
          item.ImageIndex = 3;
      else if (((ItemTag)item.Tag).IsProcess)
        if (((ItemTag)item.Tag).IsEnabled)
          item.ImageIndex = 4;
        else
          item.ImageIndex = 5;
      else if (((ItemTag)item.Tag).IsExternalPlayer)
        if (((ItemTag)item.Tag).IsEnabled)
          item.ImageIndex = 6;
        else
          item.ImageIndex = 7;
      else if (((ItemTag)item.Tag).IsEnabled)
        item.ImageIndex = 0;
      else
        item.ImageIndex = 1;
    }


    void itemMyPlugins_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsPlugins = !itemTag.IsPlugins;

      updateListViewItem(listViewPlugins.FocusedItem);
    }


    void itemMyHome_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsHome = !itemTag.IsHome;

      updateListViewItem(listViewPlugins.FocusedItem);
    }


    void itemEnabled_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsEnabled = !itemTag.IsEnabled;
      if (!itemTag.SetupForm.CanEnable())
        itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();

      updateListViewItem(listViewPlugins.FocusedItem);
    }


    void itemConfigure_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      if ((itemTag.SetupForm != null) &&
        (itemTag.SetupForm.HasSetup()))
        itemTag.SetupForm.ShowPlugin();
    }


    private void listViewPlugins_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
        contextMenuStrip.Show(MousePosition);
    }


    private void addContextMenuItem(string name, string menuEntry, Image image, bool clickable)
    {
      ToolStripItem item = null;
      if (clickable)
        item = contextMenuStrip.Items.Add(string.Empty);
      else
      {
        item = new ToolStripLabel(string.Empty);
        contextMenuStrip.Items.Add(item);
      }
      item.Text = menuEntry;
      item.Image = image;
      item.Name = name;

      switch (item.Name)
      {
        case "Config":
          item.Click += new EventHandler(itemConfigure_Click);
          break;
        case "Enabled":
          item.Click += new EventHandler(itemEnabled_Click);
          break;
        case "My Home":
          item.Click += new EventHandler(itemMyHome_Click);
          break;
        case "My Plugins":
          item.Click += new EventHandler(itemMyPlugins_Click);
          break;
        case "Name":
          item.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          break;
        case "Author":
          item.Font = new System.Drawing.Font("Tahoma", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          break;
      }
    }


    private void addContextMenuSeparator()
    {
      int item = contextMenuStrip.Items.Add(new ToolStripSeparator());
      contextMenuStrip.Items[item].Name = "Separator";
    }


    private void listViewPlugins_Click(object sender, EventArgs e)
    {
      contextMenuStrip.Items.Clear();

      if (listViewPlugins.FocusedItem != null)
      {
        ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
        
        addContextMenuItem("Name", itemTag.SetupForm.PluginName(), null, false);
        addContextMenuItem("Author", string.Format("Author: {0}", itemTag.SetupForm.Author()), null, false);
        addContextMenuSeparator();

        if (!itemTag.IsEnabled)
        {
          if (itemTag.SetupForm.CanEnable())
            addContextMenuItem("Enabled", "Plugin is disabled", null, true);
          else
            addContextMenuItem("Enabled", "Plugin is always disabled", null, true);
        }
        else
        {
          addContextMenuItem("", "Plugin is...", null, false);

          if (itemTag.SetupForm.CanEnable())
          {
            addContextMenuItem("Enabled", "  ...enabled", imageListContextMenu.Images[0], true);
            if (itemTag.IsWindow)
            {
              if (!itemTag.IsHome) addContextMenuItem("My Home", "  ...not listed in Home", null, true);
              else addContextMenuItem("My Home", "  ...listed in Home", imageListContextMenu.Images[1], true);

              if (!itemTag.IsPlugins) addContextMenuItem("My Plugins", "  ...not listed in My Plugins", null, true);
              else addContextMenuItem("My Plugins", "  ...listed in My Plugins", imageListContextMenu.Images[2], true);
            }
          }
          else
            addContextMenuItem("Enabled", "  ...always enabled", imageListContextMenu.Images[0], true);

          if (itemTag.SetupForm.HasSetup())
          {
            addContextMenuSeparator();
            addContextMenuItem("Config", "Configuration", null, true);
          }
        }

        //labelPluginName.Text = String.Format("{0}", tag.SetupForm.PluginName());
        //labelAuthor.Text = String.Format("written by {0}", tag.SetupForm.Author());
        //labelDescription.Text = String.Format("{0}", tag.SetupForm.Description());
        //groupBoxPluginInfo.Visible = true;
      }
    }
    
  }
}
