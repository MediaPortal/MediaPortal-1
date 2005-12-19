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
using System.Collections.Generic;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Control which acts as a facade to the list,thumbnail and filmstrip view controls
  /// for the application it presents itself as 1 control but in reality it
  /// will route all actions to the current selected control (list,view, or filmstrip)
  /// </summary>
  public class GUIFacadeControl : GUIControl
  {
    // enum search kinds
    public enum SearchKinds
    {
      SEARCH_STARTS_WITH = 0,
      SEARCH_CONTAINS,
      SEARCH_ENDS_WITH,
      SEARCH_IS
    }

    //enum of all possible views
    public enum ViewMode
    {
      List,
      SmallIcons,
      LargeIcons,
      Filmstrip,
      AlbumView
    }
    GUIListControl m_ListView = null;			// instance of the list control
    GUIListControl m_ListAlbumView = null;	// instance of the album list control
    GUIThumbnailPanel m_ThumbnailView = null; // instance of the thumbnail control
    GUIFilmstripControl m_FilmStripView = null; // instance of the filmstrip control
    ViewMode m_ViewMode;						// current view
    List<GUIListItem> vecItems = new List<GUIListItem>(); // unfiltered itemlist

    public GUIFacadeControl(int dwParentID)
      : base(dwParentID)
    {
    }

    public GUIFacadeControl(int dwParentID, int dwControlId)
      : base(dwParentID, dwControlId, 0, 0, 0, 0)
    {
    }

    /// <summary>
    /// Property to get/set the list control
    /// </summary>
    public GUIListControl ListView
    {
      get { return m_ListView; }
      set
      {
        m_ListView = value;
        if (m_ListView != null)
        {
          m_ListView.GetID = GetID;
          m_ListView.WindowId = WindowId;
        }
      }
    }

    /// <summary>
    /// Property to get/set the list control
    /// </summary>
    public GUIListControl AlbumListView
    {
      get { return m_ListAlbumView; }
      set
      {
        m_ListAlbumView = value;
        if (m_ListAlbumView != null)
        {
          m_ListAlbumView.GetID = GetID;
          m_ListAlbumView.WindowId = WindowId;
        }
      }
    }

    /// <summary>
    /// Property to get/set the filmstrip control
    /// </summary>
    public GUIFilmstripControl FilmstripView
    {
      get { return m_FilmStripView; }
      set
      {
        m_FilmStripView = value;
        if (m_FilmStripView != null)
        {
          m_FilmStripView.GetID = GetID;
          m_FilmStripView.WindowId = WindowId;
        }
      }
    }

    /// <summary>
    /// Property to get/set the thumbnail control
    /// </summary>
    public GUIThumbnailPanel ThumbnailView
    {
      get { return m_ThumbnailView; }
      set
      {
        m_ThumbnailView = value;
        if (m_ThumbnailView != null)
        {
          m_ThumbnailView.GetID = GetID;
          m_ThumbnailView.WindowId = WindowId;
        }
      }
    }

    /// <summary>
    /// Property to get/set the current view mode
    /// </summary>
    public ViewMode View
    {
      get { return m_ViewMode; }
      set
      {
        m_ViewMode = value;
        UpdateView();
      }
    }

    /// <summary>
    /// Render. This will render the current selected view 
    /// </summary>
    public override void Render(float timePassed)
    {
      if (m_ViewMode == ViewMode.AlbumView && AlbumListView != null) AlbumListView.Render(timePassed);
      else if (m_ViewMode == ViewMode.List && m_ListView != null) m_ListView.Render(timePassed);
      else if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null) m_FilmStripView.Render(timePassed);
      else if (m_ThumbnailView != null) m_ThumbnailView.Render(timePassed);
    }

    /// <summary>
    /// Allocate any resources needed for the controls
    /// </summary>
    public override void AllocResources()
    {
      if (AlbumListView != null)
      {
        AlbumListView.AllocResources();
        AlbumListView.GetID = GetID;
        AlbumListView.WindowId = WindowId;
      }
      if (m_ListView != null)
      {
        m_ListView.AllocResources();
        m_ListView.GetID = GetID;
        m_ListView.WindowId = WindowId;
      }

      if (m_ThumbnailView != null)
      {
        m_ThumbnailView.AllocResources();
        m_ThumbnailView.GetID = GetID;
        m_ThumbnailView.WindowId = WindowId;
      }

      if (m_FilmStripView != null)
      {
        m_FilmStripView.AllocResources();
        m_FilmStripView.GetID = GetID;
        m_FilmStripView.WindowId = WindowId;
      }
    }
    public override int WindowId
    {
      get { return m_iWindowID; }
      set
      {
        m_iWindowID = value;
        if (AlbumListView != null) AlbumListView.WindowId = value;
        if (m_ListView != null) m_ListView.WindowId = value;
        if (m_ThumbnailView != null) m_ThumbnailView.WindowId = value;
        if (m_FilmStripView != null) m_FilmStripView.WindowId = value;
      }
    }

    /// <summary>
    /// pre-Allocate any resources needed for the controls
    /// </summary>
    public override void PreAllocResources()
    {
      if (AlbumListView != null) AlbumListView.PreAllocResources();
      if (m_ListView != null) m_ListView.PreAllocResources();
      if (m_ThumbnailView != null) m_ThumbnailView.PreAllocResources();
      if (m_FilmStripView != null) m_FilmStripView.PreAllocResources();
      UpdateView();
    }

    /// <summary>
    /// Free all resources of the controls
    /// </summary>
    public override void FreeResources()
    {
      if (AlbumListView != null) AlbumListView.FreeResources();
      if (m_ListView != null) m_ListView.FreeResources();
      if (m_ThumbnailView != null) m_ThumbnailView.FreeResources();
      if (m_FilmStripView != null) m_FilmStripView.FreeResources();
      vecItems.Clear();
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = -1;
      focused = false;
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
        return m_FilmStripView.HitTest(x, y, out controlID, out focused);
      else if (m_ViewMode == ViewMode.List && m_ListView != null)
        return m_ListView.HitTest(x, y, out controlID, out focused);
      else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
        return m_ThumbnailView.HitTest(x, y, out controlID, out focused);
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
        return m_ListAlbumView.HitTest(x, y, out controlID, out focused);
      return false;
    }

    public override void OnAction(Action action)
    {
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
        m_FilmStripView.OnAction(action);
      else if (m_ViewMode == ViewMode.List && m_ListView != null)
        m_ListView.OnAction(action);
      else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
        m_ThumbnailView.OnAction(action);
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
        m_ListAlbumView.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          GUIListItem pItem = (GUIListItem)message.Object;
          Add(pItem);
          return true;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          RouteMessage(message);
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          RouteMessage(message);
          vecItems.Clear();
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          RouteMessage(message);
          return true;
        }
      }
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
        return m_FilmStripView.OnMessage(message);
      if (m_ViewMode == ViewMode.List && m_ListView != null)
        return m_ListView.OnMessage(message);
      else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
        return m_ThumbnailView.OnMessage(message);
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
        return m_ListAlbumView.OnMessage(message);
      return false;
    }

    void RouteMessage(GUIMessage message)
    {
      if (m_ListAlbumView != null) m_ListAlbumView.OnMessage(message);
      if (m_ListView != null) m_ListView.OnMessage(message);
      if (m_FilmStripView != null) m_FilmStripView.OnMessage(message);
      if (m_ThumbnailView != null) m_ThumbnailView.OnMessage(message);
    }

    public override bool CanFocus()
    {
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
        return m_FilmStripView.CanFocus();
      if (m_ViewMode == ViewMode.List && m_ListView != null)
        return m_ListView.CanFocus();
      else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
        m_ThumbnailView.CanFocus();
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
        m_ListAlbumView.CanFocus();
      return true;
    }

    public override bool Focus
    {
      get
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          return m_FilmStripView.Focus;
        else if (m_ViewMode == ViewMode.List && m_ListView != null)
          return m_ListView.Focus;
        else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          return m_ThumbnailView.Focus;
        else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          return m_ListAlbumView.Focus;
        return false;
      }
      set
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          m_FilmStripView.Focus = value;
        else if (m_ViewMode == ViewMode.List && m_ListView != null)
          m_ListView.Focus = value;
        else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          m_ThumbnailView.Focus = value;
        else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          m_ListAlbumView.Focus = value;
      }
    }

    public override bool InControl(int x, int y, out int controlID)
    {
      controlID = -1;
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
        return m_FilmStripView.InControl(x, y, out controlID);
      else if (m_ViewMode == ViewMode.List && m_ListView != null)
        return m_ListView.InControl(x, y, out controlID);
      else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
        return m_ThumbnailView.InControl(x, y, out controlID);
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
        return m_ListAlbumView.InControl(x, y, out controlID);
      return false;
    }

    public void Sort(System.Collections.Generic.IComparer<GUIListItem> comparer)
    {
      if (m_ListAlbumView != null) m_ListAlbumView.Sort(comparer);
      if (m_ListView != null) m_ListView.Sort(comparer);
      if (m_ThumbnailView != null) m_ThumbnailView.Sort(comparer);
      if (m_FilmStripView != null) m_FilmStripView.Sort(comparer);
      try
      {
        vecItems.Sort(comparer);
      }
      catch (Exception) { }
    }

    public void Add(GUIListItem item)
    {
      if (item == null) return;
      if (m_ListAlbumView != null) m_ListAlbumView.Add(item);
      if (m_ListView != null) m_ListView.Add(item);
      if (m_ThumbnailView != null) m_ThumbnailView.Add(item);
      if (m_FilmStripView != null) m_FilmStripView.Add(item);
      vecItems.Add(item);
    }

    public void Filter(int searchKind, string searchString)
    {
      if (m_ListAlbumView != null) m_ListAlbumView.Clear();
      if (m_ListView != null) m_ListView.Clear();
      if (m_ThumbnailView != null) m_ThumbnailView.Clear();
      if (m_FilmStripView != null) m_FilmStripView.Clear();

      if (searchString != "")
      {
        // Set active selection
        GUIListItem dirUp = new GUIListItem("..");
        dirUp.Path = searchString;
        dirUp.ItemId = searchKind;
        dirUp.IsFolder = true;
        dirUp.ThumbnailImage = String.Empty;
        dirUp.IconImage = "defaultFolderBack.png";
        dirUp.IconImageBig = "defaultFolderBackBig.png";
        if (m_ListAlbumView != null) m_ListAlbumView.Add(dirUp);
        if (m_ListView != null) m_ListView.Add(dirUp);
        if (m_ThumbnailView != null) m_ThumbnailView.Add(dirUp);
        if (m_FilmStripView != null) m_FilmStripView.Add(dirUp);
      }

      int iTotalItems = 0;
      bool validItem = false;
      for (int i = 0; i < vecItems.Count; i++)
      {
        validItem = false;
        GUIListItem item = vecItems[i];
        switch ((SearchKinds)searchKind)
        {
          case SearchKinds.SEARCH_STARTS_WITH:
            if (item.Label.ToLower().StartsWith(searchString.ToLower())) validItem = true;
            break;
          case SearchKinds.SEARCH_CONTAINS:
            if (item.Label.ToLower().IndexOf(searchString.ToLower()) >= 0) validItem = true;
            break;
          case SearchKinds.SEARCH_ENDS_WITH:
            if (item.Label.ToLower().EndsWith(searchString.ToLower())) validItem = true;
            break;
          case SearchKinds.SEARCH_IS:
            if (item.Label.ToLower().Equals(searchString.ToLower())) validItem = true;
            break;
        }

        if (validItem || searchString == "")
        {
          if (m_ListAlbumView != null) m_ListAlbumView.Add(item);
          if (m_ListView != null) m_ListView.Add(item);
          if (m_ThumbnailView != null) m_ThumbnailView.Add(item);
          if (m_FilmStripView != null) m_FilmStripView.Add(item);
          iTotalItems++;
        }
      }
      string strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", strObjects);
    }

    void UpdateView()
    {
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
      {
        base.XPosition = m_FilmStripView.XPosition;
        base.YPosition = m_FilmStripView.YPosition;
        base.Width = m_FilmStripView.Width;
        base.Height = m_FilmStripView.Height;
        m_FilmStripView.IsVisible = true;
        if (m_ListView != null) m_ListView.IsVisible = false;
        if (m_ThumbnailView != null) m_ThumbnailView.IsVisible = false;
        if (m_ListAlbumView != null) m_ListAlbumView.IsVisible = false;
      }
      else if (m_ViewMode == ViewMode.List && m_ListView != null)
      {
        base.XPosition = m_ListView.XPosition;
        base.YPosition = m_ListView.YPosition;
        base.Width = m_ListView.Width;
        base.Height = m_ListView.Height;
        m_ListView.IsVisible = true;
        if (m_ThumbnailView != null) m_ThumbnailView.IsVisible = false;
        if (m_FilmStripView != null) m_FilmStripView.IsVisible = false;
        if (m_ListAlbumView != null) m_ListAlbumView.IsVisible = false;
      }
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
      {
        base.XPosition = m_ListAlbumView.XPosition;
        base.YPosition = m_ListAlbumView.YPosition;
        base.Width = m_ListAlbumView.Width;
        base.Height = m_ListAlbumView.Height;
        m_ListAlbumView.IsVisible = true;
        if (m_ListView != null) m_ListView.IsVisible = false;
        if (m_ThumbnailView != null) m_ThumbnailView.IsVisible = false;
        if (m_FilmStripView != null) m_FilmStripView.IsVisible = false;
      }
      else if (m_ThumbnailView != null)
      {
        base.XPosition = m_ThumbnailView.XPosition;
        base.YPosition = m_ThumbnailView.YPosition;
        base.Width = m_ThumbnailView.Width;
        base.Height = m_ThumbnailView.Height;
        m_ThumbnailView.ShowBigIcons(m_ViewMode == ViewMode.LargeIcons);
        m_ThumbnailView.IsVisible = true;
        if (m_ListView != null) m_ListView.IsVisible = false;
        if (m_FilmStripView != null) m_FilmStripView.IsVisible = false;
        if (m_ListAlbumView != null) m_ListAlbumView.IsVisible = false;
      }
    }

    public override void DoUpdate()
    {
      if (m_ViewMode == ViewMode.List && m_ListView != null)
      {
        m_ListView.XPosition = XPosition;
        m_ListView.YPosition = YPosition;
        m_ListView.Width = Width;
        m_ListView.Height = Height;
        m_ListView.DoUpdate();
      }
      if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
      {
        m_ListAlbumView.XPosition = XPosition;
        m_ListAlbumView.YPosition = YPosition;
        m_ListAlbumView.Width = Width;
        m_ListAlbumView.Height = Height;
        m_ListAlbumView.DoUpdate();
      }


      if ((m_ViewMode == ViewMode.LargeIcons || m_ViewMode == ViewMode.SmallIcons) && m_ThumbnailView != null)
      {
        m_ThumbnailView.XPosition = XPosition;
        m_ThumbnailView.YPosition = YPosition;
        m_ThumbnailView.Width = Width;
        m_ThumbnailView.Height = Height;
        m_ThumbnailView.DoUpdate();
      }


      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
      {
        m_FilmStripView.XPosition = XPosition;
        m_FilmStripView.YPosition = YPosition;
        m_FilmStripView.Width = Width;
        m_FilmStripView.Height = Height;
        m_FilmStripView.DoUpdate();
      }
    }

    public override void StorePosition()
    {
      if (m_ListAlbumView != null) m_ListAlbumView.StorePosition();
      if (m_ListView != null) m_ListView.StorePosition();
      if (m_ThumbnailView != null) m_ThumbnailView.StorePosition();
      if (m_FilmStripView != null) m_FilmStripView.StorePosition();

      base.StorePosition();
    }

    public override void ReStorePosition()
    {
      if (m_ListAlbumView != null) m_ListAlbumView.ReStorePosition();
      if (m_ListView != null) m_ListView.ReStorePosition();
      if (m_ThumbnailView != null) m_ThumbnailView.ReStorePosition();
      if (m_FilmStripView != null) m_FilmStripView.ReStorePosition();

      base.ReStorePosition();
    }

    public override void Animate(float timePassed, Animator animator)
    {
      if (m_ListAlbumView != null) m_ListAlbumView.Animate(timePassed, animator);
      if (m_ListView != null) m_ListView.Animate(timePassed, animator);
      if (m_ThumbnailView != null) m_ThumbnailView.Animate(timePassed, animator);
      if (m_FilmStripView != null) m_FilmStripView.Animate(timePassed, animator);
      base.Animate(timePassed, animator);
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          return m_FilmStripView.SelectedListItem;
        else if (m_ViewMode == ViewMode.List && m_ListView != null)
          return m_ListView.SelectedListItem;
        else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          return m_ThumbnailView.SelectedListItem;
        else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          return m_ListAlbumView.SelectedListItem;
        return null;
      }
    }

    public int Count
    {
      get
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          return m_FilmStripView.Count;
        else if (m_ViewMode == ViewMode.List && m_ListView != null)
          return m_ListView.Count;
        else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          return m_ThumbnailView.Count;
        else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          return m_ListAlbumView.Count;
        return 0;
      }
    }

    public void Clear()
    {
      if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
        m_FilmStripView.Clear();
      else if (m_ViewMode == ViewMode.List && m_ListView != null)
        m_ListView.Clear();
      else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
        m_ThumbnailView.Clear();
      else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
        m_ListAlbumView.Clear();
      vecItems.Clear();
    }
    public int SelectedListItemIndex
    {
      get
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          return m_FilmStripView.SelectedListItemIndex;
        else if (m_ViewMode == ViewMode.List && m_ListView != null)
          return m_ListView.SelectedListItemIndex;
        else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          return m_ThumbnailView.SelectedListItemIndex;
        else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          return m_ListAlbumView.SelectedListItemIndex;
        return -1;
      }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          return m_FilmStripView[index];
        else if (m_ViewMode == ViewMode.List && m_ListView != null)
          return m_ListView[index];
        else if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          return m_ThumbnailView[index];
        else if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          return m_ListAlbumView[index];
        return null;
      }
    }
    public void RefreshCoverArt()
    {
      //GUIListItem item;
      for (int i = 0; i < Count; ++i)
      {
        if (m_ViewMode == ViewMode.Filmstrip && m_FilmStripView != null)
          m_FilmStripView[i].RefreshCoverArt();
        if (m_ViewMode == ViewMode.List && m_ListView != null)
          m_ListView[i].RefreshCoverArt();
        if ((m_ViewMode == ViewMode.SmallIcons || m_ViewMode == ViewMode.LargeIcons) && m_ThumbnailView != null)
          m_ThumbnailView[i].RefreshCoverArt();
        if (m_ViewMode == ViewMode.AlbumView && m_ListAlbumView != null)
          m_ListAlbumView[i].RefreshCoverArt();
      }
    }
  }
}
