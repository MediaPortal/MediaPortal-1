#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using MediaPortal.GUI.Library;
using TagLib;
using TagLib.Ogg;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// This class will manage all tagreader plugins
  /// See the ITag.cs for more information about tagreader plugins
  /// It will load all tagreader plugins and when Mediaportal wants information for a given music file
  /// it will check which tagreader plugin supports it and ask it to read the information
  /// which is then returned to mediaportal
  /// </summary>
  public class TagReader
  {
    /// <summary>
    /// Constructor
    /// This will load all tagreader plugins from plugins/tagreaders
    /// </summary>
    static TagReader() { }

    /// <summary>
    /// This method is called by mediaportal when it wants information for a music file
    /// The method will check which tagreader supports the file and ask it to extract the information from it
    /// </summary>
    /// <param name="strFile">filename of the music file</param>
    /// <returns>
    /// MusicTag instance when file has been read
    /// null when file type is not supported or if the file does not contain any information
    /// </returns>
    public static MusicTag ReadTag(string strFile)
    {
      // Read Cue info
      if (CueUtil.isCueFakeTrackFile(strFile))
      {
        try
        {
          return CueUtil.CueFakeTrackFile2MusicTag(strFile);
        }
        catch (Exception ex)
        {
          Log.Warn("TagReader: Exception reading file {0}. {1}", strFile, ex.Message);
        }
      }

      if (!IsAudio(strFile))
        return null;

      char[] trimChars = { ' ', '\x00' };

      try
      {
        // Set the flag to use the standard System Encoding set by the user
        // Otherwise Latin1 is used as default, which causes characters in various languages being displayed wrong
        TagLib.ByteVector.UseBrokenLatin1Behavior = true;
        TagLib.File tag = TagLib.File.Create(strFile);
        if (tag == null)
        {
          Log.Warn("Tagreader: No tag in file - {0}", strFile);
          return null;
        }

        MusicTag musictag = new MusicTag();
        string[] artists = tag.Tag.Performers;
        if (artists.Length > 0)
        {
          musictag.Artist = String.Join(";", artists).Trim(trimChars);
          // The AC/DC exception
          if (musictag.Artist.Contains("AC;DC"))
          {
            musictag.Artist = musictag.Artist.Replace("AC;DC", "AC/DC");
          }
        }

        musictag.Album = tag.Tag.Album == null ? "" : tag.Tag.Album.Trim(trimChars);
        musictag.HasAlbumArtist = false;
        string[] albumartists = tag.Tag.AlbumArtists;
        if (albumartists.Length > 0)
        {
          musictag.AlbumArtist = String.Join(";", albumartists).Trim(trimChars);
          musictag.HasAlbumArtist = true;
          // The AC/DC exception
          if (musictag.AlbumArtist.Contains("AC;DC"))
          {
            musictag.AlbumArtist = musictag.AlbumArtist.Replace("AC;DC", "AC/DC");
          }
        }
        musictag.BitRate = tag.Properties.AudioBitrate;
        musictag.Comment = tag.Tag.Comment == null ? "" : tag.Tag.Comment.Trim(trimChars);
        string[] composer = tag.Tag.Composers;
        if (composer.Length > 0)
        {
          musictag.Composer = string.Join(";", composer).Trim(trimChars);
        }
        musictag.Conductor = tag.Tag.Conductor == null ? "" : tag.Tag.Conductor.Trim(trimChars);
        IPicture[] pics = new IPicture[] { };
        pics = tag.Tag.Pictures;
        if (pics.Length > 0)
        {
          musictag.CoverArtImageBytes = pics[0].Data.Data;
        }
        musictag.Duration = (int)tag.Properties.Duration.TotalSeconds;
        musictag.FileName = strFile;
        musictag.FileType = tag.MimeType.Substring(tag.MimeType.IndexOf("/") + 1);
        string[] genre = tag.Tag.Genres;
        if (genre.Length > 0)
        {
          musictag.Genre = String.Join(";", genre).Trim(trimChars);
        }
        musictag.Lyrics = tag.Tag.Lyrics == null ? "" : tag.Tag.Lyrics.Trim(trimChars);
        musictag.Title = tag.Tag.Title == null ? "" : tag.Tag.Title.Trim(trimChars);
        // Prevent Null Ref execption, when Title is not set
        musictag.Track = (int)tag.Tag.Track;
        musictag.TrackTotal = (int)tag.Tag.TrackCount;
        musictag.DiscID = (int)tag.Tag.Disc;
        musictag.DiscTotal = (int)tag.Tag.DiscCount;
        musictag.Codec = tag.Properties.Description;
        if (tag.MimeType == "taglib/mp3")
        {
          musictag.BitRateMode = tag.Properties.Description.IndexOf("VBR") > -1 ? "VBR" : "CBR";
        }
        else
        {
          musictag.BitRateMode = "";
        }
        musictag.BPM = (int)tag.Tag.BeatsPerMinute;
        musictag.Channels = tag.Properties.AudioChannels;
        musictag.SampleRate = tag.Properties.AudioSampleRate;
        musictag.Year = (int)tag.Tag.Year;
        musictag.ReplayGainTrack = tag.Tag.ReplayGainTrack ?? "";
        musictag.ReplayGainTrackPeak = tag.Tag.ReplayGainTrackPeak ?? "";
        musictag.ReplayGainAlbum = tag.Tag.ReplayGainAlbum ?? "";
        musictag.ReplayGainAlbumPeak = tag.Tag.ReplayGainAlbumPeak ?? "";

        if (tag.MimeType == "taglib/mp3")
        {
          bool foundPopm = false;
          // Handle the Rating, which comes from the POPM frame
          TagLib.Id3v2.Tag id32_tag = tag.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
          if (id32_tag != null)
          {
            // Do we have a POPM frame written by MediaPortal or MPTagThat?
            TagLib.Id3v2.PopularimeterFrame popmFrame = TagLib.Id3v2.PopularimeterFrame.Get(id32_tag, "MediaPortal",
                                                                                            false);
            if (popmFrame == null)
            {
              popmFrame = TagLib.Id3v2.PopularimeterFrame.Get(id32_tag, "MPTagThat", false);
            }
            if (popmFrame != null)
            {
              musictag.Rating = popmFrame.Rating;
              foundPopm = true;
            }

            // Now look for a POPM frame written by WMP
            if (!foundPopm)
            {
              TagLib.Id3v2.PopularimeterFrame popm = TagLib.Id3v2.PopularimeterFrame.Get(id32_tag,
                                                                                         "Windows Media Player 9 Series",
                                                                                         false);
              if (popm != null)
              {
                // Get the rating stored in the WMP POPM frame
                int rating = popm.Rating;
                int i = 0;
                if (rating == 255)
                  i = 5;
                else if (rating == 196)
                  i = 4;
                else if (rating == 128)
                  i = 3;
                else if (rating == 64)
                  i = 2;
                else if (rating == 1)
                  i = 1;

                musictag.Rating = i;
                foundPopm = true;
              }
            }

            if (!foundPopm)
            {
              // Now look for any other POPM frame that might exist
              foreach (TagLib.Id3v2.PopularimeterFrame popm in id32_tag.GetFrames<TagLib.Id3v2.PopularimeterFrame>())
              {
                int rating = popm.Rating;
                int i = 0;
                if (rating > 205 || rating == 5)
                  i = 5;
                else if (rating > 154 || rating == 4)
                  i = 4;
                else if (rating > 104 || rating == 3)
                  i = 3;
                else if (rating > 53 || rating == 2)
                  i = 2;
                else if (rating > 0 || rating == 1)
                  i = 1;

                musictag.Rating = i;
                foundPopm = true;
                break; // we only take the first popm frame
              }
            }

            if (!foundPopm)
            {
              // If we don't have any POPM frame, we might have an APE Tag embedded in the mp3 file
              TagLib.Ape.Tag apetag = tag.GetTag(TagTypes.Ape, false) as TagLib.Ape.Tag;
              if (apetag != null)
              {
                TagLib.Ape.Item apeItem = apetag.GetItem("RATING");
                if (apeItem != null)
                {
                  string rating = apeItem.ToString();
                  try
                  {
                    musictag.Rating = Convert.ToInt32(rating);
                  }
                  catch (Exception ex)
                  {
                    musictag.Rating = 0;
                    Log.Warn("Tagreader: Unsupported APE rating format - {0} in {1} {2}", rating, strFile, ex.Message);
                  }
                }
              }
            }
          }
        }
        else if (tag.MimeType == "taglib/ape")
        {
          TagLib.Ape.Tag apetag = tag.GetTag(TagTypes.Ape, false) as TagLib.Ape.Tag;
          if (apetag != null)
          {
            TagLib.Ape.Item apeItem = apetag.GetItem("RATING");
            if (apeItem != null)
            {
              string rating = apeItem.ToString();
              try
              {
                musictag.Rating = Convert.ToInt32(rating);
              }
              catch (Exception ex)
              {
                musictag.Rating = 0;
                Log.Warn("Tagreader: Unsupported APE rating format - {0} in {1} {2}", rating, strFile, ex.Message);
              }
            }
          }
        }
        else if (tag.MimeType == "taglib/ogg" || tag.MimeType == "taglib/flac")
        {
          var xiph = tag.GetTag(TagTypes.Xiph, false) as XiphComment;
          if (xiph != null)
          {
            string[] rating = xiph.GetField("RATING");
            if (rating.Length > 0)
            {
              try
              {
                musictag.Rating = Convert.ToInt32(rating[0]);
              }
              catch (Exception)
              {
              }
            }
          }
        }

        // if we didn't get a title, use the Filename without extension to prevent the file to appear as "unknown"
        if (musictag.Title == "")
        {
          Log.Warn("TagReader: Empty Title found in file: {0}. Please retag.", strFile);
          musictag.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);
        }

        return musictag;
      }
      catch (UnsupportedFormatException)
      {
        Log.Warn("Tagreader: Unsupported File Format {0}", strFile);
      }
      catch (Exception ex)
      {
        Log.Warn("TagReader: Exception reading file {0}. {1}", strFile, ex.Message);
      }
      return null;
    }

    public static bool WriteLyrics(string strFile, string strLyrics)
    {
      if (!IsAudio(strFile))
        return false;

      try
      {
        // Set the flag to use the standard System Encoding set by the user
        // Otherwise Latin1 is used as default, which causes characters in various languages being displayed wrong
        TagLib.ByteVector.UseBrokenLatin1Behavior = true;
        TagLib.File tag = TagLib.File.Create(strFile);
        tag.Tag.Lyrics = strLyrics;
        tag.Save();
        return true;
      }
      catch (UnsupportedFormatException)
      {
        Log.Warn("Tagreader: Unsupported File Format {0}", strFile);
      }
      catch (Exception ex)
      {
        Log.Warn("TagReader: Exception writing file {0}. {1}", strFile, ex.Message);
      }
      return false;
    }

    private static bool IsAudio(string fileName)
    {
      string ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

      switch (ext)
      {
        case ".aif":
        case ".aiff":
        case ".ape":
        case ".flac":
        case ".mp3":
        case ".ogg":
        case ".wv":
        case ".wav":
        case ".wma":
        case ".mp4":
        case ".m4a":
        case ".m4b":
        case ".m4p":
        case ".mpc":
        case ".mp+":
        case ".mpp":
          return true;
      }

      return false;
    }
  }
}