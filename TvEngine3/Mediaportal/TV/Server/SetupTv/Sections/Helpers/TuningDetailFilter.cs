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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  /// <summary>
  /// This class simplifies handling of tuning detail selection for scanning.
  /// </summary>
  internal class TuningDetailFilter
  {
    private class ComboBoxFileItem
    {
      private readonly string _fileName;
      private readonly string _displayName;
      private readonly string _filter;

      public ComboBoxFileItem(string fileName, string filter)
      {
        _fileName = fileName;
        _displayName = Path.GetFileNameWithoutExtension(fileName);
        if (!string.IsNullOrEmpty(filter))
        {
          _displayName = _displayName.Replace(filter + ".", "");
        }
        _filter = filter;
      }

      public string FileName
      {
        get { return _fileName; }
      }

      public string DisplayName
      {
        get { return _displayName; }
      }

      public override string ToString()
      {
        return _displayName;
      }

      public override bool Equals(object obj)
      {
        ComboBoxFileItem item = obj as ComboBoxFileItem;
        return item != null && string.Equals(item.FileName, _fileName) && string.Equals(item._filter, _filter);
      }

      public override int GetHashCode()
      {
        return _fileName.GetHashCode() ^ (_filter ?? string.Empty).GetHashCode();
      }
    }

    public static readonly string DATA_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "TV Server Configuration", "Tuning Details");
    public const string SATELLITE_SUB_DIRECTORY = "dvbs";
    public const string ALL_TUNING_DETAIL_ITEM = "All";

    private readonly int _tunerId;
    private readonly TuningDetailGroup _group;
    private readonly MPComboBox _comboBoxLevel1 = null;
    private readonly MPComboBox _comboBoxLevel2 = null;
    private readonly MPComboBox _comboBoxLevel3 = null;
    private string[] _files;

    public TuningDetailFilter(int tunerId, TuningDetailGroup group, MPComboBox comboBoxLevel1, string selectedItemLevel1, MPComboBox comboBoxLevel2, string selectedItemLevel2, MPComboBox comboBoxLevel3 = null, string selectedItemLevel3 = null)
    {
      _tunerId = tunerId;
      _group = group;
      _comboBoxLevel1 = comboBoxLevel1;
      _comboBoxLevel2 = comboBoxLevel2;
      _comboBoxLevel3 = comboBoxLevel3;

      _comboBoxLevel1.SelectedIndexChanged += Level1SelectedIndexChanged;
      if (comboBoxLevel3 != null)
      {
        _comboBoxLevel2.DisplayMember = "DisplayName";
        _comboBoxLevel2.SelectedIndexChanged += Level2SelectedIndexChanged;
      }
      else
      {
        _comboBoxLevel1.DisplayMember = "DisplayName";
      }

      LoadFiles();

      Select(_comboBoxLevel1, selectedItemLevel1);
      Select(_comboBoxLevel2, selectedItemLevel2);
      if (_comboBoxLevel3 != null)
      {
        Select(_comboBoxLevel3, selectedItemLevel3);
      }
    }

    public static void Load(int tunerId, TuningDetailGroup group, string fileName, MPComboBox comboBox, bool isFileNameQualified = false)
    {
      if (fileName == null)
      {
        comboBox.Enabled = false;
        comboBox.Items.Clear();
        comboBox.Items.Add(TuningDetailFilter.ALL_TUNING_DETAIL_ITEM);
        comboBox.SelectedIndex = 0;
        return;
      }

      if (!isFileNameQualified)
      {
        fileName = Path.Combine(GetPathForTuningDetailGroup(group), fileName);
      }

      comboBox.BeginUpdate();
      try
      {
        comboBox.Items.Clear();
        comboBox.Items.Add(ALL_TUNING_DETAIL_ITEM);
        if (group == TuningDetailGroup.Stream)
        {
          LoadM3uPlaylist(fileName, comboBox);
          return;
        }

        List<TuningDetail> tuningDetails;
        using (XmlReader xmlReader = XmlReader.Create(fileName))
        {
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TuningDetail>));
          tuningDetails = (List<TuningDetail>)xmlSerializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        foreach (TuningDetail tuningDetail in tuningDetails)
        {
          if (ServiceAgents.Instance.ControllerServiceAgent.CanTune(tunerId, tuningDetail.GetTuningChannel()))
          {
            comboBox.Items.Add(tuningDetail);
          }
        }
        comboBox.SelectedIndex = 0;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "tuning detail filter: failed to load tuning details, file name = {0}", fileName);
        MessageBox.Show("Failed to load tuning details. " + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        comboBox.Enabled = true;
        comboBox.EndUpdate();
      }
    }

    public void Save(string fileName, IList<TuningDetail> tuningDetails)
    {
      string filePath = Path.Combine(GetPathForTuningDetailGroup(_group), fileName);

      // Expand any tuning details that have more than one frequency so that
      // there's one tuning detail per frequency.
      List<TuningDetail> tempTuningDetails = new List<TuningDetail>(tuningDetails.Count);
      foreach (TuningDetail tuningDetail in tuningDetails)
      {
        if (tuningDetail.Frequencies == null || tuningDetail.Frequencies.Count == 0)
        {
          tempTuningDetails.Add(tuningDetail);
          continue;
        }
        foreach (int frequency in tuningDetail.Frequencies)
        {
          TuningDetail tempTuningDetail = (TuningDetail)tuningDetail.Clone();
          tempTuningDetail.Frequency = frequency;
          tempTuningDetails.Add(tempTuningDetail);
        }
      }

      try
      {
        using (XmlWriter xmlWriter = XmlWriter.Create(filePath, new XmlWriterSettings() { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine }))
        {
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TuningDetail>));
          xmlSerializer.Serialize(xmlWriter, tempTuningDetails);
          xmlWriter.Close();

          object selectedItemLevel1 = _comboBoxLevel1.SelectedItem;
          object selectedItemLevel2 = _comboBoxLevel2.SelectedItem;
          object selectedItemLevel3 = null;
          if (_comboBoxLevel3 != null)
          {
            selectedItemLevel3 = _comboBoxLevel3.SelectedItem;
          }
          LoadFiles();
          _comboBoxLevel1.SelectedItem = selectedItemLevel1;
          _comboBoxLevel2.SelectedItem = selectedItemLevel2;
          if (_comboBoxLevel3 != null)
          {
            _comboBoxLevel3.SelectedItem = selectedItemLevel3;
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuning detail filter: failed to save tuning detail list, file name = {0}", fileName);
      }
    }

    public IList<TuningDetail> TuningDetails
    {
      get
      {
        if (_comboBoxLevel3 == null)
        {
          return GetTuningDetails(_comboBoxLevel2);
        }
        return GetTuningDetails(_comboBoxLevel3);
      }
    }

    public static IList<TuningDetail> GetTuningDetails(MPComboBox comboBox)
    {
      if (!string.Equals(comboBox.SelectedItem, ALL_TUNING_DETAIL_ITEM))
      {
        return new List<TuningDetail>(1) { comboBox.SelectedItem as TuningDetail };
      }

      List<TuningDetail> tuningDetails = new List<TuningDetail>(comboBox.Items.Count - 1);
      foreach (object item in comboBox.Items)
      {
        TuningDetail tuningDetail = item as TuningDetail;
        if (tuningDetail != null)
        {
          tuningDetails.Add(tuningDetail);
        }
      }
      return tuningDetails;
    }

    private void LoadFiles()
    {
      string filter = "*.xml";
      if (_group == TuningDetailGroup.Stream)
      {
        filter = "*.m3u";
      }
      _files = Directory.GetFiles(GetPathForTuningDetailGroup(_group), filter);
      Array.Sort(_files);

      _comboBoxLevel1.BeginUpdate();
      try
      {
        _comboBoxLevel1.Items.Clear();
        if (_comboBoxLevel3 == null)
        {
          _comboBoxLevel1.Items.AddRange(Filter(null));
        }
        else
        {
          HashSet<string> distinct = new HashSet<string>();
          foreach (string file in _files)
          {
            string level1Item = Path.GetFileName(file).Split('.')[0];
            if (!distinct.Contains(level1Item))
            {
              distinct.Add(level1Item);
              _comboBoxLevel1.Items.Add(level1Item);
            }
          }
        }
      }
      finally
      {
        _comboBoxLevel1.EndUpdate();
      }
    }

    private static string GetPathForTuningDetailGroup(TuningDetailGroup group)
    {
      if (group == TuningDetailGroup.Analog)
      {
        return Path.Combine(DATA_PATH, "analog");
      }
      if (group == TuningDetailGroup.AtscScte)
      {
        return Path.Combine(DATA_PATH, "atsc");
      }
      if (group == TuningDetailGroup.Cable)
      {
        return Path.Combine(DATA_PATH, "dvbc");
      }
      if (group == TuningDetailGroup.Satellite)
      {
        return Path.Combine(DATA_PATH, SATELLITE_SUB_DIRECTORY);
      }
      if (group == TuningDetailGroup.Stream)
      {
        return Path.Combine(DATA_PATH, "dvbip");
      }
      if (group == TuningDetailGroup.Terrestrial)
      {
        return Path.Combine(DATA_PATH, "dvbt");
      }
      return null;
    }

    private static void LoadM3uPlaylist(string fileName, MPComboBox comboBox)
    {
      Regex regex = new Regex(@"^\s*#EXTINF\s*:\s*(\d+)\s*,\s*([^\s].*?)\s*$");
      TuningDetail tuningDetail = new TuningDetail();
      foreach (string line in File.ReadLines(fileName))
      {
        if (!string.IsNullOrWhiteSpace(line))
        {
          Match m = regex.Match(line);
          if (!m.Success)
          {
            string l = line.Trim();
            if (!l.StartsWith("#"))
            {
              tuningDetail.BroadcastStandard = BroadcastStandard.DvbIp;
              tuningDetail.Url = l;
            }
          }
          else
          {
            if (!string.IsNullOrEmpty(tuningDetail.Url))
            {
              comboBox.Items.Add(tuningDetail);
              tuningDetail = new TuningDetail();
            }
            string lcn = m.Groups[1].Captures[0].Value;
            if (!string.Equals(lcn, "0"))
            {
              tuningDetail.StreamLogicalChannelNumber = lcn;
            }
            tuningDetail.StreamName = m.Groups[2].Captures[0].Value;
          }
        }
      }
      if (!string.IsNullOrEmpty(tuningDetail.Url))
      {
        comboBox.Items.Add(tuningDetail);
      }
    }

    private ComboBoxFileItem[] Filter(string filter)
    {
      List<ComboBoxFileItem> selected = new List<ComboBoxFileItem>(_files.Length);
      foreach (string file in _files)
      {
        if (filter == null || Path.GetFileName(file).StartsWith(filter + "."))
        {
          selected.Add(new ComboBoxFileItem(file, filter));
        }
      }
      return selected.ToArray();
    }

    private static void Select(MPComboBox comboBox, string itemLabel)
    {
      if (itemLabel != null)
      {
        foreach (object item in comboBox.Items)
        {
          if (string.Equals(itemLabel, item.ToString()))
          {
            comboBox.SelectedItem = item;
          }
        }
      }
      if (comboBox.SelectedItem == null)
      {
        comboBox.SelectedIndex = 0;
      }
    }

    private void Level1SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_comboBoxLevel3 == null)
      {
        Load(_tunerId, _group, ((ComboBoxFileItem)_comboBoxLevel1.SelectedItem).FileName, _comboBoxLevel2, true);
        return;
      }

      _comboBoxLevel2.BeginUpdate();
      try
      {
        _comboBoxLevel2.Items.Clear();
        _comboBoxLevel2.Items.AddRange(Filter(_comboBoxLevel1.SelectedItem.ToString()));
        _comboBoxLevel2.SelectedIndex = 0;
      }
      finally
      {
        _comboBoxLevel2.EndUpdate();
      }
    }

    private void Level2SelectedIndexChanged(object sender, EventArgs e)
    {
      Load(_tunerId, _group, ((ComboBoxFileItem)_comboBoxLevel2.SelectedItem).FileName, _comboBoxLevel3, true);
    }
  }
}