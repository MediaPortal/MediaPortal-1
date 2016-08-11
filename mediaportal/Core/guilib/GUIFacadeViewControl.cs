#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Threading;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Control which acts as a facade to the list,thumbnail, filmstrip, and coverflow layout controls
  /// for the application it presents itself as 1 control but in reality it
  /// will route all actions to the current selected control (list, icons, filmstrip, or coverflow)
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

    //enum of all possible layouts
    public enum Layout
    {
      List = 0,
      SmallIcons = 1,
      LargeIcons = 2,
      Filmstrip = 3,
      AlbumView = 4,
      Playlist = 5,
      CoverFlow = 6
    }

    private GUIPlayListItemListControl _layoutPlayList = null; // instance of a re-orderable playlist list control
    private GUIListControl _layoutList = null; // instance of the list control

    private GUIListControl _layoutAlbum = null; // instance of the album list control
    private GUIThumbnailPanel _layoutThumbnail = null; // instance of the thumbnail control
    private GUIFilmstripControl _layoutFilmStrip = null; // instance of the filmstrip control
    private GUICoverFlow _layoutCoverFlow = null; // instance of the coverflow control
    private Layout _currentLayout; // current layout
    private List<GUIListItem> _itemList = new List<GUIListItem>(); // unfiltered itemlist

    public GUIFacadeControl(int dwParentID)
      : base(dwParentID) {}

    public GUIFacadeControl(int dwParentID, int dwControlId)
      : base(dwParentID, dwControlId, 0, 0, 0, 0) {}

    /////<summary>
    ///// Property to get/set the type if listview that will be displayed; GUIListControl or GUIPlayListItemListControl
    /////</summary>
    //  public ListType ListViewType
    //  {
    //      get{return _listViewType;}
    //      set{_listViewType = value;}
    //  }

    protected void InitControl(GUIControl cntl)
    {
      if (cntl != null)
      {
        cntl.GetID = GetID;
        cntl.WindowId = WindowId;
        cntl.ParentControl = this;
        if (base.Animations.Count != 0)
        {
          cntl.Animations.AddRange(base.Animations);
        }
      }
    }

    /// <summary>
    /// Property to get/set the list control
    /// </summary>
    public GUIListControl ListLayout
    {
      get { return _layoutList; }
      set
      {
        _layoutList = value;
        InitControl(_layoutList);
      }
    }

    /// <summary>
    /// Property to get/set the playlist list control
    /// </summary>
    public GUIPlayListItemListControl PlayListLayout
    {
      get { return _layoutPlayList; }
      set
      {
        _layoutPlayList = value;
        InitControl(_layoutPlayList);
      }
    }

    /// <summary>
    /// Property to get/set the list control
    /// </summary>
    public GUIListControl AlbumListLayout
    {
      get { return _layoutAlbum; }
      set
      {
        _layoutAlbum = value;
        InitControl(_layoutAlbum);
      }
    }

    /// <summary>
    /// Property to get/set the filmstrip control
    /// </summary>
    public GUIFilmstripControl FilmstripLayout
    {
      get { return _layoutFilmStrip; }
      set
      {
        _layoutFilmStrip = value;
        InitControl(_layoutFilmStrip);
      }
    }

    /// <summary>
    /// Property to get/set the coverflow control
    /// </summary>
    public GUICoverFlow CoverFlowLayout
    {
      get { return _layoutCoverFlow; }
      set
      {
        _layoutCoverFlow = value;
        InitControl(_layoutCoverFlow);
      }
    }

    /// <summary>
    /// Property to get/set the thumbnail control
    /// </summary>
    public GUIThumbnailPanel ThumbnailLayout
    {
      get { return _layoutThumbnail; }
      set
      {
        _layoutThumbnail = value;
        InitControl(_layoutThumbnail);
      }
    }

    /// <summary>
    /// Property to get/set the current layout mode
    /// </summary>
    public Layout CurrentLayout
    {
      get { return _currentLayout; }
      set
      {
        if (_currentLayout != value)
        {
          _currentLayout = value;
          GUIControl ctl = LayoutControl;
          if (ctl != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESH, 0, 0, ctl.GetID, 0, 0, null);
            ctl.OnMessage(msg);
          }
        }
        // Set the layout Property
        SetLayoutProperty();
        UpdateLayout();
      }
    }

    /// <summary>
    /// Property to get the current layout 
    /// </summary>
    public GUIControl LayoutControl
    {
      get
      {
        switch (_currentLayout)
        {
          case Layout.AlbumView:
            return AlbumListLayout;
          case Layout.List:
            return _layoutList;
          case Layout.Filmstrip:
            return _layoutFilmStrip;
          case Layout.CoverFlow:
            return _layoutCoverFlow;
          case Layout.Playlist:
            return _layoutPlayList;
          case Layout.LargeIcons:
          case Layout.SmallIcons:
            return _layoutThumbnail;
        }
        return null;
      }
    }

    public bool EnableSMSsearch
    {
      get
      {
        if (_layoutList != null) return _layoutList.EnableSMSsearch;
        if (_layoutAlbum != null) return _layoutAlbum.EnableSMSsearch;
        if (_layoutFilmStrip != null) return _layoutFilmStrip.EnableSMSsearch;
        if (_layoutPlayList != null) return _layoutPlayList.EnableSMSsearch;
        if (_layoutThumbnail != null) return _layoutThumbnail.EnableSMSsearch;
        if (_layoutCoverFlow != null) return _layoutCoverFlow.EnableSMSsearch;
        return false;
      }
      set
      {
        if (_layoutList != null) _layoutList.EnableSMSsearch = value;
        if (_layoutAlbum != null) _layoutAlbum.EnableSMSsearch = value;
        if (_layoutFilmStrip != null) _layoutFilmStrip.EnableSMSsearch = value;
        if (_layoutPlayList != null) _layoutPlayList.EnableSMSsearch = value;
        if (_layoutThumbnail != null) _layoutThumbnail.EnableSMSsearch = value;
        if (_layoutCoverFlow != null) _layoutCoverFlow.EnableSMSsearch = value;
      }
    }

    public bool EnableScrollLabel
    {
      get
      {
        if (_layoutList != null) return _layoutList.EnableScrollLabel;
        return false;
      }
      set { if (_layoutList != null) _layoutList.EnableScrollLabel = value; }
    }

    public override void AddAnimations(List<VisualEffect> animations)
    {
      //base.AddAnimations(animations);
      if (_layoutList != null)
      {
        _layoutList.AddAnimations(animations);
      }
      if (_layoutAlbum != null)
      {
        _layoutAlbum.AddAnimations(animations);
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.AddAnimations(animations);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.AddAnimations(animations);
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.AddAnimations(animations);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.AddAnimations(animations);
      }
    }

    /// <summary>
    /// Render. This will render the current selected layout 
    /// </summary>
    public override void Render(float timePassed)
    {
      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        //uint currentTime = (uint) (DXUtil.Timer(DirectXTimer.GetAbsoluteTime)*1000.0);
        uint currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;
        if (GUIGraphicsContext.Animations)
        {
          cntl.UpdateVisibility();
          cntl.DoRender(timePassed, currentTime);
        }
        else
        {
          cntl.Render(timePassed);
        }
      }
      base.Render(timePassed);
    }

    /// <summary>
    /// Allocate any resources needed for the controls
    /// </summary>
    public override void AllocResources()
    {
      if (AlbumListLayout != null)
      {
        AlbumListLayout.AllocResources();
        AlbumListLayout.GetID = GetID;
        AlbumListLayout.WindowId = WindowId;
        AlbumListLayout.ParentControl = this;
      }
      if (_layoutList != null)
      {
        _layoutList.AllocResources();
        _layoutList.GetID = GetID;
        _layoutList.WindowId = WindowId;
        _layoutList.ParentControl = this;
      }

      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.AllocResources();
        _layoutThumbnail.GetID = GetID;
        _layoutThumbnail.WindowId = WindowId;
        _layoutThumbnail.ParentControl = this;
      }

      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.AllocResources();
        _layoutFilmStrip.GetID = GetID;
        _layoutFilmStrip.WindowId = WindowId;
        _layoutFilmStrip.ParentControl = this;
      }

      if (_layoutPlayList != null)
      {
        _layoutPlayList.AllocResources();
        _layoutPlayList.GetID = GetID;
        _layoutPlayList.WindowId = WindowId;
        _layoutPlayList.ParentControl = this;
      }

      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.AllocResources();
        _layoutCoverFlow.GetID = GetID;
        _layoutCoverFlow.WindowId = WindowId;
        _layoutCoverFlow.ParentControl = this;
      }
    }

    public override int WindowId
    {
      get { return _windowId; }
      set
      {
        _windowId = value;
        if (AlbumListLayout != null)
        {
          AlbumListLayout.WindowId = value;
        }
        if (_layoutList != null)
        {
          _layoutList.WindowId = value;
        }
        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.WindowId = value;
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.WindowId = value;
        }
        if (_layoutPlayList != null)
        {
          _layoutPlayList.WindowId = value;
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.WindowId = value;
        }
      }
    }

    /// <summary>
    /// pre-Allocate any resources needed for the controls
    /// </summary>
    public override void PreAllocResources()
    {
      if (AlbumListLayout != null)
      {
        AlbumListLayout.PreAllocResources();
      }
      if (_layoutList != null)
      {
        _layoutList.PreAllocResources();
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.PreAllocResources();
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.PreAllocResources();
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.PreAllocResources();
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.PreAllocResources();
      }
      UpdateLayout();
    }

    /// <summary>
    /// Free all resources of the controls
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();

      _layoutAlbum.SafeDispose();
      _layoutList.SafeDispose();
      _layoutThumbnail.SafeDispose();
      _layoutFilmStrip.SafeDispose();
      _layoutCoverFlow.SafeDispose();
      _layoutPlayList.SafeDispose();
      _itemList.DisposeAndClear();
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = -1;
      focused = false;
      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        return cntl.HitTest(x, y, out controlID, out focused);
      }
      return false;
    }

    public override void OnAction(Action action)
    {
      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        cntl.OnAction(action);
      }
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
          _itemList.DisposeAndClear();
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          RouteMessage(message);
          return true;
        }
      }
      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
      {
        RouteMessage(message);
        return true;
      }

      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        return cntl.OnMessage(message);
      }

      return false;
    }

    private void RouteMessage(GUIMessage message)
    {
      if (_layoutAlbum != null)
      {
        _layoutAlbum.OnMessage(message);
      }
      if (_layoutList != null)
      {
        _layoutList.OnMessage(message);
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.OnMessage(message);
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.OnMessage(message);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.OnMessage(message);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.OnMessage(message);
      }
    }

    public override bool CanFocus()
    {
      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        return cntl.CanFocus();
      }

      return true;
    }

    public override bool Focus
    {
      get
      {
        GUIControl cntl = LayoutControl;
        if (cntl != null)
        {
          return cntl.Focus;
        }
        return false;
      }
      set
      {
        GUIControl cntl = LayoutControl;
        if (cntl != null)
        {
          cntl.Focus = value;
        }
      }
    }

    public override bool InControl(int x, int y, out int controlID)
    {
      controlID = -1;
      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        return cntl.InControl(x, y, out controlID);
      }
      return false;
    }

    public void Sort(IComparer<GUIListItem> comparer)
    {
      var preSort = new List<GUIListItem>(_itemList);
      try
      {
        _itemList.Sort(comparer);
      }
      catch (Exception) {}
      if (_layoutAlbum != null)
      {
        if (HasSameItems(_layoutAlbum.ListItems, preSort))
        {
          if (_layoutAlbum.ListItems != _itemList) //if same instance of list, nothing to do except refresh
          {
            _layoutAlbum.ListItems.Clear();
            _layoutAlbum.ListItems.AddRange(_itemList);
          }
          _layoutAlbum.SetNeedRefresh();
        }
        else
          _layoutAlbum.Sort(comparer);
      }
      if (_layoutList != null)
      {
        if (HasSameItems(_layoutList.ListItems, preSort))
        {
          if (_layoutList.ListItems != _itemList)
          {
            _layoutList.ListItems.Clear();
            _layoutList.ListItems.AddRange(_itemList);
          }
          _layoutList.SetNeedRefresh();
        }
        else
          _layoutList.Sort(comparer);
      }
      if (_layoutThumbnail != null)
      {
        if (HasSameItems(_layoutThumbnail.ListItems, preSort))
        {
          if (_layoutThumbnail.ListItems != _itemList)
          {
            _layoutThumbnail.ListItems.Clear();
            _layoutThumbnail.ListItems.AddRange(_itemList);
          }
          _layoutThumbnail.SetNeedRefresh();
        }
        else
          _layoutThumbnail.Sort(comparer);
      }
      if (_layoutFilmStrip != null)
      {
        if (HasSameItems(_layoutFilmStrip.ListItems, preSort))
        {
          if (_layoutFilmStrip.ListItems != _itemList)
          {
            _layoutFilmStrip.ListItems.Clear();
            _layoutFilmStrip.ListItems.AddRange(_itemList);
          }
          _layoutFilmStrip.SetNeedRefresh();
        }
        else
          _layoutFilmStrip.Sort(comparer);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.Sort(comparer);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.Sort(comparer);
      }
    }

    public void Add(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      if (_layoutAlbum != null)
      {
        _layoutAlbum.Add(item);
      }
      if (_layoutList != null)
      {
        _layoutList.Add(item);
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.Add(item);
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.Add(item);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.Add(item);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.Add(item);
      }
      _itemList.Add(item);
    }

    public void Insert(int index, GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
      if (_layoutAlbum != null)
      {
        _layoutAlbum.Insert(index, item);
      }
      if (_layoutList != null)
      {
        _layoutList.Insert(index, item);
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.Insert(index, item);
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.Insert(index, item);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.Insert(index, item);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.Insert(index, item);
      }
      _itemList.Insert(index, item);
    }

    public void Filter(int searchKind, string searchString)
    {
      if (_layoutAlbum != null)
      {
        _layoutAlbum.Clear();
      }
      if (_layoutList != null)
      {
        _layoutList.Clear();
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.Clear();
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.Clear();
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.Clear();
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.Clear();
      }
      if (searchString != "")
      {
        // Set active selection
        GUIListItem dirUp = new GUIListItem("..");
        dirUp.Path = searchString;
        dirUp.ItemId = searchKind;
        dirUp.IsFolder = true;
        dirUp.ThumbnailImage = string.Empty;
        dirUp.IconImage = "defaultFolderBack.png";
        dirUp.IconImageBig = "defaultFolderBackBig.png";
        if (_layoutAlbum != null)
        {
          _layoutAlbum.Add(dirUp);
        }
        if (_layoutList != null)
        {
          _layoutList.Add(dirUp);
        }
        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.Add(dirUp);
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.Add(dirUp);
        }
        if (_layoutPlayList != null)
        {
          _layoutPlayList.Add(dirUp);
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.Add(dirUp);
        }
      }

      int iTotalItems = 0;
      bool validItem = false;
      for (int i = 0; i < _itemList.Count; i++)
      {
        validItem = false;
        GUIListItem item = _itemList[i];
        switch ((SearchKinds)searchKind)
        {
          case SearchKinds.SEARCH_STARTS_WITH:
            if (item.Label.ToLowerInvariant().StartsWith(searchString.ToLowerInvariant()))
            {
              validItem = true;
            }
            break;
          case SearchKinds.SEARCH_CONTAINS:
            if (item.Label.ToLowerInvariant().IndexOf(searchString.ToLowerInvariant()) >= 0)
            {
              validItem = true;
            }
            break;
          case SearchKinds.SEARCH_ENDS_WITH:
            if (item.Label.ToLowerInvariant().EndsWith(searchString.ToLowerInvariant()))
            {
              validItem = true;
            }
            break;
          case SearchKinds.SEARCH_IS:
            if (item.Label.ToLowerInvariant().Equals(searchString.ToLowerInvariant()))
            {
              validItem = true;
            }
            break;
        }

        if (validItem || searchString == "")
        {
          if (_layoutAlbum != null)
          {
            _layoutAlbum.Add(item);
          }
          if (_layoutList != null)
          {
            _layoutList.Add(item);
          }
          if (_layoutThumbnail != null)
          {
            _layoutThumbnail.Add(item);
          }
          if (_layoutFilmStrip != null)
          {
            _layoutFilmStrip.Add(item);
          }
          if (_layoutPlayList != null)
          {
            _layoutPlayList.Add(item);
          }
          if (_layoutCoverFlow != null)
          {
            _layoutCoverFlow.Add(item);
          }
          iTotalItems++;
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", MediaPortal.Util.Utils.GetObjectCountLabel(iTotalItems));
    }

    private new void UpdateLayout()
    {
      if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
      {
        base.XPosition = _layoutFilmStrip.XPosition;
        base.YPosition = _layoutFilmStrip.YPosition;
        base.Width = _layoutFilmStrip.Width;
        base.Height = _layoutFilmStrip.Height;
        _layoutFilmStrip.IsVisible = true;
        if (_layoutList != null)
        {
          _layoutList.IsVisible = false;
        }
        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.IsVisible = false;
        }
        if (_layoutAlbum != null)
        {
          _layoutAlbum.IsVisible = false;
        }
        if (_layoutPlayList != null)
        {
          _layoutPlayList.IsVisible = false;
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.IsVisible = false;
        }
      }
      else if (_currentLayout == Layout.List && _layoutList != null)
      {
        base.XPosition = _layoutList.XPosition;
        base.YPosition = _layoutList.YPosition;
        base.Width = _layoutList.Width;
        base.Height = _layoutList.Height;
        _layoutList.IsVisible = true;
        if (_layoutPlayList != null)
        {
          _layoutPlayList.IsVisible = false;
        }
        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.IsVisible = false;
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.IsVisible = false;
        }
        if (_layoutAlbum != null)
        {
          _layoutAlbum.IsVisible = false;
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.IsVisible = false;
        }
      }
      else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
      {
        base.XPosition = _layoutPlayList.XPosition;
        base.YPosition = _layoutPlayList.YPosition;
        base.Width = _layoutPlayList.Width;
        base.Height = _layoutPlayList.Height;
        if (_layoutList != null)
        {
          _layoutList.IsVisible = false;
        }
        _layoutPlayList.IsVisible = true;

        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.IsVisible = false;
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.IsVisible = false;
        }
        if (_layoutAlbum != null)
        {
          _layoutAlbum.IsVisible = false;
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.IsVisible = false;
        }
      }
      else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
      {
        base.XPosition = _layoutAlbum.XPosition;
        base.YPosition = _layoutAlbum.YPosition;
        base.Width = _layoutAlbum.Width;
        base.Height = _layoutAlbum.Height;
        _layoutAlbum.IsVisible = true;
        if (_layoutList != null)
        {
          _layoutList.IsVisible = false;
        }
        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.IsVisible = false;
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.IsVisible = false;
        }
        if (_layoutPlayList != null)
        {
          _layoutPlayList.IsVisible = false;
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.IsVisible = false;
        }
      }
      else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
      {
        base.XPosition = _layoutCoverFlow.XPosition;
        base.YPosition = _layoutCoverFlow.YPosition;
        base.Width = _layoutCoverFlow.Width;
        base.Height = _layoutCoverFlow.Height;
        _layoutCoverFlow.IsVisible = true;
        if (_layoutList != null)
        {
          _layoutList.IsVisible = false;
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.IsVisible = false;
        }
        if (_layoutThumbnail != null)
        {
          _layoutThumbnail.IsVisible = false;
        }
        if (_layoutAlbum != null)
        {
          _layoutAlbum.IsVisible = false;
        }
        if (_layoutPlayList != null)
        {
          _layoutPlayList.IsVisible = false;
        }
      }
      else if (_layoutThumbnail != null)
      {
        base.XPosition = _layoutThumbnail.XPosition;
        base.YPosition = _layoutThumbnail.YPosition;
        base.Width = _layoutThumbnail.Width;
        base.Height = _layoutThumbnail.Height;
        _layoutThumbnail.ShowBigIcons(_currentLayout == Layout.LargeIcons);
        _layoutThumbnail.IsVisible = true;
        if (_layoutList != null)
        {
          _layoutList.IsVisible = false;
        }
        if (_layoutFilmStrip != null)
        {
          _layoutFilmStrip.IsVisible = false;
        }
        if (_layoutAlbum != null)
        {
          _layoutAlbum.IsVisible = false;
        }
        if (_layoutPlayList != null)
        {
          _layoutPlayList.IsVisible = false;
        }
        if (_layoutCoverFlow != null)
        {
          _layoutCoverFlow.IsVisible = false;
        }
      }
    }

    private void SetLayoutProperty()
    {
      switch (_currentLayout)
      {
        case Layout.AlbumView:
          GUIPropertyManager.SetProperty("#facadeview.layout", "album");
          break;
        case Layout.Filmstrip:
          GUIPropertyManager.SetProperty("#facadeview.layout", "filmstrip");
          break;
        case Layout.LargeIcons:
          GUIPropertyManager.SetProperty("#facadeview.layout", "largeicons");
          break;
        case Layout.List:
          GUIPropertyManager.SetProperty("#facadeview.layout", "list");
          break;
        case Layout.Playlist:
          GUIPropertyManager.SetProperty("#facadeview.layout", "playlist");
          break;
        case Layout.SmallIcons:
          GUIPropertyManager.SetProperty("#facadeview.layout", "smallicons");
          break;
        case Layout.CoverFlow:
          GUIPropertyManager.SetProperty("#facadeview.layout", "coverflow");
          break;
      }
    }

    public static string GetLayoutLocalizedName(Layout layout)
    {
      switch (layout)
      {
        case Layout.List:
          return GUILocalizeStrings.Get(101);
        case Layout.SmallIcons:
          return GUILocalizeStrings.Get(100);
        case Layout.LargeIcons:
          return GUILocalizeStrings.Get(417);
        case Layout.AlbumView:
          return GUILocalizeStrings.Get(529);
        case Layout.Filmstrip:
          return GUILocalizeStrings.Get(733);
        case Layout.Playlist:
          return GUILocalizeStrings.Get(101);
        case Layout.CoverFlow:
          return GUILocalizeStrings.Get(791);
        default:
          return string.Empty;
      }
    }

    public bool IsNullLayout(Layout layout)
    {
      switch (layout)
      {
        case Layout.List:
          return (ListLayout == null);
        case Layout.SmallIcons:
        case Layout.LargeIcons:
          return (ThumbnailLayout == null);
        case Layout.AlbumView:
          return (AlbumListLayout == null);
        case Layout.CoverFlow:
          return (CoverFlowLayout == null);
        case Layout.Filmstrip:
          return (FilmstripLayout == null);
        case Layout.Playlist:
          return (PlayListLayout == null);
        default:
          return true;
      }
    }

    public override void DoUpdate()
    {
      GUIControl cntl = LayoutControl;
      if (cntl != null)
      {
        cntl.XPosition = XPosition;
        cntl.YPosition = YPosition;
        cntl.Width = Width;
        cntl.Height = Height;
        cntl.DoUpdate();
      }
    }

    public override void StorePosition()
    {
      if (_layoutAlbum != null)
      {
        _layoutAlbum.StorePosition();
      }
      if (_layoutList != null)
      {
        _layoutList.StorePosition();
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.StorePosition();
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.StorePosition();
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.StorePosition();
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.StorePosition();
      }
      base.StorePosition();
    }

    public override void ReStorePosition()
    {
      if (_layoutAlbum != null)
      {
        _layoutAlbum.ReStorePosition();
      }
      if (_layoutList != null)
      {
        _layoutList.ReStorePosition();
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.ReStorePosition();
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.ReStorePosition();
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.ReStorePosition();
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.ReStorePosition();
      }
      base.ReStorePosition();
    }

    public override void Animate(float timePassed, Animator animator)
    {
      if (_layoutAlbum != null)
      {
        _layoutAlbum.Animate(timePassed, animator);
      }
      if (_layoutList != null)
      {
        _layoutList.Animate(timePassed, animator);
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.Animate(timePassed, animator);
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.Animate(timePassed, animator);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.Animate(timePassed, animator);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.Animate(timePassed, animator);
      }
      base.Animate(timePassed, animator);
    }

    public GUIListItem SelectedListItem
    {
      get
      {
        if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
        {
          return _layoutFilmStrip.SelectedListItem;
        }
        else if (_currentLayout == Layout.List && _layoutList != null)
        {
          return _layoutList.SelectedListItem;
        }
        else if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
                 _layoutThumbnail != null)
        {
          return _layoutThumbnail.SelectedListItem;
        }
        else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
        {
          return _layoutAlbum.SelectedListItem;
        }
        else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
        {
          return _layoutPlayList.SelectedListItem;
        }
        else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
        {
          return _layoutCoverFlow.SelectedListItem;
        }
        return null;
      }
    }

    public int Count
    {
      get
      {
        if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
        {
          return _layoutFilmStrip.Count;
        }
        else if (_currentLayout == Layout.List && _layoutList != null)
        {
          return _layoutList.Count;
        }
        else if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
                 _layoutThumbnail != null)
        {
          return _layoutThumbnail.Count;
        }
        else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
        {
          return _layoutAlbum.Count;
        }
        else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
        {
          return _layoutPlayList.Count;
        }
        else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
        {
          return _layoutCoverFlow.Count;
        }
        return 0;
      }
    }

    public void Clear()
    {
      if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
      {
        _layoutFilmStrip.Clear();
      }
      else if (_currentLayout == Layout.List && _layoutList != null)
      {
        _layoutList.Clear();
      }
      else if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
               _layoutThumbnail != null)
      {
        _layoutThumbnail.Clear();
      }
      else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
      {
        _layoutAlbum.Clear();
      }
      else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
      {
        _layoutPlayList.Clear();
      }
      else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
      {
        _layoutCoverFlow.Clear();
      }
      _itemList.DisposeAndClear();
    }

    public int SelectedListItemIndex
    {
      get
      {
        if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
        {
          return _layoutFilmStrip.SelectedListItemIndex;
        }
        else if (_currentLayout == Layout.List && _layoutList != null)
        {
          return _layoutList.SelectedListItemIndex;
        }
        else if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
                 _layoutThumbnail != null)
        {
          return _layoutThumbnail.SelectedListItemIndex;
        }
        else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
        {
          return _layoutAlbum.SelectedListItemIndex;
        }
        else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
        {
          return _layoutPlayList.SelectedListItemIndex;
        }
        else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
        {
          return _layoutCoverFlow.SelectedListItemIndex;
        }
        return -1;
      }
      set
      {
        if (_currentLayout == Layout.List && _layoutList != null)
        {
          _layoutList.SelectedListItemIndex = value;
        }
        else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
        {
          _layoutAlbum.SelectedListItemIndex = value;
        }
        else if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
                 _layoutThumbnail != null)
        {
          _layoutThumbnail.SelectedListItemIndex = value;
        }
        else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
        {
          _layoutPlayList.SelectedListItemIndex = value;
        }
        else if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
        {
          _layoutFilmStrip.SelectedListItemIndex = value;
        }
        else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
        {
          _layoutCoverFlow.SelectedListItemIndex = value;
        }
      }
    }

    public GUIListItem this[int index]
    {
      get
      {
        if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
        {
          return _layoutFilmStrip[index];
        }
        else if (_currentLayout == Layout.List && _layoutList != null)
        {
          return _layoutList[index];
        }
        else if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
                 _layoutThumbnail != null)
        {
          return _layoutThumbnail[index];
        }
        else if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
        {
          return _layoutAlbum[index];
        }
        else if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
        {
          return _layoutPlayList[index];
        }
        else if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
        {
          return _layoutCoverFlow[index];
        }
        return null;
      }
    }

    public void RefreshCoverArt()
    {
      //GUIListItem item;
      for (int i = 0; i < Count; ++i)
      {
        if (_currentLayout == Layout.Filmstrip && _layoutFilmStrip != null)
        {
          _layoutFilmStrip[i].RefreshCoverArt();
        }
        if (_currentLayout == Layout.List && _layoutList != null)
        {
          _layoutList[i].RefreshCoverArt();
        }
        if ((_currentLayout == Layout.SmallIcons || _currentLayout == Layout.LargeIcons) &&
            _layoutThumbnail != null)
        {
          _layoutThumbnail[i].RefreshCoverArt();
        }
        if (_currentLayout == Layout.AlbumView && _layoutAlbum != null)
        {
          _layoutAlbum[i].RefreshCoverArt();
        }
        if (_currentLayout == Layout.Playlist && _layoutPlayList != null)
        {
          _layoutPlayList[i].RefreshCoverArt();
        }
        if (_currentLayout == Layout.CoverFlow && _layoutCoverFlow != null)
        {
          _layoutCoverFlow[i].RefreshCoverArt();
        }
      }
    }

    public void Replace(int index, GUIListItem iItem)
    {
      if (iItem == null || index < 0)
      {
        return;
      }

      if (_layoutList != null)
      {
        _layoutList.Replace(index, iItem);
      }
      if (_layoutAlbum != null)
      {
        _layoutAlbum.Replace(index, iItem);
      }
      if (_layoutThumbnail != null)
      {
        _layoutThumbnail.Replace(index, iItem);
      }
      if (_layoutFilmStrip != null)
      {
        _layoutFilmStrip.Replace(index, iItem);
      }
      if (_layoutPlayList != null)
      {
        _layoutPlayList.Replace(index, iItem);
      }
      if (_layoutCoverFlow != null)
      {
        _layoutCoverFlow.Replace(index, iItem);
      }
    }

    public int RemoveItem(int iItem)
    {
      int selectedItemIndex = -1;

      if (iItem < 0 || iItem > _itemList.Count)
      {
        return -1;
      }

      if (_layoutList != null)
      {
        selectedItemIndex = _layoutList.RemoveItem(iItem);
      }
      if (_layoutAlbum != null)
      {
        selectedItemIndex = _layoutAlbum.RemoveItem(iItem);
      }
      if (_layoutThumbnail != null)
      {
        selectedItemIndex = _layoutThumbnail.RemoveItem(iItem);
      }
      if (_layoutFilmStrip != null)
      {
        selectedItemIndex = _layoutFilmStrip.RemoveItem(iItem);
      }
      if (_layoutPlayList != null)
      {
        selectedItemIndex = _layoutPlayList.RemoveItem(iItem);
      }
      if (_layoutCoverFlow != null)
      {
        selectedItemIndex = _layoutCoverFlow.RemoveItem(iItem);
      }

      return selectedItemIndex;
    }

    public int MoveItemDown(int iItem, bool select)
    {
      int selectedItemIndex = -1;

      if (iItem < 0 || iItem >= _itemList.Count)
      {
        return -1;
      }

      int iNextItem = iItem + 1;

      if (iNextItem >= _itemList.Count)
      {
        iNextItem = 0;
      }

      GUIListItem item1 = _itemList[iItem];
      GUIListItem item2 = _itemList[iNextItem];

      if (item1 == null || item2 == null)
      {
        return -1;
      }

      try
      {
        Log.Info("Moving List Item {0} down. Old index:{1}, new index{2}", item1.Path, iItem, iNextItem);
        Monitor.Enter(this);
        _itemList[iItem] = item2;
        _itemList[iNextItem] = item1;
        selectedItemIndex = iNextItem;
      }

      catch (Exception ex)
      {
        Log.Info("GUIFacadeControl.MoveItemDown caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        if (selectedItemIndex >= 0)
        {
          if (_layoutPlayList != null)
          {
            _layoutPlayList.SelectedListItemIndex = _layoutPlayList.MoveItemDown(iItem);
          }

          if (_layoutList != null)
          {
            _layoutList.SelectedListItemIndex = _layoutList.MoveItemDown(iItem);
          }

          if (_layoutAlbum != null)
          {
            _layoutAlbum.SelectedListItemIndex = _layoutAlbum.MoveItemDown(iItem);
          }

          if (_layoutThumbnail != null)
          {
            _layoutThumbnail.MoveItemDown(iItem);
          }
        }

        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    public int MoveItemUp(int iItem, bool select)
    {
      int selectedItemIndex = -1;

      if (iItem < 0 || iItem >= _itemList.Count)
      {
        return -1;
      }

      int iPreviousItem = iItem - 1;

      if (iPreviousItem < 0)
      {
        iPreviousItem = _itemList.Count - 1;
      }

      GUIListItem item1 = _itemList[iItem];
      GUIListItem item2 = _itemList[iPreviousItem];

      if (item1 == null || item2 == null)
      {
        return -1;
      }

      try
      {
        Log.Info("Moving List Item {0} up. Old index:{1}, new index{2}", item1.Path, iItem, iPreviousItem);
        Monitor.Enter(this);
        _itemList[iItem] = item2;
        _itemList[iPreviousItem] = item1;
        selectedItemIndex = iPreviousItem;
      }

      catch (Exception ex)
      {
        Log.Info("GUIFacadeControl.MoveItemUp caused an exception: {0}", ex.Message);
        selectedItemIndex = -1;
      }

      finally
      {
        if (selectedItemIndex >= 0)
        {
          if (_layoutPlayList != null)
          {
            _layoutPlayList.SelectedListItemIndex = _layoutPlayList.MoveItemUp(iItem);
          }

          if (_layoutList != null)
          {
            _layoutList.SelectedListItemIndex = _layoutList.MoveItemUp(iItem);
          }

          if (_layoutAlbum != null)
          {
            _layoutAlbum.SelectedListItemIndex = _layoutAlbum.MoveItemUp(iItem);
          }

          if (_layoutThumbnail != null)
          {
            _layoutThumbnail.MoveItemUp(iItem);
          }
        }

        Monitor.Exit(this);
      }

      return selectedItemIndex;
    }

    private static bool HasSameItems(IList<GUIListItem> sequence1, IList<GUIListItem> sequence2)
    {
      if (sequence1 == null) return sequence2 == null;
      if (sequence2 == null) return false;
      if (sequence1.Count != sequence2.Count) return false;
      int items = sequence1.Count;
      for (int i = 0; i < items; i++)
      {
        if (sequence1[i] != sequence2[i])
          // let's be strict and require them to be the same instance, and not just same label, etc.
          return false;
      }
      return true;
    }
  }
}