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
		public delegate void OnPropertyChangedHandler(string tag, string value);
		static public event OnPropertyChangedHandler     OnPropertyChanged;

		/// <summary>
		/// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
		/// </summary>
    private GUIPropertyManager()
    {
    }
    static  GUIPropertyManager()
    {
      m_properties["#itemcount"]="";
      m_properties["#selecteditem"]="";
      m_properties["#selecteditem2"]="";
      m_properties["#selectedthumb"]="";
      m_properties["#title"]="";
      m_properties["#artist"]="";
      m_properties["#album"]="";
      m_properties["#track"]="";
      m_properties["#year"]="";
      m_properties["#comment"]="";
      m_properties["#director"]="";
      m_properties["#genre"]="";
      m_properties["#cast"]="";
      m_properties["#dvdlabel"]="";
      m_properties["#imdbnumber"]="";
      m_properties["#file"]="";
      m_properties["#plot"]="";
      m_properties["#plotoutline"]="";
      m_properties["#rating"]="";
      m_properties["#tagline"]="";
      m_properties["#votes"]="";
      m_properties["#credits"]="";
      m_properties["#thumb"]="";
      m_properties["#currentplaytime"]="";
      m_properties["#shortcurrentplaytime"]="";
      m_properties["#duration"]="";
      m_properties["#shortduration"]="";
      m_properties["#playlogo"]="";
      m_properties["#playspeed"]="";
      m_properties["#percentage"]="";
      m_properties["#currentmodule"]="";
      m_properties["#channel"]="";
      m_properties["#TV.start"]="";
      m_properties["#TV.stop"]="";
      m_properties["#TV.current"]="";
      m_properties["#TV.Record.channel"]="";
      m_properties["#TV.Record.start"]="";
      m_properties["#TV.Record.stop"]="";
      m_properties["#TV.Record.genre"]="";
      m_properties["#TV.Record.title"]="";
      m_properties["#TV.Record.description"]="";
      m_properties["#TV.Record.thumb"]="";
      m_properties["#TV.View.channel"]="";    
      m_properties["#TV.View.thumb"]="";      
      m_properties["#TV.View.start"]="";      
      m_properties["#TV.View.stop"]="";       
      m_properties["#TV.View.genre"]="";      
      m_properties["#TV.View.title"]="";      
      m_properties["#TV.View.description"]="";
      m_properties["#TV.View.Percentage"]="";
      m_properties["#TV.Guide.Day"]="";          
      m_properties["#TV.Guide.thumb"]="";        
      m_properties["#TV.Guide.Title"]="";        
      m_properties["#TV.Guide.Time"]="";         
      m_properties["#TV.Guide.Duration"]="";
      m_properties["#TV.Guide.TimeFromNow"]="";
      m_properties["#TV.Guide.Description"]="";  
      m_properties["#TV.Guide.Genre"]="";        
      m_properties["#TV.Guide.EpisodeName"]="";        
      m_properties["#TV.Guide.SeriesNumber"]="";        
      m_properties["#TV.Guide.EpisodeNumber"]="";        
      m_properties["#TV.Guide.EpisodePart"]="";        
      m_properties["#TV.Guide.EpisodeDetail"]="";        
      m_properties["#TV.Guide.Date"]="";        
      m_properties["#TV.Guide.StarRating"]="";        
      m_properties["#TV.Guide.Classification"]="";
      m_properties["#TV.RecordedTV.Title"]="";              
      m_properties["#TV.RecordedTV.Time"]="";               
      m_properties["#TV.RecordedTV.Description"]="";        
      m_properties["#TV.RecordedTV.thumb"]="";              
      m_properties["#TV.RecordedTV.Genre"]="";              
 

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

			if (GetProperty(tag) == tagvalue) return;
			m_properties[tag] = tagvalue;
			m_bChanged=true;
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
			if (m_properties.ContainsKey(tag))
				return (string)m_properties[tag];
			return String.Empty;
    }

		/// <summary>
		/// Removes the player properties from the hashtable.
		/// </summary>
    static public void RemovePlayerProperties()
    {
      m_bChanged=true;
      SetProperty("#playlogo",String.Empty);
      SetProperty("#title",String.Empty);
      SetProperty("#artist",String.Empty);
      SetProperty("#album",String.Empty);
      SetProperty("#track",String.Empty);
      SetProperty("#year",String.Empty);
      SetProperty("#comment",String.Empty);
			SetProperty("#shortduration",String.Empty);
			SetProperty("#duration",String.Empty);
      SetProperty("#thumb",String.Empty);
      SetProperty("#director",String.Empty);
      SetProperty("#genre",String.Empty);
      SetProperty("#cast",String.Empty);
      SetProperty("#dvdlabel",String.Empty);
      SetProperty("#imdbnumber",String.Empty);
      SetProperty("#file",String.Empty);
      SetProperty("#plot",String.Empty);
      SetProperty("#plotoutline",String.Empty);
      SetProperty("#rating",String.Empty);
      SetProperty("#tagline",String.Empty);
      SetProperty("#votes",String.Empty);
      SetProperty("#credits",String.Empty);
			SetProperty("#currentplaytime",String.Empty);
			SetProperty("#shortcurrentplaytime",String.Empty);
      SetProperty("#playspeed",String.Empty);
      SetProperty("#channel",String.Empty);

      
      SetProperty("#fileNext",String.Empty);
      SetProperty("#titleNext",String.Empty);
      SetProperty("#genreNext",String.Empty);
      SetProperty("#commentNext",String.Empty);
      SetProperty("#artistNext",String.Empty);
      SetProperty("#albumNext",String.Empty);
      SetProperty("#trackNext",String.Empty);
      SetProperty("#yearNext",String.Empty);
      SetProperty("#durationNext",String.Empty);
      SetProperty("#thumbNext",String.Empty);
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
      return strTxt;
    }
	}
}
