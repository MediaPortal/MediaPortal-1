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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{

  #region CustomFileName class

  /// <summary>
  /// Container class for filename handling of tuning details
  /// </summary>
  internal class CustomFileName
  {
    private readonly String _fileName;
    private readonly String _filter;

    public String FileName
    {
      get { return _fileName; }
    }

    public String DisplayName
    {
      get { return ToString(); }
    }

    public CustomFileName(String fileName, String filter)
    {
      _fileName = fileName;
      _filter = filter;
    }

    /// <summary>
    /// Returns the "region" name of file
    /// </summary>
    /// <returns>String</returns>
    public override String ToString()
    {
      return System.IO.Path.GetFileNameWithoutExtension(_fileName).Replace(_filter + ".", "");
    }
  }

  #endregion

  /// <summary>
  /// Container class for filename handling of tuning details
  /// </summary>
  internal class SimpleFileName
  {
    private readonly String _fileName;

    public String FileName
    {
      get { return _fileName; }
    }

    public String DisplayName
    {
      get { return System.IO.Path.GetFileNameWithoutExtension(_fileName); }
    }

    public SimpleFileName(String fileName)
    {
      _fileName = fileName;
    }

    /// <summary>
    /// Returns the "region" name of file
    /// </summary>
    /// <returns>String</returns>
    public override String ToString()
    {
      return DisplayName;
    }
  }

  #region

  #endregion

  #region FileFilter class

  /// <summary>
  /// Helper class to fill master/detail comboboxes with filtered tuning files
  /// </summary>
  internal class FileFilters
  {
    private readonly String[] _files;
    private readonly MPComboBox _cbxCountries;
    private readonly MPComboBox _cbxRegions;

    /// <summary>
    /// Load existing list from xml file 
    /// </summary>
    /// <param name="fileName">Path for input file</param>
    /// <param name="listType">Type of list to deserialize</param>
    public object LoadList(string fileName, Type listType)
    {
      try
      {
        using (XmlReader parFileXml = XmlReader.Create(fileName))
        {
          XmlSerializer xmlSerializer = new XmlSerializer(listType);
          object result = xmlSerializer.Deserialize(parFileXml);
          parFileXml.Close();
          return result;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error loading tuningdetails");
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
        return null;
      }
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="tuningType">Tuning type (subfolder name)</param>
    /// <param name="cbxCountries">reference to country cbx</param>
    /// <param name="cbxRegions">reference to region cbx</param>
    public FileFilters(String tuningType, ref MPComboBox cbxCountries, ref MPComboBox cbxRegions)
    {
      _cbxCountries = cbxCountries;
      _cbxRegions = cbxRegions;

      _files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\{1}", PathManager.GetDataPath, tuningType), "*.xml");
      List<String> countries = CountryList(_files);
      foreach (string t in countries)
      {
        cbxCountries.Items.Add(t);
      }
      cbxCountries.SelectedIndexChanged += SelectedIndexChanged;
      cbxCountries.SelectedIndex = 0;
      cbxRegions.DisplayMember = "DisplayName";
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="tuningType">Tuning type (subfolder name)</param>
    public FileFilters(String tuningType)
    {
      _files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\{1}", PathManager.GetDataPath, tuningType), "*.xml");
    }

    public List<SimpleFileName> AllFiles
    {
      get
      {
        return _files.Select(file => new SimpleFileName(file)).ToList();
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
    private List<CustomFileName> FilteredList(String[] fullList, String countryFilter)
    {
      List<CustomFileName> filtered = new List<CustomFileName>();
      foreach (String singleFile in fullList)
      {
        if (System.IO.Path.GetFileName(singleFile).StartsWith(countryFilter))
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
    private List<String> CountryList(String[] fullList)
    {
      List<String> filtered = new List<String>();
      foreach (String singleFile in fullList)
      {
        String country = System.IO.Path.GetFileName(singleFile).Split('.')[0];
        if (!filtered.Contains(country))
        {
          filtered.Add(country);
        }
      }
      return filtered;
    }

    /// <summary>
    /// refreshes detail combobox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void SelectedIndexChanged(object sender, EventArgs e)
    {
      _cbxRegions.DataSource = FilteredList(_files, _cbxCountries.SelectedItem.ToString());
    }
  }

  #endregion
}