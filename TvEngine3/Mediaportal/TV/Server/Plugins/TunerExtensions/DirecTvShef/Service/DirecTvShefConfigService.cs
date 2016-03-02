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

using System.Collections.Generic;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Request;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Response;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Service
{
  internal class DirecTvShefConfigService : IDirecTvShefConfigService
  {
    public delegate void OnSetTopBoxConfigChange(SetTopBoxConfig config);
    public event OnSetTopBoxConfigChange OnConfigChange;

    public SetTopBoxConfig GetSetTopBoxConfigurationForTuner(string tunerExternalId)
    {
      return SetTopBoxConfig.LoadSettings(tunerExternalId);
    }

    public void SaveSetTopBoxConfiguration(ICollection<SetTopBoxConfig> settings)
    {
      foreach (SetTopBoxConfig config in settings)
      {
        config.SaveSettings();
        if (OnConfigChange != null)
        {
          OnConfigChange(config);
        }
      }
    }

    public bool GetSetTopBoxLocations(string ipAddress, out IDictionary<string, string> locations)
    {
      this.LogDebug("DirecTV SHEF service: get locations, IP address = {0}", ipAddress);
      locations = new Dictionary<string, string>();

      ShefClient client = new ShefClient(ipAddress);
      IShefResponse shefResponse;
      if (!client.SendRequest(new ShefRequestGetLocations(), out shefResponse))
      {
        return false;
      }

      ShefResponseGetLocations response = shefResponse as ShefResponseGetLocations;
      if (response.Locations != null)
      {
        return false;
      }

      foreach (ShefLocation location in response.Locations)
      {
        locations.Add(location.LocationName, location.ClientAddress);
      }
      return true;
    }

    public bool GetSetTopBoxVersion(string ipAddress, out string accessCardId, out string receiverId, out string stbSoftwareVersion, out string shefVersion, out int systemTime)
    {
      this.LogDebug("DirecTV SHEF service: get version, IP address = {0}", ipAddress);
      ShefClient client = new ShefClient(ipAddress);
      IShefResponse shefResponse;
      bool result = client.SendRequest(new ShefRequestGetVersion(), out shefResponse);
      if (!result)
      {
        shefResponse = new ShefResponseGetVersion();
      }

      ShefResponseGetVersion response = shefResponse as ShefResponseGetVersion;
      accessCardId = response.AccessCardId;
      receiverId = response.ReceiverId;
      stbSoftwareVersion = response.StbSoftwareVersion;
      shefVersion = response.Version;
      systemTime = response.SystemTime;
      return result;
    }
  }
}