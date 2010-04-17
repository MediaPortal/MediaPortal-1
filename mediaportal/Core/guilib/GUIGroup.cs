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

      Children.SafeDispose();
      
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
    }

    protected override Size ArrangeOverride(Rect finalRect)
    {
      this.Location = finalRect.Location;
      this.Size = finalRect.Size;

      if (_layout == null)
      {
        return this.Size;
      }

      _layout.Arrange(this);

      return finalRect.Size;
    }

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

    protected override Size MeasureOverride(Size availableSize)
    {
      if (_layout == null)
      {
        return Size.Empty;
      }

      _layout.Measure(this, this.Size);

      return this.Size = _layout.Size;
    }

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

    public UIElementCollection Children
    {
      get
      {
        if (_children == null)
        {
          _children = new UIElementCollection();
        }
        return _children;
      }
    }

    #endregion Properties

    #region Fields

    private Animator _animator;
    private int _beginInitCount = 0;
    private UIElementCollection _children;

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
  }
}