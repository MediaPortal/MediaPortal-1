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
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("label")] protected string m_strProperty = "";
    [XMLSkinElement("seperator")] protected string m_strSeperator = "";
    protected GUIFont _font = null;
    protected ArrayList m__itemList = new ArrayList();
    protected bool m_bInvalidate = false;
    string m_strPrevProperty = "a";

    bool containsProperty = false;
    int _currentFrame = 0;
    double _scrollOffset = 0.0f;
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
      _fontName = strFont;

      _textColor = dwTextColor;

    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _font = GUIFontManager.GetFont(_fontName);
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
      if (null == _font) return;
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible) return;
      }

      int dwPosY = _positionY;

      timeElapsed += timePassed;
      _currentFrame = (int) (timeElapsed/TimeSlice);

      if (containsProperty)
      {
        string strText = GUIPropertyManager.Parse(m_strProperty);

        strText = strText.Replace("\\r", "\r");
        if (strText != m_strPrevProperty)
        {
          m_iOffset = 0;
          m__itemList.Clear();

          m_strPrevProperty = strText;
          SetText(strText);
        }
      }

      if (GUIGraphicsContext.graphics != null)
      {
        m_iOffset = 0;
      }
      if (m__itemList.Count > m_iItemsPerPage)
      {
        // 1 second rest before we start scrolling
        if (_currentFrame > 25 + 12)
        {
          m_bInvalidate = true;
          // adjust y-pos
          iScrollY = _currentFrame - 25 - 12;
          dwPosY -= (int) (iScrollY - _scrollOffset);

          if (_positionY - dwPosY >= m_iItemHeight)
          {
            // one line has been scrolled away entirely
            dwPosY += m_iItemHeight;
            _scrollOffset += m_iItemHeight;
            m_iOffset++;
            if (m_iOffset >= m__itemList.Count)
            {
              // restart with the first line
              if (Seperator.Length > 0)
              {
                if (m_iOffset >= m__itemList.Count + 1)
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
        GUIGraphicsContext.graphics.SetClip(new Rectangle(_positionX + GUIGraphicsContext.OffsetX, _positionY + GUIGraphicsContext.OffsetY, _width, m_iItemsPerPage*m_iItemHeight));
      }
      else
      {
        if (_width < 1) return;
        if (_height < 1) return;

        Viewport newviewport = new Viewport();
        newviewport.X = _positionX + GUIGraphicsContext.OffsetX;
        newviewport.Y = _positionY + GUIGraphicsContext.OffsetY;
        newviewport.Width = _width;
        newviewport.Height = _height;
        newviewport.MinZ = 0.0f;
        newviewport.MaxZ = 1.0f;
        GUIGraphicsContext.DX9Device.Viewport = newviewport;
      }
      for (int i = 0; i < 1 + m_iItemsPerPage; i++)
      {
        // render each line
        int dwPosX = _positionX;
        int iItem = i + m_iOffset;
        int iMaxItems = m__itemList.Count;
        if (m__itemList.Count > m_iItemsPerPage && Seperator.Length > 0) iMaxItems++;

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
          if (iItem < m__itemList.Count)
          {
            GUIListItem item = (GUIListItem) m__itemList[iItem];
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
          int dMaxWidth = _width + ixoff;
          if (strLabel2.Length > 0)
          {
            string wszText2;
            float fTextWidth = 0, fTextHeight = 0;
            wszText2 = String.Format("{0}", strLabel2);
            _font.GetTextExtent(wszText2.Trim(), ref fTextWidth, ref fTextHeight);
            dMaxWidth -= (int) (fTextWidth);

            _font.DrawTextWidth((float) dwPosX + dMaxWidth, (float) dwPosY + ioffy, _textColor, wszText2.Trim(), fTextWidth, GUIControl.Alignment.ALIGN_LEFT);
          }
          _font.DrawTextWidth((float) dwPosX, (float) dwPosY + ioffy, _textColor, wszText1.Trim(), (float) dMaxWidth, GUIControl.Alignment.ALIGN_LEFT);
          //            Log.Write("dw _positionY, dwPosY, iScrollY, _scrollOffset: {0} {1} {2} {3}", _positionY, dwPosY, iScrollY, _scrollOffset);
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
          m__itemList.Add(pItem);
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = m__itemList.Count;
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

      _font = GUIFontManager.GetFont(_fontName);
      if (null == _font) return;
      _font.GetTextExtent("abcdef", ref fWidth, ref fHeight);
      try
      {
        m_iItemHeight = (int) fHeight;
        float fTotalHeight = (float) _height;
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
      if (null == _font) return;
      base.AllocResources();

      try
      {
        float fHeight = (float) m_iItemHeight; // + (float)m_iSpaceBetweenItems;
        float fTotalHeight = (float) (_height);
        m_iItemsPerPage = (int) Math.Floor(fTotalHeight/fHeight);
        if (m_iItemsPerPage == 0)
        {
          m_iItemsPerPage = 1;
        }
        int iPages = 1;
        if (m__itemList.Count > 0)
        {
          iPages = m__itemList.Count/m_iItemsPerPage;
          if ((m__itemList.Count%m_iItemsPerPage) != 0) iPages++;
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
      m__itemList.Clear();
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
      get { return _textColor; }
    }


    public string FontName
    {
      get
      {
        if (_font == null) return "";
        return _font.FontName;
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
      if (m__itemList.Count > m_iItemsPerPage)
      {
        if (timeElapsed >= 0.02f) return true;
      }
      return false;
    }

    void SetText(string strText)
    {

      m__itemList.Clear();
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
          if (szLine.Length > 0 || m__itemList.Count > 0)
          {
            GUIListItem item = new GUIListItem(szLine);
            m__itemList.Add(item);
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
          _font.GetTextExtent(wsTmp, ref fwidth, ref fheight);
          if (fwidth > _width)
          {
            if (iLastSpace > 0 && iLastSpaceInLine != lpos)
            {
              szLine = szLine.Substring(0, iLastSpaceInLine);
              pos = iLastSpace;
            }
            if (szLine.Length > 0 || m__itemList.Count > 0)
            {
              GUIListItem item = new GUIListItem(szLine);
              m__itemList.Add(item);
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
        m__itemList.Add(item);
      }

      int istart = -1;
      for (int i = 0; i < m__itemList.Count; ++i)
      {
        GUIListItem item = (GUIListItem) m__itemList[i];
        if (item.Label.Length != 0) istart = -1;
        else if (istart == -1) istart = i;
      }
      if (istart > 0)
      {
        m__itemList.RemoveRange(istart, m__itemList.Count - istart);
      }

		// Set timeElapsed to be 0 so we delay scrolling again
		timeElapsed = 0.0f; 
		_scrollOffset = 0.0f;
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
      m__itemList.Clear();

      iScrollY = 0;
      _scrollOffset = 0.0f;
      _currentFrame = 0;
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
          m__itemList.Clear();
          SetText(value);
        }
      }
    }
  }
}