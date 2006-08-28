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
using MediaPortal.Util;

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
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
      private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage3;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPComboBox defaultSubtitleLanguageComboBox;

    //string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };
    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "zoom149", "letterbox", "panscan" };

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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
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
        PopulateLanguages(defaultSubtitleLanguageComboBox, defaultLanguage);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
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
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabPage3 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.defaultZoomModeComboBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Original Source Format",
            "Stretch",
            "Zoom",
            "Zoom 14:9",
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
      this.fileNameButton.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.folderNameTextBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.folderNameTextBox.BorderColor = System.Drawing.Color.Empty;
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
      this.mpGroupBox1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.defaultSubtitleLanguageComboBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.dropShadowTextBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(92, 17);
      this.showSubtitlesCheckBox.TabIndex = 0;
      this.showSubtitlesCheckBox.Text = "Show subtitles";
      this.showSubtitlesCheckBox.UseVisualStyleBackColor = true;
      // 
      // subtitlesButton
      // 
      this.subtitlesButton.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.subtitlesFontTextBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.tabControl1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                  | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
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
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
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
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
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
  }
}

