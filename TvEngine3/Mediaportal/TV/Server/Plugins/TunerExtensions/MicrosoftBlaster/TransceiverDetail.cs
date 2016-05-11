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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  /// <summary>
  /// Detail for a transceiver.
  /// </summary>
  [DataContract]
  internal class TransceiverDetail
  {
    /// <summary>
    /// The transceiver's device path.
    /// </summary>
    [DataMember]
    public string DevicePath = string.Empty;

    /// <summary>
    /// <c>True</c> if the transceiver supports receiving.
    /// </summary>
    [DataMember]
    public bool IsReceiveSupported = false;

    /// <summary>
    /// <c>True</c> if the transceiver supports learning.
    /// </summary>
    [DataMember]
    public bool IsLearningSupported = false;

    /// <summary>
    /// The transmit ports available from the transceiver.
    /// </summary>
    [DataMember]
    public TransmitPort AllTransmitPorts = TransmitPort.None;

    /// <summary>
    /// A bit-mask specifying which of the transceiver's transmit ports
    /// currently have an emitter connected.
    /// </summary>
    /// <remarks>
    /// The least significant bit represents the first transmit port; the next
    /// most significant bit represents the second transmit port; etc.
    /// </remarks>
    [DataMember]
    public TransmitPort ConnectedTransmitPorts = TransmitPort.None;

    public override string ToString()
    {
      return DevicePath;
    }
  }
}