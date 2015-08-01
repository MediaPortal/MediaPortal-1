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

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  internal class XmlTvImportId
  {
    private const string XMLTV_EXTERNAL_ID_PREFIX = "xmltv";

    public static bool HasXmlTvMapping(string channelExternalId)
    {
      return channelExternalId != null && channelExternalId.StartsWith(XMLTV_EXTERNAL_ID_PREFIX);
    }

    public static string GetQualifiedIdForChannel(string fileName, string xmlTvChannelId)
    {
      return string.Format("{0}|{1}|{2}", XMLTV_EXTERNAL_ID_PREFIX, fileName, xmlTvChannelId);
    }

    public static void GetQualifiedIdComponents(string channelExternalId, out string fileName, out string xmlTvChannelId)
    {
      string[] parts = channelExternalId.Split('|');
      if (parts.Length == 3)
      {
        fileName = parts[1];
        xmlTvChannelId = parts[2];
      }
      else if (parts.Length > 3)
      {
        fileName = parts[1];
        xmlTvChannelId = string.Join("|", parts, 2, parts.Length - 2);
      }
      else
      {
        throw new Exception(string.Format("Failed to split invalid XMLTV external/mapping ID \"{0}\".", channelExternalId));
      }
    }
  }
}