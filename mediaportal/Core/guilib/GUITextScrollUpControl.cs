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
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITextScrollUpControl : GUIControl
  {
    protected int m_iOffset = 0;
    protected int m_iItemsPerPage = 10;
    protected int m_iItemHeight = 10;
    [XMLSkinElement("spaceBetweenItems")] protected int m_iSpaceBetweenItems = 2;
    [XMLSkinElement("font")] protected string m_strFontName = "";
    [XMLSkinElement("textcolor")] protected long m_dwTextColor = 0xFFFFFFFF;
    [XMLSkinElement("label")] protected string m_strProperty = "";
    [XMLSkinElement("seperator")] protected string m_strSeperator = "";
    protected GUIFont m_pFont = null;
    protected ArrayList m_vecItems = new ArrayList();
    protected bool m_bInvalidate = false;
    string m_strPrevProperty = "a";

    bool containsProperty = false;
    int m_iCurrentFrame = 0;
    double iScrollOffset = 0.0f;
    int iScrollY = 0;
    double timeElapsed = 0.0f;

    public double TimeSlice
    {
      get { return 0.01f + ((11 - GUIGraphicsContext.ScrollSpeedVertical)*0.01f); }
    }

    public GUITextScrollUpControl(int dwParentID) : base(dwParentID)
    {
    }

    public GUITextScrollUpControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                  string strFont, long dwTextColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      m_strFontName = strFont;

      m_dwTextColor = dwTextColor;

    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      m_pFont = GUIFontManager.GetFont(m_strFontName);
      if (m_strProperty.IndexOf("#") >= 0)
        containsProperty = true;
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleVertical(ref m_iSpaceBetweenItems);
    }

    public override void Render(float timePassed)
    {
      m_bInvalidate = false;
      if (null == m_pFont) return;
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }

      int dwPosY = m_dwPosY;

      timeElapsed += timePassed;
      m_iCurrentFrame = (int) (timeElapsed/TimeSlice);

      if (containsProperty)
      {
        string strText = GUIPropertyManager.Parse(m_strProperty);

        strText = strText.Replace("\\r", "\r");
        if (strText != m_strPrevProperty)
        {
          m_iOffset = 0;
          m_vecItems.Clear();

          m_strPrevProperty = strText;
          SetText(strText);
        }
      }

      if (GUIGraphicsContext.graphics != null)
      {
        m_iOffset = 0;
      }
      if (m_vecItems.Count > m_iItemsPerPage)
      {
        // 1 second rest before we start scrolling
        if (m_iCurrentFrame > 25 + 12)
        {
          m_bInvalidate = true;
          // adjust y-pos
          iScrollY = m_iCurrentFrame - 25 - 12;
          dwPosY -= (int) (iScrollY - iScrollOffset);

          if (m_dwPosY - dwPosY >= m_iItemHeight)
          {
            // one line has been scrolled away entirely
            dwPosY += m_iItemHeight;
            iScrollOffset += m_iItemHeight;
            m_iOffset++;
            if (m_iOffset >= m_vecItems.Count)
            {
              // restart with the first line
              if (Seperator.Length > 0)
              {
                if (m_iOffset >= m_vecItems.Count + 1)
                  m_iOffset = 0;
              }
              else m_iOffset = 0;
            }
          }
        }
        else
        {
          m_iOffset = 0;
        }
      }
      else
      {
        m_iOffset = 0;
      }

      Viewport oldviewport = GUIGraphicsContext.DX9Device.Viewport;
      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.SetClip(new Rectangle(m_dwPosX + GUIGraphicsContext.OffsetX, m_dwPosY + GUIGraphicsContext.OffsetY, m_dwWidth, m_iItemsPerPage*m_iItemHeight));
      }
      else
      {
        if (m_dwWidth < 1) return;
        if (m_dwHeight < 1) return;

        Viewport newviewport = new Viewport();
        newviewport.X = m_dwPosX + GUIGraphicsContext.OffsetX;
        newviewport.Y = m_dwPosY + GUIGraphicsContext.OffsetY;
        newviewport.Width = m_dwWidth;
        newviewport.Height = m_dwHeight;
        newviewport.MinZ = 0.0f;
        newviewport.MaxZ = 1.0f;
        GUIGraphicsContext.DX9Device.Viewport = newviewport;
      }
      for (int i = 0; i < 1 + m_iItemsPerPage; i++)
      {
        // render each line
        int dwPosX = m_dwPosX;
        int iItem = i + m_iOffset;
        int iMaxItems = m_vecItems.Count;
        if (m_vecItems.Count > m_iItemsPerPage && Seperator.Length > 0) iMaxItems++;

        if (iItem >= iMaxItems)
        {
          if (iMaxItems > m_iItemsPerPage)
            iItem -= iMaxItems;
          else break;
        }

        if (iItem >= 0 && iItem < iMaxItems)
        {
          // render item
          string strLabel1 = "", strLabel2 = "";
          if (iItem < m_vecItems.Count)
          {
            GUIListItem item = (GUIListItem) m_vecItems[iItem];
            strLabel1 = item.Label;
            strLabel2 = item.Label2;
          }
          else
          {
            strLabel1 = Seperator;
          }

          int ixoff = 16;
          int ioffy = 2;
          GUIGraphicsContext.ScaleVertical(ref ioffy);
          GUIGraphicsContext.ScaleHorizontal(ref ixoff);
          string wszText1 = String.Format("{0}", strLabel1);
          int dMaxWidth = m_dwWidth + ixoff;
          if (strLabel2.Length > 0)
          {
            string wszText2;
            float fTextWidth = 0, fTextHeight = 0;
            wszText2 = String.Format("{0}", strLabel2);
            m_pFont.GetTextExtent(wszText2.Trim(), ref fTextWidth, ref fTextHeight);
            dMaxWidth -= (int) (fTextWidth);

            m_pFont.DrawTextWidth((float) dwPosX + dMaxWidth, (float) dwPosY + ioffy, m_dwTextColor, wszText2.Trim(), fTextWidth, GUIControl.Alignment.ALIGN_LEFT);
          }
          m_pFont.DrawTextWidth((float) dwPosX, (float) dwPosY + ioffy, m_dwTextColor, wszText1.Trim(), (float) dMaxWidth, GUIControl.Alignment.ALIGN_LEFT);
          //            Log.Write("dw m_dwPosY, dwPosY, iScrollY, iScrollOffset: {0} {1} {2} {3}", m_dwPosY, dwPosY, iScrollY, iScrollOffset);
          //            Log.Write("dw wszText1.Trim() {0}", wszText1.Trim());

          dwPosY += m_iItemHeight;
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
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          containsProperty = false;
          m_strProperty = "";
          GUIListItem pItem = (GUIListItem) message.Object;
          m_vecItems.Add(pItem);
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = m_vecItems.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
        }
      }

      if (base.OnMessage(message)) return true;

      return false;
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      float fWidth = 0, fHeight = 0;

      m_pFont = GUIFontManager.GetFont(m_strFontName);
      if (null == m_pFont) return;
      m_pFont.GetTextExtent("abcdef", ref fWidth, ref fHeight);
      try
      {
        m_iItemHeight = (int) fHeight;
        float fTotalHeight = (float) m_dwHeight;
        m_iItemsPerPage = (int) Math.Floor(fTotalHeight/fHeight);
        if (m_iItemsPerPage == 0)
        {
          m_iItemsPerPage = 1;
        }
      }
      catch (Exception)
      {
        m_iItemHeight = 1;
        m_iItemsPerPage = 1;
      }
    }


    public override void AllocResources()
    {
      if (null == m_pFont) return;
      base.AllocResources();

      try
      {
        float fHeight = (float) m_iItemHeight; // + (float)m_iSpaceBetweenItems;
        float fTotalHeight = (float) (m_dwHeight);
        m_iItemsPerPage = (int) Math.Floor(fTotalHeight/fHeight);
        if (m_iItemsPerPage == 0)
        {
          m_iItemsPerPage = 1;
        }
        int iPages = 1;
        if (m_vecItems.Count > 0)
        {
          iPages = m_vecItems.Count/m_iItemsPerPage;
          if ((m_vecItems.Count%m_iItemsPerPage) != 0) iPages++;
        }
      }
      catch (Exception)
      {
        m_iItemsPerPage = 1;

      }

    }

    public override void FreeResources()
    {
      m_strPrevProperty = "";
      m_vecItems.Clear();
      base.FreeResources();
    }


    public int ItemHeight
    {
      get { return m_iItemHeight; }
      set { m_iItemHeight = value; }
    }

    public int Space
    {
      get { return m_iSpaceBetweenItems; }
      set { m_iSpaceBetweenItems = value; }
    }


    public long TextColor
    {
      get { return m_dwTextColor; }
    }


    public string FontName
    {
      get
      {
        if (m_pFont == null) return "";
        return m_pFont.FontName;
      }
    }


    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      return false;
    }

    public override bool NeedRefresh()
    {
      if (m_vecItems.Count > m_iItemsPerPage)
      {
        if (timeElapsed >= 0.02f) return true;
      }
      return false;
    }

    void SetText(string strText)
    {

      m_vecItems.Clear();
      // start wordwrapping
      // Set a flag so we can determine initial justification effects
      //bool bStartingNewLine = true;
      //bool bBreakAtSpace = false;
      int pos = 0;
      int lpos = 0;
      int iLastSpace = -1;
      int iLastSpaceInLine = -1;
      string szLine = "";
      strText = strText.Replace("\r", " ");
      strText.Trim();
      while (pos < strText.Length)
      {
        // Get the current letter in the string
        char letter = strText[pos];

        // Handle the newline character
        if (letter == '\n')
        {
          if (szLine.Length > 0 || m_vecItems.Count > 0)
          {
            GUIListItem item = new GUIListItem(szLine);
            m_vecItems.Add(item);
          }
          iLastSpace = -1;
          iLastSpaceInLine = -1;
          lpos = 0;
          szLine = "";
        }
        else
        {
          if (letter == ' ')
          {
            iLastSpace = pos;
            iLastSpaceInLine = lpos;
          }

          if (lpos < 0 || lpos > 1023)
          {
            //OutputDebugString("ERRROR\n");
          }
          szLine += letter;

          float fwidth = 0, fheight = 0;
          string wsTmp = szLine;
          m_pFont.GetTextExtent(wsTmp, ref fwidth, ref fheight);
          if (fwidth > m_dwWidth)
          {
            if (iLastSpace > 0 && iLastSpaceInLine != lpos)
            {
              szLine = szLine.Substring(0, iLastSpaceInLine);
              pos = iLastSpace;
            }
            if (szLine.Length > 0 || m_vecItems.Count > 0)
            {
              GUIListItem item = new GUIListItem(szLine);
              m_vecItems.Add(item);
            }
            iLastSpaceInLine = -1;
            iLastSpace = -1;
            lpos = 0;
            szLine = "";
          }
          else
          {
            lpos++;
          }
        }
        pos++;
      }
      if (lpos > 0)
      {
        GUIListItem item = new GUIListItem(szLine);
        m_vecItems.Add(item);
      }

      int istart = -1;
      for (int i = 0; i < m_vecItems.Count; ++i)
      {
        GUIListItem item = (GUIListItem) m_vecItems[i];
        if (item.Label.Length != 0) istart = -1;
        else if (istart == -1) istart = i;
      }
      if (istart > 0)
      {
        m_vecItems.RemoveRange(istart, m_vecItems.Count - istart);
      }

		// Set timeElapsed to be 0 so we delay scrolling again
		timeElapsed = 0.0f; 
		iScrollOffset = 0.0f;
    }


    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Property
    {
      get { return m_strProperty; }
      set
      {
        m_strProperty = value;
        if (m_strProperty.IndexOf("#") >= 0)
          containsProperty = true;
      }
    }

    public string Seperator
    {
      get { return m_strSeperator; }
      set { m_strSeperator = value; }
    }

    public void Clear()
    {
      containsProperty = false;
      m_strProperty = "";
      m_iOffset = 0;
      m_vecItems.Clear();

      iScrollY = 0;
      iScrollOffset = 0.0f;
      m_iCurrentFrame = 0;
      timeElapsed = 0.0f;
    }

    public string Label
    {
      set
      {
        if (m_strProperty != value)
        {
          m_strProperty = value;
          if (m_strProperty.IndexOf("#") >= 0)
            containsProperty = true;

          m_iOffset = 0;
          m_vecItems.Clear();
          SetText(value);
        }
      }
    }
  }
}