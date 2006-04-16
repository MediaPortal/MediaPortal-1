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

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Remote : MediaPortal.Configuration.SectionSettings
  {

    #region Fields & Constants

    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMceEnabled;
    private System.Windows.Forms.PictureBox pictureBoxMceUsa;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonMceUsa;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonMceEurope;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHcwSettings;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwButtonRelease;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwAllowExternal;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwKeepControl;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwExtendedLogging;
    private MediaPortal.UserInterface.Controls.MPButton buttonHcwDefaults;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwEnabled;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwDriverStatus;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHcwStatus;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageMce;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageHcw;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcw1000msec;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcw20msec;
    private MediaPortal.UserInterface.Controls.MPButton buttonHcwMapping;
    private MediaPortal.Configuration.Sections.FireDtvRemote fireDtvRemote;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlRemotes;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageFireDtv;
    private System.Windows.Forms.PictureBox pictureBoxMceEurope;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMceGeneralx;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHcwGeneral;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHcwRepeatDelay;
    private HScrollBar hScrollBarHcwButtonRelease;
    private HScrollBar hScrollBarHcwRepeatSpeed;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwFast;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwRepeatSpeed;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwSlow;
    private HScrollBar hScrollBarHcwRepeatFilter;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwMax;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwRepeatFilter;
    private MediaPortal.UserInterface.Controls.MPLabel labelHcwMin;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHcwFilterDoubleKlicks;
    private ToolTip toolTip;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageX10;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxX10General;
    private MediaPortal.UserInterface.Controls.MPButton buttonX10Mapping;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxX10Enabled;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxX10Settings;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxX10ExtendedLogging;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxX10Status;
    private MediaPortal.UserInterface.Controls.MPButton buttonX10Defaults;
    private LinkLabel linkLabelHcwDownload;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonX10Ati;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonX10Medion;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonX10Other;
    private LinkLabel linkLabelDownloadX10;
    private Label labelX10DriverInfo;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxX10ChannelControl;
    private MediaPortal.UserInterface.Controls.MPButton buttonX10LearnChannel;
    private System.ComponentModel.IContainer components = null;
    private Label labelX10Status;
    private TabPage tabPageHid;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHidGeneral;
    private MediaPortal.UserInterface.Controls.MPButton buttonHidMapping;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHidEnabled;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHidInfo;
    private Label labelHidInfo;

    X10RemoteForm x10Form = null;
    private enum hcwRepeatSpeed { slow, medium, fast };
    private int x10Channel = 0;

    const string errHcwNotInstalled = "The Hauppauge IR components have not been found.\nInstall the latest Hauppauge IR drivers and use XPSP2.";
    const string errHcwOutOfDate = "The driver components are not up to date.\nUpdate your Hauppauge IR drivers to the current version.";
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxHidSettings;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxHidExtendedLogging;
    private MediaPortal.UserInterface.Controls.MPButton buttonHidDefaults;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMceGeneral;
    private MediaPortal.UserInterface.Controls.MPButton buttonMceMapping;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMceType;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMceExtendedLogging;
    private MediaPortal.UserInterface.Controls.MPButton buttonMceDefaults;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMceColouredButtons;
    private RadioButton radioButtonMceGeneral;
    private RadioButton radioButtonMceNoTeletext;
    private RadioButton radioButtonMceTeletext;
    private GroupBox groupBoxMceSettings;
    const string errHcwMissingExe = "IR application not found. You might want to use it to control external applications.\nReinstall the Hauppauge IR drivers to fix this problem.";

    #endregion

    #region Structures

    public struct RAWINPUTDEVICE
    {
      public ushort usUsagePage;
      public ushort usUsage;
      public uint dwFlags;
      public IntPtr hwndTarget;
    }

    #endregion

    #region Interop

    [DllImport("User32.dll", EntryPoint = "RegisterRawInputDevices", SetLastError = true)]
    public extern static bool RegisterRawInputDevices(
      [In] RAWINPUTDEVICE[] pRawInputDevices,
      [In] uint uiNumDevices,
      [In] uint cbSize);

    #endregion

    #region Constructor

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

    #endregion

    #region Configuration.SectionSettings

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {

        #region MCE

        checkBoxMceEnabled.Checked = xmlreader.GetValueAsBool("remote", "MCE", true);
        checkBoxMceExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "MCEVerboseLog", false);
        string mceType = xmlreader.GetValueAsString("remote", "MCEType", "General");

        switch (mceType)
        {
          case "NoTeletext":
            radioButtonMceNoTeletext.Checked = true;
            break;
          case "Teletext":
            radioButtonMceTeletext.Checked = true;
            break;
          default:
            radioButtonMceGeneral.Checked = true;
            break;
        }

        groupBoxMceType.Enabled = groupBoxMceSettings.Enabled = checkBoxMceEnabled.Checked;

        //radioButtonMceUsa.Checked = xmlreader.GetValueAsBool("remote", "USAModel", false);

        //radioButtonMceEurope.Checked = !radioButtonMceUsa.Checked;

        //if (checkBoxMceEnabled.Checked)
        //{
        //  radioButtonMceEurope.Enabled = true;
        //  radioButtonMceUsa.Enabled = true;
        //}
        //else
        //{
        //  radioButtonMceEurope.Enabled = false;
        //  radioButtonMceUsa.Enabled = false;
        //}

        //if (radioButtonMceUsa.Checked)
        //{
        //  pictureBoxMceUsa.Visible = true;
        //  pictureBoxMceEurope.Visible = false;
        //}
        //else
        //{
        //  pictureBoxMceEurope.Visible = true;
        //  pictureBoxMceUsa.Visible = false;
        //}

        #endregion

        #region HCW

        checkBoxHcwEnabled.Checked = xmlreader.GetValueAsBool("remote", "HCW", false);
        checkBoxHcwAllowExternal.Checked = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        checkBoxHcwKeepControl.Checked = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        checkBoxHcwExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        hScrollBarHcwButtonRelease.Value = xmlreader.GetValueAsInt("remote", "HCWButtonRelease", 200);
        hScrollBarHcwRepeatFilter.Value = xmlreader.GetValueAsInt("remote", "HCWRepeatFilter", 2);
        hScrollBarHcwRepeatSpeed.Value = xmlreader.GetValueAsInt("remote", "HCWRepeatSpeed", 0);
        checkBoxHcwFilterDoubleKlicks.Checked = xmlreader.GetValueAsBool("remote", "HCWFilterDoubleKlicks", false);

        if (checkBoxHcwAllowExternal.Checked)
          checkBoxHcwKeepControl.Enabled = true;
        else
          checkBoxHcwKeepControl.Enabled = false;

        if (!checkBoxHcwEnabled.Checked)
        {
          groupBoxHcwSettings.Enabled = false;
          groupBoxHcwRepeatDelay.Enabled = false;
        }

        string exePath = InputDevices.irremote.GetHCWPath();
        string dllPath = InputDevices.irremote.GetDllPath();

        if (File.Exists(exePath + "Ir.exe"))
        {
          FileVersionInfo exeVersionInfo = FileVersionInfo.GetVersionInfo(exePath + "Ir.exe");
          if (exeVersionInfo.FileVersion.CompareTo(InputDevices.irremote.CurrentVersion) < 0)
          {
            labelHcwDriverStatus.ForeColor = Color.Red;
            labelHcwDriverStatus.Text = errHcwOutOfDate;
            linkLabelHcwDownload.Visible = true;
          }
        }
        else
        {
          labelHcwDriverStatus.ForeColor = Color.Red;
          labelHcwDriverStatus.Text = errHcwMissingExe;
          linkLabelHcwDownload.Visible = true;
          checkBoxHcwAllowExternal.Enabled = false;
          checkBoxHcwKeepControl.Enabled = false;
        }

        if (File.Exists(dllPath + "irremote.DLL"))
        {
          FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath + "irremote.DLL");
          if (dllVersionInfo.FileVersion.CompareTo(InputDevices.irremote.CurrentVersion) < 0)
          {
            labelHcwDriverStatus.ForeColor = Color.Red;
            labelHcwDriverStatus.Text = errHcwOutOfDate;
            linkLabelHcwDownload.Visible = true;
          }
        }
        else
        {
          labelHcwDriverStatus.ForeColor = Color.Red;
          labelHcwDriverStatus.Text = errHcwNotInstalled;
          linkLabelHcwDownload.Visible = true;
          checkBoxHcwEnabled.Enabled = false;
          groupBoxHcwSettings.Enabled = false;
          groupBoxHcwRepeatDelay.Enabled = false;
          using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          {
            xmlwriter.SetValueAsBool("remote", "HCW", false);
          }
        }
        buttonHcwMapping.Enabled = checkBoxHcwEnabled.Checked;

        toolTip.SetToolTip(this.hScrollBarHcwButtonRelease, string.Format("{0} msec.", hScrollBarHcwButtonRelease.Value));
        toolTip.SetToolTip(this.hScrollBarHcwRepeatFilter, hScrollBarHcwRepeatFilter.Value.ToString());
        Type repeatSpeed = typeof(hcwRepeatSpeed);
        toolTip.SetToolTip(this.hScrollBarHcwRepeatSpeed, Enum.GetName(repeatSpeed, 2 - hScrollBarHcwRepeatSpeed.Value));
        toolTip.SetToolTip(this.checkBoxHcwKeepControl, "If checked, MediaPortal keeps control of the remote. Only applications launched by\nMediaPortal can steal focus (external Players, MyPrograms, ...).");
        toolTip.SetToolTip(this.checkBoxHcwAllowExternal, "If checked, MediaPortal does not keep control of the remote\nwhen it looses focus.");

        #endregion

        #region X10

        checkBoxX10Enabled.Checked = xmlreader.GetValueAsBool("remote", "X10", false);
        radioButtonX10Medion.Checked = xmlreader.GetValueAsBool("remote", "X10Medion", true);
        radioButtonX10Ati.Checked = xmlreader.GetValueAsBool("remote", "X10ATI", false);
        radioButtonX10Other.Checked = (!radioButtonX10Medion.Checked && !radioButtonX10Ati.Checked);
        checkBoxX10ExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "X10VerboseLog", false);
        checkBoxX10ChannelControl.Checked = xmlreader.GetValueAsBool("remote", "X10UseChannelControl", false);
        x10Channel = xmlreader.GetValueAsInt("remote", "X10Channel", 0);
        radioButtonX10Medion.Enabled = radioButtonX10Ati.Enabled = radioButtonX10Other.Enabled = buttonX10Mapping.Enabled = groupBoxX10Settings.Enabled = checkBoxX10Enabled.Checked;
        buttonX10LearnChannel.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;

        #endregion

        #region General HID

        checkBoxHidEnabled.Checked = xmlreader.GetValueAsBool("remote", "HID", false);
        checkBoxHidExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "HIDVerboseLog", false);
        buttonHidMapping.Enabled = checkBoxHidEnabled.Checked;
        groupBoxHidSettings.Enabled = checkBoxHidEnabled.Checked;

        #endregion

      }

      #region FireDTV

      fireDtvRemote.LoadSettings();

      #endregion
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        #region MCE

        xmlwriter.SetValueAsBool("remote", "MCE", checkBoxMceEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "MCEVerboseLog", checkBoxMceExtendedLogging.Checked);

        if (radioButtonMceTeletext.Checked)
          xmlwriter.SetValue("remote", "MCEType", "Teletext");
        else if (radioButtonMceNoTeletext.Checked)
          xmlwriter.SetValue("remote", "MCEType", "NoTeletext");
        else
          xmlwriter.SetValue("remote", "MCEType", "General");

        //xmlwriter.SetValueAsBool("remote", "mce2005", checkBoxMceEnabled.Checked);
        //xmlwriter.SetValueAsBool("remote", "USAModel", radioButtonMceUsa.Checked);

        #endregion

        #region HCW

        xmlwriter.SetValueAsBool("remote", "HCW", checkBoxHcwEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWAllowExternal", checkBoxHcwAllowExternal.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWKeepControl", checkBoxHcwKeepControl.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWVerboseLog", checkBoxHcwExtendedLogging.Checked);
        xmlwriter.SetValue("remote", "HCWButtonRelease", hScrollBarHcwButtonRelease.Value);
        xmlwriter.SetValue("remote", "HCWRepeatFilter", hScrollBarHcwRepeatFilter.Value);
        xmlwriter.SetValue("remote", "HCWRepeatSpeed", hScrollBarHcwRepeatSpeed.Value);
        xmlwriter.SetValueAsBool("remote", "HCWFilterDoubleKlicks", checkBoxHcwFilterDoubleKlicks.Checked);

        #endregion

        #region X10

        xmlwriter.SetValueAsBool("remote", "X10", checkBoxX10Enabled.Checked);
        xmlwriter.SetValueAsBool("remote", "X10Medion", radioButtonX10Medion.Checked);
        xmlwriter.SetValueAsBool("remote", "X10ATI", radioButtonX10Ati.Checked);
        xmlwriter.SetValueAsBool("remote", "X10VerboseLog", checkBoxX10ExtendedLogging.Checked);
        xmlwriter.SetValueAsBool("remote", "X10UseChannelControl", checkBoxX10ChannelControl.Checked);
        xmlwriter.SetValue("remote", "X10Channel", x10Channel);

        #endregion

        #region General HID

        xmlwriter.SetValueAsBool("remote", "HID", checkBoxHidEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "HIDVerboseLog", checkBoxHidExtendedLogging.Checked);

        #endregion
      }

      #region FireDTV

      fireDtvRemote.SaveSettings();

      #endregion
    }

    #endregion

    #region Windows Form

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
      this.pictureBoxMceUsa = new System.Windows.Forms.PictureBox();
      this.checkBoxMceEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlRemotes = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageMce = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxMceSettings = new System.Windows.Forms.GroupBox();
      this.checkBoxMceExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonMceDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxMceType = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonMceGeneral = new System.Windows.Forms.RadioButton();
      this.radioButtonMceNoTeletext = new System.Windows.Forms.RadioButton();
      this.radioButtonMceTeletext = new System.Windows.Forms.RadioButton();
      this.groupBoxMceGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonMceMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxMceGeneralx = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonMceEurope = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.pictureBoxMceEurope = new System.Windows.Forms.PictureBox();
      this.checkBoxMceColouredButtons = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButtonMceUsa = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.tabPageHcw = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxHcwRepeatDelay = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.hScrollBarHcwRepeatSpeed = new System.Windows.Forms.HScrollBar();
      this.labelHcwFast = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwRepeatSpeed = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwSlow = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarHcwRepeatFilter = new System.Windows.Forms.HScrollBar();
      this.labelHcwMax = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwRepeatFilter = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwMin = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarHcwButtonRelease = new System.Windows.Forms.HScrollBar();
      this.labelHcw1000msec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcwButtonRelease = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHcw20msec = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxHcwGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonHcwMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxHcwEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxHcwStatus = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelHcwDownload = new System.Windows.Forms.LinkLabel();
      this.buttonHcwDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelHcwDriverStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxHcwSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxHcwFilterDoubleKlicks = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwAllowExternal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwKeepControl = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageX10 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxX10Status = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelX10DriverInfo = new System.Windows.Forms.Label();
      this.linkLabelDownloadX10 = new System.Windows.Forms.LinkLabel();
      this.labelX10Status = new System.Windows.Forms.Label();
      this.groupBoxX10General = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonX10Other = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonX10Ati = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonX10Medion = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.buttonX10Mapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxX10Enabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonX10Defaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxX10Settings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonX10LearnChannel = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxX10ChannelControl = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxX10ExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageHid = new System.Windows.Forms.TabPage();
      this.buttonHidDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxHidSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxHidExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxHidInfo = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelHidInfo = new System.Windows.Forms.Label();
      this.groupBoxHidGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonHidMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxHidEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageFireDtv = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.fireDtvRemote = new MediaPortal.Configuration.Sections.FireDtvRemote();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMceUsa)).BeginInit();
      this.tabControlRemotes.SuspendLayout();
      this.tabPageMce.SuspendLayout();
      this.groupBoxMceSettings.SuspendLayout();
      this.groupBoxMceType.SuspendLayout();
      this.groupBoxMceGeneral.SuspendLayout();
      this.groupBoxMceGeneralx.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMceEurope)).BeginInit();
      this.tabPageHcw.SuspendLayout();
      this.groupBoxHcwRepeatDelay.SuspendLayout();
      this.groupBoxHcwGeneral.SuspendLayout();
      this.groupBoxHcwStatus.SuspendLayout();
      this.groupBoxHcwSettings.SuspendLayout();
      this.tabPageX10.SuspendLayout();
      this.groupBoxX10Status.SuspendLayout();
      this.groupBoxX10General.SuspendLayout();
      this.groupBoxX10Settings.SuspendLayout();
      this.tabPageHid.SuspendLayout();
      this.groupBoxHidSettings.SuspendLayout();
      this.groupBoxHidInfo.SuspendLayout();
      this.groupBoxHidGeneral.SuspendLayout();
      this.tabPageFireDtv.SuspendLayout();
      this.SuspendLayout();
      // 
      // pictureBoxMceUsa
      // 
      this.pictureBoxMceUsa.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxMceUsa.Image")));
      this.pictureBoxMceUsa.Location = new System.Drawing.Point(208, 16);
      this.pictureBoxMceUsa.Name = "pictureBoxMceUsa";
      this.pictureBoxMceUsa.Size = new System.Drawing.Size(100, 310);
      this.pictureBoxMceUsa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxMceUsa.TabIndex = 3;
      this.pictureBoxMceUsa.TabStop = false;
      // 
      // checkBoxMceEnabled
      // 
      this.checkBoxMceEnabled.AutoSize = true;
      this.checkBoxMceEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxMceEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxMceEnabled.Name = "checkBoxMceEnabled";
      this.checkBoxMceEnabled.Size = new System.Drawing.Size(150, 17);
      this.checkBoxMceEnabled.TabIndex = 0;
      this.checkBoxMceEnabled.Text = "Use Microsoft MCE remote";
      this.checkBoxMceEnabled.UseVisualStyleBackColor = true;
      this.checkBoxMceEnabled.CheckedChanged += new System.EventHandler(this.checkBoxMceEnabled_CheckedChanged);
      // 
      // tabControlRemotes
      // 
      this.tabControlRemotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlRemotes.Controls.Add(this.tabPageMce);
      this.tabControlRemotes.Controls.Add(this.tabPageHcw);
      this.tabControlRemotes.Controls.Add(this.tabPageX10);
      this.tabControlRemotes.Controls.Add(this.tabPageHid);
      this.tabControlRemotes.Controls.Add(this.tabPageFireDtv);
      this.tabControlRemotes.Location = new System.Drawing.Point(0, 8);
      this.tabControlRemotes.Name = "tabControlRemotes";
      this.tabControlRemotes.SelectedIndex = 0;
      this.tabControlRemotes.Size = new System.Drawing.Size(472, 400);
      this.tabControlRemotes.TabIndex = 0;
      // 
      // tabPageMce
      // 
      this.tabPageMce.Controls.Add(this.groupBoxMceSettings);
      this.tabPageMce.Controls.Add(this.buttonMceDefaults);
      this.tabPageMce.Controls.Add(this.groupBoxMceType);
      this.tabPageMce.Controls.Add(this.groupBoxMceGeneral);
      this.tabPageMce.Controls.Add(this.groupBoxMceGeneralx);
      this.tabPageMce.Location = new System.Drawing.Point(4, 22);
      this.tabPageMce.Name = "tabPageMce";
      this.tabPageMce.Size = new System.Drawing.Size(464, 374);
      this.tabPageMce.TabIndex = 0;
      this.tabPageMce.Text = "Microsoft MCE";
      this.tabPageMce.UseVisualStyleBackColor = true;
      // 
      // groupBoxMceSettings
      // 
      this.groupBoxMceSettings.Controls.Add(this.checkBoxMceExtendedLogging);
      this.groupBoxMceSettings.Location = new System.Drawing.Point(12, 184);
      this.groupBoxMceSettings.Name = "groupBoxMceSettings";
      this.groupBoxMceSettings.Size = new System.Drawing.Size(440, 56);
      this.groupBoxMceSettings.TabIndex = 5;
      this.groupBoxMceSettings.TabStop = false;
      this.groupBoxMceSettings.Text = "Settings";
      // 
      // checkBoxMceExtendedLogging
      // 
      this.checkBoxMceExtendedLogging.AutoSize = true;
      this.checkBoxMceExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxMceExtendedLogging.Location = new System.Drawing.Point(16, 24);
      this.checkBoxMceExtendedLogging.Name = "checkBoxMceExtendedLogging";
      this.checkBoxMceExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxMceExtendedLogging.TabIndex = 3;
      this.checkBoxMceExtendedLogging.Text = "Extended logging";
      this.checkBoxMceExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // buttonMceDefaults
      // 
      this.buttonMceDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMceDefaults.Location = new System.Drawing.Point(364, 328);
      this.buttonMceDefaults.Name = "buttonMceDefaults";
      this.buttonMceDefaults.Size = new System.Drawing.Size(72, 22);
      this.buttonMceDefaults.TabIndex = 7;
      this.buttonMceDefaults.Text = "&Defaults";
      this.buttonMceDefaults.UseVisualStyleBackColor = true;
      this.buttonMceDefaults.Click += new System.EventHandler(this.buttonMceDefaults_Click);
      // 
      // groupBoxMceType
      // 
      this.groupBoxMceType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMceType.Controls.Add(this.radioButtonMceGeneral);
      this.groupBoxMceType.Controls.Add(this.radioButtonMceNoTeletext);
      this.groupBoxMceType.Controls.Add(this.radioButtonMceTeletext);
      this.groupBoxMceType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMceType.Location = new System.Drawing.Point(12, 72);
      this.groupBoxMceType.Name = "groupBoxMceType";
      this.groupBoxMceType.Size = new System.Drawing.Size(440, 104);
      this.groupBoxMceType.TabIndex = 6;
      this.groupBoxMceType.TabStop = false;
      this.groupBoxMceType.Text = "MCE device type";
      // 
      // radioButtonMceGeneral
      // 
      this.radioButtonMceGeneral.AutoSize = true;
      this.radioButtonMceGeneral.Location = new System.Drawing.Point(16, 72);
      this.radioButtonMceGeneral.Name = "radioButtonMceGeneral";
      this.radioButtonMceGeneral.Size = new System.Drawing.Size(208, 17);
      this.radioButtonMceGeneral.TabIndex = 7;
      this.radioButtonMceGeneral.TabStop = true;
      this.radioButtonMceGeneral.Text = "Keyboard or other general MCE device";
      this.radioButtonMceGeneral.UseVisualStyleBackColor = true;
      // 
      // radioButtonMceNoTeletext
      // 
      this.radioButtonMceNoTeletext.AutoSize = true;
      this.radioButtonMceNoTeletext.Location = new System.Drawing.Point(16, 48);
      this.radioButtonMceNoTeletext.Name = "radioButtonMceNoTeletext";
      this.radioButtonMceNoTeletext.Size = new System.Drawing.Size(242, 17);
      this.radioButtonMceNoTeletext.TabIndex = 6;
      this.radioButtonMceNoTeletext.TabStop = true;
      this.radioButtonMceNoTeletext.Text = "Remote without coloured teletext buttons (US)";
      this.radioButtonMceNoTeletext.UseVisualStyleBackColor = true;
      // 
      // radioButtonMceTeletext
      // 
      this.radioButtonMceTeletext.AutoSize = true;
      this.radioButtonMceTeletext.Location = new System.Drawing.Point(16, 24);
      this.radioButtonMceTeletext.Name = "radioButtonMceTeletext";
      this.radioButtonMceTeletext.Size = new System.Drawing.Size(248, 17);
      this.radioButtonMceTeletext.TabIndex = 5;
      this.radioButtonMceTeletext.TabStop = true;
      this.radioButtonMceTeletext.Text = "Remote with coloured teletext buttons (non-US)";
      this.radioButtonMceTeletext.UseVisualStyleBackColor = true;
      // 
      // groupBoxMceGeneral
      // 
      this.groupBoxMceGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMceGeneral.Controls.Add(this.checkBoxMceEnabled);
      this.groupBoxMceGeneral.Controls.Add(this.buttonMceMapping);
      this.groupBoxMceGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMceGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxMceGeneral.Name = "groupBoxMceGeneral";
      this.groupBoxMceGeneral.Size = new System.Drawing.Size(440, 56);
      this.groupBoxMceGeneral.TabIndex = 2;
      this.groupBoxMceGeneral.TabStop = false;
      // 
      // buttonMceMapping
      // 
      this.buttonMceMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMceMapping.Location = new System.Drawing.Point(352, 20);
      this.buttonMceMapping.Name = "buttonMceMapping";
      this.buttonMceMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonMceMapping.TabIndex = 1;
      this.buttonMceMapping.Text = "Mapping";
      this.buttonMceMapping.UseVisualStyleBackColor = true;
      this.buttonMceMapping.Click += new System.EventHandler(this.buttonMceMapping_Click);
      // 
      // groupBoxMceGeneralx
      // 
      this.groupBoxMceGeneralx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMceGeneralx.Controls.Add(this.radioButtonMceEurope);
      this.groupBoxMceGeneralx.Controls.Add(this.pictureBoxMceUsa);
      this.groupBoxMceGeneralx.Controls.Add(this.pictureBoxMceEurope);
      this.groupBoxMceGeneralx.Controls.Add(this.checkBoxMceColouredButtons);
      this.groupBoxMceGeneralx.Controls.Add(this.radioButtonMceUsa);
      this.groupBoxMceGeneralx.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMceGeneralx.Location = new System.Drawing.Point(12, 160);
      this.groupBoxMceGeneralx.Name = "groupBoxMceGeneralx";
      this.groupBoxMceGeneralx.Size = new System.Drawing.Size(324, 240);
      this.groupBoxMceGeneralx.TabIndex = 0;
      this.groupBoxMceGeneralx.TabStop = false;
      this.groupBoxMceGeneralx.Visible = false;
      // 
      // radioButtonMceEurope
      // 
      this.radioButtonMceEurope.AutoSize = true;
      this.radioButtonMceEurope.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonMceEurope.Location = new System.Drawing.Point(8, 36);
      this.radioButtonMceEurope.Name = "radioButtonMceEurope";
      this.radioButtonMceEurope.Size = new System.Drawing.Size(107, 17);
      this.radioButtonMceEurope.TabIndex = 2;
      this.radioButtonMceEurope.Text = "European version";
      this.radioButtonMceEurope.UseVisualStyleBackColor = true;
      this.radioButtonMceEurope.CheckedChanged += new System.EventHandler(this.radioButtonMceEurope_CheckedChanged);
      // 
      // pictureBoxMceEurope
      // 
      this.pictureBoxMceEurope.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxMceEurope.Image")));
      this.pictureBoxMceEurope.Location = new System.Drawing.Point(208, 16);
      this.pictureBoxMceEurope.Name = "pictureBoxMceEurope";
      this.pictureBoxMceEurope.Size = new System.Drawing.Size(100, 310);
      this.pictureBoxMceEurope.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxMceEurope.TabIndex = 4;
      this.pictureBoxMceEurope.TabStop = false;
      // 
      // checkBoxMceColouredButtons
      // 
      this.checkBoxMceColouredButtons.AutoSize = true;
      this.checkBoxMceColouredButtons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxMceColouredButtons.Location = new System.Drawing.Point(24, 136);
      this.checkBoxMceColouredButtons.Name = "checkBoxMceColouredButtons";
      this.checkBoxMceColouredButtons.Size = new System.Drawing.Size(245, 17);
      this.checkBoxMceColouredButtons.TabIndex = 4;
      this.checkBoxMceColouredButtons.Text = "Remote has coloured teletext buttons (non-US)";
      this.checkBoxMceColouredButtons.UseVisualStyleBackColor = true;
      // 
      // radioButtonMceUsa
      // 
      this.radioButtonMceUsa.AutoSize = true;
      this.radioButtonMceUsa.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonMceUsa.Location = new System.Drawing.Point(8, 16);
      this.radioButtonMceUsa.Name = "radioButtonMceUsa";
      this.radioButtonMceUsa.Size = new System.Drawing.Size(203, 17);
      this.radioButtonMceUsa.TabIndex = 1;
      this.radioButtonMceUsa.Text = "USA version / Hauppauge MCE OEM";
      this.radioButtonMceUsa.UseVisualStyleBackColor = true;
      this.radioButtonMceUsa.CheckedChanged += new System.EventHandler(this.radioButtonMceUsa_CheckedChanged);
      // 
      // tabPageHcw
      // 
      this.tabPageHcw.Controls.Add(this.groupBoxHcwRepeatDelay);
      this.tabPageHcw.Controls.Add(this.groupBoxHcwGeneral);
      this.tabPageHcw.Controls.Add(this.groupBoxHcwStatus);
      this.tabPageHcw.Controls.Add(this.groupBoxHcwSettings);
      this.tabPageHcw.Location = new System.Drawing.Point(4, 22);
      this.tabPageHcw.Name = "tabPageHcw";
      this.tabPageHcw.Size = new System.Drawing.Size(464, 374);
      this.tabPageHcw.TabIndex = 1;
      this.tabPageHcw.Text = "Hauppauge";
      this.tabPageHcw.UseVisualStyleBackColor = true;
      // 
      // groupBoxHcwRepeatDelay
      // 
      this.groupBoxHcwRepeatDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwRepeatDelay.Controls.Add(this.hScrollBarHcwRepeatSpeed);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwFast);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwRepeatSpeed);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwSlow);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.hScrollBarHcwRepeatFilter);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwMax);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwRepeatFilter);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwMin);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.hScrollBarHcwButtonRelease);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcw1000msec);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcwButtonRelease);
      this.groupBoxHcwRepeatDelay.Controls.Add(this.labelHcw20msec);
      this.groupBoxHcwRepeatDelay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
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
      this.hScrollBarHcwRepeatSpeed.ValueChanged += new System.EventHandler(this.hScrollBarHcwRepeatSpeed_ValueChanged);
      // 
      // labelHcwFast
      // 
      this.labelHcwFast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcwFast.AutoSize = true;
      this.labelHcwFast.Location = new System.Drawing.Point(368, 82);
      this.labelHcwFast.Name = "labelHcwFast";
      this.labelHcwFast.Size = new System.Drawing.Size(24, 13);
      this.labelHcwFast.TabIndex = 11;
      this.labelHcwFast.Text = "fast";
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
      // labelHcwSlow
      // 
      this.labelHcwSlow.AutoSize = true;
      this.labelHcwSlow.Location = new System.Drawing.Point(123, 82);
      this.labelHcwSlow.Name = "labelHcwSlow";
      this.labelHcwSlow.Size = new System.Drawing.Size(28, 13);
      this.labelHcwSlow.TabIndex = 9;
      this.labelHcwSlow.Text = "slow";
      this.labelHcwSlow.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
      this.hScrollBarHcwRepeatFilter.ValueChanged += new System.EventHandler(this.hScrollBarHcwRepeatFilter_ValueChanged);
      // 
      // labelHcwMax
      // 
      this.labelHcwMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcwMax.AutoSize = true;
      this.labelHcwMax.Location = new System.Drawing.Point(368, 54);
      this.labelHcwMax.Name = "labelHcwMax";
      this.labelHcwMax.Size = new System.Drawing.Size(29, 13);
      this.labelHcwMax.TabIndex = 7;
      this.labelHcwMax.Text = "max.";
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
      // labelHcwMin
      // 
      this.labelHcwMin.AutoSize = true;
      this.labelHcwMin.Location = new System.Drawing.Point(128, 54);
      this.labelHcwMin.Name = "labelHcwMin";
      this.labelHcwMin.Size = new System.Drawing.Size(26, 13);
      this.labelHcwMin.TabIndex = 5;
      this.labelHcwMin.Text = "min.";
      this.labelHcwMin.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
      this.hScrollBarHcwButtonRelease.ValueChanged += new System.EventHandler(this.hScrollBarHcwButtonRelease_ValueChanged);
      // 
      // labelHcw1000msec
      // 
      this.labelHcw1000msec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcw1000msec.AutoSize = true;
      this.labelHcw1000msec.Location = new System.Drawing.Point(368, 27);
      this.labelHcw1000msec.Name = "labelHcw1000msec";
      this.labelHcw1000msec.Size = new System.Drawing.Size(62, 13);
      this.labelHcw1000msec.TabIndex = 3;
      this.labelHcw1000msec.Text = "1000 msec.";
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
      // labelHcw20msec
      // 
      this.labelHcw20msec.AutoSize = true;
      this.labelHcw20msec.Location = new System.Drawing.Point(103, 27);
      this.labelHcw20msec.Name = "labelHcw20msec";
      this.labelHcw20msec.Size = new System.Drawing.Size(50, 13);
      this.labelHcw20msec.TabIndex = 1;
      this.labelHcw20msec.Text = "20 msec.";
      this.labelHcw20msec.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // groupBoxHcwGeneral
      // 
      this.groupBoxHcwGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwGeneral.Controls.Add(this.buttonHcwMapping);
      this.groupBoxHcwGeneral.Controls.Add(this.checkBoxHcwEnabled);
      this.groupBoxHcwGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHcwGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxHcwGeneral.Name = "groupBoxHcwGeneral";
      this.groupBoxHcwGeneral.Size = new System.Drawing.Size(440, 56);
      this.groupBoxHcwGeneral.TabIndex = 0;
      this.groupBoxHcwGeneral.TabStop = false;
      // 
      // buttonHcwMapping
      // 
      this.buttonHcwMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonHcwMapping.Location = new System.Drawing.Point(352, 20);
      this.buttonHcwMapping.Name = "buttonHcwMapping";
      this.buttonHcwMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonHcwMapping.TabIndex = 1;
      this.buttonHcwMapping.Text = "Mapping";
      this.buttonHcwMapping.UseVisualStyleBackColor = true;
      this.buttonHcwMapping.Click += new System.EventHandler(this.buttonHcwMapping_Click);
      // 
      // checkBoxHcwEnabled
      // 
      this.checkBoxHcwEnabled.AutoSize = true;
      this.checkBoxHcwEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHcwEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHcwEnabled.Name = "checkBoxHcwEnabled";
      this.checkBoxHcwEnabled.Size = new System.Drawing.Size(137, 17);
      this.checkBoxHcwEnabled.TabIndex = 0;
      this.checkBoxHcwEnabled.Text = "Use Hauppauge remote";
      this.checkBoxHcwEnabled.UseVisualStyleBackColor = true;
      this.checkBoxHcwEnabled.CheckedChanged += new System.EventHandler(this.checkBoxHcwEnabled_CheckedChanged);
      // 
      // groupBoxHcwStatus
      // 
      this.groupBoxHcwStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwStatus.Controls.Add(this.linkLabelHcwDownload);
      this.groupBoxHcwStatus.Controls.Add(this.buttonHcwDefaults);
      this.groupBoxHcwStatus.Controls.Add(this.labelHcwDriverStatus);
      this.groupBoxHcwStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHcwStatus.Location = new System.Drawing.Point(12, 280);
      this.groupBoxHcwStatus.Name = "groupBoxHcwStatus";
      this.groupBoxHcwStatus.Size = new System.Drawing.Size(440, 80);
      this.groupBoxHcwStatus.TabIndex = 3;
      this.groupBoxHcwStatus.TabStop = false;
      this.groupBoxHcwStatus.Text = "Status";
      // 
      // linkLabelHcwDownload
      // 
      this.linkLabelHcwDownload.AutoSize = true;
      this.linkLabelHcwDownload.Location = new System.Drawing.Point(12, 54);
      this.linkLabelHcwDownload.Name = "linkLabelHcwDownload";
      this.linkLabelHcwDownload.Size = new System.Drawing.Size(152, 13);
      this.linkLabelHcwDownload.TabIndex = 2;
      this.linkLabelHcwDownload.TabStop = true;
      this.linkLabelHcwDownload.Text = "Click here for the latest drivers.";
      this.linkLabelHcwDownload.Visible = false;
      this.linkLabelHcwDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHcwDownload_LinkClicked);
      // 
      // buttonHcwDefaults
      // 
      this.buttonHcwDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonHcwDefaults.Location = new System.Drawing.Point(352, 48);
      this.buttonHcwDefaults.Name = "buttonHcwDefaults";
      this.buttonHcwDefaults.Size = new System.Drawing.Size(72, 22);
      this.buttonHcwDefaults.TabIndex = 1;
      this.buttonHcwDefaults.Text = "&Defaults";
      this.buttonHcwDefaults.UseVisualStyleBackColor = true;
      this.buttonHcwDefaults.Click += new System.EventHandler(this.buttonHcwDefaults_Click);
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
      // groupBoxHcwSettings
      // 
      this.groupBoxHcwSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHcwSettings.Controls.Add(this.checkBoxHcwFilterDoubleKlicks);
      this.groupBoxHcwSettings.Controls.Add(this.checkBoxHcwAllowExternal);
      this.groupBoxHcwSettings.Controls.Add(this.checkBoxHcwKeepControl);
      this.groupBoxHcwSettings.Controls.Add(this.checkBoxHcwExtendedLogging);
      this.groupBoxHcwSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHcwSettings.Location = new System.Drawing.Point(12, 72);
      this.groupBoxHcwSettings.Name = "groupBoxHcwSettings";
      this.groupBoxHcwSettings.Size = new System.Drawing.Size(440, 80);
      this.groupBoxHcwSettings.TabIndex = 1;
      this.groupBoxHcwSettings.TabStop = false;
      this.groupBoxHcwSettings.Text = "Settings";
      // 
      // checkBoxHcwFilterDoubleKlicks
      // 
      this.checkBoxHcwFilterDoubleKlicks.AutoSize = true;
      this.checkBoxHcwFilterDoubleKlicks.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHcwFilterDoubleKlicks.Location = new System.Drawing.Point(288, 24);
      this.checkBoxHcwFilterDoubleKlicks.Name = "checkBoxHcwFilterDoubleKlicks";
      this.checkBoxHcwFilterDoubleKlicks.Size = new System.Drawing.Size(108, 17);
      this.checkBoxHcwFilterDoubleKlicks.TabIndex = 1;
      this.checkBoxHcwFilterDoubleKlicks.Text = "Filter doubleclicks";
      this.checkBoxHcwFilterDoubleKlicks.UseVisualStyleBackColor = true;
      // 
      // checkBoxHcwAllowExternal
      // 
      this.checkBoxHcwAllowExternal.AutoSize = true;
      this.checkBoxHcwAllowExternal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHcwAllowExternal.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHcwAllowExternal.Name = "checkBoxHcwAllowExternal";
      this.checkBoxHcwAllowExternal.Size = new System.Drawing.Size(243, 17);
      this.checkBoxHcwAllowExternal.TabIndex = 0;
      this.checkBoxHcwAllowExternal.Text = "External processes may use the remote control";
      this.checkBoxHcwAllowExternal.UseVisualStyleBackColor = true;
      this.checkBoxHcwAllowExternal.CheckedChanged += new System.EventHandler(this.checkBoxHcwAllowExternal_CheckedChanged);
      // 
      // checkBoxHcwKeepControl
      // 
      this.checkBoxHcwKeepControl.AutoSize = true;
      this.checkBoxHcwKeepControl.Enabled = false;
      this.checkBoxHcwKeepControl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHcwKeepControl.Location = new System.Drawing.Point(32, 48);
      this.checkBoxHcwKeepControl.Name = "checkBoxHcwKeepControl";
      this.checkBoxHcwKeepControl.Size = new System.Drawing.Size(194, 17);
      this.checkBoxHcwKeepControl.TabIndex = 2;
      this.checkBoxHcwKeepControl.Text = "Keep control when MP looses focus";
      this.checkBoxHcwKeepControl.UseVisualStyleBackColor = true;
      // 
      // checkBoxHcwExtendedLogging
      // 
      this.checkBoxHcwExtendedLogging.AutoSize = true;
      this.checkBoxHcwExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHcwExtendedLogging.Location = new System.Drawing.Point(288, 48);
      this.checkBoxHcwExtendedLogging.Name = "checkBoxHcwExtendedLogging";
      this.checkBoxHcwExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxHcwExtendedLogging.TabIndex = 3;
      this.checkBoxHcwExtendedLogging.Text = "Extended logging";
      this.checkBoxHcwExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // tabPageX10
      // 
      this.tabPageX10.Controls.Add(this.groupBoxX10Status);
      this.tabPageX10.Controls.Add(this.groupBoxX10General);
      this.tabPageX10.Controls.Add(this.buttonX10Defaults);
      this.tabPageX10.Controls.Add(this.groupBoxX10Settings);
      this.tabPageX10.Location = new System.Drawing.Point(4, 22);
      this.tabPageX10.Name = "tabPageX10";
      this.tabPageX10.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageX10.Size = new System.Drawing.Size(464, 374);
      this.tabPageX10.TabIndex = 3;
      this.tabPageX10.Text = "X10";
      this.tabPageX10.UseVisualStyleBackColor = true;
      // 
      // groupBoxX10Status
      // 
      this.groupBoxX10Status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10Status.Controls.Add(this.labelX10DriverInfo);
      this.groupBoxX10Status.Controls.Add(this.linkLabelDownloadX10);
      this.groupBoxX10Status.Controls.Add(this.labelX10Status);
      this.groupBoxX10Status.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxX10Status.Location = new System.Drawing.Point(12, 228);
      this.groupBoxX10Status.Name = "groupBoxX10Status";
      this.groupBoxX10Status.Size = new System.Drawing.Size(440, 80);
      this.groupBoxX10Status.TabIndex = 4;
      this.groupBoxX10Status.TabStop = false;
      this.groupBoxX10Status.Text = "Status";
      // 
      // labelX10DriverInfo
      // 
      this.labelX10DriverInfo.AutoSize = true;
      this.labelX10DriverInfo.Location = new System.Drawing.Point(16, 24);
      this.labelX10DriverInfo.Name = "labelX10DriverInfo";
      this.labelX10DriverInfo.Size = new System.Drawing.Size(392, 13);
      this.labelX10DriverInfo.TabIndex = 6;
      this.labelX10DriverInfo.Text = "You have to use the driver below, or your remote might not work with MediaPortal." +
          "";
      // 
      // linkLabelDownloadX10
      // 
      this.linkLabelDownloadX10.AutoSize = true;
      this.linkLabelDownloadX10.Location = new System.Drawing.Point(16, 44);
      this.linkLabelDownloadX10.Name = "linkLabelDownloadX10";
      this.linkLabelDownloadX10.Size = new System.Drawing.Size(222, 13);
      this.linkLabelDownloadX10.TabIndex = 5;
      this.linkLabelDownloadX10.TabStop = true;
      this.linkLabelDownloadX10.Text = "Click here to download the X10 remote driver.";
      this.linkLabelDownloadX10.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDownloadX10_LinkClicked);
      // 
      // labelX10Status
      // 
      this.labelX10Status.AutoSize = true;
      this.labelX10Status.Location = new System.Drawing.Point(16, 24);
      this.labelX10Status.Name = "labelX10Status";
      this.labelX10Status.Size = new System.Drawing.Size(35, 13);
      this.labelX10Status.TabIndex = 5;
      this.labelX10Status.Text = "label2";
      this.labelX10Status.Visible = false;
      // 
      // groupBoxX10General
      // 
      this.groupBoxX10General.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Other);
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Ati);
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Medion);
      this.groupBoxX10General.Controls.Add(this.buttonX10Mapping);
      this.groupBoxX10General.Controls.Add(this.checkBoxX10Enabled);
      this.groupBoxX10General.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxX10General.Location = new System.Drawing.Point(12, 8);
      this.groupBoxX10General.Name = "groupBoxX10General";
      this.groupBoxX10General.Size = new System.Drawing.Size(440, 120);
      this.groupBoxX10General.TabIndex = 2;
      this.groupBoxX10General.TabStop = false;
      // 
      // radioButtonX10Other
      // 
      this.radioButtonX10Other.AutoSize = true;
      this.radioButtonX10Other.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Other.Location = new System.Drawing.Point(32, 88);
      this.radioButtonX10Other.Name = "radioButtonX10Other";
      this.radioButtonX10Other.Size = new System.Drawing.Size(48, 17);
      this.radioButtonX10Other.TabIndex = 5;
      this.radioButtonX10Other.Text = "other";
      this.radioButtonX10Other.UseVisualStyleBackColor = true;
      // 
      // radioButtonX10Ati
      // 
      this.radioButtonX10Ati.AutoSize = true;
      this.radioButtonX10Ati.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Ati.Location = new System.Drawing.Point(32, 68);
      this.radioButtonX10Ati.Name = "radioButtonX10Ati";
      this.radioButtonX10Ati.Size = new System.Drawing.Size(72, 17);
      this.radioButtonX10Ati.TabIndex = 4;
      this.radioButtonX10Ati.Text = "ATI model";
      this.radioButtonX10Ati.UseVisualStyleBackColor = true;
      // 
      // radioButtonX10Medion
      // 
      this.radioButtonX10Medion.AutoSize = true;
      this.radioButtonX10Medion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Medion.Location = new System.Drawing.Point(32, 48);
      this.radioButtonX10Medion.Name = "radioButtonX10Medion";
      this.radioButtonX10Medion.Size = new System.Drawing.Size(90, 17);
      this.radioButtonX10Medion.TabIndex = 3;
      this.radioButtonX10Medion.Text = "Medion model";
      this.radioButtonX10Medion.UseVisualStyleBackColor = true;
      // 
      // buttonX10Mapping
      // 
      this.buttonX10Mapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10Mapping.Location = new System.Drawing.Point(352, 20);
      this.buttonX10Mapping.Name = "buttonX10Mapping";
      this.buttonX10Mapping.Size = new System.Drawing.Size(72, 22);
      this.buttonX10Mapping.TabIndex = 1;
      this.buttonX10Mapping.Text = "Mapping";
      this.buttonX10Mapping.UseVisualStyleBackColor = true;
      this.buttonX10Mapping.Click += new System.EventHandler(this.buttonX10Mapping_Click);
      // 
      // checkBoxX10Enabled
      // 
      this.checkBoxX10Enabled.AutoSize = true;
      this.checkBoxX10Enabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxX10Enabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxX10Enabled.Name = "checkBoxX10Enabled";
      this.checkBoxX10Enabled.Size = new System.Drawing.Size(100, 17);
      this.checkBoxX10Enabled.TabIndex = 0;
      this.checkBoxX10Enabled.Text = "Use X10 remote";
      this.checkBoxX10Enabled.UseVisualStyleBackColor = true;
      this.checkBoxX10Enabled.CheckedChanged += new System.EventHandler(this.checkBoxX10Enabled_CheckedChanged);
      // 
      // buttonX10Defaults
      // 
      this.buttonX10Defaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10Defaults.Location = new System.Drawing.Point(364, 328);
      this.buttonX10Defaults.Name = "buttonX10Defaults";
      this.buttonX10Defaults.Size = new System.Drawing.Size(72, 22);
      this.buttonX10Defaults.TabIndex = 1;
      this.buttonX10Defaults.Text = "&Defaults";
      this.buttonX10Defaults.UseVisualStyleBackColor = true;
      this.buttonX10Defaults.Click += new System.EventHandler(this.buttonX10Defaults_Click);
      // 
      // groupBoxX10Settings
      // 
      this.groupBoxX10Settings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10Settings.Controls.Add(this.buttonX10LearnChannel);
      this.groupBoxX10Settings.Controls.Add(this.checkBoxX10ChannelControl);
      this.groupBoxX10Settings.Controls.Add(this.checkBoxX10ExtendedLogging);
      this.groupBoxX10Settings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxX10Settings.Location = new System.Drawing.Point(12, 136);
      this.groupBoxX10Settings.Name = "groupBoxX10Settings";
      this.groupBoxX10Settings.Size = new System.Drawing.Size(440, 80);
      this.groupBoxX10Settings.TabIndex = 3;
      this.groupBoxX10Settings.TabStop = false;
      this.groupBoxX10Settings.Text = "Settings";
      // 
      // buttonX10LearnChannel
      // 
      this.buttonX10LearnChannel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10LearnChannel.Location = new System.Drawing.Point(352, 44);
      this.buttonX10LearnChannel.Name = "buttonX10LearnChannel";
      this.buttonX10LearnChannel.Size = new System.Drawing.Size(72, 22);
      this.buttonX10LearnChannel.TabIndex = 5;
      this.buttonX10LearnChannel.Text = "&Learn";
      this.buttonX10LearnChannel.UseVisualStyleBackColor = true;
      this.buttonX10LearnChannel.Click += new System.EventHandler(this.buttonX10LearnChannel_Click);
      // 
      // checkBoxX10ChannelControl
      // 
      this.checkBoxX10ChannelControl.AutoSize = true;
      this.checkBoxX10ChannelControl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxX10ChannelControl.Location = new System.Drawing.Point(16, 48);
      this.checkBoxX10ChannelControl.Name = "checkBoxX10ChannelControl";
      this.checkBoxX10ChannelControl.Size = new System.Drawing.Size(136, 17);
      this.checkBoxX10ChannelControl.TabIndex = 4;
      this.checkBoxX10ChannelControl.Text = "Use RF channel control";
      this.checkBoxX10ChannelControl.UseVisualStyleBackColor = true;
      this.checkBoxX10ChannelControl.CheckedChanged += new System.EventHandler(this.checkBoxX10ChannelControl_CheckedChanged);
      // 
      // checkBoxX10ExtendedLogging
      // 
      this.checkBoxX10ExtendedLogging.AutoSize = true;
      this.checkBoxX10ExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxX10ExtendedLogging.Location = new System.Drawing.Point(16, 24);
      this.checkBoxX10ExtendedLogging.Name = "checkBoxX10ExtendedLogging";
      this.checkBoxX10ExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxX10ExtendedLogging.TabIndex = 3;
      this.checkBoxX10ExtendedLogging.Text = "Extended logging";
      this.checkBoxX10ExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // tabPageHid
      // 
      this.tabPageHid.Controls.Add(this.buttonHidDefaults);
      this.tabPageHid.Controls.Add(this.groupBoxHidSettings);
      this.tabPageHid.Controls.Add(this.groupBoxHidInfo);
      this.tabPageHid.Controls.Add(this.groupBoxHidGeneral);
      this.tabPageHid.Location = new System.Drawing.Point(4, 22);
      this.tabPageHid.Name = "tabPageHid";
      this.tabPageHid.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageHid.Size = new System.Drawing.Size(464, 374);
      this.tabPageHid.TabIndex = 4;
      this.tabPageHid.Text = "General HID";
      this.tabPageHid.UseVisualStyleBackColor = true;
      // 
      // buttonHidDefaults
      // 
      this.buttonHidDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonHidDefaults.Location = new System.Drawing.Point(364, 328);
      this.buttonHidDefaults.Name = "buttonHidDefaults";
      this.buttonHidDefaults.Size = new System.Drawing.Size(72, 22);
      this.buttonHidDefaults.TabIndex = 6;
      this.buttonHidDefaults.Text = "&Defaults";
      this.buttonHidDefaults.UseVisualStyleBackColor = true;
      this.buttonHidDefaults.Click += new System.EventHandler(this.buttonHidDefaults_Click);
      // 
      // groupBoxHidSettings
      // 
      this.groupBoxHidSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHidSettings.Controls.Add(this.checkBoxHidExtendedLogging);
      this.groupBoxHidSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHidSettings.Location = new System.Drawing.Point(12, 72);
      this.groupBoxHidSettings.Name = "groupBoxHidSettings";
      this.groupBoxHidSettings.Size = new System.Drawing.Size(440, 56);
      this.groupBoxHidSettings.TabIndex = 5;
      this.groupBoxHidSettings.TabStop = false;
      this.groupBoxHidSettings.Text = "Settings";
      // 
      // checkBoxHidExtendedLogging
      // 
      this.checkBoxHidExtendedLogging.AutoSize = true;
      this.checkBoxHidExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHidExtendedLogging.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHidExtendedLogging.Name = "checkBoxHidExtendedLogging";
      this.checkBoxHidExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxHidExtendedLogging.TabIndex = 3;
      this.checkBoxHidExtendedLogging.Text = "Extended logging";
      this.checkBoxHidExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // groupBoxHidInfo
      // 
      this.groupBoxHidInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHidInfo.Controls.Add(this.labelHidInfo);
      this.groupBoxHidInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHidInfo.Location = new System.Drawing.Point(12, 136);
      this.groupBoxHidInfo.Name = "groupBoxHidInfo";
      this.groupBoxHidInfo.Size = new System.Drawing.Size(440, 80);
      this.groupBoxHidInfo.TabIndex = 4;
      this.groupBoxHidInfo.TabStop = false;
      // 
      // labelHidInfo
      // 
      this.labelHidInfo.Location = new System.Drawing.Point(12, 24);
      this.labelHidInfo.Name = "labelHidInfo";
      this.labelHidInfo.Size = new System.Drawing.Size(420, 40);
      this.labelHidInfo.TabIndex = 0;
      this.labelHidInfo.Text = "Some keyboards and remotes natively support buttons like Play, Stop, Skip, etc.\r\n" +
          "\r\nCheck \"Use general HID device\" to enable those commands.";
      // 
      // groupBoxHidGeneral
      // 
      this.groupBoxHidGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHidGeneral.Controls.Add(this.buttonHidMapping);
      this.groupBoxHidGeneral.Controls.Add(this.checkBoxHidEnabled);
      this.groupBoxHidGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHidGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxHidGeneral.Name = "groupBoxHidGeneral";
      this.groupBoxHidGeneral.Size = new System.Drawing.Size(440, 56);
      this.groupBoxHidGeneral.TabIndex = 1;
      this.groupBoxHidGeneral.TabStop = false;
      // 
      // buttonHidMapping
      // 
      this.buttonHidMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonHidMapping.Location = new System.Drawing.Point(352, 20);
      this.buttonHidMapping.Name = "buttonHidMapping";
      this.buttonHidMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonHidMapping.TabIndex = 1;
      this.buttonHidMapping.Text = "Mapping";
      this.buttonHidMapping.UseVisualStyleBackColor = true;
      this.buttonHidMapping.Click += new System.EventHandler(this.buttonHidMapping_Click);
      // 
      // checkBoxHidEnabled
      // 
      this.checkBoxHidEnabled.AutoSize = true;
      this.checkBoxHidEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHidEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxHidEnabled.Name = "checkBoxHidEnabled";
      this.checkBoxHidEnabled.Size = new System.Drawing.Size(138, 17);
      this.checkBoxHidEnabled.TabIndex = 0;
      this.checkBoxHidEnabled.Text = "Use general HID device";
      this.checkBoxHidEnabled.UseVisualStyleBackColor = true;
      this.checkBoxHidEnabled.CheckedChanged += new System.EventHandler(this.checkBoxHidEnabled_CheckedChanged);
      // 
      // tabPageFireDtv
      // 
      this.tabPageFireDtv.Controls.Add(this.fireDtvRemote);
      this.tabPageFireDtv.Location = new System.Drawing.Point(4, 22);
      this.tabPageFireDtv.Name = "tabPageFireDtv";
      this.tabPageFireDtv.Size = new System.Drawing.Size(464, 374);
      this.tabPageFireDtv.TabIndex = 2;
      this.tabPageFireDtv.Text = "FireDTV";
      this.tabPageFireDtv.UseVisualStyleBackColor = true;
      // 
      // fireDtvRemote
      // 
      this.fireDtvRemote.AutoScroll = true;
      this.fireDtvRemote.Location = new System.Drawing.Point(0, 0);
      this.fireDtvRemote.Name = "fireDtvRemote";
      this.fireDtvRemote.Size = new System.Drawing.Size(520, 368);
      this.fireDtvRemote.TabIndex = 0;
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
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMceUsa)).EndInit();
      this.tabControlRemotes.ResumeLayout(false);
      this.tabPageMce.ResumeLayout(false);
      this.groupBoxMceSettings.ResumeLayout(false);
      this.groupBoxMceSettings.PerformLayout();
      this.groupBoxMceType.ResumeLayout(false);
      this.groupBoxMceType.PerformLayout();
      this.groupBoxMceGeneral.ResumeLayout(false);
      this.groupBoxMceGeneral.PerformLayout();
      this.groupBoxMceGeneralx.ResumeLayout(false);
      this.groupBoxMceGeneralx.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMceEurope)).EndInit();
      this.tabPageHcw.ResumeLayout(false);
      this.groupBoxHcwRepeatDelay.ResumeLayout(false);
      this.groupBoxHcwRepeatDelay.PerformLayout();
      this.groupBoxHcwGeneral.ResumeLayout(false);
      this.groupBoxHcwGeneral.PerformLayout();
      this.groupBoxHcwStatus.ResumeLayout(false);
      this.groupBoxHcwStatus.PerformLayout();
      this.groupBoxHcwSettings.ResumeLayout(false);
      this.groupBoxHcwSettings.PerformLayout();
      this.tabPageX10.ResumeLayout(false);
      this.groupBoxX10Status.ResumeLayout(false);
      this.groupBoxX10Status.PerformLayout();
      this.groupBoxX10General.ResumeLayout(false);
      this.groupBoxX10General.PerformLayout();
      this.groupBoxX10Settings.ResumeLayout(false);
      this.groupBoxX10Settings.PerformLayout();
      this.tabPageHid.ResumeLayout(false);
      this.groupBoxHidSettings.ResumeLayout(false);
      this.groupBoxHidSettings.PerformLayout();
      this.groupBoxHidInfo.ResumeLayout(false);
      this.groupBoxHidGeneral.ResumeLayout(false);
      this.groupBoxHidGeneral.PerformLayout();
      this.tabPageFireDtv.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    #endregion

    #region Form control commands MCE

    //
    // USA version
    //
    private void radioButtonMceUsa_CheckedChanged(object sender, System.EventArgs e)
    {
      pictureBoxMceUsa.Visible = radioButtonMceUsa.Checked;
      pictureBoxMceEurope.Visible = !radioButtonMceUsa.Checked;
      radioButtonMceEurope.Checked = !radioButtonMceUsa.Checked;
    }

    //
    // European version
    //
    private void radioButtonMceEurope_CheckedChanged(object sender, System.EventArgs e)
    {
      pictureBoxMceUsa.Visible = !radioButtonMceEurope.Checked;
      pictureBoxMceEurope.Visible = radioButtonMceEurope.Checked;
      radioButtonMceUsa.Checked = !radioButtonMceEurope.Checked;
    }

    //
    // Use Microsoft MCE remote
    //
    private void checkBoxMceEnabled_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxMceType.Enabled = groupBoxMceSettings.Enabled = checkBoxMceEnabled.Checked;

      //radioButtonMceEurope.Enabled = checkBoxMceEnabled.Checked;
      //radioButtonMceUsa.Enabled = checkBoxMceEnabled.Checked;
    }

    private void buttonMceDefaults_Click(object sender, EventArgs e)
    {
      radioButtonMceTeletext.Checked = true;
      checkBoxMceExtendedLogging.Checked = false;
    }

    private void buttonMceMapping_Click(object sender, EventArgs e)
    {
      InputMappingForm dlg;

      if (radioButtonMceTeletext.Checked)
        dlg = new InputMappingForm("Microsoft MCE with Teletext");
      else if (radioButtonMceNoTeletext.Checked)
        dlg = new InputMappingForm("Microsoft MCE");
      else
        dlg = new InputMappingForm("Microsoft MCE General");

      dlg.ShowDialog(this);
    }

    #region Helper methods/commands MCE

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

    #endregion

    #endregion

    #region Form control commands HCW

    //
    // External processes may use the remote control
    //
    private void checkBoxHcwAllowExternal_CheckedChanged(object sender, System.EventArgs e)
    {
      checkBoxHcwKeepControl.Enabled = checkBoxHcwAllowExternal.Checked;
    }

    //
    // Use Hauppauge remote
    //
    private void checkBoxHcwEnabled_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxHcwSettings.Enabled = checkBoxHcwEnabled.Checked;
      groupBoxHcwRepeatDelay.Enabled = checkBoxHcwEnabled.Checked;
      buttonHcwMapping.Enabled = checkBoxHcwEnabled.Checked;
    }

    //
    // Reset to default
    //    
    private void buttonHcwDefaults_Click(object sender, System.EventArgs e)
    {
      checkBoxHcwAllowExternal.Checked = false;
      checkBoxHcwKeepControl.Checked = false;
      checkBoxHcwExtendedLogging.Checked = false;
      hScrollBarHcwButtonRelease.Value = 200;
      hScrollBarHcwRepeatFilter.Value = 2;
      hScrollBarHcwRepeatSpeed.Value = 0;
      checkBoxHcwFilterDoubleKlicks.Checked = false;
    }

    private void buttonHcwMapping_Click(object sender, System.EventArgs e)
    {
      InputMappingForm dlg = new InputMappingForm("Hauppauge HCW");
      dlg.ShowDialog(this);
    }

    private void hScrollBarHcwButtonRelease_ValueChanged(object sender, EventArgs e)
    {
      toolTip.SetToolTip(this.hScrollBarHcwButtonRelease, string.Format("{0} msec.", hScrollBarHcwButtonRelease.Value));
    }

    private void hScrollBarHcwRepeatFilter_ValueChanged(object sender, EventArgs e)
    {
      toolTip.SetToolTip(this.hScrollBarHcwRepeatFilter, hScrollBarHcwRepeatFilter.Value.ToString());
    }

    private void hScrollBarHcwRepeatSpeed_ValueChanged(object sender, EventArgs e)
    {
      Type repeatSpeed = typeof(hcwRepeatSpeed);
      toolTip.SetToolTip(this.hScrollBarHcwRepeatSpeed, Enum.GetName(repeatSpeed, 2 - hScrollBarHcwRepeatSpeed.Value));
    }

    private void linkLabelHcwDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://www.team-mediaportal.com/component/option,com_remository/Itemid,35/func,select/id,28/");
    }

    #endregion

    #region Form control commands X10

    private void buttonX10Mapping_Click(object sender, EventArgs e)
    {
      InputMappingForm dlg = null;
      if (radioButtonX10Medion.Checked)
        dlg = new InputMappingForm("Medion X10");
      else if (radioButtonX10Ati.Checked)
        dlg = new InputMappingForm("ATI X10");
      else
        dlg = new InputMappingForm("Other X10");
      dlg.ShowDialog(this);
    }

    private void buttonX10Defaults_Click(object sender, EventArgs e)
    {
      checkBoxX10ExtendedLogging.Checked = false;
      checkBoxX10ChannelControl.Checked = false;
      buttonX10LearnChannel.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;
    }

    private void checkBoxX10Enabled_CheckedChanged(object sender, EventArgs e)
    {
      radioButtonX10Medion.Enabled = radioButtonX10Ati.Enabled = radioButtonX10Other.Enabled = buttonX10Mapping.Enabled = groupBoxX10Settings.Enabled = checkBoxX10Enabled.Checked;
      buttonX10LearnChannel.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;
    }

    private void linkLabelDownloadX10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://www1.medion.de/downloads/download.pl?id=1675&type=treiber&filename=remote_x10.exe&lang=de");
    }

    private void checkBoxX10ChannelControl_CheckedChanged(object sender, EventArgs e)
    {
      buttonX10LearnChannel.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;
    }

    private void buttonX10LearnChannel_Click(object sender, EventArgs e)
    {
      buttonX10LearnChannel.Enabled = false;
      try
      {
        if (x10Form == null)
          x10Form = new X10RemoteForm(new AxX10._DIX10InterfaceEvents_X10CommandEventHandler(this.IX10_X10Command));
      }
      catch (System.Runtime.InteropServices.COMException)
      {
        Log.Write("x10Remote: Can't initialize");
        x10Form = null;
        labelX10DriverInfo.Visible = false;
        linkLabelDownloadX10.Visible = false;
        labelX10Status.Visible = true;
        labelX10Status.Text = "X10 driver is not installed.";
        buttonX10LearnChannel.Enabled = true;
        return;
      }
      labelX10DriverInfo.Visible = false;
      linkLabelDownloadX10.Visible = false;
      labelX10Status.Visible = true;
      labelX10Status.Text = "Press a button on your remote control.";
    }

    #region Helper methods/commands X10

    public void IX10_X10Command(object sender, AxX10._DIX10InterfaceEvents_X10CommandEvent e)
    {
      if (e.eKeyState.ToString() == "X10KEY_ON" || e.eKeyState.ToString() == "X10KEY_REPEAT")
      {
        x10Channel = e.lAddress;
        buttonX10LearnChannel.Enabled = true;
        labelX10Status.Text = "Remote control successfully set to channel " + x10Channel.ToString() + ".";
        x10Form = null;
      }
    }

    #endregion

    #endregion

    #region Form control commands FireDTV

    #endregion

    #region Form control commands General HID

    private void buttonHidMapping_Click(object sender, EventArgs e)
    {
      InputMappingForm dlg = new InputMappingForm("General HID");
      dlg.ShowDialog(this);
    }

    private void checkBoxHidEnabled_CheckedChanged(object sender, EventArgs e)
    {
      buttonHidMapping.Enabled = checkBoxHidEnabled.Checked;
      groupBoxHidSettings.Enabled = checkBoxHidEnabled.Checked;
    }

    private void buttonHidDefaults_Click(object sender, EventArgs e)
    {
      checkBoxHidExtendedLogging.Checked = false;
    }

    #endregion





  }
}
