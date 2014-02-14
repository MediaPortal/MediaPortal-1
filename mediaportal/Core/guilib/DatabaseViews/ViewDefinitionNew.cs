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
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.View
{
  /// <summary>
  /// Summary description for ViewDefinition.
  /// </summary>
  [Serializable]
  public class ViewDefinitionNew : ICloneable
  {
    #region Variables

    protected List<FilterLevel> _listFilterLevels = new List<FilterLevel>();
    protected List<ViewDefinitionNew> _listSubViews = new List<ViewDefinitionNew>();
    protected List<FilterDefinitionNew> _listFilters = new List<FilterDefinitionNew>();
    private string _name;
    private string _parent;

    #endregion

    #region ctor
    public ViewDefinitionNew() {}
    #endregion

    /// <summary>
    /// Internal Name of the View
    /// </summary>
    [XmlElement("Name")]
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// Name of the Parent
    /// </summary>
    [XmlElement("Parent")]
    public string Parent
    {
      get { return _parent; }
      set { _parent = value; }
    }

    /// <summary>
    /// List of Filters assigned to the View
    /// </summary>
    [XmlElement("Filter")]
    public List<FilterDefinitionNew> Filters
    {
      get { return _listFilters; }
      set { _listFilters = value; }
    }

    /// <summary>
    /// Level of the View
    /// </summary>
    [XmlElement("FilterLevel")]
    public List<FilterLevel> Levels
    {
      get { return _listFilterLevels; }
      set { _listFilterLevels = value; }
    }

    /// <summary>
    /// SubViews, if this is a Main View entry
    /// </summary>
    [XmlElement("SubView")]
    public List<ViewDefinitionNew> SubViews
    {
      get { return _listSubViews; }
      set { _listSubViews = value; }
    }

    /// <summary>
    /// The Localised name
    /// </summary>
    public string LocalizedName
    {
      get
      {
        String localizedName = _name;
        GUILocalizeStrings.LocalizeLabel(ref localizedName);
        return localizedName;
      }
    }

    /// <summary>
    /// The View name composed of the localised name
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (string.IsNullOrEmpty(_parent))
      {
        return LocalizedName;
      }
      return string.Format("{0}/{1}", _parent, LocalizedName);
    }

    #region ICloneable Members

    public object Clone()
    {
      ViewDefinitionNew clonedView = new ViewDefinitionNew();
      clonedView.Name = Name;
      clonedView.Parent = null;
      clonedView.Filters = Filters.GetRange(0, Filters.Count);
      clonedView.Levels = Levels.GetRange(0, Levels.Count);
      clonedView.SubViews = SubViews.GetRange(0, SubViews.Count);
      return clonedView;
    }

    #endregion
  }
}