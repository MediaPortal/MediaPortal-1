#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.TagReader;

namespace MediaPortal.Playlists
{
  [Serializable()]
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

    protected string _fileName = "";
    protected string _description = "";
    protected int _duration = 0;
    protected object _musicTag = null;
    bool _isPlayed = false;
    PlayListItemType _itemType = PlayListItemType.Unknown;

    public PlayListItem()
    {
    }

    public PlayListItem(string description, string fileName)
      : this(description, fileName, 0)
    {
    }

    public PlayListItem(string description, string fileName, int duration)
    {
      if (description == null)
        return;
      if (fileName == null)
        return;
      _description = description;
      _fileName = fileName;
      _duration = duration;
    }

    public PlayListItem.PlayListItemType Type
    {
      get { return _itemType; }
      set { _itemType = value; }
    }

    public virtual string FileName
    {
      get { return _fileName; }
      set
      {
        if (value == null)
          return;
        _fileName = value;
      }
    }

    public string Description
    {
      get { return _description; }
      set
      {
        if (value == null)
          return;
        _description = value;
      }
    }

    public int Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    public bool Played
    {
      get { return _isPlayed; }
      set { _isPlayed = value; }
    }

    public object MusicTag
    {
      get { return _musicTag; }
      set { _musicTag = value; }
    }
  };

}