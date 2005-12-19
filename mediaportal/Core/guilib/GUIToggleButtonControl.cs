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
  /// 
  /// </summary>
	public class GUIToggleButtonControl : GUIControl
	{
		[XMLSkinElement("textureFocus")]	protected string	m_strImgFocusTexture="";
		[XMLSkinElement("textureNoFocus")]	protected string	m_strImgNoFocusTexture="";
		[XMLSkinElement("AltTextureFocus")]	protected string	m_strImgAltFocusTexture="";
		[XMLSkinElement("AltTextureNoFocus")]	
											protected string m_strImgAltNoFocusTexture="";
		protected GUIImage                m_imgFocus=null;
		protected GUIImage                m_imgNoFocus=null;  
		protected GUIImage                m_imgAltFocus=null;
		protected GUIImage                m_imgAltNoFocus=null;  
		protected int                    m_dwFrameCounter=0;
		[XMLSkinElement("font")]			protected string	m_strFontName;
		[XMLSkinElement("label")]			protected string	m_strLabel="";
		protected GUIFont 								m_pFont=null;
		[XMLSkinElement("textcolor")]		protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("disabledcolor")]	protected long		m_dwDisabledColor=0xFF606060;
		[XMLSkinElement("hyperlink")]		protected int       m_lHyperLinkWindowID=-1;
		
		protected string										m_strScriptAction="";
		[XMLSkinElement("textXOff")]		protected int       m_iTextOffsetX=0;
		[XMLSkinElement("textYOff")]		protected int       m_iTextOffsetY=0;
	
		public GUIToggleButtonControl(int dwParentID) : base(dwParentID)
		{
		}
		public GUIToggleButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,  string strTextureFocus, string strTextureNoFocus,  string strAltTextureFocus, string strAltTextureNoFocus)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			m_strImgFocusTexture = strTextureFocus;
			m_strImgNoFocusTexture = strTextureNoFocus;
			m_strImgAltFocusTexture = strAltTextureFocus;
			m_strImgAltNoFocusTexture = strAltTextureNoFocus;
			m_bSelected=false;
			FinalizeConstruction();
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			
			m_imgFocus     =new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strImgFocusTexture ,0);
			m_imgNoFocus   =new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strImgNoFocusTexture,0);
			m_imgAltFocus  =new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strImgAltFocusTexture,0);
			m_imgAltNoFocus=new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight, m_strImgAltNoFocusTexture,0);
			if (m_strFontName!="" && m_strFontName!="-")
				m_pFont=GUIFontManager.GetFont(m_strFontName);
			GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
		}

		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution ();
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextOffsetX, ref m_iTextOffsetY);
		}

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }

      if (Focus)
      {
        int dwAlphaCounter = m_dwFrameCounter+2;
        int dwAlphaChannel;
        if ((dwAlphaCounter%128)>=64)
          dwAlphaChannel = dwAlphaCounter%64;
        else
          dwAlphaChannel = 63-(dwAlphaCounter%64);

        dwAlphaChannel += 192;
        SetAlpha(dwAlphaChannel );
        if (m_bSelected)
          m_imgFocus.Render(timePassed);
        else
          m_imgAltFocus.Render(timePassed);
        m_dwFrameCounter++;
      }
      else 
      {
        SetAlpha(0xff);
        if (m_bSelected)
          m_imgNoFocus.Render(timePassed);
        else
          m_imgAltNoFocus.Render(timePassed);  
      }

      if (m_strLabel.Length > 0 && m_pFont!=null)
      {
        if (Disabled )
          m_pFont.DrawText((float)m_iTextOffsetX+m_dwPosX, (float)m_iTextOffsetY+m_dwPosY,m_dwDisabledColor,m_strLabel,GUIControl.Alignment.ALIGN_LEFT,-1);
        else
          m_pFont.DrawText((float)m_iTextOffsetX+m_dwPosX, (float)m_iTextOffsetY+m_dwPosY,m_dwTextColor,m_strLabel,GUIControl.Alignment.ALIGN_LEFT,-1);
      }
    }

    public override void OnAction( Action action) 
    {
      base.OnAction(action);
      GUIMessage message;
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          m_bSelected=!m_bSelected;
          if (m_lHyperLinkWindowID >=0)
          {
            GUIWindowManager.ActivateWindow(m_lHyperLinkWindowID);
            return;
          }
          // button selected.
          // send a message
          int iParam=1;
          if (!m_bSelected) iParam=0;
          message=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID,iParam,0,null );
          GUIGraphicsContext.SendMessage(message);
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      if ( message.TargetControlId==GetID )
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          m_strLabel=message.Label ;

          return true;
        }
      }
      if (base.OnMessage(message)) return true;
      return false;
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_imgFocus.PreAllocResources();
      m_imgNoFocus.PreAllocResources();
      m_imgAltFocus.PreAllocResources();
      m_imgAltNoFocus.PreAllocResources();
    }
    public override void AllocResources()
    {
      base.AllocResources();
      m_pFont=GUIFontManager.GetFont(m_strFontName);
      m_dwFrameCounter=0;
      m_imgFocus.AllocResources();
      m_imgNoFocus.AllocResources();
      m_imgAltFocus.AllocResources();
      m_imgAltNoFocus.AllocResources();
      m_dwWidth=m_imgFocus.Width;
      m_dwHeight=m_imgFocus.Height;
    }
    public override void FreeResources()
    {
      base.FreeResources();
      m_imgFocus.FreeResources();
      m_imgNoFocus.FreeResources();
      m_imgAltFocus.FreeResources();
      m_imgAltNoFocus.FreeResources();
    }
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      base.SetPosition(dwPosX, dwPosY);
      m_imgFocus.SetPosition(dwPosX, dwPosY);
      m_imgNoFocus.SetPosition(dwPosX, dwPosY);
      m_imgAltFocus.SetPosition(dwPosX, dwPosY);
      m_imgAltNoFocus.SetPosition(dwPosX, dwPosY);
    }
    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      m_imgFocus.SetAlpha(dwAlpha);
      m_imgNoFocus.SetAlpha(dwAlpha);
      m_imgAltFocus.SetAlpha(dwAlpha);
      m_imgAltNoFocus.SetAlpha(dwAlpha);
    }

    public long DisabledColor
    {
      get { return m_dwDisabledColor;}
      set {m_dwDisabledColor=value;}
    }
    public string TexutureNoFocusName
    { 
      get { return m_imgNoFocus.FileName;} 
    }

    public string TexutureFocusName
    { 
      get {return m_imgFocus.FileName;} 
    }
    public string AltTexutureNoFocusName
    { 
      get { return m_imgAltNoFocus.FileName;} 
    }

    public string AltTexutureFocusName
    { 
      get {return m_imgAltFocus.FileName;} 
    }
		
    public long	TextColor 
    { 
      get { return m_dwTextColor;}
      set { m_dwTextColor=value;}
    }

    public string FontName
    { 
      get { return m_strFontName; }
      set { 
        if (value==null) return;
        m_strFontName=value;
        m_pFont=GUIFontManager.GetFont(m_strFontName);
      }
    }

    public void SetLabel( string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null) return;
      if (strLabel==null) return;
      m_strLabel=strLabel;
      m_dwTextColor=dwColor;
      if (strFontName!="" && strFontName!="-")
      {
        m_strFontName=strFontName;
        m_pFont=GUIFontManager.GetFont(m_strFontName);
      }
    }

    public string Label
    { 
      get { return m_strLabel; }
      set { m_strLabel=value;}
    }

    public int HyperLink
    { 
      get { return m_lHyperLinkWindowID;}
      set {m_lHyperLinkWindowID=value;}
    }
    public string ScriptAction  
    { 
      get { return m_strScriptAction; }
      set { m_strScriptAction=value; }
    }

    protected override void  Update() 
    {
      base.Update();
  
      m_imgFocus.Width=m_dwWidth;
      m_imgFocus.Height=m_dwHeight;

      m_imgNoFocus.Width=m_dwWidth;
      m_imgNoFocus.Height=m_dwHeight;
      
      m_imgAltFocus.Width=m_dwWidth;
      m_imgAltFocus.Height=m_dwHeight;

      m_imgAltNoFocus.Width=m_dwWidth;
      m_imgAltNoFocus.Height=m_dwHeight;

      m_imgFocus.SetPosition(m_dwPosX, m_dwPosY);
      m_imgNoFocus.SetPosition(m_dwPosX, m_dwPosY);
      m_imgAltFocus.SetPosition(m_dwPosX, m_dwPosY);
      m_imgAltNoFocus.SetPosition(m_dwPosX, m_dwPosY);

    }
		/// <summary>
		/// Get/set the X-offset of the label.
		/// </summary>
		public int TextOffsetX
		{
			get { return m_iTextOffsetX;}
			set { m_iTextOffsetX=value;}
		}
		/// <summary>
		/// Get/set the Y-offset of the label.
		/// </summary>
		public int TextOffsetY
		{
			get { return m_iTextOffsetY;}
			set { m_iTextOffsetY=value;}
		}


  }
}
