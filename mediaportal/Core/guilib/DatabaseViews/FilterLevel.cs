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
using System.Xml.Serialization;

namespace MediaPortal.GUI.View
{
  /// <summary>
  /// Summary description for FilterDefinition.
  /// </summary>
  [Serializable]
  public class FilterLevel
  {
    protected string selection = "";
    protected bool skipLevel = false;
    protected string sortBy = "Name";
    protected bool sortAscending = true;
    protected string selectedValue = "";
    protected string defaultView = "List";
    protected List<FilterDefinitionNew> _listFilters = new List<FilterDefinitionNew>();

    public FilterLevel() {}
    
    [XmlElement("Selection")]
    public string Selection
    {
      get { return selection; }
      set { selection = value; }
    }

    [XmlElement("SortBy")]
    public string SortBy
    {
      get { return sortBy; }
      set { sortBy = value; }
    }

    [XmlElement("SortAscending")]
    public bool SortAscending
    {
      get { return sortAscending; }
      set { sortAscending = value; }
    }

    [XmlElement("DefaultView")]
    public string DefaultView
    {
      get { return defaultView; }
      set { defaultView = value; }
    }

    [XmlElement("SkipLevel")]
    public bool SkipLevel
    {
      get { return skipLevel; }
      set { skipLevel = value; }
    }
   
    [XmlElement("Filters")]
    public List<FilterDefinitionNew> Filters
    {
      get { return _listFilters; }
      set { _listFilters = value; }
    }

    public string SelectedValue
    {
      get { return selectedValue; }
      set { selectedValue = value; }
    }
  }
}