#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Drawing;

namespace MediaPortal.TagReader
{
  public interface ITag : IDisposable
  {
    string Album { get; }
    string Artist { get; }
    string AlbumArtist { get; }
    string ArtistURL { get; }
    int AverageBitrate { get; }
    int BitsPerSample { get; }
    int BlocksPerFrame { get; }
    string BuyURL { get; }
    int BytesPerSample { get; }
    int Channels { get; }
    string Comment { get; }
    string Composer { get; }
    int CompressionLevel { get; }
    string Copyright { get; }
    string CopyrightURL { get; }
    byte[] CoverArtImageBytes { get; }
    string FileURL { get; }
    int FormatFlags { get; }
    string Genre { get; }
    bool IsVBR { get; }
    string Keywords { get; }
    string Length { get; }
    int LengthMS { get; }
    string Lyrics { get; }
    string Notes { get; }
    string PeakLevel { get; }
    string PublisherURL { get; }
    int Rating { get; }
    string ReplayGainAlbum { get; }
    string ReplayGainRadio { get; }
    int SampleRate { get; }
    string Title { get; }
    string ToolName { get; }
    string ToolVersion { get; }
    int TotalBlocks { get; }
    int TotalFrames { get; }
    int Track { get; }
    string Version { get; }
    int Year { get; }
    bool Read(string filePath);
    bool SupportsFile(string strFileName);
    int Priority { get; }
  }
}
