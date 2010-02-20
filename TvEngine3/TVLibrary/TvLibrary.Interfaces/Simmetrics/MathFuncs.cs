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

using System;

namespace mathSimmetrics
{
  /// <summary>
  /// Math helper functions
  /// </summary>
  public sealed class MathFuncs
  {
    /// <summary>
    /// Returns the maximum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Maximum of the given three values</returns>
    public static float max3(float x, float y, float z)
    {
      return Math.Max(x, Math.Max(y, z));
    }

    /// <summary>
    /// REturns the maximum of four values
    /// </summary>
    /// <param name="w">First value</param>
    /// <param name="x">Second value</param>
    /// <param name="y">Third value</param>
    /// <param name="z">Fourth value</param>
    /// <returns>Maximum of the given fourvalues</returns>
    public static float max4(float w, float x, float y, float z)
    {
      return Math.Max(Math.Max(w, x), Math.Max(y, z));
    }

    /// <summary>
    /// Returns the minimum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Minimum of the given three values</returns>
    public static float min3(float x, float y, float z)
    {
      return Math.Min(x, Math.Min(y, z));
    }

    /// <summary>
    /// Returns the minimum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Minimum of the given three values</returns>
    public static int min3(int x, int y, int z)
    {
      return Math.Min(x, Math.Min(y, z));
    }

    /// <summary>
    /// Returns the maximum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Maximum of the given three values</returns>
    public static int max3(int x, int y, int z)
    {
      return Math.Max(x, Math.Max(y, z));
    }
  }
}