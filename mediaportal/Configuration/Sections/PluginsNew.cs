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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.MPInstaller;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class PluginsNew : SectionSettings
  {
    private ArrayList loadedPlugins = new ArrayList();
    private ArrayList availablePlugins = new ArrayList();
    private bool isLoaded = false;
    private bool wasLastLoadAdvanced = false;

    public MPInstallHelper lst = new MPInstallHelper();
    public MPInstallHelper lst_online = new MPInstallHelper();
    private string InstalDir = Config.GetFolder(Config.Dir.Base) + @"\" + "Installer";


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
      private Image activeImage = null;
      private Image inactiveImage = null;

      public Image ActiveImage
      {
        get { return activeImage; }
        set { activeImage = value; }
      }

      public Image InactiveImage
      {
        get { return inactiveImage; }
        set { inactiveImage = value; }
      }
    }

    public PluginsNew()
      : this("Plugins") {}

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
      if (!isLoaded || (wasLastLoadAdvanced != SettingsForm.AdvancedMode))
      {
        ClearLoadedPlugins();
        isLoaded = true;

        EnumeratePlugins();
        LoadPlugins();

        LoadSettings();
        PopulateListView();
      }
    }

    private void EnumeratePlugins()
    {
      // Save to determine whether the mode has changed
      wasLastLoadAdvanced = SettingsForm.AdvancedMode;

      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "subtitle"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "tagreaders"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "externalplayers"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
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
        // Show only common, stable plugins
        if (!SettingsForm.AdvancedMode)
        {
          if (tag.IsExternalPlayer)
          {
            continue;
          }
          if (tag.IsProcess && !tag.IsEnabled && tag.SetupForm.PluginName() != "Audioscrobbler")
          {
            continue;
          }

          if (tag.WindowId == 760) // GUIBurner
          {
            continue;
          }
          if (tag.WindowId == 2700) // GUIRSSFeed
          {
            continue;
          }
          if (tag.WindowId == 5000) // GUIAlarm
          {
            continue;
          }
          if (tag.WindowId == 3005) // GUITopbar
          {
            continue;
          }
        }

        // Hide the Music Share Watcher Plugin
        // It is Enabled / Disabled via the "Auto-Update DB on changes in Shares" flag
        if (tag.SetupForm.PluginName() == "Music Share Watcher")
        {
          continue;
        }

        ListViewItem item;
        if (tag.IsProcess)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupProcess"]);
        }
        else if (tag.IsWindow)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupWindow"]);
        }
        else if (tag.IsExternalPlayer)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupExternalPlayers"]);
        }
        else
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupOther"]);
        }
        item.Tag = tag;
        item.ToolTipText = string.Format("{0}", tag.SetupForm.Description());
        listViewPlugins.Items.Add(item);
        updateListViewItem(item);
      }
    }

    private void ClearLoadedPlugins()
    {
      listViewPlugins.Items.Clear();
      availablePlugins.Clear();
      loadedPlugins.Clear();
    }

    private void LoadPlugins()
    {
      foreach (string pluginFile in availablePlugins)
      {
        Assembly pluginAssembly = null;
        try
        {
          Log.Debug("PluginsNew: loadPlugins {0}", pluginFile);
          pluginAssembly = Assembly.LoadFrom(pluginFile);
        }
        catch (BadImageFormatException)
        {
          Log.Warn("PluginsNew: {0} has a bad image format", pluginFile);
        }

        if (pluginAssembly != null)
        {
          try
          {
            Type[] exportedTypes = pluginAssembly.GetExportedTypes();
            List<object> NonSetupWindows = new List<object>();

            foreach (Type type in exportedTypes)
            {
              bool isPlugin = (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null);
              bool isGuiWindow = ((type.IsClass) && (type.IsSubclassOf(typeof (GUIWindow))));

              // an abstract class cannot be instanciated
              if (type.IsAbstract)
              {
                continue;
              }

              // Try to locate the interface we're interested in
              if (isPlugin || isGuiWindow)
              {
                // Create instance of the current type
                object pluginObject;
                try
                {
                  pluginObject = Activator.CreateInstance(type);
                }
                catch (TargetInvocationException)
                {
                  MessageBox.Show(
                    string.Format(
                      "An error occured while loading the plugin {0}.\n\nIt's incompatible with the current MediaPortal version and won't be loaded.",
                      type.FullName
                      ), "Plugin Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  Log.Warn(
                    "Plugin Manager: Plugin {0} is incompatible with the current MediaPortal version! (File: {1})",
                    type.FullName, pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
                  continue;
                }

                if (isPlugin)
                {
                  ISetupForm pluginForm = pluginObject as ISetupForm;
                  IExternalPlayer extPlayer = pluginObject as IExternalPlayer;
                  IShowPlugin showPlugin = pluginObject as IShowPlugin;

                  if (pluginForm != null)
                  {
                    ItemTag tag = new ItemTag();
                    tag.SetupForm = pluginForm;
                    tag.DllName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                    tag.WindowId = pluginForm.GetWindowId();

                    if (isGuiWindow)
                    {
                      GUIWindow win = (GUIWindow)pluginObject;
                      if (tag.WindowId == win.GetID)
                      {
                        tag.Type = win.GetType().ToString();
                        tag.IsProcess = false;
                        tag.IsWindow = true;
                      }
                    }
                    else if (extPlayer != null)
                    {
                      tag.IsExternalPlayer = true;
                    }
                    else
                    {
                      tag.IsProcess = true;
                    }

                    if (showPlugin != null)
                    {
                      tag.ShowDefaultHome = showPlugin.ShowDefaultHome();
                    }

                    LoadPluginImages(type, tag);
                    loadedPlugins.Add(tag);
                  }
                }
                else
                {
                  NonSetupWindows.Add(pluginObject);
                }
              }
            }
            // Filter plugins from e.g. dialogs or other windows.
            foreach (GUIWindow win in NonSetupWindows)
            {
              foreach (ItemTag tag in loadedPlugins)
              {
                if (tag.WindowId == win.GetID)
                {
                  tag.Type = win.GetType().ToString();
                  tag.IsProcess = false;
                  tag.IsWindow = true;
                  Log.Debug(
                    "PluginsNew: {0}, window plugin, does not implement \"ISetupForm\" and \"GUIWindow\" in the same class",
                    tag.Type);
                  break;
                }
              }
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show(
              string.Format(
                "An error occured while loading the plugin file {0}.\n\nIt's broken or incompatible with the current MediaPortal version and won't be loaded.",
                pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1)), "Plugin Manager", MessageBoxButtons.OK,
              MessageBoxIcon.Error);
            Log.Warn(
              "PluginManager: Plugin file {0} is broken or incompatible with the current MediaPortal version and won't be loaded!",
              pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1));
            Log.Error("PluginManager: Exception: {0}", ex);
          }
        }
      }
    }

    /// <summary>
    /// Checks whether the a plugin has a <see cref="PluginIconsAttribute"/> defined.  If it has, the images that are indicated
    /// in the attribute are loaded
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to examine.</param>
    /// <param name="tag">The <see cref="ItemTag"/> to store the images in.</param>
    private static void LoadPluginImages(Type type, ItemTag tag)
    {
      PluginIconsAttribute[] icons =
        (PluginIconsAttribute[])type.GetCustomAttributes(typeof (PluginIconsAttribute), false);
      if (icons == null || icons.Length == 0)
      {
        //Log.Debug("PluginsNew: no icons");
        return;
      }
      string resourceName = icons[0].ActivatedResourceName;
      if (!string.IsNullOrEmpty(resourceName))
      {
        Log.Debug("PluginsNew: load active image from resource - {0}", resourceName);
        tag.ActiveImage = LoadImageFromResource(type, resourceName);
      }
      resourceName = icons[0].DeactivatedResourceName;
      if (!string.IsNullOrEmpty(resourceName))
      {
        Log.Debug("PluginsNew: load deactivated image from resource - {0}", resourceName);
        tag.InactiveImage = LoadImageFromResource(type, resourceName);
      }
    }

    private static Image LoadImageFromResource(Type type, string resourceName)
    {
      try
      {
        return Image.FromStream(type.Assembly.GetManifestResourceStream(resourceName));
      }
      catch (ArgumentException aex)
      {
        Log.Error("PluginsNew: Argument Exception loading the image - {0}, {1}", resourceName, aex.Message);
        //Thrown when the stream does not seem to contain a valid image
      }
      catch (FileLoadException lex)
      {
        Log.Error("PluginsNew: FileLoad Exception loading the image - {0}, {1}", resourceName, lex.Message);
        //Throw when the resource could not be loaded
      }
      catch (FileNotFoundException fex)
      {
        Log.Error("PluginsNew: FileNotFound Exception loading the image - {0}, {1}", resourceName, fex.Message);
        //Thrown when the resource could not be found
      }
      return null;
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        foreach (ItemTag itemTag in loadedPlugins)
        {
          if (itemTag.SetupForm != null)
          {
            if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
            {
              itemTag.IsEnabled =
                xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(), itemTag.SetupForm.DefaultEnabled());
            }
            else
            {
              itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
            }

            if (itemTag.IsWindow)
            {
              bool isHome;
              bool isPlugins;
              string dummy;

              if (itemTag.SetupForm.CanEnable() ||
                  itemTag.SetupForm.DefaultEnabled() &&
                  itemTag.SetupForm.GetHome(out dummy, out dummy, out dummy, out dummy))
              {
                isHome = itemTag.ShowDefaultHome;
                isPlugins = !isHome;
                itemTag.IsHome = xmlreader.GetValueAsBool("home", itemTag.SetupForm.PluginName(), isHome);
                itemTag.IsPlugins = xmlreader.GetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), isPlugins);
              }
            }
          }
        }
      }
    }

    public override void SaveSettings()
    {
      LoadAll();

      using (Settings xmlwriter = new MPSettings())
      {
        foreach (ListViewItem item in listViewPlugins.Items)
        {
          ItemTag itemTag = (ItemTag)item.Tag;

          bool isEnabled = itemTag.IsEnabled;
          bool isHome = itemTag.IsHome;
          bool isPlugins = itemTag.IsPlugins;

          xmlwriter.SetValueAsBool("plugins", itemTag.SetupForm.PluginName(), isEnabled);
          xmlwriter.SetValueAsBool("pluginsdlls", itemTag.DllName, isEnabled);

          if ((isEnabled) && (!isHome && !isPlugins))
          {
            isHome = true;
          }

          if (itemTag.IsWindow)
          {
            xmlwriter.SetValueAsBool("home", itemTag.SetupForm.PluginName(), isHome);
            xmlwriter.SetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), isPlugins);
            xmlwriter.SetValueAsBool("pluginswindows", itemTag.Type, isEnabled);
          }
        }
      }
    }

    private void listViewPlugins_DoubleClick(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsEnabled = !itemTag.IsEnabled;
      if (!itemTag.SetupForm.CanEnable())
      {
        itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
      }

      updateListViewItem(listViewPlugins.FocusedItem);
      listViewPlugins_Click(sender, e);
    }

    private void updateListViewItem(ListViewItem item)
    {
      ItemTag tag = (ItemTag)item.Tag;

      if (tag.IsWindow)
      {
        if (tag.IsEnabled)
        {
          item.Font = new Font(item.Font.FontFamily, 8.5f);
          if (tag.IsHome)
          {
            item.ImageIndex = 8;
            item.ForeColor = Color.Green;
          }
          else if (tag.IsPlugins)
          {
            item.ImageIndex = 9;
            item.ForeColor = Color.RoyalBlue;
          }
          else
          {
            item.ImageIndex = 2;
            item.ForeColor = Color.DarkSlateGray;
          }
        }
        else
        {
          item.Font = new Font(item.Font.FontFamily, 8.5f);
          item.ImageIndex = 3;
          item.ForeColor = Color.DimGray;
        }
      }
      else
      {
        if (tag.IsEnabled)
        {
          item.Font = new Font(item.Font.FontFamily, 8.5f);
          item.ForeColor = Color.DarkSlateGray;
        }
        else
        {
          item.Font = new Font(item.Font.FontFamily, 8.0f);
          item.ForeColor = Color.DimGray;
        }

        if (tag.IsProcess)
        {
          item.ImageIndex = tag.IsEnabled ? 4 : 5;
        }
        else if (tag.IsExternalPlayer)
        {
          item.ImageIndex = tag.IsEnabled ? 6 : 7;
        }
        else
        {
          item.ImageIndex = tag.IsEnabled ? 0 : 1;
        }
      }

      // If the plugin has its own icon we will be using this one.
      // Check the imagelist for enabled / disabled variants.
      if (tag.IsEnabled && tag.ActiveImage != null)
      {
        string enabledKey = item.Text + "_enabled";
        if (!item.ImageList.Images.ContainsKey(enabledKey))
        {
          item.ImageList.Images.Add(enabledKey, tag.ActiveImage);
        }
        item.ImageKey = enabledKey;
      }
      if (!tag.IsEnabled && tag.InactiveImage != null)
      {
        string disabledKey = item.Text + "_disabled";
        if (!item.ImageList.Images.ContainsKey(disabledKey))
        {
          item.ImageList.Images.Add(disabledKey, tag.InactiveImage);
        }
        item.ImageKey = disabledKey;
      }

      listViewPlugins.Refresh();
      //this.Refresh();
    }

    private void itemMyPlugins_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsPlugins = !itemTag.IsPlugins;
      if (itemTag.IsPlugins)
      {
        itemTag.IsHome = false;
      }
      if (!itemTag.IsPlugins && !itemTag.IsHome)
      {
        itemTag.IsPlugins = true;
      }
      updateListViewItem(listViewPlugins.FocusedItem);
      listViewPlugins_Click(sender, e);
    }

    private void itemMyHome_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsHome = !itemTag.IsHome;
      if (itemTag.IsHome)
      {
        itemTag.IsPlugins = false;
      }
      if (!itemTag.IsPlugins && !itemTag.IsHome)
      {
        itemTag.IsHome = true;
      }
      updateListViewItem(listViewPlugins.FocusedItem);
      listViewPlugins_Click(sender, e);
    }

    private void itemEnabled_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      itemTag.IsEnabled = !itemTag.IsEnabled;
      if (!itemTag.SetupForm.CanEnable())
      {
        itemTag.IsEnabled = itemTag.SetupForm.DefaultEnabled();
      }
      updateListViewItem(listViewPlugins.FocusedItem);
      listViewPlugins_Click(sender, e);
    }

    private void itemConfigure_Click(object sender, EventArgs e)
    {
      ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;
      if ((itemTag.SetupForm != null) &&
          (itemTag.SetupForm.HasSetup()))
      {
        itemTag.SetupForm.ShowPlugin();
      }
    }

    private void listViewPlugins_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        contextMenuStrip.Show(MousePosition);
      }
    }

    private void addContextMenuItem(string name, string menuEntry, Image image, bool clickable)
    {
      ToolStripItem item;
      if (clickable)
      {
        item = contextMenuStrip.Items.Add(string.Empty);
      }
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
          item.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
          break;
        case "Author":
          item.Font = new Font("Tahoma", 7.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
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
      OnItemSelected();
    }

    private void listViewPlugins_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
      if (e.Item != null)
      {
        OnItemSelected();
      }
    }

    private void OnItemSelected()
    {
      contextMenuStrip.Items.Clear();

      mpButtonHome.Enabled = false;
      mpButtonEnable.Enabled = false;
      mpButtonPlugin.Enabled = false;
      mpButtonConfig.Enabled = false;

      if (listViewPlugins.FocusedItem != null)
      {
        ItemTag itemTag = (ItemTag)listViewPlugins.FocusedItem.Tag;

        addContextMenuItem("Name", itemTag.SetupForm.PluginName(), null, false);
        addContextMenuItem("Author", string.Format("Author: {0}", itemTag.SetupForm.Author()), null, false);
        addContextMenuSeparator();
        mpButtonEnable.Enabled = true;

        if (!itemTag.IsEnabled)
        {
          addContextMenuItem("Enabled", "Disabled", imageListContextMenu.Images[1], true);
          mpButtonEnable.Text = "Enable";
        }
        else
        {
          addContextMenuItem("Enabled", "Enabled", imageListContextMenu.Images[0], true);
          string dummy;
          mpButtonEnable.Text = "Disable";
          if (itemTag.SetupForm.CanEnable() && itemTag.IsWindow &&
              itemTag.SetupForm.GetHome(out dummy, out dummy, out dummy, out dummy))
          {
            if (!itemTag.IsHome)
            {
              addContextMenuItem("My Home", "Listed in Home", imageListContextMenu.Images[1], true);
              mpButtonHome.Enabled = true;
            }
            else
            {
              addContextMenuItem("My Home", "Listed in Home", imageListContextMenu.Images[0], true);
            }

            if (!itemTag.IsPlugins)
            {
              addContextMenuItem("My Plugins", "Listed in My Plugins", imageListContextMenu.Images[1], true);
              mpButtonPlugin.Enabled = true;
            }
            else
            {
              addContextMenuItem("My Plugins", "Listed in My Plugins", imageListContextMenu.Images[0], true);
            }
          }

          if (itemTag.SetupForm.HasSetup())
          {
            addContextMenuSeparator();
            addContextMenuItem("Config", "Configuration", null, true);
            mpButtonConfig.Enabled = true;
          }
        }
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start(Config.GetFile(Config.Dir.Base, "MpeInstaller.exe"));
      }
      catch {}
    }
  }
}