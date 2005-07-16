using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
	public class Skin : MediaPortal.Configuration.SectionSettings
	{
		const string SkinDirectory = @"skin\";

		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.ListView listViewAvailableSkins;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colVersion;
    private System.Windows.Forms.PictureBox previewPictureBox;
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
			listViewAvailableSkins.Items.Clear();

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
							if(invalidDirectory.Equals(directoryName.ToLower()))
							{
								isInvalidDirectory = true;
								break;
							}
						}

						if(isInvalidDirectory == false)
						{
              //
              // Check if we have a home.xml located in the directory, if so we consider it as a
              // valid skin directory
              //
							string filename=Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
              if(File.Exists(filename))
              {	
								XmlDocument doc=new XmlDocument();
								doc.Load(filename);
								XmlNode node=doc.SelectSingleNode("/controls/skin/version");
                ListViewItem item=listViewAvailableSkins.Items.Add(directoryName);
								if (node!=null && node.InnerText!=null)
									item.SubItems.Add(node.InnerText);
								else
									item.SubItems.Add("?");
              }
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
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string currentSkin = xmlreader.GetValueAsString("skin", "name", "BlueTwo");

				//
				// Make sure the skin actually exists before setting it as the current skin
				//
				foreach(ListViewItem item in listViewAvailableSkins.Items)
				{
					if(item .SubItems[0].Text.Equals(currentSkin))
					{
						item.Selected=true;
						break;
					}
				}
			}
		}

		public override void SaveSettings()
		{
			if (listViewAvailableSkins.SelectedItems.Count==0) return;
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string prevSkin = xmlwriter.GetValueAsString("skin", "name", "BlueTwo");
				if (prevSkin!=listViewAvailableSkins.SelectedItems[0].Text)
				{
					Utils.DeleteFiles(@"skin\"+listViewAvailableSkins.Text+@"\fonts","*");
				}
				xmlwriter.SetValue("skin", "name", listViewAvailableSkins.SelectedItems[0].Text);
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
      this.listViewAvailableSkins = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colVersion = new System.Windows.Forms.ColumnHeader();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.listViewAvailableSkins);
      this.groupBox1.Controls.Add(this.previewPictureBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // listViewAvailableSkins
      // 
      this.listViewAvailableSkins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewAvailableSkins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                             this.colName,
                                                                                             this.colVersion});
      this.listViewAvailableSkins.FullRowSelect = true;
      this.listViewAvailableSkins.HideSelection = false;
      this.listViewAvailableSkins.Location = new System.Drawing.Point(16, 24);
      this.listViewAvailableSkins.Name = "listViewAvailableSkins";
      this.listViewAvailableSkins.Size = new System.Drawing.Size(440, 80);
      this.listViewAvailableSkins.TabIndex = 0;
      this.listViewAvailableSkins.View = System.Windows.Forms.View.Details;
      this.listViewAvailableSkins.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableSkins_SelectedIndexChanged);
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 338;
      // 
      // colVersion
      // 
      this.colVersion.Text = "Version";
      this.colVersion.Width = 80;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Location = new System.Drawing.Point(86, 132);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(300, 240);
      this.previewPictureBox.TabIndex = 2;
      this.previewPictureBox.TabStop = false;
      // 
      // Skin
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBox1);
      this.Name = "Skin";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion


		private void listViewAvailableSkins_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (listViewAvailableSkins.SelectedItems.Count==0) 
			{
				previewPictureBox.Image=null;
				return;
			}
			string currentSkin = (string)listViewAvailableSkins.SelectedItems[0].Text;
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

		}
	}
}

