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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace MediaPortal.Common.Utils.ExtensionMethods
{
  /// <summary>
  /// This class provides functions that enable user-friendly enum member descriptions to be
  /// displayed in user interfaces.
  /// </summary>
  public static class EnumExtMethods
  {
    /// <summary>
    /// Get the description for an enum member.
    /// </summary>
    /// <remarks>
    /// The member's name is returned if a description is not available.
    /// </remarks>
    /// <param name="value">The enum member value.</param>
    /// <returns>the description of the member</returns>
    public static string GetDescription(this Enum value)
    {
      if (value == null)
      {
        return string.Empty;
      }
      FieldInfo fi = value.GetType().GetField(value.ToString());
      if (fi == null)
      {
        return string.Empty;
      }
      DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

      if (attributes != null && attributes.Length > 0)
      {
        return attributes[0].Description;
      }

      return value.ToString();
    }

    /// <summary>
    /// Get the enum member from a given enum type that matches a description.
    /// </summary>
    /// <param name="enumType">The enum type.</param>
    /// <param name="description">The enum member description.</param>
    /// <returns>the enum type member value if a matching member is found, otherwise <c>null</c></returns>
    public static Enum GetEnumFromDescription(this Type enumType, string description)
    {
      if (enumType == null || !enumType.IsEnum || description == null)
      {
        return null;
      }
      Array enumValues = Enum.GetValues(enumType);
      foreach (Enum e in enumValues)
      {
        if (description.Equals(GetDescription(e)))
        {
          return e;
        }
      }
      return null;
    }

    /// <summary>
    /// Get the descriptions for a subset of the members of an enum type.
    /// </summary>
    /// <remarks>
    /// Members are filtered in or out of the result set by bitwise comparison
    /// of the member value with the filter value.
    /// </remarks>
    /// <param name="enumType">The enum type.</param>
    /// <param name="filter">The member filter.</param>
    /// <param name="includeZeroMember"><c>True</c> to include the member with value zero.</param>
    /// <returns>an array of strings containing the descriptions for each member that matches the filter</returns>
    public static string[] GetDescriptions(this Type enumType, int filter = -1, bool includeZeroMember = true)
    {
      if (enumType == null || !enumType.IsEnum)
      {
        return new string[0];
      }

      Array enumValues = Enum.GetValues(enumType);
      List<string> toReturn = new List<string>();
      foreach (Enum e in enumValues)
      {
        int value = Convert.ToInt32(e);
        if (filter == -1 || (value == 0 && includeZeroMember) || (value != 0 && (value & filter) == value))
        {
          toReturn.Add(GetDescription(e));
        }
      }
      return toReturn.ToArray();
    }

    /// <summary>
    /// Get the descriptions for a subset of the members of an enum type.
    /// </summary>
    /// <remarks>
    /// Members are filtered in or out of the result set by bitwise comparison
    /// of the member value with the filter value.
    /// </remarks>
    /// <param name="enumType">The enum type.</param>
    /// <param name="filter">The member filter.</param>
    /// <param name="includeZeroMember"><c>True</c> to include the member with value zero.</param>
    /// <returns>an array of strings containing the descriptions for each member that matches the filter</returns>
    public static string[] GetDescriptions(this Type enumType, IEnumerable<Enum> values)
    {
      if (enumType == null || !enumType.IsEnum)
      {
        return new string[0];
      }

      List<string> toReturn = new List<string>();
      foreach (Enum e in values)
      {
        toReturn.Add(GetDescription(e));
      }
      return toReturn.ToArray();
    }

    /// <summary>
    /// Get the members of an enum type.
    /// </summary>
    /// <param name="enumType">The enum type.</param>
    /// <param name="filter">The member filter.</param>
    /// <param name="includeZeroMember"><c>True</c> to include the member with value zero.</param>
    /// <returns>a list of member key-value pairs that match the filter</returns>
    public static IList ToList(this Type enumType, int filter = -1, bool includeZeroMember = true)
    {
      ArrayList toReturn = new ArrayList();
      if (enumType != null && enumType.IsEnum)
      {
        Array enumValues = Enum.GetValues(enumType);
        foreach (Enum e in enumValues)
        {
          int value = Convert.ToInt32(e);
          if (filter == -1 || (value == 0 && includeZeroMember) || (value != 0 && (value & filter) == value))
          {
            toReturn.Add(new KeyValuePair<int, string>(value, GetDescription(e)));
          }
        }
      }
      return toReturn;
    }
  }
}