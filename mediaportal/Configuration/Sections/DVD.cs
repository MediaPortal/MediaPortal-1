#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.ComponentModel;
using System.Globalization;
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
    private MPCheckBox showSubtitlesCheckBox;
    private MPLabel label1;
    private MPLabel defaultAudioLanguagelabel;
    private IContainer components = null;

    private string m_strDefaultRegionLanguage = "English";
    private MPCheckBox showClosedCaptions;

    public DVD()
      : this("DVD Discs/Images") {}

    public DVD(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Populate combo box with languages
      string curCultureTwoLetter = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      m_strDefaultRegionLanguage = CultureInfo.CurrentCulture.IsNeutralCulture
                                     ? CultureInfo.CurrentCulture.EnglishName
                                     : CultureInfo.CurrentCulture.Parent.EnglishName;

      Util.Utils.PopulateLanguagesToComboBox(defaultSubtitleLanguageComboBox, curCultureTwoLetter);
      Util.Utils.PopulateLanguagesToComboBox(defaultAudioLanguageComboBox, curCultureTwoLetter);
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
      using (Settings xmlreader = new MPSettings())
      {
        defaultAudioLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "audiolanguage",
                                                                               m_strDefaultRegionLanguage);
        defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage",
                                                                                  m_strDefaultRegionLanguage);

        showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", false);
        showClosedCaptions.Checked = xmlreader.GetValueAsBool("dvdplayer", "showclosedcaptions", false);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("dvdplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "subtitlelanguage", defaultSubtitleLanguageComboBox.Text);

        xmlwriter.SetValueAsBool("dvdplayer", "showsubtitles", showSubtitlesCheckBox.Checked);
        xmlwriter.SetValueAsBool("dvdplayer", "showclosedcaptions", showClosedCaptions.Checked);
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
      this.showClosedCaptions = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.defaultAudioLanguagelabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultAudioLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.defaultSubtitleLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.showClosedCaptions);
      this.groupBox1.Controls.Add(this.defaultAudioLanguagelabel);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.defaultAudioLanguageComboBox);
      this.groupBox1.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.groupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(6, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 104);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Language Settings";
      // 
      // showClosedCaptions
      // 
      this.showClosedCaptions.AutoSize = true;
      this.showClosedCaptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showClosedCaptions.Location = new System.Drawing.Point(242, 19);
      this.showClosedCaptions.Name = "showClosedCaptions";
      this.showClosedCaptions.Size = new System.Drawing.Size(153, 17);
      this.showClosedCaptions.TabIndex = 5;
      this.showClosedCaptions.Text = "Show Closed Captions (CC)";
      this.showClosedCaptions.UseVisualStyleBackColor = true;
      // 
      // defaultAudioLanguagelabel
      // 
      this.defaultAudioLanguagelabel.AutoSize = true;
      this.defaultAudioLanguagelabel.Location = new System.Drawing.Point(16, 72);
      this.defaultAudioLanguagelabel.Name = "defaultAudioLanguagelabel";
      this.defaultAudioLanguagelabel.Size = new System.Drawing.Size(129, 13);
      this.defaultAudioLanguagelabel.TabIndex = 3;
      this.defaultAudioLanguagelabel.Text = "Preferred audio language:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(136, 13);
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
      this.defaultAudioLanguageComboBox.Size = new System.Drawing.Size(278, 21);
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
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(278, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 2;
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.AutoSize = true;
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(19, 19);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(206, 17);
      this.showSubtitlesCheckBox.TabIndex = 0;
      this.showSubtitlesCheckBox.Text = "Show subtitles (overrides disc defaults)";
      this.showSubtitlesCheckBox.UseVisualStyleBackColor = true;
      // 
      // DVD
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "DVD";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion
  }
}