using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class Remote : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.Label label4;
		private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMCE;
		private System.ComponentModel.IContainer components = null;

		public Remote() : this("Remote")
		{
		}

		public Remote(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				checkBoxMCE.Checked = xmlreader.GetValueAsBool("remote", "mce2005", false);
      }
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("remote", "mce2005", checkBoxMCE.Checked);

			}
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.checkBoxMCE = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.mpGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mpGroupBox1
			// 
			this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox1.Controls.Add(this.checkBoxMCE);
			this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox1.Location = new System.Drawing.Point(8, 8);
			this.mpGroupBox1.Name = "mpGroupBox1";
			this.mpGroupBox1.Size = new System.Drawing.Size(440, 96);
			this.mpGroupBox1.TabIndex = 2;
			this.mpGroupBox1.TabStop = false;
			this.mpGroupBox1.Text = "General Settings";
			// 
			// checkBoxMCE
			// 
			this.checkBoxMCE.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxMCE.Location = new System.Drawing.Point(16, 24);
			this.checkBoxMCE.Name = "checkBoxMCE";
			this.checkBoxMCE.Size = new System.Drawing.Size(352, 24);
			this.checkBoxMCE.TabIndex = 0;
			this.checkBoxMCE.Text = "Enable MCE2005 Remote";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(0, 0);
			this.label4.Name = "label4";
			this.label4.TabIndex = 0;
			// 
			// Remote
			// 
			this.Controls.Add(this.mpGroupBox1);
			this.Name = "Remote";
			this.Size = new System.Drawing.Size(456, 384);
			this.mpGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		
	}
}

