using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for SetupExternalRadio.
	/// </summary>
	public class SetupExternalRadio : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxPlayer;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxArgs;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SetupExternalRadio()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxArgs = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonBrowse = new System.Windows.Forms.Button();
			this.textBoxPlayer = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.linkLabel1);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.textBoxArgs);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.buttonBrowse);
			this.groupBox1.Controls.Add(this.textBoxPlayer);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Location = new System.Drawing.Point(8, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(432, 248);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "External Radio player:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(32, 184);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(320, 16);
			this.label5.TabIndex = 8;
			this.label5.Text = "%MHZ will be replaced by frequency in form: 104.5";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 40);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(200, 16);
			this.label4.TabIndex = 7;
			this.label4.Text = "(The light version is freeware)";
			// 
			// linkLabel1
			// 
			this.linkLabel1.Location = new System.Drawing.Point(272, 24);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(112, 16);
			this.linkLabel1.TabIndex = 6;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "http://www.axife.com/";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(248, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "A good external radio player can be found here:";
			// 
			// textBoxArgs
			// 
			this.textBoxArgs.Location = new System.Drawing.Point(32, 152);
			this.textBoxArgs.Name = "textBoxArgs";
			this.textBoxArgs.Size = new System.Drawing.Size(304, 20);
			this.textBoxArgs.TabIndex = 1;
			this.textBoxArgs.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(192, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Command line arguments";
			// 
			// buttonBrowse
			// 
			this.buttonBrowse.Location = new System.Drawing.Point(352, 96);
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.Size = new System.Drawing.Size(24, 23);
			this.buttonBrowse.TabIndex = 2;
			this.buttonBrowse.Text = "...";
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
			// 
			// textBoxPlayer
			// 
			this.textBoxPlayer.Location = new System.Drawing.Point(32, 96);
			this.textBoxPlayer.Name = "textBoxPlayer";
			this.textBoxPlayer.Size = new System.Drawing.Size(304, 20);
			this.textBoxPlayer.TabIndex = 0;
			this.textBoxPlayer.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Radio player:";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(32, 200);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(320, 16);
			this.label7.TabIndex = 8;
			this.label7.Text = "%mhz will be replaced by frequency in form: 104,5";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(32, 216);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(320, 16);
			this.label8.TabIndex = 8;
			this.label8.Text = "%HZ will be replaced by frequency in form: 104500000";
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(392, 256);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(40, 23);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// SetupExternalRadio
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 285);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBox1);
			this.Name = "SetupExternalRadio";
			this.Text = "SetupExternalRadio";
			this.Load += new System.EventHandler(this.SetupExternalRadio_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonBrowse_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg=new OpenFileDialog();
			dlg.CheckFileExists=true;
			dlg.CheckPathExists=true;
			dlg.RestoreDirectory=true;
			dlg.Filter= "exe files (*.exe)|*.exe";
			dlg.FilterIndex=0;
			dlg.Title="Select Radio Player";
			dlg.ShowDialog();
			if (dlg.FileName!="")
			{
				textBoxPlayer.Text=dlg.FileName;
			}
		}

		private void SetupExternalRadio_Load(object sender, System.EventArgs e)
		{
			using (AMS.Profile.Xml   xmlReader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				textBoxPlayer.Text=xmlReader.GetValueAsString("radio","player","");
				textBoxArgs.Text  =xmlReader.GetValueAsString("radio","args","f %MHZ% v 100 u");
			}
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			using (AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlWriter.SetValue("radio","player",textBoxPlayer.Text);
				xmlWriter.SetValue("radio","args",textBoxArgs.Text);
			}
			this.Close();
		}



	}
}
