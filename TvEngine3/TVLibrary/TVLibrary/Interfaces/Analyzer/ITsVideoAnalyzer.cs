/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// Interface to the video analyzer com object
  /// </summary>
  [ComVisible(true), ComImport,
  Guid("59f8d617-92fd-48d5-8f6d-a97bfd95c448"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsVideoAnalyzer
  {
    /// <summary>
    /// Sets the video pid.
    /// </summary>
    /// <param name="videoPid">The video pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetVideoPid(short videoPid);

    /// <summary>
    /// Gets the video pid.
    /// </summary>
    /// <param name="videoPid">The video pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetVideoPid(out short videoPid);

    /// <summary>
    /// Sets the audio pid.
    /// </summary>
    /// <param name="audioPid">The audio pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int SetAudioPid(short audioPid);

    /// <summary>
    /// Gets the audio pid.
    /// </summary>
    /// <param name="audioPid">The audio pid.</param>
    /// <returns></returns>
    [PreserveSig]
    int GetAudioPid(out short audioPid);

    /// <summary>
    /// Determines whether video is encrypted
    /// </summary>
    /// <param name="yesNo">1 of encrypted, 0 if not encrypted.</param>
    /// <returns></returns>
    [PreserveSig]
    int IsVideoEncrypted(out short yesNo);

    /// <summary>
    /// Determines whether audio is encrypted
    /// </summary>
    /// <param name="yesNo">1 of encrypted, 0 if not encrypted.</param>
    /// <returns></returns>
    [PreserveSig]
    int IsAudioEncrypted(out short yesNo);

    /// <summary>
    /// Resets video analyzer.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int Reset();
  }
}
