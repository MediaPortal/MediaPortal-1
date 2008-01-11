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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using MediaPortal.Drawing;

namespace System.Windows.Controls
{
	public class Control : FrameworkElement
	{
		#region Constructors

		static Control()
		{
			FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata();

			#region Background

			metadata.AffectsArrange = true;
			metadata.AffectsMeasure = true;
			metadata.AffectsParentArrange = true;
			metadata.AffectsParentMeasure = true;
			metadata.AffectsRender = true;

			BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(Control), metadata);

			#endregion Background

//			BorderBrushProperty;
//			BorderThicknessProperty;
//			FontFamilyProperty;
//			FontSizeProperty;
//			FontStretchProperty;
//			FontStyleProperty;
//			FontWeightProperty;

			#region Focusable

			FocusableProperty.OverrideMetadata(typeof(Control), new FrameworkPropertyMetadata(true));

			#endregion Focusable

//			ForegroundProperty;

			#region HozitonalContentAlignment

			metadata = new FrameworkPropertyMetadata();
			metadata.DefaultValue = HorizontalAlignment.Stretch;
			metadata.AffectsArrange = true;

			HorizontalContentAlignmentProperty = DependencyProperty.Register("HozitonalContentAlignment", typeof(HorizontalAlignment), typeof(Control), metadata);

			#endregion HozitonalContentAlignment

			#region IsTabStop

			metadata = new FrameworkPropertyMetadata();
			metadata.DefaultValue = true;

			IsTabStopProperty = DependencyProperty.Register("IsTabStop", typeof(bool), typeof(Control), metadata);

			#endregion IsTabStop

			#region Padding

			metadata = new FrameworkPropertyMetadata();
			metadata.DefaultValue = Thickness.Empty;
			metadata.AffectsArrange = true;
			metadata.AffectsMeasure = true;
			metadata.AffectsParentArrange = true;
			metadata.AffectsParentMeasure = true;

			PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(Control), metadata);

			#endregion Padding

			#region TabIndex

			metadata = new FrameworkPropertyMetadata();
			metadata.DefaultValue = 1;
			metadata.AffectsArrange = true;
			metadata.AffectsMeasure = true;
			metadata.AffectsParentArrange = true;
			metadata.AffectsParentMeasure = true;

			TabIndexProperty = DependencyProperty.Register("Padding", typeof(int), typeof(Control), metadata);

			#endregion TabIndex

			#region Template

			metadata = new FrameworkPropertyMetadata();

			TemplateProperty = DependencyProperty.Register("Template", typeof(ControlTemplate), typeof(Control), metadata);

			#endregion Template

//			TextDecorationsProperty;
//			TextTrimmingProperty;

			#region VerticalContentAlignment

			metadata = new FrameworkPropertyMetadata();
			metadata.DefaultValue = VerticalAlignment.Stretch;
			metadata.AffectsArrange = true;

			VerticalContentAlignmentProperty = DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(Control), metadata);

			#endregion HozitonalContentAlignment
		}

		public Control()
		{
		}

		#endregion Constructors

		#region Events

		[BindableAttribute(true)] 
		public Brush Background
		{
			get { return GetValue(BackgroundProperty) as Brush; }
			set { SetValue(BackgroundProperty, value); }
		}

		public event MouseButtonEventHandler MouseDoubleClick
		{
			add		{ AddHandler(MouseDoubleClickEvent, value); }
			remove	{ RemoveHandler(MouseDoubleClickEvent, value); }
		}

		public event MouseButtonEventHandler PreviewMouseDoubleClick
		{
			add		{ AddHandler(PreviewMouseDoubleClickEvent, value); }
			remove	{ RemoveHandler(PreviewMouseDoubleClickEvent, value); }
		}

		#endregion Events

		#region Events (Routed)

		public static readonly RoutedEvent MouseDoubleClickEvent;
		public static readonly RoutedEvent PreviewMouseDoubleClickEvent;

		#endregion Events (Routed)

		#region Methods

		protected override Size ArrangeOverride(Rect finalRect)
		{
			return base.ArrangeOverride (finalRect);
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride (availableSize);
		}

		protected virtual void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
		}

		protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
		{
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
		}

		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
		}
			
		protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
		{
		}
		
		public override string ToString()
		{
			return base.ToString();
		}
			
		#endregion Methods

		#region Properties

		[BindableAttribute(true)] 
		public HorizontalAlignment HorizontalContentAlignment
		{
			get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
			set { SetValue(HorizontalContentAlignmentProperty, value); }
		}

		[BindableAttribute(true)] 
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}

		[BindableAttribute(true)] 
		public bool IsTabStop
		{
			get { return (bool)GetValue(IsTabStopProperty); }
			set { SetValue(IsTabStopProperty, value); }
		}

		[BindableAttribute(true)] 
		public int TabIndex
		{
			get { return (int)GetValue(TabIndexProperty); }
			set { SetValue(TabIndexProperty, value); }
		}

		public ControlTemplate Template
		{
			get { return (ControlTemplate)GetValue(TemplateProperty); }
			set { SetValue(TemplateProperty, value); }
		}

		[BindableAttribute(true)] 
		public VerticalAlignment VerticalContentAlignment
		{
			get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
			set { SetValue(VerticalContentAlignmentProperty, value); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty BackgroundProperty;
//		public static readonly DependencyProperty BorderBrushProperty;
//		public static readonly DependencyProperty BorderThicknessProperty;
//		public static readonly DependencyProperty FontFamilyProperty;
//		public static readonly DependencyProperty FontSizeProperty;
//		public static readonly DependencyProperty FontStretchProperty;
//		public static readonly DependencyProperty FontStyleProperty;
//		public static readonly DependencyProperty FontWeightProperty;
//		public static readonly DependencyProperty ForegroundProperty;
		public static readonly DependencyProperty HorizontalContentAlignmentProperty;
		public static readonly DependencyProperty IsTabStopProperty;
		public static readonly DependencyProperty PaddingProperty;
		public static readonly DependencyProperty TabIndexProperty;
		public static readonly DependencyProperty TemplateProperty;
//		public static readonly DependencyProperty TextDecorationsProperty;
//		public static readonly DependencyProperty TextTrimmingProperty;
		public static readonly DependencyProperty VerticalContentAlignmentProperty;

		#endregion Properties (Dependency)
	}
}
