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
  internal class DecoderRc6 : DecoderBase
  {
    private bool _isMode6aCommand = false;
    private ulong _header = 0;
    private bool _isPulsePrevious = false;

    // For repeat handling...
    private DateTime _commandStartTime = DateTime.MinValue;
    private ulong _previousCommand = 0;
    private DateTime _previousCommandStartTime = DateTime.MinValue;

    public DecoderRc6()
    {
      _data.IsLittleEndian = false;
    }

    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    // http://www.sbprojects.com/knowledge/ir/rc6.php
    // http://www.pcbheaven.com/userpages/The_Philips_RC6_Protocol/
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
        bool isPulse = d > 0;
        int duration = Math.Abs(d);

        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && (IsWithinTolerance(duration, 2666, TOLERANCE) || IsWithinTolerance(duration, 3111, TOLERANCE)))
            {
              _commandStartTime = dataTransmitTime.AddMilliseconds(totalDuration / 1000);
              _isMode6aCommand = IsWithinTolerance(duration, 3111, TOLERANCE);
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 889, TOLERANCE))
            {
              _state = DecoderState.Data;
              _data.Reset();
              _header = 0;
              _isPulsePrevious = false;
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (_isPulsePrevious == isPulse)
            {
              this.LogError("Microsoft blaster RC-6 RC: unexpected data, is pulse = {0}, duration = {1} us, bit count = {2}", isPulse, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
              continue;
            }
            _isPulsePrevious = isPulse;

            // Process the header when it's ready. If we don't "pre-process"
            // the header we can run out of space for the data.
            if (_header == 0 && _data.BitCount > 10)
            {
              byte offset = 0;
              bool firstDataBit = false;
              if (_data.BitCount % 2 != 0)
              {
                offset = 1;
                firstDataBit = _data.GetBits(0, 1) != 0;
                _data.AddBit(false);
              }
              _data.ProcessBiPhase(false);
              _header = _data.GetBits(offset, 5);   // 1 x start bit, 3 x mode bits, 1 x toggle bit

              if ((!_isMode6aCommand && (_header & 0x1e) != 0x10) || (_isMode6aCommand && (_header & 0x1e) != 0x16))
              {
                this.LogError("Microsoft blaster RC-6 RC: unsupported mode, start bit = {0}, mode = {1}", _header >> 4, (_header >> 1) & 0x7);
                _state = DecoderState.HeaderPulse;
                continue;
              }

              _data.Reset();
              if (offset > 0)
              {
                _data.AddBit(firstDataBit);
              }
            }

            if (IsWithinTolerance(duration, 444, TOLERANCE) && (_header != 0 || _data.BitCount <= 8))
            {
              _data.AddBit(isPulse);
            }
            else if (IsWithinTolerance(duration, 889, TOLERANCE))
            {
              _data.AddBit(isPulse);
              if (_header != 0 || (_data.BitCount != 9 && _data.BitCount != 10))  // Not double-period toggle bit.
              {
                _data.AddBit(isPulse);
              }
            }
            else if (IsWithinTolerance(duration, 1333, TOLERANCE) && _header == 0 && (_data.BitCount == 7 || _data.BitCount == 9))
            {
              // Either:
              // 1. Last half of last mode bit + first half of toggle bit.
              // 2. Last half of toggle bit + first half of first control bit.
              _data.AddBit(isPulse);
              _data.AddBit(isPulse);
            }
            else
            {
              if (duration < 2666 - TOLERANCE)
              {
                this.LogError("Microsoft blaster RC-6 RC: unexpected data, is pulse = {0}, duration = {1} us, bit count = {2}", isPulse, duration, _data.BitCount);
              }
              else
              {
                if (_data.BitCount % 2 != 0)
                {
                  _data.AddBit(false);
                }
                _data.ProcessBiPhase(false);

                //byte startBit = (byte)(_header >> 4);
                byte mode = (byte)((_header >> 1) & 0x7);
                byte toggleBit = (byte)(_header & 1);
                if (_data.BitCount < 16)
                {
                  this.LogError("Microsoft blaster RC-6 RC: unsupported format, mode = {0}, toggle bit = {1}, data bit count = {2}, data = {3}", mode, toggleBit, _data.BitCount, _data.GetBits(0, _data.BitCount));
                }
                else
                {
                  // Repeat period is expected to be ~107 ms. Maximum repeat
                  // space (any RC-6 16 bit command) can be calculated as ~84
                  // ms. Receive configuration may not enable such a long space
                  // to be received in full, so we detect repeats using
                  // slightly more convoluted conditions.
                  //bool isRepeating = _commandPeriod <= 107000 + TOLERANCE_REPEAT;
                  ulong fullCommand = (_header << _data.BitCount) | _data.GetBits(0, _data.BitCount);
                  bool isRepeating = fullCommand == _previousCommand && (_commandStartTime - _previousCommandStartTime).TotalMilliseconds < 250;

                  ulong address = _data.GetBits(8, 8);
                  ulong command = _data.GetBits(0, 8);
                  if (!_isMode6aCommand && _data.BitCount == 16)
                  {
                    this.LogDebug("Microsoft blaster RC-6 RC: key press, mode = {0}, toggle bit = {1}, address = {2}, command = {3}, is repeating = {4}", mode, toggleBit, address, command, isRepeating);
                  }
                  else if (_isMode6aCommand && (_data.BitCount == 20 || _data.BitCount == 24 || _data.BitCount == 32))
                  {
                    ulong customer = _data.GetBits(16, (byte)(_data.BitCount - 16));
                    if (customer == 0x800f)
                    {
                      // Microsoft MCE - non-standard toggle bit
                      toggleBit |= (byte)((address >> 6) & 2);
                      address = address & 0x7f;
                    }
                    this.LogDebug("Microsoft blaster RC-6 RC: key press, mode = {0}, toggle bit = {1}, customer = {2}, address = {3}, command = {4}, is repeating = {5}", mode, toggleBit, customer, address, command, isRepeating);
                  }
                  else
                  {
                    this.LogError("Microsoft blaster RC-6 RC: unsupported format, mode = {0}, toggle bit = {1}, address = {2}, command = {3}, extension bit count = {4}, extension = {5}", mode, toggleBit, address, command, _data.BitCount - 16, _data.GetBits(16, (byte)(_data.BitCount - 16)));
                  }

                  _previousCommand = _data.GetBits(0, _data.BitCount);
                  _previousCommandStartTime = _commandStartTime;
                }
              }

              _state = DecoderState.HeaderPulse;
            }
            break;
        }

        totalDuration += duration;
      }
    }
  }
}