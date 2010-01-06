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
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeMaker.Dialogs;

namespace MpeMaker.Sections
{
  public partial class GeneralSection : UserControl, ISectionControl
  {
    public PackageClass Package { get; set; }

    public GeneralSection()
    {
      InitializeComponent();
      Package = null;
    }

    private void GeneralSection_Load(object sender, EventArgs e) {}

    #region ISectionControl Members

    public void Set(PackageClass pak)
    {
      Package = null;
      txt_name.Text = pak.GeneralInfo.Name;
      txt_guid.Text = pak.GeneralInfo.Id;
      txt_version1.Text = pak.GeneralInfo.Version.Major;
      txt_version2.Text = pak.GeneralInfo.Version.Minor;
      txt_version3.Text = pak.GeneralInfo.Version.Build;
      txt_version4.Text = pak.GeneralInfo.Version.Revision;
      txt_author.Text = pak.GeneralInfo.Author;
      cmb_status.Text = pak.GeneralInfo.DevelopmentStatus;
      txt_homepage.Text = pak.GeneralInfo.HomePage;
      txt_forum.Text = pak.GeneralInfo.ForumPage;
      txt_update.Text = pak.GeneralInfo.UpdateUrl;
      txt_online.Text = pak.GeneralInfo.OnlineLocation;
      txt_description.Text = pak.GeneralInfo.ExtensionDescription;
      txt_versiondesc.Text = pak.GeneralInfo.VersionDescription;
      txt_tags.Text = pak.GeneralInfo.Tags;
      date_release.Value = pak.GeneralInfo.ReleaseDate;
      Package = pak;
      RefreshIcon();
    }

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    #endregion

    private void txt_name_TextChanged(object sender, EventArgs e)
    {
      if (Package != null)
      {
        Package.GeneralInfo.Name = txt_name.Text;
        Package.GeneralInfo.Id = txt_guid.Text;
        Package.GeneralInfo.Version.Major = txt_version1.Text;
        Package.GeneralInfo.Version.Minor = txt_version2.Text;
        Package.GeneralInfo.Version.Build = txt_version3.Text;
        Package.GeneralInfo.Version.Revision = txt_version4.Text;
        Package.GeneralInfo.Author = txt_author.Text;
        Package.GeneralInfo.DevelopmentStatus = cmb_status.Text;
        Package.GeneralInfo.HomePage = txt_homepage.Text;
        Package.GeneralInfo.ForumPage = txt_forum.Text;
        Package.GeneralInfo.UpdateUrl = txt_update.Text;
        Package.GeneralInfo.ExtensionDescription = txt_description.Text;
        Package.GeneralInfo.VersionDescription = txt_versiondesc.Text;
        Package.GeneralInfo.OnlineLocation = txt_online.Text;
        Package.GeneralInfo.Tags = txt_tags.Text;
        Package.GeneralInfo.ReleaseDate = date_release.Value;
      }
    }

    private void RefreshIcon()
    {
      if (File.Exists(Package.GeneralInfo.Params[ParamNamesConst.ICON].Value))
        img_logo.LoadAsync(Package.GeneralInfo.Params[ParamNamesConst.ICON].Value);
    }

    private void btn_gen_guid_Click(object sender, EventArgs e)
    {
      txt_guid.Text = Guid.NewGuid().ToString();
    }


    private void btn_params_Click(object sender, EventArgs e)
    {
      ParamEdit dlg = new ParamEdit();
      dlg.Set(Package.GeneralInfo.Params);
      dlg.ShowDialog();
      RefreshIcon();
    }

    private void txt_version1_KeyDown(object sender, KeyEventArgs e)
    {
      bool result = true;

      bool numericKeys = (
                           ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                            (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
                           && e.Modifiers != Keys.Shift);

      bool ctrlA = e.KeyCode == Keys.A && e.Modifiers == Keys.Control;

      bool editKeys = (
                        (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control) ||
                        (e.KeyCode == Keys.X && e.Modifiers == Keys.Control) ||
                        (e.KeyCode == Keys.C && e.Modifiers == Keys.Control) ||
                        (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) ||
                        e.KeyCode == Keys.Delete ||
                        e.KeyCode == Keys.Back);

      bool navigationKeys = (
                              e.KeyCode == Keys.Up ||
                              e.KeyCode == Keys.Right ||
                              e.KeyCode == Keys.Down ||
                              e.KeyCode == Keys.Left ||
                              e.KeyCode == Keys.Home ||
                              e.KeyCode == Keys.End);

      if (!(numericKeys || editKeys || navigationKeys))
      {
        result = false;
      }
      if (!result) // If not valid key then suppress and handle.
      {
        e.SuppressKeyPress = true;
        e.Handled = true;
      }
      else
        base.OnKeyDown(e);
    }
  }
}