using System;
using MediaPortal.GUI.Library;
using System.Collections;

namespace MediaPortal.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayList
	{
    public class PlayListItem
    {
      public enum PlayListItemType
      {
        Unknown,
        Audio,
        Radio,
        AudioStream,
        VideoStream,
        Video,
        DVD,
        TV,
        Pictures
      }
      protected string m_strFilename="";
      protected string m_strDescription="";
      protected int    m_iDuration=0;
      bool             m_bPlayed=false;
      PlayListItemType m_Type=PlayListItemType.Unknown;

      public PlayListItem()
      {
      }
      
      public PlayListItem(string strDescription, string strFileName)
      {
        m_strDescription=strDescription;
        m_strFilename=strFileName;
        m_iDuration=0;
      }

      public PlayListItem(string strDescription, string strFileName, int iDuration)
      {
        m_strDescription=strDescription;
        m_strFilename=strFileName;
        m_iDuration=iDuration;
      }
      
      public PlayListItem.PlayListItemType Type
      {
        get { return m_Type;}
        set { m_Type=value;}
      }

      public string FileName
      {
        get { return m_strFilename;}
        set { m_strFilename=value;}
      }
      public string Description
      {
        get { return m_strDescription;}
        set { m_strDescription=value;}
      }
      public int Duration
      {
        get { return m_iDuration;}
        set { m_iDuration=value;}
      }
      public bool Played
      {
        get { return m_bPlayed;}
        set { m_bPlayed=value;}
      }
    };


    protected string		m_strPlayListName="";
    protected ArrayList m_items   = new ArrayList();
    public PlayList()
    {
    }
    
    public bool AllPlayed()
    {
      foreach (PlayListItem item in m_items)
      {
        if (!item.Played) return false;
      }
      return true;
    }

    public void ResetStatus()
    {
      foreach (PlayListItem item in m_items)
      {
        item.Played=false;
      }
    }

    public void Add( PlayListItem item)
    {
      string strFile=item.FileName.ToLower();
      if (strFile.StartsWith("cdda:") ||  
          strFile.StartsWith("http:") ||  
          strFile.StartsWith("https:") ||  
          strFile.StartsWith("rtsp:") ||  
          strFile.StartsWith("ftp:") ||  
          strFile.IndexOf(".cda") >=0||  
          strFile.IndexOf(".radio") >=0||  
          strFile.StartsWith("mms:"))
      {
        m_items.Add(item);
        return;
      }
      
      //check if file exists
      try
      {
        if (System.IO.File.Exists(item.FileName))
        {
          m_items.Add(item);
          return;
        }
      }
      catch(Exception)
      {
      }
  
      Log.Write("Playlist:file does not exists:{0}", item.FileName);
    }

    public string Name
    {
      get { return m_strPlayListName;}
      set { m_strPlayListName=value;}
    }

    public int Remove( string strFileName)
    {
      for (int i=0; i < m_items.Count;++i)
      {
        PlayListItem item=(PlayListItem)m_items[i];
        if (item.FileName==strFileName)
        {
          m_items.RemoveAt(i);
          return i;
        }
      }
      return -1;
    }

    public void Clear()
    {
      m_items.Clear();
    }

    public int Count
    {
      get { return m_items.Count;}
    }

    public Playlists.PlayList.PlayListItem this [int iItem]
    {
      get { return (PlayListItem )m_items[iItem];}
    }

    
    public int	RemoveDVDItems()
    {
      //TODO
      return 0;
    }
     
    public virtual void Shuffle()
    {

      Random r = new System.Random(DateTime.Now.Millisecond);
      int nItemCount = Count;
      // iterate through each catalogue item performing arbitrary swaps
      for (int nItem=0; nItem < nItemCount; nItem++)
      {
        int nArbitrary = r.Next( nItemCount );

        PlayListItem anItem = (PlayListItem)m_items[nArbitrary];
        m_items[nArbitrary] = m_items[nItem];
        m_items[nItem] = anItem;
      }

    }
    
    public virtual bool 	Load(string strFileName)
    {
      return false;
    }

    public virtual void Save(string strFileName) 
    {
    }
    
	}
}