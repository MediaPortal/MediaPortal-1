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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Movies : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPButton fileNameButton;
    private MPTextBox folderNameTextBox;
    private MPLabel folderNameLabel;
    private MPGroupBox mpGroupBox1;
    private MPLabel label4;
    private MPLabel label6;
    private MPCheckBox repeatPlaylistCheckBox;
    private MPCheckBox showSubtitlesCheckBox;
    private MPButton subtitlesButton;
    private MPTextBox subtitlesFontTextBox;
    private FolderBrowserDialog folderBrowserDialog;
    private FontDialog fontDialog;
    private IContainer components = null;
    private string fontName;
    private string fontColor;
    private bool fontIsBold;
    private MPTextBox dropShadowTextBox;
    private int fontSize;
    private MPComboBox defaultZoomModeComboBox;
    private MPLabel label1;
    private MPTabControl tabControl1;
    private MPTabPage tabPage1;
    private MPTabPage tabPage3;
    private MPLabel label7;
    private MPComboBox defaultSubtitleLanguageComboBox;
    private MPCheckBox checkBoxShowWatched;
    private MPGroupBox gAllowedModes;
    private MPCheckBox cbAllowNormal;
    private MPCheckBox cbAllowZoom149;
    private MPCheckBox cbAllowOriginal;
    private MPCheckBox cbAllowZoom;
    private MPCheckBox cbAllowLetterbox;
    private MPCheckBox cbAllowStretch;
    private MPCheckBox cbAllowNonLinearStretch;
    private string m_strDefaultRegionLanguage = "English";
    private MPCheckBox checkBoxEachFolderIsMovie;
    private MPLabel labelsubsinfo;
    private MPGroupBox mpGroupBox2;
    private MPLabel mpLabel1;
    private MPComboBox defaultAudioLanguageComboBox;

    private string[] aspectRatio = {
                                     "normal",
                                     "original",
                                     "zoom",
                                     "zoom149",
                                     "stretch",
                                     "smartstretch",
                                     "letterbox"
                                   };

    private string m_strDefaultAudioLanguage = "English";

    public Movies()
      : this("Videos")
    {
    }

    public Movies(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      // Populate combo boxes with languages
      m_strDefaultRegionLanguage = Util.Utils.GetCultureRegionLanguage();
      m_strDefaultAudioLanguage = Util.Utils.GetCultureRegionLanguage();
      defaultSubtitleLanguageComboBox.Text = m_strDefaultRegionLanguage;
      defaultAudioLanguageComboBox.Text = m_strDefaultAudioLanguage;
      Util.Utils.PopulateLanguagesToComboBox(defaultSubtitleLanguageComboBox, m_strDefaultRegionLanguage);
      Util.Utils.PopulateLanguagesToComboBox(defaultAudioLanguageComboBox, m_strDefaultAudioLanguage);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbAllowNormal.Checked = xmlreader.GetValueAsBool("movies", "allowarnormal", true);
        cbAllowOriginal.Checked = xmlreader.GetValueAsBool("movies", "allowaroriginal", true);
        cbAllowZoom.Checked = xmlreader.GetValueAsBool("movies", "allowarzoom", true);
        cbAllowZoom149.Checked = xmlreader.GetValueAsBool("movies", "allowarzoom149", true);
        cbAllowStretch.Checked = xmlreader.GetValueAsBool("movies", "allowarstretch", true);
        cbAllowNonLinearStretch.Checked = xmlreader.GetValueAsBool("movies", "allownonlinear", true);
        cbAllowLetterbox.Checked = xmlreader.GetValueAsBool("movies", "allowarletterbox", true);

        checkBoxEachFolderIsMovie.Checked = xmlreader.GetValueAsBool("movies", "eachFolderIsMovie", false);

        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        folderNameTextBox.Text = xmlreader.GetValueAsString("movies", "playlists", playListFolder);

        if (string.Compare(folderNameTextBox.Text, playListFolder) == 0)
        {
          if (Directory.Exists(playListFolder) == false)
          {
            try
            {
              Directory.CreateDirectory(playListFolder);
            }
            catch (Exception)
            {
            }
          }
        }

        repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("movies", "repeat", true);

        showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "enabled", false);
        checkBoxShowWatched.Checked = xmlreader.GetValueAsBool("movies", "markwatched", true);

        defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("subtitles", "language",
                                                                                  m_strDefaultRegionLanguage);

        dropShadowTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("subtitles", "shadow", 5));

        //
        // Get font settings
        //
        fontName = xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
        fontColor = xmlreader.GetValueAsString("subtitles", "color", "ffffff");
        fontIsBold = xmlreader.GetValueAsBool("subtitles", "bold", true);
        fontSize = xmlreader.GetValueAsInt("subtitles", "fontsize", 18);

        subtitlesFontTextBox.Text = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

        //
        // Try to parse the specified color into a valid color
        //
        if (fontColor != null && fontColor.Length > 0)
        {
          try
          {
            int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
            subtitlesFontTextBox.BackColor = Color.Black;
            subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
          }
          catch
          {
          }
        }

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("movieplayer", "defaultar", "normal");

        for (int index = 0; index < aspectRatio.Length; index++)
        {
          if (aspectRatio[index].Equals(defaultAspectRatio))
          {
            defaultZoomModeComboBox.SelectedIndex = index;
            break;
          }
        }
        defaultAudioLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "audiolanguage",
                                                                               m_strDefaultAudioLanguage);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("movies", "repeat", repeatPlaylistCheckBox.Checked);
        xmlwriter.SetValue("movies", "playlists", folderNameTextBox.Text);

        xmlwriter.SetValueAsBool("movies", "markwatched", checkBoxShowWatched.Checked);
        xmlwriter.SetValueAsBool("movies", "eachFolderIsMovie", checkBoxEachFolderIsMovie.Checked);

        xmlwriter.SetValueAsBool("subtitles", "enabled", showSubtitlesCheckBox.Checked);
        xmlwriter.SetValue("subtitles", "shadow", dropShadowTextBox.Text);
        xmlwriter.SetValue("subtitles", "language", defaultSubtitleLanguageComboBox.Text);

        xmlwriter.SetValue("subtitles", "fontface", fontName);
        xmlwriter.SetValue("subtitles", "color", fontColor);
        xmlwriter.SetValueAsBool("subtitles", "bold", fontIsBold);
        xmlwriter.SetValue("subtitles", "fontsize", fontSize);

        xmlwriter.SetValue("movieplayer", "defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);

        xmlwriter.SetValueAsBool("movies", "allowarnormal", cbAllowNormal.Checked);
        xmlwriter.SetValueAsBool("movies", "allowaroriginal", cbAllowOriginal.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarzoom", cbAllowZoom.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarzoom149", cbAllowZoom149.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarstretch", cbAllowStretch.Checked);
        xmlwriter.SetValueAsBool("movies", "allownonlinear", cbAllowNonLinearStretch.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarletterbox", cbAllowLetterbox.Checked);

        xmlwriter.SetValue("movieplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
      }
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Movies));
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxEachFolderIsMovie = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowWatched = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.defaultZoomModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.folderNameLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultSubtitleLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.dropShadowTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.subtitlesButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.subtitlesFontTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.fontDialog = new System.Windows.Forms.FontDialog();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.gAllowedModes = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAllowNormal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowZoom149 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowOriginal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowZoom = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowLetterbox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowNonLinearStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultAudioLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelsubsinfo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.gAllowedModes.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxEachFolderIsMovie);
      this.groupBox1.Controls.Add(this.checkBoxShowWatched);
      this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.fileNameButton);
      this.groupBox1.Controls.Add(this.folderNameTextBox);
      this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
      this.groupBox1.Controls.Add(this.folderNameLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 168);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // checkBoxEachFolderIsMovie
      // 
      this.checkBoxEachFolderIsMovie.AutoSize = true;
      this.checkBoxEachFolderIsMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEachFolderIsMovie.Location = new System.Drawing.Point(19, 139);
      this.checkBoxEachFolderIsMovie.Name = "checkBoxEachFolderIsMovie";
      this.checkBoxEachFolderIsMovie.Size = new System.Drawing.Size(181, 17);
      this.checkBoxEachFolderIsMovie.TabIndex = 7;
      this.checkBoxEachFolderIsMovie.Text = "Every movie has its own directory";
      this.checkBoxEachFolderIsMovie.UseVisualStyleBackColor = true;
      // 
      // checkBoxShowWatched
      // 
      this.checkBoxShowWatched.AutoSize = true;
      this.checkBoxShowWatched.Checked = true;
      this.checkBoxShowWatched.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxShowWatched.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowWatched.Location = new System.Drawing.Point(19, 116);
      this.checkBoxShowWatched.Name = "checkBoxShowWatched";
      this.checkBoxShowWatched.Size = new System.Drawing.Size(381, 17);
      this.checkBoxShowWatched.TabIndex = 6;
      this.checkBoxShowWatched.Text = "Mark every already watched file (deactivate for performance with many files)";
      this.checkBoxShowWatched.UseVisualStyleBackColor = true;
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Original Source Format",
            "Zoom",
            "Zoom 14:9",
            "Stretch",
            "Non-linear Smart Zoom",
            "4:3 Letterbox"});
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(136, 24);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(280, 21);
      this.defaultZoomModeComboBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(120, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Default zoom mode:";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.Location = new System.Drawing.Point(344, 61);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 4;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.UseVisualStyleBackColor = true;
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.folderNameTextBox.Location = new System.Drawing.Point(136, 63);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(200, 20);
      this.folderNameTextBox.TabIndex = 3;
      // 
      // repeatPlaylistCheckBox
      // 
      this.repeatPlaylistCheckBox.AutoSize = true;
      this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(19, 93);
      this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
      this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(152, 17);
      this.repeatPlaylistCheckBox.TabIndex = 5;
      this.repeatPlaylistCheckBox.Text = "Repeat/loop video playlists";
      this.repeatPlaylistCheckBox.UseVisualStyleBackColor = true;
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 66);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(80, 16);
      this.folderNameLabel.TabIndex = 2;
      this.folderNameLabel.Text = "Playlist folder:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.label7);
      this.mpGroupBox1.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.mpGroupBox1.Controls.Add(this.dropShadowTextBox);
      this.mpGroupBox1.Controls.Add(this.label4);
      this.mpGroupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.mpGroupBox1.Controls.Add(this.subtitlesButton);
      this.mpGroupBox1.Controls.Add(this.subtitlesFontTextBox);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 191);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Subtitles";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 104);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(96, 16);
      this.label7.TabIndex = 6;
      this.label7.Text = "Default language:";
      // 
      // defaultSubtitleLanguageComboBox
      // 
      this.defaultSubtitleLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultSubtitleLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultSubtitleLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultSubtitleLanguageComboBox.Location = new System.Drawing.Point(136, 100);
      this.defaultSubtitleLanguageComboBox.Name = "defaultSubtitleLanguageComboBox";
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(280, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 7;
      // 
      // dropShadowTextBox
      // 
      this.dropShadowTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dropShadowTextBox.BorderColor = System.Drawing.Color.Empty;
      this.dropShadowTextBox.Location = new System.Drawing.Point(136, 76);
      this.dropShadowTextBox.Name = "dropShadowTextBox";
      this.dropShadowTextBox.Size = new System.Drawing.Size(280, 20);
      this.dropShadowTextBox.TabIndex = 5;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 80);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(120, 16);
      this.label4.TabIndex = 4;
      this.label4.Text = "Drop shadow (pixels):";
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.AutoSize = true;
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(16, 24);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(150, 17);
      this.showSubtitlesCheckBox.TabIndex = 0;
      this.showSubtitlesCheckBox.Text = "Show subtitles (.srt or .sub)";
      this.showSubtitlesCheckBox.UseVisualStyleBackColor = true;
      // 
      // subtitlesButton
      // 
      this.subtitlesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.subtitlesButton.Location = new System.Drawing.Point(344, 51);
      this.subtitlesButton.Name = "subtitlesButton";
      this.subtitlesButton.Size = new System.Drawing.Size(72, 22);
      this.subtitlesButton.TabIndex = 3;
      this.subtitlesButton.Text = "Browse";
      this.subtitlesButton.UseVisualStyleBackColor = true;
      this.subtitlesButton.Click += new System.EventHandler(this.subtitlesButton_Click);
      // 
      // subtitlesFontTextBox
      // 
      this.subtitlesFontTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.subtitlesFontTextBox.BorderColor = System.Drawing.Color.Empty;
      this.subtitlesFontTextBox.Location = new System.Drawing.Point(136, 52);
      this.subtitlesFontTextBox.Name = "subtitlesFontTextBox";
      this.subtitlesFontTextBox.ReadOnly = true;
      this.subtitlesFontTextBox.Size = new System.Drawing.Size(200, 20);
      this.subtitlesFontTextBox.TabIndex = 2;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 56);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(72, 16);
      this.label6.TabIndex = 1;
      this.label6.Text = "Display font:";
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage3);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.gAllowedModes);
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // gAllowedModes
      // 
      this.gAllowedModes.Controls.Add(this.cbAllowNormal);
      this.gAllowedModes.Controls.Add(this.cbAllowZoom149);
      this.gAllowedModes.Controls.Add(this.cbAllowOriginal);
      this.gAllowedModes.Controls.Add(this.cbAllowZoom);
      this.gAllowedModes.Controls.Add(this.cbAllowLetterbox);
      this.gAllowedModes.Controls.Add(this.cbAllowStretch);
      this.gAllowedModes.Controls.Add(this.cbAllowNonLinearStretch);
      this.gAllowedModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gAllowedModes.Location = new System.Drawing.Point(16, 190);
      this.gAllowedModes.Name = "gAllowedModes";
      this.gAllowedModes.Size = new System.Drawing.Size(186, 189);
      this.gAllowedModes.TabIndex = 1;
      this.gAllowedModes.TabStop = false;
      this.gAllowedModes.Text = "Allowed zoom modes";
      // 
      // cbAllowNormal
      // 
      this.cbAllowNormal.AutoSize = true;
      this.cbAllowNormal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowNormal.Location = new System.Drawing.Point(15, 22);
      this.cbAllowNormal.Name = "cbAllowNormal";
      this.cbAllowNormal.Size = new System.Drawing.Size(151, 17);
      this.cbAllowNormal.TabIndex = 0;
      this.cbAllowNormal.Text = "Normal (aspect auto mode)";
      this.cbAllowNormal.UseVisualStyleBackColor = true;
      // 
      // cbAllowZoom149
      // 
      this.cbAllowZoom149.AutoSize = true;
      this.cbAllowZoom149.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowZoom149.Location = new System.Drawing.Point(15, 91);
      this.cbAllowZoom149.Name = "cbAllowZoom149";
      this.cbAllowZoom149.Size = new System.Drawing.Size(73, 17);
      this.cbAllowZoom149.TabIndex = 3;
      this.cbAllowZoom149.Text = "14:9 zoom";
      this.cbAllowZoom149.UseVisualStyleBackColor = true;
      // 
      // cbAllowOriginal
      // 
      this.cbAllowOriginal.AutoSize = true;
      this.cbAllowOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowOriginal.Location = new System.Drawing.Point(15, 45);
      this.cbAllowOriginal.Name = "cbAllowOriginal";
      this.cbAllowOriginal.Size = new System.Drawing.Size(126, 17);
      this.cbAllowOriginal.TabIndex = 1;
      this.cbAllowOriginal.Text = "Original source format";
      this.cbAllowOriginal.UseVisualStyleBackColor = true;
      // 
      // cbAllowZoom
      // 
      this.cbAllowZoom.AutoSize = true;
      this.cbAllowZoom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowZoom.Location = new System.Drawing.Point(15, 68);
      this.cbAllowZoom.Name = "cbAllowZoom";
      this.cbAllowZoom.Size = new System.Drawing.Size(51, 17);
      this.cbAllowZoom.TabIndex = 2;
      this.cbAllowZoom.Text = "Zoom";
      this.cbAllowZoom.UseVisualStyleBackColor = true;
      // 
      // cbAllowLetterbox
      // 
      this.cbAllowLetterbox.AutoSize = true;
      this.cbAllowLetterbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowLetterbox.Location = new System.Drawing.Point(15, 160);
      this.cbAllowLetterbox.Name = "cbAllowLetterbox";
      this.cbAllowLetterbox.Size = new System.Drawing.Size(86, 17);
      this.cbAllowLetterbox.TabIndex = 6;
      this.cbAllowLetterbox.Text = "4:3 Letterbox";
      this.cbAllowLetterbox.UseVisualStyleBackColor = true;
      // 
      // cbAllowStretch
      // 
      this.cbAllowStretch.AutoSize = true;
      this.cbAllowStretch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowStretch.Location = new System.Drawing.Point(15, 114);
      this.cbAllowStretch.Name = "cbAllowStretch";
      this.cbAllowStretch.Size = new System.Drawing.Size(107, 17);
      this.cbAllowStretch.TabIndex = 4;
      this.cbAllowStretch.Text = "Fullscreen stretch";
      this.cbAllowStretch.UseVisualStyleBackColor = true;
      // 
      // cbAllowNonLinearStretch
      // 
      this.cbAllowNonLinearStretch.AutoSize = true;
      this.cbAllowNonLinearStretch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowNonLinearStretch.Location = new System.Drawing.Point(15, 137);
      this.cbAllowNonLinearStretch.Name = "cbAllowNonLinearStretch";
      this.cbAllowNonLinearStretch.Size = new System.Drawing.Size(140, 17);
      this.cbAllowNonLinearStretch.TabIndex = 5;
      this.cbAllowNonLinearStretch.Text = "Non-linear stretch && crop";
      this.cbAllowNonLinearStretch.UseVisualStyleBackColor = true;
      // 
      // tabPage3
      // 
      this.tabPage3.Controls.Add(this.mpGroupBox2);
      this.tabPage3.Controls.Add(this.labelsubsinfo);
      this.tabPage3.Controls.Add(this.mpGroupBox1);
      this.tabPage3.Location = new System.Drawing.Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new System.Drawing.Size(464, 382);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Subtitle / Audio";
      this.tabPage3.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.mpLabel1);
      this.mpGroupBox2.Controls.Add(this.defaultAudioLanguageComboBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(16, 230);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(432, 73);
      this.mpGroupBox2.TabIndex = 8;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Audio Language";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 32);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(96, 16);
      this.mpLabel1.TabIndex = 6;
      this.mpLabel1.Text = "Default language:";
      // 
      // defaultAudioLanguageComboBox
      // 
      this.defaultAudioLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultAudioLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultAudioLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultAudioLanguageComboBox.Location = new System.Drawing.Point(136, 28);
      this.defaultAudioLanguageComboBox.Name = "defaultAudioLanguageComboBox";
      this.defaultAudioLanguageComboBox.Size = new System.Drawing.Size(280, 21);
      this.defaultAudioLanguageComboBox.Sorted = true;
      this.defaultAudioLanguageComboBox.TabIndex = 7;
      // 
      // labelsubsinfo
      // 
      this.labelsubsinfo.Location = new System.Drawing.Point(32, 155);
      this.labelsubsinfo.Name = "labelsubsinfo";
      this.labelsubsinfo.Size = new System.Drawing.Size(400, 49);
      this.labelsubsinfo.TabIndex = 10;
      this.labelsubsinfo.Text = resources.GetString("labelsubsinfo.Text");
      // 
      // Movies
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "Movies";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.gAllowedModes.ResumeLayout(false);
      this.gAllowedModes.PerformLayout();
      this.tabPage3.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void fileNameButton_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where movie playlists will be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void subtitlesButton_Click(object sender, EventArgs e)
    {
      using (fontDialog = new FontDialog())
      {
        fontDialog.AllowScriptChange = false;
        fontDialog.ShowColor = true;
        fontDialog.FontMustExist = true;
        fontDialog.ShowEffects = true;

        fontDialog.Font = new Font(fontName, (float) fontSize, fontIsBold ? FontStyle.Bold : FontStyle.Regular);

        if (fontColor != null && fontColor.Length > 0)
        {
          fontDialog.Color = subtitlesFontTextBox.BackColor;
        }

        DialogResult dialogResult = fontDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          fontName = fontDialog.Font.Name;
          fontSize = (int) fontDialog.Font.Size;
          fontIsBold = fontDialog.Font.Style == FontStyle.Bold;
          fontColor = String.Format("{0:x}", fontDialog.Color.ToArgb());

          subtitlesFontTextBox.Text = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

          //
          // Try to parse the specified color into a valid color
          //
          if (fontColor != null && fontColor.Length > 0)
          {
            try
            {
              int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
              subtitlesFontTextBox.BackColor = Color.Black;
              subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
            }
            catch
            {
            }
          }
        }
      }
    }
  }
}