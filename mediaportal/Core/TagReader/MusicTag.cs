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

namespace MediaPortal.TagReader
{
  public class MusicTag
  {
    #region ctor

    /// <summary>
    /// empty constructor
    /// </summary>
    public MusicTag()
    {
      #region Tags

      AlbumId = -1;
      Album = string.Empty;
      AlbumSort = string.Empty;
      HasAlbumArtist = false;
      AlbumArtist = string.Empty;
      AlbumArtistSort = string.Empty;
      Artist = string.Empty;
      ArtistSort = string.Empty;
      AmazonId = string.Empty;
      BPM = 0;
      Comment = string.Empty;
      Composer = string.Empty;
      ComposerSort = string.Empty;
      Conductor = string.Empty;
      Copyright = string.Empty;
      CoverArtImageBytes = null;
      DateTimePlayed = DateTime.MinValue;
      DateTimeModified = DateTime.MinValue;
      DiscTotal = 0;
      DiscID = 0;
      Genre = string.Empty;
      Grouping = string.Empty;
      Lyrics = string.Empty;
      MusicBrainzArtistId = string.Empty;
      MusicBrainzDiscId = string.Empty;
      MusicBrainzReleaseArtistId = string.Empty;
      MusicBrainzReleaseCountry = string.Empty;
      MusicBrainzReleaseId = string.Empty;
      MusicBrainzReleaseStatus = string.Empty;
      MusicBrainzReleaseTrackId = string.Empty;
      MusicBrainzReleaseType = string.Empty;
      MusicIpid = string.Empty;
      Rating = 0;
      ReplayGainAlbumPeak = string.Empty;
      ReplayGainAlbum = string.Empty;
      ReplayGainTrackPeak = string.Empty;
      ReplayGainTrack = string.Empty;
      Title = string.Empty;
      TitleSort = string.Empty;
      TimesPlayed = 0;
      TrackTotal = 0;
      Track = 0;
      Year = 0;
      #endregion

      #region File Properties

      BitRate = 0;
      BitRateMode = string.Empty;
      Channels = 0;
      Codec = string.Empty;
      Duration = 0;
      FileName = string.Empty;
      FileType = string.Empty;
      SampleRate = 0;
      
      #endregion
    }

    /// <summary>
    /// copy constructor
    /// </summary>
    /// <param name="tag"></param>
    public MusicTag(MusicTag tag)
    {
      #region Init Tags

      AlbumId = -1;
      Album = string.Empty;
      AlbumSort = string.Empty;
      HasAlbumArtist = false;
      AlbumArtist = string.Empty;
      AlbumArtistSort = string.Empty;
      Artist = string.Empty;
      ArtistSort = string.Empty;
      AmazonId = string.Empty;
      BPM = 0;
      Comment = string.Empty;
      Composer = string.Empty;
      ComposerSort = string.Empty;
      Conductor = string.Empty;
      Copyright = string.Empty;
      CoverArtImageBytes = null;
      DateTimePlayed = DateTime.MinValue;
      DateTimeModified = DateTime.MinValue;
      DiscTotal = 0;
      DiscID = 0;
      Genre = string.Empty;
      Grouping = string.Empty;
      Lyrics = string.Empty;
      MusicBrainzArtistId = string.Empty;
      MusicBrainzDiscId = string.Empty;
      MusicBrainzReleaseArtistId = string.Empty;
      MusicBrainzReleaseCountry = string.Empty;
      MusicBrainzReleaseId = string.Empty;
      MusicBrainzReleaseStatus = string.Empty;
      MusicBrainzReleaseTrackId = string.Empty;
      MusicBrainzReleaseType = string.Empty;
      MusicIpid = string.Empty;
      Rating = 0;
      ReplayGainAlbumPeak = string.Empty;
      ReplayGainAlbum = string.Empty;
      ReplayGainTrackPeak = string.Empty;
      ReplayGainTrack = string.Empty;
      Title = string.Empty;
      TitleSort = string.Empty;
      TimesPlayed = 0;
      TrackTotal = 0;
      Track = 0;
      Year = 0;

      #endregion

      #region Init File Properties

      BitRate = 0;
      BitRateMode = string.Empty;
      Channels = 0;
      Codec = string.Empty;
      Duration = 0;
      FileName = string.Empty;
      FileType = string.Empty;
      SampleRate = 0;

      #endregion

      if (tag == null) return;

      #region Set Tags

      Album = tag.Album;
      AlbumSort = tag.AlbumSort;
      HasAlbumArtist = tag.HasAlbumArtist;
      AlbumArtist = tag.AlbumArtist;
      AlbumArtistSort = tag.AlbumArtistSort;
      Artist = tag.Artist;
      ArtistSort = tag.ArtistSort;
      AmazonId = tag.AmazonId;
      BPM = tag.BPM;
      Comment = tag.Comment;
      Composer = tag.Composer;
      ComposerSort = tag.ComposerSort;
      Conductor = tag.Conductor;
      Copyright = tag.Copyright;
      CoverArtImageBytes = tag.CoverArtImageBytes;
      DateTimePlayed = tag.DateTimePlayed;
      DateTimeModified = tag.DateTimeModified;
      DiscTotal = tag.DiscTotal;
      DiscID = tag.DiscID;
      Genre = tag.Genre;
      Grouping = tag.Grouping;
      Lyrics = tag.Lyrics;
      MusicBrainzArtistId = tag.MusicBrainzArtistId;
      MusicBrainzDiscId = tag.MusicBrainzDiscId;
      MusicBrainzReleaseArtistId = tag.MusicBrainzReleaseArtistId;
      MusicBrainzReleaseCountry = tag.MusicBrainzReleaseCountry;
      MusicBrainzReleaseId = tag.MusicBrainzReleaseId;
      MusicBrainzReleaseStatus = tag.MusicBrainzReleaseStatus;
      MusicBrainzReleaseTrackId = tag.MusicBrainzReleaseTrackId;
      MusicBrainzReleaseType = tag.MusicBrainzReleaseType;
      MusicIpid = tag.MusicIpid;
      Rating = tag.Rating;
      ReplayGainAlbumPeak = tag.ReplayGainAlbumPeak;
      ReplayGainAlbum = tag.ReplayGainAlbum;
      ReplayGainTrackPeak = tag.ReplayGainTrackPeak;
      ReplayGainTrack = tag.ReplayGainTrack;
      Title = tag.Title;
      TitleSort = tag.TitleSort;
      TimesPlayed = tag.TimesPlayed;
      TrackTotal = tag.TrackTotal;
      Track = tag.Track;
      Year = tag.Year;

      #endregion

      #region Set File Properties

      BitRate = tag.BitRate;
      BitRateMode = tag.BitRateMode;
      Channels = tag.Channels;
      Codec = string.Empty;
      Duration = tag.Duration;
      FileName = tag.FileName;
      FileType = tag.FileType;
      SampleRate = tag.SampleRate;

      #endregion
    }

