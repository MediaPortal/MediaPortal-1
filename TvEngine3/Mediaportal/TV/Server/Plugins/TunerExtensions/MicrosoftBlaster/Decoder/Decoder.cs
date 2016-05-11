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
using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Decoder
{
  internal class Decoder
  {
    private ICollection<DecoderBase> _decoders = new List<DecoderBase>
    {
      new DecoderAiwa(),
      new DecoderDaewoo(),
      new DecoderJvc(),
      new DecoderKaseikyo(),
      new DecoderNec(),
      new DecoderNokia(),
      new DecoderPanasonicOld(),
      new DecoderRc5(),
      new DecoderRc6(),
      new DecoderRca(),
      new DecoderRecs80(),
      new DecoderSamsung(),
      new DecoderSony()
    };

    public void Decode(int[] timingData)
    {
      if (timingData == null)
      {
        return;
      }
      foreach (DecoderBase d in _decoders)
      {
        try
        {
          d.Detect(timingData);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Microsoft blaster decoder: unexpected decode/detect error, decoder = {0}", d.GetType().Name);
        }
      }
    }
  }
}