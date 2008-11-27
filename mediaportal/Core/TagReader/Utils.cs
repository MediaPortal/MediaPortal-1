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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using MediaPortal.GUI.Library;

namespace MediaPortal.TagReader
{
  public class AudioFileTypeException : Exception
  {
    public AudioFileTypeException(string message)
      : base(message)
    {
    }
  }

  public class Utils
  {
    #region Variables
    public const int ID3_HEADERSIZE = 10;
    private static String[] GenreArray = {
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
                                             "Trash Metal",
                                             "Anime",
                                             "Jpop",
                                             "Synthpop",
        };
    #endregion

    #region Methods
    /// <summary>
    /// Converts a numeric Genre Value in the Genre string from a given Array.
    /// </summary>
    /// <param name="sGenreValue"></param>
    /// <returns></returns>
    public static string GetGenre(string sGenreValue)
    {
      try
      {
        if (sGenreValue.Length == 0)
          return string.Empty;

        int nGenreVal = int.Parse(sGenreValue);
        return GenreArray[nGenreVal];
      }

      catch
      {
        // If we got this far int.Parse threw an exception.

        // The tag probably contains a genre name string instead of a numeric 
        // genre value so just return the original string...
        return sGenreValue;
      }
    }

    /// <summary>
    /// Converts a numeric Genre Value in the Genre string from a given Array.
    /// </summary>
    /// <param name="genreValue"></param>
    /// <returns></returns>
    public static string GetGenre(int genreValue)
    {
      if (genreValue < 0 || genreValue >= GenreArray.Length)
        return string.Empty;

      return GenreArray[genreValue];
    }

    /// <summary>
    /// Deserialize given data into an object
    /// </summary>
    /// <param name="rawdatas"></param>
    /// <param name="anytype"></param>
    /// <returns></returns>
    public static object RawDeserializeEx(byte[] rawdatas, Type anytype)
    {
      int rawsize = Marshal.SizeOf(anytype);

      if (rawsize > rawdatas.Length)
        return null;

      GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
      IntPtr buffer = handle.AddrOfPinnedObject();
      object retobj = Marshal.PtrToStructure(buffer, anytype);
      handle.Free();
      return retobj;
    }

    /// <summary>
    /// Serialize an object into a byte array
    /// </summary>
    /// <param name="anything"></param>
    /// <returns></returns>
    public static byte[] RawSerializeEx(object anything)
    {
      int rawsize = Marshal.SizeOf(anything);
      byte[] rawdatas = new byte[rawsize];
      GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
      IntPtr buffer = handle.AddrOfPinnedObject();
      Marshal.StructureToPtr(anything, buffer, false);
      handle.Free();
      return rawdatas;
    }

    /// <summary>
    /// Read Unsynchronized data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static int ReadUnsynchronizedData(byte[] data, int offset, int length)
    {
      if (length == 0)
        return 0;

      int result = 0;

      for (int i = offset; i < offset + length; i++)
        result = (result << 7) | data[i];

      return result;
    }

    /// <summary>
    /// Read synchronizes Data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static int ReadSynchronizedData(byte[] data, int offset, int length)
    {
      if (length > 4)
        throw new FormatException("Length value greater than 4 bytes");

      int result = 0;

      for (int i = offset; i < offset + length; i++)
        result = (result << 8) | data[i];

      return result;
    }

    public static UInt16 ReadUInt16SynchronizedData(byte[] data, int offset, int length)
    {
      if (length > 2)
        throw new FormatException("Length value greater than 2 bytes");

      UInt16 result = 0;

      for (int i = offset; i < offset + length; i++)
        result = (UInt16)((result << 8) | data[i]);

      return result;
    }

    public static UInt64 ReadUInt64SynchronizedData(byte[] data, int offset, int length)
    {
      if (length > 8)
        throw new FormatException("Length value greater than 8 bytes");

      UInt64 result = 0;

      for (int i = offset; i < offset + length; i++)
        result = (result << 8) | data[i];

      return result;
    }

    public static int GetSynchSafeInt(byte[] data, int offset, int length)
    {
      if (length != 4)
        return 0;

      byte[] destArr = new byte[4];
      Array.Copy(data, offset, destArr, 0, length);

      return GetSynchSafeInt(destArr);
    }

    public static int GetSynchSafeInt(byte[] data)
    {
      if (data.Length != 4)
        return 0;

      int val = 0;
      val |= data[0] << 21;
      val |= data[1] << 14;
      val |= data[2] << 7;
      val |= data[3];

      return val;
    }

    public static int GetInt(byte[] data, int offset, int length)
    {
      if (length != 4)
        return 0;

      byte[] destArr = new byte[4];
      Array.Copy(data, offset, destArr, 0, length);

      return BitConverter.ToInt32(destArr, 0);
    }

    public static int GetBigEndianInt(byte[] data, int offset, int length)
    {
      if (length < 4)
        return 0;

      byte[] temp = new byte[length];
      Buffer.BlockCopy(data, offset, temp, 0, length);

      Array.Reverse(temp);
      return BitConverter.ToInt32(temp, 0);
    }

    public static int GetBigEndianInt(byte[] data)
    {
      if (data.Length < 4)
        return 0;

      Array.Reverse(data);
      return BitConverter.ToInt32(data, 0);
    }

    public static int GetBigEndianInt(int origVal)
    {
      byte[] data = BitConverter.GetBytes(origVal);

      Array.Reverse(data);
      return BitConverter.ToInt32(data, 0);
    }

    public static uint GetBigEndianUInt(byte[] data)
    {
      if (data.Length < 4)
        return 0;

      Array.Reverse(data);
      return BitConverter.ToUInt32(data, 0);
    }

