using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for EditRadioStationForm.
	/// </summary>
	public class EditRadioStationForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox channelTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox genreTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox bitrateTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox urlTextBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox typeComboBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditRadioStationForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Set default settings
			//
			UpdateControlStates();
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.typeComboBox = new System.Windows.Forms.ComboBox();
			this.urlTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.bitrateTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.genreTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.channelTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.closeButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.typeComboBox);
			this.groupBox1.Controls.Add(this.urlTextBox);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.bitrateTextBox);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.genreTextBox);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.channelTextBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.nameTextBox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(360, 196);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Radio Station";
			// 
			// typeComboBox
			// 
			this.typeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typeComboBox.Items.AddRange(new object[] {
															  "Radio",
															  "Stream"});
			this.typeComboBox.Location = new System.Drawing.Point(121, 27);
			this.typeComboBox.Name = "typeComboBox";
			this.typeComboBox.Size = new System.Drawing.Size(224, 21);
			this.typeComboBox.TabIndex = 0;
			this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
			// 
			// urlTextBox
			// 
			this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.urlTextBox.Location = new System.Drawing.Point(120, 152);
			this.urlTextBox.Name = "urlTextBox";
			this.urlTextBox.Size = new System.Drawing.Size(224, 20);
			this.urlTextBox.TabIndex = 5;
			this.urlTextBox.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 155);
			this.label6.Name = "label6";
			this.label6.TabIndex = 10;
			this.label6.Text = "URL";
			// 
			// bitrateTextBox
			// 
			this.bitrateTextBox.Location = new System.Drawing.Point(120, 127);
			this.bitrateTextBox.MaxLength = 3;
			this.bitrateTextBox.Name = "bitrateTextBox";
			this.bitrateTextBox.Size = new System.Drawing.Size(40, 20);
			this.bitrateTextBox.TabIndex = 4;
			this.bitrateTextBox.Text = "";
			this.bitrateTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.bitrateTextBox_KeyPress);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 130);
			this.label5.Name = "label5";
			this.label5.TabIndex = 8;
			this.label5.Text = "Bitrate";
			// 
			// genreTextBox
			// 
			this.genreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.genreTextBox.Location = new System.Drawing.Point(120, 102);
			this.genreTextBox.Name = "genreTextBox";
			this.genreTextBox.Size = new System.Drawing.Size(224, 20);
			this.genreTextBox.TabIndex = 3;
			this.genreTextBox.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 105);
			this.label4.Name = "label4";
			this.label4.TabIndex = 6;
			this.label4.Text = "Genre";
			// 
			// channelTextBox
			// 
			this.channelTextBox.Location = new System.Drawing.Point(120, 77);
			this.channelTextBox.MaxLength = 3;
			this.channelTextBox.Name = "channelTextBox";
			this.channelTextBox.Size = new System.Drawing.Size(40, 20);
			this.channelTextBox.TabIndex = 2;
			this.channelTextBox.Text = "";
			this.channelTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.channelTextBox_KeyPress);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 80);
			this.label3.Name = "label3";
			this.label3.TabIndex = 4;
			this.label3.Text = "Channel";
			// 
			// nameTextBox
			// 
			this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.nameTextBox.Location = new System.Drawing.Point(120, 52);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = new System.Drawing.Size(224, 20);
			this.nameTextBox.TabIndex = 1;
			this.nameTextBox.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 55);
			this.label2.Name = "label2";
			this.label2.TabIndex = 2;
			this.label2.Text = "Name";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 30);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Type";
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.closeButton.Location = new System.Drawing.Point(292, 213);
			this.closeButton.Name = "closeButton";
			this.closeButton.TabIndex = 1;
			this.closeButton.Text = "Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(211, 213);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// EditRadioStationForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(376, 246);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(384, 272);
			this.Name = "EditRadioStationForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "EditRadioStationForm";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		private void closeButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}

		private void typeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateControlStates();
		}

		private void UpdateControlStates()
		{
			//
			// Make default selection
			//
			if(typeComboBox.SelectedItem == null)
				typeComboBox.SelectedItem = "Radio";

			urlTextBox.Enabled = typeComboBox.SelectedItem.Equals("Stream");
		}

		private void channelTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

		private void bitrateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

		public RadioStation Station
		{
			get { 
				RadioStation station = new RadioStation();

				station.Type = (string)typeComboBox.SelectedItem;
				station.Name = nameTextBox.Text;
				station.Channel = Convert.ToInt32(channelTextBox.Text.Length > 0 ? channelTextBox.Text : "0");
				station.Genre = genreTextBox.Text;
				station.Bitrate = Convert.ToInt32(bitrateTextBox.Text.Length > 0 ? bitrateTextBox.Text : "0");
				station.URL = urlTextBox.Text;

				return station; 
			}

			set { 
				RadioStation station = value as RadioStation;
	
				typeComboBox.SelectedItem = (string)station.Type;
				nameTextBox.Text = station.Name;
				channelTextBox.Text = station.Channel.ToString();
				genreTextBox.Text = station.Genre;
				bitrateTextBox.Text = station.Bitrate.ToString();
				urlTextBox.Text = station.URL;
			}
		}
	}
}
