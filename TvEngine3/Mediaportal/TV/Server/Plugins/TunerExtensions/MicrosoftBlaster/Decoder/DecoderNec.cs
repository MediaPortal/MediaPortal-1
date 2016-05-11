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
  internal class DecoderNec : DecoderBase
  {
    // For repeat handling...
    private bool _isRepeating = false;
    private int _commandPeriod = 0;
    private byte _previousAddressCode = 0;
    private byte _previousAddressCodeInverse = 0;
    private byte _previousCommandCode = 0;
    private byte _previousCommandCodeInverse = 0;
    private byte _previousExtensionCode = 0;
    private byte _previousExtensionCodeInverse = 0;

    // http://techdocs.altium.com/display/FPGA/NEC+Infrared+Transmission+Protocol
    // http://www.circuitvalley.com/2013/09/nec-protocol-ir-infrared-remote-control.html
    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
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
            // DecodeIR suggests that some variants may have header pulse time
            // of 4500 us for both commands and repeats/dittos. We don't
            // support that format.
            if (isPulse && IsWithinTolerance(duration, 9000, TOLERANCE))
            {
              _commandPeriod = duration;
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse)
            {
              if (IsWithinTolerance(duration, 2250, TOLERANCE))
              {
                _isRepeating = true;
              }
              else if (IsWithinTolerance(duration, 4500, TOLERANCE))
              {
                _isRepeating = false;
              }
              else
              {
                _state = DecoderState.HeaderPulse;
                break;
              }
              _state = DecoderState.Data;
              _data.Reset();
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
              else if (IsWithinTolerance(duration, 1687, TOLERANCE))
              {
                _data.AddBit(true);
              }
              else
              {
                if (duration <= 1687 + TOLERANCE)
                {
                  this.LogError("Microsoft blaster NEC RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                }
                else
                {
                  // According to my sources:
                  // - command bit count is expected to be 32, 40 or 48
                  // - repeat period is expected to be 108 ms
                  // - repeats may be full repeats or dittos
                  // - some vendors actually use some or all of the
                  //    parity/inverse bits for data
                  if ((_data.BitCount == 0 && _isRepeating) || _data.BitCount == 32 || _data.BitCount == 40 || _data.BitCount == 48)
                  {
                    if (_data.BitCount != 0)
                    {
                      _isRepeating = _commandPeriod <= 108000 + TOLERANCE_REPEAT;
                      _previousAddressCode = (byte)_data.GetBits((byte)(_data.BitCount - 8), 8);
                      _previousAddressCodeInverse = (byte)_data.GetBits((byte)(_data.BitCount - 16), 8);
                      _previousCommandCode = (byte)_data.GetBits((byte)(_data.BitCount - 24), 8);
                      _previousCommandCodeInverse = (byte)_data.GetBits((byte)(_data.BitCount - 32), 8);
                      if (_data.BitCount == 32)
                      {
                        _previousExtensionCode = 0;
                        _previousExtensionCodeInverse = 0;
                      }
                      else if (_data.BitCount == 40)
                      {
                        _previousExtensionCode = (byte)_data.GetBits(0, 8);
                        _previousExtensionCodeInverse = 0;
                      }
                      else if (_data.BitCount == 48)
                      {
                        _previousExtensionCode = (byte)_data.GetBits(8, 8);
                        _previousExtensionCodeInverse = (byte)_data.GetBits(0, 8);
                      }
                    }

                    this.LogDebug("Microsoft blaster NEC RC: key press, bit count = {0}, address = {1}, inverse address = {2}, command = {3}, inverse command = {4}, extension = {5}, inverse extension = {6}, is repeating = {7}", _data.BitCount, _previousAddressCode, _previousAddressCodeInverse, _previousCommandCode, _previousCommandCodeInverse, _previousExtensionCode, _previousExtensionCodeInverse, _isRepeating);
                  }
                  else
                  {
                    this.LogError("Microsoft blaster NEC RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                  }
                }

                _state = DecoderState.HeaderPulse;
              }
            }
            else
            {
              this.LogError("Microsoft blaster NEC RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;
        }
      }
    }
  }
}