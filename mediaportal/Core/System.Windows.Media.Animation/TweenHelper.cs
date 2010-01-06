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

namespace System.Windows.Media.Animation
{
  internal sealed class TweenHelper
  {
    #region Constructors

    private TweenHelper() {}

    #endregion Constructors

    #region Methods

    internal static double Interpolate(Easing easing, double from, double to, double tickStart, double duration)
    {
      double t = TickCount - tickStart;
      double b = from;
      double c = to - from;
      double d = duration;
      double s = 0;
      double p = 0;
      double a = 0;

      if (d == 0)
      {
        return 0;
      }

      // Easing Equations (c) 2003 Robert Penner, all rights reserved.
      // This work is subject to the terms in http://www.robertpenner.com/easing_terms_of_use.html.

      switch (easing)
      {
        case Easing.Linear:
          return c * t / d + b;

          ///////////// QUADRATIC EASING: t^2 ///////////////////
        case Easing.QuadraticEaseIn:
          return c * (t /= d) * t + b;
        case Easing.QuadraticEaseOut:
          return -c * (t /= d) * (t - 2) + b;
        case Easing.QuadraticEaseInOut:
          return ((t /= d / 2) < 1) ? c / 2 * t * t + b : -c / 2 * ((--t) * (t - 2) - 1) + b;

          ///////////// CUBIC EASING: t^3 ///////////////////////
        case Easing.CubicEaseIn:
          return c * (t /= d) * t * t + b;
        case Easing.CubicEaseOut:
          return c * ((t = t / d - 1) * t * t + 1) + b;
        case Easing.CubicEaseInOut:
          return ((t /= d / 2) < 1) ? c / 2 * t * t * t + b : c / 2 * ((t -= 2) * t * t + 2) + b;

          ///////////// QUARTIC EASING: t^4 /////////////////////
        case Easing.QuarticEaseIn:
          return c * (t /= d) * t * t * t + b;
        case Easing.QuarticEaseOut:
          return -c * ((t = t / d - 1) * t * t * t - 1) + b;
        case Easing.QuarticEaseInOut:
          return ((t /= d / 2) < 1) ? c / 2 * t * t * t * t + b : -c / 2 * ((t -= 2) * t * t * t - 2) + b;

          ///////////// QUINTIC EASING: t^5 ////////////////////
        case Easing.QuinticEaseIn:
          return c * (t /= d) * t * t * t * t + b;
        case Easing.QuinticEaseOut:
          return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        case Easing.QuinticEaseInOut:
          return ((t /= d / 2) < 1) ? c / 2 * t * t * t * t * t + b : c / 2 * ((t -= 2) * t * t * t * t + 2) + b;

          ///////////// SINUSOIDAL EASING: sin(t) ///////////////
        case Easing.SineEaseIn:
          return -c * Math.Cos(t / d * (Math.PI / 2)) + c + b;
        case Easing.SineEaseOut:
          return c * Math.Sin(t / d * (Math.PI / 2)) + b;
        case Easing.SineEaseInOut:
          return -c / 2 * (Math.Cos(Math.PI * t / d) - 1) + b;

          ///////////// EXPONENTIAL EASING: 2^t /////////////////
        case Easing.ExponentialEaseIn:
          return (t == 0) ? b : c * Math.Pow(2, 10 * (t / d - 1)) + b;
        case Easing.ExponentialEaseOut:
          return (t == d) ? b + c : c * (-Math.Pow(2, -10 * t / d) + 1) + b;
        case Easing.ExponentialEaseInOut:
          if (t == 0)
          {
            return b;
          }
          if (t == d)
          {
            return b + c;
          }
          if ((t /= d / 2) < 1)
          {
            return c / 2 * Math.Pow(2, 10 * (t - 1)) + b;
          }
          return c / 2 * (-Math.Pow(2, -10 * --t) + 2) + b;

          /////////// CIRCULAR EASING: sqrt(1-t^2) //////////////
        case Easing.CircularEaseIn:
          return -c * (Math.Sqrt(1 - (t /= d) * t) - 1) + b;
        case Easing.CircularEaseOut:
          return c * Math.Sqrt(1 - (t = t / d - 1) * t) + b;
        case Easing.CircularEaseInOut:
          return ((t /= d / 2) < 1)
                   ? -c / 2 * (Math.Sqrt(1 - t * t) - 1) + b
                   : c / 2 * (Math.Sqrt(1 - (t -= 2) * t) + 1) + b;

          /////////// ELASTIC EASING: exponentially decaying sine wave //////////////
        case Easing.ElasticEaseIn:
          if (t == 0)
          {
            return b;
          }
          if ((t /= d) == 1)
          {
            return b + c;
          }
          if (p == 0)
          {
            p = d * .3;
          }
          if (a < Math.Abs(c))
          {
            a = c;
            s = p / 4;
          }
          else
          {
            s = p / (2 * Math.PI) * Math.Asin(c / a);
          }
          return -(a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
        case Easing.ElasticEaseOut:
          if (t == 0)
          {
            return b;
          }
          if ((t /= d) == 1)
          {
            return b + c;
          }
          if (p == 0)
          {
            p = d * .3;
          }
          if (a < Math.Abs(c))
          {
            a = c;
            s = p / 4;
          }
          else
          {
            s = p / (2 * Math.PI) * Math.Asin(c / a);
          }
          return a * Math.Pow(2, -10 * t) * Math.Sin((t * d - s) * (2 * Math.PI) / p) + c + b;
        case Easing.ElasticEaseInOut:
          if (t == 0)
          {
            return b;
          }
          if ((t /= d / 2) == 2)
          {
            return b + c;
          }
          if (p == 0)
          {
            p = d * (.3 * 1.5);
          }
          if (a < Math.Abs(c))
          {
            a = c;
            s = p / 4;
          }
          else
          {
            s = p / (2 * Math.PI) * Math.Asin(c / a);
          }
          if (t < 1)
          {
            return -.5 * (a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
          }
          return a * Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p) * .5 + c + b;


          /////////// BACK EASING: overshooting cubic easing: (s+1)*t^3 - s*t^2 //////////////
        case Easing.BackEaseIn:
          if (s == 0)
          {
            s = 1.70158;
          }
          return c * (t /= d) * t * ((s + 1) * t - s) + b;
        case Easing.BackEaseOut:
          if (s == 0)
          {
            s = 1.70158;
          }
          return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
        case Easing.BackEaseInOut:
          if (s == 0)
          {
            s = 1.70158;
          }
          if ((t /= d / 2) < 1)
          {
            return c / 2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;
          }
          return c / 2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;

          /////////// BOUNCE EASING: exponentially decaying parabolic bounce //////////////
        case Easing.BounceEaseIn:
          return c - Interpolate(Easing.BounceEaseOut, d - t, 0, c, d) + b;
        case Easing.BounceEaseOut:
          if ((t /= d) < (1 / 2.75))
          {
            return c * (7.5625 * t * t) + b;
          }
          else if (t < (2 / 2.75))
          {
            return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
          }
          else if (t < (2.5 / 2.75))
          {
            return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
          }
          return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
        case Easing.BounceEaseInOut:
          if (t < d / 2)
          {
            return Interpolate(Easing.BounceEaseIn, t * 2, 0, c, d) * .5 + b;
          }
          return Interpolate(Easing.BounceEaseOut, t * 2 - d, 0, c, d) * .5 + c * .5 + b;
      }

      return 0;
    }

    #endregion Methods

    #region Fields

    public static double TickCount;

    #endregion Fields
  }
}