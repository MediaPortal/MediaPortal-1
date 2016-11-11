#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.Common.Types.Channel.Constant
{
  public static class ModCod
  {
    public static readonly IDictionary<BroadcastStandard, IList<ModulationSchemeQam>> CABLE = new Dictionary<BroadcastStandard, IList<ModulationSchemeQam>>
    {
      {
        BroadcastStandard.DvbC, new List<ModulationSchemeQam>
        {
          ModulationSchemeQam.Automatic,
          ModulationSchemeQam.Qam16,
          ModulationSchemeQam.Qam32,
          ModulationSchemeQam.Qam64,
          ModulationSchemeQam.Qam128,
          ModulationSchemeQam.Qam256
        }
      },
      {
        BroadcastStandard.IsdbC, new List<ModulationSchemeQam>
        {
          ModulationSchemeQam.Automatic,
          ModulationSchemeQam.Qam64
        }
      },
      {
        BroadcastStandard.Scte, new List<ModulationSchemeQam>
        {
          ModulationSchemeQam.Automatic,
          ModulationSchemeQam.Qam64,
          ModulationSchemeQam.Qam256
        }
      }
    };

    public static readonly IDictionary<BroadcastStandard, IList<ModulationSchemePsk>> SATELLITE = new Dictionary<BroadcastStandard, IList<ModulationSchemePsk>>
    {
      {
        BroadcastStandard.DigiCipher2, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk4SplitI,
          ModulationSchemePsk.Psk4SplitQ,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk4Offset,
          ModulationSchemePsk.Psk8
        }
      },
      {
        BroadcastStandard.DirecTvDss, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk4,
        }
      },
      {
        BroadcastStandard.DvbDsng, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk8
        }
      },
      {
        BroadcastStandard.DvbS, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk2,
          ModulationSchemePsk.Psk4
        }
      },
      {
        BroadcastStandard.DvbS2, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk8
        }
      },
      {
        BroadcastStandard.DvbS2Pro, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk8,
          ModulationSchemePsk.Psk16,
          ModulationSchemePsk.Psk32
        }
      },
      {
        BroadcastStandard.DvbS2X, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk2,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk8,
          ModulationSchemePsk.Psk16,
          ModulationSchemePsk.Psk32,
          ModulationSchemePsk.Psk64,
          ModulationSchemePsk.Psk128,
          ModulationSchemePsk.Psk256
        }
      },
      {
        BroadcastStandard.IsdbS, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk2,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk8
        }
      },
      {
        BroadcastStandard.SatelliteTurboFec, new List<ModulationSchemePsk>
        {
          ModulationSchemePsk.Automatic,
          ModulationSchemePsk.Psk4,
          ModulationSchemePsk.Psk8
        }
      }
    };

    public static readonly IDictionary<BroadcastStandard, Dictionary<ModulationSchemePsk, IList<FecCodeRate>>> SATELLITE_CODE_RATE = new Dictionary<BroadcastStandard, Dictionary<ModulationSchemePsk, IList<FecCodeRate>>>
    {
      {
        BroadcastStandard.DigiCipher2, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          // System C:
          // https://www.itu.int/dms_pubrec/itu-r/rec/bo/R-REC-BO.1516-1-201201-I!!PDF-E.pdf
          // The above specification doesn't mention split mode QPSK, OQPSK or
          // 8 PSK even though we know they're used. Receiver specifications
          // can fill in some of the gaps:
          // https://www.sateng.com/downloads/DSR-4530A1.pdf
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_11,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9
            }
          },
          {
            ModulationSchemePsk.Psk4SplitI, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_11,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk4SplitQ, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_11,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_11,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk4Offset, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,    // seen
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,    // seen
              FecCodeRate.Rate5_11,
              FecCodeRate.Rate7_8     // seen
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>   // turbo
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,    // seen
              FecCodeRate.Rate3_4,    // seen
              FecCodeRate.Rate5_6,    // seen
              FecCodeRate.Rate8_9     // seen
            }
          }
        }
      },
      {
        BroadcastStandard.DirecTvDss, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          // System B:
          // https://www.itu.int/dms_pubrec/itu-r/rec/bo/R-REC-BO.1516-1-201201-I!!PDF-E.pdf
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate6_7
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,    // seen
              FecCodeRate.Rate6_7     // seen
            }
          }
        }
      },
      {
        BroadcastStandard.DvbDsng, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>   // trellis
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate8_9
            }
          }
          // 16 QAM (trellis; 3/4, 7/8) not supported
        }
      },
      {
        BroadcastStandard.DvbS, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk2, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8
            }
          }
        }
      },
      {
        BroadcastStandard.DvbS2, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          }
        }
      },
      {
        BroadcastStandard.DvbS2Pro, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_4,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate2_5,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_4,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate2_5,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk16, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk32, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          }
        }
      },
      {
        BroadcastStandard.DvbS2X, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_4,
              FecCodeRate.Rate1_5,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate2_5,
              FecCodeRate.Rate2_9,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate4_15,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_9,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate7_9,
              FecCodeRate.Rate7_15,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate8_15,
              FecCodeRate.Rate9_10,
              FecCodeRate.Rate9_20,
              FecCodeRate.Rate11_15,
              FecCodeRate.Rate11_20,
              FecCodeRate.Rate11_45,
              FecCodeRate.Rate13_18,
              FecCodeRate.Rate13_45,
              FecCodeRate.Rate14_45,
              FecCodeRate.Rate23_36,
              FecCodeRate.Rate25_36,
              FecCodeRate.Rate26_45,
              FecCodeRate.Rate28_45,
              FecCodeRate.Rate29_45,
              FecCodeRate.Rate31_45,
              FecCodeRate.Rate32_45,
              FecCodeRate.Rate77_90
            }
          },
          {
            ModulationSchemePsk.Psk2, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_5,
              FecCodeRate.Rate4_15,
              FecCodeRate.Rate11_45
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_4,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate2_5,
              FecCodeRate.Rate2_9,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate4_15,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate7_15,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate8_15,
              FecCodeRate.Rate9_10,
              FecCodeRate.Rate9_20,
              FecCodeRate.Rate11_20,
              FecCodeRate.Rate11_45,
              FecCodeRate.Rate13_45,
              FecCodeRate.Rate14_45,
              FecCodeRate.Rate32_45
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_9,
              FecCodeRate.Rate7_15,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate8_15,
              FecCodeRate.Rate9_10,
              FecCodeRate.Rate13_18,
              FecCodeRate.Rate23_36,
              FecCodeRate.Rate25_36,
              FecCodeRate.Rate26_45,
              FecCodeRate.Rate32_45
            }
          },
          {
            ModulationSchemePsk.Psk16, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate5_9,
              FecCodeRate.Rate7_9,
              FecCodeRate.Rate7_15,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate8_15,
              FecCodeRate.Rate9_10,
              FecCodeRate.Rate13_18,
              FecCodeRate.Rate23_36,
              FecCodeRate.Rate25_36,
              FecCodeRate.Rate26_45,
              FecCodeRate.Rate28_45,
              FecCodeRate.Rate32_45,
              FecCodeRate.Rate77_90
            }
          },
          {
            ModulationSchemePsk.Psk32, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_9,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10,
              FecCodeRate.Rate11_15,
              FecCodeRate.Rate32_45
            }
          },
          {
            ModulationSchemePsk.Psk64, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_9,
              FecCodeRate.Rate11_15,
              FecCodeRate.Rate32_45
            }
          },
          {
            ModulationSchemePsk.Psk128, new List<FecCodeRate>
            {
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate7_9
            }
          },
          {
            ModulationSchemePsk.Psk256, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate11_15,
              FecCodeRate.Rate29_45,
              FecCodeRate.Rate31_45,
              FecCodeRate.Rate32_45
            }
          }
        }
      },
      {
        BroadcastStandard.IsdbS, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          // System D:
          // https://www.itu.int/dms_pubrec/itu-r/rec/bo/R-REC-BO.1516-1-201201-I!!PDF-E.pdf
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk2, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate5_6,
              FecCodeRate.Rate7_8
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>   // trellis
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3
            }
          }
        }
      },
      {
        BroadcastStandard.SatelliteTurboFec, new Dictionary<ModulationSchemePsk, IList<FecCodeRate>>
        {
          // Currently turbo coding only seems to be used by Dish Network USA,
          // Dish Network Mexico and Bell TV (Canada). There's not very much
          // technical information available. We're going to assume that
          // combinations are the same as for DVB-S2.
          {
            ModulationSchemePsk.Automatic, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_4,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate2_5,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,    // seen
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk4, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate1_2,
              FecCodeRate.Rate1_3,
              FecCodeRate.Rate1_4,
              FecCodeRate.Rate2_3,
              FecCodeRate.Rate2_5,
              FecCodeRate.Rate3_4,
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate4_5,
              FecCodeRate.Rate5_6,    // seen
              FecCodeRate.Rate7_8,
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          },
          {
            ModulationSchemePsk.Psk8, new List<FecCodeRate>
            {
              FecCodeRate.Automatic,
              FecCodeRate.Rate2_3,    // seen
              FecCodeRate.Rate3_4,    // seen
              FecCodeRate.Rate3_5,
              FecCodeRate.Rate5_6,    // seen
              FecCodeRate.Rate8_9,
              FecCodeRate.Rate9_10
            }
          }
        }
      }
    };

    public static readonly IDictionary<BroadcastStandard, IList<RollOffFactor>> SATELLITE_ROLL_OFF_FACTOR = new Dictionary<BroadcastStandard, IList<RollOffFactor>>
    {
      {
        BroadcastStandard.DigiCipher2, new List<RollOffFactor>
        {
          RollOffFactor.Automatic
          // 0.55 or 0.33 4th order Butterworth filter; assume tuners can determine this automatically.
        }
      },
      {
        BroadcastStandard.DirecTvDss, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive,
        }
      },
      {
        BroadcastStandard.DvbDsng, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive,
          RollOffFactor.Twenty      // only for 8 PSK and 16 QAM
        }
      },
      {
        BroadcastStandard.DvbS, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive
        }
      },
      {
        BroadcastStandard.DvbS2, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive,
          RollOffFactor.TwentyFive,
          RollOffFactor.Twenty
        }
      },
      {
        BroadcastStandard.DvbS2Pro, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive,
          RollOffFactor.TwentyFive,
          RollOffFactor.Twenty
        }
      },
      {
        BroadcastStandard.DvbS2X, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive,
          RollOffFactor.TwentyFive,
          RollOffFactor.Twenty,
          RollOffFactor.Fifteen,
          RollOffFactor.Ten,
          RollOffFactor.Five
        }
      },
      {
        BroadcastStandard.IsdbS, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive
        }
      },
      {
        BroadcastStandard.SatelliteTurboFec, new List<RollOffFactor>
        {
          RollOffFactor.Automatic,
          RollOffFactor.ThirtyFive
        }
      }
    };
  }
}