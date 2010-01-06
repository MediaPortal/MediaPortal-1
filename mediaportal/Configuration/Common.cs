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
using System.Net;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration
{
  internal class Common
  {
    public static bool IsSingleSeat()
    {
      bool singleSeat = false;
      if (Util.Utils.UsingTvServer)
      {
        string servername;
        using (Settings xmlreader = new MPSettings())
        {
          servername = xmlreader.GetValueAsString("tvservice", "hostname", Environment.MachineName);
        }
        if (servername.ToLowerInvariant() == Environment.MachineName.ToLowerInvariant())
        {
          Log.Debug("Configuration: IsSingleSeat - MPSettings.HostName = {0} / Environment.MachineName = {1}",
                    servername, Environment.MachineName);
          singleSeat = true;
        }
        else
        {
          IPHostEntry ipEntry = Dns.GetHostEntry(Environment.MachineName);
          IPAddress[] addr = ipEntry.AddressList;

          for (int i = 0; i < addr.Length; i++)
          {
            if (addr[i].ToString().Equals(servername))
            {
              Log.Debug(
                "Configuration: IsSingleSeat - MPSettings.HostName = {0} / Dns.GetHostEntry(Environment.MachineName) = {1}",
                servername, addr[i]);
              singleSeat = true;
              break;
            }
          }
        }
      }
      return singleSeat;
    }
  }
}