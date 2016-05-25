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
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  #region CustomFileName class

  /// <summary>
  /// Container class for filename handling of tuning details
  /// </summary>
  internal class CustomFileName
  {
    private readonly string _fileName;
    private readonly string _displayName;
    private readonly string _filter;

    public string FileName
    {
      get { return _fileName; }
    }

    public string DisplayName
    {
      get { return _displayName; }
    }

    public CustomFileName(string fileName, string filter)
    {
      _fileName = fileName;
      _displayName = Path.GetFileNameWithoutExtension(fileName);
      if (!string.IsNullOrEmpty(filter))
      {
        _displayName = _displayName.Replace(filter + ".", "");
      }
      _filter = filter;
    }

    public override string ToString()
    {
      return _displayName;
    }

    public override bool Equals(object obj)
    {
      string s = obj as string;
      return s != null && string.Equals(obj, _displayName);
    }

    public override int GetHashCode()
    {
      return (_fileName ?? string.Empty).GetHashCode() ^ (_displayName ?? string.Empty).GetHashCode() ^ (_filter ?? string.Empty).GetHashCode();
    }
  }

  #endregion

  #region FileFilter class

  /// <summary>
  /// Helper class to fill master/detail comboboxes with filtered tuning files
  /// </summary>
  internal class TuningDetailFilter
  {
    private readonly string _folderName;
    private readonly MPComboBox _cbxCountries = null;
    private readonly MPComboBox _cbxRegions = null;
    private string[] _files;

    public static string GetDataPath()
    {
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "TV Server Configuration", "Tuning Details");
    }

    /// <summary>
    /// Load existing list from xml file 
    /// </summary>
    /// <param name="fileName">Path for input file</param>
    public List<TuningDetail> LoadList(string fileName, bool isFullPath = true)
    {
      if (!isFullPath)
      {
        fileName = Path.Combine(GetDataPath(), _folderName, fileName);
      }
      try
      {
        using (XmlReader parFileXml = XmlReader.Create(fileName))
        {
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TuningDetail>));
          object result = xmlSerializer.Deserialize(parFileXml);
          parFileXml.Close();
          return (List<TuningDetail>)result;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error loading tuning details");
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
        return null;
      }
    }

    public void SaveList(string fileName, IList<TuningDetail> tuningDetails)
    {
      string filePath = Path.Combine(GetDataPath(), _folderName, fileName);
      try
      {
        using (XmlWriter parFileXML = XmlWriter.Create(filePath, new XmlWriterSettings() { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine }))
        {
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TuningDetail>));
          xmlSerializer.Serialize(parFileXML, tuningDetails);
          parFileXML.Close();

          if (_cbxCountries == null)
          {
            LoadFiles();
            return;
          }
          object selectedCountry = _cbxCountries.SelectedItem;
          object selectedRegion = _cbxRegions.SelectedItem;
          LoadFiles();
          _cbxCountries.SelectedItem = selectedCountry;
          _cbxRegions.SelectedItem = selectedRegion;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error saving tuning details");
      }
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="tuningType">Tuning type (subfolder name)</param>
    /// <param name="cbxCountries">reference to country cbx</param>
    /// <param name="cbxRegions">reference to region cbx</param>
    public TuningDetailFilter(string tuningType, MPComboBox cbxCountries = null, MPComboBox cbxRegions = null)
    {
      _folderName = tuningType;
      _cbxCountries = cbxCountries;
      _cbxRegions = cbxRegions;
      if (_cbxCountries == null)
      {
        return;
      }

      LoadFiles();
      if (_cbxCountries != null && _cbxRegions != null)
      {
        _cbxRegions.DisplayMember = "DisplayName";
        _cbxCountries.SelectedIndexChanged += SelectedIndexChanged;
        _cbxCountries.SelectedIndex = 0;
      }
    }

    private void LoadFiles()
    {
      if (string.Equals(_folderName, "dvbip"))
      {
        _files = Directory.GetFiles(Path.Combine(GetDataPath(), _folderName), "*.m3u");
      }
      else
      {
        _files = Directory.GetFiles(Path.Combine(GetDataPath(), _folderName), "*.xml");
      }

      // Sort satellites by E/W then longitude.
      if (string.Equals(_folderName, "dvbs"))
      {
        Array.Sort(_files, delegate(string s1, string s2)
        {
          s1 = Path.GetFileNameWithoutExtension(s1);
          s2 = Path.GetFileNameWithoutExtension(s2);
          Match m1 = Regex.Match(s1, @"^(\d+).*?([EW]).*");
          Match m2 = Regex.Match(s2, @"^(\d+).*?([EW]).*");
          if (m1.Success && m2.Success)
          {
            string ew1 = m1.Groups[2].Captures[0].Value;
            string ew2 = m2.Groups[2].Captures[0].Value;
            if (string.Equals(ew1, ew2))
            {
              float f1 = float.Parse(m1.Groups[1].Captures[0].Value);
              float f2 = float.Parse(m2.Groups[1].Captures[0].Value);
              if (f1 < f2)
              {
                return -1;
              }
              if (f1 > f2)
              {
                return 1;
              }
              return string.Compare(s1, s2);
            }
            if (string.Equals(ew1, "E"))
            {
              return -1;
            }
            return 1;
          }
          if (m1.Success)
          {
            return -1;
          }
          if (m2.Success)
          {
            return 1;
          }
          return string.Compare(s1, s2);
        });
      }

      _cbxCountries.BeginUpdate();
      try
      {
        _cbxCountries.Items.Clear();
        if (_cbxRegions == null)
        {
          _cbxCountries.Items.AddRange(FilteredList(_files, null).ToArray());
        }
        else
        {
          _cbxCountries.Items.AddRange(CountryList(_files));
        }
      }
      finally
      {
        _cbxCountries.EndUpdate();
      }
    }

    /// <summary>
    /// Returns a filtered list depending on selected country
    /// 
    /// Naming format:
    ///    country.region.xml
    /// </summary>
    /// <param name="fullList">Complete file list</param>
    /// <param name="countryFilter">Selected country</param>
    /// <returns>List of CustomFileNames</returns>
    private List<CustomFileName> FilteredList(string[] fullList, string countryFilter)
    {
      List<CustomFileName> filtered = new List<CustomFileName>();
      foreach (string singleFile in fullList)
      {
        if (countryFilter == null || Path.GetFileName(singleFile).StartsWith(countryFilter))
        {
          filtered.Add(new CustomFileName(singleFile, countryFilter));
        }
      }
      return filtered;
    }

    /// <summary>
    /// Returns the distinct country names
    /// 
    /// Naming format:
    ///    country.region.xml
    /// </summary>
    /// <param name="fullList">Complete file list</param>
    /// <returns>List of countries</returns>
    private string[] CountryList(string[] fullList)
    {
      SortedSet<string> filtered = new SortedSet<string>();
      foreach (string singleFile in fullList)
      {
        string country = Path.GetFileName(singleFile).Split('.')[0];
        if (!filtered.Contains(country))
        {
          filtered.Add(country);
        }
      }
      string[] toReturn = new string[filtered.Count];
      filtered.CopyTo(toReturn);
      return toReturn;
    }

    /// <summary>
    /// refreshes detail combobox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_cbxRegions != null)
      {
        _cbxRegions.DataSource = FilteredList(_files, _cbxCountries.SelectedItem.ToString());
      }
    }
  }

  #endregion
}