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
using System.Collections;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implments a property manager for the GUI lib. Keeps track of the properties 
  /// of the currently playing item.
  /// like playtime, current position, artist,title of the song/video,...
  /// </summary>
  public class GUIPropertyManager
  {
    static Hashtable _properties = new Hashtable();
    static bool _isChanged = false;
    public delegate void OnPropertyChangedHandler(string tag, string tagValue);
    static public event OnPropertyChangedHandler OnPropertyChanged;

    /// <summary>
    /// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
    /// </summary>
    private GUIPropertyManager()
    {
    }
    static GUIPropertyManager() 
    {
      _properties["#highlightedbutton"] = String.Empty;
      _properties["#itemcount"] = String.Empty;
      _properties["#selecteditem"] = String.Empty;
      _properties["#selecteditem2"] = String.Empty;
      _properties["#selectedthumb"] = String.Empty;
      _properties["#title"] = String.Empty;			// song title, imdb movie title, recording title
      _properties["#artist"] = String.Empty;			// song artist
      _properties["#album"] = String.Empty;			// song album
      _properties["#track"] = String.Empty;			// song track number
      _properties["#year"] = String.Empty;				// song year , imdb movie year
      _properties["#comment"] = String.Empty;		// song comment
      _properties["#director"] = String.Empty;		// imdb movie director
      _properties["#genre"] = String.Empty;			// imdb movie genres
      _properties["#cast"] = String.Empty;				// imdb movie cast 
      _properties["#dvdlabel"] = String.Empty;		// imdb movie dvd label
      _properties["#imdbnumber"] = String.Empty; // imdb movie number
      _properties["#file"] = String.Empty;				// imdb movie filename
      _properties["#plot"] = String.Empty;				// imdb movie plot 
      _properties["#plotoutline"] = String.Empty;// imdb movie plot outline
      _properties["#rating"] = String.Empty;		  // imdb movie rating (0-10)
      _properties["#tagline"] = String.Empty;    // imdb movie tag line
      _properties["#votes"] = String.Empty;		  // imdb movie votes
      _properties["#credits"] = String.Empty;    // imdb movie writing credits
      _properties["#mpaarating"] = String.Empty; // imdb movie MPAA rating
      _properties["#runtime"] = String.Empty;    // imdb movie runtime 
      _properties["#iswatched"] = String.Empty;  // boolean indication movie has been watched
      _properties["#thumb"] = String.Empty;
      _properties["#currentplaytime"] = String.Empty;
      _properties["#currentremaining"] = String.Empty;
      _properties["#shortcurrentremaining"] = String.Empty;
      _properties["#shortcurrentplaytime"] = String.Empty;
      _properties["#duration"] = String.Empty;
      _properties["#shortduration"] = String.Empty;
      _properties["#playlogo"] = String.Empty;
      _properties["#playspeed"] = String.Empty;
      _properties["#percentage"] = String.Empty;
      _properties["#currentmodule"] = String.Empty;
      _properties["#channel"] = String.Empty;
      _properties["#TV.start"] = String.Empty;
      _properties["#TV.stop"] = String.Empty;
      _properties["#TV.current"] = String.Empty;
      _properties["#TV.Record.channel"] = String.Empty;
      _properties["#TV.Record.start"] = String.Empty;
      _properties["#TV.Record.stop"] = String.Empty;
      _properties["#TV.Record.genre"] = String.Empty;
      _properties["#TV.Record.title"] = String.Empty;
      _properties["#TV.Record.description"] = String.Empty;
      _properties["#TV.Record.thumb"] = String.Empty;
      _properties["#TV.View.channel"] = String.Empty;
      _properties["#TV.View.thumb"] = String.Empty;
      _properties["#TV.View.start"] = String.Empty;
      _properties["#TV.View.stop"] = String.Empty;
      _properties["#TV.View.remaining"] = String.Empty;
      _properties["#TV.View.genre"] = String.Empty;
      _properties["#TV.View.title"] = String.Empty;
      _properties["#TV.View.description"] = String.Empty;
      _properties["#TV.View.Percentage"] = String.Empty;

      _properties["#TV.Next.start"] = String.Empty;
      _properties["#TV.Next.stop"] = String.Empty;
      _properties["#TV.Next.genre"] = String.Empty;
      _properties["#TV.Next.title"] = String.Empty;
      _properties["#TV.Next.description"] = String.Empty;

      _properties["#TV.Guide.Day"] = String.Empty;
      _properties["#TV.Guide.thumb"] = String.Empty;
      _properties["#TV.Guide.Title"] = String.Empty;
      _properties["#TV.Guide.Time"] = String.Empty;
      _properties["#TV.Guide.Duration"] = String.Empty;
      _properties["#TV.Guide.TimeFromNow"] = String.Empty;
      _properties["#TV.Guide.Description"] = String.Empty;
      _properties["#TV.Guide.Genre"] = String.Empty;
      _properties["#TV.Guide.EpisodeName"] = String.Empty;
      _properties["#TV.Guide.SeriesNumber"] = String.Empty;
      _properties["#TV.Guide.EpisodeNumber"] = String.Empty;
      _properties["#TV.Guide.EpisodePart"] = String.Empty;
      _properties["#TV.Guide.EpisodeDetail"] = String.Empty;
      _properties["#TV.Guide.Date"] = String.Empty;
      _properties["#TV.Guide.StarRating"] = String.Empty;
      _properties["#TV.Guide.Classification"] = String.Empty;


      _properties["#Radio.Guide.Day"] = String.Empty;
      _properties["#Radio.Guide.thumb"] = String.Empty;
      _properties["#Radio.Guide.Title"] = String.Empty;
      _properties["#Radio.Guide.Time"] = String.Empty;
      _properties["#Radio.Guide.Duration"] = String.Empty;
      _properties["#Radio.Guide.TimeFromNow"] = String.Empty;
      _properties["#Radio.Guide.Description"] = String.Empty;
      _properties["#Radio.Guide.Genre"] = String.Empty;
      _properties["#Radio.Guide.EpisodeName"] = String.Empty;
      _properties["#Radio.Guide.SeriesNumber"] = String.Empty;
      _properties["#Radio.Guide.EpisodeNumber"] = String.Empty;
      _properties["#Radio.Guide.EpisodePart"] = String.Empty;
      _properties["#Radio.Guide.EpisodeDetail"] = String.Empty;
      _properties["#Radio.Guide.Date"] = String.Empty;
      _properties["#Radio.Guide.StarRating"] = String.Empty;
      _properties["#Radio.Guide.Classification"] = String.Empty;

      _properties["#TV.RecordedTV.Title"] = String.Empty;
      _properties["#TV.RecordedTV.Time"] = String.Empty;
      _properties["#TV.RecordedTV.Description"] = String.Empty;
      _properties["#TV.RecordedTV.thumb"] = String.Empty;
      _properties["#TV.RecordedTV.Genre"] = String.Empty;
      _properties["#TV.Signal.Quality"] = String.Empty;

      _properties["#TV.Scheduled.Title"] = String.Empty;
      _properties["#TV.Scheduled.Time"] = String.Empty;
      _properties["#TV.Scheduled.Description"] = String.Empty;
      _properties["#TV.Scheduled.thumb"] = String.Empty;
      _properties["#TV.Scheduled.Genre"] = String.Empty;

      _properties["#TV.Search.Title"] = String.Empty;
      _properties["#TV.Search.Time"] = String.Empty;
      _properties["#TV.Search.Description"] = String.Empty;
      _properties["#TV.Search.thumb"] = String.Empty;
      _properties["#TV.Search.Genre"] = String.Empty;

      _properties["#view"] = String.Empty;

      _properties["#TV.Transcoding.Percentage"] = String.Empty;
      _properties["#TV.Transcoding.File"] = String.Empty;
      _properties["#TV.Transcoding.Title"] = String.Empty;
      _properties["#TV.Transcoding.Genre"] = String.Empty;
      _properties["#TV.Transcoding.Description"] = String.Empty;
      _properties["#TV.Transcoding.Channel"] = String.Empty;


      _properties["#Play.Current.Thumb"] = String.Empty;
      _properties["#Play.Current.File"] = String.Empty;
      _properties["#Play.Current.Title"] = String.Empty;
      _properties["#Play.Current.Genre"] = String.Empty;
      _properties["#Play.Current.Comment"] = String.Empty;
      _properties["#Play.Current.Artist"] = String.Empty;
      _properties["#Play.Current.Director"] = String.Empty;
      _properties["#Play.Current.Album"] = String.Empty;
      _properties["#Play.Current.Track"] = String.Empty;
      _properties["#Play.Current.Year"] = String.Empty;
      _properties["#Play.Current.Duration"] = String.Empty;
      _properties["#Play.Current.Plot"] = String.Empty;
      _properties["#Play.Current.PlotOutline"] = String.Empty;
      _properties["#Play.Current.Channel"] = String.Empty;
      _properties["#Play.Current.Cast"] = String.Empty;
      _properties["#Play.Current.DVDLabel"] = String.Empty;
      _properties["#Play.Current.IMDBNumber"] = String.Empty;
      _properties["#Play.Current.Rating"] = String.Empty;
      _properties["#Play.Current.TagLine"] = String.Empty;
      _properties["#Play.Current.Votes"] = String.Empty;
      _properties["#Play.Current.Credits"] = String.Empty;
      _properties["#Play.Current.Runtime"] = String.Empty;
      _properties["#Play.Current.MPAARating"] = String.Empty;
      _properties["#Play.Current.IsWatched"] = String.Empty;



      _properties["#Play.Next.Thumb"] = String.Empty;
      _properties["#Play.Next.File"] = String.Empty;
      _properties["#Play.Next.Title"] = String.Empty;
      _properties["#Play.Next.Genre"] = String.Empty;
      _properties["#Play.Next.Comment"] = String.Empty;
      _properties["#Play.Next.Artist"] = String.Empty;
      _properties["#Play.Next.Director"] = String.Empty;
      _properties["#Play.Next.Album"] = String.Empty;
      _properties["#Play.Next.Track"] = String.Empty;
      _properties["#Play.Next.Year"] = String.Empty;
      _properties["#Play.Next.Duration"] = String.Empty;
      _properties["#Play.Next.Plot"] = String.Empty;
      _properties["#Play.Next.PlotOutline"] = String.Empty;
      _properties["#Play.Next.Channel"] = String.Empty;
      _properties["#Play.Next.Cast"] = String.Empty;
      _properties["#Play.Next.DVDLabel"] = String.Empty;
      _properties["#Play.Next.IMDBNumber"] = String.Empty;
      _properties["#Play.Next.Rating"] = String.Empty;
      _properties["#Play.Next.TagLine"] = String.Empty;
      _properties["#Play.Next.Votes"] = String.Empty;
      _properties["#Play.Next.Credits"] = String.Empty;
      _properties["#Play.Next.Runtime"] = String.Empty;
      _properties["#Play.Next.MPAARating"] = String.Empty;
      _properties["#Play.Next.IsWatched"] = String.Empty;


    }

    /// <summary>
    /// Get/set if the properties have changed.
    /// </summary>
    static public bool Changed
    {
      get { return _isChanged; }
      set { _isChanged = value; }
    }

    /// <summary>
    /// This method will set the value for a given property
    /// </summary>
    /// <param name="tag">property name</param>
    /// <param name="tagvalue">property value</param>
    static public void SetProperty(string tag, string tagvalue)
    {
      if (tag == null) return;
      if (tagvalue == null) return;
      if (tag == String.Empty) return;
      if (tag[0] != '#') return;

      if (tag.Equals("#currentmodule"))
      {
        try
        {
          GUIGraphicsContext.form.Text = "MediaPortal - " + tagvalue;
        }
        catch (Exception) { }
      }
      lock (_properties)
      {
        if (GetProperty(tag) == tagvalue) return;
        _properties[tag] = tagvalue;
        _isChanged = true;
      }
      if (OnPropertyChanged != null)
      {
        OnPropertyChanged(tag, tagvalue);
      }
    }
    /// <summary>
    /// This method returns the value for a given property
    /// </summary>
    /// <param name="tag">property name</param>
    /// <returns>property value</returns>
    static public string GetProperty(string tag)
    {
      if (tag == null) return String.Empty;
      if (tag == String.Empty) return String.Empty;
      if (tag[0] != '#') return String.Empty;
      lock (_properties)
      {
        if (_properties.ContainsKey(tag))
          return (string)_properties[tag];
      }
      return String.Empty;
    }

    /// <summary>
    /// Removes the player properties from the hashtable.
    /// </summary>
    static public void RemovePlayerProperties()
    {
      SetProperty("#Play.Current.Thumb", String.Empty);
      SetProperty("#Play.Current.File", String.Empty);
      SetProperty("#Play.Current.Title", String.Empty);
      SetProperty("#Play.Current.Genre", String.Empty);
      SetProperty("#Play.Current.Comment", String.Empty);
      SetProperty("#Play.Current.Artist", String.Empty);
      SetProperty("#Play.Current.Director", String.Empty);
      SetProperty("#Play.Current.Album", String.Empty);
      SetProperty("#Play.Current.Track", String.Empty);
      SetProperty("#Play.Current.Year", String.Empty);
      SetProperty("#Play.Current.Duration", String.Empty);
      SetProperty("#Play.Current.Plot", String.Empty);
      SetProperty("#Play.Current.PlotOutline", String.Empty);
      SetProperty("#Play.Current.Channel", String.Empty);
      SetProperty("#Play.Current.Cast", String.Empty);
      SetProperty("#Play.Current.DVDLabel", String.Empty);
      SetProperty("#Play.Current.IMDBNumber", String.Empty);
      SetProperty("#Play.Current.Rating", String.Empty);
      SetProperty("#Play.Current.TagLine", String.Empty);
      SetProperty("#Play.Current.Votes", String.Empty);
      SetProperty("#Play.Current.Credits", String.Empty);
      SetProperty("#Play.Current.Runtime", String.Empty);
      SetProperty("#Play.Current.MPAARating", String.Empty);
      SetProperty("#Play.Current.IsWatched", String.Empty);



      SetProperty("#Play.Next.Thumb", String.Empty);
      SetProperty("#Play.Next.File", String.Empty);
      SetProperty("#Play.Next.Title", String.Empty);
      SetProperty("#Play.Next.Genre", String.Empty);
      SetProperty("#Play.Next.Comment", String.Empty);
      SetProperty("#Play.Next.Artist", String.Empty);
      SetProperty("#Play.Next.Director", String.Empty);
      SetProperty("#Play.Next.Album", String.Empty);
      SetProperty("#Play.Next.Track", String.Empty);
      SetProperty("#Play.Next.Year", String.Empty);
      SetProperty("#Play.Next.Duration", String.Empty);
      SetProperty("#Play.Next.Plot", String.Empty);
      SetProperty("#Play.Next.PlotOutline", String.Empty);
      SetProperty("#Play.Next.Channel", String.Empty);
      SetProperty("#Play.Next.Cast", String.Empty);
      SetProperty("#Play.Next.DVDLabel", String.Empty);
      SetProperty("#Play.Next.IMDBNumber", String.Empty);
      SetProperty("#Play.Next.Rating", String.Empty);
      SetProperty("#Play.Next.TagLine", String.Empty);
      SetProperty("#Play.Next.Votes", String.Empty);
      SetProperty("#Play.Next.Credits", String.Empty);
      SetProperty("#Play.Next.Runtime", String.Empty);
      SetProperty("#Play.Next.MPAARating", String.Empty);
      SetProperty("#Play.Next.IsWatched", String.Empty);

      _isChanged = true;
    }

    /// <summary>
    /// Parses a property requrest.
    /// </summary>
    /// <param name="line ">The identification of the propertie (e.g.,#title).</param>
    /// <returns>The value of the property.</returns>
    static public string Parse(string line)
    {
      if (line == null) return String.Empty;
      if (line == String.Empty) return String.Empty;
      if (line.IndexOf('#') == -1) return line;
      lock (_properties)
      {
        try
        {
          IDictionaryEnumerator myEnumerator = _properties.GetEnumerator();
          if (myEnumerator == null) return line;
          myEnumerator.Reset();
          while (myEnumerator.MoveNext() && line.IndexOf('#') >= 0)
          {
            line = line.Replace((string)myEnumerator.Key, (string)myEnumerator.Value);
          }
        }
        catch (Exception ex)
        {
          Log.Write("PropertyManager: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }
      }
      return line;
    }
  }
}
