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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace MediaPortal.DeployTool
{
  public partial class DeployTool : Form, IDeployDialog
  {
    private DeployDialog _currentDialog;
    public string _currentCulture = "en-US";

    #region IDeployDialog interface
    public void UpdateUI()
    {
      this.Text = Utils.GetBestTranslation("MainWindow_AppName");
      //labelAppHeading.Text = Utils.GetBestTranslation("MainWindow_labelAppHeading");
      backButton.Text = Utils.GetBestTranslation("MainWindow_backButton");
      nextButton.Text = Utils.GetBestTranslation("MainWindow_nextButton");
    }
    public DeployDialog GetNextDialog()
    {
      return null;
    }
    public bool SettingsValid()
    {
      return true;
    }
    public void SetProperties()
    {
      return;
    }
    #endregion

    public DeployTool()
    {
      //Check if x86 or x64 architecture
      if (Utils.Check64bit())
      {
        InstallationProperties.Instance.Set("RegistryKeyAdd", "Wow6432Node\\");
        InstallationProperties.Instance.Set("Sql2005Download", "64");
      }
      else
      {
        InstallationProperties.Instance.Set("RegistryKeyAdd", "");
        InstallationProperties.Instance.Set("Sql2005Download", "32");
      }

      //Create necessary directory tree
      if (!Directory.Exists(Application.StartupPath + "\\deploy"))
        Directory.CreateDirectory(Application.StartupPath + "\\deploy");

      //Set default folders
      InstallationProperties.Instance.Set("MPDir", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal");
      InstallationProperties.Instance.Set("TVServerDir", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal TV Server");

      // Paint first screen
      InitializeComponent();
      Localizer.Instance.SwitchCulture("en-US");
      UpdateUI();

      _currentDialog = DialogFlowHandler.Instance.GetDialogInstance(DialogType.Welcome);
      splitContainer2.Panel1.Controls.Add(_currentDialog);
      InstallationProperties.Instance.Add("InstallTypeHeader", "Choose installation type");
      backButton.Visible = false;
      UpdateUI();
    }

    private void SwitchDialog(DeployDialog dlg)
    {
      splitContainer2.Panel1.Controls.Clear();
      splitContainer2.Panel1.Controls.Add(dlg);
    }

    private void nextButton_Click(object sender, EventArgs e)
    {
      if (nextButton.Text == Utils.GetBestTranslation("MainWindow_buttonClose"))
      {
        //
        // If in download_only mode, start explorer to show downloaded stuff
        //
        if (InstallationProperties.Instance["InstallType"] == "download_only")
        {
          Process process = new Process();
          process.StartInfo.FileName = "explorer.exe";
          process.StartInfo.Arguments = "/e, " + Application.StartupPath;
          process.StartInfo.UseShellExecute = true;
          process.Start();
        }
        Close();
        return;
      }
      if (nextButton.Text == Utils.GetBestTranslation("Install_buttonDownload") ||
          nextButton.Text == Utils.GetBestTranslation("Install_buttonInstall"))
      {
        nextButton.Enabled = false;
      }
      if (!_currentDialog.SettingsValid())
        return;
      _currentDialog.SetProperties();
      if (InstallationProperties.Instance["language"] != _currentCulture)
      {
        _currentCulture = InstallationProperties.Instance["language"];
        Localizer.Instance.SwitchCulture(_currentCulture);
        UpdateUI();
      }
      _currentDialog = _currentDialog.GetNextDialog();
      SwitchDialog(_currentDialog);
      if (!backButton.Visible)
        backButton.Visible = true;
      if (InstallationProperties.Instance["finished"] == "yes")
      {
        backButton.Visible = false;
        nextButton.Enabled = true;
        nextButton.Text = Utils.GetBestTranslation("MainWindow_buttonClose");
      }
      if (InstallationProperties.Instance["Install_Dialog"] == "yes")
      {
        if (InstallationProperties.Instance["InstallType"] == "download_only")
          nextButton.Text = Utils.GetBestTranslation("Install_buttonDownload");
        else
          nextButton.Text = Utils.GetBestTranslation("Install_buttonInstall");
        InstallationProperties.Instance.Set("Install_Dialog", "no");
      }
    }

    private void backButton_Click(object sender, EventArgs e)
    {
      bool isFirstDlg = false;
      _currentDialog = DialogFlowHandler.Instance.GetPreviousDlg(ref isFirstDlg);
      if (isFirstDlg)
        backButton.Visible = false;
      nextButton.Text = Utils.GetBestTranslation("MainWindow_nextButton");
      SwitchDialog(_currentDialog);
    }
  }
}