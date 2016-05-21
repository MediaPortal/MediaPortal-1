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
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
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
    /// The device path of the transceiver which controls the set top box that
    /// is connected to the tuner.
    /// </summary>
    [DataMember]
    public string TransceiverDevicePath = string.Empty;

    /// <summary>
    /// The transmit port which controls the set top box.
    /// </summary>
    /// <remarks>
    /// If the transceiver has more than one transmit port, this property
    /// specifies which of the available transmit ports controls the set top
    /// box.
    /// </remarks>
    [DataMember]
    public TransmitPort TransmitPort = TransmitPort.None;

    /// <summary>
    /// The name of the profile the describes how to control the set top box.
    /// </summary>
    [DataMember]
    public string ProfileName = string.Empty;

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
        string externalId = SettingsManagement.GetValue("microsoftBlasterTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(externalId))
        {
          return new TunerSetTopBoxConfig(tunerExternalId);
        }
        if (externalId.Equals(tunerExternalId))
        {
          TunerSetTopBoxConfig config = new TunerSetTopBoxConfig(tunerExternalId);
          config.TransceiverDevicePath = SettingsManagement.GetValue("microsoftBlasterTransceiverDevicePath" + i, string.Empty);
          config.TransmitPort = (TransmitPort)SettingsManagement.GetValue("microsoftBlasterTransmitPort" + i, (int)TransmitPort.Port1);
          config.ProfileName = SettingsManagement.GetValue("microsoftBlasterProfileName" + i, string.Empty);
          config.IsPowerControlEnabled = SettingsManagement.GetValue("microsoftBlasterIsPowerControlEnabled" + i, false);
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
        string tunerExternalId = SettingsManagement.GetValue("microsoftBlasterTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(tunerExternalId) || tunerExternalId.Equals(TunerExternalId))
        {
          SettingsManagement.SaveValue("microsoftBlasterTunerExternalId" + i, TunerExternalId);
          SettingsManagement.SaveValue("microsoftBlasterTransceiverDevicePath" + i, TransceiverDevicePath);
          SettingsManagement.SaveValue("microsoftBlasterTransmitPort" + i, (int)TransmitPort);
          SettingsManagement.SaveValue("microsoftBlasterProfileName" + i, ProfileName);
          SettingsManagement.SaveValue("microsoftBlasterIsPowerControlEnabled" + i, IsPowerControlEnabled);
          return;
        }
        i++;
      }
    }
  }
}