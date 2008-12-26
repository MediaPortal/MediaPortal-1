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
using System;
using System.Collections.Generic;

namespace TvLibrary.Epg
{
  /// <summary>
  /// Class which contains a single epg-program
  /// </summary>
  [Serializable]
  public class EpgProgram : IComparable<EpgProgram>
  {
    #region variables
    List<EpgLanguageText> _languageText;
    DateTime _startTime;
    DateTime _endTime;
    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="EpgProgram"/> class.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    public EpgProgram(DateTime startTime, DateTime endTime)
    {
      _startTime = startTime;
      _endTime = endTime;
      _languageText = new List<EpgLanguageText>();
    }

    #endregion


    #region properties
    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public List<EpgLanguageText> Text
    {
      get
      {
        return _languageText;
      }
      set
      {
        _languageText = value;
      }
    }

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    public DateTime StartTime
    {
      get
      {
        return _startTime;
      }
      set
      {
        _startTime = value;
      }
    }

    /// <summary>
    /// Gets or sets the end time.
    /// </summary>
    /// <value>The end time.</value>
    public DateTime EndTime
    {
      get
      {
        return _endTime;
      }
      set
      {
        _endTime = value;
      }
    }
    #endregion

    #region IComparable<EpgProgram> Members

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
    /// </returns>
    public int CompareTo(EpgProgram other)
    {
      if (other.StartTime > StartTime)
        return -1;
      if (other.StartTime < StartTime)
        return 1;
      return 0;
    }

    #endregion
  }
}
