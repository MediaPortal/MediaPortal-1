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

using System.Text.RegularExpressions;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  internal class Mc2XmlId
  {
    // By inspection, format seems to be...
    // ATSC: I<major>.<minor>.<internal ID>.<organisation>
    // cable: I<channel number>.<source>
    // Examples...
    // I13.2.35423.schedulesdirect.org = 13.2 KOVRDT2 (KOVR-DT2)
    // I15.28460357.microsoft.com = 15 DSC (The Discovery Channel [West])
    private static readonly Regex REGEX_MC2XML_ID_FORMAT = new Regex(@"^I(\d+)\.((\d+)\.)?(\d+)\.(microsoft\.com|schedulesdirect\.org)$");

    public static bool GetComponents(string identifier, out int majorChannelNumber, out int minorChannelNumber, out string internalIdentifier, out string organisation)
    {
      majorChannelNumber = -1;
      minorChannelNumber = -1;
      internalIdentifier = string.Empty;
      organisation = string.Empty;
      if (string.IsNullOrEmpty(identifier))
      {
        return false;
      }

      Match m = REGEX_MC2XML_ID_FORMAT.Match(identifier);
      if (!m.Success)
      {
        return false;
      }

      majorChannelNumber = int.Parse(m.Groups[1].Captures[0].Value);
      if (m.Groups[3].Success)
      {
        minorChannelNumber = int.Parse(m.Groups[3].Captures[0].Value);
      }
      internalIdentifier = m.Groups[4].Captures[0].Value;
      organisation = m.Groups[5].Captures[0].Value;
      return true;
    }

    public static bool IsMatch(string identifier1, string identifier2)
    {
      // Check for recoverable mc2xml guide data channel ID changes:
      // http://forums.gbpvr.com/showthread.php?55741-Changing-Channel-Mapping-Ids-Causing-Blank-Guide
      // I81.28458625.microsoft.com => I82.28458625.microsoft.com

      if (string.IsNullOrEmpty(identifier1) || string.IsNullOrEmpty(identifier2))
      {
        return false;
      }

      Match m1 = REGEX_MC2XML_ID_FORMAT.Match(identifier1);
      if (!m1.Success)
      {
        return false;
      }

      Match m2 = REGEX_MC2XML_ID_FORMAT.Match(identifier2);
      if (!m2.Success)
      {
        return false;
      }

      // Check if internal identifier and organisation match.
      return string.Equals(m1.Groups[4].Captures[0].Value, m2.Groups[4].Captures[0].Value) && string.Equals(m1.Groups[5].Captures[0].Value, m2.Groups[5].Captures[0].Value);
    }
  }
}