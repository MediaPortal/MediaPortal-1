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

namespace MediaPortal.GUI.DatabaseViews
{
  /// <summary>
  /// Summary description for FilterDefinition.
  /// </summary>
  [Serializable]
  public class DatabaseFilterLevel
  {
    protected string selection = "";
    protected bool skipLevel = false;
    protected string sortBy = "Name";
    protected bool sortAscending = true;
    protected string selectedValue = "";
    protected int selectedId = -1;
    protected string defaultView = "List";

    public DatabaseFilterLevel() { }
    
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

    public string SelectedValue
    {
      get { return selectedValue; }
      set { selectedValue = value; }
    }

    public int SelectedId
    {
      get { return selectedId; }
      set { selectedId = value; }
    }
  }
}