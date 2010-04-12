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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;

using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.IR
{
  /// <summary>
  /// Summary description for USBUIRTLearnForm.
  /// </summary>
  public class USBUIRTLearnForm : System.Windows.Forms.Form
  {
    public enum LearnType
    {
      SetTopBoxCommands,
      MediaPortalCommands
    } ;

    private System.Collections.ArrayList buttonNames = new ArrayList();

    #region Command Actions

    private object[] commands = {
                                  Action.ActionType.ACTION_MOVE_LEFT,
                                  Action.ActionType.ACTION_MOVE_RIGHT,
                                  Action.ActionType.ACTION_MOVE_UP,
                                  Action.ActionType.ACTION_MOVE_DOWN,
                                  Action.ActionType.ACTION_PAGE_UP,
                                  Action.ActionType.ACTION_PAGE_DOWN,
                                  Action.ActionType.ACTION_SELECT_ITEM,
                                  Action.ActionType.ACTION_PREVIOUS_MENU,
                                  Action.ActionType.ACTION_SHOW_INFO,
                                  Action.ActionType.ACTION_PLAY,
                                  Action.ActionType.ACTION_PAUSE,
                                  Action.ActionType.ACTION_STOP,
                                  Action.ActionType.ACTION_RECORD,
                                  Action.ActionType.ACTION_FORWARD,
                                  Action.ActionType.ACTION_REWIND,
                                  Action.ActionType.ACTION_CONTEXT_MENU,
                                  Action.ActionType.ACTION_SHOW_GUI,
                                  Action.ActionType.ACTION_QUEUE_ITEM,
                                  Action.ActionType.ACTION_EXIT,
                                  Action.ActionType.ACTION_SHUTDOWN,
                                  Action.ActionType.ACTION_REBOOT,
                                  Action.ActionType.ACTION_ASPECT_RATIO,
                                  Action.ActionType.ACTION_EJECTCD,
                                  Action.ActionType.ACTION_PREV_CHANNEL,
                                  Action.ActionType.ACTION_NEXT_CHANNEL,
                                  Action.ActionType.ACTION_DVD_MENU,
                                  Action.ActionType.ACTION_NEXT_CHAPTER,
                                  Action.ActionType.ACTION_PREV_CHAPTER,
                                  Action.ActionType.ACTION_VOLUME_DOWN,
                                  Action.ActionType.ACTION_VOLUME_UP,
                                  Action.ActionType.ACTION_VOLUME_MUTE,
                                  Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE,
                                  Action.ActionType.ACTION_SHOW_SUBTITLES,
                                  Action.ActionType.ACTION_NEXT_AUDIO,
                                  Action.ActionType.ACTION_HIGHLIGHT_ITEM,
                                  Action.ActionType.ACTION_PARENT_DIR,
                                  Action.ActionType.ACTION_NEXT_ITEM,
                                  Action.ActionType.ACTION_PREV_ITEM,
                                  Action.ActionType.ACTION_STEP_FORWARD,
                                  Action.ActionType.ACTION_STEP_BACK,
                                  Action.ActionType.ACTION_BIG_STEP_FORWARD,
                                  Action.ActionType.ACTION_BIG_STEP_BACK,
                                  Action.ActionType.ACTION_SHOW_OSD,
                                  Action.ActionType.ACTION_SHOW_CODEC,
                                  Action.ActionType.ACTION_NEXT_PICTURE,
                                  Action.ActionType.ACTION_PREV_PICTURE,
                                  Action.ActionType.ACTION_ZOOM_OUT,
                                  Action.ActionType.ACTION_ZOOM_IN,
                                  Action.ActionType.ACTION_TOGGLE_SOURCE_DEST,
                                  Action.ActionType.ACTION_SHOW_PLAYLIST,
                                  Action.ActionType.ACTION_REMOVE_ITEM,
                                  Action.ActionType.ACTION_SHOW_FULLSCREEN,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_1,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_2,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_3,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_4,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_5,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_6,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_7,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_8,
                                  Action.ActionType.ACTION_ZOOM_LEVEL_9,
                                  Action.ActionType.ACTION_CALIBRATE_SWAP_ARROWS,
                                  Action.ActionType.ACTION_CALIBRATE_RESET,
                                  Action.ActionType.ACTION_ANALOG_MOVE,
                                  Action.ActionType.ACTION_ROTATE_PICTURE,
                                  Action.ActionType.ACTION_CLOSE_DIALOG,
                                  Action.ActionType.ACTION_SUBTITLE_DELAY_MIN,
                                  Action.ActionType.ACTION_SUBTITLE_DELAY_PLUS,
                                  Action.ActionType.ACTION_AUDIO_DELAY_MIN,
                                  Action.ActionType.ACTION_AUDIO_DELAY_PLUS,
                                  Action.ActionType.ACTION_CHANGE_RESOLUTION,
                                  Action.ActionType.REMOTE_0,
                                  Action.ActionType.REMOTE_1,
                                  Action.ActionType.REMOTE_2,
                                  Action.ActionType.REMOTE_3,
                                  Action.ActionType.REMOTE_4,
                                  Action.ActionType.REMOTE_5,
                                  Action.ActionType.REMOTE_6,
                                  Action.ActionType.REMOTE_7,
                                  Action.ActionType.REMOTE_8,
                                  Action.ActionType.REMOTE_9,
                                  Action.ActionType.ACTION_OSD_SHOW_LEFT,
                                  Action.ActionType.ACTION_OSD_SHOW_RIGHT,
                                  Action.ActionType.ACTION_OSD_SHOW_UP,
                                  Action.ActionType.ACTION_OSD_SHOW_DOWN,
                                  Action.ActionType.ACTION_OSD_SHOW_SELECT,
                                  Action.ActionType.ACTION_OSD_SHOW_VALUE_PLUS,
                                  Action.ActionType.ACTION_OSD_SHOW_VALUE_MIN,
                                  Action.ActionType.ACTION_SMALL_STEP_BACK,
                                  Action.ActionType.ACTION_MUSIC_FORWARD,
                                  Action.ActionType.ACTION_MUSIC_REWIND,
                                  Action.ActionType.ACTION_MUSIC_PLAY,
                                  Action.ActionType.ACTION_DELETE_ITEM,
                                  Action.ActionType.ACTION_COPY_ITEM,
                                  Action.ActionType.ACTION_MOVE_ITEM,
                                  Action.ActionType.ACTION_SHOW_MPLAYER_OSD,
                                  Action.ActionType.ACTION_OSD_HIDESUBMENU,
                                  Action.ActionType.ACTION_TAKE_SCREENSHOT,
                                  Action.ActionType.ACTION_INCREASE_TIMEBLOCK,
                                  Action.ActionType.ACTION_DECREASE_TIMEBLOCK,
                                  Action.ActionType.ACTION_DEFAULT_TIMEBLOCK,
                                  Action.ActionType.ACTION_TVGUIDE_RESET,
                                  Action.ActionType.ACTION_BACKGROUND_TOGGLE,
                                  Action.ActionType.ACTION_TOGGLE_WINDOWED_FULLSCREEN,
                                  USBUIRT.JumpToActionType.JUMP_TO_HOME,
                                  USBUIRT.JumpToActionType.JUMP_TO_TV_GUIDE,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_TV,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_TV_FULLSCREEN,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_MOVIES,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_MOVIES_FULLSCREEN,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_MUSIC,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_PICTURES,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_RADIO,
                                  USBUIRT.JumpToActionType.JUMP_TO_MY_WEATHER,
                                  USBUIRT.JumpToActionType.JUMP_TO_TELETEXT,
                                  USBUIRT.JumpToActionType.JUMP_TO_TELETEXT_FULLSCREEN,
                                };

    #endregion

    #region delegates

    private delegate void SetStatusTextCallback(string text);

    private delegate void SetLearningStatusCallback(LearningEventArgs e);

    private delegate void SetEndLearningCallback(EventArgs e);

    #endregion

    #region Controls

    private MediaPortal.UserInterface.Controls.MPButton LearnEnabledMPCmdsBtn;
    private System.Windows.Forms.CheckedListBox ActionsCheckList;
    private MediaPortal.UserInterface.Controls.MPButton LearnSingleMPCmdBtn;
    private System.Windows.Forms.Panel MPCommandsPnl;
    private MediaPortal.UserInterface.Controls.MPButton CancelLearnBtn;
    private System.Windows.Forms.ProgressBar LearnPrgBar;
    private MediaPortal.UserInterface.Controls.MPLabel LearnStatusLbl;
    private System.Windows.Forms.Panel panel2;
    private MediaPortal.UserInterface.Controls.MPLabel receivedIrLbl;
    private MediaPortal.UserInterface.Controls.MPLabel statusLabel;
    private MediaPortal.UserInterface.Controls.MPButton SkipCodeBtn;
    private System.Windows.Forms.Panel LearnStatusPnl;
    private MediaPortal.UserInterface.Controls.MPButton CloseMPCmdsLearnFormBtn;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel MPCode1ValueLbl;
    private MediaPortal.UserInterface.Controls.MPLabel MPCode2ValueLbl;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPButton ClearSingleLearnedMPCmdBtn;
    private MediaPortal.UserInterface.Controls.MPButton ClearAllLearnedMPCmdsBtn;
    private System.Windows.Forms.StatusBar statusBar1;
    private Panel STBComandsPnl;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel STBCode1ValueLbl;
    private MediaPortal.UserInterface.Controls.MPLabel STBCode2ValueLbl;
    private MediaPortal.UserInterface.Controls.MPButton LearnEnabledSTBCmdsBtn;
    private CheckedListBox StbCommandsChkLst;
    private MediaPortal.UserInterface.Controls.MPButton LearnSingleSTBCmdBtn;
    private MediaPortal.UserInterface.Controls.MPButton CloseSTBCmdsLearnFormBtn;
    private MediaPortal.UserInterface.Controls.MPButton ClearSingleLearnedSTBCmdBtn;
    private MediaPortal.UserInterface.Controls.MPButton ClearAllLearnedSTBCmdsBtn;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    #endregion

    #region ctor and disposal

    public USBUIRTLearnForm(LearnType learnType)
    {
      InitializeComponent();

      MediaPortal.IR.USBUIRT.Instance.OnRemoteCommandFeedback +=
        new MediaPortal.IR.USBUIRT.RemoteCommandFeedbackHandler(Instance_OnRemoteCommandFeedback);

      if (learnType == LearnType.MediaPortalCommands)
      {
        CreateButtonNames();
        this.ActionsCheckList.Items.AddRange(buttonNames.ToArray());
        MPCommandsPnl.BringToFront();
        this.Text = "Media Portal Control Commands";
      }

      else
      {
        STBComandsPnl.BringToFront();
        this.Text = "Set Top Box Control Commands";
      }
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
          components.SafeDispose();
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.ActionsCheckList = new System.Windows.Forms.CheckedListBox();
      this.MPCommandsPnl = new System.Windows.Forms.Panel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.MPCode1ValueLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.MPCode2ValueLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.LearnEnabledMPCmdsBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.LearnSingleMPCmdBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.CloseMPCmdsLearnFormBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.ClearSingleLearnedMPCmdBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.ClearAllLearnedMPCmdsBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.LearnPrgBar = new System.Windows.Forms.ProgressBar();
      this.panel2 = new System.Windows.Forms.Panel();
      this.receivedIrLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.statusLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.LearnStatusPnl = new System.Windows.Forms.Panel();
      this.CancelLearnBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.LearnStatusLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SkipCodeBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.statusBar1 = new System.Windows.Forms.StatusBar();
      this.STBComandsPnl = new System.Windows.Forms.Panel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.STBCode1ValueLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.STBCode2ValueLbl = new MediaPortal.UserInterface.Controls.MPLabel();
      this.LearnEnabledSTBCmdsBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.StbCommandsChkLst = new System.Windows.Forms.CheckedListBox();
      this.LearnSingleSTBCmdBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.CloseSTBCmdsLearnFormBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.ClearSingleLearnedSTBCmdBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.ClearAllLearnedSTBCmdsBtn = new MediaPortal.UserInterface.Controls.MPButton();
      this.MPCommandsPnl.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel2.SuspendLayout();
      this.LearnStatusPnl.SuspendLayout();
      this.STBComandsPnl.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // ActionsCheckList
      // 
      this.ActionsCheckList.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.ActionsCheckList.CheckOnClick = true;
      this.ActionsCheckList.Location = new System.Drawing.Point(14, 24);
      this.ActionsCheckList.Name = "ActionsCheckList";
      this.ActionsCheckList.Size = new System.Drawing.Size(267, 169);
      this.ActionsCheckList.TabIndex = 1;
      this.ActionsCheckList.SelectedIndexChanged += new System.EventHandler(this.ActionsCheckList_SelectedIndexChanged);
      // 
      // MPCommandsPnl
      // 
      this.MPCommandsPnl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.MPCommandsPnl.Controls.Add(this.label1);
      this.MPCommandsPnl.Controls.Add(this.groupBox2);
      this.MPCommandsPnl.Controls.Add(this.LearnEnabledMPCmdsBtn);
      this.MPCommandsPnl.Controls.Add(this.ActionsCheckList);
      this.MPCommandsPnl.Controls.Add(this.LearnSingleMPCmdBtn);
      this.MPCommandsPnl.Controls.Add(this.CloseMPCmdsLearnFormBtn);
      this.MPCommandsPnl.Controls.Add(this.ClearSingleLearnedMPCmdBtn);
      this.MPCommandsPnl.Controls.Add(this.ClearAllLearnedMPCmdsBtn);
      this.MPCommandsPnl.Location = new System.Drawing.Point(0, 0);
      this.MPCommandsPnl.Name = "MPCommandsPnl";
      this.MPCommandsPnl.Size = new System.Drawing.Size(469, 281);
      this.MPCommandsPnl.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold,
                                                 System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(14, 6);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(123, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Media Portal Actions";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.MPCode1ValueLbl);
      this.groupBox2.Controls.Add(this.MPCode2ValueLbl);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(14, 196);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(441, 79);
      this.groupBox2.TabIndex = 24;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Learned Receive IR Code(s)";
      // 
      // MPCode1ValueLbl
      // 
      this.MPCode1ValueLbl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.MPCode1ValueLbl.Location = new System.Drawing.Point(15, 22);
      this.MPCode1ValueLbl.Name = "MPCode1ValueLbl";
      this.MPCode1ValueLbl.Size = new System.Drawing.Size(411, 21);
      this.MPCode1ValueLbl.TabIndex = 0;
      // 
      // MPCode2ValueLbl
      // 
      this.MPCode2ValueLbl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.MPCode2ValueLbl.ForeColor = System.Drawing.Color.DarkBlue;
      this.MPCode2ValueLbl.Location = new System.Drawing.Point(15, 43);
      this.MPCode2ValueLbl.Name = "MPCode2ValueLbl";
      this.MPCode2ValueLbl.Size = new System.Drawing.Size(411, 21);
      this.MPCode2ValueLbl.TabIndex = 1;
      // 
      // LearnEnabledMPCmdsBtn
      // 
      this.LearnEnabledMPCmdsBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.LearnEnabledMPCmdsBtn.Location = new System.Drawing.Point(287, 24);
      this.LearnEnabledMPCmdsBtn.Name = "LearnEnabledMPCmdsBtn";
      this.LearnEnabledMPCmdsBtn.Size = new System.Drawing.Size(168, 23);
      this.LearnEnabledMPCmdsBtn.TabIndex = 2;
      this.LearnEnabledMPCmdsBtn.Text = "Learn Enabled Commands";
      this.LearnEnabledMPCmdsBtn.UseVisualStyleBackColor = true;
      this.LearnEnabledMPCmdsBtn.Click += new System.EventHandler(this.LearnEnabledMPCmdsBtn_Click);
      // 
      // LearnSingleMPCmdBtn
      // 
      this.LearnSingleMPCmdBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.LearnSingleMPCmdBtn.Enabled = false;
      this.LearnSingleMPCmdBtn.Location = new System.Drawing.Point(287, 51);
      this.LearnSingleMPCmdBtn.Name = "LearnSingleMPCmdBtn";
      this.LearnSingleMPCmdBtn.Size = new System.Drawing.Size(168, 23);
      this.LearnSingleMPCmdBtn.TabIndex = 3;
      this.LearnSingleMPCmdBtn.Text = "Learn Selected Command";
      this.LearnSingleMPCmdBtn.UseVisualStyleBackColor = true;
      this.LearnSingleMPCmdBtn.Click += new System.EventHandler(this.LearnSingleMPCmdBtn_Click);
      // 
      // CloseMPCmdsLearnFormBtn
      // 
      this.CloseMPCmdsLearnFormBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.CloseMPCmdsLearnFormBtn.Location = new System.Drawing.Point(287, 170);
      this.CloseMPCmdsLearnFormBtn.Name = "CloseMPCmdsLearnFormBtn";
      this.CloseMPCmdsLearnFormBtn.Size = new System.Drawing.Size(168, 23);
      this.CloseMPCmdsLearnFormBtn.TabIndex = 6;
      this.CloseMPCmdsLearnFormBtn.Text = "Close";
      this.CloseMPCmdsLearnFormBtn.UseVisualStyleBackColor = true;
      this.CloseMPCmdsLearnFormBtn.Click += new System.EventHandler(this.CloseMPCmdsLearnFormBtn_Click);
      // 
      // ClearSingleLearnedMPCmdBtn
      // 
      this.ClearSingleLearnedMPCmdBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ClearSingleLearnedMPCmdBtn.Enabled = false;
      this.ClearSingleLearnedMPCmdBtn.Location = new System.Drawing.Point(287, 124);
      this.ClearSingleLearnedMPCmdBtn.Name = "ClearSingleLearnedMPCmdBtn";
      this.ClearSingleLearnedMPCmdBtn.Size = new System.Drawing.Size(168, 23);
      this.ClearSingleLearnedMPCmdBtn.TabIndex = 5;
      this.ClearSingleLearnedMPCmdBtn.Text = "Clear Selected Learn";
      this.ClearSingleLearnedMPCmdBtn.UseVisualStyleBackColor = true;
      this.ClearSingleLearnedMPCmdBtn.Click += new System.EventHandler(this.ClearSingleLearnedMPCmdBtn_Click);
      // 
      // ClearAllLearnedMPCmdsBtn
      // 
      this.ClearAllLearnedMPCmdsBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ClearAllLearnedMPCmdsBtn.Location = new System.Drawing.Point(287, 96);
      this.ClearAllLearnedMPCmdsBtn.Name = "ClearAllLearnedMPCmdsBtn";
      this.ClearAllLearnedMPCmdsBtn.Size = new System.Drawing.Size(168, 23);
      this.ClearAllLearnedMPCmdsBtn.TabIndex = 4;
      this.ClearAllLearnedMPCmdsBtn.Text = "Clear All Learned";
      this.ClearAllLearnedMPCmdsBtn.UseVisualStyleBackColor = true;
      this.ClearAllLearnedMPCmdsBtn.Click += new System.EventHandler(this.ClearAllLearnedMPCmdsBtn_Click);
      // 
      // LearnPrgBar
      // 
      this.LearnPrgBar.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.LearnPrgBar.Location = new System.Drawing.Point(24, 224);
      this.LearnPrgBar.Name = "LearnPrgBar";
      this.LearnPrgBar.Size = new System.Drawing.Size(421, 12);
      this.LearnPrgBar.TabIndex = 2;
      // 
      // panel2
      // 
      this.panel2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.panel2.BackColor = System.Drawing.SystemColors.Window;
      this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel2.Controls.Add(this.receivedIrLbl);
      this.panel2.Controls.Add(this.statusLabel);
      this.panel2.Location = new System.Drawing.Point(24, 32);
      this.panel2.Name = "panel2";
      this.panel2.Padding = new System.Windows.Forms.Padding(20);
      this.panel2.Size = new System.Drawing.Size(421, 188);
      this.panel2.TabIndex = 1;
      // 
      // receivedIrLbl
      // 
      this.receivedIrLbl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.receivedIrLbl.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.receivedIrLbl.Location = new System.Drawing.Point(20, 152);
      this.receivedIrLbl.Name = "receivedIrLbl";
      this.receivedIrLbl.Size = new System.Drawing.Size(379, 28);
      this.receivedIrLbl.TabIndex = 1;
      // 
      // statusLabel
      // 
      this.statusLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.statusLabel.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular,
                                                      System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.statusLabel.Location = new System.Drawing.Point(20, 20);
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(379, 128);
      this.statusLabel.TabIndex = 0;
      // 
      // LearnStatusPnl
      // 
      this.LearnStatusPnl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.LearnStatusPnl.Controls.Add(this.CancelLearnBtn);
      this.LearnStatusPnl.Controls.Add(this.LearnPrgBar);
      this.LearnStatusPnl.Controls.Add(this.LearnStatusLbl);
      this.LearnStatusPnl.Controls.Add(this.panel2);
      this.LearnStatusPnl.Controls.Add(this.SkipCodeBtn);
      this.LearnStatusPnl.Location = new System.Drawing.Point(0, 0);
      this.LearnStatusPnl.Name = "LearnStatusPnl";
      this.LearnStatusPnl.Size = new System.Drawing.Size(469, 281);
      this.LearnStatusPnl.TabIndex = 21;
      // 
      // CancelLearnBtn
      // 
      this.CancelLearnBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.CancelLearnBtn.Location = new System.Drawing.Point(357, 244);
      this.CancelLearnBtn.Name = "CancelLearnBtn";
      this.CancelLearnBtn.Size = new System.Drawing.Size(88, 23);
      this.CancelLearnBtn.TabIndex = 4;
      this.CancelLearnBtn.Text = "Cancel Learn";
      this.CancelLearnBtn.UseVisualStyleBackColor = true;
      this.CancelLearnBtn.Click += new System.EventHandler(this.CancelLearnBtn_Click);
      // 
      // LearnStatusLbl
      // 
      this.LearnStatusLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold,
                                                         System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.LearnStatusLbl.Location = new System.Drawing.Point(24, 16);
      this.LearnStatusLbl.Name = "LearnStatusLbl";
      this.LearnStatusLbl.Size = new System.Drawing.Size(408, 16);
      this.LearnStatusLbl.TabIndex = 0;
      this.LearnStatusLbl.Text = "Learn Status:";
      // 
      // SkipCodeBtn
      // 
      this.SkipCodeBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.SkipCodeBtn.Location = new System.Drawing.Point(24, 244);
      this.SkipCodeBtn.Name = "SkipCodeBtn";
      this.SkipCodeBtn.Size = new System.Drawing.Size(88, 23);
      this.SkipCodeBtn.TabIndex = 3;
      this.SkipCodeBtn.Text = "Skip this Code";
      this.SkipCodeBtn.UseVisualStyleBackColor = true;
      this.SkipCodeBtn.Click += new System.EventHandler(this.SkipCodeBtn_Click);
      // 
      // statusBar1
      // 
      this.statusBar1.Location = new System.Drawing.Point(0, 285);
      this.statusBar1.Name = "statusBar1";
      this.statusBar1.Size = new System.Drawing.Size(469, 22);
      this.statusBar1.TabIndex = 22;
      // 
      // STBComandsPnl
      // 
      this.STBComandsPnl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.STBComandsPnl.Controls.Add(this.label2);
      this.STBComandsPnl.Controls.Add(this.groupBox1);
      this.STBComandsPnl.Controls.Add(this.LearnEnabledSTBCmdsBtn);
      this.STBComandsPnl.Controls.Add(this.StbCommandsChkLst);
      this.STBComandsPnl.Controls.Add(this.LearnSingleSTBCmdBtn);
      this.STBComandsPnl.Controls.Add(this.CloseSTBCmdsLearnFormBtn);
      this.STBComandsPnl.Controls.Add(this.ClearSingleLearnedSTBCmdBtn);
      this.STBComandsPnl.Controls.Add(this.ClearAllLearnedSTBCmdsBtn);
      this.STBComandsPnl.Location = new System.Drawing.Point(0, 0);
      this.STBComandsPnl.Name = "STBComandsPnl";
      this.STBComandsPnl.Size = new System.Drawing.Size(469, 281);
      this.STBComandsPnl.TabIndex = 24;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold,
                                                 System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(14, 6);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(186, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Set-top-box Control Commands";
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.STBCode1ValueLbl);
      this.groupBox1.Controls.Add(this.STBCode2ValueLbl);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(14, 196);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(441, 79);
      this.groupBox1.TabIndex = 24;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Learned Transmit IR Code(s)";
      // 
      // STBCode1ValueLbl
      // 
      this.STBCode1ValueLbl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.STBCode1ValueLbl.Location = new System.Drawing.Point(8, 18);
      this.STBCode1ValueLbl.Name = "STBCode1ValueLbl";
      this.STBCode1ValueLbl.Size = new System.Drawing.Size(425, 25);
      this.STBCode1ValueLbl.TabIndex = 0;
      this.STBCode1ValueLbl.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // STBCode2ValueLbl
      // 
      this.STBCode2ValueLbl.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.STBCode2ValueLbl.ForeColor = System.Drawing.Color.DarkBlue;
      this.STBCode2ValueLbl.Location = new System.Drawing.Point(8, 44);
      this.STBCode2ValueLbl.Name = "STBCode2ValueLbl";
      this.STBCode2ValueLbl.Size = new System.Drawing.Size(425, 25);
      this.STBCode2ValueLbl.TabIndex = 1;
      // 
      // LearnEnabledSTBCmdsBtn
      // 
      this.LearnEnabledSTBCmdsBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.LearnEnabledSTBCmdsBtn.Location = new System.Drawing.Point(287, 24);
      this.LearnEnabledSTBCmdsBtn.Name = "LearnEnabledSTBCmdsBtn";
      this.LearnEnabledSTBCmdsBtn.Size = new System.Drawing.Size(168, 23);
      this.LearnEnabledSTBCmdsBtn.TabIndex = 2;
      this.LearnEnabledSTBCmdsBtn.Text = "Learn Enabled Commands";
      this.LearnEnabledSTBCmdsBtn.UseVisualStyleBackColor = true;
      this.LearnEnabledSTBCmdsBtn.Click += new System.EventHandler(this.LearnEnabledSTBCmdsBtn_Click);
      // 
      // StbCommandsChkLst
      // 
      this.StbCommandsChkLst.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.StbCommandsChkLst.Items.AddRange(new object[]
                                              {
                                                "0",
                                                "1",
                                                "2",
                                                "3",
                                                "4",
                                                "5",
                                                "6",
                                                "7",
                                                "8",
                                                "9",
                                                "Enter"
                                              });
      this.StbCommandsChkLst.Location = new System.Drawing.Point(14, 24);
      this.StbCommandsChkLst.Name = "StbCommandsChkLst";
      this.StbCommandsChkLst.Size = new System.Drawing.Size(267, 169);
      this.StbCommandsChkLst.TabIndex = 1;
      this.StbCommandsChkLst.SelectedIndexChanged += new System.EventHandler(this.StbCommandsChkLst_SelectedIndexChanged);
      // 
      // LearnSingleSTBCmdBtn
      // 
      this.LearnSingleSTBCmdBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.LearnSingleSTBCmdBtn.Location = new System.Drawing.Point(287, 51);
      this.LearnSingleSTBCmdBtn.Name = "LearnSingleSTBCmdBtn";
      this.LearnSingleSTBCmdBtn.Size = new System.Drawing.Size(168, 23);
      this.LearnSingleSTBCmdBtn.TabIndex = 3;
      this.LearnSingleSTBCmdBtn.Text = "Learn Selected Command";
      this.LearnSingleSTBCmdBtn.UseVisualStyleBackColor = true;
      this.LearnSingleSTBCmdBtn.Click += new System.EventHandler(this.LearnSingleSTBCmdBtn_Click);
      // 
      // CloseSTBCmdsLearnFormBtn
      // 
      this.CloseSTBCmdsLearnFormBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.CloseSTBCmdsLearnFormBtn.Location = new System.Drawing.Point(287, 170);
      this.CloseSTBCmdsLearnFormBtn.Name = "CloseSTBCmdsLearnFormBtn";
      this.CloseSTBCmdsLearnFormBtn.Size = new System.Drawing.Size(168, 23);
      this.CloseSTBCmdsLearnFormBtn.TabIndex = 6;
      this.CloseSTBCmdsLearnFormBtn.Text = "Close";
      this.CloseSTBCmdsLearnFormBtn.UseVisualStyleBackColor = true;
      this.CloseSTBCmdsLearnFormBtn.Click += new System.EventHandler(this.CloseSTBCmdsLearnFormBtn_Click);
      // 
      // ClearSingleLearnedSTBCmdBtn
      // 
      this.ClearSingleLearnedSTBCmdBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ClearSingleLearnedSTBCmdBtn.Location = new System.Drawing.Point(287, 124);
      this.ClearSingleLearnedSTBCmdBtn.Name = "ClearSingleLearnedSTBCmdBtn";
      this.ClearSingleLearnedSTBCmdBtn.Size = new System.Drawing.Size(168, 23);
      this.ClearSingleLearnedSTBCmdBtn.TabIndex = 5;
      this.ClearSingleLearnedSTBCmdBtn.Text = "Clear Selected Learn";
      this.ClearSingleLearnedSTBCmdBtn.UseVisualStyleBackColor = true;
      this.ClearSingleLearnedSTBCmdBtn.Click += new System.EventHandler(this.ClearSingleLearnedSTBCmdBtn_Click);
      // 
      // ClearAllLearnedSTBCmdsBtn
      // 
      this.ClearAllLearnedSTBCmdsBtn.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.ClearAllLearnedSTBCmdsBtn.Location = new System.Drawing.Point(287, 96);
      this.ClearAllLearnedSTBCmdsBtn.Name = "ClearAllLearnedSTBCmdsBtn";
      this.ClearAllLearnedSTBCmdsBtn.Size = new System.Drawing.Size(168, 23);
      this.ClearAllLearnedSTBCmdsBtn.TabIndex = 4;
      this.ClearAllLearnedSTBCmdsBtn.Text = "Clear All Learned";
      this.ClearAllLearnedSTBCmdsBtn.UseVisualStyleBackColor = true;
      this.ClearAllLearnedSTBCmdsBtn.Click += new System.EventHandler(this.ClearAllLearnedSTBCmdsBtn_Click);
      // 
      // USBUIRTLearnForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(469, 307);
      this.Controls.Add(this.statusBar1);
      this.Controls.Add(this.MPCommandsPnl);
      this.Controls.Add(this.LearnStatusPnl);
      this.Controls.Add(this.STBComandsPnl);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(468, 333);
      this.Name = "USBUIRTLearnForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = " USBUIRTLearnForm";
      this.MPCommandsPnl.ResumeLayout(false);
      this.MPCommandsPnl.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.LearnStatusPnl.ResumeLayout(false);
      this.STBComandsPnl.ResumeLayout(false);
      this.STBComandsPnl.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      // Set item checked for loaded MP commands
      ResetCheckListItems(ActionsCheckList);
      Hashtable mpCommandsTable = MediaPortal.IR.USBUIRT.Instance.LearnedMediaPortalCodesTable;

      if (mpCommandsTable == null)
      {
        this.LoadDefaultCommandSet();
        return;
      }

      foreach (object entry in mpCommandsTable.Keys)
      {
        string irCode = entry.ToString();
        object command = mpCommandsTable[irCode];

        EnableActionsCheckListItem((Action.ActionType)command);
      }

      if (ActionsCheckList.Items.Count > 0)
        ActionsCheckList.SelectedIndex = 0;

      // Set item checked for loaded STB control commands
      ResetCheckListItems(StbCommandsChkLst);
      Hashtable stbTunerCodesTable = MediaPortal.IR.USBUIRT.Instance.LearnedSTBCodesTable;

      if (stbTunerCodesTable == null)
      {
        LoadDefaultSTBCommandSet();
        return;
      }

      for (int i = 0; i < stbTunerCodesTable.Count; i++)
      {
        if (stbTunerCodesTable.ContainsKey(i))
          StbCommandsChkLst.SetItemChecked(i, true);
      }

      if (StbCommandsChkLst.Items.Count > 0)
        StbCommandsChkLst.SelectedIndex = 0;
    }

    private void ResetCheckListItems(CheckedListBox chkLstBox)
    {
      for (int i = 0; i < chkLstBox.Items.Count; i++)
        chkLstBox.SetItemChecked(i, false);
    }

    private void EnableActionsCheckListItem(Action.ActionType action)
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        Action.ActionType curAction = (Action.ActionType)commands[i];

        if (curAction == action)
        {
          ActionsCheckList.SetItemChecked(i, true);
          break;
        }
      }
    }

    #region Misc

    private void CreateButtonNames()
    {
      this.buttonNames.Clear();

      for (int i = 0; i < commands.Length; i++)
      {
        string curCmd = string.Empty;

        if (commands[i] is Action.ActionType)
        {
          Action.ActionType curActionType = (Action.ActionType)commands[i];
          curCmd = curActionType.ToString().Replace("ACTION_", "");

          // hack to fix incorrectly spelled MP ActionType
          if (curCmd == "TOGGLE_WINDOWED_FULSLCREEN")
            curCmd = "TOGGLE_WINDOWED_FULLSCREEN";

          curCmd = curCmd.Replace("_", " ");
        }

        else if (commands[i] is USBUIRT.JumpToActionType)
        {
          curCmd = commands[i].ToString();
          curCmd = curCmd.Replace("_", " ");
        }

        else
          continue;

        buttonNames.Add(curCmd);
      }
    }

    #endregion

    #region Media Portal learn methods

    private void DoMediaPortalCommandLearn()
    {
      int count = ActionsCheckList.CheckedItems.Count;

      object[] learncommands = new object[count];
      string[] learnbuttons = new string[count];
      int index = 0;

      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        if (ActionsCheckList.GetItemChecked(i))
        {
          learncommands[index] = commands[i];
          learnbuttons[index] = (string)buttonNames[i];
          index++;
        }
      }

      MediaPortal.IR.USBUIRT.Instance.StartLearning +=
        new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
      MediaPortal.IR.USBUIRT.Instance.OnEventLearned +=
        new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
      MediaPortal.IR.USBUIRT.Instance.OnEndLearning +=
        new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxLearning);
      MediaPortal.IR.USBUIRT.Instance.BulkLearn(learncommands, learnbuttons);
    }

    private void LearnSingleMPCmdBtn_Click(object sender, System.EventArgs e)
    {
      int selIndex = ActionsCheckList.SelectedIndex;

      if (selIndex == -1 || !ActionsCheckList.GetItemChecked(selIndex))
      {
        MessageBox.Show(this, "No commands selected.  Please select a command.\t", "USBUIRT", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        return;
      }

      // Make sure we clear any existing ir code assignements for this action
      if (MediaPortal.IR.USBUIRT.Instance.ClearLearnedCommand((Action.ActionType)commands[selIndex]))
      {
        ActionsCheckList_SelectedIndexChanged(null, null);
      }

      object[] learncommands = new object[1];
      string[] learnbuttons = new string[1];

      learncommands[0] = commands[selIndex];
      learnbuttons[0] = (string)buttonNames[selIndex];

      MediaPortal.IR.USBUIRT.Instance.StartLearning +=
        new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
      MediaPortal.IR.USBUIRT.Instance.OnEventLearned +=
        new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
      MediaPortal.IR.USBUIRT.Instance.OnEndLearning +=
        new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxSingleCommandLearning);
      MediaPortal.IR.USBUIRT.Instance.BulkLearn(learncommands, learnbuttons);
    }

    #endregion

    #region STB learn methods

    private void DoSetTopBoxCommandLearn()
    {
      MediaPortal.IR.USBUIRT.Instance.StartLearning +=
        new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartTxLearning);
      MediaPortal.IR.USBUIRT.Instance.OnEventLearned +=
        new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnTxEventLearned);
      MediaPortal.IR.USBUIRT.Instance.OnEndLearning +=
        new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnTxEndLearning);

      ArrayList cmdList = new ArrayList();

      for (int i = 0; i < StbCommandsChkLst.Items.Count; i++)
      {
        if (StbCommandsChkLst.GetItemChecked(i))
          cmdList.Add(i);
      }

      if (cmdList.Count > 0)
      {
        int[] cmds = (int[])cmdList.ToArray(typeof (int));
        MediaPortal.IR.USBUIRT.Instance.LearnTunerCodes(cmds);
      }
    }

    private void DoSingleSetTopBoxCommandLearn()
    {
      int selIndex = StbCommandsChkLst.SelectedIndex;

      if (selIndex == -1 || !StbCommandsChkLst.GetItemChecked(selIndex))
      {
        MessageBox.Show(this, "No commands selected.  Please select a command.\t", "USBUIRT", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        return;
      }

      int[] cmds = new int[1];
      cmds[0] = selIndex;

      MediaPortal.IR.USBUIRT.Instance.StartLearning +=
        new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartTxLearning);
      MediaPortal.IR.USBUIRT.Instance.OnEventLearned +=
        new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnTxEventLearned);
      MediaPortal.IR.USBUIRT.Instance.OnEndLearning +=
        new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnTxEndSingleCommandLearning);

      MediaPortal.IR.USBUIRT.Instance.LearnTunerCodes(cmds);
    }

    #endregion

    #region MP Commands Learn button click handlers

    private void LearnEnabledMPCmdsBtn_Click(object sender, System.EventArgs e)
    {
      DoMediaPortalCommandLearn();
    }

    private void ClearAllLearnedMPCmdsBtn_Click(object sender, System.EventArgs e)
    {
      if (MediaPortal.IR.USBUIRT.Instance.LearnedMediaPortalCodesTable.Count == 0)
      {
        MessageBox.Show(this, "No Media Portal control IR codes loaded.  Nothing to clear.\t", "USBUIRT",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      string msg = "You are about to clear all learned IR codes.\t\r\n\r\nContinue?";

      if (MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
      {
        if (MediaPortal.IR.USBUIRT.Instance.ClearAllLearnedCommands())
        {
          ActionsCheckList_SelectedIndexChanged(null, null);
          MessageBox.Show(this, "All learned Media Portal control IR codes cleared.\t", "USBUIRT", MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }
      }
    }

    private void ClearSingleLearnedMPCmdBtn_Click(object sender, System.EventArgs e)
    {
      int selIndex = ActionsCheckList.SelectedIndex;

      if (selIndex != -1)
      {
        string cmdName = ActionsCheckList.SelectedItem.ToString();
        int cmdVal = (int)commands[selIndex];
        object command = commands[selIndex];

        if (cmdVal < (int)USBUIRT.JumpToActionType.JUMP_TO_INVALID)
          command = (Action.ActionType)command;

        else
          command = (USBUIRT.JumpToActionType)command;


        if (MediaPortal.IR.USBUIRT.Instance.ClearLearnedCommand(command))
        {
          ActionsCheckList.SetItemChecked(selIndex, false);
          ActionsCheckList_SelectedIndexChanged(null, null);
          string msg = string.Format("Learned IR for [{0}] Media Portal action cleared.\t", cmdName);
          MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        else
        {
          string msg = string.Format("Learned IR not found for [{0}] Media Portal action.\t", cmdName);
          MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
    }

    private void CloseMPCmdsLearnFormBtn_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }

    #endregion

    #region STB Learn button click handlers

    private void LearnEnabledSTBCmdsBtn_Click(object sender, EventArgs e)
    {
      DoSetTopBoxCommandLearn();
    }

    private void LearnSingleSTBCmdBtn_Click(object sender, EventArgs e)
    {
      DoSingleSetTopBoxCommandLearn();
    }

    private void ClearAllLearnedSTBCmdsBtn_Click(object sender, EventArgs e)
    {
      if (MediaPortal.IR.USBUIRT.Instance.LearnedSTBCodesTable.Count == 0)
      {
        MessageBox.Show(this, "No Set-top box control IR codes loaded.  Nothing to clear.\t", "USBUIRT",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      string msg = "You are about to clear all learned Set-top box codes.\t\r\n\r\nContinue?";

      if (MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
      {
        if (MediaPortal.IR.USBUIRT.Instance.ClearAllLearnedSTBCommands())
        {
          StbCommandsChkLst_SelectedIndexChanged(null, null);
          MessageBox.Show(this, "All learned Set-top box control IR codes cleared.\t", "USBUIRT", MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }
      }
    }

    private void ClearSingleLearnedSTBCmdBtn_Click(object sender, EventArgs e)
    {
      int selIndex = StbCommandsChkLst.SelectedIndex;

      if (selIndex != -1)
      {
        if (MediaPortal.IR.USBUIRT.Instance.ClearLearnedSTBCommand(selIndex))
        {
          StbCommandsChkLst.SetItemChecked(selIndex, false);
          StbCommandsChkLst_SelectedIndexChanged(null, null);
          string msg = string.Format("Learned IR for Set-top box control [{0}] button cleared.\t",
                                     (selIndex < 10 ? selIndex.ToString() : "Enter"));
          MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        else
        {
          string msg = string.Format("Learned IR not found for Set-top box control [{0}] button.\t",
                                     (selIndex < 10 ? selIndex.ToString() : "Enter"));
          MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
    }

    private void CloseSTBCmdsLearnFormBtn_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    #endregion

    #region CheckedList SelectedIndexChanged event handlers

    private void StbCommandsChkLst_SelectedIndexChanged(object sender, EventArgs e)
    {
      int selIndex = StbCommandsChkLst.SelectedIndex;

      if (selIndex != -1)
      {
        LearnSingleSTBCmdBtn.Enabled = true;
        ClearSingleLearnedSTBCmdBtn.Enabled = true;
        string irCmd1 = string.Empty;
        string irCmd2 = string.Empty;

        if (MediaPortal.IR.USBUIRT.Instance.GetSTBCommandIrStrings(selIndex, ref irCmd1, ref irCmd2))
        {
          STBCode1ValueLbl.ForeColor = SystemColors.ControlText;
          STBCode1ValueLbl.Text = irCmd1;
          STBCode2ValueLbl.Text = irCmd2;

          LearnSingleSTBCmdBtn.Text = "Relearn Selected Command";
        }

        else
        {
          if (StbCommandsChkLst.GetItemChecked(selIndex))
            STBCode1ValueLbl.ForeColor = Color.Red;

          else
            STBCode1ValueLbl.ForeColor = SystemColors.ControlText;

          STBCode1ValueLbl.Text = "No code(s) Learned for this action";
          STBCode2ValueLbl.Text = "";

          LearnSingleSTBCmdBtn.Text = "Learn Selected Command";
        }
      }

      else
      {
        LearnSingleSTBCmdBtn.Enabled = false;
        ClearSingleLearnedSTBCmdBtn.Enabled = false;
      }
    }

    private void ActionsCheckList_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      int selIndex = ActionsCheckList.SelectedIndex;

      if (selIndex != -1)
      {
        LearnSingleMPCmdBtn.Enabled = true;
        ClearSingleLearnedMPCmdBtn.Enabled = true;
        string irCmd1 = string.Empty;
        string irCmd2 = string.Empty;

        if (MediaPortal.IR.USBUIRT.Instance.GetCommandIrStrings((Action.ActionType)commands[selIndex], ref irCmd1,
                                                                ref irCmd2))
        {
          MPCode1ValueLbl.ForeColor = SystemColors.ControlText;
          MPCode1ValueLbl.Text = irCmd1;
          MPCode2ValueLbl.Text = irCmd2;

          LearnSingleMPCmdBtn.Text = "Relearn Selected Command";
        }

        else
        {
          if (ActionsCheckList.GetItemChecked(selIndex))
            MPCode1ValueLbl.ForeColor = Color.Red;

          else
            MPCode1ValueLbl.ForeColor = SystemColors.ControlText;

          MPCode1ValueLbl.Text = "No code(s) Learned for this action";
          MPCode2ValueLbl.Text = "";

          LearnSingleMPCmdBtn.Text = "Learn Selected Command";
        }
      }

      else
      {
        LearnSingleMPCmdBtn.Enabled = false;
        ClearSingleLearnedMPCmdBtn.Enabled = false;
      }
    }

    #endregion

    #region RxLearningEvents

    private void Instance_StartRxLearning(object sender, LearningEventArgs e)
    {
      SetStartRxLearningStatus(e);
    }

    private void SetStartRxLearningStatus(LearningEventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetLearningStatusCallback d = new SetLearningStatusCallback(SetStartRxLearningStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        receivedIrLbl.Text = e.IrCode;
        LearnStatusLbl.Text = string.Format("Learning Media Portal Control Command: {0} of {1}", e.CurrentCodeCount + 1,
                                            e.TotalCodeCount);

        LearnPrgBar.Maximum = e.TotalCodeCount;
        LearnPrgBar.Value = e.CurrentCodeCount;
        LearnStatusPnl.BringToFront();

        if (!e.IsToggledIrCode)
        {
          statusLabel.ForeColor = Color.DarkBlue;
          statusLabel.Text = "Step 1 of 2:\r\nPress and hold the [" + e.Button + "] button on your remote";
        }

        else
        {
          statusLabel.ForeColor = Color.Red;
          statusLabel.Text = "Step 2 of 2:\r\nPress and hold the [" + e.Button + "] button on your remote";
        }

        System.Windows.Forms.Application.DoEvents();
      }
    }

    private void Instance_OnRxEventLearned(object sender, LearningEventArgs e)
    {
      SetRxEventLearnedStatus(e);
    }

    private void SetRxEventLearnedStatus(LearningEventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetLearningStatusCallback d = new SetLearningStatusCallback(SetRxEventLearnedStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        receivedIrLbl.Text = e.IrCode;
        string msg;
        MessageBoxIcon mbIcon = MessageBoxIcon.Information;

        if (e.Succeeded)
        {
          int passNumber = 1;

          if (e.IsToggledIrCode)
            passNumber = 2;

          msg = string.Format("Successfully learned IR code {0} of 2 for button:\t\r\n  [{1}]", passNumber, e.Button);
        }

        else
        {
          msg = string.Format("Failed to learn IR code for button:\t\r\n  [{0}]", e.Button);
        }

        MediaPortal.IR.USBUIRT.Instance.ReceiveEnabled = false;
        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, mbIcon);
        MediaPortal.IR.USBUIRT.Instance.ReceiveEnabled = true;

        this.statusLabel.Text = "";
      }
    }

    private void Instance_OnEndRxLearning(object sender, EventArgs e)
    {
      SetEndRxLearningStatus(e);
    }

    private void SetEndRxLearningStatus(EventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetEndLearningCallback d = new SetEndLearningCallback(SetEndRxLearningStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        statusLabel.Text = "";
        receivedIrLbl.Text = "";
        string msg = "";
        MessageBoxIcon mbIcon = MessageBoxIcon.Information;

        MediaPortal.IR.USBUIRT.Instance.StartLearning -=
          new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
        MediaPortal.IR.USBUIRT.Instance.OnEventLearned -=
          new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
        MediaPortal.IR.USBUIRT.Instance.OnEndLearning -=
          new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxLearning);

        if (MediaPortal.IR.USBUIRT.Instance.AbortLearn)
        {
          LearnPrgBar.Value = 0;
          msg = "Media Portal Control IR training aborted by user.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Media Portal Control IR training aborted by user!";
        }

        else if (MediaPortal.IR.USBUIRT.Instance.SaveInternalValues())
        {
          LearnPrgBar.Value = LearnPrgBar.Maximum;
          msg = "Media Portal Control IR training completed successfully.\t";
          mbIcon = MessageBoxIcon.Information;
          LearnStatusLbl.Text = "Media Portal Control IR training completed successfully!";
        }

        else
        {
          LearnPrgBar.Value = 0;
          msg = "Media Portal Control IR training Failed: Unable to save IR code settings to file.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Media Portal Control IR training Failed!";
        }

        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, mbIcon);
        this.Close();
      }
    }

    private void Instance_OnEndRxSingleCommandLearning(object sender, EventArgs e)
    {
      SetEndRxSingleCommandLearningStatus(e);
    }

    private void SetEndRxSingleCommandLearningStatus(EventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetEndLearningCallback d = new SetEndLearningCallback(SetEndRxSingleCommandLearningStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        MPCommandsPnl.BringToFront();
        statusLabel.Text = "";
        receivedIrLbl.Text = "";
        string msg = "";
        MessageBoxIcon mbIcon = MessageBoxIcon.Information;

        MediaPortal.IR.USBUIRT.Instance.StartLearning -=
          new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
        MediaPortal.IR.USBUIRT.Instance.OnEventLearned -=
          new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
        MediaPortal.IR.USBUIRT.Instance.OnEndLearning -=
          new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxSingleCommandLearning);

        if (MediaPortal.IR.USBUIRT.Instance.AbortLearn)
        {
          LearnPrgBar.Value = 0;
          msg = "Media Portal Control IR training aborted by user.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Media Portal Control IR training aborted by user!";
        }

        else if (MediaPortal.IR.USBUIRT.Instance.SaveInternalValues())
        {
          LearnPrgBar.Value = LearnPrgBar.Maximum;
          msg = "Media Portal Control IR training completed successfully.\t";
          mbIcon = MessageBoxIcon.Information;
          LearnStatusLbl.Text = "Media Portal Control IR training completed successfully!";
        }

        else
        {
          LearnPrgBar.Value = 0;
          msg = "Media Portal Control IR training Failed: Unable to save IR code settings to file.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Media Portal Control IR training Failed!";
        }

        ActionsCheckList_SelectedIndexChanged(null, null);
        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, mbIcon);
      }
    }

    #endregion

    #region TxLearningEvents

    private void Instance_StartTxLearning(object sender, MediaPortal.IR.LearningEventArgs e)
    {
      SetStartTxLearningStatus(e);
    }

    private void SetStartTxLearningStatus(LearningEventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetLearningStatusCallback d = new SetLearningStatusCallback(SetStartTxLearningStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        receivedIrLbl.Text = e.IrCode;
        LearnStatusLbl.Text = string.Format("Learning Set-top Box Codes: {0} of {1}", e.CurrentCodeCount + 1,
                                            e.TotalCodeCount);

        LearnPrgBar.Maximum = e.TotalCodeCount;
        LearnStatusPnl.BringToFront();

        if (!e.IsToggledIrCode)
        {
          statusLabel.ForeColor = Color.DarkBlue;
          statusLabel.Text = "Step 1 of 2:\r\nPress and hold the [" + e.Button + "] button on your Set-top box remote";
        }

        else
        {
          statusLabel.ForeColor = Color.Red;
          statusLabel.Text = "Step 2 of 2:\r\nPress and hold the [" + e.Button + "] button on your Set-top box remote";
        }

        System.Windows.Forms.Application.DoEvents();
      }
    }

    private void Instance_OnTxEventLearned(object sender, MediaPortal.IR.LearningEventArgs e)
    {
      SetTxEventLearnedStatus(e);
    }

    private void SetTxEventLearnedStatus(LearningEventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetLearningStatusCallback d = new SetLearningStatusCallback(SetTxEventLearnedStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        receivedIrLbl.Text = e.IrCode;

        if (e.IsToggledIrCode)
          this.LearnPrgBar.Value = e.CurrentCodeCount;

        System.Windows.Forms.Application.DoEvents();

        string msg;
        MessageBoxIcon mbIcon = MessageBoxIcon.Information;

        if (e.Succeeded)
        {
          int passNumber = 1;

          if (e.IsToggledIrCode)
            passNumber = 2;

          msg = string.Format("Successfully learned IR code {0} of 2 for button:\t\r\n  [{1}]", passNumber, e.Button);
        }

        else
        {
          msg = string.Format("Failed to learn IR code for button:\t\r\n  [{0}]", e.Button);
          mbIcon = MessageBoxIcon.Exclamation;
        }

        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, mbIcon);
        this.statusLabel.Text = "";
      }
    }

    private void Instance_OnTxEndLearning(object sender, EventArgs e)
    {
      SetEndTxEndLearningStatus(e);
    }

    private void SetEndTxEndLearningStatus(EventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetEndLearningCallback d = new SetEndLearningCallback(SetEndTxEndLearningStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        statusLabel.Text = "";
        receivedIrLbl.Text = "";
        string msg = "";
        MessageBoxIcon mbIcon = MessageBoxIcon.Information;

        MediaPortal.IR.USBUIRT.Instance.StartLearning -=
          new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartTxLearning);
        MediaPortal.IR.USBUIRT.Instance.OnEventLearned -=
          new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnTxEventLearned);
        MediaPortal.IR.USBUIRT.Instance.OnEndLearning -=
          new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnTxEndLearning);

        if (MediaPortal.IR.USBUIRT.Instance.AbortLearn)
        {
          LearnPrgBar.Value = 0;
          msg = "Set top box IR training aborted by user.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Set top box IR training aborted by user!";
        }

        else if (MediaPortal.IR.USBUIRT.Instance.SaveTunerValues())
        {
          LearnPrgBar.Value = LearnPrgBar.Maximum;
          msg = "Set top box IR training completed successfully.\t";
          mbIcon = MessageBoxIcon.Information;
          LearnStatusLbl.Text = "Set top box IR training completed successfully!";
        }

        else
        {
          LearnPrgBar.Value = 0;
          msg = "Set top box IR training Failed: Unable to save IR code settings to file.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Set top box IR training Failed!";
        }

        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, mbIcon);
        this.Close();
      }
    }


    private void Instance_OnTxEndSingleCommandLearning(object sender, EventArgs e)
    {
      SetTxEndSingleCommandLearningStatus(e);
    }

    private void SetTxEndSingleCommandLearningStatus(EventArgs e)
    {
      // Make sure we're calling this method from the correct thread...
      if (receivedIrLbl.InvokeRequired)
      {
        SetEndLearningCallback d = new SetEndLearningCallback(SetTxEndSingleCommandLearningStatus);
        this.Invoke(d, new object[] {e});
      }

      else
      {
        statusLabel.Text = "";
        receivedIrLbl.Text = "";
        string msg = "";
        MessageBoxIcon mbIcon = MessageBoxIcon.Information;

        MediaPortal.IR.USBUIRT.Instance.StartLearning -=
          new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartTxLearning);
        MediaPortal.IR.USBUIRT.Instance.OnEventLearned -=
          new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnTxEventLearned);
        MediaPortal.IR.USBUIRT.Instance.OnEndLearning -=
          new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnTxEndSingleCommandLearning);

        if (MediaPortal.IR.USBUIRT.Instance.AbortLearn)
        {
          LearnPrgBar.Value = 0;
          msg = "Set top box IR training aborted by user.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Set top box IR training aborted by user!";
        }

        else if (MediaPortal.IR.USBUIRT.Instance.SaveTunerValues())
        {
          LearnPrgBar.Value = LearnPrgBar.Maximum;
          msg = "Set top box IR training completed successfully.\t";
          mbIcon = MessageBoxIcon.Information;
          LearnStatusLbl.Text = "Set top box IR training completed successfully!";
        }

        else
        {
          LearnPrgBar.Value = 0;
          msg = "Set top box IR training Failed: Unable to save IR code settings to file.\t";
          mbIcon = MessageBoxIcon.Exclamation;
          LearnStatusLbl.Text = "Set top box IR training Failed!";
        }

        MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, mbIcon);
        this.Close();
      }
    }

    #endregion

    #region StatusBar text display methods

    private void Instance_OnRemoteCommandFeedback(object command, string irCode)
    {
      string actionDesc = "Unknown";

      if (command is MediaPortal.GUI.Library.Action.ActionType)
      {
        if ((Action.ActionType)command != Action.ActionType.ACTION_INVALID)
          actionDesc = ((MediaPortal.GUI.Library.Action.ActionType)command).ToString().Replace("ACTION_", "");
      }

      else if (command is USBUIRT.JumpToActionType)
      {
        actionDesc = ((USBUIRT.JumpToActionType)command).ToString();
      }

      actionDesc = string.Format("[{0}]", actionDesc.Replace("_", " "));

      string msg = string.Format("  Received: {0} — {1}", actionDesc, irCode);
      SetStatusText(msg);
    }

    private void SetStatusText(string text)
    {
      // Make sure we're calling this method from the correct thread...
      if (statusBar1.InvokeRequired)
      {
        SetStatusTextCallback d = new SetStatusTextCallback(SetStatusText);
        this.Invoke(d, new object[] {text});
      }
      else
      {
        statusBar1.Text = text;
      }
    }

    #endregion

    #region Learn progress button click handlers

    private void SkipCodeBtn_Click(object sender, System.EventArgs e)
    {
      MediaPortal.IR.USBUIRT.Instance.SkipLearnForCurrentCode = true;
    }

    private void CancelLearnBtn_Click(object sender, System.EventArgs e)
    {
      MediaPortal.IR.USBUIRT.Instance.AbortLearn = true;
    }

    #endregion

    #region MP command set presets

    private void LoadDefaultSTBCommandSet()
    {
      for (int i = 0; i < StbCommandsChkLst.Items.Count; i++)
      {
        StbCommandsChkLst.SetItemChecked(i, true);
      }

      if (!USBUIRT.Instance.NeedsEnter)
        StbCommandsChkLst.SetItemChecked(StbCommandsChkLst.Items.Count - 1, false);
    }

    private void LoadBasicCommandSet()
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        ActionsCheckList.SetItemChecked(i, (i < 16) ? true : false);
      }
    }

    private void LoadDefaultCommandSet()
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        ActionsCheckList.SetItemChecked(i, (i < 31) ? true : false);
      }
    }

    private void LoadAllCommandSet()
    {
      for (int i = 0; i < ActionsCheckList.Items.Count; i++)
      {
        ActionsCheckList.SetItemChecked(i, true);
      }
    }

    #endregion
  }
}