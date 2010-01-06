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
using System.Xml.Serialization;

namespace MpeCore.Classes
{
  public class ActionItem
  {
    public ActionItem()
    {
      Params = new SectionParamCollection();
      ConditionGroup = string.Empty;
      ActionType = string.Empty;
      ExecuteLocation = ActionExecuteLocationEnum.AfterPanelShow;
    }

    public ActionItem(ActionItem obj)
    {
      Name = obj.Name;
      Params = Params;
      ConditionGroup = obj.ConditionGroup;
      ExecuteLocation = obj.ExecuteLocation;
    }

    public ActionItem(string actionType)
    {
      Name = actionType;
      ActionType = actionType;
      Params = new SectionParamCollection();
      ConditionGroup = string.Empty;
      ExecuteLocation = ActionExecuteLocationEnum.AfterPanelShow;
    }

    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string ActionType { get; set; }

    public SectionParamCollection Params { get; set; }

    [XmlAttribute]
    public string ConditionGroup { get; set; }

    public ActionExecuteLocationEnum ExecuteLocation { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}