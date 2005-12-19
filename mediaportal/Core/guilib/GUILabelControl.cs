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
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A GUIControl for displaying text.
	/// </summary>
	public class GUILabelControl :GUIControl
	{
		[XMLSkinElement("font")]			protected string	m_strFontName="";
		[XMLSkinElement("label")]			protected string	m_strLabel="";
		[XMLSkinElement("textcolor")]	protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("align")]			Alignment         m_dwTextAlign=Alignment.ALIGN_LEFT;
		
    string                        m_strText;
    bool                          ContainsProperty=false;
    int                           textwidth=0;
    int                           textheight=0;
    bool                          m_bUseFontCache=false;
    
    GUIFont								        m_pFont=null;
		bool													useViewport=true;
		/// <summary>
		/// The constructor of the GUILabelControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strFont">The indication of the font of this control.</param>
		/// <param name="strLabel">The text of this control.</param>
		/// <param name="dwTextColor">The color of this control.</param>
		/// <param name="dwTextAlign">The alignment of this control.</param>
		/// <param name="bHasPath">Indicates if the label is containing a path.</param>
		public GUILabelControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strFont,string strLabel, long dwTextColor,GUIControl.Alignment dwTextAlign, bool bHasPath)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			m_strLabel = strLabel;
			m_strFontName = strFont;
			m_dwTextColor = dwTextColor;
			m_dwTextAlign = dwTextAlign;
			
			FinalizeConstruction();
		}
		public GUILabelControl (int dwParentID) : base(dwParentID)
		{
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
			if (m_strFontName==null) m_strFontName=String.Empty;
      if (m_strFontName!="" && m_strFontName!="-")
				m_pFont=GUIFontManager.GetFont(m_strFontName);
			GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
      if (m_strLabel==null) m_strLabel=String.Empty;
      if (m_strLabel.IndexOf("#")>=0) ContainsProperty=true;
		}

		/// <summary>
		/// Renders the text onscreen.
		/// </summary>
		public override void Render(float timePassed)
		{
			// Do not render if not visible
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }
      if (ContainsProperty) 
      {
        string text=GUIPropertyManager.Parse(m_strLabel);
        if (m_strText!=text)
        {
          m_strText=text;
					text=null;
          textwidth=0;
          textheight=0;
          
          ClearFontCache();     
        }
      }
      else m_strText=m_strLabel;
      
      if (m_strText.Length==0) return;

      if (null!=m_pFont)
			{
        
        if (GUIGraphicsContext.graphics!=null)
        {
          if (m_dwWidth>0)
            m_pFont.DrawTextWidth(m_dwPosX,m_dwPosY,m_dwTextColor, m_strText, m_dwWidth,m_dwTextAlign);
          else
            m_pFont.DrawText(m_dwPosX,m_dwPosY,m_dwTextColor, m_strText, m_dwTextAlign,-1);
          return;
        }

        if (textwidth==0 || textheight==0)
        {
          float fW=textwidth;
          float fH=textheight;
          m_pFont.GetTextExtent(m_strText,ref fW, ref fH);
          textwidth=(int)fW;
          textheight=(int)fH;
        }

			  /*string renderText=m_strText;
        if (m_dwWidth>0)
        {
          while (textwidth > m_dwWidth && renderText.Length>0)
          {
            renderText=renderText.Substring(0,renderText.Length-1);
            float fW=textwidth;
            float fH=textheight;
            m_pFont.GetTextExtent(renderText,ref fW, ref fH);
            textwidth=(int)fW;
            textheight=(int)fH;
            
          }
        }*/

        if (m_dwTextAlign==GUIControl.Alignment.ALIGN_CENTER)
        {
         
          int xoff= (int)((m_dwWidth-textwidth)/2);
          int yoff= (int)((m_dwHeight-textheight)/2);
          m_pFont.DrawText((float)m_dwPosX+xoff, (float)m_dwPosY+yoff,m_dwTextColor,m_strText,GUIControl.Alignment.ALIGN_LEFT,m_dwWidth); 
        }
        else
        {

          if (m_dwTextAlign== GUIControl.Alignment.ALIGN_RIGHT)
          {
            if (m_dwWidth==0 || textwidth < m_dwWidth)
            {
							m_pFont.DrawText((float)m_dwPosX-textwidth, (float)m_dwPosY,m_dwTextColor,m_strText,GUIControl.Alignment.ALIGN_LEFT,-1); 
            }
            else
            {
              float fPosCX = (float)m_dwPosX;
              float fPosCY = (float)m_dwPosY;
              GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
              if (fPosCX < 0) fPosCX = 0.0f;
              if (fPosCY < 0) fPosCY = 0.0f;
              if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
              float fHeight = 60.0f;
              if (fHeight + fPosCY >= GUIGraphicsContext.Height)
                fHeight = GUIGraphicsContext.Height - fPosCY - 1;
              if (fHeight <= 0) return;

              float fwidth = m_dwWidth - 5.0f;

              if (fPosCX<=0) fPosCX=0;
              if (fPosCY<=0) fPosCY=0;
							if (fwidth<1) return;
							if (fHeight<1) return;

							/*Viewport newviewport, oldviewport;
							oldviewport = GUIGraphicsContext.DX9Device.Viewport;
							if (useViewport)
							{
								newviewport = new Viewport();
								newviewport.X = (int)fPosCX;
								newviewport.Y = (int)fPosCY;
								newviewport.Width = (int)(fwidth);
								newviewport.Height = (int)(fHeight);
								newviewport.MinZ = 0.0f;
								newviewport.MaxZ = 1.0f;
								GUIGraphicsContext.DX9Device.Viewport = newviewport;
							}*/

              m_pFont.DrawText((float)m_dwPosX-textwidth, (float)m_dwPosY,m_dwTextColor,m_strText,GUIControl.Alignment.ALIGN_LEFT,(int)fwidth); 
              //if (useViewport)
	            //  GUIGraphicsContext.DX9Device.Viewport = oldviewport;
            }
            return;
          }

          if (m_dwWidth==0 || textwidth < m_dwWidth)
          {
            m_pFont.DrawText((float)m_dwPosX, (float)m_dwPosY,m_dwTextColor,m_strText,m_dwTextAlign,m_dwWidth); 
          }
          else
          {
            float fPosCX = (float)m_dwPosX;
            float fPosCY = (float)m_dwPosY;
            GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
            if (fPosCX < 0) fPosCX = 0.0f;
            if (fPosCY < 0) fPosCY = 0.0f;
            if (fPosCY > GUIGraphicsContext.Height) fPosCY = (float)GUIGraphicsContext.Height;
            float fHeight = 60.0f;
            if (fHeight + fPosCY >= GUIGraphicsContext.Height)
              fHeight = GUIGraphicsContext.Height - fPosCY - 1;
            if (fHeight <= 0) return;

            float fwidth = m_dwWidth - 5.0f;
						if (fwidth<1) return;
						if (fHeight<1) return;

            if (fPosCX<=0) fPosCX=0;
            if (fPosCY<=0) fPosCY=0;
						/*Viewport newviewport, oldviewport;
						oldviewport = GUIGraphicsContext.DX9Device.Viewport;
						if (useViewport)
						{
							newviewport = new Viewport();
							newviewport.X = (int)fPosCX;
							newviewport.Y = (int)fPosCY;
							newviewport.Width = (int)(fwidth);
							newviewport.Height = (int)(fHeight);
							newviewport.MinZ = 0.0f;
							newviewport.MaxZ = 1.0f;
							GUIGraphicsContext.DX9Device.Viewport = newviewport;
						}*/

            m_pFont.DrawText((float)m_dwPosX, (float)m_dwPosY,m_dwTextColor,m_strText,m_dwTextAlign,(int)fwidth); 
            //if (useViewport)
						//	GUIGraphicsContext.DX9Device.Viewport = oldviewport;
          }
        }		
			}
		}

		public bool UseViewPort
		{
			get { return useViewport;}
			set { useViewport=value;}
		}
		/// <summary>
		/// Checks if the control can focus.
		/// </summary>
		/// <returns>false</returns>
		public override bool CanFocus()
		{
			return false;
		}

		/// <summary>
		/// This method is called when a message was recieved by this control.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="message">message : contains the message</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
		public override bool OnMessage(GUIMessage message)
		{
			// Check if the message was ment for this control.
			if ( message.TargetControlId==GetID )
			{
				// Set the text of the label.
				if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
				{
					if (message.Label!=null)
						Label = message.Label;
					else
						Label=String.Empty;
					return true;
				}
			}
			return base.OnMessage(message);
		}

		/// <summary>
		/// Get/set the color of the text
		/// </summary>
		public long	TextColor
		{ 
			get { return m_dwTextColor;}
			set { 
        if (m_dwTextColor!=value)
        {
          m_dwTextColor=value;
          
          ClearFontCache();
        }
      }
		}

		/// <summary>
		/// Get/set the alignment of the text
		/// </summary>
		public GUIControl.Alignment TextAlignment
		{
			get { return m_dwTextAlign;}
			set { 
        if (m_dwTextAlign!=value)
        {
          m_dwTextAlign=value;
          
          ClearFontCache();
        }
      }
		}

		/// <summary>
		/// Get/set the name of the font.
		/// </summary>
		public string FontName
		{
			get { 
        return m_strFontName;
      }
      set 
      { 
        if (value==null) return;
        if (value==String.Empty) return;
        if (m_pFont==null)
        {
          m_pFont=GUIFontManager.GetFont(value);
          m_strFontName=value;
          ClearFontCache();
        }
        else if (value != m_pFont.FontName)
        {
          m_pFont=GUIFontManager.GetFont(value);
          m_strFontName=value;
          ClearFontCache();
        }
      }
		}

		/// <summary>
		/// Get/set the text of the label.
		/// </summary>
		public string Label
		{
			get { return m_strLabel; }
			set {
        if (value==null) return;
        if (value.Equals(m_strLabel)) return;
        m_strLabel=value;
        if (m_strLabel.IndexOf("#")>=0) ContainsProperty=true;
        else ContainsProperty=false;
        textwidth=0;
        textheight=0;
        ClearFontCache();
      }
		}


    /// <summary>
    /// Property which returns true if the label contains a property
    /// or false when it doenst
    /// </summary>
    public bool ContainsPropertyKey
    {
      get { return ContainsProperty;}
    }

    /// <summary>
    /// Allocate any direct3d sources
    /// </summary>
    public override void AllocResources()
    {
      m_pFont=GUIFontManager.GetFont(m_strFontName);
      Update();
    }

    /// <summary>
    /// Free any direct3d resources
    /// </summary>
    public override void FreeResources()
    {
      ClearFontCache();
    }

    /// <summary>
    /// Property to get/set the usage of the font cache
    /// if enabled the renderd text is cached
    /// if not it will be re-created on every render() call
    /// </summary>
    public bool CacheFont
    {
      get { return m_bUseFontCache;}
      set { m_bUseFontCache=false;}
    }

    /// <summary>
    /// updates the current label by deleting the fontcache 
    /// </summary>
    protected override void Update()
    {  
     
    }

    /// <summary>
    /// Returns the width of the current text
    /// </summary>
    public int TextWidth
    {
      get 
      {
        if (textwidth==0 || textheight==0)
        {
          if (m_pFont==null) return 0;
          m_strText=GUIPropertyManager.Parse(m_strLabel);
          float fW=textwidth;
          float fH=textheight;
          m_pFont.GetTextExtent(m_strText,ref fW, ref fH);
          textwidth=(int)fW;
          textheight=(int)fH;
        }
        return textwidth;
      }
    }

    /// <summary>
    /// Returns the height of the current text
    /// </summary>
    public int TextHeight
    {
      get 
      {
        if (textwidth==0 || textheight==0)
        {
          if (m_pFont==null) return 0;
          m_strText=GUIPropertyManager.Parse(m_strLabel);
          float fW=textwidth;
          float fH=textheight;
          m_pFont.GetTextExtent(m_strText,ref fW, ref fH);
          textwidth=(int)fW;
          textheight=(int)fH;
        }
        return textheight;
      }
    }
    void ClearFontCache()
    {
      Update();
    }
	}
}
