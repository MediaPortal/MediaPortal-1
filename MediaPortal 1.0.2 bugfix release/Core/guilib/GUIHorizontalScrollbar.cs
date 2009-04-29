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

using System.Windows.Forms;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implementation of a GUIHorizontalScrollbar.
  /// </summary>
  public class GUIHorizontalScrollbar : GUIControl
  {
    [XMLSkinElement("buddycontrol")] private int _buddyControlId = -1;
    [XMLSkinElement("texturebg")] private string _backgroundTextureName;
    [XMLSkinElement("lefttexture")] private string _leftTextureName;
    [XMLSkinElement("righttexture")] private string _rightTextureName;
    private GUIAnimation _imageBackGround = null;
    private GUIAnimation _imageLeft = null;
    private GUIAnimation _imageRight = null;
    private float _percentage = 0;
    private int _startX = 0;
    private int _endX = 0;
    private int _widthKnob = 0;
    private int _startPositionXKnob = 0;

    public GUIHorizontalScrollbar(int dwParentID)
      : base(dwParentID)
    {
    }

    /// <summary>
    /// The constructor of the GUIHorizontalScrollbar class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strBackGroundTexture">The background texture of the scrollbar.</param>
    /// <param name="strLeftTexture">The left texture of the scrollbar indicator.</param>
    /// <param name="strRightTexture">The right texture of the scrolbar indicator.</param>
    public GUIHorizontalScrollbar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                  string strBackGroundTexture, string strLeftTexture, string strRightTexture)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _backgroundTextureName = strBackGroundTexture;
      _rightTextureName = strRightTexture;
      _leftTextureName = strLeftTexture;
      FinalizeConstruction();
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_backgroundTextureName == null)
      {
        _backgroundTextureName = "";
      }
      if (_rightTextureName == null)
      {
        _rightTextureName = "";
      }
      if (_leftTextureName == null)
      {
        _leftTextureName = "";
      }
      _imageBackGround = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _backgroundTextureName);
      _imageBackGround.ParentControl = this;
      _imageBackGround.DimColor = DimColor;

      _imageLeft = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                        _leftTextureName);
      _imageLeft.ParentControl = this;
      _imageLeft.DimColor = DimColor;
      _imageRight = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                         _rightTextureName);
      _imageRight.ParentControl = this;
      _imageRight.DimColor = DimColor;
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
        if (_imageRight != null)
        {
          _imageRight.Dimmed = value;
        }
      }
    }

    /// Renders the control.
    /// </summary>
    public override void Render(float timePassed)
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
      if (!GUIGraphicsContext.MouseSupport)
      {
        IsVisible = false;
        base.Render(timePassed);
        return;
      }

      int iHeight = _imageBackGround.Height;
      _imageBackGround.Render(timePassed);
      _imageBackGround.Height = iHeight;

      float fPercent = (float) _percentage;
      float fPosXOff = (fPercent/100.0f);

      _widthKnob = (int) (2*_imageLeft.TextureWidth);
      int inset = 4;
      GUIGraphicsContext.ScaleHorizontal(ref inset);
      inset += (_widthKnob/2);

      inset = 0;
      _startX = inset + _imageBackGround.XPosition;
      _endX = _startX + (_imageBackGround.Width - inset);

      fPosXOff *= (float) (_endX - _startX - _widthKnob);

      _startPositionXKnob = _startX + (int) fPosXOff;
      int iXPos = _startPositionXKnob;
      int iYPos = _imageBackGround.YPosition + ((_imageBackGround.Height/2) - (_imageLeft.TextureHeight/2));

      _imageLeft.SetPosition(iXPos, iYPos);
      _imageLeft.Height = _imageLeft.TextureHeight;
      _imageLeft.Width = _imageLeft.TextureWidth;
      _imageLeft.Render(timePassed);

      iXPos += _imageLeft.TextureWidth;
      _imageRight.SetPosition(iXPos, iYPos);
      _imageRight.Height = _imageRight.TextureHeight;
      _imageRight.Width = _imageLeft.TextureWidth;
      _imageRight.Render(timePassed);

      base.Render(timePassed);
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
    /// Get/set the percentage the scrollbar indicates.
    /// </summary>
    public float Percentage
    {
      get { return _percentage; }
      set
      {
        _percentage = value;
        if (_percentage < 0)
        {
          _percentage = 0;
        }
        if (_percentage > 100)
        {
          _percentage = 100;
        }
      }
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageBackGround.FreeResources();
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
      _imageRight.AllocResources();
      _imageLeft.AllocResources();
    }

    /// <summary>
    /// Gets the name of the backgroundtexture.
    /// </summary>
    public string BackGroundTextureName
    {
      get { return _imageBackGround.FileName; }
    }

    /// <summary>
    /// Gets the name of the left texture of the scrollbar indicator.
    /// </summary>
    public string BackTextureLeftName
    {
      get { return _imageLeft.FileName; }
    }

    /// <summary>
    /// Gets the name of the right texture of the scrollbar indicator.
    /// </summary>
    public string BackTextureRightName
    {
      get { return _imageRight.FileName; }
    }

    /// <summary>
    /// Get/set the buddycontrol that is being controlled by the scrollbar.
    /// </summary>
    public int BuddyControl
    {
      get { return _buddyControlId; }
      set { _buddyControlId = value; }
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      if (!GUIGraphicsContext.MouseSupport)
      {
        IsVisible = false;
        return;
      }
      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
      {
        int id;
        bool focus;
        if (HitTest((int) action.fAmount1, (int) action.fAmount2, out id, out focus))
        {
          if (action.MouseButton == MouseButtons.Left)
          {
            float fWidth = (float) (_endX - _startX);
            _percentage = (action.fAmount1 - (float) _startX);
            _percentage /= fWidth;
            _percentage *= 100.0f;

            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED, WindowId, GetID,
                                                GetID, (int) _percentage, 0, null);
            GUIGraphicsContext.SendMessage(message);
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
      {
        if (action.MouseButton == MouseButtons.Left)
        {
          float fWidth = (float) (_endX - _startX);
          _percentage = (action.fAmount1 - (float) _startX);
          _percentage /= fWidth;
          _percentage *= 100.0f;
          GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED, WindowId, GetID, GetID,
                                              (int) _percentage, 0, null);
          GUIGraphicsContext.SendMessage(message);
        }
      }
      base.OnAction(action);
    }

    /// <summary>
    /// Checks if the x and y coordinates correspond to the current control.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>True if the control was hit.</returns>
    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = GetID;
      focused = Focus;
      if (!IsVisible)
      {
        return false;
      }
      if (x >= _startX && x < _endX)
      {
        if (y >= YPosition && y < YPosition + Height)
        {
          return true;
        }
      }
      return false;
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
        if (_imageRight != null)
        {
          _imageRight.DimColor = value;
        }
      }
    }
  }
}