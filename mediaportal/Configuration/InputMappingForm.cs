#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for ButtonMappingForm.
  /// </summary>
  public class InputMappingForm : System.Windows.Forms.Form
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

    bool changedSettings = false;

    class Data
    {
      string type;
      object value;
      object parameter;

      public Data(object newType, object newParameter, object newValue)
      {
        if (newValue == null)
          newValue = string.Empty;
        if (newParameter == null)
          newParameter = string.Empty;
        type = (string)newType;
        value = newValue;
        parameter = newParameter;
      }

      public string Type { get { return type; } }
      public object Value { get { return value; } }
      public object Parameter { get { return parameter; } }
    }

    #region Controls

    private System.Windows.Forms.TreeView treeMapping;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonWindow;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonFullscreen;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPlaying;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonNoCondition;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxCondProperty;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxCmdProperty;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCondition;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAction;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonActWindow;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonToggle;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPower;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxSound;
    private MediaPortal.UserInterface.Controls.MPLabel labelSound;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAction;
    private MediaPortal.UserInterface.Controls.MPGradientLabel headerLabel;
    private MediaPortal.UserInterface.Controls.MPButton applyButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPBeveledLine beveledLine1;
    private MediaPortal.UserInterface.Controls.MPLabel labelLayer;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxLayer;
    private MediaPortal.UserInterface.Controls.MPButton buttonUp;
    private MediaPortal.UserInterface.Controls.MPButton buttonDown;
    private MediaPortal.UserInterface.Controls.MPButton buttonNew;
    private MediaPortal.UserInterface.Controls.MPButton buttonRemove;
    private MediaPortal.UserInterface.Controls.MPButton buttonDefault;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxLayer;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonProcess;
    private Label label1;
    private TextBox textBoxKeyChar;
    private TextBox textBoxKeyCode;

    #endregion
    private Label label2;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public InputMappingForm(string name)
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      comboBoxSound.DataSource = soundList;
      comboBoxLayer.DataSource = layerList;
      inputClassName = name;
      LoadMapping(inputClassName + ".xml", false);
      headerLabel.Caption = inputClassName;
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputMappingForm));
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.radioButtonWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonFullscreen = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPlaying = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonNoCondition = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.comboBoxCondProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxCmdProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxCondition = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonAction = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonActWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonToggle = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPower = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBoxAction = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.textBoxKeyCode = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxKeyChar = new MediaPortal.UserInterface.Controls.MPTextBox();
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
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.groupBoxCondition.SuspendLayout();
      this.groupBoxAction.SuspendLayout();
      this.groupBoxLayer.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeMapping
      // 
      this.treeMapping.AllowDrop = true;
      this.treeMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.treeMapping.FullRowSelect = true;
      this.treeMapping.HideSelection = false;
      this.treeMapping.Location = new System.Drawing.Point(16, 56);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.Size = new System.Drawing.Size(312, 330);
      this.treeMapping.TabIndex = 1;
      this.treeMapping.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMapping_AfterSelect);
      // 
      // radioButtonWindow
      // 
      this.radioButtonWindow.Location = new System.Drawing.Point(24, 20);
      this.radioButtonWindow.Name = "radioButtonWindow";
      this.radioButtonWindow.Size = new System.Drawing.Size(88, 16);
      this.radioButtonWindow.TabIndex = 9;
      this.radioButtonWindow.Text = "Window";
      this.radioButtonWindow.Click += new System.EventHandler(this.radioButtonWindow_Click);
      // 
      // radioButtonFullscreen
      // 
      this.radioButtonFullscreen.Location = new System.Drawing.Point(112, 20);
      this.radioButtonFullscreen.Name = "radioButtonFullscreen";
      this.radioButtonFullscreen.Size = new System.Drawing.Size(88, 16);
      this.radioButtonFullscreen.TabIndex = 10;
      this.radioButtonFullscreen.Text = "Fullscreen";
      this.radioButtonFullscreen.Click += new System.EventHandler(this.radioButtonFullscreen_Click);
      // 
      // radioButtonPlaying
      // 
      this.radioButtonPlaying.Location = new System.Drawing.Point(24, 44);
      this.radioButtonPlaying.Name = "radioButtonPlaying";
      this.radioButtonPlaying.Size = new System.Drawing.Size(88, 16);
      this.radioButtonPlaying.TabIndex = 11;
      this.radioButtonPlaying.Text = "Playing";
      this.radioButtonPlaying.Click += new System.EventHandler(this.radioButtonPlaying_Click);
      // 
      // radioButtonNoCondition
      // 
      this.radioButtonNoCondition.Location = new System.Drawing.Point(112, 44);
      this.radioButtonNoCondition.Name = "radioButtonNoCondition";
      this.radioButtonNoCondition.Size = new System.Drawing.Size(88, 16);
      this.radioButtonNoCondition.TabIndex = 12;
      this.radioButtonNoCondition.Text = "No Condition";
      this.radioButtonNoCondition.Click += new System.EventHandler(this.radioButtonNoCondition_Click);
      // 
      // comboBoxCondProperty
      // 
      this.comboBoxCondProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCondProperty.ForeColor = System.Drawing.Color.Blue;
      this.comboBoxCondProperty.Location = new System.Drawing.Point(24, 68);
      this.comboBoxCondProperty.Name = "comboBoxCondProperty";
      this.comboBoxCondProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCondProperty.Sorted = true;
      this.comboBoxCondProperty.TabIndex = 13;
      this.comboBoxCondProperty.SelectionChangeCommitted += new System.EventHandler(this.comboBoxCondProperty_SelectionChangeCommitted);
      // 
      // comboBoxCmdProperty
      // 
      this.comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCmdProperty.ForeColor = System.Drawing.Color.DarkGreen;
      this.comboBoxCmdProperty.Location = new System.Drawing.Point(24, 92);
      this.comboBoxCmdProperty.Name = "comboBoxCmdProperty";
      this.comboBoxCmdProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCmdProperty.Sorted = true;
      this.comboBoxCmdProperty.TabIndex = 14;
      this.comboBoxCmdProperty.SelectionChangeCommitted += new System.EventHandler(this.comboBoxCmdProperty_SelectionChangeCommitted);
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
      this.groupBoxCondition.Location = new System.Drawing.Point(352, 108);
      this.groupBoxCondition.Name = "groupBoxCondition";
      this.groupBoxCondition.Size = new System.Drawing.Size(224, 100);
      this.groupBoxCondition.TabIndex = 15;
      this.groupBoxCondition.TabStop = false;
      this.groupBoxCondition.Text = "Condition";
      // 
      // radioButtonAction
      // 
      this.radioButtonAction.Location = new System.Drawing.Point(24, 20);
      this.radioButtonAction.Name = "radioButtonAction";
      this.radioButtonAction.Size = new System.Drawing.Size(88, 16);
      this.radioButtonAction.TabIndex = 14;
      this.radioButtonAction.Text = "Action";
      this.radioButtonAction.Click += new System.EventHandler(this.radioButtonAction_Click);
      // 
      // radioButtonActWindow
      // 
      this.radioButtonActWindow.Location = new System.Drawing.Point(112, 20);
      this.radioButtonActWindow.Name = "radioButtonActWindow";
      this.radioButtonActWindow.Size = new System.Drawing.Size(88, 16);
      this.radioButtonActWindow.TabIndex = 14;
      this.radioButtonActWindow.Text = "Window";
      this.radioButtonActWindow.Click += new System.EventHandler(this.radioButtonActWindow_Click);
      // 
      // radioButtonToggle
      // 
      this.radioButtonToggle.Location = new System.Drawing.Point(112, 44);
      this.radioButtonToggle.Name = "radioButtonToggle";
      this.radioButtonToggle.Size = new System.Drawing.Size(88, 16);
      this.radioButtonToggle.TabIndex = 17;
      this.radioButtonToggle.Text = "Toggle Layer";
      this.radioButtonToggle.Click += new System.EventHandler(this.radioButtonToggle_Click);
      // 
      // radioButtonPower
      // 
      this.radioButtonPower.Location = new System.Drawing.Point(24, 44);
      this.radioButtonPower.Name = "radioButtonPower";
      this.radioButtonPower.Size = new System.Drawing.Size(80, 16);
      this.radioButtonPower.TabIndex = 18;
      this.radioButtonPower.Text = "Powerdown";
      this.radioButtonPower.Click += new System.EventHandler(this.radioButtonPower_Click);
      // 
      // groupBoxAction
      // 
      this.groupBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAction.Controls.Add(this.textBoxKeyCode);
      this.groupBoxAction.Controls.Add(this.label1);
      this.groupBoxAction.Controls.Add(this.textBoxKeyChar);
      this.groupBoxAction.Controls.Add(this.radioButtonProcess);
      this.groupBoxAction.Controls.Add(this.labelSound);
      this.groupBoxAction.Controls.Add(this.comboBoxSound);
      this.groupBoxAction.Controls.Add(this.radioButtonAction);
      this.groupBoxAction.Controls.Add(this.radioButtonActWindow);
      this.groupBoxAction.Controls.Add(this.radioButtonToggle);
      this.groupBoxAction.Controls.Add(this.radioButtonPower);
      this.groupBoxAction.Controls.Add(this.comboBoxCmdProperty);
      this.groupBoxAction.Enabled = false;
      this.groupBoxAction.Location = new System.Drawing.Point(352, 216);
      this.groupBoxAction.Name = "groupBoxAction";
      this.groupBoxAction.Size = new System.Drawing.Size(224, 192);
      this.groupBoxAction.TabIndex = 16;
      this.groupBoxAction.TabStop = false;
      this.groupBoxAction.Text = "Action";
      // 
      // textBoxKeyCode
      // 
      this.textBoxKeyCode.Enabled = false;
      this.textBoxKeyCode.Location = new System.Drawing.Point(152, 124);
      this.textBoxKeyCode.MaxLength = 3;
      this.textBoxKeyCode.Name = "textBoxKeyCode";
      this.textBoxKeyCode.Size = new System.Drawing.Size(48, 20);
      this.textBoxKeyCode.TabIndex = 24;
      this.textBoxKeyCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyCode_KeyUp);
      this.textBoxKeyCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyCode_KeyPress);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(24, 128);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(28, 13);
      this.label1.TabIndex = 23;
      this.label1.Text = "Key:";
      // 
      // textBoxKeyChar
      // 
      this.textBoxKeyChar.Enabled = false;
      this.textBoxKeyChar.Location = new System.Drawing.Point(72, 124);
      this.textBoxKeyChar.MaxLength = 3;
      this.textBoxKeyChar.Name = "textBoxKeyChar";
      this.textBoxKeyChar.Size = new System.Drawing.Size(80, 20);
      this.textBoxKeyChar.TabIndex = 22;
      this.textBoxKeyChar.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyChar_KeyUp);
      this.textBoxKeyChar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyChar_KeyPress);
      // 
      // radioButtonProcess
      // 
      this.radioButtonProcess.Location = new System.Drawing.Point(24, 68);
      this.radioButtonProcess.Name = "radioButtonProcess";
      this.radioButtonProcess.Size = new System.Drawing.Size(80, 16);
      this.radioButtonProcess.TabIndex = 21;
      this.radioButtonProcess.Text = "Process";
      this.radioButtonProcess.Click += new System.EventHandler(this.radioButtonProcess_Click);
      // 
      // labelSound
      // 
      this.labelSound.AutoSize = true;
      this.labelSound.Location = new System.Drawing.Point(24, 156);
      this.labelSound.Name = "labelSound";
      this.labelSound.Size = new System.Drawing.Size(41, 13);
      this.labelSound.TabIndex = 20;
      this.labelSound.Text = "Sound:";
      // 
      // comboBoxSound
      // 
      this.comboBoxSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSound.ForeColor = System.Drawing.Color.DarkRed;
      this.comboBoxSound.Location = new System.Drawing.Point(72, 153);
      this.comboBoxSound.Name = "comboBoxSound";
      this.comboBoxSound.Size = new System.Drawing.Size(128, 21);
      this.comboBoxSound.TabIndex = 19;
      this.comboBoxSound.SelectionChangeCommitted += new System.EventHandler(this.comboBoxSound_SelectionChangeCommitted);
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(348, 437);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 20;
      this.applyButton.Text = "Apply";
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(428, 437);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 19;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(507, 437);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 18;
      this.cancelButton.Text = "Cancel";
      // 
      // groupBoxLayer
      // 
      this.groupBoxLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxLayer.Controls.Add(this.comboBoxLayer);
      this.groupBoxLayer.Controls.Add(this.labelLayer);
      this.groupBoxLayer.Enabled = false;
      this.groupBoxLayer.Location = new System.Drawing.Point(352, 48);
      this.groupBoxLayer.Name = "groupBoxLayer";
      this.groupBoxLayer.Size = new System.Drawing.Size(224, 52);
      this.groupBoxLayer.TabIndex = 22;
      this.groupBoxLayer.TabStop = false;
      this.groupBoxLayer.Text = "Layer";
      // 
      // comboBoxLayer
      // 
      this.comboBoxLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxLayer.ForeColor = System.Drawing.Color.DimGray;
      this.comboBoxLayer.Location = new System.Drawing.Point(80, 20);
      this.comboBoxLayer.Name = "comboBoxLayer";
      this.comboBoxLayer.Size = new System.Drawing.Size(121, 21);
      this.comboBoxLayer.TabIndex = 25;
      this.comboBoxLayer.SelectionChangeCommitted += new System.EventHandler(this.comboBoxLayer_SelectionChangeCommitted);
      // 
      // labelLayer
      // 
      this.labelLayer.AutoSize = true;
      this.labelLayer.Location = new System.Drawing.Point(24, 23);
      this.labelLayer.Name = "labelLayer";
      this.labelLayer.Size = new System.Drawing.Size(36, 13);
      this.labelLayer.TabIndex = 16;
      this.labelLayer.Text = "Layer:";
      // 
      // buttonUp
      // 
      this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUp.Location = new System.Drawing.Point(16, 392);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(56, 16);
      this.buttonUp.TabIndex = 23;
      this.buttonUp.Text = "Up";
      this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(80, 392);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(56, 16);
      this.buttonDown.TabIndex = 24;
      this.buttonDown.Text = "Down";
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonNew
      // 
      this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNew.Location = new System.Drawing.Point(144, 392);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(56, 16);
      this.buttonNew.TabIndex = 26;
      this.buttonNew.Text = "New";
      this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonRemove.Location = new System.Drawing.Point(208, 392);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(56, 16);
      this.buttonRemove.TabIndex = 27;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDefault.Location = new System.Drawing.Point(272, 392);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(56, 16);
      this.buttonDefault.TabIndex = 28;
      this.buttonDefault.Text = "Default";
      this.buttonDefault.Click += new System.EventHandler(this.buttonReset_Click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(328, 374);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(13, 13);
      this.label2.TabIndex = 29;
      this.label2.Text = "+";
      this.label2.Click += new System.EventHandler(this.label2_Click);
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 427);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(574, 2);
      this.beveledLine1.TabIndex = 21;
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(16, 16);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(560, 24);
      this.headerLabel.TabIndex = 17;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // InputMappingForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.AutoScroll = true;
      this.ClientSize = new System.Drawing.Size(592, 470);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.treeMapping);
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.buttonRemove);
      this.Controls.Add(this.buttonNew);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUp);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.groupBoxAction);
      this.Controls.Add(this.groupBoxCondition);
      this.Controls.Add(this.groupBoxLayer);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "InputMappingForm";
      this.ShowInTaskbar = false;
      this.Text = "MediaPortal - Setup";
      this.groupBoxCondition.ResumeLayout(false);
      this.groupBoxAction.ResumeLayout(false);
      this.groupBoxAction.PerformLayout();
      this.groupBoxLayer.ResumeLayout(false);
      this.groupBoxLayer.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion


    void LoadMapping(string xmlFile, bool defaults)
    {
      try
      {
        groupBoxLayer.Enabled = false;
        groupBoxCondition.Enabled = false;
        groupBoxAction.Enabled = false;
        treeMapping.Nodes.Clear();
        XmlDocument doc = new XmlDocument();
        string path = "InputDeviceMappings\\defaults\\" + xmlFile;
        if (!defaults && File.Exists("InputDeviceMappings\\custom\\" + xmlFile))
          path = "InputDeviceMappings\\custom\\" + xmlFile;
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
              string conditionString = string.Empty;
              string commandString = string.Empty;

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
              conditionNode.Tag = new Data("CONDITION", nodeAction.Attributes["condition"].Value, nodeAction.Attributes["conproperty"].Value);
              if (commandString == "ACTION_KEY_PRESSED")
              {
                string cmdKeyChar = nodeAction.Attributes["cmdkeychar"].Value;
                string cmdKeyCode = nodeAction.Attributes["cmdkeycode"].Value;
                TreeNode commandNode = new TreeNode(string.Format("ACTION_KEY_PRESSED: {0} [{1}]", cmdKeyChar, cmdKeyCode));

                Key key = new Key(Convert.ToInt32(cmdKeyChar), Convert.ToInt32(cmdKeyCode));

                commandNode.Tag = new Data("COMMAND", "KEY", key);
                commandNode.ForeColor = Color.DarkGreen;
                conditionNode.ForeColor = Color.Blue;
                conditionNode.Nodes.Add(commandNode);
              }
              else
              {
                TreeNode commandNode = new TreeNode(commandString);
                commandNode.Tag = new Data("COMMAND", nodeAction.Attributes["command"].Value, nodeAction.Attributes["cmdproperty"].Value);
                commandNode.ForeColor = Color.DarkGreen;
                conditionNode.ForeColor = Color.Blue;
                conditionNode.Nodes.Add(commandNode);
              }

              TreeNode soundNode = new TreeNode(sound);
              soundNode.Tag = new Data("SOUND", null, nodeAction.Attributes["sound"].Value);
              if (soundNode.Text == string.Empty)
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
        changedSettings = false;
      }
      catch
      {
        File.Delete("InputDeviceMappings\\custom\\" + xmlFile);
        LoadMapping(xmlFile, true);
      }
    }

    bool SaveMapping(string xmlFile)
    {
#if !DEBUG
      try
#endif
      {
        DirectoryInfo dir = Directory.CreateDirectory("InputDeviceMappings\\custom");
      }
#if !DEBUG
      catch
      {
        Log.Write("MAP: Error accessing directory \"InputDeviceMappings\\custom\"");
      }

      try
#endif
      {
        XmlTextWriter writer = new XmlTextWriter("InputDeviceMappings\\custom\\" + xmlFile, System.Text.Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 1;
        writer.IndentChar = (char)9;
        writer.WriteStartDocument(true);
        writer.WriteStartElement("mappings"); // <mappings>
        writer.WriteAttributeString("version", "3");
        if (treeMapping.Nodes.Count > 0)
          foreach (TreeNode remoteNode in treeMapping.Nodes)
          {
            writer.WriteStartElement("remote"); // <remote>
            writer.WriteAttributeString("family", (string)((Data)remoteNode.Tag).Value);
            if (remoteNode.Nodes.Count > 0)
              foreach (TreeNode buttonNode in remoteNode.Nodes)
              {
                writer.WriteStartElement("button"); // <button>
                writer.WriteAttributeString("name", (string)((Data)buttonNode.Tag).Parameter);
                writer.WriteAttributeString("code", (string)((Data)buttonNode.Tag).Value);

                Log.Write("Count: {0}", buttonNode.Nodes.Count);
                if (buttonNode.Nodes.Count > 0)
                  foreach (TreeNode layerNode in buttonNode.Nodes)
                  {
                    foreach (TreeNode conditionNode in layerNode.Nodes)
                    {
                      string layer;
                      string condition;
                      string conProperty;
                      string command = string.Empty;
                      string cmdProperty = string.Empty;
                      string cmdKeyChar = string.Empty;
                      string cmdKeyCode = string.Empty;
                      string sound = string.Empty;
                      foreach (TreeNode commandNode in conditionNode.Nodes)
                      {
                        switch (((Data)commandNode.Tag).Type)
                        {
                          case "COMMAND":
                            {
                              command = (string)((Data)commandNode.Tag).Parameter;
                              if (command != "KEY")
                                cmdProperty = (string)((Data)commandNode.Tag).Value;
                              else
                              {
                                command = "ACTION";
                                Key key = (Key)((Data)commandNode.Tag).Value;
                                cmdProperty = "93";
                                cmdKeyChar = key.KeyChar.ToString();
                                cmdKeyCode = key.KeyCode.ToString();
                              }
                            }
                            break;
                          case "SOUND":
                            sound = (string)((Data)commandNode.Tag).Value;
                            break;
                        }
                      }
                      condition = (string)((Data)conditionNode.Tag).Parameter;
                      conProperty = (string)((Data)conditionNode.Tag).Value;
                      layer = (string)((Data)layerNode.Tag).Value;
                      writer.WriteStartElement("action"); // <action>
                      writer.WriteAttributeString("layer", layer);
                      writer.WriteAttributeString("condition", condition);
                      writer.WriteAttributeString("conproperty", conProperty);
                      writer.WriteAttributeString("command", command);
                      writer.WriteAttributeString("cmdproperty", cmdProperty);
                      if (cmdProperty == Convert.ToInt32(Action.ActionType.ACTION_KEY_PRESSED).ToString())
                      {
                        if (cmdKeyChar != string.Empty)
                        {
                          writer.WriteAttributeString("cmdkeychar", cmdKeyChar);
                        }
                        else
                        {
                          writer.WriteAttributeString("cmdkeychar", "0");
                        }
                        if (cmdKeyCode != string.Empty)
                        {
                          writer.WriteAttributeString("cmdkeycode", cmdKeyCode);
                        }
                        else
                        {
                          writer.WriteAttributeString("cmdkeychar", "0");
                        }

                      }
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
        changedSettings = false;
        return true;
      }
#if !DEBUG
      catch (Exception ex)
      {
        Log.Write("MAP: Error saving mapping to XML file: {0}", ex.Message);
        return false;
      }
#endif
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
          if ((data.Type == "SOUND") || (data.Type == "KEY"))
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
          if ((data.Type == "COMMAND") || (data.Type == "KEY"))
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
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent;
          break;
        case "LAYER":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent;
          break;
        case "BUTTON":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent.Parent;
          else if (data.Type == "LAYER")
            return node.Parent;
          break;
        case "REMOTE":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
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
      return null;
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
        case "KEY":
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

            switch ((string)data.Parameter)
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
                UpdateCombo(ref comboBoxCondProperty, playerList, (string)data.Value);
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
                  if ((string)data.Value != string.Empty)
                    comboBoxSound.SelectedItem = data.Value;
                  else
                    comboBoxSound.SelectedItem = "none";
                  break;
                case "COMMAND":
                  switch ((string)data.Parameter)
                  {
                    case "ACTION":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonAction.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = false;
                      textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = "";
                      textBoxKeyCode.Text = "";
                      UpdateCombo(ref comboBoxCmdProperty, actionList, Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(data.Value)));
                      break;
                    case "KEY":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonAction.Checked = true;
                      textBoxKeyChar.Enabled = true;
                      textBoxKeyCode.Enabled = true;
                      textBoxKeyChar.Text = ((Key)data.Value).KeyChar.ToString();
                      textBoxKeyCode.Text = ((Key)data.Value).KeyCode.ToString();
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, actionList, "ACTION_KEY_PRESSED");
                      break;
                    case "WINDOW":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonActWindow.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = false;
                      textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = "";
                      textBoxKeyCode.Text = "";
                      UpdateCombo(ref comboBoxCmdProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(data.Value)));
                      break;
                    case "TOGGLE":
                      radioButtonToggle.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = false;
                      textBoxKeyChar.Enabled = false;
                      textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = "";
                      textBoxKeyCode.Text = "";
                      comboBoxCmdProperty.Items.Clear();
                      comboBoxCmdProperty.Text = string.Empty;
                      break;
                    case "POWER":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonPower.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = false;
                      textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = "";
                      textBoxKeyCode.Text = "";
                      UpdateCombo(ref comboBoxCmdProperty, powerList, (string)data.Value);
                      break;
                    case "PROCESS":
                      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                      radioButtonProcess.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = false;
                      textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = "";
                      textBoxKeyCode.Text = "";
                      UpdateCombo(ref comboBoxCmdProperty, processList, (string)data.Value);
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
      changedSettings = true;
    }

    private void radioButtonFullscreen_Click(object sender, System.EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      Data data = new Data("CONDITION", "FULLSCREEN", "true");
      node.Tag = data;
      UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
      changedSettings = true;
    }

    private void radioButtonPlaying_Click(object sender, System.EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      Data data = new Data("CONDITION", "PLAYER", "TV");
      node.Tag = data;
      UpdateCombo(ref comboBoxCondProperty, playerList, "TV");
      changedSettings = true;
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
      changedSettings = true;
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
      changedSettings = true;
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
      changedSettings = true;
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
      changedSettings = true;
    }

    private void radioButtonPower_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "POWER", "EXIT");
      node.Tag = data;
      switch ((string)data.Value)
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
      UpdateCombo(ref comboBoxCmdProperty, powerList, (string)data.Value);
      changedSettings = true;
    }

    private void radioButtonProcess_Click(object sender, System.EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "PROCESS", "CLOSE");
      node.Tag = data;
      switch ((string)data.Value)
      {
        case "CLOSE":
          node.Text = "Close Process";
          break;
        case "KILL":
          node.Text = "Kill Process";
          break;
      }
      UpdateCombo(ref comboBoxCmdProperty, processList, (string)data.Value);
      changedSettings = true;
    }

    private void okButton_Click(object sender, System.EventArgs e)
    {
      if (changedSettings)
        SaveMapping(inputClassName + ".xml");
      this.Close();
    }

    private void applyButton_Click(object sender, System.EventArgs e)
    {
      if (changedSettings)
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
      changedSettings = true;
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
      changedSettings = true;
    }

    private void buttonRemove_Click(object sender, System.EventArgs e)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data)node.Tag;
      if ((data.Type == "COMMAND") || (data.Type == "SOUND") || (data.Type == "CONDITION"))
      {
        node = getNode("CONDITION");
        data = (Data)node.Tag;
      }
      DialogResult result = MessageBox.Show(this, "Are you sure you want to remove this " + data.Type.ToLower() + "?", "Remove " + data.Type.ToLower(),
        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
      if (result == DialogResult.Yes)
      {
        node.Remove();
        changedSettings = true;
      }
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
      newSound.Tag = new Data("SOUND", string.Empty, string.Empty);
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
      changedSettings = true;
    }

    private void buttonReset_Click(object sender, System.EventArgs e)
    {
      if (File.Exists("InputDeviceMappings\\custom\\" + inputClassName + ".xml"))
        File.Delete("InputDeviceMappings\\custom\\" + inputClassName + ".xml");
      LoadMapping(inputClassName + ".xml", true);
    }

    private void textBoxKeyCode_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!char.IsNumber(e.KeyChar) && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void textBoxKeyChar_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!char.IsNumber(e.KeyChar) && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void comboBoxLayer_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = getNode("LAYER");
      node.Tag = new Data("LAYER", null, comboBoxLayer.SelectedIndex);
      if (comboBoxLayer.SelectedIndex == 0)
        node.Text = "All Layers";
      else
        node.Text = "Layer " + comboBoxLayer.SelectedIndex.ToString();
      changedSettings = true;
    }

    private void comboBoxCondProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = getNode("CONDITION");
      Data data = (Data)node.Tag;
      switch ((string)data.Parameter)
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
      changedSettings = true;
    }

    private void comboBoxCmdProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if ((string)comboBoxCmdProperty.SelectedItem == "ACTION_KEY_PRESSED")
      {
        textBoxKeyChar.Enabled = true;
        textBoxKeyCode.Enabled = true;
      }
      else
      {
        textBoxKeyChar.Enabled = false;
        textBoxKeyCode.Enabled = false;
      }

      TreeNode node = getNode("COMMAND");
      Data data = (Data)node.Tag;
      switch ((string)data.Parameter)
      {
        case "ACTION":
          if ((string)comboBoxCmdProperty.SelectedItem != "ACTION_KEY_PRESSED")
          {
            node.Tag = new Data("COMMAND", "ACTION", (int)Enum.Parse(typeof(Action.ActionType), (string)comboBoxCmdProperty.SelectedItem));
            node.Text = (string)comboBoxCmdProperty.SelectedItem;
          }
          else
          {
            textBoxKeyChar.Text = "0";
            textBoxKeyCode.Text = "0";
            Key key = new Key(Convert.ToInt32(textBoxKeyChar.Text), Convert.ToInt32(textBoxKeyCode.Text));
            node.Tag = new Data("COMMAND", "KEY", key);
            node.Text = string.Format("ACTION_KEY_PRESSED: {0} [{1}]", textBoxKeyChar.Text, textBoxKeyCode.Text);
          }
          break;
        case "WINDOW":
          node.Tag = new Data("COMMAND", "WINDOW", (int)Enum.Parse(typeof(GUIWindow.Window), (string)comboBoxCmdProperty.SelectedItem));
          node.Text = (string)comboBoxCmdProperty.SelectedItem;
          break;
        case "POWER":
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
        case "PROCESS":
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
      changedSettings = true;
    }

    private void comboBoxSound_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = getNode("SOUND");
      node.Text = (string)comboBoxSound.SelectedItem;
      if (node.Text == "No Sound")
        node.Tag = new Data("SOUND", null, string.Empty);
      else
        node.Tag = new Data("SOUND", null, (string)comboBoxSound.SelectedItem);
      changedSettings = true;
    }

    private void textBoxKeyChar_KeyUp(object sender, KeyEventArgs e)
    {
      string keyChar = textBoxKeyChar.Text;
      string keyCode = textBoxKeyCode.Text;
      TreeNode node = getNode("COMMAND");
      if (keyChar == string.Empty)
        keyChar = "0";
      if (keyCode == string.Empty)
        keyCode = "0";
      Key key = new Key(Convert.ToInt32(keyChar), Convert.ToInt32(keyCode));
      node.Tag = new Data("COMMAND", "KEY", key);
      node.Text = string.Format("ACTION_KEY_PRESSED: {0} [{1}]", keyChar, keyCode);
      changedSettings = true;
    }

    private void textBoxKeyCode_KeyUp(object sender, KeyEventArgs e)
    {
      textBoxKeyChar_KeyUp(sender, e);
    }

    private void label2_Click(object sender, EventArgs e)
    {
      treeMapping.SelectedNode.ExpandAll();
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
