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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;

namespace MpeMaker.Dialogs
{
  public partial class InstalledExtensionsSelector : Form
  {
    public InstalledExtensionsSelector()
    {
      InitializeComponent();
      Result = null;
    }

    public PackageClass Result { get; set; }

    private void InstalledExtensionsSelector_Load(object sender, EventArgs e)
    {
      listView1.Items.Clear();
      foreach (var extension in MpeInstaller.InstalledExtensions.Items)
      {
        var item = new ListViewItem(extension.GeneralInfo.Id) {Tag = extension};
        item.SubItems.Add(extension.GeneralInfo.Name);
        item.SubItems.Add(extension.GeneralInfo.Version.ToString());
        listView1.Items.Add(item);
      }
    }

    private void listView1_DoubleClick(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        DialogResult = DialogResult.OK;
        Result = listView1.SelectedItems[0].Tag as PackageClass;
        Close();
      }
    }
  }
}