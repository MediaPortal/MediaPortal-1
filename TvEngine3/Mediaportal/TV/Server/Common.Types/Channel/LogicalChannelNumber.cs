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

using System.Text.RegularExpressions;

namespace Mediaportal.TV.Server.Common.Types.Channel
{
  public static class LogicalChannelNumber
  {
    public static readonly Regex FORMAT = new Regex(@"^(\d+)([\.\-](\d+))?$");
    public const char SEPARATOR = '.';

    // If we were to use zero or empty string, sorting channel groups by number
    // would place the channels with default numbers first. That would be
    // undesirable.
    public static readonly string GLOBAL_DEFAULT = "10000";

    public static bool Create(ushort part1, out string lcn, ushort? part2 = null)
    {
      lcn = string.Empty;
      if (part1 == 0)
      {
        return false;
      }
      if (part2.HasValue && part2.Value > 0)
      {
        lcn = string.Format("{0}{1}{2}", part1, SEPARATOR, part2);
        return true;
      }
      lcn = part1.ToString();
      return true;
    }

    public static bool Create(string candidate, out string lcn)
    {
      lcn = null;
      ushort part1;
      ushort? part2;
      if (!Parse(candidate, false, out part1, out part2))
      {
        return false;
      }
      return Create(part1, out lcn, part2);
    }

    public static bool Parse(string lcn, out ushort part1, out ushort? part2)
    {
      return Parse(lcn, true, out part1, out part2);
    }

    private static bool Parse(string lcn, bool isStrict, out ushort part1, out ushort? part2)
    {
      part1 = 0;
      part2 = null;
      if (string.IsNullOrEmpty(lcn))
      {
        return false;
      }

      Match m = FORMAT.Match(lcn);
      if (!m.Success)
      {
        return false;
      }

      if (m.Groups[3].Captures.Count != 0)
      {
        char separator = m.Groups[2].Captures[0].Value[0];
        if (separator != SEPARATOR && isStrict)
        {
          return false;
        }
        ushort temp;
        if (!ushort.TryParse(m.Groups[3].Captures[0].Value, out temp) || (temp == 0 && isStrict))
        {
          return false;
        }
        if (temp != 0)
        {
          part2 = temp;
        }
      }
      return ushort.TryParse(m.Groups[1].Captures[0].Value, out part1) && part1 != 0;
    }
  }
}