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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for GUIUpDownButton.
  /// </summary>
  public class GUIUpDownButton : GUIButtonControl
  {
    [XMLSkinElement("spinColor")] protected long _colorSpinColor;
    [XMLSkinElement("spinHeight")] protected int _spinControlHeight;
    [XMLSkinElement("spinWidth")] protected int _spinControlWidth;
    [XMLSkinElement("spinPosX")] protected int _spinControlPositionX;
    [XMLSkinElement("spinPosY")] protected int _spinControlPositionY;
    [XMLSkinElement("textureUp")] protected string _upTextureName = "";
    [XMLSkinElement("textureDown")] protected string _downTextureName = "";
    [XMLSkinElement("textureUpFocus")] protected string _upTextureNameFocus = "";
    [XMLSkinElement("textureDownFocus")] protected string _downTextureNameFocus = "";

    private GUISpinControl _spinControl;

    public GUIUpDownButton(int dwParentID) : base(dwParentID)
    {
    }

    public GUIUpDownButton(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                           string strTextureFocus, string strTextureNoFocus,
                           int dwSpinWidth, int dwSpinHeight,
                           string strUp, string strDown,
                           string strUpFocus, string strDownFocus,
                           long dwSpinColor, int dwSpinX, int dwSpinY,
                           int dwShadowAngle, int dwShadowDistance, long dwShadowColor)
      : base(dwParentID)
    {
      _spinControlWidth = dwSpinWidth;
      _spinControlHeight = dwSpinHeight;
      _upTextureName = strUp;
      _downTextureName = strDown;
      _upTextureNameFocus = strUpFocus;
      _downTextureNameFocus = strDownFocus;
      _colorSpinColor = dwSpinColor;
      _spinControlPositionX = dwSpinX;
      _spinControlPositionY = dwSpinY;

      _parentControlId = dwParentID;
      _controlId = dwControlId;
      _positionX = dwPosX;
      _positionY = dwPosY;
      _width = dwWidth;
      _height = dwHeight;
      _shadowAngle = dwShadowAngle;
      _shadowDistance = dwShadowDistance;
      _shadowColor = dwShadowColor;

      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      FinalizeConstruction();
      DimColor = base.DimColor;
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _spinControl = new GUISpinControl(_controlId, 0, _spinControlPositionX, _spinControlPositionY, _spinControlWidth,
                                        _spinControlHeight, _upTextureName, _downTextureName, _upTextureNameFocus,
                                        _downTextureNameFocus, _fontName, _colorSpinColor,
                                        GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT, Alignment.ALIGN_RIGHT);
      _spinControl.WindowId = WindowId;
      _spinControl.AutoCheck = false;
      _spinControl.ParentControl = this;
    }

    public override void AllocResources()
    {
      base.AllocResources();
      _spinControl.AllocResources();
    }

    public override void FreeResources()
    {
      base.FreeResources();
      _spinControl.FreeResources();
    }


    /// <summary>
    /// Renders the GUIButtonControl.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }
      base.Render(timePassed);

      // The GUIButtonControl has the focus
      if (Focus)
      {
        //render the focused image
        _imageFocused.Render(timePassed);
        GUIPropertyManager.SetProperty("#highlightedbutton", Label);
      }
      else
      {
        //render the non-focused image
        _imageNonFocused.Render(timePassed);
      }

      // render the text on the button
      if (Disabled)
      {
        if (_labelControl is GUILabelControl)
        {
          ((GUILabelControl)_labelControl).Label = _label;
          ((GUILabelControl)_labelControl).TextColor = _disabledColor;
        }
        else
        {
          ((GUIFadeLabel)_labelControl).Label = _label;
          ((GUIFadeLabel)_labelControl).TextColor = _disabledColor;
        }
        _labelControl.SetPosition(_textOffsetX + _positionX, _textOffsetY + _positionY);
        _labelControl.Render(timePassed);
      }
      else
      {
        if (_labelControl is GUILabelControl)
        {
          ((GUILabelControl)_labelControl).Label = _label;
          ((GUILabelControl)_labelControl).TextColor = _disabledColor;
        }
        else
        {
          ((GUIFadeLabel)_labelControl).Label = _label;
          ((GUIFadeLabel)_labelControl).TextColor = _textColor;
        }
        _labelControl.SetPosition(_textOffsetX + _positionX, _textOffsetY + _positionY);
        _labelControl.Render(timePassed);
      }
      base.Render(timePassed);
      if (_spinControl != null)
      {
        int off = 5;
        GUIGraphicsContext.ScaleHorizontal(ref off);
        _spinControl.SetPosition(_imageNonFocused.XPosition + _imageNonFocused.Width - off - 2*_spinControlWidth,
                                 _imageNonFocused.YPosition + (_imageNonFocused.Height - _spinControlHeight)/2);
        _spinControl.Render(timePassed);
      }
      //base.Render(timePassed);
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      if (_spinControl.HitTest(x, y, out controlID, out focused))
      {
        _spinControl.Focus = true;
        return true;
      }
      else
      {
        _spinControl.Focus = false;
      }
      return base.HitTest(x, y, out controlID, out focused);
    }

    public GUISpinControl UpDownControl
    {
      get { return _spinControl; }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_spinControl != null)
        {
          _spinControl.DimColor = value;
        }
      }
    }
  }
}