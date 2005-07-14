using System;
using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class DVD : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.ComboBox defaultSubtitleLanguageComboBox;
		private System.Windows.Forms.ComboBox defaultAudioLanguageComboBox;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private MediaPortal.UserInterface.Controls.MPCheckBox pixelRatioCheckBox;
		private System.Windows.Forms.ComboBox displayModeComboBox;
		private System.Windows.Forms.ComboBox aspectRatioComboBox;
		private System.Windows.Forms.Label aspectRatioLabel;
		private MediaPortal.UserInterface.Controls.MPCheckBox showSubtitlesCheckBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label defaultAudioLanguagelabel;
		private System.Windows.Forms.Label displayModeLabel;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoPlayCheckBox;
		private System.ComponentModel.IContainer components = null;

		string m_strDefaultRegionLanguage="English";

		public DVD() : this("DVD")
		{
		}

		public DVD(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Populate combo box with languages
			//

			// set default to english
			m_strDefaultRegionLanguage = GetCultureRegionLanguage();				
			defaultSubtitleLanguageComboBox.Text = m_strDefaultRegionLanguage;
			defaultAudioLanguageComboBox.Text = m_strDefaultRegionLanguage;
			PopulateLanguages(defaultSubtitleLanguageComboBox, m_strDefaultRegionLanguage);
			PopulateLanguages(defaultAudioLanguageComboBox, m_strDefaultRegionLanguage);
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
				defaultAudioLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "audiolanguage", m_strDefaultRegionLanguage);
				defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage", m_strDefaultRegionLanguage);
        autoPlayCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "autoplay", true);

				showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", true);
				pixelRatioCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "pixelratiocorrection", false);

				aspectRatioComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "armode", "Stretch");
				displayModeComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "displaymode", "Default");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("dvdplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "subtitlelanguage", defaultSubtitleLanguageComboBox.Text);

				xmlwriter.SetValueAsBool("dvdplayer", "showsubtitles", showSubtitlesCheckBox.Checked);
				xmlwriter.SetValueAsBool("dvdplayer", "pixelratiocorrection", pixelRatioCheckBox.Checked);
        xmlwriter.SetValueAsBool("dvdplayer", "autoplay", autoPlayCheckBox.Checked);

				xmlwriter.SetValue("dvdplayer", "armode", aspectRatioComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "displaymode", displayModeComboBox.Text);
			}			
		}

		string GetCultureRegionLanguage()
		{
			string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
			int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
			string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);

			foreach(CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures)) 
			{				
				if (cultureInformation.EnglishName.ToLower().IndexOf(strShortLanguage.ToLower()) != -1)
				{
					return cultureInformation.EnglishName;
				}
			}
			return "English";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="comboBox"></param>
		/// <param name="defaultLanguage"></param>
		void PopulateLanguages(ComboBox comboBox, string defaultLanguage)
		{
			comboBox.Items.Clear();
			foreach(CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures)) 
			{
				comboBox.Items.Add(cultureInformation.EnglishName);
				
				if(String.Compare(cultureInformation.EnglishName, defaultLanguage, true) == 0) 
				{
					comboBox.Text = defaultLanguage;
				}
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
      this.defaultAudioLanguagelabel = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.defaultAudioLanguageComboBox = new System.Windows.Forms.ComboBox();
      this.defaultSubtitleLanguageComboBox = new System.Windows.Forms.ComboBox();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.displayModeLabel = new System.Windows.Forms.Label();
      this.displayModeComboBox = new System.Windows.Forms.ComboBox();
      this.aspectRatioComboBox = new System.Windows.Forms.ComboBox();
      this.aspectRatioLabel = new System.Windows.Forms.Label();
      this.pixelRatioCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.autoPlayCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.defaultAudioLanguagelabel);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.defaultAudioLanguageComboBox);
      this.groupBox1.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.groupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 112);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Languages";
      // 
      // defaultAudioLanguagelabel
      // 
      this.defaultAudioLanguagelabel.Location = new System.Drawing.Point(16, 80);
      this.defaultAudioLanguagelabel.Name = "defaultAudioLanguagelabel";
      this.defaultAudioLanguagelabel.Size = new System.Drawing.Size(88, 15);
      this.defaultAudioLanguagelabel.TabIndex = 15;
      this.defaultAudioLanguagelabel.Text = "Audio language:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(96, 16);
      this.label1.TabIndex = 14;
      this.label1.Text = "Subtitle language:";
      // 
      // defaultAudioLanguageComboBox
      // 
      this.defaultAudioLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultAudioLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultAudioLanguageComboBox.Location = new System.Drawing.Point(168, 76);
      this.defaultAudioLanguageComboBox.Name = "defaultAudioLanguageComboBox";
      this.defaultAudioLanguageComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultAudioLanguageComboBox.Sorted = true;
      this.defaultAudioLanguageComboBox.TabIndex = 2;
      // 
      // defaultSubtitleLanguageComboBox
      // 
      this.defaultSubtitleLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultSubtitleLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultSubtitleLanguageComboBox.Location = new System.Drawing.Point(168, 52);
      this.defaultSubtitleLanguageComboBox.Name = "defaultSubtitleLanguageComboBox";
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 1;
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(16, 24);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(96, 16);
      this.showSubtitlesCheckBox.TabIndex = 0;
      this.showSubtitlesCheckBox.Text = "Show subtitles";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.displayModeLabel);
      this.mpGroupBox1.Controls.Add(this.displayModeComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioLabel);
      this.mpGroupBox1.Controls.Add(this.pixelRatioCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 120);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 112);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Aspect Ratio";
      // 
      // displayModeLabel
      // 
      this.displayModeLabel.Location = new System.Drawing.Point(16, 80);
      this.displayModeLabel.Name = "displayModeLabel";
      this.displayModeLabel.Size = new System.Drawing.Size(80, 15);
      this.displayModeLabel.TabIndex = 16;
      this.displayModeLabel.Text = "Display mode:";
      // 
      // displayModeComboBox
      // 
      this.displayModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.displayModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.displayModeComboBox.Items.AddRange(new object[] {
                                                             "Default",
                                                             "16:9",
                                                             "4:3 Pan Scan",
                                                             "4:3 Letterbox"});
      this.displayModeComboBox.Location = new System.Drawing.Point(168, 76);
      this.displayModeComboBox.Name = "displayModeComboBox";
      this.displayModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.displayModeComboBox.TabIndex = 2;
      // 
      // aspectRatioComboBox
      // 
      this.aspectRatioComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.aspectRatioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aspectRatioComboBox.Items.AddRange(new object[] {
                                                             "Crop",
                                                             "Letterbox",
                                                             "Stretch",
                                                             "Follow stream"});
      this.aspectRatioComboBox.Location = new System.Drawing.Point(168, 52);
      this.aspectRatioComboBox.Name = "aspectRatioComboBox";
      this.aspectRatioComboBox.Size = new System.Drawing.Size(288, 21);
      this.aspectRatioComboBox.TabIndex = 1;
      // 
      // aspectRatioLabel
      // 
      this.aspectRatioLabel.Location = new System.Drawing.Point(16, 56);
      this.aspectRatioLabel.Name = "aspectRatioLabel";
      this.aspectRatioLabel.Size = new System.Drawing.Size(152, 16);
      this.aspectRatioLabel.TabIndex = 12;
      this.aspectRatioLabel.Text = "Aspect ratio correction mode:";
      // 
      // pixelRatioCheckBox
      // 
      this.pixelRatioCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.pixelRatioCheckBox.Location = new System.Drawing.Point(16, 24);
      this.pixelRatioCheckBox.Name = "pixelRatioCheckBox";
      this.pixelRatioCheckBox.Size = new System.Drawing.Size(136, 16);
      this.pixelRatioCheckBox.TabIndex = 0;
      this.pixelRatioCheckBox.Text = "Use pixel ratio correction";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.autoPlayCheckBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox2.Location = new System.Drawing.Point(0, 240);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(472, 56);
      this.mpGroupBox2.TabIndex = 4;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Autoplay";
      // 
      // autoPlayCheckBox
      // 
      this.autoPlayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autoPlayCheckBox.Location = new System.Drawing.Point(16, 24);
      this.autoPlayCheckBox.Name = "autoPlayCheckBox";
      this.autoPlayCheckBox.Size = new System.Drawing.Size(96, 16);
      this.autoPlayCheckBox.TabIndex = 0;
      this.autoPlayCheckBox.Text = "Autoplay DVDs";
      // 
      // DVD
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "DVD";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion
	}
}

