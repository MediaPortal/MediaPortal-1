#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Drawing;

namespace Tag
{
  public class TagBase : IDisposable, MediaPortal.TagReader.ITag
  {
    #region Variables

    protected string AudioFilePath = string.Empty;
    protected string FileName = "";
    protected long FileLength = 0;
    protected long AudioDataStartPostion = 0;
    protected Image _CoverImage = null;
    protected bool ImageChecked = false;

    protected FileStream AudioFileStream = null;

    #endregion

    #region Constructors/Destructors
    public TagBase()
    {
    }

    public TagBase(string filePath)
    {
      AudioFilePath = filePath;
    }
    #endregion

    #region Properties

    virtual public string Album
    {
      get { return string.Empty; }
    }

    virtual public string Artist
    {
      get { return string.Empty; }
    }

    virtual public string AlbumArtist
    {
      get { return string.Empty; }
    }

    virtual public string ArtistURL
    {
      get { return string.Empty; }
    }

    virtual public int AverageBitrate
    {
      get { return 0; }
    }

    virtual public int BitsPerSample
    {
      get { return 0; }
    }

    virtual public int BlocksPerFrame
    {
      get { return 0; }
    }

    virtual public string BuyURL
    {
      get { return string.Empty; }
    }

    virtual public int BytesPerSample
    {
      get { return 0; }
    }

    virtual public int Channels
    {
      get { return 0; }
    }

    virtual public string Comment
    {
      get { return string.Empty; }
    }

    virtual public string Composer
    {
      get { return string.Empty; }
    }

    virtual public int CompressionLevel
    {
      get { return 0; }
    }

    virtual public string Copyright
    {
      get { return string.Empty; }
    }

    virtual public string CopyrightURL
    {
      get { return string.Empty; }
    }

    virtual public byte[] CoverArtImageBytes
    {
      get { return null; }
    }

    virtual public string FileURL
    {
      get { return string.Empty; }
    }

    virtual public int FormatFlags
    {
      get { return 0; }
    }

    virtual public string Genre
    {
      get { return string.Empty; }
    }

    virtual public bool IsVBR
    {
      get { return false; }
    }

    virtual public string Keywords
    {
      get { return string.Empty; }
    }

    virtual public string Length
    {
      get { return string.Empty; }
    }

    virtual public int LengthMS
    {
      get { return 0; }
    }

    virtual public string Lyrics
    {
      get { return string.Empty; }
    }

    virtual public string Notes
    {
      get { return string.Empty; }
    }

    virtual public string PeakLevel
    {
      get { return string.Empty; }
    }

    virtual public string PublisherURL
    {
      get { return string.Empty; }
    }

    virtual public string ReplayGainAlbum
    {
      get { return string.Empty; }
    }

    virtual public string ReplayGainRadio
    {
      get { return string.Empty; }
    }

    virtual public int SampleRate
    {
      get { return 0; }
    }

    virtual public string Title
    {
      get { return string.Empty; }
    }

    virtual public string ToolName
    {
      get { return string.Empty; }
    }

    virtual public string ToolVersion
    {
      get { return string.Empty; }
    }

    virtual public int TotalBlocks
    {
      get { return 0; }
    }

    virtual public int TotalFrames
    {
      get { return 0; }
    }

    virtual public int Track
    {
      get { return 0; }
    }

    virtual public string Version
    {
      get { return string.Empty; }
    }

    virtual public int Year
    {
      get { return 0; }
    }

    virtual public bool SupportsFile(string strFileName)
    {
      return false;
    }

    virtual public int Priority
    {
      get { return 0; }
    }

    virtual public MediaPortal.TagReader.MusicTag Tag
    {
      get { return null; }
    }
    #endregion

    #region Public Methods
    virtual public void Dispose()
    {
      if (AudioFileStream != null)
      {
        AudioFileStream.Close();
        AudioFileStream.Dispose();
        AudioFileStream = null;
      }

      if (_CoverImage != null)
      {
        _CoverImage.Dispose();
        _CoverImage = null;
      }
      GC.SuppressFinalize(this);  //if Dispose is called, don't call the destructor anymore
    }

    virtual public bool Read(string fileName)
    {
      AudioFilePath = fileName;
      FileName = Path.GetFileName(fileName);
      FileLength = new FileInfo(fileName).Length;
      return false;
    }
    #endregion
  }
}
