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
using api;

namespace similaritymetrics.costfunctions
{
  ///<summary>
  ///</summary>
  [Serializable]
  public sealed class SubCost01 : AbstractSubstitutionCost //, System.Runtime.Serialization.ISerializable
  {
    /// <summary>
    /// Short descritption
    /// </summary>
    /// <returns>Short descritption</returns>
    public override String getShortDescriptionString()
    {
      return "SubCost01";
    }

    ///<summary>
    /// Costs
    ///</summary>
    ///<param name="str1"></param>
    ///<param name="string1Index"></param>
    ///<param name="str2"></param>
    ///<param name="string2Index"></param>
    ///<returns>Costs</returns>
    public override float getCost(String str1, int string1Index, String str2, int string2Index)
    {
      return str1[string1Index] != str2[string2Index] ? 1.0F : 0.0F;
    }

    ///<summary>
    /// Maximum cost
    ///</summary>
    ///<returns>Maximum Cost 1.0</returns>
    public override float getMaxCost()
    {
      return 1.0F;
    }

    ///<summary>
    /// Minimum cost
    ///</summary>
    ///<returns>Minimum cost 0.0</returns>
    public override float getMinCost()
    {
      return 0.0F;
    }
  }
}