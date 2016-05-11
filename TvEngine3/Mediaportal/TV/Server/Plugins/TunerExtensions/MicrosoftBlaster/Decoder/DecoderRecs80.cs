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
  internal class DecoderRecs80 : DecoderBase
  {
    private const uint TOLERANCE_SHORT = 35;      // unit = micro-seconds
    private const uint TOLERANCE_LONG = 200;

    private static ulong[] SUBSYSTEM_ADDRESS_LOOKUP = new ulong[]
    {
      8,
      12,
      255,
      255,
      10,
      14,
      255,
      19,
      9,
      13,
      16,
      255,
      11,
      15,
      18,
      20
    };

    private bool _isExtendedAddress = false;

    // For repeat handling...
    private DateTime _commandStartTime = DateTime.MinValue;
    private ulong _previousCommand = 0;
    private DateTime _previousCommandStartTime = DateTime.MinValue;

    public DecoderRecs80()
    {
      _data.IsLittleEndian = false;
    }

    // http://www.sbprojects.com/knowledge/ir/recs80.php
    public override void Detect(int[] timingData)
    {
      DateTime dataTransmitTime = DateTime.Now;
      int totalDuration = 0;
      foreach (int d in timingData)
      {
        totalDuration += Math.Abs(d);
      }
      dataTransmitTime = dataTransmitTime.AddMilliseconds(-totalDuration / 1000);
      totalDuration = 0;

      foreach (int d in timingData)
      {
        int duration = Math.Abs(d);
        bool isPulse = d > 0;

        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && IsWithinTolerance(duration, 158, TOLERANCE_SHORT))
            {
              _commandStartTime = dataTransmitTime.AddMilliseconds(totalDuration / 1000);
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && (IsWithinTolerance(duration, 7432, TOLERANCE_LONG) || IsWithinTolerance(duration, 3637, TOLERANCE_LONG)))
            {
              if (!_isExtendedAddress && IsWithinTolerance(duration, 3637, TOLERANCE_LONG))
              {
                // When the sub-system address has an extra bit, two start bits
                // ('11') are squashed into the space of the normal start bit.
                // => we have to handle the header (start bit) twice.
                _isExtendedAddress = true;
                _state = DecoderState.HeaderPulse;
                break;
              }
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            _isExtendedAddress = false;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 158, TOLERANCE_SHORT))
            {
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 4902, TOLERANCE_LONG))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 7432, TOLERANCE_LONG))
              {
                _data.AddBit(true);
              }
              else
              {
                this.LogError("Microsoft blaster RECS-80 RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
                _isExtendedAddress = false;
                break;
              }

              if ((!_isExtendedAddress && _data.BitCount == 10) || (_isExtendedAddress && _data.BitCount == 11))
              {
                // 10/11 bits = 1 bit toggle + 2/3 bit sub-system address + 6 bit command code
                _state = DecoderState.Leading;
                _isExtendedAddress = false;
              }
            }
            else
            {
              this.LogError("Microsoft blaster RECS-80 RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
              _isExtendedAddress = false;
            }
            break;

          case DecoderState.Leading:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 158, TOLERANCE_SHORT))
            {
              _data.IsHalfBit = true;
              break;
            }

            _state = DecoderState.HeaderPulse;

            // Commands (including header) will start every 121.5 ms if a
            // button is being held down. Command period is variable => repeat
            // space period is also variable. Maximum repeat space can be
            // calculated as 63.152 ms (all-zero command). Receive
            // configuration may not enable such a long space to be received in
            // full, so we detect repeats using slightly more convoluted
            // conditions.
            if (!isPulse && _data.IsHalfBit && duration > 7432 + TOLERANCE_LONG) //&& _commandPeriod >= 121500 - TOLERANCE_REPEAT)
            {
              byte ssaBitCount = (byte)(_data.BitCount - 7);
              ulong subSystemAddress = _data.GetBits(6, ssaBitCount);
              if (ssaBitCount == 3)
              {
                subSystemAddress = (subSystemAddress + 2) % 8;
              }
              else
              {
                subSystemAddress = SUBSYSTEM_ADDRESS_LOOKUP[subSystemAddress];
              }

              ulong fullCommand = _data.GetBits(0, _data.BitCount);
              bool isRepeating = fullCommand == _previousCommand && (_commandStartTime - _previousCommandStartTime).TotalMilliseconds < 250;
              //bool isRepeating = _commandPeriod <= 121500 + TOLERANCE_REPEAT;
              this.LogDebug("Microsoft blaster RECS-80 RC: key press, toggle bit = {0}, sub-system address = {1}, command = {2}, is repeating = {3}", _data.GetBits((byte)(_data.BitCount - 1), 1), subSystemAddress, _data.GetBits(0, 6), isRepeating);
              _previousCommand = _data.GetBits(0, _data.BitCount);
              _previousCommandStartTime = _commandStartTime;
              break;
            }

            this.LogError("Microsoft blaster RECS-80 RC: unexpected leading, is pulse = {0}, is half bit = {1}, duration = {2} us", isPulse, _data.IsHalfBit, duration);
            break;
        }

        totalDuration += duration;
      }
    }
  }
}