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
  internal class DecoderSony : DecoderBase
  {
    private int _commandPeriod = 0;

    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    // http://www.mikrocontroller.net/articles/IRMP
    // http://www.sbprojects.com/knowledge/ir/sirc.php
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
            if (isPulse && IsWithinTolerance(duration, 2400, TOLERANCE))
            {
              _commandPeriod = duration;
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 600, TOLERANCE))
            {
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 1200, TOLERANCE))
              {
                _data.AddBit(true);
              }
              else if (IsWithinTolerance(duration, 600, TOLERANCE))
              {
                _data.AddBit(false);
              }
              else
              {
                this.LogError("Microsoft blaster Sony RC: unexpected data pulse, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
              }
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 600, TOLERANCE))
              {
                _data.IsHalfBit = false;
                break;
              }

              if (duration < 600 + TOLERANCE)
              {
                this.LogError("Microsoft blaster Sony RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
              }
              else
              {
                bool isRepeating = _commandPeriod <= 45000 + TOLERANCE_REPEAT;
                if (_data.BitCount == 12 || _data.BitCount == 15)
                {
                  // 12/15 bits = 7 bit command code + 5/8 bit address code
                  this.LogDebug("Microsoft blaster Sony RC: key press, bit count = {0}, command = {1}, address = {2}, is repeating = {3}", _data.BitCount, _data.GetBits((byte)(_data.BitCount - 7), 7), _data.GetBits(0, (byte)(_data.BitCount - 7)), isRepeating);
                }
                else if (_data.BitCount == 20)
                {
                  // 20 bits = 7 bit command code + 5 bit address code + 8 bit extension
                  this.LogDebug("Microsoft blaster Sony RC: key press, bit count = {0}, command = {1}, address = {2}, extension = {3}, is repeating = {4}", _data.BitCount, _data.GetBits(13, 7), _data.GetBits(8, 5), _data.GetBits(0, 8), isRepeating);
                }
                else
                {
                  this.LogError("Microsoft blaster Sony RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                }
              }
              _state = DecoderState.HeaderPulse;
            }
            else
            {
              this.LogError("Microsoft blaster Sony RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;
        }
      }
    }
  }
}