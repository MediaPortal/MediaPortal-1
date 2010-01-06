#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;

namespace MediaPortal.Util
{
  public class DirectoryHistory
  {
    private class DirectoryItem
    {
      private string m_strItem = string.Empty;
      private string m_strDir = string.Empty;
      public DirectoryItem() {}

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

    private ArrayList m_history = new ArrayList();
    public DirectoryHistory() {}

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