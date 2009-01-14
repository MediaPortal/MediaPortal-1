#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System.Collections;
using System.ComponentModel;
using System.Windows.Serialization;
using MediaPortal.Drawing;

namespace System.Windows.Controls
{
  public class Page : FrameworkElement, INameScope, /* IWindowService, */ IAddChild
  {
    #region Constructors

    static Page()
    {
      BackgroundProperty = DependencyProperty.Register("Background", typeof (Brush), typeof (Page));
      ContentProperty = DependencyProperty.Register("Content", typeof (object), typeof (Page));
//			FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(Page));
      FontSizeProperty = DependencyProperty.Register("FontSize", typeof (double), typeof (Page));
      ForegroundProperty = DependencyProperty.Register("Foreground", typeof (Brush), typeof (Page));
      KeepAliveProperty = DependencyProperty.Register("KeepAlive", typeof (bool), typeof (Page));
      TemplateProperty = DependencyProperty.Register("Template", typeof (ControlTemplate), typeof (Page));
      TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (Page));

      LoadedEvent.AddOwner(typeof (Page));
    }

    public Page()
    {
    }

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

//			_child = (UIElement)child;
    }

    void IAddChild.AddText(string text)
    {
    }

    protected override Size ArrangeOverride(Rect finalRect)
    {
      return base.ArrangeOverride(finalRect);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      return base.MeasureOverride(availableSize);
    }

    protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
    {
    }

    protected internal override void OnVisualParentChanged(Visual oldParent)
    {
      base.OnVisualParentChanged(oldParent);
    }

    object INameScope.FindName(string name)
    {
      if (_names == null)
      {
        return null;
      }

      return _names[name];
    }

    public void RegisterName(string name, object context)
    {
      if (_names == null)
      {
        _names = new Hashtable();
      }

      _names[name] = context;
    }

    public void UnregisterName(string name)
    {
      if (_names == null)
      {
        return;
      }

      _names.Remove(name);
    }

    #endregion Methods

    #region Properties

    public Brush Background
    {
      get { return (Brush) GetValue(BackgroundProperty); }
      set { SetValue(BackgroundProperty, value); }
    }

    public object Content
    {
      get { return GetValue(ContentProperty); }
      set { SetValue(ContentProperty, value); }
    }

//		[BindableAttribute(true)] 
//		public FontFamily FontFamily
//		{
//			get { return (FontFamily)GetValue(FontFamilyProperty); }
//			set { SetValue(FontFamilyProperty, value); }
//		}

    [Bindable(true)]
    public double FontSize
    {
      get { return (double) GetValue(FontSizeProperty); }
      set { SetValue(FontSizeProperty, value); }
    }

    [Bindable(true)]
    public Brush Foreground
    {
      get { return (Brush) GetValue(ForegroundProperty); }
      set { SetValue(ForegroundProperty, value); }
    }

    public bool KeepAlive
    {
      get { return (bool) GetValue(KeepAliveProperty); }
      set { SetValue(KeepAliveProperty, value); }
    }

    protected internal override IEnumerator LogicalChildren
    {
      get
      {
        if (_logicalChildren == null)
        {
          return NullEnumerator.Instance;
        }
        return _logicalChildren.GetEnumerator();
      }
    }

//		public NavigationService NavigationService
//		{
//			get;
//		}

    public ControlTemplate Template
    {
      get { return (ControlTemplate) GetValue(TemplateProperty); }
      set { SetValue(TemplateProperty, value); }
    }

    public string Title
    {
      get { return (string) GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public double WindowHeight
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public string WindowTitle
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public double WindowWidth
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty BackgroundProperty;
    public static readonly DependencyProperty ContentProperty;
    public static readonly DependencyProperty FontFamilyProperty;
    public static readonly DependencyProperty FontSizeProperty;
    public static readonly DependencyProperty ForegroundProperty;
    public static readonly DependencyProperty KeepAliveProperty;
    public static readonly DependencyProperty TemplateProperty;
    public static readonly DependencyProperty TitleProperty;

    #endregion Properties (Dependency)

    #region Fields

    private UIElementCollection _logicalChildren = null;
    private Hashtable _names = new Hashtable(20);

    #endregion Fields
  }
}