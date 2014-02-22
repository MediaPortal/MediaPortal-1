#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Common.Utils;

namespace MediaPortal.Configuration.Sections
{
  public partial class PluginsNew : SectionSettings
  {
		private System.Windows.Forms.ImageList imageListLargePlugins;
		private System.Windows.Forms.ImageList imageListContextMenu;
		private System.Windows.Forms.ImageList imageListMPInstaller;
    private bool pluginsLoadedOnPage = false;

    public PluginsNew()
      : this("Plugins")
    {
    }

    public PluginsNew(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      this.imageListLargePlugins = new System.Windows.Forms.ImageList()
                                     {
                                       ColorDepth = ColorDepth.Depth32Bit,
                                       TransparentColor = Color.Transparent,
                                       ImageSize = new Size(42, 42)
                                     };
      this.imageListContextMenu = new System.Windows.Forms.ImageList()
                                    {
                                      ColorDepth = ColorDepth.Depth32Bit,
                                      TransparentColor = Color.Transparent,
                                      ImageSize = new Size(16, 16)
                                    };
      this.imageListMPInstaller = new System.Windows.Forms.ImageList()
                                    {
                                      ColorDepth = ColorDepth.Depth32Bit,
                                      TransparentColor = Color.Transparent,
                                      ImageSize = new Size(42, 42)
                                    };

			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
			// 
			// imageListLargePlugins
			// 
			this.imageListLargePlugins.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListLargePlugins.Images.Add("0",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.00_plugin_other.png")));
      this.imageListLargePlugins.Images.Add("1",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.01_plugin_other_off.png")));
      this.imageListLargePlugins.Images.Add("2",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.02_plugin_window.png")));
      this.imageListLargePlugins.Images.Add("3",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.03_plugin_window_off.png")));
      this.imageListLargePlugins.Images.Add("4",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.04_plugin_process.png")));
      this.imageListLargePlugins.Images.Add("5",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.05_plugin_process_off.png")));
      this.imageListLargePlugins.Images.Add("6",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.06_plugin_externalplayers.png")));
      this.imageListLargePlugins.Images.Add("7",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.07_plugin_externalplayers_off.png")));
      this.imageListLargePlugins.Images.Add("8",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.08_plugin_window_home.png")));
      this.imageListLargePlugins.Images.Add("9",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.09_plugin_window_plugins.png")));
      this.imageListLargePlugins.Images.Add("10",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.10_plugin_other_incomp.png")));
      this.imageListLargePlugins.Images.Add("11",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.11_plugin_other_off_incomp.png")));
      this.imageListLargePlugins.Images.Add("12",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.12_plugin_window_incomp.png")));
      this.imageListLargePlugins.Images.Add("13",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.13_plugin_window_off_incomp.png")));
      this.imageListLargePlugins.Images.Add("14",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.14_plugin_process_incomp.png")));
      this.imageListLargePlugins.Images.Add("15",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.15_plugin_process_off_incomp.png")));
      this.imageListLargePlugins.Images.Add("16",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.16_plugin_externalplayers_incomp.png")));
      this.imageListLargePlugins.Images.Add("17",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.17_plugin_externalplayers_off_incomp.png")));
      this.imageListLargePlugins.Images.Add("18",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.18_plugin_window_home_incomp.png")));
      this.imageListLargePlugins.Images.Add("19",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.19_plugin_window_plugins_incomp.png")));
      this.imageListLargePlugins.Images.Add("20",
                                            Image.FromStream(
                                              asm.GetManifestResourceStream(
                                                "MediaPortal.Configuration.Sections.Images.Plugins.imageListLargePlugins.20_alert-ovl.png")));

			// 
			// imageListContextMenu
			// 
			this.imageListContextMenu.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListContextMenu.Images.Add("0",
                                           Image.FromStream(
                                             asm.GetManifestResourceStream(
                                               "MediaPortal.Configuration.Sections.Images.Plugins.imageListContextMenu.00_Enabled.png.bmp")));
      this.imageListContextMenu.Images.Add("1",
                                           Image.FromStream(
                                             asm.GetManifestResourceStream(
                                               "MediaPortal.Configuration.Sections.Images.Plugins.imageListContextMenu.01_Enabled_off.png.bmp")));

			// 
			// imageListMPInstaller
			// 
			this.imageListMPInstaller.TransparentColor = System.Drawing.Color.Transparent;
      this.imageListMPInstaller.Images.Add("0",
                                           Image.FromStream(
                                             asm.GetManifestResourceStream(
                                               "MediaPortal.Configuration.Sections.Images.Plugins.imageListMPInstaller.00_application.ico.bmp")));

			this.listViewPlugins.LargeImageList = this.imageListLargePlugins;
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadAll();
    }

    public void LoadAll()
    {
      if (!Plugins.IsLoaded || (Plugins.WasLastLoadAdvanced != SettingsForm.AdvancedMode))
      {
        Plugins.ClearLoadedPlugins();
        Plugins.IsLoaded = true;
        Plugins.EnumeratePlugins();
        Plugins.LoadPlugins();
        pluginsLoadedOnPage = false;
      }

      if (!pluginsLoadedOnPage)
      {
        listViewPlugins.Items.Clear();
        foreach (ItemTag tag in Plugins.LoadedPlugins)
        {
          LoadPluginImages(tag.expType, tag);
        }
        LoadSettings();
        PopulateListView();
        pluginsLoadedOnPage = true;
      }
    }

    private Image OverlayImage(Image targetImage, Image overlay)
    {
      PixelFormat fmt = targetImage.PixelFormat;
      if (fmt != PixelFormat.Format32bppRgb &&
          fmt != PixelFormat.Format32bppArgb &&
          fmt != PixelFormat.Format32bppPArgb &&
          fmt != PixelFormat.Format24bppRgb)
      {
        Image result = new Bitmap(targetImage.Width, targetImage.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(result))
        {
          g.DrawImage(targetImage, 0, 0);
          g.CompositingMode = CompositingMode.SourceOver;
          g.DrawImage(overlay, 0, 0, targetImage.Width, targetImage.Height);
        }
        return result;
      }
      else
      {
        using (var g = Graphics.FromImage(targetImage))
        {
          g.CompositingMode = CompositingMode.SourceOver;
          g.DrawImage(overlay, 0, 0, targetImage.Width, targetImage.Height);
        }
        return targetImage;
      }
    }

    /// <summary>
    /// Checks whether the a plugin has a <see cref="PluginIconsAttribute"/> defined.  If it has, the images that are indicated
    /// in the attribute are loaded
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to examine.</param>
    /// <param name="tag">The <see cref="ItemTag"/> to store the images in.</param>
    private void LoadPluginImages(Type type, ItemTag tag)
    {
      PluginIconsAttribute[] icons =
        (PluginIconsAttribute[]) type.GetCustomAttributes(typeof (PluginIconsAttribute), false);
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
        if (tag.IsIncompatible && tag.ActiveImage != null)
        {
          tag.ActiveImage = OverlayImage(tag.ActiveImage, imageListLargePlugins.Images[20]);
        }
      }
      resourceName = icons[0].DeactivatedResourceName;
      if (!string.IsNullOrEmpty(resourceName))
      {
        Log.Debug("PluginsNew: load deactivated image from resource - {0}", resourceName);
        tag.InactiveImage = LoadImageFromResource(type, resourceName);
        if (tag.IsIncompatible && tag.ActiveImage != null)
        {
          tag.InactiveImage = OverlayImage(tag.InactiveImage, imageListLargePlugins.Images[20]);
        }
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

    private void PopulateListView()
    {
      foreach (ItemTag tag in Plugins.LoadedPlugins)
      {
        // Show only common, stable plugins
        if (!SettingsForm.AdvancedMode)
        {
          if (tag.IsExternalPlayer)
          {
            continue;
          }
          if (tag.IsProcess && !tag.IsEnabled && tag.SetupForm.PluginName() != "PowerScheduler")
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
        if (tag.IsIncompatible)
        {
          item = new ListViewItem(tag.SetupForm.PluginName(), listViewPlugins.Groups["listViewGroupIncompatible"]);
        }
        else if (tag.IsProcess)
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


    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        foreach (ItemTag itemTag in Plugins.LoadedPlugins)
        {
          if (itemTag.SetupForm != null)
          {
            if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
            {
              // Enable PowerScheduler if PS++ is enabled
              if (itemTag.SetupForm.PluginName() == "PowerScheduler" &&
                xmlreader.GetValueAsBool("plugins", "PowerScheduler++", false))
              {
                itemTag.IsEnabled = true;
              }
              else
              {
                itemTag.IsEnabled =
                  xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(), itemTag.SetupForm.DefaultEnabled());
              }
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
          ItemTag itemTag = (ItemTag) item.Tag;

          bool isEnabled = itemTag.IsEnabled;
          bool isHome = itemTag.IsHome;
          bool isPlugins = itemTag.IsPlugins;

          if (itemTag.IsIncompatible)
          {
            continue;
          }

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

        // Remove PS++ entries
        xmlwriter.RemoveEntry("plugins", "PowerScheduler++");
        xmlwriter.RemoveEntry("home", "PowerScheduler++");
        xmlwriter.RemoveEntry("myplugins", "PowerScheduler++");
        xmlwriter.RemoveEntry("pluginswindows", "PowerScheduler++");
      }
    }

    private void listViewPlugins_DoubleClick(object sender, EventArgs e)
    {
      if (listViewPlugins.FocusedItem == null) return;
      if (listViewPlugins.FocusedItem.Tag == null) return;

      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      if (itemTag.IsIncompatible)
      {
        return;
      }
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
      ItemTag tag = (ItemTag) item.Tag;

      int incompatibleImageOfs = tag.IsIncompatible ? 10 : 0;

      if (tag.IsWindow)
      {
        item.Font = new Font(item.Font.FontFamily, 8.5f);
        if (tag.IsEnabled)
        {
          if (tag.IsHome)
          {
            item.ImageIndex = 8 + incompatibleImageOfs;
            item.ForeColor = Color.Green;
          }
          else if (tag.IsPlugins)
          {
            item.ImageIndex = 9 + incompatibleImageOfs;
            item.ForeColor = Color.RoyalBlue;
          }
          else
          {
            item.ImageIndex = 2 + incompatibleImageOfs;
            item.ForeColor = Color.DarkSlateGray;
          }
        }
        else
        {
          item.ImageIndex = 3 + incompatibleImageOfs;
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
          item.ImageIndex = (tag.IsEnabled ? 4 : 5) + incompatibleImageOfs;
        }
        else if (tag.IsExternalPlayer)
        {
          item.ImageIndex = (tag.IsEnabled ? 6 : 7) + incompatibleImageOfs;
        }
        else
        {
          item.ImageIndex = (tag.IsEnabled ? 0 : 1) + incompatibleImageOfs;
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
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      if (itemTag.IsIncompatible)
      {
        return;
      }
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
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      if (itemTag.IsIncompatible)
      {
        return;
      }
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
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      if (itemTag.IsIncompatible)
      {
        return;
      }
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
      ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;
      if (itemTag.IsIncompatible)
      {
        return;
      }
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
        ItemTag itemTag = (ItemTag) listViewPlugins.FocusedItem.Tag;

        addContextMenuItem("Name", itemTag.SetupForm.PluginName(), null, false);
        addContextMenuItem("Author", string.Format("Author: {0}", itemTag.SetupForm.Author()), null, false);
        if (itemTag.IsIncompatible)
        {
          return;
        }

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
      catch
      {
    }
  }
  }
}