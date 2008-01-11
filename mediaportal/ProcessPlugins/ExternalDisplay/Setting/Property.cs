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
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The Property class represents a single property reference.
  /// </summary>
  /// <author>JoeDalton</author>
  [Serializable]
  public class Property : FixedValue
  {
    public Property()
    {
    }

    public Property(string _text)
    {
      value = _text;
    }

    public Property(string _text, Condition _condition) : this(_text)
    {
      Condition = _condition;
    }

    /// <summary>
    /// Evaluates the property
    /// </summary>
    /// <returns>
    /// The property's value, or an empty string if the associated <see cref="Condition"/> evaluates to false.
    /// </returns>
    protected override string DoEvaluate()
    {
      return GUIPropertyManager.GetProperty(value);
    }
  }
}