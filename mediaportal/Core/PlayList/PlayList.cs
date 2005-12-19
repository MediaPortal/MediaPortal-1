/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using MediaPortal.GUI.Library;
using System.Collections.Generic;

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
      protected string _fileName="";
      protected string _description="";
      protected int    _duration=0;
      protected object _musicTag=null;
      bool             _isPlayed=false;
      PlayListItemType _itemType=PlayListItemType.Unknown;

      public PlayListItem()
      {
      }
      
      public PlayListItem(string description, string fileName)
      {
        if (description == null) return;
        if (fileName == null) return;
        _description = description;
        _fileName = fileName;
        _duration=0;
      }

      public PlayListItem(string description, string fileName, int duration)
      {
        if (description == null) return;
        if (fileName == null) return;
        _description = description;
        _fileName = fileName;
        _duration = duration;
      }
      
      public PlayListItem.PlayListItemType Type
      {
        get { return _itemType;}
        set { _itemType=value;}
      }

      public string FileName
      {
        get { return _fileName;}
        set 
        {  
          if (value==null) return;
          _fileName=value;
        }
      }
      public string Description
      {
        get { return _description;}
        set { 
          if (value==null) return;
          _description=value;
        }
      }
      public int Duration
      {
        get { return _duration;}
        set { _duration=value;}
      }
      public bool Played
      {
        get { return _isPlayed;}
        set { _isPlayed=value;}
      }

      /// <summary>
      /// Get/set the object containing the tag info of a music file (e.g., id3 tag).
      /// </summary>
      public object MusicTag
      {
        get { return _musicTag;}
        set {_musicTag=value;}
      }
    };


    protected string		_playListName="";
    protected List<PlayListItem> _listPlayListItems = new List<PlayListItem>();
    public PlayList()
    {
    }
    
    public bool AllPlayed()
    {
      foreach (PlayListItem item in _listPlayListItems)
      {
        if (!item.Played) return false;
      }
      return true;
    }

    public void ResetStatus()
    {
      foreach (PlayListItem item in _listPlayListItems)
      {
        item.Played=false;
      }
    }

    public void Add( PlayListItem item)
    {
      if (item==null) return;
      _listPlayListItems.Add(item);
    }

    public string Name
    {
      get { return _playListName;}
      set { 
        if (value==null) return ;
        _playListName=value;
      }
    }

    public int Remove( string fileName)
    {
      if (fileName==null) return -1;
      for (int i=0; i < _listPlayListItems.Count;++i)
      {
        PlayListItem item=_listPlayListItems[i];
        if (item.FileName == fileName)
        {
          _listPlayListItems.RemoveAt(i);
          return i;
        }
      }
      return -1;
    }

    public void Clear()
    {
      _listPlayListItems.Clear();
    }

    public int Count
    {
      get { return _listPlayListItems.Count;}
    }

    public Playlists.PlayList.PlayListItem this [int iItem]
    {
      get { return _listPlayListItems[iItem];}
    }

    
    public int	RemoveDVDItems()
    {
      //TODO
      return 0;
    }
     
    public virtual void Shuffle()
    {

      Random r = new System.Random(DateTime.Now.Millisecond);
      
      // iterate through each catalogue item performing arbitrary swaps
      for (int item = 0; item < Count; item++)
      {
        int nArbitrary = r.Next(Count);

        PlayListItem anItem = _listPlayListItems[nArbitrary];
        _listPlayListItems[nArbitrary] = _listPlayListItems[item];
        _listPlayListItems[item] = anItem;
      }

    }
    
    public virtual bool 	Load(string filename)
    {
      return false;
    }

    public virtual void Save(string filename) 
    {
    }
    
	}
}