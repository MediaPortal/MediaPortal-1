using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A GUIControl for displaying labels.
	/// </summary>
	public class GUILabelControl :GUIControl
	{
		GUIFont								m_pFont=null;
		[XMLSkinElement("font")]			protected string	m_strFontName="";
		[XMLSkinElement("label")]			protected string	m_strLabel="";
		[XMLSkinElement("textcolor")]	protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("align")]			Alignment         m_dwTextAlign=Alignment.ALIGN_LEFT;
		[XMLSkinElement("hasPath")]		bool				      m_bHasPath=false;
    string                        m_strText;
    bool                          ContainsProperty=false;
    int                           textwidth=0;
    int                           textheight=0;
    bool                                      m_bUseFontCache=false;
    CustomVertex.TransformedColoredTextured[] m_cachedFontVertices=null;
    int                                       m_iFontTriangles=0;

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
			m_bHasPath = bHasPath;
			FinalizeConstruction();
		}
		public GUILabelControl (int dwParentID) : base(dwParentID)
		{
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			if (m_strFontName!="" && m_strFontName!="-")
				m_pFont=GUIFontManager.GetFont(m_strFontName);
			GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
      if (m_strLabel.IndexOf("#")>=0) ContainsProperty=true;
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		public override void Render()
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
          textwidth=0;
          textheight=0;
          
          m_cachedFontVertices=null;
          m_iFontTriangles=0;      
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
          m_pFont.DrawText(m_dwPosX,m_dwPosY,m_dwTextColor, m_strText, m_dwTextAlign);
        return;
      }

        if (m_dwTextAlign==GUIControl.Alignment.ALIGN_CENTER)
        {
          if (m_cachedFontVertices!=null)
          {
            m_pFont.DrawFontCache(ref m_cachedFontVertices,m_iFontTriangles);
            return;
          }          
          float fW=textwidth;
          float fH=textheight;
          if (textwidth==0 || textheight==0)
          {
            m_pFont.GetTextExtent(m_strText,ref fW, ref fH);
            textwidth=(int)fW;
            textheight=(int)fH;
          }
          int xoff= (int)((m_dwWidth-fW)/2);
          int yoff= (int)((m_dwHeight-fH)/2);
          if (m_bUseFontCache)
          {
            m_pFont.GetFontCache((float)m_dwPosX+xoff, (float)m_dwPosY+yoff, m_dwTextColor, m_strText, out m_cachedFontVertices , out m_iFontTriangles);
          }
          m_pFont.DrawText((float)m_dwPosX+xoff, (float)m_dwPosY+yoff,m_dwTextColor,m_strText,GUIControl.Alignment.ALIGN_LEFT); 
        }
        else
        {
          float fW=textwidth;
          float fH=textheight;
          if (textwidth==0 || textheight==0)
          {
            m_pFont.GetTextExtent(m_strText,ref fW, ref fH);
            textwidth=(int)fW;
            textheight=(int)fH;
          }

          if (m_dwTextAlign== GUIControl.Alignment.ALIGN_RIGHT)
          {
            if (m_dwWidth==0 || textwidth < m_dwWidth)
            {
              if (m_cachedFontVertices!=null)
              {
                m_pFont.DrawFontCache(ref m_cachedFontVertices,m_iFontTriangles);
                return;
              }
              if (m_bUseFontCache)
              {
                m_pFont.GetFontCache((float)m_dwPosX-fW, (float)m_dwPosY, m_dwTextColor, m_strText, out m_cachedFontVertices , out m_iFontTriangles);
              }
              m_pFont.DrawText((float)m_dwPosX-fW, (float)m_dwPosY,m_dwTextColor,m_strText,GUIControl.Alignment.ALIGN_LEFT); 
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
              Viewport newviewport, oldviewport;
              newviewport = new Viewport();
              oldviewport = GUIGraphicsContext.DX9Device.Viewport;
              newviewport.X = (int)fPosCX;
              newviewport.Y = (int)fPosCY;
              newviewport.Width = (int)(fwidth);
              newviewport.Height = (int)(fHeight);
              newviewport.MinZ = 0.0f;
              newviewport.MaxZ = 1.0f;
              GUIGraphicsContext.DX9Device.Viewport = newviewport;

              if (m_cachedFontVertices!=null)
              {
                m_pFont.DrawFontCache(ref m_cachedFontVertices,m_iFontTriangles);
              }
              else
              {
                if (m_bUseFontCache)
                {
                  m_pFont.GetFontCache((float)m_dwPosX-fW, (float)m_dwPosY, m_dwTextColor, m_strText, out m_cachedFontVertices , out m_iFontTriangles);
                }
                m_pFont.DrawText((float)m_dwPosX-fW, (float)m_dwPosY,m_dwTextColor,m_strText,GUIControl.Alignment.ALIGN_LEFT); 
              }
              GUIGraphicsContext.DX9Device.Viewport = oldviewport;
            }
            return;
          }

          if (m_dwWidth==0 || textwidth < m_dwWidth)
          {
            if (m_cachedFontVertices!=null)
            {
              m_pFont.DrawFontCache(ref m_cachedFontVertices,m_iFontTriangles);
              return;
            }
            if (m_bUseFontCache)
            {
              m_pFont.GetFontCache((float)m_dwPosX, (float)m_dwPosY, m_dwTextColor, m_strText, out m_cachedFontVertices , out m_iFontTriangles);
            }
            m_pFont.DrawText((float)m_dwPosX, (float)m_dwPosY,m_dwTextColor,m_strText,m_dwTextAlign); 
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
            Viewport newviewport, oldviewport;
            newviewport = new Viewport();
            oldviewport = GUIGraphicsContext.DX9Device.Viewport;
            newviewport.X = (int)fPosCX;
            newviewport.Y = (int)fPosCY;
            newviewport.Width = (int)(fwidth);
            newviewport.Height = (int)(fHeight);
            newviewport.MinZ = 0.0f;
            newviewport.MaxZ = 1.0f;
            GUIGraphicsContext.DX9Device.Viewport = newviewport;

            if (m_cachedFontVertices!=null)
            {
              m_pFont.DrawFontCache(ref m_cachedFontVertices,m_iFontTriangles);
            }
            else
            {
              if (m_bUseFontCache)
              {
                m_pFont.GetFontCache((float)m_dwPosX, (float)m_dwPosY, m_dwTextColor, m_strText, out m_cachedFontVertices , out m_iFontTriangles);
              }
              m_pFont.DrawText((float)m_dwPosX, (float)m_dwPosY,m_dwTextColor,m_strText,m_dwTextAlign); 
            }
            GUIGraphicsContext.DX9Device.Viewport = oldviewport;
          }
        }		
			}
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
					Label = (string)(message.Label.Clone());
					if ( m_bHasPath )
						ShortenPath();
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
          
          m_cachedFontVertices=null;
          m_iFontTriangles=0;
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
          
          m_cachedFontVertices=null;
          m_iFontTriangles=0;
        }
      }
		}

		/// <summary>
		/// Get/set the name of the font.
		/// </summary>
		public string FontName
		{
			get { return m_pFont.FontName; }
      set 
      { 
        if (value==null) return;
        if (value==String.Empty) return;
        if (m_pFont==null)
        {
          m_pFont=GUIFontManager.GetFont(value);
          m_cachedFontVertices=null;
          m_iFontTriangles=0;
        }
        else if (value != m_pFont.FontName)
        {
          m_pFont=GUIFontManager.GetFont(value);
          m_cachedFontVertices=null;
          m_iFontTriangles=0;
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
        if (value.Equals(m_strLabel)) return;
        m_strLabel=value;
        if (m_strLabel.IndexOf("#")>=0) ContainsProperty=true;
        else ContainsProperty=false;
        textwidth=0;
        textheight=0;
        m_cachedFontVertices=null;
        m_iFontTriangles=0;
      }
		}

		protected void ShortenPath()
		{
			// TODO: Implement this functionality if needed.
		}
    public bool ContainsPropertyKey
    {
      get { return ContainsProperty;}
    }

    public override void AllocResources()
    {
      m_iFontTriangles=0;
      m_cachedFontVertices=null;
    }

    public override void FreeResources()
    {
      m_cachedFontVertices=null;
      m_iFontTriangles=0;
    }

    public bool CacheFont
    {
      get { return m_bUseFontCache;}
      set { m_bUseFontCache=false;}
    }

    protected override void Update()
    {
      m_cachedFontVertices=null;
      m_iFontTriangles=0;      
    }
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
	}
}
