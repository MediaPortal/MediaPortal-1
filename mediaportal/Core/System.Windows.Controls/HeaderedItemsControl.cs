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

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Controls
{
  public class HeaderedItemsControl : ItemsControl
  {
    #region Constructors

    static HeaderedItemsControl()
    {
      HasHeaderProperty = DependencyProperty.Register("HasHeader", typeof (bool), typeof (HeaderedItemsControl),
                                                      new FrameworkPropertyMetadata());
      HeaderProperty = DependencyProperty.Register("Header", typeof (object), typeof (HeaderedItemsControl),
                                                   new FrameworkPropertyMetadata());
      HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof (DataTemplate),
                                                           typeof (HeaderedItemsControl),
                                                           new FrameworkPropertyMetadata());
      HeaderTemplateSelectorProperty = DependencyProperty.Register("HeaderTemplateSelector",
                                                                   typeof (DataTemplateSelector),
                                                                   typeof (HeaderedItemsControl),
                                                                   new FrameworkPropertyMetadata());
    }

    public HeaderedItemsControl() {}

    #endregion Constructors

    #region Methods

    protected virtual void OnHeaderChanged(object oldHeader, object newHeader) {}

    protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate) {}

    protected virtual void OnHeaderTemplateSelectorChanged(DataTemplateSelector oldHeaderTemplateSelector,
                                                           DataTemplateSelector newHeaderTemplateSelector) {}

    public override string ToString()
    {
      return base.ToString();
    }

    #endregion Methods

    #region Properties

    [Bindable(false)]
    public bool HasHeader
    {
      get { return (bool)GetValue(HasHeaderProperty); }
    }

    [Bindable(true)]
    public object Header
    {
      get { return GetValue(HeaderProperty); }
      set { SetValue(HeaderProperty, value); }
    }

    [Bindable(true)]
    public DataTemplate HeaderTemplate
    {
      get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
      set { SetValue(HeaderTemplateProperty, value); }
    }

    [Bindable(true)]
    public DataTemplateSelector HeaderTemplateSelector
    {
      get { return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty); }
      set { SetValue(HeaderTemplateSelectorProperty, value); }
    }

    protected internal override IEnumerator LogicalChildren
    {
      get { return null; }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty HasHeaderProperty;
    public static readonly DependencyProperty HeaderProperty;
    public static readonly DependencyProperty HeaderTemplateProperty;
    public static readonly DependencyProperty HeaderTemplateSelectorProperty;

    #endregion Properties (Dependency)
  }
}