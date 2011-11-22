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

namespace MediaPortal.ExtensionMethods
{
  public static class StringMethods
  {
    public static string ReplaceEx(this string original, string pattern, string replacement)
    {
      int count, position0, position1;
      count = position0 = position1 = 0;
      string upperString = original.ToUpper();
      string upperPattern = pattern.ToUpper();
      int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
      char[] chars = new char[original.Length + Math.Max(0, inc)];
      while ((position1 = upperString.IndexOf(upperPattern, position0)) != -1)
      {
        for (int i = position0; i < position1; ++i)
          chars[count++] = original[i];
        for (int i = 0; i < replacement.Length; ++i)
          chars[count++] = replacement[i];
        position0 = position1 + pattern.Length;
      }
      if (position0 == 0) return original;
      for (int i = position0; i < original.Length; ++i)
        chars[count++] = original[i];
      return new string(chars, 0, count);
    }
  }
}
