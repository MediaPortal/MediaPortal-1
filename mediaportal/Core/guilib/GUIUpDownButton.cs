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
using System.Collections;
using System.Drawing;
using System.Diagnostics;


namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for GUIUpDownButton.
	/// </summary>
	public class GUIUpDownButton : GUIButtonControl
	{
		[XMLSkinElement("spinColor")]		protected long		m_dwSpinColor;
		[XMLSkinElement("spinHeight")]		protected int		m_dwSpinHeight;
		[XMLSkinElement("spinWidth")]		protected int		m_dwSpinWidth;
		[XMLSkinElement("spinPosX")]		protected int		m_dwSpinX;
		[XMLSkinElement("spinPosY")]		protected int		m_dwSpinY;
		[XMLSkinElement("textureUp")]		protected string	m_strUp="";
		[XMLSkinElement("textureDown")]		protected string	m_strDown="";
		[XMLSkinElement("textureUpFocus")]	protected string	m_strUpFocus=""; 
		[XMLSkinElement("textureDownFocus")]protected string	m_strDownFocus="";

		GUISpinControl spinControl;
		public GUIUpDownButton(int dwParentID) : base(dwParentID)
		{
		}

		public GUIUpDownButton(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,  string strTextureFocus, string strTextureNoFocus,
			int dwSpinWidth, int dwSpinHeight, 
			string strUp, string strDown, 
			string strUpFocus, string strDownFocus, 
			long dwSpinColor, int dwSpinX, int dwSpinY)
			:base(dwParentID)
		{
			m_dwSpinWidth = dwSpinWidth;
			m_dwSpinHeight = dwSpinHeight;
			m_strUp = strUp;
			m_strDown = strDown;
			m_strUpFocus = strUpFocus;
			m_strDownFocus = strDownFocus;
			m_dwSpinColor = dwSpinColor;
			m_dwSpinX = dwSpinX;
			m_dwSpinY = dwSpinY;
			
			m_dwParentID = dwParentID;
			m_dwControlID = dwControlId;
			m_dwPosX = dwPosX;
			m_dwPosY = dwPosY;
			m_dwWidth = dwWidth;
			m_dwHeight = dwHeight;

			m_strImgFocusTexture = strTextureFocus;
			m_strImgNoFocusTexture = strTextureNoFocus;
			FinalizeConstruction();
		}

		public override void FinalizeConstruction()
		{
				base.FinalizeConstruction();
				spinControl= new GUISpinControl(m_dwControlID, 0, m_dwSpinX, m_dwSpinY, m_dwSpinWidth, m_dwSpinHeight, m_strUp, m_strDown, m_strUpFocus, m_strDownFocus, m_strFontName, m_dwSpinColor, GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, GUIControl.Alignment.ALIGN_RIGHT);
				spinControl.WindowId=WindowId;
				spinControl.AutoCheck=false;
		}
		
		public override void AllocResources()
		{
			base.AllocResources ();
			spinControl.AllocResources();
		}

		public override void FreeResources()
		{
			base.FreeResources ();
			spinControl.FreeResources();
		}



		/// <summary>
		/// Renders the GUIButtonControl.
		/// </summary>
		public override void Render(float timePassed)
		{
			// Do not render if not visible.
			if (GUIGraphicsContext.EditMode==false)
			{
				if (!IsVisible ) return;
			}

			// The GUIButtonControl has the focus
			if (Focus)
			{
				//render the focused image
				m_imgFocus.Render(timePassed);
				GUIPropertyManager.SetProperty("#highlightedbutton", Label);
			}
			else 
			{
				//render the non-focused image
				m_imgNoFocus.Render(timePassed);  		
			}

			// render the text on the button
			if (Disabled )
			{
				m_label.Label=m_strLabel;
				m_label.TextColor=m_dwDisabledColor;
				m_label.SetPosition(m_iTextOffsetX+m_dwPosX, m_iTextOffsetY+m_dwPosY);
				m_label.Render(timePassed);
			}
			else
			{
				m_label.Label=m_strLabel;
				m_label.TextColor=m_dwTextColor;
				m_label.SetPosition(m_iTextOffsetX+m_dwPosX, m_iTextOffsetY+m_dwPosY);
				m_label.Render(timePassed);
			}
			if (spinControl!=null)
			{
				int off=5;
				GUIGraphicsContext.ScaleHorizontal(ref off);
				spinControl.SetPosition(m_imgNoFocus.XPosition+m_imgNoFocus.Width-off-2*m_dwSpinWidth, 
																m_imgNoFocus.YPosition+ (m_imgNoFocus.Height-m_dwSpinHeight)/2 );
				spinControl.Render(timePassed);
			}
		}
		public override bool HitTest(int x, int y, out int controlID, out bool focused)
		{
			if (spinControl.HitTest(x,y,out controlID, out focused)) 
			{
				spinControl.Focus=true;
				return true;
			}
			else
			{
				spinControl.Focus=false;
			}
			return base.HitTest (x, y, out controlID, out focused);
		}

		public GUISpinControl UpDownControl
		{
			get { return spinControl;}
		}
	}
}
