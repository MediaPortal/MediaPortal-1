#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Threading;
using System.Windows.Forms;

namespace PostSetup
{
  /// <summary>
  /// Summary description for MPRecommendations.
  /// This is part of the MediaPortal setup.
  /// It is run after MP is installed, but are included in the setup.msi.
  /// This postsetup can be run without setup.msi, all it needs is one argument telling this app where MP is installed.
  /// 
  /// ex. PostSetup.exe "C:\Program Files\Team MediaPortal\Team MediaPortal"
  /// </summary>
  public class MPRecommendations : Form
  {
    private Panel panel2;
    private PictureBox pictureBox1;
    private Label labDescription;
    private Label labInfo;
    private PictureBox pictureBox2;
    private Button btnAction;
    private Panel MP3PartPanel;
    //private IContainer components;
    private string mpTargetDir;
    private int currentPackageIndex = 0;

    public MPRecommendations(string[] args)
    {
      Thread.CurrentThread.Name = "MPRecommendations";
      // READ THIS!!
      // You need to do some changes in Setup project for MediaPortal.
      // Click on the setup project solution and click Custom Actions Editor (at the toolbar above) 
      // Right click on Install and select Add Custom Action.
      // Dubbelclick on Application Folder and click on Output, then you get this under Install: "Primary output from PostSetup (Active)",			
      // then click properties on that one.. set Arguments to: "[TARGETDIR]" and InstallerClass=false (dont forget the " around [TARGETDIR])
      // if my guide sux.. read this: http://www.c-sharpcorner.com/Code/2003/April/SetupProjects.asp

      InitializeComponent();
      if (args.Length == 0)
      {
        MessageBox.Show(
          "This post setup needs one argument to start.\nThe argument must be the directory where MediaPortal is installed.",
          "Argument missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Exit();
        ButtonAction_Changed(MP3PartInstaller.BUTTONACTION_CLOSE);
        return;
      }
      mpTargetDir = @args[0]; // this MUST been set in the Setup project.. "[TARGETDIR]"

      // text in app.
      this.Text = this.Text;
      this.labDescription.Text =
        "MediaPortal can use some third-party applications to enhance the user experience.\nThese packages are recommended, but are not required for MediaPortal to function.\nYou will need an active internet connection to install these.";
      this.labInfo.Text = ""; // text at bottom of the dialog..not in use!

      // CHANGE HERE TO GET MORE 3 PARTY SOFTWARE IN THE LIST ( 3st in list at this time )									
      MP3PartPanel.Controls.Clear();
      MP3PartInstaller mp3PartInstaller = new MP3PartInstaller();
      mp3PartInstaller = GetNext3PartPackage();
      MP3PartPanel.Controls.Add(mp3PartInstaller);
    }

    private void ButtonAction_Changed(int buttonAction)
    {
      switch (buttonAction)
      {
        case MP3PartInstaller.BUTTONACTION_CANCEL:
          btnAction.Text = "&Cancel";
          break;
        case MP3PartInstaller.BUTTONACTION_CLOSE:
          btnAction.Text = "&Close";
          break;
        case MP3PartInstaller.BUTTONACTION_INSTALL:
          btnAction.Text = "&Install";
          break;
        case MP3PartInstaller.BUTTONACTION_NEXT:
          btnAction.Text = "&Next";
          break;
        case MP3PartInstaller.BUTTONACTION_INSTALLED:
          btnAction.Text = "&Next";
          break;
        case MP3PartInstaller.BUTTONACTION_DONTINSTALL:
          btnAction.Text = "&Next";
          break;
      }
      btnAction.Tag = buttonAction;
    }

    private MP3PartInstaller GetNext3PartPackage()
    {
      // CHANGE HERE TO GET MORE 3 PARTY SOFTWARE IN THE LIST
      // TODO: Should this be in a config file maybe?!
      MP3PartInstaller mp3PartInstaller = null;
      switch (currentPackageIndex)
      {
        case 0:
          mp3PartInstaller = new MP3PartXMLTV();
          mp3PartInstaller.Init(this.mpTargetDir);
          mp3PartInstaller.ButtonAction_Changed += new MP3PartInstaller.ButtonActionHandler(ButtonAction_Changed);
          break;
        case 1:
          mp3PartInstaller = new MP3PartFFDShow();
          mp3PartInstaller.Init(this.mpTargetDir);
          mp3PartInstaller.ButtonAction_Changed += new MP3PartInstaller.ButtonActionHandler(ButtonAction_Changed);
          break;
/* not to be included at this moment.
				case 2:
					mp3PartInstaller = new MP3PartVobSub();
					mp3PartInstaller.Init(this.mpTargetDir);
					mp3PartInstaller.ButtonAction_Changed += new MP3PartInstaller.ButtonActionHandler(ButtonAction_Changed);
					break;
*/
      }

      return mp3PartInstaller;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof (MPRecommendations));
      this.btnAction = new MediaPortal.UserInterface.Controls.MPButton();
      this.MP3PartPanel = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.labDescription = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labInfo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnAction
      // 
      this.btnAction.Location = new System.Drawing.Point(432, 392);
      this.btnAction.Name = "btnAction";
      this.btnAction.Size = new System.Drawing.Size(88, 24);
      this.btnAction.TabIndex = 4;
      this.btnAction.Tag = "0";
      this.btnAction.Text = "&Next";
      this.btnAction.Click += new System.EventHandler(this.btnAction_Click);
      // 
      // MP3PartPanel
      // 
      this.MP3PartPanel.AutoScroll = true;
      this.MP3PartPanel.BackColor = System.Drawing.SystemColors.Control;
      this.MP3PartPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.MP3PartPanel.Location = new System.Drawing.Point(8, 128);
      this.MP3PartPanel.Name = "MP3PartPanel";
      this.MP3PartPanel.Size = new System.Drawing.Size(512, 256);
      this.MP3PartPanel.TabIndex = 5;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.SystemColors.Window;
      this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel2.Controls.Add(this.pictureBox2);
      this.panel2.Controls.Add(this.pictureBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(528, 72);
      this.panel2.TabIndex = 6;
      // 
      // pictureBox2
      // 
      this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
      this.pictureBox2.Location = new System.Drawing.Point(16, 16);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(264, 40);
      this.pictureBox2.TabIndex = 5;
      this.pictureBox2.TabStop = false;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(440, 0);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(88, 64);
      this.pictureBox1.TabIndex = 4;
      this.pictureBox1.TabStop = false;
      // 
      // labDescription
      // 
      this.labDescription.Location = new System.Drawing.Point(8, 80);
      this.labDescription.Name = "labDescription";
      this.labDescription.Size = new System.Drawing.Size(512, 40);
      this.labDescription.TabIndex = 7;
      this.labDescription.Text = "labDescription";
      // 
      // labInfo
      // 
      this.labInfo.Location = new System.Drawing.Point(16, 392);
      this.labInfo.Name = "labInfo";
      this.labInfo.Size = new System.Drawing.Size(400, 24);
      this.labInfo.TabIndex = 8;
      this.labInfo.Text = "labInfo";
      // 
      // MPRecommendations
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(528, 423);
      this.Controls.Add(this.labInfo);
      this.Controls.Add(this.labDescription);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.MP3PartPanel);
      this.Controls.Add(this.btnAction);
      this.MaximizeBox = false;
      this.Name = "MPRecommendations";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MediaPortal Recommendations";
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Application.Run(new MPRecommendations(args));
    }

