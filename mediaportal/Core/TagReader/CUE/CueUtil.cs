#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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
using System.Collections.Generic;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// Different Cue functions.
  /// Some functions is Not thread safe
  /// </summary>
  public class CueUtil
  {
    #region Variables

    public const string CUE_FILE_EXT = "cue";
    public const string WAV_CUE_FILE_EXT = "wav.cue";
    public const string CUE_FAKE_TRACK_FILE_EXT = "cue.fake.track";

    public static ICueTrackFileBuilder<GUIListItem> CUE_TRACK_FILE_GUI_LIST_ITEM_BUILDER =
      new CueTrackFileGUIListItemBuilder();

    public static ICueTrackFileBuilder<string> CUE_TRACK_FILE_STRING_BUILDER = new CueTrackFileStringBuilder();

    private static string cueFakeTrackFileNameCache = null;
    private static string cueSheetCacheFileNameCache = null;
    private static CueSheet cueSheetCache = null;
    private static MusicTag musicTagCache = null;
    private static string cacheFName = null;
    private static TagCache tagCache = null;
    private static Object cacheLock = new Object();

    #endregion

    #region Public Methods

    /// <summary>
    /// Check if file is Cue file
    /// </summary>
    /// <param name="fileName">file name to check</param>
    /// <returns>Returns true if file is Cue file</returns>
    public static Boolean isWavCueFile(string fileName)
    {
      return fileName.ToLowerInvariant().EndsWith("." + WAV_CUE_FILE_EXT);
    }

    /// <summary>
    /// Check if file is Cue file
    /// </summary>
    /// <param name="fileName">file name to check</param>
    /// <returns>Returns true if file is Cue file</returns>
    public static Boolean isCueFile(string fileName)
    {
      if (!isWavCueFile(fileName) && fileName.ToLowerInvariant().EndsWith("." + CUE_FILE_EXT) && !isCueFakeTrackFile(fileName))
      {
        // Do the File Exists check only here, otherwise we will recheck the existence of all non-cue files as well.
        // This causes an unnecessary check of ALL files when scanning the shares
        if (System.IO.File.Exists(fileName))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Check if file is Cue fake track file
    /// </summary>
    /// <param name="fileName">file name to check</param>
    /// <returns>Returns true if file is Cue fake track file</returns>
    public static Boolean isCueFakeTrackFile(string fileName)
    {
      return fileName.ToLowerInvariant().Contains("." + CUE_FAKE_TRACK_FILE_EXT);
    }

    /// <summary>
    /// Builds Cue fake track file name
    /// </summary>
    /// <param name="cueFileName">Cue sheet file name</param>
    /// <param name="track">track number</param>
    /// <returns>Cue fake track file name</returns>
    public static string buildCueFakeTrackFileName(string cueFileName, Track track)
    {
      string res = cueFileName.Substring(0, cueFileName.Length - CUE_FILE_EXT.Length - 1) + "." +
                   CueUtil.CUE_FAKE_TRACK_FILE_EXT + "." + track.TrackNumber.ToString("00");

      if (System.IO.Path.HasExtension(track.DataFile.Filename))
      {
        res += System.IO.Path.GetExtension(track.DataFile.Filename).ToLowerInvariant();
      }

      return res;
    }

    /// <summary>
    /// Builds Cue fake track object
    /// </summary>
    /// <param name="cueFakeTrackFileName">Cue fake track file name</param>
    /// <returns>CueFakeTrack object</returns>
    public static CueFakeTrack parseCueFakeTrackFileName(string cueFakeTrackFileName)
    {
      CueFakeTrack res = new CueFakeTrack();
      int pos = cueFakeTrackFileName.IndexOf("." + CUE_FAKE_TRACK_FILE_EXT + ".");
      res.CueFileName = cueFakeTrackFileName.Substring(0, pos) + "." + CUE_FILE_EXT;
      string strTrackNumber = cueFakeTrackFileName.Substring(pos + CUE_FAKE_TRACK_FILE_EXT.Length + 2, 2);
      res.TrackNumber = int.Parse(strTrackNumber);

      return res;
    }

    /// <summary>
    /// Cue file filter for usage with not generic parameters
    /// See CUEFileListFilter for more infrmation
    /// </summary>
    /// <typeparam name="T">Parameter for builder (Type of IList entries)</typeparam>
    /// <param name="fileList">fileList to filter</param>
    /// <param name="builder">Builder for construct new list entries</param>
    /// <returns>filtered list</returns>
    public static IList<T> CUEFileListFilterList<T>(IList<T> fileList, ICueTrackFileBuilder<T> builder)
    {
      if (fileList == null || fileList.Count == 0)
      {
        return fileList;
      }

      // Apply CUE Filter
      fileList = CueUtil.CUEFileListFilter<T>(fileList, builder);
      return fileList;
    }

    /// <summary>
    /// Cue file filter. Parse Cue sheet file,
    /// add cue fake track files for each one cue track,
    /// removes original single file ape flac wav and etc, from list
    /// </summary>
    /// <typeparam name="T">Type of obgects in fileList</typeparam>
    /// <param name="fileList">File list to be filtered</param>
    /// <param name="adaptor"></param>
    /// <returns></returns>
    public static IList<T> CUEFileListFilter<T>(IList<T> fileList, ICueTrackFileBuilder<T> builder)
    {
      if (fileList == null || fileList.Count == 0)
      {
        return fileList;
      }

      List<string> exclusionList = new List<string>(2);
      List<T> resultList = new List<T>(fileList.Count);

      foreach (T fobj in fileList)
      {
        string fileName = builder.getFileName(fobj);
        if (CueUtil.isCueFile(fileName))
        {
          exclusionList.Add(fileName);

          CueSheet cueSheet = new CueSheet(fileName);
          string cuePath = System.IO.Path.GetDirectoryName(fileName);

          foreach (Track track in cueSheet.Tracks)
          {
            if (!exclusionList.Contains(cuePath + "\\" + track.DataFile.Filename))
            {
              exclusionList.Add(cuePath + "\\" + track.DataFile.Filename);
            }
            resultList.Add(builder.build(fileName, cueSheet, track));
          }
        }
        else
        {
          if (!isWavCueFile(fileName))
          {
            resultList.Add(fobj);
          }
        }
      }

      if (exclusionList.Count > 0)
      {
        List<T> tmpList = new List<T>(resultList.Count);

        foreach (T fobj in resultList)
        {
          if (!exclusionList.Contains(builder.getFileName(fobj)))
          {
            tmpList.Add(fobj);
          }
        }

        resultList = tmpList;
      }

      return resultList;
    }

    /// <summary>
    /// Read MusicTag information from cueFakeTrack
    /// Not thread safe!
    /// </summary>
    /// <param name="cueFakeTrackFileName">Cue fake track file name</param>
    /// <returns>MusicTag filled with cue track information</returns>
    public static MusicTag CueFakeTrackFile2MusicTag(string cueFakeTrackFileName)
    {
      lock (cacheLock)
      {
        // This metod called twice for each single file. So, cache data!
        if (cueFakeTrackFileName == cueFakeTrackFileNameCache)
        {
          return musicTagCache;
        }
        cueFakeTrackFileNameCache = cueFakeTrackFileName;

        // Cache CueSheet to pervent parsing it for each track in the album
        CueFakeTrack cueFakeTrack = parseCueFakeTrackFileName(cueFakeTrackFileName);
        if (cueSheetCacheFileNameCache != cueFakeTrack.CueFileName)
        {
          cueSheetCache = new CueSheet(cueFakeTrack.CueFileName);
          cueSheetCacheFileNameCache = cueFakeTrack.CueFileName;
        }

        int trackPosition = cueFakeTrack.TrackNumber - cueSheetCache.Tracks[0].TrackNumber;
        Track track = cueSheetCache.Tracks[trackPosition];

        musicTagCache = new MusicTag();
        if (track.TrackNumber < cueSheetCache.Tracks[cueSheetCache.Tracks.Length - 1].TrackNumber)
        {
          Track nextTrack = cueSheetCache.Tracks[trackPosition + 1];
          musicTagCache.Duration = cueIndexToIntTime(nextTrack.Indices[0]) - cueIndexToIntTime(track.Indices[0]);
        }

        string fname = Path.Combine(Path.GetDirectoryName(cueFakeTrack.CueFileName), track.DataFile.Filename);

        try
        {
          if (fname != cacheFName)
          {
            TagLib.File file = TagLib.File.Create(fname);
            tagCache = new TagCache();
            tagCache.CopyTags(file);
          }
          cacheFName = fname;

          musicTagCache.FileType = tagCache.FileType;
          musicTagCache.Codec = tagCache.Codec;
          musicTagCache.Year = tagCache.Year;
          musicTagCache.BitRate = tagCache.BitRate;
          musicTagCache.DiscID = tagCache.DiscId;
          musicTagCache.DiscTotal = tagCache.DiscTotal;
          musicTagCache.Channels = tagCache.Channels;
          musicTagCache.SampleRate = tagCache.SampleRate;
          musicTagCache.BitRateMode = tagCache.BitRateMode;

          if (musicTagCache.Duration == 0)
          {
            musicTagCache.Duration = tagCache.Duration - cueIndexToIntTime(track.Indices[0]);
          }
        }
        catch (Exception)
        {
          // If we end up here this means that we were not able to read the file
          // Most probably because of taglib-sharp not supporting the audio file
          // For example DTS file format has no Tags, but can be replayed by BASS
          // Use MediaInfo to read the properties
          if (fname != cacheFName)
          {
            tagCache = new TagCache();
            if (tagCache.CopyMediaInfo(fname))
            {
              musicTagCache.FileType = tagCache.FileType;
              musicTagCache.Codec = tagCache.Codec;
              musicTagCache.BitRate = tagCache.BitRate;
              musicTagCache.Channels = tagCache.Channels;
              musicTagCache.SampleRate = tagCache.SampleRate;
              musicTagCache.BitRateMode = tagCache.BitRateMode;

              if (musicTagCache.Duration == 0)
              {
                musicTagCache.Duration = tagCache.Duration - cueIndexToIntTime(track.Indices[0]);
              }
            }
          }
          cacheFName = fname;
        }

        // In case of having a multi file Cue sheet, we're not able to get the duration
        // from the index entries. use MediaInfo then
        if (musicTagCache.Duration == 0)
        {
          try
          {
            MediaInfo mi = new MediaInfo();
            mi.Open(fname);
            int durationms = 0;
            int.TryParse(mi.Get(StreamKind.General, 0, "Duration"), out durationms);
            musicTagCache.Duration = durationms / 1000;
            mi.Close();
          }
          catch (Exception ex1)
          {
            Log.Warn("CueFakeTrackFile2MusicTag: Exception retrieving duration for file {0}. {1}", fname, ex1.Message);
          }
        }

        if (string.IsNullOrEmpty(musicTagCache.Artist))
        {
          // if track has a performer set use this value for artist tag
          // else use global performer defined for cue sheet
          if (!string.IsNullOrEmpty(track.Performer))
          {
            musicTagCache.Artist = track.Performer;
          }
          else
          {
            musicTagCache.Artist = cueSheetCache.Performer;
          }
        }

        if (string.IsNullOrEmpty(musicTagCache.Album))
        {
          musicTagCache.Album = cueSheetCache.Title;
        }

        if (string.IsNullOrEmpty(musicTagCache.AlbumArtist))
        {
          if (!string.IsNullOrEmpty(cueSheetCache.Performer))
          {
            musicTagCache.AlbumArtist = cueSheetCache.Performer;
            musicTagCache.HasAlbumArtist = true;
          }
          else
          {
            musicTagCache.HasAlbumArtist = false;
          }
        }

        // let tagged genre override cuesheet genre
        if (string.IsNullOrEmpty(musicTagCache.Genre) &&
            !string.IsNullOrEmpty(cueSheetCache.Genre))
        {
          musicTagCache.Genre = cueSheetCache.Genre;
        }

        // let tagged year override cuesheet year
        if (musicTagCache.Year == 0 && cueSheetCache.Year != 0)
        {
          musicTagCache.Year = cueSheetCache.Year;
        }

        // let tagged composer override cuesheet songwriter
        if (string.IsNullOrEmpty(musicTagCache.Composer) &&
            !string.IsNullOrEmpty(cueSheetCache.Songwriter))
        {
          musicTagCache.Composer = cueSheetCache.Songwriter;
        }

        // in case we were not able to read the file type via taglib, we will get it vai extension
        if (string.IsNullOrEmpty(musicTagCache.FileType))
        {
          var extension = Path.GetExtension(fname);
          if (extension != null)
          {
            musicTagCache.FileType = extension.Substring(1).ToLowerInvariant();
          }
        }
        
        musicTagCache.FileName = cueFakeTrackFileName;
        musicTagCache.Title = track.Title;
        musicTagCache.Track = track.TrackNumber;
        musicTagCache.TrackTotal = cueSheetCache.Tracks.Length;

        return musicTagCache;
      }
    }

    /// <summary>
    /// Convert Cue indexes into seconds
    /// </summary>
    /// <param name="index">cue index time</param>
    /// <returns>time in seconds</returns>
    public static int cueIndexToIntTime(Index index)
    {
      return index.Minutes * 60 + index.Seconds;
    }

    /// <summary>
    /// Convert Cue indexes into seconds
    /// </summary>
    /// <param name="index">cue index time</param>
    /// <returns>time in seconds</returns>
    public static float cueIndexToFloatTime(Index index)
    {
      return index.Minutes * 60 + index.Seconds;
    }

    #endregion
  }
}