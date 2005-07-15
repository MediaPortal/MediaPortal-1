using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
  public class Remote : MediaPortal.Configuration.SectionSettings
  {
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMCE;
    private System.Windows.Forms.PictureBox pictureBoxUSA;
    private System.Windows.Forms.RadioButton radioButtonUSA;
    private System.Windows.Forms.RadioButton radioButtonEurope;
    private System.Windows.Forms.GroupBox groupBoxSettings;
    private System.Windows.Forms.Label labelDelay;
    public System.Windows.Forms.TrackBar trackBarDelay;
    private System.Windows.Forms.CheckBox checkBoxAllowExternal;
    private System.Windows.Forms.CheckBox checkBoxKeepControl;
    private System.Windows.Forms.CheckBox checkBoxVerboseLog;
    private System.Windows.Forms.Button buttonDefault;
    private System.Windows.Forms.CheckBox checkBoxHCW;
    private System.Windows.Forms.Label infoDriverStatus;
    private System.Windows.Forms.GroupBox groupBoxInformation;
    private System.Windows.Forms.TabPage tabPageMCE;
    private System.Windows.Forms.TabPage tabPageHCW;
    private System.Windows.Forms.Label label2sec;
    private System.Windows.Forms.Label label0sec;
    private System.Windows.Forms.Button btnMapping;
    private MediaPortal.Configuration.Sections.FireDTVRemote fireDTVRemote;
    private System.Windows.Forms.TabControl tabControlRemotes;
    private System.Windows.Forms.TabPage tabPageFireDTV;
    private System.Windows.Forms.PictureBox pictureBoxEU;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.ComponentModel.IContainer components = null;

    public struct RAWINPUTDEVICE 
    {
      public ushort usUsagePage;
      public ushort usUsage;
      public uint dwFlags;
      public IntPtr hwndTarget;
    }

    [DllImport("User32.dll", EntryPoint="RegisterRawInputDevices", SetLastError=true)]
    public extern static bool RegisterRawInputDevices(
      [In] RAWINPUTDEVICE[] pRawInputDevices,
      [In] uint uiNumDevices,
      [In] uint cbSize);


    public Remote() : this("Remote")
    {
    }

    public Remote(string name) : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    static public bool IsMceRemoteInstalled(IntPtr hwnd)
    {
      RAWINPUTDEVICE[] rid1 = new RAWINPUTDEVICE[1];

      rid1[0].usUsagePage = 0xFFBC;
      rid1[0].usUsage = 0x88;
      rid1[0].dwFlags = 0;
      rid1[0].hwndTarget = hwnd;
      bool Success = RegisterRawInputDevices(rid1, (uint)rid1.Length, (uint)Marshal.SizeOf(rid1[0]));
      if (Success) 
      {
        return true;
      }

      rid1[0].usUsagePage = 0x0C;
      rid1[0].usUsage = 0x01;
      rid1[0].dwFlags = 0;
      rid1[0].hwndTarget = hwnd;
      Success = RegisterRawInputDevices(rid1, (uint)rid1.Length, (uint)Marshal.SizeOf(rid1[0]));
      if (Success) 
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      string errNotInstalled       = "The Hauppauge IR components have not been found.\n\nYou should download and install the latest Hauppauge IR drivers.";
      string errOutOfDate          = "The driver components are not up to date.\n\nYou should update your Hauppauge IR drivers to the current version.";
      string errMissingExe         = "IR application not found. You might want to use it to control external applications.\n\nReinstall the Hauppauge IR drivers to fix this problem.";

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        checkBoxMCE.Checked               = xmlreader.GetValueAsBool("remote", "mce2005", false);
        radioButtonUSA.Checked            = xmlreader.GetValueAsBool("remote", "USAModel", false);
        checkBoxHCW.Checked               = xmlreader.GetValueAsBool("remote", "HCW", false);
        checkBoxAllowExternal.Checked     = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        checkBoxKeepControl.Checked       = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        checkBoxVerboseLog.Checked        = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        trackBarDelay.Value               = xmlreader.GetValueAsInt("remote", "HCWDelay", 0);
      }

      radioButtonEurope.Checked = !radioButtonUSA.Checked;

      if (checkBoxMCE.Checked)
      {
        radioButtonEurope.Enabled = true;
        radioButtonUSA.Enabled = true;
      }
      else
      {
        radioButtonEurope.Enabled = false;
        radioButtonUSA.Enabled = false;
      }

      if (radioButtonUSA.Checked)
      {
        pictureBoxUSA.Visible = true;
        pictureBoxEU.Visible = false;
      }
      else
      {
        pictureBoxEU.Visible = true;
        pictureBoxUSA.Visible = false;
      }
      
      if (checkBoxAllowExternal.Checked)
        checkBoxKeepControl.Enabled = true;
      else
        checkBoxKeepControl.Enabled = false;
      
      if (checkBoxHCW.Checked)
        groupBoxSettings.Enabled = true;
      else
        groupBoxSettings.Enabled = false;

      string exePath = HCWRemote.GetHCWPath();
      string dllPath = HCWRemote.GetDllPath();
      
      if (File.Exists(exePath + "Ir.exe"))
      {
        FileVersionInfo exeVersionInfo = FileVersionInfo.GetVersionInfo(exePath + "Ir.exe");
        if (exeVersionInfo.FileVersion.CompareTo("2.45.22350") < 0)
          infoDriverStatus.Text = errOutOfDate;
      }
      else
        infoDriverStatus.Text = errMissingExe;

      if (File.Exists(dllPath + "irremote.DLL"))
      {
        FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath + "irremote.DLL");
        if (dllVersionInfo.FileVersion.CompareTo("2.45.22350") < 0)
          infoDriverStatus.Text = errOutOfDate;
      }
      else
      {
        infoDriverStatus.Text = errNotInstalled;
        checkBoxHCW.Enabled = false;
        groupBoxSettings.Enabled = false;
      }

      fireDTVRemote.LoadSettings();
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("remote", "mce2005", checkBoxMCE.Checked);
        xmlwriter.SetValueAsBool("remote", "USAModel", radioButtonUSA.Checked);
        xmlwriter.SetValueAsBool("remote", "HCW", checkBoxHCW.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWAllowExternal", checkBoxAllowExternal.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWKeepControl", checkBoxKeepControl.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWVerboseLog", checkBoxVerboseLog.Checked);
        xmlwriter.SetValue("remote", "HCWDelay", trackBarDelay.Value);
      }

      fireDTVRemote.SaveSettings();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Remote));
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.pictureBoxUSA = new System.Windows.Forms.PictureBox();
      this.checkBoxMCE = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlRemotes = new System.Windows.Forms.TabControl();
      this.tabPageMCE = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.radioButtonEurope = new System.Windows.Forms.RadioButton();
      this.radioButtonUSA = new System.Windows.Forms.RadioButton();
      this.pictureBoxEU = new System.Windows.Forms.PictureBox();
      this.tabPageHCW = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.btnMapping = new System.Windows.Forms.Button();
      this.checkBoxHCW = new System.Windows.Forms.CheckBox();
      this.groupBoxInformation = new System.Windows.Forms.GroupBox();
      this.infoDriverStatus = new System.Windows.Forms.Label();
      this.groupBoxSettings = new System.Windows.Forms.GroupBox();
      this.label2sec = new System.Windows.Forms.Label();
      this.label0sec = new System.Windows.Forms.Label();
      this.labelDelay = new System.Windows.Forms.Label();
      this.trackBarDelay = new System.Windows.Forms.TrackBar();
      this.checkBoxAllowExternal = new System.Windows.Forms.CheckBox();
      this.checkBoxKeepControl = new System.Windows.Forms.CheckBox();
      this.checkBoxVerboseLog = new System.Windows.Forms.CheckBox();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.tabPageFireDTV = new System.Windows.Forms.TabPage();
      this.fireDTVRemote = new MediaPortal.Configuration.Sections.FireDTVRemote();
      this.tabControlRemotes.SuspendLayout();
      this.tabPageMCE.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabPageHCW.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBoxInformation.SuspendLayout();
      this.groupBoxSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarDelay)).BeginInit();
      this.tabPageFireDTV.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBoxUSA
      // 
      this.pictureBoxUSA.BackColor = System.Drawing.Color.Transparent;
      this.pictureBoxUSA.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxUSA.Image")));
      this.pictureBoxUSA.Location = new System.Drawing.Point(208, 24);
      this.pictureBoxUSA.Name = "pictureBoxUSA";
      this.pictureBoxUSA.Size = new System.Drawing.Size(100, 310);
      this.pictureBoxUSA.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxUSA.TabIndex = 3;
      this.pictureBoxUSA.TabStop = false;
      // 
      // checkBoxMCE
      // 
      this.checkBoxMCE.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxMCE.Location = new System.Drawing.Point(16, 24);
      this.checkBoxMCE.Name = "checkBoxMCE";
      this.checkBoxMCE.Size = new System.Drawing.Size(152, 16);
      this.checkBoxMCE.TabIndex = 0;
      this.checkBoxMCE.Text = "Use Microsoft MCE remote";
      this.checkBoxMCE.CheckedChanged += new System.EventHandler(this.checkBoxMCE_CheckedChanged);
      // 
      // tabControlRemotes
      // 
      this.tabControlRemotes.Controls.Add(this.tabPageMCE);
      this.tabControlRemotes.Controls.Add(this.tabPageHCW);
      this.tabControlRemotes.Controls.Add(this.tabPageFireDTV);
      this.tabControlRemotes.Location = new System.Drawing.Point(0, 0);
      this.tabControlRemotes.Name = "tabControlRemotes";
      this.tabControlRemotes.SelectedIndex = 0;
      this.tabControlRemotes.Size = new System.Drawing.Size(472, 408);
      this.tabControlRemotes.TabIndex = 5;
      // 
      // tabPageMCE
      // 
      this.tabPageMCE.Controls.Add(this.groupBox1);
      this.tabPageMCE.Location = new System.Drawing.Point(4, 22);
      this.tabPageMCE.Name = "tabPageMCE";
      this.tabPageMCE.Size = new System.Drawing.Size(464, 382);
      this.tabPageMCE.TabIndex = 0;
      this.tabPageMCE.Text = "Microsoft MCE Remote";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.checkBoxMCE);
      this.groupBox1.Controls.Add(this.radioButtonEurope);
      this.groupBox1.Controls.Add(this.radioButtonUSA);
      this.groupBox1.Controls.Add(this.pictureBoxUSA);
      this.groupBox1.Controls.Add(this.pictureBoxEU);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(12, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 352);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      // 
      // radioButtonEurope
      // 
      this.radioButtonEurope.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonEurope.Location = new System.Drawing.Point(32, 64);
      this.radioButtonEurope.Name = "radioButtonEurope";
      this.radioButtonEurope.Size = new System.Drawing.Size(104, 16);
      this.radioButtonEurope.TabIndex = 6;
      this.radioButtonEurope.Text = "European version";
      this.radioButtonEurope.CheckedChanged += new System.EventHandler(this.radioButtonEurope_CheckedChanged);
      // 
      // radioButtonUSA
      // 
      this.radioButtonUSA.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonUSA.Location = new System.Drawing.Point(32, 48);
      this.radioButtonUSA.Name = "radioButtonUSA";
      this.radioButtonUSA.Size = new System.Drawing.Size(104, 16);
      this.radioButtonUSA.TabIndex = 5;
      this.radioButtonUSA.Text = "USA version";
      this.radioButtonUSA.CheckedChanged += new System.EventHandler(this.radioButtonUSA_CheckedChanged);
      // 
      // pictureBoxEU
      // 
      this.pictureBoxEU.BackColor = System.Drawing.Color.Transparent;
      this.pictureBoxEU.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxEU.Image")));
      this.pictureBoxEU.Location = new System.Drawing.Point(208, 24);
      this.pictureBoxEU.Name = "pictureBoxEU";
      this.pictureBoxEU.Size = new System.Drawing.Size(100, 310);
      this.pictureBoxEU.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxEU.TabIndex = 4;
      this.pictureBoxEU.TabStop = false;
      // 
      // tabPageHCW
      // 
      this.tabPageHCW.Controls.Add(this.groupBox2);
      this.tabPageHCW.Controls.Add(this.groupBoxInformation);
      this.tabPageHCW.Controls.Add(this.groupBoxSettings);
      this.tabPageHCW.Location = new System.Drawing.Point(4, 22);
      this.tabPageHCW.Name = "tabPageHCW";
      this.tabPageHCW.Size = new System.Drawing.Size(464, 382);
      this.tabPageHCW.TabIndex = 1;
      this.tabPageHCW.Text = "Hauppauge Remote";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.btnMapping);
      this.groupBox2.Controls.Add(this.checkBoxHCW);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(12, 8);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(440, 56);
      this.groupBox2.TabIndex = 14;
      this.groupBox2.TabStop = false;
      // 
      // btnMapping
      // 
      this.btnMapping.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnMapping.Location = new System.Drawing.Point(344, 20);
      this.btnMapping.Name = "btnMapping";
      this.btnMapping.Size = new System.Drawing.Size(72, 22);
      this.btnMapping.TabIndex = 13;
      this.btnMapping.Text = "Mapping";
      this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
      // 
      // checkBoxHCW
      // 
      this.checkBoxHCW.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxHCW.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHCW.Name = "checkBoxHCW";
      this.checkBoxHCW.Size = new System.Drawing.Size(144, 16);
      this.checkBoxHCW.TabIndex = 10;
      this.checkBoxHCW.Text = "Use Hauppauge remote";
      this.checkBoxHCW.CheckedChanged += new System.EventHandler(this.checkBoxHCW_CheckedChanged);
      // 
      // groupBoxInformation
      // 
      this.groupBoxInformation.Controls.Add(this.infoDriverStatus);
      this.groupBoxInformation.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxInformation.Location = new System.Drawing.Point(12, 240);
      this.groupBoxInformation.Name = "groupBoxInformation";
      this.groupBoxInformation.Size = new System.Drawing.Size(440, 88);
      this.groupBoxInformation.TabIndex = 12;
      this.groupBoxInformation.TabStop = false;
      this.groupBoxInformation.Text = "Information";
      // 
      // infoDriverStatus
      // 
      this.infoDriverStatus.ForeColor = System.Drawing.SystemColors.ControlText;
      this.infoDriverStatus.Location = new System.Drawing.Point(12, 16);
      this.infoDriverStatus.Name = "infoDriverStatus";
      this.infoDriverStatus.Size = new System.Drawing.Size(414, 64);
      this.infoDriverStatus.TabIndex = 11;
      this.infoDriverStatus.Text = "No problems found.";
      this.infoDriverStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Controls.Add(this.label2sec);
      this.groupBoxSettings.Controls.Add(this.label0sec);
      this.groupBoxSettings.Controls.Add(this.labelDelay);
      this.groupBoxSettings.Controls.Add(this.trackBarDelay);
      this.groupBoxSettings.Controls.Add(this.checkBoxAllowExternal);
      this.groupBoxSettings.Controls.Add(this.checkBoxKeepControl);
      this.groupBoxSettings.Controls.Add(this.checkBoxVerboseLog);
      this.groupBoxSettings.Controls.Add(this.buttonDefault);
      this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxSettings.Location = new System.Drawing.Point(12, 72);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(440, 160);
      this.groupBoxSettings.TabIndex = 7;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // label2sec
      // 
      this.label2sec.BackColor = System.Drawing.SystemColors.Control;
      this.label2sec.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2sec.Location = new System.Drawing.Point(224, 136);
      this.label2sec.Name = "label2sec";
      this.label2sec.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.label2sec.Size = new System.Drawing.Size(40, 16);
      this.label2sec.TabIndex = 12;
      this.label2sec.Text = "2 sec.";
      // 
      // label0sec
      // 
      this.label0sec.BackColor = System.Drawing.SystemColors.Control;
      this.label0sec.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label0sec.Location = new System.Drawing.Point(112, 136);
      this.label0sec.Name = "label0sec";
      this.label0sec.Size = new System.Drawing.Size(40, 16);
      this.label0sec.TabIndex = 11;
      this.label0sec.Text = "0 sec.";
      // 
      // labelDelay
      // 
      this.labelDelay.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.labelDelay.Location = new System.Drawing.Point(12, 120);
      this.labelDelay.Name = "labelDelay";
      this.labelDelay.Size = new System.Drawing.Size(96, 23);
      this.labelDelay.TabIndex = 10;
      this.labelDelay.Text = "Repeat-delay:";
      // 
      // trackBarDelay
      // 
      this.trackBarDelay.LargeChange = 100;
      this.trackBarDelay.Location = new System.Drawing.Point(112, 112);
      this.trackBarDelay.Maximum = 2000;
      this.trackBarDelay.Name = "trackBarDelay";
      this.trackBarDelay.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.trackBarDelay.Size = new System.Drawing.Size(152, 45);
      this.trackBarDelay.SmallChange = 100;
      this.trackBarDelay.TabIndex = 3;
      this.trackBarDelay.TickFrequency = 1000;
      this.trackBarDelay.TickStyle = System.Windows.Forms.TickStyle.None;
      // 
      // checkBoxAllowExternal
      // 
      this.checkBoxAllowExternal.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxAllowExternal.Location = new System.Drawing.Point(16, 24);
      this.checkBoxAllowExternal.Name = "checkBoxAllowExternal";
      this.checkBoxAllowExternal.Size = new System.Drawing.Size(240, 24);
      this.checkBoxAllowExternal.TabIndex = 0;
      this.checkBoxAllowExternal.Text = "External processes may use the remote control";
      this.checkBoxAllowExternal.CheckedChanged += new System.EventHandler(this.checkBoxAllowExternal_CheckedChanged);
      // 
      // checkBoxKeepControl
      // 
      this.checkBoxKeepControl.Enabled = false;
      this.checkBoxKeepControl.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxKeepControl.Location = new System.Drawing.Point(32, 48);
      this.checkBoxKeepControl.Name = "checkBoxKeepControl";
      this.checkBoxKeepControl.Size = new System.Drawing.Size(192, 24);
      this.checkBoxKeepControl.TabIndex = 1;
      this.checkBoxKeepControl.Text = "Keep control when MP looses focus";
      // 
      // checkBoxVerboseLog
      // 
      this.checkBoxVerboseLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxVerboseLog.Location = new System.Drawing.Point(16, 72);
      this.checkBoxVerboseLog.Name = "checkBoxVerboseLog";
      this.checkBoxVerboseLog.Size = new System.Drawing.Size(108, 24);
      this.checkBoxVerboseLog.TabIndex = 2;
      this.checkBoxVerboseLog.Text = "Extended Logging";
      // 
      // buttonDefault
      // 
      this.buttonDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonDefault.Location = new System.Drawing.Point(344, 120);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(72, 22);
      this.buttonDefault.TabIndex = 9;
      this.buttonDefault.Text = "&Reset";
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // tabPageFireDTV
      // 
      this.tabPageFireDTV.Controls.Add(this.fireDTVRemote);
      this.tabPageFireDTV.Location = new System.Drawing.Point(4, 22);
      this.tabPageFireDTV.Name = "tabPageFireDTV";
      this.tabPageFireDTV.Size = new System.Drawing.Size(464, 382);
      this.tabPageFireDTV.TabIndex = 2;
      this.tabPageFireDTV.Text = "FireDTV Remote";
      // 
      // fireDTVRemote
      // 
      this.fireDTVRemote.AutoScroll = true;
      this.fireDTVRemote.Location = new System.Drawing.Point(0, 8);
      this.fireDTVRemote.Name = "fireDTVRemote";
      this.fireDTVRemote.Size = new System.Drawing.Size(520, 368);
      this.fireDTVRemote.TabIndex = 0;
      // 
      // Remote
      // 
      this.Controls.Add(this.tabControlRemotes);
      this.Name = "Remote";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControlRemotes.ResumeLayout(false);
      this.tabPageMCE.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.tabPageHCW.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBoxInformation.ResumeLayout(false);
      this.groupBoxSettings.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarDelay)).EndInit();
      this.tabPageFireDTV.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    #region Form control commands
    //
    // USA version
    //
    private void radioButtonUSA_CheckedChanged(object sender, System.EventArgs e)
    {
      pictureBoxUSA.Visible = radioButtonUSA.Checked;
      pictureBoxEU.Visible = !radioButtonUSA.Checked;
      radioButtonEurope.Checked = !radioButtonUSA.Checked;
    }

    //
    // European version
    //
    private void radioButtonEurope_CheckedChanged(object sender, System.EventArgs e)
    {
      pictureBoxUSA.Visible = !radioButtonEurope.Checked;
      pictureBoxEU.Visible = radioButtonEurope.Checked;
      radioButtonUSA.Checked = !radioButtonEurope.Checked;
    }

    //
    // Use Microsoft MCE remote
    //
    private void checkBoxMCE_CheckedChanged(object sender, System.EventArgs e)
    {
      radioButtonEurope.Enabled = checkBoxMCE.Checked;
      radioButtonUSA.Enabled = checkBoxMCE.Checked;
    }

    //
    // External processes may use the remote control
    //
    private void checkBoxAllowExternal_CheckedChanged(object sender, System.EventArgs e)
    {
      checkBoxKeepControl.Enabled = checkBoxAllowExternal.Checked;
    }

    //
    // Use Hauppauge remote
    //
    private void checkBoxHCW_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxSettings.Enabled = checkBoxHCW.Checked;
    }

    //
    // Reset to default
    //    
    private void buttonDefault_Click(object sender, System.EventArgs e)
    {
      checkBoxAllowExternal.Checked = false;
      checkBoxKeepControl.Checked = false;
      checkBoxVerboseLog.Checked = false;
      trackBarDelay.Value = 0;
//      comboBoxPowerButton.SelectedIndex = 1;    
    }
    #endregion

    private void btnMapping_Click(object sender, System.EventArgs e)
    {
      HCWMappingForm dlg = new HCWMappingForm("Hauppauge HCW");
      dlg.ShowDialog(this);
    }
  }
}
