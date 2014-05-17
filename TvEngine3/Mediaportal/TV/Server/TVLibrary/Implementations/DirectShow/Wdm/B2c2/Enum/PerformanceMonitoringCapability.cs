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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum
{
  [Flags]
  internal enum PerformanceMonitoringCapability
  {
    None = 0,
    BitErrorRate = 1,           // BER reporting via GetPreErrorCorrectionBER().
    BlockCount = 2,             // Block count reporting via GetTotalBlocks().
    CorrectedBlockCount = 4,    // Corrected block count reporting via GetCorrectedBlocks().
    UncorrectedBlockCount = 8,  // Uncorrected block count reporting via GetUncorrectedBlocks().
    SignalToNoiseRatio = 16,    // SNR reporting via GetSNR().
    SignalStrength = 32,        // Signal strength percentage reporting via GetSignalStrength().
    SignalQuality = 64          // Signal quality percentage reporting via GetSignalQuality().
  }
}