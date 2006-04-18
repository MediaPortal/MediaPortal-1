// MPTestTool.cs: MediaPortal testing aid tool.
// Copyright (C) 2005  Michel Otte
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

/*
 * Created by SharpDevelop.
 * User: Michel
 * Date: 12-9-2005
 * Time: 19:26
 * 
 */

using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.Win32;

namespace MPTestTool
{
  /// <summary>
  /// Main Form for MediaPortal testing tool.
  /// </summary>
  public class MainForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.Label step1Label2;
    private System.Windows.Forms.Label step1Label3;
    private System.Windows.Forms.Button logDirButton;
    private System.Windows.Forms.Label step2Label2;
    private System.Windows.Forms.TextBox logDirBox;
    private System.Windows.Forms.GroupBox step1Group;
    private System.Windows.Forms.Label step1Label1;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.Label step2Label4;
    private System.Windows.Forms.MenuItem menuItem7;
    private System.Windows.Forms.Button postTestButton;
    private System.Windows.Forms.MenuItem menuItem5;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.Label step2Label3;
    private System.Windows.Forms.MainMenu mainMenu;
    private System.Windows.Forms.MenuItem menuItem6;
    private System.Windows.Forms.StatusBar statusBar;
    private System.Windows.Forms.MenuItem menuItem4;
    private System.Windows.Forms.TextBox mpDirBox;
    private System.Windows.Forms.Button mpDirButton;
    private System.Windows.Forms.Button preTestButton;
    private System.Windows.Forms.Label nickLabel;
    private System.Windows.Forms.Label step2Label1;
    private System.Windows.Forms.Label logDirLabel;
    private System.Windows.Forms.TextBox nickBox;
    private System.Windows.Forms.Label mpDirLabel;
    private System.Windows.Forms.GroupBox settingsGroup;
    private System.Windows.Forms.GroupBox step2Group;

    // command line parameters
    private static string _mpDir = String.Empty;
    private static string _logDir = String.Empty;
    private static string _nick = String.Empty;
    private static bool _auto = false;

    private bool validNick = false;
    private bool validMPDir = false;
    private bool validLogDir = false;
    private bool havePsloglist = true;
    private bool allowAutoRun = false;

    private string myPath;
    private System.ComponentModel.IContainer components;
    private string strDefaultMPDir = Environment.GetEnvironmentVariable("ProgramFiles") +
          @"\Team MediaPortal\MediaPortal";

    protected override void OnLoad(EventArgs e)
    {
      // Determine my install directory and make it current
      try
      {
        string me = Assembly.GetExecutingAssembly().Location;
        this.myPath = me.Substring(0, me.LastIndexOf("\\"));
        Environment.CurrentDirectory = this.myPath;
      }
      catch (Exception ex)
      {
        Warning("Failed to change current directory to the installation directory of this tool. Exception:\n\n" + ex.GetBaseException());
      }

      // validate commandline parameters
      setStatus("Checking command line parameters");
      if (_mpDir != String.Empty)
      {
        mpDirBox.Text = _mpDir;
        CheckMPDir();
      }
      else
      {
        DetectMPDir();
        // Check if detection from registry worked...
        if (mpDirBox.Text.Equals(string.Empty))
        {
          // If not, check if the default is valid
          if (ValidMPDir(strDefaultMPDir))
            mpDirBox.Text = strDefaultMPDir;
          else
            Warning("Sorry, But I wasn't able to detect the MediaPortal installation directory.\n");
        }
        // Double-check detected directory and
        // set variables accordingly
        CheckMPDir();
      }
      if (_logDir != String.Empty)
      {
        logDirBox.Text = _logDir;
        CheckLogDir();
      }
      else
      {
        string strDefaultLogDir = @"C:\MediaPortallogs.zip";
        try
        {
          if (!Directory.Exists(strDefaultLogDir))
            Directory.CreateDirectory(strDefaultLogDir);
          logDirBox.Text = strDefaultLogDir;
          CheckLogDir();
        }
        catch (Exception ex)
        {
          Warning("Cannot use default log destination directory " + strDefaultLogDir + "\nPlease specify one manually. Exception:\n\n" + ex.GetBaseException());
        }
      }
      if (_nick != String.Empty)
      {
        nickBox.Text = _nick;
        CheckNickName();
      }
      setStatus("Checking for required tools");

      CheckPsLogList();

      setStatus("Idle");
      if (_auto)
      {
        if (allowAutoRun)
        {
          PerformAutoRun();
          this.Close();
        }
        else
        {
          Warning("Cannot perform automatic run since some required parameters are missing or invalid");
          Usage();
          this.Close();
        }
      }
      else
      {
        this.Show();
      }
    }

