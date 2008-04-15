#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Summary description for HCWMappingForm.
  /// </summary>
  public class MappingForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    Array windowsList = Enum.GetValues(typeof(GUIWindow.Window));
    Array actionList = Enum.GetValues(typeof(Action.ActionType));
    string[] layerList = new string[] { "all", "1", "2" };
    string[] fullScreenList = new string[] { "Fullscreen", "No Fullscreen" };
    string[] playerList = new string[] { "TV", "DVD", "MEDIA" };
    string[] powerList = new string[] { "EXIT", "REBOOT", "SHUTDOWN", "STANDBY", "HIBERNATE" };
    string[] soundList = new string[] { "none", "back.wav", "click.wav", "cursor.wav" };
    string[] keyList = new string[] {"{BACKSPACE}", "{BREAK}", "{CAPSLOCK}", "{DELETE}", "{DOWN}", "{END}", "{ENTER}", "{ESC}",
                                              "{HELP}", "{HOME}", "{INSERT}", "{LEFT}", "{NUMLOCK}", "{PGDN}", "{PGUP}", "{PRTSC}",
                                              "{RIGHT}", "{SCROLLLOCK}", "{TAB}", "{UP}", "{F1}", "{F2}", "{F3}", "{F4}", "{F5}", "{F6}",
                                              "{F7}", "{F8}", "{F9}", "{F10}", "{F11}", "{F12}", "{F13}", "{F14}", "{F15}", "{F16}",
                                              "{ADD}", "{SUBTRACT}", "{MULTIPLY}", "{DIVIDE}"};
    string[] processList = new string[] { "CLOSE", "KILL" };

    string inputClassName;

    class Data
    {
      string type;
      string value;
      string parameter;

      public Data(object newType, object newParameter, object newValue)
      {
        if (newValue == null)
          newValue = "";
        if (newParameter == null)
          newParameter = "";
        type = (string)newType;
        value = newValue.ToString();
        parameter = (string)newParameter;
      }

      public string Type { get { return type; } }
      public string Value { get { return value; } }
      public string Parameter { get { return parameter; } }
    }

    private System.Windows.Forms.TreeView treeMapping;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonWindow;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonFullscreen;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPlaying;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonNoCondition;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxCondProperty;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxCmdProperty;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCondition;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAction;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonKey;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonActWindow;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonToggle;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPower;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxSound;
    private MediaPortal.UserInterface.Controls.MPLabel labelSound;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAction;
    //    private MediaPortal.UserInterface.Controls.MPGradientLabel headerLabel;
    private MediaPortal.UserInterface.Controls.MPButton applyButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    //    private MediaPortal.UserInterface.Controls.MPBeveledLine beveledLine1;
    private MediaPortal.UserInterface.Controls.MPLabel labelLayer;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxLayer;
    private MediaPortal.UserInterface.Controls.MPButton buttonUp;
    private MediaPortal.UserInterface.Controls.MPButton buttonDown;
    private MediaPortal.UserInterface.Controls.MPButton buttonNew;
    private MediaPortal.UserInterface.Controls.MPButton buttonRemove;
    private MediaPortal.UserInterface.Controls.MPButton buttonDefault;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxLayer;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonProcess;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public MappingForm(string name)
    {

      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      comboBoxSound.DataSource = soundList;
      comboBoxLayer.DataSource = layerList;
      inputClassName = name;
      LoadMapping(inputClassName + ".xml", false);
      //      headerLabel.Caption = inputClassName;
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.radioButtonWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonFullscreen = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPlaying = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonNoCondition = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.comboBoxCondProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxCmdProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxCondition = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonAction = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonKey = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonActWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonToggle = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPower = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBoxAction = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonProcess = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.labelSound = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxSound = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.applyButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxLayer = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxLayer = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelLayer = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonNew = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxCondition.SuspendLayout();
      this.groupBoxAction.SuspendLayout();
      this.groupBoxLayer.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeMapping
      // 
      this.treeMapping.AllowDrop = true;
      this.treeMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeMapping.FullRowSelect = true;
      this.treeMapping.HideSelection = false;
      this.treeMapping.Location = new System.Drawing.Point(11, 20);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.Size = new System.Drawing.Size(312, 330);
      this.treeMapping.TabIndex = 1;
      this.treeMapping.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMapping_AfterSelect);
      // 
      // radioButtonWindow
      // 
      this.radioButtonWindow.AutoSize = true;
      this.radioButtonWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonWindow.Location = new System.Drawing.Point(24, 24);
      this.radioButtonWindow.Name = "radioButtonWindow";
      this.radioButtonWindow.Size = new System.Drawing.Size(63, 17);
      this.radioButtonWindow.TabIndex = 9;
      this.radioButtonWindow.Text = "Window";
      this.radioButtonWindow.UseVisualStyleBackColor = true;
      this.radioButtonWindow.Click += new System.EventHandler(this.radioButtonWindow_Click);
      // 
      // radioButtonFullscreen
      // 
      this.radioButtonFullscreen.AutoSize = true;
      this.radioButtonFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonFullscreen.Location = new System.Drawing.Point(112, 24);
      this.radioButtonFullscreen.Name = "radioButtonFullscreen";
      this.radioButtonFullscreen.Size = new System.Drawing.Size(72, 17);
      this.radioButtonFullscreen.TabIndex = 10;
      this.radioButtonFullscreen.Text = "Fullscreen";
      this.radioButtonFullscreen.UseVisualStyleBackColor = true;
      this.radioButtonFullscreen.Click += new System.EventHandler(this.radioButtonFullscreen_Click);
      // 
      // radioButtonPlaying
      // 
      this.radioButtonPlaying.AutoSize = true;
      this.radioButtonPlaying.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPlaying.Location = new System.Drawing.Point(24, 48);
      this.radioButtonPlaying.Name = "radioButtonPlaying";
      this.radioButtonPlaying.Size = new System.Drawing.Size(58, 17);
      this.radioButtonPlaying.TabIndex = 11;
      this.radioButtonPlaying.Text = "Playing";
      this.radioButtonPlaying.UseVisualStyleBackColor = true;
      this.radioButtonPlaying.Click += new System.EventHandler(this.radioButtonPlaying_Click);
      // 
      // radioButtonNoCondition
      // 
      this.radioButtonNoCondition.AutoSize = true;
      this.radioButtonNoCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonNoCondition.Location = new System.Drawing.Point(112, 48);
      this.radioButtonNoCondition.Name = "radioButtonNoCondition";
      this.radioButtonNoCondition.Size = new System.Drawing.Size(85, 17);
      this.radioButtonNoCondition.TabIndex = 12;
      this.radioButtonNoCondition.Text = "No Condition";
      this.radioButtonNoCondition.UseVisualStyleBackColor = true;
      this.radioButtonNoCondition.Click += new System.EventHandler(this.radioButtonNoCondition_Click);
      // 
      // comboBoxCondProperty
      // 
      this.comboBoxCondProperty.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxCondProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCondProperty.ForeColor = System.Drawing.Color.Blue;
      this.comboBoxCondProperty.Location = new System.Drawing.Point(24, 72);
      this.comboBoxCondProperty.Name = "comboBoxCondProperty";
      this.comboBoxCondProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCondProperty.Sorted = true;
      this.comboBoxCondProperty.TabIndex = 13;
      this.comboBoxCondProperty.SelectedIndexChanged += new System.EventHandler(this.comboBoxCondProperty_SelectedIndexChanged);
      // 
      // comboBoxCmdProperty
      // 
      this.comboBoxCmdProperty.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCmdProperty.ForeColor = System.Drawing.Color.DarkGreen;
      this.comboBoxCmdProperty.Location = new System.Drawing.Point(24, 96);
      this.comboBoxCmdProperty.Name = "comboBoxCmdProperty";
      this.comboBoxCmdProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCmdProperty.Sorted = true;
      this.comboBoxCmdProperty.TabIndex = 14;
      this.comboBoxCmdProperty.SelectedIndexChanged += new System.EventHandler(this.comboBoxCmdProperty_SelectedIndexChanged);
      // 
      // groupBoxCondition
      // 
      this.groupBoxCondition.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.groupBoxCondition.Controls.Add(this.radioButtonWindow);
      this.groupBoxCondition.Controls.Add(this.radioButtonFullscreen);
      this.groupBoxCondition.Controls.Add(this.radioButtonPlaying);
      this.groupBoxCondition.Controls.Add(this.radioButtonNoCondition);
      this.groupBoxCondition.Controls.Add(this.comboBoxCondProperty);
      this.groupBoxCondition.Enabled = false;
      this.groupBoxCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCondition.Location = new System.Drawing.Point(347, 93);
      this.groupBoxCondition.Name = "groupBoxCondition";
      this.groupBoxCondition.Size = new System.Drawing.Size(224, 112);
      this.groupBoxCondition.TabIndex = 15;
      this.groupBoxCondition.TabStop = false;
      this.groupBoxCondition.Text = "Condition";
      // 
      // radioButtonAction
      // 
      this.radioButtonAction.AutoSize = true;
      this.radioButtonAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAction.Location = new System.Drawing.Point(24, 24);
      this.radioButtonAction.Name = "radioButtonAction";
      this.radioButtonAction.Size = new System.Drawing.Size(54, 17);
      this.radioButtonAction.TabIndex = 14;
      this.radioButtonAction.Text = "Action";
      this.radioButtonAction.UseVisualStyleBackColor = true;
      this.radioButtonAction.Click += new System.EventHandler(this.radioButtonAction_Click);
      // 
      // radioButtonKey
      // 
      this.radioButtonKey.AutoSize = true;
      this.radioButtonKey.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonKey.Location = new System.Drawing.Point(112, 24);
      this.radioButtonKey.Name = "radioButtonKey";
      this.radioButtonKey.Size = new System.Drawing.Size(71, 17);
      this.radioButtonKey.TabIndex = 16;
      this.radioButtonKey.Text = "Keystroke";
      this.radioButtonKey.UseVisualStyleBackColor = true;
      this.radioButtonKey.Click += new System.EventHandler(this.radioButtonKey_Click);
      // 
      // radioButtonActWindow
      // 
      this.radioButtonActWindow.AutoSize = true;
      this.radioButtonActWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonActWindow.Location = new System.Drawing.Point(24, 48);
      this.radioButtonActWindow.Name = "radioButtonActWindow";
      this.radioButtonActWindow.Size = new System.Drawing.Size(63, 17);
      this.radioButtonActWindow.TabIndex = 14;
      this.radioButtonActWindow.Text = "Window";
      this.radioButtonActWindow.UseVisualStyleBackColor = true;
      this.radioButtonActWindow.Click += new System.EventHandler(this.radioButtonActWindow_Click);
      // 
      // radioButtonToggle
      // 
      this.radioButtonToggle.AutoSize = true;
      this.radioButtonToggle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonToggle.Location = new System.Drawing.Point(112, 48);
      this.radioButtonToggle.Name = "radioButtonToggle";
      this.radioButtonToggle.Size = new System.Drawing.Size(86, 17);
      this.radioButtonToggle.TabIndex = 17;
      this.radioButtonToggle.Text = "Toggle Layer";
      this.radioButtonToggle.UseVisualStyleBackColor = true;
      this.radioButtonToggle.Click += new System.EventHandler(this.radioButtonToggle_Click);
      // 
      // radioButtonPower
      // 
      this.radioButtonPower.AutoSize = true;
      this.radioButtonPower.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPower.Location = new System.Drawing.Point(24, 72);
      this.radioButtonPower.Name = "radioButtonPower";
      this.radioButtonPower.Size = new System.Drawing.Size(80, 17);
      this.radioButtonPower.TabIndex = 18;
      this.radioButtonPower.Text = "Powerdown";
      this.radioButtonPower.UseVisualStyleBackColor = true;
      this.radioButtonPower.Click += new System.EventHandler(this.radioButtonPower_Click);
      // 
      // groupBoxAction
      // 
      this.groupBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAction.Controls.Add(this.radioButtonProcess);
      this.groupBoxAction.Controls.Add(this.labelSound);
      this.groupBoxAction.Controls.Add(this.comboBoxSound);
      this.groupBoxAction.Controls.Add(this.radioButtonAction);
      this.groupBoxAction.Controls.Add(this.radioButtonKey);
      this.groupBoxAction.Controls.Add(this.radioButtonActWindow);
      this.groupBoxAction.Controls.Add(this.radioButtonToggle);
      this.groupBoxAction.Controls.Add(this.radioButtonPower);
      this.groupBoxAction.Controls.Add(this.comboBoxCmdProperty);
      this.groupBoxAction.Enabled = false;
      this.groupBoxAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAction.Location = new System.Drawing.Point(347, 211);
      this.groupBoxAction.Name = "groupBoxAction";
      this.groupBoxAction.Size = new System.Drawing.Size(224, 168);
      this.groupBoxAction.TabIndex = 16;
      this.groupBoxAction.TabStop = false;
      this.groupBoxAction.Text = "Action";
      // 
      // radioButtonProcess
      // 
      this.radioButtonProcess.AutoSize = true;
      this.radioButtonProcess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonProcess.Location = new System.Drawing.Point(112, 72);
      this.radioButtonProcess.Name = "radioButtonProcess";
      this.radioButtonProcess.Size = new System.Drawing.Size(62, 17);
      this.radioButtonProcess.TabIndex = 21;
      this.radioButtonProcess.Text = "Process";
      this.radioButtonProcess.UseVisualStyleBackColor = true;
      this.radioButtonProcess.Click += new System.EventHandler(this.radioButtonProcess_Click);
      // 
      // labelSound
      // 
      this.labelSound.Location = new System.Drawing.Point(24, 131);
      this.labelSound.Name = "labelSound";
      this.labelSound.Size = new System.Drawing.Size(40, 16);
      this.labelSound.TabIndex = 20;
      this.labelSound.Text = "Sound:";
      // 
      // comboBoxSound
      // 
      this.comboBoxSound.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSound.ForeColor = System.Drawing.Color.DarkRed;
      this.comboBoxSound.Location = new System.Drawing.Point(72, 128);
      this.comboBoxSound.Name = "comboBoxSound";
      this.comboBoxSound.Size = new System.Drawing.Size(128, 21);
      this.comboBoxSound.TabIndex = 19;
      this.comboBoxSound.SelectedIndexChanged += new System.EventHandler(this.comboBoxSound_SelectedIndexChanged);
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(343, 395);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 20;
      this.applyButton.Text = "&Apply";
      this.applyButton.UseVisualStyleBackColor = true;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(424, 395);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 19;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(505, 395);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 18;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // groupBoxLayer
      // 
      this.groupBoxLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxLayer.Controls.Add(this.comboBoxLayer);
      this.groupBoxLayer.Controls.Add(this.labelLayer);
      this.groupBoxLayer.Enabled = false;
      this.groupBoxLayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxLayer.Location = new System.Drawing.Point(347, 12);
      this.groupBoxLayer.Name = "groupBoxLayer";
      this.groupBoxLayer.Size = new System.Drawing.Size(224, 64);
      this.groupBoxLayer.TabIndex = 22;
      this.groupBoxLayer.TabStop = false;
      this.groupBoxLayer.Text = "Layer";
      // 
      // comboBoxLayer
      // 
      this.comboBoxLayer.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxLayer.ForeColor = System.Drawing.Color.DimGray;
      this.comboBoxLayer.Location = new System.Drawing.Point(80, 24);
      this.comboBoxLayer.Name = "comboBoxLayer";
      this.comboBoxLayer.Size = new System.Drawing.Size(121, 21);
      this.comboBoxLayer.TabIndex = 25;
      this.comboBoxLayer.SelectedIndexChanged += new System.EventHandler(this.comboBoxLayer_SelectedIndexChanged);
      // 
      // labelLayer
      // 
      this.labelLayer.Location = new System.Drawing.Point(24, 27);
      this.labelLayer.Name = "labelLayer";
      this.labelLayer.Size = new System.Drawing.Size(40, 16);
      this.labelLayer.TabIndex = 16;
      this.labelLayer.Text = "Layer:";
      // 
      // buttonUp
      // 
      this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUp.Location = new System.Drawing.Point(7, 356);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(56, 23);
      this.buttonUp.TabIndex = 23;
      this.buttonUp.Text = "Up";
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(75, 356);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(56, 23);
      this.buttonDown.TabIndex = 24;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonNew
      // 
      this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNew.Location = new System.Drawing.Point(139, 356);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(56, 23);
      this.buttonNew.TabIndex = 26;
      this.buttonNew.Text = "New";
      this.buttonNew.UseVisualStyleBackColor = true;
      this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonRemove.Location = new System.Drawing.Point(203, 356);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(56, 23);
      this.buttonRemove.TabIndex = 27;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDefault.Location = new System.Drawing.Point(267, 356);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(56, 23);
      this.buttonDefault.TabIndex = 28;
      this.buttonDefault.Text = "Default";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.buttonReset_Click);
      // 
      // MappingForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(592, 430);
      this.Controls.Add(this.treeMapping);
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.buttonRemove);
      this.Controls.Add(this.buttonNew);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUp);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.groupBoxAction);
      this.Controls.Add(this.groupBoxCondition);
      this.Controls.Add(this.groupBoxLayer);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "MappingForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MAME Devices - Setup";
      this.groupBoxCondition.ResumeLayout(false);
      this.groupBoxCondition.PerformLayout();
      this.groupBoxAction.ResumeLayout(false);
      this.groupBoxAction.PerformLayout();
      this.groupBoxLayer.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion


    void LoadMapping(string xmlFile, bool defaults)
    {
      groupBoxLayer.Enabled = false;
      groupBoxCondition.Enabled = false;
      groupBoxAction.Enabled = false;
      treeMapping.Nodes.Clear();
      XmlDocument doc = new XmlDocument();
      string path = Config.GetFile(Config.Dir.Base, @"InputDeviceMappings\defaults", xmlFile);
      if (!defaults && File.Exists(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile)))
        path = Config.GetFile(Config.Dir.CustomInputDevice, xmlFile);
      doc.Load(path);
      XmlNodeList listRemotes = doc.DocumentElement.SelectNodes("/mappings/remote");

      foreach (XmlNode nodeRemote in listRemotes)
      {
        TreeNode remoteNode = new TreeNode(nodeRemote.Attributes["family"].Value);
        remoteNode.Tag = new Data("REMOTE", null, nodeRemote.Attributes["family"].Value);
        XmlNodeList listButtons = nodeRemote.SelectNodes("button");
        foreach (XmlNode nodeButton in listButtons)
        {
          TreeNode buttonNode = new TreeNode((string)nodeButton.Attributes["name"].Value);
          buttonNode.Tag = new Data("BUTTON", nodeButton.Attributes["name"].Value, nodeButton.Attributes["code"].Value);
          remoteNode.Nodes.Add(buttonNode);

          TreeNode layer1Node = new TreeNode("Layer 1");
          TreeNode layer2Node = new TreeNode("Layer 2");
          TreeNode layerAllNode = new TreeNode("All Layers");
          layer1Node.Tag = new Data("LAYER", null, "1");
          layer2Node.Tag = new Data("LAYER", null, "2");
          layerAllNode.Tag = new Data("LAYER", null, "0");
          layer1Node.ForeColor = Color.DimGray;
          layer2Node.ForeColor = Color.DimGray;
          layerAllNode.ForeColor = Color.DimGray;

          XmlNodeList listActions = nodeButton.SelectNodes("action");

          foreach (XmlNode nodeAction in listActions)
          {
            string conditionString = "";
            string commandString = "";

            string condition = nodeAction.Attributes["condition"].Value.ToUpper();
            string conProperty = nodeAction.Attributes["conproperty"].Value.ToUpper();
            string command = nodeAction.Attributes["command"].Value.ToUpper();
            string cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpper();
            string sound = nodeAction.Attributes["sound"].Value;
            int layer = Convert.ToInt32(nodeAction.Attributes["layer"].Value);

            #region Conditions

            switch (condition)
            {
              case "WINDOW":
                conditionString = Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(conProperty));
                break;
              case "FULLSCREEN":
                if (conProperty == "TRUE")
                  conditionString = "Fullscreen";
                else
                  conditionString = "No Fullscreen";
                break;
              case "PLAYER":
                switch (conProperty)
                {
                  case "TV":
                    conditionString = "TV Playing";
                    break;
                  case "DVD":
                    conditionString = "DVD Playing";
                    break;
                  case "MEDIA":
                    conditionString = "Media Playing";
                    break;
                }
                break;
              case "*":
                conditionString = "No Condition";
                break;
            }

            #endregion
            #region Commands

            switch (command)
            {
              case "ACTION":
                commandString = Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(cmdProperty));
                break;
              case "KEY":
                commandString = "Key \"" + cmdProperty + "\"";
                break;
              case "WINDOW":
                commandString = Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(cmdProperty));
                break;
              case "TOGGLE":
                commandString = "Toggle Layer";
                break;
              case "POWER":
                switch (cmdProperty)
                {
                  case "EXIT":
                    commandString = "Exit MediaPortal";
                    break;
                  case "REBOOT":
                    commandString = "Reboot Windows";
                    break;
                  case "SHUTDOWN":
                    commandString = "Shutdown Windows";
                    break;
                  case "STANDBY":
                    commandString = "Suspend Windows (Standby)";
                    break;
                  case "HIBERNATE":
                    commandString = "Hibernate Windows";
                    break;
                }
                break;
              case "PROCESS":
                switch (cmdProperty)
                {
                  case "CLOSE":
                    commandString = "Close Process";
                    break;
                  case "KILL":
                    commandString = "Kill Process";
                    break;
                }
                break;
            }

            #endregion

            TreeNode conditionNode = new TreeNode(conditionString);
            TreeNode commandNode = new TreeNode(commandString);
            conditionNode.Tag = new Data("CONDITION", nodeAction.Attributes["condition"].Value, nodeAction.Attributes["conproperty"].Value);
            commandNode.Tag = new Data("COMMAND", nodeAction.Attributes["command"].Value, nodeAction.Attributes["cmdproperty"].Value);
            commandNode.ForeColor = Color.DarkGreen;
            conditionNode.ForeColor = Color.Blue;
            conditionNode.Nodes.Add(commandNode);

            TreeNode soundNode = new TreeNode(sound);
            soundNode.Tag = new Data("SOUND", null, nodeAction.Attributes["sound"].Value);
            if (soundNode.Text == "")
              soundNode.Text = "No Sound";
            soundNode.ForeColor = Color.DarkRed;
            conditionNode.Nodes.Add(soundNode);

            if (layer == 1) layer1Node.Nodes.Add(conditionNode);
            if (layer == 2) layer2Node.Nodes.Add(conditionNode);
            if (layer == 0) layerAllNode.Nodes.Add(conditionNode);
          }
          if (layer1Node.Nodes.Count > 0) buttonNode.Nodes.Add(layer1Node);
          if (layer2Node.Nodes.Count > 0) buttonNode.Nodes.Add(layer2Node);
          if (layerAllNode.Nodes.Count > 0) buttonNode.Nodes.Add(layerAllNode);
        }
        treeMapping.Nodes.Add(remoteNode);
        if (listRemotes.Count == 1)
          remoteNode.Expand();
      }
    }

    bool SaveMapping(string xmlFile)
    {
      try
      {
        DirectoryInfo dir = Directory.CreateDirectory(Config.GetFolder(Config.Dir.CustomInputDevice));
      }
      catch
      {
        Log.Info("MAP: Error accessing directory \"InputDeviceMappings\\custom\"");
      }
      DialogResult result = MessageBox.Show(this, "Information:\n\nThere is no plausibility check implemented in this version.\nMake sure your mappings are correct.\n\nThis is just an information, settings will be saved.",
        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      try
      {
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile), System.Text.Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 1;
        writer.IndentChar = (char)9;
        writer.WriteStartDocument(true);
        writer.WriteStartElement("mappings"); // <mappings>
        writer.WriteAttributeString("version", "2");
        foreach (TreeNode remoteNode in treeMapping.Nodes)
        {
          writer.WriteStartElement("remote"); // <remote>
          writer.WriteAttributeString("family", ((Data)remoteNode.Tag).Value);
          foreach (TreeNode buttonNode in remoteNode.Nodes)
          {
            writer.WriteStartElement("button"); // <button>
            writer.WriteAttributeString("name", ((Data)buttonNode.Tag).Parameter);
            writer.WriteAttributeString("code", ((Data)buttonNode.Tag).Value);

            foreach (TreeNode layerNode in buttonNode.Nodes)
            {
              foreach (TreeNode conditionNode in layerNode.Nodes)
              {
                string layer;
                string condition;
                string conProperty;
                string command = "";
                string cmdProperty = "";
                string sound = "";
                foreach (TreeNode commandNode in conditionNode.Nodes)
                {
                  if (((Data)commandNode.Tag).Type == "COMMAND")
                  {
                    command = ((Data)commandNode.Tag).Parameter;
                    cmdProperty = ((Data)commandNode.Tag).Value;
                  }
                  else
                  {
                    sound = ((Data)commandNode.Tag).Value;
                  }
                }
                condition = ((Data)conditionNode.Tag).Parameter;
                conProperty = ((Data)conditionNode.Tag).Value;
                layer = ((Data)layerNode.Tag).Value;
                writer.WriteStartElement("action"); // <action>
                writer.WriteAttributeString("layer", layer);
                writer.WriteAttributeString("condition", condition);
                writer.WriteAttributeString("conproperty", conProperty);
                writer.WriteAttributeString("command", command);
                writer.WriteAttributeString("cmdproperty", cmdProperty);
                writer.WriteAttributeString("sound", sound);
                writer.WriteEndElement(); // </action>
              }
            }
            writer.WriteEndElement(); // </button>
          }
          writer.WriteEndElement(); // </remote>
        }
        writer.WriteEndElement(); // </mapping>
        writer.WriteEndDocument();
        writer.Close();
        return true;
      }
      catch
      {
        Log.Info("MAP: Error saving mapping to XML file");
        return false;
      }
    }

    TreeNode getNode(string type)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data)node.Tag;
      if (data.Type == type)
        return node;
      #region Find Node

      switch (type)
      {
        case "COMMAND":
          if (data.Type == "SOUND")
          {
            node = node.Parent;
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data)subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          else if (data.Type == "CONDITION")
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data)subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          break;
        case "SOUND":
          if (data.Type == "COMMAND")
          {
            node = node.Parent;
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data)subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          else if (data.Type == "CONDITION")
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data)subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          break;
        case "CONDITION":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND"))
            return node.Parent;
          break;
        case "LAYER":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND"))
            return node.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent;
          break;
        case "BUTTON":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND"))
            return node.Parent.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent.Parent;
          else if (data.Type == "LAYER")
            return node.Parent;
          break;
        case "REMOTE":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND"))
            return node.Parent.Parent.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent.Parent.Parent;
          else if (data.Type == "LAYER")
            return node.Parent.Parent;
          else if (data.Type == "BUTTON")
            return node.Parent;
          break;
      }

      #endregion
      return node;
    }

    private void treeMapping_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
    {
      if (e.Action == TreeViewAction.Unknown)
        return;

      TreeNode node = e.Node;
      Data data = (Data)node.Tag;
      switch (data.Type)
      {
        case "REMOTE":
        case "BUTTON":
          groupBoxLayer.Enabled = false;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          comboBoxLayer.Text = "All Layers";
          comboBoxCondProperty.Text = "none";
          comboBoxCmdProperty.Text = "none";
          comboBoxSound.Text = "none";
          return;
        case "LAYER":
          groupBoxLayer.Enabled = true;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          comboBoxCondProperty.Text = "none";
          comboBoxCmdProperty.Text = "none";
          comboBoxSound.Text = "none";
          comboBoxLayer.SelectedIndex = Convert.ToInt32(data.Value);
          return;
        case "COMMAND":
        case "SOUND":
        case "CONDITION":
          {
            groupBoxCondition.Enabled = true;
            groupBoxAction.Enabled = true;
            groupBoxLayer.Enabled = true;
            if ((data.Type == "COMMAND") || (data.Type == "SOUND"))
            {
              comboBoxLayer.SelectedIndex = Convert.ToInt32(((Data)node.Parent.Parent.Tag).Value);
              node = node.Parent;
              data = (Data)node.Tag;
            }
            else
              comboBoxLayer.SelectedIndex = Convert.ToInt32(((Data)node.Parent.Tag).Value);

            switch (data.Parameter)
            {
              case "WINDOW":
                radioButtonWindow.Checked = true;
                comboBoxCondProperty.Enabled = true;
                UpdateCombo(ref comboBoxCondProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(data.Value)));
                break;
              case "FULLSCREEN":
                radioButtonFullscreen.Checked = true;
                comboBoxCondProperty.Enabled = true;
                if (Convert.ToBoolean(data.Value))
                  UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
                else
                  UpdateCombo(ref comboBoxCondProperty, fullScreenList, "No Fullscreen");
                break;
              case "PLAYER":
                radioButtonPlaying.Checked = true;
                comboBoxCondProperty.Enabled = true;
                UpdateCombo(ref comboBoxCondProperty, playerList, data.Value);
                break;
              case "*":
                comboBoxCondProperty.Text = "none";
                radioButtonNoCondition.Checked = true;
                comboBoxCondProperty.Enabled = false;
                comboBoxCondProperty.Items.Clear();
                break;
            }
            foreach (TreeNode typeNode in node.Nodes)
            {
              data = (Data)typeNode.Tag;
              switch (data.Type)
              {
                case "SOUND":
                  if (data.Value != "")
                    comboBoxSound.SelectedItem = data.Value;
                  else
                    comboBoxSound.SelectedItem = "none";
                  break;
                case "COMMAND":
                  switch (data.Parameter)
                  {
                    case "ACTION":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonAction.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, actionList, Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(data.Value)));
                      break;
                    case "KEY":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
                      radioButtonKey.Checked = true;
                      comboBoxSound.Enabled = false;
                      comboBoxSound.Text = "none";
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, keyList, data.Value);
                      break;
                    case "WINDOW":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonActWindow.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(data.Value)));
                      break;
                    case "TOGGLE":
                      radioButtonToggle.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = false;
                      comboBoxCmdProperty.Items.Clear();
                      comboBoxCmdProperty.Text = "";
                      break;
                    case "POWER":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonPower.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, powerList, data.Value);
                      break;
                    case "PROCESS":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonProcess.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, processList, data.Value);
                      break;
                  }
                  break;
              }
            }
          }
          break;
      }
    }

    void UpdateCombo(ref MediaPortal.UserInterface.Controls.MPComboBox comboBox, Array list, string hilight)
    {
      comboBox.Items.Clear();
      foreach (object item in list)
        comboBox.Items.Add(item.ToString());
      comboBox.Text = hilight;
      comboBox.SelectedItem = hilight;
      comboBox.Enabled = true;
    }

    private void radioButtonWindow_Click(object sender, System.EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      Data data = new Data("CONDITION", "WINDOW", "-1");
      node.Tag = data;
      UpdateCombo(ref comboBoxCondProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(data.Value)));
    }

    private void radioButtonFullscreen_Click(object sender, System.EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      Data data = new Data("CONDITION", "FULLSCREEN", "true");
      node.Tag = data;
      UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
    }

    private void radioButtonPlaying_Click(object sender, System.EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      Data data = new Data("CONDITION", "PLAYER", "TV");
      node.Tag = data;
      UpdateCombo(ref comboBoxCondProperty, playerList, "TV");
    }

    private void radioButtonNoCondition_Click(object sender, System.EventArgs e)
    {
      comboBoxCondProperty.Enabled = false;
      comboBoxCondProperty.Items.Clear();
      comboBoxCondProperty.Text = "none";
      TreeNode node = getNode("CONDITION");
      Data data = new Data("CONDITION", "*", null);
      node.Tag = data;
      node.Text = "No Condition";
    }

    private void radioButtonAction_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "ACTION", "0");
      node.Tag = data;
      UpdateCombo(ref comboBoxCmdProperty, actionList, Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(data.Value)));
    }

    private void radioButtonActWindow_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "WINDOW", "-1");
      node.Tag = data;
      UpdateCombo(ref comboBoxCmdProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(data.Value)));
    }

    private void radioButtonKey_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
      comboBoxSound.Enabled = false;
      comboBoxSound.Text = "none";
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "KEY", "{ENTER}");
      node.Tag = data;
      UpdateCombo(ref comboBoxCmdProperty, keyList, data.Value);
      node = getNode("SOUND");
      node.Tag = new Data("SOUND", null, "");
      node.Text = "No Sound";
    }

    private void radioButtonToggle_Click(object sender, System.EventArgs e)
    {
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = false;
      comboBoxCmdProperty.Items.Clear();
      comboBoxCmdProperty.Text = "none";
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "TOGGLE", "-1");
      node.Tag = data;
      node.Text = "Toggle Layer";
    }

    private void radioButtonPower_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "POWER", "EXIT");
      node.Tag = data;
      switch (data.Value)
      {
        case "EXIT":
          node.Text = "Exit MediaPortal";
          break;
        case "REBOOT":
          node.Text = "Reboot Windows";
          break;
        case "SHUTDOWN":
          node.Text = "Shutdown Windows";
          break;
        case "STANDBY":
          node.Text = "Suspend Windows (Standby)";
          break;
        case "HIBERNATE":
          node.Text = "Hibernate Windows";
          break;
      }
      UpdateCombo(ref comboBoxCmdProperty, powerList, data.Value);
    }

    private void radioButtonProcess_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "PROCESS", "CLOSE");
      node.Tag = data;
      switch (data.Value)
      {
        case "CLOSE":
          node.Text = "Close Process";
          break;
        case "KILL":
          node.Text = "Kill Process";
          break;
      }
      UpdateCombo(ref comboBoxCmdProperty, processList, data.Value);
    }

    private void okButton_Click(object sender, System.EventArgs e)
    {
      SaveMapping(inputClassName + ".xml");
      this.Close();
    }

    private void comboBoxSound_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (comboBoxSound.Enabled)
      {
        TreeNode node = getNode("SOUND");
        node.Text = (string)comboBoxSound.SelectedItem;
        if (node.Text == "No Sound")
          node.Tag = new Data("SOUND", null, "");
        else
          node.Tag = new Data("SOUND", null, (string)comboBoxSound.SelectedItem);
      }
    }

    private void comboBoxCmdProperty_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      TreeNode node = getNode("COMMAND");
      Data data = (Data)node.Tag;
      switch (data.Parameter)
      {
        case "ACTION":
          node.Tag = new Data("COMMAND", "ACTION", (int)Enum.Parse(typeof(Action.ActionType), (string)comboBoxCmdProperty.SelectedItem));
          node.Text = (string)comboBoxCmdProperty.SelectedItem;
          break;
        case "WINDOW":
          node.Tag = new Data("COMMAND", "WINDOW", (int)Enum.Parse(typeof(GUIWindow.Window), (string)comboBoxCmdProperty.SelectedItem));
          node.Text = (string)comboBoxCmdProperty.SelectedItem;
          break;
        case "KEY":
          node.Tag = new Data("COMMAND", "KEY", (string)comboBoxCmdProperty.SelectedItem);
          node.Text = "Key \"" + (string)comboBoxCmdProperty.SelectedItem + "\"";
          break;
        case "POWER":
          {
            node.Tag = new Data("COMMAND", "POWER", (string)comboBoxCmdProperty.SelectedItem);
            switch ((string)comboBoxCmdProperty.SelectedItem)
            {
              case "EXIT":
                node.Text = "Exit MediaPortal";
                break;
              case "REBOOT":
                node.Text = "Reboot Windows";
                break;
              case "SHUTDOWN":
                node.Text = "Shutdown Windows";
                break;
              case "STANDBY":
                node.Text = "Suspend Windows (Standby)";
                break;
              case "HIBERNATE":
                node.Text = "Hibernate Windows";
                break;
            }
            break;
          }
        case "PROCESS":
          {
            switch ((string)comboBoxCmdProperty.SelectedItem)
            {
              case "CLOSE":
                node.Tag = new Data("COMMAND", "PROCESS", "CLOSE");
                node.Text = "Close Process";
                break;
              case "KILL":
                node.Tag = new Data("COMMAND", "PROCESS", "KILL");
                node.Text = "Kill Process";
                break;
            }
            break;
          }
      }
    }

    private void comboBoxCondProperty_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      TreeNode node = getNode("CONDITION");
      Data data = (Data)node.Tag;
      switch (data.Parameter)
      {
        case "WINDOW":
          node.Tag = new Data("CONDITION", "WINDOW", (int)Enum.Parse(typeof(GUIWindow.Window), (string)comboBoxCondProperty.SelectedItem));
          node.Text = (string)comboBoxCondProperty.SelectedItem;
          break;
        case "FULLSCREEN":
          if ((string)comboBoxCondProperty.SelectedItem == "Fullscreen")
            node.Tag = new Data("CONDITION", "FULLSCREEN", "true");
          else
            node.Tag = new Data("CONDITION", "FULLSCREEN", "false");
          node.Text = (string)comboBoxCondProperty.SelectedItem;
          break;
        case "PLAYER":
          {
            node.Tag = new Data("CONDITION", "PLAYER", (string)comboBoxCondProperty.SelectedItem);
            switch ((string)comboBoxCondProperty.SelectedItem)
            {
              case "TV":
                node.Text = "TV Playing";
                break;
              case "DVD":
                node.Text = "DVD Playing";
                break;
              case "MEDIA":
                node.Text = "Media Playing";
                break;
            }
            break;
          }
        case "*":
          break;
      }
    }

    private void comboBoxLayer_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      TreeNode node = getNode("LAYER");
      node.Tag = new Data("LAYER", null, comboBoxLayer.SelectedIndex);
      if (comboBoxLayer.SelectedIndex == 0)
        node.Text = "All Layers";
      else
        node.Text = "Layer " + comboBoxLayer.SelectedIndex.ToString();
    }

    private void applyButton_Click(object sender, System.EventArgs e)
    {
      SaveMapping(inputClassName + ".xml");
    }

    private void buttonUp_Click(object sender, System.EventArgs e)
    {
      bool expanded = false;
      TreeNode node = treeMapping.SelectedNode;
      if (((Data)node.Tag).Type != "BUTTON")
        expanded = node.IsExpanded;
      if ((((Data)node.Tag).Type == "COMMAND") || (((Data)node.Tag).Type == "SOUND"))
      {
        node = node.Parent;
        expanded = true;
      }
      if ((((Data)node.Tag).Type != "BUTTON") && (((Data)node.Tag).Type != "CONDITION"))
        return;
      if (node.Index > 0)
      {
        int index = node.Index - 1;
        TreeNode tmpNode = (TreeNode)node.Clone();
        TreeNode parentNode = node.Parent;
        node.Remove();
        if (expanded)
          tmpNode.Expand();
        parentNode.Nodes.Insert(index, tmpNode);
        treeMapping.SelectedNode = tmpNode;
      }
    }

    private void buttonDown_Click(object sender, System.EventArgs e)
    {
      bool expanded = false;
      TreeNode node = treeMapping.SelectedNode;
      if (((Data)node.Tag).Type != "BUTTON")
        expanded = node.IsExpanded;
      if ((((Data)node.Tag).Type == "COMMAND") || (((Data)node.Tag).Type == "SOUND"))
      {
        node = node.Parent;
        expanded = true;
      }
      if ((((Data)node.Tag).Type != "BUTTON") && (((Data)node.Tag).Type != "CONDITION"))
        return;
      if (node.Index < node.Parent.Nodes.Count - 1)
      {
        int index = node.Index + 1;
        TreeNode tmpNode = (TreeNode)node.Clone();
        TreeNode parentNode = node.Parent;
        node.Remove();
        if (expanded)
          tmpNode.Expand();
        parentNode.Nodes.Insert(index, tmpNode);
        treeMapping.SelectedNode = tmpNode;
      }
    }

    private void buttonRemove_Click(object sender, System.EventArgs e)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data)node.Tag;
      DialogResult result = MessageBox.Show(this, "Are you sure you want to remove this " + data.Type.ToLower() + "?", "Remove " + data.Type.ToLower(),
        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
      if (result == DialogResult.Yes)
        treeMapping.SelectedNode.Remove();
    }

    private void buttonNew_Click(object sender, System.EventArgs e)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data)node.Tag;

      TreeNode newLayer = new TreeNode("All Layers");
      newLayer.Tag = new Data("LAYER", null, "0");
      newLayer.ForeColor = Color.DimGray;

      TreeNode newCondition = new TreeNode("No Condition");
      newCondition.Tag = new Data("CONDITION", "*", "-1");
      newCondition.ForeColor = Color.Blue;

      TreeNode newCommand = new TreeNode("ACTION_INVALID");
      newCommand.Tag = new Data("COMMAND", "ACTION", "0");
      newCommand.ForeColor = Color.DarkGreen;

      TreeNode newSound = new TreeNode("No Sound");
      newSound.Tag = new Data("SOUND", "", "");
      newSound.ForeColor = Color.DarkRed;

      switch (data.Type)
      {
        case "LAYER":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          newLayer.Nodes.Add(newCondition);
          node.Parent.Nodes.Add(newLayer);
          newLayer.Expand();
          treeMapping.SelectedNode = newLayer;
          break;
        case "CONDITION":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          node.Parent.Nodes.Add(newCondition);
          newCondition.Expand();
          treeMapping.SelectedNode = newCondition;
          break;
        case "COMMAND":
        case "SOUND":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          node.Parent.Parent.Nodes.Add(newCondition);
          newCondition.Expand();
          treeMapping.SelectedNode = newCondition;
          break;
        case "BUTTON":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          newLayer.Nodes.Add(newCondition);
          node.Nodes.Add(newLayer);
          newLayer.Expand();
          treeMapping.SelectedNode = newLayer;
          break;
      }
    }

    private void buttonReset_Click(object sender, System.EventArgs e)
    {
      LoadMapping(inputClassName + ".xml", true);
    }

    private void cancelButton_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }



    //    private TreeNode tn;
    //
    //    private void treeMapping_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
    //    {
    //      tn=e.Item as TreeNode;
    //      DoDragDrop(e.Item.ToString(), DragDropEffects.Move);
    //    }
    //
    //    private void treeMapping_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
    //    {
    //      Point pt = new Point(e.X,e.Y); 
    //      pt = treeMapping.PointToClient(pt); 
    //      TreeNode ParentNode = treeMapping.GetNodeAt(pt); 
    //      ParentNode.Nodes.Add(tn.Text); // this copies the node 
    //      tn.Remove(); // need to remove the original version of the node 
    //    }
    //
    //    private void treeMapping_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
    //    {
    //      e.Effect=DragDropEffects.Move;
    //    }
  }
}
