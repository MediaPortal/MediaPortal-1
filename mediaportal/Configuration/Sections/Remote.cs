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
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonUSA;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonEurope;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSettings;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwButtonRelease;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwAllowExternal;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwKeepControl;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwExtendedLogging;
    private MediaPortal.UserInterface.Controls.MPButton buttonDefault;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwEnabled;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwDriverStatus;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHcwStatus;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageMCE;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageHCW;
    private MediaPortal.UserInterface.Controls.MPLabel label2sec;
    private MediaPortal.UserInterface.Controls.MPLabel label0sec;
    private MediaPortal.UserInterface.Controls.MPButton btnHcwMapping;
    private MediaPortal.Configuration.Sections.FireDTVRemote fireDTVRemote;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlRemotes;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageFireDTV;
    private System.Windows.Forms.PictureBox pictureBoxEU;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHcwGeneral;
    private GroupBox groupBoxHcwRepeatDelay;
    private HScrollBar hScrollBarHcwButtonRelease;
    private HScrollBar hScrollBarHcwRepeatSpeed;
    private Label label4;
    private Label labelHcwRepeatSpeed;
    private Label label6;
    private HScrollBar hScrollBarHcwRepeatFilter;
    private Label label1;
    private Label labelHcwRepeatFilter;
    private Label label3;
    private CheckBox checkBoxHcwFilterDoubleKlicks;
    private ToolTip toolTip;
    private TabPage tabPageX10;
    private GroupBox groupBoxX10General;
    private Button buttonX10Mapping;
    private CheckBox checkBoxX10Enabled;
    private GroupBox groupBoxX10Settings;
    private CheckBox checkBoxX10ExtendedLogging;
    private GroupBox groupBoxX10Status;
    private Button buttonX10Defaults;
    private Label labelX10DriverStatus;
    private System.ComponentModel.IContainer components = null;
    private enum hcwRepeatSpeed { slow, medium, fast };

    public struct RAWINPUTDEVICE
    {
      public ushort usUsagePage;
      public ushort usUsage;
      public uint dwFlags;
      public IntPtr hwndTarget;
    }

    [DllImport("User32.dll", EntryPoint = "RegisterRawInputDevices", SetLastError = true)]
    public extern static bool RegisterRawInputDevices(
      [In] RAWINPUTDEVICE[] pRawInputDevices,
      [In] uint uiNumDevices,
      [In] uint cbSize);


    public Remote()
      : this("Remote")
    {
    }

    public Remote(string name)
      : base(name)
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
      catch (Exception)
      { }

      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      string errHCWNotInstalled = "The Hauppauge IR components have not been found.\n\nInstall the latest Hauppauge IR drivers and use XPSP2.";
      string errHCWOutOfDate = "The driver components are not up to date.\n\nUpdate your Hauppauge IR drivers to the current version.";
      string errHCWMissingExe = "IR application not found. You might want to use it to control external applications.\n\nReinstall the Hauppauge IR drivers to fix this problem.";

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        checkBoxMCE.Checked = xmlreader.GetValueAsBool("remote", "mce2005", false);
        radioButtonUSA.Checked = xmlreader.GetValueAsBool("remote", "USAModel", false);

        checkBoxHcwEnabled.Checked = xmlreader.GetValueAsBool("remote", "HCW", false);
        checkBoxHcwAllowExternal.Checked = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        checkBoxHcwKeepControl.Checked = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        checkBoxHcwExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        hScrollBarHcwButtonRelease.Value = xmlreader.GetValueAsInt("remote", "HCWButtonRelease", 500);
        hScrollBarHcwRepeatFilter.Value = xmlreader.GetValueAsInt("remote", "HCWRepeatFilter", 2);
        hScrollBarHcwRepeatSpeed.Value = xmlreader.GetValueAsInt("remote", "HCWRepeatSpeed", 1);
        checkBoxHcwFilterDoubleKlicks.Checked = xmlreader.GetValueAsBool("remote", "HCWFilterDoubleKlicks", false);

        checkBoxX10Enabled.Checked = xmlreader.GetValueAsBool("remote", "x10", false);
        checkBoxX10ExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "x10VerboseLog", false);
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

      if (checkBoxHcwAllowExternal.Checked)
        checkBoxHcwKeepControl.Enabled = true;
      else
        checkBoxHcwKeepControl.Enabled = false;

      if (!checkBoxHcwEnabled.Checked)
      {
        groupBoxSettings.Enabled = false;
        groupBoxHcwRepeatDelay.Enabled = false;
      }

      string exePath = GetHCWPath();
      string dllPath = GetDllPath();

      if (File.Exists(exePath + "Ir.exe"))
      {
        FileVersionInfo exeVersionInfo = FileVersionInfo.GetVersionInfo(exePath + "Ir.exe");
        if (exeVersionInfo.FileVersion.CompareTo("2.45.22350") < 0)
          labelHcwDriverStatus.Text = errHCWOutOfDate;
      }
      else
      {
        labelHcwDriverStatus.Text = errHCWMissingExe;
        checkBoxHcwAllowExternal.Enabled = false;
        checkBoxHcwKeepControl.Enabled = false;
      }

      if (File.Exists(dllPath + "irremote.DLL"))
      {
        FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath + "irremote.DLL");
        if (dllVersionInfo.FileVersion.CompareTo("2.45.22350") < 0)
          labelHcwDriverStatus.Text = errHCWOutOfDate;
      }
      else
      {
        labelHcwDriverStatus.Text = errHCWNotInstalled;
        checkBoxHcwEnabled.Enabled = false;
        groupBoxSettings.Enabled = false;
        groupBoxHcwRepeatDelay.Enabled = false;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          xmlwriter.SetValueAsBool("remote", "HCW", false);
        }
      }

      fireDTVRemote.LoadSettings();

      toolTip.SetToolTip(this.hScrollBarHcwButtonRelease, string.Format("{0} msec.", hScrollBarHcwButtonRelease.Value));
      toolTip.SetToolTip(this.hScrollBarHcwRepeatFilter, hScrollBarHcwRepeatFilter.Value.ToString());
      Type repeatSpeed = typeof(hcwRepeatSpeed);
      toolTip.SetToolTip(this.hScrollBarHcwRepeatSpeed, Enum.GetName(repeatSpeed, 2 - hScrollBarHcwRepeatSpeed.Value));
      toolTip.SetToolTip(this.checkBoxHcwKeepControl, "If checked, MediaPortal keeps control of the remote. Only applications launched by\nMediaPortal can steal focus (external Players, MyPrograms, ...).");
      toolTip.SetToolTip(this.checkBoxHcwAllowExternal, "If checked, MediaPortal does not keep control of the remote\nwhen it looses focus.");
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("remote", "mce2005", checkBoxMCE.Checked);
        xmlwriter.SetValueAsBool("remote", "USAModel", radioButtonUSA.Checked);

        xmlwriter.SetValueAsBool("remote", "HCW", checkBoxHcwEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWAllowExternal", checkBoxHcwAllowExternal.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWKeepControl", checkBoxHcwKeepControl.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWVerboseLog", checkBoxHcwExtendedLogging.Checked);
        xmlwriter.SetValue("remote", "HCWButtonRelease", hScrollBarHcwButtonRelease.Value);
        xmlwriter.SetValue("remote", "HCWRepeatFilter", hScrollBarHcwRepeatFilter.Value);
        xmlwriter.SetValue("remote", "HCWRepeatSpeed", hScrollBarHcwRepeatSpeed.Value);
        xmlwriter.SetValueAsBool("remote", "HCWFilterDoubleKlicks", checkBoxHcwFilterDoubleKlicks.Checked);

        xmlwriter.SetValueAsBool("remote", "x10", checkBoxX10Enabled.Checked);
        xmlwriter.SetValueAsBool("remote", "x10VerboseLog", checkBoxX10ExtendedLogging.Checked);
      }

      fireDTVRemote.SaveSettings();
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Remote));
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.pictureBoxUSA = new System.Windows.Forms.PictureBox();
      this.checkBoxMCE = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlRemotes = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageMCE = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonEurope = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonUSA = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.pictureBoxEU = new System.Windows.Forms.PictureBox();
      this.tabPageHCW = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxHcwRepeatDelay = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.hScrollBarHcwRepeatSpeed = new System.Windows.Forms.HScrollBar();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwRepeatSpeed = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarHcwRepeatFilter = new System.Windows.Forms.HScrollBar();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwRepeatFilter = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarHcwButtonRelease = new System.Windows.Forms.HScrollBar();
      this.label2sec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwButtonRelease = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label0sec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxHcwGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnHcwMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxHcwEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxHcwStatus = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelHcwDriverStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxHcwFilterDoubleKlicks = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwAllowExternal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwKeepControl = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageX10 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxX10Status = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonX10Defaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelX10DriverStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxX10General = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonX10Mapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxX10Enabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxX10Settings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxX10ExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageFireDTV = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.fireDTVRemote = new MediaPortal.Configuration.Sections.FireDTVRemote();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUSA)).BeginInit();
      this.tabControlRemotes.SuspendLayout();
      this.tabPageMCE.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEU)).BeginInit();
      this.tabPageHCW.SuspendLayout();
      this.groupBoxHcwRepeatDelay.SuspendLayout();
      this.groupBoxHcwGeneral.SuspendLayout();
      this.groupBoxHcwStatus.SuspendLayout();
      this.groupBoxSettings.SuspendLayout();
      this.tabPageX10.SuspendLayout();
      this.groupBoxX10Status.SuspendLayout();
      this.groupBoxX10General.SuspendLayout();
      this.groupBoxX10Settings.SuspendLayout();
      this.tabPageFireDTV.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBoxUSA
      // 
      this.pictureBoxUSA.BackColor = System.Drawing.Color.Transparent;
      this.pictureBoxUSA.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxUSA.Image")));
      this.pictureBoxUSA.Location = new System.Drawing.Point(248, 24);
      this.pictureBoxUSA.Name = "pictureBoxUSA";
      this.pictureBoxUSA.Size = new System.Drawing.Size(100, 310);
      this.pictureBoxUSA.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxUSA.TabIndex = 3;
      this.pictureBoxUSA.TabStop = false;
      // 
      // checkBoxMCE
      // 
      this.checkBoxMCE.AutoSize = true;
      this.checkBoxMCE.Location = new System.Drawing.Point(16, 24);
      this.checkBoxMCE.Name = "checkBoxMCE";
      this.checkBoxMCE.Size = new System.Drawing.Size(152, 17);
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
      this.tabControlRemotes.Controls.Add(this.tabPageX10);
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
      this.radioButtonUSA.Size = new System.Drawing.Size(204, 17);
      this.radioButtonUSA.TabIndex = 1;
      this.radioButtonUSA.Text = "USA version / Hauppauge MCE OEM";
      this.radioButtonUSA.CheckedChanged += new System.EventHandler(this.radioButtonUSA_CheckedChanged);
      // 
      // pictureBoxEU
      // 
      this.pictureBoxEU.BackColor = System.Drawing.Color.Transparent;
      this.pictureBoxEU.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxEU.Image")));
      this.pictureBoxEU.Location = new System.Drawing.Point(248, 24);
      this.pictureBoxEU.Name = "pictureBoxEU";
      this.pictureBoxEU.Size = new System.Drawing.Size(100, 310);
      this.pictureBoxEU.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxEU.TabIndex = 4;
      this.pictureBoxEU.TabStop = false;
      // 
      // tabPageHCW
      // 
      this.tabPageHCW.Controls.Add(this.groupBoxHcwRepeatDelay);
      this.tabPageHCW.Controls.Add(this.groupBoxHcwGeneral);
      this.tabPageHCW.Controls.Add(this.groupBoxHcwStatus);
      this.tabPageHCW.Controls.Add(this.groupBoxSettings);
      this.tabPageHCW.Location = new System.Drawing.Point(4, 22);
      this.tabPageHCW.Name = "tabPageHCW";
      this.tabPageHCW.Size = new System.Drawing.Size(464, 374);
      this.tabPageHCW.TabIndex = 1;
      this.tabPageHCW.Text = "Hauppauge Remote";
      // 
      // groupBoxHcwRepeatDelay
      // 
      this.groupBoxHcwRepeatDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwRepeatDelay.Controls.Add(this.hScrollBarHcwRepeatSpeed);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.label4);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwRepeatSpeed);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.label6);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.hScrollBarHcwRepeatFilter);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.label1);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwRepeatFilter);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.label3);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.hScrollBarHcwButtonRelease);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.label2sec);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwButtonRelease);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.label0sec);
      this.groupBoxHcwRepeatDelay.Location = new System.Drawing.Point(12, 160);
      this.groupBoxHcwRepeatDelay.Name = "groupBoxHcwRepeatDelay";
      this.groupBoxHcwRepeatDelay.Size = new System.Drawing.Size(440, 112);
      this.groupBoxHcwRepeatDelay.TabIndex = 2;
      this.groupBoxHcwRepeatDelay.TabStop = false;
      this.groupBoxHcwRepeatDelay.Text = "Repeat Delay";
      // 
      // hScrollBarHcwRepeatSpeed
      // 
      this.hScrollBarHcwRepeatSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarHcwRepeatSpeed.LargeChange = 1;
      this.hScrollBarHcwRepeatSpeed.Location = new System.Drawing.Point(152, 80);
      this.hScrollBarHcwRepeatSpeed.Maximum = 2;
      this.hScrollBarHcwRepeatSpeed.Name = "hScrollBarHcwRepeatSpeed";
      this.hScrollBarHcwRepeatSpeed.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.hScrollBarHcwRepeatSpeed.Size = new System.Drawing.Size(213, 17);
      this.hScrollBarHcwRepeatSpeed.TabIndex = 10;
      this.hScrollBarHcwRepeatSpeed.Value = 1;
      this.hScrollBarHcwRepeatSpeed.ValueChanged += new System.EventHandler(this.hScrollBarRepeatSpeed_ValueChanged);
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.AutoSize = true;
      this.label4.BackColor = System.Drawing.Color.Transparent;
      this.label4.Location = new System.Drawing.Point(368, 82);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(24, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "fast";
      // 
      // labelHcwRepeatSpeed
      // 
      this.labelHcwRepeatSpeed.AutoSize = true;
      this.labelHcwRepeatSpeed.Location = new System.Drawing.Point(12, 82);
      this.labelHcwRepeatSpeed.Name = "labelHcwRepeatSpeed";
      this.labelHcwRepeatSpeed.Size = new System.Drawing.Size(77, 13);
      this.labelHcwRepeatSpeed.TabIndex = 8;
      this.labelHcwRepeatSpeed.Text = "Repeat speed:";
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
      // hScrollBarHcwRepeatFilter
      // 
      this.hScrollBarHcwRepeatFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarHcwRepeatFilter.LargeChange = 2;
      this.hScrollBarHcwRepeatFilter.Location = new System.Drawing.Point(152, 52);
      this.hScrollBarHcwRepeatFilter.Maximum = 11;
      this.hScrollBarHcwRepeatFilter.Minimum = 2;
      this.hScrollBarHcwRepeatFilter.Name = "hScrollBarHcwRepeatFilter";
      this.hScrollBarHcwRepeatFilter.Size = new System.Drawing.Size(213, 17);
      this.hScrollBarHcwRepeatFilter.TabIndex = 6;
      this.hScrollBarHcwRepeatFilter.Value = 2;
      this.hScrollBarHcwRepeatFilter.ValueChanged += new System.EventHandler(this.hScrollBarRepeatFilter_ValueChanged);
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.BackColor = System.Drawing.Color.Transparent;
      this.label1.Location = new System.Drawing.Point(368, 54);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(29, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "max.";
      // 
      // labelHcwRepeatFilter
      // 
      this.labelHcwRepeatFilter.AutoSize = true;
      this.labelHcwRepeatFilter.Location = new System.Drawing.Point(12, 54);
      this.labelHcwRepeatFilter.Name = "labelHcwRepeatFilter";
      this.labelHcwRepeatFilter.Size = new System.Drawing.Size(67, 13);
      this.labelHcwRepeatFilter.TabIndex = 4;
      this.labelHcwRepeatFilter.Text = "Repeat filter:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.BackColor = System.Drawing.Color.Transparent;
      this.label3.Location = new System.Drawing.Point(128, 54);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(26, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "min.";
      this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // hScrollBarHcwButtonRelease
      // 
      this.hScrollBarHcwButtonRelease.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hScrollBarHcwButtonRelease.Location = new System.Drawing.Point(152, 25);
      this.hScrollBarHcwButtonRelease.Maximum = 1009;
      this.hScrollBarHcwButtonRelease.Minimum = 20;
      this.hScrollBarHcwButtonRelease.Name = "hScrollBarHcwButtonRelease";
      this.hScrollBarHcwButtonRelease.Size = new System.Drawing.Size(213, 17);
      this.hScrollBarHcwButtonRelease.TabIndex = 2;
      this.hScrollBarHcwButtonRelease.Value = 500;
      this.hScrollBarHcwButtonRelease.ValueChanged += new System.EventHandler(this.hScrollBarButtonRelease_ValueChanged);
      // 
      // label2sec
      // 
      this.label2sec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2sec.AutoSize = true;
      this.label2sec.BackColor = System.Drawing.Color.Transparent;
      this.label2sec.Location = new System.Drawing.Point(368, 27);
      this.label2sec.Name = "label2sec";
      this.label2sec.Size = new System.Drawing.Size(62, 13);
      this.label2sec.TabIndex = 3;
      this.label2sec.Text = "1000 msec.";
      // 
      // labelHcwButtonRelease
      // 
      this.labelHcwButtonRelease.AutoSize = true;
      this.labelHcwButtonRelease.Location = new System.Drawing.Point(12, 27);
      this.labelHcwButtonRelease.Name = "labelHcwButtonRelease";
      this.labelHcwButtonRelease.Size = new System.Drawing.Size(78, 13);
      this.labelHcwButtonRelease.TabIndex = 0;
      this.labelHcwButtonRelease.Text = "Button release:";
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
      // groupBoxHcwGeneral
      // 
      this.groupBoxHcwGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwGeneral.Controls.Add(this.btnHcwMapping);
      this.groupBoxHcwGeneral.Controls.Add(this.checkBoxHcwEnabled);
      this.groupBoxHcwGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxHcwGeneral.Name = "groupBoxHcwGeneral";
      this.groupBoxHcwGeneral.Size = new System.Drawing.Size(440, 56);
      this.groupBoxHcwGeneral.TabIndex = 0;
      this.groupBoxHcwGeneral.TabStop = false;
      // 
      // btnHcwMapping
      // 
      this.btnHcwMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnHcwMapping.Location = new System.Drawing.Point(352, 20);
      this.btnHcwMapping.Name = "btnHcwMapping";
      this.btnHcwMapping.Size = new System.Drawing.Size(72, 22);
      this.btnHcwMapping.TabIndex = 1;
      this.btnHcwMapping.Text = "Mapping";
      this.btnHcwMapping.Click += new System.EventHandler(this.btnMapping_Click);
      // 
      // checkBoxHcwEnabled
      // 
      this.checkBoxHcwEnabled.AutoSize = true;
      this.checkBoxHcwEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHcwEnabled.Name = "checkBoxHcwEnabled";
      this.checkBoxHcwEnabled.Size = new System.Drawing.Size(139, 17);
      this.checkBoxHcwEnabled.TabIndex = 0;
      this.checkBoxHcwEnabled.Text = "Use Hauppauge remote";
      this.checkBoxHcwEnabled.CheckedChanged += new System.EventHandler(this.checkBoxHCW_CheckedChanged);
      // 
      // groupBoxHcwStatus
      // 
      this.groupBoxHcwStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwStatus.Controls.Add(this.buttonDefault);
      this.groupBoxHcwStatus.Controls.Add(this.labelHcwDriverStatus);
      this.groupBoxHcwStatus.Location = new System.Drawing.Point(12, 280);
      this.groupBoxHcwStatus.Name = "groupBoxHcwStatus";
      this.groupBoxHcwStatus.Size = new System.Drawing.Size(440, 80);
      this.groupBoxHcwStatus.TabIndex = 3;
      this.groupBoxHcwStatus.TabStop = false;
      this.groupBoxHcwStatus.Text = "Status";
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefault.Location = new System.Drawing.Point(352, 48);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(72, 22);
      this.buttonDefault.TabIndex = 1;
      this.buttonDefault.Text = "&Defaults";
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // labelHcwDriverStatus
      // 
      this.labelHcwDriverStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcwDriverStatus.ForeColor = System.Drawing.SystemColors.ControlText;
      this.labelHcwDriverStatus.Location = new System.Drawing.Point(12, 24);
      this.labelHcwDriverStatus.Name = "labelHcwDriverStatus";
      this.labelHcwDriverStatus.Size = new System.Drawing.Size(414, 48);
      this.labelHcwDriverStatus.TabIndex = 0;
      this.labelHcwDriverStatus.Text = "No problems found.";
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.checkBoxHcwFilterDoubleKlicks);
      this.groupBoxSettings.Controls.Add(this.checkBoxHcwAllowExternal);
      this.groupBoxSettings.Controls.Add(this.checkBoxHcwKeepControl);
      this.groupBoxSettings.Controls.Add(this.checkBoxHcwExtendedLogging);
      this.groupBoxSettings.Location = new System.Drawing.Point(12, 72);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(440, 80);
      this.groupBoxSettings.TabIndex = 1;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // checkBoxHcwFilterDoubleKlicks
      // 
      this.checkBoxHcwFilterDoubleKlicks.AutoSize = true;
      this.checkBoxHcwFilterDoubleKlicks.Location = new System.Drawing.Point(288, 24);
      this.checkBoxHcwFilterDoubleKlicks.Name = "checkBoxHcwFilterDoubleKlicks";
      this.checkBoxHcwFilterDoubleKlicks.Size = new System.Drawing.Size(110, 17);
      this.checkBoxHcwFilterDoubleKlicks.TabIndex = 1;
      this.checkBoxHcwFilterDoubleKlicks.Text = "Filter doubleclicks";
      // 
      // checkBoxHcwAllowExternal
      // 
      this.checkBoxHcwAllowExternal.AutoSize = true;
      this.checkBoxHcwAllowExternal.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHcwAllowExternal.Name = "checkBoxHcwAllowExternal";
      this.checkBoxHcwAllowExternal.Size = new System.Drawing.Size(245, 17);
      this.checkBoxHcwAllowExternal.TabIndex = 0;
      this.checkBoxHcwAllowExternal.Text = "External processes may use the remote control";
      this.checkBoxHcwAllowExternal.CheckedChanged += new System.EventHandler(this.checkBoxAllowExternal_CheckedChanged);
      // 
      // checkBoxHcwKeepControl
      // 
      this.checkBoxHcwKeepControl.AutoSize = true;
      this.checkBoxHcwKeepControl.Enabled = false;
      this.checkBoxHcwKeepControl.Location = new System.Drawing.Point(32, 48);
      this.checkBoxHcwKeepControl.Name = "checkBoxHcwKeepControl";
      this.checkBoxHcwKeepControl.Size = new System.Drawing.Size(196, 17);
      this.checkBoxHcwKeepControl.TabIndex = 2;
      this.checkBoxHcwKeepControl.Text = "Keep control when MP looses focus";
      // 
      // checkBoxHcwExtendedLogging
      // 
      this.checkBoxHcwExtendedLogging.AutoSize = true;
      this.checkBoxHcwExtendedLogging.Location = new System.Drawing.Point(288, 48);
      this.checkBoxHcwExtendedLogging.Name = "checkBoxHcwExtendedLogging";
      this.checkBoxHcwExtendedLogging.Size = new System.Drawing.Size(108, 17);
      this.checkBoxHcwExtendedLogging.TabIndex = 3;
      this.checkBoxHcwExtendedLogging.Text = "Extended logging";
      // 
      // tabPageX10
      // 
      this.tabPageX10.Controls.Add(this.groupBoxX10Status);
      this.tabPageX10.Controls.Add(this.groupBoxX10General);
      this.tabPageX10.Controls.Add(this.groupBoxX10Settings);
      this.tabPageX10.Location = new System.Drawing.Point(4, 22);
      this.tabPageX10.Name = "tabPageX10";
      this.tabPageX10.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageX10.Size = new System.Drawing.Size(464, 374);
      this.tabPageX10.TabIndex = 3;
      this.tabPageX10.Text = "x10 Remote";
      // 
      // groupBoxX10Status
      // 
      this.groupBoxX10Status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10Status.Controls.Add(this.buttonX10Defaults);
      this.groupBoxX10Status.Controls.Add(this.labelX10DriverStatus);
      this.groupBoxX10Status.Location = new System.Drawing.Point(12, 136);
      this.groupBoxX10Status.Name = "groupBoxX10Status";
      this.groupBoxX10Status.Size = new System.Drawing.Size(440, 80);
      this.groupBoxX10Status.TabIndex = 4;
      this.groupBoxX10Status.TabStop = false;
      this.groupBoxX10Status.Text = "Status";
      // 
      // buttonX10Defaults
      // 
      this.buttonX10Defaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10Defaults.Location = new System.Drawing.Point(352, 48);
      this.buttonX10Defaults.Name = "buttonX10Defaults";
      this.buttonX10Defaults.Size = new System.Drawing.Size(72, 22);
      this.buttonX10Defaults.TabIndex = 1;
      this.buttonX10Defaults.Text = "&Defaults";
      this.buttonX10Defaults.Click += new System.EventHandler(this.buttonX10Defaults_Click);
      // 
      // labelX10DriverStatus
      // 
      this.labelX10DriverStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelX10DriverStatus.ForeColor = System.Drawing.SystemColors.ControlText;
      this.labelX10DriverStatus.Location = new System.Drawing.Point(12, 24);
      this.labelX10DriverStatus.Name = "labelX10DriverStatus";
      this.labelX10DriverStatus.Size = new System.Drawing.Size(414, 48);
      this.labelX10DriverStatus.TabIndex = 0;
      this.labelX10DriverStatus.Text = "Status is not implemented yet.";
      // 
      // groupBoxX10General
      // 
      this.groupBoxX10General.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10General.Controls.Add(this.buttonX10Mapping);
      this.groupBoxX10General.Controls.Add(this.checkBoxX10Enabled);
      this.groupBoxX10General.Location = new System.Drawing.Point(12, 8);
      this.groupBoxX10General.Name = "groupBoxX10General";
      this.groupBoxX10General.Size = new System.Drawing.Size(440, 56);
      this.groupBoxX10General.TabIndex = 2;
      this.groupBoxX10General.TabStop = false;
      // 
      // buttonX10Mapping
      // 
      this.buttonX10Mapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10Mapping.Location = new System.Drawing.Point(352, 20);
      this.buttonX10Mapping.Name = "buttonX10Mapping";
      this.buttonX10Mapping.Size = new System.Drawing.Size(72, 22);
      this.buttonX10Mapping.TabIndex = 1;
      this.buttonX10Mapping.Text = "Mapping";
      this.buttonX10Mapping.Click += new System.EventHandler(this.buttonX10Mapping_Click);
      // 
      // checkBoxX10Enabled
      // 
      this.checkBoxX10Enabled.AutoSize = true;
      this.checkBoxX10Enabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxX10Enabled.Name = "checkBoxX10Enabled";
      this.checkBoxX10Enabled.Size = new System.Drawing.Size(100, 17);
      this.checkBoxX10Enabled.TabIndex = 0;
      this.checkBoxX10Enabled.Text = "Use x10 remote";
      // 
      // groupBoxX10Settings
      // 
      this.groupBoxX10Settings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10Settings.Controls.Add(this.checkBoxX10ExtendedLogging);
      this.groupBoxX10Settings.Location = new System.Drawing.Point(12, 72);
      this.groupBoxX10Settings.Name = "groupBoxX10Settings";
      this.groupBoxX10Settings.Size = new System.Drawing.Size(440, 56);
      this.groupBoxX10Settings.TabIndex = 3;
      this.groupBoxX10Settings.TabStop = false;
      this.groupBoxX10Settings.Text = "Settings";
      // 
      // checkBoxX10ExtendedLogging
      // 
      this.checkBoxX10ExtendedLogging.AutoSize = true;
      this.checkBoxX10ExtendedLogging.Location = new System.Drawing.Point(16, 24);
      this.checkBoxX10ExtendedLogging.Name = "checkBoxX10ExtendedLogging";
      this.checkBoxX10ExtendedLogging.Size = new System.Drawing.Size(108, 17);
      this.checkBoxX10ExtendedLogging.TabIndex = 3;
      this.checkBoxX10ExtendedLogging.Text = "Extended logging";
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
      this.fireDTVRemote.BackColor = System.Drawing.Color.Transparent;
      this.fireDTVRemote.Location = new System.Drawing.Point(0, 0);
      this.fireDTVRemote.Name = "fireDTVRemote";
      this.fireDTVRemote.Size = new System.Drawing.Size(520, 368);
      this.fireDTVRemote.TabIndex = 0;
      // 
      // toolTip
      // 
      this.toolTip.ShowAlways = true;
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
      this.groupBoxHcwRepeatDelay.ResumeLayout(false);
      this.groupBoxHcwRepeatDelay.PerformLayout();
      this.groupBoxHcwGeneral.ResumeLayout(false);
      this.groupBoxHcwGeneral.PerformLayout();
      this.groupBoxHcwStatus.ResumeLayout(false);
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
      this.tabPageX10.ResumeLayout(false);
      this.groupBoxX10Status.ResumeLayout(false);
      this.groupBoxX10General.ResumeLayout(false);
      this.groupBoxX10General.PerformLayout();
      this.groupBoxX10Settings.ResumeLayout(false);
      this.groupBoxX10Settings.PerformLayout();
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
      checkBoxHcwKeepControl.Enabled = checkBoxHcwAllowExternal.Checked;
    }

    //
    // Use Hauppauge remote
    //
    private void checkBoxHCW_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxSettings.Enabled = checkBoxHcwEnabled.Checked;
      groupBoxHcwRepeatDelay.Enabled = checkBoxHcwEnabled.Checked;
    }

    //
    // Reset to default
    //    
    private void buttonDefault_Click(object sender, System.EventArgs e)
    {
      checkBoxHcwAllowExternal.Checked = false;
      checkBoxHcwKeepControl.Checked = false;
      checkBoxHcwExtendedLogging.Checked = false;
      hScrollBarHcwButtonRelease.Value = 500;
      hScrollBarHcwRepeatFilter.Value = 2;
      hScrollBarHcwRepeatSpeed.Value = 1;
      checkBoxHcwFilterDoubleKlicks.Checked = false;
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

    private void hScrollBarButtonRelease_ValueChanged(object sender, EventArgs e)
    {
      toolTip.SetToolTip(this.hScrollBarHcwButtonRelease, string.Format("{0} msec.", hScrollBarHcwButtonRelease.Value));
    }

    private void hScrollBarRepeatFilter_ValueChanged(object sender, EventArgs e)
    {
      toolTip.SetToolTip(this.hScrollBarHcwRepeatFilter, hScrollBarHcwRepeatFilter.Value.ToString());
    }

    private void hScrollBarRepeatSpeed_ValueChanged(object sender, EventArgs e)
    {
      Type repeatSpeed = typeof(hcwRepeatSpeed);
      toolTip.SetToolTip(this.hScrollBarHcwRepeatSpeed, Enum.GetName(repeatSpeed, 2 - hScrollBarHcwRepeatSpeed.Value));
    }

    private void buttonX10Mapping_Click(object sender, EventArgs e)
    {
      InputMappingForm dlg = new InputMappingForm("x10");
      dlg.ShowDialog(this);
    }

    private void buttonX10Defaults_Click(object sender, EventArgs e)
    {
      checkBoxX10ExtendedLogging.Checked = false;
    }

  }
}
