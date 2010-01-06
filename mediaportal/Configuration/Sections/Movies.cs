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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using DShowNET.Helper;
using System.Runtime.InteropServices;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Movies : SectionSettings
  {
    private FolderBrowserDialog folderBrowserDialog;
    private FontDialog fontDialog;
    private string fontName;
    private byte fontCharset;
    private string fontColor;
    private bool fontIsBold;
    private int fontSize;
    private readonly string m_strDefaultSubtitleLanguageISO = "EN";
    private readonly string m_strDefaultAudioLanguageISO = "EN";
    private MPTabPage tabPage1;
    private MPGroupBox groupBox1;
    private MPCheckBox checkBoxEachFolderIsMovie;
    private MPCheckBox checkBoxShowWatched;
    private MPButton fileNameButton;
    private MPTextBox folderNameTextBox;
    private MPCheckBox repeatPlaylistCheckBox;
    private MPLabel folderNameLabel;
    private MPButton advancedButton;
    private MPGroupBox mpGroupBox1;
    private MPCheckBox subPosRelativeCheckBox;
    private MPRadioButton saveAlwaysRadioButton;
    private MPRadioButton saveAskRadioButton;
    private MPRadioButton saveNeverRadioButton;
    private MPLabel mpLabel6;
    private MPLabel mpLabel5;
    private MPNumericTextBox delayTextBox;
    private MPLabel mpLabel4;
    private MPCheckBox showSubtitlesCheckBox;
    private MPTabPage mpTabPage1;
    private MPGroupBox mpGroupBox4;
    private MPLabel mpLabel7;
    private MPComboBox defaultAudioLanguageComboBox;
    private MPLabel mpLabel8;
    private MPComboBox defaultSubtitleLanguageComboBox;
    private MPGroupBox mpGroupBox2;
    private MPCheckBox subStyleOverrideCheckBox;
    private MPRadioButton opaqueBoxRadioButton;
    private MPRadioButton borderOutlineRadioButton;
    private MPNumericUpDown borderWidthUpDown;
    private MPNumericUpDown shadowDepthUpDown;
    private MPLabel mpLabel1;
    private MPLabel mpLabel9;
    private MPButton subtitlesButton;
    private MPTextBox subtitlesFontTextBox;
    private MPLabel mpLabel10;
    private MPTabControl tabControl1;
    private MPComboBox subEnginesCombo;
    private MPLabel mpLabel3;
    private TextBox subPaths;
    private Label label2;
    private List<LanguageInfo> ISOLanguagePairs = new List<LanguageInfo>();

    //private int 

    public Movies()
      : this("Videos") {}

    public Movies(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Populate combo boxes with languages
      string curCultureName = CultureInfo.CurrentCulture.Name;
      string curCultureTwoLetter = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

      m_strDefaultSubtitleLanguageISO = curCultureName;
      m_strDefaultAudioLanguageISO = curCultureName;

      Util.Utils.PopulateLanguagesToComboBox(defaultSubtitleLanguageComboBox, curCultureTwoLetter);
      Util.Utils.PopulateLanguagesToComboBox(defaultAudioLanguageComboBox, curCultureTwoLetter);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
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
            catch {}
          }
        }

        repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("movies", "repeat", true);

        subEnginesCombo.SelectedItem = xmlreader.GetValueAsString("subtitles", "engine", "MPC-HC");
        subPaths.Text = xmlreader.GetValueAsString("subtitles", "paths", @".\,.\Subtitles\");
        showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "enabled", false);
        checkBoxShowWatched.Checked = xmlreader.GetValueAsBool("movies", "markwatched", true);

        try
        {
          CultureInfo ci =
            new CultureInfo(xmlreader.GetValueAsString("subtitles", "language", m_strDefaultSubtitleLanguageISO));
          defaultSubtitleLanguageComboBox.SelectedItem = !ci.IsNeutralCulture ? ci.Parent.EnglishName : ci.EnglishName;
        }
        catch (Exception ex)
        {
          CultureInfo ci = new CultureInfo(m_strDefaultSubtitleLanguageISO);
          Log.Error("LoadSettings - failed to load default subtitle language, using {0} - {1} ", ci.EnglishName, ex);
          defaultSubtitleLanguageComboBox.SelectedItem = ci.EnglishName;
        }

        shadowDepthUpDown.Value = xmlreader.GetValueAsInt("subtitles", "shadow", 3);
        borderWidthUpDown.Value = xmlreader.GetValueAsInt("subtitles", "borderWidth", 2);
        borderOutlineRadioButton.Checked = xmlreader.GetValueAsBool("subtitles", "borderOutline", true);
        opaqueBoxRadioButton.Checked = !borderOutlineRadioButton.Checked;

        delayTextBox.Value = xmlreader.GetValueAsInt("subtitles", "delayInterval", 250);
        saveNeverRadioButton.Checked = xmlreader.GetValueAsBool("subtitles", "saveNever", true);
        saveAskRadioButton.Checked = xmlreader.GetValueAsBool("subtitles", "saveAsk", false);
        saveAlwaysRadioButton.Checked = !saveNeverRadioButton.Checked && !saveAskRadioButton.Checked;

        //
        // Get font settings
        //
        fontName = xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
        fontColor = xmlreader.GetValueAsString("subtitles", "color", "ffffff");
        fontIsBold = xmlreader.GetValueAsBool("subtitles", "bold", true);
        fontSize = xmlreader.GetValueAsInt("subtitles", "fontsize", 18);
        fontCharset = (byte)xmlreader.GetValueAsInt("subtitles", "charset", 1); //default charset

        subtitlesFontTextBox.Text = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

        //
        // Try to parse the specified color into a valid color
        //
        if (!string.IsNullOrEmpty(fontColor))
        {
          try
          {
            int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
            subtitlesFontTextBox.BackColor = Color.Black;
            subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
          }
          catch {}
        }
        subStyleOverrideCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "subStyleOverride", false);
        subPosRelativeCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "subPosRelative", false);

        try
        {
          CultureInfo ci =
            new CultureInfo(xmlreader.GetValueAsString("movieplayer", "audiolanguage", m_strDefaultAudioLanguageISO));
          defaultAudioLanguageComboBox.SelectedItem = !ci.IsNeutralCulture ? ci.Parent.EnglishName : ci.EnglishName;
        }
        catch (Exception ex)
        {
          CultureInfo ci = new CultureInfo(m_strDefaultAudioLanguageISO);
          Log.Error("LoadSettings - failed to load default audio language, using {0} - {1} ", ci.EnglishName, ex);
          defaultAudioLanguageComboBox.SelectedItem = ci.EnglishName;
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("movies", "repeat", repeatPlaylistCheckBox.Checked);
        xmlwriter.SetValue("movies", "playlists", folderNameTextBox.Text);

        xmlwriter.SetValueAsBool("movies", "markwatched", checkBoxShowWatched.Checked);
        xmlwriter.SetValueAsBool("movies", "eachFolderIsMovie", checkBoxEachFolderIsMovie.Checked);

        xmlwriter.SetValue("subtitles", "engine", subEnginesCombo.SelectedItem);
        xmlwriter.SetValue("subtitles", "paths", subPaths.Text);

        xmlwriter.SetValueAsBool("subtitles", "enabled", showSubtitlesCheckBox.Checked);
        xmlwriter.SetValue("subtitles", "shadow", shadowDepthUpDown.Value);
        xmlwriter.SetValue("subtitles", "borderWidth", borderWidthUpDown.Value);
        xmlwriter.SetValueAsBool("subtitles", "borderOutline", borderOutlineRadioButton.Checked);

        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
          if (ci.EnglishName == defaultSubtitleLanguageComboBox.Text)
          {
            xmlwriter.SetValue("subtitles", "language", ci.Name);
          }
        }

        xmlwriter.SetValue("subtitles", "fontface", fontName);
        xmlwriter.SetValue("subtitles", "color", fontColor);
        xmlwriter.SetValueAsBool("subtitles", "bold", fontIsBold);
        xmlwriter.SetValue("subtitles", "fontsize", fontSize);
        xmlwriter.SetValue("subtitles", "charset", fontCharset);

        xmlwriter.SetValue("subtitles", "delayInterval", delayTextBox.Value);
        xmlwriter.SetValueAsBool("subtitles", "saveNever", saveNeverRadioButton.Checked);
        xmlwriter.SetValueAsBool("subtitles", "saveAsk", saveAskRadioButton.Checked);

        xmlwriter.SetValueAsBool("subtitles", "subStyleOverride", subStyleOverrideCheckBox.Checked);
        xmlwriter.SetValueAsBool("subtitles", "subPosRelative", subPosRelativeCheckBox.Checked);

        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
          if (ci.EnglishName == defaultAudioLanguageComboBox.Text)
          {
            xmlwriter.SetValue("movieplayer", "audiolanguage", ci.Name);
          }
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
      MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
      MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
      MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
      System.Windows.Forms.Panel panel1;
      MediaPortal.UserInterface.Controls.MPTabPage mpTabPage2;
      this.subEnginesCombo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.advancedButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.subPaths = new System.Windows.Forms.TextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.subPosRelativeCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.saveAlwaysRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.saveAskRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.saveNeverRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.delayTextBox = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.subStyleOverrideCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.opaqueBoxRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.borderOutlineRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.borderWidthUpDown = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.shadowDepthUpDown = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.subtitlesButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.subtitlesFontTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.fontDialog = new System.Windows.Forms.FontDialog();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxEachFolderIsMovie = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowWatched = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.folderNameLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.mpTabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultAudioLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultSubtitleLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      panel1 = new System.Windows.Forms.Panel();
      mpTabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      tabPage3.SuspendLayout();
      mpGroupBox3.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      panel1.SuspendLayout();
      mpTabPage2.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.borderWidthUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.shadowDepthUpDown)).BeginInit();
      this.tabPage1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.mpTabPage1.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabPage3
      // 
      tabPage3.Controls.Add(mpGroupBox3);
      tabPage3.Controls.Add(this.mpGroupBox1);
      tabPage3.Location = new System.Drawing.Point(4, 22);
      tabPage3.Name = "tabPage3";
      tabPage3.Size = new System.Drawing.Size(464, 382);
      tabPage3.TabIndex = 7;
      tabPage3.Text = "Subtitle";
      tabPage3.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox3
      // 
      mpGroupBox3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      mpGroupBox3.Controls.Add(mpLabel2);
      mpGroupBox3.Controls.Add(this.subEnginesCombo);
      mpGroupBox3.Controls.Add(this.advancedButton);
      mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      mpGroupBox3.Location = new System.Drawing.Point(15, 12);
      mpGroupBox3.Name = "mpGroupBox3";
      mpGroupBox3.Size = new System.Drawing.Size(432, 90);
      mpGroupBox3.TabIndex = 1;
      mpGroupBox3.TabStop = false;
      mpGroupBox3.Text = "Engine";
      // 
      // mpLabel2
      // 
      mpLabel2.Location = new System.Drawing.Point(13, 23);
      mpLabel2.Name = "mpLabel2";
      mpLabel2.Size = new System.Drawing.Size(199, 16);
      mpLabel2.TabIndex = 10;
      mpLabel2.Text = "Select subtitles engine:";
      // 
      // subEnginesCombo
      // 
      this.subEnginesCombo.BorderColor = System.Drawing.Color.Empty;
      this.subEnginesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.subEnginesCombo.FormattingEnabled = true;
      this.subEnginesCombo.Items.AddRange(new object[]
                                            {
                                              "MPC-HC",
                                              "DirectVobSub",
                                              "Disabled"
                                            });
      this.subEnginesCombo.Location = new System.Drawing.Point(16, 46);
      this.subEnginesCombo.Name = "subEnginesCombo";
      this.subEnginesCombo.Size = new System.Drawing.Size(246, 21);
      this.subEnginesCombo.TabIndex = 7;
      this.subEnginesCombo.SelectedIndexChanged += new System.EventHandler(this.subEnginesCombo_SelectedIndexChanged);
      // 
      // advancedButton
      // 
      this.advancedButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.advancedButton.Location = new System.Drawing.Point(337, 46);
      this.advancedButton.Name = "advancedButton";
      this.advancedButton.Size = new System.Drawing.Size(72, 22);
      this.advancedButton.TabIndex = 6;
      this.advancedButton.Text = "Advanced";
      this.advancedButton.UseVisualStyleBackColor = true;
      this.advancedButton.Click += new System.EventHandler(this.advancedButton_Click);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.subPaths);
      this.mpGroupBox1.Controls.Add(this.mpLabel3);
      this.mpGroupBox1.Controls.Add(this.subPosRelativeCheckBox);
      this.mpGroupBox1.Controls.Add(panel1);
      this.mpGroupBox1.Controls.Add(this.mpLabel6);
      this.mpGroupBox1.Controls.Add(this.mpLabel5);
      this.mpGroupBox1.Controls.Add(this.delayTextBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel4);
      this.mpGroupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(15, 119);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 210);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Options";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(13, 68);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(81, 13);
      this.label2.TabIndex = 17;
      this.label2.Text = "Subtitle paths: *";
      // 
      // subPaths
      // 
      this.subPaths.Location = new System.Drawing.Point(100, 65);
      this.subPaths.Name = "subPaths";
      this.subPaths.Size = new System.Drawing.Size(309, 20);
      this.subPaths.TabIndex = 16;
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new System.Drawing.Point(13, 183);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(199, 16);
      this.mpLabel3.TabIndex = 15;
      this.mpLabel3.Text = "* - supported by MPC-HC engine only";
      // 
      // subPosRelativeCheckBox
      // 
      this.subPosRelativeCheckBox.AutoSize = true;
      this.subPosRelativeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.subPosRelativeCheckBox.Location = new System.Drawing.Point(16, 42);
      this.subPosRelativeCheckBox.Name = "subPosRelativeCheckBox";
      this.subPosRelativeCheckBox.Size = new System.Drawing.Size(193, 17);
      this.subPosRelativeCheckBox.TabIndex = 14;
      this.subPosRelativeCheckBox.Text = "Position relative to the video frame *";
      this.subPosRelativeCheckBox.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      panel1.Controls.Add(this.saveAlwaysRadioButton);
      panel1.Controls.Add(this.saveAskRadioButton);
      panel1.Controls.Add(this.saveNeverRadioButton);
      panel1.Location = new System.Drawing.Point(16, 144);
      panel1.Name = "panel1";
      panel1.Size = new System.Drawing.Size(246, 26);
      panel1.TabIndex = 13;
      // 
      // saveAlwaysRadioButton
      // 
      this.saveAlwaysRadioButton.AutoSize = true;
      this.saveAlwaysRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.saveAlwaysRadioButton.Location = new System.Drawing.Point(173, 1);
      this.saveAlwaysRadioButton.Name = "saveAlwaysRadioButton";
      this.saveAlwaysRadioButton.Size = new System.Drawing.Size(57, 17);
      this.saveAlwaysRadioButton.TabIndex = 12;
      this.saveAlwaysRadioButton.Text = "Always";
      this.saveAlwaysRadioButton.UseVisualStyleBackColor = true;
      // 
      // saveAskRadioButton
      // 
      this.saveAskRadioButton.AutoSize = true;
      this.saveAskRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.saveAskRadioButton.Location = new System.Drawing.Point(98, 1);
      this.saveAskRadioButton.Name = "saveAskRadioButton";
      this.saveAskRadioButton.Size = new System.Drawing.Size(42, 17);
      this.saveAskRadioButton.TabIndex = 11;
      this.saveAskRadioButton.Text = "Ask";
      this.saveAskRadioButton.UseVisualStyleBackColor = true;
      // 
      // saveNeverRadioButton
      // 
      this.saveNeverRadioButton.AutoSize = true;
      this.saveNeverRadioButton.Checked = true;
      this.saveNeverRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.saveNeverRadioButton.Location = new System.Drawing.Point(18, 1);
      this.saveNeverRadioButton.Name = "saveNeverRadioButton";
      this.saveNeverRadioButton.Size = new System.Drawing.Size(53, 17);
      this.saveNeverRadioButton.TabIndex = 10;
      this.saveNeverRadioButton.TabStop = true;
      this.saveNeverRadioButton.Text = "Never";
      this.saveNeverRadioButton.UseVisualStyleBackColor = true;
      // 
      // mpLabel6
      // 
      this.mpLabel6.Location = new System.Drawing.Point(13, 125);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(199, 16);
      this.mpLabel6.TabIndex = 9;
      this.mpLabel6.Text = "Autosave modified subtitles: *";
      // 
      // mpLabel5
      // 
      this.mpLabel5.Location = new System.Drawing.Point(196, 101);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(25, 16);
      this.mpLabel5.TabIndex = 8;
      this.mpLabel5.Text = "ms";
      // 
      // delayTextBox
      // 
      this.delayTextBox.Location = new System.Drawing.Point(143, 98);
      this.delayTextBox.Name = "delayTextBox";
      this.delayTextBox.Size = new System.Drawing.Size(46, 20);
      this.delayTextBox.TabIndex = 7;
      this.delayTextBox.Text = "250";
      this.delayTextBox.Value = 250;
      // 
      // mpLabel4
      // 
      this.mpLabel4.Location = new System.Drawing.Point(13, 101);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(107, 16);
      this.mpLabel4.TabIndex = 6;
      this.mpLabel4.Text = "Delay interval:";
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.AutoSize = true;
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(16, 19);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(107, 17);
      this.showSubtitlesCheckBox.TabIndex = 0;
      this.showSubtitlesCheckBox.Text = "Autoload subtitles";
      this.showSubtitlesCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpTabPage2
      // 
      mpTabPage2.Controls.Add(this.mpGroupBox2);
      mpTabPage2.Location = new System.Drawing.Point(4, 22);
      mpTabPage2.Name = "mpTabPage2";
      mpTabPage2.Size = new System.Drawing.Size(464, 382);
      mpTabPage2.TabIndex = 10;
      mpTabPage2.Text = "Subtitle (cont)";
      mpTabPage2.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.subStyleOverrideCheckBox);
      this.mpGroupBox2.Controls.Add(this.opaqueBoxRadioButton);
      this.mpGroupBox2.Controls.Add(this.borderOutlineRadioButton);
      this.mpGroupBox2.Controls.Add(this.borderWidthUpDown);
      this.mpGroupBox2.Controls.Add(this.shadowDepthUpDown);
      this.mpGroupBox2.Controls.Add(this.mpLabel1);
      this.mpGroupBox2.Controls.Add(this.mpLabel9);
      this.mpGroupBox2.Controls.Add(this.subtitlesButton);
      this.mpGroupBox2.Controls.Add(this.subtitlesFontTextBox);
      this.mpGroupBox2.Controls.Add(this.mpLabel10);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(15, 12);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(432, 171);
      this.mpGroupBox2.TabIndex = 1;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Style";
      // 
      // subStyleOverrideCheckBox
      // 
      this.subStyleOverrideCheckBox.AutoSize = true;
      this.subStyleOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.subStyleOverrideCheckBox.Location = new System.Drawing.Point(16, 146);
      this.subStyleOverrideCheckBox.Name = "subStyleOverrideCheckBox";
      this.subStyleOverrideCheckBox.Size = new System.Drawing.Size(160, 17);
      this.subStyleOverrideCheckBox.TabIndex = 15;
      this.subStyleOverrideCheckBox.Text = "Override ASS subtitles style *";
      this.subStyleOverrideCheckBox.UseVisualStyleBackColor = true;
      // 
      // opaqueBoxRadioButton
      // 
      this.opaqueBoxRadioButton.AutoSize = true;
      this.opaqueBoxRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.opaqueBoxRadioButton.Location = new System.Drawing.Point(24, 121);
      this.opaqueBoxRadioButton.Name = "opaqueBoxRadioButton";
      this.opaqueBoxRadioButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.opaqueBoxRadioButton.Size = new System.Drawing.Size(82, 17);
      this.opaqueBoxRadioButton.TabIndex = 8;
      this.opaqueBoxRadioButton.Text = "Opaque box";
      this.opaqueBoxRadioButton.UseVisualStyleBackColor = true;
      // 
      // borderOutlineRadioButton
      // 
      this.borderOutlineRadioButton.AutoSize = true;
      this.borderOutlineRadioButton.Checked = true;
      this.borderOutlineRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.borderOutlineRadioButton.Location = new System.Drawing.Point(24, 98);
      this.borderOutlineRadioButton.Name = "borderOutlineRadioButton";
      this.borderOutlineRadioButton.Size = new System.Drawing.Size(57, 17);
      this.borderOutlineRadioButton.TabIndex = 7;
      this.borderOutlineRadioButton.TabStop = true;
      this.borderOutlineRadioButton.Text = "Outline";
      this.borderOutlineRadioButton.UseVisualStyleBackColor = true;
      // 
      // borderWidthUpDown
      // 
      this.borderWidthUpDown.Location = new System.Drawing.Point(133, 78);
      this.borderWidthUpDown.Name = "borderWidthUpDown";
      this.borderWidthUpDown.Size = new System.Drawing.Size(79, 20);
      this.borderWidthUpDown.TabIndex = 12;
      this.borderWidthUpDown.Value = new decimal(new int[]
                                                   {
                                                     2,
                                                     0,
                                                     0,
                                                     0
                                                   });
      // 
      // shadowDepthUpDown
      // 
      this.shadowDepthUpDown.Location = new System.Drawing.Point(133, 52);
      this.shadowDepthUpDown.Name = "shadowDepthUpDown";
      this.shadowDepthUpDown.Size = new System.Drawing.Size(79, 20);
      this.shadowDepthUpDown.TabIndex = 11;
      this.shadowDepthUpDown.Value = new decimal(new int[]
                                                   {
                                                     3,
                                                     0,
                                                     0,
                                                     0
                                                   });
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(13, 54);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(81, 13);
      this.mpLabel1.TabIndex = 10;
      this.mpLabel1.Text = "Shadow Depth:";
      // 
      // mpLabel9
      // 
      this.mpLabel9.AutoSize = true;
      this.mpLabel9.Location = new System.Drawing.Point(13, 80);
      this.mpLabel9.Name = "mpLabel9";
      this.mpLabel9.Size = new System.Drawing.Size(72, 13);
      this.mpLabel9.TabIndex = 9;
      this.mpLabel9.Text = "Border Width:";
      // 
      // subtitlesButton
      // 
      this.subtitlesButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.subtitlesButton.Location = new System.Drawing.Point(337, 18);
      this.subtitlesButton.Name = "subtitlesButton";
      this.subtitlesButton.Size = new System.Drawing.Size(72, 22);
      this.subtitlesButton.TabIndex = 6;
      this.subtitlesButton.Text = "Browse";
      this.subtitlesButton.UseVisualStyleBackColor = true;
      this.subtitlesButton.Click += new System.EventHandler(this.subtitlesButton_Click);
      // 
      // subtitlesFontTextBox
      // 
      this.subtitlesFontTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.subtitlesFontTextBox.BorderColor = System.Drawing.Color.Empty;
      this.subtitlesFontTextBox.Location = new System.Drawing.Point(129, 19);
      this.subtitlesFontTextBox.Name = "subtitlesFontTextBox";
      this.subtitlesFontTextBox.ReadOnly = true;
      this.subtitlesFontTextBox.Size = new System.Drawing.Size(200, 20);
      this.subtitlesFontTextBox.TabIndex = 5;
      // 
      // mpLabel10
      // 
      this.mpLabel10.Location = new System.Drawing.Point(13, 22);
      this.mpLabel10.Name = "mpLabel10";
      this.mpLabel10.Size = new System.Drawing.Size(107, 16);
      this.mpLabel10.TabIndex = 4;
      this.mpLabel10.Text = "Display font:";
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxEachFolderIsMovie);
      this.groupBox1.Controls.Add(this.checkBoxShowWatched);
      this.groupBox1.Controls.Add(this.fileNameButton);
      this.groupBox1.Controls.Add(this.folderNameTextBox);
      this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
      this.groupBox1.Controls.Add(this.folderNameLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 150);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // checkBoxEachFolderIsMovie
      // 
      this.checkBoxEachFolderIsMovie.AutoSize = true;
      this.checkBoxEachFolderIsMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEachFolderIsMovie.Location = new System.Drawing.Point(19, 112);
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
      this.checkBoxShowWatched.Location = new System.Drawing.Point(19, 89);
      this.checkBoxShowWatched.Name = "checkBoxShowWatched";
      this.checkBoxShowWatched.Size = new System.Drawing.Size(381, 17);
      this.checkBoxShowWatched.TabIndex = 6;
      this.checkBoxShowWatched.Text = "Mark every already watched file (deactivate for performance with many files)";
      this.checkBoxShowWatched.UseVisualStyleBackColor = true;
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.Location = new System.Drawing.Point(339, 28);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 4;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.UseVisualStyleBackColor = true;
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.folderNameTextBox.Location = new System.Drawing.Point(133, 28);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(200, 20);
      this.folderNameTextBox.TabIndex = 3;
      // 
      // repeatPlaylistCheckBox
      // 
      this.repeatPlaylistCheckBox.AutoSize = true;
      this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(19, 66);
      this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
      this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(152, 17);
      this.repeatPlaylistCheckBox.TabIndex = 5;
      this.repeatPlaylistCheckBox.Text = "Repeat/loop video playlists";
      this.repeatPlaylistCheckBox.UseVisualStyleBackColor = true;
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 32);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(80, 16);
      this.folderNameLabel.TabIndex = 2;
      this.folderNameLabel.Text = "Playlist folder:";
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(tabPage3);
      this.tabControl1.Controls.Add(mpTabPage2);
      this.tabControl1.Controls.Add(this.mpTabPage1);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      // 
      // mpTabPage1
      // 
      this.mpTabPage1.Controls.Add(this.mpGroupBox4);
      this.mpTabPage1.Location = new System.Drawing.Point(4, 22);
      this.mpTabPage1.Name = "mpTabPage1";
      this.mpTabPage1.Size = new System.Drawing.Size(464, 382);
      this.mpTabPage1.TabIndex = 9;
      this.mpTabPage1.Text = "Language";
      this.mpTabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox4.Controls.Add(this.mpLabel7);
      this.mpGroupBox4.Controls.Add(this.defaultAudioLanguageComboBox);
      this.mpGroupBox4.Controls.Add(this.mpLabel8);
      this.mpGroupBox4.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(14, 12);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(432, 88);
      this.mpGroupBox4.TabIndex = 10;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Default Language";
      // 
      // mpLabel7
      // 
      this.mpLabel7.Location = new System.Drawing.Point(13, 56);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(96, 16);
      this.mpLabel7.TabIndex = 6;
      this.mpLabel7.Text = "Audio:";
      // 
      // defaultAudioLanguageComboBox
      // 
      this.defaultAudioLanguageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultAudioLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultAudioLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultAudioLanguageComboBox.Location = new System.Drawing.Point(136, 51);
      this.defaultAudioLanguageComboBox.Name = "defaultAudioLanguageComboBox";
      this.defaultAudioLanguageComboBox.Size = new System.Drawing.Size(280, 21);
      this.defaultAudioLanguageComboBox.Sorted = true;
      this.defaultAudioLanguageComboBox.TabIndex = 7;
      // 
      // mpLabel8
      // 
      this.mpLabel8.Location = new System.Drawing.Point(13, 27);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(96, 16);
      this.mpLabel8.TabIndex = 6;
      this.mpLabel8.Text = "Subtitles:";
      // 
      // defaultSubtitleLanguageComboBox
      // 
      this.defaultSubtitleLanguageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultSubtitleLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultSubtitleLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultSubtitleLanguageComboBox.Location = new System.Drawing.Point(136, 24);
      this.defaultSubtitleLanguageComboBox.Name = "defaultSubtitleLanguageComboBox";
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(280, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 7;
      // 
      // Movies
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "Movies";
      this.Size = new System.Drawing.Size(472, 408);
      tabPage3.ResumeLayout(false);
      mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      panel1.ResumeLayout(false);
      panel1.PerformLayout();
      mpTabPage2.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.borderWidthUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.shadowDepthUpDown)).EndInit();
      this.tabPage1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.mpTabPage1.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
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
        fontDialog.AllowScriptChange = true;
        fontDialog.ShowColor = true;
        fontDialog.FontMustExist = true;
        fontDialog.ShowEffects = true;

        fontDialog.Font = new Font(fontName, fontSize, fontIsBold ? FontStyle.Bold : FontStyle.Regular,
                                   GraphicsUnit.Point, fontCharset);

        if (!string.IsNullOrEmpty(fontColor))
        {
          fontDialog.Color = subtitlesFontTextBox.ForeColor;
        }

        DialogResult dialogResult = fontDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          fontName = fontDialog.Font.Name;
          fontSize = (int)fontDialog.Font.Size;
          fontIsBold = fontDialog.Font.Style == FontStyle.Bold;
          fontColor = String.Format("{0:x}", fontDialog.Color.ToArgb());
          fontCharset = fontDialog.Font.GdiCharSet;

          subtitlesFontTextBox.Text = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

          //
          // Try to parse the specified color into a valid color
          //
          if (!string.IsNullOrEmpty(fontColor))
          {
            try
            {
              int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
              subtitlesFontTextBox.BackColor = Color.Black;
              subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
            }
            catch {}
          }
        }
      }
    }

    private void subEnginesCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      string selection = (string)subEnginesCombo.SelectedItem;
      advancedButton.Enabled = !selection.Equals("Disabled");
    }

    private void advancedButton_Click(object sender, EventArgs e)
    {
      string selection = (string)subEnginesCombo.SelectedItem;
      if (selection.Equals("MPC-HC"))
      {
        MpcHcSubsForm dlg = new MpcHcSubsForm();
        DialogResult dialogResult = dlg.ShowDialog();
      }
      else if (selection.Equals("DirectVobSub"))
      {
        DirectShowLib.IBaseFilter vobSub = null;
        try
        {
          vobSub = (DirectShowLib.IBaseFilter)ClassId.CoCreateInstance(ClassId.DirectVobSubAutoload);
          DirectShowPropertyPage page = new DirectShowPropertyPage(vobSub);
          page.Show(this);
        }
        catch (Exception)
        {
          MessageBox.Show("DirectVobSub is not installed!");
        }
        finally
        {
          if (vobSub != null)
            Marshal.ReleaseComObject(vobSub);
        }
      }
    }
  }
}