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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Decoder
{
  internal abstract class DecoderBase
  {
    protected enum DecoderState
    {
      HeaderPulse,
      HeaderSpace,
      Data,
      Leading
    }

    protected const int TOLERANCE = 100;            // unit = micro-seconds
    protected const int TOLERANCE_REPEAT = 2500;    // unit = micro-seconds

    protected DecoderState _state = DecoderState.HeaderPulse;
    protected DetectionData _data = new DetectionData();

    public abstract void Detect(int[] timingData);

    protected static bool IsWithinTolerance(int valueTest, int target, uint tolerance)
    {
      valueTest -= target;
      return Math.Abs(valueTest) <= tolerance;
    }
  }
}