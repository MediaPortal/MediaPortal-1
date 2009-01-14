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

namespace System.Windows.Media.Animation
{
  public sealed class AnimationTimer
  {
    #region Constructors

    static AnimationTimer()
    {
      long frequency = 0;

      if (NativeMethods.QueryPerformanceFrequency(ref frequency) == false)
      {
        throw new NotSupportedException("Hi-res timer");
      }

      _frequency = frequency;
    }

    private AnimationTimer()
    {
    }

    #endregion Constructors

    #region Properties

    public static double TickCount
    {
      get
      {
        long tick = 0;

        if (NativeMethods.QueryPerformanceCounter(ref tick) == false)
        {
          throw new NotSupportedException("Hi-res timer");
        }

        return TweenHelper.TickCount = ((double) tick/_frequency)*1000;
      }
    }

    #endregion Properties

    #region Fields

    private static double _frequency = 0;

    #endregion Fields
  }
}