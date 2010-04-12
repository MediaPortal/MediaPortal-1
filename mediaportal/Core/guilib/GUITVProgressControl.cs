#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// todo : 
  /// - specify fill colors 2,3,4
  /// - seperate graphic for big-tick displaying current position
  /// - specify x/y position for fill color start
  /// </summary>
  public class GUITVProgressControl : GUIControl
  {
    private GUIAnimation _imageTop = null;
    private GUIAnimation _imageLogo = null;
    private GUIAnimation _imageBottom = null;
    private GUIAnimation _imageTick = null;
    private GUIAnimation _imageFillBackground = null;
    private GUIAnimation _imageFill1 = null;
    private GUIAnimation _imageFill2 = null;
    private GUIAnimation _imageFill3 = null;
    private GUIAnimation _imageLeft = null;
    private GUIAnimation _imageMid = null;
    private GUIAnimation _imageRight = null;
    private float _percentage1 = 0;
    private float _percentage2 = 0;
    private float _percentage3 = 0;


    [XMLSkinElement("label")] private string _propertyLabel = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("font")] protected string _fontName = "";

    private GUIFont _font = null;
    [XMLSkinElement("startlabel")] private string _labelLeft = "";
    [XMLSkinElement("endlabel")] private string _labelRight = "";
    [XMLSkinElement("toplabel")] private string _labelTop = "";
    [XMLSkinElement("fillbgxoff")] protected int _fillBackgroundOffsetX;
    [XMLSkinElement("fillbgyoff")] protected int _fillBackgroundOffsetY;
    [XMLSkinElement("fillheight")] protected int _fillBackgroundHeight;


    [XMLSkinElement("label")] private string _label1 = "";
    [XMLSkinElement("label1")] private string _label2 = "";
    [XMLSkinElement("label2")] private string _label3 = "";
    [XMLSkinElement("TextureOffsetY")] protected int _topTextureOffsetY = 0;
    [XMLSkinElement("toptexture")] protected string _topTextureName;
    [XMLSkinElement("bottomtexture")] protected string _bottomTextureName;
    [XMLSkinElement("fillbackgroundtexture")] protected string _fillBackGroundTextureName;
    [XMLSkinElement("lefttexture")] protected string _leftTextureName;
    [XMLSkinElement("midtexture")] protected string _midTextureName;
    [XMLSkinElement("righttexture")] protected string _rightTextureName;
    [XMLSkinElement("texturetick")] protected string _tickTextureName;
    [XMLSkinElement("filltexture1")] protected string _tickFill1TextureName;
    [XMLSkinElement("filltexture2")] protected string _tickFill2TextureName;
    [XMLSkinElement("filltexture3")] protected string _tickFill3TextureName;
    [XMLSkinElement("logotexture")] protected string _logoTextureName;

    public GUITVProgressControl(int dwParentID)
      : base(dwParentID) {}

    public GUITVProgressControl(int dwParentID, int dwControlId, int dwPosX,
                                int dwPosY, int dwWidth, int dwHeight,
                                string strBackGroundTexture, string strBackBottomTexture,
                                string strTextureFillBackground,
                                string strLeftTexture, string strMidTexture,
                                string strRightTexture, string strTickTexure,
                                string strTextureFill1, string strTextureFill2, string strTextureFill3,
                                string strLogoTextureName)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _topTextureName = strBackGroundTexture;
      _bottomTextureName = strBackBottomTexture;
      _fillBackGroundTextureName = strTextureFillBackground;
      _leftTextureName = strLeftTexture;
      _rightTextureName = strRightTexture;
      _tickTextureName = strTickTexure;
      _tickFill1TextureName = strTextureFill1;
      _tickFill2TextureName = strTextureFill2;
      _tickFill3TextureName = strTextureFill3;
      _logoTextureName = strLogoTextureName;
      FinalizeConstruction();
      DimColor = base.DimColor;
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_topTextureName == null)
      {
        _topTextureName = string.Empty;
      }
      if (_bottomTextureName == null)
      {
        _bottomTextureName = string.Empty;
      }
      if (_leftTextureName == null)
      {
        _leftTextureName = string.Empty;
      }
      if (_midTextureName == null)
      {
        _midTextureName = string.Empty;
      }
      if (_rightTextureName == null)
      {
        _rightTextureName = string.Empty;
      }
      if (_tickTextureName == null)
      {
        _tickTextureName = string.Empty;
      }
      if (_tickFill1TextureName == null)
      {
        _tickFill1TextureName = string.Empty;
      }
      if (_tickFill2TextureName == null)
      {
        _tickFill2TextureName = string.Empty;
      }
      if (_tickFill3TextureName == null)
      {
        _tickFill3TextureName = string.Empty;
      }
      if (_fillBackGroundTextureName == null)
      {
        _fillBackGroundTextureName = string.Empty;
      }
      if (_logoTextureName == null)
      {
        _logoTextureName = string.Empty;
      }
      _imageTop = LoadAnimationControl(_parentControlId, _controlId, 0, 0, 0, 0, _topTextureName);
      _imageBottom = LoadAnimationControl(_parentControlId, _controlId, 0, 0, 0, 0, _bottomTextureName);
      _imageLeft = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _leftTextureName);
      _imageMid = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _midTextureName);
      _imageRight = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _rightTextureName);
      _imageTick = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _tickTextureName);
      _imageFill1 = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0,
                                         _tickFill1TextureName);
      _imageFill2 = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0,
                                         _tickFill2TextureName);
      _imageFill3 = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0,
                                         _tickFill3TextureName);
      _imageFillBackground = LoadAnimationControl(_parentControlId, _controlId, 0, 0, 0, 0, _fillBackGroundTextureName);
      _imageTop.KeepAspectRatio = false;
      _imageBottom.KeepAspectRatio = false;
      _imageMid.KeepAspectRatio = false;
      _imageRight.KeepAspectRatio = false;
      _imageTick.KeepAspectRatio = false;
      _imageFill1.KeepAspectRatio = false;
      _imageFill2.KeepAspectRatio = false;
      _imageFill3.KeepAspectRatio = false;
      _imageFillBackground.KeepAspectRatio = false;

      _imageTop.ParentControl = this;
      _imageBottom.ParentControl = this;
      _imageMid.ParentControl = this;
      _imageRight.ParentControl = this;
      _imageTick.ParentControl = this;
      _imageFill1.ParentControl = this;
      _imageFill2.ParentControl = this;
      _imageFill3.ParentControl = this;
      _imageFillBackground.ParentControl = this;
      _imageLogo = LoadAnimationControl(_parentControlId, _controlId, 0, 0, 0, 0, _logoTextureName);
      _imageLogo.ParentControl = this;
      FontName = _fontName;
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _fillBackgroundOffsetX, ref _fillBackgroundOffsetY);
      GUIGraphicsContext.ScaleVertical(ref _fillBackgroundHeight);
      GUIGraphicsContext.ScaleVertical(ref _topTextureOffsetY);
    }

    public override bool Dimmed
    {
      get { return base.Dimmed; }
      set
      {
        base.Dimmed = value;
        if (_imageTop != null)
        {
          _imageTop.Dimmed = value;
        }
        if (_imageLogo != null)
        {
          _imageLogo.Dimmed = value;
        }
        if (_imageBottom != null)
        {
          _imageBottom.Dimmed = value;
        }
        if (_imageTick != null)
        {
          _imageTick.Dimmed = value;
        }
        if (_imageFillBackground != null)
        {
          _imageFillBackground.Dimmed = value;
        }
        if (_imageFill1 != null)
        {
          _imageFill1.Dimmed = value;
        }
        if (_imageFill2 != null)
        {
          _imageFill2.Dimmed = value;
        }
        if (_imageFill3 != null)
        {
          _imageFill3.Dimmed = value;
        }
        if (_imageLeft != null)
        {
          _imageLeft.Dimmed = value;
        }
        if (_imageMid != null)
        {
          _imageMid.Dimmed = value;
        }
        if (_imageRight != null)
        {
          _imageRight.Dimmed = value;
        }
      }
    }

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
        if (Disabled)
        {
          base.Render(timePassed);
          return;
        }
      }
      if (_propertyLabel.Length > 0)
      {
        string m_strText = GUIPropertyManager.Parse(_propertyLabel);
        if (m_strText.Length > 0)
        {
          try
          {
            Percentage1 = float.Parse(m_strText);
          }
          catch (Exception) {}
          if (Percentage1 < 0 || Percentage1 > 100)
          {
            Percentage1 = 0;
          }
        }
      }
      if (Label1.Length > 0)
      {
        string strText = GUIPropertyManager.Parse(Label1);
        if (strText.Length > 0)
        {
          try
          {
            Percentage1 = float.Parse(strText);
          }
          catch (Exception) {}
          if (Percentage1 < 0 || Percentage1 > 100)
          {
            Percentage1 = 0;
          }
        }
      }

      if (Label2.Length > 0)
      {
        string strText = GUIPropertyManager.Parse(Label2);
        if (strText.Length > 0)
        {
          try
          {
            Percentage2 = float.Parse(strText);
          }
          catch (Exception) {}
          if (Percentage2 < 0 || Percentage2 > 100)
          {
            Percentage2 = 0;
          }
        }
      }
      if (Label3.Length > 0)
      {
        string strText = GUIPropertyManager.Parse(Label3);
        if (strText.Length > 0)
        {
          try
          {
            Percentage3 = float.Parse(strText);
          }
          catch (Exception) {}
          if (Percentage3 < 0 || Percentage3 > 100)
          {
            Percentage3 = 0;
          }
        }
      }

      int xPos = _positionX;
      _imageLeft.SetPosition(xPos, _positionY);

      xPos = _positionX + _imageLeft.TextureWidth;
      _imageMid.SetPosition(xPos, _positionY);

      int iWidth = _width - (_imageLeft.TextureWidth + _imageRight.TextureWidth);
      _imageMid.Width = iWidth;

      xPos = iWidth + _positionX + _imageLeft.TextureWidth;
      _imageRight.SetPosition(xPos, _positionY);

      _imageLeft.Render(timePassed);
      _imageRight.Render(timePassed);
      _imageMid.Render(timePassed);

      int iWidth1 = 0, iWidth2 = 0, iWidth3 = 0;

      iWidth -= 2 * _fillBackgroundOffsetX;
      float fWidth = iWidth;
      int iCurPos = 0;
      // render fillbkg

      xPos = _positionX + _imageLeft.TextureWidth + _fillBackgroundOffsetX;
      _imageFillBackground.Width = iWidth;
      _imageFillBackground.Height = _imageMid.TextureHeight - _fillBackgroundOffsetY * 2;
      _imageFillBackground.SetPosition(xPos, _positionY + _fillBackgroundOffsetY);
      _imageFillBackground.Render(timePassed);

      // render first color
      int xoff = GUIGraphicsContext.ScaleHorizontal(3);

      xPos = _positionX + _imageLeft.TextureWidth + _fillBackgroundOffsetX + xoff;
      int yPos = _imageFillBackground.YPosition + (_imageFillBackground.Height / 2) - (_fillBackgroundHeight / 2);
      if (yPos < _positionY)
      {
        yPos = _positionY;
      }
      fWidth = (float)iWidth;
      fWidth /= 100.0f;
      fWidth *= (float)Percentage1;
      iWidth1 = (int)Math.Floor(fWidth);
      if (iWidth1 > 0)
      {
        _imageFill1.Height = _fillBackgroundHeight;
        _imageFill1.Width = iWidth1;
        _imageFill1.SetPosition(xPos, yPos);
        _imageFill1.Render(timePassed); // red
      }
      iCurPos = iWidth1 + xPos;

      //render 2nd color
      float fPercent;
      if (Percentage2 >= Percentage1)
      {
        fPercent = Percentage2 - Percentage1;
      }
      else
      {
        fPercent = 0;
      }
      fWidth = (float)iWidth;
      fWidth /= 100.0f;
      fWidth *= (float)fPercent;
      iWidth2 = (int)Math.Floor(fWidth);
      if (iWidth2 > 0)
      {
        _imageFill2.Width = iWidth2;
        _imageFill2.Height = _fillBackgroundHeight;
        _imageFill2.SetPosition(iCurPos, yPos);
        _imageFill2.Render(timePassed);
      }
      iCurPos = iWidth1 + iWidth2 + xPos;

      if (Percentage3 >= Percentage2)
      {
        //render 3th color
        fPercent = Percentage3 - Percentage2;
      }
      else
      {
        fPercent = 0;
      }
      fWidth = (float)iWidth;
      fWidth /= 100.0f;
      fWidth *= (float)fPercent;
      iWidth3 = (int)Math.Floor(fWidth);
      if (iWidth3 > 0)
      {
        _imageFill3.Width = iWidth3;
        _imageFill3.Height = _fillBackgroundHeight;
        _imageFill3.SetPosition(iCurPos, yPos);
        _imageFill3.Render(timePassed);
      }

      // render ticks
      _imageTick.Height = _imageTick.TextureHeight;
      _imageTick.Width = _imageTick.TextureWidth;
      int posx1 = 10;
      int posx2 = 20;
      int posy1 = 3;
      GUIGraphicsContext.ScaleHorizontal(ref posx1);
      GUIGraphicsContext.ScaleHorizontal(ref posx2);
      GUIGraphicsContext.ScaleVertical(ref posy1);
      for (int i = 0; i <= 100; i += 10)
      {
        float fpos = (float)_positionX + _imageLeft.TextureWidth + posx1;
        fWidth = (float)(iWidth - posx2);
        fWidth /= 100.0f;
        fWidth *= (float)i;
        _imageTick.SetPosition((int)(fpos + fWidth), (int)_positionY + posy1);
        _imageTick.Render(timePassed);
      }

      // render top
      _imageTop.Height = GUIGraphicsContext.ScaleVertical(_imageTop.TextureHeight);
      _imageTop.Width = GUIGraphicsContext.ScaleHorizontal(_imageTop.TextureWidth);

      xPos = iCurPos - (_imageTop.Width / 2);
      _imageTop.SetPosition(xPos,
                            _positionY - _imageTop.Height + _topTextureOffsetY - GUIGraphicsContext.ScaleVertical(1));
      _imageTop.Render(timePassed);

      //render tick @ current position
      _imageTick.Height = _imageFillBackground.TextureHeight;
      _imageTick.Width = _imageTick.TextureWidth * 2;
      _imageTick.SetPosition((int)(_imageTop.XPosition + (_imageTop.TextureWidth / 2) - (_imageTick.Width / 2)),
                             (int)_imageFillBackground.YPosition);
      _imageTick.Render(timePassed);

      // render bottom
      xPos = _imageTop.XPosition + (_imageTop.TextureWidth / 2) - (_imageBottom.TextureWidth / 2);
      _imageBottom.SetPosition(xPos, _positionY + _imageMid.TextureHeight);
      _imageBottom.Render(timePassed);


      //render logo
      float fx = (float)_imageBottom.XPosition;
      fx += (((float)_imageBottom.TextureWidth) / 2f);
      fx -= (((float)_imageLogo.TextureWidth) / 2f);

      float fy = (float)_imageBottom.YPosition;
      fy += (((float)_imageBottom.TextureHeight) / 2f);
      fy -= (((float)_imageLogo.TextureHeight) / 2f);
      _imageLogo.SetPosition((int)fx, (int)fy);
      _imageLogo.Render(timePassed);

      if (_font != null)
      {
        float fW = 0, fH = 0;
        float fHeight = 0;
        string strText = "";

        // render top text
        if (_labelTop.Length > 0)
        {
          strText = GUIPropertyManager.Parse(_labelTop);
          _font.GetTextExtent(strText, ref fW, ref fH);
          fW /= 2.0f;
          fH /= 2.0f;
          fWidth = ((float)_imageTop.TextureWidth) / 2.0f;
          fHeight = ((float)_imageTop.TextureHeight) / 2.0f;
          fWidth -= fW;
          fHeight -= fH;
          _font.DrawText((float)_imageTop.XPosition + fWidth, (float)2 + _imageTop.YPosition + fHeight, _textColor,
                         strText, Alignment.ALIGN_LEFT, -1);
        }


        // render left text
        if (_labelLeft.Length > 0)
        {
          strText = GUIPropertyManager.Parse(_labelLeft);
          _font.GetTextExtent(strText, ref fW, ref fH);
          fW /= 2.0f;
          fH /= 2.0f;
          fWidth = ((float)_imageLeft.TextureWidth) / 2.0f;
          fHeight = ((float)_imageLeft.TextureHeight) / 2.0f;
          fWidth -= fW;
          fHeight -= fH;
          _font.DrawText((float)_positionX + fWidth, (float)_positionY + fHeight, _textColor, strText,
                         Alignment.ALIGN_LEFT, -1);
        }

        // render right text
        if (_labelRight.Length > 0)
        {
          strText = GUIPropertyManager.Parse(_labelRight);
          _font.GetTextExtent(strText, ref fW, ref fH);
          fW /= 2.0f;
          fH /= 2.0f;
          fWidth = ((float)_imageRight.TextureWidth) / 2.0f;
          fHeight = ((float)_imageRight.TextureHeight) / 2.0f;
          fWidth -= fW;
          fHeight -= fH;
          _font.DrawText((float)_imageRight.XPosition + fWidth, (float)_imageRight.YPosition + fHeight, _textColor,
                         strText, Alignment.ALIGN_LEFT, -1);
        }
      }
      base.Render(timePassed);
    }

    public override bool CanFocus()
    {
      return false;
    }

    public float Percentage1
    {
      get { return _percentage1; }
      set
      {
        _percentage1 = value;
        if (_percentage1 < 0)
        {
          _percentage1 = 0;
        }
        if (_percentage1 > 100)
        {
          _percentage1 = 100;
        }
      }
    }

    public float Percentage2
    {
      get { return _percentage2; }
      set
      {
        _percentage2 = value;
        if (_percentage2 < 0)
        {
          _percentage2 = 0;
        }
        if (_percentage2 > 100)
        {
          _percentage2 = 100;
        }
      }
    }

    public float Percentage3
    {
      get { return _percentage3; }
      set
      {
        _percentage3 = value;
        if (_percentage3 < 0)
        {
          _percentage3 = 0;
        }
        if (_percentage3 > 100)
        {
          _percentage3 = 100;
        }
      }
    }

    public override void Dispose()
    {
      base.Dispose();
      _imageTop.SafeDispose();
      _imageMid.SafeDispose();
      _imageRight.SafeDispose();
      _imageLeft.SafeDispose();
      _imageFill1.SafeDispose();
      _imageFill2.SafeDispose();
      _imageFill3.SafeDispose();
      _imageFillBackground.SafeDispose();
      _imageTick.SafeDispose();
      _imageBottom.SafeDispose();
      _imageLogo.SafeDispose();
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageTop.PreAllocResources();
      _imageBottom.PreAllocResources();
      _imageMid.PreAllocResources();
      _imageRight.PreAllocResources();
      _imageLeft.PreAllocResources();
      _imageFillBackground.PreAllocResources();
      _imageFill1.PreAllocResources();
      _imageFill2.PreAllocResources();
      _imageFill3.PreAllocResources();
      _imageTick.PreAllocResources();
      _imageLogo.PreAllocResources();
    }

    public override void AllocResources()
    {
      base.AllocResources();
      _font = GUIFontManager.GetFont(_fontName);
      _imageTop.AllocResources();
      _imageBottom.AllocResources();
      _imageMid.AllocResources();
      _imageRight.AllocResources();
      _imageLeft.AllocResources();
      _imageFillBackground.AllocResources();
      _imageFill1.AllocResources();
      _imageFill2.AllocResources();
      _imageFill3.AllocResources();
      _imageTick.AllocResources();
      _imageLogo.AllocResources();

      _imageTop.Filtering = false;
      _imageBottom.Filtering = false;
      _imageMid.Filtering = false;
      _imageRight.Filtering = false;
      _imageLeft.Filtering = false;
      _imageFill1.Filtering = false;
      _imageFill2.Filtering = false;
      _imageFill3.Filtering = false;
      _imageTick.Filtering = false;
      if (_height == 0)
      {
        _height = _imageRight.TextureHeight;
      }
      //      _imageTop.Height=_height;
      _imageRight.Height = _height;
      _imageLeft.Height = _height;
      _imageMid.Height = _height;
      _imageFill1.Height = _height - 6;
      _imageFill2.Height = _height - 6;
      _imageFill3.Height = _height - 6;
      //_imageTick.Height=_height;
    }

    public string FillBackGroundName
    {
      get { return _imageFillBackground.FileName; }
    }

    public string Fill1TextureName
    {
      get { return _imageFill1.FileName; }
    }

    public string Fill2TextureName
    {
      get { return _imageFill2.FileName; }
    }

    public string Fill3TextureName
    {
      get { return _imageFill3.FileName; }
    }

    public string TickTextureName
    {
      get { return _imageTick.FileName; }
    }

    public string TopTextureName
    {
      get { return _imageTop.FileName; }
    }

    public string BottomTextureName
    {
      get { return _imageBottom.FileName; }
    }

    public string BackTextureLeftName
    {
      get { return _imageLeft.FileName; }
    }

    public string BackTextureMidName
    {
      get { return _imageMid.FileName; }
    }

    public string BackTextureRightName
    {
      get { return _imageRight.FileName; }
    }

    public string LogoTextureName
    {
      get { return _imageLogo.FileName; }
    }

    /// <summary>
    /// Get/set the text of the label.
    /// </summary>
    public string Property
    {
      get { return _propertyLabel; }
      set
      {
        if (value == null)
        {
          return;
        }
        _propertyLabel = value;
      }
    }

    public string LabelLeft
    {
      get { return _labelLeft; }
      set
      {
        if (value == null)
        {
          return;
        }
        _labelLeft = value;
      }
    }

    public string LabelTop
    {
      get { return _labelTop; }
      set
      {
        if (value == null)
        {
          return;
        }
        _labelTop = value;
      }
    }

    public string LabelRight
    {
      get { return _labelRight; }
      set
      {
        if (value == null)
        {
          return;
        }
        _labelRight = value;
      }
    }

    /// <summary>
    /// Get/set the color of the text
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName = value;
        _font = GUIFontManager.GetFont(_fontName);
      }
    }

    public int FillX
    {
      get { return _fillBackgroundOffsetX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _fillBackgroundOffsetX = value;
      }
    }

    public int FillY
    {
      get { return _fillBackgroundOffsetY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _fillBackgroundOffsetY = value;
      }
    }

    public int FillHeight
    {
      get { return _fillBackgroundHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _fillBackgroundHeight = value;
      }
    }

    public string Label1
    {
      get { return _label1; }
      set
      {
        if (value == null)
        {
          return;
        }
        _label1 = value;
      }
    }

    public string Label2
    {
      get { return _label2; }
      set
      {
        if (value == null)
        {
          return;
        }
        _label2 = value;
      }
    }

    public string Label3
    {
      get { return _label3; }
      set
      {
        if (value == null)
        {
          return;
        }
        _label3 = value;
      }
    }

    public int TopTextureYOffset
    {
      get { return _topTextureOffsetY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _topTextureOffsetY = value;
      }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageTop != null)
        {
          _imageTop.DimColor = value;
        }
        if (_imageLogo != null)
        {
          _imageLogo.DimColor = value;
        }
        if (_imageBottom != null)
        {
          _imageBottom.DimColor = value;
        }
        if (_imageTick != null)
        {
          _imageTick.DimColor = value;
        }
        if (_imageFillBackground != null)
        {
          _imageFillBackground.DimColor = value;
        }
        if (_imageFill1 != null)
        {
          _imageFill1.DimColor = value;
        }
        if (_imageFill2 != null)
        {
          _imageFill2.DimColor = value;
        }
        if (_imageFill3 != null)
        {
          _imageFill3.DimColor = value;
        }
        if (_imageLeft != null)
        {
          _imageLeft.DimColor = value;
        }
        if (_imageMid != null)
        {
          _imageMid.DimColor = value;
        }
        if (_imageRight != null)
        {
          _imageRight.DimColor = value;
        }
      }
    }
  }
}