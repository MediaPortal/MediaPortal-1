using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A GUIControl for displaying fading labels.
	/// </summary>
	public class GUIFadeLabel:GUIControl
	{
  	[XMLSkinElement("textcolor")]		protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("align")]			Alignment  m_dwTextAlign=Alignment.ALIGN_LEFT;
		[XMLSkinElement("font")]			protected string	m_strFont="";
		[XMLSkinElement("label")]			protected string	m_strLabel="";	    
    
		ArrayList       m_vecLabels= new ArrayList();
		int							m_iCurrentLabel=0;
    int 						scroll_pos=0;
    int 						iScrollX=0;
    bool						m_bFadeIn=false;
    int							m_iCurrentFrame=0;

    bool            m_bAllowScrolling=true;
    bool            m_bScrolling=false; 
    bool            ContainsProperty=false;
    
		string          m_strPrevTxt="";
    DateTime        m_dtTime=DateTime.Now;
    GUILabelControl m_label=null;
    GUIFont								m_pFont=null;
		
		public GUIFadeLabel(int dwParentID) : base(dwParentID)
		{
		}
		/// <summary>
		/// The constructor of the GUIFadeLabel class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strFont">The indication of the font of this control.</param>
		/// <param name="dwTextColor">The color of this control.</param>
		/// <param name="dwTextAlign">The alignment of this control.</param>
    public GUIFadeLabel(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strFont, long dwTextColor, GUIControl.Alignment dwTextAlign)
    :base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
     {
       m_strFont=strFont;
       m_dwTextColor=dwTextColor;
       m_dwTextAlign=dwTextAlign;
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
			base.FinalizeConstruction();
			m_dtTime=DateTime.Now;
			GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
      m_label = new GUILabelControl(m_dwParentID,0,m_dwPosX,m_dwPosY,m_dwWidth, m_dwHeight,m_strFont,m_strLabel,m_dwTextColor,m_dwTextAlign,false);
      m_label.CacheFont=false;
      if (m_strFont!="" && m_strFont!="-")
        m_pFont=GUIFontManager.GetFont(m_strFont);
      if (m_strLabel.IndexOf("#")>=0) ContainsProperty=true;
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
    public override void Render()
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
      }
      m_bScrolling=false;       

      if (m_strLabel!=null&& m_strLabel.Length > 0 )
      {
        string strText=m_strLabel;
        if (ContainsProperty) strText=GUIPropertyManager.Parse(m_strLabel);

				if (m_strPrevTxt!=strText)
				{
					m_iCurrentLabel=0;
					scroll_pos = 0;
					iScrollX=0;
					m_bFadeIn=true;
					m_iCurrentFrame =0;
					m_vecLabels.Clear();
					m_strPrevTxt=strText;
          m_dtTime=DateTime.Now;
					strText=strText.Replace("\\r","\r");
					int ipos=0;
					do
					{
						ipos=strText.IndexOf("\r");
						int ipos2=strText.IndexOf("\n");
						if (ipos>=0 && ipos2>=0 && ipos2 < ipos) ipos=ipos2;
						if (ipos<0  && ipos2>=0) ipos=ipos2;
						
						if (ipos>=0)
						{
							string strLine = strText.Substring(0,ipos);
							if (strLine.Length>1)
								m_vecLabels.Add(strLine);
							if (ipos+1>=strText.Length) break;
							strText=strText.Substring(ipos+1);
						}
						else m_vecLabels.Add(strText);
					} while (ipos>=0 && strText.Length>0);
				}
      }
			
			// if there are no labels do not render
      if (m_vecLabels.Count==0) return;

			// reset the current label is index is out of bounds
      if (m_iCurrentLabel<0||m_iCurrentLabel >= m_vecLabels.Count ) m_iCurrentLabel=0;
      
			// get the current label
      string strLabel=(string)m_vecLabels[m_iCurrentLabel];
      m_label.Width=m_dwWidth;
      m_label.Height=m_dwHeight;
      m_label.Label=strLabel;
      m_label.SetPosition(m_dwPosX,m_dwPosY);
      m_label.TextAlignment=m_dwTextAlign;
      m_label.TextColor=m_dwTextColor;
      if (m_label.TextWidth < m_dwWidth) m_label.CacheFont=true;
      else m_label.CacheFont=false;
      if (GUIGraphicsContext.graphics!=null)
      {
        m_label.Render();
        return;
      }

      // if there is only one label just draw the text
			if (m_vecLabels.Count==1 )
      {
        if (m_label.TextWidth< m_dwWidth)
        {
          m_label.Render();
          return;
        }
      }
      
      TimeSpan ts= DateTime.Now-m_dtTime;
      int iFrame=(int)(ts.TotalMilliseconds/  ((double)( (11-GUIGraphicsContext.ScrollSpeed)*15))  );
      int iDiffFrames=iFrame-m_iCurrentFrame;
      if (iDiffFrames>25) iDiffFrames=25;
      bool bAdd=false;
      if (iDiffFrames>0) bAdd=true;
      int iFrameCount=0;
      do
      {
        if (bAdd) m_iCurrentFrame++;
        // More than one label
        m_bScrolling=true;        
    
        
        // Make the label fade in
        if (m_bFadeIn && m_bAllowScrolling)
        {
          long dwAlpha = (0xff/12) * m_iCurrentFrame;
          dwAlpha <<=24;
          dwAlpha += ( m_dwTextColor &0x00ffffff);
          m_label.TextColor=dwAlpha;
          m_label.Label=GetShortenedText(strLabel,m_dwWidth);
          m_label.Render();
          if (m_iCurrentFrame >=12)
          {
            m_bFadeIn=false;
          }
        }
          // no fading
        else
        {
          if (!m_bAllowScrolling)
          {
            m_iCurrentLabel=0;
            scroll_pos=0;
            iScrollX=0;
            m_iCurrentFrame =0; 
          }
          // render the text
          bool bDone=RenderText(bAdd, (float)m_dwPosX, (float)m_dwPosY, (float) m_dwWidth,m_dwTextColor, strLabel, true );
            
          if ( bDone)
          {
            m_iCurrentLabel++;
            scroll_pos = 0;
            iScrollX=0;
            //iLastItem=-1;
            //iFrames=0;
            m_bFadeIn=true;
            m_iCurrentFrame =0; 
            m_dtTime=DateTime.Now;
          }
        }      
        iFrameCount++;
      } while (iFrameCount < iDiffFrames);
      m_iCurrentFrame=(int)(ts.TotalMilliseconds/  ((double)( (11-GUIGraphicsContext.ScrollSpeed)*15))  );
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
		/// This method is called when a message was recieved by this control.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="message">message : contains the message</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      if ( message.TargetControlId==GetID )
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          string strLabel = message.Label;
          if (strLabel.Length>0)
          {
            m_vecLabels.Add(strLabel);
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          
          m_dtTime=DateTime.Now;
          m_strPrevTxt="";
			    m_vecLabels.Clear();
          m_iCurrentLabel=0;
          scroll_pos = 0;
          iScrollX=0;
          m_bFadeIn=true;
          m_iCurrentFrame =0;

        }
      }
      return base.OnMessage(message);
    }


		/// <summary>
		/// Renders the text.
		/// </summary>
		/// <param name="fPosX">The X position of the text.</param>
		/// <param name="fPosY">The Y position of the text.</param>
		/// <param name="fMaxWidth">The maximum render width.</param>
		/// <param name="dwTextColor">The color of the text.</param>
		/// <param name="wszText">The actual text.</param>
		/// <param name="bScroll">A bool indication if there is scrolling or not.</param>
		/// <returns>true if the render was successful</returns>
    bool RenderText(bool bAdvance, float fPosX, float fPosY, float fMaxWidth,long dwTextColor, string wszText,bool bScroll )
    {
	    bool	bResult=false;
      float fTextHeight=0,fTextWidth=0;
			
			if (m_pFont==null) return true;
      //Get the text width.
      m_pFont.GetTextExtent( wszText, ref fTextWidth,ref fTextHeight);

	    float fPosCX=fPosX;
	    float fPosCY=fPosY;
			// Apply the screen calibration
	    GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
	    if (fPosCX <0) fPosCX=0.0f;
	    if (fPosCY <0) fPosCY=0.0f;
	    if (fPosCY >GUIGraphicsContext.Height) 
        fPosCY=(float)GUIGraphicsContext.Height;


			float fWidth=0;      
			float fHeight=60;
			if (fHeight+fPosCY >= GUIGraphicsContext.Height )
				fHeight = GUIGraphicsContext.Height - fPosCY -1;
			if (fHeight <= 0) return true;

			if (m_dwTextAlign==GUIControl.Alignment.ALIGN_RIGHT) fPosCX-= fMaxWidth;


			Viewport oldviewport=GUIGraphicsContext.DX9Device.Viewport;
			if (GUIGraphicsContext.graphics!=null)
			{
				GUIGraphicsContext.graphics.SetClip(new Rectangle((int)fPosCX,(int)fPosCY,(int)(fMaxWidth),(int)(fHeight)));
			}
			else
			{
				Viewport newviewport;
				newviewport=new Viewport();
	            
				newviewport.X      = (int)fPosCX;
				newviewport.Y			 = (int)fPosCY;
				newviewport.Width  = (int)(fMaxWidth);
				newviewport.Height = (int)(fHeight);
				newviewport.MinZ   = 0.0f;
				newviewport.MaxZ   = 1.0f;
				GUIGraphicsContext.DX9Device.Viewport=newviewport;
			}
      // scroll
      string wszOrgText=wszText;

      if (m_dwTextAlign!=GUIControl.Alignment.ALIGN_RIGHT) 
      {
        do
        {
          m_pFont.GetTextExtent( wszOrgText, ref fTextWidth,ref fTextHeight);
          wszOrgText+= " ";
        } while ( fTextWidth>=0 && fTextWidth < fMaxWidth);
      }
			fMaxWidth+=50.0f;
      string szText="";
      			
      if (m_iCurrentFrame>12+25)
      {
					string wTmp="";
        if (m_dwTextAlign!=GUIControl.Alignment.ALIGN_RIGHT) 
        {
          if (scroll_pos >= wszOrgText.Length )
            wTmp=" ";
          else
            wTmp=wszOrgText.Substring(scroll_pos,1);        
          m_pFont.GetTextExtent(wTmp, ref fWidth,ref fHeight);
          if ( iScrollX >= fWidth)
          {
            ++scroll_pos;
            if (scroll_pos > wszText.Length )
            {
              scroll_pos = 0;
              bResult=true;
							if (GUIGraphicsContext.graphics!=null)
							{
								GUIGraphicsContext.graphics.SetClip(new Rectangle(0,0,GUIGraphicsContext.Width,GUIGraphicsContext.Height));
							}
							else
							{
								GUIGraphicsContext.DX9Device.Viewport=oldviewport;
							}

              return true;
            }
            //iFrames=0;
            iScrollX=1;
          }
          else 
          {
             if (bAdvance) iScrollX++;
          }
        }
        else 
        {
          if (bAdvance) iScrollX++;
          if (iScrollX>=fMaxWidth)
          {
            bResult=true;
						if (GUIGraphicsContext.graphics!=null)
						{
							GUIGraphicsContext.graphics.SetClip(new Rectangle(0,0,GUIGraphicsContext.Width,GUIGraphicsContext.Height));
						}
						else
						{
							GUIGraphicsContext.DX9Device.Viewport=oldviewport;
						}

            return true;
          }
        }
				int ipos=0;
				for (int i=0; i < wszOrgText.Length; i++)
				{
					if (i+scroll_pos < wszOrgText.Length)
						szText+=wszOrgText[i+scroll_pos];
					else
					{
						szText+=' ';
						ipos++;
					}
				}
        if (fPosY >=0.0)
        {
          if (GUIControl.Alignment.ALIGN_RIGHT==m_dwTextAlign)
          {
            string strLabel=GetShortenedText(wszOrgText,m_dwWidth);
            float fwt=0,fht=0;
            m_pFont.GetTextExtent(strLabel,ref fwt,ref fht);
            int xpos=(int)(fPosX-fwt);
            m_label.Label=szText;
            m_label.Width=0;
            m_label.TextColor=m_dwTextColor;
            m_label.SetPosition((int)xpos-iScrollX,(int)fPosY);
            m_label.TextAlignment=GUIControl.Alignment.ALIGN_LEFT;
            m_label.Render();
          }
          else
          {
            m_label.Label=szText;
            m_label.Width=(int)fMaxWidth-50;
            m_label.TextColor=m_dwTextColor;
            m_label.SetPosition((int)fPosX-iScrollX,(int)fPosY);
            m_label.Render();
          }
        }
      	
			}
			else
			{
        if (fPosY >=0.0)
        {
          m_label.Label=GetShortenedText(wszText,(int)fMaxWidth-50);
          m_label.Width=(int)fMaxWidth-50;
          m_label.TextColor=m_dwTextColor;
          m_label.SetPosition((int)fPosX,(int)fPosY);
          m_label.Render();
        }
			}
  
	  if (GUIGraphicsContext.graphics!=null)
	  {
		  GUIGraphicsContext.graphics.SetClip(new Rectangle(0,0,GUIGraphicsContext.Width,GUIGraphicsContext.Height));
	  }
	  else
	  {
		  GUIGraphicsContext.DX9Device.Viewport=oldviewport;
	  }
	  return bResult;
  }

		/// <summary>
		/// Get/set the name of the font.
		/// </summary>
    public string FontName
    {
      get { return m_strFont;}
      set { 
        m_strFont=value;
        m_pFont=GUIFontManager.GetFont(m_strFont);
      }
    }
		
		/// <summary>
		/// Get/set the color of the text.
		/// </summary>
    public long TextColor
    {
      get { return m_dwTextColor;}
      set { m_dwTextColor=value;}
    }

		/// <summary>
		/// Get/set the alignment of the text.
		/// </summary>
    public GUIControl.Alignment TextAlignment
    {
      get { return m_dwTextAlign;}
      set { m_dwTextAlign=value;}
    }

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
    public override void AllocResources()
    {
      m_pFont=GUIFontManager.GetFont(m_strFont);
    }

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
    public override void FreeResources()
    {
      m_strPrevTxt="";
      m_vecLabels.Clear();
      m_pFont=null;
    }

		/// <summary>
		/// Clears the control.
		/// </summary>
    public void Clear()
    {
      m_iCurrentLabel=0;
      m_strPrevTxt="";
      m_vecLabels.Clear();
      m_dtTime=DateTime.Now;
    }

		/// <summary>
		/// Add a label to the control.
		/// </summary>
		/// <param name="strLabel"></param>
    public void Add(string strLabel)
    {
      m_vecLabels.Add(strLabel);
    }

		/// <summary>
		/// Get/set the scrolling property of the control.
		/// </summary>
    public bool AllowScrolling
    {
      get { return m_bAllowScrolling;}
      set { m_bAllowScrolling=value;}
    }
    
		/// <summary>
		/// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
		/// some controls (for example the fadelabel) contain scrolling texts and need 2
		/// ne re-rendered constantly
		/// </summary>
		/// <returns>true or false</returns>
    public override bool  NeedRefresh()
    {
      
      if (m_bScrolling&&m_bAllowScrolling) return true;
      return false;
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Label
    {
      get { return m_strLabel; }
      set {
        m_strLabel=value;
        if (m_strLabel.IndexOf("#")>=0) ContainsProperty=true;
        else ContainsProperty=false;
      }
    }

    string GetShortenedText(string strLabel, int iMaxWidth)
		{
			if (strLabel==null) return string.Empty;
			if (strLabel.Length==0) return string.Empty;
			if (m_pFont==null) return strLabel;
      if (m_dwTextAlign==GUIControl.Alignment.ALIGN_RIGHT)
      {
        if (strLabel.Length>0)
        {
          bool bTooLong=false;
          float fw=0,fh=0;
          do
          {
            bTooLong=false;
            m_pFont.GetTextExtent(strLabel,ref fw, ref fh);
            if (fw >= iMaxWidth) 
            {
              strLabel=strLabel.Substring(0,strLabel.Length-1);
              bTooLong=true;
            }
          } while (bTooLong && strLabel.Length>1);
        }
      }
      return strLabel;
    }
	}
}
