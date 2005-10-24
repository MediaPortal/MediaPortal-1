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
using System.Windows.Serialization;

using MediaPortal.Drawing;
using MediaPortal.GUI.Library;

namespace MediaPortal.Controls
{
	public abstract class Panel : FrameworkElement, IAddChild
	{
		#region Constructors

		public Panel()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			UIElement element = child as UIElement;

			if(element == null)
				throw new ArgumentException("child");

			Children.Add(element);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException("Panel.IAddChild.AddText");
		}

//		protected virtual UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)

//		public static Brush GetBackground(DependencyObject d)

//		protected virtual void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
			
//		protected override void OnRender(DrawingContext dc)

//		public static void SetBackground(DependencyObject d, Brush background);

		#endregion Methods

		#region Properties

		public Brush Background
		{
			get { return _background; }
			set { _background = value; }
		}

		public UIElementCollection Children
		{
			get { if(_children == null) _children = new UIElementCollection(); return _children; }
		}

//		protected internal UIElementCollection InternalChildren
//		{
//			get { return _children; }
//		}

//		[BindableAttribute(false)] 
//		public bool IsItemsHost { get; set; }

//		protected internal override UIElementCollection LogicalChildren
//		{
//			get { return _children; }
//		}

		#endregion Properties

		#region Properties (Dependency)

//		public static readonly DependencyProperty BackgroundProperty

//		public static readonly DependencyProperty IsItemsHostProperty

		#endregion Properties (Dependency)

		#region Fields

		Brush						_background;
		UIElementCollection			_children;

		#endregion Fields
	}
}
