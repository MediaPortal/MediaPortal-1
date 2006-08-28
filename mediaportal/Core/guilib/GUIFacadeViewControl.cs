/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
            AlbumView,
            Playlist
        }

        GUIPlayListItemListControl _viewPlayList = null; // instance of a re-orderable playlist list control
        GUIListControl _viewList = null;			// instance of the list control

        GUIListControl _viewAlbum = null;	// instance of the album list control
        GUIThumbnailPanel _viewThumbnail = null; // instance of the thumbnail control
        GUIFilmstripControl _viewFilmStrip = null; // instance of the filmstrip control
        ViewMode _currentViewMode;						// current view
        List<GUIListItem> _itemList = new List<GUIListItem>(); // unfiltered itemlist

        public GUIFacadeControl(int dwParentID)
            : base(dwParentID)
        {
        }

        public GUIFacadeControl(int dwParentID, int dwControlId)
            : base(dwParentID, dwControlId, 0, 0, 0, 0)
        {
        }

        /////<summary>
        ///// Property to get/set the type if listview that will be displayed; GUIListControl or GUIPlayListItemListControl
        /////</summary>
        //  public ListType ListViewType
        //  {
        //      get{return _listViewType;}
        //      set{_listViewType = value;}
        //  }

        /// <summary>
        /// Property to get/set the list control
        /// </summary>
        public GUIListControl ListView
        {
            get { return _viewList; }
            set
            {
                _viewList = value;
                if (_viewList != null)
                {
                    _viewList.GetID = GetID;
                    _viewList.WindowId = WindowId;
                    _viewList.ParentControl = this;
                }
            }
        }

        /// <summary>
        /// Property to get/set the playlist list control
        /// </summary>
        public GUIPlayListItemListControl PlayListView
        {
            get { return _viewPlayList; }
            set
            {
                _viewPlayList = value;
                if (_viewPlayList != null)
                {
                    _viewPlayList.GetID = GetID;
                    _viewPlayList.WindowId = WindowId;
                    _viewPlayList.ParentControl = this;
                }
            }
        }

        /// <summary>
        /// Property to get/set the list control
        /// </summary>
        public GUIListControl AlbumListView
        {
            get { return _viewAlbum; }
            set
            {
                _viewAlbum = value;
                if (_viewAlbum != null)
                {
                    _viewAlbum.GetID = GetID;
                    _viewAlbum.WindowId = WindowId;
                    _viewAlbum.ParentControl = this;
                }
            }
        }

        /// <summary>
        /// Property to get/set the filmstrip control
        /// </summary>
        public GUIFilmstripControl FilmstripView
        {
            get { return _viewFilmStrip; }
            set
            {
                _viewFilmStrip = value;
                if (_viewFilmStrip != null)
                {
                    _viewFilmStrip.GetID = GetID;
                    _viewFilmStrip.WindowId = WindowId;
                    _viewFilmStrip.ParentControl = this;
                }
            }
        }

        /// <summary>
        /// Property to get/set the thumbnail control
        /// </summary>
        public GUIThumbnailPanel ThumbnailView
        {
            get { return _viewThumbnail; }
            set
            {
                _viewThumbnail = value;
                if (_viewThumbnail != null)
                {
                    _viewThumbnail.GetID = GetID;
                    _viewThumbnail.WindowId = WindowId;
                    _viewThumbnail.ParentControl = this;
                }
            }
        }

        /// <summary>
        /// Property to get/set the current view mode
        /// </summary>
        public ViewMode View
        {
            get { return _currentViewMode; }
            set
            {
                _currentViewMode = value;
                UpdateView();
            }
        }

        /// <summary>
        /// Render. This will render the current selected view 
        /// </summary>
        public override void Render(float timePassed)
        {
            if (_currentViewMode == ViewMode.AlbumView && AlbumListView != null) AlbumListView.Render(timePassed);
            else if (_currentViewMode == ViewMode.List && _viewList != null) _viewList.Render(timePassed);
            else if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null) _viewFilmStrip.Render(timePassed);
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null) _viewPlayList.Render(timePassed);
            else if (_viewThumbnail != null) _viewThumbnail.Render(timePassed);
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
                AlbumListView.ParentControl = this;
            }
            if (_viewList != null)
            {
                _viewList.AllocResources();
                _viewList.GetID = GetID;
                _viewList.WindowId = WindowId;
                _viewList.ParentControl = this;
            }

            if (_viewThumbnail != null)
            {
                _viewThumbnail.AllocResources();
                _viewThumbnail.GetID = GetID;
                _viewThumbnail.WindowId = WindowId;
                _viewThumbnail.ParentControl = this;
            }

            if (_viewFilmStrip != null)
            {
                _viewFilmStrip.AllocResources();
                _viewFilmStrip.GetID = GetID;
                _viewFilmStrip.WindowId = WindowId;
                _viewFilmStrip.ParentControl = this;
            }

            if (_viewPlayList != null)
            {
                _viewPlayList.AllocResources();
                _viewPlayList.GetID = GetID;
                _viewPlayList.WindowId = WindowId;
                _viewPlayList.ParentControl = this;
            }
        }
        public override int WindowId
        {
            get { return _windowId; }
            set
            {
                _windowId = value;
                if (AlbumListView != null) AlbumListView.WindowId = value;
                if (_viewList != null) _viewList.WindowId = value;
                if (_viewThumbnail != null) _viewThumbnail.WindowId = value;
                if (_viewFilmStrip != null) _viewFilmStrip.WindowId = value;
                if (_viewPlayList != null) _viewPlayList.WindowId = value;
            }
        }

        /// <summary>
        /// pre-Allocate any resources needed for the controls
        /// </summary>
        public override void PreAllocResources()
        {
            if (AlbumListView != null) AlbumListView.PreAllocResources();
            if (_viewList != null) _viewList.PreAllocResources();
            if (_viewThumbnail != null) _viewThumbnail.PreAllocResources();
            if (_viewFilmStrip != null) _viewFilmStrip.PreAllocResources();
            if (_viewPlayList != null) _viewPlayList.PreAllocResources();
            UpdateView();
        }

        /// <summary>
        /// Free all resources of the controls
        /// </summary>
        public override void FreeResources()
        {
            if (AlbumListView != null) AlbumListView.FreeResources();
            if (_viewList != null) _viewList.FreeResources();
            if (_viewThumbnail != null) _viewThumbnail.FreeResources();
            if (_viewFilmStrip != null) _viewFilmStrip.FreeResources();
            if (_viewPlayList != null) _viewPlayList.FreeResources();
            _itemList.Clear();
        }

        public override bool HitTest(int x, int y, out int controlID, out bool focused)
        {
            controlID = -1;
            focused = false;
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                return _viewFilmStrip.HitTest(x, y, out controlID, out focused);
            else if (_currentViewMode == ViewMode.List && _viewList != null)
                return _viewList.HitTest(x, y, out controlID, out focused);
            else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                return _viewThumbnail.HitTest(x, y, out controlID, out focused);
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                return _viewAlbum.HitTest(x, y, out controlID, out focused);
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                return _viewPlayList.HitTest(x, y, out controlID, out focused);
            return false;
        }

        public override void OnAction(Action action)
        {
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                _viewFilmStrip.OnAction(action);
            else if (_currentViewMode == ViewMode.List && _viewList != null)
                _viewList.OnAction(action);
            else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                _viewThumbnail.OnAction(action);
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                _viewAlbum.OnAction(action);
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                _viewPlayList.OnAction(action);
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
                    _itemList.Clear();
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
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                return _viewFilmStrip.OnMessage(message);
            if (_currentViewMode == ViewMode.List && _viewList != null)
                return _viewList.OnMessage(message);
            else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                return _viewThumbnail.OnMessage(message);
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                return _viewAlbum.OnMessage(message);
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                return _viewPlayList.OnMessage(message);
            return false;
        }

        void RouteMessage(GUIMessage message)
        {
            if (_viewAlbum != null) _viewAlbum.OnMessage(message);
            if (_viewList != null) _viewList.OnMessage(message);
            if (_viewFilmStrip != null) _viewFilmStrip.OnMessage(message);
            if (_viewThumbnail != null) _viewThumbnail.OnMessage(message);
            if (_viewPlayList != null) _viewPlayList.OnMessage(message);
        }

        public override bool CanFocus()
        {
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                return _viewFilmStrip.CanFocus();
            if (_currentViewMode == ViewMode.List && _viewList != null)
                return _viewList.CanFocus();
            else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                _viewThumbnail.CanFocus();
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                _viewAlbum.CanFocus();
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                _viewPlayList.CanFocus();
            return true;
        }

        public override bool Focus
        {
            get
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    return _viewFilmStrip.Focus;
                else if (_currentViewMode == ViewMode.List && _viewList != null)
                    return _viewList.Focus;
                else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    return _viewThumbnail.Focus;
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    return _viewAlbum.Focus;
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    return _viewPlayList.Focus;
                return false;
            }
            set
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    _viewFilmStrip.Focus = value;
                else if (_currentViewMode == ViewMode.List && _viewList != null)
                    _viewList.Focus = value;
                else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    _viewThumbnail.Focus = value;
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    _viewAlbum.Focus = value;
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    _viewPlayList.Focus = value;
            }
        }

        public override bool InControl(int x, int y, out int controlID)
        {
            controlID = -1;
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                return _viewFilmStrip.InControl(x, y, out controlID);
            else if (_currentViewMode == ViewMode.List && _viewList != null)
                return _viewList.InControl(x, y, out controlID);
            else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                return _viewThumbnail.InControl(x, y, out controlID);
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                return _viewAlbum.InControl(x, y, out controlID);
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                return _viewPlayList.InControl(x, y, out controlID);
            return false;
        }

        public void Sort(System.Collections.Generic.IComparer<GUIListItem> comparer)
        {
            if (_viewAlbum != null) _viewAlbum.Sort(comparer);
            if (_viewList != null) _viewList.Sort(comparer);
            if (_viewThumbnail != null) _viewThumbnail.Sort(comparer);
            if (_viewFilmStrip != null) _viewFilmStrip.Sort(comparer);
            if (_viewPlayList != null) _viewPlayList.Sort(comparer);
            try
            {
                _itemList.Sort(comparer);
            }
            catch (Exception) { }
        }

        public void Add(GUIListItem item)
        {
            if (item == null) return;
            if (_viewAlbum != null) _viewAlbum.Add(item);
            if (_viewList != null) _viewList.Add(item);
            if (_viewThumbnail != null) _viewThumbnail.Add(item);
            if (_viewFilmStrip != null) _viewFilmStrip.Add(item);
            if (_viewPlayList != null) _viewPlayList.Add(item);
            _itemList.Add(item);
        }

        public void Filter(int searchKind, string searchString)
        {
            if (_viewAlbum != null) _viewAlbum.Clear();
            if (_viewList != null) _viewList.Clear();
            if (_viewThumbnail != null) _viewThumbnail.Clear();
            if (_viewFilmStrip != null) _viewFilmStrip.Clear();
            if (_viewPlayList != null) _viewPlayList.Clear();
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
                if (_viewAlbum != null) _viewAlbum.Add(dirUp);
                if (_viewList != null) _viewList.Add(dirUp);
                if (_viewThumbnail != null) _viewThumbnail.Add(dirUp);
                if (_viewFilmStrip != null) _viewFilmStrip.Add(dirUp);
                if (_viewPlayList != null) _viewPlayList.Add(dirUp);
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
                    if (_viewAlbum != null) _viewAlbum.Add(item);
                    if (_viewList != null) _viewList.Add(item);
                    if (_viewThumbnail != null) _viewThumbnail.Add(item);
                    if (_viewFilmStrip != null) _viewFilmStrip.Add(item);
                    if (_viewPlayList != null) _viewPlayList.Add(item);
                    iTotalItems++;
                }
            }
            string strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
            GUIPropertyManager.SetProperty("#itemcount", strObjects);
        }

        void UpdateView()
        {
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
            {
                base.XPosition = _viewFilmStrip.XPosition;
                base.YPosition = _viewFilmStrip.YPosition;
                base.Width = _viewFilmStrip.Width;
                base.Height = _viewFilmStrip.Height;
                _viewFilmStrip.IsVisible = true;
                if (_viewList != null) _viewList.IsVisible = false;
                if (_viewThumbnail != null) _viewThumbnail.IsVisible = false;
                if (_viewAlbum != null) _viewAlbum.IsVisible = false;
                if (_viewPlayList != null) _viewPlayList.IsVisible = false;
            }
            else if (_currentViewMode == ViewMode.List && _viewList != null)
            {
                base.XPosition = _viewList.XPosition;
                base.YPosition = _viewList.YPosition;
                base.Width = _viewList.Width;
                base.Height = _viewList.Height;
                _viewList.IsVisible = true;
                if (_viewPlayList != null) _viewPlayList.IsVisible = false;
                if (_viewThumbnail != null) _viewThumbnail.IsVisible = false;
                if (_viewFilmStrip != null) _viewFilmStrip.IsVisible = false;
                if (_viewAlbum != null) _viewAlbum.IsVisible = false;
            }
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
            {
                base.XPosition = _viewPlayList.XPosition;
                base.YPosition = _viewPlayList.YPosition;
                base.Width = _viewPlayList.Width;
                base.Height = _viewPlayList.Height;
                if (_viewList != null) _viewList.IsVisible = false;
                _viewPlayList.IsVisible = true;

                if (_viewThumbnail != null) _viewThumbnail.IsVisible = false;
                if (_viewFilmStrip != null) _viewFilmStrip.IsVisible = false;
                if (_viewAlbum != null) _viewAlbum.IsVisible = false;
            }
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
            {
                base.XPosition = _viewAlbum.XPosition;
                base.YPosition = _viewAlbum.YPosition;
                base.Width = _viewAlbum.Width;
                base.Height = _viewAlbum.Height;
                _viewAlbum.IsVisible = true;
                if (_viewList != null) _viewList.IsVisible = false;
                if (_viewThumbnail != null) _viewThumbnail.IsVisible = false;
                if (_viewFilmStrip != null) _viewFilmStrip.IsVisible = false;
                if (_viewPlayList != null) _viewPlayList.IsVisible = false;
            }
            else if (_viewThumbnail != null)
            {
                base.XPosition = _viewThumbnail.XPosition;
                base.YPosition = _viewThumbnail.YPosition;
                base.Width = _viewThumbnail.Width;
                base.Height = _viewThumbnail.Height;
                _viewThumbnail.ShowBigIcons(_currentViewMode == ViewMode.LargeIcons);
                _viewThumbnail.IsVisible = true;
                if (_viewList != null) _viewList.IsVisible = false;
                if (_viewFilmStrip != null) _viewFilmStrip.IsVisible = false;
                if (_viewAlbum != null) _viewAlbum.IsVisible = false;
                if (_viewPlayList != null) _viewPlayList.IsVisible = false;
            }
        }

        public override void DoUpdate()
        {
            if (_currentViewMode == ViewMode.List && _viewList != null)
            {
                _viewList.XPosition = XPosition;
                _viewList.YPosition = YPosition;
                _viewList.Width = Width;
                _viewList.Height = Height;
                _viewList.DoUpdate();
            }
            if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
            {
                _viewAlbum.XPosition = XPosition;
                _viewAlbum.YPosition = YPosition;
                _viewAlbum.Width = Width;
                _viewAlbum.Height = Height;
                _viewAlbum.DoUpdate();
            }


            if ((_currentViewMode == ViewMode.LargeIcons || _currentViewMode == ViewMode.SmallIcons) && _viewThumbnail != null)
            {
                _viewThumbnail.XPosition = XPosition;
                _viewThumbnail.YPosition = YPosition;
                _viewThumbnail.Width = Width;
                _viewThumbnail.Height = Height;
                _viewThumbnail.DoUpdate();
            }


            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
            {
                _viewFilmStrip.XPosition = XPosition;
                _viewFilmStrip.YPosition = YPosition;
                _viewFilmStrip.Width = Width;
                _viewFilmStrip.Height = Height;
                _viewFilmStrip.DoUpdate();
            }

            if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
            {
                _viewPlayList.XPosition = XPosition;
                _viewPlayList.YPosition = YPosition;
                _viewPlayList.Width = Width;
                _viewPlayList.Height = Height;
                _viewPlayList.DoUpdate();
            }
        }

        public override void StorePosition()
        {
            if (_viewAlbum != null) _viewAlbum.StorePosition();
            if (_viewList != null) _viewList.StorePosition();
            if (_viewThumbnail != null) _viewThumbnail.StorePosition();
            if (_viewFilmStrip != null) _viewFilmStrip.StorePosition();
            if (_viewPlayList != null) _viewPlayList.StorePosition();
            base.StorePosition();
        }

        public override void ReStorePosition()
        {
            if (_viewAlbum != null) _viewAlbum.ReStorePosition();
            if (_viewList != null) _viewList.ReStorePosition();
            if (_viewThumbnail != null) _viewThumbnail.ReStorePosition();
            if (_viewFilmStrip != null) _viewFilmStrip.ReStorePosition();
            if (_viewPlayList != null) _viewPlayList.ReStorePosition();
            base.ReStorePosition();
        }

        public override void Animate(float timePassed, Animator animator)
        {
            if (_viewAlbum != null) _viewAlbum.Animate(timePassed, animator);
            if (_viewList != null) _viewList.Animate(timePassed, animator);
            if (_viewThumbnail != null) _viewThumbnail.Animate(timePassed, animator);
            if (_viewFilmStrip != null) _viewFilmStrip.Animate(timePassed, animator);
            if (_viewPlayList != null) _viewPlayList.Animate(timePassed, animator);
            base.Animate(timePassed, animator);
        }

        public GUIListItem SelectedListItem
        {
            get
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    return _viewFilmStrip.SelectedListItem;
                else if (_currentViewMode == ViewMode.List && _viewList != null)
                    return _viewList.SelectedListItem;
                else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    return _viewThumbnail.SelectedListItem;
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    return _viewAlbum.SelectedListItem;
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    return _viewPlayList.SelectedListItem;
                return null;
            }
        }

        public int Count
        {
            get
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    return _viewFilmStrip.Count;
                else if (_currentViewMode == ViewMode.List && _viewList != null)
                    return _viewList.Count;
                else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    return _viewThumbnail.Count;
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    return _viewAlbum.Count;
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    return _viewPlayList.Count;
                return 0;
            }
        }

        public void Clear()
        {
            if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                _viewFilmStrip.Clear();
            else if (_currentViewMode == ViewMode.List && _viewList != null)
                _viewList.Clear();
            else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                _viewThumbnail.Clear();
            else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                _viewAlbum.Clear();
            else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                _viewPlayList.Clear();
            _itemList.Clear();
        }
        public int SelectedListItemIndex
        {
            get
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    return _viewFilmStrip.SelectedListItemIndex;
                else if (_currentViewMode == ViewMode.List && _viewList != null)
                    return _viewList.SelectedListItemIndex;
                else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    return _viewThumbnail.SelectedListItemIndex;
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    return _viewAlbum.SelectedListItemIndex;
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    return _viewPlayList.SelectedListItemIndex;
                return -1;
            }
            set
            {
                if (_currentViewMode == ViewMode.List && _viewList != null)
                    _viewList.SelectedListItemIndex = value;
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    _viewAlbum.SelectedListItemIndex = value;
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    _viewPlayList.SelectedListItemIndex = value;
            }
        }

        public GUIListItem this[int index]
        {
            get
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    return _viewFilmStrip[index];
                else if (_currentViewMode == ViewMode.List && _viewList != null)
                    return _viewList[index];
                else if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    return _viewThumbnail[index];
                else if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    return _viewAlbum[index];
                else if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    return _viewPlayList[index];
                return null;
            }
        }
        public void RefreshCoverArt()
        {
            //GUIListItem item;
            for (int i = 0; i < Count; ++i)
            {
                if (_currentViewMode == ViewMode.Filmstrip && _viewFilmStrip != null)
                    _viewFilmStrip[i].RefreshCoverArt();
                if (_currentViewMode == ViewMode.List && _viewList != null)
                    _viewList[i].RefreshCoverArt();
                if ((_currentViewMode == ViewMode.SmallIcons || _currentViewMode == ViewMode.LargeIcons) && _viewThumbnail != null)
                    _viewThumbnail[i].RefreshCoverArt();
                if (_currentViewMode == ViewMode.AlbumView && _viewAlbum != null)
                    _viewAlbum[i].RefreshCoverArt();
                if (_currentViewMode == ViewMode.Playlist && _viewPlayList != null)
                    _viewPlayList[i].RefreshCoverArt();
            }
        }

        public int MoveItemDown(int iItem, bool select)
        {
            int selectedItemIndex = -1;

            if (iItem < 0 || iItem >= _itemList.Count)
                return -1;

            int iNextItem = iItem + 1;

            if (iNextItem >= _itemList.Count)
                iNextItem = 0;

            GUIListItem item1 = _itemList[iItem];
            GUIListItem item2 = _itemList[iNextItem];

            if (item1 == null || item2 == null)
                return -1;

            try
            {
                Log.Info("Moving List Item {0} down. Old index:{1}, new index{2}", item1.Path, iItem, iNextItem);
                System.Threading.Monitor.Enter(this);
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
                    if (_viewPlayList != null)
                        _viewPlayList.SelectedListItemIndex = _viewPlayList.MoveItemDown(iItem);

                    if (_viewList != null)
                        _viewList.SelectedListItemIndex = _viewList.MoveItemDown(iItem);

                    if (_viewAlbum != null)
                        _viewAlbum.SelectedListItemIndex = _viewAlbum.MoveItemDown(iItem);

                    if (_viewThumbnail != null)
                        _viewThumbnail.MoveItemDown(iItem);
                }

                System.Threading.Monitor.Exit(this);
            }

            return selectedItemIndex;
        }

        public int MoveItemUp(int iItem, bool select)
        {
            int selectedItemIndex = -1;

            if (iItem < 0 || iItem >= _itemList.Count)
                return -1;

            int iPreviousItem = iItem - 1;

            if (iPreviousItem < 0)
                iPreviousItem = _itemList.Count - 1;

            GUIListItem item1 = _itemList[iItem];
            GUIListItem item2 = _itemList[iPreviousItem];

            if (item1 == null || item2 == null)
                return -1;

            try
            {
                Log.Info("Moving List Item {0} up. Old index:{1}, new index{2}", item1.Path, iItem, iPreviousItem);
                System.Threading.Monitor.Enter(this);
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
                    if (_viewPlayList != null)
                        _viewPlayList.SelectedListItemIndex = _viewPlayList.MoveItemUp(iItem);

                    if (_viewList != null)
                        _viewList.SelectedListItemIndex = _viewList.MoveItemUp(iItem);

                    if (_viewAlbum != null)
                        _viewAlbum.SelectedListItemIndex = _viewAlbum.MoveItemUp(iItem);

                    if (_viewThumbnail != null)
                        _viewThumbnail.MoveItemUp(iItem);
                }

                System.Threading.Monitor.Exit(this);
            }

            return selectedItemIndex;
        }

/*      public override int DimColor
      {
        get { return base.DimColor; }
        set
        {
          base.DimColor = value;
          if (_viewPlayList != null) _viewPlayList.DimColor = value;
          if (_viewList != null) _viewList.DimColor = value;
          if (_viewAlbum != null) _viewAlbum.DimColor = value;
          if (_viewThumbnail != null) _viewThumbnail.DimColor = value;
          if (_viewFilmStrip != null) _viewFilmStrip.DimColor = value;
          foreach (GUIListItem item in _itemList) item.DimColor = value;
        }
      }
*/

    }
}
