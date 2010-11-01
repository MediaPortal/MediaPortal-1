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
  // Referenced classes of package api:
  //            InterfaceSubstitutionCost

  /// <summary>
  /// Abstract SubstitutionCost
  /// </summary>
  public abstract class AbstractSubstitutionCost : InterfaceSubstitutionCost
  {
    /// <summary>
    /// Short Descritption
    /// </summary>
    /// <returns>Short Description</returns>
    public abstract String getShortDescriptionString();

    /// <summary>
    /// Returns the cost
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="i">Param2</param>
    /// <param name="s1">Param3</param>
    /// <param name="j">Param4</param>
    /// <returns>Cost</returns>
    public abstract float getCost(String s, int i, String s1, int j);

    /// <summary>
    /// Get the maximum cost
    /// </summary>
    /// <returns>Maximum cost</returns>
    public abstract float getMaxCost();

    /// <summary>
    /// Get the minimum cost
    /// </summary>
    /// <returns>Minimum cost</returns>
    public abstract float getMinCost();
  }
}