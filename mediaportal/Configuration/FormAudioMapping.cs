using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for FormAudioMapping.
	/// </summary>
	public class FormAudioMapping : System.Windows.Forms.Form
	{
		string cardName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ComboBox comboBox1Video;
		private System.Windows.Forms.ComboBox comboBox1Audio;
		private System.Windows.Forms.ComboBox comboBox2Video;
		private System.Windows.Forms.ComboBox comboBox2Audio;
		private System.Windows.Forms.ComboBox comboBox3Video;
		private System.Windows.Forms.ComboBox comboBox3Audio;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FormAudioMapping()
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBox1Video = new System.Windows.Forms.ComboBox();
			this.comboBox1Audio = new System.Windows.Forms.ComboBox();
			this.comboBox2Video = new System.Windows.Forms.ComboBox();
			this.comboBox2Audio = new System.Windows.Forms.ComboBox();
			this.comboBox3Video = new System.Windows.Forms.ComboBox();
			this.comboBox3Audio = new System.Windows.Forms.ComboBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "CVBS #1";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "CVBS#2";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 152);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 23);
			this.label3.TabIndex = 2;
			this.label3.Text = "SVHS";
			// 
			// comboBox1Video
			// 
			this.comboBox1Video.Location = new System.Drawing.Point(80, 16);
			this.comboBox1Video.Name = "comboBox1Video";
			this.comboBox1Video.Size = new System.Drawing.Size(121, 21);
			this.comboBox1Video.TabIndex = 3;
			// 
			// comboBox1Audio
			// 
			this.comboBox1Audio.Location = new System.Drawing.Point(80, 40);
			this.comboBox1Audio.Name = "comboBox1Audio";
			this.comboBox1Audio.Size = new System.Drawing.Size(121, 21);
			this.comboBox1Audio.TabIndex = 4;
			// 
			// comboBox2Video
			// 
			this.comboBox2Video.Location = new System.Drawing.Point(80, 72);
			this.comboBox2Video.Name = "comboBox2Video";
			this.comboBox2Video.Size = new System.Drawing.Size(121, 21);
			this.comboBox2Video.TabIndex = 5;
			// 
			// comboBox2Audio
			// 
			this.comboBox2Audio.Location = new System.Drawing.Point(80, 96);
			this.comboBox2Audio.Name = "comboBox2Audio";
			this.comboBox2Audio.Size = new System.Drawing.Size(121, 21);
			this.comboBox2Audio.TabIndex = 6;
			// 
			// comboBox3Video
			// 
			this.comboBox3Video.Location = new System.Drawing.Point(80, 136);
			this.comboBox3Video.Name = "comboBox3Video";
			this.comboBox3Video.Size = new System.Drawing.Size(121, 21);
			this.comboBox3Video.TabIndex = 7;
			// 
			// comboBox3Audio
			// 
			this.comboBox3Audio.Location = new System.Drawing.Point(80, 160);
			this.comboBox3Audio.Name = "comboBox3Audio";
			this.comboBox3Audio.Size = new System.Drawing.Size(121, 21);
			this.comboBox3Audio.TabIndex = 8;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(184, 192);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(32, 23);
			this.buttonOK.TabIndex = 9;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// FormAudioMapping
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(240, 230);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.comboBox3Audio);
			this.Controls.Add(this.comboBox3Video);
			this.Controls.Add(this.comboBox2Audio);
			this.Controls.Add(this.comboBox2Video);
			this.Controls.Add(this.comboBox1Audio);
			this.Controls.Add(this.comboBox1Video);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FormAudioMapping";
			this.Text = "FormAudioMapping";
			this.Load += new System.EventHandler(this.FormAudioMapping_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormAudioMapping_Load(object sender, System.EventArgs e)
		{
			comboBox1Audio.Items.Add("Audio-in #1");
			comboBox1Audio.Items.Add("Audio-in #2");
			comboBox1Audio.Items.Add("Audio-in #3");

			comboBox2Audio.Items.Add("Audio-in #1");
			comboBox2Audio.Items.Add("Audio-in #2");
			comboBox2Audio.Items.Add("Audio-in #3");
			
			comboBox3Audio.Items.Add("Audio-in #1");
			comboBox3Audio.Items.Add("Audio-in #2");
			comboBox3Audio.Items.Add("Audio-in #3");

			comboBox1Video.Items.Add("CVBS #1");
			comboBox1Video.Items.Add("CVBS #2");
			comboBox1Video.Items.Add("CVBS #3");

			comboBox2Video.Items.Add("CVBS #1");
			comboBox2Video.Items.Add("CVBS #2");
			comboBox2Video.Items.Add("CVBS #3");

			comboBox3Video.Items.Add("SVHS #1");
			comboBox3Video.Items.Add("SVHS #2");
			comboBox3Video.Items.Add("SVHS #3");

			LoadSettings();
		}

		private void buttonOK_Click(object sender, System.EventArgs e)
		{
			SaveSettings();
			this.Close();
		}
		void LoadSettings()
		{
			string filename=String.Format(@"database\card_{0}.xml", CardName);
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml(filename))
			{
				comboBox1Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio1", 0);
				comboBox2Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio2", 1);
				comboBox3Audio.SelectedIndex = xmlreader.GetValueAsInt("mapping", "audio3", 0);

				
				comboBox1Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video1", 0);
				comboBox2Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "video2", 1);
				comboBox3Video.SelectedIndex = xmlreader.GetValueAsInt("mapping", "Video3", 0);
			}
		}
		
		void SaveSettings()
		{
			string filename=String.Format(@"database\card_{0}.xml", CardName);
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml(filename))
			{
				xmlwriter.SetValue("mapping", "audio1", comboBox1Audio.SelectedIndex);
				xmlwriter.SetValue("mapping", "audio2", comboBox2Audio.SelectedIndex);
				xmlwriter.SetValue("mapping", "audio3", comboBox3Audio.SelectedIndex);
																								 
																								 
				xmlwriter.SetValue("mapping", "video1", comboBox1Video.SelectedIndex);
				xmlwriter.SetValue("mapping", "video2", comboBox2Video.SelectedIndex);
				xmlwriter.SetValue("mapping", "Video3", comboBox3Video.SelectedIndex);
			}
		}

		public string CardName
		{
			set
			{
				cardName=value;
			}
			get 
			{
				return cardName;
			}
		}
	}
}
