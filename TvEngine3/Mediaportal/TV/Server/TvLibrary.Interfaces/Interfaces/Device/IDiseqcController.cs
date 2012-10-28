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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device
{
  #region enums

  /// <summary>
  /// DiSEqC message framing part.
  /// </summary>
  public enum DiseqcFrame : byte
  {
    /// <summary>
    /// Master command: first transmission, reply not required.
    /// </summary>
    CommandFirstTransmissionNoReply = 0xe0,
    /// <summary>
    /// Master command: repeated transmission, reply not required.
    /// </summary>
    CommandRepeatTransmissionNoReply = 0xe1,
    /// <summary>
    /// Master command: first transmission, reply required.
    /// </summary>
    CommandFirstTransmissionReply = 0xe2,
    /// <summary>
    /// Master command: repeated transmission, reply required.
    /// </summary>
    CommandRepeatTransmissionReply = 0xe3,
    /// <summary>
    /// Slave reply: "OK", no errors detected.
    /// </summary>
    ReplyOk = 0xe4,
    /// <summary>
    /// Slave reply: command not supported.
    /// </summary>
    ReplyCommandNotSupported = 0xe5,
    /// <summary>
    /// Slave reply: parity error detected, repeat requested.
    /// </summary>
    ReplyParityError = 0xe6,
    /// <summary>
    /// Slave reply: unknown command, repeat requested.
    /// </summary>
    ReplyUnknownCommand = 0xe7
  }

  /// <summary>
  /// DiSEqC message device class address.
  /// </summary>
  public enum DiseqcAddress : byte
  {
    /// <summary>
    /// Any device.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Any LNB, switcher or SMATV.
    /// </summary>
    AnySwitch = 0x10,
    /// <summary>
    /// LNB.
    /// </summary>
    Lnb = 0x11,
    /// <summary>
    /// LNB with loop-through switching.
    /// </summary>
    LnbWithLoopThrough = 0x12,
    /// <summary>
    /// DC-blocking switcher.
    /// </summary>
    Switcher = 0x14,
    /// <summary>
    /// Switcher with DC loop-through.
    /// </summary>
    SwitcherWithDcLoopThrough = 0x15,
    /// <summary>
    /// SMATV.
    /// </summary>
    Smatv = 0x18,

    /// <summary>
    /// Any polariser.
    /// </summary>
    AnyPolariser = 0x20,
    /// <summary>
    /// Linear (skew) polariser.
    /// </summary>
    LinearPolariser = 0x21,

    /// <summary>
    /// Any positioner.
    /// </summary>
    AnyPositioner = 0x30,
    /// <summary>
    /// Polar/azimuth positioner.
    /// </summary>
    AzimuthPositioner = 0x31,
    /// <summary>
    /// Elevation positioner.
    /// </summary>
    ElevationPositioner = 0x32,

    /// <summary>
    /// Any installer aid.
    /// </summary>
    AnyInstallerAid = 0x40,
    /// <summary>
    /// Analog signal strength indicator.
    /// </summary>
    AnalogSignalStrengthIndicator = 0x41,

    // 0x6x = address reallocations

    /// <summary>
    /// Any intelligent slave.
    /// </summary>
    AnyIntelligentSlave = 0x70,
    /// <summary>
    /// Subscriber controlled headend.
    /// </summary>
    SubscriberControlledHeadend = 0x71

    // 0xf* = OEM extension
  }

  /// <summary>
  /// DiSEqC messages/commands.
  /// </summary>
  public enum DiseqcCommand : byte
  {
    #region power management

    /// <summary>
    /// Reset the DiSEqC microcontroller.
    /// </summary>
    Reset = 0,
    /// <summary>
    /// Clear reset [2.0].
    /// </summary>
    ClearReset = 0x01,

    /// <summary>
    /// Switch the peripheral power supply off.
    /// </summary>
    Standby = 0x2,
    /// <summary>
    /// Switch the peripheral power supply on.
    /// </summary>
    PowerOn = 0x3,

    #endregion

    #region bus contention management [DiSEqC 2.0]

    /// <summary>
    /// Set the contention flag [2.0].
    /// </summary>
    SetContend = 0x4,
    /// <summary>
    /// Read address if the contention flag is set [2.0].
    /// </summary>
    Contend = 0x5,
    /// <summary>
    /// Clear the contention flag [2.0].
    /// </summary>
    ClearContend = 0x6,
    /// <summary>
    /// Read address if the contention flag is not set [2.0].
    /// </summary>
    Address = 0x7,
    /// <summary>
    /// Change address if the contention flag is set [2.0].
    /// </summary>
    MoveC = 0x8,
    /// <summary>
    /// Change address if the contention flag is not set [2.0].
    /// </summary>
    Move = 0x9,

    #endregion

    /// <summary>
    /// Read status [2.0].
    /// </summary>
    Status = 0x10,
    /// <summary>
    /// Read configuration [2.0].
    /// </summary>
    Config = 0x11,

    #region read switch state [DiSEqC 2.0]

    /// <summary>
    /// Read committed port switch state [2.0].
    /// </summary>
    Switch0 = 0x14,
    /// <summary>
    /// Read uncommitted port switch state [2.0].
    /// </summary>
    Switch1 = 0x15,
    /// <summary>
    /// (Expansion option.)
    /// </summary>
    Switch2 = 0x16,
    /// <summary>
    /// (Expansion option.)
    /// </summary>
    Switch3 = 0x17,

    #endregion

    #region committed switch individual commands [DiSEqC 1.0]

    /// <summary>
    /// Select the low local oscillator frequency.
    /// </summary>
    SetLowLocalOscillator = 0x20,
    /// <summary>
    /// Select linear vertical/circular right polarisation.
    /// </summary>
    SetVerticalRightPolarisation = 0x21,
    /// <summary>
    ///  Select satellite position A (position A or C).
    /// </summary>
    SetSatellitePositionA = 0x22,
    /// <summary>
    ///  Select switch option A (position A or B).
    /// </summary>
    SetSwitchOptionA = 0x23,
    /// <summary>
    /// Select the high local oscillator frequency.
    /// </summary>
    SetHighLocalOscillator = 0x24,
    /// <summary>
    /// Select linear horizontal/circular left polarisation.
    /// </summary>
    SetHorizontalLeftPolarisation = 0x25,
    /// <summary>
    ///  Select satellite position B (position B or D).
    /// </summary>
    SetSatellitePositionB = 0x26,
    /// <summary>
    ///  Select switch option B (position C or D).
    /// </summary>
    SetSwitchOptionB = 0x27,

    #endregion

    #region uncommitted switch individual commands [DiSEqC 1.1]

    /// <summary>
    /// Select switch 1 input A [1.1].
    /// </summary>
    SetSwitch1InputA = 0x28,
    /// <summary>
    /// Select switch 2 input A [1.1].
    /// </summary>
    SetSwitch2InputA = 0x29,
    /// <summary>
    /// Select switch 3 input A [1.1].
    /// </summary>
    SetSwitch3InputA = 0x2a,
    /// <summary>
    /// Select switch 4 input A [1.1].
    /// </summary>
    SetSwitch4InputA = 0x2b,
    /// <summary>
    /// Select switch 1 input B [1.1].
    /// </summary>
    SetSwitch1InputB = 0x2c,
    /// <summary>
    /// Select switch 2 input B [1.1].
    /// </summary>
    SetSwitch2InputB = 0x2d,
    /// <summary>
    /// Select switch 3 input B [1.1].
    /// </summary>
    SetSwitch3InputB = 0x2e,
    /// <summary>
    /// Select switch 4 input B [1.1].
    /// </summary>
    SetSwitch4InputB = 0x2f,

    #endregion

    /// <summary>
    /// Ignore all commands except "awake".
    /// </summary>
    Sleep = 0x30,
    /// <summary>
    /// Resume normal responses (after "sleep").
    /// </summary>
    Awake = 0x31,

    #region write switch state

    /// <summary>
    /// Write to port group 0 (committed switches).
    /// </summary>
    WriteN0 = 0x38,
    /// <summary>
    /// Write to port group 1 (uncommitted switches) [DiSEqC 1.1].
    /// </summary>
    WriteN1 = 0x39,
    /// <summary>
    /// (Expansion option.)
    /// </summary>
    WriteN2 = 0x3a,
    /// <summary>
    /// (Expansion option.)
    /// </summary>
    WriteN3 = 0x3b,

    #endregion

    #region read/write analog values

    /// <summary>
    /// Read analog value A0 [2.0].
    /// </summary>
    ReadA0 = 0x40,
    /// <summary>
    /// Read analog value A1 [2.0].
    /// </summary>
    ReadA1 = 0x41,

    /// <summary>
    /// Write analog value A0 [1.1].
    /// </summary>
    WriteA0 = 0x48,
    /// <summary>
    /// Write analog value A1 [1.1].
    /// </summary>
    WriteA1 = 0x49,

    #endregion

    /// <summary>
    /// Read current frequency as a BCD string [2.0].
    /// </summary>
    LoString = 0x50,
    /// <summary>
    /// Read current frequency table entry [2.0].
    /// </summary>
    LoNow = 0x51,
    /// <summary>
    /// Read low local oscillator frequency table entry [2.0].
    /// </summary>
    LowLocalOscillator = 0x52,
    /// <summary>
    /// Read high local oscillator frequency table entry [2.0].
    /// </summary>
    HighLocalOscillator = 0x53,

    /// <summary>
    /// Write channel frequency as a BCD string [1.1].
    /// </summary>
    WriteFrequency = 0x58,
    /// <summary>
    /// Write channel number.
    /// </summary>
    ChannelNumber = 0x59,

    #region positioner commands

    /// <summary>
    /// Stop positioner movement [1.2].
    /// </summary>
    Halt = 0x60,
    /// <summary>
    /// Disable positioner movement soft-limits [1.2].
    /// </summary>
    LimitsOff = 0x63,
    /// <summary>
    /// Read positioner status [2.2].
    /// </summary>
    PositionerStatus = 0x64,
    /// <summary>
    /// Set the positioner Eastward movement soft-limit [1.2].
    /// </summary>
    LimitEast = 0x66,
    /// <summary>
    /// Set the positioner Westward movement soft-limit [1.2].
    /// </summary>
    LimitWest = 0x67,
    /// <summary>
    /// Drive the positioner East [1.2].
    /// </summary>
    DriveEast = 0x68,
    /// <summary>
    /// Drive the positioner West [1.2].
    /// </summary>
    DriveWest = 0x69,
    /// <summary>
    /// Store the current position and enable movement soft-limits [1.2].
    /// </summary>
    StorePosition = 0x6a,
    /// <summary>
    /// Drive the positioner to a given position by position number [1.2].
    /// </summary>
    GotoPosition = 0x6b,
    /// <summary>
    /// Drive the positioner to a given position by angle [1.2].
    /// </summary>
    GotoAngularPosition = 0x6e,
    /// <summary>
    /// Recalcuate satellite positions [1.2].
    /// </summary>
    RecalculatePositions = 0x6f

    #endregion
  }

  /// <summary>
  /// DiSEqC positioner status flags.
  /// </summary>
  /// <remarks>
  /// Status is retrieved using DiseqcCommand.PositionerStatus.
  /// </remarks>
  [Flags]
  public enum DiseqcPositionerStatus : byte
  {
    /// <summary>
    /// The reference position has been corrupted or lost.
    /// </summary>
    PositionReferenceLost = 0x1,
    /// <summary>
    /// A hardware switch (limit or reference) is activated.
    /// </summary>
    HardwareSwitchActivated = 0x2,
    /// <summary>
    /// Power is not available.
    /// </summary>
    PowerNotAvailable = 0x4,
    /// <summary>
    /// Movement soft-limit has been reached.
    /// </summary>
    SoftwareLimitReached = 0x8,
    /// <summary>
    /// The motor is running.
    /// </summary>
    MotorRunning = 0x10,
    /// <summary>
    /// Current or previous movement direction was West.
    /// </summary>
    DirectionWest = 0x20,
    /// <summary>
    /// Movement soft-limits are enabled.
    /// </summary>
    SoftwareLimitsEnabled = 0x40,
    /// <summary>
    /// The previous movement command has been completed.
    /// </summary>
    CommandCompleted = 0x80
  }

  /// <summary>
  /// Logical DiSEqC motor movement directions.
  /// </summary>
  public enum DiseqcDirection
  {
    /// <summary>
    /// West.
    /// </summary>
    West,
    /// <summary>
    /// East.
    /// </summary>
    East,
    /// <summary>
    /// Up.
    /// </summary>
    Up,
    /// <summary>
    /// Down.
    /// </summary>
    Down
  }

  /// <summary>
  /// Logical DiSEqC switch commands for DiSEqC 1.0 and 1.1 compatible switches.
  /// </summary>
  public enum DiseqcPort
  {
    /// <summary>
    /// DiSEqC not used.
    /// </summary>
    None = 0,
    /// <summary>
    /// Simple A (tone burst).
    /// </summary>
    SimpleA = 1,
    /// <summary>
    /// Simple B (data burst).
    /// </summary>
    SimpleB = 2,
    /// <summary>
    /// DiSEqC 1.0 port A (option A, position A)
    /// </summary>
    PortA = 3,
    /// <summary>
    /// DiSEqC 1.0 port B (option A, position B)
    /// </summary>
    PortB = 4,
    /// <summary>
    /// DiSEqC 1.0 port C (option B, position A)
    /// </summary>
    PortC = 5,
    /// <summary>
    /// DiSEqC 1.0 port D (option B, position B)
    /// </summary>
    PortD = 6,
    /// <summary>
    /// DiSEqC 1.1 port 1
    /// </summary>
    Port1 = 7,
    /// <summary>
    /// DiSEqC 1.1 port 2
    /// </summary>
    Port2 = 8,
    /// <summary>
    /// DiSEqC 1.1 port 3
    /// </summary>
    Port3 = 9,
    /// <summary>
    /// DiSEqC 1.1 port 4
    /// </summary>
    Port4 = 10,
    /// <summary>
    /// DiSEqC 1.1 port 5
    /// </summary>
    Port5 = 11,
    /// <summary>
    /// DiSEqC 1.1 port 6
    /// </summary>
    Port6 = 12,
    /// <summary>
    /// DiSEqC 1.1 port 7
    /// </summary>
    Port7 = 13,
    /// <summary>
    /// DiSEqC 1.1 port 8
    /// </summary>
    Port8 = 14,
    /// <summary>
    /// DiSEqC 1.1 port 9
    /// </summary>
    Port9 = 15,
    /// <summary>
    /// DiSEqC 1.1 port 10
    /// </summary>
    Port10 = 16,
    /// <summary>
    /// DiSEqC 1.1 port 11
    /// </summary>
    Port11 = 17,
    /// <summary>
    /// DiSEqC 1.1 port 12
    /// </summary>
    Port12 = 18,
    /// <summary>
    /// DiSEqC 1.1 port 13
    /// </summary>
    Port13 = 19,
    /// <summary>
    /// DiSEqC 1.1 port 14
    /// </summary>
    Port14 = 20,
    /// <summary>
    /// DiSEqC 1.1 port 15
    /// </summary>
    Port15 = 21,
    /// <summary>
    /// DiSEqC 1.1 port 16
    /// </summary>
    Port16 = 22
  }

  /// <summary>
  /// Logical tone burst (simple DiSEqC) states.
  /// </summary>
  public enum ToneBurst
  {
    /// <summary>
    /// Tone/data burst not used.
    /// </summary>
    None = 0,
    /// <summary>
    /// Tone burst, also known as "unmodulated" or "simple A".
    /// </summary>
    ToneBurst,
    /// <summary>
    /// Data burst, also known as "modulated" or "simple B".
    /// </summary>
    DataBurst
  }

  /// <summary>
  /// Logical 22 kHz oscillator states.
  /// </summary>
  public enum Tone22k
  {
    /// <summary>
    /// Off.
    /// </summary>
    Off = 0,
    /// <summary>
    /// On.
    /// </summary>
    On,
    /// <summary>
    /// Auto - controlled by LNB frequency parameters.
    /// </summary>
    Auto
  }

  #endregion

  /// <summary>
  /// An interface for higher level control of DiSEqC devices (<see cref="IDiseqcDevice"/>).
  /// </summary>
  public interface IDiseqcController
  {
    /// <summary>
    /// Reset a device's microcontroller.
    /// </summary>
    void Reset();

    /// <summary>
    /// Send the required switch and positioner command(s) to tune a given channel.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    void SwitchToChannel(DVBSChannel channel);

    #region positioner (motor) control

    /// <summary>
    /// Stop the movement of a positioner device.
    /// </summary>
    void Stop();

    /// <summary>
    /// Set the Eastward soft-limit of movement for a positioner device.
    /// </summary>
    void SetEastLimit();

    /// <summary>
    /// Set the Westward soft-limit of movement for a positioner device.
    /// </summary>
    void SetWestLimit();

    /// <summary>
    /// Enable/disable the movement soft-limits for a positioner device.
    /// </summary>
    bool ForceLimits { set; }

    /// <summary>
    /// Drive a positioner device in a given direction for a specified period of time.
    /// </summary>
    /// <param name="direction">The direction to move in.</param>
    /// <param name="steps">The number of position steps to move.</param>
    void DriveMotor(DiseqcDirection direction, byte steps);

    /// <summary>
    /// Store the current position of a positioner device for later use.
    /// </summary>
    /// <param name="position">The identifier to use for the position.</param>
    void StorePosition(byte position);

    /// <summary>
    /// Drive a positioner device to its reference position.
    /// </summary>
    void GotoReferencePosition();

    /// <summary>
    /// Drive a positioner device to a previously stored position.
    /// </summary>
    /// <param name="position">The position to drive to.</param>
    void GotoPosition(byte position);

    /// <summary>
    /// Get the current position of a positioner device.
    /// </summary>
    /// <param name="satellitePosition">The stored position number corresponding with the current position.</param>
    /// <param name="stepsAzimuth">The number of steps taken from the position on the azmutal axis.</param>
    /// <param name="stepsElevation">The number of steps taken from the position on the vertical (elevation) axis.</param>
    void GetPosition(out int satellitePosition, out int stepsAzimuth, out int stepsElevation);

    #endregion
  }
}