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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef
{
  /// <summary>
  /// Configuration properties for a set top box.
  /// </summary>
  [DataContract]
  internal class SetTopBoxConfig
  {
    /// <summary>
    /// The external identifier of the tuner that the set top box is connected to.
    /// </summary>
    [DataMember]
    public string TunerExternalId;

    /// <summary>
    /// The IP address of the set top box.
    /// </summary>
    [DataMember]
    public string IpAddress;

    /// <summary>
    /// <c>True</c> if this extension should turn the set top box power on and
    /// off as required.
    /// </summary>
    [DataMember]
    public bool EnablePowerControl;

    private SetTopBoxConfig(string tunerExternalId, string ipAddress, bool enablePowerControl)
    {
      TunerExternalId = tunerExternalId;
      IpAddress = ipAddress;
      EnablePowerControl = enablePowerControl;
    }

    /// <summary>
    /// Load the set top box settings for a tuner.
    /// </summary>
    /// <param name="tunerExternalId">The tuner's external identifier.</param>
    public static SetTopBoxConfig LoadSettings(string tunerExternalId)
    {
      byte i = 0;
      while (true)  // Loop until we don't find any more settings.
      {
        string externalId = SettingsManagement.GetValue("direcTvShefTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(externalId))
        {
          return new SetTopBoxConfig(tunerExternalId, string.Empty, true);
        }
        if (externalId.Equals(tunerExternalId))
        {
          return new SetTopBoxConfig(
            tunerExternalId,
            SettingsManagement.GetValue("direcTvShefIpAddress" + i, string.Empty),
            SettingsManagement.GetValue("direcTvShefEnablePowerControl" + i, true)
          );
        }
        i++;
      }
    }

    /// <summary>
    /// Save the set top box settings to the database.
    /// </summary>
    public void SaveSettings()
    {
      byte i = 0;
      while (true)  // Loop until we find existing settings or a free space to store new settings.
      {
        string tunerExternalId = SettingsManagement.GetValue("direcTvShefTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(tunerExternalId) || tunerExternalId.Equals(TunerExternalId))
        {
          SettingsManagement.SaveValue("direcTvShefTunerExternalId" + i, TunerExternalId);
          SettingsManagement.SaveValue("direcTvShefIpAddress" + i, IpAddress);
          SettingsManagement.SaveValue("direcTvShefEnablePowerControl" + i, EnablePowerControl);
          return;
        }
        i++;
      }
    }
  }
}