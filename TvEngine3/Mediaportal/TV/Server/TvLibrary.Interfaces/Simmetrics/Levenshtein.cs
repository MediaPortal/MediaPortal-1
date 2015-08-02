#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Simmetrics
{
  /// <summary>
  /// Levenstein metric
  /// </summary>
  public static class Levenshtein
  {
    /// <summary>
    /// Get a measure of the similarity of two strings.
    /// </summary>
    /// <param name="s1">The first string.</param>
    /// <param name="s2">The second string.</param>
    /// <returns>a similarity measure between <c>zero</c> (completely different) and <c>one</c> (identical)</returns>
    public static float GetSimilarity(string s1, string s2)
    {
      if (string.IsNullOrEmpty(s1))
      {
        if (string.IsNullOrEmpty(s2))
        {
          return 1;
        }
        return 0;
      }
      if (string.IsNullOrEmpty(s2))
      {
        return 0;
      }

      // Levenshtein distance calculation.
      if (s1.Length > s2.Length)
      {
        var temp = s2;
        s2 = s1;
        s1 = temp;
      }

      var d = new int[2, s2.Length + 1];
      for (var j = 1; j <= s2.Length; j++)
      {
        d[0, j] = j;
      }

      var currentRow = 0;
      for (var i = 1; i <= s1.Length; ++i)
      {
        currentRow = i & 1;
        d[currentRow, 0] = i;
        var previousRow = currentRow ^ 1;
        for (var j = 1; j <= s2.Length; j++)
        {
          var cost = s2[j - 1] == s1[i - 1] ? 0 : 1;
          d[currentRow, j] = Math.Min(Math.Min(d[previousRow, j] + 1, d[currentRow, j - 1] + 1), d[previousRow, j - 1] + cost);
        }
      }
      return 1f - (float)d[currentRow, s2.Length] / Math.Max(s1.Length, s2.Length);
    }
  }
}