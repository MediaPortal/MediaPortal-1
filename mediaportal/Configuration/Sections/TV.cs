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

#region Using

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

#endregion

namespace MediaPortal.Configuration.Sections
{
  public class TV : SectionSettings
  {
    #region Variables

    private string[] ShowEpisodeOptions = new string[]
                                            {
                                              "[None]", // Dont show episode info
                                              "Number", // Show seriesNum.episodeNum.episodePart
                                              "Title", // Show episodeName
                                              "Number + Title" // Show number and title
                                            };
    private bool _SingleSeat;

    #endregion

    #region Constructor

    public TV()
      : this("TV") {}

    public TV(string name)
      : base(name)
    {
      InitializeComponent();
      // Episode Options
      comboboxShowEpisodeInfo.Items.Clear();
      comboboxShowEpisodeInfo.Items.AddRange(ShowEpisodeOptions);
    }

    #endregion

    #region Public methods

    public override void LoadSettings()
    {
      //Load parameters from XML File
      string preferredAudioLanguages;
      string preferredSubLanguages;

      using (Settings xmlreader = new MPSettings())
      {
        cbTurnOnTv.Checked = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        cbAutoFullscreen.Checked = xmlreader.GetValueAsBool("mytv", "autofullscreen", false);
        byIndexCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "byindex", true);
        showChannelNumberCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "showchannelnumber", false);

        int channelNumberMaxLen = xmlreader.GetValueAsInt("mytv", "channelnumbermaxlength", 3);
        channelNumberMaxLengthNumUpDn.Value = channelNumberMaxLen;

        int DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 0);
        if (DeInterlaceMode < 0 || DeInterlaceMode > 3)
        {
          DeInterlaceMode = 3;
        }
        cbDeinterlace.SelectedIndex = DeInterlaceMode;

        mpCheckBoxPrefAC3.Checked = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        mpCheckBoxPrefAudioOverLang.Checked = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
        preferredAudioLanguages = xmlreader.GetValueAsString("tvservice", "preferredaudiolanguages", "");
        preferredSubLanguages = xmlreader.GetValueAsString("tvservice", "preferredsublanguages", "");

        mpCheckBoxEnableDVBSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        mpCheckBoxEnableTTXTSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        mpCheckBoxEnableCCSub.Checked = xmlreader.GetValueAsBool("tvservice", "ccsubtitles", false);
        mpCheckBoxAutoShowSubWhenTvStarts.Checked = xmlreader.GetValueAsBool("tvservice", "autoshowsubwhentvstarts", true);
        enableAudioDualMonoModes.Checked = xmlreader.GetValueAsBool("tvservice", "audiodualmono", false);
        cbHideAllChannels.Checked = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
        cbShowChannelStateIcons.Checked = xmlreader.GetValueAsBool("mytv", "showChannelStateIcons", true);
        cbContinuousScrollGuide.Checked = xmlreader.GetValueAsBool("mytv", "continuousScrollGuide", false);
        cbRelaxTsReader.Checked = xmlreader.GetValueAsBool("mytv", "relaxTsReader", false);

