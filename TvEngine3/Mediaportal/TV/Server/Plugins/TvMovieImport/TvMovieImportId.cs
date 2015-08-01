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

namespace Mediaportal.TV.Server.Plugins.TvMovieImport
{
  internal class TvMovieImportId
  {
    private const string TV_MOVIE_EXTERNAL_ID_PREFIX = "tvmovie";

    public static bool HasTvMovieMapping(string channelExternalId)
    {
      return channelExternalId != null && channelExternalId.StartsWith(TV_MOVIE_EXTERNAL_ID_PREFIX);
    }

    public static string GetQualifiedIdForChannel(string tvMovieChannelName)
    {
      return string.Format("{0}|{1}", TV_MOVIE_EXTERNAL_ID_PREFIX, tvMovieChannelName);
    }

    public static void GetQualifiedIdComponents(string channelExternalId, out string tvMovieChannelName)
    {
      string[] parts = channelExternalId.Split('|');
      if (parts.Length == 2)
      {
        tvMovieChannelName = parts[1];
      }
      else if (parts.Length > 2)
      {
        tvMovieChannelName = string.Join("|", parts, 1, parts.Length - 1);
      }
      else
      {
        throw new Exception(string.Format("Failed to split invalid TV Movie external/mapping ID \"{0}\".", channelExternalId));
      }
    }
  }
}