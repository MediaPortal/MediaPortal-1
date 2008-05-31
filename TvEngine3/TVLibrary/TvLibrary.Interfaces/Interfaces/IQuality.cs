/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Implementations.Analog;

namespace TvLibrary.Interfaces
{

    /// <summary>
  /// used by the IVideoEncoder interface getvalue(ENCAPIPARAM_BITRATE_MODE)
  /// </summary>
  public enum VIDEOENCODER_BITRATE_MODE
  {
    Undefined = -1,
    ConstantBitRate = 0,
    VariableBitRateAverage,
    VariableBitRatePeak,
    NotSet
  };


  /// <summary>
  /// QualityType's for setting the desired quality
  /// </summary>
  public enum QualityType
  {
    /// <summary>default quality</summary>
    Default = 1,
    /// <summary>custom quality setting, defined in SetupTv</summary>
    Custom = 2,
    /// <summary>portable quality setting for those recordings that dont need to be close to perfect</summary>
    Portable = 3,
    /// <summary>low quality setting for those recordings that dont need to be close to perfect</summary>
    Low = 4,
    /// <summary>medium quality but still quite a bit less diskspace needed than high</summary>
    Medium = 5,
    /// <summary>high quality setting will create larger files then the other options</summary>
    High = 6,
    /// <summary>undefined quality</summary>
    NotSet = 7
  }

  /// <summary>
  /// interface for quality control of a card
  /// </summary>
  public interface IQuality
  {
    /// <summary>
    /// Gets/Sets the quality bit type (only the bit rate)
    /// </summary>
    QualityType QualityType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets/Sets the bit rate mode. Works only if this is supported
    /// </summary>
    VIDEOENCODER_BITRATE_MODE BitRateMode
    {
      get;
      set;
    }

    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsBitRateModes();

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsPeakBitRateMode();

    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsBitRate();

    /// <summary>
    /// Called when playback starts
    /// </summary>
    void StartPlayback();

    /// <summary>
    /// Called when record starts
    /// </summary>
    void StartRecord();

    /// <summary>
    /// Sets the new configuration object
    /// </summary>
    void SetConfiguration(Configuration configuration);

  }
}
