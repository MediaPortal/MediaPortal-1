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
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  /// <summary>
  /// Configuration properties for a set top box.
  /// </summary>
  [DataContract]
  internal class SetTopBoxConfig
  {
    /// <summary>
    /// The external identifier of the tuner that the set top box is connected
    /// to.
    /// </summary>
    [DataMember]
    public string TunerExternalId = string.Empty;

    /// <summary>
    /// The device path of the transceiver which controls the set top box.
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

    private SetTopBoxConfig(string tunerExternalId)
    {
      TunerExternalId = tunerExternalId;
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
        string externalId = SettingsManagement.GetValue("microsoftBlasterTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(externalId))
        {
          return new SetTopBoxConfig(tunerExternalId);
        }
        if (externalId.Equals(tunerExternalId))
        {
          SetTopBoxConfig config = new SetTopBoxConfig(tunerExternalId);
          config.TransceiverDevicePath = SettingsManagement.GetValue("microsoftBlasterTransceiverDevicePath" + i, string.Empty);
          config.TransmitPort = (TransmitPort)SettingsManagement.GetValue("microsoftBlasterTransmitPort" + i, (int)TransmitPort.Port1);
          config.ProfileName = SettingsManagement.GetValue("microsoftBlasterProfileName" + i, string.Empty);
          config.IsPowerControlEnabled = SettingsManagement.GetValue("microsoftBlasterIsPowerControlEnabled" + i, false);
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