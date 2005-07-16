using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using MediaPortal.IR;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
	public class USBUIRT : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPCheckBox inputCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox outputCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox digitCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox enterCheckBox;
		private System.Windows.Forms.Button internalCommandsButton;
		private System.Windows.Forms.Button tunerCommandsButton;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblUSBUIRTVersion;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label label3;
		private System.ComponentModel.IContainer components = null;

		public USBUIRT() : this("USBUIRT")
		{
		}

		public USBUIRT(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// 
			// Initialize the USBUIRT component
			//
			MediaPortal.IR.USBUIRT.Create(new MediaPortal.IR.USBUIRT.OnRemoteCommand(OnRemoteCommand));
			lblUSBUIRTVersion.Text=MediaPortal.IR.USBUIRT.Instance.GetVersions();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				inputCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "internal", false) ;
				outputCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "external", false) ;
				digitCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "is3digit", false) ;
				enterCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "needsenter", false) ; 
			}
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("USBUIRT", "internal", inputCheckBox.Checked );
				xmlwriter.SetValueAsBool("USBUIRT", "external", outputCheckBox.Checked );
				xmlwriter.SetValueAsBool("USBUIRT", "is3digit", digitCheckBox.Checked );
				xmlwriter.SetValueAsBool("USBUIRT", "needsenter", enterCheckBox.Checked );			
			}			
		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblUSBUIRTVersion = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.tunerCommandsButton = new System.Windows.Forms.Button();
      this.internalCommandsButton = new System.Windows.Forms.Button();
      this.enterCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.digitCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.outputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.statusLabel = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.lblUSBUIRTVersion);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.tunerCommandsButton);
      this.groupBox1.Controls.Add(this.internalCommandsButton);
      this.groupBox1.Controls.Add(this.enterCheckBox);
      this.groupBox1.Controls.Add(this.digitCheckBox);
      this.groupBox1.Controls.Add(this.outputCheckBox);
      this.groupBox1.Controls.Add(this.inputCheckBox);
      this.groupBox1.Controls.Add(this.statusLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 208);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // lblUSBUIRTVersion
      // 
      this.lblUSBUIRTVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lblUSBUIRTVersion.Location = new System.Drawing.Point(200, 168);
      this.lblUSBUIRTVersion.Name = "lblUSBUIRTVersion";
      this.lblUSBUIRTVersion.Size = new System.Drawing.Size(256, 32);
      this.lblUSBUIRTVersion.TabIndex = 6;
      this.lblUSBUIRTVersion.Text = "Version";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 168);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(144, 16);
      this.label1.TabIndex = 5;
      this.label1.Text = "USBUIRT version detected:";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // tunerCommandsButton
      // 
      this.tunerCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.tunerCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.tunerCommandsButton.Location = new System.Drawing.Point(296, 56);
      this.tunerCommandsButton.Name = "tunerCommandsButton";
      this.tunerCommandsButton.Size = new System.Drawing.Size(160, 23);
      this.tunerCommandsButton.TabIndex = 8;
      this.tunerCommandsButton.Text = "Learn settopbox commands";
      this.tunerCommandsButton.Click += new System.EventHandler(this.tunerCommandsButton_Click);
      // 
      // internalCommandsButton
      // 
      this.internalCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.internalCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.internalCommandsButton.Location = new System.Drawing.Point(296, 24);
      this.internalCommandsButton.Name = "internalCommandsButton";
      this.internalCommandsButton.Size = new System.Drawing.Size(160, 23);
      this.internalCommandsButton.TabIndex = 7;
      this.internalCommandsButton.Text = "Learn Media Portal commands";
      this.internalCommandsButton.Click += new System.EventHandler(this.internalCommandsButton_Click);
      // 
      // enterCheckBox
      // 
      this.enterCheckBox.Enabled = false;
      this.enterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.enterCheckBox.Location = new System.Drawing.Point(32, 96);
      this.enterCheckBox.Name = "enterCheckBox";
      this.enterCheckBox.Size = new System.Drawing.Size(192, 16);
      this.enterCheckBox.TabIndex = 3;
      this.enterCheckBox.Text = "Send \'Enter\' for changing channels";
      // 
      // digitCheckBox
      // 
      this.digitCheckBox.Enabled = false;
      this.digitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.digitCheckBox.Location = new System.Drawing.Point(32, 72);
      this.digitCheckBox.Name = "digitCheckBox";
      this.digitCheckBox.Size = new System.Drawing.Size(176, 16);
      this.digitCheckBox.TabIndex = 2;
      this.digitCheckBox.Text = "Use 3 digits for channel selection";
      // 
      // outputCheckBox
      // 
      this.outputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.outputCheckBox.Location = new System.Drawing.Point(16, 48);
      this.outputCheckBox.Name = "outputCheckBox";
      this.outputCheckBox.Size = new System.Drawing.Size(208, 16);
      this.outputCheckBox.TabIndex = 1;
      this.outputCheckBox.Text = "Let Media Portal control your settopbox";
      this.outputCheckBox.CheckedChanged += new System.EventHandler(this.outputCheckBox_CheckedChanged);
      // 
      // inputCheckBox
      // 
      this.inputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.inputCheckBox.Location = new System.Drawing.Point(16, 24);
      this.inputCheckBox.Name = "inputCheckBox";
      this.inputCheckBox.Size = new System.Drawing.Size(208, 16);
      this.inputCheckBox.TabIndex = 0;
      this.inputCheckBox.Text = "Use your remote to control Media Portal";
      // 
      // statusLabel
      // 
      this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.statusLabel.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.statusLabel.Location = new System.Drawing.Point(16, 128);
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(440, 24);
      this.statusLabel.TabIndex = 4;
      this.statusLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.linkLabel1);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(0, 216);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 104);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "General Information";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 72);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(96, 16);
      this.label3.TabIndex = 1;
      this.label3.Text = "More information:";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Location = new System.Drawing.Point(112, 72);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(120, 16);
      this.linkLabel1.TabIndex = 2;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://www.usbuirt.com";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(16, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(448, 40);
      this.label2.TabIndex = 0;
      this.label2.Text = "The USBUIRT is an external USB device which allows Mediaportal to both Transmit a" +
        "nd Receive infrared signals. With USBUIRT you can tell mediaportal to remote con" +
        "trol your settop box and/or you can control mediaportal using any remote control" +
        " you may have.";
      // 
      // USBUIRT
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "USBUIRT";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void internalCommandsButton_Click(object sender, System.EventArgs e)
		{
			object[] commands = { 	Action.ActionType.ACTION_MOVE_LEFT
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
								};

			string[] buttonNames = {  "LEFT",
									   "RIGHT",
									   "UP",
									   "DOWN",
									   "PAGE_UP",
									   "PAGE_DOWN",
									   "SELECT_ITEM",
									   "PREVIOUS_MENU",
									   "SHOW_INFO",
									   "PAUSE",
									   "STOP",
									   "FORWARD",
									   "REWIND",
									   "FULLSCREEN",
									   "QUEUE",
			};
			
			statusLabel.Text="";
			SaveSettings();
			MediaPortal.IR.USBUIRT.Create(new MediaPortal.IR.USBUIRT.OnRemoteCommand(OnRemoteCommand));

			MediaPortal.IR.USBUIRT.Instance.StartLearning += new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartLearning);
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned +=new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnEventLearned);

			MediaPortal.IR.USBUIRT.Instance.BulkLearn(commands, buttonNames);
			MediaPortal.IR.USBUIRT.Instance.SaveInternalValues();

			MediaPortal.IR.USBUIRT.Instance.StartLearning -= new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartLearning);
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned -=new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnEventLearned);
			
			statusLabel.Text="";
		}

		private void tunerCommandsButton_Click(object sender, System.EventArgs e)
		{
			statusLabel.Text="";
			SaveSettings();
			MediaPortal.IR.USBUIRT.Create(new MediaPortal.IR.USBUIRT.OnRemoteCommand(OnRemoteCommand));
			lblUSBUIRTVersion.Text=MediaPortal.IR.USBUIRT.Instance.GetVersions();
			MediaPortal.IR.USBUIRT.Instance.StartLearning += new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartLearning);
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned +=new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnEventLearned);

			MediaPortal.IR.USBUIRT.Instance.LearnTunerCodes();
			MediaPortal.IR.USBUIRT.Instance.SaveTunerValues();

			MediaPortal.IR.USBUIRT.Instance.StartLearning -= new MediaPortal.IR.USBUIRT.StartLearningEventHandler(Instance_StartLearning);			
			MediaPortal.IR.USBUIRT.Instance.OnEventLearned -=new MediaPortal.IR.USBUIRT.EventLearnedHandler(Instance_OnEventLearned);
			
			statusLabel.Text="";
		}

		private void outputCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			digitCheckBox.Enabled = enterCheckBox.Enabled = outputCheckBox.Checked;
		}

		private void Instance_StartLearning(object sender, LearningEventArgs e)
		{
			statusLabel.Text = "Press and hold the '" + e.Button + "' button on your remote";
			Application.DoEvents();
		}

		private void OnRemoteCommand(object command)
		{
//			lblReceived.Text=String.Format("Remote Command = " + command.ToString());
		}

		private void label1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void Instance_OnEventLearned(object sender, LearningEventArgs e)
		{
			if (e.Succeeded)
			{
				MessageBox.Show("Successfully learned IR code for button:"+e.Button,
												"USBUIRT",MessageBoxButtons.OK, MessageBoxIcon.Information);
        
			}
			else
			{
				MessageBox.Show("Failed to learn IR code for button:"+e.Button,
					"USBUIRT",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
	}
}

