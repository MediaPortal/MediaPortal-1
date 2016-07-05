#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.InputDevices.FireDTV;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Remote : SectionSettings
  {
    #region Fields & Constants

    private FireDTVControl fireDTV = null;
    private MPGroupBox groupBoxHcwSettings;
    private MPLabel labelHcwButtonRelease;
    private MPCheckBox checkBoxHcwAllowExternal;
    private MPCheckBox checkBoxHcwKeepControl;
    private MPCheckBox checkBoxHcwExtendedLogging;
    private MPButton buttonHcwDefaults;
    private MPCheckBox checkBoxHcwEnabled;
    private MPLabel labelHcwDriverStatus;
    private MPGroupBox groupBoxHcwStatus;
    private MPTabPage tabPageHcw;
    private MPLabel labelHcw1000msec;
    private MPLabel labelHcw20msec;
    private MPTabControl tabControlRemotes;
    private MPTabPage tabPageFireDtv;
    private MPGroupBox groupBoxHcwGeneral;
    private MPGroupBox groupBoxHcwRepeatDelay;
    private HScrollBar hScrollBarHcwButtonRelease;
    private HScrollBar hScrollBarHcwRepeatSpeed;
    private MPLabel labelHcwFast;
    private MPLabel labelHcwRepeatSpeed;
    private MPLabel labelHcwSlow;
    private HScrollBar hScrollBarHcwRepeatFilter;
    private MPLabel labelHcwMax;
    private MPLabel labelHcwRepeatFilter;
    private MPLabel labelHcwMin;
    private MPCheckBox checkBoxHcwFilterDoubleKlicks;
    private ToolTip toolTip;
    private MPTabPage tabPageX10;
    private MPGroupBox groupBoxX10General;
    private MPCheckBox checkBoxX10Enabled;
    private MPGroupBox groupBoxX10Settings;
    private MPCheckBox checkBoxX10ExtendedLogging;
    private MPGroupBox groupBoxX10Status;
    private MPButton buttonX10Defaults;
    private LinkLabel linkLabelHcwDownload;
    private MPRadioButton radioButtonX10Ati;
    private MPRadioButton radioButtonX10Medion;
    private MPRadioButton radioButtonX10Firefly;
    private MPRadioButton radioButtonX10Other;
    private LinkLabel linkLabelDownloadX10;
    private Label labelX10DriverInfo;
    private MPButton buttonX10LearnMapping;
    private new IContainer components = null;
    private MPLabel labelX10Status;
    private MPTabPage tabPageHid;
    private MPTabPage tabPageIrTrans;
    private MPGroupBox groupBoxIrTransGeneral;
    private MPCheckBox checkBoxIrTransEnabled;
    private MPButton buttonIrTransMapping;
    private MPGroupBox groupBoxIrTransStatus;
    private MPLabel labelIrTransStatus;
    private MPGroupBox groupBoxIrTransServerSettings;
    private MPCheckBox checkBoxIrTransExtendedLogging;
    private MPTextBox textBoxRemoteModel;
    private MPLabel labelIrTransRemoteModel;
    private MPNumericTextBox textBoxIrTransServerPort;
    private MPLabel labelIrTransServerPort;
    private MPLabel labelIrTransNoteModel;
    private MPCheckBox checkBoxX10ChannelControl;

    private enum hcwRepeatSpeed
    {
      slow,
      medium,
      fast
    } ;

    private X10Remote X10Remote = null;
    private HcwRemote HCWRemote = null;
    private string X10mapping = null;
    private int x10Channel = 0;

    private const string errHcwNotInstalled =
      "The Hauppauge IR components have not been found.\nInstall the latest Hauppauge IR drivers and use XPSP2.";

    private const string errHcwOutOfDate =
      "The driver components are not up to date.\nUpdate your Hauppauge IR drivers to the current version.";

    private MPButton buttonIrTransTest;

    private const string errHcwMissingExe =
      "IR application not found. You might want to use it to control external applications.\nReinstall the Hauppauge IR drivers to fix this problem.";

    // FireDTV controls
    private MPGroupBox groupBoxFireDTVRecieiverSettings;
    private MPGroupBox groupBoxFireDTVReceiverGeneral;
    private MPCheckBox checkBoxFireDTVEnabled;
    private MPComboBox comboBoxFireDTVReceiver;
    private MPCheckBox checkBoxFireDTVExtendedLogging;
    private MPButton buttonFireDTVMapping;
    private MPLabel mpLabel1;
    private LinkLabel IRTransLink;
    private Button HCWLearn;
    private MPLabel LabelChannelNumber;
    private MPTextBox TextBoxChannelNumber;
    private PictureBox pictureBox3;
    private TabPage tabPageCentarea;
    private MPGroupBox groupBoxCentareaOptions;
    private MPCheckBox checkBoxCentareaVerbose;
    private MPCheckBox checkBoxCentareaEnabled;
    private MPButton buttonCentareaMapping;
    private PictureBox pictureBoxCentarea;
    private MPCheckBox mpCheckBox1;
    private MPCheckBox mpCheckBox2;
    private MPButton mpButton1;
    private MPCheckBox checkBoxCentareaReMapMouseButton;
    private MPCheckBox checkBoxMapJoystick;
    private MPGroupBox groupBox_x64;
    private LinkLabel linkLabel_x64;
    private TabPage tabPageAppCommand;
    private MPGroupBox mpGroupBox1;
    private MPCheckBox checkBoxAppCommandBackground;
    private MPCheckBox mpCheckBoxAppCommandVerbose;
    private MPButton mpButtonAppCommandMapping;
    private MPCheckBox mpCheckBoxAppCommandEnabled;
    private PictureBox pictureBox7;
    private PictureBox pictureBox8;
    private LinkLabel linkLabelMediaDocumentation;
    private Label labelMediaWarning;
    private MPGroupBox groupBoxHidGeneral;
    private NumericUpDown numericRepeatSpeed;
    private NumericUpDown numericRepeatDelay;
    private LinkLabel linkLabelDocumentation;
    private MPCheckBox checkBoxHidExtendedLogging;
    private MPButton buttonHidMapping;
    private MPCheckBox checkBoxHidEnabled;
    private Label labelRepeatSpeed;
    private Label labelRepeatDelay;
    private PictureBox pictureBox11;
    private PictureBox pictureBox12;
    private PictureBox pictureBox9;
    private PictureBox pictureBox6;
    private PictureBox pictureBox1;
    private PictureBox pictureBox4;
    private PictureBox pictureBox5;
    private PictureBox pictureBox2;
    private MPLabel labelHidProfiles;
    private MPComboBox comboBoxHidProfiles;
    private MPLabel labelFireDTVModel;

    #endregion

    #region Structures


    #endregion

    #region Interop


    #endregion

    #region Constructor

    public Remote()
      : this("Remotes and Input Devices") {}

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
      using (Settings xmlreader = new MPSettings())
      {

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
        {
          checkBoxHcwKeepControl.Enabled = true;
        }
        else
        {
          checkBoxHcwKeepControl.Enabled = false;
        }

        if (!checkBoxHcwEnabled.Checked)
        {
          groupBoxHcwSettings.Enabled = false;
          groupBoxHcwRepeatDelay.Enabled = false;
        }

        string exePath = irremote.GetHCWPath();
        string dllPath = irremote.GetDllPath();

        if (File.Exists(exePath + "Ir.exe"))
        {
          FileVersionInfo exeVersionInfo = FileVersionInfo.GetVersionInfo(exePath + "Ir.exe");
          if (exeVersionInfo.FileVersion.CompareTo(irremote.CurrentVersion) < 0)
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

          //Initialize the remote for learning purposes
          if (HCWRemote == null)
          {
            HCWRemote = new HcwRemote();
          }
          HCWRemote.Init();
          //HCWRemote.StartHcw();

          if (dllVersionInfo.FileVersion.CompareTo(irremote.CurrentVersion) < 0)
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
          using (Settings xmlwriter = new MPSettings())
          {
            xmlwriter.SetValueAsBool("remote", "HCW", false);
          }
        }
        HCWLearn.Enabled = buttonHcwDefaults.Enabled = checkBoxHcwEnabled.Checked;

        toolTip.SetToolTip(hScrollBarHcwButtonRelease, string.Format("{0} msec.", hScrollBarHcwButtonRelease.Value));
        toolTip.SetToolTip(hScrollBarHcwRepeatFilter, hScrollBarHcwRepeatFilter.Value.ToString());
        Type repeatSpeed = typeof (hcwRepeatSpeed);
        toolTip.SetToolTip(hScrollBarHcwRepeatSpeed, Enum.GetName(repeatSpeed, 2 - hScrollBarHcwRepeatSpeed.Value));
        toolTip.SetToolTip(checkBoxHcwKeepControl,
                           "If checked, MediaPortal keeps control of the remote. Only applications launched by\nMediaPortal can steal focus (external Players, MyPrograms, ...).");
        toolTip.SetToolTip(checkBoxHcwAllowExternal,
                           "If checked, MediaPortal does not keep control of the remote\nwhen it looses focus.");

        #endregion

        #region X10

        checkBoxX10Enabled.Checked = xmlreader.GetValueAsBool("remote", "X10", false);
        radioButtonX10Medion.Checked = xmlreader.GetValueAsBool("remote", "X10Medion", false);
        radioButtonX10Ati.Checked = xmlreader.GetValueAsBool("remote", "X10ATI", false);
        radioButtonX10Firefly.Checked = xmlreader.GetValueAsBool("remote", "X10Firefly", false);
        radioButtonX10Other.Checked = (!radioButtonX10Medion.Checked && !radioButtonX10Ati.Checked &&
                                       !radioButtonX10Firefly.Checked);
        checkBoxX10ExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "X10VerboseLog", false);
        checkBoxX10ChannelControl.Checked = xmlreader.GetValueAsBool("remote", "X10UseChannelControl", false);
        x10Channel = xmlreader.GetValueAsInt("remote", "X10Channel", 0);
        TextBoxChannelNumber.Text = x10Channel.ToString();

        radioButtonX10Medion.Enabled =
          radioButtonX10Ati.Enabled =
          radioButtonX10Other.Enabled =
          radioButtonX10Firefly.Enabled =
          buttonX10LearnMapping.Enabled =
          groupBoxX10Settings.Enabled = checkBoxX10Enabled.Checked;

        TextBoxChannelNumber.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;

        //See if the X10 driver is installed
        try
        {
          if (X10Remote == null)
          {
            X10Remote = new X10Remote();
            X10Remote.Init();
          }

          if (X10Remote._remotefound == false)
          {
            Log.Warn("x10Remote: Can't initialize");
            labelX10DriverInfo.Visible = true;
            labelX10DriverInfo.ForeColor = Color.Red;
            labelX10DriverInfo.Text =
              "The X10 Driver is not installed.\nYou have to use the driver below, or your remote might not work with MediaPortal.";
            linkLabelDownloadX10.Visible = true;
            labelX10Status.Visible = false;
            buttonX10LearnMapping.Enabled = false;
            checkBoxX10Enabled.Checked = false;
            checkBoxX10Enabled.Enabled = false;
          }
          else
          {
            Log.Info("x10Remote:Initialized");
            labelX10DriverInfo.Visible = false;
            linkLabelDownloadX10.Visible = true;
            labelX10Status.Visible = true;
            labelX10Status.Text =
              "The X10 Driver is installed. If you experience problems with this driver,\nuninstall your current driver and download the version below";
          }
        }
        catch (COMException)
        {
          Log.Warn("x10Remote: Can't initialize");
          labelX10DriverInfo.Visible = true;
          labelX10DriverInfo.ForeColor = Color.Red;
          labelX10DriverInfo.Text =
            "The X10 Driver is not installed.\nYou have to use the driver below, or your remote might not work with MediaPortal.";
          linkLabelDownloadX10.Visible = true;
          labelX10Status.Visible = false;
          buttonX10LearnMapping.Enabled = false;
          checkBoxX10Enabled.Checked = false;
          checkBoxX10Enabled.Enabled = false;
        }

        #endregion

        #region Generic HID

        //We want HID to be enabled by default
        //HID is also using MCE legacy setting for a smooth transition from MCE to HID
        checkBoxHidEnabled.Checked = xmlreader.GetValueAsBool("remote", "HidEnabled", true) || xmlreader.GetValueAsBool("remote", "MCE", false);
        checkBoxHidExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "HidVerbose", false) || xmlreader.GetValueAsBool("remote", "MCEVerboseLog", false);
        numericRepeatDelay.Value = xmlreader.GetValueAsInt("remote", "HidRepeatDelayInMs", -1);
        numericRepeatSpeed.Value = xmlreader.GetValueAsInt("remote", "HidRepeatSpeedInMs", -1);
        buttonHidMapping.Enabled = checkBoxHidEnabled.Checked;
        string hidProfile = xmlreader.GetValueAsString("remote", "HidProfile", "Generic-HID");
 
        //Populate our profiles
        PopulateProfiles(hidProfile);

        #endregion

        #region AppCommand

        mpCheckBoxAppCommandEnabled.Checked = xmlreader.GetValueAsBool("remote", "AppCommand", false);
        mpCheckBoxAppCommandVerbose.Checked = xmlreader.GetValueAsBool("remote", "AppCommandVerbose", false);
        checkBoxAppCommandBackground.Checked = xmlreader.GetValueAsBool("remote", "AppCommandBackground", false);
        mpButtonAppCommandMapping.Enabled = mpCheckBoxAppCommandEnabled.Checked;

        #endregion

        #region IRTrans

        checkBoxIrTransEnabled.Checked = xmlreader.GetValueAsBool("remote", "IRTrans", false);
        textBoxRemoteModel.Text = xmlreader.GetValueAsString("remote", "IRTransRemoteModel", "mediacenter");
        textBoxIrTransServerPort.Value = xmlreader.GetValueAsInt("remote", "IRTransServerPort", 21000);
        textBoxIrTransServerPort.Text = textBoxIrTransServerPort.Value.ToString();
        checkBoxIrTransExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "IRTransVerboseLog", false);
        buttonIrTransMapping.Enabled = checkBoxIrTransEnabled.Checked;
        groupBoxIrTransStatus.Enabled = groupBoxIrTransServerSettings.Enabled = checkBoxIrTransEnabled.Checked;

        #endregion

        #region FireDTV

        // Enable wiki link if x64 is detected
        bool _isx64 = Util.Win32API.Check64Bit();
        groupBox_x64.Visible = _isx64;
        linkLabel_x64.Enabled = _isx64;

        try
        {
          string prgPath = Environment.GetEnvironmentVariable("ProgramW6432");
          if (string.IsNullOrEmpty(prgPath))
          {
            prgPath = Environment.GetEnvironmentVariable("ProgramFiles");
          }

          // Look for Digital Everywhere's software which uses a hardcoded path
          string fullDllPath = Path.Combine(prgPath, @"FireDTV\Tools\FiresatApi.dll");
          if (File.Exists(fullDllPath))
          {
            // Is the FireDTV remote enabled
            checkBoxFireDTVEnabled.Checked = xmlreader.GetValueAsBool("remote", "FireDTV", false);

            // Fill combobox with list of availabe FireDTV recievers
            try
            {
              Log.Info("FireDTV: Using FiresatApi.dll located in FireDTV's install path {0}", fullDllPath);
              fireDTV = new FireDTVControl((IntPtr) 0);
              if (fireDTV.OpenDrivers())
              {
                comboBoxFireDTVReceiver.DataSource = fireDTV.SourceFilters;
                comboBoxFireDTVReceiver.DisplayMember = "FriendlyName";
                comboBoxFireDTVReceiver.ValueMember = "Name";
              }
            }
            catch (Exception e)
            {
              Log.Error("FireDTVRemote: Exception during setting combo {0}", e.Message);
            }

            // Set the rest of the controls
            checkBoxFireDTVExtendedLogging.Checked = xmlreader.GetValueAsBool("remote", "FireDTVVerboseLog", false);
            string deviceName = xmlreader.GetValueAsString("remote", "FireDTVDeviceName", string.Empty);
            try
            {
              if ((deviceName != null) && (!deviceName.Equals(string.Empty)))
              {
                comboBoxFireDTVReceiver.SelectedValue = deviceName;
              }
            }
            catch (InvalidOperationException ex)
            {
              Log.Error("FireDTV: Error setting device name - device unplugged?! - {0}", ex.Message);
            }

            // Enable/Disable the controls
            buttonFireDTVMapping.Enabled = checkBoxFireDTVEnabled.Checked;
            checkBoxFireDTVExtendedLogging.Enabled = checkBoxFireDTVEnabled.Checked;
            comboBoxFireDTVReceiver.Enabled = checkBoxFireDTVEnabled.Checked;
            groupBoxFireDTVRecieiverSettings.Enabled = checkBoxFireDTVEnabled.Checked;
          }
          else
          {
            checkBoxFireDTVEnabled.Enabled = false;
            checkBoxFireDTVExtendedLogging.Enabled = false;
            buttonFireDTVMapping.Enabled = false;
            groupBoxFireDTVRecieiverSettings.Enabled = false;
            Log.Info("FireDTV: FiresatApi.dll could not be found on your system!");
          }
        }
        catch (Exception)
        {
          Log.Error("FireDTVRemote: Exception during checking path and dll");
        }

        #endregion

        #region Sceneo

        checkBoxCentareaEnabled.Checked = xmlreader.GetValueAsBool("remote", "Centarea", false);
        checkBoxCentareaVerbose.Checked = xmlreader.GetValueAsBool("remote", "CentareaVerbose", false);
        checkBoxCentareaReMapMouseButton.Checked = xmlreader.GetValueAsBool("remote", "CentareaMouseOkMap", true);
        checkBoxMapJoystick.Checked = xmlreader.GetValueAsBool("remote", "CentareaJoystickMap", false);

        #endregion
      }
    }

    /// <summary>
    /// Populate our profiles
    /// </summary>
    public void PopulateProfiles(string aCurrentProfileName)
    {      
      comboBoxHidProfiles.Items.Clear();
      foreach (string profile in HidProfiles.GetExistingProfilesFileNames())
      {
        string profileName = HidProfiles.GetProfileName(profile);
        comboBoxHidProfiles.Items.Add(profileName);
        //Select our current profile
        if (profileName.Equals(aCurrentProfileName) || comboBoxHidProfiles.Items.Count == 1)
        {
          comboBoxHidProfiles.SelectedIndex = comboBoxHidProfiles.Items.Count - 1;
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {

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
        xmlwriter.SetValueAsBool("remote", "X10Firefly", radioButtonX10Firefly.Checked);
        xmlwriter.SetValueAsBool("remote", "X10VerboseLog", checkBoxX10ExtendedLogging.Checked);
        xmlwriter.SetValueAsBool("remote", "X10UseChannelControl", checkBoxX10ChannelControl.Checked);
        x10Channel = Int32.Parse(TextBoxChannelNumber.Text);
        xmlwriter.SetValue("remote", "X10Channel", x10Channel);

        #endregion

        #region Generic HID

        xmlwriter.SetValueAsBool("remote", "HidEnabled", checkBoxHidEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "HidVerbose", checkBoxHidExtendedLogging.Checked);
        xmlwriter.SetValue("remote", "HidRepeatDelayInMs", ((int)numericRepeatDelay.Value).ToString());
        xmlwriter.SetValue("remote", "HidRepeatSpeedInMs", ((int)numericRepeatSpeed.Value).ToString());
        xmlwriter.SetValue("remote", "HidProfile", comboBoxHidProfiles.Text);
        
        #endregion

        #region AppCommand

        xmlwriter.SetValueAsBool("remote", "AppCommand", mpCheckBoxAppCommandEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "AppCommandVerbose", mpCheckBoxAppCommandVerbose.Checked);
        xmlwriter.SetValueAsBool("remote", "AppCommandBackground", checkBoxAppCommandBackground.Checked);

        #endregion


        #region IRTRans

        xmlwriter.SetValueAsBool("remote", "IRTrans", checkBoxIrTransEnabled.Checked);
        xmlwriter.SetValue("remote", "IRTransRemoteModel", textBoxRemoteModel.Text);
        xmlwriter.SetValue("remote", "IRTransServerPort", textBoxIrTransServerPort.Text);
        xmlwriter.SetValueAsBool("remote", "IRTransVerboseLog", checkBoxIrTransExtendedLogging.Checked);

        #endregion

        #region FireDTV

        // Load FireDTV specific settings.
        xmlwriter.SetValueAsBool("remote", "FireDTV", checkBoxFireDTVEnabled.Checked);
        xmlwriter.SetValue("remote", "FireDTVDeviceName", comboBoxFireDTVReceiver.SelectedValue);
        xmlwriter.SetValue("remote", "FireDTVVerboseLog", checkBoxFireDTVExtendedLogging.Checked);

        #endregion

        #region Sceneo

        xmlwriter.SetValueAsBool("remote", "Centarea", checkBoxCentareaEnabled.Checked);
        xmlwriter.SetValueAsBool("remote", "CentareaVerbose", checkBoxCentareaVerbose.Checked);
        xmlwriter.SetValueAsBool("remote", "CentareaMouseOkMap", checkBoxCentareaReMapMouseButton.Checked);
        xmlwriter.SetValueAsBool("remote", "CentareaJoystickMap", checkBoxMapJoystick.Checked);

        #endregion
      }
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
        if (HCWRemote != null)
        {
          HCWRemote.StopHcw();
          HCWRemote.DeInit();
          HCWRemote = null;
        }

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
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.checkBoxHidEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlRemotes = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageHid = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.pictureBox11 = new System.Windows.Forms.PictureBox();
      this.pictureBox12 = new System.Windows.Forms.PictureBox();
      this.pictureBox9 = new System.Windows.Forms.PictureBox();
      this.pictureBox6 = new System.Windows.Forms.PictureBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.pictureBox4 = new System.Windows.Forms.PictureBox();
      this.pictureBox5 = new System.Windows.Forms.PictureBox();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.groupBoxHidGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelHidProfiles = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxHidProfiles = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.numericRepeatSpeed = new System.Windows.Forms.NumericUpDown();
      this.numericRepeatDelay = new System.Windows.Forms.NumericUpDown();
      this.linkLabelDocumentation = new System.Windows.Forms.LinkLabel();
      this.checkBoxHidExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonHidMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelRepeatSpeed = new System.Windows.Forms.Label();
      this.labelRepeatDelay = new System.Windows.Forms.Label();
      this.tabPageAppCommand = new System.Windows.Forms.TabPage();
      this.labelMediaWarning = new System.Windows.Forms.Label();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelMediaDocumentation = new System.Windows.Forms.LinkLabel();
      this.checkBoxAppCommandBackground = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxAppCommandVerbose = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButtonAppCommandMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCheckBoxAppCommandEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.pictureBox8 = new System.Windows.Forms.PictureBox();
      this.pictureBox7 = new System.Windows.Forms.PictureBox();
      this.tabPageCentarea = new System.Windows.Forms.TabPage();
      this.groupBoxCentareaOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxMapJoystick = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxCentareaReMapMouseButton = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxCentareaVerbose = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxCentareaEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonCentareaMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxCentarea = new System.Windows.Forms.PictureBox();
      this.tabPageFireDtv = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox_x64 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabel_x64 = new System.Windows.Forms.LinkLabel();
      this.groupBoxFireDTVRecieiverSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelFireDTVModel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxFireDTVReceiver = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxFireDTVReceiverGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxFireDTVExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonFireDTVMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxFireDTVEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.pictureBox3 = new System.Windows.Forms.PictureBox();
      this.tabPageX10 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxX10Status = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelX10DriverInfo = new System.Windows.Forms.Label();
      this.linkLabelDownloadX10 = new System.Windows.Forms.LinkLabel();
      this.labelX10Status = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxX10General = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonX10LearnMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.radioButtonX10Firefly = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonX10Other = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonX10Ati = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonX10Medion = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxX10Enabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxX10Settings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.LabelChannelNumber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.TextBoxChannelNumber = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.checkBoxX10ChannelControl = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxX10ExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonX10Defaults = new MediaPortal.UserInterface.Controls.MPButton();
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
      this.HCWLearn = new System.Windows.Forms.Button();
      this.buttonHcwDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxHcwEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxHcwStatus = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelHcwDownload = new System.Windows.Forms.LinkLabel();
      this.labelHcwDriverStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxHcwSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxHcwFilterDoubleKlicks = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwAllowExternal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwKeepControl = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxHcwExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageIrTrans = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxIrTransStatus = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.IRTransLink = new System.Windows.Forms.LinkLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelIrTransStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxIrTransServerSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonIrTransTest = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelIrTransNoteModel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxRemoteModel = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelIrTransRemoteModel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxIrTransServerPort = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.labelIrTransServerPort = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxIrTransGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxIrTransExtendedLogging = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxIrTransEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonIrTransMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCheckBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBox2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabControlRemotes.SuspendLayout();
      this.tabPageHid.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox11)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      this.groupBoxHidGeneral.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericRepeatSpeed)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericRepeatDelay)).BeginInit();
      this.tabPageAppCommand.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
      this.tabPageCentarea.SuspendLayout();
      this.groupBoxCentareaOptions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCentarea)).BeginInit();
      this.tabPageFireDtv.SuspendLayout();
      this.groupBox_x64.SuspendLayout();
      this.groupBoxFireDTVRecieiverSettings.SuspendLayout();
      this.groupBoxFireDTVReceiverGeneral.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
      this.tabPageX10.SuspendLayout();
      this.groupBoxX10Status.SuspendLayout();
      this.groupBoxX10General.SuspendLayout();
      this.groupBoxX10Settings.SuspendLayout();
      this.tabPageHcw.SuspendLayout();
      this.groupBoxHcwRepeatDelay.SuspendLayout();
      this.groupBoxHcwGeneral.SuspendLayout();
      this.groupBoxHcwStatus.SuspendLayout();
      this.groupBoxHcwSettings.SuspendLayout();
      this.tabPageIrTrans.SuspendLayout();
      this.groupBoxIrTransStatus.SuspendLayout();
      this.groupBoxIrTransServerSettings.SuspendLayout();
      this.groupBoxIrTransGeneral.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolTip
      // 
      this.toolTip.ShowAlways = true;
      // 
      // checkBoxHidEnabled
      // 
      this.checkBoxHidEnabled.AutoSize = true;
      this.checkBoxHidEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHidEnabled.Location = new System.Drawing.Point(16, 53);
      this.checkBoxHidEnabled.Name = "checkBoxHidEnabled";
      this.checkBoxHidEnabled.Size = new System.Drawing.Size(138, 17);
      this.checkBoxHidEnabled.TabIndex = 0;
      this.checkBoxHidEnabled.Text = "Use generic HID device";
      this.toolTip.SetToolTip(this.checkBoxHidEnabled, "Supports the largest range of remote and control devices. This should be you pref" +
        "erred option.");
      this.checkBoxHidEnabled.UseVisualStyleBackColor = true;
      this.checkBoxHidEnabled.CheckedChanged += new System.EventHandler(this.checkBoxHidEnabled_CheckedChanged);
      // 
      // tabControlRemotes
      // 
      this.tabControlRemotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlRemotes.Controls.Add(this.tabPageHid);
      this.tabControlRemotes.Controls.Add(this.tabPageAppCommand);
      this.tabControlRemotes.Controls.Add(this.tabPageCentarea);
      this.tabControlRemotes.Controls.Add(this.tabPageFireDtv);
      this.tabControlRemotes.Controls.Add(this.tabPageX10);
      this.tabControlRemotes.Controls.Add(this.tabPageHcw);
      this.tabControlRemotes.Controls.Add(this.tabPageIrTrans);
      this.tabControlRemotes.Location = new System.Drawing.Point(0, 8);
      this.tabControlRemotes.Name = "tabControlRemotes";
      this.tabControlRemotes.SelectedIndex = 0;
      this.tabControlRemotes.Size = new System.Drawing.Size(480, 448);
      this.tabControlRemotes.TabIndex = 0;
      // 
      // tabPageHid
      // 
      this.tabPageHid.Controls.Add(this.pictureBox11);
      this.tabPageHid.Controls.Add(this.pictureBox12);
      this.tabPageHid.Controls.Add(this.pictureBox9);
      this.tabPageHid.Controls.Add(this.pictureBox6);
      this.tabPageHid.Controls.Add(this.pictureBox1);
      this.tabPageHid.Controls.Add(this.pictureBox4);
      this.tabPageHid.Controls.Add(this.pictureBox5);
      this.tabPageHid.Controls.Add(this.pictureBox2);
      this.tabPageHid.Controls.Add(this.groupBoxHidGeneral);
      this.tabPageHid.Location = new System.Drawing.Point(4, 22);
      this.tabPageHid.Name = "tabPageHid";
      this.tabPageHid.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageHid.Size = new System.Drawing.Size(472, 422);
      this.tabPageHid.TabIndex = 4;
      this.tabPageHid.Text = "HID";
      this.tabPageHid.UseVisualStyleBackColor = true;
      // 
      // pictureBox11
      // 
      this.pictureBox11.Image = global::MediaPortal.Configuration.Properties.Resources.gamepad_LogitechCordlessRumblePad2;
      this.pictureBox11.Location = new System.Drawing.Point(160, 327);
      this.pictureBox11.Name = "pictureBox11";
      this.pictureBox11.Size = new System.Drawing.Size(150, 89);
      this.pictureBox11.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox11.TabIndex = 12;
      this.pictureBox11.TabStop = false;
      this.pictureBox11.Click += new System.EventHandler(this.pictureBox11_Click);
      // 
      // pictureBox12
      // 
      this.pictureBox12.Image = global::MediaPortal.Configuration.Properties.Resources.gamepad_MicrosoftXbox360WirelessController;
      this.pictureBox12.Location = new System.Drawing.Point(316, 327);
      this.pictureBox12.Name = "pictureBox12";
      this.pictureBox12.Size = new System.Drawing.Size(150, 89);
      this.pictureBox12.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox12.TabIndex = 11;
      this.pictureBox12.TabStop = false;
      // 
      // pictureBox9
      // 
      this.pictureBox9.Image = global::MediaPortal.Configuration.Properties.Resources.gamepad_LogitechWirelessGamepadF710;
      this.pictureBox9.Location = new System.Drawing.Point(6, 327);
      this.pictureBox9.Name = "pictureBox9";
      this.pictureBox9.Size = new System.Drawing.Size(148, 89);
      this.pictureBox9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox9.TabIndex = 10;
      this.pictureBox9.TabStop = false;
      // 
      // pictureBox6
      // 
      this.pictureBox6.Image = global::MediaPortal.Configuration.Properties.Resources.remote_MceHp;
      this.pictureBox6.Location = new System.Drawing.Point(51, 168);
      this.pictureBox6.Name = "pictureBox6";
      this.pictureBox6.Size = new System.Drawing.Size(46, 153);
      this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox6.TabIndex = 9;
      this.pictureBox6.TabStop = false;
      this.pictureBox6.Click += new System.EventHandler(this.pictureBox6_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.Configuration.Properties.Resources.remote_MceVista;
      this.pictureBox1.Location = new System.Drawing.Point(6, 168);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(39, 153);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 8;
      this.pictureBox1.TabStop = false;
      // 
      // pictureBox4
      // 
      this.pictureBox4.Image = global::MediaPortal.Configuration.Properties.Resources.remote_Mce2005;
      this.pictureBox4.Location = new System.Drawing.Point(103, 168);
      this.pictureBox4.Name = "pictureBox4";
      this.pictureBox4.Size = new System.Drawing.Size(61, 153);
      this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox4.TabIndex = 7;
      this.pictureBox4.TabStop = false;
      // 
      // pictureBox5
      // 
      this.pictureBox5.Image = global::MediaPortal.Configuration.Properties.Resources.remote_Mce2004;
      this.pictureBox5.Location = new System.Drawing.Point(170, 168);
      this.pictureBox5.Name = "pictureBox5";
      this.pictureBox5.Size = new System.Drawing.Size(61, 153);
      this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox5.TabIndex = 6;
      this.pictureBox5.TabStop = false;
      // 
      // pictureBox2
      // 
      this.pictureBox2.Image = global::MediaPortal.Configuration.Properties.Resources.ms_wed_7000;
      this.pictureBox2.Location = new System.Drawing.Point(237, 199);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(229, 122);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox2.TabIndex = 4;
      this.pictureBox2.TabStop = false;
      // 
      // groupBoxHidGeneral
      // 
      this.groupBoxHidGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxHidGeneral.Controls.Add(this.labelHidProfiles);
      this.groupBoxHidGeneral.Controls.Add(this.comboBoxHidProfiles);
      this.groupBoxHidGeneral.Controls.Add(this.numericRepeatSpeed);
      this.groupBoxHidGeneral.Controls.Add(this.numericRepeatDelay);
      this.groupBoxHidGeneral.Controls.Add(this.linkLabelDocumentation);
      this.groupBoxHidGeneral.Controls.Add(this.checkBoxHidExtendedLogging);
      this.groupBoxHidGeneral.Controls.Add(this.buttonHidMapping);
      this.groupBoxHidGeneral.Controls.Add(this.checkBoxHidEnabled);
      this.groupBoxHidGeneral.Controls.Add(this.labelRepeatSpeed);
      this.groupBoxHidGeneral.Controls.Add(this.labelRepeatDelay);
      this.groupBoxHidGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHidGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxHidGeneral.Name = "groupBoxHidGeneral";
      this.groupBoxHidGeneral.Size = new System.Drawing.Size(448, 154);
      this.groupBoxHidGeneral.TabIndex = 1;
      this.groupBoxHidGeneral.TabStop = false;
      // 
      // labelHidProfiles
      // 
      this.labelHidProfiles.AutoSize = true;
      this.labelHidProfiles.Location = new System.Drawing.Point(6, 19);
      this.labelHidProfiles.Name = "labelHidProfiles";
      this.labelHidProfiles.Size = new System.Drawing.Size(39, 13);
      this.labelHidProfiles.TabIndex = 11;
      this.labelHidProfiles.Text = "Profile:";
      // 
      // comboBoxHidProfiles
      // 
      this.comboBoxHidProfiles.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxHidProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxHidProfiles.FormattingEnabled = true;
      this.comboBoxHidProfiles.Location = new System.Drawing.Point(56, 16);
      this.comboBoxHidProfiles.Name = "comboBoxHidProfiles";
      this.comboBoxHidProfiles.Size = new System.Drawing.Size(163, 21);
      this.comboBoxHidProfiles.TabIndex = 10;
      // 
      // numericRepeatSpeed
      // 
      this.numericRepeatSpeed.Location = new System.Drawing.Point(269, 76);
      this.numericRepeatSpeed.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
      this.numericRepeatSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericRepeatSpeed.Name = "numericRepeatSpeed";
      this.numericRepeatSpeed.Size = new System.Drawing.Size(47, 20);
      this.numericRepeatSpeed.TabIndex = 7;
      this.numericRepeatSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      // 
      // numericRepeatDelay
      // 
      this.numericRepeatDelay.Location = new System.Drawing.Point(269, 53);
      this.numericRepeatDelay.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
      this.numericRepeatDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      this.numericRepeatDelay.Name = "numericRepeatDelay";
      this.numericRepeatDelay.Size = new System.Drawing.Size(47, 20);
      this.numericRepeatDelay.TabIndex = 6;
      this.numericRepeatDelay.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
      // 
      // linkLabelDocumentation
      // 
      this.linkLabelDocumentation.AutoSize = true;
      this.linkLabelDocumentation.Location = new System.Drawing.Point(330, 124);
      this.linkLabelDocumentation.Name = "linkLabelDocumentation";
      this.linkLabelDocumentation.Size = new System.Drawing.Size(112, 13);
      this.linkLabelDocumentation.TabIndex = 5;
      this.linkLabelDocumentation.TabStop = true;
      this.linkLabelDocumentation.Text = "Online Documentation";
      this.linkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDocumentation_LinkClicked);
      // 
      // checkBoxHidExtendedLogging
      // 
      this.checkBoxHidExtendedLogging.AutoSize = true;
      this.checkBoxHidExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxHidExtendedLogging.Location = new System.Drawing.Point(16, 76);
      this.checkBoxHidExtendedLogging.Name = "checkBoxHidExtendedLogging";
      this.checkBoxHidExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxHidExtendedLogging.TabIndex = 4;
      this.checkBoxHidExtendedLogging.Text = "Extended logging";
      this.checkBoxHidExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // buttonHidMapping
      // 
      this.buttonHidMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonHidMapping.Location = new System.Drawing.Point(367, 14);
      this.buttonHidMapping.Name = "buttonHidMapping";
      this.buttonHidMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonHidMapping.TabIndex = 1;
      this.buttonHidMapping.Text = "Mapping";
      this.buttonHidMapping.UseVisualStyleBackColor = true;
      this.buttonHidMapping.Click += new System.EventHandler(this.buttonHidMapping_Click);
      // 
      // labelRepeatSpeed
      // 
      this.labelRepeatSpeed.AutoSize = true;
      this.labelRepeatSpeed.Location = new System.Drawing.Point(169, 78);
      this.labelRepeatSpeed.Name = "labelRepeatSpeed";
      this.labelRepeatSpeed.Size = new System.Drawing.Size(96, 13);
      this.labelRepeatSpeed.TabIndex = 9;
      this.labelRepeatSpeed.Text = "Repeat speed (ms)";
      // 
      // labelRepeatDelay
      // 
      this.labelRepeatDelay.AutoSize = true;
      this.labelRepeatDelay.Location = new System.Drawing.Point(169, 55);
      this.labelRepeatDelay.Name = "labelRepeatDelay";
      this.labelRepeatDelay.Size = new System.Drawing.Size(92, 13);
      this.labelRepeatDelay.TabIndex = 8;
      this.labelRepeatDelay.Text = "Repeat delay (ms)";
      // 
      // tabPageAppCommand
      // 
      this.tabPageAppCommand.Controls.Add(this.labelMediaWarning);
      this.tabPageAppCommand.Controls.Add(this.mpGroupBox1);
      this.tabPageAppCommand.Controls.Add(this.pictureBox8);
      this.tabPageAppCommand.Controls.Add(this.pictureBox7);
      this.tabPageAppCommand.Location = new System.Drawing.Point(4, 22);
      this.tabPageAppCommand.Name = "tabPageAppCommand";
      this.tabPageAppCommand.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageAppCommand.Size = new System.Drawing.Size(472, 422);
      this.tabPageAppCommand.TabIndex = 7;
      this.tabPageAppCommand.Text = "Media";
      this.tabPageAppCommand.UseVisualStyleBackColor = true;
      // 
      // labelMediaWarning
      // 
      this.labelMediaWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelMediaWarning.ForeColor = System.Drawing.Color.Red;
      this.labelMediaWarning.Location = new System.Drawing.Point(7, 371);
      this.labelMediaWarning.Name = "labelMediaWarning";
      this.labelMediaWarning.Size = new System.Drawing.Size(459, 48);
      this.labelMediaWarning.TabIndex = 8;
      this.labelMediaWarning.Text = "Consider using HID instead.\r\nDo not enable together with HID unless you know what" +
    " you are doing.\r\n\r\n\r\n";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.linkLabelMediaDocumentation);
      this.mpGroupBox1.Controls.Add(this.checkBoxAppCommandBackground);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxAppCommandVerbose);
      this.mpGroupBox1.Controls.Add(this.mpButtonAppCommandMapping);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxAppCommandEnabled);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(12, 8);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(448, 95);
      this.mpGroupBox1.TabIndex = 5;
      this.mpGroupBox1.TabStop = false;
      // 
      // linkLabelMediaDocumentation
      // 
      this.linkLabelMediaDocumentation.AutoSize = true;
      this.linkLabelMediaDocumentation.Location = new System.Drawing.Point(319, 70);
      this.linkLabelMediaDocumentation.Name = "linkLabelMediaDocumentation";
      this.linkLabelMediaDocumentation.Size = new System.Drawing.Size(112, 13);
      this.linkLabelMediaDocumentation.TabIndex = 6;
      this.linkLabelMediaDocumentation.TabStop = true;
      this.linkLabelMediaDocumentation.Text = "Online Documentation";
      this.linkLabelMediaDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelMediaDocumentation_LinkClicked);
      // 
      // checkBoxAppCommandBackground
      // 
      this.checkBoxAppCommandBackground.AutoSize = true;
      this.checkBoxAppCommandBackground.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAppCommandBackground.Location = new System.Drawing.Point(16, 70);
      this.checkBoxAppCommandBackground.Name = "checkBoxAppCommandBackground";
      this.checkBoxAppCommandBackground.Size = new System.Drawing.Size(222, 17);
      this.checkBoxAppCommandBackground.TabIndex = 5;
      this.checkBoxAppCommandBackground.Text = "Support input even if MP is in background";
      this.checkBoxAppCommandBackground.UseVisualStyleBackColor = true;
      // 
      // mpCheckBoxAppCommandVerbose
      // 
      this.mpCheckBoxAppCommandVerbose.AutoSize = true;
      this.mpCheckBoxAppCommandVerbose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxAppCommandVerbose.Location = new System.Drawing.Point(16, 47);
      this.mpCheckBoxAppCommandVerbose.Name = "mpCheckBoxAppCommandVerbose";
      this.mpCheckBoxAppCommandVerbose.Size = new System.Drawing.Size(106, 17);
      this.mpCheckBoxAppCommandVerbose.TabIndex = 4;
      this.mpCheckBoxAppCommandVerbose.Text = "Extended logging";
      this.mpCheckBoxAppCommandVerbose.UseVisualStyleBackColor = true;
      // 
      // mpButtonAppCommandMapping
      // 
      this.mpButtonAppCommandMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonAppCommandMapping.Location = new System.Drawing.Point(359, 21);
      this.mpButtonAppCommandMapping.Name = "mpButtonAppCommandMapping";
      this.mpButtonAppCommandMapping.Size = new System.Drawing.Size(72, 22);
      this.mpButtonAppCommandMapping.TabIndex = 1;
      this.mpButtonAppCommandMapping.Text = "Mapping";
      this.mpButtonAppCommandMapping.UseVisualStyleBackColor = true;
      this.mpButtonAppCommandMapping.Click += new System.EventHandler(this.mpButtonAppCommandMapping_Click);
      // 
      // mpCheckBoxAppCommandEnabled
      // 
      this.mpCheckBoxAppCommandEnabled.AutoSize = true;
      this.mpCheckBoxAppCommandEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxAppCommandEnabled.Location = new System.Drawing.Point(16, 24);
      this.mpCheckBoxAppCommandEnabled.Name = "mpCheckBoxAppCommandEnabled";
      this.mpCheckBoxAppCommandEnabled.Size = new System.Drawing.Size(99, 17);
      this.mpCheckBoxAppCommandEnabled.TabIndex = 0;
      this.mpCheckBoxAppCommandEnabled.Text = "Use media keys";
      this.mpCheckBoxAppCommandEnabled.UseVisualStyleBackColor = true;
      this.mpCheckBoxAppCommandEnabled.CheckedChanged += new System.EventHandler(this.mpCheckBoxAppCommandEnabled_CheckedChanged);
      // 
      // pictureBox8
      // 
      this.pictureBox8.Image = global::MediaPortal.Configuration.Properties.Resources.k60_media_keys;
      this.pictureBox8.Location = new System.Drawing.Point(210, 109);
      this.pictureBox8.Name = "pictureBox8";
      this.pictureBox8.Size = new System.Drawing.Size(250, 227);
      this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox8.TabIndex = 7;
      this.pictureBox8.TabStop = false;
      // 
      // pictureBox7
      // 
      this.pictureBox7.Image = global::MediaPortal.Configuration.Properties.Resources.ms_wed_7000;
      this.pictureBox7.Location = new System.Drawing.Point(12, 109);
      this.pictureBox7.Name = "pictureBox7";
      this.pictureBox7.Size = new System.Drawing.Size(192, 177);
      this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox7.TabIndex = 6;
      this.pictureBox7.TabStop = false;
      // 
      // tabPageCentarea
      // 
      this.tabPageCentarea.Controls.Add(this.groupBoxCentareaOptions);
      this.tabPageCentarea.Controls.Add(this.pictureBoxCentarea);
      this.tabPageCentarea.Location = new System.Drawing.Point(4, 22);
      this.tabPageCentarea.Name = "tabPageCentarea";
      this.tabPageCentarea.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageCentarea.Size = new System.Drawing.Size(472, 422);
      this.tabPageCentarea.TabIndex = 6;
      this.tabPageCentarea.Text = "Sceneo";
      this.tabPageCentarea.UseVisualStyleBackColor = true;
      // 
      // groupBoxCentareaOptions
      // 
      this.groupBoxCentareaOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxCentareaOptions.Controls.Add(this.checkBoxMapJoystick);
      this.groupBoxCentareaOptions.Controls.Add(this.checkBoxCentareaReMapMouseButton);
      this.groupBoxCentareaOptions.Controls.Add(this.checkBoxCentareaVerbose);
      this.groupBoxCentareaOptions.Controls.Add(this.checkBoxCentareaEnabled);
      this.groupBoxCentareaOptions.Controls.Add(this.buttonCentareaMapping);
      this.groupBoxCentareaOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCentareaOptions.Location = new System.Drawing.Point(12, 8);
      this.groupBoxCentareaOptions.Name = "groupBoxCentareaOptions";
      this.groupBoxCentareaOptions.Size = new System.Drawing.Size(448, 120);
      this.groupBoxCentareaOptions.TabIndex = 4;
      this.groupBoxCentareaOptions.TabStop = false;
      // 
      // checkBoxMapJoystick
      // 
      this.checkBoxMapJoystick.AutoSize = true;
      this.checkBoxMapJoystick.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxMapJoystick.Location = new System.Drawing.Point(16, 93);
      this.checkBoxMapJoystick.Name = "checkBoxMapJoystick";
      this.checkBoxMapJoystick.Size = new System.Drawing.Size(291, 17);
      this.checkBoxMapJoystick.TabIndex = 6;
      this.checkBoxMapJoystick.Text = "Map mouse movement to cursor directions (experimental)";
      this.checkBoxMapJoystick.UseVisualStyleBackColor = true;
      // 
      // checkBoxCentareaReMapMouseButton
      // 
      this.checkBoxCentareaReMapMouseButton.AutoSize = true;
      this.checkBoxCentareaReMapMouseButton.Checked = true;
      this.checkBoxCentareaReMapMouseButton.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxCentareaReMapMouseButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCentareaReMapMouseButton.Location = new System.Drawing.Point(16, 70);
      this.checkBoxCentareaReMapMouseButton.Name = "checkBoxCentareaReMapMouseButton";
      this.checkBoxCentareaReMapMouseButton.Size = new System.Drawing.Size(227, 17);
      this.checkBoxCentareaReMapMouseButton.TabIndex = 5;
      this.checkBoxCentareaReMapMouseButton.Text = "Map mouse clicks to \'Ok\' and \"Info\" button";
      this.checkBoxCentareaReMapMouseButton.UseVisualStyleBackColor = true;
      // 
      // checkBoxCentareaVerbose
      // 
      this.checkBoxCentareaVerbose.AutoSize = true;
      this.checkBoxCentareaVerbose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCentareaVerbose.Location = new System.Drawing.Point(16, 47);
      this.checkBoxCentareaVerbose.Name = "checkBoxCentareaVerbose";
      this.checkBoxCentareaVerbose.Size = new System.Drawing.Size(106, 17);
      this.checkBoxCentareaVerbose.TabIndex = 4;
      this.checkBoxCentareaVerbose.Text = "Extended logging";
      this.checkBoxCentareaVerbose.UseVisualStyleBackColor = true;
      // 
      // checkBoxCentareaEnabled
      // 
      this.checkBoxCentareaEnabled.AutoSize = true;
      this.checkBoxCentareaEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCentareaEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxCentareaEnabled.Name = "checkBoxCentareaEnabled";
      this.checkBoxCentareaEnabled.Size = new System.Drawing.Size(277, 17);
      this.checkBoxCentareaEnabled.TabIndex = 0;
      this.checkBoxCentareaEnabled.Text = "Use Centarea Master Remote II / Sunwave SMR-140";
      this.checkBoxCentareaEnabled.UseVisualStyleBackColor = true;
      this.checkBoxCentareaEnabled.CheckedChanged += new System.EventHandler(this.checkBoxCentareaEnabled_CheckedChanged);
      // 
      // buttonCentareaMapping
      // 
      this.buttonCentareaMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCentareaMapping.Enabled = false;
      this.buttonCentareaMapping.Location = new System.Drawing.Point(359, 21);
      this.buttonCentareaMapping.Name = "buttonCentareaMapping";
      this.buttonCentareaMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonCentareaMapping.TabIndex = 1;
      this.buttonCentareaMapping.Text = "Mapping";
      this.buttonCentareaMapping.UseVisualStyleBackColor = true;
      this.buttonCentareaMapping.Click += new System.EventHandler(this.buttonCentareaMapping_Click);
      // 
      // pictureBoxCentarea
      // 
      this.pictureBoxCentarea.Image = global::MediaPortal.Configuration.Properties.Resources.remote_Centarea;
      this.pictureBoxCentarea.Location = new System.Drawing.Point(12, 149);
      this.pictureBoxCentarea.Name = "pictureBoxCentarea";
      this.pictureBoxCentarea.Size = new System.Drawing.Size(122, 222);
      this.pictureBoxCentarea.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxCentarea.TabIndex = 5;
      this.pictureBoxCentarea.TabStop = false;
      // 
      // tabPageFireDtv
      // 
      this.tabPageFireDtv.Controls.Add(this.groupBox_x64);
      this.tabPageFireDtv.Controls.Add(this.groupBoxFireDTVRecieiverSettings);
      this.tabPageFireDtv.Controls.Add(this.groupBoxFireDTVReceiverGeneral);
      this.tabPageFireDtv.Controls.Add(this.pictureBox3);
      this.tabPageFireDtv.Location = new System.Drawing.Point(4, 22);
      this.tabPageFireDtv.Name = "tabPageFireDtv";
      this.tabPageFireDtv.Size = new System.Drawing.Size(472, 422);
      this.tabPageFireDtv.TabIndex = 2;
      this.tabPageFireDtv.Text = "FireDTV";
      this.tabPageFireDtv.UseVisualStyleBackColor = true;
      // 
      // groupBox_x64
      // 
      this.groupBox_x64.Controls.Add(this.linkLabel_x64);
      this.groupBox_x64.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox_x64.Location = new System.Drawing.Point(156, 191);
      this.groupBox_x64.Name = "groupBox_x64";
      this.groupBox_x64.Size = new System.Drawing.Size(296, 105);
      this.groupBox_x64.TabIndex = 5;
      this.groupBox_x64.TabStop = false;
      this.groupBox_x64.Text = "Additional info";
      this.groupBox_x64.Visible = false;
      // 
      // linkLabel_x64
      // 
      this.linkLabel_x64.AutoSize = true;
      this.linkLabel_x64.Location = new System.Drawing.Point(16, 42);
      this.linkLabel_x64.Name = "linkLabel_x64";
      this.linkLabel_x64.Size = new System.Drawing.Size(259, 26);
      this.linkLabel_x64.TabIndex = 0;
      this.linkLabel_x64.TabStop = true;
      this.linkLabel_x64.Text = "Please check wiki to learn how to enable your remote\r\non a x64 operating system";
      this.linkLabel_x64.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_x64_LinkClicked);
      // 
      // groupBoxFireDTVRecieiverSettings
      // 
      this.groupBoxFireDTVRecieiverSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFireDTVRecieiverSettings.Controls.Add(this.labelFireDTVModel);
      this.groupBoxFireDTVRecieiverSettings.Controls.Add(this.comboBoxFireDTVReceiver);
      this.groupBoxFireDTVRecieiverSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxFireDTVRecieiverSettings.Location = new System.Drawing.Point(12, 86);
      this.groupBoxFireDTVRecieiverSettings.Name = "groupBoxFireDTVRecieiverSettings";
      this.groupBoxFireDTVRecieiverSettings.Size = new System.Drawing.Size(448, 55);
      this.groupBoxFireDTVRecieiverSettings.TabIndex = 2;
      this.groupBoxFireDTVRecieiverSettings.TabStop = false;
      this.groupBoxFireDTVRecieiverSettings.Text = "Receiver settings";
      // 
      // labelFireDTVModel
      // 
      this.labelFireDTVModel.AutoSize = true;
      this.labelFireDTVModel.Location = new System.Drawing.Point(198, 25);
      this.labelFireDTVModel.Name = "labelFireDTVModel";
      this.labelFireDTVModel.Size = new System.Drawing.Size(236, 13);
      this.labelFireDTVModel.TabIndex = 2;
      this.labelFireDTVModel.Text = "Multiple FireDTV can be connected - select one.";
      // 
      // comboBoxFireDTVReceiver
      // 
      this.comboBoxFireDTVReceiver.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxFireDTVReceiver.DisplayMember = "FriendlyName";
      this.comboBoxFireDTVReceiver.FormattingEnabled = true;
      this.comboBoxFireDTVReceiver.Location = new System.Drawing.Point(16, 22);
      this.comboBoxFireDTVReceiver.Name = "comboBoxFireDTVReceiver";
      this.comboBoxFireDTVReceiver.Size = new System.Drawing.Size(168, 21);
      this.comboBoxFireDTVReceiver.TabIndex = 0;
      this.comboBoxFireDTVReceiver.ValueMember = "Name";
      // 
      // groupBoxFireDTVReceiverGeneral
      // 
      this.groupBoxFireDTVReceiverGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFireDTVReceiverGeneral.Controls.Add(this.checkBoxFireDTVExtendedLogging);
      this.groupBoxFireDTVReceiverGeneral.Controls.Add(this.buttonFireDTVMapping);
      this.groupBoxFireDTVReceiverGeneral.Controls.Add(this.checkBoxFireDTVEnabled);
      this.groupBoxFireDTVReceiverGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxFireDTVReceiverGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxFireDTVReceiverGeneral.Name = "groupBoxFireDTVReceiverGeneral";
      this.groupBoxFireDTVReceiverGeneral.Size = new System.Drawing.Size(448, 72);
      this.groupBoxFireDTVReceiverGeneral.TabIndex = 0;
      this.groupBoxFireDTVReceiverGeneral.TabStop = false;
      // 
      // checkBoxFireDTVExtendedLogging
      // 
      this.checkBoxFireDTVExtendedLogging.AutoSize = true;
      this.checkBoxFireDTVExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxFireDTVExtendedLogging.Location = new System.Drawing.Point(16, 47);
      this.checkBoxFireDTVExtendedLogging.Name = "checkBoxFireDTVExtendedLogging";
      this.checkBoxFireDTVExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxFireDTVExtendedLogging.TabIndex = 0;
      this.checkBoxFireDTVExtendedLogging.Text = "Extended logging";
      this.checkBoxFireDTVExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // buttonFireDTVMapping
      // 
      this.buttonFireDTVMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFireDTVMapping.Location = new System.Drawing.Point(359, 21);
      this.buttonFireDTVMapping.Name = "buttonFireDTVMapping";
      this.buttonFireDTVMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonFireDTVMapping.TabIndex = 1;
      this.buttonFireDTVMapping.Text = "Mapping";
      this.buttonFireDTVMapping.UseVisualStyleBackColor = true;
      this.buttonFireDTVMapping.Click += new System.EventHandler(this.buttonFireDTVMapping_Click);
      // 
      // checkBoxFireDTVEnabled
      // 
      this.checkBoxFireDTVEnabled.AutoSize = true;
      this.checkBoxFireDTVEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxFireDTVEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxFireDTVEnabled.Name = "checkBoxFireDTVEnabled";
      this.checkBoxFireDTVEnabled.Size = new System.Drawing.Size(126, 17);
      this.checkBoxFireDTVEnabled.TabIndex = 0;
      this.checkBoxFireDTVEnabled.Text = "Use FireDTV receiver";
      this.checkBoxFireDTVEnabled.UseVisualStyleBackColor = true;
      this.checkBoxFireDTVEnabled.CheckedChanged += new System.EventHandler(this.checkBoxFireDTVEnabled_CheckedChanged);
      // 
      // pictureBox3
      // 
      this.pictureBox3.Image = global::MediaPortal.Configuration.Properties.Resources.remote_FireDtv;
      this.pictureBox3.Location = new System.Drawing.Point(12, 149);
      this.pictureBox3.Name = "pictureBox3";
      this.pictureBox3.Size = new System.Drawing.Size(122, 222);
      this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox3.TabIndex = 4;
      this.pictureBox3.TabStop = false;
      // 
      // tabPageX10
      // 
      this.tabPageX10.Controls.Add(this.groupBoxX10Status);
      this.tabPageX10.Controls.Add(this.groupBoxX10General);
      this.tabPageX10.Controls.Add(this.groupBoxX10Settings);
      this.tabPageX10.Location = new System.Drawing.Point(4, 22);
      this.tabPageX10.Name = "tabPageX10";
      this.tabPageX10.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageX10.Size = new System.Drawing.Size(472, 422);
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
      this.groupBoxX10Status.Location = new System.Drawing.Point(12, 230);
      this.groupBoxX10Status.Name = "groupBoxX10Status";
      this.groupBoxX10Status.Size = new System.Drawing.Size(448, 123);
      this.groupBoxX10Status.TabIndex = 2;
      this.groupBoxX10Status.TabStop = false;
      this.groupBoxX10Status.Text = "Status";
      // 
      // labelX10DriverInfo
      // 
      this.labelX10DriverInfo.AutoSize = true;
      this.labelX10DriverInfo.Location = new System.Drawing.Point(13, 24);
      this.labelX10DriverInfo.Name = "labelX10DriverInfo";
      this.labelX10DriverInfo.Size = new System.Drawing.Size(392, 13);
      this.labelX10DriverInfo.TabIndex = 0;
      this.labelX10DriverInfo.Text = "You have to use the driver below, or your remote might not work with MediaPortal." +
    "";
      // 
      // linkLabelDownloadX10
      // 
      this.linkLabelDownloadX10.AutoSize = true;
      this.linkLabelDownloadX10.Location = new System.Drawing.Point(16, 64);
      this.linkLabelDownloadX10.Name = "linkLabelDownloadX10";
      this.linkLabelDownloadX10.Size = new System.Drawing.Size(222, 13);
      this.linkLabelDownloadX10.TabIndex = 1;
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
      this.groupBoxX10General.Controls.Add(this.buttonX10LearnMapping);
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Firefly);
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Other);
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Ati);
      this.groupBoxX10General.Controls.Add(this.radioButtonX10Medion);
      this.groupBoxX10General.Controls.Add(this.checkBoxX10Enabled);
      this.groupBoxX10General.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxX10General.Location = new System.Drawing.Point(12, 8);
      this.groupBoxX10General.Name = "groupBoxX10General";
      this.groupBoxX10General.Size = new System.Drawing.Size(448, 128);
      this.groupBoxX10General.TabIndex = 0;
      this.groupBoxX10General.TabStop = false;
      // 
      // buttonX10LearnMapping
      // 
      this.buttonX10LearnMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10LearnMapping.Location = new System.Drawing.Point(359, 21);
      this.buttonX10LearnMapping.Name = "buttonX10LearnMapping";
      this.buttonX10LearnMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonX10LearnMapping.TabIndex = 2;
      this.buttonX10LearnMapping.Text = "&Learn";
      this.buttonX10LearnMapping.UseVisualStyleBackColor = true;
      this.buttonX10LearnMapping.Click += new System.EventHandler(this.buttonX10LearnMapping_Click);
      // 
      // radioButtonX10Firefly
      // 
      this.radioButtonX10Firefly.AutoSize = true;
      this.radioButtonX10Firefly.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Firefly.Location = new System.Drawing.Point(32, 82);
      this.radioButtonX10Firefly.Name = "radioButtonX10Firefly";
      this.radioButtonX10Firefly.Size = new System.Drawing.Size(82, 17);
      this.radioButtonX10Firefly.TabIndex = 3;
      this.radioButtonX10Firefly.Text = "Firefly model";
      this.radioButtonX10Firefly.UseVisualStyleBackColor = true;
      // 
      // radioButtonX10Other
      // 
      this.radioButtonX10Other.AutoSize = true;
      this.radioButtonX10Other.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Other.Location = new System.Drawing.Point(32, 100);
      this.radioButtonX10Other.Name = "radioButtonX10Other";
      this.radioButtonX10Other.Size = new System.Drawing.Size(48, 17);
      this.radioButtonX10Other.TabIndex = 4;
      this.radioButtonX10Other.Text = "other";
      this.radioButtonX10Other.UseVisualStyleBackColor = true;
      // 
      // radioButtonX10Ati
      // 
      this.radioButtonX10Ati.AutoSize = true;
      this.radioButtonX10Ati.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Ati.Location = new System.Drawing.Point(32, 64);
      this.radioButtonX10Ati.Name = "radioButtonX10Ati";
      this.radioButtonX10Ati.Size = new System.Drawing.Size(72, 17);
      this.radioButtonX10Ati.TabIndex = 2;
      this.radioButtonX10Ati.Text = "ATI model";
      this.radioButtonX10Ati.UseVisualStyleBackColor = true;
      // 
      // radioButtonX10Medion
      // 
      this.radioButtonX10Medion.AutoSize = true;
      this.radioButtonX10Medion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonX10Medion.Location = new System.Drawing.Point(32, 46);
      this.radioButtonX10Medion.Name = "radioButtonX10Medion";
      this.radioButtonX10Medion.Size = new System.Drawing.Size(90, 17);
      this.radioButtonX10Medion.TabIndex = 1;
      this.radioButtonX10Medion.Text = "Medion model";
      this.radioButtonX10Medion.UseVisualStyleBackColor = true;
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
      // groupBoxX10Settings
      // 
      this.groupBoxX10Settings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxX10Settings.Controls.Add(this.LabelChannelNumber);
      this.groupBoxX10Settings.Controls.Add(this.TextBoxChannelNumber);
      this.groupBoxX10Settings.Controls.Add(this.checkBoxX10ChannelControl);
      this.groupBoxX10Settings.Controls.Add(this.checkBoxX10ExtendedLogging);
      this.groupBoxX10Settings.Controls.Add(this.buttonX10Defaults);
      this.groupBoxX10Settings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxX10Settings.Location = new System.Drawing.Point(12, 144);
      this.groupBoxX10Settings.Name = "groupBoxX10Settings";
      this.groupBoxX10Settings.Size = new System.Drawing.Size(448, 80);
      this.groupBoxX10Settings.TabIndex = 1;
      this.groupBoxX10Settings.TabStop = false;
      this.groupBoxX10Settings.Text = "Settings";
      // 
      // LabelChannelNumber
      // 
      this.LabelChannelNumber.AutoSize = true;
      this.LabelChannelNumber.Location = new System.Drawing.Point(259, 52);
      this.LabelChannelNumber.Name = "LabelChannelNumber";
      this.LabelChannelNumber.Size = new System.Drawing.Size(87, 13);
      this.LabelChannelNumber.TabIndex = 5;
      this.LabelChannelNumber.Text = "Channel number:";
      // 
      // TextBoxChannelNumber
      // 
      this.TextBoxChannelNumber.BorderColor = System.Drawing.Color.Empty;
      this.TextBoxChannelNumber.Location = new System.Drawing.Point(351, 48);
      this.TextBoxChannelNumber.Name = "TextBoxChannelNumber";
      this.TextBoxChannelNumber.Size = new System.Drawing.Size(72, 20);
      this.TextBoxChannelNumber.TabIndex = 4;
      // 
      // checkBoxX10ChannelControl
      // 
      this.checkBoxX10ChannelControl.AutoSize = true;
      this.checkBoxX10ChannelControl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxX10ChannelControl.Location = new System.Drawing.Point(16, 48);
      this.checkBoxX10ChannelControl.Name = "checkBoxX10ChannelControl";
      this.checkBoxX10ChannelControl.Size = new System.Drawing.Size(136, 17);
      this.checkBoxX10ChannelControl.TabIndex = 1;
      this.checkBoxX10ChannelControl.Text = "Use RF channel control";
      this.checkBoxX10ChannelControl.UseVisualStyleBackColor = true;
      this.checkBoxX10ChannelControl.CheckedChanged += new System.EventHandler(this.checkBoxX10ChannelControl_CheckedChanged);
      // 
      // checkBoxX10ExtendedLogging
      // 
      this.checkBoxX10ExtendedLogging.AutoSize = true;
      this.checkBoxX10ExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxX10ExtendedLogging.Location = new System.Drawing.Point(16, 25);
      this.checkBoxX10ExtendedLogging.Name = "checkBoxX10ExtendedLogging";
      this.checkBoxX10ExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxX10ExtendedLogging.TabIndex = 0;
      this.checkBoxX10ExtendedLogging.Text = "Extended logging";
      this.checkBoxX10ExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // buttonX10Defaults
      // 
      this.buttonX10Defaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonX10Defaults.Location = new System.Drawing.Point(359, 22);
      this.buttonX10Defaults.Name = "buttonX10Defaults";
      this.buttonX10Defaults.Size = new System.Drawing.Size(72, 22);
      this.buttonX10Defaults.TabIndex = 3;
      this.buttonX10Defaults.Text = "&Defaults";
      this.buttonX10Defaults.UseVisualStyleBackColor = true;
      this.buttonX10Defaults.Click += new System.EventHandler(this.buttonX10Defaults_Click);
      // 
      // tabPageHcw
      // 
      this.tabPageHcw.Controls.Add(this.groupBoxHcwRepeatDelay);
      this.tabPageHcw.Controls.Add(this.groupBoxHcwGeneral);
      this.tabPageHcw.Controls.Add(this.groupBoxHcwStatus);
      this.tabPageHcw.Controls.Add(this.groupBoxHcwSettings);
      this.tabPageHcw.Location = new System.Drawing.Point(4, 22);
      this.tabPageHcw.Name = "tabPageHcw";
      this.tabPageHcw.Size = new System.Drawing.Size(472, 422);
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
      this.groupBoxHcwRepeatDelay.Size = new System.Drawing.Size(448, 112);
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
      this.hScrollBarHcwRepeatSpeed.Size = new System.Drawing.Size(221, 17);
      this.hScrollBarHcwRepeatSpeed.TabIndex = 10;
      this.hScrollBarHcwRepeatSpeed.Value = 1;
      this.hScrollBarHcwRepeatSpeed.ValueChanged += new System.EventHandler(this.hScrollBarHcwRepeatSpeed_ValueChanged);
      // 
      // labelHcwFast
      // 
      this.labelHcwFast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcwFast.AutoSize = true;
      this.labelHcwFast.Location = new System.Drawing.Point(376, 82);
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
      this.hScrollBarHcwRepeatFilter.Size = new System.Drawing.Size(221, 17);
      this.hScrollBarHcwRepeatFilter.TabIndex = 6;
      this.hScrollBarHcwRepeatFilter.Value = 2;
      this.hScrollBarHcwRepeatFilter.ValueChanged += new System.EventHandler(this.hScrollBarHcwRepeatFilter_ValueChanged);
      // 
      // labelHcwMax
      // 
      this.labelHcwMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcwMax.AutoSize = true;
      this.labelHcwMax.Location = new System.Drawing.Point(376, 54);
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
      this.hScrollBarHcwButtonRelease.Size = new System.Drawing.Size(221, 17);
      this.hScrollBarHcwButtonRelease.TabIndex = 2;
      this.hScrollBarHcwButtonRelease.Value = 500;
      this.hScrollBarHcwButtonRelease.ValueChanged += new System.EventHandler(this.hScrollBarHcwButtonRelease_ValueChanged);
      // 
      // labelHcw1000msec
      // 
      this.labelHcw1000msec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcw1000msec.AutoSize = true;
      this.labelHcw1000msec.Location = new System.Drawing.Point(376, 27);
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
      this.groupBoxHcwGeneral.Controls.Add(this.HCWLearn);
      this.groupBoxHcwGeneral.Controls.Add(this.buttonHcwDefaults);
      this.groupBoxHcwGeneral.Controls.Add(this.checkBoxHcwEnabled);
      this.groupBoxHcwGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHcwGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxHcwGeneral.Name = "groupBoxHcwGeneral";
      this.groupBoxHcwGeneral.Size = new System.Drawing.Size(448, 56);
      this.groupBoxHcwGeneral.TabIndex = 0;
      this.groupBoxHcwGeneral.TabStop = false;
      // 
      // HCWLearn
      // 
      this.HCWLearn.Location = new System.Drawing.Point(351, 21);
      this.HCWLearn.Name = "HCWLearn";
      this.HCWLearn.Size = new System.Drawing.Size(72, 22);
      this.HCWLearn.TabIndex = 2;
      this.HCWLearn.Text = "Learn";
      this.HCWLearn.UseVisualStyleBackColor = true;
      this.HCWLearn.Click += new System.EventHandler(this.HCWLearn_Click);
      // 
      // buttonHcwDefaults
      // 
      this.buttonHcwDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonHcwDefaults.Location = new System.Drawing.Point(281, 21);
      this.buttonHcwDefaults.Name = "buttonHcwDefaults";
      this.buttonHcwDefaults.Size = new System.Drawing.Size(72, 22);
      this.buttonHcwDefaults.TabIndex = 1;
      this.buttonHcwDefaults.Text = "&Defaults";
      this.buttonHcwDefaults.UseVisualStyleBackColor = true;
      this.buttonHcwDefaults.Click += new System.EventHandler(this.buttonHcwDefaults_Click);
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
      this.groupBoxHcwStatus.Controls.Add(this.labelHcwDriverStatus);
      this.groupBoxHcwStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxHcwStatus.Location = new System.Drawing.Point(12, 280);
      this.groupBoxHcwStatus.Name = "groupBoxHcwStatus";
      this.groupBoxHcwStatus.Size = new System.Drawing.Size(448, 80);
      this.groupBoxHcwStatus.TabIndex = 3;
      this.groupBoxHcwStatus.TabStop = false;
      this.groupBoxHcwStatus.Text = "Status";
      // 
      // linkLabelHcwDownload
      // 
      this.linkLabelHcwDownload.AutoSize = true;
      this.linkLabelHcwDownload.Location = new System.Drawing.Point(12, 54);
      this.linkLabelHcwDownload.Name = "linkLabelHcwDownload";
      this.linkLabelHcwDownload.Size = new System.Drawing.Size(275, 13);
      this.linkLabelHcwDownload.TabIndex = 2;
      this.linkLabelHcwDownload.TabStop = true;
      this.linkLabelHcwDownload.Text = "Click here for the latest \"IR.exe\" driver (inside WinTV cd)";
      this.linkLabelHcwDownload.Visible = false;
      this.linkLabelHcwDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHcwDownload_LinkClicked);
      // 
      // labelHcwDriverStatus
      // 
      this.labelHcwDriverStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelHcwDriverStatus.ForeColor = System.Drawing.SystemColors.ControlText;
      this.labelHcwDriverStatus.Location = new System.Drawing.Point(12, 24);
      this.labelHcwDriverStatus.Name = "labelHcwDriverStatus";
      this.labelHcwDriverStatus.Size = new System.Drawing.Size(422, 48);
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
      this.groupBoxHcwSettings.Size = new System.Drawing.Size(448, 80);
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
      this.checkBoxHcwKeepControl.Location = new System.Drawing.Point(15, 48);
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
      // tabPageIrTrans
      // 
      this.tabPageIrTrans.Controls.Add(this.groupBoxIrTransStatus);
      this.tabPageIrTrans.Controls.Add(this.groupBoxIrTransServerSettings);
      this.tabPageIrTrans.Controls.Add(this.groupBoxIrTransGeneral);
      this.tabPageIrTrans.Location = new System.Drawing.Point(4, 22);
      this.tabPageIrTrans.Name = "tabPageIrTrans";
      this.tabPageIrTrans.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageIrTrans.Size = new System.Drawing.Size(472, 422);
      this.tabPageIrTrans.TabIndex = 5;
      this.tabPageIrTrans.Text = "IRTrans";
      this.tabPageIrTrans.UseVisualStyleBackColor = true;
      // 
      // groupBoxIrTransStatus
      // 
      this.groupBoxIrTransStatus.Controls.Add(this.IRTransLink);
      this.groupBoxIrTransStatus.Controls.Add(this.mpLabel1);
      this.groupBoxIrTransStatus.Controls.Add(this.labelIrTransStatus);
      this.groupBoxIrTransStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxIrTransStatus.Location = new System.Drawing.Point(12, 271);
      this.groupBoxIrTransStatus.Name = "groupBoxIrTransStatus";
      this.groupBoxIrTransStatus.Size = new System.Drawing.Size(440, 89);
      this.groupBoxIrTransStatus.TabIndex = 5;
      this.groupBoxIrTransStatus.TabStop = false;
      this.groupBoxIrTransStatus.Text = "Status";
      // 
      // IRTransLink
      // 
      this.IRTransLink.AutoSize = true;
      this.IRTransLink.Location = new System.Drawing.Point(16, 53);
      this.IRTransLink.Name = "IRTransLink";
      this.IRTransLink.Size = new System.Drawing.Size(100, 13);
      this.IRTransLink.TabIndex = 10;
      this.IRTransLink.TabStop = true;
      this.IRTransLink.Text = "IRTrans Information";
      this.IRTransLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.IRTransLink_LinkClicked);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(16, 24);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(343, 13);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "If you experience duplicate key presses, please see the following article";
      // 
      // labelIrTransStatus
      // 
      this.labelIrTransStatus.Location = new System.Drawing.Point(16, 20);
      this.labelIrTransStatus.Name = "labelIrTransStatus";
      this.labelIrTransStatus.Size = new System.Drawing.Size(408, 20);
      this.labelIrTransStatus.TabIndex = 8;
      this.labelIrTransStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxIrTransServerSettings
      // 
      this.groupBoxIrTransServerSettings.Controls.Add(this.buttonIrTransTest);
      this.groupBoxIrTransServerSettings.Controls.Add(this.labelIrTransNoteModel);
      this.groupBoxIrTransServerSettings.Controls.Add(this.textBoxRemoteModel);
      this.groupBoxIrTransServerSettings.Controls.Add(this.labelIrTransRemoteModel);
      this.groupBoxIrTransServerSettings.Controls.Add(this.textBoxIrTransServerPort);
      this.groupBoxIrTransServerSettings.Controls.Add(this.labelIrTransServerPort);
      this.groupBoxIrTransServerSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxIrTransServerSettings.Location = new System.Drawing.Point(12, 136);
      this.groupBoxIrTransServerSettings.Name = "groupBoxIrTransServerSettings";
      this.groupBoxIrTransServerSettings.Size = new System.Drawing.Size(440, 129);
      this.groupBoxIrTransServerSettings.TabIndex = 4;
      this.groupBoxIrTransServerSettings.TabStop = false;
      this.groupBoxIrTransServerSettings.Text = "Server Settings";
      // 
      // buttonIrTransTest
      // 
      this.buttonIrTransTest.Location = new System.Drawing.Point(352, 20);
      this.buttonIrTransTest.Name = "buttonIrTransTest";
      this.buttonIrTransTest.Size = new System.Drawing.Size(72, 22);
      this.buttonIrTransTest.TabIndex = 10;
      this.buttonIrTransTest.Text = "Test";
      this.buttonIrTransTest.UseVisualStyleBackColor = true;
      this.buttonIrTransTest.Click += new System.EventHandler(this.buttonIrTransTest_Click);
      // 
      // labelIrTransNoteModel
      // 
      this.labelIrTransNoteModel.Location = new System.Drawing.Point(136, 76);
      this.labelIrTransNoteModel.Name = "labelIrTransNoteModel";
      this.labelIrTransNoteModel.Size = new System.Drawing.Size(292, 44);
      this.labelIrTransNoteModel.TabIndex = 8;
      this.labelIrTransNoteModel.Text = "This must be exactly the name of the remote as found in the *.rem file of IRTrans" +
    ", for example \"mediacenter\", when using the MCE remote.";
      // 
      // textBoxRemoteModel
      // 
      this.textBoxRemoteModel.BorderColor = System.Drawing.Color.Empty;
      this.textBoxRemoteModel.Location = new System.Drawing.Point(138, 52);
      this.textBoxRemoteModel.MaxLength = 128;
      this.textBoxRemoteModel.Name = "textBoxRemoteModel";
      this.textBoxRemoteModel.Size = new System.Drawing.Size(128, 20);
      this.textBoxRemoteModel.TabIndex = 3;
      // 
      // labelIrTransRemoteModel
      // 
      this.labelIrTransRemoteModel.AutoSize = true;
      this.labelIrTransRemoteModel.Location = new System.Drawing.Point(16, 56);
      this.labelIrTransRemoteModel.Name = "labelIrTransRemoteModel";
      this.labelIrTransRemoteModel.Size = new System.Drawing.Size(79, 13);
      this.labelIrTransRemoteModel.TabIndex = 2;
      this.labelIrTransRemoteModel.Text = "Remote Model:";
      // 
      // textBoxIrTransServerPort
      // 
      this.textBoxIrTransServerPort.Location = new System.Drawing.Point(138, 25);
      this.textBoxIrTransServerPort.MaxLength = 5;
      this.textBoxIrTransServerPort.Name = "textBoxIrTransServerPort";
      this.textBoxIrTransServerPort.Size = new System.Drawing.Size(58, 20);
      this.textBoxIrTransServerPort.TabIndex = 1;
      this.textBoxIrTransServerPort.Text = "21000";
      this.textBoxIrTransServerPort.Value = 21000;
      // 
      // labelIrTransServerPort
      // 
      this.labelIrTransServerPort.AutoSize = true;
      this.labelIrTransServerPort.Location = new System.Drawing.Point(16, 28);
      this.labelIrTransServerPort.Name = "labelIrTransServerPort";
      this.labelIrTransServerPort.Size = new System.Drawing.Size(104, 13);
      this.labelIrTransServerPort.TabIndex = 0;
      this.labelIrTransServerPort.Text = "IRTrans Server Port:";
      // 
      // groupBoxIrTransGeneral
      // 
      this.groupBoxIrTransGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxIrTransGeneral.Controls.Add(this.checkBoxIrTransExtendedLogging);
      this.groupBoxIrTransGeneral.Controls.Add(this.checkBoxIrTransEnabled);
      this.groupBoxIrTransGeneral.Controls.Add(this.buttonIrTransMapping);
      this.groupBoxIrTransGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxIrTransGeneral.Location = new System.Drawing.Point(12, 8);
      this.groupBoxIrTransGeneral.Name = "groupBoxIrTransGeneral";
      this.groupBoxIrTransGeneral.Size = new System.Drawing.Size(448, 72);
      this.groupBoxIrTransGeneral.TabIndex = 3;
      this.groupBoxIrTransGeneral.TabStop = false;
      // 
      // checkBoxIrTransExtendedLogging
      // 
      this.checkBoxIrTransExtendedLogging.AutoSize = true;
      this.checkBoxIrTransExtendedLogging.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxIrTransExtendedLogging.Location = new System.Drawing.Point(16, 47);
      this.checkBoxIrTransExtendedLogging.Name = "checkBoxIrTransExtendedLogging";
      this.checkBoxIrTransExtendedLogging.Size = new System.Drawing.Size(106, 17);
      this.checkBoxIrTransExtendedLogging.TabIndex = 7;
      this.checkBoxIrTransExtendedLogging.Text = "Extended logging";
      this.checkBoxIrTransExtendedLogging.UseVisualStyleBackColor = true;
      // 
      // checkBoxIrTransEnabled
      // 
      this.checkBoxIrTransEnabled.AutoSize = true;
      this.checkBoxIrTransEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxIrTransEnabled.Location = new System.Drawing.Point(16, 24);
      this.checkBoxIrTransEnabled.Name = "checkBoxIrTransEnabled";
      this.checkBoxIrTransEnabled.Size = new System.Drawing.Size(125, 17);
      this.checkBoxIrTransEnabled.TabIndex = 0;
      this.checkBoxIrTransEnabled.Text = "Use IRTrans receiver";
      this.checkBoxIrTransEnabled.UseVisualStyleBackColor = true;
      this.checkBoxIrTransEnabled.CheckedChanged += new System.EventHandler(this.checkBoxIrTransEnabled_CheckedChanged);
      // 
      // buttonIrTransMapping
      // 
      this.buttonIrTransMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonIrTransMapping.Location = new System.Drawing.Point(359, 21);
      this.buttonIrTransMapping.Name = "buttonIrTransMapping";
      this.buttonIrTransMapping.Size = new System.Drawing.Size(72, 22);
      this.buttonIrTransMapping.TabIndex = 1;
      this.buttonIrTransMapping.Text = "Mapping";
      this.buttonIrTransMapping.UseVisualStyleBackColor = true;
      this.buttonIrTransMapping.Click += new System.EventHandler(this.buttonIrTransMapping_Click);
      // 
      // mpCheckBox1
      // 
      this.mpCheckBox1.AutoSize = true;
      this.mpCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox1.Location = new System.Drawing.Point(16, 47);
      this.mpCheckBox1.Name = "mpCheckBox1";
      this.mpCheckBox1.Size = new System.Drawing.Size(106, 17);
      this.mpCheckBox1.TabIndex = 4;
      this.mpCheckBox1.Text = "Extended logging";
      this.mpCheckBox1.UseVisualStyleBackColor = true;
      // 
      // mpCheckBox2
      // 
      this.mpCheckBox2.AutoSize = true;
      this.mpCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox2.Location = new System.Drawing.Point(16, 24);
      this.mpCheckBox2.Name = "mpCheckBox2";
      this.mpCheckBox2.Size = new System.Drawing.Size(209, 17);
      this.mpCheckBox2.TabIndex = 0;
      this.mpCheckBox2.Text = "Use Microsoft MCE remote or keyboard";
      this.mpCheckBox2.UseVisualStyleBackColor = true;
      // 
      // mpButton1
      // 
      this.mpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButton1.Location = new System.Drawing.Point(351, 21);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(72, 22);
      this.mpButton1.TabIndex = 1;
      this.mpButton1.Text = "Mapping";
      this.mpButton1.UseVisualStyleBackColor = true;
      // 
      // Remote
      // 
      this.Controls.Add(this.tabControlRemotes);
      this.Name = "Remote";
      this.Size = new System.Drawing.Size(480, 456);
      this.tabControlRemotes.ResumeLayout(false);
      this.tabPageHid.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox11)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      this.groupBoxHidGeneral.ResumeLayout(false);
      this.groupBoxHidGeneral.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericRepeatSpeed)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericRepeatDelay)).EndInit();
      this.tabPageAppCommand.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
      this.tabPageCentarea.ResumeLayout(false);
      this.groupBoxCentareaOptions.ResumeLayout(false);
      this.groupBoxCentareaOptions.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCentarea)).EndInit();
      this.tabPageFireDtv.ResumeLayout(false);
      this.groupBox_x64.ResumeLayout(false);
      this.groupBox_x64.PerformLayout();
      this.groupBoxFireDTVRecieiverSettings.ResumeLayout(false);
      this.groupBoxFireDTVRecieiverSettings.PerformLayout();
      this.groupBoxFireDTVReceiverGeneral.ResumeLayout(false);
      this.groupBoxFireDTVReceiverGeneral.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
      this.tabPageX10.ResumeLayout(false);
      this.groupBoxX10Status.ResumeLayout(false);
      this.groupBoxX10Status.PerformLayout();
      this.groupBoxX10General.ResumeLayout(false);
      this.groupBoxX10General.PerformLayout();
      this.groupBoxX10Settings.ResumeLayout(false);
      this.groupBoxX10Settings.PerformLayout();
      this.tabPageHcw.ResumeLayout(false);
      this.groupBoxHcwRepeatDelay.ResumeLayout(false);
      this.groupBoxHcwRepeatDelay.PerformLayout();
      this.groupBoxHcwGeneral.ResumeLayout(false);
      this.groupBoxHcwGeneral.PerformLayout();
      this.groupBoxHcwStatus.ResumeLayout(false);
      this.groupBoxHcwStatus.PerformLayout();
      this.groupBoxHcwSettings.ResumeLayout(false);
      this.groupBoxHcwSettings.PerformLayout();
      this.tabPageIrTrans.ResumeLayout(false);
      this.groupBoxIrTransStatus.ResumeLayout(false);
      this.groupBoxIrTransStatus.PerformLayout();
      this.groupBoxIrTransServerSettings.ResumeLayout(false);
      this.groupBoxIrTransServerSettings.PerformLayout();
      this.groupBoxIrTransGeneral.ResumeLayout(false);
      this.groupBoxIrTransGeneral.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    #endregion

    #region Form control commands HCW

    //
    // External processes may use the remote control
    //
    private void checkBoxHcwAllowExternal_CheckedChanged(object sender, EventArgs e)
    {
      checkBoxHcwKeepControl.Enabled = checkBoxHcwAllowExternal.Checked;
    }

    //
    // Use Hauppauge remote
    //
    private void checkBoxHcwEnabled_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxHcwSettings.Enabled = checkBoxHcwEnabled.Checked;
      groupBoxHcwRepeatDelay.Enabled = checkBoxHcwEnabled.Checked;
      buttonHcwDefaults.Enabled = HCWLearn.Enabled = checkBoxHcwEnabled.Checked;
    }

    //
    // Reset to default
    //    
    private void buttonHcwDefaults_Click(object sender, EventArgs e)
    {
      checkBoxHcwAllowExternal.Checked = false;
      checkBoxHcwKeepControl.Checked = false;
      checkBoxHcwExtendedLogging.Checked = false;
      hScrollBarHcwButtonRelease.Value = 200;
      hScrollBarHcwRepeatFilter.Value = 2;
      hScrollBarHcwRepeatSpeed.Value = 0;
      checkBoxHcwFilterDoubleKlicks.Checked = false;
    }

    private void hScrollBarHcwButtonRelease_ValueChanged(object sender, EventArgs e)
    {
      toolTip.SetToolTip(hScrollBarHcwButtonRelease, string.Format("{0} msec.", hScrollBarHcwButtonRelease.Value));
    }

    private void hScrollBarHcwRepeatFilter_ValueChanged(object sender, EventArgs e)
    {
      toolTip.SetToolTip(hScrollBarHcwRepeatFilter, hScrollBarHcwRepeatFilter.Value.ToString());
    }

    private void hScrollBarHcwRepeatSpeed_ValueChanged(object sender, EventArgs e)
    {
      Type repeatSpeed = typeof (hcwRepeatSpeed);
      toolTip.SetToolTip(hScrollBarHcwRepeatSpeed, Enum.GetName(repeatSpeed, 2 - hScrollBarHcwRepeatSpeed.Value));
    }

    private void linkLabelHcwDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://www.hauppauge.co.uk/site/support/downloadcounter.php/");
    }

    private void HCWLearn_Click(object sender, EventArgs e)
    {
      RemoteLearn TeachHCW = new RemoteLearn("HCW", "Hauppauge HCW", HCWRemote);
      ;
      if (TeachHCW != null)
      {
        TeachHCW.ShowDialog(this);
      }
    }

    #endregion

    #region Form control commands X10

    private void buttonX10Defaults_Click(object sender, EventArgs e)
    {
      DialogResult reset = MessageBox.Show("Do you wish to lose all customized settings for the X10 Remote?",
                                           "Reset X10",
                                           MessageBoxButtons.OKCancel, MessageBoxIcon.None);
      if (reset == DialogResult.OK)
      {
        if (radioButtonX10Medion.Checked)
        {
          X10mapping = "Medion X10";
        }
        else if (radioButtonX10Ati.Checked)
        {
          X10mapping = "ATI X10";
        }
        else if (radioButtonX10Firefly.Checked)
        {
          X10mapping = "Firefly X10";
        }
        else
        {
          X10mapping = "Other X10";
        }

        string pathCustom = Path.Combine(InputHandler.CustomizedMappingsDirectory, X10mapping + ".xml");
        if (File.Exists(pathCustom))
        {
          File.Delete(pathCustom);
        }
        checkBoxX10ExtendedLogging.Checked = false;

        MessageBox.Show("All X10 Customizations have been reset");
      }
    }

    private void checkBoxX10Enabled_CheckedChanged(object sender, EventArgs e)
    {
      radioButtonX10Medion.Enabled =
        radioButtonX10Ati.Enabled =
        radioButtonX10Firefly.Enabled =
        radioButtonX10Other.Enabled =
        buttonX10LearnMapping.Enabled = groupBoxX10Settings.Enabled = checkBoxX10Enabled.Checked;
      TextBoxChannelNumber.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;
    }

    private void linkLabelDownloadX10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://ftp.x10.com/pub/applications/drivers/x10drivers_rf_all.exe");
    }

    private void checkBoxX10ChannelControl_CheckedChanged(object sender, EventArgs e)
    {
      TextBoxChannelNumber.Enabled = checkBoxX10ChannelControl.Enabled && checkBoxX10ChannelControl.Checked;
    }

    private void buttonX10LearnMapping_Click(object sender, EventArgs e)
    {
      RemoteLearn TeachX10;

      if (radioButtonX10Medion.Checked)
      {
        TeachX10 = new RemoteLearn("X10", "Medion X10", X10Remote);
      }
      else if (radioButtonX10Ati.Checked)
      {
        TeachX10 = new RemoteLearn("X10", "ATI X10", X10Remote);
      }
      else if (radioButtonX10Firefly.Checked)
      {
        TeachX10 = new RemoteLearn("X10", "Firefly X10", X10Remote);
      }
      else
      {
        TeachX10 = new RemoteLearn("X10", "Other X10", X10Remote);
      }

      TeachX10.ShowDialog(this);
    }

    #endregion

    #region Form control commands FireDTV

    /// <summary>
    /// Open the dialogbox for changing the inputmap.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonFireDTVMapping_Click(object sender, EventArgs e)
    {
      InputMappingForm dlg = new InputMappingForm("FireDTV");
      dlg.ShowDialog(this);
    }

    /// <summary>
    /// Toggle the FireDTV Setting on/off when enable checkbox is toggled.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void checkBoxFireDTVEnabled_CheckedChanged(object sender, EventArgs e)
    {
      buttonFireDTVMapping.Enabled = checkBoxFireDTVEnabled.Checked;
      groupBoxFireDTVRecieiverSettings.Enabled = checkBoxFireDTVEnabled.Checked;
      checkBoxFireDTVExtendedLogging.Enabled = checkBoxFireDTVEnabled.Checked;
      comboBoxFireDTVReceiver.Enabled = checkBoxFireDTVEnabled.Checked;
    }

    private void linkLabel_x64_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://wiki.team-mediaportal.com/MediaPortal/Configuration/Remote/FireDTV");
    }

    #endregion

    #region Form control commands Generic HID

    private void buttonHidMapping_Click(object sender, EventArgs e)
    {
      HidInputMappingForm dlg = new HidInputMappingForm(comboBoxHidProfiles.Text);
      dlg.ShowDialog(this);
      PopulateProfiles(dlg.ProfileName);
    }

    private void checkBoxHidEnabled_CheckedChanged(object sender, EventArgs e)
    {
      buttonHidMapping.Enabled = checkBoxHidEnabled.Checked;
    }

    private void buttonHidDefaults_Click(object sender, EventArgs e)
    {
      checkBoxHidExtendedLogging.Checked = false;
    }

    #endregion

    #region Form control commands IRTrans

    private void checkBoxIrTransEnabled_CheckedChanged(object sender, EventArgs e)
    {
      buttonIrTransMapping.Enabled = checkBoxIrTransEnabled.Checked;
      groupBoxIrTransStatus.Enabled = groupBoxIrTransServerSettings.Enabled = checkBoxIrTransEnabled.Checked;
    }

    private void buttonIrTransMapping_Click(object sender, EventArgs e)
    {
      // Construct xml file name "IrTrans " + remotename
      string keyfile = "IrTrans " + textBoxRemoteModel.Text.Trim();
      InputMappingForm dlg = new InputMappingForm(keyfile);
      dlg.ShowDialog(this);
    }

    private void buttonIrTransTest_Click(object sender, EventArgs e)
    {
      try
      {
        Socket m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.Connect("localhost", Convert.ToInt32(textBoxIrTransServerPort.Text.Trim()));
        // Send Client id to Server
        int clientID = 0;
        byte[] sendData = BitConverter.GetBytes(clientID);
        m_Socket.Send(sendData, sendData.Length, SocketFlags.None);
        m_Socket.Close();
        labelIrTransStatus.Text = "IRTrans server up and running.";
      }
      catch (SocketException)
      {
        labelIrTransStatus.Text = "No IRTrans server found.";
      }
    }


    private void IRTransLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://wiki.team-mediaportal.com/MediaPortalSetup_Remote/IRTrans");
    }

    #endregion

    #region Form control commands Sceneo

    private void buttonCentareaMapping_Click(object sender, EventArgs e)
    {
      InputMappingForm dlg;

      dlg = new InputMappingForm("Centarea HID");

      dlg.ShowDialog(this);
    }

    private void checkBoxCentareaEnabled_CheckedChanged(object sender, EventArgs e)
    {
      buttonCentareaMapping.Enabled = checkBoxCentareaEnabled.Checked;
    }

    #endregion

    private void mpButtonAppCommandMapping_Click(object sender, EventArgs e)
    {
        InputMappingForm dlg;

        dlg = new InputMappingForm("AppCommand");

        dlg.ShowDialog(this);
    }

    private void mpCheckBoxAppCommandEnabled_CheckedChanged(object sender, EventArgs e)
    {
        //Enable/Disable mapping button 
        mpButtonAppCommandMapping.Enabled = mpCheckBoxAppCommandEnabled.Checked;
    }

    private void pictureBox11_Click(object sender, EventArgs e)
    {

    }

    private void pictureBox6_Click(object sender, EventArgs e)
    {

    }

    private void linkLabelDocumentation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      ProcessStartInfo sInfo = new ProcessStartInfo("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/141_Configuration/MediaPortal_Configuration/8_Remote/HID");
      Process.Start(sInfo);
    }

    private void linkLabelMediaDocumentation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      ProcessStartInfo sInfo = new ProcessStartInfo("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/141_Configuration/MediaPortal_Configuration/8_Remote/Media");
      Process.Start(sInfo);
    }

    private void mpButtonHidProfiles_Click(object sender, EventArgs e)
    {
      //TODO: show profile selection dialog

    }

  }
}