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

namespace MediaPortal.Animation
{
	public sealed class AnimationCollection : CollectionBase, IAddChild
	{
		#region Constructors

		public AnimationCollection()
		{
		}

		public AnimationCollection(object target)
		{
			_target = target;
		}

		internal AnimationCollection(object target, DependencyBinding binding)
		{
			_target = target;
			_binding = binding;
		}

		#endregion Constructors

		#region Methods

		public void Add(AnimationBase animation)
		{
			if(animation == null)
                throw new ArgumentNullException("animation");

			if(_target != null && _binding != null)
				animation.Attach(_target, _binding);

			List.Add(animation);
		}

		void IAddChild.AddChild(object child)
		{
			AnimationBase animation = child as AnimationBase;

			if(animation == null)
				throw new ArgumentException("child");

			List.Add(animation);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException("AnimationCollection.IAddChild.AddText");
		}

		public bool Contains(AnimationBase animation)
		{
			if(animation == null)
				throw new ArgumentNullException("animation");

			return List.Contains(animation);
		}

		public void CopyTo(AnimationBase[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public int IndexOf(AnimationBase animation)
		{
			if(animation == null)
				throw new ArgumentNullException("animation");

			return List.IndexOf(animation);
		}

		public void Insert(int index, AnimationBase animation)
		{
			if(animation == null)
				throw new ArgumentNullException("animation");

			List.Insert(index, animation);
		}

		public bool Remove(AnimationBase animation)
		{
			if(animation == null)
				throw new ArgumentNullException("animation");
			
			if(List.Contains(animation) == false)
				return false;

			List.Remove(animation);

			return true;
		}

		#endregion Methods

		#region Properties

		public AnimationBase this[int index]
		{ 
			get { return (AnimationBase)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties

		#region Fields

		DependencyBinding			_binding;
		object						_target;

		#endregion Fields
	}
}