    #endregion

    #region Methods

    /// <summary>
    /// Method to clear the current item
    /// </summary>
    public void Clear()
    {
      #region Tags

      AlbumId = -1;
      Album = string.Empty;
      AlbumSort = string.Empty;
      HasAlbumArtist = false;
      AlbumArtist = string.Empty;
      AlbumArtistSort = string.Empty;
      Artist = string.Empty;
      ArtistSort = string.Empty;
      AmazonId = string.Empty;
      BPM = 0;
      Comment = string.Empty;
      Composer = string.Empty;
      ComposerSort = string.Empty;
      Conductor = string.Empty;
      Copyright = string.Empty;
      CoverArtImageBytes = null;
      DateTimePlayed = DateTime.MinValue;
      DateTimeModified = DateTime.MinValue;
      DiscTotal = 0;
      DiscID = 0;
      Genre = string.Empty;
      Grouping = string.Empty;
      Lyrics = string.Empty;
      MusicBrainzArtistId = string.Empty;
      MusicBrainzDiscId = string.Empty;
      MusicBrainzReleaseArtistId = string.Empty;
      MusicBrainzReleaseCountry = string.Empty;
      MusicBrainzReleaseId = string.Empty;
      MusicBrainzReleaseStatus = string.Empty;
      MusicBrainzReleaseTrackId = string.Empty;
      MusicBrainzReleaseType = string.Empty;
      MusicIpid = string.Empty;
      Rating = 0;
      ReplayGainAlbumPeak = string.Empty;
      ReplayGainAlbum = string.Empty;
      ReplayGainTrackPeak = string.Empty;
      ReplayGainTrack = string.Empty;
      Title = string.Empty;
      TitleSort = string.Empty;
      TimesPlayed = 0;
      TrackTotal = 0;
      Track = 0;
      Year = 0;

      #endregion

      #region File Properties

      BitRate = 0;
      BitRateMode = string.Empty;
      Channels = 0;
      Codec = string.Empty;
      Duration = 0;
      FileName = string.Empty;
      FileType = string.Empty;
      SampleRate = 0;

      #endregion
    }

    public bool IsMissingData
    {
      get
      {
        return Artist.Length == 0
               || Album.Length == 0
               || Title.Length == 0
               || Artist.Length == 0
               || Genre.Length == 0
               || Track == 0
               || Duration == 0;
      }
    }

    #endregion

    #region Properties

    #region Tags

    /// <summary>
    /// Property to get/set the Album id 
    /// </summary>
    public int AlbumId { get; set; }

    /// <summary>
    /// Property to get/set the Album name 
    /// </summary>
    public string Album { get; set; }

    /// <summary>
    /// Property to get/set the Album nSort ame 
    /// </summary>
    public string AlbumSort { get; set; }

    /// <summary>
    /// Property to indicate if an AlbumArtist is present
    /// </summary>
    public bool HasAlbumArtist { get; set; }

    /// <summary>
    /// Property to get/set the AlbumArtist
    /// </summary>
    public string AlbumArtist { get; set; }

    /// <summary>
    /// Property to get/set the AlbumArtistSort
    /// </summary>
    public string AlbumArtistSort { get; set; }

