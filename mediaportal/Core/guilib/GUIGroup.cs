using System;
using System.Collections;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A class which implements a group
	/// A group can hold 1 or more controls
	/// and apply an animation to the entire group
	/// </summary>
	public class GUIGroup: GUIControl
	{
		//TODO: add comments
		// Animation type
    [XMLSkinElement("animation")] Animator.AnimationType m_Animation=Animator.AnimationType.None;
		protected ArrayList     m_Controls = new ArrayList(); //array list holding all controls
		protected bool          m_bStart=false;								//boolean indicating if a new animation should be started
    protected Animator      m_animator;										//class which does the animations

		public GUIGroup()
		{
		}
		public GUIGroup(int dwParentID) : base(dwParentID)
		{
		}

		/// <summary>
		/// Get/set animation type
		/// </summary>
    public Animator.AnimationType Animation
    {
      get { return m_Animation;}
      set { 
        m_Animation=value;
      }
    }

    public override void OnInit()
    {
      m_bStart   = true;
      m_animator = new Animator(m_Animation);
    }

    public void AddControl(GUIControl control)
    {
        m_Controls.Add(control);
    }

    public int Count
    {
        get { return m_Controls.Count;}
    }

    public GUIControl this[int index]
    {
        get { 
					if (index<=0 || index>=m_Controls.Count) return null;
					return (GUIControl)m_Controls[index]; 
				}
    }

    public override void Render(long timePassed)
    {
      if (GUIGraphicsContext.Animations)
      {
        if (m_animator!=null)
        {
          if (m_bStart)
          {
            m_bStart=false;
            StorePosition();
          }

          for (int i=0; i < m_Controls.Count;++i)
          {
            GUIControl cntl=m_Controls[i] as GUIControl;
            if (cntl!=null) cntl.Animate(timePassed,m_animator);
          }
          m_animator.Advance(timePassed);
        }
      }

      for (int i=0; i < m_Controls.Count;++i)
      {
        ((GUIControl)m_Controls[i]).Render(timePassed);
      }
			
			if (m_animator!=null)
			{
				if (m_animator.IsDone())
				{
					ReStorePosition();
					m_animator=null;
				}
			}
    }

    public override void FreeResources()
		{
			if (m_animator!=null)
			{
				ReStorePosition();
				m_animator=null;
			}
      for (int i=0; i < m_Controls.Count;++i)
      {
        ((GUIControl)m_Controls[i]).FreeResources();
      }
    }

    public override void AllocResources()
    {
      for (int i=0; i < m_Controls.Count;++i)
      {
        ((GUIControl)m_Controls[i]).AllocResources();
      }
    }

    public override void PreAllocResources()
    {
      for (int i=0; i < m_Controls.Count;++i)
      {
        ((GUIControl)m_Controls[i]).PreAllocResources();
      }
    }

    public override GUIControl GetControlById(int ID)
    {
      for (int i=0; i < m_Controls.Count;++i)
      {
        GUIControl cntl = ((GUIControl)m_Controls[i]).GetControlById(ID);
        if (cntl!=null) return cntl;
      }
      return null;
    }

    public override bool NeedRefresh()
    {
      for (int i=0; i < m_Controls.Count;++i)
      {
        if ( ((GUIControl)m_Controls[i]).NeedRefresh() ) return true;
      }
      return false;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID=-1;
      focused=false;
      for (int i=0; i < m_Controls.Count;++i)
      {
        if ( ((GUIControl)m_Controls[i]).HitTest(x,y, out controlID,out  focused) ) return true;
      }
      return false;
    }

    public override void OnAction(Action action)
    {
      for (int i=0; i < m_Controls.Count;++i)
      {
        if ( ((GUIControl)m_Controls[i]).Focus ) 
        {
          ((GUIControl)m_Controls[i]).OnAction(action);
        }
      }
    }

    public void Remove(int dwId)
    {
      int index = 0;
      foreach (GUIControl control in m_Controls)
      {
        GUIGroup grp = control as GUIGroup;
        if (grp !=null)
        {
          grp.Remove(dwId);
        }
        else
        {
          if (control.GetID == dwId)
          {
						if (index >=0 && index < m_Controls.Count)
							m_Controls.RemoveAt(index);
            return;
          }
        }
        index++;
      }
    }
    public int GetFocusControlId()
    {
      for (int x = 0; x < m_Controls.Count; ++x)
      {
        GUIGroup grp = m_Controls[x] as GUIGroup;
        if (grp!=null)
        {
          int iFocusedControlId=grp.GetFocusControlId();
          if (iFocusedControlId>=0) return iFocusedControlId;
        }
        else
        {
          if (((GUIControl)m_Controls[x]).Focus) return ((GUIControl)m_Controls[x]).GetID;
        }
      }
      return - 1;
    }

    public override void DoUpdate()
    {
      for (int x = 0; x < m_Controls.Count; ++x)
      {
        ((GUIControl)m_Controls[x]).DoUpdate();
      }
    }

    
    public ArrayList GUIControls
    {
      get 
      {
        return m_Controls;
      }
    }

    public override void StorePosition()
    {
      for (int x = 0; x < m_Controls.Count; ++x)
      {
        ((GUIControl)m_Controls[x]).StorePosition();
      }
      
      base.StorePosition();
    }

    public override void ReStorePosition()
    {
      for (int x = 0; x < m_Controls.Count; ++x)
      {
        ((GUIControl)m_Controls[x]).ReStorePosition();
      }
      
      base.ReStorePosition();
    }

    public override void Animate(long timePassed,Animator animator)
    {
      for (int x = 0; x < m_Controls.Count; ++x)
      {
        ((GUIControl)m_Controls[x]).Animate(timePassed,animator);
      }
      base.Animate(timePassed,animator);
    }

		/// <summary>
		/// Property to get/set the id of the window 
		/// to which this control belongs
		/// </summary>
		public override int WindowId
		{
			get { return m_iWindowID; }
			set { 
				m_iWindowID = value; 
				for (int x = 0; x < m_Controls.Count; ++x)
				{
					((GUIControl)m_Controls[x]).WindowId=value;
				}
			}
		}

	}
}
