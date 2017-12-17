#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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

#endregion Copyright (C) 2005-2017 Team MediaPortal

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.InputDevices
{
  /// <summary>
  ///   Summary description for ButtonMappingForm.
  /// </summary>
  public class HidInputMappingForm : MPConfigForm
  {
    /// <summary>
    /// Name of the HID profile we are configuring
    /// </summary>
    public string ProfileName{ get; private set;}

    private readonly ArrayList actionList = new ArrayList();

    /// <summary>
    ///   Required designer variable.
    /// </summary>
    private readonly Container components = null;

    private readonly string[] fullScreenList = {"Fullscreen", "No Fullscreen"};    
    private readonly string[] layerList = {"all", "1", "2"};
    private readonly Array nativeActionList = Enum.GetValues(typeof (Action.ActionType));
    private readonly string[] nativePlayerList = {"TV", "DVD", "MEDIA", "MUSIC"};
    private readonly string[] nativePowerList = {"EXIT", "REBOOT", "SHUTDOWN", "STANDBY", "HIBERNATE", "POWEROFF"};
    private readonly string[] nativeProcessList = {"CLOSE", "KILL"};
    private readonly Array nativeWindowsList = Enum.GetValues(typeof (GUIWindow.Window));
    private readonly string[] playerList = {"TV is running", "DVD is playing", "Media is playing", "Music is playing"};

    private readonly string[] powerList =
    {
      "Exit MediaPortal", "Reboot Windows", "Shutdown Windows", "Standby Windows",
      "Hibernate Windows", "Power Off"
    };

    private readonly string[] processList = {"Close Process", "Kill Process"};
    private readonly string[] soundList = {"none", "back.wav", "click.wav", "cursor.wav"};
    private readonly ArrayList windowsList = new ArrayList();
    private readonly ArrayList windowsListFiltered = new ArrayList();    
    private MPButton buttonNew;
    private bool changedSettings;
    private MPGroupBox groupBoxButton;
    private MPCheckBox mpCheckBoxWindows;
    private MPCheckBox mpCheckBoxShift;
    private MPCheckBox mpCheckBoxAlt;
    private MPCheckBox mpCheckBoxControl;
    private MPCheckBox mpCheckBoxBackground;
    private MPCheckBox mpCheckBoxRepeat;
    private MPComboBox mpComboBoxCode;
    private MPLabel mpLabelCode;


    public HidInputMappingForm(string aProfileName)
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      foreach (GUIWindow.Window wnd in nativeWindowsList)
      {
        if (wnd.ToString().IndexOf("DIALOG") == -1)
        {
          switch ((int) Enum.Parse(typeof (GUIWindow.Window), wnd.ToString()))
          {
            case (int) GUIWindow.Window.WINDOW_ARTIST_INFO:
            case (int) GUIWindow.Window.WINDOW_INVALID:
            case (int) GUIWindow.Window.WINDOW_MINI_GUIDE:
            case (int) GUIWindow.Window.WINDOW_TV_CROP_SETTINGS:
            case (int) GUIWindow.Window.WINDOW_MUSIC:
            case (int) GUIWindow.Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS:
            case (int) GUIWindow.Window.WINDOW_MUSIC_INFO:
            case (int) GUIWindow.Window.WINDOW_OSD:
            case (int) GUIWindow.Window.WINDOW_TOPBAR:
            case (int) GUIWindow.Window.WINDOW_TVOSD:
            case (int) GUIWindow.Window.WINDOW_TVZAPOSD:
            case (int) GUIWindow.Window.WINDOW_VIDEO_ARTIST_INFO:
            case (int) GUIWindow.Window.WINDOW_VIDEO_INFO:
            case (int) GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD:
              break;

            default:
              windowsListFiltered.Add(GetFriendlyName(wnd.ToString()));
              break;
          }
        }
        windowsList.Add(GetFriendlyName(wnd.ToString()));
      }

      foreach (Action.ActionType actn in nativeActionList)
      {
        actionList.Add(GetFriendlyName(actn.ToString()));
      }

      comboBoxSound.DataSource = soundList;
      comboBoxLayer.DataSource = layerList;
      ProfileName = aProfileName;
      LoadMapping(ProfileName, false);
      headerLabel.Caption = ProfileName;
    }

    /// <summary>
    ///   Clean up any resources being used.
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
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.labelExpand = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.buttonApply = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.groupBoxAction = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxGainFocus = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.textBoxKeyCode = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxKeyChar = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.radioButtonProcess = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.labelSound = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxSound = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.radioButtonAction = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonActWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonToggle = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPower = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.comboBoxCmdProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxCondition = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonFullscreen = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPlaying = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonNoCondition = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.comboBoxCondProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxLayer = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxLayer = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelLayer = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonNew = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxButton = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpComboBoxCode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabelCode = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpCheckBoxWindows = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxShift = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxAlt = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxControl = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxBackground = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxRepeat = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxAction.SuspendLayout();
      this.groupBoxCondition.SuspendLayout();
      this.groupBoxLayer.SuspendLayout();
      this.groupBoxButton.SuspendLayout();
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
      this.treeMapping.Location = new System.Drawing.Point(16, 56);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.Size = new System.Drawing.Size(397, 504);
      this.treeMapping.TabIndex = 1;
      this.treeMapping.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMapping_AfterSelect);
      // 
      // labelExpand
      // 
      this.labelExpand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelExpand.AutoSize = true;
      this.labelExpand.Location = new System.Drawing.Point(413, 543);
      this.labelExpand.Name = "labelExpand";
      this.labelExpand.Size = new System.Drawing.Size(13, 13);
      this.labelExpand.TabIndex = 29;
      this.labelExpand.Text = "+";
      this.labelExpand.Click += new System.EventHandler(this.labelExpand_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefault.Location = new System.Drawing.Point(347, 611);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(75, 23);
      this.buttonDefault.TabIndex = 28;
      this.buttonDefault.Text = "Reset";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonRemove.Location = new System.Drawing.Point(272, 566);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(56, 20);
      this.buttonRemove.TabIndex = 27;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(97, 566);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(56, 20);
      this.buttonDown.TabIndex = 24;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUp
      // 
      this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUp.Location = new System.Drawing.Point(16, 566);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(56, 20);
      this.buttonUp.TabIndex = 23;
      this.buttonUp.Text = "Up";
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 601);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(657, 2);
      this.beveledLine1.TabIndex = 21;
      // 
      // buttonApply
      // 
      this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonApply.Location = new System.Drawing.Point(428, 611);
      this.buttonApply.Name = "buttonApply";
      this.buttonApply.Size = new System.Drawing.Size(75, 23);
      this.buttonApply.TabIndex = 20;
      this.buttonApply.Text = "Apply";
      this.buttonApply.UseVisualStyleBackColor = true;
      this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOk.Location = new System.Drawing.Point(509, 611);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 19;
      this.buttonOk.Text = "OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(590, 611);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 18;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
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
      this.headerLabel.Size = new System.Drawing.Size(643, 24);
      this.headerLabel.TabIndex = 17;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // groupBoxAction
      // 
      this.groupBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAction.Controls.Add(this.checkBoxGainFocus);
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
      this.groupBoxAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAction.Location = new System.Drawing.Point(435, 390);
      this.groupBoxAction.Name = "groupBoxAction";
      this.groupBoxAction.Size = new System.Drawing.Size(224, 192);
      this.groupBoxAction.TabIndex = 16;
      this.groupBoxAction.TabStop = false;
      this.groupBoxAction.Text = "Action";
      // 
      // checkBoxGainFocus
      // 
      this.checkBoxGainFocus.AutoSize = true;
      this.checkBoxGainFocus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxGainFocus.Location = new System.Drawing.Point(112, 68);
      this.checkBoxGainFocus.Name = "checkBoxGainFocus";
      this.checkBoxGainFocus.Size = new System.Drawing.Size(78, 17);
      this.checkBoxGainFocus.TabIndex = 25;
      this.checkBoxGainFocus.Text = "Gain Focus";
      this.checkBoxGainFocus.UseVisualStyleBackColor = true;
      this.checkBoxGainFocus.CheckedChanged += new System.EventHandler(this.checkBoxGainFocus_CheckedChanged);
      // 
      // textBoxKeyCode
      // 
      this.textBoxKeyCode.BorderColor = System.Drawing.Color.Empty;
      this.textBoxKeyCode.Enabled = false;
      this.textBoxKeyCode.Location = new System.Drawing.Point(152, 124);
      this.textBoxKeyCode.MaxLength = 3;
      this.textBoxKeyCode.Name = "textBoxKeyCode";
      this.textBoxKeyCode.Size = new System.Drawing.Size(48, 20);
      this.textBoxKeyCode.TabIndex = 24;
      this.textBoxKeyCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyCode_KeyPress);
      this.textBoxKeyCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyCode_KeyUp);
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
      this.textBoxKeyChar.BorderColor = System.Drawing.Color.Empty;
      this.textBoxKeyChar.Enabled = false;
      this.textBoxKeyChar.Location = new System.Drawing.Point(72, 124);
      this.textBoxKeyChar.MaxLength = 3;
      this.textBoxKeyChar.Name = "textBoxKeyChar";
      this.textBoxKeyChar.Size = new System.Drawing.Size(80, 20);
      this.textBoxKeyChar.TabIndex = 22;
      this.textBoxKeyChar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyChar_KeyPress);
      this.textBoxKeyChar.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyChar_KeyUp);
      // 
      // radioButtonProcess
      // 
      this.radioButtonProcess.AutoSize = true;
      this.radioButtonProcess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonProcess.Location = new System.Drawing.Point(24, 68);
      this.radioButtonProcess.Name = "radioButtonProcess";
      this.radioButtonProcess.Size = new System.Drawing.Size(62, 17);
      this.radioButtonProcess.TabIndex = 21;
      this.radioButtonProcess.Text = "Process";
      this.radioButtonProcess.UseVisualStyleBackColor = true;
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
      this.comboBoxSound.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSound.ForeColor = System.Drawing.Color.DarkRed;
      this.comboBoxSound.Location = new System.Drawing.Point(72, 153);
      this.comboBoxSound.Name = "comboBoxSound";
      this.comboBoxSound.Size = new System.Drawing.Size(128, 21);
      this.comboBoxSound.TabIndex = 19;
      this.comboBoxSound.SelectionChangeCommitted += new System.EventHandler(this.comboBoxSound_SelectionChangeCommitted);
      // 
      // radioButtonAction
      // 
      this.radioButtonAction.AutoSize = true;
      this.radioButtonAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAction.Location = new System.Drawing.Point(24, 20);
      this.radioButtonAction.Name = "radioButtonAction";
      this.radioButtonAction.Size = new System.Drawing.Size(54, 17);
      this.radioButtonAction.TabIndex = 14;
      this.radioButtonAction.Text = "Action";
      this.radioButtonAction.UseVisualStyleBackColor = true;
      this.radioButtonAction.Click += new System.EventHandler(this.radioButtonAction_Click);
      // 
      // radioButtonActWindow
      // 
      this.radioButtonActWindow.AutoSize = true;
      this.radioButtonActWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonActWindow.Location = new System.Drawing.Point(112, 20);
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
      this.radioButtonToggle.Location = new System.Drawing.Point(112, 44);
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
      this.radioButtonPower.Location = new System.Drawing.Point(24, 44);
      this.radioButtonPower.Name = "radioButtonPower";
      this.radioButtonPower.Size = new System.Drawing.Size(80, 17);
      this.radioButtonPower.TabIndex = 18;
      this.radioButtonPower.Text = "Powerdown";
      this.radioButtonPower.UseVisualStyleBackColor = true;
      this.radioButtonPower.Click += new System.EventHandler(this.radioButtonPower_Click);
      // 
      // comboBoxCmdProperty
      // 
      this.comboBoxCmdProperty.BorderColor = System.Drawing.Color.Empty;
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
      this.groupBoxCondition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxCondition.Controls.Add(this.radioButtonWindow);
      this.groupBoxCondition.Controls.Add(this.radioButtonFullscreen);
      this.groupBoxCondition.Controls.Add(this.radioButtonPlaying);
      this.groupBoxCondition.Controls.Add(this.radioButtonNoCondition);
      this.groupBoxCondition.Controls.Add(this.comboBoxCondProperty);
      this.groupBoxCondition.Enabled = false;
      this.groupBoxCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCondition.Location = new System.Drawing.Point(435, 249);
      this.groupBoxCondition.Name = "groupBoxCondition";
      this.groupBoxCondition.Size = new System.Drawing.Size(224, 100);
      this.groupBoxCondition.TabIndex = 15;
      this.groupBoxCondition.TabStop = false;
      this.groupBoxCondition.Text = "Condition";
      // 
      // radioButtonWindow
      // 
      this.radioButtonWindow.AutoSize = true;
      this.radioButtonWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonWindow.Location = new System.Drawing.Point(24, 20);
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
      this.radioButtonFullscreen.Location = new System.Drawing.Point(112, 20);
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
      this.radioButtonPlaying.Location = new System.Drawing.Point(24, 44);
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
      this.radioButtonNoCondition.Location = new System.Drawing.Point(112, 44);
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
      this.comboBoxCondProperty.Location = new System.Drawing.Point(24, 68);
      this.comboBoxCondProperty.Name = "comboBoxCondProperty";
      this.comboBoxCondProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCondProperty.Sorted = true;
      this.comboBoxCondProperty.TabIndex = 13;
      this.comboBoxCondProperty.SelectionChangeCommitted += new System.EventHandler(this.comboBoxCondProperty_SelectionChangeCommitted);
      // 
      // groupBoxLayer
      // 
      this.groupBoxLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxLayer.Controls.Add(this.comboBoxLayer);
      this.groupBoxLayer.Controls.Add(this.labelLayer);
      this.groupBoxLayer.Enabled = false;
      this.groupBoxLayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxLayer.Location = new System.Drawing.Point(435, 189);
      this.groupBoxLayer.Name = "groupBoxLayer";
      this.groupBoxLayer.Size = new System.Drawing.Size(224, 52);
      this.groupBoxLayer.TabIndex = 22;
      this.groupBoxLayer.TabStop = false;
      this.groupBoxLayer.Text = "Layer";
      // 
      // comboBoxLayer
      // 
      this.comboBoxLayer.BorderColor = System.Drawing.Color.Empty;
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
      // buttonNew
      // 
      this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNew.Location = new System.Drawing.Point(189, 566);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(56, 20);
      this.buttonNew.TabIndex = 26;
      this.buttonNew.Text = "New";
      this.buttonNew.UseVisualStyleBackColor = true;
      this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
      // 
      // groupBoxButton
      // 
      this.groupBoxButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxButton.Controls.Add(this.mpComboBoxCode);
      this.groupBoxButton.Controls.Add(this.mpLabelCode);
      this.groupBoxButton.Controls.Add(this.mpCheckBoxWindows);
      this.groupBoxButton.Controls.Add(this.mpCheckBoxShift);
      this.groupBoxButton.Controls.Add(this.mpCheckBoxAlt);
      this.groupBoxButton.Controls.Add(this.mpCheckBoxControl);
      this.groupBoxButton.Controls.Add(this.mpCheckBoxBackground);
      this.groupBoxButton.Controls.Add(this.mpCheckBoxRepeat);
      this.groupBoxButton.Enabled = false;
      this.groupBoxButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxButton.Location = new System.Drawing.Point(435, 46);
      this.groupBoxButton.Name = "groupBoxButton";
      this.groupBoxButton.Size = new System.Drawing.Size(224, 137);
      this.groupBoxButton.TabIndex = 30;
      this.groupBoxButton.TabStop = false;
      this.groupBoxButton.Text = "Button";
      // 
      // mpComboBoxCode
      // 
      this.mpComboBoxCode.BorderColor = System.Drawing.Color.Empty;
      this.mpComboBoxCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpComboBoxCode.ForeColor = System.Drawing.Color.DimGray;
      this.mpComboBoxCode.Location = new System.Drawing.Point(80, 19);
      this.mpComboBoxCode.Name = "mpComboBoxCode";
      this.mpComboBoxCode.Size = new System.Drawing.Size(121, 21);
      this.mpComboBoxCode.TabIndex = 27;
      this.mpComboBoxCode.SelectedIndexChanged += new System.EventHandler(this.mpComboBoxCode_SelectedIndexChanged);
      // 
      // mpLabelCode
      // 
      this.mpLabelCode.AutoSize = true;
      this.mpLabelCode.Location = new System.Drawing.Point(24, 22);
      this.mpLabelCode.Name = "mpLabelCode";
      this.mpLabelCode.Size = new System.Drawing.Size(35, 13);
      this.mpLabelCode.TabIndex = 26;
      this.mpLabelCode.Text = "Code:";
      // 
      // mpCheckBoxWindows
      // 
      this.mpCheckBoxWindows.AutoSize = true;
      this.mpCheckBoxWindows.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxWindows.Location = new System.Drawing.Point(71, 108);
      this.mpCheckBoxWindows.Name = "mpCheckBoxWindows";
      this.mpCheckBoxWindows.Size = new System.Drawing.Size(68, 17);
      this.mpCheckBoxWindows.TabIndex = 5;
      this.mpCheckBoxWindows.Text = "Windows";
      this.mpCheckBoxWindows.UseVisualStyleBackColor = true;
      this.mpCheckBoxWindows.CheckedChanged += new System.EventHandler(this.mpCheckBoxWindows_CheckedChanged);
      // 
      // mpCheckBoxShift
      // 
      this.mpCheckBoxShift.AutoSize = true;
      this.mpCheckBoxShift.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxShift.Location = new System.Drawing.Point(5, 84);
      this.mpCheckBoxShift.Name = "mpCheckBoxShift";
      this.mpCheckBoxShift.Size = new System.Drawing.Size(45, 17);
      this.mpCheckBoxShift.TabIndex = 4;
      this.mpCheckBoxShift.Text = "Shift";
      this.mpCheckBoxShift.UseVisualStyleBackColor = true;
      this.mpCheckBoxShift.CheckedChanged += new System.EventHandler(this.mpCheckBoxShift_CheckedChanged);
      // 
      // mpCheckBoxAlt
      // 
      this.mpCheckBoxAlt.AutoSize = true;
      this.mpCheckBoxAlt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxAlt.Location = new System.Drawing.Point(5, 107);
      this.mpCheckBoxAlt.Name = "mpCheckBoxAlt";
      this.mpCheckBoxAlt.Size = new System.Drawing.Size(36, 17);
      this.mpCheckBoxAlt.TabIndex = 3;
      this.mpCheckBoxAlt.Text = "Alt";
      this.mpCheckBoxAlt.UseVisualStyleBackColor = true;
      this.mpCheckBoxAlt.CheckedChanged += new System.EventHandler(this.mpCheckBoxAlt_CheckedChanged);
      // 
      // mpCheckBoxControl
      // 
      this.mpCheckBoxControl.AutoSize = true;
      this.mpCheckBoxControl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxControl.Location = new System.Drawing.Point(71, 85);
      this.mpCheckBoxControl.Name = "mpCheckBoxControl";
      this.mpCheckBoxControl.Size = new System.Drawing.Size(57, 17);
      this.mpCheckBoxControl.TabIndex = 2;
      this.mpCheckBoxControl.Text = "Control";
      this.mpCheckBoxControl.UseVisualStyleBackColor = true;
      this.mpCheckBoxControl.CheckedChanged += new System.EventHandler(this.mpCheckBoxControl_CheckedChanged);
      // 
      // mpCheckBoxBackground
      // 
      this.mpCheckBoxBackground.AutoSize = true;
      this.mpCheckBoxBackground.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxBackground.Location = new System.Drawing.Point(71, 62);
      this.mpCheckBoxBackground.Name = "mpCheckBoxBackground";
      this.mpCheckBoxBackground.Size = new System.Drawing.Size(82, 17);
      this.mpCheckBoxBackground.TabIndex = 1;
      this.mpCheckBoxBackground.Text = "Background";
      this.mpCheckBoxBackground.UseVisualStyleBackColor = true;
      this.mpCheckBoxBackground.CheckedChanged += new System.EventHandler(this.mpCheckBoxBackground_CheckedChanged);
      // 
      // mpCheckBoxRepeat
      // 
      this.mpCheckBoxRepeat.AutoSize = true;
      this.mpCheckBoxRepeat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxRepeat.Location = new System.Drawing.Point(6, 62);
      this.mpCheckBoxRepeat.Name = "mpCheckBoxRepeat";
      this.mpCheckBoxRepeat.Size = new System.Drawing.Size(59, 17);
      this.mpCheckBoxRepeat.TabIndex = 0;
      this.mpCheckBoxRepeat.Text = "Repeat";
      this.mpCheckBoxRepeat.UseVisualStyleBackColor = true;
      this.mpCheckBoxRepeat.CheckedChanged += new System.EventHandler(this.mpCheckBoxRepeat_CheckedChanged);
      // 
      // HidInputMappingForm
      // 
      this.AcceptButton = this.buttonOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(675, 644);
      this.Controls.Add(this.groupBoxButton);
      this.Controls.Add(this.labelExpand);
      this.Controls.Add(this.treeMapping);
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.buttonRemove);
      this.Controls.Add(this.buttonNew);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUp);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.buttonApply);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.groupBoxAction);
      this.Controls.Add(this.groupBoxCondition);
      this.Controls.Add(this.groupBoxLayer);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "HidInputMappingForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MediaPortal - Setup";
      this.groupBoxAction.ResumeLayout(false);
      this.groupBoxAction.PerformLayout();
      this.groupBoxCondition.ResumeLayout(false);
      this.groupBoxCondition.PerformLayout();
      this.groupBoxLayer.ResumeLayout(false);
      this.groupBoxLayer.PerformLayout();
      this.groupBoxButton.ResumeLayout(false);
      this.groupBoxButton.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion Windows Form Designer generated code

    private void CloseThread()
    {
      Thread.Sleep(200);
      Close();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aAttribute"></param>
    /// <param name="aDefault"></param>
    /// <returns></returns>
    public static string GetAttributeValue(XmlAttribute aAttribute, string aDefault)
    {
      if (aAttribute != null)
      {
        return aAttribute.Value;
      }

      return aDefault;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aWriter"></param>
    /// <param name="aAtrributeName"></param>
    /// <param name="aValue"></param>
    /// <param name="aDefaultIsNegative"></param>
    public static void WriteAttribute(XmlTextWriter aWriter, string aAtrributeName, string aValue, bool aDefaultIsNegative=true)
    {
      aValue=aValue.ToLower();

      if (aDefaultIsNegative && (aValue.Equals("false") || aValue.Equals("disabled") || aValue.Equals("0")))
      {
        //No need to write anything as we are using defaults
        return;
      }

      if (!aDefaultIsNegative && (aValue.Equals("true") || aValue.Equals("enabled") || aValue.Equals("1")))
      {
        //No need to write anything as we are using defaults
        return;
      }


      aWriter.WriteAttributeString(aAtrributeName,aValue);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="aProfileName"></param>
    /// <param name="defaults"></param>
    private void LoadMapping(string aProfileName, bool defaults)
    {
      var pathDefault = HidProfiles.GetDefaultProfilePath(aProfileName);
      var pathCustom = HidProfiles.GetCustomProfilePath(aProfileName);

      try
      {
        groupBoxLayer.Enabled = false;
        groupBoxCondition.Enabled = false;
        groupBoxAction.Enabled = false;
        treeMapping.Nodes.Clear();
        var doc = new XmlDocument();
        var path = pathDefault;
        if (!defaults && File.Exists(pathCustom))
        {
          path = pathCustom;
        }
        if (!File.Exists(path))
        {
          MessageBox.Show(
            "Can't locate mapping file " + aProfileName + "\n\nMake sure it exists in /InputDeviceMappings/defaults",
            "Mapping file missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
          buttonUp.Enabled =
            buttonDown.Enabled =
              buttonNew.Enabled = buttonRemove.Enabled = buttonDefault.Enabled = buttonApply.Enabled = false;
          ShowInTaskbar = true;
          WindowState = FormWindowState.Minimized;
          var closeThread = new Thread(CloseThread);
          closeThread.Name = "InputMapperClose";
          closeThread.Start();
          return;
        }
        doc.Load(path);
        var listRemotes = doc.DocumentElement.SelectNodes("/HidHandler/HidUsageAction");

        foreach (XmlNode nodeRemote in listRemotes)
        {
          var usagePageAndCollection = nodeRemote.Attributes["UsagePage"].Value + "/" +
                                       nodeRemote.Attributes["UsageCollection"].Value;
          var remoteNode = new TreeNode(usagePageAndCollection);
          var huaAttributes = new HidUsageActionAttributes(nodeRemote.Attributes["UsagePage"].Value,
            nodeRemote.Attributes["UsageCollection"].Value,
            nodeRemote.Attributes["HandleHidEventsWhileInBackground"].Value);
          remoteNode.Tag = new NodeData("REMOTE", huaAttributes, null);
          var listButtons = nodeRemote.SelectNodes("button");
          foreach (XmlNode nodeButton in listButtons)
          {
            //Use code as name if no name attribute
            var buttonName = GetAttributeValue(nodeButton.Attributes["name"], nodeButton.Attributes["code"].Value);
            
            //Get background attribute and default to false
            var background = GetAttributeValue(nodeButton.Attributes["background"], "false");

            //Get repeat attribute and default to false
            var repeat = GetAttributeValue(nodeButton.Attributes["repeat"], "false");

            var shift = GetAttributeValue(nodeButton.Attributes["shift"], "false");
            var ctrl = GetAttributeValue(nodeButton.Attributes["ctrl"], "false");
            var alt = GetAttributeValue(nodeButton.Attributes["alt"], "false");
            var win = GetAttributeValue(nodeButton.Attributes["win"], "false");

            HidButtonAttributes hbAttributes = new HidButtonAttributes(buttonName, nodeButton.Attributes["code"].Value, background, repeat, shift, ctrl, alt, win);
            var buttonNode = new TreeNode(hbAttributes.GetText());            
            buttonNode.Tag = new NodeData("BUTTON", hbAttributes, null);
            remoteNode.Nodes.Add(buttonNode);

            var layer1Node = new TreeNode("Layer 1");
            var layer2Node = new TreeNode("Layer 2");
            var layerAllNode = new TreeNode("All Layers");
            layer1Node.Tag = new NodeData("LAYER", null, "1");
            layer2Node.Tag = new NodeData("LAYER", null, "2");
            layerAllNode.Tag = new NodeData("LAYER", null, "0");
            layer1Node.ForeColor = Color.DimGray;
            layer2Node.ForeColor = Color.DimGray;
            layerAllNode.ForeColor = Color.DimGray;

            var listActions = nodeButton.SelectNodes("action");

            foreach (XmlNode nodeAction in listActions)
            {
              var conditionString = string.Empty;
              var commandString = string.Empty;

              var condition = nodeAction.Attributes["condition"].Value.ToUpperInvariant();
              var conProperty = nodeAction.Attributes["conproperty"].Value.ToUpperInvariant();
              var command = nodeAction.Attributes["command"].Value.ToUpperInvariant();
              var cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpperInvariant();
              var sound = string.Empty;
              var soundAttribute = nodeAction.Attributes["sound"];
              if (soundAttribute != null)
              {
                sound = soundAttribute.Value;
              }
              var gainFocus = false;
              var focusAttribute = nodeAction.Attributes["focus"];
              if (focusAttribute != null)
              {
                gainFocus = Convert.ToBoolean(focusAttribute.Value);
              }
              var layer = Convert.ToInt32(nodeAction.Attributes["layer"].Value);

              #region Conditions

              switch (condition)
              {
                case "WINDOW":
                  conditionString =
                    GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(conProperty)));
                  if (string.IsNullOrEmpty(conditionString))
                  {
                    continue;
                  }
                  break;

                case "FULLSCREEN":
                  if (conProperty == "TRUE")
                  {
                    conditionString = "Fullscreen";
                  }
                  else
                  {
                    conditionString = "No Fullscreen";
                  }
                  break;

                case "PLAYER":
                  conditionString = playerList[Array.IndexOf(nativePlayerList, conProperty)];
                  break;

                case "*":
                  conditionString = "No Condition";
                  break;
              }

              #endregion Conditions

              #region Commands

              switch (command)
              {
                case "ACTION":
                  commandString = "Action \"" +
                                  GetFriendlyName(Enum.GetName(typeof (Action.ActionType), Convert.ToInt32(cmdProperty))) +
                                  "\"";
                  break;

                case "KEY":
                  commandString = "Key \"" + cmdProperty + "\"";
                  break;

                case "WINDOW":
                  commandString = "Window \"" +
                                  GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(cmdProperty))) +
                                  "\"";
                  break;

                case "TOGGLE":
                  commandString = "Toggle Layer";
                  break;

                case "POWER":
                  commandString = powerList[Array.IndexOf(nativePowerList, cmdProperty)];
                  break;

                case "PROCESS":
                  commandString = processList[Array.IndexOf(nativeProcessList, cmdProperty)];
                  break;
              }

              #endregion Commands

              var conditionNode = new TreeNode(conditionString);
              conditionNode.Tag = new NodeData("CONDITION", condition, conProperty);
              if (commandString == "Action \"Key Pressed\"")
              {
                var cmdKeyChar = nodeAction.Attributes["cmdkeychar"].Value;
                var cmdKeyCode = nodeAction.Attributes["cmdkeycode"].Value;
                var commandNode = new TreeNode(string.Format("Key Pressed: {0} [{1}]", cmdKeyChar, cmdKeyCode));

                var key = new Key(Convert.ToInt32(cmdKeyChar), Convert.ToInt32(cmdKeyCode));

                commandNode.Tag = new NodeData("COMMAND", "KEY", key, gainFocus);
                commandNode.ForeColor = Color.DarkGreen;
                conditionNode.ForeColor = Color.Blue;
                conditionNode.Nodes.Add(commandNode);
              }
              else
              {
                var commandNode = new TreeNode(commandString);
                commandNode.Tag = new NodeData("COMMAND", command, cmdProperty, gainFocus);
                commandNode.ForeColor = Color.DarkGreen;
                conditionNode.ForeColor = Color.Blue;
                conditionNode.Nodes.Add(commandNode);
              }

              var soundNode = new TreeNode(sound);
              soundNode.Tag = new NodeData("SOUND", null, sound);
              if (sound == string.Empty)
              {
                soundNode.Text = "No Sound";
              }
              soundNode.ForeColor = Color.DarkRed;
              conditionNode.Nodes.Add(soundNode);

              if (layer == 1)
              {
                layer1Node.Nodes.Add(conditionNode);
              }
              if (layer == 2)
              {
                layer2Node.Nodes.Add(conditionNode);
              }
              if (layer == 0)
              {
                layerAllNode.Nodes.Add(conditionNode);
              }
            }
            if (layer1Node.Nodes.Count > 0)
            {
              buttonNode.Nodes.Add(layer1Node);
            }
            if (layer2Node.Nodes.Count > 0)
            {
              buttonNode.Nodes.Add(layer2Node);
            }
            if (layerAllNode.Nodes.Count > 0)
            {
              buttonNode.Nodes.Add(layerAllNode);
            }
          }
          treeMapping.Nodes.Add(remoteNode);
          if (listRemotes.Count == 1)
          {
            remoteNode.Expand();
          }
        }
        changedSettings = false;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        //Force loading defaults if we were not already doing it
        if (!defaults)
        {
          //Possibly corrupted custom configuration
          //Try loading the defaults then
          LoadMapping("classic", true);
        }
        else
        {
          //Loading the default configuration failed
          //Just propagate our exception then
          throw ex;
        }        
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aProfileName"></param>
    /// <returns></returns>
    private bool SaveMapping(string aProfileName)
    {
      string pathDefault = HidProfiles.GetDefaultProfilePath(aProfileName);
      string pathCustom = HidProfiles.GetCustomProfilePath(aProfileName);

      //If default profile exists and custom profile was not yet created
      if (File.Exists(pathDefault) && !File.Exists(pathCustom))
      {
        //Prevent occluding default profiles
        string newProfileName = aProfileName + ".user";
        pathCustom = HidProfiles.GetCustomProfilePath(newProfileName);
        //Do not overwrite existing file after customizing default profile
        if (File.Exists(pathCustom))
        {
          //Make up a file name that does not exists for our new custom profile
          uint i = 0;
          do
          {
            i++;
            aProfileName = newProfileName + i.ToString();
            pathCustom = HidProfiles.GetCustomProfilePath(aProfileName);
          }
          while (File.Exists(pathCustom));        
        }
        else
        {
          aProfileName=newProfileName;
        }
      }

#if !DEBUG
      try
#endif
      {
        var dir = Directory.CreateDirectory(InputHandler.CustomizedMappingsDirectory);
      }
#if !DEBUG
      catch
      {
        Log.Info("MAP: Error accessing directory \"InputDeviceMappings\\custom\"");
      }

      //try
#endif
      {
        var writer = new XmlTextWriter(pathCustom, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 1;
        writer.IndentChar = (char) 9;
        writer.WriteStartDocument(true);
        writer.WriteStartElement("HidHandler"); // <mappings>
        writer.WriteAttributeString("version", "1");
        if (treeMapping.Nodes.Count > 0)
        {
          foreach (TreeNode remoteNode in treeMapping.Nodes)
          {
            writer.WriteStartElement("HidUsageAction"); // <remote>
            var uaAttributres = (HidUsageActionAttributes) ((NodeData) remoteNode.Tag).Parameter;
            writer.WriteAttributeString("UsagePage", uaAttributres.UsagePage);
            writer.WriteAttributeString("UsageCollection", uaAttributres.UsageCollection);
            writer.WriteAttributeString("HandleHidEventsWhileInBackground",
              uaAttributres.HandleHidEventsWhileInBackground);
            if (remoteNode.Nodes.Count > 0)
            {
              foreach (TreeNode buttonNode in remoteNode.Nodes)
              {
                writer.WriteStartElement("button"); // <button>
                var buttonAttributes = (HidButtonAttributes) ((NodeData) buttonNode.Tag).Parameter;
                if (buttonAttributes.Name != buttonAttributes.Code)
                {
                  //Only save the name if different from the code
                  writer.WriteAttributeString("name", buttonAttributes.Name);
                }

                //Save code no matter what
                writer.WriteAttributeString("code", buttonAttributes.Code);

                //Only save background handling if different from the defaults
                WriteAttribute(writer, "background", buttonAttributes.Background);

                //Only save repeat handling if different from the defaults
                WriteAttribute(writer, "repeat", buttonAttributes.Repeat);

                //Modifiers
                WriteAttribute(writer, "shift", buttonAttributes.ModifierShift);
                WriteAttribute(writer, "ctrl", buttonAttributes.ModifierControl);
                WriteAttribute(writer, "alt", buttonAttributes.ModifierAlt);
                WriteAttribute(writer, "win", buttonAttributes.ModifierWindows);

                if (buttonNode.Nodes.Count > 0)
                {
                  foreach (TreeNode layerNode in buttonNode.Nodes)
                  {
                    foreach (TreeNode conditionNode in layerNode.Nodes)
                    {
                      string layer;
                      string condition;
                      string conProperty;
                      var command = string.Empty;
                      var cmdProperty = string.Empty;
                      var cmdKeyChar = string.Empty;
                      var cmdKeyCode = string.Empty;
                      var sound = string.Empty;
                      var focus = false;
                      foreach (TreeNode commandNode in conditionNode.Nodes)
                      {
                        switch (((NodeData) commandNode.Tag).Type)
                        {
                          case "COMMAND":
                          {
                            command = (string) ((NodeData) commandNode.Tag).Parameter;
                            focus = ((NodeData) commandNode.Tag).Focus;
                            if (command != "KEY")
                            {
                              cmdProperty = ((NodeData) commandNode.Tag).Value.ToString();
                            }
                            else
                            {
                              command = "ACTION";
                              var key = (Key) ((NodeData) commandNode.Tag).Value;
                              cmdProperty = "93";
                              cmdKeyChar = key.KeyChar.ToString();
                              cmdKeyCode = key.KeyCode.ToString();
                            }
                          }
                            break;

                          case "SOUND":
                            sound = (string) ((NodeData) commandNode.Tag).Value;
                            break;
                        }
                      }
                      condition = (string) ((NodeData) conditionNode.Tag).Parameter;
                      conProperty = ((NodeData) conditionNode.Tag).Value.ToString();
                      layer = Convert.ToString(((NodeData) layerNode.Tag).Value);
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
                      if (sound != string.Empty)
                      {
                        writer.WriteAttributeString("sound", sound);
                      }
                      if (focus)
                      {
                        writer.WriteAttributeString("focus", focus.ToString());
                      }
                      writer.WriteEndElement(); // </action>
                    }
                  }
                }
                writer.WriteEndElement(); // </button>
              }
            }
            writer.WriteEndElement(); // </remote>
          }
        }
        writer.WriteEndElement(); // </mapping>
        writer.WriteEndDocument();
        writer.Close();
        changedSettings = false;
        ProfileName = aProfileName;
        return true;
      }
#if !DEBUG
  //catch (Exception ex)
  //{
  //  Log.Info("MAP: Error saving mapping to XML file: {0}", ex.Message);
  //  return false;
  //}
#endif
    }

    private TreeNode getNode(string type)
    {
      var node = treeMapping.SelectedNode;
      var data = (NodeData) node.Tag;
      if (data.Type == type)
      {
        return node;
      }

      #region Find Node

      switch (type)
      {
        case "COMMAND":
          if ((data.Type == "SOUND") || (data.Type == "KEY"))
          {
            node = node.Parent;
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (NodeData) subNode.Tag;
              if (data.Type == type)
              {
                return subNode;
              }
            }
          }
          else if (data.Type == "CONDITION")
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (NodeData) subNode.Tag;
              if (data.Type == type)
              {
                return subNode;
              }
            }
          }
          break;

        case "SOUND":
          if ((data.Type == "COMMAND") || (data.Type == "KEY"))
          {
            node = node.Parent;
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (NodeData) subNode.Tag;
              if (data.Type == type)
              {
                return subNode;
              }
            }
          }
          else if (data.Type == "CONDITION")
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (NodeData) subNode.Tag;
              if (data.Type == type)
              {
                return subNode;
              }
            }
          }
          break;

        case "CONDITION":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
          {
            return node.Parent;
          }
          break;

        case "LAYER":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
          {
            return node.Parent.Parent;
          }
          if (data.Type == "CONDITION")
          {
            return node.Parent;
          }
          break;

        case "BUTTON":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
          {
            return node.Parent.Parent.Parent;
          }
          if (data.Type == "CONDITION")
          {
            return node.Parent.Parent;
          }
          if (data.Type == "LAYER")
          {
            return node.Parent;
          }
          break;

        case "REMOTE":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
          {
            return node.Parent.Parent.Parent.Parent;
          }
          if (data.Type == "CONDITION")
          {
            return node.Parent.Parent.Parent;
          }
          if (data.Type == "LAYER")
          {
            return node.Parent.Parent;
          }
          if (data.Type == "BUTTON")
          {
            return node.Parent;
          }
          break;
      }

      #endregion Find Node

      return null;
    }

    private void treeMapping_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (e.Action == TreeViewAction.Unknown)
      {
        return;
      }

      var node = e.Node;
      NodeData data = (NodeData)node.Tag;
      switch (data.Type)
      {
        case "REMOTE":
          groupBoxLayer.Enabled = false;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          groupBoxButton.Enabled = false;
          comboBoxLayer.Text = "All Layers";
          comboBoxCondProperty.Text = "none";
          comboBoxCmdProperty.Text = "none";
          comboBoxSound.Text = "none";
          return;

        case "BUTTON":          
          groupBoxLayer.Enabled = false;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          groupBoxButton.Enabled = true;
          comboBoxLayer.Text = "All Layers";
          comboBoxCondProperty.Text = "none";
          comboBoxCmdProperty.Text = "none";
          comboBoxSound.Text = "none";
          HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
          mpCheckBoxAlt.Checked = HidUsageAction.AttributeValueToBoolean(attributes.ModifierAlt);
          mpCheckBoxShift.Checked = HidUsageAction.AttributeValueToBoolean(attributes.ModifierShift);
          mpCheckBoxControl.Checked = HidUsageAction.AttributeValueToBoolean(attributes.ModifierControl);
          mpCheckBoxWindows.Checked = HidUsageAction.AttributeValueToBoolean(attributes.ModifierWindows);
          mpCheckBoxRepeat.Checked = HidUsageAction.AttributeValueToBoolean(attributes.Repeat);
          mpCheckBoxBackground.Checked = HidUsageAction.AttributeValueToBoolean(attributes.Background);

          //Populate our code combo box
          mpComboBoxCode.Items.Clear();
          mpComboBoxCode.Text = "";
          
          //Check our usage page and collection
          TreeNode remoteNode = getNode("REMOTE");
          NodeData remoteData = (NodeData)remoteNode.Tag;
          HidUsageActionAttributes remoteAttributes = (HidUsageActionAttributes) remoteData.Parameter;

          if (remoteAttributes.UsagePage.Equals(SharpLib.Hid.UsagePage.GenericDesktopControls.ToString()) &&
              remoteAttributes.UsageCollection.Equals(SharpLib.Hid.UsageCollection.GenericDesktop.Keyboard.ToString()))
          {
            // Only supporting code selection for keyboard for now
            Type typeOfCode = typeof(Keys);
            foreach (string keyName in Enum.GetNames(typeOfCode))
            {
              Keys value = (Keys) Enum.Parse(typeOfCode, keyName);
              if (value > Keys.None && value < Keys.KeyCode)
              {
                mpComboBoxCode.Items.Add(keyName);
              }
            }

            mpComboBoxCode.Text = attributes.Code;
            mpComboBoxCode.Enabled = true;
            mpCheckBoxShift.Enabled = true;
            mpCheckBoxControl.Enabled = true;
            mpCheckBoxAlt.Enabled = true;
            mpCheckBoxWindows.Enabled = true;
          }
          else
          {
            //Disable code and modifiers check boxes for the rest
            mpComboBoxCode.Enabled = false;
            mpCheckBoxShift.Enabled = false;
            mpCheckBoxControl.Enabled = false;
            mpCheckBoxAlt.Enabled = false;
            mpCheckBoxWindows.Enabled = false;
          }


          return;

        case "LAYER":
          groupBoxLayer.Enabled = true;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          groupBoxButton.Enabled = false;
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
          groupBoxButton.Enabled = false;
          groupBoxCondition.Enabled = true;
          groupBoxAction.Enabled = true;
          groupBoxLayer.Enabled = true;
          if ((data.Type == "COMMAND") || (data.Type == "SOUND"))
          {
            comboBoxLayer.SelectedIndex = Convert.ToInt32(((NodeData) node.Parent.Parent.Tag).Value);
            node = node.Parent;
            data = (NodeData) node.Tag;
          }
          else
          {
            comboBoxLayer.SelectedIndex = Convert.ToInt32(((NodeData) node.Parent.Tag).Value);
          }

          switch ((string) data.Parameter)
          {
            case "WINDOW":
              radioButtonWindow.Checked = true;
              comboBoxCondProperty.Enabled = true;
              UpdateCombo(ref comboBoxCondProperty, windowsList,
                GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(data.Value))));
              break;

            case "FULLSCREEN":
              radioButtonFullscreen.Checked = true;
              comboBoxCondProperty.Enabled = true;
              if (Convert.ToBoolean(data.Value))
              {
                UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
              }
              else
              {
                UpdateCombo(ref comboBoxCondProperty, fullScreenList, "No Fullscreen");
              }
              break;

            case "PLAYER":
              radioButtonPlaying.Checked = true;
              comboBoxCondProperty.Enabled = true;
              UpdateCombo(ref comboBoxCondProperty, playerList,
                playerList[Array.IndexOf(nativePlayerList, (string) data.Value)]);
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
            data = (NodeData) typeNode.Tag;
            switch (data.Type)
            {
              case "SOUND":
                if ((string) data.Value != string.Empty)
                {
                  comboBoxSound.SelectedItem = data.Value;
                }
                else
                {
                  comboBoxSound.SelectedItem = "none";
                }
                break;

              case "COMMAND":
                checkBoxGainFocus.Checked = data.Focus;
                switch ((string) data.Parameter)
                {
                  case "ACTION":
                    comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                    radioButtonAction.Checked = true;
                    comboBoxSound.Enabled = true;
                    comboBoxCmdProperty.Enabled = true;
                    textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                    textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
                    UpdateCombo(ref comboBoxCmdProperty, actionList,
                      GetFriendlyName(Enum.GetName(typeof (Action.ActionType), Convert.ToInt32(data.Value))));
                    break;

                  case "KEY":
                    comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                    radioButtonAction.Checked = true;
                    textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = true;
                    textBoxKeyChar.Text = ((Key) data.Value).KeyChar.ToString();
                    textBoxKeyCode.Text = ((Key) data.Value).KeyCode.ToString();
                    comboBoxCmdProperty.Enabled = true;
                    UpdateCombo(ref comboBoxCmdProperty, actionList, "Key Pressed");
                    break;

                  case "WINDOW":
                    comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                    radioButtonActWindow.Checked = true;
                    comboBoxSound.Enabled = true;
                    comboBoxCmdProperty.Enabled = true;
                    textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                    textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
                    UpdateCombo(ref comboBoxCmdProperty, windowsListFiltered,
                      GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(data.Value))));
                    break;

                  case "TOGGLE":
                    radioButtonToggle.Checked = true;
                    comboBoxSound.Enabled = true;
                    comboBoxCmdProperty.Enabled = false;
                    textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                    textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
                    comboBoxCmdProperty.Items.Clear();
                    comboBoxCmdProperty.Text = string.Empty;
                    break;

                  case "POWER":
                    comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                    radioButtonPower.Checked = true;
                    comboBoxSound.Enabled = true;
                    comboBoxCmdProperty.Enabled = true;
                    textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                    textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
                    var friendlyName = string.Empty;
                    UpdateCombo(ref comboBoxCmdProperty, powerList,
                      powerList[Array.IndexOf(nativePowerList, (string) data.Value)]);
                    break;

                  case "PROCESS":
                    comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                    radioButtonProcess.Checked = true;
                    comboBoxSound.Enabled = true;
                    comboBoxCmdProperty.Enabled = true;
                    textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                    textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
                    UpdateCombo(ref comboBoxCmdProperty, processList,
                      processList[Array.IndexOf(nativeProcessList, (string) data.Value)]);
                    break;
                }
                break;
            }
          }
        }
          break;
      }
    }

    private void UpdateCombo(ref MPComboBox comboBox, Array list, string hilight)
    {
      comboBox.Items.Clear();
      foreach (var item in list)
      {
        comboBox.Items.Add(item.ToString());
      }
      comboBox.Text = hilight;
      comboBox.SelectedItem = hilight;
      comboBox.Enabled = true;
    }

    private void UpdateCombo(ref MPComboBox comboBox, ArrayList list, string hilight)
    {
      UpdateCombo(ref comboBox, list.ToArray(), hilight);
    }

    private string GetFriendlyName(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return string.Empty;
      }

      if ((name.IndexOf("ACTION") != -1) || (name.IndexOf("WINDOW") != -1))
      {
        name = name.Substring(7);
      }

      var upcase = true;
      var newName = string.Empty;

      foreach (var c in name)
      {
        if (c == '_')
        {
          newName += " ";
          upcase = true;
        }
        else if (upcase)
        {
          newName += c.ToString();
          upcase = false;
        }
        else
        {
          newName += c.ToString().ToLowerInvariant();
        }
      }

      CleanAbbreviation(ref newName, "TV");
      CleanAbbreviation(ref newName, "DVD");
      CleanAbbreviation(ref newName, "UI");
      CleanAbbreviation(ref newName, "Guide");
      CleanAbbreviation(ref newName, "MSN");
      CleanAbbreviation(ref newName, "OSD");
      CleanAbbreviation(ref newName, "LCD");
      CleanAbbreviation(ref newName, "EPG");
      CleanAbbreviation(ref newName, "DVBC");
      CleanAbbreviation(ref newName, "DVBS");
      CleanAbbreviation(ref newName, "DVBT");

      return newName;
    }

    private string GetWindowName(string friendlyName)
    {
      return "WINDOW_" + friendlyName.Replace(' ', '_').ToUpperInvariant();
    }

    private string GetActionName(string friendlyName)
    {
      var actionName = string.Empty;

      try
      {
        if (Enum.Parse(typeof (Action.ActionType), "ACTION_" + friendlyName.Replace(' ', '_').ToUpperInvariant()) !=
            null)
        {
          actionName = "ACTION_" + friendlyName.Replace(' ', '_').ToUpperInvariant();
        }
      }
      catch (ArgumentException)
      {
        try
        {
          if (Enum.Parse(typeof (Action.ActionType), friendlyName.Replace(' ', '_').ToUpperInvariant()) != null)
          {
            actionName = friendlyName.Replace(' ', '_').ToUpperInvariant();
          }
        }
        catch (ArgumentException)
        {
        }
      }

      return actionName;
    }

    private void CleanAbbreviation(ref string name, string abbreviation)
    {
      var index = name.ToUpperInvariant().IndexOf(abbreviation.ToUpperInvariant());
      if (index != -1)
      {
        name = name.Substring(0, index) + abbreviation + name.Substring(index + abbreviation.Length);
      }
    }

    private void radioButtonWindow_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      var node = getNode("CONDITION");
      node.Tag = new NodeData("CONDITION", "WINDOW", "0");
      UpdateCombo(ref comboBoxCondProperty, windowsList, GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), 0)));
      node.Text = (string) comboBoxCondProperty.SelectedItem;
      changedSettings = true;
    }

    private void radioButtonFullscreen_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      var node = getNode("CONDITION");
      node.Tag = new NodeData("CONDITION", "FULLSCREEN", "true");
      UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
      node.Text = (string) comboBoxCondProperty.SelectedItem;
      changedSettings = true;
    }

    private void radioButtonPlaying_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.Enabled = true;
      var node = getNode("CONDITION");
      node.Tag = new NodeData("CONDITION", "PLAYER", "TV");
      node.Text = playerList[0];
      UpdateCombo(ref comboBoxCondProperty, playerList, playerList[0]);
      changedSettings = true;
    }

    private void radioButtonNoCondition_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.Enabled = false;
      comboBoxCondProperty.Items.Clear();
      comboBoxCondProperty.Text = "none";
      var node = getNode("CONDITION");
      node.Tag = new NodeData("CONDITION", "*", null);
      node.Text = "No Condition";
      changedSettings = true;
    }

    private void radioButtonAction_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      var node = getNode("COMMAND");
      var data = new NodeData("COMMAND", "ACTION", "7");
      node.Tag = data;
      UpdateCombo(ref comboBoxCmdProperty, actionList,
        GetFriendlyName(Enum.GetName(typeof (Action.ActionType), Convert.ToInt32(data.Value))));
      node.Text = "Action \"" + (string) comboBoxCmdProperty.SelectedItem + "\"";
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonActWindow_Click(object sender, EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      var node = getNode("COMMAND");
      var data = new NodeData("COMMAND", "WINDOW", "0");
      node.Tag = data;
      UpdateCombo(ref comboBoxCmdProperty, windowsListFiltered,
        GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(data.Value))));
      node.Text = "Window \"" + (string) comboBoxCmdProperty.SelectedItem + "\"";
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonToggle_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = false;
      comboBoxCmdProperty.Items.Clear();
      comboBoxCmdProperty.Text = "none";
      var node = getNode("COMMAND");
      var data = new NodeData("COMMAND", "TOGGLE", "-1");
      node.Tag = data;
      node.Text = "Toggle Layer";
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonPower_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      var node = getNode("COMMAND");
      node.Tag = new NodeData("COMMAND", "POWER", "EXIT");
      node.Text = powerList[0];
      UpdateCombo(ref comboBoxCmdProperty, powerList, powerList[0]);
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonProcess_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      var node = getNode("COMMAND");
      node.Tag = new NodeData("COMMAND", "PROCESS", "CLOSE");
      node.Text = processList[0];
      UpdateCombo(ref comboBoxCmdProperty, processList, processList[0]);
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      if (changedSettings)
      {
        SaveMapping(ProfileName);
      }
      Close();
    }

    private void buttonApply_Click(object sender, EventArgs e)
    {
      if (changedSettings)
      {
        SaveMapping(ProfileName);
      }
    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      var expanded = false;
      var node = treeMapping.SelectedNode;
      if (((NodeData) node.Tag).Type != "BUTTON")
      {
        expanded = node.IsExpanded;
      }
      if ((((NodeData) node.Tag).Type == "COMMAND") || (((NodeData) node.Tag).Type == "SOUND"))
      {
        node = node.Parent;
        expanded = true;
      }
      if ((((NodeData) node.Tag).Type != "BUTTON") && (((NodeData) node.Tag).Type != "CONDITION"))
      {
        return;
      }
      if (node.Index > 0)
      {
        var index = node.Index - 1;
        var tmpNode = (TreeNode) node.Clone();
        var parentNode = node.Parent;
        node.Remove();
        if (expanded)
        {
          tmpNode.Expand();
        }
        parentNode.Nodes.Insert(index, tmpNode);
        treeMapping.SelectedNode = tmpNode;
      }
      changedSettings = true;
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      var expanded = false;
      var node = treeMapping.SelectedNode;
      if (((NodeData) node.Tag).Type != "BUTTON")
      {
        expanded = node.IsExpanded;
      }
      if ((((NodeData) node.Tag).Type == "COMMAND") || (((NodeData) node.Tag).Type == "SOUND"))
      {
        node = node.Parent;
        expanded = true;
      }
      if ((((NodeData) node.Tag).Type != "BUTTON") && (((NodeData) node.Tag).Type != "CONDITION"))
      {
        return;
      }
      if (node.Index < node.Parent.Nodes.Count - 1)
      {
        var index = node.Index + 1;
        var tmpNode = (TreeNode) node.Clone();
        var parentNode = node.Parent;
        node.Remove();
        if (expanded)
        {
          tmpNode.Expand();
        }
        parentNode.Nodes.Insert(index, tmpNode);
        treeMapping.SelectedNode = tmpNode;
      }
      changedSettings = true;
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
      var node = treeMapping.SelectedNode;
      var data = (NodeData) node.Tag;
      if ((data.Type == "COMMAND") || (data.Type == "SOUND") || (data.Type == "CONDITION"))
      {
        node = getNode("CONDITION");
        data = (NodeData) node.Tag;
      }
      var result = MessageBox.Show(this, "Are you sure you want to remove this " + data.Type.ToLowerInvariant() + "?",
        "Remove " + data.Type.ToLowerInvariant(),
        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
      if (result == DialogResult.Yes)
      {
        node.Remove();
        changedSettings = true;
      }
    }

    private void buttonNew_Click(object sender, EventArgs e)
    {
      var node = treeMapping.SelectedNode;
      var data = (NodeData) node.Tag;

      var newLayer = new TreeNode("All Layers");
      newLayer.Tag = new NodeData("LAYER", null, "0");
      newLayer.ForeColor = Color.DimGray;

      var newCondition = new TreeNode("No Condition");
      newCondition.Tag = new NodeData("CONDITION", "*", "-1");
      newCondition.ForeColor = Color.Blue;

      var newCommand = new TreeNode("Action \"Select Item\"");
      newCommand.Tag = new NodeData("COMMAND", "ACTION", "7");
      newCommand.ForeColor = Color.DarkGreen;

      var newSound = new TreeNode("No Sound");
      newSound.Tag = new NodeData("SOUND", string.Empty, string.Empty);
      newSound.ForeColor = Color.DarkRed;

      HidButtonAttributes newButtonAttributes = new HidButtonAttributes(Keys.A.ToString(), Keys.A.ToString(), false.ToString(), false.ToString(), false.ToString(), false.ToString(), false.ToString(), false.ToString());
      var newButtonNode = new TreeNode(newButtonAttributes.GetText());
      newButtonNode.Tag = new NodeData("BUTTON", newButtonAttributes, null);

      switch (data.Type)
      {
        case "REMOTE":
        {
          //Check that the selected "remote" is a keyboard
          HidUsageActionAttributes remoteAttributes = (HidUsageActionAttributes) data.Parameter;
          if (remoteAttributes.UsagePage.Equals(SharpLib.Hid.UsagePage.GenericDesktopControls.ToString()) &&
              remoteAttributes.UsageCollection.Equals(SharpLib.Hid.UsageCollection.GenericDesktop.Keyboard.ToString()))
          {
            //We support adding new buttons to keyboards
            newCondition.Nodes.Add(newCommand);
            newCondition.Nodes.Add(newSound);
            newLayer.Nodes.Add(newCondition);
            newButtonNode.Nodes.Add(newLayer);
            node.Nodes.Add(newButtonNode);
            newButtonNode.ExpandAll();
            treeMapping.SelectedNode = newButtonNode;
          }
        }
          break;
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

        default:
          //NewButtonForm newButtonForm = new NewButtonForm();
          //newButtonForm.ShowDialog();
          //if (newButtonForm.Accepted)
          //{
          //  Log.Info("Name: {0}", newButtonForm.ButtonName);
          //  Log.Info("Code: {0}", newButtonForm.ButtonCode);
          //}
          break;
      }
      changedSettings = true;

      treeMapping_AfterSelect(this, new TreeViewEventArgs(treeMapping.SelectedNode, TreeViewAction.ByKeyboard));
    }

    private void buttonDefault_Click(object sender, EventArgs e)
    {
      var pathCustom = Path.Combine(InputHandler.CustomizedMappingsDirectory, ProfileName );
      if (File.Exists(pathCustom))
      {
        File.Delete(pathCustom);
      }
      LoadMapping(ProfileName, true);
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
      var node = getNode("LAYER");
      node.Tag = new NodeData("LAYER", null, comboBoxLayer.SelectedIndex);
      if (comboBoxLayer.SelectedIndex == 0)
      {
        node.Text = "All Layers";
      }
      else
      {
        node.Text = "Layer " + comboBoxLayer.SelectedIndex;
      }
      changedSettings = true;
    }

    private void comboBoxCondProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      var node = getNode("CONDITION");
      var data = (NodeData) node.Tag;
      switch ((string) data.Parameter)
      {
        case "WINDOW":
          node.Tag = new NodeData("CONDITION", "WINDOW",
            (int)
              Enum.Parse(typeof (GUIWindow.Window),
                GetWindowName((string) comboBoxCondProperty.SelectedItem)));
          node.Text = (string) comboBoxCondProperty.SelectedItem;
          break;

        case "FULLSCREEN":
          if ((string) comboBoxCondProperty.SelectedItem == "Fullscreen")
          {
            node.Tag = new NodeData("CONDITION", "FULLSCREEN", "true");
          }
          else
          {
            node.Tag = new NodeData("CONDITION", "FULLSCREEN", "false");
          }
          node.Text = (string) comboBoxCondProperty.SelectedItem;
          break;

        case "PLAYER":
        {
          node.Tag = new NodeData("CONDITION", "PLAYER",
            nativePlayerList[Array.IndexOf(playerList, (string) comboBoxCondProperty.SelectedItem)]);
          node.Text = (string) comboBoxCondProperty.SelectedItem;
          break;
        }
        case "*":
          break;
      }
      changedSettings = true;
    }

    private void comboBoxCmdProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if ((string) comboBoxCmdProperty.SelectedItem == "Key Pressed")
      {
        textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = true;
      }
      else
      {
        textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
        textBoxKeyChar.Text = textBoxKeyCode.Text = string.Empty;
      }

      var node = getNode("COMMAND");
      var data = (NodeData) node.Tag;
      switch ((string) data.Parameter)
      {
        case "ACTION":
          if ((string) comboBoxCmdProperty.SelectedItem != "Key Pressed")
          {
            node.Tag = new NodeData("COMMAND", "ACTION",
              (int)
                Enum.Parse(typeof (Action.ActionType),
                  GetActionName((string) comboBoxCmdProperty.SelectedItem)));
            node.Text = "Action \"" + (string) comboBoxCmdProperty.SelectedItem + "\"";
          }
          else
          {
            textBoxKeyChar.Text = "0";
            textBoxKeyCode.Text = "0";
            var key = new Key(Convert.ToInt32(textBoxKeyChar.Text), Convert.ToInt32(textBoxKeyCode.Text));
            node.Tag = new NodeData("COMMAND", "KEY", key);
            node.Text = string.Format("Key Pressed: {0} [{1}]", textBoxKeyChar.Text, textBoxKeyCode.Text);
          }
          break;

        case "WINDOW":
          node.Tag = new NodeData("COMMAND", "WINDOW",
            (int)
              Enum.Parse(typeof (GUIWindow.Window),
                GetWindowName((string) comboBoxCmdProperty.SelectedItem)));
          node.Text = "Window \"" + (string) comboBoxCmdProperty.SelectedItem + "\"";
          break;

        case "POWER":
          node.Tag = new NodeData("COMMAND", "POWER",
            nativePowerList[Array.IndexOf(powerList, (string) comboBoxCmdProperty.SelectedItem)]);
          node.Text = (string) comboBoxCmdProperty.SelectedItem;
          break;

        case "PROCESS":
          node.Tag = new NodeData("COMMAND", "PROCESS",
            nativeProcessList[Array.IndexOf(processList, (string) comboBoxCmdProperty.SelectedItem)]);
          node.Text = (string) comboBoxCmdProperty.SelectedItem;
          break;
      }
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void comboBoxSound_SelectionChangeCommitted(object sender, EventArgs e)
    {
      var node = getNode("SOUND");
      node.Text = (string) comboBoxSound.SelectedItem;
      if (node.Text == "none")
      {
        node.Tag = new NodeData("SOUND", null, string.Empty);
        node.Text = "No Sound";
      }
      else
      {
        node.Tag = new NodeData("SOUND", null, (string) comboBoxSound.SelectedItem);
      }
      changedSettings = true;
    }

    private void textBoxKeyChar_KeyUp(object sender, KeyEventArgs e)
    {
      var keyChar = textBoxKeyChar.Text;
      var keyCode = textBoxKeyCode.Text;
      var node = getNode("COMMAND");
      if (keyChar == string.Empty)
      {
        keyChar = "0";
      }
      if (keyCode == string.Empty)
      {
        keyCode = "0";
      }
      var key = new Key(Convert.ToInt32(keyChar), Convert.ToInt32(keyCode));
      node.Tag = new NodeData("COMMAND", "KEY", key);
      node.Text = string.Format("Key Pressed: {0} [{1}]", keyChar, keyCode);
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void textBoxKeyCode_KeyUp(object sender, KeyEventArgs e)
    {
      textBoxKeyChar_KeyUp(sender, e);
    }

    private void labelExpand_Click(object sender, EventArgs e)
    {
      if (treeMapping.SelectedNode == null)
      {
        treeMapping.Select();
      }
      treeMapping.SelectedNode.ExpandAll();
    }

    private void checkBoxGainFocus_CheckedChanged(object sender, EventArgs e)
    {
      var node = getNode("COMMAND");
      ((NodeData) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    /// <summary>
    ///   Store data in our TreeNode to be able to put it back in our XML when saving.
    /// </summary>
    private class HidUsageActionAttributes
    {
      public HidUsageActionAttributes(string aUsagePage, string aUsageCollection, string aBackground)
      {
        UsagePage = aUsagePage;
        UsageCollection = aUsageCollection;
        HandleHidEventsWhileInBackground = aBackground;
      }

      public string UsagePage { get; private set; }
      public string UsageCollection { get; private set; }
      public string HandleHidEventsWhileInBackground { get; private set; }
    }

    /// <summary>
    ///   Store data in our TreeNode to be able to put it back in our XML when saving.
    /// </summary>
    private class HidButtonAttributes
    {
      public HidButtonAttributes(string aName, string aCode, string aBackground, string aRepeat, string aShift, string aControl, string aAlt, string aWindows)
      {
        Name = aName;
        Code = aCode;
        Background = aBackground;
        Repeat = aRepeat;
        ModifierShift = aShift;
        ModifierControl = aControl;
        ModifierAlt = aAlt;
        ModifierWindows = aWindows;
      }

      public string Name { get; set; }
      public string Code { get; set; }
      public string Background { get; set; }
      public string Repeat { get; set; }
      public string ModifierShift { get; set; }
      public string ModifierControl { get; set; }
      public string ModifierAlt { get; set; }
      public string ModifierWindows { get; set; }

      public string GetText()
      {
        if (Name != Code)
        {
          return Name;
        }

        //Build a neat name
        string name = Code;
        if (HidUsageAction.AttributeValueToBoolean(ModifierShift))
        {
          name += " + SHIFT";
        }

        if (HidUsageAction.AttributeValueToBoolean(ModifierControl))
        {
          name += " + CTRL";
        }

        if (HidUsageAction.AttributeValueToBoolean(ModifierAlt))
        {
          name += " + ALT";
        }

        if (HidUsageAction.AttributeValueToBoolean(ModifierWindows))
        {
          name += " + WIN";
        }

        if (HidUsageAction.AttributeValueToBoolean(Repeat) || HidUsageAction.AttributeValueToBoolean(Background))
        {
          name += " ( ";
          bool needSeparator = false;
          if (HidUsageAction.AttributeValueToBoolean(Background))
          {
            name += "background";
            needSeparator = true;
          }

          if (HidUsageAction.AttributeValueToBoolean(Repeat))
          {
            if (needSeparator)
            {
              name += ", ";
            }
            name += "repeat";
          }

          name += " )";
        }

        return name;
      }
    }

    private class NodeData
    {
      public NodeData(object newType, object newParameter, object newValue)
      {
        Focus = false;
        if (newValue == null)
        {
          newValue = string.Empty;
        }
        if (newParameter == null)
        {
          newParameter = string.Empty;
        }
        Type = (string) newType;
        Value = newValue;
        Parameter = newParameter;
      }

      public NodeData(object newType, object newParameter, object newValue, bool newFocus)
      {
        if (newValue == null)
        {
          newValue = string.Empty;
        }
        if (newParameter == null)
        {
          newParameter = string.Empty;
        }
        Type = (string) newType;
        Value = newValue;
        Parameter = newParameter;
        Focus = newFocus;
      }

      public string Type { get; private set; }
      public object Value { get; set; }
      public object Parameter { get; set; }
      public bool Focus { get; set; }
    }

    #region Controls

    private TreeView treeMapping;
    private MPRadioButton radioButtonWindow;
    private MPRadioButton radioButtonFullscreen;
    private MPRadioButton radioButtonPlaying;
    private MPRadioButton radioButtonNoCondition;
    private MPComboBox comboBoxCondProperty;
    private MPComboBox comboBoxCmdProperty;
    private MPGroupBox groupBoxCondition;
    private MPRadioButton radioButtonAction;
    private MPRadioButton radioButtonActWindow;
    private MPRadioButton radioButtonToggle;
    private MPRadioButton radioButtonPower;
    private MPComboBox comboBoxSound;
    private MPLabel labelSound;
    private MPGroupBox groupBoxAction;
    private MPGradientLabel headerLabel;
    private MPButton buttonApply;
    private MPButton buttonOk;
    private MPButton buttonCancel;
    private MPBeveledLine beveledLine1;
    private MPLabel labelLayer;
    private MPComboBox comboBoxLayer;
    private MPButton buttonUp;
    private MPButton buttonDown;
    private MPButton buttonRemove;
    private MPButton buttonDefault;
    private MPGroupBox groupBoxLayer;
    private MPRadioButton radioButtonProcess;
    private MPLabel label1;
    private MPTextBox textBoxKeyChar;
    private MPTextBox textBoxKeyCode;
    private MPLabel labelExpand;
    private MPCheckBox checkBoxGainFocus;

    #endregion Controls

    private void mpCheckBoxRepeat_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes) data.Parameter;
      attributes.Repeat = ((CheckBox) sender).Checked.ToString();
      node.Text = attributes.GetText();
      changedSettings = true;
    }

    private void mpCheckBoxBackground_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
      attributes.Background = ((CheckBox)sender).Checked.ToString();
      node.Text = attributes.GetText();
      changedSettings = true;
    }

    private void mpCheckBoxShift_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
      attributes.ModifierShift = ((CheckBox)sender).Checked.ToString();
      node.Text = attributes.GetText();
      changedSettings = true;
    }

    private void mpCheckBoxControl_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
      attributes.ModifierControl = ((CheckBox)sender).Checked.ToString();
      node.Text = attributes.GetText();
      changedSettings = true;
    }

    private void mpCheckBoxWindows_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
      attributes.ModifierWindows = ((CheckBox)sender).Checked.ToString();
      node.Text = attributes.GetText();
      changedSettings = true;
    }

    private void mpCheckBoxAlt_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
      attributes.ModifierAlt = ((CheckBox)sender).Checked.ToString();
      node.Text = attributes.GetText();
      changedSettings = true;
    }

    private void mpComboBoxCode_SelectedIndexChanged(object sender, EventArgs e)
    {
      //Button code was changed
      TreeNode node = getNode("BUTTON");
      NodeData data = (NodeData)node.Tag;
      HidButtonAttributes attributes = (HidButtonAttributes)data.Parameter;
      if (attributes.Code.Equals(attributes.Name))
      {
        //Change both code and name if they are already in sync
        attributes.Code = ((ComboBox) sender).Text;
        attributes.Name = ((ComboBox)sender).Text;
      }
      else
      {
        //Change only the code then
        attributes.Code = ((ComboBox)sender).Text;
      }

      //Update text in our tree
      node.Text = attributes.GetText();
      // We will need to save
      changedSettings = true;
    }

  }
}