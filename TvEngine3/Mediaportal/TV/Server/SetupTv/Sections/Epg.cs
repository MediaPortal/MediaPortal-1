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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
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

      // First activation.
      if (_languageItems == null)
      {
        // Languages.
        _languageItems = new List<ListViewItem>();
        foreach (Iso639Language lang in Iso639LanguageCollection.Instance.Languages)
        {
          _languageItems.Add(new ListViewItem(new string[] { lang.Name, lang.TerminologicCode }));
          if (!lang.TerminologicCode.Equals(lang.BibliographicCode))
          {
            _languageItems.Add(new ListViewItem(new string[] { lang.Name, lang.BibliographicCode }));
          }
        }
      }

      // General (all EPG sources including plugins).
      string preferredLanguageCodes = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgPreferredLanguages", string.Empty);
      this.LogDebug("  preferred languages = {0}", preferredLanguageCodes);
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

      textBoxPreferredClassificationSystems.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgPreferredClassificationSystems", string.Empty);
      this.LogDebug("  preferred classification systems = {0}", textBoxPreferredClassificationSystems.Text);
      textBoxPreferredRatingSystems.Text = ServiceAgents.Instance.SettingServiceAgent.GetValue("epgPreferredRatingSystems", string.Empty);
      this.LogDebug("  preferred rating systems = {0}", textBoxPreferredRatingSystems.Text);

      // Tuner EPG grabber.
      checkBoxTunerEpgGrabberTimeShiftingRecordingEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberTimeShiftingRecordingEnabled", true);
      numericUpDownTunerEpgGrabberTimeShiftingRecordingTimeOut.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberTimeShiftingRecordingTimeOut", 30);
      checkBoxTunerEpgGrabberIdleEnable.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberIdleEnabled", true);
      numericUpDownTunerEpgGrabberIdleTimeOut.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberIdleTimeOut", 600);
      numericUpDownTunerEpgGrabberIdleRefresh.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberIdleRefresh", 240);

      bool defaultGrabAtsc = false;
      bool defaultGrabBellExpressVu = false;
      bool defaultGrabDishNetwork = false;
      bool defaultGrabFreesat = false;
      bool defaultGrabMhw1 = false;
      bool defaultGrabMhw2 = false;
      bool defaultGrabMultiChoice = false;
      bool defaultGrabOpenTv = false;
      bool defaultGrabPremiere = false;
      bool defaultGrabScte = false;
      bool defaultGrabViasatSweden = false;
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      if (countryName != null)
      {
        if (countryName.Contains("America"))
        {
          defaultGrabAtsc = true;
          defaultGrabDishNetwork = true;
          defaultGrabScte = true;
        }
        else if (countryName.Equals("Australia"))
        {
          defaultGrabOpenTv = true;   // Foxtel
        }
        else if (countryName.Equals("Canada"))
        {
          defaultGrabAtsc = true;
          defaultGrabBellExpressVu = true;
          defaultGrabScte = true;
        }
        else if (countryName.Equals("France"))
        {
          defaultGrabMhw1 = true;     // Canal Satellite
        }
        else if (countryName.Equals("Germany"))
        {
          defaultGrabPremiere = true;
        }
        else if (countryName.Equals("Italy"))
        {
          defaultGrabOpenTv = true;   // Sky
        }
        else if (countryName.Contains("Netherlands"))
        {
          defaultGrabMhw1 = true;     // Canal Digitaal Satellite
        }
        else if (countryName.Equals("New Zealand"))
        {
          defaultGrabOpenTv = true;   // Sky
        }
        else if (countryName.Equals("Poland"))
        {
          defaultGrabMhw1 = true;     // Cyfra+
        }
        else if (countryName.Equals("Spain"))
        {
          defaultGrabMhw2 = true;     // Digital+
        }
        else if (countryName.Equals("South Africa"))
        {
          defaultGrabMultiChoice = true;
        }
        else if (countryName.Equals("Sweden"))
        {
          defaultGrabViasatSweden = true;
        }
        else if (countryName.Equals("United Kingdom"))
        {
          defaultGrabFreesat = true;
          defaultGrabOpenTv = true;   // Sky
        }
      }

      checkBoxTunerEpgGrabberProtocolAtsc.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolAtsc", defaultGrabAtsc);
      checkBoxTunerEpgGrabberProtocolBellExpressVu.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolBellExpressVu", defaultGrabBellExpressVu);
      checkBoxTunerEpgGrabberProtocolDishNetwork.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolDishNetwork", defaultGrabDishNetwork);
      checkBoxTunerEpgGrabberProtocolDvb.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolDvb", true);
      checkBoxTunerEpgGrabberProtocolFreesat.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolFreesat", defaultGrabFreesat);
      checkBoxTunerEpgGrabberProtocolMhw1.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolMhw1", defaultGrabMhw1);
      checkBoxTunerEpgGrabberProtocolMhw2.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolMhw2", defaultGrabMhw2);
      checkBoxTunerEpgGrabberProtocolMultiChoice.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolMultiChoice", defaultGrabMultiChoice);
      checkBoxTunerEpgGrabberProtocolOpenTv.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolOpenTv", defaultGrabOpenTv);
      checkBoxTunerEpgGrabberProtocolPremiere.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolPremiere", defaultGrabPremiere);
      checkBoxTunerEpgGrabberProtocolScte.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolScte", defaultGrabScte);
      checkBoxTunerEpgGrabberProtocolViasatSweden.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tunerEpgGrabberProtocolViasatSweden", defaultGrabViasatSweden);
      DebugTunerEpgGrabberSettings();

      // Guide categories.
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

      int row = 0;
      foreach (GuideCategory category in guideCategories)
      {
        DataGridViewRow gridRow;
        if (!_guideCategories.TryGetValue(category.IdGuideCategory, out gridRow))
        {
          gridRow = dataGridViewGuideCategories.Rows[row++];
          _guideCategories[category.IdGuideCategory] = gridRow;
        }
        gridRow.Cells["dataGridViewColumnGuideCategoryName"].Value = category.Name;
        gridRow.Cells["dataGridViewColumnGuideCategoryIsEnabled"].Value = category.IsEnabled;
        DataGridViewCell cell = gridRow.Cells["dataGridViewColumnGuideCategoryIsMovieCategory"];
        cell.Value = category.IsMovie;
        cell.ReadOnly = category.IsMovie;
        gridRow.Tag = category;
        _programCategories[category.IdGuideCategory] = new List<ListViewItem>(programCategories.Count);
        programCategoryNamesByGuideCategory[category.IdGuideCategory] = new List<string>(programCategories.Count);
      }
      _handlingGuideCategoryIsMovieChange = false;

      foreach (ProgramCategory category in programCategories)
      {
        ListViewItem item = new ListViewItem(category.Category);
        item.Tag = category;
        _programCategories[category.IdGuideCategory ?? -1].Add(item);
        programCategoryNamesByGuideCategory[category.IdGuideCategory ?? -1].Add(category.Category);
      }
      dataGridViewGuideCategories_SelectionChanged(null, null);

      foreach (GuideCategory category in guideCategories)
      {
        this.LogDebug("    ID = {0}, name = {1}, is enabled = {2}, is movie = {3}, program categories = {4}", category.IdGuideCategory, category.Name, category.IsEnabled, category.IsMovie, string.Join(", ", programCategoryNamesByGuideCategory[category.IdGuideCategory]));
      }
      this.LogDebug("    unmapped program categories = {0}", string.Join(", ", programCategoryNamesByGuideCategory[-1]));

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      this.LogDebug("EPG: deactivating");

      List<string> preferredLanguageCodes = new List<string>();
      foreach (ListViewItem item in listViewLanguagesPreferred.Items)
      {
        preferredLanguageCodes.Add(item.SubItems[1].Text);
      }
      _previousPreferredLanguages = string.Join(",", preferredLanguageCodes);
      this.LogDebug("  preferred languages = {0}", _previousPreferredLanguages);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgPreferredLanguages", _previousPreferredLanguages);

      this.LogDebug("  preferred classification systems = {0}", textBoxPreferredClassificationSystems.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgPreferredClassificationSystems", textBoxPreferredClassificationSystems.Text);
      this.LogDebug("  preferred rating systems = {0}", textBoxPreferredRatingSystems.Text);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("epgPreferredRatingSystems", textBoxPreferredRatingSystems.Text);

      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberTimeShiftingRecordingEnabled", checkBoxTunerEpgGrabberTimeShiftingRecordingEnable.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberTimeShiftingRecordingTimeOut", (int)numericUpDownTunerEpgGrabberTimeShiftingRecordingTimeOut.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberIdleEnabled", checkBoxTunerEpgGrabberIdleEnable.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberIdleTimeOut", (int)numericUpDownTunerEpgGrabberIdleTimeOut.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberIdleRefresh", (int)numericUpDownTunerEpgGrabberIdleRefresh.Value);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolAtsc", checkBoxTunerEpgGrabberProtocolAtsc.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolBellExpressVu", checkBoxTunerEpgGrabberProtocolBellExpressVu.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolDishNetwork", checkBoxTunerEpgGrabberProtocolDishNetwork.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolDvb", checkBoxTunerEpgGrabberProtocolDvb.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolFreesat", checkBoxTunerEpgGrabberProtocolFreesat.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolMhw1", checkBoxTunerEpgGrabberProtocolMhw1.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolMhw2", checkBoxTunerEpgGrabberProtocolMhw2.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolMultiChoice", checkBoxTunerEpgGrabberProtocolMultiChoice.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolOpenTv", checkBoxTunerEpgGrabberProtocolOpenTv.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolPremiere", checkBoxTunerEpgGrabberProtocolPremiere.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolScte", checkBoxTunerEpgGrabberProtocolScte.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tunerEpgGrabberProtocolViasatSweden", checkBoxTunerEpgGrabberProtocolViasatSweden.Checked);
      DebugTunerEpgGrabberSettings();

      foreach (DataGridViewRow gridRow in dataGridViewGuideCategories.Rows)
      {
        bool save = false;
        GuideCategory category = gridRow.Tag as GuideCategory;
        string newName = (string)gridRow.Cells["dataGridViewColumnGuideCategoryName"].Value;
        if (!string.Equals(category.Name, newName))
        {
          this.LogInfo("EPG: guide category {0} name changed from {1} to {2}", category.IdGuideCategory, category.Name, newName);
          category.Name = newName;
          save = true;
        }
        bool newBoolValue = (bool)gridRow.Cells["dataGridViewColumnGuideCategoryIsEnabled"].Value;
        if (category.IsEnabled != newBoolValue)
        {
          this.LogInfo("EPG: guide category {0} enabled changed from {1} to {2}", category.IdGuideCategory, category.IsEnabled, newBoolValue);
          category.IsEnabled = newBoolValue;
          save = true;
        }
        newBoolValue = (bool)gridRow.Cells["dataGridViewColumnGuideCategoryIsMovieCategory"].Value;
        if (category.IsMovie != newBoolValue)
        {
          this.LogInfo("EPG: guide category {0} movie indicator changed from {1} to {2}", category.IdGuideCategory, category.IsMovie, newBoolValue);
          category.IsMovie = newBoolValue;
          save = true;
        }

        if (save)
        {
          category = ServiceAgents.Instance.ProgramCategoryServiceAgent.SaveGuideCategory(category);
          gridRow.Tag = category;
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

    private void DebugTunerEpgGrabberSettings()
    {
      this.LogDebug("  tuner EPG grabber...");
      this.LogDebug("    TS/rec.?          = {0}", checkBoxTunerEpgGrabberTimeShiftingRecordingEnable.Checked);
      this.LogDebug("      time-out        = {0} seconds", numericUpDownTunerEpgGrabberTimeShiftingRecordingTimeOut.Value);
      this.LogDebug("    idle?             = {0}", checkBoxTunerEpgGrabberIdleEnable.Checked);
      this.LogDebug("      time-out        = {0} seconds", numericUpDownTunerEpgGrabberIdleTimeOut.Value);
      this.LogDebug("      refresh         = {0} minutes", numericUpDownTunerEpgGrabberIdleRefresh.Value);
      this.LogDebug("    protocols...");
      this.LogDebug("      ATSC            = {0}", checkBoxTunerEpgGrabberProtocolAtsc.Checked);
      this.LogDebug("      Bell ExpressVu  = {0}", checkBoxTunerEpgGrabberProtocolBellExpressVu.Checked);
      this.LogDebug("      Dish Network    = {0}", checkBoxTunerEpgGrabberProtocolDishNetwork.Checked);
      this.LogDebug("      DVB             = {0}", checkBoxTunerEpgGrabberProtocolDvb.Checked);
      this.LogDebug("      Freesat         = {0}", checkBoxTunerEpgGrabberProtocolFreesat.Checked);
      this.LogDebug("      MediaHighway 1  = {0}", checkBoxTunerEpgGrabberProtocolMhw1.Checked);
      this.LogDebug("      MediaHighway 2  = {0}", checkBoxTunerEpgGrabberProtocolMhw2.Checked);
      this.LogDebug("      MultiChoice     = {0}", checkBoxTunerEpgGrabberProtocolMultiChoice.Checked);
      this.LogDebug("      OpenTV          = {0}", checkBoxTunerEpgGrabberProtocolOpenTv.Checked);
      this.LogDebug("      Premiere        = {0}", checkBoxTunerEpgGrabberProtocolPremiere.Checked);
      this.LogDebug("      SCTE            = {0}", checkBoxTunerEpgGrabberProtocolScte.Checked);
      this.LogDebug("      Viasat Sweden   = {0}", checkBoxTunerEpgGrabberProtocolViasatSweden.Checked);
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
      IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels(ChannelIncludeRelationEnum.None);
      foreach (Channel ch in channels)
      {
        ch.LastGrabTime = Schedule.MinSchedule;
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