using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIFacadeControl : GUIControl
	{
        public enum ViewMode
        {
            List,
            SmallIcons,
            LargeIcons,
            Filmstrip
        }
        GUIListControl      m_ListView=null;
        GUIThumbnailPanel   m_ThumbnailView=null;
        GUIFilmstripControl m_FilmStripView=null;
        ViewMode            m_ViewMode;

		public GUIFacadeControl(int dwParentID) : base(dwParentID)
		{
		}

		    public GUIFacadeControl(int dwParentID, int dwControlId)
          :base(dwParentID, dwControlId,0,0, 0,0)
		    {
		    }

        public GUIListControl ListView
        {
            get { return m_ListView;}
            set { 
                  m_ListView=value;
                  m_ListView.GetID=GetID;
            }
        }

        public GUIFilmstripControl FilmstripView
        {
          get { return m_FilmStripView;}
          set 
          { 
            m_FilmStripView=value;
            m_FilmStripView.GetID=GetID;
          }
        }

        public GUIThumbnailPanel ThumbnailView
        {
            get { return m_ThumbnailView;}
            set { 
              m_ThumbnailView=value;
              m_ThumbnailView.GetID=GetID;
            }
        }

        public ViewMode View
        {
            get { return m_ViewMode;}
            set { 
              m_ViewMode=value;
              UpdateView();
            }
        }

        public override void Render()
        {
          m_ListView.Render();
          m_ThumbnailView.Render();
          m_FilmStripView.Render();
        }

        public override void AllocResources()
        {
            m_ListView.AllocResources();
            m_ThumbnailView.AllocResources();
            m_FilmStripView.AllocResources();
					  m_ListView.GetID=GetID;
						m_ThumbnailView.GetID=GetID;
						m_FilmStripView.GetID=GetID;
        }

        public override void PreAllocResources()
        {
            m_ListView.PreAllocResources();
            m_ThumbnailView.PreAllocResources();
            m_FilmStripView.PreAllocResources();
            UpdateView();
        }

        public override void FreeResources()
        {
            m_ListView.FreeResources();
            m_ThumbnailView.FreeResources();
            m_FilmStripView.FreeResources();
        }

        public override bool HitTest(int x, int y, out int controlID, out bool focused)
        {
          if (m_ViewMode==ViewMode.Filmstrip)
              return m_FilmStripView.HitTest (x, y, out controlID, out focused);
          else if (m_ViewMode==ViewMode.List)
                return m_ListView.HitTest (x, y, out controlID, out focused);
            return m_ThumbnailView.HitTest (x, y, out controlID, out focused);
        }

        public override void OnAction(Action action)
        {
          if (m_ViewMode==ViewMode.Filmstrip)
              m_FilmStripView.OnAction( action);
          else if (m_ViewMode==ViewMode.List)
              m_ListView.OnAction( action);
          else 
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
              m_ListView.OnMessage (message);
              m_ThumbnailView.OnMessage (message);
              m_FilmStripView.OnMessage (message);
              return true;
            }
            if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
            {
              m_ListView.OnMessage (message);
              m_ThumbnailView.OnMessage (message);
              m_FilmStripView.OnMessage (message);
              return true;
            }
            if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
            {
              m_ListView.OnMessage (message);
              m_ThumbnailView.OnMessage (message);
              m_FilmStripView.OnMessage (message);
              return true;
            }
          }
          if (m_ViewMode==ViewMode.Filmstrip)
            return m_FilmStripView.OnMessage (message);
          if (m_ViewMode==ViewMode.List)
              return m_ListView.OnMessage (message);
          return m_ThumbnailView.OnMessage( message);
        }

        public override bool CanFocus()
        {
          if (m_ViewMode==ViewMode.Filmstrip)
            return m_FilmStripView.CanFocus ();
          if (m_ViewMode==ViewMode.List)
              return m_ListView.CanFocus ();
          return m_ThumbnailView.CanFocus( );
        }

        public override bool Focus
        {
          get { 
            if (m_ViewMode==ViewMode.Filmstrip)
              return m_FilmStripView.Focus ;
            if (m_ViewMode==ViewMode.List)
              return m_ListView.Focus;
            return m_ThumbnailView.Focus;
          }
          set 
          { 
            if (m_ViewMode==ViewMode.Filmstrip)
              m_FilmStripView.Focus=value ;
            else if (m_ViewMode==ViewMode.List)
              m_ListView.Focus=value;
            else m_ThumbnailView.Focus=value;
          }
        }

        public override bool InControl(int x, int y, out int controlID)
        {
          if (m_ViewMode==ViewMode.Filmstrip)
            return m_FilmStripView.InControl (x, y, out controlID);
          if (m_ViewMode==ViewMode.List)
              return m_ListView.InControl (x, y, out controlID);
          return m_ThumbnailView.InControl (x, y, out controlID);
        }

        public void Sort(System.Collections.IComparer comparer)
        {
          m_ListView.Sort(comparer);
          m_ThumbnailView.Sort(comparer);
          m_FilmStripView.Sort(comparer);
        }

        public void Add(GUIListItem item)
        {
          m_ListView.Add(item);
          m_ThumbnailView.Add(item);
          m_FilmStripView.Add(item);
        }
        void UpdateView()
        {
          if (m_ViewMode==ViewMode.Filmstrip)
          {
            base.XPosition=m_FilmStripView.XPosition;
            base.YPosition=m_FilmStripView.YPosition;
            base.Width=m_FilmStripView.Width;
            base.Height=m_FilmStripView.Height;
            m_FilmStripView.IsVisible=true;
            m_ListView.IsVisible=false;
            m_ThumbnailView.IsVisible=false;
          }       
          else if (m_ViewMode==ViewMode.List)
          {
            base.XPosition=m_ListView.XPosition;
            base.YPosition=m_ListView.YPosition;
            base.Width=m_ListView.Width;
            base.Height=m_ListView.Height;
            m_ListView.IsVisible=true;
            m_ThumbnailView.IsVisible=false;
            m_FilmStripView.IsVisible=false;
          }
          else
          {
            base.XPosition=m_ThumbnailView.XPosition;
            base.YPosition=m_ThumbnailView.YPosition;
            base.Width=m_ThumbnailView.Width;
            base.Height=m_ThumbnailView.Height;
            m_ThumbnailView.ShowBigIcons(m_ViewMode==ViewMode.LargeIcons);
            m_ThumbnailView.IsVisible=true;
            m_ListView.IsVisible=false;
            m_FilmStripView.IsVisible=false;
          }
        }

        public override void DoUpdate()
        {
          if (m_ViewMode==ViewMode.List)
          {
            m_ListView.XPosition=XPosition;
            m_ListView.YPosition=YPosition;
            m_ListView.Width=Width;
            m_ListView.Height=Height;
            m_ListView.DoUpdate();
          }

          
          if (m_ViewMode==ViewMode.LargeIcons||m_ViewMode==ViewMode.SmallIcons)
          {
            m_ThumbnailView.XPosition=XPosition;
            m_ThumbnailView.YPosition=YPosition;
            m_ThumbnailView.Width=Width;
            m_ThumbnailView.Height=Height;
            m_ThumbnailView.DoUpdate();
          }

          
          if (m_ViewMode==ViewMode.Filmstrip)
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
        m_ListView.StorePosition();
        m_ThumbnailView.StorePosition();
        m_FilmStripView.StorePosition();
        
        base.StorePosition();
      }

      public override void ReStorePosition()
      {
        m_ListView.ReStorePosition();
        m_ThumbnailView.ReStorePosition();
        m_FilmStripView.ReStorePosition();
        
        base.ReStorePosition();
      }

      public override void Animate(Animator animator)
      {
        m_ListView.Animate(animator);
        m_ThumbnailView.Animate(animator);
        m_FilmStripView.Animate(animator);
        base.Animate(animator);
      }
		
	}
}
