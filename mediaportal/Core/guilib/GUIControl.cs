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
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using MediaPortal.Drawing;
using Point=System.Drawing.Point;
using Size=MediaPortal.Drawing.Size;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Base class for GUIControls.
  /// </summary>
  public abstract class GUIControl : Control
  {
    [XMLSkinElement("subtype")] protected string _subType = "";
    [XMLSkinElement("onleft")] protected int _leftControlId = 0;
    [XMLSkinElement("onright")] protected int _rightControlId = 0;
    [XMLSkinElement("onup")] protected int _upControlId = 0;
    [XMLSkinElement("ondown")] protected int _downControlId = 0;
    [XMLSkinElement("colordiffuse")] protected long _diffuseColor = 0xFFFFFFFF;
    [XMLSkinElement("id")] protected int _controlId = 0;
    [XMLSkinElement("type")] protected string _controlType = "";
    [XMLSkinElement("description")] protected string _description = "";
    [XMLSkinElement("dimColor")] protected int _dimColor = 0x60ffffff;

    protected int _parentControlId = 0;
    protected bool _isSelected = false;
    protected bool _calibration = true;
    protected object _data;
    protected int _windowId;
    protected int _selectedItem = 0;
    protected ArrayList _subItemList = new ArrayList();
    protected Rectangle _originalRectangle;
    protected bool _isAnimating = false;
    protected long _originalDiffuseColor;
    protected GUIControl _parentControl = null;
    protected bool _isDimmed = false;
    private bool _hasRendered = false;
    private bool _visibleFromSkinCondition = true;
    private int _visibleCondition = 0;
    private bool _allowHiddenFocus = false;
    private bool _hasCamera = false;
    private Point _camera;

    private List<VisualEffect> _animations = new List<VisualEffect>();
    private List<VisualEffect> _thumbAnimations = new List<VisualEffect>();
    private List<int> _infoList = new List<int>();
    //protected int DimColor = 0x60ffffff;

    /// <summary>
    /// enum to specify the alignment of the control
    /// </summary>
    public enum Alignment
    {
      ALIGN_LEFT,
      ALIGN_RIGHT,
      ALIGN_CENTER,

      // added to support XAML parser
      Left = ALIGN_LEFT,
      Right = ALIGN_RIGHT,
      Center = ALIGN_CENTER,
    }


    public enum eOrientation
    {
      Horizontal,
      Vertical
    } ;

    /// <summary>
    /// empty constructor
    /// </summary>
    public GUIControl()
    {
    }

    /// <summary>
    /// The basic constructur of the GUIControl class.
    /// </summary>
    public GUIControl(int dwParentID)
      : this()
    {
      _parentControlId = dwParentID;
    }


    /// <summary>
    /// The constructor of the GUIControl class.
    /// </summary>
    /// <param name="dwParentID">The id of the parent control.</param>
    /// <param name="dwControlId">The id of this control.</param>
    /// <param name="dwPosX">The X position on the screen of this control.</param>
    /// <param name="dwPosY">The Y position on the screen of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    public GUIControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight)
      : this()
    {
      _parentControlId = dwParentID;
      _controlId = dwControlId;
      _positionX = dwPosX;
      _positionY = dwPosY;

      base.Width = dwWidth;
      base.Height = dwHeight;
      _visibleFromSkinCondition = true;
      _visibleCondition = 0;
    }

    public List<int> Info
    {
      get { return _infoList; }
      set
      {
        if (value != null)
        {
          _infoList = value;
        }
      }
    }

    public GUIControl ParentControl
    {
      get { return _parentControl; }
      set { _parentControl = value; }
    }

    public virtual bool Dimmed
    {
      get
      {
        if (_parentControl != null)
        {
          return _parentControl.Dimmed;
        }
        return _isDimmed;
      }
      set { _isDimmed = value; }
    }

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public virtual void FinalizeConstruction()
    {
      //			if (_upControlId == 0) _upControlId		= _controlId - 1; 
      //			if (_downControlId == 0) _downControlId	= _controlId + 1; 
      //			if (_leftControlId == 0) _leftControlId	= _controlId; 
      //			if (_rightControlId == 0) _rightControlId = _controlId; 
    }

    /// <summary>
    /// Does any scaling on the inital size\position values to fit them to screen 
    /// resolution. 
    /// </summary>
    public virtual void ScaleToScreenResolution()
    {
      int x = _positionX;
      int y = _positionY;
      int w = base.Width;
      int h = base.Height;

      GUIGraphicsContext.ScaleRectToScreenResolution(ref x, ref y, ref w, ref h);

      _positionX = x;
      _positionY = y;
      base.Width = w;
      base.Height = h;
    }

    public virtual void DoRender(float timePassed, uint currentTime)
    {
      Animate(currentTime);
      if (_hasCamera)
      {
        GUIGraphicsContext.SetCameraPosition(_camera);
      }
      Render(timePassed);
      if (_hasCamera)
      {
        GUIGraphicsContext.RestoreCameraPosition();
      }
      GUIGraphicsContext.RemoveTransform();
    }

    /// <summary>
    /// The default render method. This needs to be overwritten when inherited to give every control 
    /// its specific look and feel.
    /// </summary>
    public virtual void Render(float timePassed)
    {
      _hasRendered = true;
    }

    /// <summary>
    /// Property to get/set the id of the window 
    /// to which this control belongs
    /// </summary>
    public virtual int WindowId
    {
      get { return _windowId; }
      set { _windowId = value; }
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public virtual void OnAction(Action action)
    {
      if (Focus == false)
      {
        return;
      }

      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOVE_DOWN:
        case Action.ActionType.ACTION_MOVE_UP:
        case Action.ActionType.ACTION_MOVE_LEFT:
        case Action.ActionType.ACTION_MOVE_RIGHT:
          {
            int controlId = 0;

            switch (action.wID)
            {
              case Action.ActionType.ACTION_MOVE_DOWN:
                controlId = _downControlId;
                break;
              case Action.ActionType.ACTION_MOVE_UP:
                controlId = _upControlId;
                break;
              case Action.ActionType.ACTION_MOVE_LEFT:
                controlId = _leftControlId;
                break;
              case Action.ActionType.ACTION_MOVE_RIGHT:
                controlId = _rightControlId;
                break;
            }

            if (controlId == 0)
            {
              controlId = Navigate((Direction) action.wID);
            }

            if (controlId != -1 && controlId != GetID)
            {
              FocusControl(WindowId, controlId, (Direction) action.wID);
            }

            break;
          }
      }
    }

    private int Navigate(Direction direction)
    {
      int currentX = this.XPosition;
      int currentY = this.YPosition;

      if (this is GUIListControl)
      {
        Rectangle rect = ((GUIListControl) this).SelectedRectangle;

        currentX = rect.X;
        currentY = rect.Y;
      }

      int nearestIndex = -1;
      double distanceMin = 10000;
      double bearingMin = 10000;

      foreach (GUIControl control in FlattenHierarchy(GUIWindowManager.GetWindow(WindowId).Children))
      {
        if (control.GetID == GetID)
        {
          continue;
        }

        if (control.CanFocus() == false)
        {
          continue;
        }

        double bearing = CalcBearing(new Drawing.Point(currentX, currentY),
                                     new Drawing.Point(control.XPosition, control.YPosition));

        if (direction == Direction.Left && (bearing < 215 || bearing > 325))
        {
          continue;
        }

        if (direction == Direction.Right && (bearing < -145 || bearing > -35))
        {
          continue;
        }

        if (direction == Direction.Up && (bearing < -45 || bearing > 45))
        {
          continue;
        }

        if (direction == Direction.Down && !(bearing <= -135 || bearing >= 135))
        {
          continue;
        }

        double distance = CalcDistance(new Drawing.Point(currentX, currentY),
                                       new Drawing.Point(control.XPosition, control.YPosition));

        if (!(distance <= distanceMin && bearing <= bearingMin))
        {
          continue;
        }

        bearingMin = bearing;
        distanceMin = distance;
        nearestIndex = control.GetID;
      }

      return nearestIndex == -1 ? GetID : nearestIndex;
    }

    private static double CalcBearing(Drawing.Point p1, Drawing.Point p2)
    {
      double horzDelta = p2.X - p1.X;
      double vertDelta = p2.Y - p1.Y;

      // arctan gives us the bearing, just need to convert -pi..+pi to 0..360 deg
      double bearing = Math.Round(90 - Math.Atan2(vertDelta, horzDelta)/Math.PI*180 + 360)%360;

      // normalize
      bearing = bearing > 180 ? ((bearing + 180)%360) - 180 : bearing < -180 ? ((bearing - 180)%360) + 180 : bearing;

      return bearing >= 0 ? bearing - 180 : 180 - bearing;
    }

    private static double CalcDistance(Drawing.Point p2, Drawing.Point p1)
    {
      double horzDelta = p2.X - p1.X;
      double vertDelta = p2.Y - p1.Y;

      return Math.Round(Math.Sqrt((horzDelta*horzDelta) + (vertDelta*vertDelta)));
    }

    private ArrayList FlattenHierarchy(UIElementCollection elements)
    {
      ArrayList targetList = new ArrayList();

      FlattenHierarchy(elements, targetList);

      return targetList;
    }

    private void FlattenHierarchy(ICollection collection, ArrayList targetList)
    {
      foreach (GUIControl control in collection)
      {
        if (control.GetID == 1)
        {
          continue;
        }

        if (control is GUIGroup)
        {
          FlattenHierarchy(((GUIGroup) control).Children, targetList);
          continue;
        }

        if (control is GUIFacadeControl)
        {
          GUIFacadeControl facade = (GUIFacadeControl) control;

          switch (facade.View)
          {
            case GUIFacadeControl.ViewMode.AlbumView:
              targetList.Add(facade.AlbumListView);
              break;
            case GUIFacadeControl.ViewMode.Filmstrip:
              targetList.Add(facade.FilmstripView);
              break;
            case GUIFacadeControl.ViewMode.List:
              targetList.Add(facade.ListView);
              break;
            default:
              targetList.Add(facade.ThumbnailView);
              break;
          }

          continue;
        }

        targetList.Add(control);
      }
    }

    /// <summary>
    /// OnMessage() This method gets called when there's a new message. 
    /// Controls send messages to notify their parents about their state (changes)
    /// By overriding this method a control can respond to the messages of its controls
    /// </summary>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public virtual bool OnMessage(GUIMessage message)
    {
      if (message.TargetControlId == GetID)
      {
        switch (message.Message)
        {
          case GUIMessage.MessageType.GUI_MSG_SETFOCUS:

            // if control is disabled then move 2 the next control
            if (Disabled || !CanFocus())
            {
              int controlId = 0;

              switch ((Action.ActionType) message.Param1)
              {
                case Action.ActionType.ACTION_MOVE_DOWN:
                  controlId = _downControlId;
                  break;
                case Action.ActionType.ACTION_MOVE_UP:
                  controlId = _upControlId;
                  break;
                case Action.ActionType.ACTION_MOVE_LEFT:
                  controlId = _leftControlId;
                  break;
                case Action.ActionType.ACTION_MOVE_RIGHT:
                  controlId = _rightControlId;
                  break;
              }

              if (controlId == 0)
              {
                controlId = Navigate((Direction) message.Param1);
              }

              if (controlId != -1 && controlId != GetID)
              {
                FocusControl(WindowId, controlId, (Direction) message.Param1);
              }

              return true;
            }

            Focus = true;
            return true;

          case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
            {
              Focus = false;
              return true;
            }

          case GUIMessage.MessageType.GUI_MSG_VISIBLE:
            Visible = (_visibleCondition != 0) ? GUIInfoManager.GetBool(_visibleCondition, ParentID) : true;

            return true;

          case GUIMessage.MessageType.GUI_MSG_HIDDEN:
            if (IsEffectAnimating(AnimationType.Visible))
            {
              //        CLog::DebugLog("Resetting visible animation on control %i (we are %s)", m_dwControlID, m_visible ? "visible" : "hidden");
              List<VisualEffect> visibleAnims = GetAnimations(AnimationType.Visible, false);
              foreach (VisualEffect anim in visibleAnims)
              {
                anim.ResetAnimation();
              }
            }
            Visible = false;
            return true;

          case GUIMessage.MessageType.GUI_MSG_ENABLED:
            IsEnabled = true;
            return true;


          case GUIMessage.MessageType.GUI_MSG_DISABLED:
            IsEnabled = false;
            return true;

          case GUIMessage.MessageType.GUI_MSG_SELECTED:
            _isSelected = true;
            return true;


          case GUIMessage.MessageType.GUI_MSG_DESELECTED:
            _isSelected = false;
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the ID of the control.
    /// </summary>
    public virtual int GetID
    {
      get { return _controlId; }
      set { _controlId = value; }
    }

    /// <summary>
    /// Gets the ID of the parent control.
    /// </summary>
    public int ParentID
    {
      get { return _parentControlId; }
      set { _parentControlId = value; }
    }

    /// <summary>
    /// Sets and gets the status of the focus of the control.
    /// </summary>
    public new virtual bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (Focus && !value)
        {
          QueueAnimation(AnimationType.Unfocus);
        }
        else if (!Focus && value)
        {
          QueueAnimation(AnimationType.Focus);
        }
        SetValue(IsFocusedProperty, value);
      }
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public virtual void PreAllocResources()
    {
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public virtual void AllocResources()
    {
      _hasRendered = false;
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public virtual void FreeResources()
    {
      // Reset our animation states
      for (int i = 0; i < _animations.Count; i++)
      {
        _animations[i].ResetAnimation();
      }
      _hasRendered = false;
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
    /// some controls (for example the fadelabel) contain scrolling texts and need 2
    /// ne re-rendered constantly
    /// </summary>
    /// <returns>true or false</returns>
    public virtual bool NeedRefresh()
    {
      return false;
    }

    /// <summary>
    /// Checks if the control can focus.
    /// </summary>
    /// <returns>true or false</returns>
    public virtual bool CanFocus()
    {
      if (_allowHiddenFocus && Focusable && IsEnabled)
      {
        return true;
      }
      return Focusable && IsEnabled && IsVisible;
    }

    /// <summary>
    /// Gets and sets the Disabled property of the control.
    /// </summary>
    public virtual bool Disabled
    {
      get { return !IsEnabled; }
      set { IsEnabled = !value; }
    }

    /// <summary>
    /// Gets and sets the Selected property of the control.
    /// </summary>
    public virtual bool Selected
    {
      get { return _isSelected; }
      set { _isSelected = value; }
    }

    /// <summary>
    /// Sets the position of the control.
    /// </summary>
    /// <param name="dwPosX">The X position.</param>
    /// <param name="dwPosY">The Y position.</param>
    public virtual void SetPosition(int dwPosX, int dwPosY)
    {
      if (_positionX == dwPosX && _positionY == dwPosY)
      {
        return;
      }
      _positionX = dwPosX;
      _positionY = dwPosY;
      Update();
    }

    /// <summary>
    /// Changes the alpha transparency component of the colordiffuse.
    /// </summary>
    /// <param name="dwAlpha"></param>
    public virtual void SetAlpha(int dwAlpha)
    {
    }

    /// <summary>
    /// ColourDiffuse allows you to mix a color & a graphics texture.
    /// (E.g., if you have a graphics texture like the button which is blue you can mix it 
    ///  with lets say a yellow color diffuse and the end result will b green).
    /// </summary>
    public virtual long ColourDiffuse
    {
      get { return _diffuseColor; }
      set
      {
        if (value != _diffuseColor)
        {
          _diffuseColor = value;
          Update();
        }
      }
    }

    /// <summary>
    /// Gets and sets the X position of the control.
    /// </summary>
    public virtual int XPosition
    {
      get { return _positionX; }
      set
      {
        if (_positionX != value)
        {
          _positionX = Math.Max(0, value);
          Update();
        }
      }
    }

    /// <summary>
    /// Gets and sets the Y position of the control.
    /// </summary>
    public virtual int YPosition
    {
      get { return _positionY; }
      set
      {
        if (_positionY != value)
        {
          _positionY = Math.Max(0, value);
          Update();
        }
      }
    }

    public bool Visible
    {
      get { return IsVisible; }
      set
      {
        if (IsVisible == value)
        {
          return;
        }
        if (IsVisible && !value)
        {
          QueueAnimation(AnimationType.Hidden);
        }
        else if (!IsVisible && value)
        {
          QueueAnimation(AnimationType.Visible);
        }
        IsVisible = value;
      }
    }

    /// <summary>
    /// Set the up/down/left/right control
    /// </summary>
    /// <param name="dwUp">The control above this control.</param>
    /// <param name="dwDown">The control under this control.</param>
    /// <param name="dwLeft">The control left to this control.</param>
    /// <param name="dwRight">The control right to this control.</param>
    public virtual void SetNavigation(int dwUp, int dwDown, int dwLeft, int dwRight)
    {
      _leftControlId = dwLeft;
      _rightControlId = dwRight;
      _upControlId = dwUp;
      _downControlId = dwDown;
    }

    public virtual int NavigateUp
    {
      get { return _upControlId; }
      set { _upControlId = value; }
    }

    public virtual int NavigateDown
    {
      get { return _downControlId; }
      set { _downControlId = value; }
    }

    public virtual int NavigateLeft
    {
      get { return _leftControlId; }
      set { _leftControlId = value; }
    }

    public virtual int NavigateRight
    {
      get { return _rightControlId; }
      set { _rightControlId = value; }
    }

    /// <summary>
    /// Gets and sets if the control is in calibration mode
    /// </summary>
    public bool CalibrationEnabled
    {
      get { return _calibration; }
      set { _calibration = value; }
    }

    /// <summary>
    /// Gets and sets the type of the control. E.g. image, label, etc.
    /// </summary>
    public string Type
    {
      get { return _controlType; }
      set
      {
        if (_controlType == null)
        {
          return;
        }
        _controlType = value;
      }
    }

    /// <summary>
    /// Gets and sets the data that is contained within the control. E.g. a TVProgram
    /// </summary>
    public object Data
    {
      get { return _data; }
      set { _data = value; }
    }

    /// <summary>
    /// Checks if the x and y coordinates correspond to the current control.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>True if the control was hit.</returns>
    public virtual bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      focused = Focus;
      controlID = GetID;
      if (!IsVisible)
      {
        return false;
      }
      if (Disabled)
      {
        return false;
      }
      if (CanFocus() == false)
      {
        return false;
      }
      return InControl(x, y, out controlID);
    }


    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>
    protected virtual void Update()
    {
    }

    /// <summary>
    /// Sends a GUI_MSG_HIDDEN message to a control (Hide a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void HideControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_VISIBLE message to a control (Make a control visible).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void ShowControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }


    public static void RefreshControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESH, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_LOSTFOCUS message to a control (Set the focus on a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void UnfocusControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LOSTFOCUS, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_SETFOCUS message to a control (Set the focus on a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void FocusControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    public static void FocusControl(int iWindowId, int iControlId, Direction direction)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, iWindowId, 0, iControlId, (int) direction,
                                      0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_LABEL_SET message to a control (Set the label of a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <param name="strText">The text that needs to be set on the target label.</param>
    public static void SetControlLabel(int iWindowId, int iControlId, string strText)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, iWindowId, 0, iControlId, 0, 0, null);
      msg.Label = strText;
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_LABEL_ADD message to a control (Add a ListItem to a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <param name="item">The item that needs to be added to the ListControl</param>
    public static void AddListItemControl(int iWindowId, int iControlId, GUIListItem item)
    {
      // TODO The AddListItemControl should use another message type for adding Items. (REQUIRES a check of every GUI_MSG_LABEL_ADD!).
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId, 0, iControlId, 0, 0, item);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_LABEL_ADD message to a control (Add an ItemLabel to a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <param name="strLabel">The text of the label that needs to be added.</param>
    public static void AddItemLabelControl(int iWindowId, int iControlId, string strLabel)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId, 0, iControlId, 0, 0, null);
      msg.Label = strLabel;
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_LABEL_RESET message to a control (Clears a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void ClearControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_ITEM_SELECT message to a control (Select an item in a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <param name="iItem">The id of the item that is selected on the control.</param>
    public static void SelectItemControl(int iWindowId, int iControlId, int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, iWindowId, 0, iControlId, iItem, 0,
                                      null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_ITEM_FOCUS message to a control (set item in control to selected state).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <param name="iItem">The id of the item that should have the selected state.</param>
    public static void FocusItemControl(int iWindowId, int iControlId, int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, iWindowId, 0, iControlId, iItem, 0,
                                      null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_GET_ITEM message to a control (Gets a GUIListItem based on the lWindowId, iControlId, iItem parameters).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <param name="iItem">The item id of the item that needs to be returned.</param>
    /// <returns>The GUIListItem that corresponds to the lWindowId, iControlId, iItem</returns>
    public static GUIListItem GetListItem(int lWindowId, int iControlId, int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_ITEM, lWindowId, 0, iControlId, iItem, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(lWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }

      return msg.Object as GUIListItem;
    }

    /// <summary>
    /// Sends a GUI_MSG_GET_SELECTED_ITEM message to a control (Gets the selected GUIListItem based on the lWindowId, iControlId).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <returns>The GUIListItem that is selected.</returns>
    public static GUIListItem GetSelectedListItem(int lWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM, 0, lWindowId, iControlId, 0, 0,
                                      null);
      GUIWindow window = GUIWindowManager.GetWindow(lWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }

      return msg.Object as GUIListItem;
    }

    /// <summary>
    /// Sends a GUI_MSG_ITEMS message to a control (Gets the number of Items in a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    /// <returns>The number of items in a control.</returns>
    public static int GetItemCount(int lWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEMS, lWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(lWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }

      return msg.Param1;
    }

    /// <summary>
    /// Sends a GUI_MSG_SELECTED message to a control (Select a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void SelectControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_DESELECTED message to a control (Deselect a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void DeSelectControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_DISABLED message to a control (Disables a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void DisableControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Sends a GUI_MSG_ENABLED message to a control (Enables a control).
    /// </summary>
    /// <param name="iWindowId">The SenderId.</param>
    /// <param name="iControlId">The target control.</param>
    public static void EnableControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, iWindowId, 0, iControlId, 0, 0, null);
      GUIWindow window = GUIWindowManager.GetWindow(iWindowId);
      if (window != null)
      {
        window.OnMessage(msg);
      }
    }

    /// <summary>
    /// Method which determines of the coordinate(x,y) is within the current control
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordiate </param>
    /// <param name="controlID">return id of control if coordinate is within control</param>
    /// <returns>true: point is in control
    ///          false: point is not within control
    /// </returns>
    public virtual bool InControl(int x, int y, out int controlID)
    {
      controlID = -1;
      if (x >= XPosition && x < XPosition + Width)
      {
        if (y >= YPosition && y < YPosition + Height)
        {
          controlID = GetID;
          return true;
        }
      }
      return false;
    }

    public virtual void DoUpdate()
    {
      Update();
    }


    /// <summary>
    /// Add an subitem to a control
    /// </summary>
    /// <param name="obj">subitem</param>
    public void AddSubItem(object obj)
    {
      _subItemList.Add(obj);
    }

    /// <summary>
    /// Remove an subitem from an control
    /// </summary>
    /// <param name="obj">subitem</param>
    public void RemoveSubItem(object obj)
    {
      _subItemList.Remove(obj);
    }

    /// <summary>
    /// Remove an subitem from an control
    /// </summary>
    /// <param name="obj">index</param>
    public void RemoveSubItem(int index)
    {
      if (index <= 0 || index >= _subItemList.Count)
      {
        return;
      }
      _subItemList.RemoveAt(index);
    }

    /// <summary>
    /// Property to get the # of subitems for the control
    /// </summary>
    public int SubItemCount
    {
      get { return _subItemList.Count; }
    }

    /// <summary>
    /// Property to get a subitem
    /// </summary>
    /// <param name="index">index</param>
    /// <returns>subitem object</returns>
    public object GetSubItem(int index)
    {
      if (index < 0 || index >= _subItemList.Count)
      {
        return null;
      }
      return _subItemList[index];
    }

    /// <summary>
    /// Property to set an subitem
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="o">subitem</param>
    public void SetSubItem(int index, object o)
    {
      if (index < 0 || index >= _subItemList.Count)
      {
        return;
      }
      _subItemList[index] = o;
    }

    /// <summary>
    /// Property to get/set the current selected subitem
    /// </summary>
    public virtual int SelectedItem
    {
      get { return _selectedItem; }
      set { _selectedItem = value; }
    }

    /// <summary>
    /// Property to get the control for a specific control ID
    /// </summary>
    /// <param name="ID">Id of wanted control</param>
    /// <returns>null if not found or
    ///          GUIControl if found
    /// </returns>
    public virtual GUIControl GetControlById(int ID)
    {
      if (ID == GetID)
      {
        return this;
      }
      return null;
    }

    /// <summary>
    /// Virtual method. This method gets called when the control is initialized
    /// and allows it to do any initalization
    /// </summary>
    public virtual void OnInit()
    {
    }

    /// <summary>
    /// Virtual method. This method gets called when the control is de-initialized
    /// and allows it to do any de-initalization
    /// </summary>
    public virtual void OnDeInit()
    {
    }

    /// <summary>
    /// Description (from xml skin file) for control
    /// </summary>
    public string Description
    {
      get { return _description; }
      set
      {
        if (value == null)
        {
          return;
        }
        _description = value;
      }
    }

    /// <summary>
    /// Method to store(save) the current control rectangle
    /// </summary>
    public virtual void StorePosition()
    {
      _isAnimating = false;
      _originalRectangle = new Rectangle(_positionX, _positionY, base.Width, base.Height);
      _originalDiffuseColor = _diffuseColor;
    }

    /// <summary>
    /// Property to determine if control is animating
    /// </summary>
    public bool IsAnimating
    {
      get { return _isAnimating; }
    }

    public virtual int DimColor
    {
      get { return _dimColor; }
      set { _dimColor = value; }
    }

    /// <summary>
    /// Method to restore the saved-current control rectangle
    /// </summary>
    public virtual void ReStorePosition()
    {
      _positionX = _originalRectangle.X;
      _positionY = _originalRectangle.Y;
      base.Width = _originalRectangle.Width;
      base.Height = _originalRectangle.Height;
      _diffuseColor = _originalDiffuseColor;
      Update();
      _isAnimating = false;
    }

    /// <summary>
    /// Method to get the rectangle of the current control 
    /// </summary>
    public virtual void GetRect(out int x, out int y, out int width, out int height)
    {
      x = _positionX;
      y = _positionY;
      width = base.Width;
      height = base.Height;
    }

    /// <summary>
    /// Method to get animate the current control
    /// </summary>
    public virtual void Animate(float timePassed, Animator animator)
    {
      _isAnimating = true;
      int x = _originalRectangle.X;
      int y = _originalRectangle.Y;
      int w = _originalRectangle.Width;
      int h = _originalRectangle.Height;
      long color = _diffuseColor;
      animator.Animate(timePassed, ref x, ref y, ref w, ref h, ref color);

      _diffuseColor = color;
      _positionX = x;
      _positionY = y;
      base.Width = w;
      base.Height = h;
      DoUpdate();
    }

    public List<GUIControl> LoadControl(string xmlFilename)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(GUIGraphicsContext.Skin + "\\" + xmlFilename);
      List<GUIControl> listControls = new List<GUIControl>();


      if (doc.DocumentElement == null)
      {
        return listControls;
      }
      if (doc.DocumentElement.Name != "window")
      {
        return listControls;
      }

      // Load Definitions
      Hashtable table = new Hashtable();
      try
      {
        foreach (XmlNode node in doc.SelectNodes("/window/define"))
        {
          string[] tokens = node.InnerText.Split(':');
          if (tokens.Length < 2)
          {
            continue;
          }
          table[tokens[0]] = tokens[1];
        }
      }
      catch (Exception e)
      {
        Log.Info("LoadDefines: {0}", e.Message);
      }

      foreach (XmlNode controlNode in doc.DocumentElement.SelectNodes("/window/controls/control"))
      {
        try
        {
          GUIControl newControl = GUIControlFactory.Create(_windowId, controlNode, table);
          if (newControl != null)
          {
            listControls.Add(newControl);
          }
        }
        catch (Exception ex)
        {
          Log.Error("Unable to load control: {0}", ex.ToString());
        }
      }
      return listControls;
    }

    public GUIAnimation LoadAnimationControl(int parentID, int controlId, int posX, int posY, int width, int height,
                                             string texture)
    {
      if ((texture != null) && (texture.Contains(".xml")))
      {
        List<GUIControl> list = LoadControl(texture);
        foreach (GUIControl control in list)
        {
          GUIAnimation animation = control as GUIAnimation;
          if (animation != null)
          {
            animation.SetPosition(posX, posY);
            animation.WindowId = parentID;
            animation.GetID = controlId;
            animation.Width = width;
            animation.Height = height;
            return animation;
          }
        }
      }
      return new GUIAnimation(parentID, controlId, posX, posY, width, height, texture);
    }

    public string SubType
    {
      get { return _subType; }
    }

    [XMLSkinElement("width")]
    public int _width
    {
      get { return base.Width; }
      set { base.Width = value; }
    }

    [XMLSkinElement("height")]
    public int _height
    {
      get { return base.Height; }
      set { base.Height = value; }
    }

    [XMLSkinElement("posX")]
    public int _positionX
    {
      get { return (int) base.Location.X; }
      set { base.Location = new Drawing.Point(value, base.Location.Y); }
    }

    [XMLSkinElement("posY")]
    public int _positionY
    {
      get { return (int) base.Location.Y; }
      set { base.Location = new Drawing.Point(base.Location.X, value); }
    }

    /////////////////////////////////////////////

    #region Enums

    public enum Direction
    {
      None = 0,
      Up = Action.ActionType.ACTION_MOVE_UP,
      Down = Action.ActionType.ACTION_MOVE_DOWN,
      Left = Action.ActionType.ACTION_MOVE_LEFT,
      Right = Action.ActionType.ACTION_MOVE_RIGHT,
    }

    #endregion Enums

    #region Methods

    protected override Size ArrangeOverride(Rect finalRect)
    {
      Size size = base.ArrangeOverride(finalRect);

      Update();

      return size;
    }

    #endregion Methods

    #region Properties

    public override int Width
    {
      get { return base.Width; }
      set
      {
        if (base.Width != value)
        {
          base.Width = Math.Max(0, value);
          Update();
        }
      }
    }

    public override int Height
    {
      get { return base.Height; }
      set
      {
        if (base.Height != value)
        {
          base.Height = Math.Max(0, value);
          Update();
        }
      }
    }

    public override double Opacity
    {
      get { return 255.0/Color.FromArgb((int) _diffuseColor).A; }
      set { _diffuseColor = Color.FromArgb((int) (255*value), Color.FromArgb((int) _diffuseColor)).ToArgb(); }
    }

    public Size Size
    {
      get { return new Size(base.Width, base.Height); }
      set
      {
        base.Width = (int) value.Width;
        base.Height = (int) value.Height;
        Update();
      }
    }

    #endregion Properties

    public List<VisualEffect> Animations
    {
      get { return _animations; }
    }

    public List<VisualEffect> ThumbAnimations
    {
      get { return _thumbAnimations; }
    }

    public virtual void SetAnimations(List<VisualEffect> animations)
    {
      _animations = new List<VisualEffect>();
      foreach (VisualEffect effect in animations)
      {
        _animations.Add((VisualEffect) effect.Clone());
      }
    }

    public virtual void AddAnimations(List<VisualEffect> animations)
    {
      if (animations.Count == 0)
      {
        return;
      }

      if (_animations == null)
      {
        _animations = new List<VisualEffect>();
      }
      foreach (VisualEffect effect in animations)
      {
        _animations.Add((VisualEffect) effect.Clone());
      }
    }

    public virtual void SetThumbAnimations(List<VisualEffect> animations)
    {
      _thumbAnimations = new List<VisualEffect>();
      foreach (VisualEffect effect in animations)
      {
        _thumbAnimations.Add((VisualEffect) effect.Clone());
      }
    }

    public virtual void QueueAnimation(AnimationType animType)
    {
      if (false == GUIGraphicsContext.Animations)
      {
        return;
      }
      // rule out the animations we shouldn't perform
      if (!IsVisible || !HasRendered)
      {
        // hidden or never rendered - don't allow exit or entry animations for this control
        if (animType == AnimationType.WindowClose && !IsEffectAnimating(AnimationType.WindowOpen))
        {
          return;
        }
      }
      if (!IsVisible)
      {
        // hidden - only allow hidden anims if we're animating a visible anim
        if (animType == AnimationType.Hidden && !IsEffectAnimating(AnimationType.Visible))
        {
          return;
        }
        if (animType == AnimationType.WindowOpen)
        {
          return;
        }
      }
      List<VisualEffect> reverseAnims = GetAnimations((AnimationType) (-(int) animType), false);
      List<VisualEffect> forwardAnims = GetAnimations(animType, false);
      bool done = false;
      foreach (VisualEffect reverseAnim in reverseAnims)
      {
        if (reverseAnim.IsReversible &&
            (reverseAnim.CurrentState == AnimationState.InProcess || reverseAnim.CurrentState == AnimationState.Delayed))
        {
          reverseAnim.QueuedProcess = AnimationProcess.Reverse;
          foreach (VisualEffect forwardAnim in forwardAnims)
          {
            forwardAnim.ResetAnimation();
          }
          done = true;
        }
      }
      if (!done)
      {
        foreach (VisualEffect forwardAnim in forwardAnims)
        {
          forwardAnim.QueuedProcess = AnimationProcess.Normal;
          foreach (VisualEffect reverseAnim in reverseAnims)
          {
            reverseAnim.ResetAnimation();
          }
          done = true;
        }
      }
      if (!done)
      {
        foreach (VisualEffect reverseAnim in reverseAnims)
        {
          reverseAnim.ResetAnimation();
        }
        UpdateStates(animType, AnimationProcess.Normal, AnimationState.StateApplied);
      }
      /*
      VisualEffect reverseAnim = GetAnimations((AnimationType)(-(int)animType), false);
    VisualEffect forwardAnim = GetAnimations(animType, true);
    // we first check whether the reverse animation is in progress (and reverse it)
    // then we check for the normal animation, and queue it
    if (reverseAnim != null && reverseAnim.IsReversible && (reverseAnim.CurrentState == AnimationState.InProcess || reverseAnim.CurrentState == AnimationState.Delayed))
    {
      reverseAnim.QueuedProcess = AnimationProcess.Reverse;
      if (forwardAnim != null) forwardAnim.ResetAnimation();
    }
    else if (forwardAnim != null)
    {
      forwardAnim.QueuedProcess = AnimationProcess.Normal;
      if (reverseAnim != null) reverseAnim.ResetAnimation();
    }
    else
    { // hidden and visible animations delay the change of state.  If there is no animations
      // to perform, then we should just change the state straightaway
      if (reverseAnim != null) reverseAnim.ResetAnimation();
      UpdateStates(animType, AnimationProcess.Normal, AnimationState.StateApplied);
    }
       */
    }

    public virtual int GetVisibleCondition()
    {
      return _visibleCondition;
      ;
    }

    public virtual List<VisualEffect> GetAnimations(AnimationType type, bool checkConditions /* = true */)
    {
      if (false == GUIGraphicsContext.Animations)
      {
        return null;
      }
      List<VisualEffect> effects = new List<VisualEffect>();
      for (int i = 0; i < _animations.Count; i++)
      {
        if (_animations[i].AnimationType == type)
        {
          if (!checkConditions || _animations[i].Condition == 0 || GUIInfoManager.GetBool(_animations[i].Condition, 0))
          {
            effects.Add(_animations[i]);
          }
        }
      }
      return effects;
    }

    public virtual VisualEffect GetAnimation(AnimationType type, bool checkConditions /* = true */)
    {
      if (false == GUIGraphicsContext.Animations)
      {
        return null;
      }
      for (int i = 0; i < _animations.Count; i++)
      {
        if (_animations[i].AnimationType == type)
        {
          if (!checkConditions || _animations[i].Condition == 0 || GUIInfoManager.GetBool(_animations[i].Condition, 0))
          {
            return _animations[i];
          }
        }
      }
      return null;
    }

    private void UpdateStates(AnimationType type, AnimationProcess currentProcess, AnimationState currentState)
    {
      if (GUIGraphicsContext.Animations == false)
      {
        return;
      }
      bool visible = IsVisible;
      // Make sure control is hidden or visible at the appropriate times
      // while processing a visible or hidden animation it needs to be visible,
      // but when finished a hidden operation it needs to be hidden
      if (type == AnimationType.Visible)
      {
        if (currentProcess == AnimationProcess.Reverse)
        {
          if (currentState == AnimationState.StateApplied)
          {
            IsVisible = false;
          }
        }
        else if (currentProcess == AnimationProcess.Normal)
        {
          if (currentState == AnimationState.Delayed)
          {
            IsVisible = false;
          }
          else
          {
            IsVisible = _visibleFromSkinCondition;
          }
        }
      }
      else if (type == AnimationType.Hidden)
      {
        if (currentProcess == AnimationProcess.Normal) // a hide animation
        {
          if (currentState == AnimationState.StateApplied)
          {
            IsVisible = false; // finished
          }
          else
          {
            IsVisible = true; // have to be visible until we are finished
          }
        }
        else if (currentProcess == AnimationProcess.Reverse) // a visible animation
        {
          // no delay involved here - just make sure it's visible
          IsVisible = _visibleFromSkinCondition;
        }
      }
      else if (type == AnimationType.WindowOpen)
      {
/*
        if (currentProcess == AnimationProcess.Normal)
        {
          if (currentState == AnimationState.Delayed)
            IsVisible = false; // delayed
          else
            IsVisible = _visibleFromSkinCondition;
        }*/
      }
      else if (type == AnimationType.Focus)
      {
        // call the focus function if we have finished a focus animation
        // (buttons can "click" on focus)
        if (currentProcess == AnimationProcess.Normal && currentState == AnimationState.StateApplied)
        {
          OnFocus();
        }
      }
      //  if (visible != m_visible)
      //    CLog::DebugLog("UpdateControlState of control id %i - now %s (type=%d, process=%d, state=%d)", m_dwControlID, m_visible ? "visible" : "hidden", type, currentProcess, currentState);
    }

    public virtual void GetCenter(ref float centerX, ref float centerY)
    {
      centerX = (float) (XPosition + (Width/2));
      centerY = (float) (YPosition + (Height/2));
    }

    public TransformMatrix getTransformMatrix(uint currentTime)
    {
      TransformMatrix transform = new TransformMatrix();
      if (GUIGraphicsContext.Animations)
      {
        for (int i = 0; i < _animations.Count; i++)
        {
          VisualEffect anim = _animations[i];
          anim.Animate(currentTime, HasRendered);
          // Update the control states (such as visibility)
          UpdateStates(anim.AnimationType, anim.CurrentProcess, anim.CurrentState);
          // and render the animation effect
          float centerXOrg = anim.CenterX;
          float centerYOrg = anim.CenterY;
          float centerX = 0, centerY = 0;
          GetCenter(ref centerX, ref centerY);
          //GUIGraphicsContext.ScaleHorizontal(ref centerX);
          //GUIGraphicsContext.ScaleVertical(ref centerY);
          anim.SetCenter(centerX, centerY);
          anim.RenderAnimation(ref transform);
          anim.CenterX = centerXOrg;
          anim.CenterY = centerYOrg;
        }
      }
      return transform;
    }

    public void Animate(uint currentTime)
    {
      TransformMatrix transform = getTransformMatrix(currentTime);
      GUIGraphicsContext.AddTransform(transform);
    }

    public virtual bool IsEffectAnimating(AnimationType animType)
    {
      if (false == GUIGraphicsContext.Animations)
      {
        return false;
      }
      for (int i = 0; i < _animations.Count; i++)
      {
        VisualEffect anim = _animations[i];
        if (anim.AnimationType == animType)
        {
          if (anim.QueuedProcess == AnimationProcess.Normal)
          {
            return true;
          }
          if (anim.CurrentProcess == AnimationProcess.Normal)
          {
            return true;
          }
        }
        else if (anim.AnimationType == (AnimationType) (-(int) animType))
        {
          if (anim.QueuedProcess == AnimationProcess.Reverse)
          {
            return true;
          }
          if (anim.CurrentProcess == AnimationProcess.Reverse)
          {
            return true;
          }
        }
      }
      return false;
    }

    public bool HasRendered
    {
      get { return _hasRendered; }
    }

    public virtual void UpdateVisibility()
    {
      if (_visibleCondition == 0)
      {
        return;
      }
      bool bWasVisible = _visibleFromSkinCondition;
      _visibleFromSkinCondition = GUIInfoManager.GetBool(_visibleCondition, ParentID);
      if (GUIGraphicsContext.Animations == false)
      {
        Visible = _visibleFromSkinCondition;
        return;
      }
      if (!bWasVisible && _visibleFromSkinCondition)
      {
        // automatic change of visibility - queue the in effect
        //    CLog::DebugLog("Visibility changed to visible for control id %i", m_dwControlID);
        QueueAnimation(AnimationType.Visible);
      }
      else if (bWasVisible && !_visibleFromSkinCondition)
      {
        // automatic change of visibility - do the out effect
        //    CLog::DebugLog("Visibility changed to hidden for control id %i", m_dwControlID);
        QueueAnimation(AnimationType.Hidden);
      }
    }

    public virtual void SetInitialVisibility()
    {
      if (_visibleCondition == 0)
      {
        _visibleFromSkinCondition = Visible;
        return;
      }
      _visibleFromSkinCondition = Visible = GUIInfoManager.GetBool(_visibleCondition, ParentID);

      // no need to enquire every frame if we are always visible or always hidden
      if (_visibleCondition == GUIInfoManager.SYSTEM_ALWAYS_TRUE ||
          _visibleCondition == GUIInfoManager.SYSTEM_ALWAYS_FALSE)
      {
        _visibleCondition = 0;
      }
    }


    public virtual void SetVisibleCondition(int visible, bool allowHiddenFocus)
    {
      _visibleCondition = visible;
      _allowHiddenFocus = allowHiddenFocus;
    }

    protected virtual void OnFocus()
    {
    }

    public Point Camera
    {
      get { return _camera; }
      set { _camera = value; }
    }

    public bool HasCamera
    {
      get { return _hasCamera; }
      set { _hasCamera = value; }
    }

    public void ResetAnimations()
    {
      if (false == GUIGraphicsContext.Animations)
      {
        return;
      }
      for (int i = 0; i < _animations.Count; i++)
      {
        VisualEffect anim = _animations[i];
        anim.ResetAnimation();
      }
      return;
    }
  }
}