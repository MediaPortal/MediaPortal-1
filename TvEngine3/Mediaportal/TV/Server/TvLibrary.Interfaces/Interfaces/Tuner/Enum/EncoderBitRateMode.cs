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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum
{
  /// <summary>
  /// used by the IVideoEncoder interface getvalue(ENCAPIPARAM_BITRATE_MODE)
  /// </summary>
  public enum EncoderBitRateMode
  {
    /// <summary>
    /// Undefined bit rate mode.
    /// </summary>
    Undefined = -1,
    /// <summary>
    /// Constant bit rate mode.
    /// </summary>
    ConstantBitRate = 0,
    /// <summary>
    /// Variable bit rate mode.
    /// </summary>
    VariableBitRateAverage,
    /// <summary>
    /// Variable peak bit rate mode.
    /// </summary>
    VariableBitRatePeak,
    /// <summary>
    /// Bit rate mode not set.
    /// </summary>
    NotSet
  }
}