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
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using MediaPortal.WinCustomControls;

namespace MediaPortal.Configuration.Sections
{
  public class TVClient : SectionSettings
  {
    #region variables

    private string _preferredAudioLanguages;
    private string _preferredSubLanguages;
    private IList<string> _languageCodes;

    private MPGroupBox mpGroupBox2;
    private MPTextBox mpTextBoxHostname;
    private MPLabel mpLabel3;
    private MPGroupBox mpGroupBox1;
    private MPCheckBox mpCheckBoxPrefAC3;
    private IList<string> _languagesAvail;
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
    private TabPage tabPage1;
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
    private MPGroupBox mpGroupBox900;
    private MPTextBox mpTextBoxMacAddress;
    private MPLabel mpLabelWOLTimeOut;
    private MPNumericTextBox mpNumericTextBoxWOLTimeOut;
    private MPLabel mpLabel400;
    private MPCheckBox mpCheckBoxIsAutoMacAddressEnabled;
    private MPCheckBox mpCheckBoxIsWakeOnLanEnabled;
    private ListViewColumnSorter _columnSorter;
    private MPLabel labelShowEpisodeinfo;
    private MPComboBox comboboxShowEpisodeInfo;

    private string[] ShowEpisodeOptions = new string[]
                                            {
                                              "[None]", // Dont show episode info
                                              "Number", // Show seriesNum.episodeNum.episodePart
                                              "Title", // Show episodeName
                                              "Number + Title" // Show number and title
                                            };

    private MPCheckBox cbContinuousScrollGuide;
    private MPCheckBox mpCheckBoxEnableCCSub;
    private TabPage tabPage2;
    private MPTabControl tabControl2;
    private MPTabPage tabPageGenreMap;
    private MPGroupBox groupBox4;
    private MPButton buttonMapGenres;
    private MPButton buttonUnmapGenres;
    private MPListView listViewProgramGenres;
    private ColumnHeader columnHeader12;
    private MPListView listViewMappedGenres;
    private ColumnHeader columnHeader13;

    private bool _SingleSeat;
    private MPListView listViewGuideGenres;
    private ColumnHeader columnHeader9;

    protected const int LOCALIZED_GENRE_STRING_BASE = 1250;
    protected const int LOCALIZED_GENRE_STRING_COUNT = 7;

    protected IList<string> _allProgramGenres;
    protected List<string> _genreList = new List<string>();
    protected IDictionary<string, string> _genreMap = new Dictionary<string, string>();
    private MPCheckBox mpCheckBoxRatingAsMovie;

    #endregion

    public TVClient()
      : this("TV Client") {}

    public TVClient(string name)
      : base(name)
    {
      InitializeComponent();
      // Episode Options
      comboboxShowEpisodeInfo.Items.Clear();
      comboboxShowEpisodeInfo.Items.AddRange(ShowEpisodeOptions);
    }

    private bool LoadGenreMap(Settings xmlreader)
    {
      int genreId;
      string genre;
      List<string> programGenres;
      IDictionary<string, string> allGenres = xmlreader.GetSection<string>("genremap");

      // Each genre map entry is a '{' delimited list of "program" genre names (those that may be compared with the genre from the program listings).
      // It is an error if a single "program" genre is mapped to more than one guide genre; behavior is undefined for this condition.
      foreach (var genreMapEntry in allGenres)
      {
        // The genremap key is an integer value that is added to a base value in order to locate the correct localized genre name string.
        genreId = int.Parse(genreMapEntry.Key);
        genre = GUILocalizeStrings.Get(LOCALIZED_GENRE_STRING_BASE + genreId);
        _genreList.Add(genre);

        programGenres = new List<string>(genreMapEntry.Value.Split(new char[] { '{' }, StringSplitOptions.RemoveEmptyEntries));

        foreach (string programGenre in programGenres)
        {
          try
          {
            _genreMap.Add(programGenre, genre);
          }
          catch (ArgumentException)
          {
            Log.Warn("TvGuideBase.cs: The following genre name appears more than once in the genre map: {0}", programGenre);
          }
        }
      }

      return _genreMap.Count > 0;
    }

    private void SaveGenreMap(Settings xmlwriter)
    {
      // Each genre map entry is a '{' delimited list of "program" genre names (those that may be compared with the genre from the program listings).
      string programGenreList;
      foreach (var genre in _genreList)
      {
        programGenreList = "";
        foreach (var genreMapEntry in _genreMap)
        {
          if (genreMapEntry.Value.Equals(genre))
          {
            programGenreList += genreMapEntry.Key + "{";
          }
        }

        // Write the genre map using the index of the genre as the key (entry name), see LoadGenreMap().
        xmlwriter.SetValue("genremap", _genreList.IndexOf(genre).ToString(), programGenreList.TrimEnd('{'));
      }
    }

    private void PopulateGuideGenreList()
    {
      // Populate the guide genre list with names.
      listViewGuideGenres.BeginUpdate();
      listViewGuideGenres.Items.Clear();

      for (int i=0; i < _genreList.Count; i++)
      {
        ListViewItem item = new ListViewItem(new string[] { _genreList[i] });
        item.Name = _genreList[i];
        listViewGuideGenres.Items.Add(item);
      }
      listViewGuideGenres.EndUpdate();
    }

