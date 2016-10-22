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

using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  // * = last number repeats
  // ' = numbers repeat (eg. 0.45454545...)
  // ... = no repeating pattern
  public enum FecCodeRate
  {
    Automatic,
    [Description("1/5")]
    Rate1_5,                  // 0.2
    [Description("2/9")]
    Rate2_9,                  // 0.22*
    [Description("11/45")]
    Rate11_45,                // 0.244*
    [Description("1/4")]
    Rate1_4,                  // 0.25
    [Description("4/15")]
    Rate4_15,                 // 0.266*
    [Description("13/45")]
    Rate13_45,                // 0.288*
    [Description("14/45")]
    Rate14_45,                // 0.311*
    [Description("1/3")]
    Rate1_3,                  // 0.33*
    [Description("2/5")]
    Rate2_5,                  // 0.4
    [Description("9/20")]
    Rate9_20,                 // 0.45
    [Description("5/11")]
    Rate5_11,                 // 0.45'
    [Description("7/15")]
    Rate7_15,                 // 0.466*
    [Description("1/2")]
    Rate1_2,                  // 0.5
    [Description("8/15")]
    Rate8_15,                 // 0.533*
    [Description("11/20")]
    Rate11_20,                // 0.55
    [Description("5/9")]
    Rate5_9,                  // 0.55*
    [Description("26/45")]
    Rate26_45,                // 0.577*
    [Description("3/5")]
    Rate3_5,                  // 0.6
    [Description("28/45")]
    Rate28_45,                // 0.622*
    [Description("23/36")]
    Rate23_36,                // 0.6388*
    [Description("29/45")]
    Rate29_45,                // 0.644*
    [Description("2/3")]
    Rate2_3,                  // 0.66*
    [Description("31/45")]
    Rate31_45,                // 0.688*
    [Description("25/36")]
    Rate25_36,                // 0.6944*
    [Description("32/45")]
    Rate32_45,                // 0.711*
    [Description("13/18")]
    Rate13_18,                // 0.722*
    [Description("11/15")]
    Rate11_15,                // 0.733*
    [Description("3/4")]
    Rate3_4,                  // 0.75
    [Description("7/9")]
    Rate7_9,                  // 0.77*
    [Description("4/5")]
    Rate4_5,                  // 0.8
    [Description("5/6")]
    Rate5_6,                  // 0.833*
    [Description("77/90")]
    Rate77_90,                // 0.855*
    [Description("6/7")]
    Rate6_7,                  // 0.857...
    [Description("7/8")]
    Rate7_8,                  // 0.875
    [Description("8/9")]
    Rate8_9,                  // 0.88*
    [Description("9/10")]
    Rate9_10                  // 0.9
  }
}