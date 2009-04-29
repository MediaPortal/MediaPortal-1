///////////////////////////////////////////////////////////////////////
// Tween.h
// A couple of tweening classes implemented in C#
// ref: http://www.robertpenner.com/easing/
//
// Author: d4rk <d4rk@xboxmediacenter.com>
///////////////////////////////////////////////////////////////////////


///////////////////////////////////////////////////////////////////////
// Current list of classes:
//
// LinearTweener
// QuadTweener
// CubicTweener
// SineTweener
// CircleTweener
// BackTweener
// BounceTweener
// ElasticTweener
//
///////////////////////////////////////////////////////////////////////
using System;

namespace MediaPortal.GUI.Library
{
  public enum TweenerType
  {
    EASE_IN,
    EASE_OUT,
    EASE_INOUT
  } ;

  public class Tweener
  {
    public const float M_PI = 3.14159265358979323846f;
    protected int _ref;
    protected TweenerType m_tweenerType;

    public Tweener()
    {
      m_tweenerType = TweenerType.EASE_OUT;
      _ref = 1;
    }

    public Tweener(TweenerType tweenerType)
    {
      m_tweenerType = tweenerType;
      _ref = 1;
    }

    public TweenerType Easing
    {
      get { return m_tweenerType; }
      set { m_tweenerType = value; }
    }

    public virtual float Tween(float time, float start, float change, float duration)
    {
      return 0.0f;
    }
  } ;

  public class LinearTweener : Tweener
  {
    public override float Tween(float time, float start, float change, float duration)
    {
      return change*time/duration + start;
    }
  } ;

  public class QuadTweener : Tweener
  {
    private float _a;

    public QuadTweener()
    {
      _a = 1.0f;
    }

    public QuadTweener(float a)
    {
      _a = a;
    }

