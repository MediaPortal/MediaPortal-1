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
  /// Configuration for the set top box associated with a tuner.
  /// </summary>
  [DataContract]
  internal class TunerSetTopBoxConfig
  {
    /// <summary>
    /// The tuner's external identifier.
    /// </summary>
    [DataMember]
    public string TunerExternalId = string.Empty;

    /// <summary>
    /// The IP address of the set top box that is connected to the tuner.
    /// </summary>
    /// <remarks>
    /// If the set top box is a Genie Mini, this should be the IP address of
    /// the Genie that the Genie Mini is linked to.
    /// </remarks>
    [DataMember]
    public string IpAddress = string.Empty;

    /// <summary>
    /// The location of the set top box.
    /// </summary>
    /// <remarks>
    /// This property is only applicable for the Genie Mini set top box model.
    /// The value is a unique, human-readable description entered by the owner
    /// when the Genie Mini is linked to a Genie.
    /// </remarks>
    [DataMember]
    public string Location = string.Empty;

    /// <summary>
    /// The MAC address of the set top box.
    /// </summary>
    /// <remarks>
    /// This property is only applicable for the Genie Mini set top box model.
    /// </remarks>
    [DataMember]
    public string MacAddress = string.Empty;

    /// <summary>
    /// <c>True</c> if this extension should turn the set top box power on and
    /// off to minimise power use.
    /// </summary>
    [DataMember]
    public bool IsPowerControlEnabled = false;

    private TunerSetTopBoxConfig(string tunerExternalId)
    {
      TunerExternalId = tunerExternalId;
    }

    /// <summary>
    /// Load the configuration for a tuner.
    /// </summary>
    /// <param name="tunerExternalId">The tuner's external identifier.</param>
    public static TunerSetTopBoxConfig Load(string tunerExternalId)
    {
      byte i = 0;
      while (true)  // Loop until we don't find any more configuration.
      {
        string externalId = SettingsManagement.GetValue("direcTvShefTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(externalId))
        {
          return new TunerSetTopBoxConfig(tunerExternalId);
        }
        if (externalId.Equals(tunerExternalId))
        {
          TunerSetTopBoxConfig config = new TunerSetTopBoxConfig(tunerExternalId);
          config.IpAddress = SettingsManagement.GetValue("direcTvShefIpAddress" + i, string.Empty);
          config.Location = SettingsManagement.GetValue("direcTvShefLocation" + i, string.Empty);
          config.MacAddress = SettingsManagement.GetValue("direcTvShefMacAddress" + i, string.Empty);
          config.IsPowerControlEnabled = SettingsManagement.GetValue("direcTvShefIsPowerControlEnabled" + i, false);
        }
        i++;
      }
    }

    /// <summary>
    /// Save the configuration for a tuner.
    /// </summary>
    public void Save()
    {
      byte i = 0;
      while (true)  // Loop until we find existing configuration for the tuner or a free space to store new configuration.
      {
        string tunerExternalId = SettingsManagement.GetValue("direcTvShefTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(tunerExternalId) || tunerExternalId.Equals(TunerExternalId))
        {
          SettingsManagement.SaveValue("direcTvShefTunerExternalId" + i, TunerExternalId);
          SettingsManagement.SaveValue("direcTvShefIpAddress" + i, IpAddress);
          SettingsManagement.SaveValue("direcTvShefLocation" + i, Location);
          SettingsManagement.SaveValue("direcTvShefMacAddress" + i, MacAddress);
          SettingsManagement.SaveValue("direcTvShefIsPowerControlEnabled" + i, IsPowerControlEnabled);
          return;
        }
        i++;
      }
    }
  }
}