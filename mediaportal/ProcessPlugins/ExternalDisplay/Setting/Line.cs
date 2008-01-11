#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using ExternalDisplay.Setting;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// This class represents a single display line
  /// </summary>
  /// <author>JoeDalton</author>
  [Serializable]
  public class Line
  {
    /// <summary>
    /// Needed for XmlSerializer
    /// </summary>
    public Line()
    {
    }

    public Line(string value)
    {
      values.Add(new Parse(value));
    }

    public Line(string value, Alignment alignment) : this(value)
    {
      Alignment = alignment;
    }

    public Line(Value value)
    {
      values.Add(value);
    }

    public Line(Value value, Alignment alignment) : this(value)
    {
      Alignment = alignment;
    }

    /// <summary>
    /// Text alignment for this line
    /// </summary>
    [XmlAttribute]
    public Alignment Alignment = Alignment.Left;

    /// <summary>
    /// List of values to display on this line
    /// </summary>
    [XmlElement("Text", typeof(Text))]
    [XmlElement("Property", typeof(Property))]
    [XmlElement("Parse", typeof(Parse))]
    [XmlElement("PerformanceCounter", typeof(PerformanceCounter))]
    [XmlElement("TextProgressBar", typeof(TextProgressBar))]
    public List<Value> values = new List<Value>();

    /// <summary>
    /// Process the line
    /// </summary>
    /// <returns>the string to display</returns>
    public string Process()
    {
      StringBuilder s = new StringBuilder();
      foreach (Value val in values)
      {
        s.Append(val.Evaluate());
      }
      for (int i = 0; i < Settings.Instance.TranslateFrom.Length; i++)
      {
        s.Replace(Settings.Instance.TranslateFrom[i], Settings.Instance.TranslateTo[i]);
      }
      return s.ToString();
    }
  }
}