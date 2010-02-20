#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Text;
using TvLibrary.Log;
using MediaPortal.UserInterface.Controls;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;

namespace SetupTv.Sections
{

  #region CustomFileName class

  /// <summary>
  /// Container class for filename handling of tuning details
  /// </summary>
  internal class CustomFileName
  {
    private String m_fileName;
    private String m_filter;

    public String FileName
    {
      get { return m_fileName; }
    }

    public String DisplayName
    {
      get { return this.ToString(); }
    }

    public CustomFileName(String fileName, String filter)
    {
      m_fileName = fileName;
      m_filter = filter;
    }

    /// <summary>
    /// Returns the "region" name of file
    /// </summary>
    /// <returns>String</returns>
    public override String ToString()
    {
      return System.IO.Path.GetFileNameWithoutExtension(m_fileName).Replace(m_filter + ".", "");
    }
  }

  #endregion

  /// <summary>
  /// Container class for filename handling of tuning details
  /// </summary>
  internal class SimpleFileName
  {
    private String m_fileName;

    public String FileName
    {
      get { return m_fileName; }
    }

    public String DisplayName
    {
      get { return System.IO.Path.GetFileNameWithoutExtension(m_fileName); }
    }

    public SimpleFileName(String fileName)
    {
      m_fileName = fileName;
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
    private String[] files;
    private MPComboBox m_cbxCountries;
    private MPComboBox m_cbxRegions;

    /// <summary>
    /// Load existing list from xml file 
    /// </summary>
    /// <param name="fileName">Path for input filen</param>
    public object LoadList(string fileName, Type ListType)
    {
      try
      {
        XmlReader parFileXML = XmlReader.Create(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(ListType);
        object result = xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
        return result;
      }
      catch (Exception ex)
      {
        Log.Error("Error loading tuningdetails: {0}", ex.ToString());
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
        return null;
      }
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="TuningType">Tuning type (subfolder name)</param>
    /// <param name="cbxCountries">reference to country cbx</param>
    /// <param name="cbxRegions">reference to region cbx</param>
    public FileFilters(String TuningType, ref MPComboBox cbxCountries, ref MPComboBox cbxRegions)
    {
      m_cbxCountries = cbxCountries;
      m_cbxRegions = cbxRegions;

      files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\{1}", Log.GetPathName(), TuningType),
                                           "*.xml");
      List<String> countries = CountryList(files);
      for (int i = 0; i < countries.Count; ++i)
      {
        cbxCountries.Items.Add(countries[i]);
      }
      cbxCountries.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
      cbxCountries.SelectedIndex = 0;
      cbxRegions.DisplayMember = "DisplayName";
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="TuningType">Tuning type (subfolder name)</param>
    public FileFilters(String TuningType)
    {
      files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\{1}", Log.GetPathName(), TuningType),
                                           "*.xml");
    }

    public List<SimpleFileName> AllFiles
    {
      get
      {
        List<SimpleFileName> list = new List<SimpleFileName>();
        foreach (String file in files)
        {
          list.Add(new SimpleFileName(file));
        }
        return list;
      }
    }

    /// <summary>
    /// Returns a filtered list depending on selected country
    /// 
    /// Naming format:
    ///    country.region.xml
    /// </summary>
    /// <param name="FullList">Complete file list</param>
    /// <param name="CountryFilter">Selected country</param>
    /// <returns>List of CustomFileNames</returns>
    private List<CustomFileName> FilteredList(String[] FullList, String CountryFilter)
    {
      List<CustomFileName> filtered = new List<CustomFileName>();
      foreach (String SingleFile in FullList)
      {
        if (System.IO.Path.GetFileName(SingleFile).StartsWith(CountryFilter))
        {
          filtered.Add(new CustomFileName(SingleFile, CountryFilter));
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
    /// <param name="FullList">Complete file list</param>
    /// <returns>List of countries</returns>
    private List<String> CountryList(String[] FullList)
    {
      List<String> filtered = new List<String>();
      foreach (String SingleFile in FullList)
      {
        String country = System.IO.Path.GetFileName(SingleFile).Split('.')[0];
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
      m_cbxRegions.DataSource = FilteredList(files, m_cbxCountries.SelectedItem.ToString());
    }
  }

  #endregion
}