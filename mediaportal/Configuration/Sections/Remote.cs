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
using Microsoft.Win32;
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
    private GroupBox groupBox3;
    private HScrollBar hScrollBarButtonRelease;
    private HScrollBar hScrollBarRepeatSpeed;
    private Label label4;
    private Label label5;
    private Label label6;
    private HScrollBar hScrollBarRepeatFilter;
    private Label label1;
    private Label label2;
    private Label label3;
    private CheckBox checkBoxFilterDoubleKlicks;
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
		try
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
		}
		catch(Exception)
		{ }
		
		return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      string errNotInstalled       = "The Hauppauge IR components have not been found.\n\nYou should download and install the latest Hauppauge IR drivers and use XPSP2.";
      string errOutOfDate          = "The driver components are not up to date.\n\nYou should update your Hauppauge IR drivers to the current version.";
      string errMissingExe         = "IR application not found. You might want to use it to control external applications.\n\nReinstall the Hauppauge IR drivers to fix this problem.";

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        checkBoxMCE.Checked                = xmlreader.GetValueAsBool("remote", "mce2005", false);
        radioButtonUSA.Checked             = xmlreader.GetValueAsBool("remote", "USAModel", false);
        checkBoxHCW.Checked                = xmlreader.GetValueAsBool("remote", "HCW", false);
        checkBoxAllowExternal.Checked      = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        checkBoxKeepControl.Checked        = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        checkBoxVerboseLog.Checked         = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        hScrollBarButtonRelease.Value      = xmlreader.GetValueAsInt ("remote", "HCWButtonRelease", 100);
        hScrollBarRepeatFilter.Value       = xmlreader.GetValueAsInt ("remote", "HCWRepeatFilter", 2);
        hScrollBarRepeatSpeed.Value        = xmlreader.GetValueAsInt ("remote", "HCWRepeatSpeed", 1);
        checkBoxFilterDoubleKlicks.Checked = xmlreader.GetValueAsBool("remote", "HCWFilterDoubleKlicks", false);
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

      string exePath = GetHCWPath();
      string dllPath = GetDllPath();
      
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
        xmlwriter.SetValue      ("remote", "HCWButtonRelease", hScrollBarButtonRelease.Value);
        xmlwriter.SetValue      ("remote", "HCWRepeatFilter", hScrollBarRepeatFilter.Value);
        xmlwriter.SetValue      ("remote", "HCWRepeatSpeed", hScrollBarRepeatSpeed.Value);
        xmlwriter.SetValueAsBool("remote", "HCWFilterDoubleKlicks", checkBoxFilterDoubleKlicks.Checked);
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Remote));
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
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.hScrollBarRepeatSpeed = new System.Windows.Forms.HScrollBar();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.hScrollBarRepeatFilter = new System.Windows.Forms.HScrollBar();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.hScrollBarButtonRelease = new System.Windows.Forms.HScrollBar();
      this.label2sec = new System.Windows.Forms.Label();
      this.labelDelay = new System.Windows.Forms.Label();
      this.label0sec = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.btnMapping = new System.Windows.Forms.Button();
      this.checkBoxHCW = new System.Windows.Forms.CheckBox();
      this.groupBoxInformation = new System.Windows.Forms.GroupBox();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.infoDriverStatus = new System.Windows.Forms.Label();
      this.groupBoxSettings = new System.Windows.Forms.GroupBox();
      this.checkBoxFilterDoubleKlicks = new System.Windows.Forms.CheckBox();
      this.checkBoxAllowExternal = new System.Windows.Forms.CheckBox();
      this.checkBoxKeepControl = new System.Windows.Forms.CheckBox();
      this.checkBoxVerboseLog = new System.Windows.Forms.CheckBox();
      this.tabPageFireDTV = new System.Windows.Forms.TabPage();
      this.fireDTVRemote = new MediaPortal.Configuration.Sections.FireDTVRemote();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUSA)).BeginInit();
      this.tabControlRemotes.SuspendLayout();
      this.tabPageMCE.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEU)).BeginInit();
      this.tabPageHCW.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBoxInformation.SuspendLayout();
      this.groupBoxSettings.SuspendLayout();
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
      this.checkBoxMCE.AutoSize = true;
      this.checkBoxMCE.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxMCE.Location = new System.Drawing.Point(16, 24);
      this.checkBoxMCE.Name = "checkBoxMCE";
      this.checkBoxMCE.Size = new System.Drawing.Size(158, 18);
      this.checkBoxMCE.TabIndex = 0;
      this.checkBoxMCE.Text = "Use Microsoft MCE remote";
      this.checkBoxMCE.CheckedChanged += new System.EventHandler(this.checkBoxMCE_CheckedChanged);
      // 
      // tabControlRemotes
      // 
      this.tabControlRemotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlRemotes.Controls.Add(this.tabPageMCE);
      this.tabControlRemotes.Controls.Add(this.tabPageHCW);
      this.tabControlRemotes.Controls.Add(this.tabPageFireDTV);
      this.tabControlRemotes.Location = new System.Drawing.Point(0, 8);
      this.tabControlRemotes.Name = "tabControlRemotes";
      this.tabControlRemotes.SelectedIndex = 0;
      this.tabControlRemotes.Size = new System.Drawing.Size(472, 400);
      this.tabControlRemotes.TabIndex = 0;
      // 
      // tabPageMCE
      // 
      this.tabPageMCE.Controls.Add(this.groupBox1);
      this.tabPageMCE.Location = new System.Drawing.Point(4, 22);
      this.tabPageMCE.Name = "tabPageMCE";
      this.tabPageMCE.Size = new System.Drawing.Size(464, 374);
      this.tabPageMCE.TabIndex = 0;
      this.tabPageMCE.Text = "Microsoft MCE Remote";
      this.tabPageMCE.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.BackColor = System.Drawing.Color.Transparent;
      this.groupBox1.Controls.Add(this.checkBoxMCE);
      this.groupBox1.Controls.Add(this.radioButtonEurope);
      this.groupBox1.Controls.Add(this.radioButtonUSA);
      this.groupBox1.Controls.Add(this.pictureBoxUSA);
      this.groupBox1.Controls.Add(this.pictureBoxEU);
      this.groupBox1.Location = new System.Drawing.Point(12, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 352);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // radioButtonEurope
      // 
      this.radioButtonEurope.AutoSize = true;
      this.radioButtonEurope.Location = new System.Drawing.Point(32, 68);
      this.radioButtonEurope.Name = "radioButtonEurope";
      this.radioButtonEurope.Size = new System.Drawing.Size(108, 17);
      this.radioButtonEurope.TabIndex = 2;
      this.radioButtonEurope.Text = "European version";
      this.radioButtonEurope.CheckedChanged += new System.EventHandler(this.radioButtonEurope_CheckedChanged);
      // 
      // radioButtonUSA
      // 
      this.radioButtonUSA.AutoSize = true;
      this.radioButtonUSA.Location = new System.Drawing.Point(32, 48);
      this.radioButtonUSA.Name = "radioButtonUSA";
      this.radioButtonUSA.Size = new System.Drawing.Size(84, 17);
      this.radioButtonUSA.TabIndex = 1;
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
      this.tabPageHCW.BackColor = System.Drawing.Color.Transparent;
      this.tabPageHCW.Controls.Add(this.groupBox3);
      this.tabPageHCW.Controls.Add(this.groupBox2);
      this.tabPageHCW.Controls.Add(this.groupBoxInformation);
      this.tabPageHCW.Controls.Add(this.groupBoxSettings);
      this.tabPageHCW.Location = new System.Drawing.Point(4, 22);
      this.tabPageHCW.Name = "tabPageHCW";
      this.tabPageHCW.Size = new System.Drawing.Size(464, 374);
      this.tabPageHCW.TabIndex = 1;
      this.tabPageHCW.Text = "Hauppauge Remote";
      this.tabPageHCW.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.hScrollBarRepeatSpeed);
      this.groupBox3.Controls.Add(this.label4);
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.Controls.Add(this.label6);
      this.groupBox3.Controls.Add(this.hScrollBarRepeatFilter);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.hScrollBarButtonRelease);
      this.groupBox3.Controls.Add(this.label2sec);
      this.groupBox3.Controls.Add(this.labelDelay);
      this.groupBox3.Controls.Add(this.label0sec);
      this.groupBox3.Location = new System.Drawing.Point(12, 160);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(440, 112);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Repeat Delay";
      // 
      // hScrollBarRepeatSpeed
      // 
      this.hScrollBarRepeatSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarRepeatSpeed.LargeChange = 1;
      this.hScrollBarRepeatSpeed.Location = new System.Drawing.Point(152, 80);
      this.hScrollBarRepeatSpeed.Maximum = 2;
      this.hScrollBarRepeatSpeed.Name = "hScrollBarRepeatSpeed";
      this.hScrollBarRepeatSpeed.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.hScrollBarRepeatSpeed.Size = new System.Drawing.Size(213, 17);
      this.hScrollBarRepeatSpeed.TabIndex = 10;
      this.hScrollBarRepeatSpeed.Value = 1;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.AutoSize = true;
      this.label4.BackColor = System.Drawing.Color.Transparent;
      this.label4.Location = new System.Drawing.Point(368, 82);
      this.label4.Name = "label4";
      this.label4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.label4.Size = new System.Drawing.Size(24, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "fast";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(12, 82);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(77, 13);
      this.label5.TabIndex = 8;
      this.label5.Text = "Repeat speed:";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.BackColor = System.Drawing.Color.Transparent;
      this.label6.Location = new System.Drawing.Point(123, 82);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(28, 13);
      this.label6.TabIndex = 9;
      this.label6.Text = "slow";
      this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // hScrollBarRepeatFilter
      // 
      this.hScrollBarRepeatFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarRepeatFilter.LargeChange = 2;
      this.hScrollBarRepeatFilter.Location = new System.Drawing.Point(152, 52);
      this.hScrollBarRepeatFilter.Maximum = 11;
      this.hScrollBarRepeatFilter.Minimum = 2;
      this.hScrollBarRepeatFilter.Name = "hScrollBarRepeatFilter";
      this.hScrollBarRepeatFilter.Size = new System.Drawing.Size(213, 17);
      this.hScrollBarRepeatFilter.TabIndex = 6;
      this.hScrollBarRepeatFilter.Value = 2;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.BackColor = System.Drawing.Color.Transparent;
      this.label1.Location = new System.Drawing.Point(368, 54);
      this.label1.Name = "label1";
      this.label1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.label1.Size = new System.Drawing.Size(19, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "10";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 54);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(67, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Repeat filter:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.BackColor = System.Drawing.Color.Transparent;
      this.label3.Location = new System.Drawing.Point(137, 54);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(13, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "2";
      this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // hScrollBarButtonRelease
      // 
      this.hScrollBarButtonRelease.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarButtonRelease.Location = new System.Drawing.Point(152, 25);
      this.hScrollBarButtonRelease.Maximum = 209;
      this.hScrollBarButtonRelease.Minimum = 20;
      this.hScrollBarButtonRelease.Name = "hScrollBarButtonRelease";
      this.hScrollBarButtonRelease.Size = new System.Drawing.Size(213, 17);
      this.hScrollBarButtonRelease.TabIndex = 2;
      this.hScrollBarButtonRelease.Value = 100;
      // 
      // label2sec
      // 
      this.label2sec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2sec.AutoSize = true;
      this.label2sec.BackColor = System.Drawing.Color.Transparent;
      this.label2sec.Location = new System.Drawing.Point(368, 27);
      this.label2sec.Name = "label2sec";
      this.label2sec.Size = new System.Drawing.Size(56, 13);
      this.label2sec.TabIndex = 3;
      this.label2sec.Text = "200 msec.";
      // 
      // labelDelay
      // 
      this.labelDelay.AutoSize = true;
      this.labelDelay.Location = new System.Drawing.Point(12, 27);
      this.labelDelay.Name = "labelDelay";
      this.labelDelay.Size = new System.Drawing.Size(78, 13);
      this.labelDelay.TabIndex = 0;
      this.labelDelay.Text = "Button release:";
      // 
      // label0sec
      // 
      this.label0sec.AutoSize = true;
      this.label0sec.BackColor = System.Drawing.Color.Transparent;
      this.label0sec.Location = new System.Drawing.Point(103, 27);
      this.label0sec.Name = "label0sec";
      this.label0sec.Size = new System.Drawing.Size(50, 13);
      this.label0sec.TabIndex = 1;
      this.label0sec.Text = "20 msec.";
      this.label0sec.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.btnMapping);
      this.groupBox2.Controls.Add(this.checkBoxHCW);
      this.groupBox2.Location = new System.Drawing.Point(12, 8);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(440, 56);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      // 
      // btnMapping
      // 
      this.btnMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMapping.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnMapping.Location = new System.Drawing.Point(352, 20);
      this.btnMapping.Name = "btnMapping";
      this.btnMapping.Size = new System.Drawing.Size(72, 22);
      this.btnMapping.TabIndex = 1;
      this.btnMapping.Text = "Mapping";
      this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
      // 
      // checkBoxHCW
      // 
      this.checkBoxHCW.AutoSize = true;
      this.checkBoxHCW.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHCW.Name = "checkBoxHCW";
      this.checkBoxHCW.Size = new System.Drawing.Size(139, 17);
      this.checkBoxHCW.TabIndex = 0;
      this.checkBoxHCW.Text = "Use Hauppauge remote";
      this.checkBoxHCW.CheckedChanged += new System.EventHandler(this.checkBoxHCW_CheckedChanged);
      // 
      // groupBoxInformation
      // 
      this.groupBoxInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxInformation.Controls.Add(this.buttonDefault);
      this.groupBoxInformation.Controls.Add(this.infoDriverStatus);
      this.groupBoxInformation.Location = new System.Drawing.Point(12, 280);
      this.groupBoxInformation.Name = "groupBoxInformation";
      this.groupBoxInformation.Size = new System.Drawing.Size(440, 80);
      this.groupBoxInformation.TabIndex = 3;
      this.groupBoxInformation.TabStop = false;
      this.groupBoxInformation.Text = "Information";
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonDefault.Location = new System.Drawing.Point(352, 48);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(72, 22);
      this.buttonDefault.TabIndex = 1;
      this.buttonDefault.Text = "&Reset";
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // infoDriverStatus
      // 
      this.infoDriverStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.infoDriverStatus.ForeColor = System.Drawing.SystemColors.ControlText;
      this.infoDriverStatus.Location = new System.Drawing.Point(12, 16);
      this.infoDriverStatus.Name = "infoDriverStatus";
      this.infoDriverStatus.Size = new System.Drawing.Size(414, 56);
      this.infoDriverStatus.TabIndex = 0;
      this.infoDriverStatus.Text = "No problems found.";
      this.infoDriverStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.checkBoxFilterDoubleKlicks);
      this.groupBoxSettings.Controls.Add(this.checkBoxAllowExternal);
      this.groupBoxSettings.Controls.Add(this.checkBoxKeepControl);
      this.groupBoxSettings.Controls.Add(this.checkBoxVerboseLog);
      this.groupBoxSettings.Location = new System.Drawing.Point(12, 72);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(440, 80);
      this.groupBoxSettings.TabIndex = 1;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // checkBoxFilterDoubleKlicks
      // 
      this.checkBoxFilterDoubleKlicks.AutoSize = true;
      this.checkBoxFilterDoubleKlicks.Location = new System.Drawing.Point(288, 24);
      this.checkBoxFilterDoubleKlicks.Name = "checkBoxFilterDoubleKlicks";
      this.checkBoxFilterDoubleKlicks.Size = new System.Drawing.Size(110, 17);
      this.checkBoxFilterDoubleKlicks.TabIndex = 1;
      this.checkBoxFilterDoubleKlicks.Text = "Filter doubleclicks";
      // 
      // checkBoxAllowExternal
      // 
      this.checkBoxAllowExternal.AutoSize = true;
      this.checkBoxAllowExternal.Location = new System.Drawing.Point(16, 24);
      this.checkBoxAllowExternal.Name = "checkBoxAllowExternal";
      this.checkBoxAllowExternal.Size = new System.Drawing.Size(245, 17);
      this.checkBoxAllowExternal.TabIndex = 0;
      this.checkBoxAllowExternal.Text = "External processes may use the remote control";
      this.checkBoxAllowExternal.CheckedChanged += new System.EventHandler(this.checkBoxAllowExternal_CheckedChanged);
      // 
      // checkBoxKeepControl
      // 
      this.checkBoxKeepControl.AutoSize = true;
      this.checkBoxKeepControl.Enabled = false;
      this.checkBoxKeepControl.Location = new System.Drawing.Point(32, 48);
      this.checkBoxKeepControl.Name = "checkBoxKeepControl";
      this.checkBoxKeepControl.Size = new System.Drawing.Size(196, 17);
      this.checkBoxKeepControl.TabIndex = 2;
      this.checkBoxKeepControl.Text = "Keep control when MP looses focus";
      // 
      // checkBoxVerboseLog
      // 
      this.checkBoxVerboseLog.AutoSize = true;
      this.checkBoxVerboseLog.Location = new System.Drawing.Point(288, 48);
      this.checkBoxVerboseLog.Name = "checkBoxVerboseLog";
      this.checkBoxVerboseLog.Size = new System.Drawing.Size(108, 17);
      this.checkBoxVerboseLog.TabIndex = 3;
      this.checkBoxVerboseLog.Text = "Extended logging";
      // 
      // tabPageFireDTV
      // 
      this.tabPageFireDTV.Controls.Add(this.fireDTVRemote);
      this.tabPageFireDTV.Location = new System.Drawing.Point(4, 22);
      this.tabPageFireDTV.Name = "tabPageFireDTV";
      this.tabPageFireDTV.Size = new System.Drawing.Size(464, 374);
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
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUSA)).EndInit();
      this.tabControlRemotes.ResumeLayout(false);
      this.tabPageMCE.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEU)).EndInit();
      this.tabPageHCW.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBoxInformation.ResumeLayout(false);
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
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
      hScrollBarButtonRelease.Value = 100;
      hScrollBarRepeatFilter.Value = 2;
      hScrollBarRepeatSpeed.Value = 1;
      checkBoxFilterDoubleKlicks.Checked = false;
    }
    #endregion

    private void btnMapping_Click(object sender, System.EventArgs e)
    {
      InputMappingForm dlg = new InputMappingForm("Hauppauge HCW");
      dlg.ShowDialog(this);
    }

    /// <summary>
    /// Get the Hauppauge IR components installation path from the windows registry.
    /// </summary>
    /// <returns>Installation path of the Hauppauge IR components</returns>
    public static string GetHCWPath()
    {
      string dllPath = null;
      try
      {
        RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Hauppauge WinTV Infrared Remote");
        dllPath = rkey.GetValue("UninstallString").ToString();
        if (dllPath.IndexOf("UNir32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNir32"));
        else if (dllPath.IndexOf("UNIR32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNIR32"));
      }
      catch (System.NullReferenceException)
      {
        Log.Write("HCW: Could not find registry entries for driver components! (Not installed?)");
      }
      return dllPath;
    }

    /// <summary>
    /// Returns the path of the DLL component
    /// </summary>
    /// <returns>DLL path</returns>
    public static string GetDllPath()
    {
      string dllPath = GetHCWPath();
      if (!File.Exists(dllPath + "irremote.DLL"))
      {
        dllPath = null;
      }
      return dllPath;
    }
  }
}
