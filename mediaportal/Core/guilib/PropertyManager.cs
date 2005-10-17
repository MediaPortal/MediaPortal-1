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
    static Hashtable m_properties = new Hashtable();
    static bool m_bChanged=false;
		public delegate void OnPropertyChangedHandler(string tag, string tagValue);
		static public event OnPropertyChangedHandler     OnPropertyChanged;

		/// <summary>
		/// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
		/// </summary>
    private GUIPropertyManager()
    {
    }
    static  GUIPropertyManager()
    {
			m_properties["#highlightedbutton"]=String.Empty;
			m_properties["#itemcount"]=String.Empty;
      m_properties["#selecteditem"]=String.Empty;
      m_properties["#selecteditem2"]=String.Empty;
      m_properties["#selectedthumb"]=String.Empty;
      m_properties["#title"]=String.Empty;			// song title, imdb movie title, recording title
      m_properties["#artist"]=String.Empty;			// song artist
      m_properties["#album"]=String.Empty;			// song album
      m_properties["#track"]=String.Empty;			// song track number
      m_properties["#year"]=String.Empty;				// song year , imdb movie year
      m_properties["#comment"]=String.Empty;		// song comment
      m_properties["#director"]=String.Empty;		// imdb movie director
      m_properties["#genre"]=String.Empty;			// imdb movie genres
      m_properties["#cast"]=String.Empty;				// imdb movie cast 
      m_properties["#dvdlabel"]=String.Empty;		// imdb movie dvd label
      m_properties["#imdbnumber"]=String.Empty; // imdb movie number
      m_properties["#file"]=String.Empty;				// imdb movie filename
      m_properties["#plot"]=String.Empty;				// imdb movie plot 
      m_properties["#plotoutline"]=String.Empty;// imdb movie plot outline
      m_properties["#rating"]=String.Empty;		  // imdb movie rating (0-10)
      m_properties["#tagline"]=String.Empty;    // imdb movie tag line
      m_properties["#votes"]=String.Empty;		  // imdb movie votes
      m_properties["#credits"]=String.Empty;    // imdb movie writing credits
			m_properties["#mpaarating"]=String.Empty; // imdb movie MPAA rating
			m_properties["#runtime"]=String.Empty;    // imdb movie runtime 
			m_properties["#iswatched"]=String.Empty;  // boolean indication movie has been watched
			m_properties["#thumb"]=String.Empty;
      m_properties["#currentplaytime"]=String.Empty;
		m_properties["#currentplaytimeremaining"]=String.Empty;
		m_properties["#shortcurrentplaytime"]=String.Empty;
      m_properties["#duration"]=String.Empty;
      m_properties["#shortduration"]=String.Empty;
      m_properties["#playlogo"]=String.Empty;
      m_properties["#playspeed"]=String.Empty;
      m_properties["#percentage"]=String.Empty;
      m_properties["#currentmodule"]=String.Empty;
      m_properties["#channel"]=String.Empty;
      m_properties["#TV.start"]=String.Empty;
      m_properties["#TV.stop"]=String.Empty;
      m_properties["#TV.current"]=String.Empty;
      m_properties["#TV.Record.channel"]=String.Empty;
      m_properties["#TV.Record.start"]=String.Empty;
      m_properties["#TV.Record.stop"]=String.Empty;
      m_properties["#TV.Record.genre"]=String.Empty;
      m_properties["#TV.Record.title"]=String.Empty;
      m_properties["#TV.Record.description"]=String.Empty;
      m_properties["#TV.Record.thumb"]=String.Empty;
      m_properties["#TV.View.channel"]=String.Empty;    
      m_properties["#TV.View.thumb"]=String.Empty;      
      m_properties["#TV.View.start"]=String.Empty;      
      m_properties["#TV.View.stop"]=String.Empty;       
      m_properties["#TV.View.genre"]=String.Empty;      
      m_properties["#TV.View.title"]=String.Empty;      
      m_properties["#TV.View.description"]=String.Empty;
      m_properties["#TV.View.Percentage"]=String.Empty;
      m_properties["#TV.Guide.Day"]=String.Empty;          
      m_properties["#TV.Guide.thumb"]=String.Empty;        
      m_properties["#TV.Guide.Title"]=String.Empty;        
      m_properties["#TV.Guide.Time"]=String.Empty;         
      m_properties["#TV.Guide.Duration"]=String.Empty;
      m_properties["#TV.Guide.TimeFromNow"]=String.Empty;
      m_properties["#TV.Guide.Description"]=String.Empty;  
      m_properties["#TV.Guide.Genre"]=String.Empty;        
      m_properties["#TV.Guide.EpisodeName"]=String.Empty;        
      m_properties["#TV.Guide.SeriesNumber"]=String.Empty;        
      m_properties["#TV.Guide.EpisodeNumber"]=String.Empty;        
      m_properties["#TV.Guide.EpisodePart"]=String.Empty;        
      m_properties["#TV.Guide.EpisodeDetail"]=String.Empty;        
      m_properties["#TV.Guide.Date"]=String.Empty;        
      m_properties["#TV.Guide.StarRating"]=String.Empty;        
      m_properties["#TV.Guide.Classification"]=String.Empty;
      m_properties["#TV.RecordedTV.Title"]=String.Empty;              
      m_properties["#TV.RecordedTV.Time"]=String.Empty;               
      m_properties["#TV.RecordedTV.Description"]=String.Empty;        
      m_properties["#TV.RecordedTV.thumb"]=String.Empty;              
      m_properties["#TV.RecordedTV.Genre"]=String.Empty;  
			m_properties["#TV.Signal.Quality"]=String.Empty;  
      
			m_properties["#TV.Scheduled.Title"]=String.Empty;              
			m_properties["#TV.Scheduled.Time"]=String.Empty;               
			m_properties["#TV.Scheduled.Description"]=String.Empty;        
			m_properties["#TV.Scheduled.thumb"]=String.Empty;              
			m_properties["#TV.Scheduled.Genre"]=String.Empty;       
      
			m_properties["#TV.Search.Title"]=String.Empty;              
			m_properties["#TV.Search.Time"]=String.Empty;               
			m_properties["#TV.Search.Description"]=String.Empty;        
			m_properties["#TV.Search.thumb"]=String.Empty;              
			m_properties["#TV.Search.Genre"]=String.Empty;        
 
			m_properties["#view"]=String.Empty;  
      
			m_properties["#TV.Transcoding.Percentage"]=String.Empty;        
			m_properties["#TV.Transcoding.File"]=String.Empty;        
			m_properties["#TV.Transcoding.Title"]=String.Empty;        
			m_properties["#TV.Transcoding.Genre"]=String.Empty;        
			m_properties["#TV.Transcoding.Description"]=String.Empty;        
			m_properties["#TV.Transcoding.Channel"]=String.Empty; 
      
 
			m_properties["#Play.Current.Thumb"]=String.Empty; 
			m_properties["#Play.Current.File"]=String.Empty; 
			m_properties["#Play.Current.Title"]=String.Empty; 
			m_properties["#Play.Current.Genre"]=String.Empty; 
			m_properties["#Play.Current.Comment"]=String.Empty; 
			m_properties["#Play.Current.Artist"]=String.Empty;  
			m_properties["#Play.Current.Director"]=String.Empty;  
			m_properties["#Play.Current.Album"]=String.Empty;   
			m_properties["#Play.Current.Track"]=String.Empty;   
			m_properties["#Play.Current.Year"]=String.Empty;    
			m_properties["#Play.Current.Duration"]=String.Empty; 
			m_properties["#Play.Current.Plot"]=String.Empty; 
			m_properties["#Play.Current.PlotOutline"]=String.Empty; 
			m_properties["#Play.Current.Channel"]=String.Empty; 
			m_properties["#Play.Current.Cast"]=String.Empty; 
			m_properties["#Play.Current.DVDLabel"]=String.Empty; 
			m_properties["#Play.Current.IMDBNumber"]=String.Empty; 
			m_properties["#Play.Current.Rating"]=String.Empty; 
			m_properties["#Play.Current.TagLine"]=String.Empty; 
			m_properties["#Play.Current.Votes"]=String.Empty; 
			m_properties["#Play.Current.Credits"]=String.Empty; 
			m_properties["#Play.Current.Runtime"]=String.Empty; 
			m_properties["#Play.Current.MPAARating"]=String.Empty; 
			m_properties["#Play.Current.IsWatched"]=String.Empty; 

 
			
			m_properties["#Play.Next.Thumb"]=String.Empty; 
			m_properties["#Play.Next.File"]=String.Empty; 
			m_properties["#Play.Next.Title"]=String.Empty; 
			m_properties["#Play.Next.Genre"]=String.Empty; 
			m_properties["#Play.Next.Comment"]=String.Empty; 
			m_properties["#Play.Next.Artist"]=String.Empty;  
			m_properties["#Play.Next.Director"]=String.Empty;  
			m_properties["#Play.Next.Album"]=String.Empty;   
			m_properties["#Play.Next.Track"]=String.Empty;   
			m_properties["#Play.Next.Year"]=String.Empty;    
			m_properties["#Play.Next.Duration"]=String.Empty; 
			m_properties["#Play.Next.Plot"]=String.Empty; 
			m_properties["#Play.Next.PlotOutline"]=String.Empty; 
			m_properties["#Play.Next.Channel"]=String.Empty; 
			m_properties["#Play.Next.Cast"]=String.Empty; 
			m_properties["#Play.Next.DVDLabel"]=String.Empty; 
			m_properties["#Play.Next.IMDBNumber"]=String.Empty; 
			m_properties["#Play.Next.Rating"]=String.Empty; 
			m_properties["#Play.Next.TagLine"]=String.Empty; 
			m_properties["#Play.Next.Votes"]=String.Empty; 
			m_properties["#Play.Next.Credits"]=String.Empty; 
			m_properties["#Play.Next.Runtime"]=String.Empty; 
			m_properties["#Play.Next.MPAARating"]=String.Empty; 
			m_properties["#Play.Next.IsWatched"]=String.Empty; 


    }

		/// <summary>
		/// Get/set if the properties have changed.
		/// </summary>
    static public bool Changed
    {
      get {return m_bChanged;}
      set {m_bChanged=value;}
    }

		/// <summary>
		/// This method will set the value for a given property
		/// </summary>
		/// <param name="tag">property name</param>
		/// <param name="tagvalue">property value</param>
    static public void SetProperty(string tag, string tagvalue)
    {
			if (tag==null) return;
			if (tagvalue==null) return;
			if (tag==String.Empty) return;
			if (tag[0]!='#') return;

			if (tag.Equals("#currentmodule"))
			{
				GUIGraphicsContext.form.Text="Media Portal - "+  tagvalue;
			}
			lock (typeof(GUIPropertyManager))
			{
				if (GetProperty(tag) == tagvalue) return;
				m_properties[tag] = tagvalue;
				m_bChanged=true;
			}
			if (OnPropertyChanged!=null)
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
      if (tag==null) return String.Empty;
      if (tag==String.Empty) return String.Empty;
      if (tag[0]!='#') return String.Empty;
			lock (typeof(GUIPropertyManager))
			{
					if (m_properties.ContainsKey(tag))
					return (string)m_properties[tag];
			}
			return String.Empty;
    }

		/// <summary>
		/// Removes the player properties from the hashtable.
		/// </summary>
    static public void RemovePlayerProperties()
    {
			SetProperty("#Play.Current.Thumb",String.Empty);
			SetProperty("#Play.Current.File",String.Empty);
			SetProperty("#Play.Current.Title",String.Empty);
			SetProperty("#Play.Current.Genre",String.Empty);
			SetProperty("#Play.Current.Comment",String.Empty);
			SetProperty("#Play.Current.Artist",String.Empty); 
			SetProperty("#Play.Current.Director",String.Empty); 
			SetProperty("#Play.Current.Album",String.Empty);  
			SetProperty("#Play.Current.Track",String.Empty);  
			SetProperty("#Play.Current.Year",String.Empty);   
			SetProperty("#Play.Current.Duration",String.Empty);
			SetProperty("#Play.Current.Plot",String.Empty);
			SetProperty("#Play.Current.PlotOutline",String.Empty);
			SetProperty("#Play.Current.Channel",String.Empty);
			SetProperty("#Play.Current.Cast",String.Empty);
			SetProperty("#Play.Current.DVDLabel",String.Empty);
			SetProperty("#Play.Current.IMDBNumber",String.Empty);
			SetProperty("#Play.Current.Rating",String.Empty);
			SetProperty("#Play.Current.TagLine",String.Empty);
			SetProperty("#Play.Current.Votes",String.Empty);
			SetProperty("#Play.Current.Credits",String.Empty);
			SetProperty("#Play.Current.Runtime",String.Empty);
			SetProperty("#Play.Current.MPAARating",String.Empty);
			SetProperty("#Play.Current.IsWatched",String.Empty);

 
			
			SetProperty("#Play.Next.Thumb",String.Empty);
			SetProperty("#Play.Next.File",String.Empty);
			SetProperty("#Play.Next.Title",String.Empty);
			SetProperty("#Play.Next.Genre",String.Empty);
			SetProperty("#Play.Next.Comment",String.Empty);
			SetProperty("#Play.Next.Artist",String.Empty); 
			SetProperty("#Play.Next.Director",String.Empty); 
			SetProperty("#Play.Next.Album",String.Empty);  
			SetProperty("#Play.Next.Track",String.Empty);  
			SetProperty("#Play.Next.Year",String.Empty);   
			SetProperty("#Play.Next.Duration",String.Empty);
			SetProperty("#Play.Next.Plot",String.Empty);
			SetProperty("#Play.Next.PlotOutline",String.Empty);
			SetProperty("#Play.Next.Channel",String.Empty);
			SetProperty("#Play.Next.Cast",String.Empty);
			SetProperty("#Play.Next.DVDLabel",String.Empty);
			SetProperty("#Play.Next.IMDBNumber",String.Empty);
			SetProperty("#Play.Next.Rating",String.Empty);
			SetProperty("#Play.Next.TagLine",String.Empty);
			SetProperty("#Play.Next.Votes",String.Empty);
			SetProperty("#Play.Next.Credits",String.Empty);
			SetProperty("#Play.Next.Runtime",String.Empty);
			SetProperty("#Play.Next.MPAARating",String.Empty);
			SetProperty("#Play.Next.IsWatched",String.Empty);

      m_bChanged=true;
    }

		/// <summary>
		/// Parses a property requrest.
		/// </summary>
		/// <param name="strTxt">The identification of the propertie (e.g.,#title).</param>
		/// <returns>The value of the property.</returns>
    static public string Parse(string strTxt)
    {
      if (strTxt==null) return String.Empty;
      if (strTxt==String.Empty) return String.Empty;
      if (strTxt.IndexOf('#')==-1) return strTxt;
			lock (typeof(GUIPropertyManager))
			{
				try
				{
					IDictionaryEnumerator myEnumerator = m_properties.GetEnumerator();
					if (myEnumerator==null) return strTxt;
					myEnumerator.Reset();
					while ( myEnumerator.MoveNext() && strTxt.IndexOf('#') >=0)
					{
						strTxt=strTxt.Replace((string)myEnumerator.Key,(string)myEnumerator.Value);
					}
				}
				catch (Exception ex)
				{
					Log.Write("GUIProp {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
				}
			}
      return strTxt;
    }
	}
}
