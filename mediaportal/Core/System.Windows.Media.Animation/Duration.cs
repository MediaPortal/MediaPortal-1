#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.ComponentModel;

namespace System.Windows.Media.Animation
{
  [TypeConverter(typeof (DurationConverter))]
  public class Duration
  {
    #region Constructors

    public Duration() {}

    public Duration(double duration)
    {
      // according to docs this isn't a constructor in Avalon
      _timeSpan = TimeSpan.FromMilliseconds(duration);
    }

    public Duration(TimeSpan timeSpan)
    {
      _timeSpan = timeSpan;
    }

    #endregion Constructors

    #region Methods

    public static Duration Parse(string text)
    {
      if (string.Compare(text, "Automatic", true) == 0)
      {
        return Automatic;
      }

      if (string.Compare(text, "Forever", true) == 0)
      {
        return Forever;
      }

      return new Duration(TimeSpan.Parse(text));
    }

    #endregion Methods

    #region Operators

    public static implicit operator double(Duration duration)
    {
      return duration.TimeSpan.TotalMilliseconds;
    }

    #endregion Operators

    #region Properties

    public bool HasTimeSpan
    {
      get { return _timeSpan.TotalMilliseconds != 0; }
    }

    public TimeSpan TimeSpan
    {
      get { return _timeSpan; }
    }

    #endregion Properties

    #region Fields

    private TimeSpan _timeSpan;

    public static readonly Duration Automatic = new Duration();
    public static readonly Duration Forever = new Duration();

    #endregion Fields
  }
}