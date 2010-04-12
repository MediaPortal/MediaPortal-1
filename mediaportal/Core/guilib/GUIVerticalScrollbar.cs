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

using System.Windows.Forms;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Implementation of a GUIVerticalScrollbar.
  /// </summary>
  public class GUIVerticalScrollbar : GUIControl
  {
    [XMLSkinElement("buddycontrol")] protected int _buddyControlId = -1;
    [XMLSkinElement("scrollbarbg")] protected string _scrollbarBackgroundName;
    [XMLSkinElement("scrollbartop")] protected string _scrollbarTopTextureName;
    [XMLSkinElement("scrollbarbottom")] protected string _scrollbarBottomTextureName;
    private GUIImage _imageBackground = null;
    private GUIImage _imageTop = null;
    private GUIImage _imageBottom = null;
    private float _percentage = 0;
    private int _startPositionY = 0;
    private int _endPositionY = 0;
    private int _knobPositionY = 0;
    private bool _sendNotifies = true;

    public GUIVerticalScrollbar(int dwParentID) : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUIverticalScrollbar class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strBackGroundTexture">The background texture of the scrollbar.</param>
    /// <param name="strTopTexture">The top texture of the scrollbar indicator.</param>
    /// <param name="strBottomTexture">The bottom texture of the scrolbar indicator.</param>
    public GUIVerticalScrollbar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                                string strBackGroundTexture, string strTopTexture, string strBottomTexture)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _scrollbarBackgroundName = strBackGroundTexture;
      _scrollbarTopTextureName = strTopTexture;
      _scrollbarBottomTextureName = strBottomTexture;
      FinalizeConstruction();
      DimColor = base.DimColor;
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageBackground = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                      _scrollbarBackgroundName, 0);
      _imageTop = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                               _scrollbarTopTextureName, 0);
      _imageBottom = new GUIImage(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                  _scrollbarBottomTextureName, 0);

      _imageBackground.ParentControl = this;
      _imageTop.ParentControl = this;
      _imageBottom.ParentControl = this;

      _imageBackground.DimColor = DimColor;
      _imageTop.DimColor = DimColor;
      _imageBottom.DimColor = DimColor;
    }


    /// <summary>
    /// Renders the control.
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
      if (!GUIGraphicsContext.MouseSupport)
      {
        IsVisible = false;
        base.Render(timePassed);
        return;
      }

      int iHeight = _height;
      _imageBackground.Height = iHeight;
      _imageBackground.Render(timePassed);

      float fPercent = (float)_percentage;
      float fPosYOff = (fPercent / 100.0f);

      _startPositionY = _imageBackground.YPosition;
      _endPositionY = _startPositionY + _imageBackground.Height;

      int iKnobHeight = (int)(_imageTop.TextureHeight);
      fPosYOff *= (float)(_endPositionY - _startPositionY - iKnobHeight);

      _knobPositionY = _startPositionY + (int)fPosYOff;
      int iXPos = _imageBackground.XPosition + ((_imageBackground.Width / 2) - (_imageTop.TextureWidth));
      int iYPos = _knobPositionY;

      _imageTop.SetPosition(iXPos, iYPos);
      _imageTop.Height = _imageTop.TextureHeight;
      _imageTop.Width = _imageTop.TextureWidth;
      _imageTop.Render(timePassed);

      iXPos += _imageTop.TextureWidth;
      _imageBottom.SetPosition(iXPos, iYPos);
      _imageBottom.Height = _imageBottom.TextureHeight;
      _imageBottom.Width = _imageTop.TextureWidth;
      _imageBottom.Render(timePassed);

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
    public override void Dispose()
    {
      base.Dispose();
      _imageBackground.SafeDispose();
      _imageBottom.SafeDispose();
      _imageTop.SafeDispose();
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageBackground.PreAllocResources();
      _imageBottom.PreAllocResources();
      _imageTop.PreAllocResources();
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _imageBackground.AllocResources();
      _imageBottom.AllocResources();
      _imageTop.AllocResources();

      _imageBackground.DimColor = DimColor;
      _imageTop.DimColor = DimColor;
      _imageBottom.DimColor = DimColor;
    }

    /// <summary>
    /// Gets the name of the backgroundtexture.
    /// </summary>
    public string BackGroundTextureName
    {
      get { return _imageBackground.FileName; }
    }

    /// <summary>
    /// Gets the name of the top texture of the scrollbar indicator.
    /// </summary>
    public string BackTextureTopName
    {
      get { return _imageTop.FileName; }
    }

    /// <summary>
    /// Gets the name of the bottom texture of the scrollbar indicator.
    /// </summary>
    public string BackTextureBottomName
    {
      get { return _imageBottom.FileName; }
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
        if (HitTest((int)action.fAmount1, (int)action.fAmount2, out id, out focus))
        {
          if (action.MouseButton == MouseButtons.Left)
          {
            float fHeight = (float)(_endPositionY - _startPositionY);
            _percentage = (action.fAmount2 - (float)_startPositionY);
            _percentage /= fHeight;
            _percentage *= 100.0f;

            if (_sendNotifies)
            {
              GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED, WindowId, GetID,
                                                  GetID, (int)_percentage, 0, null);
              GUIGraphicsContext.SendMessage(message);
            }
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
      {
        if (action.MouseButton == MouseButtons.Left)
        {
          float fHeight = (float)(_endPositionY - _startPositionY);
          _percentage = (action.fAmount2 - (float)_startPositionY);
          _percentage /= fHeight;
          _percentage *= 100.0f;
          if (_sendNotifies)
          {
            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED, WindowId, GetID,
                                                GetID, (int)_percentage, 0, null);
            GUIGraphicsContext.SendMessage(message);
          }
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
      if (x >= XPosition && x < XPosition + Width)
      {
        if (y >= _startPositionY && y < _endPositionY)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Get/set the sendnotifies flag. Make sure that the control notifies when its percentage has changed. (See OnAction method).
    /// </summary>
    public bool SendNotifies
    {
      get { return _sendNotifies; }
      set { _sendNotifies = false; }
    }

    public override void DoUpdate()
    {
      _imageBackground.Height = _height;
      _imageBackground.DoUpdate();
      base.DoUpdate();
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageBackground != null)
        {
          _imageBackground.DimColor = value;
        }
        if (_imageTop != null)
        {
          _imageTop.DimColor = value;
        }
        if (_imageBottom != null)
        {
          _imageBottom.DimColor = value;
        }
      }
    }
  }
}