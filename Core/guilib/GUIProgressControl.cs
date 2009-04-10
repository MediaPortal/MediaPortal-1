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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The implementation of a progress bar used by the OSD.
  /// The progress bar uses the following images 
  /// -) a background image 
  /// -) a left texture which presents the left part of the progress bar
  /// -) a mid texture which presents the middle part of the progress bar
  /// -) a right texture which presents the right part of the progress bar
  /// -) a label which is drawn inside the progressbar control
  /// </summary>
  public class GUIProgressControl : GUIControl
  {
    [XMLSkinElement("label")] private string _property = "";
    [XMLSkinElement("texturebg")] private string _backgroundTextureName;
    [XMLSkinElement("lefttexture")] private string _leftTextureName;
    [XMLSkinElement("midtexture")] private string _midTextureName;
    [XMLSkinElement("righttexture")] private string _rightTextureName;
    private GUIAnimation _imageBackGround = null;
    private GUIAnimation _imageLeft = null;
    private GUIAnimation _imageMid = null;
    private GUIAnimation _imageRight = null;
    private int _percentage = 0;
    private bool _containsProperty = false;


    public GUIProgressControl(int dwParentID)
      : base(dwParentID)
    {
    }

    /// <summary>
    /// Creates a GUIProgressControl.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strBackGroundTexture">The background texture.</param>
    /// <param name="strLeftTexture">The left side texture.</param>
    /// <param name="strMidTexture">The middle texture.</param>
    /// <param name="strRightTexture">The right side texture.</param>
    public GUIProgressControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                              string strBackGroundTexture, string strLeftTexture, string strMidTexture,
                              string strRightTexture)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _backgroundTextureName = strBackGroundTexture;
      _leftTextureName = strLeftTexture;
      _midTextureName = strMidTexture;
      _rightTextureName = strRightTexture;
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
      _imageBackGround = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _backgroundTextureName);
      _imageBackGround.ParentControl = this;
      _imageBackGround.DimColor = DimColor;

      _imageLeft = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _leftTextureName);
      _imageLeft.ParentControl = this;
      _imageLeft.DimColor = DimColor;

      _imageMid = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _midTextureName);
      _imageMid.ParentControl = this;
      _imageMid.DimColor = DimColor;

      _imageRight = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, 0, 0, _rightTextureName);
      _imageRight.ParentControl = this;
      _imageRight.DimColor = DimColor;

      _imageBackGround.KeepAspectRatio = false;
      _imageMid.KeepAspectRatio = false;
      _imageRight.KeepAspectRatio = false;
      if (_property == null)
      {
        _property = string.Empty;
      }
      if (_property.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }
    }

    /// <summary>
    /// Update the subcontrols with the current position of the progress control
    /// </summary>
    protected override void Update()
    {
      base.Update();
      _imageBackGround.SetPosition(_positionX, _positionY);
      _imageLeft.SetPosition(_positionX, _positionY);
      _imageMid.SetPosition(_positionX, _positionY);
      _imageRight.SetPosition(_positionX, _positionY);
    }

    public override bool Dimmed
    {
      get { return base.Dimmed; }
      set
      {
        base.Dimmed = value;
        if (_imageBackGround != null)
        {
          _imageBackGround.Dimmed = value;
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

    /// <summary>
    /// Renders the progress control.
    /// </summary>
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

      if (_containsProperty)
      {
        string m_strText = GUIPropertyManager.Parse(_property);
        if (m_strText != string.Empty)
        {
          try
          {
            Percentage = Int32.Parse(m_strText);
          }
          catch (Exception)
          {
          }
        }
      }

      // Render the background
      int iBkgHeight = _height;
      _imageBackGround.Height = iBkgHeight;
      _imageBackGround.SetPosition(_imageBackGround.XPosition, _imageBackGround.YPosition);
      _imageBackGround.Render(timePassed);

      GUIFontManager.Present();

      int iWidthLeft = _imageLeft.TextureWidth;
      int iHeightLeft = _imageLeft.TextureHeight;
      int iWidthRight = _imageRight.TextureWidth;
      int iHeightRight = _imageRight.TextureHeight;
      GUIGraphicsContext.ScaleHorizontal(ref iWidthLeft);
      GUIGraphicsContext.ScaleHorizontal(ref iWidthRight);
      GUIGraphicsContext.ScaleVertical(ref iHeightLeft);
      GUIGraphicsContext.ScaleVertical(ref iHeightRight);
      //iHeight=20;
      int off = 12;
      GUIGraphicsContext.ScaleHorizontal(ref off);
      int percent = _percentage;
      if (percent > 100)
      {
        percent = 100;
      }
      float fWidth = (float) percent;
      fWidth /= 100.0f;
      fWidth *= (float) (_imageBackGround.Width - 2*off - iWidthLeft - iWidthRight);

      int iXPos = off + _imageBackGround.XPosition;

      int iYPos = _imageBackGround.YPosition + (iBkgHeight - iHeightLeft)/2;
      //_imageLeft.SetHeight(iHeight);
      _imageLeft.SetPosition(iXPos, iYPos);
      _imageLeft.Height = iHeightLeft;
      _imageLeft.Width = iWidthLeft;
      _imageLeft.SetPosition(iXPos, iYPos);
      _imageLeft.Render(timePassed);

      iXPos += iWidthLeft;
      if (percent > 0 && (int) fWidth > 1)
      {
        _imageMid.SetPosition(iXPos, iYPos);
        _imageMid.Height = iHeightLeft; //_imageMid.TextureHeight;
        _imageMid.Width = (int) Math.Abs(fWidth);
        _imageMid.SetPosition(iXPos, iYPos);
        _imageMid.Render(timePassed);
        iXPos += (int) fWidth;
      }
      //_imageRight.SetHeight(iHeight);
      _imageRight.SetPosition(iXPos, iYPos);
      _imageRight.Height = iHeightRight;
      _imageRight.Width = iWidthRight;
      _imageRight.SetPosition(iXPos, iYPos);
      _imageRight.Render(timePassed);
      base.Render(timePassed);
    }

    /// <summary>
    /// Returns if the control can have the focus.
    /// </summary>
    /// <returns>False</returns>
    public override bool CanFocus()
    {
      return false;
    }

    /// <summary>
    /// Get/set the percentage the progressbar indicates.
    /// </summary>
    public int Percentage
    {
      get { return _percentage; }
      set { _percentage = value; }
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageBackGround.FreeResources();
      _imageMid.FreeResources();
      _imageRight.FreeResources();
      _imageLeft.FreeResources();
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageBackGround.PreAllocResources();
      _imageMid.PreAllocResources();
      _imageRight.PreAllocResources();
      _imageLeft.PreAllocResources();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _imageBackGround.AllocResources();
      _imageMid.AllocResources();
      _imageRight.AllocResources();
      _imageLeft.AllocResources();

      _imageBackGround.Filtering = false;
      _imageMid.Filtering = false;
      _imageRight.Filtering = false;
      _imageLeft.Filtering = false;

      _imageBackGround.Height = 25;
      _imageRight.Height = 20;
      _imageLeft.Height = 20;
      _imageMid.Height = 20;
    }

    /// <summary>
    /// Gets the filename of the background texture.
    /// </summary>
    public string BackGroundTextureName
    {
      get { return _imageBackGround.FileName; }
    }

    /// <summary>
    /// Gets the filename of the left texture.
    /// </summary>
    public string BackTextureLeftName
    {
      get { return _imageLeft.FileName; }
    }

    /// <summary>
    /// Gets the filename of the middle texture.
    /// </summary>
    public string BackTextureMidName
    {
      get { return _imageMid.FileName; }
    }

    /// <summary>
    /// Gets the filename of the right texture.
    /// </summary>
    public string BackTextureRightName
    {
      get { return _imageRight.FileName; }
    }

    /// <summary>
    /// Get/set the property.
    /// The property contains text which is shown in the progress control
    /// normally this is a percentage (0%-100%)
    /// </summary>
    public string Property
    {
      get { return _property; }
      set
      {
        if (value != null)
        {
          _property = value;
          if (_property.IndexOf("#") >= 0)
          {
            _containsProperty = true;
          }
          else
          {
            _containsProperty = false;
          }
        }
      }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageBackGround != null)
        {
          _imageBackGround.DimColor = value;
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