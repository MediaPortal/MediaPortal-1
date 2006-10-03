#region Copyright (C) 2006 Team MediaPortal
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using MediaPortal.TagReader;
using Tag.MP4.MiscUtil.Conversion;

using MediaPortal.GUI.Library;

namespace Tag.MP4
{
  public class Mp4Tag : TagBase
  {
    #region Genre Array

    private static string[] m_genreArray = 
            {
            "",
            "Blues",
            "Classic Rock",
            "Country",
            "Dance",
            "Disco",
            "Funk",
            "Grunge",
            "Hip-Hop",
            "Jazz",
            "Metal",
            "New Age",
            "Oldies",
            "Other",
            "Pop",
            "R&B",
            "Rap",
            "Reggae",
            "Rock",
            "Techno",
            "Industrial",
            "Alternative",
            "Ska",
            "Death Metal",
            "Pranks",
            "Soundtrack",
            "Euro-Techno",
            "Ambient",
            "Trip-Hop",
            "Vocal",
            "Jazz+Funk",
            "Fusion",
            "Trance",
            "Classical",
            "Instrumental",
            "Acid",
            "House",
            "Game",
            "Sound Clip",
            "Gospel",
            "Noise",
            "Alt. Rock",
            "Bass",
            "Soul",
            "Punk",
            "Space",
            "Meditative",
            "Instrum. Pop",
            "Instrum. Rock",
            "Ethnic",
            "Gothic",
            "Darkwave",
            "Techno-Indust.",
            "Electronic",
            "Pop-Folk",
            "Eurodance",
            "Dream",
            "Southern Rock",
            "Comedy",
            "Cult",
            "Gangsta",
            "Top 40",
            "Christian Rap",
            "Pop/Funk",
            "Jungle",
            "Native American",
            "Cabaret",
            "New Wave",
            "Psychadelic",
            "Rave",
            "Showtunes",
            "Trailer",
            "Lo-Fi",
            "Tribal",
            "Acid Punk",
            "Acid Jazz",
            "Polka",
            "Retro",
            "Musical",
            "Rock & Roll",
            "Hard Rock",
            "Folk",
            "Folk/Rock",
            "National Folk",
            "Swing",
            "Fusion",
            "Bebob",
            "Latin",
            "Revival",
            "Celtic",
            "Bluegrass",
            "Avantgarde",
            "Gothic Rock",
            "Progress. Rock",
            "Psychadel. Rock",
            "Symphonic Rock",
            "Slow Rock",
            "Big Band",
            "Chorus",
            "Easy Listening",
            "Acoustic",
            "Humour",
            "Speech",
            "Chanson",
            "Opera",
            "Chamber Music",
            "Sonata",
            "Symphony",
            "Booty Bass",
            "Primus",
            "Porn Groove",
            "Satire",
            "Slow Jam",
            "Club",
            "Tango",
            "Samba",
            "Folklore",
            "Ballad",
            "Power Ballad",
            "Rhythmic Soul",
            "Freestyle",
            "Duet",
            "Punk Rock",
            "Drum Solo",
            "A Capella",
            "Euro-House",
            "Dance Hall",
            "Goa",
            "Drum & Bass",
            "Club-House",
            "Hardcore",
            "Terror",
            "Indie",
            "BritPop",
            "Negerpunk",
            "Polsk Punk",
            "Beat",
            "Christian Gangsta Rap",
            "Heavy Metal",
            "Black Metal",
            "Crossover",
            "Contemporary Christian",
            "Christian Rock",
            "Merengue",
            "Salsa",
            "Thrash Metal",
            "Anime",
            "Jpop",
            "Synthpop"
		};

    #endregion

    #region Variables

    private ParsedContainerAtom DataAtoms = null;
    private ParsedMvhdAtom MvhdAtom = null;
    private ParsedStsdAtom StsdAtom = null;

    #endregion

    #region Constructors/Destructors
    public Mp4Tag()
      : base()
    {
    }

    ~Mp4Tag()
    {
      Dispose();
    }
    #endregion

    #region Properties

    public override string Album
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©ALB.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string AlbumArtist
    {
      get { return base.AlbumArtist; }
    }

    public override string Artist
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©ART.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string ArtistURL
    {
      get { return base.ArtistURL; }
    }

