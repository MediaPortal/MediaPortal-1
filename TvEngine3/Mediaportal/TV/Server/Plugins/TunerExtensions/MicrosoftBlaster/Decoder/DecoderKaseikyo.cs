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
  internal class DecoderKaseikyo : DecoderBase
  {
    // http://www.hifi-remote.com/wiki/index.php?title=DecodeIR
    public override void Detect(int[] timingData)
    {
      foreach (int d in timingData)
      {
        int duration = Math.Abs(d);
        bool isPulse = d > 0;

        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && IsWithinTolerance(duration, 3456, TOLERANCE))
            {
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && IsWithinTolerance(duration, 1728, TOLERANCE))
            {
              _state = DecoderState.Data;
              _data.Reset();
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (isPulse && !_data.IsHalfBit && IsWithinTolerance(duration, 432, TOLERANCE))
            {
              _data.IsHalfBit = true;
            }
            else if (!isPulse && _data.IsHalfBit)
            {
              if (IsWithinTolerance(duration, 432, TOLERANCE))
              {
                _data.AddBit(false);
              }
              else if (IsWithinTolerance(duration, 1296, TOLERANCE))
              {
                _data.AddBit(true);
              }
              else
              {
                if (duration <= 1296 + TOLERANCE)
                {
                  this.LogError("Microsoft blaster Kaseikyo RC: unexpected data space, duration = {0} us, bit count = {1}", duration, _data.BitCount);
                }
                else
                {
                  // According to IRMP:
                  // - command bit count is expected to be 48, but some or all
                  //    implementations may have 56 bit variations
                  // - repeat space period (or is it repeat period???) varies:
                  //    - ~75 ms for Denon, JVC, Panasonic and other implementations
                  //    - ~45 ms for Fujitsu, Mitsubishi and Teac
                  //    - ~20 ms for Sharp
                  // - Teac repeats using dittos
                  //
                  // Implementing full support for this stuff would be complex.
                  // I also have doubts about the information's accuracy and
                  // precision. As a result I've decided to:
                  // - assume the repeat space period is really a repeat space
                  //    period; this would be unusual (repeat period seems to be
                  //    far more common), but seems more fitting given that a
                  //    command consisting of 48 ones would take ~83 ms
                  // - completely avoid implementing Teac/ditto support
                  bool isRepeating = duration < 48000 + TOLERANCE_REPEAT;
                  if (_data.BitCount == 48)
                  {
                    Handle48BitFormat(isRepeating);
                  }
                  else if (_data.BitCount == 56)
                  {
                    Handle56BitFormat(isRepeating);
                  }
                  else
                  {
                    this.LogError("Microsoft blaster Kaseikyo RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                  }
                }

                _state = DecoderState.HeaderPulse;
              }
            }
            else
            {
              this.LogError("Microsoft blaster Kaseikyo RC: unexpected data, is pulse = {0}, is half bit = {1}, duration = {2} us, bit count = {3}", isPulse, _data.IsHalfBit, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
            }
            break;
        }
      }
    }

    private void Handle48BitFormat(bool isRepeating)
    {
      // Check manufacturer code parity.
      ulong manufacturerCode = _data.GetBits(32, 16);
      if ((_data.GetBits(44, 4) ^ _data.GetBits(40, 4) ^ _data.GetBits(36, 4) ^ _data.GetBits(32, 4)) != _data.GetBits(28, 4))
      {
        this.LogError("Microsoft blaster Kaseikyo RC: manufacturer code parity error, bit count = 48, full command = {0}", _data.GetBits(0, 48));
        return;
      }

      if (manufacturerCode == 0x0103 || manufacturerCode == 0x2002 || manufacturerCode == 0x3254)
      {
        // Denon = 16 bit manufacturer code + 4 parity bits + 4 bit genre 1 code + 4 bit genre 2 code + 12 bit data code + 8 parity bits
        // JVC, Panasonic = 16 bit manufacturer code + 4 parity bits + 4 bit system code + 8 bit product code + 8 bit function code + 8 parity bits
        ulong system = _data.GetBits(24, 8);
        ulong product = _data.GetBits(16, 8);
        ulong function = _data.GetBits(8, 8);
        if ((system ^ product ^ function) != _data.GetBits(0, 8))
        {
          this.LogError("Microsoft blaster Kaseikyo RC: Denon/JVC/Panasonic data parity error, bit count = 48, full command = {0}", _data.GetBits(0, 48));
          return;
        }

        system >>= 4;
        if (manufacturerCode == 0x0103)
        {
          this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x0103 [JVC], system = {0}, product = {1}, function = {2}, is repeating = {3}", system, product, function, isRepeating);
          return;
        }
        else if (manufacturerCode == 0x2002)
        {
          this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x2002 [Panasonic], system = {0}, product = {1}, function = {2}, is repeating = {3}", system, product, function, isRepeating);
        }
        else
        {
          // Denon has different terminology and divisions.
          ulong genre2 = _data.GetBits(20, 4);
          ulong data = _data.GetBits(8, 12);
          this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x3254 [Denon], genre 1 = {0}, genre 2 = {1}, data = {2}, is repeating = {3}", system, genre2, data, isRepeating);
        }
        return;
      }

      if (manufacturerCode == 0x5343)
      {
        // Teac = 16 bit manufacturer code + 4 parity bits + 4 bit system code + 8 bit product code + 8 bit function code + 8 parity bits
        ulong system = _data.GetBits(24, 4);
        ulong product = _data.GetBits(16, 8);
        ulong function = _data.GetBits(8, 8);
        if (system + (product >> 4) + (product & 0xf) + (function >> 4) + (function & 0xf) != _data.GetBits(0, 8))
        {
          this.LogError("Microsoft blaster Kaseikyo RC: Teac data parity error, bit count = 48, full command = {0}", _data.GetBits(0, 48));
          return;
        }

        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x5343 [Teac], system = {0}, product = {1}, function = {2}, is repeating = {3}", system, product, function, isRepeating);
      }
      else if (manufacturerCode == 0x5aaa)
      {
        // Sharp = 16 bit manufacturer code + 4 parity bits + 4 bit system code + 8 bit product code + 8 bit function code + 4 bit extension + 4 parity bits
        ulong system = _data.GetBits(24, 4);
        ulong product = _data.GetBits(16, 8);
        ulong function = _data.GetBits(8, 8);
        ulong extension = _data.GetBits(4, 4);
        if ((system ^ (product >> 4) ^ (product & 0xf) ^ (function >> 4) ^ (function & 0xf) ^ extension) != _data.GetBits(0, 4))
        {
          this.LogError("Microsoft blaster Kaseikyo RC: Sharp data parity error, bit count = 48, full command = {0}", _data.GetBits(0, 48));
          return;
        }

        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x5aaa [Sharp], system = {0}, product = {1}, function = {2}, extension = {3}, is repeating = {4}", system, product, function, extension, isRepeating);
      }
      else if (manufacturerCode == 0x6314)
      {
        // Fujitsu = 16 bit manufacturer code + 4 parity bits + 4 bit extension + 8 bit system code + 8 bit product code + 8 bit function code
        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x6314 [Fujitsu], extension = {0}, system = {1}, product = {2}, function = {3}, is repeating = {4}", _data.GetBits(24, 4), _data.GetBits(16, 8), _data.GetBits(8, 8), _data.GetBits(0, 8), isRepeating);
      }
      else if (manufacturerCode == 0xcb23)
      {
        // Mitsubishi = 16 bit manufacturer code + 4 parity bits + 8 bit system code + 8 bit product code + 8 bit function code + 4 bit extension
        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0xcb23 [Mitsubishi], system = {0}, product = {1}, function = {2}, extension = {3}, is repeating = {4}", _data.GetBits(20, 8), _data.GetBits(12, 8), _data.GetBits(4, 8), _data.GetBits(0, 4), isRepeating);
      }
      else
      {
        // other = 16 bit manufacturer code + 4 parity bits + 4 bit system code + 8 bit product code + 8 bit function code + 8 bit extension
        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = {0}, system = {1}, product = {2}, function = {3}, extension = {4}, is repeating = {5}", manufacturerCode, _data.GetBits(24, 4), _data.GetBits(16, 8), _data.GetBits(8, 8), _data.GetBits(0, 8), isRepeating);
      }
    }

    private void Handle56BitFormat(bool isRepeating)
    {
      // Check manufacturer code parity.
      ulong manufacturerCode = _data.GetBits(40, 16);
      if ((_data.GetBits(44, 4) ^ _data.GetBits(40, 4) ^ _data.GetBits(36, 4) ^ _data.GetBits(32, 4)) != _data.GetBits(28, 4))
      {
        this.LogError("Microsoft blaster Kaseikyo RC: manufacturer code parity error, bit count = 56, full command = {0}", _data.GetBits(0, 56));
        return;
      }

      if (manufacturerCode == 0x0103 || manufacturerCode == 0x2002)
      {
        // JVC, Panasonic = 16 bit manufacturer code + 4 parity bits + 4 bit system code + 8 bit product code + 8 bit extension code + 8 bit function code + 8 parity bits
        ulong system = _data.GetBits(32, 8);
        ulong product = _data.GetBits(24, 8);
        ulong extension = _data.GetBits(16, 8);
        ulong function = _data.GetBits(8, 8);
        if ((system ^ product ^ extension ^ function) != _data.GetBits(0, 8))
        {
          this.LogError("Microsoft blaster Kaseikyo RC: JVC/Panasonic data parity error, bit count = 56, full command = {0}", _data.GetBits(0, 56));
          return;
        }

        system >>= 4;
        if (manufacturerCode == 0x0103)
        {
          this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x0103 [JVC], system = {0}, product = {1}, extension = {2}, function = {3}, is repeating = {4}", system, product, extension, function, isRepeating);
          return;
        }

        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x2002 [Panasonic], system = {0}, product = {1}, extension = {2}, function = {3}, is repeating = {4}", system, product, extension, function, isRepeating);
        return;
      }

      if (manufacturerCode == 0x6314)
      {
        // Fujitsu = 16 bit manufacturer code + 4 parity bits + 4 bit extension code + 8 bit system code + 8 bit product code + 8 bit extension 2 code + 8 bit function code
        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = 0x6314 [Fujitsu], extension = {0}, system = {1}, product = {2}, extension 2 = {3}, function = {4}, is repeating = {5}", _data.GetBits(32, 4), _data.GetBits(24, 8), _data.GetBits(16, 8), _data.GetBits(8, 8), _data.GetBits(0, 8), isRepeating);
      }
      else
      {
        // other = 16 bit manufacturer code + 4 parity bits + 4 bit system code + 8 bit product code + 8 bit function code + 8 bit extension code + 8 bit extension 2 code
        this.LogDebug("Microsoft blaster Kaseikyo RC: key press, manufacturer = {0}, system = {1}, product = {2}, function = {3}, extension = {4}, extension 2 = {5}, is repeating = {6}", manufacturerCode, _data.GetBits(32, 4), _data.GetBits(24, 8), _data.GetBits(16, 8), _data.GetBits(8, 8), _data.GetBits(0, 8), isRepeating);
      }
    }
  }
}