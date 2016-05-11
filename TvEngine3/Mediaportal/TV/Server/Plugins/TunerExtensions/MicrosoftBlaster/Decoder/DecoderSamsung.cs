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
  internal class DecoderSamsung : DecoderBase
  {
    private bool _is36BitCommand = false;
    private int _commandPeriod = 0;

    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    // http://www.mikrocontroller.net/articles/IRMP
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
            if (isPulse && IsWithinTolerance(duration, 4500, TOLERANCE))
            {
              _commandPeriod = duration;
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 4500, TOLERANCE))
            {
              _state = DecoderState.Data;
              _data.Reset();
              _is36BitCommand = false;
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 562, TOLERANCE))
            {
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 562, TOLERANCE))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 1688, TOLERANCE))
              {
                _data.AddBit(true);
              }
              else if (IsWithinTolerance(duration, 4500, TOLERANCE) && _data.BitCount == 16)
              {
                // sync
                _is36BitCommand = true;
              }
              else if (_is36BitCommand && _data.BitCount == 28)
              {
                // DecodeIR: gap before inverted command (existence uncertain, length unknown)
              }
              else if (duration < 1688 + TOLERANCE)
              {
                this.LogError("Microsoft blaster Samsung RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
              }
              else
              {
                if (!_is36BitCommand && _data.BitCount == 20)
                {
                  // 20 bits = 6 bit device code + 6 bit sub-device code + 8 bit command code
                  bool isRepeating = _commandPeriod <= 60000 + TOLERANCE_REPEAT;
                  this.LogDebug("Microsoft blaster Samsung RC: key press, device = {0}, sub-device = {1}, command = {2}, is repeating = {3}", _data.GetBits(14, 6), _data.GetBits(8, 6), _data.GetBits(0, 8), isRepeating);
                }
                else if (!_is36BitCommand && _data.BitCount == 32)
                {
                  // IRMP-only
                  // 32 bits = 8 bit device code + 8 bit sub-device code + 16 bit command code
                  // Don't know how repeats are handled (32 bits won't always
                  // fit in 60 ms, and 118 ms seems too generous).
                  this.LogDebug("Microsoft blaster Samsung RC: key press, device = {0}, sub-device = {1}, command = {2}", _data.GetBits(24, 8), _data.GetBits(16, 8), _data.GetBits(0, 16));
                }
                else if (_is36BitCommand && _data.BitCount == 36)
                {
                  // 36 bits = 8 bit device code + 8 bit sub-device code + 4 bit extension + 8 bit command code + 8 bit inverted command code
                  // Check parity.
                  ulong command = _data.GetBits(8, 8);
                  if (command != (~_data.GetBits(0, 8) & 0xff))
                  {
                    this.LogError("Microsoft blaster Samsung RC: data parity error, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                  }
                  else
                  {
                    bool isRepeating = _commandPeriod <= 118000 + TOLERANCE_REPEAT;
                    this.LogDebug("Microsoft blaster Samsung RC: key press, device = {0}, sub-device = {1}, extension = {2}, command = {3}, is repeating = {4}", _data.GetBits(28, 8), _data.GetBits(20, 8), _data.GetBits(16, 4), _data.GetBits(8, 8), isRepeating);
                  }
                }
                else
                {
                  this.LogError("Microsoft blaster Samsung RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                }

                _state = DecoderState.HeaderPulse;
              }
            }
            else
            {
              this.LogError("Microsoft blaster Samsung RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;
        }
      }
    }
  }
}