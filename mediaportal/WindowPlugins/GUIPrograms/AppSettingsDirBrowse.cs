/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  public class AppSettingsDirBrowse: AppSettings
  {
    private CheckBox chkbUseShellExecute;
    private CheckBox chkbUseQuotes;
    private Button btnStartup;
    private TextBox txtStartupDir;
    private Label label5;
    private ComboBox cbWindowStyle;
    private Label label1;
    private TextBox txtArguments;
    private Label lblArg;
    private Label lblImageFile;
    private TextBox txtImageFile;
    private CheckBox chkbEnabled;
    private TextBox txtTitle;
    private Label lblTitle;
    private Label lblFilename;
    private Label lblImgDirectories;
    private TextBox txtExtensions;
    private TextBox txtImageDirs;
    private TextBox txtFiles;
    private Label label4;
    private Label label2;
    private Label label3;
    private Button buttonImagefile;
    private Button buttonLaunchingApp;
    private Button buttonFiledir;
    private Button buttonImageDirs;
    private TextBox txtPinCode;
    private Label LblPinCode;
    private Button buttonGetExtensions;
    private CheckBox chkbWaitForExit;
    private ComboBox cbFilename;
    private IContainer components = null;

    public AppSettingsDirBrowse()
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      this.txtPinCode.PasswordChar = (char)0x25CF;
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppSettingsDirBrowse));
      this.chkbUseShellExecute = new System.Windows.Forms.CheckBox();
      this.chkbUseQuotes = new System.Windows.Forms.CheckBox();
      this.btnStartup = new System.Windows.Forms.Button();
      this.txtStartupDir = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.cbWindowStyle = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.txtArguments = new System.Windows.Forms.TextBox();
      this.lblArg = new System.Windows.Forms.Label();
      this.lblImageFile = new System.Windows.Forms.Label();
      this.buttonImagefile = new System.Windows.Forms.Button();
      this.txtImageFile = new System.Windows.Forms.TextBox();
      this.chkbEnabled = new System.Windows.Forms.CheckBox();
      this.txtTitle = new System.Windows.Forms.TextBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblFilename = new System.Windows.Forms.Label();
      this.buttonLaunchingApp = new System.Windows.Forms.Button();
      this.lblImgDirectories = new System.Windows.Forms.Label();
      this.buttonFiledir = new System.Windows.Forms.Button();
      this.txtExtensions = new System.Windows.Forms.TextBox();
      this.txtImageDirs = new System.Windows.Forms.TextBox();
      this.txtFiles = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.buttonImageDirs = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.txtPinCode = new System.Windows.Forms.TextBox();
      this.LblPinCode = new System.Windows.Forms.Label();
      this.buttonGetExtensions = new System.Windows.Forms.Button();
      this.chkbWaitForExit = new System.Windows.Forms.CheckBox();
      this.cbFilename = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // chkbUseShellExecute
      // 
      this.chkbUseShellExecute.Location = new System.Drawing.Point(120, 88);
      this.chkbUseShellExecute.Name = "chkbUseShellExecute";
      this.chkbUseShellExecute.Size = new System.Drawing.Size(168, 24);
      this.chkbUseShellExecute.TabIndex = 9;
      this.chkbUseShellExecute.Text = "Startup using ShellExecute";
      // 
      // chkbUseQuotes
      // 
      this.chkbUseQuotes.Checked = true;
      this.chkbUseQuotes.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbUseQuotes.Location = new System.Drawing.Point(120, 232);
      this.chkbUseQuotes.Name = "chkbUseQuotes";
      this.chkbUseQuotes.Size = new System.Drawing.Size(184, 24);
      this.chkbUseQuotes.TabIndex = 21;
      this.chkbUseQuotes.Text = "Quotes around Filenames";
      // 
      // btnStartup
      // 
      this.btnStartup.Image = ((System.Drawing.Image)(resources.GetObject("btnStartup.Image")));
      this.btnStartup.Location = new System.Drawing.Point(376, 208);
      this.btnStartup.Name = "btnStartup";
      this.btnStartup.Size = new System.Drawing.Size(20, 20);
      this.btnStartup.TabIndex = 20;
      this.btnStartup.Click += new System.EventHandler(this.btnStartup_Click);
      // 
      // txtStartupDir
      // 
      this.txtStartupDir.Location = new System.Drawing.Point(120, 208);
      this.txtStartupDir.Name = "txtStartupDir";
      this.txtStartupDir.Size = new System.Drawing.Size(250, 20);
      this.txtStartupDir.TabIndex = 19;
      this.txtStartupDir.Text = "";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(0, 208);
      this.label5.Name = "label5";
      this.label5.TabIndex = 18;
      this.label5.Text = "Startup Directory:";
      // 
      // cbWindowStyle
      // 
      this.cbWindowStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbWindowStyle.Items.AddRange(new object[] {
                                                       "Normal",
                                                       "Minimized",
                                                       "Maximized",
                                                       "Hidden"});
      this.cbWindowStyle.Location = new System.Drawing.Point(120, 184);
      this.cbWindowStyle.Name = "cbWindowStyle";
      this.cbWindowStyle.Size = new System.Drawing.Size(250, 21);
      this.cbWindowStyle.TabIndex = 17;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(0, 184);
      this.label1.Name = "label1";
      this.label1.TabIndex = 16;
      this.label1.Text = "Window-Style:";
      // 
      // txtArguments
      // 
      this.txtArguments.Location = new System.Drawing.Point(120, 160);
      this.txtArguments.Name = "txtArguments";
      this.txtArguments.Size = new System.Drawing.Size(250, 20);
      this.txtArguments.TabIndex = 15;
      this.txtArguments.Text = "";
      // 
      // lblArg
      // 
      this.lblArg.Location = new System.Drawing.Point(0, 160);
      this.lblArg.Name = "lblArg";
      this.lblArg.TabIndex = 14;
      this.lblArg.Text = "Arguments:";
      // 
      // lblImageFile
      // 
      this.lblImageFile.Location = new System.Drawing.Point(0, 136);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(80, 20);
      this.lblImageFile.TabIndex = 11;
      this.lblImageFile.Text = "Imagefile:";
      // 
      // buttonImagefile
      // 
      this.buttonImagefile.Image = ((System.Drawing.Image)(resources.GetObject("buttonImagefile.Image")));
      this.buttonImagefile.Location = new System.Drawing.Point(376, 136);
      this.buttonImagefile.Name = "buttonImagefile";
      this.buttonImagefile.Size = new System.Drawing.Size(20, 20);
      this.buttonImagefile.TabIndex = 13;
      this.buttonImagefile.Click += new System.EventHandler(this.buttonImagefile_Click);
      // 
      // txtImageFile
      // 
      this.txtImageFile.Location = new System.Drawing.Point(120, 136);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(250, 20);
      this.txtImageFile.TabIndex = 12;
      this.txtImageFile.Text = "";
      // 
      // chkbEnabled
      // 
      this.chkbEnabled.Checked = true;
      this.chkbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.chkbEnabled.Location = new System.Drawing.Point(320, 8);
      this.chkbEnabled.Name = "chkbEnabled";
      this.chkbEnabled.Size = new System.Drawing.Size(72, 24);
      this.chkbEnabled.TabIndex = 30;
      this.chkbEnabled.Text = "Enabled";
      this.chkbEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // txtTitle
      // 
      this.txtTitle.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtTitle.Location = new System.Drawing.Point(120, 40);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(248, 20);
      this.txtTitle.TabIndex = 4;
      this.txtTitle.Text = "";
      // 
      // lblTitle
      // 
      this.lblTitle.Location = new System.Drawing.Point(0, 40);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.TabIndex = 3;
      this.lblTitle.Text = "Title:";
      // 
      // lblFilename
      // 
      this.lblFilename.Location = new System.Drawing.Point(0, 64);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(120, 20);
      this.lblFilename.TabIndex = 5;
      this.lblFilename.Text = "Launching Application:";
      // 
      // buttonLaunchingApp
      // 
      this.buttonLaunchingApp.Image = ((System.Drawing.Image)(resources.GetObject("buttonLaunchingApp.Image")));
      this.buttonLaunchingApp.Location = new System.Drawing.Point(376, 64);
      this.buttonLaunchingApp.Name = "buttonLaunchingApp";
      this.buttonLaunchingApp.Size = new System.Drawing.Size(20, 20);
      this.buttonLaunchingApp.TabIndex = 7;
      this.buttonLaunchingApp.Click += new System.EventHandler(this.buttonLaunchingApp_Click);
      // 
      // lblImgDirectories
      // 
      this.lblImgDirectories.Location = new System.Drawing.Point(0, 312);
      this.lblImgDirectories.Name = "lblImgDirectories";
      this.lblImgDirectories.TabIndex = 27;
      this.lblImgDirectories.Text = "Image directories:";
      // 
      // buttonFiledir
      // 
      this.buttonFiledir.Image = ((System.Drawing.Image)(resources.GetObject("buttonFiledir.Image")));
      this.buttonFiledir.Location = new System.Drawing.Point(376, 264);
      this.buttonFiledir.Name = "buttonFiledir";
      this.buttonFiledir.Size = new System.Drawing.Size(20, 20);
      this.buttonFiledir.TabIndex = 24;
      this.buttonFiledir.Click += new System.EventHandler(this.buttonFiledir_Click);
      // 
      // txtExtensions
      // 
      this.txtExtensions.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtExtensions.Location = new System.Drawing.Point(120, 288);
      this.txtExtensions.Name = "txtExtensions";
      this.txtExtensions.Size = new System.Drawing.Size(250, 20);
      this.txtExtensions.TabIndex = 26;
      this.txtExtensions.Text = "";
      // 
      // txtImageDirs
      // 
      this.txtImageDirs.Location = new System.Drawing.Point(120, 312);
      this.txtImageDirs.Multiline = true;
      this.txtImageDirs.Name = "txtImageDirs";
      this.txtImageDirs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtImageDirs.Size = new System.Drawing.Size(250, 64);
      this.txtImageDirs.TabIndex = 28;
      this.txtImageDirs.Text = "txtImageDirs";
      // 
      // txtFiles
      // 
      this.txtFiles.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtFiles.Location = new System.Drawing.Point(120, 264);
      this.txtFiles.Name = "txtFiles";
      this.txtFiles.Size = new System.Drawing.Size(250, 20);
      this.txtFiles.TabIndex = 23;
      this.txtFiles.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(0, 288);
      this.label4.Name = "label4";
      this.label4.TabIndex = 25;
      this.label4.Text = "File-Extensions:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(0, 264);
      this.label2.Name = "label2";
      this.label2.TabIndex = 22;
      this.label2.Text = "File Directory:";
      // 
      // buttonImageDirs
      // 
      this.buttonImageDirs.Image = ((System.Drawing.Image)(resources.GetObject("buttonImageDirs.Image")));
      this.buttonImageDirs.Location = new System.Drawing.Point(376, 312);
      this.buttonImageDirs.Name = "buttonImageDirs";
      this.buttonImageDirs.Size = new System.Drawing.Size(20, 20);
      this.buttonImageDirs.TabIndex = 29;
      this.buttonImageDirs.Click += new System.EventHandler(this.buttonImageDirs_Click);
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label3.Location = new System.Drawing.Point(0, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(184, 32);
      this.label3.TabIndex = 1;
      this.label3.Text = "Directory-Browse";
      // 
      // txtPinCode
      // 
      this.txtPinCode.Location = new System.Drawing.Point(120, 112);
      this.txtPinCode.MaxLength = 4;
      this.txtPinCode.Name = "txtPinCode";
      this.txtPinCode.Size = new System.Drawing.Size(64, 20);
      this.txtPinCode.TabIndex = 10;
      this.txtPinCode.Text = "";
      this.txtPinCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPinCode_KeyPress);
      // 
      // LblPinCode
      // 
      this.LblPinCode.Location = new System.Drawing.Point(0, 112);
      this.LblPinCode.Name = "LblPinCode";
      this.LblPinCode.Size = new System.Drawing.Size(96, 16);
      this.LblPinCode.TabIndex = 8;
      this.LblPinCode.Text = "Pin-Code";
      // 
      // buttonGetExtensions
      // 
      this.buttonGetExtensions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.buttonGetExtensions.Location = new System.Drawing.Point(376, 288);
      this.buttonGetExtensions.Name = "buttonGetExtensions";
      this.buttonGetExtensions.Size = new System.Drawing.Size(20, 20);
      this.buttonGetExtensions.TabIndex = 31;
      this.buttonGetExtensions.Text = "?";
      this.toolTip.SetToolTip(this.buttonGetExtensions, "Read all available file extensions from the file directory");
      this.buttonGetExtensions.Click += new System.EventHandler(this.buttonGetExtensions_Click);
      // 
      // chkbWaitForExit
      // 
      this.chkbWaitForExit.Checked = true;
      this.chkbWaitForExit.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbWaitForExit.Location = new System.Drawing.Point(280, 88);
      this.chkbWaitForExit.Name = "chkbWaitForExit";
      this.chkbWaitForExit.Size = new System.Drawing.Size(88, 24);
      this.chkbWaitForExit.TabIndex = 32;
      this.chkbWaitForExit.Text = "Wait for exit";
      // 
      // cbFilename
      // 
      this.cbFilename.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.cbFilename.Items.AddRange(new object[] {
                                                    "%PLAY%",
                                                    "%PLAYAUDIOSTREAM%",
                                                    "%PLAYVIDEOSTREAM%"});
      this.cbFilename.Location = new System.Drawing.Point(120, 64);
      this.cbFilename.Name = "cbFilename";
      this.cbFilename.Size = new System.Drawing.Size(250, 21);
      this.cbFilename.TabIndex = 38;
      // 
      // AppSettingsDirBrowse
      // 
      this.Controls.Add(this.cbFilename);
      this.Controls.Add(this.chkbWaitForExit);
      this.Controls.Add(this.buttonGetExtensions);
      this.Controls.Add(this.LblPinCode);
      this.Controls.Add(this.txtPinCode);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.lblImgDirectories);
      this.Controls.Add(this.buttonFiledir);
      this.Controls.Add(this.txtExtensions);
      this.Controls.Add(this.txtImageDirs);
      this.Controls.Add(this.txtFiles);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.buttonImageDirs);
      this.Controls.Add(this.chkbUseShellExecute);
      this.Controls.Add(this.chkbUseQuotes);
      this.Controls.Add(this.btnStartup);
      this.Controls.Add(this.txtStartupDir);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.cbWindowStyle);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtArguments);
      this.Controls.Add(this.lblArg);
      this.Controls.Add(this.lblImageFile);
      this.Controls.Add(this.buttonImagefile);
      this.Controls.Add(this.txtImageFile);
      this.Controls.Add(this.chkbEnabled);
      this.Controls.Add(this.txtTitle);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.lblFilename);
      this.Controls.Add(this.buttonLaunchingApp);
      this.Name = "AppSettingsDirBrowse";
      this.Size = new System.Drawing.Size(408, 384);
      this.Load += new System.EventHandler(this.AppSettingsDirBrowse_Load);
      this.ResumeLayout(false);

    }
    #endregion 

    public override bool AppObj2Form(AppItem curApp)
    {
      this.chkbEnabled.Checked = curApp.Enabled;
      this.txtTitle.Text = curApp.Title;
      this.cbFilename.Text = curApp.Filename;
      this.txtArguments.Text = curApp.Arguments;
      SetWindowStyle(curApp.WindowStyle);
      this.txtStartupDir.Text = curApp.Startupdir;
      this.chkbUseShellExecute.Checked = (curApp.UseShellExecute);
      this.chkbUseQuotes.Checked = (curApp.UseQuotes);
      this.chkbWaitForExit.Checked = (curApp.WaitForExit);
      this.txtImageFile.Text = curApp.Imagefile;
      this.txtFiles.Text = curApp.FileDirectory;
      this.txtImageDirs.Text = curApp.ImageDirectory;
      this.txtExtensions.Text = curApp.ValidExtensions;
      if (curApp.Pincode > 0)
      {
        this.txtPinCode.Text = String.Format("{0}", curApp.Pincode);
      }
      else
      {
        this.txtPinCode.Text = "";
      }

      return true;
    }


    public override void Form2AppObj(AppItem curApp)
    {
      curApp.Enabled = this.chkbEnabled.Checked;
      curApp.Title = this.txtTitle.Text;
      curApp.Filename = this.cbFilename.Text;
      curApp.Arguments = this.txtArguments.Text;
      curApp.WindowStyle = GetSelectedWindowStyle();
      curApp.Startupdir = this.txtStartupDir.Text;
      curApp.UseShellExecute = (this.chkbUseShellExecute.Checked);
      curApp.UseQuotes = (this.chkbUseQuotes.Checked);
      curApp.WaitForExit = (this.chkbWaitForExit.Checked);
      curApp.SourceType = myProgSourceType.DIRBROWSE;
      curApp.Imagefile = this.txtImageFile.Text;
      curApp.FileDirectory = this.txtFiles.Text;
      curApp.ImageDirectory = this.txtImageDirs.Text;
      curApp.ValidExtensions = this.txtExtensions.Text;
      curApp.Pincode = ProgramUtils.StrToIntDef(this.txtPinCode.Text,  - 1);
    }

    public override bool EntriesOK(AppItem curApp)
    {
      m_Checker.Clear();
      m_Checker.DoCheck(txtTitle.Text != "", "No title entered!");
      if (cbFilename.Text == "")
      {
        m_Checker.DoCheck(chkbUseShellExecute.Checked, "No launching filename entered!");
      }
      m_Checker.DoCheck(txtFiles.Text != "", "No filedirectory entered!");
      m_Checker.DoCheck(txtExtensions.Text != "", "No file extensions entered!");

      if (!m_Checker.IsOk)
      {
        string strHeader = "The following entries are invalid: \r\n\r\n";
        string strFooter = "\r\n\r\n(Click DELETE to remove this item)";
        MessageBox.Show(strHeader + m_Checker.Problems + strFooter, "Invalid Entries");
      }
      else
      {}
        return m_Checker.IsOk;
    }

    private void SetWindowStyle(ProcessWindowStyle val)
    {
      switch (val)
      {
        case ProcessWindowStyle.Normal:
          cbWindowStyle.SelectedIndex = 0;
          break;
        case ProcessWindowStyle.Minimized:
          cbWindowStyle.SelectedIndex = 1;
          break;
        case ProcessWindowStyle.Maximized:
          cbWindowStyle.SelectedIndex = 2;
          break;
        case ProcessWindowStyle.Hidden:
          cbWindowStyle.SelectedIndex = 3;
          break;
      }

    }

    private ProcessWindowStyle GetSelectedWindowStyle()
    {
      if (this.cbWindowStyle.SelectedIndex == 1)
      {
        return ProcessWindowStyle.Minimized;
      }
      else if (this.cbWindowStyle.SelectedIndex == 2)
      {
        return ProcessWindowStyle.Maximized;
      }
      else if (this.cbWindowStyle.SelectedIndex == 3)
      {
        return ProcessWindowStyle.Hidden;
      }
      else
        return ProcessWindowStyle.Normal;

    }

    private void AppSettingsDirBrowse_Load(object sender, EventArgs e)
    {
      // set tooltip-stuff..... 
      toolTip.SetToolTip(txtTitle, "This text will appear in the listitem of MediaPortal\r\n(mandatory)");
      toolTip.SetToolTip(txtFiles, "First directory to display in MediaPortal.\r\n(mandatory)");
      toolTip.SetToolTip(chkbUseShellExecute, "Enable this if you want to run a program that is associated with a specific file-" + 
        "extension.\r\nYou can omit the \"Launching Application\" in this case.");
      toolTip.SetToolTip(chkbUseQuotes, "Quotes are usually needed to handle filenames with spaces correctly. \r\nAvoid double" + " quotes though!");
      toolTip.SetToolTip(txtStartupDir, "Optional path that is passed as the launch-directory \r\n\r\n(advanced hint: Use %FILEDIR" + 
        "% if you want to use the directory where the launched file is stored)");
      toolTip.SetToolTip(cbWindowStyle, "Appearance of the launched program. \r\nTry HIDDEN or MINIMIZED for a seamless integr" + "ation in MediaPortal");
      toolTip.SetToolTip(txtArguments, "Optional arguments that are needed to launch the program \r\n\r\n(advanced hint: Use %FIL" + 
        "E% if the filename needs to be placed in some specific place between several arg" + "uments)");
      toolTip.SetToolTip(txtImageFile, "Optional filename for an image to display in MediaPortal");
      toolTip.SetToolTip(chkbEnabled, "Only enabled items will appear in MediaPortal");
      toolTip.SetToolTip(cbFilename, "Program you wish to execute, include the full path (mandatory if ShellExecute is " + "OFF)");
      toolTip.SetToolTip(txtExtensions, "Only files with matching extensions will be displayed. \r\nSeparate several extension" + 
        "s by a coma (.zip,.txt)\r\n(mandatory)");
      toolTip.SetToolTip(txtImageDirs, "Optional directory where MediaPortal searches for matching images. \r\n MediaPort" + 
        "al will cycle through all the directories and display a mini-slideshow of all ma" + "tching images.");
    }

    private void buttonLaunchingApp_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = cbFilename.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        cbFilename.Text = dialogFile.FileName;
      }
    }

    private void buttonImagefile_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = txtImageFile.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        txtImageFile.Text = dialogFile.FileName;
      }
    }

    private void btnStartup_Click(object sender, EventArgs e)
    {
      dialogFolder.SelectedPath = txtStartupDir.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        txtStartupDir.Text = dialogFolder.SelectedPath;
      }
    }

    private void buttonFiledir_Click(object sender, EventArgs e)
    {
      dialogFolder.SelectedPath = txtFiles.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        txtFiles.Text = dialogFolder.SelectedPath;
      }
    }

    private void buttonImageDirs_Click(object sender, EventArgs e)
    {
      if (txtImageDirs.Text != "")
      {
        dialogFolder.SelectedPath = txtImageDirs.Lines[0];
      }
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        string strSep = "";
        if (txtImageDirs.Text != "")
        {
          strSep = "\r\n";
        }
        txtImageDirs.Text = txtImageDirs.Text + strSep + dialogFolder.SelectedPath;
      }
    }

    private void txtPinCode_KeyPress(object sender, KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void buttonGetExtensions_Click(object sender, EventArgs e)
    {
      txtExtensions.Text = ProgramUtils.GetAvailableExtensions(txtFiles.Text);
    }


    public override void LoadFromAppItem(AppItem tempApp)
    {
      this.txtTitle.Text = tempApp.Title;
      if (cbFilename.Text == "")
      {
        this.cbFilename.Text = tempApp.Filename;
      }
      this.txtArguments.Text = tempApp.Arguments;
      SetWindowStyle(tempApp.WindowStyle);
      if (this.txtStartupDir.Text == "")
      {
        this.txtStartupDir.Text = tempApp.Startupdir;
      }
      this.chkbUseShellExecute.Checked = (tempApp.UseShellExecute);
      this.chkbUseQuotes.Checked = (tempApp.UseQuotes);
      this.chkbWaitForExit.Checked = (tempApp.WaitForExit);
      if (this.txtExtensions.Text == "")
      {
        this.txtExtensions.Text = tempApp.ValidExtensions;
      }
    }


  }
}
