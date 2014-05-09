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
using System.ComponentModel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// Supported tuner types.
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
    /// ATSC and/or SCTE (ITU-T annex B) North American cable tuner.
    /// </summary>
    Atsc,
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
  /// Observable stream types.
  /// </summary>
  public enum PidType
  {
    /// <summary>
    /// Video stream.
    /// </summary>
    Video = 0,
    /// <summary>
    /// Audio stream.
    /// </summary>
    Audio,
    /// <summary>
    /// Other stream.
    /// </summary>
    Other
  }

  /// <summary>
  /// Idle modes. An idle mode determines the action that will be taken when a tuner is no longer
  /// being actively used.
  /// </summary>
  public enum IdleMode
  {
    /// <summary>
    /// For a tuner that presents a DirectShow/BDA filter interface, pause the graph.
    /// - not supported by some tuners
    /// - average power use (tuner dependent)
    /// - fast first (re)tune
    /// </summary>
    Pause,
    /// <summary>
    /// For a tuner that presents a DirectShow/BDA filter interface, stop the graph.
    /// - highly compatible
    /// - low power use (tuner dependent)
    /// - average/default first (re)tune speed
    /// </summary>
    Stop,
    /// <summary>
    /// For a tuner that presents a DirectShow/BDA filter interface, dismantle and dispose the graph.
    /// - ultimate compatibility
    /// - minimal power use
    /// - slowest first (re)tune
    /// </summary>
    Unload,
    /// <summary>
    /// For a tuner that presents a DirectShow/BDA filter interface, keep the graph running.
    /// - reasonable compatibility
    /// - highest power use
    /// - fastest possible first (re)tune
    /// </summary>
    [Description("Always On")]
    AlwaysOn
  }

  /// <summary>
  /// Tuner actions. Plugins can specify actions at certain stages of the tuner lifecycle that optimise
  /// compatibility, performance, power use etc.
  /// </summary>
  /// <remarks>
  /// The order specified here is important. When there is a conflict between plugins, the action with the
  /// highest value will be performed (eg. stop would be performed in preference to pause). The order is
  /// intended to have more compatible actions listed higher than unusual or less compatible actions.
  /// </remarks>
  public enum TunerAction
  {
    /// <summary>
    /// Default behaviour will continue. No alternate action will be taken.
    /// </summary>
    Default,
    /// <summary>
    /// Start the tuner.
    /// For a tuner that presents a DirectShow/BDA filter interface, run the graph.
    /// </summary>
    Start,
    /// <summary>
    /// Pause the tuner.
    /// For a tuner that presents a DirectShow/BDA filter interface, pause the graph.
    /// </summary>
    Pause,
    /// <summary>
    /// Stop the tuner.
    /// For a tuner that presents a DirectShow/BDA filter interface, stop the graph.
    /// </summary>
    Stop,
    /// <summary>
    /// Retart the tuner.
    /// For a tuner that presents a DirectShow/BDA filter interface, stop then run the graph.
    /// </summary>
    Restart,
    /// <summary>
    /// Reset the tuner.
    /// For a tuner that presents a DirectShow/BDA filter interface, rebuild the graph.
    /// </summary>
    Reset,
    /// <summary>
    /// Unload the tuner.
    /// For a tuner that presents a DirectShow/BDA filter interface, dismantle and dispose the graph.
    /// </summary>
    Unload
  }

  /// <summary>
  /// Tuner state. For a tuner that presents a DirectShow/BDA filter interface, this measure of
  /// state roughly mirrors the state of the graph.
  /// </summary>
  public enum TunerState
  {
    /// <summary>
    /// The tuner is not yet loaded. It must be loaded/initialised before any interaction may occur.
    /// </summary>
    [Description("Not Loaded")]
    NotLoaded,
    /// <summary>
    /// The tuner is being loaded.
    /// </summary>
    Loading,
    /// <summary>
    /// The tuner is loaded but not paused or started.
    /// </summary>
    Stopped,
    /// <summary>
    /// The tuner is paused.
    /// </summary>
    Paused,
    /// <summary>
    /// The tuner is started. Note a tuner may be started and idle if the tuner is configured to
    /// use the always on idle mode.
    /// </summary>
    Started
  }

  /// <summary>
  /// PID filter modes.
  /// </summary>
  /// <remarks>
  /// Plugins can implement PID filters to support tuners connected via low bandwith or bandwidth sensitive
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

  /// <summary>
  /// ATSC and SCTE modulation modes.
  /// </summary>
  public enum AtscScteModulation
  {
    /// <summary>
    /// Auto.
    /// </summary>
    Auto = 100,
    /// <summary>
    /// ATSC 8 VSB.
    /// </summary>
    [Description("8 VSB")]
    Atsc_8Vsb,
    /// <summary>
    /// ATSC 16 VSB.
    /// </summary>
    [Description("16 VSB")]
    Atsc_16Vsb,
    /// <summary>
    /// 64 QAM.
    /// </summary>
    [Description("64 QAM")]
    Scte_64Qam,
    /// <summary>
    /// 256 QAM.
    /// </summary>
    [Description("256 QAM")]
    Scte_256Qam
  }

  /// <summary>
  /// Cable (DVB-C and ISDB-C) modulation modes.
  /// </summary>
  public enum CableModulation
  {
    /// <summary>
    /// Auto.
    /// </summary>
    Auto = 100,
    /// <summary>
    /// 16 QAM.
    /// </summary>
    [Description("16 QAM")]
    Qam16,
    /// <summary>
    /// 32 QAM.
    /// </summary>
    [Description("32 QAM")]
    Qam32,
    /// <summary>
    /// 32 QAM.
    /// </summary>
    [Description("64 QAM")]
    Qam64,
    /// <summary>
    /// 128 QAM.
    /// </summary>
    [Description("128 QAM")]
    Qam128,
    /// <summary>
    /// 256 QAM.
    /// </summary>
    [Description("256 QAM")]
    Qam256,
    /// <summary>
    /// 512 QAM.
    /// </summary>
    [Description("512 QAM")]
    Qam512,
    /// <summary>
    /// 1024 QAM.
    /// </summary>
    [Description("1024 QAM")]
    Qam1024,
    /// <summary>
    /// 2048 QAM.
    /// </summary>
    [Description("2048 QAM")]
    Qam2048,
    /// <summary>
    /// 4096 QAM.
    /// </summary>
    [Description("4096 QAM")]
    Qam4096
  }

  /// <summary>
  /// Satellite (DVB-S, DVB-DSNG, DVB-S2, DC II, DSS and ISDB-S) modulation modes.
  /// </summary>
  public enum SatelliteModulation
  {
    /// <summary>
    /// Auto.
    /// </summary>
    Auto = 100,
    // This is set to separate the value range from the BDA ModulationType range. Plugins may need to
    // translate to specific ModulationType values before the tuning translation.
    /// <summary>
    /// 16 QAM.
    /// </summary>
    [Description("16 QAM")]
    Qam16,
    /// <summary>
    /// BPSK.
    /// </summary>
    [Description("BPSK")]
    Psk2,
    /// <summary>
    /// QPSK.
    /// </summary>
    [Description("QPSK")]
    Psk4,
    /// <summary>
    /// 8 PSK.
    /// </summary>
    [Description("8 PSK")]
    Psk8,
    /// <summary>
    /// 16 [A]PSK.
    /// </summary>
    [Description("16 PSK")]
    Psk16,
    /// <summary>
    /// 32 [A]PSK.
    /// </summary>
    [Description("32 PSK")]
    Psk32,
    /// <summary>
    /// Offset QPSK (used for DigiCipher 2).
    /// </summary>
    [Description("Offset QPSK")]
    OffsetQpsk,
    /// <summary>
    /// Split-mode QPSK (used for DigiCipher 2).
    /// </summary>
    [Description("Split-mode QPSK")]
    SplitQpsk,
    /// <summary>
    /// VCM (DVB-S2 variable coding and modulation).
    /// </summary>
    [Description("VCM")]
    Vcm,
    /// <summary>
    /// ACM (DVB-S2 adaptive coding and modulation).
    /// </summary>
    [Description("ACM")]
    Acm
  }

  /// <summary>
  /// Broadcast standards.
  /// </summary>
  [Flags]
  public enum BroadcastStandard
  {
    /// <summary>
    /// Analog input, for example s-video, composite etc.
    /// </summary>
    [Description("Analog Input")]
    AnalogInput = 0x00000001,
    /// <summary>
    /// Digital input, for example HDMI.
    /// </summary>
    [Description("Digital Input")]
    DigitalInput = 0x00000002,
    /// <summary>
    /// Analog television (NTSC, PAL, SECAM) transmitted over-the-air.
    /// </summary>
    [Description("Analog Terrestrial Television")]
    AnalogTerrestrialTelevision = 0x00000004,
    /// <summary>
    /// Analog television (NTSC, PAL, SECAM) transmitted via cable.
    /// </summary>
    [Description("Analog Cable Television")]
    AnalogCableTelevision = 0x00000008,
    /// <summary>
    /// Amplitude-modulated analog radio.
    /// </summary>
    [Description("AM Radio")]
    AmRadio = 0x00000010,
    /// <summary>
    /// Frequency-modulated analog radio.
    /// </summary>
    [Description("FM Radio")]
    FmRadio = 0x00000020,
    /// <summary>
    /// First generation Digital Video Broadcast standard for cable broadcast.
    /// </summary>
    [Description("DVB-C")]
    DvbC = 0x00000040,
    /// <summary>
    /// Second generation Digital Video Broadcast standard for cable broadcast.
    /// </summary>
    [Description("DVB-C2")]
    DvbC2 = 0x00000080,
    /// <summary>
    /// Digital Video Broadcast standard for digital satellite news gathering.
    /// </summary>
    [Description("DVB-DSNG")]
    DvbDsng = 0x00000100,
    /// <summary>
    /// Digital Video Broadcast standard for internet protocol broadcast.
    /// </summary>
    [Description("DVB-IP")]
    DvbIp = 0x00000200,
    /// <summary>
    /// First generation Digital Video Broadcast standard for satellite broadcast.
    /// </summary>
    [Description("DVB-S")]
    DvbS = 0x00000400,
    /// <summary>
    /// Second generation Digital Video Broadcast standard for satellite broadcast.
    /// </summary>
    [Description("DVB-S2")]
    DvbS2 = 0x00000800,
    /// <summary>
    /// First generation Digital Video Broadcast standard for terrestrial broadcast.
    /// </summary>
    [Description("DVB-T")]
    DvbT = 0x00001000,
    /// <summary>
    /// Second generation Digital Video Broadcast standard for terrestrial broadcast.
    /// </summary>
    [Description("DVB-T2")]
    DvbT2 = 0x00002000,
    /// <summary>
    /// Advanced Television Systems Committee standard for terrestrial broadcast.
    /// </summary>
    [Description("ATSC")]
    Atsc = 0x00004000,
    /// <summary>
    /// Society of Cable and Telecommunications Engineers standard for cable broadcast.
    /// </summary>
    [Description("SCTE")]
    Scte = 0x00008000,
    /// <summary>
    /// Integrated Services Digital Broadcasting standard for cable broadcast.
    /// </summary>
    [Description("ISDB-C")]
    IsdbC = 0x00010000,
    /// <summary>
    /// Integrated Services Digital Broadcasting standard for satellite broadcast.
    /// </summary>
    [Description("ISDB-S")]
    IsdbS = 0x00020000,
    /// <summary>
    /// Integrated Services Digital Broadcasting standard for terrestrial broadcast.
    /// </summary>
    [Description("ISDB-T")]
    IsdbT = 0x00040000,
    /// <summary>
    /// Non-standardised turbo-FEC satellite broadcast.
    /// </summary>
    [Description("Turbo FEC")]
    TurboFec = 0x00080000,
    /// <summary>
    /// Motorolla DigiCipher 2 (DC II) standard for satellite and cable broadcast.
    /// </summary>
    [Description("DigiCipher 2")]
    DigiCipher2 = 0x00100000,
    /// <summary>
    /// DirecTV standard for satellite broadcast.
    /// </summary>
    [Description("DirecTV")]
    DirecTvDss = 0x00200000,
    /// <summary>
    /// Digital Audio Broadcast standard.
    /// </summary>
    [Description("DAB")]
    Dab = 0x00400000,

    /// <summary>
    /// A mask for identifying analog broadcast standards.
    /// </summary>
    AnalogMask = 0x0000003d,
    /// <summary>
    /// A mask for identifying digital broadcast standards.
    /// </summary>
    DigitalMask = 0x007ffffc2,
    /// <summary>
    /// A mask for identifying Digital Video Broadcast standards.
    /// </summary>
    DvbMask = 0x00003fc0,
    /// <summary>
    /// A mask for identifying Integrated Services Digital Broadcasting broadcast standards.
    /// </summary>
    IsdbMask = 0x00070000,
    /// <summary>
    /// A mask for identifying broadcast standards that are applicable for cable transmission.
    /// </summary>
    CableMask = 0x000180c8,
    /// <summary>
    /// A mask for identifying broadcast standards that are applicable for satellite transmission.
    /// </summary>
    SatelliteMask = 0x003a0d00,
    /// <summary>
    /// A mask for identifying broadcast standards that are applicable for terrestrial transmission.
    /// </summary>
    TerrestrialMask = 0x0044703c
  }

  /// <summary>
  /// Capture video sources.
  /// </summary>
  public enum CaptureSourceVideo
  {
    /// <summary>
    /// No video.
    /// </summary>
    None = -1,
    /// <summary>
    /// Tuner.
    /// </summary>
    Tuner,
    /// <summary>
    /// Composite (CVBS) input #1.
    /// </summary>
    [Description("Composite #1")]
    Composite1,
    /// <summary>
    /// Composite (CVBS) input #2.
    /// </summary>
    [Description("Composite #2")]
    Composite2,
    /// <summary>
    /// Composite (CVBS) input #3.
    /// </summary>
    [Description("Composite #3")]
    Composite3,
    /// <summary>
    /// S-video input #1.
    /// </summary>
    [Description("S-video #1")]
    Svideo1,
    /// <summary>
    /// S-video input #2.
    /// </summary>
    [Description("S-video #2")]
    Svideo2,
    /// <summary>
    /// S-video input #3.
    /// </summary>
    [Description("S-video #3")]
    Svideo3,
    /// <summary>
    /// RGB input #1.
    /// </summary>
    [Description("RGB #1")]
    Rgb1,
    /// <summary>
    /// RGB input #2.
    /// </summary>
    [Description("RGB #2")]
    Rgb2,
    /// <summary>
    /// RGB input #3.
    /// </summary>
    [Description("RGB #3")]
    Rgb3,
    /// <summary>
    /// YrYbY input #1.
    /// </summary>
    [Description("YrYbY #1")]
    Yryby1,
    /// <summary>
    /// YrYbY input #2.
    /// </summary>
    [Description("YrYbY #2")]
    Yryby2,
    /// <summary>
    /// YrYbY input #3.
    /// </summary>
    [Description("YrYbY #3")]
    Yryby3,
    /// <summary>
    /// HDMI input #1.
    /// </summary>
    [Description("HDMI #1")]
    Hdmi1,
    /// <summary>
    /// HDMI input #2.
    /// </summary>
    [Description("HDMI #2")]
    Hdmi2,
    /// <summary>
    /// HDMI input #3.
    /// </summary>
    [Description("HDMI #3")]
    Hdmi3
  }

  /// <summary>
  /// Audio input type.
  /// </summary>
  public enum CaptureSourceAudio
  {
    /// <summary>
    /// No audio.
    /// </summary>
    None = -1,
    /// <summary>
    /// Automatic (detection based on related pin index reported by the driver).
    /// </summary>
    Automatic,
    /// <summary>
    /// Tuner.
    /// </summary>
    Tuner,
    /// <summary>
    /// Auxiliary input #1.
    /// </summary>
    [Description("Auxiliary #1")]
    Auxiliary1,
    /// <summary>
    /// Auxiliary input #2.
    /// </summary>
    [Description("Auxiliary #2")]
    Auxiliary2,
    /// <summary>
    /// Auxiliary input #3.
    /// </summary>
    [Description("Auxiliary #3")]
    Auxiliary3,
    /// <summary>
    /// Line input #1.
    /// </summary>
    [Description("Line #1")]
    Line1,
    /// <summary>
    /// Line input #2.
    /// </summary>
    [Description("Line #2")]
    Line2,
    /// <summary>
    /// Line input #3.
    /// </summary>
    [Description("Line #3")]
    Line3,
    /// <summary>
    /// S/PDIF input #1.
    /// </summary>
    [Description("S/PDIF #1")]
    Spdif1,
    /// <summary>
    /// S/PDIF input #2.
    /// </summary>
    [Description("S/PDIF #2")]
    Spdif2,
    /// <summary>
    /// S/PDIF input #3.
    /// </summary>
    [Description("S/PDIF #3")]
    Spdif3,
    /// <summary>
    /// AES input #1.
    /// </summary>
    [Description("AES #1")]
    Aes1,
    /// <summary>
    /// AES input #2.
    /// </summary>
    [Description("AES #2")]
    Aes2,
    /// <summary>
    /// AES input #3.
    /// </summary>
    [Description("AES #3")]
    Aes3
  }

  /// <summary>
  /// Tuner power states.
  /// </summary>
  public enum PowerState
  {
    /// <summary>
    /// Power off.
    /// </summary>
    Off,
    /// <summary>
    /// Power on.
    /// </summary>
    On
  }
}