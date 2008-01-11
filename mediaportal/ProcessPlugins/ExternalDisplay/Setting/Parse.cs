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
using System.Xml.Serialization;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The Parse class represents a string containing references to properties.
  /// </summary>
  /// <remarks>
  /// When the Parse is evaluated all references to properties are replaces by their property values.
  /// </remarks>
  /// <author>JoeDalton</author>
  [Serializable]
  public class Parse : Value
  {
    [XmlAttribute("Value")]
    public string value = "";

    public Parse()
    {
    }

    public Parse(string _text)
    {
      value = _text;
    }

    public Parse(string _text, Condition _condition) : this(_text)
    {
      Condition = _condition;
    }

    /// <summary>
    /// Evaluates the <see cref="Parse"/>.
    /// </summary>
    /// <returns>The Parse string with all propertie references replaced by their values, or an empty
    /// string if the associated <see cref="Condition"/> evaluates to false.</returns>
    protected override string DoEvaluate()
    {
      return GUIPropertyManager.Parse(value);
    }
  }
}