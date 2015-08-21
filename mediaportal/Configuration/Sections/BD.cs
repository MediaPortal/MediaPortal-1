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
using DirectShowLib;
using FFDShow;
using FFDShow.Interfaces;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class BD : SectionSettings
  {
    private FolderBrowserDialog folderBrowserDialog;
    private FontDialog fontDialog;
    private string m_strDefaultRegionLanguage = "English";
    private MPTabPage mpTabPage1;
    private MPGroupBox mpGroupBox4;
    private MPLabel mpLabel7;
    private MPComboBox defaultAudioLanguageComboBox;
    private MPLabel mpLabel8;
    private MPComboBox defaultSubtitleLanguageComboBox;
    private MPTabControl tabControl1;
    private MPTabPage tabPage1;
    private MPGroupBox groupBox1;
    private MPLabel labelPlayAll;
    private MPCheckBox useInternalBDPlayer;
    private PictureBox pictureBoxRegions;
    private NumericUpDown ParentalControlUpDown;
    private Label label2;
    private Label label1;
    private ComboBox RegionCodeComboBox;
    private CheckBox SubsEnabled;
    private MPLabel mpLabel1;
    private MPComboBox preferredAudioTypeComboBox;
    private List<LanguageInfo> ISOLanguagePairs = new List<LanguageInfo>();

    //private int 

    public BD()
      : this("Blu-ray") {}

    public BD(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Populate combo boxes with languages
      string curCultureTwoLetter = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      m_strDefaultRegionLanguage = CultureInfo.CurrentCulture.IsNeutralCulture
                                     ? CultureInfo.CurrentCulture.EnglishName
                                     : CultureInfo.CurrentCulture.Parent.EnglishName;

      Util.Utils.PopulateLanguagesToComboBox(defaultSubtitleLanguageComboBox, curCultureTwoLetter);
      Util.Utils.PopulateLanguagesToComboBox(defaultAudioLanguageComboBox, curCultureTwoLetter);
      string[] regions = { "A", "B", "C" };
      RegionCodeComboBox.Items.AddRange(regions);
      string[] audioType = { "AC3", "AC3+", "DTS", "DTS-HD", "DTS-HD Master", "LPCM", "TrueHD" };
      preferredAudioTypeComboBox.Items.AddRange(audioType);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        defaultAudioLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("bdplayer", "audiolanguage",
                                                                               m_strDefaultRegionLanguage);
        defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("bdplayer", "subtitlelanguage",
                                                                                  m_strDefaultRegionLanguage);

        RegionCodeComboBox.SelectedItem = xmlreader.GetValueAsString("bdplayer", "regioncode", "B");
        preferredAudioTypeComboBox.SelectedItem = xmlreader.GetValueAsString("bdplayer", "audiotype", "AC3");
        ParentalControlUpDown.Value = xmlreader.GetValueAsInt("bdplayer", "parentalcontrol", 99);
        SubsEnabled.Checked = xmlreader.GetValueAsBool("bdplayer", "subtitlesenabled", true);
        useInternalBDPlayer.Checked = xmlreader.GetValueAsBool("bdplayer", "useInternalBDPlayer", true);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        //Use Internel Menu
        xmlwriter.SetValueAsBool("bdplayer", "useInternalBDPlayer", useInternalBDPlayer.Checked);
        xmlwriter.SetValue("bdplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
        xmlwriter.SetValue("bdplayer", "subtitlelanguage", defaultSubtitleLanguageComboBox.Text);
        xmlwriter.SetValue("bdplayer", "audiotype", preferredAudioTypeComboBox.Text);
        xmlwriter.SetValue("bdplayer", "regioncode", RegionCodeComboBox.SelectedItem);
        xmlwriter.SetValue("bdplayer", "parentalcontrol", ParentalControlUpDown.Value.ToString());
        xmlwriter.SetValueAsBool("bdplayer", "subtitlesenabled", SubsEnabled.Checked);
      }
    }
    
    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.fontDialog = new System.Windows.Forms.FontDialog();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ParentalControlUpDown = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.RegionCodeComboBox = new System.Windows.Forms.ComboBox();
      this.pictureBoxRegions = new System.Windows.Forms.PictureBox();
      this.labelPlayAll = new MediaPortal.UserInterface.Controls.MPLabel();
      this.useInternalBDPlayer = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpTabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.preferredAudioTypeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SubsEnabled = new System.Windows.Forms.CheckBox();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultAudioLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultSubtitleLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.ParentalControlUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRegions)).BeginInit();
      this.mpTabPage1.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // fontDialog
      // 
      this.fontDialog.Color = System.Drawing.SystemColors.ControlText;
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.mpTabPage1);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Controls.Add(this.useInternalBDPlayer);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.ParentalControlUpDown);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.RegionCodeComboBox);
      this.groupBox1.Controls.Add(this.pictureBoxRegions);
      this.groupBox1.Controls.Add(this.labelPlayAll);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 298);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // ParentalControlUpDown
      // 
      this.ParentalControlUpDown.Location = new System.Drawing.Point(249, 247);
      this.ParentalControlUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
      this.ParentalControlUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.ParentalControlUpDown.Name = "ParentalControlUpDown";
      this.ParentalControlUpDown.Size = new System.Drawing.Size(40, 20);
      this.ParentalControlUpDown.TabIndex = 10;
      this.ParentalControlUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.ParentalControlUpDown.Value = new decimal(new int[] {
            99,
            0,
            0,
            0});
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(246, 227);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(125, 13);
      this.label2.TabIndex = 9;
      this.label2.Text = "Parental control age limit:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(25, 227);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(101, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Blu-ray region code:";
      // 
      // RegionCodeComboBox
      // 
      this.RegionCodeComboBox.FormattingEnabled = true;
      this.RegionCodeComboBox.Location = new System.Drawing.Point(28, 246);
      this.RegionCodeComboBox.Name = "RegionCodeComboBox";
      this.RegionCodeComboBox.Size = new System.Drawing.Size(36, 21);
      this.RegionCodeComboBox.TabIndex = 7;
      // 
      // pictureBoxRegions
      // 
      this.pictureBoxRegions.Image = global::MediaPortal.Configuration.Properties.Resources.blu_ray_regions;
      this.pictureBoxRegions.Location = new System.Drawing.Point(28, 26);
      this.pictureBoxRegions.Name = "pictureBoxRegions";
      this.pictureBoxRegions.Size = new System.Drawing.Size(376, 185);
      this.pictureBoxRegions.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxRegions.TabIndex = 6;
      this.pictureBoxRegions.TabStop = false;
      // 
      // labelPlayAll
      // 
      this.labelPlayAll.Location = new System.Drawing.Point(0, 0);
      this.labelPlayAll.Name = "labelPlayAll";
      this.labelPlayAll.Size = new System.Drawing.Size(100, 23);
      this.labelPlayAll.TabIndex = 0;
      // 
      // useInternalBDPlayer
      // 
      this.useInternalBDPlayer.AutoSize = true;
      this.useInternalBDPlayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useInternalBDPlayer.Location = new System.Drawing.Point(19, 332);
      this.useInternalBDPlayer.Name = "useInternalBDPlayer";
      this.useInternalBDPlayer.Size = new System.Drawing.Size(180, 17);
      this.useInternalBDPlayer.TabIndex = 6;
      this.useInternalBDPlayer.Text = "Use internal Blu-ray menu player";
      this.useInternalBDPlayer.UseVisualStyleBackColor = true;
      // 
      // mpTabPage1
      // 
      this.mpTabPage1.Controls.Add(this.mpGroupBox4);
      this.mpTabPage1.Location = new System.Drawing.Point(4, 22);
      this.mpTabPage1.Name = "mpTabPage1";
      this.mpTabPage1.Size = new System.Drawing.Size(464, 382);
      this.mpTabPage1.TabIndex = 9;
      this.mpTabPage1.Text = "Audio & subtitles";
      this.mpTabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox4.Controls.Add(this.preferredAudioTypeComboBox);
      this.mpGroupBox4.Controls.Add(this.mpLabel1);
      this.mpGroupBox4.Controls.Add(this.SubsEnabled);
      this.mpGroupBox4.Controls.Add(this.mpLabel7);
      this.mpGroupBox4.Controls.Add(this.defaultAudioLanguageComboBox);
      this.mpGroupBox4.Controls.Add(this.mpLabel8);
      this.mpGroupBox4.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(14, 12);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(432, 171);
      this.mpGroupBox4.TabIndex = 10;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Default Language";
      // 
      // preferredAudioTypeComboBox
      // 
      this.preferredAudioTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.preferredAudioTypeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.preferredAudioTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.preferredAudioTypeComboBox.Location = new System.Drawing.Point(136, 78);
      this.preferredAudioTypeComboBox.Name = "preferredAudioTypeComboBox";
      this.preferredAudioTypeComboBox.Size = new System.Drawing.Size(280, 21);
      this.preferredAudioTypeComboBox.Sorted = true;
      this.preferredAudioTypeComboBox.TabIndex = 10;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(13, 81);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(96, 16);
      this.mpLabel1.TabIndex = 9;
      this.mpLabel1.Text = "Audio type:";
      // 
      // SubsEnabled
      // 
      this.SubsEnabled.AutoSize = true;
      this.SubsEnabled.Location = new System.Drawing.Point(16, 148);
      this.SubsEnabled.Name = "SubsEnabled";
      this.SubsEnabled.Size = new System.Drawing.Size(100, 17);
      this.SubsEnabled.TabIndex = 8;
      this.SubsEnabled.Text = "Enable subtitles";
      this.SubsEnabled.UseVisualStyleBackColor = true;
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
      this.defaultAudioLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
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
      this.defaultSubtitleLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultSubtitleLanguageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultSubtitleLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultSubtitleLanguageComboBox.Location = new System.Drawing.Point(136, 24);
      this.defaultSubtitleLanguageComboBox.Name = "defaultSubtitleLanguageComboBox";
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(280, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 7;
      // 
      // BD
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "BD";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.ParentalControlUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRegions)).EndInit();
      this.mpTabPage1.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
      this.mpGroupBox4.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion    
  }
}