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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum
{
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
    /// Recalculate satellite positions [1.2].
    /// </summary>
    RecalculatePositions = 0x6f

    #endregion
  }
}