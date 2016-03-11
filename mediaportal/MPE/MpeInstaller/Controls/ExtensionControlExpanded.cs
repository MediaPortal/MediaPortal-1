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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeInstaller.Dialogs;
using System.Drawing.Imaging;

namespace MpeInstaller.Controls
{
  public partial class ExtensionControlExpanded : UserControl
  {
    public PackageClass Package;
    public PackageClass UpdatePackage;
    public bool IsInitialized { get; protected set; }

    public ExtensionControlExpanded()
    {
      InitializeComponent();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
      base.OnVisibleChanged(e);
      if (Package != null)
        toolStrip1.Visible = Visible; // explicitly make the toolstrip visible, otherwise memory leak!
    }

    public void Initialize(bool isInstalled, bool meetsAllDependencies, PackageClass package, PackageClass updatePackage)
    {
      Package = package;
      UpdatePackage = updatePackage;

      lbl_name.Text = package.GeneralInfo.Name;
      lblAuthors.Text = string.Format("[{0}]", package.GeneralInfo.Author);
      lbl_version.Text = package.GeneralInfo.Version.ToString();
      img_dep.Visible = meetsAllDependencies;
      if (meetsAllDependencies)
      {
        (Parent.Parent.Parent as ExtensionListControl).toolTip1.SetToolTip(img_dep,
          "Some dependencies are not met.\r\nThe extension may not work properly.\r\nClick here for more information.");
      }
      lbl_description.Text = package.GeneralInfo.ExtensionDescription;
      btn_screenshot.Visible = !string.IsNullOrEmpty(package.GeneralInfo.Params[ParamNamesConst.ONLINE_SCREENSHOT].Value);      
      btn_conf.Visible = isInstalled && !string.IsNullOrEmpty(Package.GeneralInfo.Params[ParamNamesConst.CONFIG].GetValueAsPath());
      btn_uninstall.Visible = isInstalled;
      if (string.IsNullOrEmpty(Package.GeneralInfo.HomePage))
      {
        btn_home.Visible = false;
      }
      else
      {
        btn_home.Visible = true;
        (Parent.Parent.Parent as ExtensionListControl).toolTip1.SetToolTip(btn_home, "Extension web page");
      }
      if (string.IsNullOrEmpty(Package.GeneralInfo.ForumPage))
      {
        btn_forum.Visible = false;
      }
      else
      {
        btn_forum.Visible = true;
        (Parent.Parent.Parent as ExtensionListControl).toolTip1.SetToolTip(btn_forum, "Extension forum page");
      }
      PopulateInstallBtn();

      if (isInstalled && updatePackage != null)
      {
        btn_update.Visible = true;
        img_update.Visible = true;
        (Parent.Parent.Parent as ExtensionListControl).toolTip1.SetToolTip(img_update, 
          string.Format("New update available. Version: {0}", updatePackage.GeneralInfo.Version.ToString()));
      }
      else
      {
        btn_update.Visible = false;
        img_update.Visible = false;
      }

      if (Package.Parent != null)
      {
        chk_ignore.Checked = Package.Parent.IgnoredUpdates.Contains(Package.GeneralInfo.Id);
      }

      GetThumbnail();

      IsInitialized = true;
    }

    private void GetThumbnail()
    {
      if (Directory.Exists(Package.LocationFolder))
      {
        DirectoryInfo di = new DirectoryInfo(Package.LocationFolder);
        FileInfo[] fileInfos = di.GetFiles("icon.*");
        if (fileInfos.Length > 0)
        {
          img_logo.LoadAsync(fileInfos[0].FullName);
          return;
        }
      }
      if (!string.IsNullOrEmpty(Package.GeneralInfo.Params[ParamNamesConst.ONLINE_ICON].Value))
      {
        new Thread(DownloadThumbnail) { IsBackground = true, Name = "ThumbnailDownload" }.Start();
      }
    }

