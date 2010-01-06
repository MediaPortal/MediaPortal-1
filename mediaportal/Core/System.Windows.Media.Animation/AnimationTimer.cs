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

using System.Diagnostics;
using MediaPortal.ServiceImplementations;

namespace System.Windows.Media.Animation
{
  public sealed class AnimationTimer
  {
    private static Stopwatch clockWatch = new Stopwatch();

    #region Constructors

    static AnimationTimer()
    {
      clockWatch.Reset();
      clockWatch.Start();

      if (!Stopwatch.IsHighResolution)
      {
        Log.Warn("AnimationTimer: *** Your system does not support high resolution timers! ***");
      }
    }

    private AnimationTimer() {}

    #endregion Constructors

    #region Properties

    public static double TickCount
    {
      get
      {
        return TweenHelper.TickCount = ((double)clockWatch.ElapsedTicks / Stopwatch.Frequency) * 1000;
        // ((double) tick/_frequency)*1000;
      }
    }

    #endregion Properties
  }
}