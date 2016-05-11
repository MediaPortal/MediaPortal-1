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
  internal class DecoderJvc : DecoderBase
  {
    private const uint TOLERANCE_REPEAT_JVC = 2000;   // unit = micro-seconds

    private int _commandPeriod = 0;

    // http://support.jvc.com/consumer/support/documents/RemoteCodes.pdf
    // http://www.sbprojects.com/knowledge/ir/jvc.php
    // Note tolerances in this code are as per JVC specifications.
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
            if (isPulse && IsWithinTolerance(duration, 8440, 100))
            {
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            // The standard says this is optional, but we require it.
            if (!isPulse && IsWithinTolerance(duration, 4220, 100))
            {
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 527, 60))
            {
              _data.IsHalfBit = true;
              if (_data.BitCount == 0)
              {
                _commandPeriod = duration;
              }
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 528, 120))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 1583, 160))
              {
                _data.AddBit(true);
              }
              else
              {
                this.LogError("Microsoft blaster JVC RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                _state = DecoderState.HeaderPulse;
                break;
              }

              if (_data.BitCount == 16)
              {
                // 16 bits = 8 bit custom code + 8 bit data code
                _state = DecoderState.Leading;
              }
            }
            else
            {
              this.LogError("Microsoft blaster JVC RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;

          case DecoderState.Leading:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 527, 60))
            {
              _data.IsHalfBit = true;
              break;
            }

            _state = DecoderState.HeaderPulse;
            if (!isPulse && _data.IsHalfBit && duration > 1583 + 160)   //&& _commandPeriod >= 46420 - TOLERANCE_REPEAT_JVC)
            {
              // Commands (excluding header) will start every 46.42 ms if a
              // button is being held down. Command period is variable =>
              // repeat space period is also variable.
              bool isRepeating = _commandPeriod <= 46420 + TOLERANCE_REPEAT_JVC;
              this.LogDebug("Microsoft blaster JVC RC: key press, custom = {0}, data = {1}, is repeating = {2}", _data.GetBits(8, 8), _data.GetBits(0, 8), isRepeating);
              if (isRepeating)
              {
                _state = DecoderState.Data;
                _data.Reset();
              }
              break;
            }

            this.LogError("Microsoft blaster JVC RC: unexpected leading, is pulse = {0}, is half bit = {1}, duration = {2} us", isPulse, _data.IsHalfBit, duration);
            break;
        }
      }
    }
  }
}