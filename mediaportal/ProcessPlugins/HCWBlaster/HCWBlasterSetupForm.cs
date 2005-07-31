using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.HCWBlaster
{
	/// <summary>
	/// Summary description for HCWBlasterSetupForm.
	/// </summary>
	public class HCWBlasterSetupForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.CheckBox chkExtendedLog;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public HCWBlasterSetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			LoadSettings();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		private void LoadSettings()
		{
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				this.chkExtendedLog.Checked = xmlreader.GetValueAsBool("HCWBlaster", "ExtendedLogging", false);
			}
		}

		private bool SaveSettings()
		{
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("HCWBlaster", "ExtendedLogging", this.chkExtendedLog.Checked);
			}
			return true;
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
			this.label1 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.chkExtendedLog = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(4, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(280, 40);
			this.label1.TabIndex = 6;
			this.label1.Text = "You must configure the IR Blaster using the Hauppauge IR configuration sofware. ";
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(72, 120);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(128, 32);
			this.btnOK.TabIndex = 5;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// chkExtendedLog
			// 
			this.chkExtendedLog.Location = new System.Drawing.Point(4, 59);
			this.chkExtendedLog.Name = "chkExtendedLog";
			this.chkExtendedLog.Size = new System.Drawing.Size(280, 32);
			this.chkExtendedLog.TabIndex = 4;
			this.chkExtendedLog.Text = "Enable Extended Logging";
			// 
			// HCWBlasterSetupForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(288, 166);
			this.ControlBox = false;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.chkExtendedLog);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "HCWBlasterSetupForm";
			this.Text = "Hauppauge IR Blaster Setup";
			this.ResumeLayout(false);

		}
		#endregion

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			SaveSettings();
			this.Close();
		}
	}
}
