using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Control which acts as a facade to the list,thumbnail and filmstrip view controls
	/// for the application it presents itself as 1 control but in reality it
	/// will route all actions to the current selected control (list,view, or filmstrip)
	/// </summary>
	public class GUIFacadeControl : GUIControl
	{
			//enum of all possible views
      public enum ViewMode
      {
          List,
          SmallIcons,
          LargeIcons,
          Filmstrip
      }
      GUIListControl      m_ListView=null;			// instance of the list control
      GUIThumbnailPanel   m_ThumbnailView=null; // instance of the thumbnail control
      GUIFilmstripControl m_FilmStripView=null; // instance of the filmstrip control
      ViewMode            m_ViewMode;						// current view

		public GUIFacadeControl(int dwParentID) : base(dwParentID)
		{
		}

		public GUIFacadeControl(int dwParentID, int dwControlId)
      :base(dwParentID, dwControlId,0,0, 0,0)
		{
		}

		/// <summary>
		/// Property to get/set the list control
		/// </summary>
    public GUIListControl ListView
    {
      get { return m_ListView;}
      set { 
            m_ListView=value;
            if (m_ListView!=null) m_ListView.GetID=GetID;
      }
    }

		/// <summary>
		/// Property to get/set the filmstrip control
		/// </summary>
    public GUIFilmstripControl FilmstripView
    {
      get { return m_FilmStripView;}
      set 
      { 
        m_FilmStripView=value;
        if (m_FilmStripView!=null) m_FilmStripView.GetID=GetID;
      }
    }

		/// <summary>
		/// Property to get/set the thumbnail control
		/// </summary>
    public GUIThumbnailPanel ThumbnailView
    {
        get { return m_ThumbnailView;}
        set { 
          m_ThumbnailView=value;
          if (m_ThumbnailView!=null) m_ThumbnailView.GetID=GetID;
        }
    }

		/// <summary>
		/// Property to get/set the current view mode
		/// </summary>
    public ViewMode View
    {
        get { return m_ViewMode;}
        set { 
          m_ViewMode=value;
          UpdateView();
        }
    }

		/// <summary>
		/// Render. This will render the current selected view 
		/// </summary>
    public override void Render()
    {
      if (m_ListView!=null) m_ListView.Render();
      if (m_ThumbnailView!=null) m_ThumbnailView.Render();
      if (m_FilmStripView!=null) m_FilmStripView.Render();
    }

		/// <summary>
		/// Allocate any resources needed for the controls
		/// </summary>
    public override void AllocResources()
    {
			if (m_ListView!=null)
			{
				m_ListView.AllocResources();
				m_ListView.GetID=GetID;
			}

			if (m_ThumbnailView!=null)
			{
				m_ThumbnailView.AllocResources();
				m_ThumbnailView.GetID=GetID;
			}

			if (m_FilmStripView!=null)
			{
				m_FilmStripView.AllocResources();
				m_FilmStripView.GetID=GetID;
			}
    }

		/// <summary>
		/// pre-Allocate any resources needed for the controls
		/// </summary>
    public override void PreAllocResources()
    {
        if (m_ListView!=null) m_ListView.PreAllocResources();
        if (m_ThumbnailView!=null) m_ThumbnailView.PreAllocResources();
        if (m_FilmStripView!=null) m_FilmStripView.PreAllocResources();
        UpdateView();
    }

		/// <summary>
		/// Free all resources of the controls
		/// </summary>
    public override void FreeResources()
    {
        if (m_ListView!=null) m_ListView.FreeResources();
        if (m_ThumbnailView!=null) m_ThumbnailView.FreeResources();
        if (m_FilmStripView!=null) m_FilmStripView.FreeResources();
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
			controlID=-1;
			focused=false;
      if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
          return m_FilmStripView.HitTest (x, y, out controlID, out focused);
      else if (m_ViewMode==ViewMode.List && m_ListView!=null)
            return m_ListView.HitTest (x, y, out controlID, out focused);
      else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
				return m_ThumbnailView.HitTest (x, y, out controlID, out focused);
			return false;
    }

    public override void OnAction(Action action)
    {
      if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
          m_FilmStripView.OnAction( action);
      else if (m_ViewMode==ViewMode.List&& m_ListView!=null)
          m_ListView.OnAction( action);
			else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
				m_ThumbnailView.OnAction( action);
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

        if (message.Message== GUIMessage.MessageType.GUI_MSG_REFRESH)
        {
          RouteMessage (message);
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          RouteMessage (message);
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
        {
          RouteMessage (message);
          return true;
        }
      }
      if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
        return m_FilmStripView.OnMessage (message);
      if (m_ViewMode==ViewMode.List && m_ListView!=null)
          return m_ListView.OnMessage (message);
			else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
				return m_ThumbnailView.OnMessage( message);
			return false;
    }
		void RouteMessage(GUIMessage message)
		{
			if (m_ListView!=null) m_ListView.OnMessage(message);
			if (m_FilmStripView!=null) m_FilmStripView.OnMessage(message);
			if (m_ThumbnailView!=null) m_ThumbnailView.OnMessage(message);
		}

    public override bool CanFocus()
    {
      if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
        return m_FilmStripView.CanFocus ();
      if (m_ViewMode==ViewMode.List && m_ListView!=null)
          return m_ListView.CanFocus ();
			else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
				m_ThumbnailView.CanFocus( );
			return true;
    }

    public override bool Focus
    {
      get { 
        if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
          return m_FilmStripView.Focus ;
        if (m_ViewMode==ViewMode.List && m_ListView!=null)
          return m_ListView.Focus;
				else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
					return m_ThumbnailView.Focus;
				return false;
      }
      set 
      { 
				if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
					m_FilmStripView.Focus=value ;
				if (m_ViewMode==ViewMode.List && m_ListView!=null)
					m_ListView.Focus=value;
				else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
					m_ThumbnailView.Focus=value;
      }
    }

    public override bool InControl(int x, int y, out int controlID)
    {
			controlID=-1;
			if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
				return m_FilmStripView.InControl (x, y, out controlID);
			if (m_ViewMode==ViewMode.List && m_ListView!=null)
				return m_ListView.InControl (x, y, out controlID);
			else if ( (m_ViewMode==ViewMode.SmallIcons ||m_ViewMode==ViewMode.LargeIcons ) && m_ThumbnailView!=null)  
				return m_ThumbnailView.InControl (x, y, out controlID);
			return false;
    }

    public void Sort(System.Collections.IComparer comparer)
    {
      if (m_ListView!=null) m_ListView.Sort(comparer);
      if (m_ThumbnailView!=null) m_ThumbnailView.Sort(comparer);
      if (m_FilmStripView!=null) m_FilmStripView.Sort(comparer);
    }

    public void Add(GUIListItem item)
    {
      if (item==null) return;
      if (m_ListView!=null) m_ListView.Add(item);
      if (m_ThumbnailView!=null) m_ThumbnailView.Add(item);
      if (m_FilmStripView!=null) m_FilmStripView.Add(item);
    }
    void UpdateView()
    {
      if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
      {
        base.XPosition=m_FilmStripView.XPosition;
        base.YPosition=m_FilmStripView.YPosition;
        base.Width=m_FilmStripView.Width;
        base.Height=m_FilmStripView.Height;
        m_FilmStripView.IsVisible=true;
        if (m_ListView!=null) m_ListView.IsVisible=false;
        if (m_ThumbnailView!=null) m_ThumbnailView.IsVisible=false;
      }       
      else if (m_ViewMode==ViewMode.List && m_ListView!=null)
      {
        base.XPosition=m_ListView.XPosition;
        base.YPosition=m_ListView.YPosition;
        base.Width=m_ListView.Width;
        base.Height=m_ListView.Height;
        m_ListView.IsVisible=true;
        if (m_ThumbnailView!=null) m_ThumbnailView.IsVisible=false;
        if (m_FilmStripView!=null) m_FilmStripView.IsVisible=false;
      }
      else if (m_ThumbnailView!=null)
      {
        base.XPosition=m_ThumbnailView.XPosition;
        base.YPosition=m_ThumbnailView.YPosition;
        base.Width=m_ThumbnailView.Width;
        base.Height=m_ThumbnailView.Height;
        m_ThumbnailView.ShowBigIcons(m_ViewMode==ViewMode.LargeIcons);
        m_ThumbnailView.IsVisible=true;
        if (m_ListView!=null) m_ListView.IsVisible=false;
        if (m_FilmStripView!=null) m_FilmStripView.IsVisible=false;
      }
    }

    public override void DoUpdate()
    {
      if (m_ViewMode==ViewMode.List && m_ListView!=null)
      {
        m_ListView.XPosition=XPosition;
        m_ListView.YPosition=YPosition;
        m_ListView.Width=Width;
        m_ListView.Height=Height;
        m_ListView.DoUpdate();
      }

      
      if ( (m_ViewMode==ViewMode.LargeIcons||m_ViewMode==ViewMode.SmallIcons) && m_ThumbnailView!=null)
      {
        m_ThumbnailView.XPosition=XPosition;
        m_ThumbnailView.YPosition=YPosition;
        m_ThumbnailView.Width=Width;
        m_ThumbnailView.Height=Height;
        m_ThumbnailView.DoUpdate();
      }

      
      if (m_ViewMode==ViewMode.Filmstrip && m_FilmStripView!=null)
      {
        m_FilmStripView.XPosition=XPosition;
        m_FilmStripView.YPosition=YPosition;
        m_FilmStripView.Width=Width;
        m_FilmStripView.Height=Height;
        m_FilmStripView.DoUpdate();
      }
    }

    public override void StorePosition()
    {
      if (m_ListView!=null) m_ListView.StorePosition();
      if (m_ThumbnailView!=null) m_ThumbnailView.StorePosition();
      if (m_FilmStripView!=null) m_FilmStripView.StorePosition();
      
      base.StorePosition();
    }

    public override void ReStorePosition()
    {
      if (m_ListView!=null) m_ListView.ReStorePosition();
      if (m_ThumbnailView!=null) m_ThumbnailView.ReStorePosition();
      if (m_FilmStripView!=null) m_FilmStripView.ReStorePosition();
      
      base.ReStorePosition();
    }

    public override void Animate(Animator animator)
    {
      if (m_ListView!=null) m_ListView.Animate(animator);
      if (m_ThumbnailView!=null) m_ThumbnailView.Animate(animator);
      if (m_FilmStripView!=null) m_FilmStripView.Animate(animator);
      base.Animate(animator);
    }
		
	}
}
