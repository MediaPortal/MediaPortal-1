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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace MPTestTool
{
  public partial class MPTestTool : Form
  {
    #region Variables
    string _tempDir = Path.GetTempPath()+"\\MPTemp";
    string _zipFile = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+"\\MediaPortalLogs.zip";
    bool _autoMode = false;
    bool _crashed = false;
    #endregion

    #region Helper functions
    private void ShowUsage()
    {
      string usageText = "\n" +
        "Usage: MPTestTool.exe [-auto] [-crashed] [-zipFile <path+filename>] \n" +
        "\n" +
        "\tauto   : Perform all actions automatically and start MediaPortal in between\n" +
        "\tcrashed: Used internally by MediaPortal if it crashes\n" +
        "\tzipFile: full path and filename to the zip where all logfiles will be included\n" +
        "\n";
      MessageBox.Show(usageText,"MediaPortal test tool usage",MessageBoxButtons.OK,MessageBoxIcon.Information);
    }
    private void setStatus(string status)
    {
      this.statusBar.Text = string.Format("Status: {0}", status);
    }
    #endregion

    public MPTestTool()
    {
      InitializeComponent();
      tbZipFile.Text = _zipFile;
      if (!ParseCommandLine())
        Application.Exit();
      if (_autoMode)
      {
        if (!CheckRequirements())
          Application.Exit();
        preTestButton.Enabled = false;
        LaunchMPButton.Enabled = false;
        postTestButton.Enabled = false;
        setStatus("Running in auto/debug mode...");
        tmrUnAttended.Enabled = true;
      }
      if (_crashed)
      {
        CheckRequirements();
        preTestButton.Enabled = false;
        LaunchMPButton.Enabled = false;
        Utils.ErrorDlg("MediaPortal crashed unexpectedly.");
      }
    }

    #region Checks
    private bool ParseCommandLine()
    {
      string[] args = Environment.GetCommandLineArgs();
      for (int i = 1; i < args.Length; )
      {
        switch (args[i].ToLower())
        {
          case "-zipfile":
            _zipFile = args[++i];
            break;
          case "-auto":
            _autoMode = true;
            break;
          case "-crashed":
            _crashed = true;
            break;
          default:
            ShowUsage();
            return false;
        }
        i++;
      }
      return true;
    }
    public bool CheckRequirements()
    {
      Directory.CreateDirectory(_tempDir);
      if (!Directory.Exists(Path.GetDirectoryName(_zipFile)))
      {
        try
        {
          Directory.CreateDirectory(Path.GetDirectoryName(_zipFile));
        }
        catch (Exception)
        {
          Utils.ErrorDlg("You supplied an invalid path for the zip file.");
          return false;
        }
      }
      return true;
    }
    #endregion

    #region Form Events
    private void btnZipFile_Click(object sender, EventArgs e)
    {
      SaveFileDialog saveDialog = new SaveFileDialog();
      //Default settings
      saveDialog.AddExtension = true;
      saveDialog.OverwritePrompt = true;
      saveDialog.DefaultExt = ".zip";
      saveDialog.Title = "Choose ZIP file to create";

      saveDialog.FileName = tbZipFile.Text;
      DialogResult dr = saveDialog.ShowDialog();
      if (dr == DialogResult.OK)
      {
        tbZipFile.Text = saveDialog.FileName;
        _zipFile = tbZipFile.Text;
      }
    }
    private void menuItem2_Click(object sender, EventArgs e)
    {
      Close();
    }
    private void preTestButton_Click(object sender, EventArgs e)
    {
      PerformPreTestActions();
    }
    private void LaunchMPButton_Click(object sender, EventArgs e)
    {
      LaunchMediaPortalAction();
    }
    private void postTestButton_Click(object sender, EventArgs e)
    {
      PerformPostTestActions();
    }
    #endregion

    #region Perform actions
    void PerformPreTestActions()
    {
      setStatus("Busy performing pre-test actions...");
      PreTestActions pta = new PreTestActions();
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);

      if (pta.PerformActions())
      {
        setStatus("Done performing pre-test actions.");
      }
      else
      {
        setStatus("Pre-test actions were aborted.");
      }
      if (_autoMode)
      {
        pta.Close();
        pta = null;
      }
    }
    void LaunchMediaPortalAction()
    {
      setStatus("Launching MediaPortal...");
      // Set the loglevel to "debug"
      int lastLogLevel;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Application.StartupPath+"\\MediaPortal.xml",false))
      {
        lastLogLevel=xmlreader.GetValueAsInt("general", "loglevel", 1);
        xmlreader.SetValue("general", "loglevel", 3);
      }
      Process pr = new Process();
      pr.StartInfo.WorkingDirectory = Application.StartupPath;
      pr.StartInfo.FileName = "mediaportal.exe";
      pr.Start();
      setStatus("MediaPortal started. Waiting for exit...");
      Update();
      pr.WaitForExit();
      // Reset the loglevel to "debug"
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Application.StartupPath + "\\MediaPortal.xml",false))
      {
        xmlreader.SetValue("general", "loglevel", lastLogLevel);
      }
      setStatus("idle");
    }
    void PerformPostTestActions()
    {
      setStatus("Busy performing post-test actions...");
      PostTestActions pta = new PostTestActions(_tempDir,_zipFile);
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);
      
      if (pta.PerformActions())
      {
        setStatus("Done performing post-test actions.");
      }
      else
      {
        setStatus("Post-test actions were aborted.");
      }
      if (_autoMode)
      {
        pta.Close();
        pta = null;
      }
    }
    #endregion

    private void tmrUnAttended_Tick(object sender, EventArgs e)
    {
      tmrUnAttended.Enabled = false;
      PerformPreTestActions();
      LaunchMediaPortalAction();
      PerformPostTestActions();
      preTestButton.Enabled = true;
      LaunchMPButton.Enabled = true;
      postTestButton.Enabled = true;
    }

    private void menuItem7_Click(object sender, EventArgs e)
    {
      AboutForm dlg = new AboutForm();
      dlg.ShowDialog();
    }
  }
}