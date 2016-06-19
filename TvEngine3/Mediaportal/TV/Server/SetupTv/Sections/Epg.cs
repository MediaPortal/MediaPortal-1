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

using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.Localisation;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Epg : SectionSettings
  {
    private List<ListViewItem> _languageItems = null;
    private string _previousPreferredLanguages = null;
    private SortOrder _previousPreferredLanguageSortOrder = SortOrder.None;

    private MPListViewStringColumnSorter _listViewColumnSorterLanguagesAvailable = null;
    private MPListViewStringColumnSorter _listViewColumnSorterLanguagesPreferred = null;
    private MPListViewStringColumnSorter _listViewColumnSorterProgramCategoriesUnmapped = null;
    private MPListViewStringColumnSorter _listViewColumnSorterProgramCategoriesMapped = null;

    private TunerEpgGrabberProtocol _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.None;

    private bool _handlingGuideCategoryIsMovieChange = false;
    private IDictionary<int, DataGridViewRow> _guideCategories = null;
    private IDictionary<int, List<ListViewItem>> _programCategories = null;
    private GuideCategory _previousGuideCategory = null;

    public Epg()
      : base("EPG")
    {
      InitializeComponent();

      _listViewColumnSorterLanguagesAvailable = new MPListViewStringColumnSorter();
      _listViewColumnSorterLanguagesAvailable.Order = SortOrder.Ascending;
      listViewLanguagesAvailable.ListViewItemSorter = _listViewColumnSorterLanguagesAvailable;
      _listViewColumnSorterLanguagesPreferred = new MPListViewStringColumnSorter();
      _listViewColumnSorterLanguagesPreferred.Order = SortOrder.None;
      listViewLanguagesPreferred.ListViewItemSorter = _listViewColumnSorterLanguagesPreferred;

      _listViewColumnSorterProgramCategoriesUnmapped = new MPListViewStringColumnSorter();
      _listViewColumnSorterProgramCategoriesUnmapped.Order = SortOrder.Ascending;
      listViewProgramCategoriesUnmapped.ListViewItemSorter = _listViewColumnSorterProgramCategoriesUnmapped;
      _listViewColumnSorterProgramCategoriesMapped = new MPListViewStringColumnSorter();
      _listViewColumnSorterProgramCategoriesMapped.Order = SortOrder.Ascending;
      listViewProgramCategoriesMapped.ListViewItemSorter = _listViewColumnSorterProgramCategoriesMapped;
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("EPG: activating");

      // first activation
      if (_languageItems == null)
      {
        _languageItems = new List<ListViewItem>();
        foreach (Iso639Language lang in Iso639LanguageCollection.Instance.Languages)
        {
          _languageItems.Add(new ListViewItem(new string[] { lang.Name, lang.TerminologicCode }));
          if (!lang.TerminologicCode.Equals(lang.BibliographicCode))
          {
            _languageItems.Add(new ListViewItem(new string[] { lang.Name, lang.BibliographicCode }));
          }
        }

        _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.None;
        string countryName = RegionInfo.CurrentRegion.EnglishName;
        if (countryName != null)
        {
          if (countryName.Equals("Australia"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.OpenTv;           // Foxtel
          }
          else if (countryName.Equals("Canada"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.AtscEit | TunerEpgGrabberProtocol.BellTv | TunerEpgGrabberProtocol.ScteAeit;
          }
          else if (countryName.Equals("France"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.MediaHighway1;    // Canalsat
          }
          else if (countryName.Equals("Germany"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.Premiere;
          }
          else if (countryName.Equals("Italy"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.OpenTv;           // Sky
          }
          else if (countryName.Equals("Netherlands, The"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.MediaHighway1;    // Canal Digitaal
          }
          else if (countryName.Equals("New Zealand"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.OpenTv;           // Sky
          }
          else if (countryName.Equals("Poland"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.MediaHighway1;    // Cyfra+
          }
          else if (countryName.Equals("Spain"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.MediaHighway2;    // Canal+/Digital+
          }
          else if (countryName.Equals("South Africa"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.MultiChoice;
          }
          else if (countryName.Equals("Sweden"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.ViasatSweden;
          }
          else if (countryName.Equals("United Kingdom"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.Freesat | TunerEpgGrabberProtocol.OpenTv;   // Sky
          }
          else if (countryName.Equals("United States"))
          {
            _defaultEpgGrabberProtocols = TunerEpgGrabberProtocol.AtscEit | TunerEpgGrabberProtocol.DishNetwork | TunerEpgGrabberProtocol.ScteAeit;
          }
        }
        _defaultEpgGrabberProtocols |= TunerEpgGrabberProtocol.DvbEit;

        listViewTunerEpgGrabberFormatsAndProtocols.BeginUpdate();
        try
        {
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Clear();
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("DVB")).Tag = TunerEpgGrabberProtocol.DvbEit;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("OpenTV [AU, IT, NZ, UK]")).Tag = TunerEpgGrabberProtocol.OpenTv;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("MediaHighway 1 [FR, NL, PL]")).Tag = TunerEpgGrabberProtocol.MediaHighway1;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("MediaHighway 2 [ES]")).Tag = TunerEpgGrabberProtocol.MediaHighway2;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("Freesat [UK]")).Tag = TunerEpgGrabberProtocol.Freesat;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("MultiChoice DStv [ZA]")).Tag = TunerEpgGrabberProtocol.MultiChoice;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("Sky Germany")).Tag = TunerEpgGrabberProtocol.Premiere;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("Viasat Sweden")).Tag = TunerEpgGrabberProtocol.ViasatSweden;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("ATSC [CA, US]")).Tag = TunerEpgGrabberProtocol.AtscEit;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("SCTE [CA, US]")).Tag = TunerEpgGrabberProtocol.ScteAeit;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("Bell TV [CA]")).Tag = TunerEpgGrabberProtocol.BellTv;
          listViewTunerEpgGrabberFormatsAndProtocols.Items.Add(new ListViewItem("Dish Network [US]")).Tag = TunerEpgGrabberProtocol.DishNetwork;
        }
        finally
        {
          listViewTunerEpgGrabberFormatsAndProtocols.EndUpdate();
        }
      }

      // general (all EPG sources including plugins)
      string preferredLanguageCodes = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgPreferredLanguages", string.Empty);
      this.LogDebug("  languages              = [{0}]", string.Join(", ", preferredLanguageCodes.Split('|')));
      if (!string.Equals(_previousPreferredLanguages, preferredLanguageCodes))
      {
        listViewLanguagesAvailable.BeginUpdate();
        listViewLanguagesPreferred.BeginUpdate();
        SortOrder previousSortOrder = _listViewColumnSorterLanguagesAvailable.Order;
        _listViewColumnSorterLanguagesAvailable.Order = SortOrder.None;
        try
        {
          listViewLanguagesAvailable.Items.Clear();
          listViewLanguagesPreferred.Items.Clear();

          SortedDictionary<int, ListViewItem> preferredLanguageItems = new SortedDictionary<int, ListViewItem>();
          foreach (ListViewItem item in _languageItems)
          {
            int index = preferredLanguageCodes.IndexOf(item.SubItems[1].Text);
            if (index >= 0)
            {
              preferredLanguageItems.Add(index, item);
            }
            else
            {
              listViewLanguagesAvailable.Items.Add(item);
            }
          }

          foreach (ListViewItem item in preferredLanguageItems.Values)
          {
            listViewLanguagesPreferred.Items.Add(item);
          }

          _listViewColumnSorterLanguagesAvailable.Order = previousSortOrder;
          listViewLanguagesAvailable.Sort();
        }
        finally
        {
          listViewLanguagesAvailable.EndUpdate();
          listViewLanguagesPreferred.EndUpdate();
        }
      }

      string[] tempArray = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgPreferredClassificationSystems", string.Empty).Split('|');
      textBoxPreferredClassificationSystems.Text = string.Join(", ", tempArray);
      this.LogDebug("  classification systems = [{0}]", textBoxPreferredClassificationSystems.Text);
      tempArray = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgPreferredRatingSystems", string.Empty).Split('|');
      textBoxPreferredRatingSystems.Text = string.Join(", ", tempArray);
      this.LogDebug("  rating systems         = [{0}]", textBoxPreferredRatingSystems.Text);

      // tuner EPG grabber
      checkBoxTunerEpgGrabberTimeShiftingRecordingEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberTimeShiftingRecordingEnabled", true);
      numericUpDownTunerEpgGrabberTimeShiftingRecordingTimeLimit.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberTimeShiftingRecordingTimeLimit", 30);
      checkBoxTunerEpgGrabberIdleEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberIdleEnabled", true);
      numericUpDownTunerEpgGrabberIdleTimeLimit.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberIdleTimeLimit", 600);
      numericUpDownTunerEpgGrabberIdleRefresh.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberIdleRefresh", 240);

      TunerEpgGrabberProtocol protocols = (TunerEpgGrabberProtocol)ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocols", (int)_defaultEpgGrabberProtocols);
      foreach (ListViewItem item in listViewTunerEpgGrabberFormatsAndProtocols.Items)
      {
        if (protocols.HasFlag((TunerEpgGrabberProtocol)item.Tag))
        {
          item.Checked = true;
        }
      }

      dataGridViewTunerEpgGrabberTransmitters.Rows.Clear();
      IList<TuningDetail> tuningDetails = ServiceAgents.Instance.ChannelServiceAgent.ListAllDigitalTransmitterTuningDetails();
      dataGridViewTunerEpgGrabberTransmitters.Rows.Add(tuningDetails.Count);
      int rowIndex = 0;
      foreach (TuningDetail tuningDetail in tuningDetails)
      {
        DataGridViewRow row = dataGridViewTunerEpgGrabberTransmitters.Rows[rowIndex++];
        row.Tag = tuningDetail;
        row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterIsEnabled"].Value = tuningDetail.GrabEpg;
        row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterTuningDetail"].Value = tuningDetail.GetDescriptiveString();
        row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterChannels"].Value = tuningDetail.Name;
        row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterLastGrabTime"].Value = tuningDetail.LastEpgGrabTime.ToString("yyyy-MM-dd HH:mm:ss");
      }

      // guide categories
      this.LogDebug("  guide categories...");
      _handlingGuideCategoryIsMovieChange = true;
      IList<GuideCategory> guideCategories = ServiceAgents.Instance.ProgramCategoryServiceAgent.ListAllGuideCategories();
      IList<ProgramCategory> programCategories = ServiceAgents.Instance.ProgramCategoryServiceAgent.ListAllProgramCategories();

      if (dataGridViewGuideCategories.Rows.Count == 0)
      {
        dataGridViewGuideCategories.Rows.Add(guideCategories.Count);
        _guideCategories = new Dictionary<int, DataGridViewRow>(guideCategories.Count);
      }
      _programCategories = new Dictionary<int, List<ListViewItem>>(guideCategories.Count + 1);
      _programCategories[-1] = new List<ListViewItem>(programCategories.Count);
      IDictionary<int, IList<string>> programCategoryNamesByGuideCategory = new Dictionary<int, IList<string>>(guideCategories.Count + 1);
      programCategoryNamesByGuideCategory[-1] = new List<string>(programCategories.Count);

      rowIndex = 0;
      foreach (GuideCategory category in guideCategories)
      {
        DataGridViewRow row;
        if (!_guideCategories.TryGetValue(category.IdGuideCategory, out row))
        {
          row = dataGridViewGuideCategories.Rows[rowIndex++];
          _guideCategories[category.IdGuideCategory] = row;
        }
        row.Cells["dataGridViewColumnGuideCategoryName"].Value = category.Name;
        row.Cells["dataGridViewColumnGuideCategoryIsEnabled"].Value = category.IsEnabled;
        DataGridViewCell cell = row.Cells["dataGridViewColumnGuideCategoryIsMovieCategory"];
        cell.Value = category.IsMovie;
        cell.ReadOnly = category.IsMovie;
        row.Tag = category;
        _programCategories[category.IdGuideCategory] = new List<ListViewItem>(programCategories.Count);
        programCategoryNamesByGuideCategory[category.IdGuideCategory] = new List<string>(programCategories.Count);
      }
      _handlingGuideCategoryIsMovieChange = false;

      foreach (ProgramCategory category in programCategories)
      {
        ListViewItem item = new ListViewItem(category.Category);
        item.Tag = category;
        _programCategories[category.IdGuideCategory.GetValueOrDefault(-1)].Add(item);
        programCategoryNamesByGuideCategory[category.IdGuideCategory.GetValueOrDefault(-1)].Add(category.Category);
      }
      dataGridViewGuideCategories_SelectionChanged(null, null);

      foreach (GuideCategory category in guideCategories)
      {
        this.LogDebug("    ID = {0}, name = {1}, is enabled = {2}, is movie = {3}, program categories = {4}", category.IdGuideCategory, category.Name, category.IsEnabled, category.IsMovie, string.Join(", ", programCategoryNamesByGuideCategory[category.IdGuideCategory]));
      }
      this.LogDebug("    unmapped program categories = {0}", string.Join(", ", programCategoryNamesByGuideCategory[-1]));

      DebugTunerEpgGrabberSettings(protocols, true);

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("EPG: deactivating");

      // general
      List<string> preferredLanguageCodes = new List<string>();
      foreach (ListViewItem item in listViewLanguagesPreferred.Items)
      {
        preferredLanguageCodes.Add(item.SubItems[1].Text);
      }
      this.LogDebug("  languages              = [{0}]", string.Join(", ", preferredLanguageCodes));
      _previousPreferredLanguages = string.Join("|", preferredLanguageCodes);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgPreferredLanguages", _previousPreferredLanguages);
      HashSet<string> systems = new HashSet<string>(Regex.Split(textBoxPreferredClassificationSystems.Text.Trim(), @"\s*,\s*"));
      this.LogDebug("  classification systems = [{0}]", string.Join(", ", systems));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgPreferredClassificationSystems", string.Join("|", systems));
      systems = new HashSet<string>(Regex.Split(textBoxPreferredRatingSystems.Text.Trim(), @"\s*,\s*"));
      this.LogDebug("  rating systems         = [{0}]", string.Join(", ", systems));
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgPreferredRatingSystems", string.Join("|", systems));

      // tuner EPG grabber
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberTimeShiftingRecordingEnabled", checkBoxTunerEpgGrabberTimeShiftingRecordingEnable.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberTimeShiftingRecordingTimeLimit", (int)numericUpDownTunerEpgGrabberTimeShiftingRecordingTimeLimit.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberIdleEnabled", checkBoxTunerEpgGrabberIdleEnable.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberIdleTimeLimit", (int)numericUpDownTunerEpgGrabberIdleTimeLimit.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberIdleRefresh", (int)numericUpDownTunerEpgGrabberIdleRefresh.Value);

      TunerEpgGrabberProtocol protocols = TunerEpgGrabberProtocol.None;
      foreach (ListViewItem item in listViewTunerEpgGrabberFormatsAndProtocols.Items)
      {
        if (item.Checked)
        {
          protocols |= (TunerEpgGrabberProtocol)item.Tag;
        }
      }
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocols", (int)protocols);
      DebugTunerEpgGrabberSettings(protocols, false);

      foreach (DataGridViewRow row in dataGridViewTunerEpgGrabberTransmitters.Rows)
      {
        bool isEnabled = (bool)row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterIsEnabled"].Value;
        TuningDetail tuningDetail = row.Tag as TuningDetail;
        if (tuningDetail.GrabEpg != isEnabled)
        {
          this.LogInfo("EPG: transmitter [{0}] grab EPG flag changed to {1}", tuningDetail.GetDescriptiveString(), isEnabled);
          tuningDetail.GrabEpg = isEnabled;
          ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetailEpgInfo(tuningDetail);
        }
      }

      // guide categories
      foreach (DataGridViewRow row in dataGridViewGuideCategories.Rows)
      {
        bool save = false;
        GuideCategory category = row.Tag as GuideCategory;
        string newName = (string)row.Cells["dataGridViewColumnGuideCategoryName"].Value;
        if (!string.Equals(category.Name, newName))
        {
          this.LogInfo("EPG: guide category {0} name changed from {1} to {2}", category.IdGuideCategory, category.Name, newName);
          category.Name = newName;
          save = true;
        }
        bool newBoolValue = (bool)row.Cells["dataGridViewColumnGuideCategoryIsEnabled"].Value;
        if (category.IsEnabled != newBoolValue)
        {
          this.LogInfo("EPG: guide category {0} enabled changed from {1} to {2}", category.IdGuideCategory, category.IsEnabled, newBoolValue);
          category.IsEnabled = newBoolValue;
          save = true;
        }
        newBoolValue = (bool)row.Cells["dataGridViewColumnGuideCategoryIsMovieCategory"].Value;
        if (category.IsMovie != newBoolValue)
        {
          this.LogInfo("EPG: guide category {0} movie indicator changed from {1} to {2}", category.IdGuideCategory, category.IsMovie, newBoolValue);
          category.IsMovie = newBoolValue;
          save = true;
        }

        if (save)
        {
          category = ServiceAgents.Instance.ProgramCategoryServiceAgent.SaveGuideCategory(category);
          row.Tag = category;
        }
      }

      IList<ProgramCategory> programCategoriesToSave = new List<ProgramCategory>();
      foreach (KeyValuePair<int, List<ListViewItem>> programCategoriesForGuideCategory in _programCategories)
      {
        foreach (ListViewItem item in programCategoriesForGuideCategory.Value)
        {
          ProgramCategory category = item.Tag as ProgramCategory;
          if (!category.IdGuideCategory.HasValue && programCategoriesForGuideCategory.Key != -1)
          {
            this.LogInfo("EPG: program category {0} guide category changed from [none] to {1}", category.IdProgramCategory, programCategoriesForGuideCategory.Key);
            category.IdGuideCategory = programCategoriesForGuideCategory.Key;
            programCategoriesToSave.Add(category);
          }
          else if (category.IdGuideCategory.HasValue && category.IdGuideCategory.Value != programCategoriesForGuideCategory.Key)
          {
            if (programCategoriesForGuideCategory.Key == -1)
            {
              this.LogInfo("EPG: program category {0} guide category changed from {1} to [none]", category.IdProgramCategory, category.IdGuideCategory.Value);
              category.IdGuideCategory = null;
            }
            else
            {
              this.LogInfo("EPG: program category {0} guide category changed from {1} to {2}", category.IdProgramCategory, category.IdGuideCategory.Value, programCategoriesForGuideCategory.Key);
              category.IdGuideCategory = programCategoriesForGuideCategory.Key;
            }
            programCategoriesToSave.Add(category);
          }
        }
      }
      if (programCategoriesToSave.Count > 0)
      {
        ServiceAgents.Instance.ProgramCategoryServiceAgent.SaveProgramCategories(programCategoriesToSave);
      }

      base.OnSectionDeActivated();
    }

    private void DebugTunerEpgGrabberSettings(TunerEpgGrabberProtocol protocols, bool doTransmitters)
    {
      this.LogDebug("  tuner EPG grabber...");
      this.LogDebug("    TS/rec.?             = {0}", checkBoxTunerEpgGrabberTimeShiftingRecordingEnable.Checked);
      this.LogDebug("      time limit         = {0} s", numericUpDownTunerEpgGrabberTimeShiftingRecordingTimeLimit.Value);
      this.LogDebug("    idle?                = {0}", checkBoxTunerEpgGrabberIdleEnable.Checked);
      this.LogDebug("      time limit         = {0} s", numericUpDownTunerEpgGrabberIdleTimeLimit.Value);
      this.LogDebug("      refresh            = {0} m", numericUpDownTunerEpgGrabberIdleRefresh.Value);
      this.LogDebug("    protocol(s)          = [{0}]", protocols);

      if (doTransmitters)
      {
        ThreadPool.QueueUserWorkItem(delegate
        {
          this.LogDebug("    transmitters...");
          foreach (DataGridViewRow row in dataGridViewTunerEpgGrabberTransmitters.Rows)
          {
            bool grabEpg = (bool)row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterIsEnabled"].Value;
            string lastGrabTime = (string)row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterLastGrabTime"].Value;
            string description = (string)row.Cells["dataGridViewColumnTunerEpgGrabberTransmitterTuningDetail"].Value;
            this.LogDebug("      {0, 5} [{1}]: {2}", grabEpg, lastGrabTime, description);
          }
        });
      }
    }

    private void buttonDeleteAll_Click(object sender, System.EventArgs e)
    {
      this.LogDebug("EPG: delete all EPG data");
      ServiceAgents.Instance.ProgramServiceAgent.DeleteAllPrograms();
    }

    private void buttonTunerEpgGrabberRefreshNow_Click(object sender, System.EventArgs e)
    {
      this.LogDebug("EPG: refresh EPG data");
      ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;

      // TODO this is far from optimal; we should update a single grabber setting
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels(ChannelRelation.None);
      foreach (Channel ch in channels)
      {
        ch.LastGrabTime = SqlDateTime.MinValue.Value;
      }
      ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);

      ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
    }

    private bool IsValidDragDropData(DragEventArgs e, MPListView expectListView)
    {
      MPListView listView = e.Data.GetData(typeof(MPListView)) as MPListView;
      if (listView == null || listView != expectListView)
      {
        e.Effect = DragDropEffects.None;
        return false;
      }
      e.Effect = DragDropEffects.Move;
      return true;
    }

    #region preferred languages

    #region button and mouse event handlers

    private void buttonPreferredLanguageAdd_Click(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection selectedItems = listViewLanguagesAvailable.SelectedItems;
      if (selectedItems == null || selectedItems.Count == 0)
      {
        return;
      }

      listViewLanguagesAvailable.BeginUpdate();
      listViewLanguagesAvailable.ListViewItemSorter = null;
      listViewLanguagesPreferred.BeginUpdate();
      listViewLanguagesPreferred.ListViewItemSorter = null;
      try
      {
        ListViewItem[] items = new ListViewItem[selectedItems.Count];
        int i = 0;
        foreach (ListViewItem item in selectedItems)
        {
          listViewLanguagesAvailable.Items.Remove(item);
          items[i++] = item;
        }
        listViewLanguagesPreferred.Items.AddRange(items);
      }
      finally
      {
        listViewLanguagesAvailable.ListViewItemSorter = _listViewColumnSorterLanguagesAvailable;
        listViewLanguagesAvailable.EndUpdate();
        listViewLanguagesPreferred.ListViewItemSorter = _listViewColumnSorterLanguagesPreferred;
        listViewLanguagesPreferred.Sort();
        listViewLanguagesPreferred.EndUpdate();
      }

      listViewLanguagesPreferred.Focus();
    }

    private void buttonPreferredLanguageRemove_Click(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection selectedItems = listViewLanguagesPreferred.SelectedItems;
      if (selectedItems == null || selectedItems.Count == 0)
      {
        return;
      }

      listViewLanguagesAvailable.BeginUpdate();
      listViewLanguagesAvailable.ListViewItemSorter = null;
      listViewLanguagesPreferred.BeginUpdate();
      listViewLanguagesPreferred.ListViewItemSorter = null;
      try
      {
        ListViewItem[] items = new ListViewItem[selectedItems.Count];
        int i = 0;
        foreach (ListViewItem item in selectedItems)
        {
          listViewLanguagesPreferred.Items.Remove(item);
          items[i++] = item;
        }
        listViewLanguagesAvailable.Items.AddRange(items);
      }
      finally
      {
        listViewLanguagesAvailable.ListViewItemSorter = _listViewColumnSorterLanguagesAvailable;
        listViewLanguagesAvailable.Sort();
        listViewLanguagesAvailable.EndUpdate();
        listViewLanguagesPreferred.ListViewItemSorter = _listViewColumnSorterLanguagesPreferred;
        listViewLanguagesPreferred.EndUpdate();
      }

      listViewLanguagesAvailable.Focus();
    }

    private void buttonPreferredLanguagePriorityUp_Click(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewLanguagesPreferred.SelectedItems;
      if (items == null || listViewLanguagesPreferred.Items.Count < 2)
      {
        return;
      }
      listViewLanguagesPreferred.BeginUpdate();
      try
      {
        foreach (ListViewItem item in items)
        {
          int index = item.Index;
          if (index > 0)
          {
            listViewLanguagesPreferred.Items.RemoveAt(index);
            listViewLanguagesPreferred.Items.Insert(index - 1, item);
          }
        }
      }
      finally
      {
        listViewLanguagesPreferred.EndUpdate();
      }

      listViewLanguagesPreferred.Focus();
    }

    private void buttonPreferredLanguagePriorityDown_Click(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewLanguagesPreferred.SelectedItems;
      if (items == null || listViewLanguagesPreferred.Items.Count < 2)
      {
        return;
      }
      listViewLanguagesPreferred.BeginUpdate();
      try
      {
        for (int i = items.Count - 1; i >= 0; i--)
        {
          ListViewItem item = items[i];
          int index = item.Index;
          if (index + 1 < listViewLanguagesPreferred.Items.Count)
          {
            listViewLanguagesPreferred.Items.RemoveAt(index);
            listViewLanguagesPreferred.Items.Insert(index + 1, item);
          }
        }
      }
      finally
      {
        listViewLanguagesPreferred.EndUpdate();
      }

      listViewLanguagesPreferred.Focus();
    }

    private void listViewLanguagesAvailable_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == _listViewColumnSorterLanguagesAvailable.SortColumn)
      {
        _listViewColumnSorterLanguagesAvailable.Order = _listViewColumnSorterLanguagesAvailable.Order == SortOrder.Ascending
                                                        ? SortOrder.Descending
                                                        : SortOrder.Ascending;
      }
      else
      {
        _listViewColumnSorterLanguagesAvailable.SortColumn = e.Column;
        _listViewColumnSorterLanguagesAvailable.Order = SortOrder.Ascending;
      }

      listViewLanguagesAvailable.Sort();
    }

    private void listViewLanguagesPreferred_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == _listViewColumnSorterLanguagesPreferred.SortColumn)
      {
        _listViewColumnSorterLanguagesPreferred.Order = _previousPreferredLanguageSortOrder == SortOrder.Ascending
                                                        ? SortOrder.Descending
                                                        : SortOrder.Ascending;
      }
      else
      {
        _listViewColumnSorterLanguagesPreferred.SortColumn = e.Column;
        _listViewColumnSorterLanguagesPreferred.Order = SortOrder.Ascending;
      }

      listViewLanguagesPreferred.Sort();
      _previousPreferredLanguageSortOrder = _listViewColumnSorterLanguagesPreferred.Order;
      _listViewColumnSorterLanguagesPreferred.Order = SortOrder.None;
    }

    private void listViewLanguagesAvailable_DoubleClick(object sender, System.EventArgs e)
    {
      buttonPreferredLanguageAdd_Click(null, null);
    }

    private void listViewLanguagesPreferred_DoubleClick(object sender, System.EventArgs e)
    {
      buttonPreferredLanguageRemove_Click(null, null);
    }

    #endregion

    #region drag and drop

    private void listViewLanguages_ItemDrag(object sender, ItemDragEventArgs e)
    {
      listViewLanguagesAvailable.DoDragDrop(listViewLanguagesAvailable, DragDropEffects.Move);
    }

    private void listViewLanguages_DragOver(object sender, DragEventArgs e)
    {
      if (IsValidDragDropData(e, listViewLanguagesPreferred))
      {
        listViewLanguagesAvailable.DefaultDragOverHandler(e);
      }
    }

    private void listViewLanguages_DragEnter(object sender, DragEventArgs e)
    {
      IsValidDragDropData(e, listViewLanguagesPreferred);
    }

    private void listViewLanguages_DragDrop(object sender, DragEventArgs e)
    {
      if (!IsValidDragDropData(e, listViewLanguagesPreferred))
      {
        // (Unsupported drag/drop source.)
        return;
      }

      // Determine where we're going to insert the dragged item(s).
      Point cp = listViewLanguagesAvailable.PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = listViewLanguagesAvailable.GetItemAt(cp.X, cp.Y);
      int dropIndex;
      if (dragToItem == null)
      {
        if (listViewLanguagesAvailable.Items.Count != 0)
        {
          return;
        }
        dropIndex = 0;
      }
      else
      {
        // Always drop below as there is no space to draw the insert line above the first item.
        dropIndex = dragToItem.Index;
        dropIndex++;
      }

      listViewLanguagesAvailable.BeginUpdate();
      listViewLanguagesPreferred.BeginUpdate();
      try
      {
        // Move the items.
        foreach (ListViewItem item in listViewLanguagesPreferred.SelectedItems)
        {
          listViewLanguagesPreferred.Items.RemoveAt(item.Index);
          listViewLanguagesAvailable.Items.Insert(dropIndex++, item);
        }
      }
      finally
      {
        listViewLanguagesAvailable.EndUpdate();
        listViewLanguagesPreferred.EndUpdate();
      }
    }

    private void listViewLanguagesPreferred_ItemDrag(object sender, ItemDragEventArgs e)
    {
      listViewLanguagesPreferred.DoDragDrop(listViewLanguagesPreferred, DragDropEffects.Move);
    }

    private void listViewLanguagesPreferred_DragOver(object sender, DragEventArgs e)
    {
      if (IsValidDragDropData(e, listViewLanguagesAvailable) || IsValidDragDropData(e, listViewLanguagesPreferred))
      {
        listViewLanguagesPreferred.DefaultDragOverHandler(e);
      }
    }

    private void listViewLanguagesPreferred_DragEnter(object sender, DragEventArgs e)
    {
      if (!IsValidDragDropData(e, listViewLanguagesAvailable))
      {
        IsValidDragDropData(e, listViewLanguagesPreferred);
      }
    }

    private void listViewLanguagesPreferred_DragDrop(object sender, DragEventArgs e)
    {
      if (IsValidDragDropData(e, listViewLanguagesPreferred))
      {
        // Prioritising (row ordering).
        listViewLanguagesPreferred.DefaultDragDropHandler(e);
        return;
      }
      else if (!IsValidDragDropData(e, listViewLanguagesAvailable))
      {
        // (Unsupported drag/drop source.)
        return;
      }

      // Determine where we're going to insert the dragged item(s).
      Point cp = listViewLanguagesPreferred.PointToClient(new Point(e.X, e.Y));
      ListViewItem dragToItem = listViewLanguagesPreferred.GetItemAt(cp.X, cp.Y);
      int dropIndex;
      if (dragToItem == null)
      {
        if (listViewLanguagesPreferred.Items.Count != 0)
        {
          return;
        }
        dropIndex = 0;
      }
      else
      {
        // Always drop below as there is no space to draw the insert line above the first item.
        dropIndex = dragToItem.Index;
        dropIndex++;
      }

      listViewLanguagesAvailable.BeginUpdate();
      listViewLanguagesPreferred.BeginUpdate();
      try
      {
        // Move the items.
        foreach (ListViewItem item in listViewLanguagesAvailable.SelectedItems)
        {
          listViewLanguagesAvailable.Items.RemoveAt(item.Index);
          listViewLanguagesPreferred.Items.Insert(dropIndex++, item);
        }
      }
      finally
      {
        listViewLanguagesAvailable.EndUpdate();
        listViewLanguagesPreferred.EndUpdate();
      }
    }

    #endregion

    #endregion

    #region guide categories

    private void dataGridViewGuideCategories_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
      // Ensure that one and only one row is always ticked in the movie column.
      if (e.RowIndex >= 0 && e.ColumnIndex == 2 && !_handlingGuideCategoryIsMovieChange)
      {
        _handlingGuideCategoryIsMovieChange = true;
        foreach (DataGridViewRow gridRow in dataGridViewGuideCategories.Rows)
        {
          DataGridViewCell cell = gridRow.Cells[e.ColumnIndex];
          if (gridRow.Index != e.RowIndex)
          {
            if ((bool)cell.Value)
            {
              cell.Value = false;
              cell.ReadOnly = false;
            }
          }
          else
          {
            cell.ReadOnly = true;
          }
        }
        _handlingGuideCategoryIsMovieChange = false;
      }
    }

    private void dataGridViewGuideCategories_SelectionChanged(object sender, System.EventArgs e)
    {
      if (_handlingGuideCategoryIsMovieChange)
      {
        return;
      }
      listViewProgramCategoriesUnmapped.BeginUpdate();
      listViewProgramCategoriesUnmapped.ListViewItemSorter = null;
      listViewProgramCategoriesMapped.BeginUpdate();
      listViewProgramCategoriesMapped.ListViewItemSorter = null;
      try
      {
        if (_previousGuideCategory != null)
        {
          List<ListViewItem> items = _programCategories[-1];
          items.Clear();
          foreach (ListViewItem item in listViewProgramCategoriesUnmapped.Items)
          {
            items.Add(item);
          }

          items = _programCategories[_previousGuideCategory.IdGuideCategory];
          items.Clear();
          foreach (ListViewItem item in listViewProgramCategoriesMapped.Items)
          {
            items.Add(item);
          }
        }

        listViewProgramCategoriesUnmapped.Items.Clear();
        listViewProgramCategoriesMapped.Items.Clear();
        if (dataGridViewGuideCategories.SelectedRows == null || dataGridViewGuideCategories.SelectedRows.Count != 1)
        {
          _previousGuideCategory = null;
          return;
        }

        _previousGuideCategory = dataGridViewGuideCategories.SelectedRows[0].Tag as GuideCategory;
        listViewProgramCategoriesUnmapped.Items.AddRange(_programCategories[-1].ToArray());
        listViewProgramCategoriesMapped.Items.AddRange(_programCategories[_previousGuideCategory.IdGuideCategory].ToArray());
      }
      finally
      {
        listViewProgramCategoriesUnmapped.ListViewItemSorter = _listViewColumnSorterProgramCategoriesUnmapped;
        listViewProgramCategoriesUnmapped.Sort();
        listViewProgramCategoriesUnmapped.EndUpdate();
        listViewProgramCategoriesMapped.ListViewItemSorter = _listViewColumnSorterProgramCategoriesMapped;
        listViewProgramCategoriesMapped.Sort();
        listViewProgramCategoriesMapped.EndUpdate();
      }
    }

    #region button and mouse event handlers

    private void buttonProgramCategoryMap_Click(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection selectedItems = listViewProgramCategoriesUnmapped.SelectedItems;
      if (selectedItems == null || selectedItems.Count == 0)
      {
        return;
      }

      listViewProgramCategoriesUnmapped.BeginUpdate();
      listViewProgramCategoriesUnmapped.ListViewItemSorter = null;
      listViewProgramCategoriesMapped.BeginUpdate();
      listViewProgramCategoriesMapped.ListViewItemSorter = null;
      try
      {
        ListViewItem[] items = new ListViewItem[selectedItems.Count];
        int i = 0;
        foreach (ListViewItem item in selectedItems)
        {
          listViewProgramCategoriesUnmapped.Items.Remove(item);
          items[i++] = item;
        }
        listViewProgramCategoriesMapped.Items.AddRange(items);
      }
      finally
      {
        listViewProgramCategoriesUnmapped.ListViewItemSorter = _listViewColumnSorterLanguagesAvailable;
        listViewProgramCategoriesUnmapped.EndUpdate();
        listViewProgramCategoriesMapped.ListViewItemSorter = _listViewColumnSorterLanguagesPreferred;
        listViewProgramCategoriesMapped.Sort();
        listViewProgramCategoriesMapped.EndUpdate();
      }

      listViewProgramCategoriesMapped.Focus();
    }

    private void buttonProgramCategoryUnmap_Click(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection selectedItems = listViewProgramCategoriesMapped.SelectedItems;
      if (selectedItems == null || selectedItems.Count == 0)
      {
        return;
      }

      listViewProgramCategoriesUnmapped.BeginUpdate();
      listViewProgramCategoriesUnmapped.ListViewItemSorter = null;
      listViewProgramCategoriesMapped.BeginUpdate();
      listViewProgramCategoriesMapped.ListViewItemSorter = null;
      try
      {
        ListViewItem[] items = new ListViewItem[selectedItems.Count];
        int i = 0;
        foreach (ListViewItem item in selectedItems)
        {
          listViewProgramCategoriesMapped.Items.Remove(item);
          items[i++] = item;
        }
        listViewProgramCategoriesUnmapped.Items.AddRange(items);
      }
      finally
      {
        listViewProgramCategoriesUnmapped.ListViewItemSorter = _listViewColumnSorterProgramCategoriesUnmapped;
        listViewProgramCategoriesUnmapped.Sort();
        listViewProgramCategoriesUnmapped.EndUpdate();
        listViewProgramCategoriesMapped.ListViewItemSorter = _listViewColumnSorterProgramCategoriesMapped;
        listViewProgramCategoriesMapped.EndUpdate();
      }

      listViewProgramCategoriesUnmapped.Focus();
    }

    private void listViewProgramCategoriesUnmapped_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == _listViewColumnSorterProgramCategoriesUnmapped.SortColumn)
      {
        _listViewColumnSorterProgramCategoriesUnmapped.Order = _listViewColumnSorterProgramCategoriesUnmapped.Order == SortOrder.Ascending
                                                        ? SortOrder.Descending
                                                        : SortOrder.Ascending;
      }
      else
      {
        _listViewColumnSorterProgramCategoriesUnmapped.SortColumn = e.Column;
        _listViewColumnSorterProgramCategoriesUnmapped.Order = SortOrder.Ascending;
      }

      listViewProgramCategoriesUnmapped.Sort();
    }

    private void listViewProgramCategoriesMapped_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      if (e.Column == _listViewColumnSorterProgramCategoriesMapped.SortColumn)
      {
        _listViewColumnSorterProgramCategoriesMapped.Order = _listViewColumnSorterProgramCategoriesMapped.Order == SortOrder.Ascending
                                                        ? SortOrder.Descending
                                                        : SortOrder.Ascending;
      }
      else
      {
        _listViewColumnSorterProgramCategoriesMapped.SortColumn = e.Column;
        _listViewColumnSorterProgramCategoriesMapped.Order = SortOrder.Ascending;
      }

      listViewProgramCategoriesMapped.Sort();
    }

    private void listViewProgramCategoriesUnmapped_DoubleClick(object sender, System.EventArgs e)
    {
      buttonProgramCategoryMap_Click(null, null);
    }

    private void listViewProgramCategoriesMapped_DoubleClick(object sender, System.EventArgs e)
    {
      buttonProgramCategoryUnmap_Click(null, null);
    }

    #endregion

    #region drag and drop

    private void listViewProgramCategoriesUnmapped_ItemDrag(object sender, ItemDragEventArgs e)
    {
      listViewProgramCategoriesUnmapped.DoDragDrop(listViewProgramCategoriesUnmapped, DragDropEffects.Move);
    }

    private void listViewProgramCategoriesUnmapped_DragOver(object sender, DragEventArgs e)
    {
      IsValidDragDropData(e, listViewProgramCategoriesMapped);
    }

    private void listViewProgramCategoriesUnmapped_DragEnter(object sender, DragEventArgs e)
    {
      IsValidDragDropData(e, listViewProgramCategoriesMapped);
    }

    private void listViewProgramCategoriesUnmapped_DragDrop(object sender, DragEventArgs e)
    {
      if (IsValidDragDropData(e, listViewProgramCategoriesMapped))
      {
        buttonProgramCategoryUnmap_Click(null, null);
      }
    }

    private void listViewProgramCategoriesMapped_ItemDrag(object sender, ItemDragEventArgs e)
    {
      listViewProgramCategoriesMapped.DoDragDrop(listViewProgramCategoriesMapped, DragDropEffects.Move);
    }

    private void listViewProgramCategoriesMapped_DragOver(object sender, DragEventArgs e)
    {
      IsValidDragDropData(e, listViewProgramCategoriesUnmapped);
    }

    private void listViewProgramCategoriesMapped_DragEnter(object sender, DragEventArgs e)
    {
      IsValidDragDropData(e, listViewProgramCategoriesUnmapped);
    }

    private void listViewProgramCategoriesMapped_DragDrop(object sender, DragEventArgs e)
    {
      if (IsValidDragDropData(e, listViewProgramCategoriesUnmapped))
      {
        buttonProgramCategoryMap_Click(null, null);
      }
    }

    #endregion

    #endregion
  }
}