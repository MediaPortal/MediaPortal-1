#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Profile;
using MediaPortal.SerialIR;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class RemoteSerialUIR : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPCheckBox inputCheckBox;
    private MPButton internalCommandsButton;
    private MPGroupBox groupBox2;
    private MPLabel statusLabel;
    private MPLabel label1;
    private MPComboBox CommPortCombo;
    private MPCheckBox checkBoxInitUIRIrman;
    private MPLabel label2;
    private MPLabel label3;
    private MPLabel label4;
    private MPComboBox IRLengthCombo;
    private MPComboBox HandShakeCombo;
    private MPComboBox BaudRateCombo;
    private IContainer components = null;
    private MPLabel label5;
    private MPComboBox LearningTimeoutCombo;
    private MPLabel label6;
    private MPComboBox CommandDelayCombo;
    private MPLabel label7;
    private CheckedListBox ActionsCheckList;
    private MPButton buttonAllCodes;
    private MPButton buttonDefaultCodes;
    private MPButton buttonNoneCodes;
    private MPLabel label8;
    private MPComboBox ParityCombo;
    private MPCheckBox checkBoxDTR;
    private MPCheckBox checkBoxRTS;

    private bool initialize = false;

    public RemoteSerialUIR()
      : this("SerialUIR") {}

    public RemoteSerialUIR(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      this.ActionsCheckList.Items.AddRange(buttonNames);

      // 
      // Initialize the SerialUIR component
      //
      SerialUIR.Create(new SerialUIR.OnRemoteCommand(OnRemoteCommand));
      SerialUIR.Instance.StartListening += new StartListeningEventHandler(Instance_CodeReceived);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      SerialUIR.Instance.Close();
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        initialize = false;
        inputCheckBox.Checked = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
        BaudRateCombo.Text = xmlreader.GetValueAsString("SerialUIR", "baudrate", "9600");
        HandShakeCombo.Text = xmlreader.GetValueAsString("SerialUIR", "handshake", "None");
        ParityCombo.Text = xmlreader.GetValueAsString("SerialUIR", "parity", "None");
        IRLengthCombo.Text = xmlreader.GetValueAsString("SerialUIR", "irbytes", "6");
        checkBoxInitUIRIrman.Checked = xmlreader.GetValueAsString("SerialUIR", "uirirmaninit", "true") == "true";
        checkBoxRTS.Checked = xmlreader.GetValueAsString("SerialUIR", "rts", "true") == "true";
        checkBoxDTR.Checked = xmlreader.GetValueAsString("SerialUIR", "dtr", "true") == "true";
        CommandDelayCombo.Text = xmlreader.GetValueAsString("SerialUIR", "delay", "300");
        LearningTimeoutCombo.Text = xmlreader.GetValueAsString("SerialUIR", "timeout", "5");

        for (int i = 0; i < 16; i++)
        {
          ActionsCheckList.SetItemChecked(i,
                                          xmlreader.GetValueAsString("SerialUIR", "learn" + i.ToString(), "true") ==
                                          "true");
        }
        for (int i = 16; i < ActionsCheckList.Items.Count; i++)
        {
          ActionsCheckList.SetItemChecked(i,
                                          xmlreader.GetValueAsString("SerialUIR", "learn" + i.ToString(), "false") ==
                                          "true");
        }

        initialize = true;
        CommPortCombo.Text = xmlreader.GetValueAsString("SerialUIR", "commport", "COM1:");
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("SerialUIR", "internal", inputCheckBox.Checked ? "true" : "false");
        xmlwriter.SetValue("SerialUIR", "baudrate", BaudRateCombo.Text);
        xmlwriter.SetValue("SerialUIR", "handshake", HandShakeCombo.Text);
        xmlwriter.SetValue("SerialUIR", "parity", ParityCombo.Text);
        xmlwriter.SetValue("SerialUIR", "irbytes", IRLengthCombo.Text);
        xmlwriter.SetValue("SerialUIR", "uirirmaninit", checkBoxInitUIRIrman.Checked ? "true" : "false");
        xmlwriter.SetValue("SerialUIR", "rts", checkBoxRTS.Checked ? "true" : "false");
        xmlwriter.SetValue("SerialUIR", "dtr", checkBoxDTR.Checked ? "true" : "false");
        xmlwriter.SetValue("SerialUIR", "commport", CommPortCombo.Text);
        xmlwriter.SetValue("SerialUIR", "delay", CommandDelayCombo.Text);
        xmlwriter.SetValue("SerialUIR", "timeout", LearningTimeoutCombo.Text);
        for (int i = 0; i < ActionsCheckList.Items.Count; i++)
        {
          xmlwriter.SetValue("SerialUIR", "learn" + i.ToString(), ActionsCheckList.GetItemChecked(i) ? "true" : "false");
        }
      }
    }

    public void Close()
    {
      SerialUIR.Instance.Close();
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxRTS = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDTR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ParityCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.buttonNoneCodes = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDefaultCodes = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonAllCodes = new MediaPortal.UserInterface.Controls.MPButton();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.ActionsCheckList = new System.Windows.Forms.CheckedListBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.CommandDelayCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.LearningTimeoutCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.IRLengthCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.HandShakeCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.BaudRateCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxInitUIRIrman = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.CommPortCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.internalCommandsButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.statusLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxRTS);
      this.groupBox1.Controls.Add(this.checkBoxDTR);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.ParityCombo);
      this.groupBox1.Controls.Add(this.buttonNoneCodes);
      this.groupBox1.Controls.Add(this.buttonDefaultCodes);
      this.groupBox1.Controls.Add(this.buttonAllCodes);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.ActionsCheckList);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.CommandDelayCombo);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.LearningTimeoutCombo);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.IRLengthCombo);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.HandShakeCombo);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.BaudRateCombo);
      this.groupBox1.Controls.Add(this.checkBoxInitUIRIrman);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.CommPortCombo);
      this.groupBox1.Controls.Add(this.internalCommandsButton);
      this.groupBox1.Controls.Add(this.inputCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 336);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // checkBoxRTS
      // 
      this.checkBoxRTS.AutoSize = true;
      this.checkBoxRTS.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkBoxRTS.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxRTS.Location = new System.Drawing.Point(16, 200);
      this.checkBoxRTS.Name = "checkBoxRTS";
      this.checkBoxRTS.Size = new System.Drawing.Size(46, 17);
      this.checkBoxRTS.TabIndex = 12;
      this.checkBoxRTS.Text = "RTS";
      this.checkBoxRTS.UseVisualStyleBackColor = true;
      this.checkBoxRTS.CheckedChanged += new System.EventHandler(this.checkBoxRTS_CheckedChanged);
      // 
      // checkBoxDTR
      // 
      this.checkBoxDTR.AutoSize = true;
      this.checkBoxDTR.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkBoxDTR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDTR.Location = new System.Drawing.Point(16, 176);
      this.checkBoxDTR.Name = "checkBoxDTR";
      this.checkBoxDTR.Size = new System.Drawing.Size(47, 17);
      this.checkBoxDTR.TabIndex = 10;
      this.checkBoxDTR.Text = "DTR";
      this.checkBoxDTR.UseVisualStyleBackColor = true;
      this.checkBoxDTR.CheckedChanged += new System.EventHandler(this.checkBoxDTR_CheckedChanged);
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(120, 120);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 16);
      this.label8.TabIndex = 7;
      this.label8.Text = "Parity";
      // 
      // ParityCombo
      // 
      this.ParityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ParityCombo.Items.AddRange(new object[]
                                        {
                                          "None",
                                          "Odd",
                                          "Even",
                                          "Mark",
                                          "Space"
                                        });
      this.ParityCombo.Location = new System.Drawing.Point(120, 136);
      this.ParityCombo.Name = "ParityCombo";
      this.ParityCombo.Size = new System.Drawing.Size(88, 21);
      this.ParityCombo.TabIndex = 9;
      this.ParityCombo.SelectedIndexChanged += new System.EventHandler(this.ParityCombo_SelectedIndexChanged);
      // 
      // buttonNoneCodes
      // 
      this.buttonNoneCodes.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonNoneCodes.Location = new System.Drawing.Point(280, 22);
      this.buttonNoneCodes.Name = "buttonNoneCodes";
      this.buttonNoneCodes.Size = new System.Drawing.Size(56, 16);
      this.buttonNoneCodes.TabIndex = 20;
      this.buttonNoneCodes.Text = "Mini";
      this.buttonNoneCodes.UseVisualStyleBackColor = true;
      this.buttonNoneCodes.Click += new System.EventHandler(this.buttonNoneCodes_Click);
      // 
      // buttonDefaultCodes
      // 
      this.buttonDefaultCodes.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefaultCodes.Location = new System.Drawing.Point(344, 22);
      this.buttonDefaultCodes.Name = "buttonDefaultCodes";
      this.buttonDefaultCodes.Size = new System.Drawing.Size(56, 16);
      this.buttonDefaultCodes.TabIndex = 21;
      this.buttonDefaultCodes.Text = "Extended";
      this.buttonDefaultCodes.UseVisualStyleBackColor = true;
      this.buttonDefaultCodes.Click += new System.EventHandler(this.buttonDefaultCodes_Click);
      // 
      // buttonAllCodes
      // 
      this.buttonAllCodes.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAllCodes.Location = new System.Drawing.Point(408, 22);
      this.buttonAllCodes.Name = "buttonAllCodes";
      this.buttonAllCodes.Size = new System.Drawing.Size(56, 16);
      this.buttonAllCodes.TabIndex = 22;
      this.buttonAllCodes.Text = "All";
      this.buttonAllCodes.UseVisualStyleBackColor = true;
      this.buttonAllCodes.Click += new System.EventHandler(this.buttonAllCodes_Click);
      // 
      // label7
      // 
      this.label7.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label7.Location = new System.Drawing.Point(224, 24);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(32, 16);
      this.label7.TabIndex = 19;
      this.label7.Text = "Learn:";
      // 
      // ActionsCheckList
      // 
      this.ActionsCheckList.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.ActionsCheckList.CheckOnClick = true;
      this.ActionsCheckList.Location = new System.Drawing.Point(224, 40);
      this.ActionsCheckList.Name = "ActionsCheckList";
      this.ActionsCheckList.Size = new System.Drawing.Size(240, 289);
      this.ActionsCheckList.TabIndex = 23;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 222);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(88, 32);
      this.label6.TabIndex = 14;
      this.label6.Text = "Delay between commands (msec):";
      // 
      // CommandDelayCombo
      // 
      this.CommandDelayCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.CommandDelayCombo.Items.AddRange(new object[]
                                              {
                                                "150",
                                                "200",
                                                "250",
                                                "300",
                                                "250",
                                                "400",
                                                "450",
                                                "500"
                                              });
      this.CommandDelayCombo.Location = new System.Drawing.Point(120, 224);
      this.CommandDelayCombo.Name = "CommandDelayCombo";
      this.CommandDelayCombo.Size = new System.Drawing.Size(88, 21);
      this.CommandDelayCombo.TabIndex = 15;
      this.CommandDelayCombo.SelectedIndexChanged += new System.EventHandler(this.CommandDelayCombo_SelectedIndexChanged);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 262);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 32);
      this.label5.TabIndex = 16;
      this.label5.Text = "Learning timeout (sec):";
      // 
      // LearningTimeoutCombo
      // 
      this.LearningTimeoutCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.LearningTimeoutCombo.Items.AddRange(new object[]
                                                 {
                                                   "1",
                                                   "2",
                                                   "3",
                                                   "4",
                                                   "5",
                                                   "6",
                                                   "7",
                                                   "8",
                                                   "9",
                                                   "10"
                                                 });
      this.LearningTimeoutCombo.Location = new System.Drawing.Point(120, 264);
      this.LearningTimeoutCombo.Name = "LearningTimeoutCombo";
      this.LearningTimeoutCombo.Size = new System.Drawing.Size(88, 21);
      this.LearningTimeoutCombo.TabIndex = 17;
      this.LearningTimeoutCombo.SelectedIndexChanged +=
        new System.EventHandler(this.LearningTimeoutCombo_SelectedIndexChanged);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(120, 168);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(88, 16);
      this.label4.TabIndex = 11;
      this.label4.Text = "IR Code Length";
      // 
      // IRLengthCombo
      // 
      this.IRLengthCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.IRLengthCombo.Items.AddRange(new object[]
                                          {
                                            "1",
                                            "2",
                                            "3",
                                            "4",
                                            "5",
                                            "6",
                                            "7",
                                            "8",
                                            "9",
                                            "10",
                                            "11",
                                            "12",
                                            "13",
                                            "14",
                                            "15",
                                            "16",
                                            "17",
                                            "18",
                                            "19",
                                            "20",
                                            "21",
                                            "22",
                                            "23",
                                            "24",
                                            "25",
                                            "26",
                                            "27",
                                            "28",
                                            "29",
                                            "30",
                                            "31",
                                            "32"
                                          });
      this.IRLengthCombo.Location = new System.Drawing.Point(120, 184);
      this.IRLengthCombo.Name = "IRLengthCombo";
      this.IRLengthCombo.Size = new System.Drawing.Size(88, 21);
      this.IRLengthCombo.TabIndex = 13;
      this.IRLengthCombo.SelectedIndexChanged += new System.EventHandler(this.IRLengthCombo_SelectedIndexChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 120);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(64, 16);
      this.label3.TabIndex = 6;
      this.label3.Text = "HandShake";
      // 
      // HandShakeCombo
      // 
      this.HandShakeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.HandShakeCombo.Items.AddRange(new object[]
                                           {
                                             "None",
                                             "CtsRts",
                                             "DsrDtr",
                                             "XonXoff"
                                           });
      this.HandShakeCombo.Location = new System.Drawing.Point(16, 136);
      this.HandShakeCombo.Name = "HandShakeCombo";
      this.HandShakeCombo.Size = new System.Drawing.Size(88, 21);
      this.HandShakeCombo.TabIndex = 8;
      this.HandShakeCombo.SelectedIndexChanged += new System.EventHandler(this.HandShakeCombo_SelectedIndexChanged);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(120, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(56, 16);
      this.label2.TabIndex = 3;
      this.label2.Text = "Baud Rate";
      // 
      // BaudRateCombo
      // 
      this.BaudRateCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.BaudRateCombo.Items.AddRange(new object[]
                                          {
                                            "300",
                                            "600",
                                            "1200",
                                            "2400",
                                            "4800",
                                            "9600",
                                            "14400",
                                            "19200",
                                            "28800",
                                            "38400",
                                            "56000",
                                            "57600",
                                            "115200"
                                          });
      this.BaudRateCombo.Location = new System.Drawing.Point(120, 88);
      this.BaudRateCombo.Name = "BaudRateCombo";
      this.BaudRateCombo.Size = new System.Drawing.Size(88, 21);
      this.BaudRateCombo.TabIndex = 5;
      this.BaudRateCombo.SelectedIndexChanged += new System.EventHandler(this.BaudRateCombo_SelectedIndexChanged);
      // 
      // checkBoxInitUIRIrman
      // 
      this.checkBoxInitUIRIrman.AutoSize = true;
      this.checkBoxInitUIRIrman.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkBoxInitUIRIrman.Checked = true;
      this.checkBoxInitUIRIrman.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxInitUIRIrman.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxInitUIRIrman.Location = new System.Drawing.Point(16, 48);
      this.checkBoxInitUIRIrman.Name = "checkBoxInitUIRIrman";
      this.checkBoxInitUIRIrman.Size = new System.Drawing.Size(184, 17);
      this.checkBoxInitUIRIrman.TabIndex = 1;
      this.checkBoxInitUIRIrman.Text = "Initialize UIR/IRMan type receiver";
      this.checkBoxInitUIRIrman.UseVisualStyleBackColor = true;
      this.checkBoxInitUIRIrman.CheckedChanged += new System.EventHandler(this.checkBoxInitUIRIrman_CheckedChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 72);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 16);
      this.label1.TabIndex = 2;
      this.label1.Text = "Comm. Port";
      // 
      // CommPortCombo
      // 
      this.CommPortCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.CommPortCombo.Items.AddRange(new object[]
                                          {
                                            "COM1:",
                                            "COM2:",
                                            "COM3:",
                                            "COM4:",
                                            "COM5:",
                                            "COM6:",
                                            "COM7:",
                                            "COM8:"
                                          });
      this.CommPortCombo.Location = new System.Drawing.Point(16, 88);
      this.CommPortCombo.Name = "CommPortCombo";
      this.CommPortCombo.Size = new System.Drawing.Size(88, 21);
      this.CommPortCombo.TabIndex = 4;
      this.CommPortCombo.SelectedIndexChanged += new System.EventHandler(this.CommPortCombo_SelectedIndexChanged);
      // 
      // internalCommandsButton
      // 
      this.internalCommandsButton.Location = new System.Drawing.Point(16, 296);
      this.internalCommandsButton.Name = "internalCommandsButton";
      this.internalCommandsButton.Size = new System.Drawing.Size(192, 23);
      this.internalCommandsButton.TabIndex = 18;
      this.internalCommandsButton.Text = "Learn selected commands";
      this.internalCommandsButton.UseVisualStyleBackColor = true;
      this.internalCommandsButton.Click += new System.EventHandler(this.internalCommandsButton_Click);
      // 
      // inputCheckBox
      // 
      this.inputCheckBox.AutoSize = true;
      this.inputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.inputCheckBox.Location = new System.Drawing.Point(16, 24);
      this.inputCheckBox.Name = "inputCheckBox";
      this.inputCheckBox.Size = new System.Drawing.Size(198, 17);
      this.inputCheckBox.TabIndex = 0;
      this.inputCheckBox.Text = "Enable Serial UIR for remote controls";
      this.inputCheckBox.UseVisualStyleBackColor = true;
      this.inputCheckBox.CheckedChanged += new System.EventHandler(this.inputCheckBox_CheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.statusLabel);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(0, 344);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 64);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Device Status";
      // 
      // statusLabel
      // 
      this.statusLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.statusLabel.Location = new System.Drawing.Point(16, 24);
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(448, 32);
      this.statusLabel.TabIndex = 0;
      // 
      // SerialUIR
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "SerialUIR";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private string[] buttonNames = {
                                     "MOVE_LEFT"
                                     , "MOVE_RIGHT"
                                     , "MOVE_UP"
                                     , "MOVE_DOWN"
                                     , "PAGE_UP"
                                     , "PAGE_DOWN"
                                     , "SELECT_ITEM"
                                     , "PREVIOUS_MENU"
                                     , "SHOW_INFO"
                                     , "PAUSE"
                                     , "STOP"
                                     , "FORWARD"
                                     , "REWIND"
                                     , "SHOW_GUI"
                                     , "QUEUE_ITEM"
                                     , "EXIT"
                                     , "SHUTDOWN"
                                     , "ASPECT_RATIO"
                                     , "PLAY"
                                     , "EJECTCD"
                                     , "PREV_CHANNEL"
                                     , "NEXT_CHANNEL"
                                     , "RECORD"
                                     , "DVD_MENU"
                                     , "NEXT_CHAPTER"
                                     , "PREV_CHAPTER"
                                     , "VOLUME_DOWN"
                                     , "VOLUME_UP"
                                     , "AUDIO_NEXT_LANGUAGE"
                                     , "SHOW_SUBTITLES"
                                     , "NEXT_SUBTITLE"
                                     , "HIGHLIGHT_ITEM"
                                     , "PARENT_DIR"
                                     , "NEXT_ITEM"
                                     , "PREV_ITEM"
                                     , "STEP_FORWARD"
                                     , "STEP_BACK"
                                     , "BIG_STEP_FORWARD"
                                     , "BIG_STEP_BACK"
                                     , "SHOW_OSD"
                                     , "SHOW_CODEC"
                                     , "NEXT_PICTURE"
                                     , "PREV_PICTURE"
                                     , "ZOOM_OUT"
                                     , "ZOOM_IN"
                                     , "TOGGLE_SOURCE_DEST"
                                     , "SHOW_PLAYLIST"
                                     , "REMOVE_ITEM"
                                     , "SHOW_FULLSCREEN"
                                     , "ZOOM_LEVEL_NORMAL"
                                     , "ZOOM_LEVEL_1"
                                     , "ZOOM_LEVEL_2"
                                     , "ZOOM_LEVEL_3"
                                     , "ZOOM_LEVEL_4"
                                     , "ZOOM_LEVEL_5"
                                     , "ZOOM_LEVEL_6"
                                     , "ZOOM_LEVEL_7"
                                     , "ZOOM_LEVEL_8"
                                     , "ZOOM_LEVEL_9"
                                     , "CALIBRATE_SWAP_ARROWS"
                                     , "CALIBRATE_RESET"
                                     , "ANALOG_MOVE"
                                     , "ROTATE_PICTURE"
                                     , "CLOSE_DIALOG"
                                     , "SUBTITLE_DELAY_MIN"
                                     , "SUBTITLE_DELAY_PLUS"
                                     , "AUDIO_DELAY_MIN"
                                     , "AUDIO_DELAY_PLUS"
                                     , "CHANGE_RESOLUTION"
                                     , "REMOTE_0"
                                     , "REMOTE_1"
                                     , "REMOTE_2"
                                     , "REMOTE_3"
                                     , "REMOTE_4"
                                     , "REMOTE_5"
                                     , "REMOTE_6"
                                     , "REMOTE_7"
                                     , "REMOTE_8"
                                     , "REMOTE_9"
                                     , "OSD_SHOW_LEFT"
                                     , "OSD_SHOW_RIGHT"
                                     , "OSD_SHOW_UP"
                                     , "OSD_SHOW_DOWN"
                                     , "OSD_SHOW_SELECT"
                                     , "OSD_SHOW_VALUE_PLUS"
                                     , "OSD_SHOW_VALUE_MIN"
                                     , "SMALL_STEP_BACK"
                                     , "MUSIC_FORWARD"
                                     , "MUSIC_REWIND"
                                     , "MUSIC_PLAY"
                                     , "DELETE_ITEM"
                                     , "COPY_ITEM"
                                     , "MOVE_ITEM"
                                     , "SHOW_MPLAYER_OSD"
                                     , "OSD_HIDESUBMENU"
                                     , "TAKE_SCREENSHOT"
                                     , "INCREASE_TIMEBLOCK"
                                     , "DECREASE_TIMEBLOCK"
                                     , "DEFAULT_TIMEBLOCK"
                                     , "TVGUIDE_RESET"
                                     , "BACKGROUND_TOGGLE"
                                     , "TOGGLE_WINDOWED_FULSLCREEN"
                                     , "REBOOT"
                                   };


    private void internalCommandsButton_Click(object sender, EventArgs e)
    {
      internalCommandsButton.Enabled = false;
      object[] commands = {
                            Action.ActionType.ACTION_MOVE_LEFT
                            , Action.ActionType.ACTION_MOVE_RIGHT
                            , Action.ActionType.ACTION_MOVE_UP
                            , Action.ActionType.ACTION_MOVE_DOWN
                            , Action.ActionType.ACTION_PAGE_UP
                            , Action.ActionType.ACTION_PAGE_DOWN
                            , Action.ActionType.ACTION_SELECT_ITEM
                            , Action.ActionType.ACTION_PREVIOUS_MENU
                            , Action.ActionType.ACTION_SHOW_INFO
                            , Action.ActionType.ACTION_PAUSE
                            , Action.ActionType.ACTION_STOP
                            , Action.ActionType.ACTION_FORWARD
                            , Action.ActionType.ACTION_REWIND
                            , Action.ActionType.ACTION_SHOW_GUI
                            , Action.ActionType.ACTION_QUEUE_ITEM
                            , Action.ActionType.ACTION_EXIT
                            , Action.ActionType.ACTION_SHUTDOWN
                            , Action.ActionType.ACTION_ASPECT_RATIO
                            , Action.ActionType.ACTION_PLAY
                            , Action.ActionType.ACTION_EJECTCD
                            , Action.ActionType.ACTION_PREV_CHANNEL
                            , Action.ActionType.ACTION_NEXT_CHANNEL
                            , Action.ActionType.ACTION_RECORD
                            , Action.ActionType.ACTION_DVD_MENU
                            , Action.ActionType.ACTION_NEXT_CHAPTER
                            , Action.ActionType.ACTION_PREV_CHAPTER
                            , Action.ActionType.ACTION_VOLUME_DOWN
                            , Action.ActionType.ACTION_VOLUME_UP
                            , Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE
                            , Action.ActionType.ACTION_SHOW_SUBTITLES
                            , Action.ActionType.ACTION_NEXT_AUDIO
                            , Action.ActionType.ACTION_HIGHLIGHT_ITEM
                            , Action.ActionType.ACTION_PARENT_DIR
                            , Action.ActionType.ACTION_NEXT_ITEM
                            , Action.ActionType.ACTION_PREV_ITEM
                            , Action.ActionType.ACTION_STEP_FORWARD
                            , Action.ActionType.ACTION_STEP_BACK
                            , Action.ActionType.ACTION_BIG_STEP_FORWARD
                            , Action.ActionType.ACTION_BIG_STEP_BACK
                            , Action.ActionType.ACTION_SHOW_OSD
                            , Action.ActionType.ACTION_SHOW_CODEC
                            , Action.ActionType.ACTION_NEXT_PICTURE
                            , Action.ActionType.ACTION_PREV_PICTURE
                            , Action.ActionType.ACTION_ZOOM_OUT
                            , Action.ActionType.ACTION_ZOOM_IN
                            , Action.ActionType.ACTION_TOGGLE_SOURCE_DEST
                            , Action.ActionType.ACTION_SHOW_PLAYLIST
                            , Action.ActionType.ACTION_REMOVE_ITEM
                            , Action.ActionType.ACTION_SHOW_FULLSCREEN
                            , Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL
                            , Action.ActionType.ACTION_ZOOM_LEVEL_1
                            , Action.ActionType.ACTION_ZOOM_LEVEL_2
                            , Action.ActionType.ACTION_ZOOM_LEVEL_3
                            , Action.ActionType.ACTION_ZOOM_LEVEL_4
                            , Action.ActionType.ACTION_ZOOM_LEVEL_5
                            , Action.ActionType.ACTION_ZOOM_LEVEL_6
                            , Action.ActionType.ACTION_ZOOM_LEVEL_7
                            , Action.ActionType.ACTION_ZOOM_LEVEL_8
                            , Action.ActionType.ACTION_ZOOM_LEVEL_9
                            , Action.ActionType.ACTION_CALIBRATE_SWAP_ARROWS
                            , Action.ActionType.ACTION_CALIBRATE_RESET
                            , Action.ActionType.ACTION_ANALOG_MOVE
                            , Action.ActionType.ACTION_ROTATE_PICTURE
                            , Action.ActionType.ACTION_CLOSE_DIALOG
                            , Action.ActionType.ACTION_SUBTITLE_DELAY_MIN
                            , Action.ActionType.ACTION_SUBTITLE_DELAY_PLUS
                            , Action.ActionType.ACTION_AUDIO_DELAY_MIN
                            , Action.ActionType.ACTION_AUDIO_DELAY_PLUS
                            , Action.ActionType.ACTION_CHANGE_RESOLUTION
                            , Action.ActionType.REMOTE_0
                            , Action.ActionType.REMOTE_1
                            , Action.ActionType.REMOTE_2
                            , Action.ActionType.REMOTE_3
                            , Action.ActionType.REMOTE_4
                            , Action.ActionType.REMOTE_5
                            , Action.ActionType.REMOTE_6
                            , Action.ActionType.REMOTE_7
                            , Action.ActionType.REMOTE_8
                            , Action.ActionType.REMOTE_9
                            , Action.ActionType.ACTION_OSD_SHOW_LEFT
                            , Action.ActionType.ACTION_OSD_SHOW_RIGHT
                            , Action.ActionType.ACTION_OSD_SHOW_UP
                            , Action.ActionType.ACTION_OSD_SHOW_DOWN
                            , Action.ActionType.ACTION_OSD_SHOW_SELECT
                            , Action.ActionType.ACTION_OSD_SHOW_VALUE_PLUS
                            , Action.ActionType.ACTION_OSD_SHOW_VALUE_MIN
                            , Action.ActionType.ACTION_SMALL_STEP_BACK
                            , Action.ActionType.ACTION_MUSIC_FORWARD
                            , Action.ActionType.ACTION_MUSIC_REWIND
                            , Action.ActionType.ACTION_MUSIC_PLAY
                            , Action.ActionType.ACTION_DELETE_ITEM
                            , Action.ActionType.ACTION_COPY_ITEM
                            , Action.ActionType.ACTION_MOVE_ITEM
                            , Action.ActionType.ACTION_SHOW_MPLAYER_OSD
                            , Action.ActionType.ACTION_OSD_HIDESUBMENU
                            , Action.ActionType.ACTION_TAKE_SCREENSHOT
                            , Action.ActionType.ACTION_INCREASE_TIMEBLOCK
                            , Action.ActionType.ACTION_DECREASE_TIMEBLOCK
                            , Action.ActionType.ACTION_DEFAULT_TIMEBLOCK
                            , Action.ActionType.ACTION_TVGUIDE_RESET
                            , Action.ActionType.ACTION_BACKGROUND_TOGGLE
                            , Action.ActionType.ACTION_TOGGLE_WINDOWED_FULLSCREEN
                            , Action.ActionType.ACTION_REBOOT
                          };

      int count = 0;
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        if (ActionsCheckList.GetItemChecked(i))
        {
          count++;
        }
      }

      object[] learncommands = new object[count];
      string[] learnbuttons = new string[count];

      count = 0;
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        if (ActionsCheckList.GetItemChecked(i))
        {
          learncommands[count] = commands[i];
          learnbuttons[count] = buttonNames[i];
          count++;
        }
      }

      SerialUIR.Instance.StartListening -= new StartListeningEventHandler(Instance_CodeReceived);
      SerialUIR.Instance.StartLearning += new StartLearningEventHandler(Instance_StartLearning);

      SerialUIR.Instance.BulkLearn(learncommands, learnbuttons);
      SerialUIR.Instance.SaveInternalValues();

      SerialUIR.Instance.StartLearning -= new StartLearningEventHandler(Instance_StartLearning);
      SerialUIR.Instance.StartListening += new StartListeningEventHandler(Instance_CodeReceived);
      statusLabel.Text = "Learning finished !";
      internalCommandsButton.Enabled = true;
      Application.DoEvents();
    }

    private void Instance_CodeReceived(object sender, ListeningEventArgs e)
    {
      statusLabel.Text = e.Code;
      Application.DoEvents();
    }

    private void Instance_StartLearning(object sender, LearningEventArgs e)
    {
      statusLabel.Text = "Press and hold the '" + e.Button + "' button on your remote";
      Application.DoEvents();
    }

    private void OnRemoteCommand(object command)
    {
      Debug.WriteLine("Remote Command = " + command.ToString());
    }

    private void SetReOpenStatus(bool available)
    {
      if (available)
      {
        statusLabel.Text = "Port " + CommPortCombo.Text + " available";
        internalCommandsButton.Enabled = true;
      }
      else
      {
        statusLabel.Text = "Error : Port " + CommPortCombo.Text + " unavailable !";
        internalCommandsButton.Enabled = false;
      }
    }

    private void CommPortCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetReOpenStatus(SerialUIR.Instance.SetPort(CommPortCombo.Text));
    }

    private void BaudRateCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SetReOpenStatus(SerialUIR.Instance.SetBaudRate(int.Parse(BaudRateCombo.Text)));
      }
    }

    private void HandShakeCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SetReOpenStatus(SerialUIR.Instance.SetHandShake(HandShakeCombo.Text));
      }
    }

    private void IRLengthCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SetReOpenStatus(SerialUIR.Instance.SetIRBytes(int.Parse(IRLengthCombo.Text)));
      }
    }

    private void checkBoxInitUIRIrman_CheckedChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SetReOpenStatus(SerialUIR.Instance.SetUIRIRmanInit(checkBoxInitUIRIrman.Checked));
      }
    }

    private void CommandDelayCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      SerialUIR.Instance.CommandDelay = int.Parse(CommandDelayCombo.Text);
    }

    private void LearningTimeoutCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      SerialUIR.Instance.LearningTimeOut = 1000 * int.Parse(LearningTimeoutCombo.Text);
    }

    private void buttonAllCodes_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        ActionsCheckList.SetItemChecked(i, true);
      }
    }

    private void buttonDefaultCodes_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        ActionsCheckList.SetItemChecked(i, (i < 31) ? true : false);
      }
    }

    private void buttonNoneCodes_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        ActionsCheckList.SetItemChecked(i, (i < 16) ? true : false);
      }
    }

    private void ParityCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SetReOpenStatus(SerialUIR.Instance.SetParity(ParityCombo.Text));
      }
    }

    private void checkBoxDTR_CheckedChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SerialUIR.Instance.DTR = checkBoxRTS.Checked;
      }
    }

    private void checkBoxRTS_CheckedChanged(object sender, EventArgs e)
    {
      if (initialize)
      {
        SerialUIR.Instance.RTS = checkBoxRTS.Checked;
      }
    }

    private void inputCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      SerialUIR.Instance.InternalCommandsActive = inputCheckBox.Checked;
    }
  }
}