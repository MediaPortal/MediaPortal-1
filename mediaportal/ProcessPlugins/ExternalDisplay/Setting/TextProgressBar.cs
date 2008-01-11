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
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  [Serializable]
  public class TextProgressBar : Value
  {
    private char startChar = '[';
    private char endChar = ']';
    private char valueChar = '#';
    private char fillChar = '-';
    private int length = 8;
    private Property valueProperty;
    private Property targetProperty;

    public TextProgressBar()
    {
    }

    public TextProgressBar(string valueProperty, string targetProperty, int length)
    {
      this.length = length;
      this.valueProperty = new Property(valueProperty);
      this.targetProperty = new Property(targetProperty);
    }

    /// <summary>
    /// The character to start the progress bar with.
    /// </summary>
    /// <remarks>
    /// Default value is [
    /// </remarks>
    [XmlAttribute]
    public string StartChar
    {
      get { return new string(startChar, 1); }
      set
      {
        if (value != null && value.Length > 0)
        {
          startChar = value[0];
        }
      }
    }

    /// <summary>
    /// The character to end the progress bar with.
    /// </summary>
    /// <remarks>
    /// Default value is ]
    /// </remarks>
    [XmlAttribute]
    public string EndChar
    {
      get { return new string(endChar, 1); }
      set
      {
        if (value != null && value.Length > 0)
        {
          endChar = value[0];
        }
      }
    }

    /// <summary>
    /// The character to draw the progress bar's value with.
    /// </summary>
    /// <remarks>
    /// Default value is #
    /// </remarks>
    [XmlAttribute]
    public string ValueChar
    {
      get { return new string(valueChar, 1); }
      set
      {
        if (value != null && value.Length > 0)
        {
          valueChar = value[0];
        }
      }
    }

    /// <summary>
    /// The character to fill the rest of the progress bar with.
    /// </summary>
    /// <remarks>
    /// Default value is -
    /// </remarks>
    [XmlAttribute]
    public string FillChar
    {
      get { return new string(fillChar, 1); }
      set
      {
        if (value != null && value.Length > 0)
        {
          fillChar = value[0];
        }
      }
    }

    /// <summary>
    /// The number of characters the complete progress bar should be.
    /// </summary>
    /// <remarks>This number includes the begin- and end characters.</remarks>
    [XmlAttribute]
    public int Length
    {
      get { return length; }
      set { length = value; }
    }


    /// <summary>
    /// The property that holds the value to draw.
    /// </summary>
    /// <remarks>Only properties holding time- and number values are supported.</remarks>
    [XmlElement]
    public Property ValueProperty
    {
      get { return valueProperty; }
      set { valueProperty = value; }
    }

    /// <summary>
    /// The property that holds the value that represents a completely filled progress bar.
    /// </summary>
    /// <remarks>Only properties holding time- and number values are supported.</remarks>
    [XmlElement]
    public Property TargetProperty
    {
      get { return targetProperty; }
      set { targetProperty = value; }
    }

    /// <summary>
    /// Evaluates the properties and returns the progress bar.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the complete progress bar.</returns>
    protected override string DoEvaluate()
    {
      double currentValue = ConvertToInt(valueProperty.Evaluate());
      //We used to cache the targetValue, but as the target property also changes
      //(when skipping songs, for example) it is no longer cached.
      double targetValue = ConvertToInt(targetProperty.Evaluate());
      int barLength = (int) (currentValue <= 0 ? 0 : (currentValue/targetValue)*(length - 2));
      StringBuilder b = new StringBuilder(length);
      b.Append(startChar);
      b.Append(valueChar, barLength);
      b.Append(fillChar, length - 2 - barLength);
      b.Append(endChar);
      return b.ToString();
    }

    /// <summary>
    /// Tries to convert the passed <see cref="string"/> to a <see cref="double"/>.
    /// </summary>
    /// <param name="stringValue">The value to convert.</param>
    /// <returns>The result of the conversion.</returns>
    private double ConvertToInt(string stringValue)
    {
      DateTime dateResult;
      double result;

      if (stringValue == null || stringValue.Length == 0)
      {
        result = 0;
      }
      else if (
        DateTime.TryParseExact(stringValue, new string[] {"m:ss", "mm:ss", "h:mm:ss", "hh:mm:ss"}, null,
                               DateTimeStyles.None, out dateResult))
      {
        result = dateResult.TimeOfDay.TotalSeconds;
      }
      else if (!Double.TryParse(stringValue, out result))
      {
        result = Convert.ToInt32(stringValue);
      }
      return result;
    }
  }
}