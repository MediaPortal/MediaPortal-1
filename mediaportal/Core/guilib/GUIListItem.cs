using System;

namespace MediaPortal.GUI.Library
{

	/// <summary>
	/// An implementation of an item that is part of a collection. (E.g, a GUIThumbnailPanel).
	/// </summary>
	public class GUIListItem
  {
    public delegate void    ItemSelectedHandler(GUIListItem item, GUIControl parent);
    public delegate void    RetrieveCoverArtHandler(GUIListItem item);

    //event which gets fired when the user has selected the item in the
    //list,thumbnail or filmstrip view
    public event ItemSelectedHandler OnItemSelected=null;

    //even which gets fired if the list,thumbnail or filmstrip few needs the
    //coverart for the specified item
    public event            RetrieveCoverArtHandler OnRetrieveArt=null;
		protected string        m_strLabel="";							// text of column1
		protected string        m_strLabel2="";							// text of column2
		protected string        m_strLabel3="";							// text of column3
		protected string        m_strThumbnailImage="";			// filename of thumbnail 
		protected string        m_strIcon="";								// filename of icon
    protected string        m_strIconBig="";						// filename of icon
    protected GUIImage      m_pThumbnailImage=null;			// pointer to CImage containing the thumbnail
		protected GUIImage      m_pIconImage=null;					// pointer to CImage containing the icon
    protected GUIImage      m_pIconImageBig=null;				// pointer to CImage containing the icon
    protected bool          m_bSelected=false;					// item is selected or not
		protected bool					m_bFolder=false;						// indicated if the item is a folder or a path
		protected string        m_strPath="";								// path + filename of the item
		protected string        m_strDVDLabel="";						// indicates the disc number of movie
		protected int           m_iDuration=0;							// duration (in seconds) of the movie or song
    System.IO.FileInfo      m_info=null;								// file info (size, date/time etc.) of the file
    bool                    m_bShaded=false;						// indicates if the item needs to be rendered shaded
    float                   m_fRating=0;								// rating of a movie
    int                     m_iYear=0;									// release year of the movie/song
    object                  m_musicTag;									// object containing the tag info of a music file (e.g., id3 tag)
    object                  m_TVTag;										// object containing the tag info of a tv-recording
    object                  m_AlbumInfoTag;							// object tag info of a music album
    bool                    m_bArtRetrieved=false;
	  string                  m_PinIcon=String.Empty;
	  protected GUIImage      m_PinIconImage=null;

    /// <summary>
		/// The (empty) constructor of the GUIListItem.
		/// </summary>
		public GUIListItem()
		{
		}
    

		/// <summary>
		/// Creates a GUIListItem based on another GUIListItem.
		/// </summary>
		/// <param name="item">The item on which the new item is based.</param>
		public GUIListItem(GUIListItem item)
		{
			m_strLabel=item.m_strLabel;
			m_strLabel2=item.m_strLabel2;
			m_strLabel3=item.m_strLabel3;
			m_strThumbnailImage=item.m_strThumbnailImage;
			m_strIcon=item.m_strIcon;
      m_strIconBig=item.m_strIconBig;
      m_PinIcon    =item.m_PinIcon;
      m_bSelected=item.m_bSelected;
			m_bFolder=item.m_bFolder;
      m_strPath=item.m_strPath;
			m_strDVDLabel=item.m_strDVDLabel;
			m_iDuration=item.m_iDuration;
			m_info=item.m_info;
			m_fRating=item.m_fRating;
			m_iYear=item.m_iYear;
      m_musicTag=item.m_musicTag;
      m_TVTag=item.m_TVTag;
      m_AlbumInfoTag=item.m_AlbumInfoTag;
		}

		/// <summary>
		/// Creates a GUIListItem.
		/// </summary>
		/// <param name="strLabel">The text of the first label of the item.</param>
		public GUIListItem(string strLabel)
		{
			 m_strLabel=strLabel;
		}

