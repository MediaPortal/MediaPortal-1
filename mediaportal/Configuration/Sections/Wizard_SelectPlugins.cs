using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class Wizard_SelectPlugins : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox myMusicCheckBox;
		private System.Windows.Forms.CheckBox myMoviesCheckBox;
		private System.Windows.Forms.CheckBox myPicturesCheckBox;
		private System.ComponentModel.IContainer components = null;

		public Wizard_SelectPlugins() : this("Plugin Selection")
		{
		}

		public Wizard_SelectPlugins(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.myMusicCheckBox = new System.Windows.Forms.CheckBox();
			this.myMoviesCheckBox = new System.Windows.Forms.CheckBox();
			this.myPicturesCheckBox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.myMusicCheckBox);
			this.groupBox1.Controls.Add(this.myMoviesCheckBox);
			this.groupBox1.Controls.Add(this.myPicturesCheckBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(416, 224);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Plugin Selection";
			// 
			// myMusicCheckBox
			// 
			this.myMusicCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.myMusicCheckBox.Location = new System.Drawing.Point(24, 184);
			this.myMusicCheckBox.Name = "myMusicCheckBox";
			this.myMusicCheckBox.Size = new System.Drawing.Size(336, 24);
			this.myMusicCheckBox.TabIndex = 2;
			this.myMusicCheckBox.Text = "My Music";
			// 
			// myMoviesCheckBox
			// 
			this.myMoviesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.myMoviesCheckBox.Location = new System.Drawing.Point(24, 120);
			this.myMoviesCheckBox.Name = "myMoviesCheckBox";
			this.myMoviesCheckBox.Size = new System.Drawing.Size(336, 24);
			this.myMoviesCheckBox.TabIndex = 1;
			this.myMoviesCheckBox.Text = "My Movies";
			// 
			// myPicturesCheckBox
			// 
			this.myPicturesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.myPicturesCheckBox.Location = new System.Drawing.Point(24, 56);
			this.myPicturesCheckBox.Name = "myPicturesCheckBox";
			this.myPicturesCheckBox.Size = new System.Drawing.Size(336, 24);
			this.myPicturesCheckBox.TabIndex = 0;
			this.myPicturesCheckBox.Text = "My Pictures";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(16, 152);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(392, 32);
			this.label3.TabIndex = 5;
			this.label3.Text = "The My Music plugin takes care of your music collection. It supports playing, fet" +
				"ching music information for the currently playing song etc.";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(392, 32);
			this.label2.TabIndex = 4;
			this.label2.Text = "The My Movies plugin takes care of your movie collection in a quick and easy way." +
				"";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(392, 40);
			this.label1.TabIndex = 3;
			this.label1.Text = "The My Pictures plugin takes care of your picture collection in an quick and easy" +
				" way. It supports showing single pictures or pictures in a slideshow.";
			// 
			// Wizard_SelectPlugins
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Wizard_SelectPlugins";
			this.Size = new System.Drawing.Size(432, 384);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public override void LoadSettings()
		{
			using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				myMoviesCheckBox.Checked = xmlreader.GetValueAsBool("plugins", "My Movies", true);
				myMusicCheckBox.Checked = xmlreader.GetValueAsBool("plugins", "My Music", true);
				myPicturesCheckBox.Checked = xmlreader.GetValueAsBool("plugins", "My Pictures", true);
			}						
		}

		public override void SaveSettings()
		{
			using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("plugins", "My Movies", myMoviesCheckBox.Checked);
				xmlwriter.SetValueAsBool("plugins", "My Music", myMusicCheckBox.Checked);
				xmlwriter.SetValueAsBool("plugins", "My Pictures", myPicturesCheckBox.Checked);
			}			
		}

		public override object GetSetting(string name)
		{
			switch(name.ToLower())
			{
				case "plugin.mymusic":
					return myMusicCheckBox.Checked;

				case "plugin.mypictures":
					return myPicturesCheckBox.Checked;

				case "plugin.mymovies":
					return myMoviesCheckBox.Checked;
			}

			return null;
		}
	}
}

