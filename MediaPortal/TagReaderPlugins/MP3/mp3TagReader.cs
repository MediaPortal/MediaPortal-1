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

using System;
using System.IO;
using System.Text.RegularExpressions;
using MediaPortal.TagReader;
using id3;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace MediaPortal.TagReader.ID3
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class MP3TagReader: ITagReader
	{
  
    /// <summary>
    /// Dieses Array dient der Zuordnung des Genre-Bytes aus
    /// den Dateiinformationen zu einem Klarnamen. Es enthält
    /// die Genrenamen im Wortlaut der Spezifikation plus WinAmp-
    /// Erweiterungen, einzusehen unter http://www.id3.org/id3v2-00.txt,
    /// Abschnitt A3.
    /// </summary>
    private static String[] m_genreArray = {
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
                                             "Alternative Rock",
                                             "Bass",
                                             "Soul",
                                             "Punk",
                                             "Space",
                                             "Meditative",
                                             "Instrumental Pop",
                                             "Instrumental Rock",
                                             "Ethnic",
                                             "Gothic",
                                             "Darkwave",
                                             "Techno-Industrial",
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
                                             "Native US",
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
                                             "Folk-Rock",
                                             "National Folk",
                                             "Swing",
                                             "Fast Fusion",
                                             "Bebob",
                                             "Latin",
                                             "Revival",
                                             "Celtic",
                                             "Bluegrass",
                                             "Avantgarde",
                                             "Gothic Rock",
                                             "Progressive Rock",
                                             "Psychedelic Rock",
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
                                             "Rhytmic Soul",
                                             "Freestyle",
                                             "Duet",
                                             "Punk Rock",
                                             "Drum Solo",
                                             "Acapella",
                                             "Euro-House",
                                             "Dance Hall",
    };

    

    const int MPEG_VERSION2_5 =0;
    const int MPEG_VERSION1 =  1;
    const int MPEG_VERSION2 =  2;

        /* Xing header information */
    const int VBR_FRAMES_FLAG= 0x01;
    const int VBR_BYTES_FLAG = 0x02;
    const int VBR_TOC_FLAG   = 0x04;

        // mp3 header flags
    const ulong SYNC_MASK =0xFFE00000000;//(0x7ff << 21);
    const uint VERSION_MASK= (3 << 19);
    const uint LAYER_MASK =(3 << 17);
    const uint PROTECTION_MASK= (1 << 16);
    const uint BITRATE_MASK =(0xf << 12);
    const uint SAMPLERATE_MASK= (3 << 10);
    const uint PADDING_MASK= (1 << 9);
    const uint PRIVATE_MASK= (1 << 8);
    const uint CHANNELMODE_MASK= (3 << 6);
    const uint MODE_EXT_MASK =(3 << 4);
    const uint COPYRIGHT_MASK =(1 << 3);
    const uint ORIGINAL_MASK =(1 << 2);
    const uint EMPHASIS_MASK =3;

    MusicTag m_tag=new MusicTag();
    byte[] m_imageBytes = null;
    bool     m_containsID3Information=false;

    public override bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower()==".mp3") return true;
      return false;
    }
    
    public override int Priority
    {
      get { return 2; }
    }
    /// <summary>
    /// Liest die angegebene Datei ein und speichert, so vorhanden, die
    /// ID3v1-Titelinformationen in den Properties der Instanz dieser Klasse.
    /// </summary>
    /// <param name="filename">Der vollständige Pfad der einzulesenden Datei</param>
    public override bool ReadTag(String filename)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog log = services.Get<ILog>();

      try
      {
//        _log.Info("id3 tag: scan {0}",filename);
        m_imageBytes = null;
        m_tag.Clear();
        m_containsID3Information=false;
        if (File.Exists(filename))
        {
          Tags idtag = new Tags();
          Stream s = null;
          FileInfo file = new FileInfo(filename);
          s = file.OpenRead();
					try
					{
						idtag.Deserialize(s);
						//Log.Write (" read id3tagv2");
						Frame frame = new Frame(idtag.Header);
						foreach(RawFrame rawFrame in idtag)
						{
							try
							{
								frame.Parse(rawFrame);
								string strTag=Strip(rawFrame.Tag).Trim();
								string strValue=Strip(frame.ToString()).Trim();
								if ( (strTag=="TCON" || strTag=="TCO" ) && strValue.Length>0) 
								{
									string strGenre=strValue;
									if (strGenre.Length>2)
									{
										if (strGenre[0]=='(' && strGenre[strGenre.Length-1]==')')
										{
											strGenre=strValue.Substring(1,strGenre.Length-2);
											m_tag.Genre=GetGenre(GetInt(strGenre));
										}
										else m_tag.Genre=strValue;
									}
									else m_tag.Genre=strValue;
								}
								else if ( (strTag=="TALB" || strTag=="TAL" ) && strValue.Length>0) m_tag.Album=strValue;
								else if ( (strTag=="TP1"  || strTag=="TPE1") && strValue.Length>0) m_tag.Artist=strValue;
								else if ( (strTag=="TIT2" || strTag=="TT2" ) && strValue.Length>0) m_tag.Title=strValue;
								else if ( (strTag=="TYER" || strTag=="TYE" ) && strValue.Length>0) m_tag.Year=GetInt(strValue);
								else if ( (strTag=="TRCK" || strTag=="TRK" ) && strValue.Length>0) m_tag.Track=GetInt(strValue);
								else if ( strTag=="COMM" )
								{
									//          FrameLCText lcTextFrame = (FrameLCText)frame.FrameBase;
								}
								else if ( strTag == "APIC" ) 
								{
									FrameAPIC apicFrame = (FrameAPIC)frame.FrameBase;
									m_imageBytes = apicFrame.PictureData;
								}
								m_containsID3Information=true;
							}
							catch(Exception )
							{
								//Log.Write (" error reading id3tagv2");
							}
						}
						try
						{
							m_tag.Duration = ReadDuration(s);

						}
						catch (Exception)
						{
							//Log.Write (" error reading id3tagv1");
						}
					}
					catch(Exception)
					{
					}
          //Log.Write (" read id3tagv1");
          ID3v1 id3v1 = new ID3v1();
          try
          {
            //ParseFileName(filename);
            id3v1.Deserialize(s);
            idtag = id3v1.Tags;
            m_containsID3Information = true;
            if (m_tag.Genre.Length == 0) m_tag.Genre = GetGenre((int)id3v1.Genre);
            if (m_tag.Title.Length == 0) m_tag.Title = Strip(id3v1.Song).Trim();
            if (m_tag.Album.Length == 0) m_tag.Album = Strip(id3v1.Album).Trim();
            if (m_tag.Artist.Length == 0) m_tag.Artist = Strip(id3v1.Artist).Trim();
            if (m_tag.Duration == 0) m_tag.Duration = ReadDuration(s);
            try
            {
              //m_tag.Track = Int32.Parse(id3v1.Track);
                // SourceForge patch #1435798
                if (m_tag.Track == 0) m_tag.Track = Int32.Parse(id3v1.Track);
            }
            catch (Exception)
            {
              //Log.Write (" error reading id3tagv1");
            }
            if (m_tag.Year == 0)
            {
              int year = 0;
              if (int.TryParse(id3v1.Year, out year))
                m_tag.Year = year;
            }
          }
          catch (Exception)
          {
            //Log.Write (" error reading id3tagv1");
          }

          m_tag.Duration=ReadDuration(s);
					try
					{
						m_tag.Duration=ReadDuration(s);
            m_containsID3Information=true;

					}
					catch(Exception )
					{
						//Log.Write (" error reading id3tagv1");
					}
            s.Close();
        }
        else
        {
          log.Warn("id3 tag: scan {0} does not exists?",filename);
        }
      }
      catch (Exception ex)
      {
        log.Error( "Exception reading id3tag of {0} err:{1} stack:{2}",filename,ex.Message, ex.StackTrace);

      }
      if (m_tag!=null)
      {
        if (m_tag.Artist!=null)
        {
          if (m_tag.Artist.ToLower().Equals("va")) 
          {
            m_tag.Artist="Various Artists";
          }
        }
      }

      return m_containsID3Information;
    }//Read

    string Strip(string strLine)
    {
      for (int i=0; i < strLine.Length;++i)
      {
        if (strLine[i]=='\0')
        {
          strLine=strLine.Substring(0,i);
          return strLine;
        }
      }
      return strLine;
    }


    /// <summary>
    /// Liefert das Genre als Klarname aus einer internen Liste.
    /// </summary>
    /// <param name="genreNr">Die im MP3-File gespeicherte Genre-Nr.</param>
    /// <returns>Klarnamen des Genres oder 'Unknown'.</returns>
    private static String GetGenre(int genreNr)
    {
      if (m_genreArray.Length > genreNr)
      {
        return m_genreArray[genreNr];
      }
      else
      {
        return Strings.Unknown;
      }
    }//GetGenre


    public override MusicTag Tag
    {
      get { return m_tag;}
    }

    public override byte[] Image
    {
      get { return m_imageBytes; }
    }
   
    //	Inspired by http://rockbox.haxx.se/ and http://www.xs4all.nl/~rwvtveer/scilla 
    int ReadDuration(Stream file)
    {
	    //int nDuration=0;
	    int nPrependedBytes=0;
	    int xing=0;
	    byte[] buffer = new byte[16384];

	    /* Make sure file has a ID3v2 tag */
	    file.Seek(0,SeekOrigin.Begin);
	    file.Read(buffer,0, 6);

	    if (buffer[0]==0x49/*'I'*/ && buffer[1]==0x44/*'D*/ && buffer[2]==0x33/*'3'*/)
	    {
		    /* Now check what the ID3v2 size field says */
		    file.Read(buffer,0, 4);
		    nPrependedBytes = UNSYNC(buffer[0], buffer[1], buffer[2], buffer[3]) + 10;
	    }

	    //raw mp3Data = FileSize - ID3v1 tag - ID3v2 tag
	    int nMp3DataSize=((int)file.Length)-/*id3tag.GetAppendedBytes()-*/nPrependedBytes;

	    int[,] freqtab=new int[3,4] 
	    {
			    {11025, 12000, 8000, 0},  /* MPEG version 2.5 */
			    {44100, 48000, 32000, 0}, /* MPEG Version 1 */
			    {22050, 24000, 16000, 0}, /* MPEG version 2 */
	    };

	    // Skip ID3V2 tag when reading mp3 data
	    file.Seek(nPrependedBytes, SeekOrigin.Begin);
	    file.Read(buffer, 0,8192);

	    int frequency=0, bitrate=0, bittable=0;
	    int frame_count=0;
	    double tpf=0.0, bpf=0.0;
	    for (int i=0; i<8192; i++)
	    {
		    ulong mpegheader=(ulong)(
																	    ( (buffer[i] & 255) << 24) |
																	    ( (buffer[i+1] & 255) << 16) |
																	    ( (buffer[i+2] & 255) <<  8) |
																	    ( (buffer[i+3] & 255)      )
																    ); 

		    //	Do we have a Xing header before the first mpeg frame?
		    if (buffer[i  ]==0x58/*'X'*/ &&
				    buffer[i+1]==0x69/*'i'*/ &&
				    buffer[i+2]==0x6e/*'n'*/ &&
				    buffer[i+3]==0x67/*'g'*/)
		    {
			    if ((buffer[i+7] & VBR_FRAMES_FLAG) >0)/* Is the frame count there? */
			    {
					    frame_count = BYTES2INT(buffer[i+8], buffer[i+8+1], buffer[i+8+2], buffer[i+8+3]);
			    }
		    }

		    if (IsMp3FrameHeader(mpegheader))
		    {
			    //	skip mpeg header
			    i+=4;
			    int version=0;
			    /* MPEG Audio Version */
			    switch(mpegheader & VERSION_MASK) 
          {
			        case 0:
					        /* MPEG version 2.5 is not an official standard */
					        version = MPEG_VERSION2_5;
					        bittable = MPEG_VERSION2 - 1; /* use the V2 bit rate table */
					        break;
        		      
			        case (1 << 19):
					    return 0;
    		      
			        case (2 << 19):
					      /* MPEG version 2 (ISO/IEC 13818-3) */
					      version = MPEG_VERSION2;
					      bittable = MPEG_VERSION2 - 1;
					    break;
    		      
			        case (3 << 19):
					      /* MPEG version 1 (ISO/IEC 11172-3) */
					      version = MPEG_VERSION1;
					      bittable = MPEG_VERSION1 - 1;
					    break;
			    }

			    int layer=0;
			    switch(mpegheader & LAYER_MASK)
			    {
			      case (3 << 17):	//	LAYER_I
				      layer=1;
				    break;
			      case (2 << 17):	//	LAYER_II
				      layer=2;
				    break;
			      case (1 << 17):	//	LAYER_III
				      layer=3;
				    break;
			    }

			    /* Table of bitrates for MP3 files, all values in kilo.
			    * Indexed by version, layer and value of bit 15-12 in header.
			    */
			    int[,,] bitrate_table =new int[2,4,16] {
					    {
							    {0 , 0, 0, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,0},
							    {0 ,32,64,96,128,160,192,224,256,288,320,352,384,416,448,0},
							    {0 ,32,48,56, 64, 80, 96,112,128,160,192,224,256,320,384,0},
							    {0 ,32,40,48, 56, 64, 80, 96,112,128,160,192,224,256,320,0}
					    },
					    {
							    {0 , 0, 0, 0, 0, 0, 0,  0,  0,  0,  0,  0,  0,  0,  0,0},
							    {0 ,32,48,56,64,80,96,112,128,144,160,176,192,224,256,0},
							    {0 , 8,16,24,32,40,48, 56, 64, 80, 96,112,128,144,160,0},
							    {0 , 8,16,24,32,40,48, 56, 64, 80, 96,112,128,144,160,0}
					    }
			    };

			    /* Bitrate */
			    int bitindex = (int)((mpegheader & 0xf000) >> 12);
			    int freqindex = (int)((mpegheader & 0x0C00) >> 10);
			    bitrate = bitrate_table[bittable,layer,bitindex];
      
			    /* Calculate bytes per frame, calculation depends on layer */
			    switch(layer) 
          {
			        case 1:
					        bpf = bitrate;
					        bpf *= 48000;
					        bpf /= freqtab[version,freqindex] << (version-1);
					    break;
			        case 2:
                goto case 3;
			        case 3:
					        bpf = bitrate;
					        bpf *= 144000;
					        bpf /= freqtab[version,freqindex] << (version-1);
					    break;
			        default:
					      bpf = 1;
              break;
			    }
			    double[] tpfbs = new double[4] { 0, 384.0f, 1152.0f, 1152.0f };
			    frequency = freqtab[version,freqindex];
			    tpf=tpfbs[layer] / (double) frequency;
			    if (version==MPEG_VERSION2_5 && version==MPEG_VERSION2)
				    tpf/=2;

			    if(frequency == 0)
				    return 0;

			    /* Channel mode (stereo/mono) */
			    int chmode = (int)((mpegheader & 0xc0) >> 6);
			    /* calculate position of Xing VBR header */
			    if (version == MPEG_VERSION1) {
				    if (chmode == 3) /* mono */
					    xing = i + 17;
				    else
					    xing = i + 32;
			    }
			    else {
				    if (chmode == 3) /* mono */
					    xing = i + 9;
				    else
					    xing = i + 17;
			    }

            //	Do we have a Xing header
			    if (buffer[xing+0]==0x58/*'X'*/ &&
					    buffer[xing+1]==0x69/*'i'*/ &&
					    buffer[xing+2]==0x6e/*'n'*/ &&
					    buffer[xing+3]==0x67/*'g'*/)
			    {
				    if ((buffer[xing+7] & VBR_FRAMES_FLAG)>0) /* Is the frame count there? */
				    {
						    frame_count = BYTES2INT(buffer[xing+8], buffer[xing+8+1], buffer[xing+8+2], buffer[xing+8+3]);
				    }
			    }
			    //	We are done!
			    break;
		    }
	    }

	    if (frame_count > 0)
	    {
		    double d=tpf * frame_count;
		    return (int)d;
	    }

	    //	Now song length is ((filesize)/(bytes per frame))*(time per frame) 
	    double dDur=(double)nMp3DataSize / bpf * tpf;
	    return (int)dDur;

    }

    bool IsMp3FrameHeader(ulong head)
    {
      if ((head & SYNC_MASK) != SYNC_MASK) /* bad sync? */
        return false;
      if ((head & VERSION_MASK) == (1 << 19)) /* bad version? */
        return false;
      if (0==(head & LAYER_MASK)) /* no layer? */
        return false;
      if ((head & BITRATE_MASK) == BITRATE_MASK) /* bad bitrate? */
        return false;
      if (0==(head & BITRATE_MASK)) /* no bitrate? */
        return false;
      if ((head & SAMPLERATE_MASK) == SAMPLERATE_MASK) /* bad sample rate? */
        return false;
      if (((head >> 19) & 1) == 1 &&
          ((head >> 17) & 3) == 3 &&
          ((head >> 16) & 1) == 1)
        return false;
      if ((head & 0xffff0000) == 0xfffe0000)
        return false;
      return true;
    }

    int BYTES2INT(byte b1,byte b2,byte b3,byte b4) 
    {
      int iReturn=
        (((b1 & 0xFF) << (3*8)) | 
        ((b2 & 0xFF) << (2*8)) | 
        ((b3 & 0xFF) << (1*8)) | 
        ((b4 & 0xFF) << (0*8)));
      return iReturn;
    }

    int UNSYNC(byte b1,byte b2,byte b3,byte b4) 
    {
      int iReturn=
        (((b1 & 0x7F) << (3*7)) | 
        ((b2 & 0x7F) << (2*7)) | 
        ((b3 & 0x7F) << (1*7)) | 
        ((b4 & 0x7F) << (0*7)));
      return iReturn;
    }
    int GetInt(string strValue)
    {
      if (strValue==null) return 0;
      if (strValue.Length==0) return  0;

      string strNumber="";
      for (int i=0; i < strValue.Length;++i)
      {
        if (Char.IsDigit(strValue[i])) strNumber += strValue[i];
        else break;
      }
      int iNumber=0;
      try
      {
        iNumber=Int32.Parse(strNumber);
      }
      catch(Exception){}
      return iNumber;
    }
		void ParseFileName(string path)
		{
			string filename=System.IO.Path.GetFileName(path);
			
			//find track number
			int posTrack=10000;
			int posSong=10000;
			for (int i=0; i < filename.Length-4;++i)
			{
				char nrTens=filename[i];
				char nrUnits=filename[i+1];
				if (nrTens>='0'&& nrTens <='9' && nrUnits >='0'&& nrUnits <='9')
				{
					posTrack=i;
					m_tag.Track = Int32.Parse( String.Format("{0}{1}", nrTens, nrUnits) );
					break;
				}
			}
			// find song name
			int posMinus=filename.LastIndexOf("-");
			if (posMinus>=0)
			{
				posSong=posMinus+1;
				m_tag.Title = filename.Substring(posSong, filename.Length-(4+posSong) ).Trim();
			}
			else
			{
				m_tag.Title=System.IO.Path.GetFileNameWithoutExtension(filename);
			}

			//find artist
			int posArtist=filename.IndexOf("-");
			if (posArtist>=0 && posArtist < posTrack && posArtist < posSong )
			{
				m_tag.Artist=filename.Substring(0,posArtist-1).Trim();
			}


			string[] parts=path.Split( new char[]{'\\'});
			if (parts.Length>=2)
			{
				string folder=parts[parts.Length-2];
				posMinus=folder.IndexOf("-");
				if (posMinus>0)
				{
					m_tag.Artist=folder.Substring(0,posMinus-1).Trim();
					posMinus++;
					m_tag.Album=folder.Substring(posMinus).Trim();
				}
				else
				{
					m_tag.Album=folder;
				}
			}
		}
	}
}
