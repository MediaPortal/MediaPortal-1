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
  // See SCTE 43 section 5.1.2 for valid combinations.
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct EncoderVideoProfile
  {
    public ushort VerticalSize;   // unit = pixels
    public ushort HorizontalSize; // unit = pixels
    public EncoderVideoAspectRatio AspectRatioInformation;
    public EncoderVideoFrameRate FrameRateCode;
    public EncoderVideoProgressiveSequence ProgressiveSequence;
  }
}