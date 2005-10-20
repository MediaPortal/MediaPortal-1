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
using System.ComponentModel;
using System.Windows.Serialization;

namespace System.Windows
{
	public class Style : IAddChild, INameScope
	{
		#region Constructors

		public Style()
		{
		}

		public Style(Type type)
		{
			if(type == null)
				throw new ArgumentNullException("type");

			_type = type;
		}

		public Style(Type type, Style baseStyle)
		{
			if(type == null)
				throw new ArgumentNullException("type");

			if(baseStyle == null)
				throw new ArgumentNullException("basedOn");

			_type = type;
			_baseStyle = baseStyle;
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			if(_setters == null)
				_setters = new SetterCollection();

			_setters.Add((Setter)child);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException();
		}

		public void Apply(object o)
		{
		}

		object INameScope.FindName(string name)
		{
			return _styles[name];				
		}

		#endregion Methods

		#region Properties

		public Style BasedOn
		{
			get { return _baseStyle; }
			set { _baseStyle = value; }
		}

		public SetterCollection Setters
		{ 
			get { return _setters == null ? _setters = new SetterCollection() : _setters; }
		}

		public Type TargetType
		{ 
			get { return _type; }
			set { _type = value; }
		}

		public TriggerCollection Triggers
		{
			get { return _triggers == null ? _triggers = new TriggerCollection() : _triggers; }
		}

/*		ICollection IScenegraphGroup.Children
		{
			get { if(_children == null) _children = new ScenegraphCollection(this); return _children; }
		}

		bool IScenegraphGroup.HasChildren
		{
			get { return _children != null && _children.Count != 0; }
		}

		bool IScenegraphElement.HasParents
		{
			get { return _parents != null && _parents.Count != 0; }
		}

		ICollection IScenegraphElement.Parents
		{
			get { if(_parents == null) _parents = new ScenegraphCollection(this); return _parents; }
		}

		protected ICollection ScenegraphChildren
		{
			get { if(_children == null) _children = new ScenegraphCollection(this); return _children; }
		}
*/
		#endregion Properties

		#region Fields

		Style						_baseStyle;
//		ScenegraphCollection		_children;
//		ScenegraphCollection		_parents;
		SetterCollection			_setters;
		static Hashtable			_styles = new Hashtable();
		TriggerCollection			_triggers;
		Type						_type;

		#endregion Fields
	}
}
