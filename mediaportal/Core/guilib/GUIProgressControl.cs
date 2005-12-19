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

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The implementation of a progress bar used by the OSD.
	/// The progress bar uses the following images 
	/// -) a background image 
	/// -) a left texture which presents the left part of the progress bar
	/// -) a mid texture which presents the middle part of the progress bar
	/// -) a right texture which presents the right part of the progress bar
	/// -) a label which is drawn inside the progressbar control
	/// </summary>
	public class GUIProgressControl : GUIControl
	{

		[XMLSkinElement("label")]			string   m_strProperty="";
		[XMLSkinElement("texturebg")]		string   m_strBackground;
		[XMLSkinElement("lefttexture")]		string   m_strLeft;
		[XMLSkinElement("midtexture")]		string   m_strMid;
		[XMLSkinElement("righttexture")]	string	 m_strRight;
											GUIImage m_guiBackground=null;
											GUIImage m_guiLeft=null;
											GUIImage m_guiMid=null;
											GUIImage m_guiRight=null;
    int      m_iPercent=0;
    bool                          ContainsProperty=false;
		
		
		public GUIProgressControl (int dwParentID) : base(dwParentID)
		{
		}

		/// <summary>
		/// Creates a GUIProgressControl.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strBackGroundTexture">The background texture.</param>
		/// <param name="strLeftTexture">The left side texture.</param>
		/// <param name="strMidTexture">The middle texture.</param>
		/// <param name="strRightTexture">The right side texture.</param>
		public GUIProgressControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strBackGroundTexture,string strLeftTexture,string strMidTexture,string strRightTexture)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			m_strBackground = strBackGroundTexture;
			m_strLeft = strLeftTexture;
			m_strMid = strMidTexture;
			m_strRight = strRightTexture;
			FinalizeConstruction();
		}

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			m_guiBackground = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strBackground,0);
			m_guiLeft		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strLeft,0);
			m_guiMid		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strMid,0);
			m_guiRight		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strRight,0);
			
			m_guiBackground.KeepAspectRatio=false;
			m_guiMid.KeepAspectRatio=false;
			m_guiRight.KeepAspectRatio=false;
      if (m_strProperty==null) m_strProperty=String.Empty;
      if (m_strProperty.IndexOf("#")>=0) ContainsProperty=true;
		}

    /// <summary>
    /// Update the subcontrols with the current position of the progress control
    /// </summary>
		protected override void Update()
		{
			base.Update ();
			m_guiBackground.SetPosition(m_dwPosX,m_dwPosY);
			m_guiLeft.SetPosition(m_dwPosX,m_dwPosY);
			m_guiMid.SetPosition(m_dwPosX,m_dwPosY);
			m_guiRight.SetPosition(m_dwPosX,m_dwPosY);
		}

		/// <summary>
		/// Renders the progress control.
		/// </summary>
		public override void Render(float timePassed)
		{
			if (GUIGraphicsContext.EditMode==false)
			{
				if (!IsVisible) return;
				if (Disabled) return;
			}
			
			if (ContainsProperty)
			{
				string m_strText=GUIPropertyManager.Parse(m_strProperty);
				if(m_strText!=String.Empty)
				{
					try
					{
						Percentage=Int32.Parse(m_strText);
					}
					catch(Exception){}
				}
			}
			
			// Render the background
			int iBkgHeight=m_dwHeight;
			m_guiBackground.Height=iBkgHeight;
			m_guiBackground.SetPosition(m_guiBackground.XPosition,m_guiBackground.YPosition);
			m_guiBackground.Render(timePassed);

      GUIFontManager.Present();

			int iWidthLeft=m_guiLeft.TextureWidth;
			int iHeightLeft=m_guiLeft.TextureHeight;
			int iWidthRight=m_guiRight.TextureWidth;
			int iHeightRight=m_guiRight.TextureHeight;
			GUIGraphicsContext.ScaleHorizontal(ref iWidthLeft);
			GUIGraphicsContext.ScaleHorizontal(ref iWidthRight);
			GUIGraphicsContext.ScaleVertical(ref iHeightLeft);
			GUIGraphicsContext.ScaleVertical(ref iHeightRight);
			//iHeight=20;
			int percent=m_iPercent;
			if (percent>100) percent=100;
			float fWidth = (float)percent;
			fWidth/=100.0f;
			fWidth *= (float) (m_guiBackground.Width-24-iWidthLeft-iWidthRight);

			int off=12;
			GUIGraphicsContext.ScaleHorizontal(ref off);
			int iXPos=off+m_guiBackground.XPosition;

			int iYPos= m_guiBackground.YPosition + (iBkgHeight  - iHeightLeft ) / 2;
			//m_guiLeft.SetHeight(iHeight);
			m_guiLeft.SetPosition(iXPos,iYPos);
			m_guiLeft.Height=iHeightLeft;
			m_guiLeft.Width=iWidthLeft;
      m_guiLeft.SetPosition(iXPos,iYPos);
			m_guiLeft.Render(timePassed);

			iXPos += iWidthLeft;
			if (percent>0 && (int)fWidth > 1)
			{
				m_guiMid.SetPosition(iXPos,iYPos);
				m_guiMid.Height=iHeightLeft;//m_guiMid.TextureHeight;
				m_guiMid.Width=(int)fWidth;
				m_guiMid.SetPosition(iXPos,iYPos);
				m_guiMid.Render(timePassed);
				iXPos += (int)fWidth;
			}
			//m_guiRight.SetHeight(iHeight);
			m_guiRight.SetPosition(iXPos,iYPos);
			m_guiRight.Height=iHeightRight;
			m_guiRight.Width=iWidthRight;
      m_guiRight.SetPosition(iXPos,iYPos);
			m_guiRight.Render(timePassed);
		}

		/// <summary>
		/// Returns if the control can have the focus.
		/// </summary>
		/// <returns>False</returns>
		public override bool  CanFocus()
		{
			return false;
		}

		/// <summary>
		/// Get/set the percentage the progressbar indicates.
		/// </summary>
		public int Percentage
		{
			get { return m_iPercent;}
			set { m_iPercent=value;}
		}

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources()
		{
			base.FreeResources();
			m_guiBackground.FreeResources();
			m_guiMid.FreeResources();
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
			m_guiMid.PreAllocResources();
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
			m_guiMid.AllocResources();
			m_guiRight.AllocResources();
			m_guiLeft.AllocResources();

			m_guiBackground.Filtering=false;
			m_guiMid.Filtering=false;
			m_guiRight.Filtering=false;
			m_guiLeft.Filtering=false;

			m_guiBackground.Height=25;
			m_guiRight.Height=20;
			m_guiLeft.Height=20;
			m_guiMid.Height=20;
		}

		/// <summary>
		/// Gets the filename of the background texture.
		/// </summary>
		public string BackGroundTextureName
		{
			get { return m_guiBackground.FileName;}
		}

		/// <summary>
		/// Gets the filename of the left texture.
		/// </summary>
		public string BackTextureLeftName
		{
			get { return m_guiLeft.FileName;}
		}

		/// <summary>
		/// Gets the filename of the middle texture.
		/// </summary>
		public string BackTextureMidName
		{
			get { return m_guiMid.FileName;}
		}

		/// <summary>
		/// Gets the filename of the right texture.
		/// </summary>
		public string BackTextureRightName
		{
			get { return m_guiRight.FileName;}
		}

		/// <summary>
		/// Get/set the property.
		/// The property contains text which is shown in the progress control
		/// normally this is a percentage (0%-100%)
		/// </summary>
		public string Property
		{
			get { return m_strProperty; }
			set {
        if (value!=null)
        {
          m_strProperty=value;
          if (m_strProperty.IndexOf("#")>=0) ContainsProperty=true;
          else ContainsProperty=false;
        }
      }
		}
	}
}
