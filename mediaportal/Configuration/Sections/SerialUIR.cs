using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using MediaPortal.SerialIR;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
	public class SerialUIR : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPCheckBox inputCheckBox;
		private System.Windows.Forms.Button internalCommandsButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox CommPortCombo;
		private System.ComponentModel.IContainer components = null;

		public SerialUIR() : this("SerialUIR")
		{
		}

		public SerialUIR(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// 
			// Initialize the SerialUIR component
			//
			MediaPortal.SerialIR.SerialUIR.Create(new MediaPortal.SerialIR.SerialUIR.OnRemoteCommand(OnRemoteCommand));
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			MediaPortal.SerialIR.SerialUIR.Instance.Close();
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
				inputCheckBox.Checked	= xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
				CommPortCombo.Text   	= xmlreader.GetValueAsString("SerialUIR", "commport", "COM1:");
			}
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("SerialUIR", "internal", inputCheckBox.Checked ? "true" : "false");
				xmlwriter.SetValue("SerialUIR", "commport", CommPortCombo.Text);
			}
		}

		public void Close()
		{
			MediaPortal.SerialIR.SerialUIR.Instance.Close();
		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.CommPortCombo = new System.Windows.Forms.ComboBox();
			this.internalCommandsButton = new System.Windows.Forms.Button();
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
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.CommPortCombo);
			this.groupBox1.Controls.Add(this.internalCommandsButton);
			this.groupBox1.Controls.Add(this.inputCheckBox);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 144);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General settings";
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(16, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 16);
			this.label1.TabIndex = 13;
			this.label1.Text = "Communication Port:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// CommPortCombo
			// 
			this.CommPortCombo.Items.AddRange(new object[] {
															   "COM1:",
															   "COM2:",
															   "COM3:",
															   "COM4:"});
			this.CommPortCombo.Location = new System.Drawing.Point(144, 64);
			this.CommPortCombo.Name = "CommPortCombo";
			this.CommPortCombo.Size = new System.Drawing.Size(121, 21);
			this.CommPortCombo.TabIndex = 12;
			this.CommPortCombo.TextChanged += new System.EventHandler(this.CommPortCombo_SelectedIndexChanged);
			// 
			// internalCommandsButton
			// 
			this.internalCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.internalCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.internalCommandsButton.Location = new System.Drawing.Point(16, 104);
			this.internalCommandsButton.Name = "internalCommandsButton";
			this.internalCommandsButton.Size = new System.Drawing.Size(408, 23);
			this.internalCommandsButton.TabIndex = 11;
			this.internalCommandsButton.Text = "Learn internal commands";
			this.internalCommandsButton.Click += new System.EventHandler(this.internalCommandsButton_Click);
			// 
			// inputCheckBox
			// 
			this.inputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.inputCheckBox.Location = new System.Drawing.Point(16, 24);
			this.inputCheckBox.Name = "inputCheckBox";
			this.inputCheckBox.Size = new System.Drawing.Size(264, 24);
			this.inputCheckBox.TabIndex = 7;
			this.inputCheckBox.Text = "Enable Serial UIR for input from remote controls";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.statusLabel);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 160);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 56);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Status Display";
			// 
			// statusLabel
			// 
			this.statusLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.statusLabel.Font = new System.Drawing.Font("Verdana", 9.75F);
			this.statusLabel.Location = new System.Drawing.Point(16, 24);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(408, 23);
			this.statusLabel.TabIndex = 1;
			// 
			// SerialUIR
			// 
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "SerialUIR";
			this.Size = new System.Drawing.Size(456, 440);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void internalCommandsButton_Click(object sender, System.EventArgs e)
		{
			internalCommandsButton.Enabled = false;
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

			MediaPortal.SerialIR.SerialUIR.Instance.StartLearning += new StartLearningEventHandler(Instance_StartLearning);

			MediaPortal.SerialIR.SerialUIR.Instance.BulkLearn(commands, buttonNames);
			MediaPortal.SerialIR.SerialUIR.Instance.SaveInternalValues();

			MediaPortal.SerialIR.SerialUIR.Instance.StartLearning -= new StartLearningEventHandler(Instance_StartLearning);
			statusLabel.Text = "Learning finished !";
			internalCommandsButton.Enabled = true;
			Application.DoEvents();
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

		private void CommPortCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(MediaPortal.SerialIR.SerialUIR.Instance.SetPort(CommPortCombo.Text))
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

	}
}
