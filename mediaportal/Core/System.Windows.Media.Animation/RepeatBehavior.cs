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
  [TypeConverter(typeof (RepeatBehaviorConverter))]
  public struct RepeatBehavior
  {
    #region Constructors

    public RepeatBehavior(double iterationCount)
    {
      if (iterationCount <= 0)
      {
        throw new ArgumentNullException("iterationCount");
      }

      _iterationCount = iterationCount;
      _duration = null;
    }

    public RepeatBehavior(Duration duration)
    {
      if (duration == null)
      {
        throw new ArgumentNullException("duration");
      }

      _iterationCount = 0;
      _duration = duration;
    }

    #endregion Constructors

    #region Methods

    public static RepeatBehavior Parse(string text)
    {
      if (string.Compare(text, "Forever", true) == 0)
      {
        return Forever;
      }

      return new RepeatBehavior(double.Parse((string)text));
    }

    #endregion Methods

    #region Properties

    public bool IsIterationCount
    {
      get { return _iterationCount != 0; }
    }

    public bool IsRepeatDuration
    {
      get { return _duration != null; }
    }

    public double IterationCount
    {
      get { return _iterationCount; }
    }

    public Duration RepeatDuration
    {
      get { return _duration; }
    }

    #endregion Properties

    #region Members

    private double _iterationCount;
    private Duration _duration;
    public static readonly RepeatBehavior Forever = new RepeatBehavior();

    #endregion Members
  }
}