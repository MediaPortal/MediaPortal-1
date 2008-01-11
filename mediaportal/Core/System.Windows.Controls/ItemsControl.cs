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
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Serialization;

namespace System.Windows.Controls
{
	public class ItemsControl : Control, IAddChild //, IGeneratorHost
	{
		#region Constructors

		static ItemsControl()
		{
			HasItemsProperty = DependencyProperty.Register("HasItems", typeof(bool), typeof(ItemsControl), new PropertyMetadata(false));
		}

		public ItemsControl()
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
			if(_items == null)
				_items = new ItemCollection();

			_items.Add(child);
		}

		void IAddChild.AddText(string text)
		{
			AddText(text);
		}

		protected virtual void AddText(string text)
		{
			// no default implementation
		}

		public override void BeginInit()
		{
		}

		public override void EndInit()
		{
		}

		protected virtual DependencyObject GetContainerForItemOverride(object item)
		{
			throw new NotImplementedException();
		}
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
		}
			
		protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
		}
			
		protected virtual bool IsItemItsOwnContainerOverride(object item)
		{
			throw new NotImplementedException();
		}
			
		#endregion Methods

		#region Properties

		public bool HasItems
		{
			get { return (bool)GetValue(HasItemsProperty); }
			set { SetValue(HasItemsProperty, value); }
		}

		[BindableAttribute(true)] 
		public ItemCollection Items
		{
			get { if(_items == null) _items = new ItemCollection(); return _items; }
		}

		protected internal override IEnumerator LogicalChildren
		{
			get { if(_items == null) return NullEnumerator.Instance; return _items.GetEnumerator(); }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty HasItemsProperty;

		#endregion Properties (Dependency)

		#region Fields

		ItemCollection				_items;

		#endregion Fields
	}
}
