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

namespace MediaPortal.DeployTool
{
  public partial class DeployTool : Form
  {
    private DeployDialog _currentDialog;
    private string _currentCulture="en-US";

    private void UpdateUI()
    {
      if (InstallationProperties.Instance.Get("SVNMode")=="true")
        this.Text = Localizer.Instance.GetString("MainWindow_AppName_SVN");
      else
        this.Text = Localizer.Instance.GetString("MainWindow_AppName");
      labelAppHeading.Text = Localizer.Instance.GetString("MainWindow_labelAppHeading");
      backButton.Text = Localizer.Instance.GetString("MainWindow_backButton");
      nextButton.Text = Localizer.Instance.GetString("MainWindow_nextButton");
    }

    public DeployTool()
    {
      InitializeComponent();
      if (!Directory.Exists(Application.StartupPath + "\\deploy"))
        Directory.CreateDirectory(Application.StartupPath + "\\deploy");
      Localizer.Instance.SwitchCulture("en-US");
      UpdateUI();
      InstallationProperties.Instance.Add("SVNMode", "false");
      
      //Set default folders
      InstallationProperties.Instance.Set("MPDir", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal");
      InstallationProperties.Instance.Set("TVServerDir", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Team MediaPortal\\MediaPortal TV Server");
      
      //Identify 64bit systems for new registry path
      if (IntPtr.Size == 8)
          InstallationProperties.Instance.Set("RegistryKeyAdd", "Wow6432Node\\");
      else
          InstallationProperties.Instance.Set("RegistryKeyAdd", "");

      //Identify OS. Supporting XP SP2 and newer, but XP 64bit
      Version OsVersion = Environment.OSVersion.Version;
      bool OsSupport = false;

        switch(OsVersion.Major)
        {
            case 4:                         // 4.x = Win95,98,ME and NT 
                OsSupport = false;
                break;
            case 5:
                if (OsVersion.Minor == 0)   // 5.0 = Windows2000
                    OsSupport = false;
                if (OsVersion.Minor == 1)   // 5.1 = WindowsXP
                {
                    if ( (int.Parse(Environment.OSVersion.ServicePack.Replace("Service Pack ", "")) < 2) | (IntPtr.Size == 8))
                        OsSupport = false;
                    else
                        OsSupport = true;
                }
                if (OsVersion.Major == 2)   // 5.2 = Windows2003
                    OsSupport = true;
                break;
            case 6:                         // 6.0 = Windows Vista, 2008
                OsSupport = true;
                break;
        }

      if (!OsSupport)
      {
          MessageBox.Show("Sorry your OS is not supported by current MediaPortal installer", Environment.OSVersion.VersionString, MessageBoxButtons.OK, MessageBoxIcon.Stop);
          Application.Exit();
      }
      
      string[] cmdArgs = Environment.GetCommandLineArgs();
      foreach (string arg in cmdArgs)
      {
        if (arg.ToLowerInvariant() == "svn")
        {
          InstallationProperties.Instance.Set("SVNMode", "true");
          break;
        }
      }
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
      if (nextButton.Text == Localizer.Instance.GetString("MainWindow_buttonClose"))
      {
        Close();
        return;
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
        nextButton.Text = Localizer.Instance.GetString("MainWindow_buttonClose");
      }
    }

    private void backButton_Click(object sender, EventArgs e)
    {
      bool isFirstDlg=false;
      _currentDialog = DialogFlowHandler.Instance.GetPreviousDlg(ref isFirstDlg);
      if (isFirstDlg)
        backButton.Visible = false;
      SwitchDialog(_currentDialog);
    }
  }
}