/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Text;
using MediaPortal.TagReader;
using MediaPortal.TagReader.MP4.MiscUtil.Conversion;
using MediaPortal.GUI.Library;

namespace MediaPortal.TagReader.MP4
{
	/// <summary>
	/// 
	/// </summary>
	public class MP4TagReader: ITagReader
	{
		protected MusicTag m_tag = new MusicTag();
		protected byte[] _imageBytes;

		public MP4TagReader()
		{
			// 
			// TODO: Add constructor logic here
			//
		}

    public override int Priority
    {
      get { return 2; }
    }
		public override bool SupportsFile(string strFileName)
		{
			if (System.IO.Path.GetExtension(strFileName).ToLower()==".m4a") return true;
			if (System.IO.Path.GetExtension(strFileName).ToLower()==".m4p") return true;
			return false;
		}
		public override bool ReadTag(string strFileName)
		{
			Log.Write("mp4 tag: scan {0}",strFileName);
			m_tag.Clear();
			try 
			{
				ParsedAtom[] atoms = MP4Parser.parseAtoms(strFileName);
				ParsedContainerAtom ilstAtom = (ParsedContainerAtom)MP4Parser.findAtom(atoms, "MOOV.UDTA.META.ILST");

				if (ilstAtom != null) 
				{
					// name
					ParsedDataAtom dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "©NAM.DATA");
					if (dataAtom != null) 
					{
						m_tag.Title = Encoding.Default.GetString(dataAtom.Data);
					}

					// artist
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "©ART.DATA");
					if (dataAtom != null) 
					{
						m_tag.Artist = Encoding.Default.GetString(dataAtom.Data);
					}

					// album
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "©ALB.DATA");
					if (dataAtom != null) 
					{
						m_tag.Album = Encoding.Default.GetString(dataAtom.Data);
					}

					// comment
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "©CMT.DATA");
					if (dataAtom != null) 
					{
						m_tag.Comment = Encoding.Default.GetString(dataAtom.Data);
					}

					// genre
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "GNRE.DATA");
					if (dataAtom != null) 
					{
						m_tag.Genre = GetGenre(EndianBitConverter.Big.ToInt16(dataAtom.Data, 0));
					}

					// year
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "©DAY.DATA");
					if (dataAtom != null) 
					{
						m_tag.Year = Convert.ToInt32(Encoding.Default.GetString(dataAtom.Data, 0, 4), 10);
					}

					// track number
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "TRKN.DATA");
					if (dataAtom != null) 
					{
						m_tag.Track = EndianBitConverter.Big.ToInt32(dataAtom.Data, 0);
					}

					// cover image
					dataAtom = (ParsedDataAtom)MP4Parser.findAtom(ilstAtom.Children, "COVR.DATA");

					// This will be followed by 8 bytes (three for frame size, one for text encoding, three for image format [iTunes seems to always be JPG], one for picture type [probably 0x00 for "other]), then a null-terminated description string (looks like iTunes doesn't use the String, so there's just a 0x00), and then the image data.
					if(dataAtom != null) 
					{
						try
						{
							_imageBytes = new byte[dataAtom.Data.Length];

							Array.Copy(dataAtom.Data, 0, _imageBytes, 0, dataAtom.Data.Length);
						}
						catch(Exception e)
						{
							Log.Write("MP4TagReader.ReadTag: {0} while extracting embedded cover art", e.Message);
						}
					}

					// Log.Write("Title={0}", m_tag.Title);
					// Log.Write("Artist={0}", m_tag.Artist);
					// Log.Write("Album={0}", m_tag.Album);
					// Log.Write("Comment={0}", m_tag.Comment);
					// Log.Write("Genre={0}", m_tag.Genre);
					// Log.Write("Year={0}", m_tag.Year);
					// Log.Write("Track={0}", m_tag.Track);

					return true;
				}
			} 
			catch (Exception e) 
			{
				Log.Write("MP4 Parser Error: '{0}'", e.Message);
			}
			return false;
		}

		public override MusicTag Tag
		{
			get { return m_tag;}
		}

		public override byte[] Image
		{
			get { return _imageBytes; }
		}

		protected static String GetGenre(int genreNr)
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

		private static string[] m_genreArray = 
		{
			"", "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", 
			"Grunge", "Hip-Hop", "Jazz", "Metal", "New Age", "Oldies", "Other", 
			"Pop", "R&B", "Rap", "Reggae", "Rock", "Techno", "Industrial", 
			"Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", 
			"Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk", "Fusion", 
			"Trance", "Classical", "Instrumental", "Acid", "House", "Game", 
			"Sound Clip", "Gospel", "Noise", "Alt. Rock", "Bass", "Soul", 
			"Punk", "Space", "Meditative", "Instrum. Pop", "Instrum. Rock", 
			"Ethnic", "Gothic", "Darkwave", "Techno-Indust.", "Electronic", 
			"Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy", 
			"Cult", "Gangsta", "Top 40", "Christian Rap", "Pop/Funk", "Jungle", 
			"Native American", "Cabaret", "New Wave", "Psychadelic", "Rave", 
			"Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", 
			"Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock", "Folk", 
			"Folk/Rock", "National Folk", "Swing", "Fusion", "Bebob", "Latin", 
			"Revival", "Celtic", "Bluegrass", "Avantgarde", "Gothic Rock", 
			"Progress. Rock", "Psychadel. Rock", "Symphonic Rock", "Slow Rock", 
			"Big Band", "Chorus", "Easy Listening", "Acoustic", "Humour", 
			"Speech", "Chanson", "Opera", "Chamber Music", "Sonata", "Symphony", 
			"Booty Bass", "Primus", "Porn Groove", "Satire", "Slow Jam", 
			"Club", "Tango", "Samba", "Folklore", "Ballad", "Power Ballad", 
			"Rhythmic Soul", "Freestyle", "Duet", "Punk Rock", "Drum Solo", 
			"A Capella", "Euro-House", "Dance Hall", "Goa", "Drum & Bass", 
			"Club-House", "Hardcore", "Terror", "Indie", "BritPop", "Negerpunk", 
			"Polsk Punk", "Beat", "Christian Gangsta Rap", "Heavy Metal", 
			"Black Metal", "Crossover", "Contemporary Christian", "Christian Rock",
			"Merengue", "Salsa", "Thrash Metal", "Anime", "Jpop", "Synthpop"
		};
	}
}