    public override float Tween(float time, float start, float change, float duration)
    {
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          time /= duration;
          return change*time*(_a*time + 1 - _a) + start;

        case TweenerType.EASE_OUT:
          time /= duration;
          return -change*time*(_a*time - 1 - _a) + start;

        case TweenerType.EASE_INOUT:
          time /= duration/2;
          if (time < 1)
          {
            return (change)*time*(_a*time + 1 - _a) + start;
          }
          time--;
          return (-change)*time*(_a*time - 1 - _a) + start;
      }
      return change*time*time + start;
    }
  } ;

  public class CubicTweener : Tweener
  {
    public override float Tween(float time, float start, float change, float duration)
    {
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          time /= duration;
          return change*time*time*time + start;

        case TweenerType.EASE_OUT:
          time /= duration;
          time--;
          return change*(time*time*time + 1) + start;

        case TweenerType.EASE_INOUT:
          time /= duration/2;
          if (time < 1)
          {
            return (change/2)*time*time*time + start;
          }
          time -= 2;
          return (change/2)*(time*time*time + 2) + start;
      }
      return change*time*time + start;
    }
  } ;

  public class CircleTweener : Tweener
  {
    public override float Tween(float time, float start, float change, float duration)
    {
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          time /= duration;
          return (float) ((-change)*(Math.Sqrt(1 - time*time) - 1) + start);

        case TweenerType.EASE_OUT:
          time /= duration;
          time--;
          return (float) (change*Math.Sqrt(1 - time*time) + start);

        case TweenerType.EASE_INOUT:
          time /= duration/2;
          if (time < 1)
          {
            return (float) ((-change/2)*(Math.Sqrt(1 - time*time) - 1) + start);
          }
          time -= 2;
          return (float) (change/2*(Math.Sqrt(1 - time*time) + 1) + start);
      }
      return (float) (change*Math.Sqrt(1 - time*time) + start);
    }
  } ;

  public class BackTweener : Tweener
  {
    private float _s;

    public BackTweener()
    {
      _s = 1.70158f;
    }

    public BackTweener(float s)
    {
      _s = s;
    }

    public override float Tween(float time, float start, float change, float duration)
    {
      float s = _s;
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          time /= duration;
          return (float) (change*time*time*((s + 1)*time - s) + start);


        case TweenerType.EASE_OUT:
          time /= duration;
          time--;
          return (float) (change*(time*time*((s + 1)*time + s) + 1) + start);


        case TweenerType.EASE_INOUT:
          time /= duration/2;
          s *= (1.525f);
          if ((time) < 1)
          {
            return (float) ((change/2)*(time*time*((s + 1)*time - s)) + start);
          }
          time -= 2;
          return (float) ((change/2)*(time*time*((s + 1)*time + s) + 2) + start);
      }
      return (float) (change*((time - 1)*time*((s + 1)*time + s) + 1) + start);
    }
  } ;

  public class SineTweener : Tweener
  {
    public override float Tween(float time, float start, float change, float duration)
    {
      time /= duration;
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          return (float) (change*(1 - Math.Cos(time*M_PI/2.0f)) + start);


        case TweenerType.EASE_OUT:
          return (float) (change*Math.Sin(time*M_PI/2.0f) + start);


        case TweenerType.EASE_INOUT:
          return (float) (change/2*(1 - Math.Cos(M_PI*time)) + start);
      }
      return (float) ((change/2)*(1 - Math.Cos(M_PI*time)) + start);
    }
  } ;

  public class BounceTweener : Tweener
  {
    public override float Tween(float time, float start, float change, float duration)
    {
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          return (change - easeOut(duration - time, 0, change, duration)) + start;


        case TweenerType.EASE_OUT:
          return easeOut(time, start, change, duration);


        case TweenerType.EASE_INOUT:
          if (time < duration/2)
          {
            return (change - easeOut(duration - (time*2), 0, change, duration) + start)*.5f + start;
          }
          else
          {
            return (easeOut(time*2 - duration, 0, change, duration)*.5f + change*.5f) + start;
          }
      }

      return easeOut(time, start, change, duration);
    }


    private float easeOut(float time, float start, float change, float duration)
    {
      time /= duration;
      if (time < (1/2.75))
      {
        return change*(7.5625f*time*time) + start;
      }
      else if (time < (2/2.75))
      {
        time -= (1.5f/2.75f);
        return change*(7.5625f*time*time + .75f) + start;
      }
      else if (time < (2.5/2.75))
      {
        time -= (2.25f/2.75f);
        return change*(7.5625f*time*time + .9375f) + start;
      }
      else
      {
        time -= (2.625f/2.75f);
        return change*(7.5625f*time*time + .984375f) + start;
      }
    }
  } ;

  public class ElasticTweener : Tweener
  {
    private float _a;
    private float _p;

    public ElasticTweener()
    {
      _a = 0.0f;
      _p = 0.0f;
    }

    public ElasticTweener(float a)
    {
      _a = a;
      _p = 0.0f;
    }

    public ElasticTweener(float a, float p)
    {
      _a = a;
      _p = p;
    }

    public override float Tween(float time, float start, float change, float duration)
    {
      switch (m_tweenerType)
      {
        case TweenerType.EASE_IN:
          return easeIn(time, start, change, duration);

        case TweenerType.EASE_OUT:
          return easeOut(time, start, change, duration);

        case TweenerType.EASE_INOUT:
          return easeInOut(time, start, change, duration);
      }
      return easeOut(time, start, change, duration);
    }


    private float easeIn(float time, float start, float change, float duration)
    {
      float s = 0;
      float a = _a;
      float p = _p;

      if (time == 0)
      {
        return start;
      }
      time /= duration;
      if (time == 1)
      {
        return start + change;
      }
      if (0.0f == p)
      {
        p = duration*.3f;
      }
      if (0.0f == a || a < Math.Abs(change))
      {
        a = change;
        s = p/4.0f;
      }
      else
      {
        s = (float) (p/(2*M_PI)*Math.Asin(change/a));
      }
      time--;
      return (float) (-(a*Math.Pow(2.0f, 10*time)*Math.Sin((time*duration - s)*(2*M_PI)/p)) + start);
    }

    private float easeOut(float time, float start, float change, float duration)
    {
      float s = 0;
      float a = _a;
      float p = _p;

      if (time == 0)
      {
        return start;
      }
      time /= duration;
      if (time == 1)
      {
        return start + change;
      }
      if (0.0f == p)
      {
        p = duration*.3f;
      }
      if (0.0f == a || a < Math.Abs(change))
      {
        a = change;
        s = p/4.0f;
      }
      else
      {
        s = (float) (p/(2*M_PI)*Math.Asin(change/a));
      }
      return (float) ((a*Math.Pow(2.0f, -10*time)*Math.Sin((time*duration - s)*(2*M_PI)/p)) + change + start);
    }

    private float easeInOut(float time, float start, float change, float duration)
    {
      float s = 0;
      float a = _a;
      float p = _p;

      if (time == 0)
      {
        return start;
      }
      time /= duration/2;
      if (time == 2)
      {
        return start + change;
      }
      if (0.0f == p)
      {
        p = duration*.3f*1.5f;
      }
      if (0.0f == a || a < Math.Abs(change))
      {
        a = change;
        s = p/4.0f;
      }
      else
      {
        s = (float) (p/(2*M_PI)*Math.Asin(change/a));
      }

      if (time < 1)
      {
        time--;
        return (float) (-.5f*(a*Math.Pow(2.0f, 10*(time))*Math.Sin((time*duration - s)*(2*M_PI)/p)) + start);
      }
      time--;
      return (float) (a*Math.Pow(2.0f, -10*(time))*Math.Sin((time*duration - s)*(2*M_PI)/p)*.5f + change + start);
    }
  } ;
}