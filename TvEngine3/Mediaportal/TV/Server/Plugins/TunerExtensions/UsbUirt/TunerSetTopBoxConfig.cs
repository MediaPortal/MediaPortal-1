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
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt
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
    /// The device path of the USB-UIRT which controls the set top box that is
    /// connected to the tuner.
    /// </summary>
    [DataMember]
    public int UsbUirtIndex = -1;

    /// <summary>
    /// The transmit zone which is associated with the set top box.
    /// </summary>
    /// <remarks>
    /// If the USB-UIRT has more than one set top box connected to it, this
    /// property specifies which of the transmit zones should be used to
    /// control the set top box associated with this tuner.
    /// </remarks>
    [DataMember]
    public TransmitZone TransmitZone = TransmitZone.None;

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
        string externalId = SettingsManagement.GetValue("usbUirtTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(externalId))
        {
          return new TunerSetTopBoxConfig(tunerExternalId);
        }
        if (externalId.Equals(tunerExternalId))
        {
          TunerSetTopBoxConfig config = new TunerSetTopBoxConfig(tunerExternalId);
          config.UsbUirtIndex = SettingsManagement.GetValue("usbUirtIndex" + i, -1);
          config.TransmitZone = (TransmitZone)SettingsManagement.GetValue("usbUirtTransmitZone" + i, (int)TransmitZone.All);
          config.ProfileName = SettingsManagement.GetValue("usbUirtProfileName" + i, string.Empty);
          config.IsPowerControlEnabled = SettingsManagement.GetValue("usbUirtIsPowerControlEnabled" + i, false);
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
        string tunerExternalId = SettingsManagement.GetValue("usbUirtTunerExternalId" + i, string.Empty);
        if (string.IsNullOrEmpty(tunerExternalId) || tunerExternalId.Equals(TunerExternalId))
        {
          SettingsManagement.SaveValue("usbUirtTunerExternalId" + i, TunerExternalId);
          SettingsManagement.SaveValue("usbUirtIndex" + i, UsbUirtIndex);
          SettingsManagement.SaveValue("usbUirtTransmitZone" + i, (int)TransmitZone);
          SettingsManagement.SaveValue("usbUirtProfileName" + i, ProfileName);
          SettingsManagement.SaveValue("usbUirtIsPowerControlEnabled" + i, IsPowerControlEnabled);
          return;
        }
        i++;
      }
    }
  }
}