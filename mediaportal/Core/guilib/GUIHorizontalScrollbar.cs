/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
  /// Implementation of a GUIHorizontalScrollbar.
  /// </summary>
	public class GUIHorizontalScrollbar : GUIControl
	{
		[XMLSkinElement("buddycontrol")]	int      m_iBuddyControl=-1;
		[XMLSkinElement("texturebg")]		string   m_strBackground;
		[XMLSkinElement("lefttexture")]		string   m_strLeft;
		[XMLSkinElement("righttexture")]	string	 m_strRight;
											GUIImage m_guiBackground=null;
											GUIImage m_guiLeft=null;
											GUIImage m_guiRight=null;
											float    m_fPercent=0;
											int      m_iStartX=0;
											int      m_iEndX=0;
											int      m_iKnobWidth=0;
											int      m_iXStartKnob=0;
		
		public GUIHorizontalScrollbar(int dwParentID) : base(dwParentID)
		{
		}
		/// <summary>
		/// The constructor of the GUIHorizontalScrollbar class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strBackGroundTexture">The background texture of the scrollbar.</param>
		/// <param name="strLeftTexture">The left texture of the scrollbar indicator.</param>
		/// <param name="strRightTexture">The right texture of the scrolbar indicator.</param>
		public GUIHorizontalScrollbar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strBackGroundTexture,string strLeftTexture,string strRightTexture)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			m_strBackground = strBackGroundTexture;
			m_strRight = strRightTexture;
			m_strLeft = strLeftTexture;
			FinalizeConstruction();
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
      if (m_strBackground==null) m_strBackground="";
      if (m_strRight==null) m_strRight="";
      if (m_strLeft==null) m_strLeft="";
			m_guiBackground	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strBackground,0);
			m_guiLeft		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strLeft,0);
			m_guiRight		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strRight,0);
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		public override void Render(float timePassed)
		{
			if (!IsVisible) return;
			if (Disabled) return;
			if (!GUIGraphicsContext.MouseSupport)
			{
				IsVisible=false;
				return;
			}

			int iHeight=m_guiBackground.Height;
			m_guiBackground.Render(timePassed);
			m_guiBackground.Height=iHeight;

			float fPercent= (float)m_fPercent;
			float fPosXOff= (fPercent/100.0f);

			m_iKnobWidth = (int)(2*m_guiLeft.TextureWidth);
			int inset=4;
			GUIGraphicsContext.ScaleHorizontal(ref inset);
			inset+=(m_iKnobWidth/2);
			
			inset=0;
			m_iStartX    =inset+m_guiBackground.XPosition;
			m_iEndX      =m_iStartX+(m_guiBackground.Width-inset);

			fPosXOff    *= (float) (m_iEndX-m_iStartX-m_iKnobWidth);
      
			m_iXStartKnob=m_iStartX+(int)fPosXOff;
			int iXPos=m_iXStartKnob;
			int iYPos= m_guiBackground.YPosition + ((m_guiBackground.Height / 2) - (m_guiLeft.TextureHeight / 2));

			m_guiLeft.SetPosition(iXPos,iYPos);
			m_guiLeft.Height=m_guiLeft.TextureHeight;
			m_guiLeft.Width=m_guiLeft.TextureWidth;
      m_guiLeft.Render(timePassed);

			iXPos += m_guiLeft.TextureWidth;
			m_guiRight.SetPosition(iXPos,iYPos);
			m_guiRight.Height=m_guiRight.TextureHeight;
			m_guiRight.Width=m_guiLeft.TextureWidth;
      m_guiRight.Render(timePassed);

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
			set { m_fPercent=value;
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
			m_guiRight.FreeResources();
			m_guiLeft.FreeResources();
		}

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
		public override void PreAllocResources()
		{
			base.PreAllocResources();
			m_guiBackground.PreAllocResources();
			m_guiRight.PreAllocResources();
			m_guiLeft.PreAllocResources();
		}

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
		public override void AllocResources()
		{
			base.AllocResources();
			m_guiBackground.AllocResources();
			m_guiRight.AllocResources();
			m_guiLeft.AllocResources();

		}

		/// <summary>
		/// Gets the name of the backgroundtexture.
		/// </summary>
		public string BackGroundTextureName
		{
			get { return m_guiBackground.FileName;}
		}
		
		/// <summary>
		/// Gets the name of the left texture of the scrollbar indicator.
		/// </summary>
		public string BackTextureLeftName
		{
			get { return m_guiLeft.FileName;}
		}

		/// <summary>
		/// Gets the name of the right texture of the scrollbar indicator.
		/// </summary>
		public string BackTextureRightName
		{
			get { return m_guiRight.FileName;}
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
						float fWidth=(float)(m_iEndX-m_iStartX);
						m_fPercent=(action.fAmount1 - (float)m_iStartX);
						m_fPercent /= fWidth;
						m_fPercent *=100.0f;
            
						GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED,WindowId,GetID, GetID,(int)m_fPercent,0,null );
						GUIGraphicsContext.SendMessage(message);

					}
				}
			}

			if (action.wID==Action.ActionType.ACTION_MOUSE_MOVE)
			{
				if (action.MouseButton == MouseButtons.Left)
				{
					float fWidth=(float)(m_iEndX-m_iStartX);
					m_fPercent=(action.fAmount1 - (float)m_iStartX);
					m_fPercent /= fWidth;
					m_fPercent *=100.0f;
					GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED,WindowId,GetID, GetID,(int)m_fPercent,0,null );
					GUIGraphicsContext.SendMessage(message);

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
			if (x>=m_iStartX && x < m_iEndX)
			{
				if (y>=YPosition && y < YPosition+Height)
				{
					return true;
				}
			}
			return false;
		}
	}
}
