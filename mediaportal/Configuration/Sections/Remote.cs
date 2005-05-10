using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MediaPortal.Configuration.Sections
{
	public class Remote : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.Label label4;
		private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMCE;
		private System.Windows.Forms.CheckBox checkBoxUSA;
		private System.Windows.Forms.CheckBox checkBoxEurope;
		private System.Windows.Forms.PictureBox pictureBoxUSA;
		private System.Windows.Forms.PictureBox pictureBoxEU;
		private System.ComponentModel.IContainer components = null;
		public struct RAWINPUTDEVICE 
		{
			public ushort usUsagePage;
			public ushort usUsage;
			public uint dwFlags;
			public IntPtr hwndTarget;
		} ;
		[DllImport("User32.dll",EntryPoint="RegisterRawInputDevices",SetLastError=true)]
		public extern static bool RegisterRawInputDevices(
			[In] RAWINPUTDEVICE[] pRawInputDevices,
			[In] uint uiNumDevices,
			[In] uint cbSize);  

		public Remote() : this("Remote")
		{
		}

		public Remote(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

		}
		static public bool IsMceRemoteInstalled(IntPtr hwnd)
		{
			RAWINPUTDEVICE[] rid1= new RAWINPUTDEVICE[1];

			rid1[0].usUsagePage = 0xFFBC;
			rid1[0].usUsage = 0x88;
			rid1[0].dwFlags = 0;
			rid1[0].hwndTarget=hwnd;
			bool Success=RegisterRawInputDevices(rid1, (uint)rid1.Length,(uint)Marshal.SizeOf(rid1[0]));
			if (Success) 
			{
				return true;
			}

			rid1[0].usUsagePage = 0x0C;
			rid1[0].usUsage = 0x01;
			rid1[0].dwFlags = 0;
			rid1[0].hwndTarget=hwnd;
			Success=RegisterRawInputDevices(rid1, (uint)rid1.Length,(uint)Marshal.SizeOf(rid1[0]));
			if (Success) 
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				checkBoxMCE.Checked		 = xmlreader.GetValueAsBool("remote", "mce2005", false);
				checkBoxUSA.Checked		 = xmlreader.GetValueAsBool("remote", "USAModel", false);
				checkBoxEurope.Checked = !xmlreader.GetValueAsBool("remote", "USAModel", false);
      }
			if (checkBoxMCE.Checked)
			{
				checkBoxEurope.Enabled=true;
				checkBoxUSA.Enabled=true;
			}
			else
			{
				checkBoxEurope.Enabled=false;
				checkBoxUSA.Enabled=false;
			}
			if (checkBoxUSA.Checked)
			{
				pictureBoxUSA.Visible=true;
				pictureBoxEU.Visible=false;
			}
			else
			{
				pictureBoxEU.Visible=true;
				pictureBoxUSA.Visible=false;
			}
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("remote", "mce2005", checkBoxMCE.Checked);
				xmlwriter.SetValueAsBool("remote", "USAModel", checkBoxUSA.Checked);

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Remote));
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.pictureBoxEU = new System.Windows.Forms.PictureBox();
			this.pictureBoxUSA = new System.Windows.Forms.PictureBox();
			this.checkBoxEurope = new System.Windows.Forms.CheckBox();
			this.checkBoxUSA = new System.Windows.Forms.CheckBox();
			this.checkBoxMCE = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.mpGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mpGroupBox1
			// 
			this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox1.Controls.Add(this.pictureBoxEU);
			this.mpGroupBox1.Controls.Add(this.pictureBoxUSA);
			this.mpGroupBox1.Controls.Add(this.checkBoxEurope);
			this.mpGroupBox1.Controls.Add(this.checkBoxUSA);
			this.mpGroupBox1.Controls.Add(this.checkBoxMCE);
			this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox1.Location = new System.Drawing.Point(8, 8);
			this.mpGroupBox1.Name = "mpGroupBox1";
			this.mpGroupBox1.Size = new System.Drawing.Size(440, 336);
			this.mpGroupBox1.TabIndex = 0;
			this.mpGroupBox1.TabStop = false;
			this.mpGroupBox1.Text = "General Settings";
			// 
			// pictureBoxEU
			// 
			this.pictureBoxEU.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxEU.Image")));
			this.pictureBoxEU.Location = new System.Drawing.Point(168, 24);
			this.pictureBoxEU.Name = "pictureBoxEU";
			this.pictureBoxEU.Size = new System.Drawing.Size(171, 296);
			this.pictureBoxEU.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxEU.TabIndex = 4;
			this.pictureBoxEU.TabStop = false;
			// 
			// pictureBoxUSA
			// 
			this.pictureBoxUSA.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxUSA.Image")));
			this.pictureBoxUSA.Location = new System.Drawing.Point(168, 24);
			this.pictureBoxUSA.Name = "pictureBoxUSA";
			this.pictureBoxUSA.Size = new System.Drawing.Size(258, 300);
			this.pictureBoxUSA.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBoxUSA.TabIndex = 3;
			this.pictureBoxUSA.TabStop = false;
			// 
			// checkBoxEurope
			// 
			this.checkBoxEurope.Location = new System.Drawing.Point(32, 80);
			this.checkBoxEurope.Name = "checkBoxEurope";
			this.checkBoxEurope.Size = new System.Drawing.Size(104, 16);
			this.checkBoxEurope.TabIndex = 2;
			this.checkBoxEurope.Text = "Europe Model";
			this.checkBoxEurope.CheckedChanged += new System.EventHandler(this.checkBoxEurope_CheckedChanged);
			// 
			// checkBoxUSA
			// 
			this.checkBoxUSA.Location = new System.Drawing.Point(32, 56);
			this.checkBoxUSA.Name = "checkBoxUSA";
			this.checkBoxUSA.Size = new System.Drawing.Size(104, 32);
			this.checkBoxUSA.TabIndex = 1;
			this.checkBoxUSA.Text = "USA Model";
			this.checkBoxUSA.CheckedChanged += new System.EventHandler(this.checkBoxUSA_CheckedChanged);
			// 
			// checkBoxMCE
			// 
			this.checkBoxMCE.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxMCE.Location = new System.Drawing.Point(16, 24);
			this.checkBoxMCE.Name = "checkBoxMCE";
			this.checkBoxMCE.Size = new System.Drawing.Size(136, 24);
			this.checkBoxMCE.TabIndex = 0;
			this.checkBoxMCE.Text = "Enable MCE2005 Remote";
			this.checkBoxMCE.CheckedChanged += new System.EventHandler(this.checkBoxMCE_CheckedChanged);
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

		private void checkBoxUSA_CheckedChanged(object sender, System.EventArgs e)
		{
			if (checkBoxUSA.Checked)
			{
				pictureBoxUSA.Visible=true;
				pictureBoxEU.Visible=false;
			}
			else
			{
				pictureBoxEU.Visible=true;
				pictureBoxUSA.Visible=false;
			}
			checkBoxEurope.Checked=!checkBoxUSA.Checked;
		}

		private void checkBoxEurope_CheckedChanged(object sender, System.EventArgs e)
		{
			if (checkBoxEurope.Checked)
			{
				pictureBoxUSA.Visible=false;
				pictureBoxEU.Visible=true;
			}
			else
			{
				pictureBoxEU.Visible=false;
				pictureBoxUSA.Visible=true;
			}
			checkBoxUSA.Checked=!checkBoxEurope.Checked;
		}

		private void checkBoxMCE_CheckedChanged(object sender, System.EventArgs e)
		{
			if (checkBoxMCE.Checked)
			{
				checkBoxEurope.Enabled=true;
				checkBoxUSA.Enabled=true;
			}
			else
			{
				checkBoxEurope.Enabled=false;
				checkBoxUSA.Enabled=false;
			}
		}

		
	}
}