    public static uint GetBigEndianUInt(byte[] data, int offset, int length)
    {
      if (length < 4)
        return 0;

      byte[] temp = new byte[length];
      Buffer.BlockCopy(data, offset, temp, 0, length);

      Array.Reverse(temp);
      return BitConverter.ToUInt32(temp, 0);
    }

    public static uint GetBigEndianUInt(uint origVal)
    {
      byte[] data = BitConverter.GetBytes(origVal);
      Array.Reverse(data);
      return BitConverter.ToUInt32(data, 0);
    }

    public static bool IsFlagSet(byte data, byte bitFlag)
    {
      return ((data & bitFlag) == bitFlag);
    }


    /// <summary>
    /// Clean data from garbage data.
    /// Standard encoding of ISO-8859-1 allows characters 0x20 - 0xFF
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static byte[] CleanData(byte[] array)
    {
      byte[] tempBytes = new byte[array.Length];
      int i;
      int x = 0;

      for (i = 0; i < array.Length; i++)
      {
        byte b = array[i];

        if (b >= 0x20 && b <= 0xFF || b == (byte)'=')
        {
          tempBytes[x++] = b;
        }
      }

      byte[] retval = new byte[x];
      Array.Copy(tempBytes, 0, retval, 0, x);
      return retval;
    }

    /// <summary>
    /// Do we have an alphanueric value
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool IsAlphaNumericValue(char c)
    {
      return (c >= '0' && c <= '9')
          || (c >= 'A' && c <= 'Z')
          || (c >= 'a' && c <= 'z');
    }

    /// <summary>
    /// Do we have an alpha value
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool IsAlphaValue(char c)
    {
      return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    /// <summary>
    /// Do we have a numeric value
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool IsNumericValue(char c)
    {
      return c >= '0' && c <= '9';
    }

    /// <summary>
    /// Convert Duration to string
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public static string GetDurationString(int ms)
    {
      if (ms == 0)
        return "00:00";

      TimeSpan ts = new TimeSpan(0, 0, 0, 0, ms);
      int hr = ts.Hours;
      int min = ts.Minutes;
      int sec = ts.Seconds;

      string sHr = hr > 0 ? string.Format("{0}:", hr) : "";
      return string.Format("{0}{1:D2}:{2:D2}", sHr, min, sec);
    }

    /// <summary>
    /// Extract Year out of a string
    /// </summary>
    /// <param name="sYear"></param>
    /// <returns></returns>
    public static int GetYear(string sYear)
    {
      if (sYear == null)
        return 0;

      sYear = sYear.Trim();

      if (sYear.Length == 0)
        return 0;

      if (sYear[0] == '\0')
        return 0;

      // Handle cases where year includes month:day formatting (2001:12:6)
      char[] delims = new char[] { ':', '-', '_' };

      if (sYear.IndexOfAny(delims) != -1)
      {
        string[] dateSegments = sYear.Split(delims);

        if (dateSegments.Length > 0)
        {
          // Date component should be the first item in the array
          sYear = dateSegments[0];

          if (sYear.Length == 0)
            return 0;
        }
      }
      int yearValue;
      if (!Int32.TryParse(sYear, out yearValue)) yearValue = 0;
      return yearValue;
    }

    /// <summary>
    /// Extract image out of a byte array - please think of disposing the Image in the calling method
    /// </summary>
    /// <param name="imgBytes"></param>
    /// <returns></returns>
    public static Image GetImage(byte[] imgBytes)
    {
      return GetImage(imgBytes, string.Empty);
    }

    /// <summary>
    /// Extract image out of a byte array
    /// </summary>
    /// <param name="imgBytes"></param>
    /// <param name="fileSavePath"></param>
    /// <returns></returns>
    public static Image GetImage(byte[] imgBytes, string fileSavePath)
    {
      if (imgBytes == null || imgBytes.Length == 0)
        return null;

      if (!String.IsNullOrEmpty(fileSavePath))
      {
        FileStream fs = null;

        try
        {
          fs = new FileStream(fileSavePath, FileMode.Create, FileAccess.Write, FileShare.None);
          fs.Write(imgBytes, 0, imgBytes.Length);
        }
        finally
        {
          if (fs != null)
          {
            fs.Close();
            fs = null;
          }
        }
      }

      Image img = null;
      MemoryStream stream = null;

      try
      {
        stream = new MemoryStream(imgBytes);

        try
        {
          // Try without validation first for more speed
          img = Image.FromStream(stream, true, false);
        }
        catch (ArgumentException)
        {
          img = Image.FromStream(stream, true, true);
        }        
      }
      catch (Exception ex)
      {
        Log.Debug("Could not extract Image: {0}", ex.Message);
      }
      finally
      {
        if (stream != null)
        {
          stream.Close();
          stream = null;
        }
      }
      return img;
    }

    public static bool UTF16HasBigEndianBOM(byte[] bom)
    {
      if (bom == null || bom.Length != 2)
        return false;

      return bom[0] == 0xFE && bom[1] == 0xFF;
    }
    /// <summary>
    /// Cleanup a Lyrics string
    /// </summary>
    /// <param name="orig"></param>
    /// <returns></returns>
    public static string CleanLyrics(string orig)
    {
      if (orig.Length == 0)
        return orig;

      int pos = orig.IndexOf("\r\n");

      if (pos == -1)
      {
        pos = orig.IndexOf("\r");

        if (pos > -1)
          orig = orig.Replace("\r", "\r\n");
      }

      string temp = "";

      while (temp != orig)
      {
        temp = orig;
        orig = orig.Replace("\r\n\r\n\r\n", "\r\n\r\n");
      }

      return orig;
    }
    #endregion
  }
}
