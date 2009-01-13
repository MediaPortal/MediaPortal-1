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

using System;
using MediaPortal.GUI.Library;

namespace WindowPlugins.AutoCropper
{
  /// <summary>
  /// Provides functionality for maintaining a the average and minimum
  /// over a sliding window of data. (Note: minimum is implemented naively
  /// and thus takes time linear in the window length). 
  /// </summary>
  internal class MovingAverage
  {
    private int sum = 0;
    private int length = 0;
    private int[] list = null;
    private int next = 0;

    public float Average
    {
      get { return sum/((float) length); }
    }

    public MovingAverage(int length, int startValue)
    {
      this.length = length;
      sum = 0;
      list = new int[length];
      next = 0;
      for (int i = 0; i < length; i++)
      {
        list[i] = startValue;
      }
      sum = startValue*length;
    }

    public void Add(int v)
    {
      int outValue = list[next];
      list[next] = v;
      next = (next + 1)%length;
      sum += v - outValue;
    }

    public int GetMin()
    {
      int min = Int32.MaxValue;
      string s = "MA Content: ";
      for (int i = 0; i < length; i++)
      {
        if (list[i] < min)
        {
          min = list[i];
        }
        s += list[i] + " ";
      }
      Log.Debug(s);
      return min;
    }

    public void Reset(int startValue)
    {
      next = 0;

      for (int i = 0; i < length; i++)
      {
        list[i] = startValue;
      }
      sum = startValue*length;
    }
  }
}