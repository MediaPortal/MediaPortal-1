using System;
using System.Globalization;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
	public class FileMenu : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.CheckBox chbEnabled;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textPinCodeBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textTrashcanFolder;
		private System.ComponentModel.IContainer components = null;

		public FileMenu() : this("File Menu")
		{
		}

		public FileMenu(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

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

		
		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				chbEnabled.Checked = xmlreader.GetValueAsBool("filemenu", "enabled", true);
				textPinCodeBox.Text = xmlreader.GetValueAsString("filemenu", "pincode", "");
				textTrashcanFolder.Text = xmlreader.GetValueAsString("filemenu", "trashcan", "");
			}
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("filemenu", "enabled", chbEnabled.Checked);
				xmlwriter.SetValue("filemenu", "pincode", textPinCodeBox.Text);
				xmlwriter.SetValue("filemenu", "trashcane", textTrashcanFolder.Text);				
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
      this.label1 = new System.Windows.Forms.Label();
      this.textPinCodeBox = new System.Windows.Forms.TextBox();
      this.chbEnabled = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.textTrashcanFolder = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.textPinCodeBox);
      this.groupBox1.Controls.Add(this.chbEnabled);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 88);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Pincode:";
      // 
      // textPinCodeBox
      // 
      this.textPinCodeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textPinCodeBox.Location = new System.Drawing.Point(168, 52);
      this.textPinCodeBox.Name = "textPinCodeBox";
      this.textPinCodeBox.Size = new System.Drawing.Size(288, 20);
      this.textPinCodeBox.TabIndex = 2;
      this.textPinCodeBox.Text = "";
      // 
      // chbEnabled
      // 
      this.chbEnabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.chbEnabled.Location = new System.Drawing.Point(16, 24);
      this.chbEnabled.Name = "chbEnabled";
      this.chbEnabled.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.chbEnabled.Size = new System.Drawing.Size(104, 16);
      this.chbEnabled.TabIndex = 0;
      this.chbEnabled.Text = "Enable file menu";
      this.chbEnabled.CheckedChanged += new System.EventHandler(this.chbEnabled_CheckedChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 104);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 16);
      this.label3.TabIndex = 1;
      this.label3.Text = "Trashcan folder:";
      this.label3.Visible = false;
      // 
      // textTrashcanFolder
      // 
      this.textTrashcanFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textTrashcanFolder.Location = new System.Drawing.Point(168, 100);
      this.textTrashcanFolder.Name = "textTrashcanFolder";
      this.textTrashcanFolder.Size = new System.Drawing.Size(288, 20);
      this.textTrashcanFolder.TabIndex = 2;
      this.textTrashcanFolder.Text = "";
      this.textTrashcanFolder.Visible = false;
      // 
      // FileMenu
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.textTrashcanFolder);
      this.Name = "FileMenu";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void chbEnabled_CheckedChanged(object sender, System.EventArgs e)
		{
			textPinCodeBox.Enabled = chbEnabled.Checked;
			textTrashcanFolder.Enabled = chbEnabled.Checked;
		}

    
	}
}

