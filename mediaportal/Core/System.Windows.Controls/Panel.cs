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

using System.ComponentModel;
using System.Windows.Serialization;
using MediaPortal.Drawing;

namespace System.Windows.Controls
{
  public abstract class Panel : FrameworkElement, IAddChild
  {
    #region Constructors

    static Panel()
    {
      BackgroundProperty = DependencyProperty.Register("Background", typeof (Brush), typeof (Panel));
      IsItemsHostProperty = DependencyProperty.Register("IsItemsHostProperty", typeof (bool), typeof (Panel),
                                                        new PropertyMetadata(false));
    }

    public Panel() {}

    #endregion Constructors

    #region Methods

    void IAddChild.AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is UIElement == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (UIElement)));
      }

      Children.Add((UIElement)child);
    }

    void IAddChild.AddText(string text) {}

//		protected virtual UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)

    public static Brush GetBackground(DependencyObject d)
    {
      return (Brush)d.GetValue(BackgroundProperty);
    }

//		protected virtual void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)

    protected override void OnRender(DrawingContext dc) {}

    public static void SetBackground(DependencyObject d, Brush background)
    {
      d.SetValue(BackgroundProperty, background);
    }

    #endregion Methods

    #region Properties

    public Brush Background
    {
      get { return (Brush)GetValue(BackgroundProperty); }
      set { SetValue(BackgroundProperty, value); }
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

    [Bindable(false)]
    public bool IsItemsHost
    {
      get { return (bool)GetValue(IsItemsHostProperty); }
      set { SetValue(IsItemsHostProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty BackgroundProperty;
    public static readonly DependencyProperty IsItemsHostProperty;

    #endregion Properties (Dependency)

    #region Fields

    private UIElementCollection _children;

    #endregion Fields
  }
}