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

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Serialization;
using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A class which implements a group
  /// A group can hold 1 or more controls
  /// and apply an animation to the entire group
  /// </summary>
  public class GUIGroup : GUIControl, ISupportInitialize, IAddChild
  {
    #region Constructors

    public GUIGroup(int parentId)
      : base(parentId) {}

    #endregion Constructors

    #region Methods

    public override void OnInit()
    {
      _startAnimation = true;
      _animator = new Animator(_animatorType);
    }

    public void AddControl(GUIControl control)
    {
      //control.AddAnimations(base.Animations);
      if (base.Animations.Count != 0)
      {
        control.Animations.AddRange(base.Animations);
      }
      control.DimColor = DimColor;
      Children.Add(control);
    }

    public override bool Dimmed
    {
      get { return (GetFocusControlId() == -1); }
      set { }
    }

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.Animations)
      {
        if (_animator != null)
        {
          if (_startAnimation)
          {
            _startAnimation = false;
            StorePosition();
          }

          foreach (GUIControl control in Children)
          {
            if (control != null)
            {
              control.Animate(timePassed, _animator);
            }
          }

          _animator.Advance(timePassed);
        }
      }

      //uint currentTime = (uint) (DXUtil.Timer(DirectXTimer.GetAbsoluteTime)*1000.0);
      uint currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;
      foreach (GUIControl control in Children)
      {
        control.UpdateVisibility();
        control.DoRender(timePassed, currentTime);
      }

      if (_animator != null && _animator.IsDone())
      {
        ReStorePosition();
        _animator = null;
      }
      base.Render(timePassed);
    }

    public override void Dispose()
    {
      if (_animator != null)
      {
        ReStorePosition();
        _animator = null;
      }

      _children.SafeDispose();
      
      base.Dispose();
    }

    public override void AllocResources()
    {
      foreach (GUIControl control in Children)
      {
        control.ParentControl = this;
        control.AllocResources();
      }
      base.AllocResources();
    }

    public override void PreAllocResources()
    {
      foreach (GUIControl control in Children)
      {
        control.PreAllocResources();
      }
    }

    public override GUIControl GetControlById(int ID)
    {
      foreach (GUIControl control in Children)
      {
        GUIControl childControl = control.GetControlById(ID);

        if (childControl != null)
        {
          return childControl;
        }
      }

      return null;
    }

    public override bool NeedRefresh()
    {
      foreach (GUIControl control in Children)
      {
        if (control.NeedRefresh())
        {
          return true;
        }
      }

      return false;
    }

    public override bool HitTest(int x, int y, out int controlID, out bool focused)
    {
      controlID = -1;
      focused = false;

      for (int index = Children.Count - 1; index >= 0; index--)
      {
        if ((((GUIControl)Children[index])).HitTest(x, y, out controlID, out focused))
        {
          return true;
        }
      }

      return false;
    }

    public override void OnAction(Action action)
    {
      foreach (GUIControl control in Children)
      {
        if (control.Focus)
        {
          control.OnAction(action);
        }
      }
    }

    public void Remove(int controlId)
    {
      foreach (GUIControl control in Children)
      {
        if (control is GUIGroup)
        {
          ((GUIGroup)control).Remove(controlId);
          break;
        }
        else if (control.GetID == controlId)
        {
          Children.Remove(control);
          break;
        }
      }
    }

    public int GetFocusControlId()
    {
      foreach (GUIControl control in Children)
      {
        if (control is GUIGroup)
        {
          int focusedId = ((GUIGroup)control).GetFocusControlId();

          if (focusedId != -1)
          {
            return focusedId;
          }
        }
        else if (control.Focus)
        {
          return control.GetID;
        }
      }

      return -1;
    }

    public override void DoUpdate()
    {
      foreach (GUIControl control in Children)
      {
        if(!(control is GUIFacadeControl)) // a facadecontrol inside a group with layout, stay compatible with previous implementation
          control.DoUpdate();
      }
    }

    public override void StorePosition()
    {
      foreach (GUIControl control in Children)
      {
        control.StorePosition();
      }

      base.StorePosition();
    }

    public override void ReStorePosition()
    {
      foreach (GUIControl control in Children)
      {
        control.ReStorePosition();
      }

      base.ReStorePosition();
    }

    public override void Animate(float timePassed, Animator animator)
    {
      foreach (GUIControl control in Children)
      {
        control.Animate(timePassed, animator);
      }

      base.Animate(timePassed, animator);
    }

    #endregion Methods

    #region Properties

    public Animator.AnimationType Animation
    {
      get { return _animatorType; }
      set { _animatorType = value; }
    }

    public int Count
    {
      get { return Children.Count; }
    }

    public GUIControl this[int index]
    {
      get { return (GUIControl)Children[index]; }
    }

    /// <summary>
    /// Property to get/set the id of the window 
    /// to which this control belongs
    /// </summary>
    public override int WindowId
    {
      get { return base.WindowId; }
      set
      {
        base.WindowId = value;
        foreach (GUIControl control in Children)
        {
          control.WindowId = value;
        }
      }
    }

    #endregion Properties

    ////////////////////////////

    #region Methods

    void IAddChild.AddChild(object value)
    {
      if (value is GUIControl == false)
      {
        return;
      }
      GUIControl cntl = (GUIControl)value;
      //cntl.AddAnimations(base.Animations);
      if (base.Animations.Count != 0)
      {
        cntl.Animations.AddRange(base.Animations);
      }
      cntl.DimColor = DimColor;
      Children.Add(cntl);
      if (cntl is GUIButtonControl)
      {
        if (_buttons == null)
        {
          _buttons = new List<GUIButtonControl>();
        }
        _buttons.Add((GUIButtonControl)cntl);
      }
    }

    void IAddChild.AddText(string text) {}

    protected void Arrange()
    {
      if (_beginInitCount != 0)
      {
        return;
      }

      if (_layout == null)
      {
        return;
      }

      this.Size = _layout.Measure(this, this.Size);

      _layout.Arrange(this);
      DoUpdate();
    }

    //protected override Size ArrangeOverride(Rect finalRect)
    //{
    //  this.Location = finalRect.Location;
    //  this.Size = finalRect.Size;

    //  if (_layout == null)
    //  {
    //    return this.Size;
    //  }

    //  _layout.Arrange(this);

    //  return finalRect.Size;
    //}

    void ISupportInitialize.BeginInit()
    {
      _beginInitCount++;
    }

    void ISupportInitialize.EndInit()
    {
      if (--_beginInitCount == 0)
      {
        Arrange();
      }
    }

    //protected override Size MeasureOverride(Size availableSize)
    //{
    //  if (_layout == null)
    //  {
    //    return Size.Empty;
    //  }

    //  _layout.Measure(this, this.Size);

    //  return this.Size = _layout.Size;
    //}

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        foreach (GUIControl control in Children)
        {
          if (control != null)
          {
            control.DimColor = value;
          }
        }
      }
    }

    #endregion Methods

    #region Properties

    public ILayout Layout
    {
      get { return _layout; }
      set { _layout = value; }
    }

    public GUIControlCollection Children
    {
      get
      {
        if (_children == null)
        {
          _children = new GUIControlCollection();
        }
        return _children;
      }
    }

    #endregion Properties

    #region Fields

    private Animator _animator;
    private int _beginInitCount = 0;
    private GUIControlCollection _children;
    private List<GUIButtonControl> _buttons = null;
    private Point[] _positions = null;
    private Point[] _modPositions = null;
    private bool _first = true;

    [XMLSkinElement("layout")] private ILayout _layout;

    [XMLSkinElement("animation")] private Animator.AnimationType _animatorType = Animator.AnimationType.None;

    private bool _startAnimation;

    #endregion Fields

    public override void QueueAnimation(AnimationType animType)
    {
      foreach (GUIControl control in Children)
      {
        control.QueueAnimation(animType);
      }
    }

    public override List<VisualEffect> GetAnimations(AnimationType type, bool checkConditions)
    {
      List<VisualEffect> effects = new List<VisualEffect>();

      foreach (GUIControl control in Children)
      {
        if (control != null)
        {
          List<VisualEffect> effects2 = control.GetAnimations(type, checkConditions);
          effects.AddRange(effects2);
        }
      }
      return effects;
    }

    public override VisualEffect GetAnimation(AnimationType type, bool checkConditions /* = true */)
    {
      VisualEffect effect = null;
      foreach (GUIControl control in Children)
      {
        if (control != null)
        {
          effect = control.GetAnimation(type, checkConditions);
          if (effect != null)
          {
            return effect;
          }
        }
      }
      return null;
    }

    public override bool IsEffectAnimating(AnimationType animType)
    {
      foreach (GUIControl control in Children)
      {
        if (control != null)
        {
          bool yes = control.IsEffectAnimating(animType);
          if (yes)
          {
            return true;
          }
        }
      }
      return false;
    }

    public override void UpdateVisibility()
    {
      base.UpdateVisibility();
      if (_buttons == null)
      {
        return;
      }
      if (_layout == null)
      {
        return;
      }

      if (_layout is StackLayout)
      {
        StackLayout layout = _layout as StackLayout;
        if (!layout.CollapseHiddenButtons)
        {
          return;
        }
        if (_first)
        {
          StoreButtonsPosition();
        }
        bool isVisible = IsVisible;
        int visCon = GetVisibleCondition();
        if (isVisible && visCon != 0) isVisible = GUIInfoManager.GetBool(visCon, ParentID);
        //bool isVisible = GUIInfoManager.GetBool(GetVisibleCondition(), ParentID);
        if (!isVisible)
        {
          RestoreButtonsPosition();
          _first = true;
          return;
        }

        if (!_first && CheckButtonsModifiedPosition())
            _first = true;

        for (int i = 0; i < _buttons.Count; i++)
        {
          //buttons[i].UpdateVisibility();
          bool bWasvisible = _buttons[i].IsVisible;
          //if (bWasvisible)
          //  bWasvisible = _buttons[i].VisibleFromSkinCondition;

          int bVisCon = _buttons[i].GetVisibleCondition();
          bool bVisible = _buttons[i].IsVisible;
          if (bVisCon != 0)
            bVisible = GUIInfoManager.GetBool(bVisCon, _buttons[i].ParentID);
          
          if (_first && !bVisible)
          {
            if (layout.Orientation == System.Windows.Controls.Orientation.Vertical)
            {
              ShiftControlsUp(i);
            }
            else
            {
              ShiftControlsLeft(i);
            }
          }
          if (!bWasvisible && bVisible)
          {
            if (!_first)
            {
              if (layout.Orientation == System.Windows.Controls.Orientation.Vertical)
              {
                ShiftControlsDown(i);
              }
              else
              {
                ShiftControlsRight(i);
              }
            }

          }
          else if (bWasvisible && !bVisible)
          {
            if (!_first)
            {
              if (layout.Orientation == System.Windows.Controls.Orientation.Vertical)
              {
                ShiftControlsUp(i);
              }
              else
              {
                ShiftControlsLeft(i);
              }
            }

          }
        }
        _first = false;
        StoreButtonsModifiedPosition();
      }
    }

    private void ShiftControlsUp(int index)
    {
      for (int i = index; i < _buttons.Count; i++)
      {
        if (i + 1 < _buttons.Count)
        {
          _buttons[i + 1].YPosition -= _buttons[i].Height;
        }
      }
    }

    private void ShiftControlsDown(int index)
    {
      for (int i = index; i < _buttons.Count; i++)
      {
        if (i + 1 < _buttons.Count)
        {
          _buttons[i + 1].YPosition += _buttons[i].Height;
        }
      }
    }

    private void ShiftControlsRight(int index)
    {
      for (int i = index; i < _buttons.Count; i++)
      {
        if (i + 1 < _buttons.Count)
        {
          _buttons[i + 1].XPosition += _buttons[i].Width;
        }
      }
    }

    private void ShiftControlsLeft(int index)
    {
      for (int i = index; i < _buttons.Count; i++)
      {
        if (i + 1 < _buttons.Count)
        {
          _buttons[i + 1].XPosition -= _buttons[i].Width;
        }
      }
    }

    private void StoreButtonsPosition()
    {
      if (_positions == null)
      {
        _positions = new Point[_buttons.Count];
      }
      for (int i = 0; i < _buttons.Count; i++)
      {
        _positions[i].X = _buttons[i].XPosition;
        _positions[i].Y = _buttons[i].YPosition;
      }
    }

    private void RestoreButtonsPosition()
    {
      if (_positions == null)
      {
        return;
      }
      for (int i = 0; i < _positions.Length; i++)
      {
        _buttons[i].XPosition = (int)_positions[i].X;
        _buttons[i].YPosition = (int)_positions[i].Y;
      }
    }

    private void StoreButtonsModifiedPosition()
    {
        if (_modPositions == null)
        {
          _modPositions = new Point[_buttons.Count];
        }
        for (int i = 0; i < _buttons.Count; i++)
        {
          _modPositions[i].X = _buttons[i].XPosition;
          _modPositions[i].Y = _buttons[i].YPosition;
        }
    }

    private bool CheckButtonsModifiedPosition()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            if (_modPositions[i].X != _buttons[i].XPosition || _modPositions[i].Y != _buttons[i].YPosition)
            {
                return true;
            }
        }
        return false;
    }

  }
}