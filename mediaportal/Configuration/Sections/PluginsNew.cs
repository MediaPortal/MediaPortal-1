#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod/Frodo

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod/Frodo
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
      public bool ShowDefaultHome = false;
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
        Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

        if (pluginAssembly != null)
        {
          Type[] exportedTypes = pluginAssembly.GetExportedTypes();

          foreach (Type type in exportedTypes)
          {
            // an abstract class cannot be instanciated
            if (type.IsAbstract)
              continue;
            //
            // Try to locate the interface we're interested in
            //
            if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
            {
              //
              // Create instance of the current type
              //
              object pluginObject = Activator.CreateInstance(type);
              ISetupForm pluginForm = pluginObject as ISetupForm;
              IExternalPlayer extPlayer = pluginObject as IExternalPlayer;
              IShowPlugin showPlugin = pluginObject as IShowPlugin;

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
                if (showPlugin != null)
                  tag.ShowDefaultHome = showPlugin.ShowDefaultHome();
                loadedPlugins.Add(tag);
              }
            }
          }
          foreach (Type t in exportedTypes)
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
      }
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings("MediaPortal.xml"))
        foreach (ListViewItem item in listViewPlugins.Items)
        {
          ItemTag itemTag = (ItemTag)item.Tag;

          if (itemTag.SetupForm != null)
          {
            if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
              itemTag.IsEnabled = xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(), itemTag.SetupForm.DefaultEnabled());
            else
              itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();

            if (itemTag.IsWindow)
            {
              bool isHome = false;
              bool isPlugins = false;
              string dummy = string.Empty;

              if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled() && itemTag.SetupForm.GetHome(out dummy, out dummy, out dummy, out dummy))
              {
                isHome = itemTag.ShowDefaultHome;
                isPlugins = !isHome;
                itemTag.IsHome = xmlreader.GetValueAsBool("home", itemTag.SetupForm.PluginName(), isHome);
                itemTag.IsPlugins = xmlreader.GetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), isPlugins);
              }
            }
          }
          updateListViewItem(item);
        }
    }

    public override void SaveSettings()
    {
      LoadAll();
      using (Settings xmlwriter = new Settings("MediaPortal.xml"))
        foreach (ListViewItem item in listViewPlugins.Items)
        {
          ItemTag itemTag = (ItemTag)item.Tag;

          bool isEnabled = itemTag.IsEnabled;
          bool isHome = itemTag.IsHome;
          bool isPlugins = itemTag.IsPlugins;

          xmlwriter.SetValueAsBool("plugins", itemTag.SetupForm.PluginName(), isEnabled);
          xmlwriter.SetValueAsBool("pluginsdlls", itemTag.DllName, isEnabled);

          if ((isEnabled) && (!isHome && !isPlugins))
            isHome = true;

          if (itemTag.IsWindow)
          {
            xmlwriter.SetValueAsBool("home", itemTag.SetupForm.PluginName(), isHome);
            xmlwriter.SetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), isPlugins);
            xmlwriter.SetValueAsBool("pluginswindows", itemTag.Type, isEnabled);
          }
        }
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
          if (((ItemTag)item.Tag).IsHome)
            item.ImageIndex = 8;
          else if (((ItemTag)item.Tag).IsPlugins)
            item.ImageIndex = 9;
          else
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
      if (itemTag.IsPlugins)
        itemTag.IsHome = false;
      if (!itemTag.IsPlugins && !itemTag.IsHome)
        itemTag.IsPlugins = true;
      updateListViewItem(listViewPlugins.FocusedItem);
    }

    void itemMyHome_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsHome = !itemTag.IsHome;
      if (itemTag.IsHome)
        itemTag.IsPlugins = false;
      if (!itemTag.IsPlugins && !itemTag.IsHome)
        itemTag.IsHome = true;
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
          addContextMenuItem("Enabled", "Disabled", imageListContextMenu.Images[1], true);
        }
        else
        {
          addContextMenuItem("Enabled", "Enabled", imageListContextMenu.Images[0], true);
          string dummy = string.Empty;
          if (itemTag.SetupForm.CanEnable() && itemTag.IsWindow && itemTag.SetupForm.GetHome(out dummy, out dummy, out dummy, out dummy))
          {
            if (!itemTag.IsHome) addContextMenuItem("My Home", "Listed in Home", imageListContextMenu.Images[1], true);
            else addContextMenuItem("My Home", "Listed in Home", imageListContextMenu.Images[0], true);

            if (!itemTag.IsPlugins) addContextMenuItem("My Plugins", "Listed in My Plugins", imageListContextMenu.Images[1], true);
            else addContextMenuItem("My Plugins", "Listed in My Plugins", imageListContextMenu.Images[0], true);
          }

          if (itemTag.SetupForm.HasSetup())
          {
            addContextMenuSeparator();
            addContextMenuItem("Config", "Configuration", null, true);
          }
        }
      }
    }

  }
}
