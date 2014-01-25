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

using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(false)]
  public struct EncoderAudioProfile
  {
    public EncoderAudioAlgorithm AudioAlgorithmCode;
    public uint SamplingRate;     // unit = Hz, 48 kHz guaranteed supported
    public byte BitDepth;         // 16 bit per sample guaranteed supported
    public byte NumberChannel;    // stereo (2 channel) guaranteed supported
  }
}