        chkRecnotifications.Checked = xmlreader.GetValueAsBool("mytv", "enableRecNotifier", false);
        txtNotifyBefore.Text = xmlreader.GetValueAsString("mytv", "notifyTVBefore", "300");
        txtNotifyAfter.Text = xmlreader.GetValueAsString("mytv", "notifyTVTimeout", "15");
        checkBoxNotifyPlaySound.Checked = xmlreader.GetValueAsBool("mytv", "notifybeep", true);
        cbConfirmTimeshiftStop.Checked = xmlreader.GetValueAsBool("mytv", "confirmTimeshiftStop", true);
        int showEpisodeinfo = xmlreader.GetValueAsInt("mytv", "showEpisodeInfo", 0);
        if (showEpisodeinfo > this.ShowEpisodeOptions.Length)
        {
          showEpisodeinfo = 0;
        }
        comboboxShowEpisodeInfo.SelectedIndex = showEpisodeinfo;
      }

      // Enable this Panel if the TvPlugin exists in the plug-in Directory
      Enabled = true;

      // Retrieve the languages and language codes for the Epg.
      Dictionary<String, String> languages = new Dictionary<String, String>();
      foreach (KeyValuePair<String, String> kv in TvLibrary.Epg.Languages.Instance.GetLanguagePairs())
      {
        if (!languages.ContainsKey(kv.Key))
        {
          languages.Add(kv.Key, kv.Value);
        }
      }
      // languages now holds one language name for each language code

      FillLists(mpListViewAvailAudioLang, mpListViewPreferredAudioLang, preferredAudioLanguages, languages);
      FillLists(mpListViewAvailSubLang, mpListViewPreferredSubLang, preferredSubLanguages, languages);
      _SingleSeat = Network.IsSingleSeat();
    }

    private void FillLists(MPListView availList, MPListView preferredList, string preferredLanguages, Dictionary<String, String> languages)
    {
      availList.Items.Clear();
      preferredList.Items.Clear();
      string[] preferredLanguageKeys = preferredLanguages.Split(';');

      // fill preferredList with the preferred languages
      foreach (string key in preferredLanguageKeys)
      {
        if (!String.IsNullOrEmpty(key) && languages.ContainsKey(key) && !preferredList.Items.ContainsKey(key))
        {
          ListViewItem item = new ListViewItem(new string[] { languages[key], key }) { Name = key };
          preferredList.Items.Add(item);
        }
      }

      // fill availList with the rest of them
      foreach (KeyValuePair<string, string> kv in languages)
      {
        if (!availList.Items.ContainsKey(kv.Key) && !preferredList.Items.ContainsKey(kv.Key))
        {
          ListViewItem item = new ListViewItem(new string[] { kv.Value, kv.Key }) { Name = kv.Key };
          availList.Items.Add(item);
        }
      }

      availList.ListViewItemSorter = new ListViewColumnSorter() { SortColumn = 0, Order = SortOrder.Ascending };
      availList.Sort();
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        if (cbDeinterlace.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("mytv", "deinterlace", cbDeinterlace.SelectedIndex.ToString());
        }

        xmlwriter.SetValueAsBool("mytv", "autoturnontv", cbTurnOnTv.Checked);
        xmlwriter.SetValueAsBool("mytv", "autofullscreen", cbAutoFullscreen.Checked);
        xmlwriter.SetValueAsBool("mytv", "byindex", byIndexCheckBox.Checked);
        xmlwriter.SetValueAsBool("mytv", "showchannelnumber", showChannelNumberCheckBox.Checked);
        xmlwriter.SetValue("mytv", "channelnumbermaxlength", channelNumberMaxLengthNumUpDn.Value);

        xmlwriter.SetValueAsBool("tvservice", "preferac3", mpCheckBoxPrefAC3.Checked);
        xmlwriter.SetValueAsBool("tvservice", "preferAudioTypeOverLang", mpCheckBoxPrefAudioOverLang.Checked);

        xmlwriter.SetValueAsBool("tvservice", "dvbbitmapsubtitles", mpCheckBoxEnableDVBSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "dvbttxtsubtitles", mpCheckBoxEnableTTXTSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "ccsubtitles", mpCheckBoxEnableCCSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "autoshowsubwhentvstarts", mpCheckBoxAutoShowSubWhenTvStarts.Checked);
        xmlwriter.SetValueAsBool("tvservice", "audiodualmono", enableAudioDualMonoModes.Checked);
        xmlwriter.SetValueAsBool("mytv", "hideAllChannelsGroup", cbHideAllChannels.Checked);
        xmlwriter.SetValueAsBool("mytv", "showChannelStateIcons", cbShowChannelStateIcons.Checked);
        xmlwriter.SetValueAsBool("mytv", "continuousScrollGuide", cbContinuousScrollGuide.Checked);
        xmlwriter.SetValueAsBool("mytv", "relaxTsReader", cbRelaxTsReader.Checked);

        xmlwriter.SetValueAsBool("mytv", "enableRecNotifier", chkRecnotifications.Checked);
        xmlwriter.SetValue("mytv", "notifyTVBefore", txtNotifyBefore.Text);
        xmlwriter.SetValue("mytv", "notifyTVTimeout", txtNotifyAfter.Text);
        xmlwriter.SetValueAsBool("mytv", "notifybeep", checkBoxNotifyPlaySound.Checked);
        xmlwriter.SetValueAsBool("mytv", "confirmTimeshiftStop", cbConfirmTimeshiftStop.Checked);
        xmlwriter.SetValue("mytv", "showEpisodeInfo", comboboxShowEpisodeInfo.SelectedIndex);

        string prefLangs = "";
        foreach (ListViewItem item in mpListViewPreferredAudioLang.Items)
        {
          prefLangs += (string)item.Name + ";";
        }
        xmlwriter.SetValue("tvservice", "preferredaudiolanguages", prefLangs);

        prefLangs = "";
        foreach (ListViewItem item in mpListViewPreferredSubLang.Items)
        {
          prefLangs += (string)item.Name + ";";
        }
        xmlwriter.SetValue("tvservice", "preferredsublanguages", prefLangs);

        //When TvServer is changed, if user changed mode (SingleSeat/MultiSeat), he needs to review the RTSP setting in DebugOptions section
        if ((xmlwriter.GetValueAsBool("tvservice", "DebugOptions", false) || SettingsForm.debug_options) &&
            (_SingleSeat != Network.IsSingleSeat()))
        {
          MessageBox.Show("Please review your RTSP settings in \"DebugOptions\" section", "Warning",
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
    }

    #endregion

    #region Designer generated code

    private MPGroupBox mpGroupBox1;
    private MPCheckBox mpCheckBoxPrefAC3;
    private MPCheckBox mpCheckBoxPrefAudioOverLang;
    private MPTabControl tabControlTVGeneral;
    private MPTabPage tabPageGeneralSettings;
    private MPTabPage tabPageAudioLanguages;
    private MPGroupBox groupBox2;
    private MPTabPage tabPageSubtitles;
    private MPButton mpButtonAddAudioLang;
    private MPButton mpButtonRemoveAudioLang;
    private MPListView mpListViewPreferredAudioLang;
    private MPListView mpListViewAvailAudioLang;
    private MPButton mpButtonDownAudioLang;
    private MPButton mpButtonUpAudioLang;
    private MPLabel mpLabel5;
    private MPLabel mpLabel1;
    private MPGroupBox mpGroupBox3;
    private MPLabel mpLabel6;
    private MPLabel mpLabel7;
    private MPButton mpButtonDownSubLang;
    private MPButton mpButtonUpSubLang;
    private MPButton mpButtonAddSubLang;
    private MPButton mpButtonRemoveSubLang;
    private MPListView mpListViewPreferredSubLang;
    private MPListView mpListViewAvailSubLang;
    private MPGroupBox mpGroupBox4;
    private MPCheckBox mpCheckBoxEnableTTXTSub;
    private MPCheckBox mpCheckBoxEnableDVBSub;
    private MPCheckBox mpCheckBoxAutoShowSubWhenTvStarts;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader4;
    private MPGroupBox mpGroupBox5;
    private MPCheckBox cbHideAllChannels;
    private MPGroupBox mpGroupBox6;
    private MPCheckBox cbShowChannelStateIcons;
    private MPCheckBox enableAudioDualMonoModes;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private ColumnHeader columnHeader8;
    private MPTabPage tabPage1;
    private MPGroupBox mpGroupBox7;
    private MPLabel mpLabel2;
    private MPTextBox txtNotifyBefore;
    private MPGroupBox mpGroupBox8;
    private MPCheckBox chkRecnotifications;
    private MPCheckBox checkBoxNotifyPlaySound;
    private MPTextBox txtNotifyAfter;
    private MPLabel labelNotifyTimeout;
    private ColumnHeader columnHeader7;
    private MPGroupBox grpTsReader;
    private MPCheckBox cbRelaxTsReader;
    private MPLabel labelShowEpisodeinfo;
    private MPComboBox comboboxShowEpisodeInfo;
    private MPCheckBox cbContinuousScrollGuide;
    private MPCheckBox mpCheckBoxEnableCCSub;
    private MPGroupBox mpGroupBoxAdditional;
    private MPComboBox cbDeinterlace;
    private MPLabel label8;
    private MPGroupBox groupBox5;
    private MPCheckBox cbAutoFullscreen;
    private MPCheckBox cbTurnOnTv;
    private MPGroupBox groupBox3;
    private MPCheckBox byIndexCheckBox;
    private MPCheckBox showChannelNumberCheckBox;
    private MPNumericUpDown channelNumberMaxLengthNumUpDn;
    private MPLabel lblChanNumMaxLen;
    private MPCheckBox cbConfirmTimeshiftStop;

    private void InitializeComponent()
    {
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.enableAudioDualMonoModes = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxPrefAudioOverLang = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlTVGeneral = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageGeneralSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBoxAdditional = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAutoFullscreen = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTurnOnTv = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.byIndexCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.showChannelNumberCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.channelNumberMaxLengthNumUpDn = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.lblChanNumMaxLen = new MediaPortal.UserInterface.Controls.MPLabel();
      this.grpTsReader = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbRelaxTsReader = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox6 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbContinuousScrollGuide = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbShowChannelStateIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelShowEpisodeinfo = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboboxShowEpisodeInfo = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbHideAllChannels = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageAudioLanguages = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDownAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonUpAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAddAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonRemoveAudioLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListViewPreferredAudioLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpListViewAvailAudioLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tabPageSubtitles = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxEnableCCSub = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxAutoShowSubWhenTvStarts = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxEnableTTXTSub = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxEnableDVBSub = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpButtonDownSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonUpSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonAddSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonRemoveSubLang = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpListViewPreferredSubLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpListViewAvailSubLang = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox8 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.chkRecnotifications = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox7 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbConfirmTimeshiftStop = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.txtNotifyAfter = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelNotifyTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxNotifyPlaySound = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtNotifyBefore = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpGroupBox1.SuspendLayout();
      this.tabControlTVGeneral.SuspendLayout();
      this.tabPageGeneralSettings.SuspendLayout();
      this.mpGroupBoxAdditional.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).BeginInit();
      this.grpTsReader.SuspendLayout();
      this.mpGroupBox6.SuspendLayout();
      this.mpGroupBox5.SuspendLayout();
      this.tabPageAudioLanguages.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tabPageSubtitles.SuspendLayout();
      this.mpGroupBox4.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.mpGroupBox8.SuspendLayout();
      this.mpGroupBox7.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.enableAudioDualMonoModes);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAudioOverLang);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 308);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 91);
      this.mpGroupBox1.TabIndex = 9;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio stream settings";
      // 
      // enableAudioDualMonoModes
      // 
      this.enableAudioDualMonoModes.AutoSize = true;
      this.enableAudioDualMonoModes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.enableAudioDualMonoModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableAudioDualMonoModes.Location = new System.Drawing.Point(8, 51);
      this.enableAudioDualMonoModes.Name = "enableAudioDualMonoModes";
      this.enableAudioDualMonoModes.Size = new System.Drawing.Size(386, 30);
      this.enableAudioDualMonoModes.TabIndex = 12;
      this.enableAudioDualMonoModes.Text = "Enable AudioDualMono mode switching\r\n(if 1 audio stream contains 2x mono channels" +
          ", you can switch between them)";
      this.enableAudioDualMonoModes.UseVisualStyleBackColor = true;
      // 
      // mpCheckBoxPrefAudioOverLang
      // 
      this.mpCheckBoxPrefAudioOverLang.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpCheckBoxPrefAudioOverLang.AutoSize = true;
      this.mpCheckBoxPrefAudioOverLang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAudioOverLang.Location = new System.Drawing.Point(238, 28);
      this.mpCheckBoxPrefAudioOverLang.Name = "mpCheckBoxPrefAudioOverLang";
      this.mpCheckBoxPrefAudioOverLang.Size = new System.Drawing.Size(172, 17);
      this.mpCheckBoxPrefAudioOverLang.TabIndex = 11;
      this.mpCheckBoxPrefAudioOverLang.Text = "Prefer audiotype over language";
      this.mpCheckBoxPrefAudioOverLang.UseVisualStyleBackColor = false;
      // 
      // mpCheckBoxPrefAC3
      // 
      this.mpCheckBoxPrefAC3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpCheckBoxPrefAC3.AutoSize = true;
      this.mpCheckBoxPrefAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxPrefAC3.Location = new System.Drawing.Point(8, 28);
      this.mpCheckBoxPrefAC3.Name = "mpCheckBoxPrefAC3";
      this.mpCheckBoxPrefAC3.Size = new System.Drawing.Size(110, 17);
      this.mpCheckBoxPrefAC3.TabIndex = 7;
      this.mpCheckBoxPrefAC3.Text = "Prefer AC-3 sound";
      this.mpCheckBoxPrefAC3.UseVisualStyleBackColor = false;
      // 
      // tabControlTVGeneral
      // 
      this.tabControlTVGeneral.Controls.Add(this.tabPageGeneralSettings);
      this.tabControlTVGeneral.Controls.Add(this.tabPageAudioLanguages);
      this.tabControlTVGeneral.Controls.Add(this.tabPageSubtitles);
      this.tabControlTVGeneral.Controls.Add(this.tabPage1);
      this.tabControlTVGeneral.Location = new System.Drawing.Point(0, 2);
      this.tabControlTVGeneral.Name = "tabControlTVGeneral";
      this.tabControlTVGeneral.SelectedIndex = 0;
      this.tabControlTVGeneral.Size = new System.Drawing.Size(472, 445);
      this.tabControlTVGeneral.TabIndex = 11;
      // 
      // tabPageGeneralSettings
      // 
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBoxAdditional);
      this.tabPageGeneralSettings.Controls.Add(this.groupBox5);
      this.tabPageGeneralSettings.Controls.Add(this.groupBox3);
      this.tabPageGeneralSettings.Controls.Add(this.grpTsReader);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox6);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox5);
      this.tabPageGeneralSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageGeneralSettings.Name = "tabPageGeneralSettings";
      this.tabPageGeneralSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageGeneralSettings.Size = new System.Drawing.Size(464, 419);
      this.tabPageGeneralSettings.TabIndex = 0;
      this.tabPageGeneralSettings.Text = "General settings";
      this.tabPageGeneralSettings.UseVisualStyleBackColor = true;
      // 
      // mpGroupBoxAdditional
      // 
      this.mpGroupBoxAdditional.Controls.Add(this.cbDeinterlace);
      this.mpGroupBoxAdditional.Controls.Add(this.label8);
      this.mpGroupBoxAdditional.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxAdditional.Location = new System.Drawing.Point(16, 322);
      this.mpGroupBoxAdditional.Name = "mpGroupBoxAdditional";
      this.mpGroupBoxAdditional.Size = new System.Drawing.Size(431, 63);
      this.mpGroupBoxAdditional.TabIndex = 19;
      this.mpGroupBoxAdditional.TabStop = false;
      this.mpGroupBoxAdditional.Text = "Additional settings";
      // 
      // cbDeinterlace
      // 
      this.cbDeinterlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDeinterlace.BorderColor = System.Drawing.Color.Empty;
      this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDeinterlace.Items.AddRange(new object[] {
            "None",
            "Bob",
            "Weave",
            "Best"});
      this.cbDeinterlace.Location = new System.Drawing.Point(166, 19);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(259, 21);
      this.cbDeinterlace.TabIndex = 0;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(6, 23);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(146, 17);
      this.label8.TabIndex = 14;
      this.label8.Text = "Fallback de-interlace mode:";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.cbAutoFullscreen);
      this.groupBox5.Controls.Add(this.cbTurnOnTv);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(258, 169);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(189, 94);
      this.groupBox5.TabIndex = 18;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "When entering the TV screen:";
      // 
      // cbAutoFullscreen
      // 
      this.cbAutoFullscreen.AutoSize = true;
      this.cbAutoFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAutoFullscreen.Location = new System.Drawing.Point(17, 40);
      this.cbAutoFullscreen.Name = "cbAutoFullscreen";
      this.cbAutoFullscreen.Size = new System.Drawing.Size(152, 17);
      this.cbAutoFullscreen.TabIndex = 1;
      this.cbAutoFullscreen.Text = "Directly show fullscreen TV";
      this.cbAutoFullscreen.UseVisualStyleBackColor = true;
      this.cbAutoFullscreen.CheckedChanged += new System.EventHandler(this.cbAutoFullscreen_CheckedChanged);
      // 
      // cbTurnOnTv
      // 
      this.cbTurnOnTv.AutoSize = true;
      this.cbTurnOnTv.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTurnOnTv.Location = new System.Drawing.Point(17, 20);
      this.cbTurnOnTv.Name = "cbTurnOnTv";
      this.cbTurnOnTv.Size = new System.Drawing.Size(78, 17);
      this.cbTurnOnTv.TabIndex = 0;
      this.cbTurnOnTv.Text = "Turn on TV";
      this.cbTurnOnTv.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.byIndexCheckBox);
      this.groupBox3.Controls.Add(this.showChannelNumberCheckBox);
      this.groupBox3.Controls.Add(this.channelNumberMaxLengthNumUpDn);
      this.groupBox3.Controls.Add(this.lblChanNumMaxLen);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(16, 169);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(233, 94);
      this.groupBox3.TabIndex = 17;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Channel numbers";
      // 
      // byIndexCheckBox
      // 
      this.byIndexCheckBox.AutoSize = true;
      this.byIndexCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.byIndexCheckBox.Location = new System.Drawing.Point(17, 20);
      this.byIndexCheckBox.Name = "byIndexCheckBox";
      this.byIndexCheckBox.Size = new System.Drawing.Size(182, 17);
      this.byIndexCheckBox.TabIndex = 0;
      this.byIndexCheckBox.Text = "Select channel by index (non-US)";
      this.byIndexCheckBox.UseVisualStyleBackColor = true;
      // 
      // showChannelNumberCheckBox
      // 
      this.showChannelNumberCheckBox.AutoSize = true;
      this.showChannelNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showChannelNumberCheckBox.Location = new System.Drawing.Point(17, 40);
      this.showChannelNumberCheckBox.Name = "showChannelNumberCheckBox";
      this.showChannelNumberCheckBox.Size = new System.Drawing.Size(135, 17);
      this.showChannelNumberCheckBox.TabIndex = 1;
      this.showChannelNumberCheckBox.Text = "Show channel numbers";
      this.showChannelNumberCheckBox.UseVisualStyleBackColor = true;
      // 
      // channelNumberMaxLengthNumUpDn
      // 
      this.channelNumberMaxLengthNumUpDn.AutoSize = true;
      this.channelNumberMaxLengthNumUpDn.Location = new System.Drawing.Point(178, 60);
      this.channelNumberMaxLengthNumUpDn.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.channelNumberMaxLengthNumUpDn.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.channelNumberMaxLengthNumUpDn.Name = "channelNumberMaxLengthNumUpDn";
      this.channelNumberMaxLengthNumUpDn.Size = new System.Drawing.Size(42, 20);
      this.channelNumberMaxLengthNumUpDn.TabIndex = 2;
      this.channelNumberMaxLengthNumUpDn.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
      // 
      // lblChanNumMaxLen
      // 
      this.lblChanNumMaxLen.AutoSize = true;
      this.lblChanNumMaxLen.Location = new System.Drawing.Point(31, 62);
      this.lblChanNumMaxLen.Name = "lblChanNumMaxLen";
      this.lblChanNumMaxLen.Size = new System.Drawing.Size(141, 13);
      this.lblChanNumMaxLen.TabIndex = 2;
      this.lblChanNumMaxLen.Text = "Channel number max. length";
      // 
      // grpTsReader
      // 
      this.grpTsReader.Controls.Add(this.cbRelaxTsReader);
      this.grpTsReader.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpTsReader.Location = new System.Drawing.Point(16, 269);
      this.grpTsReader.Name = "grpTsReader";
      this.grpTsReader.Size = new System.Drawing.Size(431, 47);
      this.grpTsReader.TabIndex = 12;
      this.grpTsReader.TabStop = false;
      this.grpTsReader.Text = "TsReader options";
      // 
      // cbRelaxTsReader
      // 
      this.cbRelaxTsReader.AutoSize = true;
      this.cbRelaxTsReader.Checked = true;
      this.cbRelaxTsReader.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbRelaxTsReader.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbRelaxTsReader.Location = new System.Drawing.Point(22, 19);
      this.cbRelaxTsReader.Name = "cbRelaxTsReader";
      this.cbRelaxTsReader.Size = new System.Drawing.Size(347, 17);
      this.cbRelaxTsReader.TabIndex = 0;
      this.cbRelaxTsReader.Text = "Don\'t drop discontinued packets in TsReader (can reduce stuttering)";
      this.cbRelaxTsReader.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox6
      // 
      this.mpGroupBox6.Controls.Add(this.cbContinuousScrollGuide);
      this.mpGroupBox6.Controls.Add(this.cbShowChannelStateIcons);
      this.mpGroupBox6.Controls.Add(this.labelShowEpisodeinfo);
      this.mpGroupBox6.Controls.Add(this.comboboxShowEpisodeInfo);
      this.mpGroupBox6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox6.Location = new System.Drawing.Point(16, 67);
      this.mpGroupBox6.Name = "mpGroupBox6";
      this.mpGroupBox6.Size = new System.Drawing.Size(431, 96);
      this.mpGroupBox6.TabIndex = 12;
      this.mpGroupBox6.TabStop = false;
      this.mpGroupBox6.Text = "Guide";
      // 
      // cbContinuousScrollGuide
      // 
      this.cbContinuousScrollGuide.AutoSize = true;
      this.cbContinuousScrollGuide.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbContinuousScrollGuide.Location = new System.Drawing.Point(22, 40);
      this.cbContinuousScrollGuide.Name = "cbContinuousScrollGuide";
      this.cbContinuousScrollGuide.Size = new System.Drawing.Size(210, 17);
      this.cbContinuousScrollGuide.TabIndex = 1;
      this.cbContinuousScrollGuide.Text = "Loop guide seamlessly (top and bottom)";
      this.cbContinuousScrollGuide.UseVisualStyleBackColor = true;
      // 
      // cbShowChannelStateIcons
      // 
      this.cbShowChannelStateIcons.AutoSize = true;
      this.cbShowChannelStateIcons.Checked = true;
      this.cbShowChannelStateIcons.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbShowChannelStateIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbShowChannelStateIcons.Location = new System.Drawing.Point(22, 19);
      this.cbShowChannelStateIcons.Name = "cbShowChannelStateIcons";
      this.cbShowChannelStateIcons.Size = new System.Drawing.Size(308, 17);
      this.cbShowChannelStateIcons.TabIndex = 0;
      this.cbShowChannelStateIcons.Text = "Show channel state icons in Mini Guide (on supported skins)";
      this.cbShowChannelStateIcons.UseVisualStyleBackColor = true;
      // 
      // labelShowEpisodeinfo
      // 
      this.labelShowEpisodeinfo.AutoSize = true;
      this.labelShowEpisodeinfo.Location = new System.Drawing.Point(19, 68);
      this.labelShowEpisodeinfo.Name = "labelShowEpisodeinfo";
      this.labelShowEpisodeinfo.Size = new System.Drawing.Size(97, 13);
      this.labelShowEpisodeinfo.TabIndex = 1;
      this.labelShowEpisodeinfo.Text = "Show episode info:";
      // 
      // comboboxShowEpisodeInfo
      // 
      this.comboboxShowEpisodeInfo.BorderColor = System.Drawing.Color.Empty;
      this.comboboxShowEpisodeInfo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboboxShowEpisodeInfo.FormattingEnabled = true;
      this.comboboxShowEpisodeInfo.Location = new System.Drawing.Point(126, 64);
      this.comboboxShowEpisodeInfo.Name = "comboboxShowEpisodeInfo";
      this.comboboxShowEpisodeInfo.Size = new System.Drawing.Size(229, 21);
      this.comboboxShowEpisodeInfo.TabIndex = 2;
      // 
      // mpGroupBox5
      // 
      this.mpGroupBox5.Controls.Add(this.cbHideAllChannels);
      this.mpGroupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox5.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox5.Name = "mpGroupBox5";
      this.mpGroupBox5.Size = new System.Drawing.Size(431, 45);
      this.mpGroupBox5.TabIndex = 11;
      this.mpGroupBox5.TabStop = false;
      this.mpGroupBox5.Text = "Group options";
      // 
      // cbHideAllChannels
      // 
      this.cbHideAllChannels.AutoSize = true;
      this.cbHideAllChannels.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbHideAllChannels.Location = new System.Drawing.Point(22, 19);
      this.cbHideAllChannels.Name = "cbHideAllChannels";
      this.cbHideAllChannels.Size = new System.Drawing.Size(149, 17);
      this.cbHideAllChannels.TabIndex = 0;
      this.cbHideAllChannels.Text = "Hide \"All Channels\" Group";
      this.cbHideAllChannels.UseVisualStyleBackColor = true;
      // 
      // tabPageAudioLanguages
      // 
      this.tabPageAudioLanguages.Controls.Add(this.groupBox2);
      this.tabPageAudioLanguages.Controls.Add(this.mpGroupBox1);
      this.tabPageAudioLanguages.Location = new System.Drawing.Point(4, 22);
      this.tabPageAudioLanguages.Name = "tabPageAudioLanguages";
      this.tabPageAudioLanguages.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageAudioLanguages.Size = new System.Drawing.Size(464, 419);
      this.tabPageAudioLanguages.TabIndex = 3;
      this.tabPageAudioLanguages.Text = "Audio settings";
      this.tabPageAudioLanguages.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.mpLabel5);
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.mpButtonDownAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonUpAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonAddAudioLang);
      this.groupBox2.Controls.Add(this.mpButtonRemoveAudioLang);
      this.groupBox2.Controls.Add(this.mpListViewPreferredAudioLang);
      this.groupBox2.Controls.Add(this.mpListViewAvailAudioLang);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(16, 16);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 284);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Preferred audio languages";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(236, 21);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(102, 13);
      this.mpLabel5.TabIndex = 7;
      this.mpLabel5.Text = "Preferred languages";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(6, 21);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(102, 13);
      this.mpLabel1.TabIndex = 6;
      this.mpLabel1.Text = "Available languages";
      // 
      // mpButtonDownAudioLang
      // 
      this.mpButtonDownAudioLang.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonDownAudioLang.Location = new System.Drawing.Point(289, 255);
      this.mpButtonDownAudioLang.Name = "mpButtonDownAudioLang";
      this.mpButtonDownAudioLang.Size = new System.Drawing.Size(46, 20);
      this.mpButtonDownAudioLang.TabIndex = 5;
      this.mpButtonDownAudioLang.Text = "Down";
      this.mpButtonDownAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonDownAudioLang.Click += new System.EventHandler(this.mpButtonDownAudioLang_Click);
      // 
      // mpButtonUpAudioLang
      // 
      this.mpButtonUpAudioLang.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonUpAudioLang.Location = new System.Drawing.Point(237, 255);
      this.mpButtonUpAudioLang.Name = "mpButtonUpAudioLang";
      this.mpButtonUpAudioLang.Size = new System.Drawing.Size(46, 20);
      this.mpButtonUpAudioLang.TabIndex = 4;
      this.mpButtonUpAudioLang.Text = "Up";
      this.mpButtonUpAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonUpAudioLang.Click += new System.EventHandler(this.mpButtonUpAudioLang_Click);
      // 
      // mpButtonAddAudioLang
      // 
      this.mpButtonAddAudioLang.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonAddAudioLang.Location = new System.Drawing.Point(202, 40);
      this.mpButtonAddAudioLang.Name = "mpButtonAddAudioLang";
      this.mpButtonAddAudioLang.Size = new System.Drawing.Size(28, 20);
      this.mpButtonAddAudioLang.TabIndex = 3;
      this.mpButtonAddAudioLang.Text = ">";
      this.mpButtonAddAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonAddAudioLang.Click += new System.EventHandler(this.mpButtonAddAudioLang_Click);
      // 
      // mpButtonRemoveAudioLang
      // 
      this.mpButtonRemoveAudioLang.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpButtonRemoveAudioLang.Location = new System.Drawing.Point(202, 66);
      this.mpButtonRemoveAudioLang.Name = "mpButtonRemoveAudioLang";
      this.mpButtonRemoveAudioLang.Size = new System.Drawing.Size(28, 20);
      this.mpButtonRemoveAudioLang.TabIndex = 2;
      this.mpButtonRemoveAudioLang.Text = "<";
      this.mpButtonRemoveAudioLang.UseVisualStyleBackColor = true;
      this.mpButtonRemoveAudioLang.Click += new System.EventHandler(this.mpButtonRemoveAudioLang_Click);
      // 
      // mpListViewPreferredAudioLang
      // 
      this.mpListViewPreferredAudioLang.AllowDrop = true;
      this.mpListViewPreferredAudioLang.AllowRowReorder = true;
      this.mpListViewPreferredAudioLang.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewPreferredAudioLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader6});
      this.mpListViewPreferredAudioLang.FullRowSelect = true;
      this.mpListViewPreferredAudioLang.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.mpListViewPreferredAudioLang.HideSelection = false;
      this.mpListViewPreferredAudioLang.Location = new System.Drawing.Point(239, 40);
      this.mpListViewPreferredAudioLang.Name = "mpListViewPreferredAudioLang";
      this.mpListViewPreferredAudioLang.Size = new System.Drawing.Size(183, 209);
      this.mpListViewPreferredAudioLang.TabIndex = 1;
      this.mpListViewPreferredAudioLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewPreferredAudioLang.View = System.Windows.Forms.View.Details;
      this.mpListViewPreferredAudioLang.SelectedIndexChanged += new System.EventHandler(this.mpListView2_SelectedIndexChanged);
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Language";
      this.columnHeader2.Width = 125;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "ID";
      this.columnHeader6.Width = 35;
      // 
      // mpListViewAvailAudioLang
      // 
      this.mpListViewAvailAudioLang.AllowDrop = true;
      this.mpListViewAvailAudioLang.AllowRowReorder = true;
      this.mpListViewAvailAudioLang.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpListViewAvailAudioLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader5});
      this.mpListViewAvailAudioLang.FullRowSelect = true;
      this.mpListViewAvailAudioLang.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.mpListViewAvailAudioLang.HideSelection = false;
      this.mpListViewAvailAudioLang.Location = new System.Drawing.Point(6, 40);
      this.mpListViewAvailAudioLang.Name = "mpListViewAvailAudioLang";
      this.mpListViewAvailAudioLang.Size = new System.Drawing.Size(183, 209);
      this.mpListViewAvailAudioLang.TabIndex = 0;
      this.mpListViewAvailAudioLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewAvailAudioLang.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Language";
      this.columnHeader1.Width = 125;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "ID";
      this.columnHeader5.Width = 35;
      // 
      // tabPageSubtitles
      // 
      this.tabPageSubtitles.Controls.Add(this.mpGroupBox4);
      this.tabPageSubtitles.Controls.Add(this.mpGroupBox3);
      this.tabPageSubtitles.Location = new System.Drawing.Point(4, 22);
      this.tabPageSubtitles.Name = "tabPageSubtitles";
      this.tabPageSubtitles.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSubtitles.Size = new System.Drawing.Size(464, 419);
      this.tabPageSubtitles.TabIndex = 2;
      this.tabPageSubtitles.Text = "Subtitle settings";
      this.tabPageSubtitles.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableCCSub);
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxAutoShowSubWhenTvStarts);
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableTTXTSub);
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableDVBSub);
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(16, 308);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(432, 86);
      this.mpGroupBox4.TabIndex = 10;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Subtitle settings";
      // 
      // mpCheckBoxEnableCCSub
      // 
      this.mpCheckBoxEnableCCSub.AutoSize = true;
      this.mpCheckBoxEnableCCSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxEnableCCSub.Location = new System.Drawing.Point(244, 51);
      this.mpCheckBoxEnableCCSub.Name = "mpCheckBoxEnableCCSub";
      this.mpCheckBoxEnableCCSub.Size = new System.Drawing.Size(115, 17);
      this.mpCheckBoxEnableCCSub.TabIndex = 12;
      this.mpCheckBoxEnableCCSub.Text = "Enable CC subtitles";
      this.mpCheckBoxEnableCCSub.UseVisualStyleBackColor = false;
      // 
      // mpCheckBoxAutoShowSubWhenTvStarts
      // 
      this.mpCheckBoxAutoShowSubWhenTvStarts.AutoSize = true;
      this.mpCheckBoxAutoShowSubWhenTvStarts.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxAutoShowSubWhenTvStarts.Location = new System.Drawing.Point(8, 51);
      this.mpCheckBoxAutoShowSubWhenTvStarts.Name = "mpCheckBoxAutoShowSubWhenTvStarts";
      this.mpCheckBoxAutoShowSubWhenTvStarts.Size = new System.Drawing.Size(186, 17);
      this.mpCheckBoxAutoShowSubWhenTvStarts.TabIndex = 12;
      this.mpCheckBoxAutoShowSubWhenTvStarts.Text = "Autoshow subtitles when TV starts";
      this.mpCheckBoxAutoShowSubWhenTvStarts.UseVisualStyleBackColor = false;
      // 
      // mpCheckBoxEnableTTXTSub
      // 
      this.mpCheckBoxEnableTTXTSub.AutoSize = true;
      this.mpCheckBoxEnableTTXTSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxEnableTTXTSub.Location = new System.Drawing.Point(244, 28);
      this.mpCheckBoxEnableTTXTSub.Name = "mpCheckBoxEnableTTXTSub";
      this.mpCheckBoxEnableTTXTSub.Size = new System.Drawing.Size(135, 17);
      this.mpCheckBoxEnableTTXTSub.TabIndex = 11;
      this.mpCheckBoxEnableTTXTSub.Text = "Enable teletext subtitles";
      this.mpCheckBoxEnableTTXTSub.UseVisualStyleBackColor = false;
      this.mpCheckBoxEnableTTXTSub.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // mpCheckBoxEnableDVBSub
      // 
      this.mpCheckBoxEnableDVBSub.AutoSize = true;
      this.mpCheckBoxEnableDVBSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxEnableDVBSub.Location = new System.Drawing.Point(8, 28);
      this.mpCheckBoxEnableDVBSub.Name = "mpCheckBoxEnableDVBSub";
      this.mpCheckBoxEnableDVBSub.Size = new System.Drawing.Size(123, 17);
      this.mpCheckBoxEnableDVBSub.TabIndex = 7;
      this.mpCheckBoxEnableDVBSub.Text = "Enable DVB subtitles";
      this.mpCheckBoxEnableDVBSub.UseVisualStyleBackColor = false;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel7);
      this.mpGroupBox3.Controls.Add(this.mpButtonDownSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonUpSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonAddSubLang);
      this.mpGroupBox3.Controls.Add(this.mpButtonRemoveSubLang);
      this.mpGroupBox3.Controls.Add(this.mpListViewPreferredSubLang);
      this.mpGroupBox3.Controls.Add(this.mpListViewAvailSubLang);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(432, 286);
      this.mpGroupBox3.TabIndex = 3;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Preferred subtitle languages";
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(236, 21);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(102, 13);
      this.mpLabel6.TabIndex = 7;
      this.mpLabel6.Text = "Preferred languages";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(6, 21);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(102, 13);
      this.mpLabel7.TabIndex = 6;
      this.mpLabel7.Text = "Available languages";
      // 
      // mpButtonDownSubLang
      // 
      this.mpButtonDownSubLang.Location = new System.Drawing.Point(289, 257);
      this.mpButtonDownSubLang.Name = "mpButtonDownSubLang";
      this.mpButtonDownSubLang.Size = new System.Drawing.Size(46, 20);
      this.mpButtonDownSubLang.TabIndex = 5;
      this.mpButtonDownSubLang.Text = "Down";
      this.mpButtonDownSubLang.UseVisualStyleBackColor = true;
      this.mpButtonDownSubLang.Click += new System.EventHandler(this.mpButtonDownSubLang_Click);
      // 
      // mpButtonUpSubLang
      // 
      this.mpButtonUpSubLang.Location = new System.Drawing.Point(237, 257);
      this.mpButtonUpSubLang.Name = "mpButtonUpSubLang";
      this.mpButtonUpSubLang.Size = new System.Drawing.Size(46, 20);
      this.mpButtonUpSubLang.TabIndex = 4;
      this.mpButtonUpSubLang.Text = "Up";
      this.mpButtonUpSubLang.UseVisualStyleBackColor = true;
      this.mpButtonUpSubLang.Click += new System.EventHandler(this.mpButtonUpSubLang_Click);
      // 
      // mpButtonAddSubLang
      // 
      this.mpButtonAddSubLang.Location = new System.Drawing.Point(202, 40);
      this.mpButtonAddSubLang.Name = "mpButtonAddSubLang";
      this.mpButtonAddSubLang.Size = new System.Drawing.Size(28, 20);
      this.mpButtonAddSubLang.TabIndex = 3;
      this.mpButtonAddSubLang.Text = ">";
      this.mpButtonAddSubLang.UseVisualStyleBackColor = true;
      this.mpButtonAddSubLang.Click += new System.EventHandler(this.mpButtonAddSubLang_Click);
      // 
      // mpButtonRemoveSubLang
      // 
      this.mpButtonRemoveSubLang.Location = new System.Drawing.Point(202, 66);
      this.mpButtonRemoveSubLang.Name = "mpButtonRemoveSubLang";
      this.mpButtonRemoveSubLang.Size = new System.Drawing.Size(28, 20);
      this.mpButtonRemoveSubLang.TabIndex = 2;
      this.mpButtonRemoveSubLang.Text = "<";
      this.mpButtonRemoveSubLang.UseVisualStyleBackColor = true;
      this.mpButtonRemoveSubLang.Click += new System.EventHandler(this.mpButtonRemoveSubLang_Click);
      // 
      // mpListViewPreferredSubLang
      // 
      this.mpListViewPreferredSubLang.AllowDrop = true;
      this.mpListViewPreferredSubLang.AllowRowReorder = true;
      this.mpListViewPreferredSubLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader8});
      this.mpListViewPreferredSubLang.FullRowSelect = true;
      this.mpListViewPreferredSubLang.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.mpListViewPreferredSubLang.HideSelection = false;
      this.mpListViewPreferredSubLang.Location = new System.Drawing.Point(239, 40);
      this.mpListViewPreferredSubLang.Name = "mpListViewPreferredSubLang";
      this.mpListViewPreferredSubLang.Size = new System.Drawing.Size(183, 211);
      this.mpListViewPreferredSubLang.TabIndex = 1;
      this.mpListViewPreferredSubLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewPreferredSubLang.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Language";
      this.columnHeader4.Width = 125;
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "ID";
      this.columnHeader8.Width = 35;
      // 
      // mpListViewAvailSubLang
      // 
      this.mpListViewAvailSubLang.AllowDrop = true;
      this.mpListViewAvailSubLang.AllowRowReorder = true;
      this.mpListViewAvailSubLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader7});
      this.mpListViewAvailSubLang.FullRowSelect = true;
      this.mpListViewAvailSubLang.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.mpListViewAvailSubLang.HideSelection = false;
      this.mpListViewAvailSubLang.Location = new System.Drawing.Point(6, 40);
      this.mpListViewAvailSubLang.Name = "mpListViewAvailSubLang";
      this.mpListViewAvailSubLang.Size = new System.Drawing.Size(183, 211);
      this.mpListViewAvailSubLang.TabIndex = 0;
      this.mpListViewAvailSubLang.UseCompatibleStateImageBehavior = false;
      this.mpListViewAvailSubLang.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Language";
      this.columnHeader3.Width = 125;
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "ID";
      this.columnHeader7.Width = 35;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.mpGroupBox8);
      this.tabPage1.Controls.Add(this.mpGroupBox7);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(464, 419);
      this.tabPage1.TabIndex = 4;
      this.tabPage1.Text = "Notifications";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox8
      // 
      this.mpGroupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox8.Controls.Add(this.chkRecnotifications);
      this.mpGroupBox8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox8.Location = new System.Drawing.Point(16, 166);
      this.mpGroupBox8.Name = "mpGroupBox8";
      this.mpGroupBox8.Size = new System.Drawing.Size(431, 82);
      this.mpGroupBox8.TabIndex = 13;
      this.mpGroupBox8.TabStop = false;
      this.mpGroupBox8.Text = "Recording notifications";
      // 
      // chkRecnotifications
      // 
      this.chkRecnotifications.AutoSize = true;
      this.chkRecnotifications.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.chkRecnotifications.Location = new System.Drawing.Point(22, 19);
      this.chkRecnotifications.Name = "chkRecnotifications";
      this.chkRecnotifications.Size = new System.Drawing.Size(327, 17);
      this.chkRecnotifications.TabIndex = 0;
      this.chkRecnotifications.Text = "Enabled (shows a notification when a recording starts and stops)";
      this.chkRecnotifications.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox7
      // 
      this.mpGroupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox7.Controls.Add(this.cbConfirmTimeshiftStop);
      this.mpGroupBox7.Controls.Add(this.txtNotifyAfter);
      this.mpGroupBox7.Controls.Add(this.labelNotifyTimeout);
      this.mpGroupBox7.Controls.Add(this.checkBoxNotifyPlaySound);
      this.mpGroupBox7.Controls.Add(this.mpLabel2);
      this.mpGroupBox7.Controls.Add(this.txtNotifyBefore);
      this.mpGroupBox7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox7.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox7.Name = "mpGroupBox7";
      this.mpGroupBox7.Size = new System.Drawing.Size(431, 135);
      this.mpGroupBox7.TabIndex = 12;
      this.mpGroupBox7.TabStop = false;
      this.mpGroupBox7.Text = "TV notifications";
      // 
      // cbConfirmTimeshiftStop
      // 
      this.cbConfirmTimeshiftStop.AutoSize = true;
      this.cbConfirmTimeshiftStop.Checked = true;
      this.cbConfirmTimeshiftStop.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbConfirmTimeshiftStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbConfirmTimeshiftStop.Location = new System.Drawing.Point(22, 103);
      this.cbConfirmTimeshiftStop.Name = "cbConfirmTimeshiftStop";
      this.cbConfirmTimeshiftStop.Size = new System.Drawing.Size(230, 17);
      this.cbConfirmTimeshiftStop.TabIndex = 12;
      this.cbConfirmTimeshiftStop.Text = "Ask for confirmation when stopping timeshift";
      this.cbConfirmTimeshiftStop.UseVisualStyleBackColor = true;
      // 
      // txtNotifyAfter
      // 
      this.txtNotifyAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtNotifyAfter.BorderColor = System.Drawing.Color.Empty;
      this.txtNotifyAfter.Location = new System.Drawing.Point(164, 47);
      this.txtNotifyAfter.Name = "txtNotifyAfter";
      this.txtNotifyAfter.Size = new System.Drawing.Size(229, 20);
      this.txtNotifyAfter.TabIndex = 11;
      this.txtNotifyAfter.Text = "15";
      // 
      // labelNotifyTimeout
      // 
      this.labelNotifyTimeout.AutoSize = true;
      this.labelNotifyTimeout.Location = new System.Drawing.Point(19, 50);
      this.labelNotifyTimeout.Name = "labelNotifyTimeout";
      this.labelNotifyTimeout.Size = new System.Drawing.Size(139, 13);
      this.labelNotifyTimeout.TabIndex = 10;
      this.labelNotifyTimeout.Text = "Hide notification after (sec.):";
      // 
      // checkBoxNotifyPlaySound
      // 
      this.checkBoxNotifyPlaySound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxNotifyPlaySound.AutoSize = true;
      this.checkBoxNotifyPlaySound.Checked = true;
      this.checkBoxNotifyPlaySound.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxNotifyPlaySound.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxNotifyPlaySound.Location = new System.Drawing.Point(22, 73);
      this.checkBoxNotifyPlaySound.Name = "checkBoxNotifyPlaySound";
      this.checkBoxNotifyPlaySound.Size = new System.Drawing.Size(105, 17);
      this.checkBoxNotifyPlaySound.TabIndex = 9;
      this.checkBoxNotifyPlaySound.Text = "Play \"notify.wav\"";
      this.checkBoxNotifyPlaySound.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(19, 24);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(96, 13);
      this.mpLabel2.TabIndex = 8;
      this.mpLabel2.Text = "Notify before (sec):";
      // 
      // txtNotifyBefore
      // 
      this.txtNotifyBefore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtNotifyBefore.BorderColor = System.Drawing.Color.Empty;
      this.txtNotifyBefore.Location = new System.Drawing.Point(164, 21);
      this.txtNotifyBefore.Name = "txtNotifyBefore";
      this.txtNotifyBefore.Size = new System.Drawing.Size(229, 20);
      this.txtNotifyBefore.TabIndex = 7;
      this.txtNotifyBefore.Text = "300";
      // 
      // TV
      // 
      this.Controls.Add(this.tabControlTVGeneral);
      this.Name = "TV";
      this.Size = new System.Drawing.Size(510, 450);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.tabControlTVGeneral.ResumeLayout(false);
      this.tabPageGeneralSettings.ResumeLayout(false);
      this.mpGroupBoxAdditional.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).EndInit();
      this.grpTsReader.ResumeLayout(false);
      this.grpTsReader.PerformLayout();
      this.mpGroupBox6.ResumeLayout(false);
      this.mpGroupBox6.PerformLayout();
      this.mpGroupBox5.ResumeLayout(false);
      this.mpGroupBox5.PerformLayout();
      this.tabPageAudioLanguages.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tabPageSubtitles.ResumeLayout(false);
      this.mpGroupBox4.ResumeLayout(false);
      this.mpGroupBox4.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.tabPage1.ResumeLayout(false);
      this.mpGroupBox8.ResumeLayout(false);
      this.mpGroupBox8.PerformLayout();
      this.mpGroupBox7.ResumeLayout(false);
      this.mpGroupBox7.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    #region Private methods

    private void mpListView2_SelectedIndexChanged(object sender, EventArgs e) {}

    private void mpCheckBox1_CheckedChanged(object sender, EventArgs e) {}

    private void mpButtonAddAudioLang_Click(object sender, EventArgs e)
    {
      if (mpListViewAvailAudioLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewAvailAudioLang.SelectedItems)
      {
        mpListViewAvailAudioLang.Items.Remove(lang);
        mpListViewPreferredAudioLang.Items.Add(lang);
      }
    }

    private void mpButtonRemoveAudioLang_Click(object sender, EventArgs e)
    {
      if (mpListViewPreferredAudioLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewPreferredAudioLang.SelectedItems)
      {
        mpListViewPreferredAudioLang.Items.Remove(lang);
        mpListViewAvailAudioLang.Items.Add(lang);
      }
    }

    private void mpButtonUpAudioLang_Click(object sender, EventArgs e)
    {
      moveItemUp(mpListViewPreferredAudioLang);
    }

    private void mpButtonDownAudioLang_Click(object sender, EventArgs e)
    {
      moveItemDown(mpListViewPreferredAudioLang);
    }

    private void mpButtonAddSubLang_Click(object sender, EventArgs e)
    {
      if (mpListViewAvailSubLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewAvailSubLang.SelectedItems)
      {
        mpListViewAvailSubLang.Items.Remove(lang);
        mpListViewPreferredSubLang.Items.Add(lang);
      }
    }

    private void mpButtonRemoveSubLang_Click(object sender, EventArgs e)
    {
      if (mpListViewPreferredSubLang.SelectedItems.Count == 0)
      {
        return;
      }

      foreach (ListViewItem lang in mpListViewPreferredSubLang.SelectedItems)
      {
        mpListViewPreferredSubLang.Items.Remove(lang);
        mpListViewAvailSubLang.Items.Add(lang);
      }
    }

    private void mpButtonUpSubLang_Click(object sender, EventArgs e)
    {
      moveItemUp(mpListViewPreferredSubLang);
    }

    private void mpButtonDownSubLang_Click(object sender, EventArgs e)
    {
      moveItemDown(mpListViewPreferredSubLang);
    }

    private void moveItemDown(MPListView mplistView)
    {
      ListView.SelectedIndexCollection indexes = mplistView.SelectedIndices;
      if (indexes.Count == 0)
      {
        return;
      }
      if (mplistView.Items.Count < 2)
      {
        return;
      }
      for (int i = indexes.Count - 1; i >= 0; i--)
      {
        int index = indexes[i];
        ListViewItem item = mplistView.Items[index];
        mplistView.Items.RemoveAt(index);
        if (index + 1 < mplistView.Items.Count)
        {
          mplistView.Items.Insert(index + 1, item);
        }
        else
        {
          mplistView.Items.Add(item);
        }
      }
    }

    private void moveItemUp(MPListView mplistView)
    {
      ListView.SelectedIndexCollection indexes = mplistView.SelectedIndices;
      if (indexes.Count == 0)
      {
        return;
      }
      for (int i = 0; i < indexes.Count; ++i)
      {
        int index = indexes[i];
        if (index > 0)
        {
          ListViewItem item = mplistView.Items[index];
          mplistView.Items.RemoveAt(index);
          mplistView.Items.Insert(index - 1, item);
        }
      }
    }

    private void cbAutoFullscreen_CheckedChanged(object sender, EventArgs e)
    {
      if (cbAutoFullscreen.Checked)
      {
        cbTurnOnTv.Checked = true;
      }
    }

    #endregion
  }
}