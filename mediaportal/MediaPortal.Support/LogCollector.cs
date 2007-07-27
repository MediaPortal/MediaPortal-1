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

namespace MediaPortal.Support
{
  public partial class LogCollector : Form
  {
    public enum DialogCategory
    {
      DEBUG_VIEW,
      EXCEPTION_VIEW
    }

    public LogCollector(DialogCategory category)
    {
      InitializeComponent();
      switch (category)
      {
        case DialogCategory.DEBUG_VIEW:
          labelHeading.Text = "MediaPortal has exsited.";
          break;
        case DialogCategory.EXCEPTION_VIEW:
          labelHeading.ForeColor = Color.Red;
          labelHeading.Text = "MediaPortal has crashed !!!";
          break;
      }
    }

    private void linkLabelQA_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://forum.team-mediaportal.com/quality_assurance-f74.html");
    }

    private void linkLabelHowTo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://forum.team-mediaportal.com/announcement.php?f=206&a=3");
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void btnCollect_Click(object sender, EventArgs e)
    {
      // Prepare the temporary directory where we collect the logs
      string mpdir = Path.GetDirectoryName(Application.ExecutablePath);
      string tmpLogPath=mpdir+"\\tmplog";
      if (!Directory.Exists(tmpLogPath))
        Directory.CreateDirectory(tmpLogPath);
      else
      {
        DirectoryInfo di=new DirectoryInfo(tmpLogPath);
        FileInfo[] fis=di.GetFiles();
        foreach (FileInfo fi in fis)
          fi.Delete();
      }
      ProgressDialog dlg = new ProgressDialog();
      dlg.Show();

      // Export the event log
      EventLogCsvLogger evtLogger = new EventLogCsvLogger("Application");
      dlg.SetCurrentAction(evtLogger.ActionMessage);
      evtLogger.CreateLogs(tmpLogPath);
      evtLogger = new EventLogCsvLogger("System");
      evtLogger.CreateLogs(tmpLogPath);
      dlg.UpdateProgress();

      // Copy MediaPortal logs
      MediaPortalLogs mplogger = new MediaPortalLogs(mpdir + "\\log");
      dlg.SetCurrentAction(mplogger.ActionMessage);
      mplogger.CreateLogs(tmpLogPath);
      dlg.UpdateProgress();

      // Collect hotfix infos
      HotFixInformationLogger hflogger = new HotFixInformationLogger();
      dlg.SetCurrentAction(hflogger.ActionMessage);
      hflogger.CreateLogs(tmpLogPath);
      dlg.UpdateProgress();

      // Collect TvServer logs if TvServer is installed
      TvServerLogger tvlogger = new TvServerLogger();
      dlg.SetCurrentAction(tvlogger.ActionMessage);
      tvlogger.CreateLogs(tmpLogPath);
      dlg.UpdateProgress();

      // Zip all files 
      dlg.SetCurrentAction("Zipping all gathered files...");
      Archiver arch = new Archiver(mpdir+ "\\mplogs.zip");
      arch.AddDirectory(tmpLogPath);
      arch.Dispose();
      dlg.Close();

      MessageBox.Show("All logs have been collected an compressed to the file [" + mpdir + "\\mplogs.zip]", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
      btnCollect.Visible = false;
    }
  }
}