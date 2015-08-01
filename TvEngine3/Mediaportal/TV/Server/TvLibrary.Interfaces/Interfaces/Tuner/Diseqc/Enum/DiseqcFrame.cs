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
}