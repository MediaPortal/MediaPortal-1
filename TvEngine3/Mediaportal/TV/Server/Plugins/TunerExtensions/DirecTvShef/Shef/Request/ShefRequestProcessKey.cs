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
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Response;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Request
{
  internal class ShefRequestProcessKey : IShefRequest
  {
    private ShefRemoteKey _key = null;
    private ShefRemoteKeyPress _hold = null;

    public ShefRequestProcessKey(ShefRemoteKey key, ShefRemoteKeyPress hold = null)
    {
      _key = key;
      if (hold == null)
      {
        _hold = ShefRemoteKeyPress.Press;
      }
      else
      {
        _hold = hold;
      }
    }

    public string GetQueryUri()
    {
      return string.Format("remote/processKey?key={0}&hold={1}", _key.ToString(), _hold.ToString());
    }

    public Type GetResponseType()
    {
      return typeof(ShefResponseProcessKey);
    }
  }
}