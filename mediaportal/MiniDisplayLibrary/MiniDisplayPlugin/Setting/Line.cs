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
using System.Text;
using System.Xml.Serialization;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Line
  {
    [XmlAttribute] public Alignment Alignment;

    [XmlElement("PerformanceCounter", typeof (PerformanceCounter)),
     XmlElement("TextProgressBar", typeof (TextProgressBar)), XmlElement("Parse", typeof (Parse)),
     XmlElement("Property", typeof (Property)), XmlElement("Text", typeof (Text))] public List<Value> values;

    public Line()
    {
      this.values = new List<Value>();
    }

    public Line(Value value)
    {
      this.values = new List<Value>();
      this.values.Add(value);
    }

    public Line(string value)
    {
      this.values = new List<Value>();
      this.values.Add(new Parse(value));
    }

    public Line(Value value, Alignment alignment)
      : this(value)
    {
      this.Alignment = alignment;
    }

    public Line(string value, Alignment alignment)
      : this(value)
    {
      this.Alignment = alignment;
    }

    public string Process()
    {
      StringBuilder builder = new StringBuilder();
      foreach (Value value2 in this.values)
      {
        builder.Append(value2.Evaluate());
      }
      for (int i = 0; i < Settings.Instance.TranslateFrom.Length; i++)
      {
        builder.Replace(Settings.Instance.TranslateFrom[i], Settings.Instance.TranslateTo[i]);
      }
      return builder.ToString();
    }
  }
}