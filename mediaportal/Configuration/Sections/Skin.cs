using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class Skin : MediaPortal.Configuration.SectionSettings
	{
		const string SkinDirectory = @"skin\";

		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.ListBox availableSkinsListBox;
		private System.Windows.Forms.PictureBox previewPictureBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.ComponentModel.IContainer components = null;

		public Skin() : this("Skin")
		{
		}

		public Skin(string name) : base(name)
		{

			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Load available skins
			//
			availableSkinsListBox.Items.Clear();

			if(Directory.Exists(SkinDirectory))
			{
				string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");
				
				foreach(string skinFolder in skinFolders)
				{
					bool		isInvalidDirectory = false;
					string[]	invalidDirectoryNames = new string[] { "cvs" };
					
					string directoryName = skinFolder.Substring(SkinDirectory.Length);

					if(directoryName != null && directoryName.Length > 0)
					{
						foreach(string invalidDirectory in invalidDirectoryNames)
						{
							if(invalidDirectory.Equals(directoryName))
							{
								isInvalidDirectory = true;
								break;
							}
						}

						if(isInvalidDirectory == false)
						{
							availableSkinsListBox.Items.Add(directoryName);
						}
					}
				}
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

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string currentSkin = xmlreader.GetValueAsString("skin", "name", "MetalMedia");

				//
				// Make sure the skin actually exists before setting it as the current skin
				//
				foreach(string availableSkin in availableSkinsListBox.Items)
				{
					if(availableSkin.Equals(currentSkin))
					{
						availableSkinsListBox.SelectedItem = currentSkin;
						break;
					}
				}
			}
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("skin", "name", availableSkinsListBox.Text);
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
			this.previewPictureBox = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.availableSkinsListBox = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.previewPictureBox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.availableSkinsListBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(520, 440);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General settings";
			// 
			// previewPictureBox
			// 
			this.previewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewPictureBox.Location = new System.Drawing.Point(200, 40);
			this.previewPictureBox.Name = "previewPictureBox";
			this.previewPictureBox.Size = new System.Drawing.Size(300, 256);
			this.previewPictureBox.TabIndex = 1;
			this.previewPictureBox.TabStop = false;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(200, 24);
			this.label2.Name = "label2";
			this.label2.TabIndex = 4;
			this.label2.Text = "Preview";
			// 
			// availableSkinsListBox
			// 
			this.availableSkinsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.availableSkinsListBox.Location = new System.Drawing.Point(16, 40);
			this.availableSkinsListBox.Name = "availableSkinsListBox";
			this.availableSkinsListBox.Size = new System.Drawing.Size(168, 381);
			this.availableSkinsListBox.TabIndex = 0;
			this.availableSkinsListBox.SelectedIndexChanged += new System.EventHandler(this.availableSkinsListBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.TabIndex = 3;
			this.label1.Text = "Available skins";
			// 
			// Skin
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.groupBox1);
			this.Name = "Skin";
			this.Size = new System.Drawing.Size(536, 456);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void availableSkinsListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			string currentSkin = (string)availableSkinsListBox.SelectedItem;
			string previewFile = String.Format(@"{0}{1}\media\preview.png", SkinDirectory, currentSkin);

			//
			// Clear image
			//
			previewPictureBox.Image = null;

			if(File.Exists(previewFile))
			{
				previewPictureBox.Image = Image.FromFile(previewFile);
			}
			else
			{
				string logoFile = "mplogo.gif";

				if(File.Exists(logoFile))
				{
					previewPictureBox.Image = Image.FromFile(logoFile);
				}
			}

			//
			// Check for the 
		}
	}
}

