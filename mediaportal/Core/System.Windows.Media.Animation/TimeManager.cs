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

namespace System.Windows.Media.Animation
{
  public sealed class TimeManager
  {
    #region Constructors

    public TimeManager()
    {
    }

    public TimeManager(Clock clock)
    {
      _clock = clock;
    }

    #endregion Constructors

    #region Methods

    public void Pause()
    {
      if (_isStarted == false)
      {
        return;
      }

      if (_isPaused)
      {
        return;
      }

      _pauseTick = _clock.CurrentTime.Milliseconds;
      _isPaused = true;
    }

    public void Restart()
    {
      Stop();
      Start();
    }

    public void Resume()
    {
      if (_isPaused == false)
      {
        return;
      }

      _startTick = _startTick + _pauseTick;
      _isPaused = false;
    }

    public void Seek(int offset, TimeSeekOrigin origin)
    {
      // This method seeks the global clock. The timelines of all timelines in the timing tree are 
      // also updated accordingly. Any events that those timelines would have fired in between the
      // old and new clock positions are skipped. Because the global clock is infinite there is no
      // defined end point, therefore seeking from the end position has no effect.

      // docs state that Seek while paused involes resume and seek
      if (_isStarted == false)
      {
        return;
      }

      throw new NotImplementedException();
    }

    public void Start()
    {
      if (_isStarted)
      {
        return;
      }

      _isStarted = true;
      _startTick = _clock.CurrentTime.Milliseconds;
    }

    public void Stop()
    {
      if (_isStarted == false)
      {
        return;
      }

      _isStarted = false;
      _isPaused = false;
    }

    public void Tick()
    {
      if (_isStarted == false)
      {
        return;
      }

      if (_isPaused)
      {
        return;
      }

      // The associated reference clock is used to determine the current time,
      // The new position of the clock will be equal to the difference between
      // the starting system time and the current system time. The time manager requires 
      // the system time to move forward.
    }

    #endregion Methods

    #region Properties

    public Clock Clock
    {
      get { return _clock; }
      set { _clock = value; }
    }

    public static TimeSpan CurrentGlobalTime
    {
      get { return TimeSpan.Zero; }
    }

    public TimeSpan CurrentTime
    {
      get { return TimeSpan.Zero; }
    }

    public bool IsDirty
    {
      // True if the structure of the timing tree has changed since the last tick.
      get { return _isDirty; }
    }

    #endregion Properties

    #region Fields

    private Clock _clock;
    private bool _isDirty = false;
    private bool _isPaused;
    private bool _isStarted;
    private long _startTick;
    private long _pauseTick;

    #endregion Fields
  }
}