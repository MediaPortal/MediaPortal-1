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
    private readonly string m_strDefaultSubtitleLanguageISO = "English";
    private readonly string m_strDefaultAudioLanguageISO = "English";
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
    private MPCheckBox useBDInternalMenu;
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
      string curCultureName = CultureInfo.CurrentCulture.EnglishName;      

      m_strDefaultSubtitleLanguageISO = curCultureName;
      m_strDefaultAudioLanguageISO = curCultureName;

      Util.Utils.PopulateLanguagesToComboBox(defaultSubtitleLanguageComboBox, curCultureName);
      Util.Utils.PopulateLanguagesToComboBox(defaultAudioLanguageComboBox, curCultureName);
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("bdplayer", "subtitlelanguage", m_strDefaultSubtitleLanguageISO);
        }
        catch (Exception ex)
        {
          CultureInfo ci = new CultureInfo(m_strDefaultSubtitleLanguageISO);
          Log.Error("LoadSettings - failed to load default subtitle language, using {0} - {1} ", ci.EnglishName, ex);
          defaultSubtitleLanguageComboBox.SelectedItem = ci.EnglishName;
        }
        //Use Internel Menu
        useBDInternalMenu.Checked = xmlreader.GetValueAsBool("bdplayer", "useInternalBDMenu", false);

        try
        {
          defaultAudioLanguageComboBox.SelectedItem=xmlreader.GetValueAsString("bdplayer", "audiolanguage", m_strDefaultAudioLanguageISO);         
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
        xmlwriter.SetValue("bdplayer", "subtitlelanguage", defaultSubtitleLanguageComboBox.Text);
        xmlwriter.SetValue("bdplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
        //Use Internel Menu
        xmlwriter.SetValueAsBool("bdplayer", "useInternalBDMenu", useBDInternalMenu.Checked);
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
      this.useBDInternalMenu = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpTabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultAudioLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultSubtitleLanguageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPlayAll = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabControl1.SuspendLayout();
      this.mpTabPage1.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
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
      this.mpGroupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
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
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.useBDInternalMenu);
      this.groupBox1.Controls.Add(this.labelPlayAll);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 186);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
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
      // BD
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "BD";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.ResumeLayout(false);
      this.mpTabPage1.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabPage1.ResumeLayout(false);
      this.ResumeLayout(false);
      //
      // useBDInternalMenu
      // 
      this.useBDInternalMenu.AutoSize = true;
      this.useBDInternalMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useBDInternalMenu.Location = new System.Drawing.Point(15, 26);
      this.useBDInternalMenu.Name = "useBDInternalMenu";
      this.useBDInternalMenu.Size = new System.Drawing.Size(151, 17);
      this.useBDInternalMenu.TabIndex = 6;
      this.useBDInternalMenu.Text = "Use internal Blu-Ray player";
      this.useBDInternalMenu.UseVisualStyleBackColor = true;
    }

    #endregion    
  }
}
