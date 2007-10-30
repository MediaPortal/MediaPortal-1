#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
      Localizer.Instance.SwitchCulture("en-US");
      UpdateUI();
      InstallationProperties.Instance.Add("SVNMode", "false");
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