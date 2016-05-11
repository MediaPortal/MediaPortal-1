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
  // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
  internal class DecoderAiwa : DecoderBase
  {
    // For repeat handling...
    private byte _previousDeviceCode = 0;
    private byte _previousSubDeviceCode = 0;
    private byte _previousFunctionCode = 0;

    public override void Detect(int[] timingData)
    {
      foreach (int d in timingData)
      {
        int duration = Math.Abs(d);
        bool isPulse = d > 0;

        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && IsWithinTolerance(duration, 8800, TOLERANCE))
            {
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 4400, TOLERANCE))
            {
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 550, TOLERANCE))
            {
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 550, TOLERANCE))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 1650, TOLERANCE))
              {
                _data.AddBit(true);
              }
              else if (duration < 90750 + TOLERANCE_REPEAT && _data.BitCount == 0)
              {
                this.LogDebug("Microsoft blaster Aiwa RC: key press, device = {0}, sub-device = {1}, function = {2}, is repeating = true", _previousDeviceCode, _previousSubDeviceCode, _previousFunctionCode);
                _state = DecoderState.HeaderPulse;
                break;
              }
              else
              {
                this.LogError("Microsoft blaster Aiwa RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
                break;
              }

              if (_data.BitCount == 42)
              {
                // 42 bits = 8 bit device code + 5 bit sub-device code + 8 bit inverted device code + 5 bit inverted sub-device code + 8 bit function code + 8 bit inverted function code
                // Check parity.
                if (
                  _data.GetBits(34, 8) != (~_data.GetBits(21, 8) & 0xff) ||
                  _data.GetBits(29, 5) != (~_data.GetBits(16, 5) & 0x1f) ||
                  _data.GetBits(8, 8) != (_data.GetBits(0, 8) & 0xff)
                )
                {
                  this.LogError("Microsoft blaster Aiwa RC: data parity error, full command = {0}", _data.GetBits(0, 42));
                  _previousDeviceCode = 0;
                  _previousSubDeviceCode = 0;
                  _previousFunctionCode = 0;
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
              this.LogError("Microsoft blaster Aiwa RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;

          case DecoderState.Leading:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 550, TOLERANCE))
            {
              _data.IsHalfBit = true;
              break;
            }

            if (!isPulse && _data.IsHalfBit && duration > 1650 + TOLERANCE)
            {
              _previousDeviceCode = (byte)_data.GetBits(34, 8);
              _previousSubDeviceCode = (byte)_data.GetBits(29, 5);
              _previousFunctionCode = (byte)_data.GetBits(8, 8);
              this.LogDebug("Microsoft blaster Aiwa RC: key press, device = {0}, sub-device = {1}, function = {2}, is repeating = false", _previousDeviceCode, _previousSubDeviceCode, _previousFunctionCode);
            }
            else
            {
              this.LogError("Microsoft blaster Aiwa RC: unexpected leading, is pulse = {0}, is half bit = {1}, duration = {2} us", isPulse, _data.IsHalfBit, duration);
            }
            _state = DecoderState.HeaderPulse;
            break;
        }
      }
    }
  }
}