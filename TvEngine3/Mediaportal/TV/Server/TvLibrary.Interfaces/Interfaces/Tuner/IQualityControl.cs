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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner
{
  /// <summary>
  /// Tuner quality control interface.
  /// </summary>
  public interface IQualityControl
  {
    /// <summary>
    /// Determine which (if any) quality control features are supported by the tuner.
    /// </summary>
    /// <param name="supportedEncodeModes">The encoding modes supported by the tuner.</param>
    /// <param name="canSetBitRate"><c>True</c> if the tuner's average and/or peak bit-rate can be set.</param>
    void GetSupportedFeatures(out EncodeMode supportedEncodeModes, out bool canSetBitRate);

    /// <summary>
    /// Get and/or set the tuner's video and/or audio encoding mode.
    /// </summary>
    EncodeMode EncodeMode { get; set; }

    /// <summary>
    /// Get and/or set the tuner's average video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    int AverageBitRate { get; set; }

    /// <summary>
    /// Get and/or set the tuner's peak video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    int PeakBitRate { get; set; }
  }
}