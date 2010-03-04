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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.IO;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeInstaller.Dialogs;

namespace MpeInstaller.Controls
{
  public partial class ExtensionControl : UserControl
  {
    public PackageClass Package;
    public PackageClass UpdatePackage = null;
    private DownloadItem _downloadItem = new DownloadItem();
    private WebClient _client = new WebClient();

    public ExtensionControl(PackageClass packageClass)
    {

      _client.DownloadFileCompleted += _client_DownloadFileCompleted;

      InitializeComponent();
      lbl_name.Text = packageClass.GeneralInfo.Name + " - " + packageClass.GeneralInfo.Author;
      lbl_version.Text = packageClass.GeneralInfo.Version.ToString();
      lbl_description.Text = packageClass.GeneralInfo.ExtensionDescription;
      bool haveimage = false;

      if (Directory.Exists(packageClass.LocationFolder))
      {
        DirectoryInfo di = new DirectoryInfo(packageClass.LocationFolder);
        FileInfo[] fileInfos = di.GetFiles("icon.*");
        if (fileInfos.Length > 0)
        {
          img_logo.LoadAsync(fileInfos[0].FullName);
          haveimage = true;
        }
      }

      if (!haveimage && !string.IsNullOrEmpty(packageClass.GeneralInfo.Params[ParamNamesConst.ONLINE_ICON].Value))
      {
        try
        {
          _downloadItem.SourceUrl = packageClass.GeneralInfo.Params[ParamNamesConst.ONLINE_ICON].Value;
          _downloadItem.TempDestination = Path.GetTempFileName();
          _downloadItem.Destination = packageClass.LocationFolder + "icon" + Path.GetExtension(_downloadItem.SourceUrl);
          if (!Directory.Exists(Path.GetDirectoryName(_downloadItem.Destination)))
            Directory.CreateDirectory(Path.GetDirectoryName(_downloadItem.Destination));
          _client.DownloadFileAsync(new Uri(_downloadItem.SourceUrl), _downloadItem.TempDestination);
        }
        catch (Exception) {}
      }

      btn_screenshot.Enabled = !string.IsNullOrEmpty(packageClass.GeneralInfo.Params[ParamNamesConst.ONLINE_SCREENSHOT].Value);

      Package = MpeCore.MpeInstaller.InstalledExtensions.Get(packageClass);
      if (Package == null)
      {
        Package = packageClass;
        btn_conf.Visible = false;
        btn_update.Visible = false;
        btn_uninstall.Visible = false;
      }

      btn_home.Visible = !string.IsNullOrEmpty(Package.GeneralInfo.HomePage);
      btn_forum.Visible = !string.IsNullOrEmpty(Package.GeneralInfo.ForumPage);

      PopulateInstallBtn();
      btn_conf.Enabled = !string.IsNullOrEmpty(Package.GeneralInfo.Params[ParamNamesConst.CONFIG].GetValueAsPath());

      UpdatePackage = MpeCore.MpeInstaller.KnownExtensions.GetUpdate(Package);
      if (UpdatePackage != null)
      {
        btn_update.Visible = true;
        img_update.Visible = true;
        img_update1.Visible = true;
        toolTip1.SetToolTip(img_update, "New update available. Version: " + UpdatePackage.GeneralInfo.Version.ToString());
        toolTip1.SetToolTip(img_update1, "New update available. Version: " + UpdatePackage.GeneralInfo.Version.ToString());
      }
      else
      {
        btn_update.Visible = false;
        img_update.Visible = false;
        img_update1.Visible = false;
      }
      if (!Package.CheckDependency(true))
      {
        img_dep.Visible = true;
        img_dep1.Visible = true;
      }
      else
      {
        img_dep.Visible = false;
        img_dep1.Visible = false;
      }
      Selected = false;
      SelectControl();
    }

    void _client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      try
      {
        if (e.Error == null)
        {
          File.Copy(_downloadItem.TempDestination, _downloadItem.Destination, true);
          img_logo.LoadAsync(_downloadItem.Destination);
        }
      }
      catch (Exception)
      {

      }
    }

