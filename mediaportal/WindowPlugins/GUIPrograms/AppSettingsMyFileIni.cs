using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  public class AppSettingsMyFileIni: AppSettings
  {
    private Label label3;
    private Label lblImgDirectories;
    private TextBox txtImageDirs;
    private Button btnImageDirs;
    private CheckBox chkbUseShellExecute;
    private CheckBox chkbUseQuotes;
    private Button btnStartup;
    private TextBox txtStartupDir;
    private Label label5;
    private ComboBox cbWindowStyle;
    private Label label6;
    private TextBox txtArguments;
    private Label lblArg;
    private Label lblImageFile;
    private TextBox txtImageFile;
    private CheckBox chkbEnabled;
    private TextBox txtTitle;
    private Label lblTitle;
    private Label lblFilename;
    private CheckBox chkbValidImagesOnly;
    private TextBox txtSource;
    private Label label1;
    private Button buttonImageFile;
    private Button buttonLaunchingApp;
    private Button buttonSourceFile;
    private CheckBox chkbEnableGUIRefresh;
    private Label LblPinCode;
    private TextBox txtPinCode;
    private Label lblContent;
    private ComboBox cbContentProfile;
    private CheckBox chkbWaitForExit;
    private ComboBox cbFilename;
    private IContainer components = null;

    public AppSettingsMyFileIni()
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      this.txtPinCode.PasswordChar = (char)0x25CF;
      FillCombo();
    }

    void FillCombo()
    {
      cbContentProfile.Items.Clear();
      for (int i = 0; i < ProgramContentManager.NodeCount; i++)
      {
        cbContentProfile.Items.Add(ProgramContentManager.NodeTitle(i));
      }
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppSettingsMyFileIni));
      this.label3 = new System.Windows.Forms.Label();
      this.lblImgDirectories = new System.Windows.Forms.Label();
      this.txtImageDirs = new System.Windows.Forms.TextBox();
      this.btnImageDirs = new System.Windows.Forms.Button();
      this.chkbUseShellExecute = new System.Windows.Forms.CheckBox();
      this.chkbUseQuotes = new System.Windows.Forms.CheckBox();
      this.btnStartup = new System.Windows.Forms.Button();
      this.txtStartupDir = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.cbWindowStyle = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.txtArguments = new System.Windows.Forms.TextBox();
      this.lblArg = new System.Windows.Forms.Label();
      this.lblImageFile = new System.Windows.Forms.Label();
      this.buttonImageFile = new System.Windows.Forms.Button();
      this.txtImageFile = new System.Windows.Forms.TextBox();
      this.chkbEnabled = new System.Windows.Forms.CheckBox();
      this.txtTitle = new System.Windows.Forms.TextBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblFilename = new System.Windows.Forms.Label();
      this.buttonLaunchingApp = new System.Windows.Forms.Button();
      this.chkbValidImagesOnly = new System.Windows.Forms.CheckBox();
      this.buttonSourceFile = new System.Windows.Forms.Button();
      this.txtSource = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.chkbEnableGUIRefresh = new System.Windows.Forms.CheckBox();
      this.LblPinCode = new System.Windows.Forms.Label();
      this.txtPinCode = new System.Windows.Forms.TextBox();
      this.chkbWaitForExit = new System.Windows.Forms.CheckBox();
      this.lblContent = new System.Windows.Forms.Label();
      this.cbContentProfile = new System.Windows.Forms.ComboBox();
      this.cbFilename = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label3.Location = new System.Drawing.Point(0, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(184, 32);
      this.label3.TabIndex = 0;
      this.label3.Text = "My-File Importer";
      // 
      // lblImgDirectories
      // 
      this.lblImgDirectories.Location = new System.Drawing.Point(0, 312);
      this.lblImgDirectories.Name = "lblImgDirectories";
      this.lblImgDirectories.TabIndex = 24;
      this.lblImgDirectories.Text = "Image directories:";
      // 
      // txtImageDirs
      // 
      this.txtImageDirs.Location = new System.Drawing.Point(120, 312);
      this.txtImageDirs.Multiline = true;
      this.txtImageDirs.Name = "txtImageDirs";
      this.txtImageDirs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtImageDirs.Size = new System.Drawing.Size(250, 64);
      this.txtImageDirs.TabIndex = 25;
      this.txtImageDirs.Text = "txtImageDirs";
      // 
      // btnImageDirs
      // 
      this.btnImageDirs.Image = ((System.Drawing.Image)(resources.GetObject("btnImageDirs.Image")));
      this.btnImageDirs.Location = new System.Drawing.Point(376, 312);
      this.btnImageDirs.Name = "btnImageDirs";
      this.btnImageDirs.Size = new System.Drawing.Size(20, 20);
      this.btnImageDirs.TabIndex = 26;
      this.btnImageDirs.Click += new System.EventHandler(this.btnImageDirs_Click);
      // 
      // chkbUseShellExecute
      // 
      this.chkbUseShellExecute.Location = new System.Drawing.Point(120, 88);
      this.chkbUseShellExecute.Name = "chkbUseShellExecute";
      this.chkbUseShellExecute.Size = new System.Drawing.Size(176, 24);
      this.chkbUseShellExecute.TabIndex = 7;
      this.chkbUseShellExecute.Text = "Startup using ShellExecute";
      // 
      // chkbUseQuotes
      // 
      this.chkbUseQuotes.Checked = true;
      this.chkbUseQuotes.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbUseQuotes.Location = new System.Drawing.Point(120, 232);
      this.chkbUseQuotes.Name = "chkbUseQuotes";
      this.chkbUseQuotes.Size = new System.Drawing.Size(184, 24);
      this.chkbUseQuotes.TabIndex = 19;
      this.chkbUseQuotes.Text = "Quotes around Filenames";
      // 
      // btnStartup
      // 
      this.btnStartup.Image = ((System.Drawing.Image)(resources.GetObject("btnStartup.Image")));
      this.btnStartup.Location = new System.Drawing.Point(376, 208);
      this.btnStartup.Name = "btnStartup";
      this.btnStartup.Size = new System.Drawing.Size(20, 20);
      this.btnStartup.TabIndex = 18;
      this.btnStartup.Click += new System.EventHandler(this.btnStartup_Click);
      // 
      // txtStartupDir
      // 
      this.txtStartupDir.Location = new System.Drawing.Point(120, 208);
      this.txtStartupDir.Name = "txtStartupDir";
      this.txtStartupDir.Size = new System.Drawing.Size(250, 20);
      this.txtStartupDir.TabIndex = 17;
      this.txtStartupDir.Text = "";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(0, 208);
      this.label5.Name = "label5";
      this.label5.TabIndex = 16;
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
      this.cbWindowStyle.TabIndex = 15;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(0, 184);
      this.label6.Name = "label6";
      this.label6.TabIndex = 14;
      this.label6.Text = "Window-Style:";
      // 
      // txtArguments
      // 
      this.txtArguments.Location = new System.Drawing.Point(120, 160);
      this.txtArguments.Name = "txtArguments";
      this.txtArguments.Size = new System.Drawing.Size(250, 20);
      this.txtArguments.TabIndex = 13;
      this.txtArguments.Text = "";
      // 
      // lblArg
      // 
      this.lblArg.Location = new System.Drawing.Point(0, 160);
      this.lblArg.Name = "lblArg";
      this.lblArg.TabIndex = 12;
      this.lblArg.Text = "Arguments:";
      // 
      // lblImageFile
      // 
      this.lblImageFile.Location = new System.Drawing.Point(0, 136);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(80, 20);
      this.lblImageFile.TabIndex = 9;
      this.lblImageFile.Text = "Imagefile:";
      // 
      // buttonImageFile
      // 
      this.buttonImageFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonImageFile.Image")));
      this.buttonImageFile.Location = new System.Drawing.Point(376, 136);
      this.buttonImageFile.Name = "buttonImageFile";
      this.buttonImageFile.Size = new System.Drawing.Size(20, 20);
      this.buttonImageFile.TabIndex = 11;
      this.buttonImageFile.Click += new System.EventHandler(this.buttonImageFile_Click);
      // 
      // txtImageFile
      // 
      this.txtImageFile.Location = new System.Drawing.Point(120, 136);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(250, 20);
      this.txtImageFile.TabIndex = 10;
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
      this.chkbEnabled.TabIndex = 28;
      this.chkbEnabled.Text = "Enabled";
      this.chkbEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // txtTitle
      // 
      this.txtTitle.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtTitle.Location = new System.Drawing.Point(120, 40);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(250, 20);
      this.txtTitle.TabIndex = 2;
      this.txtTitle.Text = "";
      // 
      // lblTitle
      // 
      this.lblTitle.Location = new System.Drawing.Point(0, 40);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.TabIndex = 1;
      this.lblTitle.Text = "Title:";
      // 
      // lblFilename
      // 
      this.lblFilename.Location = new System.Drawing.Point(0, 64);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(120, 20);
      this.lblFilename.TabIndex = 3;
      this.lblFilename.Text = "Launching Application:";
      // 
      // buttonLaunchingApp
      // 
      this.buttonLaunchingApp.Image = ((System.Drawing.Image)(resources.GetObject("buttonLaunchingApp.Image")));
      this.buttonLaunchingApp.Location = new System.Drawing.Point(376, 64);
      this.buttonLaunchingApp.Name = "buttonLaunchingApp";
      this.buttonLaunchingApp.Size = new System.Drawing.Size(20, 20);
      this.buttonLaunchingApp.TabIndex = 5;
      this.buttonLaunchingApp.Click += new System.EventHandler(this.buttonLaunchingApp_Click);
      // 
      // chkbValidImagesOnly
      // 
      this.chkbValidImagesOnly.Location = new System.Drawing.Point(120, 280);
      this.chkbValidImagesOnly.Name = "chkbValidImagesOnly";
      this.chkbValidImagesOnly.Size = new System.Drawing.Size(224, 24);
      this.chkbValidImagesOnly.TabIndex = 23;
      this.chkbValidImagesOnly.Text = "Only import files with valid images";
      // 
      // buttonSourceFile
      // 
      this.buttonSourceFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonSourceFile.Image")));
      this.buttonSourceFile.Location = new System.Drawing.Point(376, 256);
      this.buttonSourceFile.Name = "buttonSourceFile";
      this.buttonSourceFile.Size = new System.Drawing.Size(20, 20);
      this.buttonSourceFile.TabIndex = 22;
      this.buttonSourceFile.Click += new System.EventHandler(this.buttonSourceFile_Click);
      // 
      // txtSource
      // 
      this.txtSource.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtSource.Location = new System.Drawing.Point(120, 256);
      this.txtSource.Name = "txtSource";
      this.txtSource.Size = new System.Drawing.Size(250, 20);
      this.txtSource.TabIndex = 21;
      this.txtSource.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(0, 256);
      this.label1.Name = "label1";
      this.label1.TabIndex = 20;
      this.label1.Text = "Source:";
      // 
      // chkbEnableGUIRefresh
      // 
      this.chkbEnableGUIRefresh.Checked = true;
      this.chkbEnableGUIRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbEnableGUIRefresh.Location = new System.Drawing.Point(120, 379);
      this.chkbEnableGUIRefresh.Name = "chkbEnableGUIRefresh";
      this.chkbEnableGUIRefresh.Size = new System.Drawing.Size(208, 24);
      this.chkbEnableGUIRefresh.TabIndex = 27;
      this.chkbEnableGUIRefresh.Text = "Allow refresh in MediaPortal";
      // 
      // LblPinCode
      // 
      this.LblPinCode.Location = new System.Drawing.Point(0, 112);
      this.LblPinCode.Name = "LblPinCode";
      this.LblPinCode.Size = new System.Drawing.Size(96, 16);
      this.LblPinCode.TabIndex = 6;
      this.LblPinCode.Text = "Pin-Code";
      // 
      // txtPinCode
      // 
      this.txtPinCode.Location = new System.Drawing.Point(120, 112);
      this.txtPinCode.MaxLength = 4;
      this.txtPinCode.Name = "txtPinCode";
      this.txtPinCode.Size = new System.Drawing.Size(64, 20);
      this.txtPinCode.TabIndex = 8;
      this.txtPinCode.Text = "";
      this.txtPinCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPinCode_KeyPress);
      // 
      // chkbWaitForExit
      // 
      this.chkbWaitForExit.Checked = true;
      this.chkbWaitForExit.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbWaitForExit.Location = new System.Drawing.Point(280, 88);
      this.chkbWaitForExit.Name = "chkbWaitForExit";
      this.chkbWaitForExit.Size = new System.Drawing.Size(88, 24);
      this.chkbWaitForExit.TabIndex = 33;
      this.chkbWaitForExit.Text = "Wait for exit";
      // 
      // lblContent
      // 
      this.lblContent.Location = new System.Drawing.Point(0, 411);
      this.lblContent.Name = "lblContent";
      this.lblContent.Size = new System.Drawing.Size(100, 16);
      this.lblContent.TabIndex = 34;
      this.lblContent.Text = "Content-Profile:";
      // 
      // cbContentProfile
      // 
      this.cbContentProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbContentProfile.Location = new System.Drawing.Point(120, 408);
      this.cbContentProfile.Name = "cbContentProfile";
      this.cbContentProfile.Size = new System.Drawing.Size(248, 21);
      this.cbContentProfile.TabIndex = 35;
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
      this.cbFilename.TabIndex = 36;
      // 
      // AppSettingsMyFileIni
      // 
      this.Controls.Add(this.cbFilename);
      this.Controls.Add(this.cbContentProfile);
      this.Controls.Add(this.lblContent);
      this.Controls.Add(this.chkbWaitForExit);
      this.Controls.Add(this.LblPinCode);
      this.Controls.Add(this.txtPinCode);
      this.Controls.Add(this.chkbEnableGUIRefresh);
      this.Controls.Add(this.chkbValidImagesOnly);
      this.Controls.Add(this.buttonSourceFile);
      this.Controls.Add(this.txtSource);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.lblImgDirectories);
      this.Controls.Add(this.txtImageDirs);
      this.Controls.Add(this.btnImageDirs);
      this.Controls.Add(this.chkbUseShellExecute);
      this.Controls.Add(this.chkbUseQuotes);
      this.Controls.Add(this.btnStartup);
      this.Controls.Add(this.txtStartupDir);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.cbWindowStyle);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.txtArguments);
      this.Controls.Add(this.lblArg);
      this.Controls.Add(this.lblImageFile);
      this.Controls.Add(this.buttonImageFile);
      this.Controls.Add(this.txtImageFile);
      this.Controls.Add(this.chkbEnabled);
      this.Controls.Add(this.txtTitle);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.lblFilename);
      this.Controls.Add(this.buttonLaunchingApp);
      this.Controls.Add(this.label3);
      this.Name = "AppSettingsMyFileIni";
      this.Size = new System.Drawing.Size(408, 448);
      this.Load += new System.EventHandler(this.AppSettingsMyFileIni_Load);
      this.ResumeLayout(false);

    }
    #endregion 

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


    private void AppSettingsMyFileIni_Load(object sender, EventArgs e)
    {
      // set tooltip-stuff..... 
      toolTip.SetToolTip(txtTitle, "This text will appear in the listitem of MediaPortal\r\n(mandatory)");
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
      toolTip.SetToolTip(txtImageDirs, "Optional directory where MediaPortal searches for matching images. \r\n MediaPort" + 
        "al will cycle through all the directories and display a mini-slideshow of all ma" + "tching images.");
      toolTip.SetToolTip(txtSource, "(*.my) file to import with the complete path.");
      toolTip.SetToolTip(chkbValidImagesOnly, "Check this if you want to display only items where at least one matching image was found.");
      toolTip.SetToolTip(chkbEnableGUIRefresh, "Check this if users can run the import through the REFRESH button in MediaPortal.");
    }

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
      this.txtSource.Text = curApp.Source;
      this.txtImageDirs.Text = curApp.ImageDirectory;
      this.chkbValidImagesOnly.Checked = curApp.ImportValidImagesOnly;
      this.chkbEnableGUIRefresh.Checked = curApp.EnableGUIRefresh;
      if (curApp.Pincode > 0)
      {
        this.txtPinCode.Text = String.Format("{0}", curApp.Pincode);
      }
      else
      {
        this.txtPinCode.Text = "";
      }
      this.cbContentProfile.SelectedIndex = ProgramContentManager.GetIndexOfID(curApp.ContentID);
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
      curApp.SourceType = myProgSourceType.MYFILEINI;
      curApp.Imagefile = this.txtImageFile.Text;
      curApp.Source = this.txtSource.Text;
      curApp.ImageDirectory = this.txtImageDirs.Text;
      curApp.ImportValidImagesOnly = this.chkbValidImagesOnly.Checked;
      curApp.EnableGUIRefresh = this.chkbEnableGUIRefresh.Checked;
      curApp.Pincode = ProgramUtils.StrToIntDef(this.txtPinCode.Text,  - 1);
      curApp.ContentID = ProgramContentManager.NodeID(cbContentProfile.SelectedIndex);
      // Log.Write(" dw: wrote contentID {0}", curApp.ContentID);
    }

    public override bool EntriesOK(AppItem curApp)
    {
      m_Checker.Clear();
      m_Checker.DoCheck(txtTitle.Text != "", "No title entered!");
      if (cbFilename.Text == "")
      {
        m_Checker.DoCheck(chkbUseShellExecute.Checked, "No launching filename entered!");
      }
      m_Checker.DoCheck(txtSource.Text != "", "No sourcefile entered!");
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

    private void buttonLaunchingApp_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = cbFilename.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        cbFilename.Text = dialogFile.FileName;
      }
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

    private void btnStartup_Click(object sender, EventArgs e)
    {
      dialogFolder.SelectedPath = txtStartupDir.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        txtStartupDir.Text = dialogFolder.SelectedPath;
      }
    }

    private void btnImageDirs_Click(object sender, EventArgs e)
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

    private void buttonSourceFile_Click(object sender, EventArgs e)
    {
      dialogFile.FileName = txtSource.Text;
      this.dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        txtSource.Text = dialogFile.FileName;
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
      this.cbContentProfile.SelectedIndex = ProgramContentManager.GetIndexOfID(tempApp.ContentID);
    }

  }
}
