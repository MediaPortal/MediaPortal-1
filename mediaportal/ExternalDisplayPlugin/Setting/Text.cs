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
using System.Text;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The Text class represents either a fixed text, or a reference to a translatable resource string.
  /// </summary>
  /// <author>JoeDalton</author>
  [Serializable]
  public class Text : FixedValue
  {
    public Text()
    {
    }

    public Text(string _text)
    {
      value = _text;
    }

    public Text(string _text, Condition _condition)
    {
      value = _text;
      Condition = _condition;
    }

    /// <summary>
    /// Evaluates the <see cref="Text"/>
    /// </summary>
    /// <returns>
    /// The fixed text, or in case of a reference to a translatable resource string, the translation.
    /// If the associated <see cref="Condition"/> evaluates to false, the return value is an empty string.
    /// </returns>
    protected override string DoEvaluate()
    {
      string text = value;
      int pos = text.IndexOf("#") + 1;
      while (pos > 0)
      {
        StringBuilder b = new StringBuilder();
        while (pos >= 1 && pos < text.Length && text[pos] >= '0' && text[pos] <= '9')
        {
          b.Append(text[pos++]);
        }
        string num = b.ToString();
        int val = int.Parse(num);
        text = text.Replace("#" + num, GUILocalizeStrings.Get(val));
        pos = text.IndexOf("#") + 1;
      }
      return text;
    }
  }
}