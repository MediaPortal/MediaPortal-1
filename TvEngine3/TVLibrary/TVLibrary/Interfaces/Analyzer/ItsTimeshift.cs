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

using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// interface to the timeshift com object
  /// </summary>
  [ComVisible(true), ComImport,
   Guid("89459BF6-D00E-4d28-928E-9DA8F76B6D3A"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsTimeShift
  {
    ///<summary>
    /// Sets the timeshift filename
    ///</summary>
    ///<param name="fileName">Filename</param>
    ///<returns>Error code</returns>
    [PreserveSig]
    int SetTimeShiftingFileName([In, MarshalAs(UnmanagedType.LPStr)] string fileName);

    /// <summary>
    /// Starts timeshifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Start();

    /// <summary>
    /// Stops timeshifting.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Stop();

    /// <summary>
    /// Resets the timeshifting .
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();

    /// <summary>
    /// Gets the size of the buffer.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetBufferSize(out uint size);

    /// <summary>
    /// Gets the number of timeshifting files added.
    /// </summary>
    /// <param name="numbAdd">The number of timeshifting files added.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNumbFilesAdded(out ushort numbAdd);

    /// <summary>
    /// Gets the number of timeshifting  removed.
    /// </summary>
    /// <param name="numbRem">The number of timeshifting  removed.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetNumbFilesRemoved(out ushort numbRem);

    /// <summary>
    /// Gets the current file id.
    /// </summary>
    /// <param name="fileID">The file ID.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetCurrentFileId(out ushort fileID);

    /// <summary>
    /// Gets the mininium number of .TS files.
    /// </summary>
    /// <param name="minFiles">The mininium number of .TS files.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMinTSFiles(out ushort minFiles);

    /// <summary>
    /// Sets the mininium number of .TS files.
    /// </summary>
    /// <param name="minFiles">The mininium number of .TS files.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMinTSFiles(ushort minFiles);

    /// <summary>
    /// Gets the max number of .TS files.
    /// </summary>
    /// <param name="maxFiles">The max number of .TS files</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMaxTSFiles(out ushort maxFiles);

    /// <summary>
    /// Sets the max number of .TS files..
    /// </summary>
    /// <param name="maxFiles">the max number of .TS files.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMaxTSFiles(ushort maxFiles);

    /// <summary>
    /// Gets the maxium filesize for each .ts file
    /// </summary>
    /// <param name="maxSize">the maxium filesize for each .ts file.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetMaxTSFileSize(out long maxSize);

    /// <summary>
    /// Sets the maxium filesize for each .ts file
    /// </summary>
    /// <param name="maxSize">the maxium filesize for each .ts file</param>
    /// <returns></returns>
    [PreserveSig]
    int SetMaxTSFileSize(long maxSize);

    /// <summary>
    /// Gets the chunk reserve for each .ts file.
    /// </summary>
    /// <param name="chunkSize">Size of the chunk.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetChunkReserve(out long chunkSize);

    /// <summary>
    /// Sets the chunk reserve for each .ts file.
    /// </summary>
    /// <param name="chunkSize">Size of the chunk.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetChunkReserve(long chunkSize);

    /// <summary>
    /// Gets the size of the file buffer.
    /// </summary>
    /// <param name="lpllsize">The lpllsize.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetFileBufferSize(out long lpllsize);

    /// <summary>
    /// Sets the PMT pid.
    /// </summary>
    /// <param name="pmtPid">The PMT pid.</param>
    /// <param name="serviceId">The service id</param>
    /// <returns></returns>
    [PreserveSig]
    int SetPmtPid(int pmtPid, int serviceId);

    /// <summary>
    /// pauses or continues writing to the timeshifting file.
    /// </summary>
    /// <param name="onoff">if true then pause, else run.</param>
    /// <returns></returns>
    [PreserveSig]
    int Pause(byte onoff);
  }
}