    /// <summary>
    /// Property to get/set the Artist 
    /// </summary>
    public string Artist { get; set; }

    /// <summary>
    /// Property to get/set the ArtistSort 
    /// </summary>
    public string ArtistSort { get; set; }

    /// <summary>
    /// Property to get/set the AmazonId 
    /// </summary>
    public string AmazonId { get; set; }

    /// <summary>
    /// Property to get/set the Beats per Minute
    /// </summary>
    public int BPM { get; set; }

    /// <summary>
    /// Property to get/set the comment field
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Property to get/set the Composers 
    /// </summary>
    public string Composer { get; set; }

    /// <summary>
    /// Property to get/set the Composer Sort 
    /// </summary>
    public string ComposerSort { get; set; }

    /// <summary>
    /// Property to get/set the Conductor
    /// </summary>
    public string Conductor { get; set; }

    /// <summary>
    /// Property to get/set the Copyright
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    /// Property to get/set the Cover Art Image
    /// </summary>
    public byte[] CoverArtImageBytes { get; set; }

    /// <summary>
    /// Returns the Cover Art
    /// </summary>
    public string CoverArtFile
    {
      get { return Utils.GetImageFile(CoverArtImageBytes, String.Empty); }
    }

    /// <summary>
    /// Last time the song was played
    /// </summary>
    public DateTime DateTimePlayed { get; set; }

    /// <summary>
    /// Last Time the song was modified
    /// </summary>
    public DateTime DateTimeModified { get; set; }

    /// <summary>
    /// Property to get/set the Disc Id 
    /// </summary>
    public int DiscID { get; set; }

    /// <summary>
    /// Property to get/set the Total Disc number
    /// </summary>
    public int DiscTotal { get; set; }

    /// <summary>
    /// Property to get/set the Genre 
    /// </summary>
    public string Genre { get; set; }

    /// <summary>
    /// Property to get/set the Grouping 
    /// </summary>
    public string Grouping { get; set; }

    /// <summary>
    /// Property to get/set the URL of the Image
    /// </summary>
    public string ImageURL { get; set; }

    /// <summary>
    /// Property to get/set the Lyrics
    /// </summary>
    public string Lyrics { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzArtistId
    /// </summary>
    public string MusicBrainzArtistId { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzDisctId
    /// </summary>
    public string MusicBrainzDiscId { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzReleaseArtistId
    /// </summary>
    public string MusicBrainzReleaseArtistId { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzReleaseCountry
    /// </summary>
    public string MusicBrainzReleaseCountry { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzReleaseId
    /// </summary>
    public string MusicBrainzReleaseId { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzReleaseStatus
    /// </summary>
    public string MusicBrainzReleaseStatus { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzReleaseType
    /// </summary>
    public string MusicBrainzReleaseType { get; set; }

    /// <summary>
    /// Property to get/set the MusicBrainzReleaseTrackId
    /// </summary>
    public string MusicBrainzReleaseTrackId { get; set; }

    /// <summary>
    /// Property to get/set the MusicIpid
    /// </summary>
    public string MusicIpid { get; set; }

    /// <summary>
    /// Property to get/set the Rating
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Property to get/set the ReplayGain of the Track
    /// </summary>
    public string ReplayGainTrack { get; set; }

    /// <summary>
    /// Property to get/set the Peak of the Track
    /// </summary>
    public string ReplayGainTrackPeak { get; set; }

    /// <summary>
    /// Property to get/set the ReplayGain of the Album
    /// </summary>
    public string ReplayGainAlbum { get; set; }

    /// <summary>
    /// Property to get/set the Peak of the Album
    /// </summary>
    public string ReplayGainAlbumPeak { get; set; }

    /// <summary>
    /// Property to get/set the Title 
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Property to get/set the TitleSort 
    /// </summary>
    public string TitleSort { get; set; }

    /// <summary>
    /// Property to get/set the number of times this file has been played
    /// </summary>
    public int TimesPlayed { get; set; }

    /// <summary>
    /// Property to get/set the Track number
    /// </summary>
    public int Track { get; set; }

    /// <summary>
    /// Property to get/set the Total Track number
    /// </summary>
    public int TrackTotal { get; set; }
    
    /// <summary>
    /// Property to get/set the Year
    /// </summary>
    public int Year { get; set; }

    #endregion

    #region File Properties

    /// <summary>
    /// Property to get/set the Bitrate 
    /// </summary>
    public int BitRate { get; set; }

    /// <summary>
    /// Property to get/set the Bitrate Mode (CBR/VBR)
    /// </summary>
    public string BitRateMode { get; set; }

    /// <summary>
    /// Property to get/set the Channels
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Property to get/set the Codec
    /// </summary>
    public string Codec { get; set; }

    /// <summary>
    /// Property to get/set the duration in seconds
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Property to get/set the FileName
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// Property to get/set the FileType
    /// </summary>
    public string FileType { get; set; }

    /// <summary>
    /// Property to get/set the SampleRate
    /// </summary>
    public int SampleRate { get; set; }

    #endregion

    #endregion
  }
}