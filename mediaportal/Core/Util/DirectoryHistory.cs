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
using System.Collections;

namespace MediaPortal.Util
{
  public class DirectoryHistory
  {
    class DirectoryItem
    {
      string m_strItem = string.Empty;
      string m_strDir = string.Empty;
      public DirectoryItem()
      {
      }
      public DirectoryItem(string strItem, string strDir)
      {
        if (strItem == null || strDir == null) return;
        m_strItem = strItem;
        m_strDir = strDir;
      }
      public string Item
      {
        get { return m_strItem; }
        set
        {
          if (value == null) return;
          m_strItem = value;
        }
      }
      public string Dir
      {
        get { return m_strDir; }
        set
        {
          if (value == null) return;
          m_strDir = value;
        }
      }
    }

    ArrayList m_history = new ArrayList();
    public DirectoryHistory()
    {
    }

    public string Get(string strDir)
    {
      if (strDir == null) return string.Empty;
      foreach (DirectoryItem item in m_history)
      {
        if (item.Dir == strDir)
        {
          return item.Item;
        }
      }
      return string.Empty;
    }

    public void Set(string strItem, string strDir)
    {
      if (strItem == null) return;
      if (strDir == null) return;
      foreach (DirectoryItem item in m_history)
      {
        if (item.Dir == strDir)
        {
          item.Item = strItem;
          return;
        }
      }
      DirectoryItem newItem = new DirectoryItem(strItem, strDir);
      m_history.Add(newItem);
    }
  }
}