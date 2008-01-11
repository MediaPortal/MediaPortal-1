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
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// This class represents a display message
  /// </summary>
  /// <remarks>
  /// A message has a number of triggers like the MediaPortal status, a list of window IDs and a 
  /// condition.  Only if all triggers match the message will be sent to the 
  /// <see cref="DisplayHandler"/> who is responsible for formatting it and sending it to the display.
  /// </remarks>
  /// <author>JoeDalton</author>
  [Serializable]
  public class Message
  {
    /// <summary>
    /// MP status that will trigger this message
    /// </summary>
    [XmlAttribute]
    [DefaultValue(Status.Any)]
    public Status Status = Status.Any;

    /// <summary>
    /// List of active windows that will trigger this message
    /// </summary>
    [XmlElement("Window", typeof(int))]
    public List<int> Windows = new List<int>();

    /// <summary>
    /// List of lines that this message contains
    /// </summary>
    [XmlElement("Line", typeof(Line))]
    public List<Line> Lines = new List<Line>();

    /// <summary>
    /// Condition for this message
    /// </summary>
    [XmlElement("IsNull", typeof(IsNullCondition))]
    [XmlElement("NotNull", typeof(NotNullCondition))]
    [XmlElement("And", typeof(AndCondition))]
    [XmlElement("Or", typeof(OrCondition))]
    [DefaultValue(null)]
    public Condition Condition = null;

    [XmlElement("Image", typeof(Image))]
    public List<Image> Images = new List<Image>();

    /// <summary>
    /// Process the message
    /// </summary>
    /// <param name="_keeper">The <see cref="DisplayHandler"/> that will put this message on the display</param>
    /// <returns>A boolean, indicating whether this message is processed</returns>
    public bool Process(DisplayHandler _keeper)
    {
      if (Condition == null || Condition.Evaluate())
      {
        _keeper.Lines = Lines;
        _keeper.Images = Images;
        return true;
      }
      return false;
    }
  }
}