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
      protected object m_pMusicTag=null;
      bool             m_bPlayed=false;
      PlayListItemType m_Type=PlayListItemType.Unknown;

      public PlayListItem()
      {
      }
      
      public PlayListItem(string strDescription, string strFileName)
      {
        if (strDescription==null) return;
        if (strFileName==null) return;
        m_strDescription=strDescription;
        m_strFilename=strFileName;
        m_iDuration=0;
      }

      public PlayListItem(string strDescription, string strFileName, int iDuration)
      {
        if (strDescription==null) return;
        if (strFileName==null) return;
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
        set 
        {  
          if (value==null) return;
          m_strFilename=value;
        }
      }
      public string Description
      {
        get { return m_strDescription;}
        set { 
          if (value==null) return;
          m_strDescription=value;
        }
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

      /// <summary>
      /// Get/set the object containing the tag info of a music file (e.g., id3 tag).
      /// </summary>
      public object MusicTag
      {
        get { return m_pMusicTag;}
        set {m_pMusicTag=value;}
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
      if (item==null) return;
      m_items.Add(item);
    }

    public string Name
    {
      get { return m_strPlayListName;}
      set { 
        if (value==null) return ;
        m_strPlayListName=value;
      }
    }

    public int Remove( string strFileName)
    {
      if (strFileName==null) return -1;
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