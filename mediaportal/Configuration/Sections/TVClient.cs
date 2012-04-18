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
    private TabPage tabPage2;
    private MPTabControl tabControl2;
    private MPTabPage tabPageGenreMap;
    private MPGroupBox groupBox4;
    private MPButton buttonRemoveGenre;
    private MPButton buttonNewGenre;
    private MPButton buttonMapGenres;
    private MPButton buttonUnmapGenres;
    private MPListView listViewProgramGenres;
    private ColumnHeader columnHeader12;
    private MPListView listViewMappedGenres;
    private ColumnHeader columnHeader13;
    private MPButton mpButtonOnLaterColor;
    private MPButton mpButtonOnNowColor;

    private bool _SingleSeat;
    private MPListView listViewGuideGenres;
    private ColumnHeader columnHeader9;
    private ColumnHeader columnHeader10;
    private ColumnHeader columnHeader11;
    private string _genreBeforeEdit;

    protected bool _guideColorsLoaded = false;
    protected long _guideColorProgramOnNow = 0;
    protected long _guideColorProgramOnLater = 0;
    protected long _guideColorChannelButton = 0;
    protected long _guideColorChannelButtonSelected = 0;
    protected long _guideColorGroupButton = 0;
    protected long _guideColorGroupButtonSelected = 0;
    protected long _guideColorProgramEnded = 0;
    protected long _guideColorProgramSelected = 0;
    protected long _guideColorBorderHighlight = 0;
    protected IList<string> _allProgramGenres;
    protected List<string> _genreList = new List<string>();
    protected IDictionary<string, string> _genreMap = new Dictionary<string, string>();
    protected IDictionary<string, long> _genreColorsOnNow = new Dictionary<string, long>();
    private TabPage tabGuideOptions;
    private GroupBox groupDefaultColors;
    private GroupBox groupBox3;
    private MPLabel mpLabel11;
    private ColorComboBox colorComboBoxGroupSel;
    private MPLabel mpLabel12;
    private ColorComboBox colorComboBoxGroup;
    private GroupBox groupBox1;
    private MPLabel mpLabel9;
    private ColorComboBox colorComboBoxChannelSel;
    private MPLabel mpLabel10;
    private ColorComboBox colorComboBoxChannel;
    private MPLabel mpLabel8;
    private ColorComboBox colorComboBoxPgmSel;
    private MPLabel mpLabel4;
    private ColorComboBox colorComboBoxPgmEnded;
    private MPCheckBox mpCheckBoxRatingAsMovie;
    private MPLabel mpLabel13;
    private ColorComboBox colorComboBoxPgmBorder;
    private MPLabel mpLabel14;
    private ColorComboBox colorComboBoxPgmOnLater;
    private MPLabel mpLabel15;
    private ColorComboBox colorComboBoxPgmOnNow;
    protected IDictionary<string, long> _genreColorsOnLater = new Dictionary<string, long>();

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
      string genre;
      List<string> programGenres;
      IDictionary<string, string> allGenres = xmlreader.GetSection<string>("genremap");

      // Each genre map entry is a csv list of "program" genre names (those that may be compared with the genre from the program listings).
      // It is an error if a single "program" genre is mapped to more than one genre color category; behavior is undefined for this condition.
      foreach (var genreMapEntry in allGenres)
      {
        genre = genreMapEntry.Key;
        _genreList.Add(genre);
        programGenres = new List<string>(genreMapEntry.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

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
      // Each genre map entry is a csv list of "program" genre names (those that may be compared with the genre from the program listings).
      string programGenreCsv;
      foreach (var genre in _genreList)
      {
        programGenreCsv = "";
        foreach (var genreMapEntry in _genreMap)
        {
          if (genreMapEntry.Value.Equals(genre))
          {
            programGenreCsv += genreMapEntry.Key + ",";
          }
        }

        xmlwriter.SetValue("genremap", genre, programGenreCsv.TrimEnd(','));
      }
    }

    private bool LoadGuideColors(Settings xmlreader)
    {
      List<string> temp;

      // Load supporting guide colors.
      _guideColorChannelButton = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorchannelbutton", "ff0e517b"));
      _guideColorChannelButtonSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorchannelbuttonselected", "Green"));
      _guideColorGroupButton = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorgroupbutton", "ff0e517b"));
      _guideColorGroupButtonSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorgroupbuttonselected", "Green"));
      _guideColorProgramSelected = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorprogramselected", "Green"));
      _guideColorProgramEnded = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorprogramended", "Gray"));
      _guideColorBorderHighlight = GetColorFromString(xmlreader.GetValueAsString("tvguidecolors", "guidecolorborderhighlight", "99ffffff"));

      // Load the default genre colors.
      temp = new List<string>((xmlreader.GetValueAsString("tvguidecolors", "defaultgenre", String.Empty)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
      if (temp.Count == 2)
      {
        _guideColorProgramOnNow = GetColorFromString(temp[0]);
        _guideColorProgramOnLater = GetColorFromString(temp[1]);
      }
      else if (temp.Count == 1)
      {
        _guideColorProgramOnNow = GetColorFromString(temp[0]);
        _guideColorProgramOnLater = _guideColorProgramOnNow;
      }
      else
      {
        _guideColorProgramOnNow = 0xff1d355b; // Dark blue
        _guideColorProgramOnLater = 0xff0e517b; // Light blue
      }

      // Each genre color entry is a csv list.  The first value is the color for program "on now", the second value is for program "on later".
      // If only one value is provided then that value is used for both.
      long color0;

      foreach (string genre in _genreList)
      {
        temp = new List<string>((xmlreader.GetValueAsString("tvguidecolors", genre, String.Empty)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        if (temp.Count > 0)
        {
          color0 = GetColorFromString(temp[0]);
          if (temp.Count == 2)
          {
            _genreColorsOnNow.Add(genre, color0);
            _genreColorsOnLater.Add(genre, GetColorFromString(temp[1]));
          }
          else if (temp.Count == 1)
          {
            _genreColorsOnNow.Add(genre, color0);
            _genreColorsOnLater.Add(genre, color0);
          }
        }
      }

      return _genreColorsOnNow.Count > 0;
    }

    private void SaveGuideColors(Settings xmlwriter)
    {
      // Save supporting guide colors.
      xmlwriter.SetValue("tvguidecolors", "guidecolorchannelbutton", String.Format("{0:X8}", (uint)_guideColorChannelButton));
      xmlwriter.SetValue("tvguidecolors", "guidecolorchannelbuttonselected", String.Format("{0:X8}", (uint)_guideColorChannelButtonSelected));
      xmlwriter.SetValue("tvguidecolors", "guidecolorgroupbutton", String.Format("{0:X8}", (uint)_guideColorGroupButton));
      xmlwriter.SetValue("tvguidecolors", "guidecolorgroupbuttonselected", String.Format("{0:X8}", (uint)_guideColorGroupButtonSelected));
      xmlwriter.SetValue("tvguidecolors", "guidecolorprogramselected", String.Format("{0:X8}", (uint)_guideColorProgramSelected));
      xmlwriter.SetValue("tvguidecolors", "guidecolorprogramended", String.Format("{0:X8}", (uint)_guideColorProgramEnded));
      xmlwriter.SetValue("tvguidecolors", "guidecolorborderhighlight", String.Format("{0:X8}", (uint)_guideColorBorderHighlight));
      xmlwriter.SetValue("tvguidecolors", "defaultgenre", String.Format("{0:X8}", (uint)_guideColorProgramOnNow) + "," + String.Format("{0:X8}", (uint)_guideColorProgramOnLater));

      // Each genre color entry is a csv list.  The first value is the color for program "on now", the second value is for program "on later".
      // If only one value is provided then that value is used for both.
      long onNowColor;
      long onLaterColor;

      foreach (string genre in _genreList)
      {
        _genreColorsOnNow.TryGetValue(genre, out onNowColor);
        _genreColorsOnLater.TryGetValue(genre, out onLaterColor);
        xmlwriter.SetValue("tvguidecolors", genre, String.Format("{0:X8}", (uint)onNowColor) + "," + String.Format("{0:X8}", (uint)onLaterColor));
      }
    }

    private long GetColorFromString(string strColor)
    {
      long result = 0xFFFFFFFF;

      if (long.TryParse(strColor, System.Globalization.NumberStyles.HexNumber, null, out result))
      {
        // Result set in out param
      }
      else if (Color.FromName(strColor).IsKnownColor)
      {
        result = Color.FromName(strColor).ToArgb();
      }

      return result;
    }

    private void PopulateGuideGenreList()
    {
      // Populate the guide genre list with names and colors.
      listViewGuideGenres.BeginUpdate();
      listViewGuideGenres.Items.Clear();

      foreach (string genre in _genreList)
      {
        long lColorOnNow;
        _genreColorsOnNow.TryGetValue(genre, out lColorOnNow);
        string colorOnNow = String.Format("{0:X8}", (int)lColorOnNow);

        long lColorOnLater;
        _genreColorsOnLater.TryGetValue(genre, out lColorOnLater);
        string colorOnLater = String.Format("{0:X8}", (int)lColorOnLater);

        ListViewItem item = new ListViewItem(new string[] { genre, colorOnNow, colorOnLater });
        item.Name = genre;
        item.UseItemStyleForSubItems = false;
        item.SubItems[1].BackColor = Color.FromArgb((int)lColorOnNow);
        item.SubItems[2].BackColor = Color.FromArgb((int)lColorOnLater);

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

        mpCheckBoxRatingAsMovie.Checked = xmlreader.GetValueAsBool("genreoptions", "specifympaaratedasmovie", false);
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

              // Load the genre map from MP settings.
              if (_genreMap.Count == 0)
              {
                using (Settings xmlreader = new MPSettings())
                {
                  LoadGenreMap(xmlreader);
                }
              }

              if (!_guideColorsLoaded)
              {
                using (Settings xmlreader = new MPSettings())
                {
                  _guideColorsLoaded = LoadGuideColors(xmlreader);
                }
              }

              // Populate the guide genre list with names and colors.
              PopulateGuideGenreList();
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
                    if (!_preferredAudioLanguages.Contains(_languagesAvail[i]))
                    {
                      ListViewItem item = new ListViewItem(new string[] {_languagesAvail[i], _languageCodes[i]});
                      item.Name = _languageCodes[i];
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
                    if (!_preferredSubLanguages.Contains(_languagesAvail[i]))
                    {
                      ListViewItem item = new ListViewItem(new string[] {_languagesAvail[i], _languageCodes[i]});
                      item.Name = _languageCodes[i];
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
        SaveGuideColors(xmlwriter);
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
      this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.mpButtonOnLaterColor = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButtonOnNowColor = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRemoveGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonNewGenre = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonMapGenres = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUnmapGenres = new MediaPortal.UserInterface.Controls.MPButton();
      this.listViewProgramGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.listViewMappedGenres = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tabGuideOptions = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.mpLabel11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxGroupSel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxGroup = new MediaPortal.WinCustomControls.ColorComboBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.mpLabel9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxChannelSel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxChannel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.groupDefaultColors = new System.Windows.Forms.GroupBox();
      this.mpLabel14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmOnLater = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmOnNow = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmBorder = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmSel = new MediaPortal.WinCustomControls.ColorComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.colorComboBoxPgmEnded = new MediaPortal.WinCustomControls.ColorComboBox();
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
      this.tabGuideOptions.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupDefaultColors.SuspendLayout();
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
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableTTXTSub);
      this.mpGroupBox4.Controls.Add(this.mpCheckBoxEnableDVBSub);
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(16, 308);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(432, 60);
      this.mpGroupBox4.TabIndex = 10;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Subtitle settings";
      // 
      // mpCheckBoxEnableTTXTSub
      // 
      this.mpCheckBoxEnableTTXTSub.AutoSize = true;
      this.mpCheckBoxEnableTTXTSub.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBoxEnableTTXTSub.Location = new System.Drawing.Point(239, 28);
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
      this.mpCheckBoxEnableDVBSub.Location = new System.Drawing.Point(9, 28);
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
      this.tabControl2.Controls.Add(this.tabGuideOptions);
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
      this.groupBox4.Controls.Add(this.mpButtonOnLaterColor);
      this.groupBox4.Controls.Add(this.mpButtonOnNowColor);
      this.groupBox4.Controls.Add(this.buttonRemoveGenre);
      this.groupBox4.Controls.Add(this.buttonNewGenre);
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
      this.mpCheckBoxRatingAsMovie.Location = new System.Drawing.Point(6, 355);
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
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11});
      this.listViewGuideGenres.HideSelection = false;
      this.listViewGuideGenres.LabelEdit = true;
      this.listViewGuideGenres.Location = new System.Drawing.Point(6, 11);
      this.listViewGuideGenres.Name = "listViewGuideGenres";
      this.listViewGuideGenres.Size = new System.Drawing.Size(429, 130);
      this.listViewGuideGenres.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewGuideGenres.TabIndex = 14;
      this.listViewGuideGenres.UseCompatibleStateImageBehavior = false;
      this.listViewGuideGenres.View = System.Windows.Forms.View.Details;
      this.listViewGuideGenres.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewGuideGenres_AfterLabelEdit);
      this.listViewGuideGenres.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewGuideGenres_BeforeLabelEdit);
      this.listViewGuideGenres.SelectedIndexChanged += new System.EventHandler(this.listViewGuideGenres_SelectedIndexChanged);
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "Guide Genre";
      this.columnHeader9.Width = 190;
      // 
      // columnHeader10
      // 
      this.columnHeader10.Text = "On Now Color";
      this.columnHeader10.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.columnHeader10.Width = 100;
      // 
      // columnHeader11
      // 
      this.columnHeader11.Text = "On Later Color";
      this.columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.columnHeader11.Width = 100;
      // 
      // mpButtonOnLaterColor
      // 
      this.mpButtonOnLaterColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOnLaterColor.Location = new System.Drawing.Point(102, 147);
      this.mpButtonOnLaterColor.Name = "mpButtonOnLaterColor";
      this.mpButtonOnLaterColor.Size = new System.Drawing.Size(90, 22);
      this.mpButtonOnLaterColor.TabIndex = 11;
      this.mpButtonOnLaterColor.Text = "On Later Color";
      this.mpButtonOnLaterColor.UseVisualStyleBackColor = true;
      this.mpButtonOnLaterColor.Click += new System.EventHandler(this.mpButton2_Click);
      // 
      // mpButtonOnNowColor
      // 
      this.mpButtonOnNowColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButtonOnNowColor.Location = new System.Drawing.Point(6, 147);
      this.mpButtonOnNowColor.Name = "mpButtonOnNowColor";
      this.mpButtonOnNowColor.Size = new System.Drawing.Size(90, 22);
      this.mpButtonOnNowColor.TabIndex = 10;
      this.mpButtonOnNowColor.Text = "On Now Color";
      this.mpButtonOnNowColor.UseVisualStyleBackColor = true;
      this.mpButtonOnNowColor.Click += new System.EventHandler(this.mpButton1_Click);
      // 
      // buttonRemoveGenre
      // 
      this.buttonRemoveGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonRemoveGenre.Location = new System.Drawing.Point(375, 147);
      this.buttonRemoveGenre.Name = "buttonRemoveGenre";
      this.buttonRemoveGenre.Size = new System.Drawing.Size(60, 22);
      this.buttonRemoveGenre.TabIndex = 6;
      this.buttonRemoveGenre.Text = "Remove";
      this.buttonRemoveGenre.UseVisualStyleBackColor = true;
      this.buttonRemoveGenre.Click += new System.EventHandler(this.btnDeleteGenre_Click);
      // 
      // buttonNewGenre
      // 
      this.buttonNewGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNewGenre.Location = new System.Drawing.Point(309, 147);
      this.buttonNewGenre.Name = "buttonNewGenre";
      this.buttonNewGenre.Size = new System.Drawing.Size(60, 22);
      this.buttonNewGenre.TabIndex = 5;
      this.buttonNewGenre.Text = "Add";
      this.buttonNewGenre.UseVisualStyleBackColor = true;
      this.buttonNewGenre.Click += new System.EventHandler(this.buttonNewGenre_Click);
      // 
      // buttonMapGenres
      // 
      this.buttonMapGenres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonMapGenres.Location = new System.Drawing.Point(202, 274);
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
      this.buttonUnmapGenres.Location = new System.Drawing.Point(202, 234);
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
      this.listViewProgramGenres.Location = new System.Drawing.Point(244, 182);
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
      this.listViewMappedGenres.Location = new System.Drawing.Point(6, 182);
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
      // tabGuideOptions
      // 
      this.tabGuideOptions.Controls.Add(this.groupBox3);
      this.tabGuideOptions.Controls.Add(this.groupBox1);
      this.tabGuideOptions.Controls.Add(this.groupDefaultColors);
      this.tabGuideOptions.Location = new System.Drawing.Point(4, 22);
      this.tabGuideOptions.Name = "tabGuideOptions";
      this.tabGuideOptions.Padding = new System.Windows.Forms.Padding(3);
      this.tabGuideOptions.Size = new System.Drawing.Size(444, 381);
      this.tabGuideOptions.TabIndex = 2;
      this.tabGuideOptions.Text = "Color options";
      this.tabGuideOptions.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.mpLabel11);
      this.groupBox3.Controls.Add(this.colorComboBoxGroupSel);
      this.groupBox3.Controls.Add(this.mpLabel12);
      this.groupBox3.Controls.Add(this.colorComboBoxGroup);
      this.groupBox3.Location = new System.Drawing.Point(6, 203);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(432, 60);
      this.groupBox3.TabIndex = 12;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Group button color";
      // 
      // mpLabel11
      // 
      this.mpLabel11.AutoSize = true;
      this.mpLabel11.Location = new System.Drawing.Point(214, 27);
      this.mpLabel11.Name = "mpLabel11";
      this.mpLabel11.Size = new System.Drawing.Size(52, 13);
      this.mpLabel11.TabIndex = 11;
      this.mpLabel11.Text = "Selected:";
      // 
      // colorComboBoxGroupSel
      // 
      this.colorComboBoxGroupSel.Extended = false;
      this.colorComboBoxGroupSel.Location = new System.Drawing.Point(274, 22);
      this.colorComboBoxGroupSel.Name = "colorComboBoxGroupSel";
      this.colorComboBoxGroupSel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxGroupSel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxGroupSel.TabIndex = 10;
      this.colorComboBoxGroupSel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnGroupSelColorChanged);
      this.colorComboBoxGroupSel.Load += new System.EventHandler(this.colorComboBoxGroupSel_Load);
      // 
      // mpLabel12
      // 
      this.mpLabel12.AutoSize = true;
      this.mpLabel12.Location = new System.Drawing.Point(45, 27);
      this.mpLabel12.Name = "mpLabel12";
      this.mpLabel12.Size = new System.Drawing.Size(43, 13);
      this.mpLabel12.TabIndex = 9;
      this.mpLabel12.Text = "Normal:";
      // 
      // colorComboBoxGroup
      // 
      this.colorComboBoxGroup.Extended = false;
      this.colorComboBoxGroup.Location = new System.Drawing.Point(97, 22);
      this.colorComboBoxGroup.Name = "colorComboBoxGroup";
      this.colorComboBoxGroup.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxGroup.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxGroup.TabIndex = 0;
      this.colorComboBoxGroup.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnGroupColorChanged);
      this.colorComboBoxGroup.Load += new System.EventHandler(this.colorComboBoxGroup_Load);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.mpLabel9);
      this.groupBox1.Controls.Add(this.colorComboBoxChannelSel);
      this.groupBox1.Controls.Add(this.mpLabel10);
      this.groupBox1.Controls.Add(this.colorComboBoxChannel);
      this.groupBox1.Location = new System.Drawing.Point(6, 137);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 60);
      this.groupBox1.TabIndex = 12;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Channel button color";
      // 
      // mpLabel9
      // 
      this.mpLabel9.AutoSize = true;
      this.mpLabel9.Location = new System.Drawing.Point(214, 27);
      this.mpLabel9.Name = "mpLabel9";
      this.mpLabel9.Size = new System.Drawing.Size(52, 13);
      this.mpLabel9.TabIndex = 11;
      this.mpLabel9.Text = "Selected:";
      // 
      // colorComboBoxChannelSel
      // 
      this.colorComboBoxChannelSel.Extended = false;
      this.colorComboBoxChannelSel.Location = new System.Drawing.Point(274, 22);
      this.colorComboBoxChannelSel.Name = "colorComboBoxChannelSel";
      this.colorComboBoxChannelSel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxChannelSel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxChannelSel.TabIndex = 10;
      this.colorComboBoxChannelSel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnChannelSelColorChanged);
      this.colorComboBoxChannelSel.Load += new System.EventHandler(this.colorComboBoxChannelSel_Load);
      // 
      // mpLabel10
      // 
      this.mpLabel10.AutoSize = true;
      this.mpLabel10.Location = new System.Drawing.Point(45, 27);
      this.mpLabel10.Name = "mpLabel10";
      this.mpLabel10.Size = new System.Drawing.Size(43, 13);
      this.mpLabel10.TabIndex = 9;
      this.mpLabel10.Text = "Normal:";
      // 
      // colorComboBoxChannel
      // 
      this.colorComboBoxChannel.Extended = false;
      this.colorComboBoxChannel.Location = new System.Drawing.Point(97, 22);
      this.colorComboBoxChannel.Name = "colorComboBoxChannel";
      this.colorComboBoxChannel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxChannel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxChannel.TabIndex = 0;
      this.colorComboBoxChannel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnChannelColorChanged);
      this.colorComboBoxChannel.Load += new System.EventHandler(this.colorComboBoxChannel_Load);
      // 
      // groupDefaultColors
      // 
      this.groupDefaultColors.Controls.Add(this.mpLabel14);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmOnLater);
      this.groupDefaultColors.Controls.Add(this.mpLabel15);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmOnNow);
      this.groupDefaultColors.Controls.Add(this.mpLabel13);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmBorder);
      this.groupDefaultColors.Controls.Add(this.mpLabel8);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmSel);
      this.groupDefaultColors.Controls.Add(this.mpLabel4);
      this.groupDefaultColors.Controls.Add(this.colorComboBoxPgmEnded);
      this.groupDefaultColors.Location = new System.Drawing.Point(6, 12);
      this.groupDefaultColors.Name = "groupDefaultColors";
      this.groupDefaultColors.Size = new System.Drawing.Size(432, 120);
      this.groupDefaultColors.TabIndex = 0;
      this.groupDefaultColors.TabStop = false;
      this.groupDefaultColors.Text = "Program button color";
      // 
      // mpLabel14
      // 
      this.mpLabel14.AutoSize = true;
      this.mpLabel14.Location = new System.Drawing.Point(219, 28);
      this.mpLabel14.Name = "mpLabel14";
      this.mpLabel14.Size = new System.Drawing.Size(47, 13);
      this.mpLabel14.TabIndex = 17;
      this.mpLabel14.Text = "On later:";
      // 
      // colorComboBoxPgmOnLater
      // 
      this.colorComboBoxPgmOnLater.Extended = false;
      this.colorComboBoxPgmOnLater.Location = new System.Drawing.Point(274, 23);
      this.colorComboBoxPgmOnLater.Name = "colorComboBoxPgmOnLater";
      this.colorComboBoxPgmOnLater.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmOnLater.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmOnLater.TabIndex = 16;
      this.colorComboBoxPgmOnLater.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmOnLaterColorChanged);
      this.colorComboBoxPgmOnLater.Load += new System.EventHandler(this.colorComboBoxPgmOnLater_Load);
      // 
      // mpLabel15
      // 
      this.mpLabel15.AutoSize = true;
      this.mpLabel15.Location = new System.Drawing.Point(41, 28);
      this.mpLabel15.Name = "mpLabel15";
      this.mpLabel15.Size = new System.Drawing.Size(47, 13);
      this.mpLabel15.TabIndex = 15;
      this.mpLabel15.Text = "On now:";
      // 
      // colorComboBoxPgmOnNow
      // 
      this.colorComboBoxPgmOnNow.Extended = false;
      this.colorComboBoxPgmOnNow.Location = new System.Drawing.Point(97, 23);
      this.colorComboBoxPgmOnNow.Name = "colorComboBoxPgmOnNow";
      this.colorComboBoxPgmOnNow.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmOnNow.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmOnNow.TabIndex = 14;
      this.colorComboBoxPgmOnNow.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmOnNowColorChanged);
      this.colorComboBoxPgmOnNow.Load += new System.EventHandler(this.colorComboBoxPgmOnNow_Load);
      // 
      // mpLabel13
      // 
      this.mpLabel13.AutoSize = true;
      this.mpLabel13.Location = new System.Drawing.Point(5, 86);
      this.mpLabel13.Name = "mpLabel13";
      this.mpLabel13.Size = new System.Drawing.Size(83, 13);
      this.mpLabel13.TabIndex = 13;
      this.mpLabel13.Text = "Border highlight:";
      // 
      // colorComboBoxPgmBorder
      // 
      this.colorComboBoxPgmBorder.Extended = false;
      this.colorComboBoxPgmBorder.Location = new System.Drawing.Point(97, 81);
      this.colorComboBoxPgmBorder.Name = "colorComboBoxPgmBorder";
      this.colorComboBoxPgmBorder.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmBorder.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmBorder.TabIndex = 12;
      this.colorComboBoxPgmBorder.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmBorderColorChanged);
      this.colorComboBoxPgmBorder.Load += new System.EventHandler(this.colorComboBoxPgmBorder_Load);
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(214, 57);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(52, 13);
      this.mpLabel8.TabIndex = 11;
      this.mpLabel8.Text = "Selected:";
      // 
      // colorComboBoxPgmSel
      // 
      this.colorComboBoxPgmSel.Extended = false;
      this.colorComboBoxPgmSel.Location = new System.Drawing.Point(274, 52);
      this.colorComboBoxPgmSel.Name = "colorComboBoxPgmSel";
      this.colorComboBoxPgmSel.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmSel.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmSel.TabIndex = 10;
      this.colorComboBoxPgmSel.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmSelColorChanged);
      this.colorComboBoxPgmSel.Load += new System.EventHandler(this.colorComboBoxPgmSel_Load);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(47, 57);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(41, 13);
      this.mpLabel4.TabIndex = 9;
      this.mpLabel4.Text = "Ended:";
      // 
      // colorComboBoxPgmEnded
      // 
      this.colorComboBoxPgmEnded.Extended = false;
      this.colorComboBoxPgmEnded.Location = new System.Drawing.Point(97, 52);
      this.colorComboBoxPgmEnded.Name = "colorComboBoxPgmEnded";
      this.colorComboBoxPgmEnded.SelectedColor = System.Drawing.Color.Black;
      this.colorComboBoxPgmEnded.Size = new System.Drawing.Size(103, 23);
      this.colorComboBoxPgmEnded.TabIndex = 0;
      this.colorComboBoxPgmEnded.ColorChanged += new MediaPortal.WinCustomControls.ColorChangedHandler(this.OnPgmEndedColorChanged);
      this.colorComboBoxPgmEnded.Load += new System.EventHandler(this.colorComboBoxPgmEnded_Load);
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
      this.tabGuideOptions.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupDefaultColors.ResumeLayout(false);
      this.groupDefaultColors.PerformLayout();
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

    private void mpButton1_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        ColorChooser dlg = new ColorChooser();
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.Color = listViewGuideGenres.SelectedItems[0].SubItems[1].BackColor;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          // Update the color map.
          _genreColorsOnNow[listViewGuideGenres.SelectedItems[0].Text] = dlg.Color.ToArgb();

          // Update the control.
          listViewGuideGenres.SelectedItems[0].SubItems[1].BackColor = dlg.Color;
          listViewGuideGenres.SelectedItems[0].SubItems[1].Text = String.Format("{0:X8}", dlg.Color.ToArgb());
        }
      }
    }

    private void mpButton2_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        ColorChooser dlg = new ColorChooser();
        dlg.StartPosition = FormStartPosition.CenterParent;
        dlg.Color = listViewGuideGenres.SelectedItems[0].SubItems[2].BackColor;
        if (dlg.ShowDialog() == DialogResult.OK)
        {
          // Update the color map.
          _genreColorsOnLater[listViewGuideGenres.SelectedItems[0].Text] = dlg.Color.ToArgb();

          // Update the control.
          listViewGuideGenres.SelectedItems[0].SubItems[2].BackColor = dlg.Color;
          listViewGuideGenres.SelectedItems[0].SubItems[2].Text = String.Format("{0:X8}", dlg.Color.ToArgb());
        }
      }
    }

    private void listViewGuideGenres_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        PopulateGenreLists();
      }
    }

    private void listViewGuideGenres_BeforeLabelEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
    {
      _genreBeforeEdit = listViewGuideGenres.SelectedItems[0].Text;
    }

    private void listViewGuideGenres_AfterLabelEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
    {
      // Don't create an empty string genre.
      if (e.Label == null || e.Label.Trim().Length == 0)
      {
        return;
      }

      // Rename the guide genre and update genre and color map entries that match the pre-edit name.
      _genreList.Remove(_genreBeforeEdit);
      _genreList.Add(e.Label);

      // Genre map.
      Dictionary<string, string> genreMapCopy = new Dictionary<string, string>(_genreMap);
      foreach (var genre in _genreMap)
      {
        if (genre.Value.Equals(_genreBeforeEdit))
        {
          genreMapCopy[genre.Key] = e.Label;
        }
      }
      _genreMap = genreMapCopy;

      // On now color map.
      Dictionary<string, long> genreColorsOnNowCopy = new Dictionary<string, long>(_genreColorsOnNow);
      foreach (var genre in _genreColorsOnNow)
      {
        if (genre.Key.Equals(_genreBeforeEdit))
        {
          genreColorsOnNowCopy.Add(e.Label, genreColorsOnNowCopy[genre.Key]);
          genreColorsOnNowCopy.Remove(genre.Key);
        }
      }
      _genreColorsOnNow = genreColorsOnNowCopy;

      // On later color map.
      Dictionary<string, long> genreColorsOnLaterCopy = new Dictionary<string, long>(_genreColorsOnLater);
      foreach (var genre in _genreColorsOnLater)
      {
        if (genre.Key.Equals(_genreBeforeEdit))
        {
          genreColorsOnLaterCopy.Add(e.Label, genreColorsOnLaterCopy[genre.Key]);
          genreColorsOnLaterCopy.Remove(genre.Key);
        }
      }
      _genreColorsOnLater = genreColorsOnLaterCopy;
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

    private void btnDeleteGenre_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        Dictionary<string, string> genreMapCopy = new Dictionary<string, string>(_genreMap);
        foreach (ListViewItem genre in listViewGuideGenres.SelectedItems)
        {
          _genreList.Remove(genre.Text);
          listViewGuideGenres.Items.Remove(genre);

          // Remove entries from the genre map.
          foreach (var genreMapEntry in _genreMap)
          {
            if (genre.Text.Equals(genreMapEntry.Value))
            {
              genreMapCopy.Remove(genreMapEntry.Key);
            }
          }
        }
        _genreMap = genreMapCopy;

        listViewMappedGenres.Items.Clear();
        listViewProgramGenres.Items.Clear();
      }
    }

    private void buttonNewGenre_Click(object sender, EventArgs e)
    {
      DlgAddGenre dlg = new DlgAddGenre();

      if (dlg.ShowDialog() == DialogResult.OK)
      {
        // Don't create an empty string genre.
        if (dlg.Value != null && dlg.Value.Trim().Length != 0)
        {
          _genreList.Add(dlg.Value);
          PopulateGuideGenreList();
        }
      }
    }

    private void colorComboBoxPgmEnded_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmEnded.SelectedColor = Color.FromArgb((int)_guideColorProgramEnded);
    }

    protected void OnPgmEndedColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramEnded = e.color.ToArgb();
    }

    private void colorComboBoxPgmSel_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmSel.SelectedColor = Color.FromArgb((int)_guideColorProgramSelected);
    }

    protected void OnPgmSelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramSelected = e.color.ToArgb();
    }

    private void colorComboBoxPgmBorder_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmBorder.SelectedColor = Color.FromArgb((int)_guideColorBorderHighlight);
    }

    protected void OnPgmBorderColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorBorderHighlight = e.color.ToArgb();
    }

    private void colorComboBoxChannel_Load(object sender, EventArgs e)
    {
      colorComboBoxChannel.SelectedColor = Color.FromArgb((int)_guideColorChannelButton);
    }

    protected void OnChannelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorChannelButton = e.color.ToArgb();
    }

    private void colorComboBoxChannelSel_Load(object sender, EventArgs e)
    {
      colorComboBoxChannelSel.SelectedColor = Color.FromArgb((int)_guideColorChannelButtonSelected);
    }

    protected void OnChannelSelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorChannelButtonSelected = e.color.ToArgb();
    }

    private void colorComboBoxGroup_Load(object sender, EventArgs e)
    {
      colorComboBoxGroup.SelectedColor = Color.FromArgb((int)_guideColorGroupButton);
    }

    protected void OnGroupColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorGroupButton = e.color.ToArgb();
    }

    private void colorComboBoxGroupSel_Load(object sender, EventArgs e)
    {
      colorComboBoxGroupSel.SelectedColor = Color.FromArgb((int)_guideColorGroupButtonSelected);
    }

    protected void OnGroupSelColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorGroupButtonSelected = e.color.ToArgb();
    }

    private void colorComboBoxPgmOnNow_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmOnNow.SelectedColor = Color.FromArgb((int)_guideColorProgramOnNow);
    }

    protected void OnPgmOnNowColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramOnNow = e.color.ToArgb();
    }

    private void colorComboBoxPgmOnLater_Load(object sender, EventArgs e)
    {
      colorComboBoxPgmOnLater.SelectedColor = Color.FromArgb((int)_guideColorProgramOnLater);
    }

    protected void OnPgmOnLaterColorChanged(object sender, ColorChangeArgs e)
    {
      _guideColorProgramOnLater = e.color.ToArgb();
    }
  }
}