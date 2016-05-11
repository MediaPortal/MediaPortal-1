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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Decoder
{
  internal class DecoderPanasonicOld : DecoderBase
  {
    private const uint TOLERANCE_OTHER = 150;   // unit = micro-seconds
    private const uint TOLERANCE_HEADER = 250;

    // This decoder handles several variations which may in fact be distinct
    // protocols that are too similar to be reliably distinguishable.
    //
    // Panasonic [old]
    // http://www.mikrocontroller.net/articles/IRMP
    // http://users.telenet.be/davshomepage/panacode.htm
    // 22 bits = 5 bit custom code + 6 bit data code + 5 bit inverted custom code + 6 bit inverted data code
    // Apparently North America and the rest of the world have slightly
    // different timing periods:
    // - NA = 422 us
    // - RoW = 457 us
    //
    // Matsushita AKA Emerson AKA Scientific Atlanta AKA Sampo
    // http://www.celadon.com/SC-33B-programmable-remote-control/SC-33B-MATSUSHITA-IR-Protocol.pdf
    // http://educypedia.karadimov.info/library/infrared_protocols_samples.pdf
    // http://www.mikrocontroller.net/articles/IRMP
    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    // Specification information is conflicting.
    // 24 bits = 6 bit custom code + 6 bit data code + 6 bit inverted custom code [Sampo: extension/sub-device] + 6 bit inverted data code
    // Timing period is assumed to be between ~415 and ~435 us.
    //
    // Combine ==> timing period = 435 us, bit count = 22 or 24
    public override void Detect(int[] timingData)
    {
      foreach (int d in timingData)
      {
        int duration = Math.Abs(d);
        bool isPulse = d > 0;

        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && IsWithinTolerance(duration, 3480, TOLERANCE_HEADER))
            {
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 3480, TOLERANCE_HEADER))
            {
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 870, TOLERANCE_OTHER))
            {
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 870, TOLERANCE_OTHER))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 2610, TOLERANCE_OTHER))
              {
                _data.AddBit(true);
              }
              else if (duration < 2610 + TOLERANCE_OTHER)
              {
                this.LogError("Microsoft blaster Panasonic old RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
              }
              else
              {
                if (_data.BitCount == 22 || _data.BitCount == 24)
                {
                  // 22/24 bits = 5/6 bit custom code + 6 bit data code + 5/6 bit inverted custom code + 6 bit inverted data code
                  // Check parity.
                  byte customBitCount = 5;
                  ulong parityMask = 0x1f;
                  if (_data.BitCount == 24)
                  {
                    customBitCount++;
                    parityMask = 0x3f;
                  }
                  ulong custom = _data.GetBits((byte)(12 + customBitCount), customBitCount);
                  ulong data = _data.GetBits((byte)(6 + customBitCount), 6);
                  if (custom != (~_data.GetBits(6, customBitCount) & parityMask) || data != (~_data.GetBits(0, 6) & 0x3f))
                  {
                    this.LogError("Microsoft blaster Panasonic old RC: data parity error, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                  }
                  else
                  {
                    // This repeating check is uncertain.
                    bool isRepeating = duration <= 40000 + TOLERANCE_REPEAT;
                    this.LogDebug("Microsoft blaster Panasonic old RC: key press, bit count = {0}, custom = {1}, data = {2}, is repeating = {3}", _data.BitCount, custom, data, isRepeating);
                  }
                }
                else
                {
                  this.LogError("Microsoft blaster Panasonic old RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                }

                _state = DecoderState.HeaderPulse;
              }
            }
            else
            {
              this.LogError("Microsoft blaster Panasonic old RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;
        }
      }
    }
  }
}