#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Windows.Threading;

namespace System.Windows.Media.Animation
{
  public class Clock : DispatcherObject
  {
    #region Constructors

    protected internal Clock(Timeline timeline)
    {
      _timeline = timeline;
      _timeline.CurrentGlobalSpeedInvalidated += new EventHandler(TimelineCurrentGlobalSpeedInvalidated);
      _timeline.CurrentStateInvalidated += new EventHandler(TimelineCurrentStateInvalidated);
      _timeline.CurrentTimeInvalidated += new EventHandler(TimelineCurrentTimeInvalidated);
    }

    #endregion Constructors

    #region Events

    public event EventHandler CurrentGlobalSpeedInvalidated;
    public event EventHandler CurrentStateInvalidated;
    public event EventHandler CurrentTimeInvalidated;

    #endregion Events

    #region Methods

    protected virtual void DiscontinuousTimeMovement() {}

    protected virtual void SpeedChanged() {}

    protected virtual void Stopped() {}

    private void TimelineCurrentGlobalSpeedInvalidated(object sender, EventArgs e)
    {
      SpeedChanged();

      if (CurrentGlobalSpeedInvalidated != null)
      {
        CurrentGlobalSpeedInvalidated(this, e);
      }
    }

    private void TimelineCurrentStateInvalidated(object sender, EventArgs e)
    {
      if (CurrentStateInvalidated != null)
      {
        CurrentStateInvalidated(this, e);
      }
    }

    private void TimelineCurrentTimeInvalidated(object sender, EventArgs e)
    {
      if (CurrentTimeInvalidated != null)
      {
        CurrentTimeInvalidated(this, e);
      }
    }

    #endregion Methods

    #region Properties

    public ClockController Controller
    {
      get { return null; }
    }

    // should be Nullable<double> or Nullable<double>
    public double CurrentGlobalSpeed
    {
      // if the clock is stopped this should return null (when using appropriate nullable type)
      get { return 1; }
    }

    public int CurrentIteration
    {
      // if the timeline is not active, the value of this property is only valid if the fill attribute specifies that the timing attributes should be extended. Otherwise, the property returns -1. 
      get { return 1; }
    }

    public double CurrentProgress
    {
      // If the clock is active, the progress is always a value between 0 and 1, inclusive
      // If the clock is inactive and the fill attribute is not in effect, this property returns null
      get { return 1; }
    }

    public ClockState CurrentState
    {
      get { return _currentState; }
    }

    public TimeSpan CurrentTime
    {
      get { return _currentTime; }
    }

    public bool IsPaused
    {
      get { return _isPaused; }
    }

    public Duration NaturalDuration
    {
      get { return _timeline.Duration; }
    }

    public Clock Parent
    {
      get { return null; }
    }

    public Timeline Timeline
    {
      get { return _timeline; }
    }

    #endregion Properties

    #region Fields

    private ClockState _currentState = ClockState.Stopped;
    private TimeSpan _currentTime = TimeSpan.Zero;
    private bool _isPaused = false;
    private Duration _naturalDuration = Duration.Automatic;
    private Timeline _timeline = null;

    #endregion Fields
  }
}