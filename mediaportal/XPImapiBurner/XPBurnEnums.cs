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

namespace XPBurn
{
  /// <summary>
  /// Ignore this enum for the moment, in the future it will be returned by a property which will indicate which 
  /// types of recording operations are permitted by the active recorder. 
  /// </summary>
  public enum SupportedRecordTypes
  {
    /// <summary>
    /// No recording options available on the recorder set by <see cref="XPBurnCD.BurnerDrive"/>.
    /// </summary>
    sfNone = 0,
    /// <summary>
    /// The current <see cref="XPBurnCD.BurnerDrive"/> allows burning data CDs.
    /// </summary>
    sfData = 1,
    /// <summary>
    /// The currrent <see cref="XPBurnCD.BurnerDrive"/> allows burning music CDs.
    /// </summary>
    sfMusic = 2,
    /// <summary>
    /// The current <see cref="XPBurnCD.BurnerDrive"/> allow burning both data and music CDs.
    /// </summary>
    sfBoth = 3
  }

  /// <summary>
  /// This type is returned by the <see cref="XPBurnCD.ActiveFormat" /> property and indicates whether the component is set to 
  /// write data or music to the CD.
  /// </summary>
  public enum RecordType
  {
    /// <summary>
    /// Used to indicate that the active format is to burn music CDs.
    /// </summary>
    afMusic,
    /// <summary>
    /// Used to indicate that the active format is to burn data CDs.
    /// </summary>
    afData
  }

  /// <summary>
  /// This type is passed as a parameter to the <see cref="XPBurnCD.Erase(EraseKind)" /> procedure.  If set to ekQuick, only the table of contents 
  /// is erased, all of the data still exists on the CD though it is inaccessible by normal means (and may be 
  /// overwritten with a subsequent write).  If set to ekFull all data is erased from each track of the CD.
  /// </summary>
  public enum EraseKind
  {
    /// <summary>
    /// Indicates that burner should perform a quick erase where the table of contents is erased, making the data inaccessible but leaving the
    /// majority of the data on the disc.
    /// </summary>
    ekQuick,
    /// <summary>
    /// Indicates that the burner should perform a full erase where all of the data is removed from the disc.
    /// </summary>
    ekFull
  }

  /// <summary>
  /// This type is returned by the <see cref="XPBurnCD.RecorderType"/> property and indicates whether the active 
  /// recorder can both write and erase CDs (rtCDRW) or only write them (rtCDR).
  /// </summary>
  public enum RecorderType
  {
    /// <summary>
    /// The recorder supports burning CDRs.
    /// </summary>
    rtCDR,
    /// <summary>
    /// The recorder supports burning both CDRs and CDRWs.
    /// </summary>
    rtCDRW
  }
}