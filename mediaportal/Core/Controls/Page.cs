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
using System.Collections;
using System.Windows;
using System.Windows.Serialization;

using MediaPortal.Drawing;

namespace MediaPortal.Controls
{
	public class Page : FrameworkElement, INameScope, /* IWindowService, */ IAddChild
	{
		#region Constructors

		static Page()
		{
			LoadedEvent.AddOwner(typeof(Page));
		}

		public Page()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is UIElement == false)
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof(UIElement)));

			_child = (UIElement)child;
		}

		void IAddChild.AddText(string text)
		{
		}

		object INameScope.FindName(string name)
		{
			if(_names == null)
				return null;

			return _names[name];
		}

		public void RegisterName(string name, object context)
		{
			if(_names == null)
				_names = new Hashtable();

			_names[name] = context;
		}

		public void UnregisterName(string name)
		{
			if(_names == null)
				return;

			_names.Remove(name);
		}
		
		#endregion Methods

		#region Properties

		public Brush Background
		{
			get { return _background; }
			set { _background = value; }
		}

		public UIElement Child
		{
			get { return _child; }
			set { _child = value; }
		}

//		public IconData Icon { get; set; }

		protected internal override IEnumerator LogicalChildren
		{
			get { if(_logicalChildren == null) return NullEnumerator.Instance; return _logicalChildren.GetEnumerator(); }
		}

		public object StatusBarContent
		{
			get { return _statusBarContent; }
			set { _statusBarContent = value; }
		}

		public ControlTemplate Template
		{
			get { return _template; }
			set { _template = value; }
		}

		public string Text
		{
			get { return _text; }
			set { _text = value; }
		}

		public double Top
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public double WindowHeight
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public WindowState WindowState
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

		#region Fields

		Brush						_background;
		UIElement					_child;
		UIElementCollection			_logicalChildren = null;
		Hashtable					_names;
		object						_statusBarContent;
		ControlTemplate				_template;
		string						_text;

		#endregion Fields
	}
}
