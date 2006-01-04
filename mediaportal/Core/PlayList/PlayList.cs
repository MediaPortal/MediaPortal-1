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
using MediaPortal.TagReader;
using System.Collections;

namespace MediaPortal.Playlists
{
	public abstract class PlayList 
	{
    protected string _playListName="";
    protected List<PlayListItem> _listPlayListItems = new List<PlayListItem>();
   
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

    public PlayListItem this [int iItem]
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
    
    public abstract bool Load(string filename);

    public abstract void Save( string filename );
  }
}