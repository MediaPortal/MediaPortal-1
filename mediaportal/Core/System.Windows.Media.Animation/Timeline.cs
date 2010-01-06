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
  public abstract class Timeline : Animatable
  {
    #region Constructors

    protected Timeline()
    {
      // stop compiled never used warnings
      if (CurrentGlobalSpeedInvalidated != null)
      {
        CurrentGlobalSpeedInvalidated(this, EventArgs.Empty);
      }

      if (CurrentStateInvalidated != null)
      {
        CurrentStateInvalidated(this, EventArgs.Empty);
      }

      if (CurrentTimeInvalidated != null)
      {
        CurrentTimeInvalidated(this, EventArgs.Empty);
      }
    }

    protected Timeline(Nullable<TimeSpan> beginTime) : this(beginTime, Duration.Automatic) {}

    protected Timeline(Nullable<TimeSpan> beginTime, Duration duration)
      : this(beginTime, duration, RepeatBehavior.Forever) {}

    protected Timeline(Nullable<TimeSpan> beginTime, Duration duration, RepeatBehavior repeatBehavior)
    {
      _beginTime = beginTime;
      _duration = duration;
      _repeatBehavior = repeatBehavior;
    }

    #endregion Constructors

    #region Events

    public event EventHandler CurrentGlobalSpeedInvalidated;
    public event EventHandler CurrentStateInvalidated;
    public event EventHandler CurrentTimeInvalidated;

    #endregion Events

    #region Methods

    // Clock.FromTimeline 

    protected internal virtual Clock AllocateClock()
    {
      return new Clock(this);
    }

    public new Timeline Copy()
    {
      return (Timeline)base.Copy();
    }

    protected override void CopyCore(Freezable sourceFreezable)
    {
      base.CopyCore(sourceFreezable);

      Timeline sourceTimeline = (Timeline)sourceFreezable;

      sourceTimeline._accelerationRatio = _accelerationRatio;
      sourceTimeline._beginTime = _beginTime;
      sourceTimeline._cutoffTime = _cutoffTime;
      sourceTimeline._decelerationRatio = _decelerationRatio;
      sourceTimeline._duration = _duration;
      sourceTimeline._fillBehavior = _fillBehavior;
      sourceTimeline._isAutoReverse = _isAutoReverse;
      sourceTimeline._name = _name;
      sourceTimeline._repeatBehavior = _repeatBehavior;
      sourceTimeline._speedRatio = _speedRatio;
    }

    protected override void CopyCurrentValueCore(Animatable sourceAnimatable)
    {
      base.CopyCurrentValueCore(sourceAnimatable);
    }

    public Clock CreateClock()
    {
      return new Clock(this);
    }

    protected internal Duration GetNaturalDuration(Clock clock)
    {
      // only be called when the Duration property is set to Automatic
      return GetNaturalDurationCore(clock);
    }

    protected virtual Duration GetNaturalDurationCore(Clock clock)
    {
      return clock.NaturalDuration;
    }

    protected override void OnPropertyInvalidated(DependencyProperty dp, PropertyMetadata metadata) {}

    #endregion Methods

    #region Properties

    public double AccelerationRatio
    {
      get { return _accelerationRatio; }
      set
      {
        if (value < 0 || value > 1)
        {
          throw new ArgumentOutOfRangeException();
        }
        _accelerationRatio = value;
      }
    }

    public bool AutoReverse
    {
      get { return _isAutoReverse; }
      set { _isAutoReverse = value; }
    }

    public Nullable<TimeSpan> BeginTime
    {
      get { return _beginTime; }
      set { _beginTime = value; }
    }

    public Nullable<TimeSpan> CutoffTime
    {
      get { return _cutoffTime; }
      set { _cutoffTime = value; }
    }

    public double DecelerationRatio
    {
      get { return _decelerationRatio; }
      set
      {
        if (value < 0 || value > 1)
        {
          throw new ArgumentOutOfRangeException();
        }
        _decelerationRatio = value;
      }
    }

    public Duration Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    public FillBehavior FillBehavior
    {
      get { return _fillBehavior; }
      set { _fillBehavior = value; }
    }

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    public RepeatBehavior RepeatBehavior
    {
      get { return _repeatBehavior; }
      set { _repeatBehavior = value; }
    }

    public double SpeedRatio
    {
      get { return _speedRatio; }
      set { _speedRatio = value; }
    }

    #endregion Properties

    #region Fields

    private double _accelerationRatio = 0;
    private Nullable<TimeSpan> _beginTime;
    private Nullable<TimeSpan> _cutoffTime;
    private double _decelerationRatio = 0;
    private Duration _duration = new Duration();
    private FillBehavior _fillBehavior;
    private bool _isAutoReverse;
    private string _name = string.Empty;
    private RepeatBehavior _repeatBehavior = RepeatBehavior.Forever;
    private double _speedRatio = 1;

    #endregion Fields
  }
}