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
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label statusLabel;
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
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				inputCheckBox.Checked	= xmlreader.GetValueAsString("USBUIRT", "internal", "false") == "true";
				outputCheckBox.Checked	= xmlreader.GetValueAsString("USBUIRT", "external", "false") == "true";
				digitCheckBox.Checked	= xmlreader.GetValueAsString("USBUIRT", "is3digit", "false") == "true";
				enterCheckBox.Checked	= xmlreader.GetValueAsString("USBUIRT", "needsenter", "false") == "true"; 

//				inputCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "internal", false);
//				outputCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "external", false);
//				digitCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "is3digit", false);
//				enterCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "needsenter", false); 
			}
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("USBUIRT", "internal", inputCheckBox.Checked ? "true" : "false");
				xmlwriter.SetValue("USBUIRT", "external", outputCheckBox.Checked ? "true" : "false");
				xmlwriter.SetValue("USBUIRT", "is3digit", digitCheckBox.Checked ? "true" : "false");
				xmlwriter.SetValue("USBUIRT", "needsenter", enterCheckBox.Checked ? "true" : "false");			

				//				xmlwriter.SetValueAsBool("USBUIRT", "internal", inputCheckBox.Checked);
				//				xmlwriter.SetValueAsBool("USBUIRT", "externak", outputCheckBox.Checked);
				//				xmlwriter.SetValueAsBool("USBUIRT", "is3digit", digitCheckBox.Checked);
				//				xmlwriter.SetValueAsBool("USBUIRT", "needsenter", inputCheckBox.Checked);
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
      this.tunerCommandsButton = new System.Windows.Forms.Button();
      this.internalCommandsButton = new System.Windows.Forms.Button();
      this.enterCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.digitCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.outputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.statusLabel = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.tunerCommandsButton);
      this.groupBox1.Controls.Add(this.internalCommandsButton);
      this.groupBox1.Controls.Add(this.enterCheckBox);
      this.groupBox1.Controls.Add(this.digitCheckBox);
      this.groupBox1.Controls.Add(this.outputCheckBox);
      this.groupBox1.Controls.Add(this.inputCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 224);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General settings";
      // 
      // tunerCommandsButton
      // 
      this.tunerCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tunerCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.tunerCommandsButton.Location = new System.Drawing.Point(16, 176);
      this.tunerCommandsButton.Name = "tunerCommandsButton";
      this.tunerCommandsButton.Size = new System.Drawing.Size(408, 23);
      this.tunerCommandsButton.TabIndex = 12;
      this.tunerCommandsButton.Text = "Learn tuner commands";
      this.tunerCommandsButton.Click += new System.EventHandler(this.tunerCommandsButton_Click);
      // 
      // internalCommandsButton
      // 
      this.internalCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.internalCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.internalCommandsButton.Location = new System.Drawing.Point(16, 144);
      this.internalCommandsButton.Name = "internalCommandsButton";
      this.internalCommandsButton.Size = new System.Drawing.Size(408, 23);
      this.internalCommandsButton.TabIndex = 11;
      this.internalCommandsButton.Text = "Learn internal commands";
      this.internalCommandsButton.Click += new System.EventHandler(this.internalCommandsButton_Click);
      // 
      // enterCheckBox
      // 
      this.enterCheckBox.Enabled = false;
      this.enterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.enterCheckBox.Location = new System.Drawing.Point(32, 96);
      this.enterCheckBox.Name = "enterCheckBox";
      this.enterCheckBox.Size = new System.Drawing.Size(312, 24);
      this.enterCheckBox.TabIndex = 10;
      this.enterCheckBox.Text = "Tuner needs \"Enter\" to be sent when changing channel";
      // 
      // digitCheckBox
      // 
      this.digitCheckBox.Enabled = false;
      this.digitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.digitCheckBox.Location = new System.Drawing.Point(32, 72);
      this.digitCheckBox.Name = "digitCheckBox";
      this.digitCheckBox.Size = new System.Drawing.Size(264, 24);
      this.digitCheckBox.TabIndex = 9;
      this.digitCheckBox.Text = "Tuner has three (3) digits";
      // 
      // outputCheckBox
      // 
      this.outputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.outputCheckBox.Location = new System.Drawing.Point(16, 48);
      this.outputCheckBox.Name = "outputCheckBox";
      this.outputCheckBox.Size = new System.Drawing.Size(264, 24);
      this.outputCheckBox.TabIndex = 8;
      this.outputCheckBox.Text = "Enable USBUIRT for output to external tuner";
      this.outputCheckBox.CheckedChanged += new System.EventHandler(this.outputCheckBox_CheckedChanged);
      // 
      // inputCheckBox
      // 
      this.inputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.inputCheckBox.Location = new System.Drawing.Point(16, 24);
      this.inputCheckBox.Name = "inputCheckBox";
      this.inputCheckBox.Size = new System.Drawing.Size(264, 24);
      this.inputCheckBox.TabIndex = 7;
      this.inputCheckBox.Text = "Enable USBUIRT for input from remote controls";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.statusLabel);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(8, 240);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(440, 56);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Status Display";
      // 
      // statusLabel
      // 
      this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.statusLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.statusLabel.Location = new System.Drawing.Point(16, 24);
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(408, 23);
      this.statusLabel.TabIndex = 1;
      this.statusLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // USBUIRT
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "USBUIRT";
      this.Size = new System.Drawing.Size(456, 440);
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

			MediaPortal.IR.USBUIRT.Instance.StartLearning += new StartLearningEventHandler(Instance_StartLearning);

			MediaPortal.IR.USBUIRT.Instance.BulkLearn(commands, buttonNames);
			MediaPortal.IR.USBUIRT.Instance.SaveInternalValues();

			MediaPortal.IR.USBUIRT.Instance.StartLearning -= new StartLearningEventHandler(Instance_StartLearning);
		}

		private void tunerCommandsButton_Click(object sender, System.EventArgs e)
		{
			MediaPortal.IR.USBUIRT.Instance.StartLearning += new StartLearningEventHandler(Instance_StartLearning);

			MediaPortal.IR.USBUIRT.Instance.LearnTunerCodes();
			MediaPortal.IR.USBUIRT.Instance.SaveTunerValues();

			MediaPortal.IR.USBUIRT.Instance.StartLearning -= new StartLearningEventHandler(Instance_StartLearning);			
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
			System.Diagnostics.Debug.WriteLine("Remote Command = " + command.ToString());
		}
	}
}

