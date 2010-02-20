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

using System.Xml.Serialization;

namespace MediaPortal.WebEPG.Config.Grabber
{
  /// <summary>
  /// The information for DataPreferences
  /// </summary>
  public class DataPreference
  {
    #region Variables

    [XmlAttribute("template")] public string Template;
    [XmlAttribute("title")] public int Title = 0;
    [XmlAttribute("subtitle")] public int Subtitle = 0;
    [XmlAttribute("genre")] public int Genre = 0;
    [XmlAttribute("description")] public int Description = 0;

    #endregion

    #region Constructors/Destructors

    public DataPreference() {}

    public DataPreference(DataPreference preference)
    {
      if (preference != null)
      {
        Template = preference.Template;
        Title = preference.Title;
        Subtitle = preference.Subtitle;
        Genre = preference.Genre;
        Description = preference.Description;
      }
    }

    #endregion
  }
}