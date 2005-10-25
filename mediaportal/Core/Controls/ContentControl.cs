#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Windows;
using System.Windows.Serialization;

namespace MediaPortal.Controls
{
	public class ContentControl : Control, IAddChild
	{
		#region Constructors

		static ContentControl()
		{
			ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ContentControl), new PropertyMetadata(new PropertyInvalidatedCallback(ContentChanged)));
			ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ContentControl), new PropertyMetadata(new PropertyInvalidatedCallback(ContentTemplateChanged)));
			ContentTemplateSelectorProperty = DependencyProperty.Register("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(ContentControl), new PropertyMetadata(new PropertyInvalidatedCallback(ContentTemplateSelectorChanged)));
			HasContentProperty = DependencyProperty.Register("HasContent", typeof(bool), typeof(ContentControl), new PropertyMetadata(false));
		}

		public ContentControl()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			AddChild(child);
		}

		protected virtual void AddChild(object child)
		{
		}		

		void IAddChild.AddText(string text)
		{
			AddText(text);
		}

		protected virtual void AddText(string text)
		{
		}

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

		protected virtual void OnContentChanged(object oldContent, object newContent)
		{
		}

		protected virtual void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
		{
		}

		protected virtual void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
		{
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

		#region Properties (Dependency)

		public static readonly DependencyProperty ContentProperty;
		public static readonly DependencyProperty ContentTemplateProperty;
		public static readonly DependencyProperty ContentTemplateSelectorProperty;
		public static readonly DependencyProperty HasContentProperty;

		#endregion Properties (Dependency)
	}
}
