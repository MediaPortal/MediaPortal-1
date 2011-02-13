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
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class ManualDownloadFileMissing : Form
  {
    private string target_file;
    private string target_dir;

    private void UpdateUI()
    {
      Text = Localizer.GetBestTranslation("ManualDownload_Title");
      buttonBrowse.Text = Localizer.GetBestTranslation("MainWindow_browseButton");
    }

    public ManualDownloadFileMissing()
    {
      InitializeComponent();
      Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
      UpdateUI();
    }

    public DialogResult ShowDialog(string targetDir, string targetFile)
    {
      target_file = targetFile;
      target_dir = targetDir;
      labelHeading.Text = String.Format(Localizer.GetBestTranslation("ManualDownload_errFileNotFound"), target_file);
      ShowDialog();
      return DialogResult.OK;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      openFileDialog.FileName = target_file;
      openFileDialog.InitialDirectory = target_dir;
      openFileDialog.ValidateNames = true;
      DialogResult res = openFileDialog.ShowDialog();
      textBox1.Text = res == DialogResult.OK ? openFileDialog.FileName : string.Empty;
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      if (openFileDialog.FileName != null)
      {
        File.Move(openFileDialog.FileName, target_dir + "\\" + target_file);
        Close();
      }
    }
  }
}