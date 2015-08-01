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
  public class RadioSearch : SearchBase
  {
    public RadioSearch()
    {
      GetID = (int)Window.WINDOW_SEARCH_RADIO;
    }

    public override bool IsTv
    {
      get
      {
        return false;
      }
    }

    protected override string SkinFileName
    {
      get
      {
        return @"\myradiosearch.xml";
      }
    }

    protected override MediaType MediaType
    {
      get
      {
        return MediaType.Radio;
      }
    }

    protected override string ThumbsType
    {
      get
      {
        return Thumbs.Radio;
      }
    }

    protected override string SkinPropertyPrefix
    {
      get
      {
        return "#Radio";
      }
    }

    protected override string DefaultLogo
    {
      get
      {
        return "defaultMyradioBig.png";
      }
    }

    protected override string SettingsSection
    {
      get
      {
        return "radiosearch";
      }
    }
  }
}