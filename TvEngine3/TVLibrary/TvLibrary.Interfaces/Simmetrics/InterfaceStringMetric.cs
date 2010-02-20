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

namespace api
{
  /// <summary>
  /// String metric interface
  /// </summary>
  public interface InterfaceStringMetric
  {
    /// <summary>
    /// Gets the short description
    /// </summary>
    String ShortDescriptionString { get; }

    /// <summary>
    /// Gets the long description
    /// </summary>
    String LongDescriptionString { get; }

    /// <summary>
    /// Return Similarity Timing Actual
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>Similarity Timing</returns>
    long getSimilarityTimingActual(String s, String s1);

    /// <summary>
    /// Return the similarty timing estimation
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>similarty timing estimation</returns>
    float getSimilarityTimingEstimated(String s, String s1);

    /// <summary>
    /// Return the similarity
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>Similarity</returns>
    float getSimilarity(String s, String s1);
  }
}