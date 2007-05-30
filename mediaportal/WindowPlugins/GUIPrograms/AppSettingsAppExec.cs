using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ProgramsDatabase;
using System.Diagnostics;
using Programs.Utils;

namespace WindowPlugins.GUIPrograms
{
  public class AppSettingsAppExec : AppSettings
  {
    private MediaPortal.UserInterface.Controls.MPButton btnPrePost;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkbWaitForExit;
    private MediaPortal.UserInterface.Controls.MPLabel LblPinCode;
    private MediaPortal.UserInterface.Controls.MPTextBox txtPinCode;
    private MediaPortal.UserInterface.Controls.MPButton btnStartup;
    private MediaPortal.UserInterface.Controls.MPTextBox txtStartupDir;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPComboBox cbWindowStyle;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox txtArguments;
    private MediaPortal.UserInterface.Controls.MPLabel lblArg;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkbEnabled;
    private MediaPortal.UserInterface.Controls.MPTextBox txtTitle;
    private MediaPortal.UserInterface.Controls.MPLabel lblTitle;
    private MediaPortal.UserInterface.Controls.MPLabel lblFilename;
    private MediaPortal.UserInterface.Controls.MPButton buttonLaunchingApp;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPTextBox txtFilename;
    private MediaPortal.UserInterface.Controls.MPLabel lblImageFile;
    private MediaPortal.UserInterface.Controls.MPButton buttonImageFile;
    private MediaPortal.UserInterface.Controls.MPTextBox txtImageFile;
    private IContainer components = null;

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