		/// <summary>
		/// Get/set the text of the first label of the item.
		/// </summary>
		public string Label
		{
			get { return m_strLabel;}
			set {m_strLabel=value;}
		}

		/// <summary>
		/// Get/set the text of the second label of the item.
		/// </summary>
		public string Label2
		{
			get { return m_strLabel2;}
			set {m_strLabel2=value;}
		}

		/// <summary>
		/// Get/set the text of the 3th label of the item.
		/// </summary>
		public string Label3
		{
			get { return m_strLabel3;}
			set {m_strLabel3=value;}
		}

		/// <summary>
		/// Get/set the filename of the ThumbnailImage of the item.
		/// </summary>
		public string ThumbnailImage
		{
			get { 
        
        RetrieveArt();
        return m_strThumbnailImage;
      }
			set {m_strThumbnailImage=value;}
		}

		/// <summary>
		/// Get/set the filename of the IconImage of the item.
		/// </summary>
		public string IconImage
		{
			get { 
        
        RetrieveArt();
        return m_strIcon;
      }
			set {m_strIcon=value;}
    }
		
    public string PinImage
    {
      get 
      { 
        
        RetrieveArt();
        return m_PinIcon;
      }
      set {m_PinIcon=value;}
    }
		

		/// <summary>
		/// Get/set the filename of the IconImageBig of the item.
		/// </summary>
    public string IconImageBig
    {
      get { 
        
        RetrieveArt();
        return m_strIconBig;
      }
      set {m_strIconBig=value;}
    }

		/// <summary>
		/// Get/set if the current item is selected.
		/// </summary>
		public bool Selected
		{
			get { return m_bSelected;}
			set {m_bSelected=value;}
		}

		/// <summary>
		/// Returns if the item has a thumbnail.
		/// </summary>
		public bool HasThumbnail
		{
			get { return m_strThumbnailImage.Length>0;}
		}

		/// <summary>
		/// Returns if the item has an icon.
		/// </summary>
		public bool HasIcon
		{
			get { 
        RetrieveArt();
        return m_strIcon.Length>0;
      }
    }


    /// <summary>
    /// Returns if the item has an icon.
    /// </summary>
    public bool HasPinIcon
    {
      get 
      { 
        RetrieveArt();
        return m_PinIcon.Length>0;
      }
    }

		/// <summary>
		/// Returns if the item has a bigicon.
		/// </summary>
    public bool HasIconBig
    {
      get { 
        
        RetrieveArt();
        return m_strIconBig.Length>0;
      }
    }

		/// <summary>
		/// Get/set the Thumbnail image.
		/// </summary>
		public GUIImage Thumbnail
		{
			get { return m_pThumbnailImage;}
			set { m_pThumbnailImage=value;}
		}

		/// <summary>
		/// Get/set the icon image.
		/// </summary>
		public GUIImage Icon
		{
			get { return m_pIconImage;}
			set { m_pIconImage=value;}
    }

    /// <summary>
    /// Get/set the pinicon image.
    /// </summary>
    public GUIImage PinIcon
    {
      get { return m_PinIconImage;}
      set { m_PinIconImage=value;}
    }

		/// <summary>
		/// Get/set the big icon image.
		/// </summary>
    public GUIImage IconBig
    {
      get { return m_pIconImageBig;}
      set { m_pIconImageBig=value;}
    }

		/// <summary>
		/// Get/set if the item is a folder.
		/// </summary>
		public bool IsFolder
		{
			get { return m_bFolder;}
			set { m_bFolder=value;}
		}

		/// <summary>
		/// Get/set the path + filename of the item.
		/// </summary>
    public string Path
    {
			get { return m_strPath;}
      set {m_strPath=value;}
		}

		/// <summary>
		/// Get/set the DVDLabel of the item. This indicates the disc number of movie.
		/// </summary>
		public string DVDLabel
		{
			get { return m_strDVDLabel;}
			set {m_strDVDLabel=value;}
		}

