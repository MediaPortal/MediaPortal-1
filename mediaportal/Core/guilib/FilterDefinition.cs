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
using System.Xml.Serialization;

namespace MediaPortal.GUI.View
{
  /// <summary>
  /// Summary description for FilterDefinition.
  /// </summary>
  [Serializable]
  public class FilterDefinition
  {
    protected bool distinct = false;
    protected bool sortAscending = true;
    protected string restriction = "";
    protected string whereClause = "";
    protected string fromStatement = "";
    protected string sqloperator = "";
    protected string whereValue = "*";
    protected string selectedValue = "";
    protected string defaultView = "List";
    protected string defaultSort = "Name";
    protected int limit = -1;

    public FilterDefinition() {}

    [XmlElement("distinct")]
    public bool Distinct
    {
      get { return distinct; }
      set { distinct = value; }
    }

    [XmlElement("SortAscending")]
    public bool SortAscending
    {
      get { return sortAscending; }
      set { sortAscending = value; }
    }

    [XmlElement("Restriction")]
    public string Restriction
    {
      get { return restriction; }
      set { restriction = value; }
    }

    [XmlElement("operator")]
    public string SqlOperator
    {
      get { return sqloperator; }
      set { sqloperator = value; }
    }


    [XmlElement("DefaultView")]
    public string DefaultView
    {
      get { return defaultView; }
      set { defaultView = value; }
    }

    [XmlElement("DefaultSort")]
    public string DefaultSort
    {
      get { return defaultSort; }
      set { defaultSort = value; }
    }

    [XmlElement("Where")]
    public string Where
    {
      get { return whereClause; }
      set { whereClause = value; }
    }

    [XmlElement("WhereValue")]
    public string WhereValue
    {
      get { return whereValue; }
      set { whereValue = value; }
    }

    [XmlElement("Limit")]
    public int Limit
    {
      get { return limit; }
      set { limit = value; }
    }

    public string SelectedValue
    {
      get { return selectedValue; }
      set { selectedValue = value; }
    }
  }
}