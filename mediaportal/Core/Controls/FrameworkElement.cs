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

using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;
using MediaPortal.GUI.Library;

namespace MediaPortal.Controls
{
	public class FrameworkElement : UIElement
	{
		#region Constructors

		public FrameworkElement()
		{
		}

		#endregion Constructors

		#region Methods

		protected override sealed void ArrangeCore(Rect finalRect)
		{
			ArrangeOverride(finalRect);
		}

		// TODO: finalRect is wrong, this should be using Size finalSize
		protected virtual Size ArrangeOverride(Rect finalRect)
		{
			_location = finalRect.Location;
			_width = finalRect.Width;
			_height = finalRect.Height;

			return finalRect.Size;
		}

		protected override sealed Size MeasureCore(Size availableSize)
		{
			return MeasureOverride(availableSize);
		}

		protected virtual Size MeasureOverride(Size availableSize)
		{
			return new Size(_width, _height);
		}

		#endregion Methods

		#region Properties

		public virtual Point Location
		{
			get { return _location; }
			set { _location = value; }
		}

		// TODO: should not be virtual and must be double
		public virtual int Height
		{
			get { return (int)_height; }
			set { _height = value; }
		}

		public HorizontalAlignment HorizontalAlignment
		{
			get { return _horizontalAlignment; }
			set { _horizontalAlignment = value; }
		}

		// TODO: this needs to be IEnumerator LogicalChildren
		public GUIControlCollection LogicalChildren
		{
			get { return _logicalChildren; }
		}

		public Thickness Margin
		{
			get { return _margin; }
			set { _margin = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		
		public ResourceDictionary Resources
		{
			get { if(_resources == null) _resources = new ResourceDictionary(); return _resources; }
			set { _resources = value; }
		}

		public Style Style
		{
			get { return _style; }
			set { _style = value; }
		}

		public TriggerCollection Triggers
		{
			get { if(_triggers == null) _triggers = new TriggerCollection(); return _triggers; }
		}

		public VerticalAlignment VerticalAlignment
		{
			get { return _verticalAlignment; }
			set { _verticalAlignment = value; }
		}

		// TODO: should not be virtual and must be double
		public virtual int Width
		{
			get { return (int)_width; }
			set { _width = value; }
		}

		#endregion Properties

		#region Fields

		double						_height;
		HorizontalAlignment			_horizontalAlignment;
		Point						_location = Point.Empty;
		GUIControlCollection		_logicalChildren = new GUIControlCollection();
		Thickness					_margin;
		string						_name = string.Empty;
		Style						_style;
		ResourceDictionary			_resources;
		TriggerCollection			_triggers;
		VerticalAlignment			_verticalAlignment;
		double						_width;

		#endregion Fields
	}
}
