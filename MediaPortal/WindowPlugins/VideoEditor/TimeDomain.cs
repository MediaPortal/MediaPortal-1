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

using System;

namespace WindowPlugins.VideoEditor
{
  /// <summary>
  /// Holds a Timedomain with the starttime, the endtime and the duration
  /// </summary>
  internal class TimeDomain
  {
    private double startTime, endTime, duration;
    private TimeSpan startTimeSp, endTimeSp, durationSp;

    public TimeDomain(double startTime, double endTime)
    {
      this.startTime = startTime;
      this.endTime = endTime;
      startTimeSp = new TimeSpan((long) (startTime*10e6));
      endTimeSp = new TimeSpan((long) (endTime*10e6));
      durationSp = new TimeSpan();
      if (endTime > startTime)
      {
        duration = endTime - startTime;
        durationSp = new TimeSpan(endTimeSp.Ticks - startTimeSp.Ticks);
      }
      else
      {
        duration = -1;
        //durationSp. = -1;
      }
    }

    public void SetBoth(double startTime, double endTime)
    {
      this.startTime = startTime;
      this.endTime = endTime;
      startTimeSp = new TimeSpan((long) (startTime*10e6));
      endTimeSp = new TimeSpan((long) (endTime*10e6));
      durationSp = new TimeSpan();
      if (endTime > startTime)
      {
        duration = endTime - startTime;
        durationSp = new TimeSpan(endTimeSp.Ticks - startTimeSp.Ticks);
      }
      else
      {
        duration = -1;
      }
    }

    public TimeSpan StartTimeSp
    {
      get { return startTimeSp; }
    }

    public TimeSpan EndTimeSp
    {
      get { return endTimeSp; }
    }

    public TimeSpan DurationSp
    {
      get { return durationSp; }
    }

    public double StartTime
    {
      get { return startTime; }
      set { startTime = value; }
    }

    public double EndTime
    {
      get { return endTime; }
      set { endTime = value; }
    }

    /// <summary>
    /// Gets the difference between the starttime and the endtime.
    /// Is the endtime lower than the starttime it gets -1.
    /// </summary>
    public double Duration
    {
      get
      {
        if (endTime > startTime)
        {
          duration = endTime - startTime;
        }
        else
        {
          duration = -1;
        }
        return duration;
      }
    }
  }
}