    private void PopulateInstallBtn()
    {
      ExtensionCollection collection = MpeCore.MpeInstaller.KnownExtensions.GetList(Package.GeneralInfo.Id);
      collection.Add(Package);
      foreach (PackageClass item in collection.GetList(Package.GeneralInfo.Id).Items)
      {
        ToolStripMenuItem testToolStripMenuItem = new ToolStripMenuItem();
        testToolStripMenuItem.Text = string.Format("Version - {0} [{1}]", item.GeneralInfo.Version,
                                                   item.GeneralInfo.DevelopmentStatus);
        PackageClass pak = MpeCore.MpeInstaller.InstalledExtensions.Get(Package.GeneralInfo.Id);
        if (pak != null && item.GeneralInfo.Version.CompareTo(pak.GeneralInfo.Version) == 0)
        {
          testToolStripMenuItem.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold,
                                                GraphicsUnit.Point, ((byte)(0)));
        }
        
        testToolStripMenuItem.ToolTipText = item.GeneralInfo.VersionDescription.Length > 1024
                                              ? item.GeneralInfo.VersionDescription.Substring(0, 1024) + "..."
                                              : item.GeneralInfo.VersionDescription;
        testToolStripMenuItem.Tag = item;
        testToolStripMenuItem.Click += testToolStripMenuItem_Click;
        btn_install.DropDownItems.Add(testToolStripMenuItem);
      }
    }

    private void testToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menu = sender as ToolStripMenuItem;
      ExtensionListControl parent = Parent.Parent as ExtensionListControl;
      if (parent == null)
        return;
      parent.OnInstallExtension(this, menu.Tag as PackageClass);
    }

    private bool _selected;

    public bool Selected
    {
      get { return _selected; }
      set
      {
        if (_selected != value)
        {
          _selected = value;
          SelectControl();
        }
      }
    }

    private void SelectControl()
    {
      BackColor = _selected ? SystemColors.GradientInactiveCaption : Color.White;
      BorderStyle = _selected ? BorderStyle.FixedSingle : BorderStyle.FixedSingle;
      lbl_description.ForeColor = _selected ? Color.Blue : Color.Black;
      lbl_name.ForeColor = _selected ? Color.Blue : Color.Black;
      lbl_version.ForeColor = _selected ? Color.Blue : Color.Black;
      //AutoSize = _selected;
      //Height = _selected ? 123 : 90;
      Height = 20;
      timer1.Enabled = _selected;
      
      if (Parent == null)
        return;
      var parent = Parent.Parent as ExtensionListControl;
      if (Selected)
      {
        if (parent != null && Selected)
        {
          if (parent.SelectedItem != null)
          {
            parent.SelectedItem.Selected = false;
          }
        }
        if (parent != null) parent.SelectedItem = this;
      }
    }

    private void ExtensionControl_Click(object sender, EventArgs e)
    {
      this.Selected = true;
    }

    private void lbl_description_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void lbl_name_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void img_logo_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void btn_uninstall_Click(object sender, EventArgs e)
    {
      ExtensionListControl parent = Parent.Parent as ExtensionListControl;
      if (parent == null)
        return;
      parent.OnUninstallExtension(this);
    }

    private void lbl_version_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void img_update_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void btn_update_Click(object sender, EventArgs e)
    {
      ExtensionListControl parent = Parent.Parent as ExtensionListControl;
      if (parent == null)
        return;
      parent.OnUpdateExtension(this);
    }

    private void img_dep_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void btn_conf_Click(object sender, EventArgs e)
    {
      ExtensionListControl parent = Parent.Parent as ExtensionListControl;
      if (parent == null)
        return;
      parent.OnConfigureExtension(this);
    }

    private void btn_forum_Click(object sender, EventArgs e)
    {
      try
      {
        Process.Start(Package.GeneralInfo.ForumPage);
      }
      catch (Exception) {}
    }

    private void btn_home_Click(object sender, EventArgs e)
    {
      try
      {
        Process.Start(Package.GeneralInfo.HomePage);
      }
      catch (Exception) {}
    }

    public bool Filter(string str, string tag)
    {
      tag = tag.Trim();
      if (tag.ToUpper() == "ALL")
        tag = string.Empty;

      bool strResult = string.IsNullOrEmpty(str);
      bool tagResult = string.IsNullOrEmpty(tag);

      if (string.IsNullOrEmpty(str))
        strResult = true;
      if (Package.GeneralInfo.Name.ToUpper().Contains(str.ToUpper()))
        strResult = true;
      if (Package.GeneralInfo.ExtensionDescription.ToUpper().Contains(str.ToUpper()))
        strResult = true;
      if (Package.GeneralInfo.TagList.Tags.Contains(str.ToLower()))
        strResult = true;
      if(!string.IsNullOrEmpty(tag))
      {
        if (Package.GeneralInfo.TagList.Tags.Contains(tag.ToLower()))
          tagResult = true;
      }
      return strResult && tagResult;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (Height < 120)
        Height = Height + 3;
      else
      {
        timer1.Enabled = false;
      }
    }

    private void btn_screenshot_Click(object sender, EventArgs e)
    {
      ExtensionListControl parent = Parent.Parent as ExtensionListControl;
      if (parent == null)
        return;
      parent.OnShowScreenShot(this, Package);
    }

    private void btn_more_info_Click(object sender, EventArgs e)
    {

    }

    private void img_dep1_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }

    private void img_update1_Click(object sender, EventArgs e)
    {
      ExtensionControl_Click(null, null);
    }


  }
}