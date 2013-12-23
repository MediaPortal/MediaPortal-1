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
using System.Xml.Serialization;

namespace MediaPortal.GUI.View
{
  /// <summary>
  /// Summary description for FilterDefinition.
  /// </summary>
  [Serializable]
  public class FilterDefinitionNew
  {
    protected bool skipLevel = false;
    protected string tableName = "";
    protected string whereClause = "";
    protected string sqlOperator = "";
    protected string whereValue = "*";
    protected string selectedValue = "";
    protected string defaultView = "List";

    public FilterDefinitionNew() {}

    [XmlElement("TableName")]
    public string TableName
    {
      get { return tableName; }
      set { tableName = value; }
    }

    [XmlElement("Where")]
    public string Where
    {
      get { return whereClause; }
      set { whereClause = value; }
    }

    [XmlElement("SqlOperator")]
    public string SqlOperator
    {
      get { return sqlOperator; }
      set { sqlOperator = value; }
    }

    [XmlElement("WhereValue")]
    public string WhereValue
    {
      get { return whereValue; }
      set { whereValue = value; }
    }

    [XmlElement("SkipLevel")]
    public bool SkipLevel
    {
      get { return skipLevel; }
      set { skipLevel = value; }
    }

    [XmlElement("DefaultView")]
    public string DefaultView
    {
      get { return defaultView; }
      set { defaultView = value; }
    }
    
    public string SelectedValue
    {
      get { return selectedValue; }
      set { selectedValue = value; }
    }
  }
}