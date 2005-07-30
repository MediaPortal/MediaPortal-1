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
using System.Drawing;
using System.Collections;
using System.Windows.Forms; // used for Keys definition
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The implementation of a GUICheckListControl
	/// </summary>
	public class GUICheckListControl : GUIListControl
	{
		[XMLSkinElement("textureCheckmarkNoFocus")] protected string	m_strCheckMarkNoFocus;
		[XMLSkinElement("textureCheckmark")]	protected string	m_strCheckMark;
		[XMLSkinElement("MarkWidth")]			protected int		m_iCheckMarkWidth;
		[XMLSkinElement("MarkHeight")]			protected int		m_iCheckMarkHeight;
		[XMLSkinElement("MarkOffsetX")]			protected int		markOffsetX;
		[XMLSkinElement("MarkOffsetY")]			protected int		markOffsetY;
				
		
		public GUICheckListControl(int dwParentID) : base(dwParentID)
		{
		}
		/// <summary>
		/// The constructor of the GUICheckListControl.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="dwSpinWidth">TODO </param>
		/// <param name="dwSpinHeight">TODO</param>
		/// <param name="strUp">The name of the scroll up unfocused texture.</param>
		/// <param name="strDown">The name of the scroll down unfocused texture.</param>
		/// <param name="strUpFocus">The name of the scroll up focused texture.</param>
		/// <param name="strDownFocus">The name of the scroll down unfocused texture.</param>
		/// <param name="dwSpinColor">TODO </param>
		/// <param name="dwSpinX">TODO </param>
		/// <param name="dwSpinY">TODO </param>
		/// <param name="strFont">The font used in the spin control.</param>
		/// <param name="dwTextColor">The color of the text.</param>
		/// <param name="dwSelectedColor">The color of the text when it is selected.</param>
		/// <param name="strButton">The name of the unfocused button texture.</param>
		/// <param name="strButtonFocus">The name of the focused button texture.</param>
		/// <param name="strScrollbarBackground">The name of the background of the scrollbar texture.</param>
		/// <param name="strScrollbarTop">The name of the top of the scrollbar texture.</param>
		/// <param name="strScrollbarBottom">The name of the bottom of the scrollbar texture.</param>
		public GUICheckListControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, 
			int dwSpinWidth, int dwSpinHeight, 
			string strUp, string strDown, 
			string strUpFocus, string strDownFocus, 
			long dwSpinColor, int dwSpinX, int dwSpinY, 
			string strFont, long dwTextColor, long dwSelectedColor, 
			string strButton, string strButtonFocus, 
			string strScrollbarBackground, string strScrollbarTop, string strScrollbarBottom)
			: base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, 
			dwSpinWidth, dwSpinHeight, 
			strUp, strDown, 
			strUpFocus, strDownFocus, 
			dwSpinColor, dwSpinX, dwSpinY, 
			strFont, dwTextColor, dwSelectedColor, 
			strButton, strButtonFocus, 
			strScrollbarBackground, strScrollbarTop, strScrollbarBottom)

		{
			FinalizeConstruction();
		}		

		protected override void AllocButtons()
		{
			for (int i=0; i < m_iItemsPerPage;++i)
			{
				GUICheckButton cntl = new GUICheckButton(m_dwControlID, 0, m_dwSpinX, m_dwSpinY, m_dwWidth, m_iItemHeight, m_strButtonFocused, m_strButtonUnfocused,m_strCheckMark,m_strCheckMarkNoFocus,m_iCheckMarkWidth,m_iCheckMarkHeight);
				cntl.AllocResources();
				cntl.CheckOffsetX=markOffsetX;
				cntl.CheckOffsetY=markOffsetY;
				m_imgButton.Add(cntl);
			}
		}
		protected override void OnLeft()
		{
			base.OnLeft();
			UpdateUpDownControls();
		}

		protected override void OnRight()
		{
			base.OnRight ();
			UpdateUpDownControls();
		}


		protected override void OnUp()
		{
			base.OnUp();
			UpdateUpDownControls();
		}
		protected override void OnDown()
		{
			base.OnDown();
			UpdateUpDownControls();
		}
		void UpdateUpDownControls()
		{
			for (int i=0; i < m_iItemsPerPage;++i)
			{
				bool selected=false;
				if (i < m_vecItems.Count)
				{
					GUIListItem item = (GUIListItem)m_vecItems[i+m_iOffset];
					if (item.Selected) selected=true;
				}
				GUIControl btn = (GUIControl)m_imgButton[i];
				btn.Focus=false;
				btn.Selected=selected;
				if (i==m_iCursorY)
				{
					btn.Focus=true;
				}
			}
		}

		protected override void RenderButton(float timePassed, int buttonNr, int x, int y, bool gotFocus)
		{
			if (buttonNr + m_iOffset>=0 && buttonNr + m_iOffset < m_vecItems.Count)
			{
				GUIListItem item=(GUIListItem )m_vecItems[buttonNr + m_iOffset];
				GUICheckButton cntl = (GUICheckButton)m_imgButton[buttonNr];
				cntl.Selected=item.Selected;
				
			}
			base.RenderButton (timePassed, buttonNr, x, y, gotFocus);
		}

		public override bool OnMessage(GUIMessage message)
		{
			bool result= base.OnMessage (message);
			if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECT)
			{
				
				UpdateUpDownControls();
			}
			return result;
		}




	}
}
