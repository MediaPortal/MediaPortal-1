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

using System.ComponentModel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// Supported device types.
  /// </summary>
  public enum CardType
  {
    /// <summary>
    /// Analog tuner or capture device.
    /// </summary>
    Analog,
    /// <summary>
    /// DVB-S or DVB-S2 tuner. ISDB-S may also be supported.
    /// </summary>
    DvbS,
    /// <summary>
    /// DVB-T or DVB-T2 tuner. ISDB-T may also be supported.
    /// </summary>
    DvbT,
    /// <summary>
    /// DVB-C tuner.
    /// </summary>
    DvbC,
    /// <summary>
    /// ATSC and/or annex-C North American cable tuner.
    /// </summary>
    Atsc,
    /// <summary>
    /// Internet radio stream tuner.
    /// </summary>
    RadioWebStream,
    /// <summary>
    /// DVB-IP tuner.
    /// </summary>
    DvbIP,
    /// <summary>
    /// Unknown device.
    /// </summary>
    Unknown
  }

  /// <summary>
  /// Device idle modes. A device's idle mode determines the action that will be taken when a device is no longer
  /// being actively used.
  /// </summary>
  public enum DeviceIdleMode
  {
    /// <summary>
    /// For a BDA device, pause the DirectShow/BDA graph.
    /// - not supported by some devices
    /// - average power use (device dependent)
    /// - fast first (re)tune
    /// </summary>
    Pause,
    /// <summary>
    /// For a BDA device, stop the DirectShow/BDA graph.
    /// - highly compatible
    /// - low power use (device dependent)
    /// - average/default first (re)tune speed
    /// </summary>
    Stop,
    /// <summary>
    /// For a BDA device, dismantle the DirectShow/BDA graph.
    /// - ultimate compatibility
    /// - minimal power use
    /// - slowest first (re)tune.
    /// </summary>
    Unload,
    /// <summary>
    /// For a BDA device, keep the DirectShow/BDA graph running.
    /// - reasonable compatibility
    /// - highest power use
    /// - fastest possible first (re)tune.
    /// </summary>
    [Description("Always On")]
    AlwaysOn
  }

  /// <summary>
  /// Device actions. Plugins can specify actions at certain stages of the device lifecycle that optimise
  /// compatibility, performance, power use etc.
  /// </summary>
  /// <remarks>
  /// The order specified here is important. When there is a conflict between plugins, the action with the
  /// highest value will be performed (eg. stop would be performed in preference to pause). The order is
  /// intended to have more compatible actions listed higher than unusual or less compatible actions.
  /// </remarks>
  public enum DeviceAction
  {
    /// <summary>
    /// Default behaviour will continue. No alternate action will be taken.
    /// </summary>
    Default,
    /// <summary>
    /// Start the device.
    /// For a BDA device, start the DirectShow/BDA graph.
    /// </summary>
    Start,
    /// <summary>
    /// Pause the device.
    /// For a BDA device, pause the DirectShow/BDA graph.
    /// </summary>
    Pause,
    /// <summary>
    /// Stop the device.
    /// For a BDA device, stop the DirectShow/BDA graph.
    /// </summary>
    Stop,
    /// <summary>
    /// Retart the device.
    /// For a BDA device, stop then restart the DirectShow/BDA graph.
    /// </summary>
    Restart,
    /// <summary>
    /// Reset the device.
    /// For a BDA device, rebuild the DirectShow/BDA graph.
    /// </summary>
    Reset,
    /// <summary>
    /// Unload the device.
    /// For a BDA device, dismantle the DirectShow/BDA graph.
    /// </summary>
    Unload
  }

  /// <summary>
  /// PID filter modes.
  /// </summary>
  /// <remarks>
  /// Plugins can implement PID filters to support devices connected via low bandwith or bandwidth sensitive
  /// connections.
  /// </remarks>
  public enum PidFilterMode
  {
    /// <summary>
    /// The PID filter will be disabled.
    /// </summary>
    Disabled,
    /// <summary>
    /// The PID filter will be enabled.
    /// </summary>
    Enabled,
    /// <summary>
    /// The PID filter will be enabled or disabled based on logic in the plugin.
    /// </summary>
    Auto
  }

  /// <summary>
  /// Modes for decrypting multiple services from a single transponder/multiplex simultaneously.
  /// </summary>
  /// <remarks>
  /// The mode is applied within TvLibrary and appropriate commands are sent to conditional access plugins. In
  /// other words, plugins should not have to contain complex logic.
  /// </remarks>
  public enum MultiChannelDecryptMode
  {
    /// <summary>
    /// Send only one service.
    /// </summary>
    Disabled,
    /// <summary>
    /// Send the current list of services that must be decrypted. Most compatible.
    /// </summary>
    List,
    /// <summary>
    /// Send only the service that is being added to or removed from the list.
    /// </summary>
    Changes
  }

  /// <summary>
  /// Conditional access module types.
  /// </summary>
  /// <remarks>
  /// In some cases, specific handling is required for different types of CAMs.
  /// </remarks>
  public enum CamType
  {
    /// <summary>
    /// Default
    /// </summary>
    Default = 0,
    /// <summary>
    /// Astoncrypt 2
    /// </summary>
    [Description("Astoncrypt 2")]
    Astoncrypt2 = 1
  }

  /// <summary>
  /// Possible service free-to-air/encrypted transmission modes.
  /// </summary>
  public enum EncryptionMode
  {
    /// <summary>
    /// Always encrypted.
    /// </summary>
    Encrypted,
    /// <summary>
    /// Sometimes free-to-air; sometimes encrypted.
    /// </summary>
    Mixed,
    /// <summary>
    /// Always free-to-air.
    /// </summary>
    Clear
  }
}