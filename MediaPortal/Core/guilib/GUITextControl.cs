#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Collections;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITextControl : GUIControl
  {
    public enum ListType
    {
      CONTROL_LIST,
      CONTROL_UPDOWN
    } ;

    protected int _spaceBetweenItems = 4;
    protected int _offset = 0;
    protected int _itemsPerPage = 10;
    protected int _itemHeight = 10;
    protected int _textOffsetX = 0;
    protected int _textOffsetY = 0;
    protected int _textOffsetX2 = 0;
    protected int _textOffsetY2 = 0;
    protected int _imageWidth = 16;
    protected int _imageHeight = 16;

    protected GUIFont _font = null;
    protected GUISpinControl _upDownControl = null;
    protected string _suffix = "|";
    protected ArrayList _itemList = new ArrayList();
    protected bool _upDownEnabled = true;

    protected int _scrollPosition = 0;
    protected int _scrollPosititionX = 0;
    protected int _lastItem = -1;
    protected int _frames = 0;
    protected int _startFrame = 0;
    [XMLSkinElement("font")] protected string _fontName = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textureUp")] protected string _upTextureName;
    [XMLSkinElement("textureDown")] protected string _downTextureName;
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus;
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus;
    [XMLSkinElement("spinHeight")] protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")] protected int _spinControlWidth;
    [XMLSkinElement("spinColor")] protected long _colorSpinColor;
    [XMLSkinElement("spinPosX")] protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")] protected int _spinControlPositionY;
    [XMLSkinElement("label")] protected string _property = "";
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;

    private bool _containsProperty = false;
    private string _previousProperty = "a";

    public GUITextControl(int dwParentID)
      : base(dwParentID)
    {
    }

    public GUITextControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                          string strFontName,
                          int dwSpinWidth, int dwSpinHeight,
                          string strUp, string strDown,
                          string strUpFocus, string strDownFocus,
                          long dwSpinColor, int dwSpinX, int dwSpinY,
                          long dwTextColor)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _fontName = strFontName;
      _spinControlHeight = dwSpinHeight;
      _spinControlWidth = dwSpinWidth;
      _upTextureName = strUp;
      _upTextureNameFocus = strUpFocus;
      _downTextureName = strDown;
      _downTextureNameFocus = strDownFocus;
      _colorSpinColor = dwSpinColor;
      _spinControlPositionX = dwSpinX;
      _spinControlPositionY = dwSpinY;
      _textColor = dwTextColor;
      FinalizeConstruction();
      DimColor = base.DimColor; // let us init all GUIControls with DimColor
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _upDownControl = new GUISpinControl(_controlId, _controlId, _spinControlPositionX, _spinControlPositionY,
                                          _spinControlWidth, _spinControlHeight, _upTextureName, _downTextureName,
                                          _upTextureNameFocus, _downTextureNameFocus, _fontName, _colorSpinColor,
                                          GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, Alignment.ALIGN_LEFT);
      _upDownControl.ParentControl = this;
      _font = GUIFontManager.GetFont(_fontName);
      if (_property.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScaleRectToScreenResolution(ref _spinControlPositionX, ref _spinControlPositionY,
                                                     ref _spinControlWidth, ref _spinControlHeight);
    }

    public override void Render(float timePassed)
    {
      if (null == _font)
      {
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

      if (_containsProperty)
      {
        string strText = GUIPropertyManager.Parse(_property);

        strText = strText.Replace("\\r", "\r");
        if (strText != _previousProperty)
        {
          _offset = 0;
          _itemList.Clear();

          _previousProperty = strText;
          SetText(strText);
        }
      }

      int dwPosY = _positionY;

      for (int i = 0; i < _itemsPerPage; i++)
      {
        int dwPosX = _positionX;
        if (i + _offset < _itemList.Count)
        {
          // render item
          GUIListItem item = (GUIListItem) _itemList[i + _offset];
          string strLabel1 = item.Label;
          string strLabel2 = item.Label2;

          string wszText1 = String.Format("{0}", strLabel1);
          int dMaxWidth = _width + 16;
          float x = 0;
          if (strLabel2.Length > 0)
          {
            string wszText2;
            float fTextWidth = 0, fTextHeight = 0;
            wszText2 = String.Format("{0}", strLabel2);
            _font.GetTextExtent(wszText2, ref fTextWidth, ref fTextHeight);
            dMaxWidth -= (int) (fTextWidth);
            _font.DrawTextWidth((float) dwPosX + dMaxWidth, (float) dwPosY + 2, _textColor, wszText2, (float) fTextWidth,
                                _textAlignment);
          }
          switch (_textAlignment)
          {
            case Alignment.ALIGN_RIGHT:
              x = (float) dwPosX + _width;
              break;
            default:
              x = (float) dwPosX;
              break;
          }
          _font.DrawTextWidth(x, (float) dwPosY + 2, _textColor, wszText1, (float) dMaxWidth, _textAlignment);
          dwPosY += (int) _itemHeight;
        }
      }
      if (_upDownEnabled)
      {
        int iPages = _itemList.Count/_itemsPerPage;
        if ((_itemList.Count%_itemsPerPage) != 0)
        {
          iPages++;
        }

        if (iPages > 1)
        {
          _upDownControl.Render(timePassed);
        }
      }
      base.Render(timePassed);
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          OnDown();
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          OnUp();
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          OnLeft();
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          OnRight();
          break;

        default:
          int ivalue = _upDownControl.Value;
          _upDownControl.OnAction(action);
          if (_upDownControl.Value != ivalue)
          {
            _offset = (_upDownControl.Value - 1)*_itemsPerPage;
          }
          break;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        if (message.SenderControlId == 0)
        {
          if (message.Message == GUIMessage.MessageType.GUI_MSG_CLICKED)
          {
            _offset = (_upDownControl.Value - 1)*_itemsPerPage;
            while (_offset >= _itemList.Count)
            {
              _offset--;
            }
          }
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_ITEM)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < _itemList.Count)
          {
            message.Object = _itemList[iItem];
          }
          else
          {
            message.Object = null;
          }
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
            message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
        {
          base.OnMessage(message);
          _upDownControl.Focus = Focus;
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM)
        {
          int iItem = _offset;
          if (iItem >= 0 && iItem < _itemList.Count)
          {
            message.Object = _itemList[iItem];
          }
          else
          {
            message.Object = null;
          }
          return true;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_ADD)
        {
          _containsProperty = false;
          _property = "";
          GUIListItem pItem = message.Object as GUIListItem;
          if (pItem != null)
          {
            _itemList.Add(pItem);
            Calculate();
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET)
        {
          _containsProperty = false;
          _property = "";
          _offset = 0;
          _itemList.Clear();
          _upDownControl.SetRange(1, 1);
          _upDownControl.Value = 1;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEMS)
        {
          message.Param1 = _itemList.Count;
        }
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL2_SET)
        {
          int iItem = message.Param1;
          if (iItem >= 0 && iItem < _itemList.Count)
          {
            GUIListItem item = (GUIListItem) _itemList[iItem];
            item.Label2 = message.Label;
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
        }

        if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED)
        {
          message.Param1 = _offset;
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
      if (null == _font)
      {
        return;
      }
      base.PreAllocResources();
      _upDownControl.PreAllocResources();
    }


    public override void AllocResources()
    {
      if (null == _font)
      {
        return;
      }
      base.AllocResources();
      _upDownControl.AllocResources();

      _font = GUIFontManager.GetFont(_fontName);
      _upDownControl.WindowId = this._windowId;
      Calculate();
    }

    public override void FreeResources()
    {
      _previousProperty = "";
      _itemList.Clear();
      base.FreeResources();
      _upDownControl.FreeResources();
    }

    protected void OnRight()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_RIGHT;
      _upDownControl.OnAction(action);
      if (!_upDownControl.Focus)
      {
        Focus = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _rightControlId,
                                        (int) action.wID, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }
    }


    protected void OnLeft()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_LEFT;
      _upDownControl.OnAction(action);
      if (!_upDownControl.Focus)
      {
        Focus = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _leftControlId,
                                        (int) action.wID, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }
    }


    protected void OnUp()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_UP;
      _upDownControl.OnAction(action);
      if (!_upDownControl.Focus)
      {
        Focus = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _upControlId,
                                        (int) action.wID, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }
    }

    protected void OnDown()
    {
      Action action = new Action();
      action.wID = Action.ActionType.ACTION_MOVE_DOWN;
      _upDownControl.OnAction(action);

      if (!_upDownControl.Focus)
      {
        Focus = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, _downControlId,
                                        (int) action.wID, 0, null);
        GUIGraphicsContext.SendMessage(msg);
      }
    }

    public String ScrollySuffix
    {
      get { return _suffix; }
      set
      {
        if (value == null)
        {
          return;
        }
        _suffix = value;
      }
    }

    protected void OnPageUp()
    {
      int iPage = _upDownControl.Value;
      if (iPage > 1)
      {
        iPage--;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1)*_itemsPerPage;
      }
      else
      {
        // already on page 1, then select the 1st item
      }
    }

    protected void OnPageDown()
    {
      int iPages = _itemList.Count/_itemsPerPage;
      if ((_itemList.Count%_itemsPerPage) != 0)
      {
        iPages++;
      }

      int iPage = _upDownControl.Value;
      if (iPage + 1 <= iPages)
      {
        iPage++;
        _upDownControl.Value = iPage;
        _offset = (_upDownControl.Value - 1)*_itemsPerPage;
      }
      else
      {
        // already on last page, move 2 last item in list
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, WindowId, GetID, GetID,
                                        _itemList.Count - 1, 0, null);
        OnMessage(msg);
      }
    }

    public void SetTextOffsets(int iXoffset, int iYOffset, int iXoffset2, int iYOffset2)
    {
      if (iXoffset < 0 || iYOffset < 0)
      {
        return;
      }
      if (iXoffset2 < 0 || iYOffset2 < 0)
      {
        return;
      }
      _textOffsetX = iXoffset;
      _textOffsetY = iYOffset;
      _textOffsetX2 = iXoffset2;
      _textOffsetY2 = iYOffset2;
    }

    public void SetImageDimensions(int iWidth, int iHeight)
    {
      if (iWidth < 0 || iHeight < 0)
      {
        return;
      }
      _imageWidth = iWidth;
      _imageHeight = iHeight;
    }

    public int ItemHeight
    {
      get { return _itemHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _itemHeight = value;
      }
    }

    public int Space
    {
      get { return _spaceBetweenItems; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _spaceBetweenItems = value;
      }
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

    public int SpinWidth
    {
      get { return _upDownControl.Width/2; }
    }

    public int SpinHeight
    {
      get { return _upDownControl.Height; }
    }

    public string TexutureUpName
    {
      get { return _upDownControl.TexutureUpName; }
    }

    public string TexutureDownName
    {
      get { return _upDownControl.TexutureDownName; }
    }

    public string TexutureUpFocusName
    {
      get { return _upDownControl.TexutureUpFocusName; }
    }

    public string TexutureDownFocusName
    {
      get { return _upDownControl.TexutureDownFocusName; }
    }

    public long SpinTextColor
    {
      get { return _upDownControl.TextColor; }
    }

    public int SpinX
    {
      get { return _upDownControl.XPosition; }
    }

    public int SpinY
    {
      get { return _upDownControl.YPosition; }
    }


    public int TextOffsetX
    {
      get { return _textOffsetX; }
    }

    public int TextOffsetY
    {
      get { return _textOffsetY; }
    }

    public int TextOffsetX2
    {
      get { return _textOffsetX2; }
    }

    public int TextOffsetY2
    {
      get { return _textOffsetY2; }
    }

    public int ImageWidth
    {
      get { return _imageWidth; }
    }

    public int ImageHeight
    {
      get { return _imageHeight; }
    }

    public string Suffix
    {
      get { return _suffix; }
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      int id;
      bool focus;
      if (_upDownControl.HitTest(x, y, out id, out focus))
      {
        if (_upDownControl.GetMaximum() > 1)
        {
          _upDownControl.Focus = true;
          return true;
        }
        return true;
      }
      if (!base.HitTest(x, y, out id, out focus))
      {
        Focus = false;
        return false;
      }
      return false;
    }

    private void SetText(string strText)
    {
      if (strText == null)
      {
        return;
      }
      _itemList.Clear();
      // start wordwrapping
      // Set a flag so we can determine initial justification effects
      //bool bStartingNewLine = true;
      //bool bBreakAtSpace = false;
      int pos = 0;
      int lpos = 0;
      int iLastSpace = -1;
      int iLastSpaceInLine = -1;
      string szLine = "";
      while (pos < (int) strText.Length)
      {
        // Get the current letter in the string
        char letter = (char) strText[pos];

        // Handle the newline character
        if (letter == '\n')
        {
          GUIListItem item = new GUIListItem(szLine);
          _itemList.Add(item);
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
            GUIListItem item = new GUIListItem(szLine);
            _itemList.Add(item);
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
        _itemList.Add(item);
      }

      Calculate();
    }

    private void Calculate()
    {
      float fWidth = 0, fHeight = 0;
      if (_font == null)
      {
        return;
      }
      _font.GetTextExtent("y", ref fWidth, ref fHeight);
      //fHeight+=10.0f;

      //fHeight+=2;
      if (fHeight <= 0)
      {
        fHeight = 1;
      }
      _itemHeight = (int) fHeight;

      float fTotalHeight = (float) (_height);
      _itemsPerPage = (int) (fTotalHeight/fHeight);
      if (_itemsPerPage == 0)
      {
        _itemsPerPage = 1;
      }

      int iPages = _itemList.Count/_itemsPerPage;
      if ((_itemList.Count%_itemsPerPage) > 0)
      {
        iPages++;
      }
      if (iPages > 1)
      {
        fTotalHeight = (float) (_height - _upDownControl.Height - 5);
        _itemsPerPage = (int) (fTotalHeight/fHeight);

        if (_itemsPerPage == 0)
        {
          _itemsPerPage = 1;
        }
        iPages = _itemList.Count/_itemsPerPage;
        if ((_itemList.Count%_itemsPerPage) > 0)
        {
          iPages++;
        }
      }
      _upDownControl.SetRange(1, iPages);
      _upDownControl.Value = 1;
      if (iPages == 0)
      {
        _upDownControl.IsVisible = false;
      }
      else
      {
        _upDownControl.IsVisible = true;
      }
    }

    public bool EnableUpDown
    {
      get { return _upDownEnabled; }
      set { _upDownEnabled = value; }
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

    public void Clear()
    {
      _containsProperty = false;
      _property = "";
      _offset = 0;
      _itemList.Clear();
      _upDownControl.SetRange(1, 1);
      _upDownControl.Value = 1;
    }

    public string Label
    {
      set
      {
        if (_property != value || _itemList.Count == 0)
        {
          _property = value;
          if (_property.IndexOf("#") >= 0)
          {
            _containsProperty = true;
          }

          _itemList.Clear();
          _upDownControl.SetRange(1, 1);
          _upDownControl.Value = 1;
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
        if (_upDownControl != null)
        {
          _upDownControl.DimColor = value;
        }
      }
    }

    public bool HasText
    {
      get { return this._itemList.Count > 0; }
    }
  }
}