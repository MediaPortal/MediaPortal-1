#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implments a property manager for the GUI lib. Keeps track of the properties 
  /// of the currently playing item.
  /// like playtime, current position, artist,title of the song/video,...
  /// </summary>
  public class GUIPropertyManager
  {
    // pattern that matches a property tag, e.g. '#myproperty' or '#some.property_string'
    private static Regex propertyExpr = new Regex(@"#[a-z0-9\._]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static Dictionary<string, string> _properties = new Dictionary<string, string>();
    private static bool _isChanged = false;

    public delegate void OnPropertyChangedHandler(string tag, string tagValue);

    public static event OnPropertyChangedHandler OnPropertyChanged;

    /// <summary>
    /// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
    /// </summary>
    private GUIPropertyManager() {}

    static GUIPropertyManager()
    {
      _properties["#highlightedbutton"] = string.Empty;
      _properties["#itemcount"] = string.Empty;
      _properties["#selectedindex"] = string.Empty;
      _properties["#selecteditem"] = string.Empty;
      _properties["#selecteditem2"] = string.Empty;
      _properties["#selectedthumb"] = string.Empty;
      _properties["#homedate"] = string.Empty;
      _properties["#title"] = string.Empty; // song title, imdb movie title, recording title
      _properties["#artist"] = string.Empty; // song artist
      _properties["#album"] = string.Empty; // song album
      _properties["#track"] = string.Empty; // song track number
      _properties["#year"] = string.Empty; // song year , imdb movie year
      _properties["#comment"] = string.Empty; // song comment
      _properties["#director"] = string.Empty; // imdb movie director
      _properties["#genre"] = string.Empty; // imdb movie genres
      _properties["#cast"] = string.Empty; // imdb movie cast 
      _properties["#dvdlabel"] = string.Empty; // imdb movie dvd label
      _properties["#imdbnumber"] = string.Empty; // imdb movie number
      _properties["#file"] = string.Empty; // imdb movie filename
      _properties["#plot"] = string.Empty; // imdb movie plot 
      _properties["#plotoutline"] = string.Empty; // imdb movie plot outline
      _properties["#rating"] = string.Empty; // imdb movie rating (0-10)
      _properties["#tagline"] = string.Empty; // imdb movie tag line
      _properties["#votes"] = string.Empty; // imdb movie votes
      _properties["#credits"] = string.Empty; // imdb movie writing credits
      _properties["#mpaarating"] = string.Empty; // imdb movie MPAA rating
      _properties["#runtime"] = string.Empty; // imdb movie runtime 
      _properties["#iswatched"] = string.Empty; // boolean indication movie has been watched
      _properties["#thumb"] = string.Empty;
      _properties["#currentplaytime"] = string.Empty;
      _properties["#currentremaining"] = string.Empty;
      _properties["#shortcurrentremaining"] = string.Empty;
      _properties["#shortcurrentplaytime"] = string.Empty;
      _properties["#duration"] = string.Empty;
      _properties["#shortduration"] = string.Empty;
      _properties["#playlogo"] = string.Empty;
      _properties["#playspeed"] = string.Empty;
      _properties["#percentage"] = string.Empty;
      _properties["#currentmodule"] = string.Empty;
      _properties["#currentmoduleid"] = string.Empty;
      _properties["#currentmodulefullscreenstate"] = string.Empty;
      _properties["#channel"] = string.Empty;
      _properties["#TV.start"] = string.Empty;
      _properties["#TV.stop"] = string.Empty;
      _properties["#TV.current"] = string.Empty;
      _properties["#TV.Record.channel"] = string.Empty;
      _properties["#TV.Record.start"] = string.Empty;
      _properties["#TV.Record.stop"] = string.Empty;
      _properties["#TV.Record.genre"] = string.Empty;
      _properties["#TV.Record.title"] = string.Empty;
      _properties["#TV.Record.description"] = string.Empty;
      _properties["#TV.Record.thumb"] = string.Empty;
      _properties["#TV.View.channel"] = string.Empty;
      _properties["#TV.View.thumb"] = string.Empty;
      _properties["#TV.View.start"] = string.Empty;
      _properties["#TV.View.stop"] = string.Empty;
      _properties["#TV.View.remaining"] = string.Empty;
      _properties["#TV.View.genre"] = string.Empty;
      _properties["#TV.View.title"] = string.Empty;
      _properties["#TV.View.compositetitle"] = string.Empty;
      _properties["#TV.View.description"] = string.Empty;
      _properties["#TV.View.Percentage"] = string.Empty;

      _properties["#TV.Next.start"] = string.Empty;
      _properties["#TV.Next.stop"] = string.Empty;
      _properties["#TV.Next.genre"] = string.Empty;
      _properties["#TV.Next.title"] = string.Empty;
      _properties["#TV.Next.compositetitle"] = string.Empty;
      _properties["#TV.Next.description"] = string.Empty;

      _properties["#TV.Guide.Day"] = string.Empty;
      _properties["#TV.Guide.thumb"] = string.Empty;
      _properties["#TV.Guide.Title"] = string.Empty;
      _properties["#TV.Guide.SubTitle"] = string.Empty;
      _properties["#TV.Guide.CompositeTitle"] = string.Empty;
      _properties["#TV.Guide.Time"] = string.Empty;
      _properties["#TV.Guide.Duration"] = string.Empty;
      _properties["#TV.Guide.TimeFromNow"] = string.Empty;
      _properties["#TV.Guide.Description"] = string.Empty;
      _properties["#TV.Guide.Genre"] = string.Empty;
      _properties["#TV.Guide.EpisodeName"] = string.Empty;
      _properties["#TV.Guide.SeriesNumber"] = string.Empty;
      _properties["#TV.Guide.EpisodeNumber"] = string.Empty;
      _properties["#TV.Guide.EpisodePart"] = string.Empty;
      _properties["#TV.Guide.EpisodeDetail"] = string.Empty;
      _properties["#TV.Guide.Date"] = string.Empty;
      _properties["#TV.Guide.StarRating"] = string.Empty;
      _properties["#TV.Guide.Classification"] = string.Empty;
      _properties["#TV.Guide.Group"] = string.Empty;

      _properties["#Radio.Guide.Day"] = string.Empty;
      _properties["#Radio.Guide.thumb"] = string.Empty;
      _properties["#Radio.Guide.Title"] = string.Empty;
      _properties["#Radio.Guide.Time"] = string.Empty;
      _properties["#Radio.Guide.Duration"] = string.Empty;
      _properties["#Radio.Guide.TimeFromNow"] = string.Empty;
      _properties["#Radio.Guide.Description"] = string.Empty;
      _properties["#Radio.Guide.Genre"] = string.Empty;
      _properties["#Radio.Guide.EpisodeName"] = string.Empty;
      _properties["#Radio.Guide.SeriesNumber"] = string.Empty;
      _properties["#Radio.Guide.EpisodeNumber"] = string.Empty;
      _properties["#Radio.Guide.EpisodePart"] = string.Empty;
      _properties["#Radio.Guide.EpisodeDetail"] = string.Empty;
      _properties["#Radio.Guide.Date"] = string.Empty;
      _properties["#Radio.Guide.StarRating"] = string.Empty;
      _properties["#Radio.Guide.Classification"] = string.Empty;

      _properties["#TV.RecordedTV.Title"] = string.Empty;
      _properties["#TV.RecordedTV.Time"] = string.Empty;
      _properties["#TV.RecordedTV.Description"] = string.Empty;
      _properties["#TV.RecordedTV.thumb"] = string.Empty;
      _properties["#TV.RecordedTV.Genre"] = string.Empty;
      _properties["#TV.Signal.Quality"] = string.Empty;

      _properties["#TV.Scheduled.Title"] = string.Empty;
      _properties["#TV.Scheduled.Time"] = string.Empty;
      _properties["#TV.Scheduled.Description"] = string.Empty;
      _properties["#TV.Scheduled.thumb"] = string.Empty;
      _properties["#TV.Scheduled.Genre"] = string.Empty;
      _properties["#TV.Scheduled.Channel"] = string.Empty;

      _properties["#TV.Search.Title"] = string.Empty;
      _properties["#TV.Search.Time"] = string.Empty;
      _properties["#TV.Search.Description"] = string.Empty;
      _properties["#TV.Search.thumb"] = string.Empty;
      _properties["#TV.Search.Genre"] = string.Empty;

      _properties["#view"] = string.Empty;

      _properties["#TV.Transcoding.Percentage"] = string.Empty;
      _properties["#TV.Transcoding.File"] = string.Empty;
      _properties["#TV.Transcoding.Title"] = string.Empty;
      _properties["#TV.Transcoding.Genre"] = string.Empty;
      _properties["#TV.Transcoding.Description"] = string.Empty;
      _properties["#TV.Transcoding.Channel"] = string.Empty;


      _properties["#Play.Current.Thumb"] = string.Empty;
      _properties["#Play.Current.File"] = string.Empty;
      _properties["#Play.Current.Title"] = string.Empty;
      _properties["#Play.Current.Genre"] = string.Empty;
      _properties["#Play.Current.Comment"] = string.Empty;
      _properties["#Play.Current.Artist"] = string.Empty;
      _properties["#Play.Current.Director"] = string.Empty;
      _properties["#Play.Current.Album"] = string.Empty;
      _properties["#Play.Current.Track"] = string.Empty;
      _properties["#Play.Current.Year"] = string.Empty;
      _properties["#Play.Current.Duration"] = string.Empty;
      _properties["#Play.Current.Plot"] = string.Empty;
      _properties["#Play.Current.PlotOutline"] = string.Empty;
      _properties["#Play.Current.Channel"] = string.Empty;
      _properties["#Play.Current.Cast"] = string.Empty;
      _properties["#Play.Current.DVDLabel"] = string.Empty;
      _properties["#Play.Current.IMDBNumber"] = string.Empty;
      _properties["#Play.Current.Rating"] = string.Empty;
      _properties["#Play.Current.TagLine"] = string.Empty;
      _properties["#Play.Current.Votes"] = string.Empty;
      _properties["#Play.Current.Credits"] = string.Empty;
      _properties["#Play.Current.Runtime"] = string.Empty;
      _properties["#Play.Current.MPAARating"] = string.Empty;
      _properties["#Play.Current.IsWatched"] = string.Empty;

      _properties["#Play.Current.ArtistThumb"] = string.Empty;
      _properties["#Play.Current.Lastfm.TrackTags"] = string.Empty;
      _properties["#Play.Current.Lastfm.SimilarArtists"] = string.Empty;
      _properties["#Play.Current.Lastfm.ArtistInfo"] = string.Empty;
      _properties["#Play.Current.Lastfm.CurrentStream"] = string.Empty;

      _properties["#Play.Next.Thumb"] = string.Empty;
      _properties["#Play.Next.File"] = string.Empty;
      _properties["#Play.Next.Title"] = string.Empty;
      _properties["#Play.Next.Genre"] = string.Empty;
      _properties["#Play.Next.Comment"] = string.Empty;
      _properties["#Play.Next.Artist"] = string.Empty;
      _properties["#Play.Next.Director"] = string.Empty;
      _properties["#Play.Next.Album"] = string.Empty;
      _properties["#Play.Next.Track"] = string.Empty;
      _properties["#Play.Next.Year"] = string.Empty;
      _properties["#Play.Next.Duration"] = string.Empty;
      _properties["#Play.Next.Plot"] = string.Empty;
      _properties["#Play.Next.PlotOutline"] = string.Empty;
      _properties["#Play.Next.Channel"] = string.Empty;
      _properties["#Play.Next.Cast"] = string.Empty;
      _properties["#Play.Next.DVDLabel"] = string.Empty;
      _properties["#Play.Next.IMDBNumber"] = string.Empty;
      _properties["#Play.Next.Rating"] = string.Empty;
      _properties["#Play.Next.TagLine"] = string.Empty;
      _properties["#Play.Next.Votes"] = string.Empty;
      _properties["#Play.Next.Credits"] = string.Empty;
      _properties["#Play.Next.Runtime"] = string.Empty;
      _properties["#Play.Next.MPAARating"] = string.Empty;
      _properties["#Play.Next.IsWatched"] = string.Empty;

      _properties["#Lastfm.Rating.AlbumTrack1"] = string.Empty;
      _properties["#Lastfm.Rating.AlbumTrack2"] = string.Empty;
      _properties["#Lastfm.Rating.AlbumTrack3"] = string.Empty;

      _properties["#numberplace.time"] = string.Empty;
      _properties["#numberplace.name1"] = string.Empty;
      _properties["#numberplace.name2"] = string.Empty;
      _properties["#numberplace.name3"] = string.Empty;
      _properties["#numberplace.name4"] = string.Empty;
      _properties["#numberplace.name5"] = string.Empty;
      _properties["#numberplace.score1"] = string.Empty;
      _properties["#numberplace.score2"] = string.Empty;
      _properties["#numberplace.score3"] = string.Empty;
      _properties["#numberplace.score4"] = string.Empty;
      _properties["#numberplace.score5"] = string.Empty;

      _properties["#facadeview.viewmode"] = string.Empty;

      _properties["#VUMeterL"] = string.Empty; // The name of the VUMeterfile Left
      _properties["#VUMeterR"] = string.Empty; // The name of the VUMeterfile Right
    }

    /// <summary>
    /// Get/set if the properties have changed.
    /// </summary>
    public static bool Changed
    {
      get { return _isChanged; }
      set { _isChanged = value; }
    }

    /// <summary>
    /// This method will set the value for a given property
    /// </summary>
    /// <param name="tag">property name</param>
    /// <param name="tagvalue">property value</param>
    public static void SetProperty(string tag, string tagvalue)
    {
      if (String.IsNullOrEmpty(tag) || tag[0] != '#')
      {
        return;
      }

      if (tagvalue == null)
      {
        tagvalue = string.Empty;
      }

      if (tag.Equals("#currentmodule"))
      {
        try
        {
          GUIGraphicsContext.form.Text = "MediaPortal - " + tagvalue;
        }
        catch (Exception) {}
      }

      bool changed = false;
      lock (_properties)
      {
        changed = (!_properties.ContainsKey(tag) || _properties[tag] != tagvalue);
        if (changed)
        {
          _properties[tag] = tagvalue;
        }
      }

      if (changed && OnPropertyChanged != null)
      {
        _isChanged = true;
        OnPropertyChanged(tag, tagvalue);
      }
    }

    /// <summary>
    /// This method returns the value for a given property
    /// </summary>
    /// <param name="tag">property name</param>
    /// <returns>property value</returns>
    public static string GetProperty(string tag)
    {
      string property = string.Empty;
      if (tag != null && tag.IndexOf('#') > -1)
      {
        lock (_properties)
        {          
          _properties.TryGetValue(tag, out property);                    
        }
      }

      return property;
    }

    /// <summary>
    /// Removes the player properties from the hashtable.
    /// </summary>
    public static void RemovePlayerProperties()
    {
      SetProperty("#Play.Current.Thumb", string.Empty);
      SetProperty("#Play.Current.File", string.Empty);
      SetProperty("#Play.Current.Title", string.Empty);
      SetProperty("#Play.Current.Genre", string.Empty);
      SetProperty("#Play.Current.Comment", string.Empty);
      SetProperty("#Play.Current.Artist", string.Empty);
      SetProperty("#Play.Current.Director", string.Empty);
      SetProperty("#Play.Current.Album", string.Empty);
      SetProperty("#Play.Current.Track", string.Empty);
      SetProperty("#Play.Current.Year", string.Empty);
      SetProperty("#Play.Current.Duration", string.Empty);
      SetProperty("#Play.Current.Plot", string.Empty);
      SetProperty("#Play.Current.PlotOutline", string.Empty);
      SetProperty("#Play.Current.Channel", string.Empty);
      SetProperty("#Play.Current.Cast", string.Empty);
      SetProperty("#Play.Current.DVDLabel", string.Empty);
      SetProperty("#Play.Current.IMDBNumber", string.Empty);
      SetProperty("#Play.Current.Rating", string.Empty);
      SetProperty("#Play.Current.TagLine", string.Empty);
      SetProperty("#Play.Current.Votes", string.Empty);
      SetProperty("#Play.Current.Credits", string.Empty);
      SetProperty("#Play.Current.Runtime", string.Empty);
      SetProperty("#Play.Current.MPAARating", string.Empty);
      SetProperty("#Play.Current.IsWatched", string.Empty);


      SetProperty("#Play.Next.Thumb", string.Empty);
      SetProperty("#Play.Next.File", string.Empty);
      SetProperty("#Play.Next.Title", string.Empty);
      SetProperty("#Play.Next.Genre", string.Empty);
      SetProperty("#Play.Next.Comment", string.Empty);
      SetProperty("#Play.Next.Artist", string.Empty);
      SetProperty("#Play.Next.Director", string.Empty);
      SetProperty("#Play.Next.Album", string.Empty);
      SetProperty("#Play.Next.Track", string.Empty);
      SetProperty("#Play.Next.Year", string.Empty);
      SetProperty("#Play.Next.Duration", string.Empty);
      SetProperty("#Play.Next.Plot", string.Empty);
      SetProperty("#Play.Next.PlotOutline", string.Empty);
      SetProperty("#Play.Next.Channel", string.Empty);
      SetProperty("#Play.Next.Cast", string.Empty);
      SetProperty("#Play.Next.DVDLabel", string.Empty);
      SetProperty("#Play.Next.IMDBNumber", string.Empty);
      SetProperty("#Play.Next.Rating", string.Empty);
      SetProperty("#Play.Next.TagLine", string.Empty);
      SetProperty("#Play.Next.Votes", string.Empty);
      SetProperty("#Play.Next.Credits", string.Empty);
      SetProperty("#Play.Next.Runtime", string.Empty);
      SetProperty("#Play.Next.MPAARating", string.Empty);
      SetProperty("#Play.Next.IsWatched", string.Empty);

      _isChanged = true;
    }

    /// <summary>
    /// Parses a property request.
    /// </summary>
    /// <param name="line ">The identification of the propertie (e.g.,#title).</param>
    /// <returns>The value of the property.</returns>
    public static string Parse(string line)
    {
      if (line == null)
      {
        return string.Empty;
      }

      if (line.IndexOf('#') > -1)
      {
        // Matches a property tag and replaces it with the value for that property
        MatchCollection matches = propertyExpr.Matches(line);
        foreach (Match match in matches)
        {
          line = line.Replace(match.Value, ParseProperty(match.Value));
        }
      }

      return line;
    }

    // Because currently the properties don't have delimiters there are some situations 
    // where we cannot properly get the complete property tag. In order to be 
    // backwards compatible with current skins/plugins this logic is to ensure 
    // everything is parsed similar to the pre-patch situation, it actually _can_ make 
    // the parsing a bit slower depending on how the properties are used, but it's still 
    // significantly faster than before.
    private static string ParseProperty(string property)
    {
      for (int i = property.Length; i > 1; i--)
      {
        // if the property is not an existing property we iterate through every
        // substring of the value untill we find the property, we replace the substring with
        // the value of the property and in turn leave the overhead string intact.
        string tag = property.Substring(0, i);
        lock (_properties)
        {
          string propertyValue = string.Empty;
          if (_properties.TryGetValue(tag, out propertyValue))
          {
            return property.Replace(tag, propertyValue);
          }          
        }
      }

      return property;
    }
  }
}