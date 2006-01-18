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
using System.Windows.Forms;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implementation of a GUIverticalScrollbar.
  /// </summary>
  public class GUIverticalScrollbar : GUIControl
  {
	  [XMLSkinElement("buddycontrol")]		protected int      m_iBuddyControl=-1;
	  [XMLSkinElement("scrollbarbg")]		protected string   m_strBackground;
	  [XMLSkinElement("scrollbartop")]		protected string   m_strTopTexture;
	  [XMLSkinElement("scrollbarbottom")]	protected string	 m_strBottomTexture;
    GUIImage m_guiBackground=null;
    GUIImage m_guiTop=null;
    GUIImage m_guiBottom=null;
    float    m_fPercent=0;
    int      m_iStartY=0;
    int      m_iEndY=0;
    int      m_iYStartKnob=0;
    bool     m_bSendNotifies=true;

    public GUIverticalScrollbar(int dwParentID) : base(dwParentID)
    {
    }
		/// <summary>
		/// The constructor of the GUIverticalScrollbar class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strBackGroundTexture">The background texture of the scrollbar.</param>
		/// <param name="strTopTexture">The top texture of the scrollbar indicator.</param>
		/// <param name="strBottomTexture">The bottom texture of the scrolbar indicator.</param>

	  public GUIverticalScrollbar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strBackGroundTexture,string strTopTexture,string strBottomTexture)
		  :base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
	  {
		  m_strBackground =strBackGroundTexture;
		  m_strTopTexture =strTopTexture;
		  m_strBottomTexture = strBottomTexture;
		  FinalizeConstruction();
	  }
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction();
		  m_guiBackground = new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height,m_strBackground,0);
		  m_guiTop = new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height,m_strTopTexture,0);
		  m_guiBottom = new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height,m_strBottomTexture,0);

      m_guiBackground.ParentControl = this;
      m_guiTop.ParentControl = this;
      m_guiBottom.ParentControl = this;
	  }


		/// <summary>
		/// Renders the control.
		/// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
        if (Disabled) return;
      }
      if (!GUIGraphicsContext.MouseSupport)
      {
        IsVisible=false;
        return;
      }

      int iHeight=_height;
      m_guiBackground.Height=iHeight;
      m_guiBackground.Render(timePassed);

      float fPercent= (float)m_fPercent;
      float fPosYOff= (fPercent/100.0f);

			m_iStartY    =m_guiBackground.YPosition;
			m_iEndY      =m_iStartY+m_guiBackground.Height;

      int iKnobHeight = (int)(m_guiTop.TextureHeight);
			fPosYOff    *= (float) (m_iEndY-m_iStartY-iKnobHeight);
      
      m_iYStartKnob=m_iStartY+(int)fPosYOff;
      int iXPos=m_guiBackground.XPosition+ ((m_guiBackground.Width / 2) - (m_guiTop.TextureWidth ));
      int iYPos=m_iYStartKnob;

      m_guiTop.SetPosition(iXPos,iYPos);
      m_guiTop.Height=m_guiTop.TextureHeight;
      m_guiTop.Width=m_guiTop.TextureWidth;
      m_guiTop.Render(timePassed);

      iXPos += m_guiTop.TextureWidth;
      m_guiBottom.SetPosition(iXPos,iYPos);
      m_guiBottom.Height=m_guiBottom.TextureHeight;
      m_guiBottom.Width=m_guiTop.TextureWidth;
      m_guiBottom.Render(timePassed);

    }

		
		/// <summary>
		/// Checks if the control can focus.
		/// </summary>
		/// <returns>false</returns>
    public override bool  CanFocus()
    {
      return false;
    }

		/// <summary>
		/// Get/set the percentage the scrollbar indicates.
		/// </summary>
    public float Percentage
    {
      get { return m_fPercent;}
      set { 
        m_fPercent=value;
        if (m_fPercent<0) m_fPercent=0;
        if (m_fPercent>100) m_fPercent=100;
      }
    }

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      m_guiBackground.FreeResources();
      m_guiBottom.FreeResources();
      m_guiTop.FreeResources();
    }

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
		public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_guiBackground.PreAllocResources();
      m_guiBottom.PreAllocResources();
      m_guiTop.PreAllocResources();
    }

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      m_guiBackground.AllocResources();
      m_guiBottom.AllocResources();
      m_guiTop.AllocResources();

    }

		/// <summary>
		/// Gets the name of the backgroundtexture.
		/// </summary>
    public string BackGroundTextureName
    {
      get { return m_guiBackground.FileName;}
    }

		/// <summary>
		/// Gets the name of the top texture of the scrollbar indicator.
		/// </summary>
    public string BackTextureTopName
    {
      get { return m_guiTop.FileName;}
    }

		/// <summary>
		/// Gets the name of the bottom texture of the scrollbar indicator.
		/// </summary>
    public string BackTextureBottomName
    {
      get { return m_guiBottom.FileName;}
    }

		/// <summary>
		/// Get/set the buddycontrol that is being controlled by the scrollbar.
		/// </summary>
    public int BuddyControl
    {
      get { return m_iBuddyControl;}
      set { m_iBuddyControl=value;}
    }

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      if (!GUIGraphicsContext.MouseSupport)
      {
        IsVisible=false;
        return;
      }
      if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK)
      {
        int id;
        bool focus;
        if (HitTest( (int)action.fAmount1, (int)action.fAmount2, out id, out focus))
        {
          if (action.MouseButton == MouseButtons.Left)
          {
            float fHeight=(float)(m_iEndY-m_iStartY);
            m_fPercent=(action.fAmount2 - (float)m_iStartY);
            m_fPercent /= fHeight;
            m_fPercent *=100.0f;

            if (m_bSendNotifies)
            {
              GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED,WindowId,GetID, GetID,(int)m_fPercent,0,null );
              GUIGraphicsContext.SendMessage(message);
            }
          }
        }
      }

      if (action.wID==Action.ActionType.ACTION_MOUSE_MOVE)
      {
        if (action.MouseButton == MouseButtons.Left)
        {
          float fHeight=(float)(m_iEndY-m_iStartY);
          m_fPercent=(action.fAmount2 - (float)m_iStartY);
          m_fPercent /= fHeight;
          m_fPercent *=100.0f;
          if (m_bSendNotifies)
          {
            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED,WindowId,GetID, GetID,(int)m_fPercent,0,null );
            GUIGraphicsContext.SendMessage(message);
          }
        }
      }
      base.OnAction (action);
    }

		/// <summary>
		/// Checks if the x and y coordinates correspond to the current control.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>True if the control was hit.</returns>
    public override bool HitTest(int x,int y,out int controlID, out bool focused)
    {
      controlID=GetID;
      focused=Focus;
      if (!IsVisible) return false;
      if (x>=XPosition && x < XPosition+Width)
      {
        if (y>=m_iStartY && y < m_iEndY)
        {
          return true;
        }
      }
      return false;
    }

		/// <summary>
		/// Get/set the sendnotifies flag. Make sure that the control notifies when its percentage has changed. (See OnAction method).
		/// </summary>
    public bool SendNotifies
    {
      get { return m_bSendNotifies;}
      set { m_bSendNotifies=false;}
    }
    public override void DoUpdate()
    {
      m_guiBackground.Height=_height;
      m_guiBackground.DoUpdate();
      base.DoUpdate ();
    }

  }
}
