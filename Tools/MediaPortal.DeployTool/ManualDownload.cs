#region Copyright (C) 2005-2008 Team MediaPortal
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class ManualDownload : Form
  {
    private string _url;

    private void UpdateUI()
    {
      this.Text = Utils.GetBestTranslation("ManualDownload_Title");
      labelHeading.Text = Utils.GetBestTranslation("ManualDownload_labelHeading");
      linkURL.Text = labelTargetFile.Text = Utils.GetBestTranslation("ManualDownload_linkURL");
      linkDir.Text = Utils.GetBestTranslation("ManualDownload_linkDir");
      labelDesc.Text = Utils.GetBestTranslation("ManualDownload_labelDesc");
      buttonContinue.Text = Utils.GetBestTranslation("ManualDownload_buttonContinue");
    }

    public ManualDownload()
    {
      InitializeComponent();
      UpdateUI();
    }

    public DialogResult ShowDialog(string url, string targetFile, string targetDir)
    {
      _url = url;
      labelTargetFile.Text = targetFile;
      labelTargetDir.Text = targetDir;
      return base.ShowDialog();
    }

    private void linkURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(_url);
    }

    private void linkDir_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(labelTargetDir.Text);
    }

    private void buttonContinue_Click(object sender, EventArgs e)
    {
      if (!File.Exists(labelTargetDir.Text + "\\" + labelTargetFile.Text))
        Utils.ErrorDlg(Utils.GetBestTranslation("ManualDownload_errFileNotFound"));
      else
        DialogResult = DialogResult.OK;
    }
  }
}