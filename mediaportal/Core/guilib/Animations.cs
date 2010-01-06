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

using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace MediaPortal.GUI.Library
{
  public class Animation
  {
    #region Fields

    protected double _start = 0;
    protected double _stop = 0;
    protected Duration _duration = Duration.Automatic;
    protected double _startTick = 0;
    protected Easing _easing = Easing.Linear;
    protected bool _running = false;

    #endregion

    #region Constructors

    public Animation() {}

    public Animation(double duration, int StartValue, int StopValue)
    {
      _duration = new Duration(duration);
      _start = StartValue;
      _stop = StopValue;
    }

    #endregion

    #region Methods

    public virtual void Begin()
    {
      _startTick = AnimationTimer.TickCount;
      _running = true;
    }

    public virtual void Begin(double StartTick)
    {
      Begin();
      _startTick = StartTick;
    }

    public virtual void Stop()
    {
      _running = false;
    }

    #endregion

    #region Properties

    public virtual bool Running
    {
      get { return _running; }
    }

    public double ElapsedTicks
    {
      get { return (AnimationTimer.TickCount - _startTick); }
    }

    public double duration
    {
      get { return _duration; }
    }

    public virtual double StartValue
    {
      get { return _start; }
    }

    public virtual double StopValue
    {
      get { return _stop; }
    }

    public virtual double Value
    {
      get
      {
        if (!_running)
        {
          return _stop;
        }

        if (ElapsedTicks <= _duration)
        {
          return (TweenHelper.Interpolate(_easing, _start, _stop, _startTick, _duration));
        }
        _running = false;
        return _stop;
      }
    }

    #endregion
  }

  public class AnimationGroup : Animation
  {
    #region Fields

    protected List<Animation> _animationList = new List<Animation>();
    protected int _currentIndex = 0;
    protected double _lastValue = 0;

    #endregion

    #region Constructors

    #endregion

    #region Methods

    public override void Begin()
    {
      if (_animationList.Count < 1)
      {
        return;
      }
      base.Begin();
      _currentIndex = 0;
      InitValues();
      _animationList[_currentIndex].Begin();
    }

    protected void InitValues()
    {
      _start = _lastValue = _animationList[0].StartValue;
      _stop = _animationList[_animationList.Count - 1].StopValue;
      //_duration = 0.0;
      //foreach (Animation animation in _animationList) duration += animation.duration;
    }

    public override void Stop()
    {
      base.Stop();
      foreach (Animation animation in _animationList)
      {
        animation.Stop();
      }
      //_animationList[_currentIndex].Stop();
    }

    public void Add(Animation item)
    {
      _animationList.Add(item);
    }

    public void Clear()
    {
      Stop();
      _animationList.Clear();
      _start = _stop = _startTick = 0;
    }

    #endregion

    #region Properties

    public int Count
    {
      get { return _animationList.Count; }
    }

    public int CurrentIndex
    {
      get { return _currentIndex; }
    }

    public override bool Running
    {
      get
      {
        foreach (Animation item in _animationList)
        {
          if (item.Running)
          {
            return true;
          }
        }
        return false;
      }
    }


    public override double Value
    {
      get
      {
        if (_animationList.Count <= 0)
        {
          return _stop;
        }
        if ((!Running) || (_currentIndex >= _animationList.Count))
        {
          return _stop;
        }

        if ((!_animationList[_currentIndex].Running) ||
            (_animationList[_currentIndex].ElapsedTicks > _animationList[_currentIndex].duration))
        {
          double elapsedTicks = _animationList[_currentIndex].ElapsedTicks - _animationList[_currentIndex].duration;
          if (elapsedTicks < 0)
          {
            elapsedTicks = 0;
          }
          _animationList[_currentIndex].Stop();

          if (_currentIndex + 1 >= _animationList.Count)
          {
            return _stop;
          }
          _currentIndex++;
          while (_animationList[_currentIndex].duration < elapsedTicks)
          {
            if (_currentIndex + 1 >= _animationList.Count)
            {
              return _stop;
            }
            elapsedTicks -= _animationList[_currentIndex].duration;
            _currentIndex++;
          }

          _animationList[_currentIndex].Begin(AnimationTimer.TickCount - elapsedTicks);
        }

        return (_animationList[_currentIndex].Value);
      }
    }

    #endregion
  }
}