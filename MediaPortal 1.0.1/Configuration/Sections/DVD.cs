#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class DVD : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPComboBox defaultSubtitleLanguageComboBox;
    private MPComboBox defaultAudioLanguageComboBox;
    private MPGroupBox mpGroupBox1;
    private MPCheckBox pixelRatioCheckBox;
    private MPComboBox displayModeComboBox;
    private MPComboBox aspectRatioComboBox;
    private MPLabel aspectRatioLabel;
    private MPCheckBox showSubtitlesCheckBox;
    private MPLabel label1;
    private MPLabel defaultAudioLanguagelabel;
    private MPLabel displayModeLabel;
    private MPGroupBox mpGroupBox2;
    private IContainer components = null;
    private MPComboBox autoPlayComboBox;
    private MPLabel labelAutoPlay;

    private string m_strDefaultRegionLanguage = "English";

    private string[] autoPlayOptions = new string[]
                                         {
                                           "Autoplay, never ask", "Don't autoplay, never ask",
                                           "Ask every time a DVD is inserted"
                                         };

    public DVD()
      : this("DVD")
    {
    }

    public DVD(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Populate combo box with languages
      m_strDefaultRegionLanguage = Util.Utils.GetCultureRegionLanguage();
      defaultSubtitleLanguageComboBox.Text = m_strDefaultRegionLanguage;
      defaultAudioLanguageComboBox.Text = m_strDefaultRegionLanguage;
      Util.Utils.PopulateLanguagesToComboBox(defaultSubtitleLanguageComboBox, m_strDefaultRegionLanguage);
      Util.Utils.PopulateLanguagesToComboBox(defaultAudioLanguageComboBox, m_strDefaultRegionLanguage);

      autoPlayComboBox.Items.Clear();
      autoPlayComboBox.Items.AddRange(autoPlayOptions);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        defaultAudioLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "audiolanguage",
                                                                               m_strDefaultRegionLanguage);
        defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage",
                                                                                  m_strDefaultRegionLanguage);

        showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", false);
        pixelRatioCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "pixelratiocorrection", false);

        aspectRatioComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "armode", "Follow stream");

        displayModeComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "displaymode", "Default");

        string autoPlayText = xmlreader.GetValueAsString("dvdplayer", "autoplay", "Ask");
        switch (autoPlayText)
        {
          case "No":
            autoPlayComboBox.Text = autoPlayOptions[1];
            break;
          case "Ask":
            autoPlayComboBox.Text = autoPlayOptions[2];
            break;
          default:
            autoPlayComboBox.Text = autoPlayOptions[0];
            break;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("dvdplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "subtitlelanguage", defaultSubtitleLanguageComboBox.Text);

        xmlwriter.SetValueAsBool("dvdplayer", "showsubtitles", showSubtitlesCheckBox.Checked);
        xmlwriter.SetValueAsBool("dvdplayer", "pixelratiocorrection", pixelRatioCheckBox.Checked);

        xmlwriter.SetValue("dvdplayer", "armode", aspectRatioComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "displaymode", displayModeComboBox.Text);

        string autoPlayText;

        if (autoPlayComboBox.Text == autoPlayOptions[1])
        {
          autoPlayText = "No";
        }
        else if (autoPlayComboBox.Text == autoPlayOptions[2])
        {
          autoPlayText = "Ask";
        }
        else
        {
          autoPlayText = "Yes";
        }

        xmlwriter.SetValue("dvdplayer", "autoplay", autoPlayText);
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
      this.defaultAudioLanguagelabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultAudioLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.defaultSubtitleLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.displayModeLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.displayModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.aspectRatioComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.aspectRatioLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.pixelRatioCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelAutoPlay = new MediaPortal.UserInterface.Controls.MPLabel();
      this.autoPlayComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.defaultAudioLanguagelabel);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.defaultAudioLanguageComboBox);
      this.groupBox1.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.groupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 104);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Language Settings";
      // 
      // defaultAudioLanguagelabel
      // 
      this.defaultAudioLanguagelabel.AutoSize = true;
      this.defaultAudioLanguagelabel.Location = new System.Drawing.Point(16, 72);
      this.defaultAudioLanguagelabel.Name = "defaultAudioLanguagelabel";
      this.defaultAudioLanguagelabel.Size = new System.Drawing.Size(133, 13);
      this.defaultAudioLanguagelabel.TabIndex = 3;
      this.defaultAudioLanguagelabel.Text = "Preferred audio language:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(142, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Preferred subtitle language:";
      // 
      // defaultAudioLanguageComboBox
      // 
      this.defaultAudioLanguageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultAudioLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultAudioLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultAudioLanguageComboBox.Location = new System.Drawing.Point(168, 68);
      this.defaultAudioLanguageComboBox.Name = "defaultAudioLanguageComboBox";
      this.defaultAudioLanguageComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultAudioLanguageComboBox.Sorted = true;
      this.defaultAudioLanguageComboBox.TabIndex = 4;
      // 
      // defaultSubtitleLanguageComboBox
      // 
      this.defaultSubtitleLanguageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultSubtitleLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultSubtitleLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultSubtitleLanguageComboBox.Location = new System.Drawing.Point(168, 44);
      this.defaultSubtitleLanguageComboBox.Name = "defaultSubtitleLanguageComboBox";
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 2;
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.AutoSize = true;
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(168, 20);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(213, 17);
      this.showSubtitlesCheckBox.TabIndex = 0;
      this.showSubtitlesCheckBox.Text = "Force subtitles (overrides disc defaults)";
      this.showSubtitlesCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.displayModeLabel);
      this.mpGroupBox1.Controls.Add(this.displayModeComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioLabel);
      this.mpGroupBox1.Controls.Add(this.pixelRatioCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 108);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 104);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Aspect Ratio";
      // 
      // displayModeLabel
      // 
      this.displayModeLabel.AutoSize = true;
      this.displayModeLabel.Location = new System.Drawing.Point(16, 72);
      this.displayModeLabel.Name = "displayModeLabel";
      this.displayModeLabel.Size = new System.Drawing.Size(74, 13);
      this.displayModeLabel.TabIndex = 3;
      this.displayModeLabel.Text = "Display mode:";
      // 
      // displayModeComboBox
      // 
      this.displayModeComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.displayModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.displayModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.displayModeComboBox.Items.AddRange(new object[]
                                                {
                                                  "Default",
                                                  "16:9",
                                                  "4:3 Pan Scan",
                                                  "4:3 Letterbox"
                                                });
      this.displayModeComboBox.Location = new System.Drawing.Point(168, 68);
      this.displayModeComboBox.Name = "displayModeComboBox";
      this.displayModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.displayModeComboBox.TabIndex = 4;
      // 
      // aspectRatioComboBox
      // 
      this.aspectRatioComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.aspectRatioComboBox.BorderColor = System.Drawing.Color.Empty;
      this.aspectRatioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aspectRatioComboBox.Items.AddRange(new object[]
                                                {
                                                  "Crop",
                                                  "Letterbox",
                                                  "Stretch",
                                                  "Follow stream"
                                                });
      this.aspectRatioComboBox.Location = new System.Drawing.Point(168, 44);
      this.aspectRatioComboBox.Name = "aspectRatioComboBox";
      this.aspectRatioComboBox.Size = new System.Drawing.Size(288, 21);
      this.aspectRatioComboBox.TabIndex = 2;
      // 
      // aspectRatioLabel
      // 
      this.aspectRatioLabel.AutoSize = true;
      this.aspectRatioLabel.Location = new System.Drawing.Point(16, 48);
      this.aspectRatioLabel.Name = "aspectRatioLabel";
      this.aspectRatioLabel.Size = new System.Drawing.Size(149, 13);
      this.aspectRatioLabel.TabIndex = 1;
      this.aspectRatioLabel.Text = "Aspect ratio correction mode:";
      // 
      // pixelRatioCheckBox
      // 
      this.pixelRatioCheckBox.AutoSize = true;
      this.pixelRatioCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.pixelRatioCheckBox.Location = new System.Drawing.Point(168, 20);
      this.pixelRatioCheckBox.Name = "pixelRatioCheckBox";
      this.pixelRatioCheckBox.Size = new System.Drawing.Size(143, 17);
      this.pixelRatioCheckBox.TabIndex = 0;
      this.pixelRatioCheckBox.Text = "Use pixel ratio correction";
      this.pixelRatioCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.labelAutoPlay);
      this.mpGroupBox2.Controls.Add(this.autoPlayComboBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(0, 216);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(472, 60);
      this.mpGroupBox2.TabIndex = 2;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Autoplay";
      // 
      // labelAutoPlay
      // 
      this.labelAutoPlay.AutoSize = true;
      this.labelAutoPlay.Location = new System.Drawing.Point(16, 28);
      this.labelAutoPlay.Name = "labelAutoPlay";
      this.labelAutoPlay.Size = new System.Drawing.Size(77, 13);
      this.labelAutoPlay.TabIndex = 0;
      this.labelAutoPlay.Text = "Autoplay DVD:";
      // 
      // autoPlayComboBox
      // 
      this.autoPlayComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.autoPlayComboBox.BorderColor = System.Drawing.Color.Empty;
      this.autoPlayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.autoPlayComboBox.Location = new System.Drawing.Point(168, 24);
      this.autoPlayComboBox.Name = "autoPlayComboBox";
      this.autoPlayComboBox.Size = new System.Drawing.Size(288, 21);
      this.autoPlayComboBox.TabIndex = 1;
      // 
      // DVD
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "DVD";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion
  }
}