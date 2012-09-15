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
using System.Windows.Forms;
using System.Collections.Generic;
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

    public Dictionary<string, int> TagList;

    public ExtensionListControl()
    {
      InitializeComponent();
      SelectedItem = null;
      TagList = new Dictionary<string, int>();
    }

    public ExtensionControlHost SelectedItem { get; set; }

    public void Set(ExtensionCollection collection, bool isListOfInstalledExtensions)
    {
      var oldCursor = ParentForm.Cursor;
      try
      {
        ParentForm.Cursor = Cursors.WaitCursor;
        flowLayoutPanel1.SuspendLayout();
        collection.Sort(false);
        comboBox1.Items.Clear();
        comboBox1.Items.Add("All");
        TagList.Clear();
        toolTip1.RemoveAll(); // removes all created user-objects for the tooltip - reuse this tooltip instance, otherwise memory leak!
        foreach (Control c in flowLayoutPanel1.Controls) c.Dispose();
        flowLayoutPanel1.Controls.Clear();
        foreach (PackageClass item in collection.Items)
        {
          var extHostCtrl = new ExtensionControlHost();
          flowLayoutPanel1.Controls.Add(extHostCtrl);
          extHostCtrl.Initialize(item, isListOfInstalledExtensions);
          AddTags(item.GeneralInfo.TagList);
        }
        comboBox1.Text = "All";
        textBox1.Text = string.Empty;
        foreach (KeyValuePair<string, int> tagList in TagList)
        {
          if (tagList.Value > 1)
            comboBox1.Items.Add(tagList.Key);
        }
        flowLayoutPanel1.ResumeLayout();
        flowLayoutPanel1_SizeChanged(this, EventArgs.Empty);
      }
      finally
      {
        ParentForm.Cursor = oldCursor;
      }
    }

    private void AddTags(TagCollection tags)
    {
      foreach (var tag in tags.Tags)
      {
        if (!TagList.ContainsKey(tag))
          TagList.Add(tag, 1);
        else
          TagList[tag]++;
      }
    }

    public void OnUninstallExtension(ExtensionControlExpanded control)
    {
      if (UnInstallExtension != null)
        UnInstallExtension(control, control.Package);
    }

    public void OnUpdateExtension(ExtensionControlExpanded control)
    {
      if (UpdateExtension != null)
        UpdateExtension(control, control.Package, control.UpdatePackage);
    }

    public void OnConfigureExtension(ExtensionControlExpanded control)
    {
      if (ConfigureExtension != null)
        ConfigureExtension(control, control.Package);
    }

    public void OnInstallExtension(ExtensionControlExpanded control, PackageClass pak)
    {
      if (InstallExtension != null)
        InstallExtension(control, pak);
    }

    public void OnShowScreenShot(ExtensionControlExpanded control, PackageClass pak)
    {
      if (ShowScreenShot != null)
        ShowScreenShot(control, pak);
    }

    public void Filter(string filter, string tag)
    {
      flowLayoutPanel1.SuspendLayout();
      foreach (var control in flowLayoutPanel1.Controls)
      {
        var cnt = control as ExtensionControlHost;
        if (cnt != null)
        {
          cnt.Visible = cnt.Filter(filter, tag);
        }
      }
      flowLayoutPanel1.ResumeLayout();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      Filter(textBox1.Text, comboBox1.Text);
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      Filter(textBox1.Text, comboBox1.Text);
    }

    private void flowLayoutPanel1_SizeChanged(object sender, EventArgs e)
    {
      // when the panel resizes, resize all children to fill the available width
      // use the ClientSize with an extra 4 pixel 
      // to make sure no horizontal scrollbar will show even when a vertical scrollbar is visible
      if (flowLayoutPanel1.Controls.Count > 0 && flowLayoutPanel1.Controls[0].Width != flowLayoutPanel1.ClientSize.Width - 4)
      {
        foreach (Control control in flowLayoutPanel1.Controls)
        {
          control.Width = flowLayoutPanel1.ClientSize.Width - 4;
        }
      }
      
      /*if (flowLayoutPanel1.Controls.Count > 1 && flowLayoutPanel1.Size.Width > flowLayoutPanel1.Controls[0].Width + 30)
        flowLayoutPanel1.WrapContents = true;
      else
        flowLayoutPanel1.WrapContents = false;*/
    }

    private void flowLayoutPanel1_MouseEnter(object sender, EventArgs e)
    {
      if (!flowLayoutPanel1.Focused) flowLayoutPanel1.Focus();
    }
  }
}