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

using Mediaportal.TV.Server.Common.Types.Enum;
using MediaPortal.Common.Utils.ExtensionMethods;
using BroadcastStandardEnum = Mediaportal.TV.Server.Common.Types.Enum.BroadcastStandard;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class TuningDetail
  {
    public override string ToString()
    {
      return Name;
    }

    public string GetDescriptiveString()
    {
      string frequencyMhz = string.Format("{0:#.##}", (float)Frequency / 1000);
      switch ((BroadcastStandardEnum)BroadcastStandard)
      {
        case BroadcastStandardEnum.ExternalInput:
          if (string.Equals(LogicalChannelNumber, "0") || string.Equals(LogicalChannelNumber, "10000"))
          {
            return string.Format("{0}, {1}", ((CaptureSourceVideo)VideoSource).GetDescription(), ((CaptureSourceAudio)AudioSource).GetDescription());
          }
          return string.Format("#{0} {1}, {2}", LogicalChannelNumber, ((CaptureSourceVideo)VideoSource).GetDescription(), ((CaptureSourceAudio)AudioSource).GetDescription());
        case BroadcastStandardEnum.AnalogTelevision:
          if (Frequency > 0)
          {
            return string.Format("#{0} {1} MHz", PhysicalChannelNumber, frequencyMhz);
          }
          return string.Format("#{0}", PhysicalChannelNumber);
        case BroadcastStandardEnum.AmRadio:
          return string.Format("{0} kHz", Frequency);
        case BroadcastStandardEnum.FmRadio:
          return string.Format("{0} MHz", frequencyMhz);
        case BroadcastStandardEnum.DvbC:
          return string.Format("{0} MHz, {1}, {2} ks/s", frequencyMhz, ((ModulationSchemeQam)Modulation).GetDescription(), SymbolRate);
        case BroadcastStandardEnum.DvbC2:
        case BroadcastStandardEnum.DvbT:
        case BroadcastStandardEnum.DvbT2:
          if (BroadcastStandard != (int)BroadcastStandardEnum.DvbT && StreamId >= 0)
          {
            return string.Format("{0} MHz, BW {1:#.##} MHz, PLP {2}", frequencyMhz, (float)Bandwidth / 1000, StreamId);
          }
          return string.Format("{0} MHz, BW {1:#.##} MHz", frequencyMhz, (float)Bandwidth / 1000);
        case BroadcastStandardEnum.DvbDsng:
        case BroadcastStandardEnum.DvbS:
        case BroadcastStandardEnum.DvbS2:
        case BroadcastStandardEnum.DvbS2X:
        case BroadcastStandardEnum.SatelliteTurboFec:
        case BroadcastStandardEnum.DigiCipher2:
          string satellite = string.Format("sat {0}", IdSatellite);
          if (Satellite != null)
          {
            satellite = Satellite.LongitudeString();
          }
          return string.Format("{0} {1} MHz, {2}, {3}, {4} ks/s", satellite, frequencyMhz, ((Polarisation)Polarisation).GetDescription(), ((ModulationSchemePsk)Modulation).GetDescription(), SymbolRate);
        case BroadcastStandardEnum.DvbIp:
          return Url;
        case BroadcastStandardEnum.Atsc:
          return string.Format("{0} MHz, {1}", frequencyMhz, ((ModulationSchemeVsb)Modulation).GetDescription());
        case BroadcastStandardEnum.Scte:
          return string.Format("{0} MHz, {1}", frequencyMhz, ((ModulationSchemeQam)Modulation).GetDescription());

        // Not implemented.
        case BroadcastStandardEnum.IsdbC:
        case BroadcastStandardEnum.IsdbS:
        case BroadcastStandardEnum.IsdbT:
        case BroadcastStandardEnum.DirecTvDss:
        case BroadcastStandardEnum.Dab:
        default:
          return string.Empty;
      }
    }
  }
}