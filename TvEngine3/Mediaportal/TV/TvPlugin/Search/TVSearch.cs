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

using Mediaportal.TV.Server.Common.Types.Enum;
using MediaPortal.Util;

namespace Mediaportal.TV.TvPlugin.Search
{
  public class TvSearch : SearchBase
  {
    public TvSearch()
    {
      GetID = (int)Window.WINDOW_SEARCHTV;
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    protected override string SkinFileName
    {
      get
      {
        return @"\mytvsearch.xml";
      }
    }

    protected override MediaType MediaType
    {
      get
      {
        return MediaType.Television;
      }
    }

    protected override string ThumbsType
    {
      get
      {
        return Thumbs.TVChannel;
      }
    }

    protected override string SkinPropertyPrefix
    {
      get
      {
        return "#TV";
      }
    }

    protected override string DefaultLogo
    {
      get
      {
        return "defaultVideoBig.png";
      }
    }

    protected override string SettingsSection
    {
      get
      {
        return "tvsearch";
      }
    }
  }
}