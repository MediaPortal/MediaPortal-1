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

using System.Collections.Generic;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MusicViews : BaseViewsNew
  {
    private string[] viewsAs = new string[]
                                 {
                                   "List",
                                   "Icons",
                                   "Big Icons",
                                   "Filmstrip",
                                   "Cover Flow",
                                   "Albums",
                                 };

    private Dictionary<string, Dictionary<string, string>> dbTables = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, string> tracksTable = new Dictionary<string, string>();
    private Dictionary<string, string> artistTable = new Dictionary<string, string>();
    private Dictionary<string, string> albumArtistTable = new Dictionary<string, string>();
    private Dictionary<string, string> genreTable = new Dictionary<string, string>();
    private Dictionary<string, string> composerTable = new Dictionary<string, string>();

    public MusicViews()
      : this("Music Views")
    {
      tracksTable.Add("Path", "strPath");
      tracksTable.Add("Artist", "strArtist");
      tracksTable.Add("AlbumArtist", "strAlbumArtist");
      tracksTable.Add("Album", "strAlbum");
      tracksTable.Add("Genre", "strGenre");
      tracksTable.Add("Composer", "strComposer");
      tracksTable.Add("Conductor", "strConductor");
      tracksTable.Add("Title", "strTitle");
      tracksTable.Add("TrackNr", "iTrack");
      tracksTable.Add("NumTracks", "iNumTracks");
      tracksTable.Add("Duration", "iDuration");
      tracksTable.Add("Year", "iYear");
      tracksTable.Add("TimesPlayed", "iTimesPlayed");
      tracksTable.Add("Rating", "iRating");
      tracksTable.Add("Favourite", "iFavorite");
      tracksTable.Add("ResumeAt", "iResumeAt");
      tracksTable.Add("DiscNr", "iDisc");
      tracksTable.Add("NumDiscs", "iNumDisc");
      tracksTable.Add("Lyrics", "strLyrics");
      tracksTable.Add("Comment", "strComment");
      tracksTable.Add("FileType", "strFileType");
      tracksTable.Add("FullCodec", "strFullCodec");
      tracksTable.Add("BitRateMode", "strBitRateMode");
      tracksTable.Add("BPM", "iBPM");
      tracksTable.Add("Bitrate", "iBitrate");
      tracksTable.Add("Channels", "iChannels");
      tracksTable.Add("SampleRate", "iSampleRate");
      tracksTable.Add("DateLastPlayed", "dateLastPlayed");
      tracksTable.Add("DateAdded", "dateAdded");
      dbTables.Add("Tracks", tracksTable);

      artistTable.Add("Artist", "strArtist");
      dbTables.Add("Artist", artistTable);

      albumArtistTable.Add("AlbumArtist", "strAlbumArtist");
      dbTables.Add("AlbumArtist", albumArtistTable);

      genreTable.Add("Genre", "strGenre");
      dbTables.Add("Genre", genreTable);

      composerTable.Add("Composer", "strComposer");
      dbTables.Add("Composer", composerTable);
    }

    public MusicViews(string name)
      : base(name) {}

    public override void LoadSettings()
    {
      base.LoadSettings("Music", dbTables, viewsAs);
    }

    public override void SaveSettings()
    {
      base.SaveSettings("Music");
    }

    public override void OnSectionActivated()
    {
      base.Section = "Music";
      base.OnSectionActivated();
    }
  }
}