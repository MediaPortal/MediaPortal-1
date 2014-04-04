#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Linq;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  public class GUITextScrollUpControl : GUIControl
  {
    #region Variables

    #region XML properties

    [XMLSkinElement("scrollStartDelaySec")] protected int _scrollStartDelay = 3;
    [XMLSkinElement("scrollYOffset")] protected int _yOffset = 2;
    [XMLSkinElement("spaceBetweenItems")] protected int _spaceBetweenItems = 2;
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("label")] protected string _property = "";
    [XMLSkinElement("seperator")] protected string _seperator = "";
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("lineSpacing")] protected float _lineSpacing = 1.0f;
    [XMLSkinElement("shadowAngle")] protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")] protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")] protected long _shadowColor = 0xFF000000;

    #endregion

    protected int _offset = 0;
    protected int _itemsPerPage = 10;
    protected int _itemHeight = 10;
    protected int _xOffset = 16;
    protected GUIFont _font = null;
    protected ArrayList _listItems = new ArrayList();
    protected bool _invalidate = false;

    private string _previousProperty = "a";

    private bool _containsProperty = false;
    private int _frameLimiter = 1;
    private double _scrollOffset = 0.0f;
    private double _yPositionScroll = 0.0f;
    private double _timeElapsed = 0.0f;

    #endregion

    #region Constructors

    public GUITextScrollUpControl(int dwParentID)
      : base(dwParentID) {}

    public GUITextScrollUpControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                  string strFont, long dwTextColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _fontName = strFont;

      _textColor = dwTextColor;
    }

    #endregion

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _font = GUIFontManager.GetFont(_fontName);
      if (_property.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScaleVertical(ref _spaceBetweenItems);
    }

    public override void Render(float timePassed)
    {
      try
      {
        _invalidate = false;
        // Nothing visibile - save CPU cycles
        if (null == _font)
        {
          base.Render(timePassed);
          return;
        }
        if (GUIGraphicsContext.EditMode == false)
        {
          if (!IsVisible)
          {
            base.Render(timePassed);
            return;
          }
        }

        int dwPosY = _positionY;

        _timeElapsed += timePassed;
        if (_frameLimiter < GUIGraphicsContext.MaxFPS)
          _frameLimiter++;
        else
          _frameLimiter = 1;

        if (_containsProperty)
        {
          string strText = GUIPropertyManager.Parse(_property);

          strText = strText.Replace("\\r", "\r");
          if (strText != _previousProperty)
          {
            // Reset the scrolling position - e.g. if we switch in TV Guide between various items
            ClearOffsets();

            _previousProperty = strText;
            SetText(strText);
          }
        }

        if (GUIGraphicsContext.graphics != null)
        {
          _offset = 0;
        }
        if (_listItems.Count > _itemsPerPage)
        {
          // rest before we start scrolling
          if ((int)_timeElapsed > _scrollStartDelay)
          {
            _invalidate = true;
            // apply user scroll speed setting. 1 = slowest / 10 = fastest
            //int userSpeed = 11 - GUIGraphicsContext.ScrollSpeedVertical;           //  10 - 1

            //if (_frameLimiter % (6 - GUIGraphicsContext.ScrollSpeedVertical) == 0)
            //  _yPositionScroll++;
            //_yPositionScroll = _yPositionScroll + GUIGraphicsContext.ScrollSpeedVertical;

            int vScrollSpeed = (6 - GUIGraphicsContext.ScrollSpeedVertical);
            if (vScrollSpeed == 0) vScrollSpeed = 1;

            _yPositionScroll += (1.0 / vScrollSpeed) * _lineSpacing; // faster scrolling, if there is bigger linespacing

            dwPosY -= (int)((_yPositionScroll - _scrollOffset));
            // Log.Debug("*** _frameLimiter: {0}, dwPosY: {1}, _scrollOffset: {2}, _yPositionScroll: {3}", _frameLimiter, dwPosY, _scrollOffset, _yPositionScroll);

            if (_positionY - dwPosY >= _itemHeight * _lineSpacing)
            {
              // one line has been scrolled away entirely
              dwPosY += (int)(_itemHeight * _lineSpacing);
              _scrollOffset += (_itemHeight * _lineSpacing);
              _offset++;
              if (_offset >= _listItems.Count)
              {
                // restart with the first line
                if (Seperator.Length > 0)
                {
                  if (_offset >= _listItems.Count + 1)
                  {
                    _offset = 0;
                  }
                }
                else
                {
                  _offset = 0;
                }
              }
            }
          }
          else
          {
            _scrollOffset = 0.0f;
            _frameLimiter = 1;
            _offset = 0;
          }
        }
        else
        {
          _scrollOffset = 0.0f;
          _frameLimiter = 1;
          _offset = 0;
        }

        if (GUIGraphicsContext.graphics != null)
        {
          GUIGraphicsContext.graphics.SetClip(new Rectangle(_positionX, _positionY, _width, _height));
        }
        else
        {
          if (_width < 1 || _height < 1)
          {
            base.Render(timePassed);
            return;
          }

          Rectangle clipRect = new Rectangle(_positionX, _positionY, _width, _height);
          GUIGraphicsContext.BeginClip(clipRect);
        }
        long color = _textColor;
        if (Dimmed)
        {
          color &= DimColor;
        }
        for (int i = 0; i < 2 + _itemsPerPage; i++) // add one as the itemsPerPage might be almost one less than actual height plus one for the "incoming" item
        {
          // skip half lines - enable, if only full lines should be visible on initial view (before scrolling starts)
          // if (!_invalidate && i >= _itemsPerPage) continue;

          // render each line
          int dwPosX = _positionX;
          int iItem = i + _offset;
          int iMaxItems = _listItems.Count;
          if (_listItems.Count > _itemsPerPage && Seperator.Length > 0)
          {
            iMaxItems++;
          }

          if (iItem >= iMaxItems)
          {
            if (iMaxItems > _itemsPerPage)
            {
              iItem -= iMaxItems;
            }
            else
            {
              break;
            }
          }

          if (iItem >= 0 && iItem < iMaxItems)
          {
            // render item
            string strLabel1 = "", strLabel2 = "";
            if (iItem < _listItems.Count)
            {
              GUIListItem item = (GUIListItem)_listItems[iItem];
              strLabel1 = item.Label;
              strLabel2 = item.Label2;
            }
            else
            {
              strLabel1 = Seperator;
            }

            int ixoff = _xOffset;
            int ioffy = _yOffset;
            GUIGraphicsContext.ScaleVertical(ref ioffy);
            GUIGraphicsContext.ScaleHorizontal(ref ixoff);
            string wszText1 = String.Format("{0}", strLabel1);
            int dMaxWidth = _width + ixoff;
            float x = dwPosX;
            if (strLabel2.Length > 0)
            {
              string wszText2;
              float fTextWidth = 0, fTextHeight = 0;
              wszText2 = String.Format("{0}", strLabel2);
              _font.GetTextExtent(wszText2.Trim(), ref fTextWidth, ref fTextHeight);
              dMaxWidth -= (int)(fTextWidth);

              switch (_textAlignment)
              {
                case Alignment.ALIGN_LEFT:
                case Alignment.ALIGN_CENTER:
                  x = dwPosX + dMaxWidth;
                  break;

                case Alignment.ALIGN_RIGHT:
                  x = dwPosX + dMaxWidth + _width;
                  break;
              }

              uint aColor = GUIGraphicsContext.MergeAlpha((uint)color);
              if (Shadow)
              {
                uint sColor = GUIGraphicsContext.MergeAlpha((uint)_shadowColor);
                _font.DrawShadowTextWidth(x, (float)dwPosY + ioffy,
                                          (uint)GUIGraphicsContext.MergeAlpha((uint)_textColor), wszText2.Trim(),
                                          _textAlignment,
                                          _shadowAngle, _shadowDistance, sColor, (float)dMaxWidth);
              }
              else
              {
                _font.DrawTextWidth(x, (float)dwPosY + ioffy, (uint)GUIGraphicsContext.MergeAlpha((uint)_textColor),
                                    wszText2.Trim(), fTextWidth, _textAlignment);
              }
            }

            switch (_textAlignment)
            {
              case Alignment.ALIGN_CENTER:
              case Alignment.ALIGN_LEFT:
                x = dwPosX;
                break;

              case Alignment.ALIGN_RIGHT:
                x = dwPosX + _width;
                break;
            }
            {
              uint aColor = GUIGraphicsContext.MergeAlpha((uint)color);
              if (Shadow)
              {
                uint sColor = GUIGraphicsContext.MergeAlpha((uint)_shadowColor);
                _font.DrawShadowTextWidth(x, (float)dwPosY + ioffy, (uint)GUIGraphicsContext.MergeAlpha((uint)_textColor),
                                          wszText1.Trim(), _textAlignment,
                                          _shadowAngle, _shadowDistance, sColor, (float)dMaxWidth);
              }
              else
              {
                _font.DrawTextWidth(x, (float)dwPosY + ioffy, (uint)GUIGraphicsContext.MergeAlpha((uint)_textColor),
                                    wszText1.Trim(), (float)dMaxWidth, _textAlignment);
              }

              // Log.Info("dw _positionY, dwPosY, _yPositionScroll, _scrollOffset: {0} {1} {2} {3} - wszText1.Trim() {4}", _positionY, dwPosY, _yPositionScroll, _scrollOffset, wszText1.Trim());

              dwPosY += (int)(_itemHeight * _lineSpacing);
            }
          }
        }

        if (GUIGraphicsContext.graphics != null)
        {
          GUIGraphicsContext.graphics.SetClip(new Rectangle(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height));
        }
        else
        {
          GUIGraphicsContext.EndClip();
        }
        base.Render(timePassed);
      }
      catch (Exception ex)
      {
        Log.Error("GUITextScrollUpControl: Error during the render process - maybe a threading issue. {0}",
                  ex.ToString());
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          _containsProperty = false;
          _property = "";
          GUIListItem pItem = (GUIListItem)message.Object;
          pItem.DimColor = DimColor;
          _listItems.Add(pItem);
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          Clear();
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = _listItems.Count;
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
        }
      }

      if (base.OnMessage(message))
      {
        return true;
      }

      return false;
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      float fWidth = 0, fHeight = 0;

      _font = GUIFontManager.GetFont(_fontName);
      if (null == _font)
      {
        return;
      }
      _font.GetTextExtent("afy", ref fWidth, ref fHeight);
      try
      {
        _itemHeight = (int)(fHeight);
        float fTotalHeight = (float)_height;
        _itemsPerPage = (int)Math.Floor(fTotalHeight / (fHeight  * _lineSpacing));
        if (_itemsPerPage == 0)
        {
          _itemsPerPage = 1;
        }
      }
      catch (Exception)
      {
        _itemHeight = 1;
        _itemsPerPage = 1;
      }
    }


    public override void AllocResources()
    {
      if (null == _font)
      {
        return;
      }
      base.AllocResources();

      try
      {
        float fHeight = (float)_itemHeight;
        float fTotalHeight = (float)(_height);
        _itemsPerPage = (int)Math.Floor(fTotalHeight / (fHeight * _lineSpacing));
        if (_itemsPerPage == 0)
        {
          _itemsPerPage = 1;
        }
      }
      catch (Exception)
      {
        _itemsPerPage = 1;
      }
    }

    public override void Dispose()
    {
      _previousProperty = "";
      _listItems.DisposeAndClearList();
      _font = null;
      base.Dispose();
    }


    public int ItemHeight
    {
      get { return _itemHeight; }
      set { _itemHeight = value; }
    }

    public float LineSpacing
    {
      get { return _lineSpacing; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _lineSpacing = value;
      }
    }

    public int Space
    {
      get { return _spaceBetweenItems; }
      set { _spaceBetweenItems = value; }
    }


    public long TextColor
    {
      get { return _textColor; }
    }


    public string FontName
    {
      get
      {
        if (_font == null)
        {
          return "";
        }
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
      if (_listItems.Count > _itemsPerPage)
      {
        if (_timeElapsed >= 0.02f)
        {
          return true;
        }
      }
      return false;
    }

    private void SetText(string strText)
    {
      try
      {
        _listItems.DisposeAndClearList();
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
            if (szLine.Length > 0 || _listItems.Count > 0)
            {
              GUIListItem item = new GUIListItem(szLine);
              item.DimColor = DimColor;
              _listItems.Add(item);
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
            if (fwidth > (_width - _xOffset))
            {
              if (iLastSpace > 0 && iLastSpaceInLine != lpos)
              {
                szLine = szLine.Substring(0, iLastSpaceInLine);
                pos = iLastSpace;
              }
              if (szLine.Length > 0 || _listItems.Count > 0)
              {
                GUIListItem item = new GUIListItem(szLine);
                item.DimColor = DimColor;
                _listItems.Add(item);
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
          item.DimColor = DimColor;
          _listItems.Add(item);
        }

        int istart = -1;
        for (int i = 0; i < _listItems.Count; ++i)
        {
          GUIListItem item = (GUIListItem)_listItems[i];
          if (item.Label.Length != 0)
          {
            istart = -1;
          }
          else if (istart == -1)
          {
            istart = i;
          }
        }
        if (istart > 0)
        {
          _listItems.RemoveRange(istart, _listItems.Count - istart);
        }

        // Set _timeElapsed to be 0 so we delay scrolling again
        _timeElapsed = 0.0f;
        _scrollOffset = 0.0f;
      }
      catch (Exception ex)
      {
        Log.Error("TextBoxScrollUp: Error in SetText - {0}", ex.ToString());
      }
    }


    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Property
    {
      get { return _property; }
      set
      {
        _property = value;
        if (_property.IndexOf("#") >= 0)
        {
          _containsProperty = true;
        }
      }
    }

    public string Seperator
    {
      get { return _seperator; }
      set { _seperator = value; }
    }

    public void Clear()
    {
      _containsProperty = false;
      _property = "";
      ClearOffsets();
    }

    /// <summary>
    /// Resets the scrolling offsets
    /// </summary>
    private void ClearOffsets()
    {
      _offset = 0;
      _listItems.DisposeAndClearList();

      _yPositionScroll = 0.0f;
      _scrollOffset = 0.0f;
      _frameLimiter = 1;
      _timeElapsed = 0.0f;
    }

    public string Label
    {
      set
      {
        if (_property != value)
        {
          Clear();
          Property = value; // use setter to remove redundant code
          SetText(value);
        }
      }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        foreach (GUIListItem item in _listItems)
        {
          item.DimColor = value;
        }
      }
    }

    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    public bool HasText
    {
      get { return this._listItems.Count > 0; }
    }

    public int ShadowAngle
    {
      get { return _shadowAngle; }
      set { _shadowAngle = value; }
    }

    public int ShadowDistance
    {
      get { return _shadowDistance; }
      set { _shadowDistance = value; }
    }

    public long ShadowColor
    {
      get { return _shadowColor; }
      set { _shadowColor = value; }
    }

    private bool Shadow
    {
      get { return (_shadowDistance > 0) && ((_shadowColor >> 24) > 0); }
    }
  }
}