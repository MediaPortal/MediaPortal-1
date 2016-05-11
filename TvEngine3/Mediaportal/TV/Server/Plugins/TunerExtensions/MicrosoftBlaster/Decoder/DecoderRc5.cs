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
  internal class DecoderRc5 : DecoderBase
  {
    private bool _isExtendedCommand = false;
    private bool _isPulsePrevious = false;

    // For repeat handling...
    private DateTime _commandStartTime = DateTime.MinValue;
    private ulong _previousCommand = 0;
    private DateTime _previousCommandStartTime = DateTime.MinValue;

    public DecoderRc5()
    {
      _data.IsLittleEndian = false;
    }

    // https://en.wikipedia.org/wiki/RC-5
    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    // http://www.sbprojects.com/knowledge/ir/rc5.php
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
        if (_data.BitCount == 0 && (!isPulse || !IsWithinTolerance(duration, 889, TOLERANCE)))
        {
          continue;
        }

        bool isValid = true;
        try
        {
          if (_isPulsePrevious == isPulse)
          {
            this.LogError("Microsoft blaster RC-5 RC: unexpected data, is pulse = {0}, duration = {1} us, bit count = {2}", isPulse, duration, _data.BitCount);
            isValid = false;
            continue;
          }
          _isPulsePrevious = isPulse;

          if (IsWithinTolerance(duration, 889, TOLERANCE))
          {
            _data.AddBit(isPulse);
          }
          else if (IsWithinTolerance(duration, 1778, TOLERANCE))
          {
            _data.AddBit(isPulse);
            _data.AddBit(isPulse);
          }
          else if (IsWithinTolerance(duration, 3556, TOLERANCE) && !isPulse && _data.BitCount == 15)
          {
            // RC-5x gap (4 x 889 us)
            _isExtendedCommand = true;
          }
          else if (
            IsWithinTolerance(duration, 4445, TOLERANCE) &&
            !isPulse &&
            (_data.BitCount == 14 || _data.BitCount == 15)
          )
          {
            // Either:
            // 1. Last half of zero-value bit 8 + RC-5x gap.
            // 2. RC-5x gap + first half of one-value bit 9.
            _isExtendedCommand = true;
            _data.AddBit(isPulse);
          }
          else if (IsWithinTolerance(duration, 5334, TOLERANCE) && !isPulse && _data.BitCount == 14)
          {
            // Last half of zero-value bit 8 + RC-5x gap + first half of one-value bit 9.
            _isExtendedCommand = true;
            _data.AddBit(isPulse);
            _data.AddBit(isPulse);
          }
          else if (!isPulse && duration > 5334 + TOLERANCE)
          {
            if (_data.BitCount % 2 == 0)
            {
              _data.AddBit(false);
            }
            _data.ProcessBiPhase(true);

            // Repeat period is expected to be 113.77* ms. Maximum repeat space
            // (any RC-5 10 bit command) can be calculated as ~89 ms. Receive
            // configuration may not enable such a long space to be received in
            // full, so we detect repeats using slightly more convoluted
            // conditions.
            //bool isRepeating = _commandPeriod <= 113778 + TOLERANCE_REPEAT;
            ulong fullCommand = _data.GetBits(0, _data.BitCount);
            bool isRepeating = fullCommand == _previousCommand && (_commandStartTime - _previousCommandStartTime).TotalMilliseconds < 250;
            if (_isExtendedCommand && _data.BitCount == 20)
            {
              this.LogDebug("Microsoft blaster RC-5 RC: RC-5x key press, field bit = {0}, toggle bit = {1}, system address = {2}, command = {3}, data = {4}, is repeating = {5}", _data.GetBits(18, 1), _data.GetBits(17, 1), _data.GetBits(12, 5), _data.GetBits(6, 6), _data.GetBits(0, 6), isRepeating);
            }
            else if (!_isExtendedCommand && (_data.BitCount == 14 || _data.BitCount == 15))
            {
              this.LogDebug("Microsoft blaster RC-5 RC: RC-5 key press, field bit = {0}, toggle bit = {1}, system address = {2}, command = {3}, is repeating = {4}", _data.GetBits((byte)(_data.BitCount - 2), 1), _data.GetBits((byte)(_data.BitCount - 3), 1), _data.GetBits((byte)(_data.BitCount - 8), 5), _data.GetBits(0, (byte)(_data.BitCount - 8)), isRepeating);
            }
            else
            {
              this.LogError("Microsoft blaster RC-5 RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
              isValid = false;
            }

            if (isValid)
            {
              _previousCommand = _data.GetBits(0, _data.BitCount);
              _previousCommandStartTime = _commandStartTime;
              isValid = false;
            }
          }
          else
          {
            this.LogError("Microsoft blaster RC-5 RC: unexpected data, is pulse = {0}, duration = {1} us, bit count = {2}", isPulse, duration, _data.BitCount);
            isValid = false;
          }
        }
        finally
        {
          totalDuration += duration;

          if (!isValid)
          {
            _data.Reset();
            _isExtendedCommand = false;
            _isPulsePrevious = false;
            _commandStartTime = dataTransmitTime.AddMilliseconds(totalDuration / 1000);
          }
        }
      }
    }
  }
}