   public AppSettingsAppExec()
    {
      InitializeComponent();
      this.txtPinCode.PasswordChar = (char)0x25CF;
    }

    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppSettingsAppExec));
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnPrePost = new MediaPortal.UserInterface.Controls.MPButton();
      this.chkbWaitForExit = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.LblPinCode = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtPinCode = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.btnStartup = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtStartupDir = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbWindowStyle = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtArguments = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblArg = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkbEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.txtTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblTitle = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblFilename = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonLaunchingApp = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtFilename = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblImageFile = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonImageFile = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtImageFile = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.SuspendLayout();
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(236, 32);
      this.label3.TabIndex = 2;
      this.label3.Text = "Application launcher";
      // 
      // btnPrePost
      // 
      this.btnPrePost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPrePost.Image = ((System.Drawing.Image)(resources.GetObject("btnPrePost.Image")));
      this.btnPrePost.Location = new System.Drawing.Point(367, 83);
      this.btnPrePost.Name = "btnPrePost";
      this.btnPrePost.Size = new System.Drawing.Size(20, 20);
      this.btnPrePost.TabIndex = 56;
      this.toolTip.SetToolTip(this.btnPrePost, "Edit Pre / Postlaunch options");
      this.btnPrePost.UseVisualStyleBackColor = true;
      this.btnPrePost.Click += new System.EventHandler(this.btnPrePost_Click);
      // 
      // chkbWaitForExit
      // 
      this.chkbWaitForExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chkbWaitForExit.AutoSize = true;
      this.chkbWaitForExit.Checked = true;
      this.chkbWaitForExit.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbWaitForExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkbWaitForExit.Location = new System.Drawing.Point(277, 86);
      this.chkbWaitForExit.Name = "chkbWaitForExit";
      this.chkbWaitForExit.Size = new System.Drawing.Size(84, 17);
      this.chkbWaitForExit.TabIndex = 54;
      this.chkbWaitForExit.Text = "Wait for exit";
      this.chkbWaitForExit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chkbWaitForExit.UseVisualStyleBackColor = true;
      // 
      // LblPinCode
      // 
      this.LblPinCode.AutoSize = true;
      this.LblPinCode.Location = new System.Drawing.Point(5, 112);
      this.LblPinCode.Name = "LblPinCode";
      this.LblPinCode.Size = new System.Drawing.Size(50, 13);
      this.LblPinCode.TabIndex = 44;
      this.LblPinCode.Text = "Pin-Code";
      this.LblPinCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtPinCode
      // 
      this.txtPinCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtPinCode.BorderColor = System.Drawing.Color.Empty;
      this.txtPinCode.Location = new System.Drawing.Point(121, 109);
      this.txtPinCode.MaxLength = 4;
      this.txtPinCode.Name = "txtPinCode";
      this.txtPinCode.Size = new System.Drawing.Size(64, 21);
      this.txtPinCode.TabIndex = 45;
      this.txtPinCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPinCode_KeyPress);
      // 
      // btnStartup
      // 
      this.btnStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStartup.Image = ((System.Drawing.Image)(resources.GetObject("btnStartup.Image")));
      this.btnStartup.Location = new System.Drawing.Point(367, 237);
      this.btnStartup.Name = "btnStartup";
      this.btnStartup.Size = new System.Drawing.Size(20, 20);
      this.btnStartup.TabIndex = 52;
      this.btnStartup.UseVisualStyleBackColor = true;
      this.btnStartup.Click += new System.EventHandler(this.btnStartup_Click);
      // 
      // txtStartupDir
      // 
      this.txtStartupDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtStartupDir.BorderColor = System.Drawing.Color.Empty;
      this.txtStartupDir.Location = new System.Drawing.Point(121, 237);
      this.txtStartupDir.Name = "txtStartupDir";
      this.txtStartupDir.Size = new System.Drawing.Size(240, 21);
      this.txtStartupDir.TabIndex = 51;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(4, 241);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(94, 13);
      this.label5.TabIndex = 50;
      this.label5.Text = "Startup Directory:";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbWindowStyle
      // 
      this.cbWindowStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbWindowStyle.BorderColor = System.Drawing.Color.Empty;
      this.cbWindowStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbWindowStyle.Items.AddRange(new object[] {
            "Normal",
            "Minimized",
            "Maximized",
            "Hidden"});
      this.cbWindowStyle.Location = new System.Drawing.Point(121, 213);
      this.cbWindowStyle.Name = "cbWindowStyle";
      this.cbWindowStyle.Size = new System.Drawing.Size(240, 21);
      this.cbWindowStyle.TabIndex = 49;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(5, 216);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(77, 13);
      this.label1.TabIndex = 48;
      this.label1.Text = "Window-Style:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtArguments
      // 
      this.txtArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtArguments.BorderColor = System.Drawing.Color.Empty;
      this.txtArguments.Location = new System.Drawing.Point(121, 189);
      this.txtArguments.Name = "txtArguments";
      this.txtArguments.Size = new System.Drawing.Size(240, 21);
      this.txtArguments.TabIndex = 47;
      // 
      // lblArg
      // 
      this.lblArg.AutoSize = true;
      this.lblArg.Location = new System.Drawing.Point(5, 192);
      this.lblArg.Name = "lblArg";
      this.lblArg.Size = new System.Drawing.Size(63, 13);
      this.lblArg.TabIndex = 46;
      this.lblArg.Text = "Arguments:";
      this.lblArg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // chkbEnabled
      // 
      this.chkbEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.chkbEnabled.AutoSize = true;
      this.chkbEnabled.Checked = true;
      this.chkbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkbEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.chkbEnabled.Location = new System.Drawing.Point(291, 6);
      this.chkbEnabled.Name = "chkbEnabled";
      this.chkbEnabled.Size = new System.Drawing.Size(70, 17);
      this.chkbEnabled.TabIndex = 53;
      this.chkbEnabled.Text = "Enabled";
      this.chkbEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.chkbEnabled.UseVisualStyleBackColor = true;
      // 
      // txtTitle
      // 
      this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTitle.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtTitle.BorderColor = System.Drawing.Color.Empty;
      this.txtTitle.Location = new System.Drawing.Point(121, 32);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(240, 21);
      this.txtTitle.TabIndex = 41;
      // 
      // lblTitle
      // 
      this.lblTitle.AutoSize = true;
      this.lblTitle.Location = new System.Drawing.Point(5, 35);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(31, 13);
      this.lblTitle.TabIndex = 40;
      this.lblTitle.Text = "Title:";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFilename
      // 
      this.lblFilename.AutoSize = true;
      this.lblFilename.Location = new System.Drawing.Point(5, 63);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(110, 13);
      this.lblFilename.TabIndex = 42;
      this.lblFilename.Text = "Application to launch:";
      this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonLaunchingApp
      // 
      this.buttonLaunchingApp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLaunchingApp.Image = ((System.Drawing.Image)(resources.GetObject("buttonLaunchingApp.Image")));
      this.buttonLaunchingApp.Location = new System.Drawing.Point(367, 59);
      this.buttonLaunchingApp.Name = "buttonLaunchingApp";
      this.buttonLaunchingApp.Size = new System.Drawing.Size(20, 20);
      this.buttonLaunchingApp.TabIndex = 43;
      this.buttonLaunchingApp.UseVisualStyleBackColor = true;
      this.buttonLaunchingApp.Click += new System.EventHandler(this.buttonLaunchingApp_Click);
      // 
      // txtFilename
      // 
      this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtFilename.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtFilename.BorderColor = System.Drawing.Color.Empty;
      this.txtFilename.Location = new System.Drawing.Point(121, 59);
      this.txtFilename.Name = "txtFilename";
      this.txtFilename.Size = new System.Drawing.Size(240, 21);
      this.txtFilename.TabIndex = 57;
      // 
      // lblImageFile
      // 
      this.lblImageFile.AutoSize = true;
      this.lblImageFile.Location = new System.Drawing.Point(5, 141);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(55, 13);
      this.lblImageFile.TabIndex = 58;
      this.lblImageFile.Text = "Imagefile:";
      this.lblImageFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonImageFile
      // 
      this.buttonImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonImageFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonImageFile.Image")));
      this.buttonImageFile.Location = new System.Drawing.Point(367, 137);
      this.buttonImageFile.Name = "buttonImageFile";
      this.buttonImageFile.Size = new System.Drawing.Size(20, 20);
      this.buttonImageFile.TabIndex = 60;
      this.buttonImageFile.UseVisualStyleBackColor = true;
      this.buttonImageFile.Click += new System.EventHandler(this.buttonImageFile_Click);
      // 
      // txtImageFile
      // 
      this.txtImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtImageFile.BorderColor = System.Drawing.Color.Empty;
      this.txtImageFile.Location = new System.Drawing.Point(121, 136);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(240, 21);
      this.txtImageFile.TabIndex = 59;
      // 
      // AppSettingsAppExec
      // 
      this.Controls.Add(this.lblImageFile);
      this.Controls.Add(this.buttonImageFile);
      this.Controls.Add(this.txtImageFile);
      this.Controls.Add(this.txtFilename);
      this.Controls.Add(this.btnPrePost);
      this.Controls.Add(this.chkbWaitForExit);
      this.Controls.Add(this.LblPinCode);
      this.Controls.Add(this.txtPinCode);
      this.Controls.Add(this.btnStartup);
      this.Controls.Add(this.txtStartupDir);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.cbWindowStyle);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtArguments);
      this.Controls.Add(this.lblArg);
      this.Controls.Add(this.chkbEnabled);
      this.Controls.Add(this.txtTitle);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.lblFilename);
      this.Controls.Add(this.buttonLaunchingApp);
      this.Controls.Add(this.label3);
      this.Name = "AppSettingsAppExec";
      this.Size = new System.Drawing.Size(398, 273);
      this.Load += new System.EventHandler(this.AppSettingsAppExec_Load_1);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    public override bool AppObj2Form(AppItem curApp)
    {
      base.AppObj2Form(curApp);
      this.chkbEnabled.Checked = curApp.Enabled;
      this.txtTitle.Text = curApp.Title;
      this.txtFilename.Text = curApp.Filename;
      SetWindowStyle(curApp.WindowStyle);
      this.txtStartupDir.Text = curApp.Startupdir;
      this.chkbWaitForExit.Checked = (curApp.WaitForExit);
      this.txtImageFile.Text = curApp.Imagefile;
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
      base.Form2AppObj(curApp);
      curApp.Enabled = this.chkbEnabled.Checked;
      curApp.Title = this.txtTitle.Text;
      curApp.Filename = this.txtFilename.Text;
      curApp.Arguments = this.txtArguments.Text;
      curApp.WindowStyle = GetSelectedWindowStyle();
      curApp.Startupdir = this.txtStartupDir.Text;
      curApp.WaitForExit = (this.chkbWaitForExit.Checked);
      curApp.SourceType = myProgSourceType.APPEXEC;
      curApp.Imagefile = this.txtImageFile.Text;
      curApp.Pincode = ProgramUtils.StrToIntDef(this.txtPinCode.Text, -1);
    }

    public override bool EntriesOK(AppItem curApp)
    {
      m_Checker.Clear();
      m_Checker.DoCheck(txtTitle.Text != "", "No title entered!");
      m_Checker.DoCheck(txtFilename.Text != "", "No application filename entered!");

      if (!m_Checker.IsOk)
      {
        string strHeader = "The following entries are invalid: \r\n\r\n";
        string strFooter = "\r\n\r\n(Click DELETE to remove this item)";
        MessageBox.Show(strHeader + m_Checker.Problems + strFooter, "Invalid Entries");
      }
      else
      { }
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

    private void buttonLaunchingApp_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = txtFilename.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        txtFilename.Text = dialogFile.FileName;
      }
    }

    private void btnPrePost_Click(object sender, EventArgs e)
    {
      PrePostLaunchClick(txtTitle.Text);
    }

    private void btnStartup_Click(object sender, EventArgs e)
    {
      dialogFolder.SelectedPath = txtStartupDir.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        txtStartupDir.Text = dialogFolder.SelectedPath;
      }
    }
    
    public override void LoadFromAppItem(AppItem tempApp)
    {
      this.txtTitle.Text = tempApp.Title;
      if (txtFilename.Text == "")
      {
        this.txtFilename.Text = tempApp.Filename;
      }
      this.txtArguments.Text = tempApp.Arguments;
      SetWindowStyle(tempApp.WindowStyle);
      if (this.txtStartupDir.Text == "")
      {
        this.txtStartupDir.Text = tempApp.Startupdir;
      }
      this.chkbWaitForExit.Checked = (tempApp.WaitForExit);
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

    private void AppSettingsAppExec_Load_1(object sender, EventArgs e)
    {
      // set tooltip-stuff..... 
      toolTip.SetToolTip(txtTitle, "This text will appear in the listitem of MediaPortal\r\n(mandatory)");
      toolTip.SetToolTip(txtStartupDir, "Optional path that is passed as the launch-directory \r\n\r\n(advanced hint: Use %FILEDIR" +
        "% if you want to use the directory where the launched application is stored)");
      toolTip.SetToolTip(cbWindowStyle, "Appearance of the launched program. \r\nTry HIDDEN or MINIMIZED for a seamless integr" + "ation in MediaPortal");
      toolTip.SetToolTip(txtArguments, "Optional arguments that are needed to launch the program \r\n\r\n(Enter a filename to open a file with the specified application)");
      toolTip.SetToolTip(chkbEnabled, "Only enabled items will appear in MediaPortal");
      toolTip.SetToolTip(txtFilename, "Program you wish to execute, include the full path (mandatory)");
      toolTip.SetToolTip(txtImageFile, "Optional filename for an image to display in MediaPortal");
    }

    private void buttonImageFile_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = txtImageFile.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        txtImageFile.Text = dialogFile.FileName;
      }
    }
  }
}