    public override int AverageBitrate
    {
      get
      {
        try
        {
          if (StsdAtom == null)
            return 0;

          int avgBitrate = StsdAtom.AverageBitRate;

          if (avgBitrate > 0)
          {
            float fAvgBitrate = avgBitrate / 1000f;
            return (int)(fAvgBitrate + .5f);
          }

          return avgBitrate;
        }

        catch (Exception ex)
        {
          Log.Error("MP4Tag.get_AverageBitrate caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    public override int BitsPerSample
    {
      get { return base.BitsPerSample; }
    }

    public override int BlocksPerFrame
    {
      get { return base.BlocksPerFrame; }
    }

    public override string BuyURL
    {
      get { return base.BuyURL; }
    }

    public override int BytesPerSample
    {
      get { return base.BytesPerSample; }
    }

    public override int Channels
    {
      get { return base.Channels; }
    }

    public override string Comment
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©CMT.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string Composer
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©WRT.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override int CompressionLevel
    {
      get { return base.CompressionLevel; }
    }

    public override string Copyright
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©CPY.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string CopyrightURL
    {
      get { return base.CopyrightURL; }
    }

    public override byte[] CoverArtImageBytes
    {
      get
      {
        if (DataAtoms == null)
          return null;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "COVR.DATA");

        // This will be followed by 8 bytes (three for frame size, one for text encoding, three for image format 
        // [iTunes seems to always be JPG], one for picture type [probably 0x00 for "other]), then a null-terminated 
        // description string (looks like iTunes doesn't use the String, so there's just a 0x00), and then the image 
        //data.
        if (dataAtom != null)
        {
          try
          {
            byte[] imgBytes = new byte[dataAtom.Data.Length];
            Array.Copy(dataAtom.Data, 0, imgBytes, 0, dataAtom.Data.Length);
            return imgBytes;
          }

          catch (Exception ex)
          {
            Log.Error("MP4Tag.get_CoverArtImageBytes caused an exception in file {0} : {1}", base.FileName, ex.Message);
          }
        }

        return null;
      }
    }

    public override string FileURL
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "FURL.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override int FormatFlags
    {
      get { return base.FormatFlags; }
    }

    public override string Genre
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "GNRE.DATA");

        if (dataAtom != null)
          return GetGenre(EndianBitConverter.Big.ToInt16(dataAtom.Data, 0));

        else
        {
          dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©GEN.DATA");

          if (dataAtom != null)
            return Encoding.UTF8.GetString(dataAtom.Data);

          else
            return string.Empty;
        }
      }
    }

    public override bool IsVBR
    {
      get { return base.IsVBR; }
    }

    public override string Keywords
    {
      get { return base.Keywords; }
    }

    public override string Length
    {
      get { return Utils.GetDurationString(LengthMS); }
    }

    public override int LengthMS
    {
      get
      {
        if (MvhdAtom == null)
          return 0;

        return MvhdAtom.DurationMS;
      }
    }

    public override string Lyrics
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©LYR.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string Notes
    {
      get { return base.Notes; }
    }

    public override string PeakLevel
    {
      get { return base.PeakLevel; }
    }

    public override string PublisherURL
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "WPUB.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string ReplayGainAlbum
    {
      get { return base.ReplayGainAlbum; }
    }

    public override string ReplayGainRadio
    {
      get { return base.ReplayGainRadio; }
    }

    public override int SampleRate
    {
      get { return base.SampleRate; }
    }

    public override string Title
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©NAM.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string ToolName
    {
      get
      {
        if (DataAtoms == null)
          return string.Empty;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©ENC.DATA");

        if (dataAtom != null)
          return Encoding.UTF8.GetString(dataAtom.Data);

        else
          return string.Empty;
      }
    }

    public override string ToolVersion
    {
      get { return base.ToolVersion; }
    }

    public override int TotalBlocks
    {
      get { return base.TotalBlocks; }
    }

    public override int TotalFrames
    {
      get { return base.TotalFrames; }
    }

    public override int Track
    {
      get
      {
        if (DataAtoms == null)
          return 0;

        ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "TRKN.DATA");

        if (dataAtom != null)
          return EndianBitConverter.Big.ToInt32(dataAtom.Data, 0);

        else
          return 0;
      }
    }

    public override string Version
    {
      get { return base.Version; }
    }

    public override int Year
    {
      get
      {
        try
        {
          if (DataAtoms == null)
            return 0;

          ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(DataAtoms.Children, "©DAY.DATA");

          if (dataAtom != null)
            return Utils.GetYear(Encoding.Default.GetString(dataAtom.Data, 0, 4));

          else
            return 0;
        }

        catch (Exception ex)
        {
          Log.Error("MP4Tag.get_Year caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    #region Public Methods
    public override bool SupportsFile(string strFileName)
    {
      string ext = System.IO.Path.GetExtension(strFileName).ToLower();
      if (ext == ".m4a" || ext == ".m4p") return true;
      return false;
    }

    public override bool Read(string fileName)
    {
      bool result = true;

      try
      {
        ParsedAtom[] atoms = MP4Parser.parseAtoms(fileName);
        DataAtoms = (ParsedContainerAtom)MP4Parser.findAtom(atoms, "MOOV.UDTA.META.ILST");
        ParsedContainerAtom moovAtoms = (ParsedContainerAtom)MP4Parser.findAtom(atoms, "MOOV");

        if (DataAtoms == null)
          result = false;

        if (moovAtoms != null)
        {
          MvhdAtom = (ParsedMvhdAtom)MP4Parser.findAtom(moovAtoms.Children, "MVHD");
        }

        ParsedContainerAtom stblAtoms = (ParsedContainerAtom)MP4Parser.findAtom(atoms, "MOOV.TRAK.MDIA.MINF.STBL");

        if (stblAtoms != null)
          StsdAtom = (ParsedStsdAtom)MP4Parser.findAtom(stblAtoms.Children, "STSD");
      }

      catch (Exception ex)
      {
        Log.Error("MP4Tag.Read caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }
    #endregion

    #region Private Methods
    protected static String GetGenre(int genreNr)
    {
      if (m_genreArray.Length > genreNr)
      {
        return m_genreArray[genreNr];
      }

      else
      {
        //return Strings.Unknown;
        return "Uknown";
      }
    }
    #endregion
  }
}