    private void PopulateGenreLists()
    {
      ListViewItem selectedGenre = listViewGuideGenres.SelectedItems[0];

      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();
      
      listViewMappedGenres.Items.Clear();
      listViewProgramGenres.Items.Clear();

      // Populate the list of mapped and program genres.
      foreach (var genre in _genreMap)
      {
        // If the program genre is mapped to the selected genre then add it to the mapped list.
        if (genre.Value.Equals(selectedGenre.Text))
        {
          listViewMappedGenres.Items.Add(genre.Key);
        }
      }

      // Initially add all program genres that are not mapped to any guide genre to the list.
      foreach (string genre in _allProgramGenres)
      {
        if (!_genreMap.ContainsKey(genre))
        {
          listViewProgramGenres.Items.Add(genre);
        }
      }

      listViewMappedGenres.EndUpdate();
      listViewProgramGenres.EndUpdate();
    }

    protected void MapProgramGenres()
    {
      ListViewItem selectedGenre = listViewGuideGenres.SelectedItems[0];

      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();

      foreach (ListViewItem genre in listViewProgramGenres.SelectedItems)
      {
        listViewProgramGenres.Items.Remove(genre);
        listViewMappedGenres.Items.Add(genre);

        // Update the genre map.
        _genreMap.Add(genre.Text, selectedGenre.Text);
      }

      listViewMappedGenres.EndUpdate();
      listViewProgramGenres.EndUpdate();
    }

    protected void UnmapProgramGenres()
    {
      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();

      foreach (ListViewItem genre in listViewMappedGenres.SelectedItems)
      {
        listViewMappedGenres.Items.Remove(genre);
        listViewProgramGenres.Items.Add(genre);

        // Update the genre map.
        _genreMap.Remove(genre.Text);
      }

      listViewMappedGenres.EndUpdate();
      listViewProgramGenres.EndUpdate();
    }

    private void CreateDefaultGenres(Settings settings)
    {
      for (int i = 0; i < LOCALIZED_GENRE_STRING_COUNT; i++)
      {
        settings.SetValue("genremap", i.ToString(), String.Empty);  // Genre not mapped
      }
    }

