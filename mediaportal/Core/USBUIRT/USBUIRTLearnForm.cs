using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.IR
{
	/// <summary>
	/// Summary description for USBUIRTLearnForm.
	/// </summary>
	public class	USBUIRTLearnForm : System.Windows.Forms.Form
	{
		public enum LearnType{SetTopBoxCommands, MediaPortalCommands};
		private System.Collections.ArrayList buttonNames = new ArrayList();

		#region Command Actions
		object[] commands = {
								Action.ActionType.ACTION_MOVE_LEFT,
								Action.ActionType.ACTION_MOVE_RIGHT,             
								Action.ActionType.ACTION_MOVE_UP,            
								Action.ActionType.ACTION_MOVE_DOWN,              
								Action.ActionType.ACTION_PAGE_UP,             
								Action.ActionType.ACTION_PAGE_DOWN,              
								Action.ActionType.ACTION_SELECT_ITEM,            
								Action.ActionType.ACTION_PREVIOUS_MENU,          
								Action.ActionType.ACTION_SHOW_INFO,
								Action.ActionType.ACTION_PAUSE,
								Action.ActionType.ACTION_STOP,
								Action.ActionType.ACTION_FORWARD,
								Action.ActionType.ACTION_REWIND,
								Action.ActionType.ACTION_SHOW_GUI,
								Action.ActionType.ACTION_QUEUE_ITEM,
								Action.ActionType.ACTION_EXIT,

								Action.ActionType.ACTION_SHUTDOWN,
								Action.ActionType.ACTION_ASPECT_RATIO,
								Action.ActionType.ACTION_PLAY,
								Action.ActionType.ACTION_EJECTCD,
								Action.ActionType.ACTION_PREV_CHANNEL,
								Action.ActionType.ACTION_NEXT_CHANNEL,
								Action.ActionType.ACTION_RECORD,
								Action.ActionType.ACTION_DVD_MENU,
								Action.ActionType.ACTION_NEXT_CHAPTER,
								Action.ActionType.ACTION_PREV_CHAPTER,
								Action.ActionType.ACTION_VOLUME_DOWN,
								Action.ActionType.ACTION_VOLUME_UP,
								Action.ActionType.ACTION_VOLUME_MUTE,
								Action.ActionType.ACTION_AUDIO_NEXT_LANGUAGE,
								Action.ActionType.ACTION_SHOW_SUBTITLES,
								Action.ActionType.ACTION_NEXT_SUBTITLE,

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
								Action.ActionType.ACTION_TOGGLE_WINDOWED_FULSLCREEN,
								Action.ActionType.ACTION_REBOOT,

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

        delegate void SetStatusTextCallback(string text);
        delegate void SetLearningStatusCallback(LearningEventArgs e);
        delegate void SetEndLearningCallback(EventArgs e);

        #endregion

        #region Controls

        private System.Windows.Forms.Button internalCommandsButton;
		private System.Windows.Forms.CheckedListBox ActionsCheckList;
		private System.Windows.Forms.Button LearnSelectedCommandBtn;
		private System.Windows.Forms.Panel MPCommandsPnl;
		private System.Windows.Forms.Button CancelLearnBtn;
		private System.Windows.Forms.ProgressBar LearnPrgBar;
		private System.Windows.Forms.Label LearnStatusLbl;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label receivedIrLbl;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Button SkipCodeBtn;
		private System.Windows.Forms.Panel LearnStatusPnl;
		private System.Windows.Forms.Button CancelBtn;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label Code1ValueLbl;
		private System.Windows.Forms.Label Code2ValueLbl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ClearSelectedLearnedCommandsBtn;
		private System.Windows.Forms.Button ClearAllLearnedCommandsBtn;
		private System.Windows.Forms.StatusBar statusBar1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        #endregion

        public USBUIRTLearnForm(LearnType learnType)
		{
			InitializeComponent();

			MediaPortal.IR.USBUIRT.Instance.OnRemoteCommandFeedback += new MediaPortal.IR.USBUIRT.RemoteCommandFeedbackHandler(Instance_OnRemoteCommandFeedback);

			if(learnType == LearnType.MediaPortalCommands)
			{
				MPCommandsPnl.BringToFront();
				CreateButtonNames();
				this.ActionsCheckList.Items.AddRange(buttonNames.ToArray());

				MPCommandsPnl.BringToFront();
				this.Text = "Media Portal Control Commands";
			}

			else
			{
				LearnStatusPnl.BringToFront();
				this.Text = "Set Top Box Control Commands";
				DoSetTopBoxCommandLearn();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.internalCommandsButton = new System.Windows.Forms.Button();
            this.ActionsCheckList = new System.Windows.Forms.CheckedListBox();
            this.LearnSelectedCommandBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.MPCommandsPnl = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Code1ValueLbl = new System.Windows.Forms.Label();
            this.Code2ValueLbl = new System.Windows.Forms.Label();
            this.ClearSelectedLearnedCommandsBtn = new System.Windows.Forms.Button();
            this.ClearAllLearnedCommandsBtn = new System.Windows.Forms.Button();
            this.CancelLearnBtn = new System.Windows.Forms.Button();
            this.LearnPrgBar = new System.Windows.Forms.ProgressBar();
            this.LearnStatusLbl = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.receivedIrLbl = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SkipCodeBtn = new System.Windows.Forms.Button();
            this.LearnStatusPnl = new System.Windows.Forms.Panel();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.MPCommandsPnl.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.LearnStatusPnl.SuspendLayout();
            this.SuspendLayout();
            // 
            // internalCommandsButton
            // 
            this.internalCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.internalCommandsButton.Location = new System.Drawing.Point(302, 24);
            this.internalCommandsButton.Name = "internalCommandsButton";
            this.internalCommandsButton.Size = new System.Drawing.Size(140, 23);
            this.internalCommandsButton.TabIndex = 19;
            this.internalCommandsButton.Text = "Learn Enabled Commands";
            this.internalCommandsButton.Click += new System.EventHandler(this.internalCommandsButton_Click);
            // 
            // ActionsCheckList
            // 
            this.ActionsCheckList.Location = new System.Drawing.Point(14, 24);
            this.ActionsCheckList.Name = "ActionsCheckList";
            this.ActionsCheckList.Size = new System.Drawing.Size(270, 169);
            this.ActionsCheckList.TabIndex = 0;
            this.ActionsCheckList.SelectedIndexChanged += new System.EventHandler(this.ActionsCheckList_SelectedIndexChanged);
            // 
            // LearnSelectedCommandBtn
            // 
            this.LearnSelectedCommandBtn.Enabled = false;
            this.LearnSelectedCommandBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LearnSelectedCommandBtn.Location = new System.Drawing.Point(302, 51);
            this.LearnSelectedCommandBtn.Name = "LearnSelectedCommandBtn";
            this.LearnSelectedCommandBtn.Size = new System.Drawing.Size(140, 23);
            this.LearnSelectedCommandBtn.TabIndex = 19;
            this.LearnSelectedCommandBtn.Text = "Learn Selected Command";
            this.LearnSelectedCommandBtn.Click += new System.EventHandler(this.LearnSelectedCommandBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CancelBtn.Location = new System.Drawing.Point(302, 240);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(140, 23);
            this.CancelBtn.TabIndex = 1;
            this.CancelBtn.Text = "Close";
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // MPCommandsPnl
            // 
            this.MPCommandsPnl.Controls.Add(this.label1);
            this.MPCommandsPnl.Controls.Add(this.groupBox2);
            this.MPCommandsPnl.Controls.Add(this.internalCommandsButton);
            this.MPCommandsPnl.Controls.Add(this.ActionsCheckList);
            this.MPCommandsPnl.Controls.Add(this.LearnSelectedCommandBtn);
            this.MPCommandsPnl.Controls.Add(this.CancelBtn);
            this.MPCommandsPnl.Controls.Add(this.ClearSelectedLearnedCommandsBtn);
            this.MPCommandsPnl.Controls.Add(this.ClearAllLearnedCommandsBtn);
            this.MPCommandsPnl.Location = new System.Drawing.Point(2, 0);
            this.MPCommandsPnl.Name = "MPCommandsPnl";
            this.MPCommandsPnl.Size = new System.Drawing.Size(456, 276);
            this.MPCommandsPnl.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Media Portal Actions";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.Code1ValueLbl);
            this.groupBox2.Controls.Add(this.Code2ValueLbl);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Location = new System.Drawing.Point(14, 196);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(270, 68);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Learned IR Code(s)";
            // 
            // Code1ValueLbl
            // 
            this.Code1ValueLbl.Location = new System.Drawing.Point(16, 24);
            this.Code1ValueLbl.Name = "Code1ValueLbl";
            this.Code1ValueLbl.Size = new System.Drawing.Size(240, 16);
            this.Code1ValueLbl.TabIndex = 22;
            // 
            // Code2ValueLbl
            // 
            this.Code2ValueLbl.Location = new System.Drawing.Point(16, 40);
            this.Code2ValueLbl.Name = "Code2ValueLbl";
            this.Code2ValueLbl.Size = new System.Drawing.Size(240, 16);
            this.Code2ValueLbl.TabIndex = 22;
            // 
            // ClearSelectedLearnedCommandsBtn
            // 
            this.ClearSelectedLearnedCommandsBtn.Enabled = false;
            this.ClearSelectedLearnedCommandsBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ClearSelectedLearnedCommandsBtn.Location = new System.Drawing.Point(302, 124);
            this.ClearSelectedLearnedCommandsBtn.Name = "ClearSelectedLearnedCommandsBtn";
            this.ClearSelectedLearnedCommandsBtn.Size = new System.Drawing.Size(140, 23);
            this.ClearSelectedLearnedCommandsBtn.TabIndex = 19;
            this.ClearSelectedLearnedCommandsBtn.Text = "Clear Selected Learn";
            this.ClearSelectedLearnedCommandsBtn.Click += new System.EventHandler(this.ClearSelectedLearnedCommandsBtn_Click);
            // 
            // ClearAllLearnedCommandsBtn
            // 
            this.ClearAllLearnedCommandsBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ClearAllLearnedCommandsBtn.Location = new System.Drawing.Point(302, 96);
            this.ClearAllLearnedCommandsBtn.Name = "ClearAllLearnedCommandsBtn";
            this.ClearAllLearnedCommandsBtn.Size = new System.Drawing.Size(140, 23);
            this.ClearAllLearnedCommandsBtn.TabIndex = 19;
            this.ClearAllLearnedCommandsBtn.Text = "Clear All Learned";
            this.ClearAllLearnedCommandsBtn.Click += new System.EventHandler(this.ClearAllLearnedCommandsBtn_Click);
            // 
            // CancelLearnBtn
            // 
            this.CancelLearnBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CancelLearnBtn.Location = new System.Drawing.Point(344, 244);
            this.CancelLearnBtn.Name = "CancelLearnBtn";
            this.CancelLearnBtn.Size = new System.Drawing.Size(88, 23);
            this.CancelLearnBtn.TabIndex = 3;
            this.CancelLearnBtn.Text = "Cancel Learn";
            this.CancelLearnBtn.Click += new System.EventHandler(this.CancelLearnBtn_Click);
            // 
            // LearnPrgBar
            // 
            this.LearnPrgBar.Location = new System.Drawing.Point(24, 224);
            this.LearnPrgBar.Name = "LearnPrgBar";
            this.LearnPrgBar.Size = new System.Drawing.Size(408, 12);
            this.LearnPrgBar.TabIndex = 2;
            // 
            // LearnStatusLbl
            // 
            this.LearnStatusLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LearnStatusLbl.Location = new System.Drawing.Point(24, 16);
            this.LearnStatusLbl.Name = "LearnStatusLbl";
            this.LearnStatusLbl.Size = new System.Drawing.Size(408, 16);
            this.LearnStatusLbl.TabIndex = 1;
            this.LearnStatusLbl.Text = "Learn Status:";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Window;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.receivedIrLbl);
            this.panel2.Controls.Add(this.statusLabel);
            this.panel2.Location = new System.Drawing.Point(24, 32);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(20);
            this.panel2.Size = new System.Drawing.Size(408, 188);
            this.panel2.TabIndex = 0;
            // 
            // receivedIrLbl
            // 
            this.receivedIrLbl.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.receivedIrLbl.Location = new System.Drawing.Point(20, 152);
            this.receivedIrLbl.Name = "receivedIrLbl";
            this.receivedIrLbl.Size = new System.Drawing.Size(366, 28);
            this.receivedIrLbl.TabIndex = 5;
            // 
            // statusLabel
            // 
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusLabel.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(20, 20);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(366, 128);
            this.statusLabel.TabIndex = 4;
            // 
            // SkipCodeBtn
            // 
            this.SkipCodeBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SkipCodeBtn.Location = new System.Drawing.Point(24, 244);
            this.SkipCodeBtn.Name = "SkipCodeBtn";
            this.SkipCodeBtn.Size = new System.Drawing.Size(88, 23);
            this.SkipCodeBtn.TabIndex = 3;
            this.SkipCodeBtn.Text = "Skip this Code";
            this.SkipCodeBtn.Click += new System.EventHandler(this.SkipCodeBtn_Click);
            // 
            // LearnStatusPnl
            // 
            this.LearnStatusPnl.Controls.Add(this.CancelLearnBtn);
            this.LearnStatusPnl.Controls.Add(this.LearnPrgBar);
            this.LearnStatusPnl.Controls.Add(this.LearnStatusLbl);
            this.LearnStatusPnl.Controls.Add(this.panel2);
            this.LearnStatusPnl.Controls.Add(this.SkipCodeBtn);
            this.LearnStatusPnl.Location = new System.Drawing.Point(2, 0);
            this.LearnStatusPnl.Name = "LearnStatusPnl";
            this.LearnStatusPnl.Size = new System.Drawing.Size(456, 276);
            this.LearnStatusPnl.TabIndex = 21;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 278);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(460, 22);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 22;
            // 
            // USBUIRTLearnForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(460, 300);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.MPCommandsPnl);
            this.Controls.Add(this.LearnStatusPnl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "USBUIRTLearnForm";
            this.ShowInTaskbar = false;
            this.Text = " USBUIRTLearnForm";
            this.MPCommandsPnl.ResumeLayout(false);
            this.MPCommandsPnl.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.LearnStatusPnl.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);

			ResetActionsCheckList();
			Hashtable mpCommandsTable = MediaPortal.IR.USBUIRT.Instance.LearnedMediaPortalCodesTable;

			if(mpCommandsTable == null)
			{
				this.LoadDefaultCommandSet();
				return;
			}

			foreach(object entry in mpCommandsTable.Keys) 
			{
				string irCode = entry.ToString();
				object command = mpCommandsTable[irCode];

				EnableActionsCheckListItem((Action.ActionType)command);
			}

			if(ActionsCheckList.Items.Count > 0)
				ActionsCheckList.SelectedIndex = 0;
		}

		private void ResetActionsCheckList()
		{
			for(int i = 0; i < ActionsCheckList.Items.Count; i++)
				ActionsCheckList.SetItemChecked(i, false);
		}

		private void EnableActionsCheckListItem(Action.ActionType action)
		{
			for(int i = 0; i < ActionsCheckList.Items.Count; i++)
			{
				Action.ActionType curAction = (Action.ActionType)commands[i];

				if(curAction == action)
				{
					ActionsCheckList.SetItemChecked(i, true);
					break;
				}
			}
		}

		private void DoMediaPortalCommandLearn()
		{
			int count =	ActionsCheckList.CheckedItems.Count;

			object[] learncommands = new object[count];
			string[] learnbuttons = new string[count];
			int index = 0;

			for(int i = 0; i < ActionsCheckList.Items.Count; i++)
			{
				if(ActionsCheckList.GetItemChecked(i))
				{
					learncommands[index] = commands[i];
					learnbuttons[index] = (string)buttonNames[i];
					index++;
				}		
			}

			MediaPortal.IR.USBUIRT.Instance.StartLearning += new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned += new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
			MediaPortal.IR.USBUIRT.Instance.OnEndLearning += new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxLearning);
			MediaPortal.IR.USBUIRT.Instance.BulkLearn(learncommands, learnbuttons);
		}

		private void DoSetTopBoxCommandLearn()
		{
			MediaPortal.IR.USBUIRT.Instance.StartLearning += new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartTxLearning);
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned += new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnTxEventLearned);
			MediaPortal.IR.USBUIRT.Instance.OnEndLearning += new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnTxEndLearning);
			
			MediaPortal.IR.USBUIRT.Instance.LearnTunerCodes();
		}

		private void CreateButtonNames()
		{
			this.buttonNames.Clear();

			for(int i = 0; i < commands.Length; i++)
			{
				string curCmd = string.Empty;

				if(commands[i] is Action.ActionType) 
				{
					Action.ActionType curActionType = (Action.ActionType)commands[i];
					curCmd = curActionType.ToString().Replace("ACTION_", "");

					// hack to fix incorrectly spelled MP ActionType
					if(curCmd == "TOGGLE_WINDOWED_FULSLCREEN")
						curCmd = "TOGGLE_WINDOWED_FULLSCREEN";

					curCmd = curCmd.Replace("_", " ");
				}

				else if(commands[i] is USBUIRT.JumpToActionType)
				{
					curCmd = commands[i].ToString();
					curCmd = curCmd.Replace("_", " ");
				}

				else
					continue;

				buttonNames.Add(curCmd);
			}		
		}

		private void internalCommandsButton_Click(object sender, System.EventArgs e)
		{
			DoMediaPortalCommandLearn();
		}

		private void LearnSelectedCommandBtn_Click(object sender, System.EventArgs e)
		{
			int selIndex = ActionsCheckList.SelectedIndex;

			if(selIndex == -1 || !ActionsCheckList.GetItemChecked(selIndex))
			{
				MessageBox.Show(this, "No commands selected.  Please select a command.\t", "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			// Make sure we clear any existing ir code assignements for this action
			if(MediaPortal.IR.USBUIRT.Instance.ClearLearnedCommand((Action.ActionType)commands[selIndex]))
			{
				//ActionsCheckList.SetItemChecked(selIndex, false);
				ActionsCheckList_SelectedIndexChanged(null, null);
			}

			object[] learncommands = new object[1];
			string[] learnbuttons = new string[1];

			learncommands[0] = commands[selIndex];
			learnbuttons[0] = (string)buttonNames[selIndex];
		
			MediaPortal.IR.USBUIRT.Instance.StartLearning += new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned += new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
			MediaPortal.IR.USBUIRT.Instance.OnEndLearning += new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxSingleCommandLearning);
			MediaPortal.IR.USBUIRT.Instance.BulkLearn(learncommands, learnbuttons);
		}

		private void ClearAllLearnedCommandsBtn_Click(object sender, System.EventArgs e)
		{
			if(MediaPortal.IR.USBUIRT.Instance.LearnedMediaPortalCodesTable.Count == 0)
			{
				MessageBox.Show(this, "No Media Portal control IR codes loaded.  Nothing to clear.\t", "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			string msg = "You are about to clear all learned IR codes.\t\r\n\r\nContinue?";
			
			if(MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
			{
				if(MediaPortal.IR.USBUIRT.Instance.ClearAllLearnedCommands())
				{
					ActionsCheckList_SelectedIndexChanged(null, null);
					MessageBox.Show(this, "All learned Media Portal control IR codes cleared.\t", "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private void ClearSelectedLearnedCommandsBtn_Click(object sender, System.EventArgs e)
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

		private void ActionsCheckList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int selIndex = ActionsCheckList.SelectedIndex;

			if(selIndex != -1)
			{
				LearnSelectedCommandBtn.Enabled = true;
				ClearSelectedLearnedCommandsBtn.Enabled = true;
				string irCmd1 = string.Empty;
				string irCmd2 = string.Empty;

                if(MediaPortal.IR.USBUIRT.Instance.GetCommandIrStrings((Action.ActionType)commands[selIndex], ref irCmd1, ref irCmd2))
				{
					Code1ValueLbl.ForeColor = SystemColors.ControlText;
					Code1ValueLbl.Text = irCmd1;
					Code2ValueLbl.Text = irCmd2;

					LearnSelectedCommandBtn.Text = "Relearn Selected Command";
				}

				else
				{
					if(ActionsCheckList.GetItemChecked(selIndex))
						Code1ValueLbl.ForeColor = Color.Red;

					else
						Code1ValueLbl.ForeColor = SystemColors.ControlText;
					
					Code1ValueLbl.Text = "No code(s) Learned for this action";
					Code2ValueLbl.Text = "";

					LearnSelectedCommandBtn.Text = "Learn Selected Command";
				}
			}

			else
			{
				LearnSelectedCommandBtn.Enabled = false;
				ClearSelectedLearnedCommandsBtn.Enabled = false;
			}
		}

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
                LearnStatusLbl.Text = string.Format("Learning Media Portal Control Command: {0} of {1}", e.CurrentCodeCount + 1, e.TotalCodeCount);

                LearnPrgBar.Maximum = e.TotalCodeCount;
                LearnPrgBar.Value = e.CurrentCodeCount;
                LearnStatusPnl.BringToFront();

                if (!e.IsToggledIrCode)
                {
                    statusLabel.ForeColor = Color.DarkBlue;
                    statusLabel.Text = "Step 1 of 2:\r\nPress and hold the '" + e.Button + "' button on your remote";
                }

                else
                {
                    statusLabel.ForeColor = Color.Red;
                    statusLabel.Text = "Step 2 of 2:\r\nPress and hold the '" + e.Button + "' button on your remote";
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
                this.Invoke(d, new object[] { e });
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

                    msg = string.Format("Successfully learned IR code {0} of 2 for button:\r\n  '{1}'", passNumber, e.Button);
                }

                else
                {
                    msg = string.Format("Failed to learn IR code for button:\r\n  '{0}'", e.Button);
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
                this.Invoke(d, new object[] { e });
            }

            else
            {
                statusLabel.Text = "";
                receivedIrLbl.Text = "";
                string msg = "";
                MessageBoxIcon mbIcon = MessageBoxIcon.Information;

                MediaPortal.IR.USBUIRT.Instance.StartLearning -= new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
                MediaPortal.IR.USBUIRT.Instance.OnEventLearned -= new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
                MediaPortal.IR.USBUIRT.Instance.OnEndLearning -= new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxLearning);

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
                this.Invoke(d, new object[] { e });
            }

            else
            {
                MPCommandsPnl.BringToFront();
                statusLabel.Text = "";
                receivedIrLbl.Text = "";
                string msg = "";
                MessageBoxIcon mbIcon = MessageBoxIcon.Information;

                MediaPortal.IR.USBUIRT.Instance.StartLearning -= new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartRxLearning);
                MediaPortal.IR.USBUIRT.Instance.OnEventLearned -= new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnRxEventLearned);
                MediaPortal.IR.USBUIRT.Instance.OnEndLearning -= new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnEndRxSingleCommandLearning);

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
                this.Invoke(d, new object[] { e });
            }

            else
            {
                receivedIrLbl.Text = e.IrCode;
                LearnStatusLbl.Text = string.Format("Learning Set Top Box Codes: {0} of {1}", e.CurrentCodeCount + 1, e.TotalCodeCount);

                LearnPrgBar.Maximum = e.TotalCodeCount;
                LearnPrgBar.Value = e.CurrentCodeCount;

                LearnStatusPnl.BringToFront();
                statusLabel.ForeColor = Color.DarkBlue;
                statusLabel.Text = "Press and hold the '" + e.Button + "' button on your remote";
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
                this.Invoke(d, new object[] { e });
            }

            else
            {
                receivedIrLbl.Text = e.IrCode;
                this.LearnPrgBar.Value = e.CurrentCodeCount;
                string msg;
                MessageBoxIcon mbIcon = MessageBoxIcon.Information;

                if (e.Succeeded)
                {
                    msg = string.Format("Successfully learned IR code for button:\r\n  {0}", e.Button);
                }

                else
                {
                    msg = string.Format("Failed to learn IR code for button:\r\n  {0}", e.Button);
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
                this.Invoke(d, new object[] { e });
            }

            else
            {
                statusLabel.Text = "";
                receivedIrLbl.Text = "";
                string msg = "";
                MessageBoxIcon mbIcon = MessageBoxIcon.Information;

                MediaPortal.IR.USBUIRT.Instance.StartLearning -= new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartTxLearning);
                MediaPortal.IR.USBUIRT.Instance.OnEventLearned -= new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnTxEventLearned);
                MediaPortal.IR.USBUIRT.Instance.OnEndLearning -= new MediaPortal.IR.USBUIRT.EndLearnedHandler(Instance_OnTxEndLearning);

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

		private void Instance_OnRemoteCommandFeedback(object command, string irCode)
		{
			string actionDesc = "Unknown";
		
			if(command is MediaPortal.GUI.Library.Action.ActionType)
			{
				if((Action.ActionType)command != Action.ActionType.ACTION_INVALID)
					actionDesc = ((MediaPortal.GUI.Library.Action.ActionType)command).ToString().Replace("ACTION_", "");
			}

			else if(command is USBUIRT.JumpToActionType)
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
                this.Invoke(d, new object[] { text });
            }
            else
            {
                statusBar1.Text = text;
            }
        }

        private void SkipCodeBtn_Click(object sender, System.EventArgs e)
		{
			MediaPortal.IR.USBUIRT.Instance.SkipLearnForCurrentCode = true;		
		}

		private void CancelLearnBtn_Click(object sender, System.EventArgs e)
		{
			MediaPortal.IR.USBUIRT.Instance.AbortLearn = true;
		}

		private void CancelBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void LoadBasicCommandSet()
		{
			for(int i = 0; i < ActionsCheckList.Items.Count; i++)
			{
				ActionsCheckList.SetItemChecked(i, (i < 16 ) ? true : false);
			}
		}

		private void LoadDefaultCommandSet()
		{
			for(int i = 0; i < ActionsCheckList.Items.Count; i++)
			{
				ActionsCheckList.SetItemChecked(i, (i < 31 ) ? true : false);
			}		
		}

		private void LoadAllCommandSet()
		{
			for(int i = 0; i < ActionsCheckList.Items.Count; i++)
			{
				ActionsCheckList.SetItemChecked(i, true);
			}		
		}
	}
}
