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
using System.Windows.Controls;
using System.Windows.Serialization;

namespace System.Windows
{
  public class ContentControl : Control, IAddChild
  {
    #region Constructors

    static ContentControl()
    {
      FrameworkPropertyMetadata metadata;

      #region Content

      metadata = new FrameworkPropertyMetadata();
      metadata.GetValueOverride = new GetValueOverride(OnContentPropertyGetValue);
      metadata.PropertyInvalidatedCallback = new PropertyInvalidatedCallback(OnContentPropertyInvalidated);

      ContentProperty = DependencyProperty.Register("Content", typeof (object), typeof (ContentControl), metadata);

      #endregion Content

      #region ContentTemplate

      metadata = new FrameworkPropertyMetadata();

      ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof (DataTemplate),
                                                            typeof (ContentControl), metadata);

      #endregion ContentTemplate

      #region ContentTemplateSelector

      metadata = new FrameworkPropertyMetadata();

      ContentTemplateSelectorProperty = DependencyProperty.Register("ContentTemplateSelector",
                                                                    typeof (DataTemplateSelector),
                                                                    typeof (ContentControl), metadata);

      #endregion ContentTemplateSelector

      #region HasContent

      metadata = new FrameworkPropertyMetadata();
      metadata.GetValueOverride = new GetValueOverride(OnHasContentPropertyGetValue);
      metadata.SetReadOnly();

      HasContentProperty = DependencyProperty.Register("HasContent", typeof (bool), typeof (ContentControl), metadata);

      #endregion HasContent
    }

    public ContentControl() {}

    #endregion Constructors

    #region Methods

    void IAddChild.AddChild(object child)
    {
      AddChild(child);
    }

    protected virtual void AddChild(object child) {}

    void IAddChild.AddText(string text)
    {
      AddText(text);
    }

    protected virtual void AddText(string text) {}

    protected static void ContentChanged(DependencyObject d)
    {
      ((ContentControl)d).OnContentChanged(null, null);
    }

    protected static void ContentTemplateChanged(DependencyObject d)
    {
      ((ContentControl)d).OnContentTemplateChanged(null, null);
    }

    protected static void ContentTemplateSelectorChanged(DependencyObject d)
    {
      ((ContentControl)d).OnContentTemplateChanged(null, null);
    }

    public static object GetContent(DependencyObject d)
    {
      return d.GetValue(ContentProperty);
    }

    public static DataTemplateSelector GetContentTemplateSelector(DependencyObject d)
    {
      return (DataTemplateSelector)d.GetValue(ContentTemplateProperty);
    }

    protected virtual void OnContentChanged(object oldContent, object newContent) {}

    protected virtual void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate) {}

    protected virtual void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector,
                                                            DataTemplateSelector newContentTemplateSelector) {}

    private static object OnContentPropertyGetValue(DependencyObject d)
    {
      return ((ContentControl)d).Content;
    }

    private static void OnContentPropertyInvalidated(DependencyObject d)
    {
      ((ContentControl)d)._contentDirty = true;
    }

    private static object OnHasContentPropertyGetValue(DependencyObject d)
    {
      return ((ContentControl)d).HasContent;
    }

    public static void SetContent(DependencyObject d, object content)
    {
      d.SetValue(ContentProperty, content);
    }

    public static void SetContentTemplate(DependencyObject d, DataTemplate contentTemplate)
    {
      d.SetValue(ContentTemplateProperty, contentTemplate);
    }

    public static void SetContentTemplateSelector(DependencyObject d, DataTemplateSelector dataTemplateSelector)
    {
      d.SetValue(ContentTemplateSelectorProperty, dataTemplateSelector);
    }

    #endregion Methods

    #region Properties

    [Bindable(true)]
    public object Content
    {
      get
      {
        if (_contentDirty)
        {
          _contentCache = GetValueBase(ContentProperty);
          _contentDirty = false;
        }
        return _contentCache;
      }
      set { SetValue(ContentProperty, value); }
    }

    [Bindable(true)]
    public DataTemplate ContentTemplate
    {
      get { return GetValue(ContentTemplateProperty) as DataTemplate; }
      set { SetValue(ContentTemplateProperty, value); }
    }

    [Bindable(true)]
    public DataTemplateSelector ContentTemplateSelector
    {
      get { return GetValue(ContentTemplateSelectorProperty) as DataTemplateSelector; }
      set { SetValue(ContentTemplateSelectorProperty, value); }
    }

    public bool HasContent
    {
      get { return Content != null; }
    }

    protected internal override IEnumerator LogicalChildren
    {
      get { return NullEnumerator.Instance; }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty ContentProperty;
    public static readonly DependencyProperty ContentTemplateProperty;
    public static readonly DependencyProperty ContentTemplateSelectorProperty;
    public static readonly DependencyProperty HasContentProperty;

    #endregion Properties (Dependency)

    #region Fields

    private object _contentCache;
    private bool _contentDirty;

    #endregion Fields
  }
}