    /// <summary>
    /// Next/Close/Cancel button..
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnAction_Click(object sender, EventArgs e)
    {
      int buttonAction = int.Parse(btnAction.Tag.ToString());
      if (buttonAction == MP3PartInstaller.BUTTONACTION_NEXT || buttonAction == MP3PartInstaller.BUTTONACTION_INSTALLED ||
          buttonAction == MP3PartInstaller.BUTTONACTION_DONTINSTALL)
      {
        currentPackageIndex++;
        MP3PartInstaller mp3PartInstaller = GetNext3PartPackage();
        if (mp3PartInstaller != null)
        {
          MP3PartPanel.Controls.Clear();
          MP3PartPanel.Controls.Add(mp3PartInstaller);
        }
        else
        {
          // No more packages.
          // Application.Exit();
          Application.ExitThread();
        }
      }
      else if (buttonAction == MP3PartInstaller.BUTTONACTION_INSTALL)
      {
        MP3PartInstaller mp3PartInstaller = (MP3PartInstaller)MP3PartPanel.Controls[0];
        mp3PartInstaller.Install();
      }
      else if (buttonAction == MP3PartInstaller.BUTTONACTION_CANCEL)
      {
        MP3PartInstaller mp3PartInstaller = (MP3PartInstaller)MP3PartPanel.Controls[0];
        mp3PartInstaller.Abort();
      }
      else if (buttonAction == MP3PartInstaller.BUTTONACTION_CLOSE)
      {
        Application.ExitThread();
      }
    }
  }
}