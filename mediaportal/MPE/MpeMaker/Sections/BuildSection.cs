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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;

namespace MpeMaker.Sections
{
  public partial class BuildSection : UserControl, ISectionControl
  {
    public PackageClass Package { get; set; }
    private bool _loading = false;

    public BuildSection()
    {
      InitializeComponent();
    }

    #region ISectionControl Members

    public void Set(PackageClass pak)
    {
      _loading = true;
      Package = pak;
      txt_outfile.Text = pak.GeneralInfo.Location;
      lbl_file.Text = Package.ReplaceInfo(txt_outfile.Text);
      _loading = false;

      txt_outfile_TextChanged(null, null);
    }

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    #endregion

    private void btn_browse_Click(object sender, EventArgs e)
    {
      saveFileDialog1.Filter = "Mpe package file(*.mpe1)|*.mpe1|All files|*.*";
      if (saveFileDialog1.ShowDialog() == DialogResult.OK)
      {
        txt_outfile.Text = saveFileDialog1.FileName;
      }
    }

    public void btn_generate_Click(object sender, EventArgs e)
    {
      GenPak(false);
    }

    /// <summary>
    /// Handles the TextChanged event of the txt_outfile control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void txt_outfile_TextChanged(object sender, EventArgs e)
    {
      lbl_file.Text = Package.ReplaceInfo(txt_outfile.Text);
      if (_loading) return;

      Package.GeneralInfo.Location = txt_outfile.Text;

      btn_generate.Enabled = !String.IsNullOrEmpty(txt_outfile.Text);
      btn_test.Enabled = !String.IsNullOrEmpty(txt_outfile.Text);
    }


    private void btn_test_Click(object sender, EventArgs e)
    {
      GenPak(true);
    }

    private void GenPak(bool run)
    {
      list_error.Items.Clear();
      list_message.Items.Clear();
      string fileName = txt_outfile.Text;
      if (string.IsNullOrEmpty(fileName))
      {
        list_error.Items.Add("Error: No output file is specified!");
      }
      else
      {
        foreach (FolderGroup folderGroup in Package.ProjectSettings.FolderGroups)
        {
          ProjectSettings.UpdateFiles(Package, folderGroup);
        }

        if (!Path.IsPathRooted(fileName))
          fileName = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Package.ProjectSettings.ProjectFilename), fileName));

        foreach (string s in Package.ValidatePackage())
        {
          list_error.Items.Add(s);
        }

        list_message.Items.Add("Creating package started at : " + DateTime.Now.ToLongTimeString());
        if (string.IsNullOrEmpty(Package.GeneralInfo.Params[ParamNamesConst.CONFIG].Value) && Package.ProvidesPlugins())
        {
          list_message.Items.Add("Notice: Package provides a plugin, but no configuration file set under General Information Parameters!");
        }
        if (string.IsNullOrEmpty(Package.GeneralInfo.Params[ParamNamesConst.ONLINE_SCREENSHOT].Value))
        {
          list_message.Items.Add("Notice: No screenshot URL set under General Information Parameters!");
        }
        list_message.Refresh();
        Refresh();
        fileName = Package.ReplaceInfo(fileName);
        MpeInstaller.ZipProvider.Save(Package, fileName);
        list_message.Items.Add("Ended at : " + DateTime.Now.ToLongTimeString());
      }
      if (list_error.Items.Count > 0)
      {
        tabControl1.SelectTab(1);
        return;
      }
      else
      {
        tabControl1.SelectTab(0);
      }

      if (run && File.Exists(fileName))
        Process.Start(fileName);
    }
  }
}