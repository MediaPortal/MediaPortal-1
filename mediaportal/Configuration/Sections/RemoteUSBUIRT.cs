#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.IR;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class RemoteUSBUIRT : SectionSettings
  {
    // ReSharper disable InconsistentNaming
    private const string USBUIRT_PLUGINVER = "1.17 (June 06, 2009)";
    // ReSharper restore InconsistentNaming

    private MPGroupBox _groupBox1;
    private MPCheckBox _inputCheckBox;
    private MPCheckBox _outputCheckBox;
    private MPCheckBox _digitCheckBox;
    private MPCheckBox _enterCheckBox;
    private MPButton _internalCommandsButton;
    private MPButton _tunerCommandsButton;
    private MPLabel _label1;
    private MPLabel _usbuirtVersion;
    private MPGroupBox _groupBox2;
    private MPLabel _label2;
    private LinkLabel _linkLabel1;
    private MPLabel _label3;
    private Panel _settingsPnl;
    private MPLabel _label4;
    private MPLabel _label5;
    private NumericUpDown _commandRepeat;
    private NumericUpDown _interCommandDelay;
    private MPLabel _label6;
    private MPTextBox _testSendIrText;
    private MPButton _testSendIrButton;
    private MPGroupBox _groupBox3;
    private MPLabel _usbuirtConfigVersion;
    private MPLabel _label7;
    private MPLabel _labelDLLVersion;
    private MPLabel _labelAPIVersion;
    private MPLabel _valueDLLVersion;
    private MPLabel _valueAPIVersion;
    private Label _label8;
    private NumericUpDown _repeatDelay;
    private NumericUpDown _repeatWait;
    private Label _label9;
    private readonly IContainer components = null;

    #region Properties

    public override bool CanActivate
    {
      get { return true; }
    }

    #endregion

    public RemoteUSBUIRT()
      : this("USBUIRT") {}

    public RemoteUSBUIRT(string name)
      : base(name)
    {
      InitializeComponent();

      _usbuirtConfigVersion.Text = USBUIRT_PLUGINVER;
    }

    public override void OnSectionActivated()
    {
      if (_inputCheckBox.Checked || _outputCheckBox.Checked)
      {
        if (USBUIRT.Instance == null)
        {
          if (!InitUSBUIRTInstance())
          {
            Log.Info("USBUIRT: InitUSBUIRTInstance failed in OnSectionActivated!");
          }
        }
      }
    }

    private bool InitUSBUIRTInstance()
    {
      USBUIRT.Create(OnRemoteCommand);

      if (USBUIRT.Instance == null || !GetUsbUirtDriverStatusOk())
      {
        ShowDriverOfflineMsg();
        return false;
      }

      _usbuirtVersion.Text = USBUIRT.Instance.GetVersions();
      _valueAPIVersion.Text = USBUIRT.Instance.GetAPIVersions();
      _valueDLLVersion.Text = USBUIRT.Instance.GetDLLVersions();

      EnableTestIrControls();

      USBUIRT.Instance.CommandRepeatCount = (int)_commandRepeat.Value;
      USBUIRT.Instance.InterCommandDelay = (int)_interCommandDelay.Value;

      return true;
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

        if (USBUIRT.Instance != null)
        {
          USBUIRT.Instance.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          Log.Info("USBUIRT: Setting configuration control values");
          _inputCheckBox.Checked = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
          _outputCheckBox.Checked = xmlreader.GetValueAsBool("USBUIRT", "external", false);
          _digitCheckBox.Checked = xmlreader.GetValueAsBool("USBUIRT", "is3digit", false);
          _enterCheckBox.Checked = xmlreader.GetValueAsBool("USBUIRT", "needsenter", false);
          _repeatWait.Value = xmlreader.GetValueAsInt("USBUIRT", "repeatwait", 300);
          _repeatDelay.Value = xmlreader.GetValueAsInt("USBUIRT", "repeatdelay", 30);
          _commandRepeat.Value = xmlreader.GetValueAsInt("USBUIRT", "repeatcount", 2);
          _interCommandDelay.Value = xmlreader.GetValueAsInt("USBUIRT", "commanddelay", 100);
        }
        catch (Exception ex)
        {
          Log.Info("USBUIRT: Setting control values failed: " + ex.Message);
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("USBUIRT", "internal", _inputCheckBox.Checked);
        xmlwriter.SetValueAsBool("USBUIRT", "external", _outputCheckBox.Checked);
        xmlwriter.SetValueAsBool("USBUIRT", "is3digit", _digitCheckBox.Checked);
        xmlwriter.SetValueAsBool("USBUIRT", "needsenter", _enterCheckBox.Checked);
        xmlwriter.SetValue("USBUIRT", "repeatwait", _repeatWait.Value);
        xmlwriter.SetValue("USBUIRT", "repeatdelay", _repeatDelay.Value);
        xmlwriter.SetValue("USBUIRT", "repeatcount", _commandRepeat.Value);
        xmlwriter.SetValue("USBUIRT", "commanddelay", _interCommandDelay.Value);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteUSBUIRT));
      this._groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._settingsPnl = new System.Windows.Forms.Panel();
      this._repeatDelay = new System.Windows.Forms.NumericUpDown();
      this._repeatWait = new System.Windows.Forms.NumericUpDown();
      this._label9 = new System.Windows.Forms.Label();
      this._label8 = new System.Windows.Forms.Label();
      this._testSendIrButton = new MediaPortal.UserInterface.Controls.MPButton();
      this._testSendIrText = new MediaPortal.UserInterface.Controls.MPTextBox();
      this._interCommandDelay = new System.Windows.Forms.NumericUpDown();
      this._label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._commandRepeat = new System.Windows.Forms.NumericUpDown();
      this._label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._tunerCommandsButton = new MediaPortal.UserInterface.Controls.MPButton();
      this._internalCommandsButton = new MediaPortal.UserInterface.Controls.MPButton();
      this._enterCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this._digitCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this._outputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this._inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this._usbuirtVersion = new MediaPortal.UserInterface.Controls.MPLabel();
      this._label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._linkLabel1 = new System.Windows.Forms.LinkLabel();
      this._label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this._valueDLLVersion = new MediaPortal.UserInterface.Controls.MPLabel();
      this._valueAPIVersion = new MediaPortal.UserInterface.Controls.MPLabel();
      this._labelDLLVersion = new MediaPortal.UserInterface.Controls.MPLabel();
      this._labelAPIVersion = new MediaPortal.UserInterface.Controls.MPLabel();
      this._usbuirtConfigVersion = new MediaPortal.UserInterface.Controls.MPLabel();
      this._label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this._groupBox1.SuspendLayout();
      this._settingsPnl.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this._repeatDelay)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this._repeatWait)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this._interCommandDelay)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this._commandRepeat)).BeginInit();
      this._groupBox2.SuspendLayout();
      this._groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // _groupBox1
      // 
      this._groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._groupBox1.Controls.Add(this._settingsPnl);
      this._groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._groupBox1.Location = new System.Drawing.Point(6, 0);
      this._groupBox1.Name = "_groupBox1";
      this._groupBox1.Size = new System.Drawing.Size(462, 251);
      this._groupBox1.TabIndex = 0;
      this._groupBox1.TabStop = false;
      // 
      // _settingsPnl
      // 
      this._settingsPnl.Controls.Add(this._repeatDelay);
      this._settingsPnl.Controls.Add(this._repeatWait);
      this._settingsPnl.Controls.Add(this._label9);
      this._settingsPnl.Controls.Add(this._label8);
      this._settingsPnl.Controls.Add(this._testSendIrButton);
      this._settingsPnl.Controls.Add(this._testSendIrText);
      this._settingsPnl.Controls.Add(this._interCommandDelay);
      this._settingsPnl.Controls.Add(this._label6);
      this._settingsPnl.Controls.Add(this._commandRepeat);
      this._settingsPnl.Controls.Add(this._label5);
      this._settingsPnl.Controls.Add(this._tunerCommandsButton);
      this._settingsPnl.Controls.Add(this._internalCommandsButton);
      this._settingsPnl.Controls.Add(this._enterCheckBox);
      this._settingsPnl.Controls.Add(this._digitCheckBox);
      this._settingsPnl.Controls.Add(this._outputCheckBox);
      this._settingsPnl.Controls.Add(this._inputCheckBox);
      this._settingsPnl.Location = new System.Drawing.Point(8, 8);
      this._settingsPnl.Name = "_settingsPnl";
      this._settingsPnl.Size = new System.Drawing.Size(448, 235);
      this._settingsPnl.TabIndex = 0;
      // 
      // _repeatDelay
      // 
      this._repeatDelay.Location = new System.Drawing.Point(153, 66);
      this._repeatDelay.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this._repeatDelay.Name = "_repeatDelay";
      this._repeatDelay.Size = new System.Drawing.Size(48, 20);
      this._repeatDelay.TabIndex = 4;
      this._repeatDelay.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
      // 
      // _repeatWait
      // 
      this._repeatWait.Location = new System.Drawing.Point(153, 40);
      this._repeatWait.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
      this._repeatWait.Name = "_repeatWait";
      this._repeatWait.Size = new System.Drawing.Size(48, 20);
      this._repeatWait.TabIndex = 2;
      this._repeatWait.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
      // 
      // _label9
      // 
      this._label9.AutoSize = true;
      this._label9.Location = new System.Drawing.Point(24, 68);
      this._label9.Name = "_label9";
      this._label9.Size = new System.Drawing.Size(86, 13);
      this._label9.TabIndex = 3;
      this._label9.Text = "Repeat delay ms";
      this._label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // _label8
      // 
      this._label8.AutoSize = true;
      this._label8.Location = new System.Drawing.Point(24, 42);
      this._label8.Name = "_label8";
      this._label8.Size = new System.Drawing.Size(80, 13);
      this._label8.TabIndex = 1;
      this._label8.Text = "Repeat wait ms";
      this._label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // _testSendIrButton
      // 
      this._testSendIrButton.Location = new System.Drawing.Point(280, 183);
      this._testSendIrButton.Name = "_testSendIrButton";
      this._testSendIrButton.Size = new System.Drawing.Size(160, 23);
      this._testSendIrButton.TabIndex = 14;
      this._testSendIrButton.Text = "Test settop box control";
      this._testSendIrButton.UseVisualStyleBackColor = true;
      this._testSendIrButton.Click += new System.EventHandler(this.TestSendIrBtnClick);
      // 
      // _testSendIrText
      // 
      this._testSendIrText.AcceptsReturn = true;
      this._testSendIrText.BorderColor = System.Drawing.Color.Empty;
      this._testSendIrText.Location = new System.Drawing.Point(280, 206);
      this._testSendIrText.Name = "_testSendIrText";
      this._testSendIrText.Size = new System.Drawing.Size(160, 20);
      this._testSendIrText.TabIndex = 15;
      this._testSendIrText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TestSendIrTxtBoxKeyPress);
      // 
      // _interCommandDelay
      // 
      this._interCommandDelay.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this._interCommandDelay.Location = new System.Drawing.Point(153, 156);
      this._interCommandDelay.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this._interCommandDelay.Name = "_interCommandDelay";
      this._interCommandDelay.Size = new System.Drawing.Size(48, 20);
      this._interCommandDelay.TabIndex = 9;
      this._interCommandDelay.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      // 
      // _label6
      // 
      this._label6.AutoSize = true;
      this._label6.Location = new System.Drawing.Point(24, 158);
      this._label6.Name = "_label6";
      this._label6.Size = new System.Drawing.Size(121, 13);
      this._label6.TabIndex = 8;
      this._label6.Text = "Inter-command delay ms";
      this._label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // _commandRepeat
      // 
      this._commandRepeat.Location = new System.Drawing.Point(153, 132);
      this._commandRepeat.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this._commandRepeat.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this._commandRepeat.Name = "_commandRepeat";
      this._commandRepeat.Size = new System.Drawing.Size(48, 20);
      this._commandRepeat.TabIndex = 7;
      this._commandRepeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // _label5
      // 
      this._label5.AutoSize = true;
      this._label5.Location = new System.Drawing.Point(24, 134);
      this._label5.Name = "_label5";
      this._label5.Size = new System.Drawing.Size(117, 13);
      this._label5.TabIndex = 6;
      this._label5.Text = "Command repeat count";
      this._label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // _tunerCommandsButton
      // 
      this._tunerCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._tunerCommandsButton.Location = new System.Drawing.Point(280, 103);
      this._tunerCommandsButton.Name = "_tunerCommandsButton";
      this._tunerCommandsButton.Size = new System.Drawing.Size(160, 23);
      this._tunerCommandsButton.TabIndex = 13;
      this._tunerCommandsButton.Text = "Learn settop box commands";
      this._tunerCommandsButton.UseVisualStyleBackColor = true;
      this._tunerCommandsButton.Click += new System.EventHandler(this.TunerCommandsButtonClick);
      // 
      // _internalCommandsButton
      // 
      this._internalCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._internalCommandsButton.Location = new System.Drawing.Point(280, 16);
      this._internalCommandsButton.Name = "_internalCommandsButton";
      this._internalCommandsButton.Size = new System.Drawing.Size(160, 23);
      this._internalCommandsButton.TabIndex = 12;
      this._internalCommandsButton.Text = "Learn MediaPortal commands";
      this._internalCommandsButton.UseVisualStyleBackColor = true;
      this._internalCommandsButton.Click += new System.EventHandler(this.InternalCommandsButtonClick);
      // 
      // _enterCheckBox
      // 
      this._enterCheckBox.AutoSize = true;
      this._enterCheckBox.Enabled = false;
      this._enterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._enterCheckBox.Location = new System.Drawing.Point(24, 206);
      this._enterCheckBox.Name = "_enterCheckBox";
      this._enterCheckBox.Size = new System.Drawing.Size(189, 17);
      this._enterCheckBox.TabIndex = 11;
      this._enterCheckBox.Text = "Send \'Enter\' for changing channels";
      this._enterCheckBox.UseVisualStyleBackColor = true;
      this._enterCheckBox.CheckedChanged += new System.EventHandler(this.EnterCheckBoxCheckedChanged);
      // 
      // _digitCheckBox
      // 
      this._digitCheckBox.AutoSize = true;
      this._digitCheckBox.Enabled = false;
      this._digitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._digitCheckBox.Location = new System.Drawing.Point(24, 190);
      this._digitCheckBox.Name = "_digitCheckBox";
      this._digitCheckBox.Size = new System.Drawing.Size(180, 17);
      this._digitCheckBox.TabIndex = 10;
      this._digitCheckBox.Text = "Use 3 digits for channel selection";
      this._digitCheckBox.UseVisualStyleBackColor = true;
      this._digitCheckBox.CheckedChanged += new System.EventHandler(this.DigitCheckBoxCheckedChanged);
      // 
      // _outputCheckBox
      // 
      this._outputCheckBox.AutoSize = true;
      this._outputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._outputCheckBox.Location = new System.Drawing.Point(8, 106);
      this._outputCheckBox.Name = "_outputCheckBox";
      this._outputCheckBox.Size = new System.Drawing.Size(205, 17);
      this._outputCheckBox.TabIndex = 5;
      this._outputCheckBox.Text = "Let MediaPortal control your settopbox";
      this._outputCheckBox.UseVisualStyleBackColor = true;
      this._outputCheckBox.CheckedChanged += new System.EventHandler(this.OutputCheckBoxCheckedChanged);
      // 
      // _inputCheckBox
      // 
      this._inputCheckBox.AutoSize = true;
      this._inputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._inputCheckBox.Location = new System.Drawing.Point(8, 16);
      this._inputCheckBox.Name = "_inputCheckBox";
      this._inputCheckBox.Size = new System.Drawing.Size(207, 17);
      this._inputCheckBox.TabIndex = 0;
      this._inputCheckBox.Text = "Use your remote to control MediaPortal";
      this._inputCheckBox.UseVisualStyleBackColor = true;
      this._inputCheckBox.CheckedChanged += new System.EventHandler(this.InputCheckBoxCheckedChanged);
      // 
      // _usbuirtVersion
      // 
      this._usbuirtVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._usbuirtVersion.Location = new System.Drawing.Point(200, 15);
      this._usbuirtVersion.Name = "_usbuirtVersion";
      this._usbuirtVersion.Size = new System.Drawing.Size(246, 16);
      this._usbuirtVersion.TabIndex = 1;
      this._usbuirtVersion.Text = "Version";
      // 
      // _label1
      // 
      this._label1.Location = new System.Drawing.Point(16, 15);
      this._label1.Name = "_label1";
      this._label1.Size = new System.Drawing.Size(144, 16);
      this._label1.TabIndex = 0;
      this._label1.Text = "USBUIRT driver version:";
      // 
      // _label4
      // 
      this._label4.Location = new System.Drawing.Point(0, 0);
      this._label4.Name = "_label4";
      this._label4.Size = new System.Drawing.Size(100, 23);
      this._label4.TabIndex = 0;
      // 
      // _groupBox2
      // 
      this._groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._groupBox2.Controls.Add(this._label2);
      this._groupBox2.Controls.Add(this._label3);
      this._groupBox2.Controls.Add(this._linkLabel1);
      this._groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._groupBox2.Location = new System.Drawing.Point(6, 346);
      this._groupBox2.Name = "_groupBox2";
      this._groupBox2.Size = new System.Drawing.Size(462, 104);
      this._groupBox2.TabIndex = 2;
      this._groupBox2.TabStop = false;
      this._groupBox2.Text = "General Information";
      // 
      // _label3
      // 
      this._label3.Location = new System.Drawing.Point(16, 72);
      this._label3.Name = "_label3";
      this._label3.Size = new System.Drawing.Size(96, 16);
      this._label3.TabIndex = 1;
      this._label3.Text = "More information:";
      // 
      // _linkLabel1
      // 
      this._linkLabel1.Location = new System.Drawing.Point(112, 72);
      this._linkLabel1.Name = "_linkLabel1";
      this._linkLabel1.Size = new System.Drawing.Size(120, 16);
      this._linkLabel1.TabIndex = 2;
      this._linkLabel1.TabStop = true;
      this._linkLabel1.Text = "http://www.usbuirt.com";
      this._linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1LinkClicked);
      // 
      // _label2
      // 
      this._label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._label2.Location = new System.Drawing.Point(16, 24);
      this._label2.Name = "_label2";
      this._label2.Size = new System.Drawing.Size(438, 40);
      this._label2.TabIndex = 0;
      this._label2.Text = resources.GetString("_label2.Text");
      // 
      // _groupBox3
      // 
      this._groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._groupBox3.Controls.Add(this._valueDLLVersion);
      this._groupBox3.Controls.Add(this._valueAPIVersion);
      this._groupBox3.Controls.Add(this._labelDLLVersion);
      this._groupBox3.Controls.Add(this._labelAPIVersion);
      this._groupBox3.Controls.Add(this._usbuirtConfigVersion);
      this._groupBox3.Controls.Add(this._label7);
      this._groupBox3.Controls.Add(this._usbuirtVersion);
      this._groupBox3.Controls.Add(this._label1);
      this._groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this._groupBox3.Location = new System.Drawing.Point(6, 257);
      this._groupBox3.Name = "_groupBox3";
      this._groupBox3.Size = new System.Drawing.Size(462, 83);
      this._groupBox3.TabIndex = 1;
      this._groupBox3.TabStop = false;
      this._groupBox3.Text = "Status";
      // 
      // _valueDLLVersion
      // 
      this._valueDLLVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._valueDLLVersion.Location = new System.Drawing.Point(200, 47);
      this._valueDLLVersion.Name = "_valueDLLVersion";
      this._valueDLLVersion.Size = new System.Drawing.Size(246, 16);
      this._valueDLLVersion.TabIndex = 5;
      this._valueDLLVersion.Text = "Version";
      // 
      // _valueAPIVersion
      // 
      this._valueAPIVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._valueAPIVersion.Location = new System.Drawing.Point(200, 31);
      this._valueAPIVersion.Name = "_valueAPIVersion";
      this._valueAPIVersion.Size = new System.Drawing.Size(246, 16);
      this._valueAPIVersion.TabIndex = 3;
      this._valueAPIVersion.Text = "Version";
      // 
      // _labelDLLVersion
      // 
      this._labelDLLVersion.AutoSize = true;
      this._labelDLLVersion.Location = new System.Drawing.Point(16, 47);
      this._labelDLLVersion.Name = "_labelDLLVersion";
      this._labelDLLVersion.Size = new System.Drawing.Size(118, 13);
      this._labelDLLVersion.TabIndex = 4;
      this._labelDLLVersion.Text = "USBUIRT DLL version:";
      // 
      // _labelAPIVersion
      // 
      this._labelAPIVersion.AutoSize = true;
      this._labelAPIVersion.Location = new System.Drawing.Point(16, 31);
      this._labelAPIVersion.Name = "_labelAPIVersion";
      this._labelAPIVersion.Size = new System.Drawing.Size(115, 13);
      this._labelAPIVersion.TabIndex = 2;
      this._labelAPIVersion.Text = "USBUIRT API version:";
      // 
      // _usbuirtConfigVersion
      // 
      this._usbuirtConfigVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._usbuirtConfigVersion.Location = new System.Drawing.Point(200, 63);
      this._usbuirtConfigVersion.Name = "_usbuirtConfigVersion";
      this._usbuirtConfigVersion.Size = new System.Drawing.Size(246, 13);
      this._usbuirtConfigVersion.TabIndex = 7;
      this._usbuirtConfigVersion.Text = "Version";
      // 
      // _label7
      // 
      this._label7.AutoSize = true;
      this._label7.Location = new System.Drawing.Point(16, 63);
      this._label7.Name = "_label7";
      this._label7.Size = new System.Drawing.Size(126, 13);
      this._label7.TabIndex = 6;
      this._label7.Text = "USBUIRT plugin version:";
      // 
      // RemoteUSBUIRT
      // 
      this.Controls.Add(this._groupBox3);
      this.Controls.Add(this._groupBox2);
      this.Controls.Add(this._groupBox1);
      this.Name = "RemoteUSBUIRT";
      this.Size = new System.Drawing.Size(472, 458);
      this.Load += new System.EventHandler(this.USBUIRTLoad);
      this._groupBox1.ResumeLayout(false);
      this._settingsPnl.ResumeLayout(false);
      this._settingsPnl.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this._repeatDelay)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this._repeatWait)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this._interCommandDelay)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this._commandRepeat)).EndInit();
      this._groupBox2.ResumeLayout(false);
      this._groupBox3.ResumeLayout(false);
      this._groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private void ShowDriverOfflineMsg()
    {
      MessageBox.Show(this,
                      "USBUIRT Driver is not loaded.  Please ensure the USBUIRT is\t\r\nproperly connected and drivers are installed.",
                      "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void InternalCommandsButtonClick(object sender, EventArgs e)
    {
      if (!GetUsbUirtDriverStatusOk())
      {
        ShowDriverOfflineMsg();
        return;
      }

      var learnForm = new USBUIRTLearnForm(USBUIRTLearnForm.LearnType.MediaPortalCommands);
      learnForm.ShowDialog(this);
    }

    private void TunerCommandsButtonClick(object sender, EventArgs e)
    {
      if (!GetUsbUirtDriverStatusOk())
      {
        ShowDriverOfflineMsg();
        return;
      }

      var learnForm = new USBUIRTLearnForm(USBUIRTLearnForm.LearnType.SetTopBoxCommands);
      learnForm.ShowDialog(this);
    }

    private void OutputCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (_inputCheckBox.Checked || _outputCheckBox.Checked)
      {
        if (USBUIRT.Instance == null)
        {
          if (!InitUSBUIRTInstance())
          {
            _outputCheckBox.Checked = false;
            return;
          }
        }
        else if (!GetUsbUirtDriverStatusOk())
        {
          ShowDriverOfflineMsg();
          _outputCheckBox.Checked = false;
          return;
        }
        if (USBUIRT.Instance != null)
        {
          USBUIRT.Instance.TransmitEnabled = _outputCheckBox.Checked;
        }
      }
      bool setTopControl = _outputCheckBox.Checked;
      _digitCheckBox.Enabled = setTopControl;
      _enterCheckBox.Enabled = setTopControl;
      _tunerCommandsButton.Enabled = setTopControl;
      _commandRepeat.Enabled = setTopControl;
      _interCommandDelay.Enabled = setTopControl;
      _testSendIrButton.Enabled = setTopControl;
      _testSendIrText.Enabled = setTopControl;
    }

    private void InputCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (_inputCheckBox.Checked || _outputCheckBox.Checked)
      {
        if (USBUIRT.Instance == null)
        {
          if (!InitUSBUIRTInstance())
          {
            _inputCheckBox.Checked = false;
            return;
          }
        }
        else if (!GetUsbUirtDriverStatusOk())
        {
          ShowDriverOfflineMsg();
          _inputCheckBox.Checked = false;
          return;
        }
        if (USBUIRT.Instance != null)
        {
          USBUIRT.Instance.ReceiveEnabled = _inputCheckBox.Checked;
        }
      }
      bool useUSBUIRT = _inputCheckBox.Checked;
      _internalCommandsButton.Enabled = useUSBUIRT;
      _repeatWait.Enabled = useUSBUIRT;
      _repeatDelay.Enabled = useUSBUIRT;
    }

    private void OnRemoteCommand(object command) {}

    private bool GetUsbUirtDriverStatusOk()
    {
      bool driverStatusOk = USBUIRT.Instance.IsUsbUirtLoaded;

      // Check if the driver is loaded...
      driverStatusOk = driverStatusOk ? USBUIRT.Instance.IsUsbUirtConnected : USBUIRT.Instance.Reconnect();

      _usbuirtVersion.Text = USBUIRT.Instance.GetVersions();
      _valueAPIVersion.Text = USBUIRT.Instance.GetAPIVersions();
      _valueDLLVersion.Text = USBUIRT.Instance.GetDLLVersions();
      return driverStatusOk;
    }

    private void USBUIRTLoad(object sender, EventArgs e)
    {
      InputCheckBoxCheckedChanged(null, null);
      OutputCheckBoxCheckedChanged(null, null);

      _settingsPnl.BringToFront();
    }

    private void TestSendIrTxtBoxKeyPress(object sender, KeyPressEventArgs e)
    {
      if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == 8) // keys 0-9 and the BackSpace key
      {
        // do nothing...
      }

      else if (e.KeyChar == 13) // Enter key
      {
        TestSendIrBtnClick(null, null);
        e.Handled = true;
      }

      else
      {
        e.Handled = true;
      }
    }

    private void TestSendIrBtnClick(object sender, EventArgs e)
    {
      if (!GetUsbUirtDriverStatusOk())
      {
        ShowDriverOfflineMsg();
        return;
      }

      if (_testSendIrText.Text.Length == 0)
      {
        MessageBox.Show(this, "No channel number entered.  Nothing to send.\t", "USBUIRT", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        _testSendIrText.Focus();
      }

      else if (USBUIRT.Instance.TunerCodesLoaded)
      {
        USBUIRT.Instance.CommandRepeatCount = (int)_commandRepeat.Value;
        USBUIRT.Instance.InterCommandDelay = (int)_interCommandDelay.Value;
        USBUIRT.Instance.Is3Digit = _digitCheckBox.Checked;
        USBUIRT.Instance.NeedsEnter = _enterCheckBox.Checked;

        if (USBUIRT.Instance.Is3Digit && _testSendIrText.Text.Length > 3)
        {
          string msg =
            string.Format(
              "Channel number is longer than 3 digits.  Either disable '{0}'\t\r\nor enter a channel number less than 3 digits long.",
              _digitCheckBox.Text);
          MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        else
        {
          USBUIRT.Instance.ChangeTunerChannel(_testSendIrText.Text, true);
        }
      }

      else
      {
        const string msg = "No set top box codes loaded--nothing to send.\t\r\n Please ensure you have completed IR training.";
        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void EnableTestIrControls()
    {
      _commandRepeat.Enabled = USBUIRT.Instance.TunerCodesLoaded;
      _interCommandDelay.Enabled = USBUIRT.Instance.TunerCodesLoaded;
    }

    private void DigitCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      USBUIRT.Instance.Is3Digit = _digitCheckBox.Checked;
    }

    private void EnterCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      USBUIRT.Instance.NeedsEnter = _enterCheckBox.Checked;
    }

    private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(_linkLabel1.Text);
    }
  }
}