    private void CheckPsLogList()
    {
      if (File.Exists(myPath + @"\psloglist.exe"))
      {
        havePsloglist = true;
        CheckParameters();
      }
      else
      {
        havePsloglist = false;
        Form f = new PsloglistDialog();
        f.ShowDialog();
      }
    }

    public MainForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();

      // Custom component initializations

      // Disable some menu items at first
      this.menuItem4.Enabled = false;
      this.menuItem5.Enabled = false;

      // Configure Event handlers for menu
      this.menuItem2.Click += new System.EventHandler(this.MenuExitClick);
      this.menuItem4.Click += new System.EventHandler(this.MenuPreTestClick);
      this.menuItem5.Click += new System.EventHandler(this.MenuPostTestClick);
      this.menuItem7.Click += new System.EventHandler(this.MenuAboutClick);

      // Configure Event handlers for TextBoxes
      this.nickBox.TextChanged += new System.EventHandler(this.NickChanged);
      this.mpDirBox.Leave += new System.EventHandler(this.MPDirChanged);
      this.logDirBox.Leave += new System.EventHandler(this.LogDirChanged);
      this.Hide();
    }

    public static int Main(string[] args)
    {
      for (int i = 0; i < args.Length; )
      {
        switch (args[i])
        {
          case "-mpdir":
            _mpDir = args[++i];
            break;
          case "-logdir":
            _logDir = args[++i];
            break;
          case "-nick":
            _nick = args[++i];
            break;
          case "-auto":
            _auto = true;
            break;
          default:
            MainForm.Usage();
            return 1;
        }
        i++;
      }
      Form f = new MainForm();
      Application.Run(f);
      return 0;
    }
    public static void Usage()
    {
      string usageText = "\n" +
        "Usage: MPTestTool.exe [-nick <forum nickname>] [-mpdir <directory>] [-logdir <directory>] \n" +
        "\n" +
        "\tnick    : forum nickname to prepend filenames with\n" +
        "\tmpdir : full path to the directory where MediaPortal is installed\n" +
        "\tlogdir : full path to the directory where all output files are gathered\n" +
        "\tauto   : Perform all actions automatically and start MediaPortal in between\n" +
        "\n";
      MessageBox.Show(
                      usageText,
                      "MediaPortal test tool usage",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information
                     );
    }
    void Warning(string text)
    {
      MessageBox.Show(
                      text,
                      "Warning",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Warning
                     );
    }
    void setStatus(string status)
    {
      this.statusBar.Text = string.Format("Status: {0}", status);
    }
    string SanitizeDirectory(string dir)
    {
      if (!dir.EndsWith("\\"))
      {
        dir += "\\";
      }
      return dir;
    }
    bool ValidMPDir(string dir)
    {
      return ((File.Exists(dir + "MediaPortal.exe")) &&
            (File.Exists(dir + "MediaPortal.xml")) &&
            (Directory.Exists(dir + "log")));
    }
    void CheckMPDir()
    {
      string dir = this.mpDirBox.Text;
      string warnTxt = "Invalid MediaPortal directory specified: " + dir;
      if (Directory.Exists(dir))
      {
        dir = SanitizeDirectory(dir);
        if (ValidMPDir(dir))
        {
          validMPDir = true;
          this.mpDirBox.Text = dir;
          CheckParameters();
          return;
        }
        else
        {
          Warning(warnTxt);
        }
      }
      else if (!dir.Equals(string.Empty))
      {
        Warning(warnTxt);
      }
      this.validMPDir = false;
      this.mpDirBox.Text = string.Empty;
      CheckParameters();
    }
    void CheckLogDir()
    {
      string dir = this.logDirBox.Text;
      CheckParameters();
    }
    void CheckNickName()
    {
      string n = this.nickBox.Text;
      if (n.Equals(string.Empty))
      {
        this.validNick = false;
      }
      else if ((HasChar(n, @"/")) ||
               (HasChar(n, @"\")) ||
               (HasChar(n, @":")) ||
               (HasChar(n, @"*")) ||
               (HasChar(n, @"?")) ||
               (HasChar(n, "\"")) ||
               (HasChar(n, @"<")) ||
               (HasChar(n, @">")) ||
               (HasChar(n, @"|")))
      {
        Warning("Invalid characters in nickname!\n" +
                "Don't use one of the following characters:\n\n" +
                "/ \\ : * ? \" < > |\n"
               );
        this.validNick = false;
        this.nickBox.Text = string.Empty;
      }
      else
      {
        this.validNick = true;
      }
      CheckParameters();
    }
    bool HasChar(string str, string chr)
    {
      if (str.IndexOf(chr) != -1)
        return true;
      return false;
    }

    // Checks if all input parameters are valid
    void CheckParameters()
    {
      if (validMPDir && validLogDir)
      {
        this.menuItem4.Enabled = true;
        this.preTestButton.Enabled = true;
        if (validNick)
        {
          this.menuItem5.Enabled = true;
          this.allowAutoRun = true;
        }
        else
        {
          this.menuItem5.Enabled = false;
        }
      }
      else
      {
        this.menuItem4.Enabled = false;
        this.preTestButton.Enabled = false;
        this.menuItem5.Enabled = false;
      }
    }
    // Try to determine MP install directory via registry
    // If that fails, some default is assumed (and tried)
    void DetectMPDir()
    {
      string strMPDir = string.Empty;
      string searchKey = "MediaPortal.exe";
      // try to determine via HKCU (install for current user only)
      try
      {
        string subKey = @"Software\Microsoft\Installer\Assemblies";
        RegistryKey rk = Registry.CurrentUser.OpenSubKey(subKey, false);
        foreach (string keyName in rk.GetSubKeyNames())
        {
          if (keyName.EndsWith(searchKey))
          {
            strMPDir = keyName.Substring(0, keyName.IndexOf(searchKey) - 1);
            strMPDir = strMPDir.Replace(@"|", @"\");
          }
        }
      }
      catch { }
      if (!strMPDir.Equals(string.Empty))
      {
        mpDirBox.Text = strMPDir;
        return;
      }
      // try to determine via HKLM (install for all users)
      try
      {
        string subKey = @"Software\Classes\Installer\Assemblies";
        RegistryKey rk = Registry.LocalMachine.OpenSubKey(subKey, false);
        foreach (string keyName in rk.GetSubKeyNames())
        {
          if (keyName.EndsWith(searchKey))
          {
            strMPDir = keyName.Substring(0, keyName.IndexOf(searchKey) - 1);
            strMPDir = strMPDir.Replace(@"|", @"\");
          }
        }
      }
      catch { }
      mpDirBox.Text = strMPDir;
    }

    #region Windows Forms Designer generated code
    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.step2Group = new System.Windows.Forms.GroupBox();
      this.step2Label4 = new System.Windows.Forms.Label();
      this.step2Label3 = new System.Windows.Forms.Label();
      this.step2Label2 = new System.Windows.Forms.Label();
      this.step2Label1 = new System.Windows.Forms.Label();
      this.postTestButton = new System.Windows.Forms.Button();
      this.settingsGroup = new System.Windows.Forms.GroupBox();
      this.nickBox = new System.Windows.Forms.TextBox();
      this.nickLabel = new System.Windows.Forms.Label();
      this.logDirButton = new System.Windows.Forms.Button();
      this.mpDirButton = new System.Windows.Forms.Button();
      this.logDirBox = new System.Windows.Forms.TextBox();
      this.logDirLabel = new System.Windows.Forms.Label();
      this.mpDirLabel = new System.Windows.Forms.Label();
      this.mpDirBox = new System.Windows.Forms.TextBox();
      this.preTestButton = new System.Windows.Forms.Button();
      this.menuItem4 = new System.Windows.Forms.MenuItem();
      this.statusBar = new System.Windows.Forms.StatusBar();
      this.menuItem6 = new System.Windows.Forms.MenuItem();
      this.menuItem7 = new System.Windows.Forms.MenuItem();
      this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.menuItem3 = new System.Windows.Forms.MenuItem();
      this.menuItem5 = new System.Windows.Forms.MenuItem();
      this.step1Label1 = new System.Windows.Forms.Label();
      this.step1Group = new System.Windows.Forms.GroupBox();
      this.step1Label3 = new System.Windows.Forms.Label();
      this.step1Label2 = new System.Windows.Forms.Label();
      this.step2Group.SuspendLayout();
      this.settingsGroup.SuspendLayout();
      this.step1Group.SuspendLayout();
      this.SuspendLayout();
      // 
      // step2Group
      // 
      this.step2Group.Controls.Add(this.step2Label4);
      this.step2Group.Controls.Add(this.step2Label3);
      this.step2Group.Controls.Add(this.step2Label2);
      this.step2Group.Controls.Add(this.step2Label1);
      this.step2Group.Controls.Add(this.postTestButton);
      this.step2Group.Location = new System.Drawing.Point(8, 267);
      this.step2Group.Name = "step2Group";
      this.step2Group.Size = new System.Drawing.Size(408, 90);
      this.step2Group.TabIndex = 3;
      this.step2Group.TabStop = false;
      this.step2Group.Text = "Step 2 (after testing)";
      // 
      // step2Label4
      // 
      this.step2Label4.Location = new System.Drawing.Point(8, 67);
      this.step2Label4.Name = "step2Label4";
      this.step2Label4.Size = new System.Drawing.Size(296, 15);
      this.step2Label4.TabIndex = 4;
      this.step2Label4.Text = "- create ZIP file with all gathered information";
      // 
      // step2Label3
      // 
      this.step2Label3.Location = new System.Drawing.Point(8, 52);
      this.step2Label3.Name = "step2Label3";
      this.step2Label3.Size = new System.Drawing.Size(296, 15);
      this.step2Label3.TabIndex = 3;
      this.step2Label3.Text = "- gather system information (dxdiag / Windows hotfixes)";
      // 
      // step2Label2
      // 
      this.step2Label2.Location = new System.Drawing.Point(8, 37);
      this.step2Label2.Name = "step2Label2";
      this.step2Label2.Size = new System.Drawing.Size(296, 15);
      this.step2Label2.TabIndex = 2;
      this.step2Label2.Text = "- gather all events from System / Application logbooks";
      // 
      // step2Label1
      // 
      this.step2Label1.Location = new System.Drawing.Point(8, 22);
      this.step2Label1.Name = "step2Label1";
      this.step2Label1.Size = new System.Drawing.Size(296, 15);
      this.step2Label1.TabIndex = 1;
      this.step2Label1.Text = "- gather logfiles generated by MediaPortal";
      // 
      // postTestButton
      // 
      this.postTestButton.Location = new System.Drawing.Point(312, 15);
      this.postTestButton.Name = "postTestButton";
      this.postTestButton.Size = new System.Drawing.Size(88, 59);
      this.postTestButton.TabIndex = 6;
      this.postTestButton.Text = "Perform actions necessary after testing";
      this.postTestButton.Click += new System.EventHandler(this.PostTestButtonClick);
      // 
      // settingsGroup
      // 
      this.settingsGroup.Controls.Add(this.nickBox);
      this.settingsGroup.Controls.Add(this.nickLabel);
      this.settingsGroup.Controls.Add(this.logDirButton);
      this.settingsGroup.Controls.Add(this.mpDirButton);
      this.settingsGroup.Controls.Add(this.logDirBox);
      this.settingsGroup.Controls.Add(this.logDirLabel);
      this.settingsGroup.Controls.Add(this.mpDirLabel);
      this.settingsGroup.Controls.Add(this.mpDirBox);
      this.settingsGroup.Location = new System.Drawing.Point(8, 22);
      this.settingsGroup.Name = "settingsGroup";
      this.settingsGroup.Size = new System.Drawing.Size(408, 149);
      this.settingsGroup.TabIndex = 1;
      this.settingsGroup.TabStop = false;
      this.settingsGroup.Text = "Settings";
      // 
      // nickBox
      // 
      this.nickBox.Location = new System.Drawing.Point(8, 119);
      this.nickBox.Name = "nickBox";
      this.nickBox.Size = new System.Drawing.Size(88, 20);
      this.nickBox.TabIndex = 4;
      // 
      // nickLabel
      // 
      this.nickLabel.Location = new System.Drawing.Point(8, 104);
      this.nickLabel.Name = "nickLabel";
      this.nickLabel.Size = new System.Drawing.Size(96, 15);
      this.nickLabel.TabIndex = 6;
      this.nickLabel.Text = "Forum nickname:";
      // 
      // logDirButton
      // 
      this.logDirButton.Location = new System.Drawing.Point(336, 74);
      this.logDirButton.Name = "logDirButton";
      this.logDirButton.Size = new System.Drawing.Size(64, 23);
      this.logDirButton.TabIndex = 3;
      this.logDirButton.Text = "Browse";
      this.logDirButton.Click += new System.EventHandler(this.LogDirButtonClick);
      // 
      // mpDirButton
      // 
      this.mpDirButton.Location = new System.Drawing.Point(336, 30);
      this.mpDirButton.Name = "mpDirButton";
      this.mpDirButton.Size = new System.Drawing.Size(64, 22);
      this.mpDirButton.TabIndex = 1;
      this.mpDirButton.Text = "Browse";
      this.mpDirButton.Click += new System.EventHandler(this.MpDirButtonClick);
      // 
      // logDirBox
      // 
      this.logDirBox.Location = new System.Drawing.Point(8, 74);
      this.logDirBox.Name = "logDirBox";
      this.logDirBox.Size = new System.Drawing.Size(320, 20);
      this.logDirBox.TabIndex = 2;
      // 
      // logDirLabel
      // 
      this.logDirLabel.Location = new System.Drawing.Point(8, 59);
      this.logDirLabel.Name = "logDirLabel";
      this.logDirLabel.Size = new System.Drawing.Size(152, 15);
      this.logDirLabel.TabIndex = 2;
      this.logDirLabel.Text = "Resulting ZIP of logs";
      // 
      // mpDirLabel
      // 
      this.mpDirLabel.Location = new System.Drawing.Point(8, 15);
      this.mpDirLabel.Name = "mpDirLabel";
      this.mpDirLabel.Size = new System.Drawing.Size(176, 15);
      this.mpDirLabel.TabIndex = 1;
      this.mpDirLabel.Text = "MediaPortal installation directory:";
      // 
      // mpDirBox
      // 
      this.mpDirBox.Location = new System.Drawing.Point(8, 30);
      this.mpDirBox.Name = "mpDirBox";
      this.mpDirBox.Size = new System.Drawing.Size(320, 20);
      this.mpDirBox.TabIndex = 0;
      // 
      // preTestButton
      // 
      this.preTestButton.Enabled = false;
      this.preTestButton.Location = new System.Drawing.Point(312, 15);
      this.preTestButton.Name = "preTestButton";
      this.preTestButton.Size = new System.Drawing.Size(88, 59);
      this.preTestButton.TabIndex = 5;
      this.preTestButton.Text = "Perform actions necessary before testing";
      this.preTestButton.Click += new System.EventHandler(this.PreTestButtonClick);
      // 
      // menuItem4
      // 
      this.menuItem4.Index = 0;
      this.menuItem4.Text = "Perform pre-test actions";
      // 
      // statusBar
      // 
      this.statusBar.Location = new System.Drawing.Point(0, 376);
      this.statusBar.Name = "statusBar";
      this.statusBar.Size = new System.Drawing.Size(422, 20);
      this.statusBar.TabIndex = 0;
      this.statusBar.Text = "Status: Idle";
      // 
      // menuItem6
      // 
      this.menuItem6.Index = 2;
      this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem7});
      this.menuItem6.Text = "Help";
      // 
      // menuItem7
      // 
      this.menuItem7.Index = 0;
      this.menuItem7.Text = "About";
      // 
      // mainMenu
      // 
      this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem3,
            this.menuItem6});
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 0;
      this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2});
      this.menuItem1.Text = "File";
      // 
      // menuItem2
      // 
      this.menuItem2.Index = 0;
      this.menuItem2.Text = "Exit";
      // 
      // menuItem3
      // 
      this.menuItem3.Index = 1;
      this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4,
            this.menuItem5});
      this.menuItem3.Text = "Action";
      // 
      // menuItem5
      // 
      this.menuItem5.Index = 1;
      this.menuItem5.Text = "Perform post-test actions";
      // 
      // step1Label1
      // 
      this.step1Label1.Location = new System.Drawing.Point(8, 22);
      this.step1Label1.Name = "step1Label1";
      this.step1Label1.Size = new System.Drawing.Size(304, 15);
      this.step1Label1.TabIndex = 1;
      this.step1Label1.Text = "- remove all files in the MediaPortal installation log directory";
      // 
      // step1Group
      // 
      this.step1Group.AccessibleDescription = "";
      this.step1Group.AccessibleName = "Step 1 (before testing)";
      this.step1Group.BackColor = System.Drawing.SystemColors.Control;
      this.step1Group.Controls.Add(this.step1Label3);
      this.step1Group.Controls.Add(this.step1Label2);
      this.step1Group.Controls.Add(this.step1Label1);
      this.step1Group.Controls.Add(this.preTestButton);
      this.step1Group.Location = new System.Drawing.Point(8, 178);
      this.step1Group.Name = "step1Group";
      this.step1Group.Size = new System.Drawing.Size(408, 82);
      this.step1Group.TabIndex = 2;
      this.step1Group.TabStop = false;
      this.step1Group.Text = "Step 1 (before testing)";
      // 
      // step1Label3
      // 
      this.step1Label3.Location = new System.Drawing.Point(8, 52);
      this.step1Label3.Name = "step1Label3";
      this.step1Label3.Size = new System.Drawing.Size(272, 15);
      this.step1Label3.TabIndex = 3;
      this.step1Label3.Text = "- remove all files in the logfile destination directory";
      // 
      // step1Label2
      // 
      this.step1Label2.Location = new System.Drawing.Point(8, 37);
      this.step1Label2.Name = "step1Label2";
      this.step1Label2.Size = new System.Drawing.Size(304, 15);
      this.step1Label2.TabIndex = 2;
      this.step1Label2.Text = "- clear all events in the System / Application event logbooks";
      // 
      // MainForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(422, 396);
      this.Controls.Add(this.step2Group);
      this.Controls.Add(this.step1Group);
      this.Controls.Add(this.settingsGroup);
      this.Controls.Add(this.statusBar);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(430, 430);
      this.Menu = this.mainMenu;
      this.MinimumSize = new System.Drawing.Size(430, 430);
      this.Name = "MainForm";
      this.Text = "MediaPortal Test Tool";
      this.step2Group.ResumeLayout(false);
      this.settingsGroup.ResumeLayout(false);
      this.settingsGroup.PerformLayout();
      this.step1Group.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    // Handle directory browse requests
    // true: directory changed
    // false: directory unchanged
    bool ChangeDirectory(TextBox tb)
    {

      SaveFileDialog saveDialog = new SaveFileDialog();
      saveDialog.OverwritePrompt = true;
      saveDialog.DefaultExt = ".zip";
      saveDialog.Title = "Choose ZIP file to create";

      bool changed = false;
      saveDialog.FileName = tb.Text;
      DialogResult dr = saveDialog.ShowDialog();
      if (dr == DialogResult.OK)
      {
        tb.Text = saveDialog.FileName;
        changed = true;
      }
      return changed;
    }
    // Handle browse-click for MP directory
    void MpDirButtonClick(object sender, EventArgs e)
    {
      if (ChangeDirectory(mpDirBox))
      {
        CheckMPDir();
      }
    }
    // Handle browse-click for log dst. directory
    void LogDirButtonClick(object sender, EventArgs e)
    {
      if (ChangeDirectory(logDirBox))
      {
        CheckLogDir();
      }
    }

    // Handle main menu events
    void MenuExitClick(object sender, EventArgs e)
    {
      this.Close();
    }
    void MenuPreTestClick(object sender, EventArgs e)
    {
      PerformPreTestActions();
    }
    void MenuPostTestClick(object sender, EventArgs e)
    {
      PerformPostTestActions();
    }
    void MenuAboutClick(object sender, EventArgs e)
    {
      Form f = new MPTestTool.AboutForm();
      DialogResult dr = f.ShowDialog();
    }
    void MPDirChanged(object sender, EventArgs e)
    {
      CheckMPDir();
    }
    void LogDirChanged(object sender, EventArgs e)
    {
      CheckLogDir();
    }
    void NickChanged(object sender, EventArgs e)
    {
      CheckNickName();
    }
    void PreTestButtonClick(object sender, System.EventArgs e)
    {
      PerformPreTestActions();
    }
    void PerformPreTestActions()
    {
      setStatus("Busy performing pre-test actions...");
      PreTestActions pta = new PreTestActions(
                                              mpDirBox.Text + "log",
                                              logDirBox.Text
                                             );
      pta.Show();
      if (pta.PerformActions())
      {
        setStatus("Done performing pre-test actions.");
      }
      else
      {
        setStatus("Pre-test actions were aborted.");
      }
    }

    void PostTestButtonClick(object sender, System.EventArgs e)
    {
      PerformPostTestActions();
    }
    void PerformPostTestActions()
    {
      setStatus("Busy performing post-test actions...");
      PostTestActions pta = new PostTestActions(
                                              myPath,
                                              mpDirBox.Text,
                                              logDirBox.Text,
                                              nickBox.Text,
                                              havePsloglist
                                             );
      pta.Show();
      if (pta.PerformActions())
      {
        setStatus("Done performing post-test actions.");
      }
      else
      {
        setStatus("Post-test actions were aborted.");
      }
    }
    void PerformAutoRun()
    {
      // Do pre-test stuff
      PreTestActions preta = new PreTestActions(
                                              mpDirBox.Text + "log",
                                              logDirBox.Text
                                             );
      preta.Show();
      if (!preta.PerformActions())
      {
        Warning("Pre-test actions procedure canceled!");
      }
      preta.Hide();
      preta = null;
      // Start MediaPortal
      Process pr = new Process();
      pr.StartInfo.WorkingDirectory = this.mpDirBox.Text;
      pr.StartInfo.FileName = "mediaportal.exe";
      pr.Start();
      pr.WaitForExit();
      // Do post-test stuff
      PostTestActions postta = new PostTestActions(
                                              myPath,
                                              mpDirBox.Text,
                                              logDirBox.Text,
                                              nickBox.Text,
                                              havePsloglist
                                             );
      postta.Show();
      if (!postta.PerformActions())
      {
        Warning("Post-test actions procedure canceled!");
      }
      postta.Hide();
      postta = null;
    }
  }
}
