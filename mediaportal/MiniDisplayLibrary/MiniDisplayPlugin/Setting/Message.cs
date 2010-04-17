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
using System.ComponentModel;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Message
  {
    [XmlElement("IsNull", typeof (IsNullCondition)), XmlElement("And", typeof (AndCondition)),
     XmlElement("NotNull", typeof (NotNullCondition)), DefaultValue((string)null),
     XmlElement("Or", typeof (OrCondition))] public Condition Condition;

    [XmlElement("Image", typeof (Image))] public List<Image> Images = new List<Image>();
    [XmlElement("Line", typeof (Line))] public List<Line> Lines = new List<Line>();
    [XmlAttribute, DefaultValue(9)] public Status Status = Status.Any;
    [XmlElement("Window", typeof (int))] public List<int> Windows = new List<int>();

    public bool Process(DisplayHandler _keeper)
    {
      if ((this.Condition != null) && !this.Condition.Evaluate())
      {
        return false;
      }
      _keeper.Lines = this.Lines;
      _keeper.Images = this.Images;
      return true;
    }
  }
}