    public override void LoadSettings()
    {
      //Load parameters from XML File
      using (Settings xmlreader = new MPSettings())
      {
        mpTextBoxHostname.Text = xmlreader.GetValueAsString("tvservice", "hostname", "");
        mpCheckBoxPrefAC3.Checked = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        mpCheckBoxPrefAudioOverLang.Checked = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
        _preferredAudioLanguages = xmlreader.GetValueAsString("tvservice", "preferredaudiolanguages", "");
        _preferredSubLanguages = xmlreader.GetValueAsString("tvservice", "preferredsublanguages", "");

        mpCheckBoxEnableDVBSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        mpCheckBoxEnableTTXTSub.Checked = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        mpCheckBoxEnableCCSub.Checked = xmlreader.GetValueAsBool("tvservice", "ccsubtitles", false);
        mpCheckBoxAutoShowSubWhenTvStarts.Checked = xmlreader.GetValueAsBool("tvservice", "autoshowsubwhentvstarts", true);
        enableAudioDualMonoModes.Checked = xmlreader.GetValueAsBool("tvservice", "audiodualmono", false);
        cbHideAllChannels.Checked = xmlreader.GetValueAsBool("mytv", "hideAllChannelsGroup", false);
        cbShowChannelStateIcons.Checked = xmlreader.GetValueAsBool("mytv", "showChannelStateIcons", true);
        cbContinuousScrollGuide.Checked = xmlreader.GetValueAsBool("mytv", "continuousScrollGuide", false);
        cbRelaxTsReader.Checked = xmlreader.GetValueAsBool("mytv", "relaxTsReader", false);

        mpCheckBoxIsWakeOnLanEnabled.Checked = xmlreader.GetValueAsBool("tvservice", "isWakeOnLanEnabled", false);
        mpNumericTextBoxWOLTimeOut.Text = xmlreader.GetValueAsString("tvservice", "WOLTimeOut", "10");
        mpCheckBoxIsAutoMacAddressEnabled.Checked = xmlreader.GetValueAsBool("tvservice", "isAutoMacAddressEnabled",
                                                                             true);
        mpTextBoxMacAddress.Text = xmlreader.GetValueAsString("tvservice", "macAddress", "00:00:00:00:00:00");

        mpCheckBoxRatingAsMovie.Checked = xmlreader.GetValueAsBool("genreoptions", "specifympaaratedasmovie", true);
        chkRecnotifications.Checked = xmlreader.GetValueAsBool("mytv", "enableRecNotifier", false);
        txtNotifyBefore.Text = xmlreader.GetValueAsString("mytv", "notifyTVBefore", "300");
        txtNotifyAfter.Text = xmlreader.GetValueAsString("mytv", "notifyTVTimeout", "15");
        checkBoxNotifyPlaySound.Checked = xmlreader.GetValueAsBool("mytv", "notifybeep", true);
        int showEpisodeinfo = xmlreader.GetValueAsInt("mytv", "showEpisodeInfo", 0);
        if (showEpisodeinfo > this.ShowEpisodeOptions.Length)
        {
          showEpisodeinfo = 0;
        }
        comboboxShowEpisodeInfo.SelectedIndex = showEpisodeinfo;
      }

      // Populate the list of program genres from the tv database.
      Assembly assem = Assembly.LoadFrom(Config.GetFolder(Config.Dir.Base) + "\\TvControl.dll");
      if (assem != null)
      {
        Type[] types = assem.GetExportedTypes();
        foreach (Type exportedType in types)
        {
          try
          {
            if (exportedType.Name == "TvServer")
            {
              Object genreObject = null;
              genreObject = Activator.CreateInstance(exportedType);
              MethodInfo methodInfo = exportedType.GetMethod("GetGenres", BindingFlags.Public | BindingFlags.Instance);
              _allProgramGenres = methodInfo.Invoke(genreObject, null) as List<String>;

              using (Settings xmlreader = new MPSettings())
              {
                // If the genre map does not contain any entries then we'll create an initial default map.
                if (!xmlreader.HasSection<string>("genremap"))
                {
                  CreateDefaultGenres(xmlreader);
                }

                // Load the genre map from MP settings.
                if (_genreMap.Count == 0)
                {
                  LoadGenreMap(xmlreader);
                }

                if (!xmlreader.HasSection<string>("genreoptions"))
                {
                  xmlreader.SetValueAsBool("genreoptions", "specifympaaratedasmovie", true);  // Rated programs are movies
                }

                // Populate the guide genre list with names.
                PopulateGuideGenreList();
              }
            }
          }
          catch (TargetInvocationException ex)
          {
            Log.Warn("TVClient: Failed to load genres {0}", ex.ToString());
            continue;
          }
          catch (Exception gex)
          {
            Log.Warn("TVClient: Failed to load settings {0}", gex.Message);
          }
        }
      }

      mpCheckBoxIsWakeOnLanEnabled_CheckedChanged(null, null);

      // Enable this Panel if the TvPlugin exists in the plug-in Directory
      Enabled = true;

      try
      {
        assem = Assembly.LoadFrom(Config.GetFolder(Config.Dir.Base) + "\\TvLibrary.Interfaces.dll");
        if (assem != null)
        {
          Type[] types = assem.GetExportedTypes();
          foreach (Type exportedType in types)
          {
            try
            {
              if (exportedType.Name == "Languages")
              {
                // Load available languages into variables. 
                Object languageObject = null;
                languageObject = Activator.CreateInstance(exportedType);
                MethodInfo methodInfo = exportedType.GetMethod("GetLanguages",
                                                               BindingFlags.Public | BindingFlags.Instance);
                _languagesAvail = methodInfo.Invoke(languageObject, null) as List<String>;
                methodInfo = exportedType.GetMethod("GetLanguageCodes", BindingFlags.Public | BindingFlags.Instance);
                _languageCodes = (List<String>)methodInfo.Invoke(languageObject, null);

                if (_languagesAvail == null || _languageCodes == null)
                {
                  Log.Debug("Failed to load languages");
                  return;
                }
                else
                {
                  mpListViewAvailAudioLang.Items.Clear();
                  mpListViewPreferredAudioLang.Items.Clear();
                  for (int i = 0; i < _languagesAvail.Count; i++)
                  {
                    ListViewItem item = new ListViewItem(new string[] { _languagesAvail[i], _languageCodes[i] });
                    item.Name = _languageCodes[i];
                    if (!_preferredAudioLanguages.Contains(item.Name))
                    {
                      if (!mpListViewAvailAudioLang.Items.ContainsKey(item.Name))
                      {
                        mpListViewAvailAudioLang.Items.Add(item);
                      }
                    }
                  }
                  mpListViewAvailAudioLang.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
                  _columnSorter.SortColumn = 0;
                  _columnSorter.Order = SortOrder.Ascending;
                  mpListViewAvailAudioLang.Sort();

                  if (_preferredAudioLanguages.Length > 0)
                  {
                    string[] langArr = _preferredAudioLanguages.Split(';');

                    for (int i = 0; i < langArr.Length; i++)
                    {
                      string langStr = langArr[i];
                      if (langStr.Trim().Length > 0)
                      {
                        for (int j = 0; j < _languagesAvail.Count; j++)
                        {
                          if (_languageCodes[j].Contains(langStr))
                          {
                            ListViewItem item = new ListViewItem(new string[] {_languagesAvail[j], _languageCodes[j]});
                            item.Name = _languageCodes[j];
                            mpListViewPreferredAudioLang.Items.Add(item);
                            break;
                          }
                        }
                      }
                    }
                  }

                  mpListViewAvailSubLang.Items.Clear();
                  mpListViewPreferredSubLang.Items.Clear();
                  for (int i = 0; i < _languagesAvail.Count; i++)
                  {
                    ListViewItem item = new ListViewItem(new string[] { _languagesAvail[i], _languageCodes[i] });
                    item.Name = _languageCodes[i];
                    if (!_preferredSubLanguages.Contains(item.Name))
                    {
                      if (!mpListViewAvailSubLang.Items.ContainsKey(item.Name))
                      {
                        mpListViewAvailSubLang.Items.Add(item);
                      }
                    }
                  }
                  mpListViewAvailSubLang.ListViewItemSorter = _columnSorter = new ListViewColumnSorter();
                  _columnSorter.SortColumn = 0;
                  _columnSorter.Order = SortOrder.Ascending;
                  mpListViewAvailSubLang.Sort();

                  if (_preferredSubLanguages.Length > 0)
                  {
                    string[] langArr = _preferredSubLanguages.Split(';');

                    for (int i = 0; i < langArr.Length; i++)
                    {
                      string langStr = langArr[i];
                      if (langStr.Trim().Length > 0)
                      {
                        for (int j = 0; j < _languagesAvail.Count; j++)
                        {
                          if (_languageCodes[j].Contains(langStr))
                          {
                            ListViewItem item = new ListViewItem(new string[] {_languagesAvail[j], _languageCodes[j]});
                            item.Name = _languageCodes[j];
                            mpListViewPreferredSubLang.Items.Add(item);
                            break;
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Warn("TVClient: Failed to load languages {0}", ex.ToString());
              continue;
            }
            catch (Exception gex)
            {
              Log.Warn("TVClient: Failed to load settings {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Debug("Configuration: Loading TVLibrary.Interface assembly");
        Log.Debug("Configuration: Exception: {0}", ex);
      }

      _SingleSeat = Network.IsSingleSeat();
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        string prefLangs = "";
        xmlwriter.SetValue("tvservice", "hostname", mpTextBoxHostname.Text);
        xmlwriter.SetValueAsBool("tvservice", "preferac3", mpCheckBoxPrefAC3.Checked);
        xmlwriter.SetValueAsBool("tvservice", "preferAudioTypeOverLang", mpCheckBoxPrefAudioOverLang.Checked);

        xmlwriter.SetValueAsBool("tvservice", "dvbbitmapsubtitles", mpCheckBoxEnableDVBSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "dvbttxtsubtitles", mpCheckBoxEnableTTXTSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "ccsubtitles", mpCheckBoxEnableCCSub.Checked);
        xmlwriter.SetValueAsBool("tvservice", "autoshowsubwhentvstarts", mpCheckBoxAutoShowSubWhenTvStarts.Checked);
        xmlwriter.SetValueAsBool("tvservice", "audiodualmono", enableAudioDualMonoModes.Checked);
        xmlwriter.SetValueAsBool("mytv", "hideAllChannelsGroup", cbHideAllChannels.Checked);
        xmlwriter.SetValueAsBool("myradio", "hideAllChannelsGroup", cbHideAllChannels.Checked);
        //currently we use the same checkbox for radio but different settings in config file
        xmlwriter.SetValueAsBool("mytv", "showChannelStateIcons", cbShowChannelStateIcons.Checked);
        xmlwriter.SetValueAsBool("mytv", "continuousScrollGuide", cbContinuousScrollGuide.Checked);
        xmlwriter.SetValueAsBool("mytv", "relaxTsReader", cbRelaxTsReader.Checked);

        xmlwriter.SetValueAsBool("tvservice", "isWakeOnLanEnabled", mpCheckBoxIsWakeOnLanEnabled.Checked);
        xmlwriter.SetValue("tvservice", "WOLTimeOut", mpNumericTextBoxWOLTimeOut.Text);
        xmlwriter.SetValueAsBool("tvservice", "isAutoMacAddressEnabled", mpCheckBoxIsAutoMacAddressEnabled.Checked);
        xmlwriter.SetValue("tvservice", "macAddress", mpTextBoxMacAddress.Text);

        xmlwriter.SetValueAsBool("genreoptions", "specifympaaratedasmovie", mpCheckBoxRatingAsMovie.Checked);

        xmlwriter.SetValueAsBool("mytv", "enableRecNotifier", chkRecnotifications.Checked);
        xmlwriter.SetValue("mytv", "notifyTVBefore", txtNotifyBefore.Text);
        xmlwriter.SetValue("mytv", "notifyTVTimeout", txtNotifyAfter.Text);
        xmlwriter.SetValueAsBool("mytv", "notifybeep", checkBoxNotifyPlaySound.Checked);
        xmlwriter.SetValue("mytv", "showEpisodeInfo", comboboxShowEpisodeInfo.SelectedIndex);

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

        SaveGenreMap(xmlwriter);
      }
    }

    private void InitializeComponent()
    {
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTextBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.enableAudioDualMonoModes = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxPrefAudioOverLang = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxPrefAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlTVGeneral = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageGeneralSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.mpGroupBox900 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpNumericTextBoxWOLTimeOut = new MediaPortal.UserInterface.Controls.MPNumericTextBox();
      this.mpLabelWOLTimeOut = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxMacAddress = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel400 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpCheckBoxIsAutoMacAddressEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpCheckBoxIsWakeOnLanEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
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
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.mpGroupBox8 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.chkRecnotifications = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox7 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.txtNotifyAfter = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelNotifyTimeout = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxNotifyPlaySound = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtNotifyBefore = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tabControl2 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageGenreMap = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBoxRatingAsMovie = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.listViewGuideGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.buttonMapGenres = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUnmapGenres = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewProgramGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.listViewMappedGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.tabControlTVGeneral.SuspendLayout();
      this.tabPageGeneralSettings.SuspendLayout();
      this.mpGroupBox900.SuspendLayout();
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
      this.tabPage2.SuspendLayout();
      this.tabControl2.SuspendLayout();
      this.tabPageGenreMap.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.mpTextBoxHostname);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(16, 16);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(431, 53);
      this.mpGroupBox2.TabIndex = 10;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "TV-Server";
      // 
      // mpTextBoxHostname
      // 
      this.mpTextBoxHostname.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxHostname.Location = new System.Drawing.Point(126, 22);
      this.mpTextBoxHostname.Name = "mpTextBoxHostname";
      this.mpTextBoxHostname.Size = new System.Drawing.Size(229, 20);
      this.mpTextBoxHostname.TabIndex = 6;
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(19, 25);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(58, 13);
      this.mpLabel3.TabIndex = 5;
      this.mpLabel3.Text = "Hostname:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.enableAudioDualMonoModes);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAudioOverLang);
      this.mpGroupBox1.Controls.Add(this.mpCheckBoxPrefAC3);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(16, 338);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(432, 75);
      this.mpGroupBox1.TabIndex = 9;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio stream settings";
      // 
      // enableAudioDualMonoModes
      // 
      this.enableAudioDualMonoModes.AutoSize = true;
      this.enableAudioDualMonoModes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.enableAudioDualMonoModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableAudioDualMonoModes.Location = new System.Drawing.Point(9, 41);
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
      this.mpCheckBoxPrefAudioOverLang.Location = new System.Drawing.Point(239, 18);
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
      this.mpCheckBoxPrefAC3.Location = new System.Drawing.Point(9, 18);
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
      this.tabControlTVGeneral.Controls.Add(this.tabPage2);
      this.tabControlTVGeneral.Location = new System.Drawing.Point(0, 2);
      this.tabControlTVGeneral.Name = "tabControlTVGeneral";
      this.tabControlTVGeneral.SelectedIndex = 0;
      this.tabControlTVGeneral.Size = new System.Drawing.Size(472, 445);
      this.tabControlTVGeneral.TabIndex = 11;
      // 
      // tabPageGeneralSettings
      // 
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox900);
      this.tabPageGeneralSettings.Controls.Add(this.grpTsReader);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox6);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox5);
      this.tabPageGeneralSettings.Controls.Add(this.mpGroupBox2);
      this.tabPageGeneralSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageGeneralSettings.Name = "tabPageGeneralSettings";
      this.tabPageGeneralSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageGeneralSettings.Size = new System.Drawing.Size(464, 419);
      this.tabPageGeneralSettings.TabIndex = 0;
      this.tabPageGeneralSettings.Text = "General settings";
      this.tabPageGeneralSettings.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox900
      // 
      this.mpGroupBox900.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.mpGroupBox900.Controls.Add(this.mpNumericTextBoxWOLTimeOut);
      this.mpGroupBox900.Controls.Add(this.mpLabelWOLTimeOut);
      this.mpGroupBox900.Controls.Add(this.mpTextBoxMacAddress);
      this.mpGroupBox900.Controls.Add(this.mpLabel400);
      this.mpGroupBox900.Controls.Add(this.mpCheckBoxIsAutoMacAddressEnabled);
      this.mpGroupBox900.Controls.Add(this.mpCheckBoxIsWakeOnLanEnabled);
      this.mpGroupBox900.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox900.Location = new System.Drawing.Point(16, 286);
      this.mpGroupBox900.Name = "mpGroupBox900";
      this.mpGroupBox900.Size = new System.Drawing.Size(431, 126);
      this.mpGroupBox900.TabIndex = 14;
      this.mpGroupBox900.TabStop = false;
      this.mpGroupBox900.Text = "Wake-On-Lan";
      // 
      // mpNumericTextBoxWOLTimeOut
      // 
      this.mpNumericTextBoxWOLTimeOut.AutoCompleteCustomSource.AddRange(new string[] {
            "10",
            "20",
            "30",
            "40",
            "50",
            "60",
            "70",
            "80",
            "90"});
      this.mpNumericTextBoxWOLTimeOut.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.mpNumericTextBoxWOLTimeOut.Enabled = false;
      this.mpNumericTextBoxWOLTimeOut.Location = new System.Drawing.Point(126, 42);
      this.mpNumericTextBoxWOLTimeOut.MaxLength = 4;
      this.mpNumericTextBoxWOLTimeOut.Name = "mpNumericTextBoxWOLTimeOut";
      this.mpNumericTextBoxWOLTimeOut.Size = new System.Drawing.Size(45, 20);
      this.mpNumericTextBoxWOLTimeOut.TabIndex = 9;
      this.mpNumericTextBoxWOLTimeOut.Tag = "Default timeout is 10 seconds";
      this.mpNumericTextBoxWOLTimeOut.Text = "10";
      this.mpNumericTextBoxWOLTimeOut.Value = 10;
      this.mpNumericTextBoxWOLTimeOut.WordWrap = false;
      // 
      // mpLabelWOLTimeOut
      // 
      this.mpLabelWOLTimeOut.AutoSize = true;
      this.mpLabelWOLTimeOut.Location = new System.Drawing.Point(41, 45);
      this.mpLabelWOLTimeOut.Name = "mpLabelWOLTimeOut";
      this.mpLabelWOLTimeOut.Size = new System.Drawing.Size(72, 13);
      this.mpLabelWOLTimeOut.TabIndex = 8;
      this.mpLabelWOLTimeOut.Text = "WOL timeout:";
      // 
      // mpTextBoxMacAddress
      // 
      this.mpTextBoxMacAddress.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxMacAddress.Location = new System.Drawing.Point(126, 91);
      this.mpTextBoxMacAddress.MaxLength = 17;
      this.mpTextBoxMacAddress.Name = "mpTextBoxMacAddress";
      this.mpTextBoxMacAddress.Size = new System.Drawing.Size(97, 20);
      this.mpTextBoxMacAddress.TabIndex = 7;
      this.mpTextBoxMacAddress.Text = "00:00:00:00:00:00";
      // 
      // mpLabel400
      // 
      this.mpLabel400.AutoSize = true;
      this.mpLabel400.Location = new System.Drawing.Point(41, 94);
      this.mpLabel400.Name = "mpLabel400";
      this.mpLabel400.Size = new System.Drawing.Size(74, 13);
      this.mpLabel400.TabIndex = 6;
      this.mpLabel400.Text = "MAC Address:";
      // 
      // mpCheckBoxIsAutoMacAddressEnabled
      // 
      this.mpCheckBoxIsAutoMacAddressEnabled.AutoSize = true;
      this.mpCheckBoxIsAutoMacAddressEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxIsAutoMacAddressEnabled.Location = new System.Drawing.Point(44, 68);
      this.mpCheckBoxIsAutoMacAddressEnabled.Name = "mpCheckBoxIsAutoMacAddressEnabled";
      this.mpCheckBoxIsAutoMacAddressEnabled.Size = new System.Drawing.Size(192, 17);
      this.mpCheckBoxIsAutoMacAddressEnabled.TabIndex = 1;
      this.mpCheckBoxIsAutoMacAddressEnabled.Text = "Auto-configure server MAC Address";
      this.mpCheckBoxIsAutoMacAddressEnabled.UseVisualStyleBackColor = true;
      this.mpCheckBoxIsAutoMacAddressEnabled.CheckedChanged += new System.EventHandler(this.mpCheckBoxIsAutoMacAddressEnabled_CheckedChanged);
      // 
      // mpCheckBoxIsWakeOnLanEnabled
      // 
      this.mpCheckBoxIsWakeOnLanEnabled.AutoSize = true;
      this.mpCheckBoxIsWakeOnLanEnabled.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxIsWakeOnLanEnabled.Location = new System.Drawing.Point(22, 19);
      this.mpCheckBoxIsWakeOnLanEnabled.Name = "mpCheckBoxIsWakeOnLanEnabled";
      this.mpCheckBoxIsWakeOnLanEnabled.Size = new System.Drawing.Size(172, 17);
      this.mpCheckBoxIsWakeOnLanEnabled.TabIndex = 0;
      this.mpCheckBoxIsWakeOnLanEnabled.Text = "Wake up TV Server as needed";
      this.mpCheckBoxIsWakeOnLanEnabled.UseVisualStyleBackColor = true;
      this.mpCheckBoxIsWakeOnLanEnabled.CheckedChanged += new System.EventHandler(this.mpCheckBoxIsWakeOnLanEnabled_CheckedChanged);
      // 
      // grpTsReader
      // 
      this.grpTsReader.Controls.Add(this.cbRelaxTsReader);
      this.grpTsReader.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpTsReader.Location = new System.Drawing.Point(16, 231);
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
      this.mpGroupBox6.Location = new System.Drawing.Point(16, 127);
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
      this.cbContinuousScrollGuide.TabIndex = 2;
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
      this.comboboxShowEpisodeInfo.TabIndex = 1;
      // 
      // mpGroupBox5
      // 
      this.mpGroupBox5.Controls.Add(this.cbHideAllChannels);
      this.mpGroupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox5.Location = new System.Drawing.Point(16, 75);
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
      this.tabPage1.Text = "Notifier";
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
      // txtNotifyAfter
      // 
      this.txtNotifyAfter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtNotifyAfter.BorderColor = System.Drawing.Color.Empty;
      this.txtNotifyAfter.Location = new System.Drawing.Point(164, 73);
      this.txtNotifyAfter.Name = "txtNotifyAfter";
      this.txtNotifyAfter.Size = new System.Drawing.Size(229, 20);
      this.txtNotifyAfter.TabIndex = 11;
      this.txtNotifyAfter.Text = "15";
      // 
      // labelNotifyTimeout
      // 
      this.labelNotifyTimeout.AutoSize = true;
      this.labelNotifyTimeout.Location = new System.Drawing.Point(19, 76);
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
      this.checkBoxNotifyPlaySound.Location = new System.Drawing.Point(22, 99);
      this.checkBoxNotifyPlaySound.Name = "checkBoxNotifyPlaySound";
      this.checkBoxNotifyPlaySound.Size = new System.Drawing.Size(105, 17);
      this.checkBoxNotifyPlaySound.TabIndex = 9;
      this.checkBoxNotifyPlaySound.Text = "Play \"notify.wav\"";
      this.checkBoxNotifyPlaySound.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(19, 50);
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
      this.txtNotifyBefore.Location = new System.Drawing.Point(164, 47);
      this.txtNotifyBefore.Name = "txtNotifyBefore";
      this.txtNotifyBefore.Size = new System.Drawing.Size(229, 20);
      this.txtNotifyBefore.TabIndex = 7;
      this.txtNotifyBefore.Text = "300";
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.tabControl2);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(464, 419);
      this.tabPage2.TabIndex = 5;
      this.tabPage2.Text = "Guide settings";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // tabControl2
      // 
      this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl2.Controls.Add(this.tabPageGenreMap);
      this.tabControl2.HotTrack = true;
      this.tabControl2.Location = new System.Drawing.Point(6, 6);
      this.tabControl2.Name = "tabControl2";
      this.tabControl2.SelectedIndex = 0;
      this.tabControl2.Size = new System.Drawing.Size(452, 407);
      this.tabControl2.TabIndex = 1;
      // 
      // tabPageGenreMap
      // 
      this.tabPageGenreMap.Controls.Add(this.groupBox4);
      this.tabPageGenreMap.Location = new System.Drawing.Point(4, 22);
      this.tabPageGenreMap.Name = "tabPageGenreMap";
      this.tabPageGenreMap.Size = new System.Drawing.Size(444, 381);
      this.tabPageGenreMap.TabIndex = 1;
      this.tabPageGenreMap.Text = "Genre map";
      this.tabPageGenreMap.UseVisualStyleBackColor = true;
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.mpCheckBoxRatingAsMovie);
      this.groupBox4.Controls.Add(this.listViewGuideGenres);
      this.groupBox4.Controls.Add(this.buttonMapGenres);
      this.groupBox4.Controls.Add(this.buttonUnmapGenres);
      this.groupBox4.Controls.Add(this.listViewProgramGenres);
      this.groupBox4.Controls.Add(this.listViewMappedGenres);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.ForeColor = System.Drawing.SystemColors.ControlText;
      this.groupBox4.Location = new System.Drawing.Point(0, 0);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(441, 378);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      // 
      // mpCheckBoxRatingAsMovie
      // 
      this.mpCheckBoxRatingAsMovie.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpCheckBoxRatingAsMovie.AutoSize = true;
      this.mpCheckBoxRatingAsMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxRatingAsMovie.Location = new System.Drawing.Point(6, 353);
      this.mpCheckBoxRatingAsMovie.Name = "mpCheckBoxRatingAsMovie";
      this.mpCheckBoxRatingAsMovie.Size = new System.Drawing.Size(379, 17);
      this.mpCheckBoxRatingAsMovie.TabIndex = 15;
      this.mpCheckBoxRatingAsMovie.Text = "Automatically add programs with a movie rating to the \"Movie\" genre           ";
      this.mpCheckBoxRatingAsMovie.UseVisualStyleBackColor = true;
      // 
      // listViewGuideGenres
      // 
      this.listViewGuideGenres.AllowDrop = true;
      this.listViewGuideGenres.AllowRowReorder = true;
      this.listViewGuideGenres.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewGuideGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9});
      this.listViewGuideGenres.HideSelection = false;
      this.listViewGuideGenres.Location = new System.Drawing.Point(6, 11);
      this.listViewGuideGenres.Name = "listViewGuideGenres";
      this.listViewGuideGenres.Size = new System.Drawing.Size(429, 123);
      this.listViewGuideGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewGuideGenres.TabIndex = 14;
      this.listViewGuideGenres.UseCompatibleStateImageBehavior = false;
      this.listViewGuideGenres.View = System.Windows.Forms.View.Details;
      this.listViewGuideGenres.SelectedIndexChanged += new System.EventHandler(this.listViewGuideGenres_SelectedIndexChanged);
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "Guide Genre";
      this.columnHeader9.Width = 190;
      // 
      // buttonMapGenres
      // 
      this.buttonMapGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMapGenres.Location = new System.Drawing.Point(202, 228);
      this.buttonMapGenres.MaximumSize = new System.Drawing.Size(36, 22);
      this.buttonMapGenres.MinimumSize = new System.Drawing.Size(36, 22);
      this.buttonMapGenres.Name = "buttonMapGenres";
      this.buttonMapGenres.Size = new System.Drawing.Size(36, 22);
      this.buttonMapGenres.TabIndex = 2;
      this.buttonMapGenres.Text = "<<";
      this.buttonMapGenres.UseVisualStyleBackColor = true;
      this.buttonMapGenres.Click += new System.EventHandler(this.buttonMapGenre_Click);
      // 
      // buttonUnmapGenres
      // 
      this.buttonUnmapGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonUnmapGenres.Location = new System.Drawing.Point(202, 268);
      this.buttonUnmapGenres.MaximumSize = new System.Drawing.Size(36, 22);
      this.buttonUnmapGenres.MinimumSize = new System.Drawing.Size(36, 22);
      this.buttonUnmapGenres.Name = "buttonUnmapGenres";
      this.buttonUnmapGenres.Size = new System.Drawing.Size(36, 22);
      this.buttonUnmapGenres.TabIndex = 1;
      this.buttonUnmapGenres.Text = ">>";
      this.buttonUnmapGenres.UseVisualStyleBackColor = true;
      this.buttonUnmapGenres.Click += new System.EventHandler(this.buttonUnmapGenre_Click);
      // 
      // listViewProgramGenres
      // 
      this.listViewProgramGenres.AllowDrop = true;
      this.listViewProgramGenres.AllowRowReorder = true;
      this.listViewProgramGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewProgramGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12});
      this.listViewProgramGenres.FullRowSelect = true;
      this.listViewProgramGenres.HideSelection = false;
      this.listViewProgramGenres.Location = new System.Drawing.Point(244, 176);
      this.listViewProgramGenres.Name = "listViewProgramGenres";
      this.listViewProgramGenres.Size = new System.Drawing.Size(190, 167);
      this.listViewProgramGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewProgramGenres.TabIndex = 3;
      this.listViewProgramGenres.UseCompatibleStateImageBehavior = false;
      this.listViewProgramGenres.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader12
      // 
      this.columnHeader12.Text = "Unmapped Genres";
      this.columnHeader12.Width = 184;
      // 
      // listViewMappedGenres
      // 
      this.listViewMappedGenres.AllowDrop = true;
      this.listViewMappedGenres.AllowRowReorder = true;
      this.listViewMappedGenres.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewMappedGenres.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader13});
      this.listViewMappedGenres.FullRowSelect = true;
      this.listViewMappedGenres.HideSelection = false;
      this.listViewMappedGenres.Location = new System.Drawing.Point(6, 176);
      this.listViewMappedGenres.Name = "listViewMappedGenres";
      this.listViewMappedGenres.Size = new System.Drawing.Size(190, 167);
      this.listViewMappedGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewMappedGenres.TabIndex = 0;
      this.listViewMappedGenres.UseCompatibleStateImageBehavior = false;
      this.listViewMappedGenres.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader13
      // 
      this.columnHeader13.Text = "Mapped Genres";
      this.columnHeader13.Width = 184;
      // 
      // TVClient
      // 
      this.Controls.Add(this.tabControlTVGeneral);
      this.Name = "TVClient";
      this.Size = new System.Drawing.Size(510, 450);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.tabControlTVGeneral.ResumeLayout(false);
      this.tabPageGeneralSettings.ResumeLayout(false);
      this.mpGroupBox900.ResumeLayout(false);
      this.mpGroupBox900.PerformLayout();
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
      this.tabPage2.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.tabPageGenreMap.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.ResumeLayout(false);

    }

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

    private void mpCheckBoxIsAutoMacAddressEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (mpCheckBoxIsAutoMacAddressEnabled.Checked)
      {
        mpTextBoxMacAddress.Enabled = false;
      }
      else
      {
        mpTextBoxMacAddress.Enabled = true;
      }
    }

    private void mpCheckBoxIsWakeOnLanEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (mpCheckBoxIsWakeOnLanEnabled.Checked)
      {
        mpNumericTextBoxWOLTimeOut.Enabled = true;
        mpCheckBoxIsAutoMacAddressEnabled.Enabled = true;

        if (mpCheckBoxIsAutoMacAddressEnabled.Checked)
        {
          mpTextBoxMacAddress.Enabled = false;
        }
        else
        {
          mpTextBoxMacAddress.Enabled = true;
        }
      }
      else
      {
        mpNumericTextBoxWOLTimeOut.Enabled = false;
        mpCheckBoxIsAutoMacAddressEnabled.Enabled = false;
        mpTextBoxMacAddress.Enabled = false;
      }
    }

    private void listViewGuideGenres_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        PopulateGenreLists();
      }
    }
  
     private void buttonUnmapGenre_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        UnmapProgramGenres();
      }
    }

    private void buttonMapGenre_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        MapProgramGenres();
      }
    }
  }
}