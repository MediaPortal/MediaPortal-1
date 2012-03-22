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
using System.Collections.Generic;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Class generic for parser data.
  /// </summary>
  [Serializable]
  public class HtmlParserTemplate
  {
    #region Variables

    [XmlAttribute("name")] public string Name;
    [XmlAttribute("start")] public string Start;
    [XmlAttribute("end")] public string End;
    [XmlArray("Replaces")] [XmlArrayItem("Replace")] public List<HtmlReplaceData> replaceList; //list with replace tasks
    [XmlElement("SectionTemplate")] public HtmlSectionTemplate SectionTemplate;

    #endregion
  }
}