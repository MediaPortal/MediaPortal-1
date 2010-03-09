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
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeInstaller.Controls
{
  public partial class ExtensionListControl : UserControl
  {
    public event UnInstallExtensionHandler UnInstallExtension;

    public delegate void UnInstallExtensionHandler(object sender, PackageClass packageClass);

    public event UpdateExtensionHandler UpdateExtension;

    public delegate void UpdateExtensionHandler(
      object sender, PackageClass packageClass, PackageClass newpackageClass);

    public event ConfigureExtensionHandler ConfigureExtension;

    public delegate void ConfigureExtensionHandler(object sender, PackageClass packageClass);

    public event InstallExtensionHandler InstallExtension;

    public delegate void InstallExtensionHandler(object sender, PackageClass packageClass);

    public event ShowScreenShotHandler ShowScreenShot;

    public delegate void ShowScreenShotHandler(object sender, PackageClass packageClass);

    public ExtensionListControl()
    {
      InitializeComponent();
      SelectedItem = null;
      flowLayoutPanel1.VerticalScroll.Visible = true;
    }

    public ExtensionControl SelectedItem { get; set; }

    public void Set(ExtensionCollection collection)
    {
      comboBox1.Items.Clear();
      comboBox1.Items.Add("All");
      flowLayoutPanel1.Controls.Clear();
      foreach (PackageClass item in collection.Items)
      {
        flowLayoutPanel1.Controls.Add(new ExtensionControl(item));
        AddTags(item.GeneralInfo.TagList);
      }
      comboBox1.Text = "All";
      textBox1.Text = string.Empty;
    }

    private void AddTags(TagCollection tags)
    {
      foreach (var tag in tags.Tags)
      {
        if (!comboBox1.Items.Contains(tag))
          comboBox1.Items.Add(tag);
      }
    }

    private void flowLayoutPanel1_Click(object sender, EventArgs e) {}

    public void OnUninstallExtension(ExtensionControl control)
    {
      if (UnInstallExtension != null)
        UnInstallExtension(control, control.Package);
    }


    public void OnUpdateExtension(ExtensionControl control)
    {
      if (UpdateExtension != null)
        UpdateExtension(control, control.Package, control.UpdatePackage);
    }

    public void OnConfigureExtension(ExtensionControl control)
    {
      if (ConfigureExtension != null)
        ConfigureExtension(control, control.Package);
    }

    public void OnInstallExtension(ExtensionControl control, PackageClass pak)
    {
      if (InstallExtension != null)
        InstallExtension(control, pak);
    }

    public void OnShowScreenShot(ExtensionControl control, PackageClass pak)
    {
      if (ShowScreenShot != null)
        ShowScreenShot(control, pak);
    }

    public void Filter(string filter, string tag)
    {
      foreach (var control in flowLayoutPanel1.Controls)
      {
        var cnt = control as ExtensionControl;
        if (cnt != null)
        {
          cnt.Visible = cnt.Filter(filter, tag);
        }
      }
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      Filter(textBox1.Text, comboBox1.Text);
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      Filter(textBox1.Text, comboBox1.Text);
    }

    private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }

  }
}