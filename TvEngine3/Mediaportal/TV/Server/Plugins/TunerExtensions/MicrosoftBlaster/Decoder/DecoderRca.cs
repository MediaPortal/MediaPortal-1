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
  internal class DecoderRca : DecoderBase
  {
    private int _commandPeriod = 0;

    // http://www.sbprojects.com/knowledge/ir/rca.php
    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    // There's some disagreement about exact timings. We've gone with original
    // IRSS timings (which agree with sbprojects).
    public override void Detect(int[] timingData)
    {
      foreach (int d in timingData)
      {
        int duration = Math.Abs(d);
        bool isPulse = d > 0;

        _commandPeriod += duration;
        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && IsWithinTolerance(duration, 4000, TOLERANCE))
            {
              _commandPeriod = duration;
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 4000, TOLERANCE))
            {
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 500, TOLERANCE))
            {
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 1000, TOLERANCE))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 2000, TOLERANCE))
              {
                _data.AddBit(true);
              }
              else
              {
                this.LogError("Microsoft blaster RCA RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
                break;
              }

              if (_data.BitCount == 24)
              {
                // 24 bits = 4 bit address code + 8 bit command code + 4 bit inverted address code + 8 bit inverted command code
                // Check parity.
                if (_data.GetBits(20, 4) != (~_data.GetBits(8, 4) & 0xf) || _data.GetBits(12, 8) != (~_data.GetBits(0, 8) & 0xff))
                {
                  this.LogError("Microsoft blaster RCA RC: data parity error, full command = {0}", _data.GetBits(0, 24));
                  _state = DecoderState.HeaderPulse;
                }
                else
                {
                  _state = DecoderState.Leading;
                }
              }
            }
            else
            {
              this.LogError("Microsoft blaster RCA RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;

          case DecoderState.Leading:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 500, TOLERANCE))
            {
              _data.IsHalfBit = true;
              break;
            }

            _state = DecoderState.HeaderPulse;
            if (!isPulse && _data.IsHalfBit && duration > 2000 + TOLERANCE)   //&& _commandPeriod >= 64000 - TOLERANCE_REPEAT)
            {
              // Commands (including header) will start every 64 ms if a button
              // is being held down. Command period is variable => repeat space
              // period is also variable.
              bool isRepeating = _commandPeriod <= 64000 + TOLERANCE_REPEAT;
              this.LogDebug("Microsoft blaster RCA RC: key press, address = {0}, command = {1}, is repeating = {2}", _data.GetBits(20, 4), _data.GetBits(12, 8), isRepeating);
              break;
            }

            this.LogError("Microsoft blaster RCA RC: unexpected leading, is pulse = {0}, is half bit = {1}, duration = {2} us", isPulse, _data.IsHalfBit, duration);
            break;
        }
      }
    }
  }
}