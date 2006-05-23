#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class Movies : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPButton fileNameButton;
    private MediaPortal.UserInterface.Controls.MPTextBox folderNameTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel folderNameLabel;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPCheckBox repeatPlaylistCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox showSubtitlesCheckBox;
    private MediaPortal.UserInterface.Controls.MPButton subtitlesButton;
    private MediaPortal.UserInterface.Controls.MPTextBox subtitlesFontTextBox;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private System.Windows.Forms.FontDialog fontDialog;
    private System.ComponentModel.IContainer components = null;

    string fontName;
    string fontColor;
    bool fontIsBold;
    private MediaPortal.UserInterface.Controls.MPTextBox dropShadowTextBox;
    int fontSize;
    private MediaPortal.UserInterface.Controls.MPComboBox defaultZoomModeComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private System.Windows.Forms.ListView lvDatabase;
    private System.Windows.Forms.ColumnHeader chDatabaseDB;
    private System.Windows.Forms.ColumnHeader chDatabaseLanguage;
    private System.Windows.Forms.ColumnHeader chDatabaseLimit;
    private MediaPortal.UserInterface.Controls.MPButton bDatabaseUp;
      private MediaPortal.UserInterface.Controls.MPButton bDatabaseDown;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
      private MediaPortal.UserInterface.Controls.MPComboBox defaultSubtitleLanguageComboBox;
      private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
      private MediaPortal.UserInterface.Controls.MPButton mpButton1;
      private MediaPortal.UserInterface.Controls.MPComboBox mpComboBox1;
      internal class ComboBoxItemDatabase
      {
          public string database;
          public string language;
          public string limit;

          public ComboBoxItemDatabase(string database, string language, string limit)
          {
              this.database = database;
              this.language = language;
              this.limit = limit;
          }
          public override string ToString()
          {
              return String.Format("{0} ({1})",database, language);
          }
      }

    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };

    public Movies()
      : this("Movies")
    {
    }

    public Movies(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        folderNameTextBox.Text = xmlreader.GetValueAsString("movies", "playlists", playListFolder);

        if (string.Compare(folderNameTextBox.Text, playListFolder) == 0)
        {
          if (System.IO.Directory.Exists(playListFolder) == false)
          {
            try
            {
              System.IO.Directory.CreateDirectory(playListFolder);
            }
            catch (Exception) { }
          }
        }

        repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("movies", "repeat", true);

        showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "enabled", true);
        string defaultLanguage = xmlreader.GetValueAsString("subtitles", "language", "English");

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
          catch { }
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
        // Load settings for Database
        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
        if (iNumber > 0)
        {
          string strLimit = "";
          string strDatabase = "";
          string strLanguage = "";
          Boolean imdbFound = false;
          Boolean ofdbFound = false;
          Boolean frdbFound = false;
          Boolean filmAffinityFound = false;
          for (int i = 0; i < iNumber; i++)
          {
            strLimit = xmlreader.GetValueAsString("moviedatabase", "limit" + i.ToString(), "false");
            strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i.ToString(), "false");
            strLanguage = xmlreader.GetValueAsString("moviedatabase", "language" + i.ToString(), "false");
            if ((strLimit != "false") && (strDatabase != "false") && (strLanguage != "false"))
            {
              // create entry for the database
              this.lvDatabase.Items.Add(strDatabase);
              this.lvDatabase.Items[i].SubItems.Add(strLanguage);
              this.lvDatabase.Items[i].SubItems.Add(strLimit);
              if (strDatabase == "IMDB")
              {
                  imdbFound = true;
              }
              else if (strDatabase == "OFDB")
              {
                  ofdbFound = true;
              }
              else if (strDatabase == "FRDB")
              {
                  frdbFound = true;
              }
              else if (strDatabase == "FilmAffinity")
              {
                  filmAffinityFound = true;
              }
            }
          }
          if (!imdbFound)
          {
            this.mpComboBox1.Items.Add(new ComboBoxItemDatabase("IMDB","english", "25"));
          }
          if (!ofdbFound)
          {
              this.mpComboBox1.Items.Add(new ComboBoxItemDatabase("OFDB","german", "25"));
          }
          if (!frdbFound)
          {
              this.mpComboBox1.Items.Add(new ComboBoxItemDatabase("FRDB","french", "25"));
          }
          if (!filmAffinityFound)
          {
              this.mpComboBox1.Items.Add(new ComboBoxItemDatabase("FilmAffinity","spanish", "20"));
          }
          if (mpComboBox1.Items.Count > 0)
          {
              mpComboBox1.SelectedIndex = 0;
          }
      }
        // set the first entry "activ"
        this.lvDatabase.Items[0].Selected = true;
        PopulateLanguages(defaultSubtitleLanguageComboBox, defaultLanguage);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("movies", "repeat", repeatPlaylistCheckBox.Checked);
        xmlwriter.SetValue("movies", "playlists", folderNameTextBox.Text);

        xmlwriter.SetValueAsBool("subtitles", "enabled", showSubtitlesCheckBox.Checked);
        xmlwriter.SetValue("subtitles", "shadow", dropShadowTextBox.Text);
        xmlwriter.SetValue("subtitles", "language", defaultSubtitleLanguageComboBox.Text);

        xmlwriter.SetValue("subtitles", "fontface", fontName);
        xmlwriter.SetValue("subtitles", "color", fontColor);
        xmlwriter.SetValueAsBool("subtitles", "bold", fontIsBold);
        xmlwriter.SetValue("subtitles", "fontsize", fontSize);

        xmlwriter.SetValue("movieplayer", "defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);

        // Database
        xmlwriter.SetValue("moviedatabase", "number", this.lvDatabase.Items.Count);
        for (int i = 0; i < this.lvDatabase.Items.Count; i++)
        {
          xmlwriter.SetValue("moviedatabase", "database" + i.ToString(), this.lvDatabase.Items[i].SubItems[0].Text);
          xmlwriter.SetValue("moviedatabase", "limit" + i.ToString(), this.lvDatabase.Items[i].SubItems[2].Text);
          xmlwriter.SetValue("moviedatabase", "language" + i.ToString(), this.lvDatabase.Items[i].SubItems[1].Text);
        }
        for (int i = this.lvDatabase.Items.Count; i < 4; i++)
        {
            xmlwriter.RemoveEntry("moviedatabase", "database" + i.ToString());
            xmlwriter.RemoveEntry("moviedatabase", "limit" + i.ToString());
            xmlwriter.RemoveEntry("moviedatabase", "language" + i.ToString());
        }
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
        this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
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
        this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
        this.mpComboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.bDatabaseDown = new MediaPortal.UserInterface.Controls.MPButton();
        this.bDatabaseUp = new MediaPortal.UserInterface.Controls.MPButton();
        this.lvDatabase = new System.Windows.Forms.ListView();
        this.chDatabaseDB = new System.Windows.Forms.ColumnHeader();
        this.chDatabaseLanguage = new System.Windows.Forms.ColumnHeader();
        this.chDatabaseLimit = new System.Windows.Forms.ColumnHeader();
        this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
        this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
        this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
        this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
        this.groupBox1.SuspendLayout();
        this.mpGroupBox1.SuspendLayout();
        this.groupBox2.SuspendLayout();
        this.tabControl1.SuspendLayout();
        this.tabPage1.SuspendLayout();
        this.tabPage2.SuspendLayout();
        this.tabPage3.SuspendLayout();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
        this.groupBox1.Controls.Add(this.label1);
        this.groupBox1.Controls.Add(this.fileNameButton);
        this.groupBox1.Controls.Add(this.folderNameTextBox);
        this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
        this.groupBox1.Controls.Add(this.folderNameLabel);
        this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox1.Location = new System.Drawing.Point(16, 16);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(432, 116);
        this.groupBox1.TabIndex = 0;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Settings";
        // 
        // defaultZoomModeComboBox
        // 
        this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.defaultZoomModeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Original Source Format",
            "Stretch",
            "Zoom",
            "4:3 Letterbox",
            "4:3 Pan and scan"});
        this.defaultZoomModeComboBox.Location = new System.Drawing.Point(136, 76);
        this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
        this.defaultZoomModeComboBox.Size = new System.Drawing.Size(280, 21);
        this.defaultZoomModeComboBox.TabIndex = 5;
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(16, 80);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(120, 16);
        this.label1.TabIndex = 4;
        this.label1.Text = "Default zoom mode:";
        // 
        // fileNameButton
        // 
        this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.fileNameButton.Location = new System.Drawing.Point(344, 51);
        this.fileNameButton.Name = "fileNameButton";
        this.fileNameButton.Size = new System.Drawing.Size(72, 22);
        this.fileNameButton.TabIndex = 3;
        this.fileNameButton.Text = "Browse";
        this.fileNameButton.UseVisualStyleBackColor = true;
        this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
        // 
        // folderNameTextBox
        // 
        this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.folderNameTextBox.Location = new System.Drawing.Point(136, 52);
        this.folderNameTextBox.Name = "folderNameTextBox";
        this.folderNameTextBox.Size = new System.Drawing.Size(200, 20);
        this.folderNameTextBox.TabIndex = 2;
        // 
        // repeatPlaylistCheckBox
        // 
        this.repeatPlaylistCheckBox.AutoSize = true;
        this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(16, 24);
        this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
        this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(152, 17);
        this.repeatPlaylistCheckBox.TabIndex = 0;
        this.repeatPlaylistCheckBox.Text = "Repeat/loop video playlists";
        this.repeatPlaylistCheckBox.UseVisualStyleBackColor = true;
        // 
        // folderNameLabel
        // 
        this.folderNameLabel.Location = new System.Drawing.Point(16, 56);
        this.folderNameLabel.Name = "folderNameLabel";
        this.folderNameLabel.Size = new System.Drawing.Size(80, 16);
        this.folderNameLabel.TabIndex = 1;
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
        this.mpGroupBox1.Size = new System.Drawing.Size(432, 136);
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
        this.showSubtitlesCheckBox.Size = new System.Drawing.Size(92, 17);
        this.showSubtitlesCheckBox.TabIndex = 0;
        this.showSubtitlesCheckBox.Text = "Show subtitles";
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
        // groupBox2
        // 
        this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox2.Controls.Add(this.mpButton1);
        this.groupBox2.Controls.Add(this.mpComboBox1);
        this.groupBox2.Controls.Add(this.mpLabel1);
        this.groupBox2.Controls.Add(this.bDatabaseDown);
        this.groupBox2.Controls.Add(this.bDatabaseUp);
        this.groupBox2.Controls.Add(this.lvDatabase);
        this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox2.Location = new System.Drawing.Point(16, 16);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(432, 192);
        this.groupBox2.TabIndex = 0;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "IMDB Database search results";
        // 
        // mpButton1
        // 
        this.mpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.mpButton1.Location = new System.Drawing.Point(202, 151);
        this.mpButton1.Name = "mpButton1";
        this.mpButton1.Size = new System.Drawing.Size(72, 22);
        this.mpButton1.TabIndex = 7;
        this.mpButton1.Text = "Add";
        this.mpButton1.UseVisualStyleBackColor = true;
        this.mpButton1.Click += new System.EventHandler(this.mpButton1_Click);
        // 
        // mpComboBox1
        // 
        this.mpComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.mpComboBox1.Location = new System.Drawing.Point(86, 153);
        this.mpComboBox1.Name = "mpComboBox1";
        this.mpComboBox1.Size = new System.Drawing.Size(97, 21);
        this.mpComboBox1.TabIndex = 6;
        // 
        // mpLabel1
        // 
        this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.mpLabel1.Location = new System.Drawing.Point(13, 158);
        this.mpLabel1.Name = "mpLabel1";
        this.mpLabel1.Size = new System.Drawing.Size(67, 16);
        this.mpLabel1.TabIndex = 5;
        this.mpLabel1.Text = "Databases:";
        // 
        // bDatabaseDown
        // 
        this.bDatabaseDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.bDatabaseDown.Location = new System.Drawing.Point(94, 112);
        this.bDatabaseDown.Name = "bDatabaseDown";
        this.bDatabaseDown.Size = new System.Drawing.Size(72, 22);
        this.bDatabaseDown.TabIndex = 4;
        this.bDatabaseDown.Text = "Down";
        this.bDatabaseDown.UseVisualStyleBackColor = true;
        this.bDatabaseDown.Click += new System.EventHandler(this.bDatabaseDown_Click);
        // 
        // bDatabaseUp
        // 
        this.bDatabaseUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.bDatabaseUp.Location = new System.Drawing.Point(16, 112);
        this.bDatabaseUp.Name = "bDatabaseUp";
        this.bDatabaseUp.Size = new System.Drawing.Size(72, 22);
        this.bDatabaseUp.TabIndex = 3;
        this.bDatabaseUp.Text = "Up";
        this.bDatabaseUp.UseVisualStyleBackColor = true;
        this.bDatabaseUp.Click += new System.EventHandler(this.bDatabaseUp_Click);
        // 
        // lvDatabase
        // 
        this.lvDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.lvDatabase.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chDatabaseDB,
            this.chDatabaseLanguage,
            this.chDatabaseLimit});
        this.lvDatabase.FullRowSelect = true;
        this.lvDatabase.GridLines = true;
        this.lvDatabase.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.lvDatabase.HideSelection = false;
        this.lvDatabase.Location = new System.Drawing.Point(16, 24);
        this.lvDatabase.MultiSelect = false;
        this.lvDatabase.Name = "lvDatabase";
        this.lvDatabase.Size = new System.Drawing.Size(400, 82);
        this.lvDatabase.TabIndex = 0;
        this.lvDatabase.UseCompatibleStateImageBehavior = false;
        this.lvDatabase.View = System.Windows.Forms.View.Details;
        this.lvDatabase.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lvDatabase_KeyUp);
        // 
        // chDatabaseDB
        // 
        this.chDatabaseDB.Text = "Database";
        this.chDatabaseDB.Width = 70;
        // 
        // chDatabaseLanguage
        // 
        this.chDatabaseLanguage.Text = "Language";
        this.chDatabaseLanguage.Width = 70;
        // 
        // chDatabaseLimit
        // 
        this.chDatabaseLimit.Text = "Limit";
        this.chDatabaseLimit.Width = 55;
        // 
        // tabControl1
        // 
        this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.tabControl1.Controls.Add(this.tabPage1);
        this.tabControl1.Controls.Add(this.tabPage2);
        this.tabControl1.Controls.Add(this.tabPage3);
        this.tabControl1.Location = new System.Drawing.Point(0, 0);
        this.tabControl1.Name = "tabControl1";
        this.tabControl1.SelectedIndex = 0;
        this.tabControl1.Size = new System.Drawing.Size(472, 408);
        this.tabControl1.TabIndex = 0;
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
        // tabPage2
        // 
        this.tabPage2.Controls.Add(this.groupBox2);
        this.tabPage2.Location = new System.Drawing.Point(4, 22);
        this.tabPage2.Name = "tabPage2";
        this.tabPage2.Size = new System.Drawing.Size(464, 382);
        this.tabPage2.TabIndex = 1;
        this.tabPage2.Text = "IMDB";
        this.tabPage2.UseVisualStyleBackColor = true;
        // 
        // tabPage3
        // 
        this.tabPage3.Controls.Add(this.mpGroupBox1);
        this.tabPage3.Location = new System.Drawing.Point(4, 22);
        this.tabPage3.Name = "tabPage3";
        this.tabPage3.Size = new System.Drawing.Size(464, 382);
        this.tabPage3.TabIndex = 2;
        this.tabPage3.Text = "Subtitles";
        this.tabPage3.UseVisualStyleBackColor = true;
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
        this.groupBox2.ResumeLayout(false);
        this.tabControl1.ResumeLayout(false);
        this.tabPage1.ResumeLayout(false);
        this.tabPage2.ResumeLayout(false);
        this.tabPage3.ResumeLayout(false);
        this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void fileNameButton_Click(object sender, System.EventArgs e)
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
    private void subtitlesButton_Click(object sender, System.EventArgs e)
    {
      using (fontDialog = new FontDialog())
      {
        fontDialog.AllowScriptChange = false;
        fontDialog.ShowColor = true;
        fontDialog.FontMustExist = true;
        fontDialog.ShowEffects = true;

        fontDialog.Font = new Font(fontName, (float)fontSize, fontIsBold ? FontStyle.Bold : FontStyle.Regular);

        if (fontColor != null && fontColor.Length > 0)
          fontDialog.Color = subtitlesFontTextBox.BackColor;

        DialogResult dialogResult = fontDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          fontName = fontDialog.Font.Name;
          fontSize = (int)fontDialog.Font.Size;
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
            catch { }
          }

        }
      }
    }

    private void bDatabaseDown_Click(object sender, System.EventArgs e)
    {
      // Moves the selected entry down
      // get the entry
      ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
      // guilty entry?
      if (indexes.Count == 1)
      {
        int index = indexes[0];
        // not the last entry?
        if (index < lvDatabase.Items.Count - 1)
        {
          // save current text
          string strSub0 = lvDatabase.Items[index + 1].SubItems[0].Text;
          string strSub1 = lvDatabase.Items[index + 1].SubItems[1].Text;
          string strSub2 = lvDatabase.Items[index + 1].SubItems[2].Text;
          // copy text
          lvDatabase.Items[index + 1].SubItems[0].Text = lvDatabase.Items[index].SubItems[0].Text;
          lvDatabase.Items[index + 1].SubItems[1].Text = lvDatabase.Items[index].SubItems[1].Text;
          lvDatabase.Items[index + 1].SubItems[2].Text = lvDatabase.Items[index].SubItems[2].Text;
          // restore backuped text
          lvDatabase.Items[index].SubItems[0].Text = strSub0;
          lvDatabase.Items[index].SubItems[1].Text = strSub1;
          lvDatabase.Items[index].SubItems[2].Text = strSub2;
          // move the selection down
          lvDatabase.Items[index].Selected = false;
          lvDatabase.Items[index + 1].Selected = true;
        }
      }
    }

    private void bDatabaseUp_Click(object sender, System.EventArgs e)
    {
      // Moves the selected entry up
      // get the entry
      ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
      // guilty entry?
      if (indexes.Count == 1)
      {
        int index = indexes[0];
        // not the first entry?
        if (index > 0)
        {
          // save current text
          string strSub0 = lvDatabase.Items[index - 1].SubItems[0].Text;
          string strSub1 = lvDatabase.Items[index - 1].SubItems[1].Text;
          string strSub2 = lvDatabase.Items[index - 1].SubItems[2].Text;
          // copy text
          lvDatabase.Items[index - 1].SubItems[0].Text = lvDatabase.Items[index].SubItems[0].Text;
          lvDatabase.Items[index - 1].SubItems[1].Text = lvDatabase.Items[index].SubItems[1].Text;
          lvDatabase.Items[index - 1].SubItems[2].Text = lvDatabase.Items[index].SubItems[2].Text;
          // restore backuped text
          lvDatabase.Items[index].SubItems[0].Text = strSub0;
          lvDatabase.Items[index].SubItems[1].Text = strSub1;
          lvDatabase.Items[index].SubItems[2].Text = strSub2;
          // move the selection up
          lvDatabase.Items[index].Selected = false;
          lvDatabase.Items[index - 1].Selected = true;
        }
      }
    }


    void PopulateLanguages(ComboBox comboBox, string defaultLanguage)
    {
      comboBox.Items.Clear();

      foreach (CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
      {
        comboBox.Items.Add(cultureInformation.EnglishName);

        if (String.Compare(cultureInformation.EnglishName, defaultLanguage, true) == 0)
        {
          comboBox.Text = defaultLanguage;
        }
      }
      comboBox.SelectedItem = defaultLanguage;
    }

    private void lvDatabase_KeyUp(Object o, KeyEventArgs e)
    {
        if (e.KeyCode == System.Windows.Forms.Keys.Delete || e.KeyCode == System.Windows.Forms.Keys.Back)
        {
            if (lvDatabase.SelectedIndices.Count > 0)
            {
                string strSub0 = lvDatabase.SelectedItems[0].SubItems[0].Text;
                string strSub1 = lvDatabase.SelectedItems[0].SubItems[1].Text;
                string strSub2 = lvDatabase.SelectedItems[0].SubItems[2].Text;
                mpComboBox1.Items.Add(new ComboBoxItemDatabase(strSub0,strSub1,strSub2));
                int index = lvDatabase.SelectedItems[0].Index;
                lvDatabase.Items.Remove(lvDatabase.SelectedItems[0]);
                lvDatabase.Update();
                if (lvDatabase.Items.Count > 0)
                {
                    if (index >= lvDatabase.Items.Count)
                    {
                        index = lvDatabase.Items.Count - 1;
                    }
                    lvDatabase.SelectedIndices.Clear();
                    lvDatabase.SelectedIndices.Add(index);
                }
                SaveSettings();
            }
        }
    }

      private void mpButton1_Click(object sender, EventArgs e)
      {
          ComboBoxItemDatabase database = mpComboBox1.SelectedItem as ComboBoxItemDatabase;
          if (database != null)
          {
              ListViewItem item = this.lvDatabase.Items.Add(database.database);
              item.SubItems.Add(database.language);
              item.SubItems.Add(database.limit);
              mpComboBox1.Items.Remove(database);
              if (mpComboBox1.Items.Count > 0)
              {
                  mpComboBox1.SelectedIndex = 0;
              }
              SaveSettings();
          }
      }
  }
}