    private void DownloadThumbnail()
    {
        try
        {
          if (!Directory.Exists(Package.LocationFolder))
            Directory.CreateDirectory(Package.LocationFolder);

          string url = Package.GeneralInfo.Params[ParamNamesConst.ONLINE_ICON].Value;
          if (url.IndexOf("://") < 0) url = "http://" + url;
          var client = new CompressionWebClient();
          byte[] imgData = client.DownloadData(url);
          var ms = new MemoryStream(imgData);
          var image = Image.FromStream(ms);
          img_logo.Image = image;
          string fileName = "";
          if (ImageFormat.Jpeg.Equals(image.RawFormat))
          {
            fileName = "icon.jpg";
          }
          else if (ImageFormat.Png.Equals(image.RawFormat))
          {
            fileName = "icon.png";
          }
          else if (ImageFormat.Gif.Equals(image.RawFormat))
          {
            fileName = "icon.gif";
          }
          else if (ImageFormat.Icon.Equals(image.RawFormat))
          {
            fileName = "icon.ico";
          }
          else if (ImageFormat.Bmp.Equals(image.RawFormat))
          {
            fileName = "icon.bmp";
          }
          if (!string.IsNullOrEmpty(fileName))
          {
            File.WriteAllBytes(Path.Combine(Package.LocationFolder, fileName), imgData);
          }
        }
        catch (Exception) { } // invalid url or image file or no internet connection
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
        if (!item.CheckDependency(true))
          testToolStripMenuItem.ForeColor = Color.Red;
        if (item.GeneralInfo.VersionDescription != null)
          testToolStripMenuItem.ToolTipText = item.GeneralInfo.VersionDescription.Length > 1024
                                                ? item.GeneralInfo.VersionDescription.Substring(0, 1024) + "..."
                                                : item.GeneralInfo.VersionDescription;
        testToolStripMenuItem.Tag = item;
        testToolStripMenuItem.Click += testToolStripMenuItem_Click;
        btn_install.DropDownItems.Add(testToolStripMenuItem);
      }
    }

    private void img_dep_Click(object sender, EventArgs e)
    {
      DependencyForm depForm = new DependencyForm(this.Package);
      depForm.ShowDialog();
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

    private void chk_ignore_CheckedChanged(object sender, EventArgs e)
    {
      if (Package.Parent != null)
      {
        if (chk_ignore.Checked)
        {
          if (!Package.Parent.IgnoredUpdates.Contains(Package.GeneralInfo.Id))
            Package.Parent.IgnoredUpdates.Add(Package.GeneralInfo.Id);
        }
        else
        {
          if (Package.Parent.IgnoredUpdates.Contains(Package.GeneralInfo.Id))
            Package.Parent.IgnoredUpdates.Remove(Package.GeneralInfo.Id);
        }
      }
    }

    private void testToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem menu = sender as ToolStripMenuItem;
      var parent = Parent.Parent.Parent as ExtensionListControl;
      if (parent == null)
        return;
      parent.OnInstallExtension(this, menu.Tag as PackageClass);
    }

    private void btn_screenshot_Click(object sender, EventArgs e)
    {
      var parent = Parent.Parent.Parent as ExtensionListControl;
      if (parent != null)
        parent.OnShowScreenShot(this, Package);
    }

    private void btn_uninstall_Click(object sender, EventArgs e)
    {
      var parent = Parent.Parent.Parent as ExtensionListControl;
      if (parent != null)
        parent.OnUninstallExtension(this);
    }

    private void btn_update_Click(object sender, EventArgs e)
    {
      var parent = Parent.Parent.Parent as ExtensionListControl;
      if (parent != null)
        parent.OnUpdateExtension(this);
    }

    private void btn_conf_Click(object sender, EventArgs e)
    {
      var parent = Parent.Parent.Parent as ExtensionListControl;
      if (parent != null)
        parent.OnConfigureExtension(this);
    }

    private void btn_install_ButtonClick(object sender, EventArgs e)
    {
      if (btn_install.DropDownItems.Count > 0)
      {
        var parent = Parent.Parent.Parent as ExtensionListControl;
        if (parent == null)
          return;
        parent.OnInstallExtension(this, btn_install.DropDownItems[0].Tag as PackageClass);
      }
    }
  }
}