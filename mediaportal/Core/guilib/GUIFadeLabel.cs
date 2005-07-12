using System.Collections;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying fading labels.
  /// </summary>
  public class GUIFadeLabel : GUIControl
  {
    [XMLSkinElement("textcolor")] protected long m_dwTextColor = 0xFFFFFFFF;
    [XMLSkinElement("align")] Alignment m_dwTextAlign = Alignment.ALIGN_LEFT;
    [XMLSkinElement("font")] protected string m_strFont = "";
    [XMLSkinElement("label")] protected string m_strLabel = "";

    ArrayList m_vecLabels = new ArrayList();
    int m_iCurrentLabel = 0;
    int scroll_pos = 0;
    double iScrollOffset = 0.0f;
    int iScrollX = 0;
    bool m_bFadeIn = false;
    int m_iCurrentFrame = 0;

    double timeElapsed = 0.0f;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedHorizontal)*0.01f); }
    }

    bool m_bAllowScrolling = true;
    bool m_bScrolling = false;
    bool ContainsProperty = false;

    string m_strPrevTxt = "";
    GUILabelControl m_label = null;
    GUIFont m_pFont = null;

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
    public GUIFadeLabel(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strFont, long dwTextColor, Alignment dwTextAlign)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      m_strFont = strFont;
      m_dwTextColor = dwTextColor;
      m_dwTextAlign = dwTextAlign;
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
      GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
      m_label = new GUILabelControl(m_dwParentID, 0, m_dwPosX, m_dwPosY, m_dwWidth, m_dwHeight, m_strFont, m_strLabel, m_dwTextColor, m_dwTextAlign, false);
      m_label.CacheFont = false;
      if (m_strFont != "" && m_strFont != "-")
        m_pFont = GUIFontManager.GetFont(m_strFont);
      if (m_strLabel.IndexOf("#") >= 0) ContainsProperty = true;
    }

    /// <summary>
    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }
      m_bScrolling = false;
      if (m_strLabel != null && m_strLabel.Length > 0)
      {
        string strText = m_strLabel;
        if (ContainsProperty) strText = GUIPropertyManager.Parse(m_strLabel);

        if (m_strPrevTxt != strText)
        {
          m_iCurrentLabel = 0;
          scroll_pos = 0;
          iScrollX = 0;
          iScrollOffset = 0.0f;
          m_iCurrentFrame = 0;
          timeElapsed = 0.0f;
          m_bFadeIn = true;
          m_vecLabels.Clear();
          m_strPrevTxt = strText;
          strText = strText.Replace("\\r", "\r");
          int ipos = 0;
          do
          {
            ipos = strText.IndexOf("\r");
            int ipos2 = strText.IndexOf("\n");
            if (ipos >= 0 && ipos2 >= 0 && ipos2 < ipos) ipos = ipos2;
            if (ipos < 0 && ipos2 >= 0) ipos = ipos2;

            if (ipos >= 0)
            {
              string strLine = strText.Substring(0, ipos);
              if (strLine.Length > 1)
                m_vecLabels.Add(strLine);
              if (ipos + 1 >= strText.Length) break;
              strText = strText.Substring(ipos + 1);
            }
            else m_vecLabels.Add(strText);
          } while (ipos >= 0 && strText.Length > 0);
        }
      }

      // if there are no labels do not render
      if (m_vecLabels.Count == 0) return;

      // reset the current label is index is out of bounds
      if (m_iCurrentLabel < 0 || m_iCurrentLabel >= m_vecLabels.Count) m_iCurrentLabel = 0;

      // get the current label
      string strLabel = (string) m_vecLabels[m_iCurrentLabel];
      m_label.Width = m_dwWidth;
      m_label.Height = m_dwHeight;
      m_label.Label = strLabel;
      m_label.SetPosition(m_dwPosX, m_dwPosY);
      m_label.TextAlignment = m_dwTextAlign;
      m_label.TextColor = m_dwTextColor;
      if (m_label.TextWidth < m_dwWidth) m_label.CacheFont = true;
      else m_label.CacheFont = false;
      if (GUIGraphicsContext.graphics != null)
      {
        m_label.Render(timePassed);
        return;
      }

      // if there is only one label just draw the text
      if (m_vecLabels.Count == 1)
      {
        if (m_label.TextWidth < m_dwWidth)
        {
          m_label.Render(timePassed);
          return;
        }
      }

      timeElapsed += timePassed;
      m_iCurrentFrame = (int) (timeElapsed / TimeSlice);

      // More than one label
      m_bScrolling = true;


      // Make the label fade in
      if (m_bFadeIn && m_bAllowScrolling)
      {
        long dwAlpha = (0xff/12)*m_iCurrentFrame;
        dwAlpha <<= 24;
        dwAlpha += (m_dwTextColor & 0x00ffffff);
        m_label.TextColor = dwAlpha;
        float fwt = 0;
        m_label.Label = GetShortenedText(strLabel, m_dwWidth, ref fwt);
        if (m_dwTextAlign == Alignment.ALIGN_RIGHT)
          m_label.Width = (int)(fwt);
        m_label.Render(timePassed);
        if (m_iCurrentFrame >= 12)
        {
          m_bFadeIn = false;
        }
      }
      // no fading
      else
      {
        if (!m_bAllowScrolling)
        {
          m_iCurrentLabel = 0;
          scroll_pos = 0;
          iScrollX = 0;
          iScrollOffset = 0.0f;
          m_iCurrentFrame = 0;
        }
        // render the text
        bool bDone = RenderText(timePassed, (float) m_dwPosX, (float) m_dwPosY, (float) m_dwWidth, m_dwTextColor, strLabel);
        if (bDone)
        {
          m_iCurrentLabel++;
          scroll_pos = 0;
          iScrollX = 0;
          iScrollOffset = 0.0f;
          m_bFadeIn = true;
          m_iCurrentFrame = 0;
          timeElapsed = 0.0f;
          m_iCurrentFrame = 0;
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
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          m_strPrevTxt = "";
          m_vecLabels.Clear();
          m_iCurrentLabel = 0;
          scroll_pos = 0;
          iScrollX = 0;
          iScrollOffset = 0.0f;
          m_bFadeIn = true;
          m_iCurrentFrame = 0;
          timeElapsed = 0.0f;
          if (message.Label != null)
          {
            string strLabel = message.Label;
            if (strLabel.Length > 0)
            {
              m_vecLabels.Add(strLabel);
            }
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          if (message.Label != null)
          {
            string strLabel = message.Label;
            if (strLabel.Length > 0)
            {
              m_vecLabels.Add(strLabel);
            }
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          m_strPrevTxt = "";
          m_vecLabels.Clear();
          m_iCurrentLabel = 0;
          scroll_pos = 0;
          iScrollX = 0;
          iScrollOffset = 0.0f;
          m_bFadeIn = true;
          m_iCurrentFrame = 0;
          timeElapsed = 0.0f;
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
    /// <returns>true if the render was successful</returns>
    bool RenderText(float timePassed, float fPosX, float fPosY, float fMaxWidth, long dwTextColor, string wszText)
    {
      bool bResult = false;
      float fTextHeight = 0, fTextWidth = 0;

      if (m_pFont == null) return true;
      //Get the text width.
      m_pFont.GetTextExtent(wszText, ref fTextWidth, ref fTextHeight);

      float fPosCX = fPosX;
      float fPosCY = fPosY;
      // Apply the screen calibration
      GUIGraphicsContext.Correct(ref fPosCX, ref fPosCY);
      if (fPosCX < 0) fPosCX = 0.0f;
      if (fPosCY < 0) fPosCY = 0.0f;
      if (fPosCY > GUIGraphicsContext.Height)
        fPosCY = (float) GUIGraphicsContext.Height;


      float fWidth = 0;
      float fHeight = 60;
      if (fHeight + fPosCY >= GUIGraphicsContext.Height)
        fHeight = GUIGraphicsContext.Height - fPosCY - 1;
      if (fHeight <= 0) return true;

      if (m_dwTextAlign == Alignment.ALIGN_RIGHT) fPosCX -= fMaxWidth;

      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.SetClip(new Rectangle((int) fPosCX, (int) fPosCY, (int) (fMaxWidth), (int) (fHeight)));
      }
      else
      {
        Viewport newviewport;
        newviewport = new Viewport();
        if (fMaxWidth < 1) return true;
        if (fHeight < 1) return true;

        newviewport.X = (int) fPosCX;
        newviewport.Y = (int) fPosCY;
        newviewport.Width = (int) (fMaxWidth);
        newviewport.Height = (int) (fHeight);
        newviewport.MinZ = 0.0f;
        newviewport.MaxZ = 1.0f;
        GUIGraphicsContext.DX9Device.Viewport = newviewport;
      }
      // scroll
      string wszOrgText = wszText;

      if (m_dwTextAlign != Alignment.ALIGN_RIGHT)
      {
        do
        {
          m_pFont.GetTextExtent(wszOrgText, ref fTextWidth, ref fTextHeight);
          wszOrgText += " ";
        } while (fTextWidth >= 0 && fTextWidth < fMaxWidth);
      }
      fMaxWidth += 50.0f; 
      string szText = "";

      if (m_iCurrentFrame > 12 + 25)
      {
        // doscroll (after having waited some frames)
        string wTmp = "";
        iScrollX = m_iCurrentFrame - (12 + 25);
          if (scroll_pos >= wszOrgText.Length)
            wTmp = " ";
          else
            wTmp = wszOrgText.Substring(scroll_pos, 1);
          m_pFont.GetTextExtent(wTmp, ref fWidth, ref fHeight);
        if (iScrollX - iScrollOffset >= fWidth)
        {
          ++scroll_pos;
          if (scroll_pos > wszText.Length)
          {
            scroll_pos = 0;
            bResult = true;
            if (GUIGraphicsContext.graphics != null)
            {
              GUIGraphicsContext.graphics.SetClip(new Rectangle(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height));
            }
            else
            {
              GUIGraphicsContext.DX9Device.Viewport = oldviewport;
            }

            return true;
          }
          // now we need to correct iScrollX
          // with the sum-length of all cut-off characters
          iScrollOffset += fWidth; 
        }
        int ipos = 0;
        for (int i = 0; i < wszOrgText.Length; i++)
        {
          if (i + scroll_pos < wszOrgText.Length)
            szText += wszOrgText[i + scroll_pos];
          else
          {
            szText += ' ';
            ipos++;
          }
        }
        if (fPosY >= 0.0)
        {
          if (Alignment.ALIGN_RIGHT == m_dwTextAlign)
          {
            // right alignment => calculate xpos differently
            float fwt = 0;
            //            string strLabel = GetShortenedText(wszOrgText, m_dwWidth, ref fwt);
            GetShortenedText(wszOrgText, m_dwWidth, ref fwt); 
            int xpos = (int)(fPosX - fwt - iScrollX + iScrollOffset);
            m_label.Label = szText;
            m_label.Width = (int) (fMaxWidth - 50 + iScrollX - iScrollOffset); 
            m_label.TextColor = dwTextColor;
            m_label.SetPosition(xpos, (int) fPosY);
            m_label.TextAlignment = Alignment.ALIGN_LEFT;
            m_label.Render(timePassed);
          }
          else
          {
            // left or centered alignment
            m_label.Label = szText;
            // 1) reduce maxwidth to ensure faded right edge is drawn
            // 2) compensate the Width to ensure the faded right edge does not move
            m_label.Width = (int) (fMaxWidth - 50 + iScrollX - iScrollOffset); 
            m_label.TextColor = dwTextColor;
            int xpos = (int) (fPosX - iScrollX + iScrollOffset);
            //            Log.Write("fPosX, iScrollX, iScrollOffset, xpos: {0} {1} {2} {3}", fPosX, iScrollX, iScrollOffset, xpos);
            //            Log.Write("szText {0}", szText);
            m_label.SetPosition(xpos, (int) fPosY);
            m_label.Render(timePassed);
          }
        }
      }
      else
      {
        // wait some frames before scrolling
        if (fPosY >= 0.0)
        {
          float fwt = 0, fht = 0;
          m_label.Label = GetShortenedText(wszText, (int) fMaxWidth - 50, ref fwt);
          m_pFont.GetTextExtent(m_label.Label, ref fwt, ref fht);
          if (m_dwTextAlign == Alignment.ALIGN_RIGHT)
          {
            m_label.Width = (int)(fwt); 
          }
          else
          {
            m_label.Width = (int) fMaxWidth - 50;
          }
          m_label.TextColor = dwTextColor;
          m_label.SetPosition((int) fPosX, (int) fPosY);
          m_label.Render(timePassed);
        }
      }

      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.SetClip(new Rectangle(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height));
      }
      else
      {
        GUIGraphicsContext.DX9Device.Viewport = oldviewport;
      }
      return bResult;
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName
    {
      get { return m_strFont; }
      set
      {
        if (value == null) return;
        m_strFont = value;
        m_pFont = GUIFontManager.GetFont(m_strFont);
      }
    }

    /// <summary>
    /// Get/set the color of the text.
    /// </summary>
    public long TextColor
    {
      get { return m_dwTextColor; }
      set { m_dwTextColor = value; }
    }

    /// <summary>
    /// Get/set the alignment of the text.
    /// </summary>
    public Alignment TextAlignment
    {
      get { return m_dwTextAlign; }
      set { m_dwTextAlign = value; }
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      m_pFont = GUIFontManager.GetFont(m_strFont);
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      m_strPrevTxt = "";
      m_vecLabels.Clear();
      m_pFont = null;
    }

    /// <summary>
    /// Clears the control.
    /// </summary>
    public void Clear()
    {
      m_iCurrentLabel = 0;
      m_strPrevTxt = "";
      m_vecLabels.Clear();
      m_iCurrentFrame = 0;
      timeElapsed = 0.0f;
    }

    /// <summary>
    /// Add a label to the control.
    /// </summary>
    /// <param name="strLabel"></param>
    public void Add(string strLabel)
    {
      if (strLabel == null) return;
      m_vecLabels.Add(strLabel);
    }

    /// <summary>
    /// Get/set the scrolling property of the control.
    /// </summary>
    public bool AllowScrolling
    {
      get { return m_bAllowScrolling; }
      set { m_bAllowScrolling = value; }
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
    /// some controls (for example the fadelabel) contain scrolling texts and need 2
    /// ne re-rendered constantly
    /// </summary>
    /// <returns>true or false</returns>
    public override bool NeedRefresh()
    {
      if (m_bScrolling && m_bAllowScrolling) return true;
      return false;
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Label
    {
      get { return m_strLabel; }
      set
      {
        if (value == null) return;
        m_strLabel = value;
        if (m_strLabel.IndexOf("#") >= 0) ContainsProperty = true;
        else ContainsProperty = false;
      }
    }

    string GetShortenedText(string strLabel, int iMaxWidth, ref float fw)
    {
      if (strLabel == null) return string.Empty;
      if (strLabel.Length == 0) return string.Empty;
      if (m_pFont == null) return strLabel;
      if (m_dwTextAlign == Alignment.ALIGN_RIGHT)
      {
        if (strLabel.Length > 0)
        {
          bool bTooLong = false;
          float fh = 0;
          do
          {
            bTooLong = false;
            m_pFont.GetTextExtent(strLabel, ref fw, ref fh);
            if (fw >= iMaxWidth)
            {
              strLabel = strLabel.Substring(0, strLabel.Length - 1);
              bTooLong = true;
            }
          } while (bTooLong && strLabel.Length > 1);
        }
      }
      return strLabel;
    }
  }
}