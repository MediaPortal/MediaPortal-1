#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System.ComponentModel;
using System.Windows.Forms;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  public class AppFilesImportProgress : AppSettings
  {
    private Label titleLabel;
    private TextBox progressText;
    private IContainer components = null;
    private AppItem curApp;
    private bool isImportRunning;

    const string importStartedText = "=== import started...";
    const string importFinishedText = "=== import finished.";

    public AppFilesImportProgress()
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.titleLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.progressText = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.SuspendLayout();
      // 
      // label3
      // 
      this.titleLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)))
        ;
      this.titleLabel.Location = new System.Drawing.Point(0, 0);
      this.titleLabel.Name = "titleLabel";
      this.titleLabel.Size = new System.Drawing.Size(248, 32);
      this.titleLabel.TabIndex = 1;
      this.titleLabel.Text = "Import running....";
      // 
      // ProgressText
      // 
      this.progressText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) |
        System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
      this.progressText.Location = new System.Drawing.Point(8, 32);
      this.progressText.Multiline = true;
      this.progressText.Name = "progressText";
      this.progressText.ReadOnly = true;
      this.progressText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.progressText.Size = new System.Drawing.Size(336, 368);
      this.progressText.TabIndex = 2;
      this.progressText.Text = "";
      // 
      // AppFilesImportProgress
      // 
      this.Controls.Add(this.progressText);
      this.Controls.Add(this.titleLabel);
      this.Name = "AppFilesImportProgress";
      this.Size = new System.Drawing.Size(360, 408);
      this.ResumeLayout(false);

    }

    #endregion

    public void RunImport()
    {
      if (CurApp != null)
      {
        progressText.Text = "";
        System.Windows.Forms.Application.DoEvents(); // make sure the title caption appears....
        CurApp.OnRefreshInfo += new AppItem.RefreshInfoEventHandler(RefreshInfo);
        try
        {
          progressText.Text = importStartedText;
          isImportRunning = true;
          CurApp.Refresh(false);
        }
        finally
        {
          CurApp.OnRefreshInfo -= new AppItem.RefreshInfoEventHandler(RefreshInfo);
          this.Text = "Import finished.";
          progressText.Text = progressText.Text + "\r\n" + importFinishedText;
          isImportRunning = false;
        }
      }
    }

    private void RefreshInfo(string Message)
    {
      progressText.Text = progressText.Text + "\r\n" + Message;
      progressText.SelectionStart = progressText.Text.Length;
      progressText.ScrollToCaret();
      System.Windows.Forms.Application.DoEvents();
    }

    public AppItem CurApp
    {
      get
      {
        return curApp;
      }
      set
      {
        curApp = value;
      }
    }

    public bool ImportRunning
    {
      get
      {
        return isImportRunning;
      }
      set
      {
        isImportRunning = value;
      }
    }

  }
}
