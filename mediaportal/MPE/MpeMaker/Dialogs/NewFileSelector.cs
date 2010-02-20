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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
  public enum MpeStartupResult
  {
    NewFile,
    OpenFile,
    SkinWizard,
    MruFile
  }

  public partial class NewFileSelector : Form
  {
    #region Fields

    public MpeStartupResult MpeStartupResult;
    public string MpeStartupResultParam;

    #region ImageKeys for ImageList

    private const string ImageKeyNew = "new";
    private const string ImageKeyOpen = "open";
    private const string ImageKeySkinWizard = "skin_wizard";
    private const string ImageKeyMruFile = "mru";

    #endregion

    #endregion

    #region Constructors

    public NewFileSelector()
    {
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

      imageList.Images.Add(ImageKeyNew, Properties.Resources.document_new);
      imageList.Images.Add(ImageKeyOpen, Properties.Resources.document_open);
      imageList.Images.Add(ImageKeySkinWizard, Properties.Resources.applications_graphics);

      ListViewGroup wizardGroup = new ListViewGroup("Wizards", HorizontalAlignment.Left);
      listView.Groups.Add(wizardGroup);

      listView.Items.Add(new ListViewItem("New Project", ImageKeyNew) { Tag = MpeStartupResult.NewFile });
      listView.Items.Add(new ListViewItem("Open project", ImageKeyOpen) { Tag = MpeStartupResult.OpenFile });
      listView.Items.Add(new ListViewItem("New Skin Project Wizard", ImageKeySkinWizard, wizardGroup) { Tag = MpeStartupResult.SkinWizard });

      listView.Items[0].Selected = true;
    }

    public NewFileSelector(ICollection<string> mruFiles)
      : this()
    {
      if (mruFiles.Count == 0) return;

      imageList.Images.Add(ImageKeyMruFile, Icon.ExtractAssociatedIcon(Application.ExecutablePath));

      ListViewGroup mruGroup = new ListViewGroup("Most Recently Used", HorizontalAlignment.Left);
      listView.Groups.Add(mruGroup);

      foreach (string filepath in mruFiles)
      {
        if (!File.Exists(filepath)) continue;
        string filename = Path.GetFileNameWithoutExtension(filepath);

        var item = new ListViewItem(filename, ImageKeyMruFile, mruGroup);
        item.Tag = MpeStartupResult.MruFile;
        item.ToolTipText = filepath;

        listView.Items.Add(item);
      }
    }

    #endregion

    private void btn_ok_Click(object sender, EventArgs e)
    {
      Hide();

      MpeStartupResult = (MpeStartupResult) listView.SelectedItems[0].Tag;
      if (MpeStartupResult == MpeStartupResult.MruFile)
      {
        MpeStartupResultParam = listView.SelectedItems[0].ToolTipText;
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      btn_ok.Enabled = listView.SelectedItems.Count > 0;
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView.SelectedItems.Count > 0)
        btn_ok_Click(sender, null);
    }
  }
}