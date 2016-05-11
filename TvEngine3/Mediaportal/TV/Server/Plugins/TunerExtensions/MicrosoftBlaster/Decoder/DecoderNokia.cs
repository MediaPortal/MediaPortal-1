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
  internal class DecoderNokia : DecoderBase
  {
    private bool _isRepeating = false;
    private bool _isPulsePrevious = false;

    // http://www.sbprojects.com/knowledge/ir/nrc17.php
    public override void Detect(int[] timingData)
    {
      foreach (int d in timingData)
      {
        bool isPulse = d > 0;
        int duration = Math.Abs(d);

        switch (_state)
        {
          case DecoderState.HeaderPulse:
            if (isPulse && IsWithinTolerance(duration, 500, TOLERANCE))
            {
              _state = DecoderState.HeaderSpace;
            }
            break;

          case DecoderState.HeaderSpace:
            if (!isPulse && (IsWithinTolerance(duration, 2500, TOLERANCE) || IsWithinTolerance(duration, 3500, TOLERANCE)))
            {
              if (IsWithinTolerance(duration, 3500, TOLERANCE))
              {
                this.LogWarn("Microsoft blaster Nokia RC: detected remote control battery power is low");
              }
              _state = DecoderState.Data;
              _data.Reset();
              _isPulsePrevious = false;
              break;
            }
            _state = DecoderState.HeaderPulse;
            break;

          case DecoderState.Data:
            if (_isPulsePrevious == isPulse)
            {
              this.LogError("Microsoft blaster Nokia RC: unexpected data, is pulse = {0}, duration = {1} us, bit count = {2}", isPulse, duration, _data.BitCount);
              _state = DecoderState.HeaderPulse;
              continue;
            }
            _isPulsePrevious = isPulse;

            if (IsWithinTolerance(duration, 500, TOLERANCE))
            {
              _data.AddBit(isPulse);
            }
            else if (IsWithinTolerance(duration, 1000, TOLERANCE))
            {
              _data.AddBit(isPulse);
              _data.AddBit(isPulse);
            }
            else
            {
              if (duration < 1000 - TOLERANCE)
              {
                this.LogError("Microsoft blaster Nokia RC: unexpected data, is pulse = {0}, duration = {1} us, bit count = {2}", isPulse, duration, _data.BitCount);
              }
              else
              {
                if (_data.BitCount % 2 != 0)
                {
                  _data.AddBit(false);
                }
                _data.ProcessBiPhase(false);

                if (_data.BitCount == 17)
                {
                  // start        cmd1          cmd2          cmdn          stop
                  // |<- 40 ms ->||<- 100 ms ->||<- 100 ms ->||<- 100 ms ->|

                  //ulong startBit = _data.GetCodeBits(16, 1);
                  ulong command = _data.GetBits(8, 8);
                  ulong address = _data.GetBits(4, 4);
                  ulong subCode = _data.GetBits(0, 4);
                  if (command == 0xfe && address == 0xf && subCode == 0xf)
                  {
                    // Start or stop command.
                    _isRepeating = false;
                  }
                  else
                  {
                    this.LogDebug("Microsoft blaster Nokia RC: key press, command = {0}, address = {1}, sub-code = {2}, is repeating = {3}", command, address, subCode, _isRepeating);
                    _isRepeating = true;
                  }
                }
                else
                {
                  this.LogError("Microsoft blaster Nokia RC: unsupported format, bit count = {0}, full command = {1}", _data.BitCount, _data.GetBits(0, _data.BitCount));
                }
              }

              _state = DecoderState.HeaderPulse;
            }
            break;
        }
      }
    }
  }
}