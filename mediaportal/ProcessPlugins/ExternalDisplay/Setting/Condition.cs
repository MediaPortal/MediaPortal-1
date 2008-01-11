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
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The abstract base class for all conditions
  /// </summary>
  /// <author>JoeDalton</author>
  [XmlInclude(typeof(NotNullCondition))]
  [XmlInclude(typeof(IsNullCondition))]
  [XmlInclude(typeof(AndCondition))]
  [XmlInclude(typeof(OrCondition))]
  [Serializable]
  public abstract class Condition
  {
    /// <summary>
    /// The property to evaluate
    /// </summary>
    [XmlIgnore]
    protected Property Property;

    [XmlAttribute]
    [DefaultValue("")]
    public string Value
    {
      get
      {
        if (Property == null)
        {
          return "";
        }
        return Property.value;
      }
      set { Property = new Property(value); }
    }

    /// <summary>
    /// Evaluates the condition
    /// </summary>
    /// <returns></returns>
    public abstract bool Evaluate();
  }
}