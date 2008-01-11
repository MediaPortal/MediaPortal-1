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

using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// Wrapper class for the Windows HighPerformanceCounter.
  /// </summary>
  /// <author>JoeDalton</author>
  public class HighPerformanceCounter
  {
    private static long frequency;
    private long start;
    private long end;

    [DllImport("kernel32")]
    private static extern int QueryPerformanceCounter(out long count);

    [DllImport("kernel32")]
    private static extern int QueryPerformanceFrequency(out long frequency);

    /// <summary>
    /// Static constructor
    /// </summary>
    static HighPerformanceCounter()
    {
      QueryPerformanceFrequency(out frequency);
    }

    /// <summary>
    /// Starts the HighPerformanceCounter
    /// </summary>
    public void Start()
    {
      if (QueryPerformanceCounter(out start) == 0)
      {
        start = 0;
      }
    }

    /// <summary>
    /// Ends the HighPerformanceCounter
    /// </summary>
    /// <remarks>
    /// After calling this method, the properties for determing the elapsed number
    /// of (micro/milli)seconds make sense.</remarks>
    public void End()
    {
      if (QueryPerformanceCounter(out end) == 0)
      {
        end = 0;
      }
    }

    /// <summary>
    /// Returns the number of HighPerformanceCounter ticks between the calls to <see cref="Start"/>
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of ticks
    /// </value>
    public long PeriodCount
    {
      get { return (end - start); }
    }

    /// <summary>
    /// Returns the number of µseconds that elapsed between the calls to <see cref="Start"/> 
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of µseconds.
    /// </value>
    public long MicroSeconds
    {
      get
      {
        long period = PeriodCount;
        if (period < 0x8637bd05af6)
        {
          return ((period*1000000)/frequency);
        }
        return ((period/frequency)*1000000);
      }
    }

    /// <summary>
    /// Returns the number of milliseconds that elapsed between the calls to <see cref="Start"/> 
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of milliseconds.
    /// </value>
    public long MilliSeconds
    {
      get
      {
        long period = PeriodCount;
        if (period < 0x20c49ba5e353f7)
        {
          return ((period*1000)/frequency);
        }
        return ((period/frequency)*1000);
      }
    }

    /// <summary>
    /// Returns the number of seconds that elapsed between the calls to <see cref="Start"/> 
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of seconds.
    /// </value>
    public long Seconds
    {
      get { return (PeriodCount/frequency); }
    }
  }
}