		/// <summary>
		/// Gets the file size of the item.
		/// </summary>
    public long Size
    {
      get 
      { 
        if (m_info!=null) 
          return m_info.Length;
        else
          return 0L;
      }
    }

		/// <summary>
		/// Get/set the file info of the item.
		/// </summary>
    public System.IO.FileInfo FileInfo
    {
      get { return m_info;}
      set { 
        try
        {
          m_info=null;
          if (value!=null)
          {
            if (value.Length!=0)
            {
              m_info=value;
            }
          }
        }
        catch(Exception){}
      }
    }

		/// <summary>
		/// Get/set the duration (in seconds) of the movie or song.
		/// </summary>
		public int Duration
		{
			get { return m_iDuration;}
			set {m_iDuration=value;}
		}

		/// <summary>
		/// Get/set the release year of the movie/song.
		/// </summary>
    public int Year
    {
			get { return m_iYear;}
      set {m_iYear=value;}
    }

		/// <summary>
		/// Get/set the rating of a movie.
		/// </summary>
    public float Rating
    {
			get { return m_fRating;}
      set {m_fRating=value;}
    }
   
		/// <summary>
		/// Get/set the object containing the tag info of a tv-recording.
		/// </summary>
    public object TVTag
    { 
			get { return m_TVTag;}
      set {m_TVTag=value;}
    }

		/// <summary>
		/// Get/set the object tag info of a music album
		/// </summary>
    public object AlbumInfoTag
    {
			get { return m_AlbumInfoTag;}
      set {m_AlbumInfoTag=value;}
    }
    
		/// <summary>
		/// Get/set the object containing the tag info of a music file (e.g., id3 tag).
		/// </summary>
    public object MusicTag
    {
			get { return m_musicTag;}
      set {m_musicTag=value;}
    }

		/// <summary>
		/// Get/set if the control is shaded.
		/// </summary>
    public bool Shaded
    {
      get { return m_bShaded;}
      set {m_bShaded=value;}
    }

		/// <summary>
		/// Free the memory that is used.
		/// </summary>
		public void FreeMemory()
		{
      m_bArtRetrieved=false;

			if (null!=m_pThumbnailImage) 
			{
				m_pThumbnailImage.FreeResources();
				m_pThumbnailImage=null;
			}
			if (null!=m_pIconImage)
			{
				m_pIconImage.FreeResources();
				m_pIconImage=null;
      }
      if (null!=m_PinIconImage)
      {
        m_PinIconImage.FreeResources();
        m_PinIconImage=null;
      }
		}

		/// <summary>
		/// Free the memory that is used by the icons.
		/// </summary>
		public void FreeIcons()
		{
			FreeMemory();
			m_strThumbnailImage="";
			m_strIcon="";
      m_PinIcon="";
		}
    
    /// <summary>
    /// This method will raise the OnRetrieveArt() event to
    /// ask the listener to supply the thumbnail(s) for this item
    /// </summary>
    void RetrieveArt()
    {
      if (m_bArtRetrieved) return;
      m_bArtRetrieved=true;
      if (OnRetrieveArt!=null) OnRetrieveArt(this);
    }
    
    
    /// <summary>
    /// this method will raise the OnItemSelected() event to let any
    /// listener know that this item has been selected by the user in a
    /// list,thumbnail or filmstrip control
    /// </summary>
    /// <param name="parent"></param>
    public void ItemSelected(GUIControl parent)
    {
      if (OnItemSelected!=null) 
      {
        AsyncCallback callback = new AsyncCallback(itemSelectedCallback);
        OnItemSelected.BeginInvoke(this, parent,callback,this);
      }
    }

    private void itemSelectedCallback(IAsyncResult ar)
    {
      OnItemSelected.EndInvoke(ar);
    }
	}
}
