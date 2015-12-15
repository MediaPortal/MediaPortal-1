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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum
{
  // Refer to SCTE 57 table 5.23.
  internal enum AudioSelection : byte
  {
    /// <summary>
    /// Indicates the audio for the virtual channel is processed in the
    /// standard way appropriate to the waveform standard.
    /// </summary>
    DefaultAudio = 0,
    /// <summary>
    /// Indicates the audio should be taken from sub-carrier 1, the location of
    /// which is defined in the TDT for the referenced transponder.
    /// </summary>
    SubCarrier1Mono = 1,
    /// <summary>
    /// Indicates the audio should be taken from sub-carrier 2, the location of
    /// which is defined in the TDT for the referenced transponder.
    /// </summary>
    SubCarrier2Mono = 2,
    /// <summary>
    /// Indicates the stereo audio should be processed from two sub-carriers;
    /// sub-carrier locations and matrixing mode are defined in the TDT for the
    /// referenced transponder.
    /// </summary>
    SubCarrierStereo = 3
  }
}