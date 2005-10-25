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

namespace MediaPortal.Animation
{
	public abstract class Animatable : Freezable, IAnimatable
	{
		#region Constructors

		protected Animatable()
		{
		}

		#endregion Constructors

		#region Methods

		public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock)
		{
		}

		public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior)
		{
		}

		public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
		{
		}

		public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
		{
		}

		public new Animatable Copy()
		{
			return (Animatable)base.Copy();
		}

		protected virtual void CopyCurrentValueCore(Animatable sourceAnimatable)
		{
			throw new NotImplementedException();
		}

		protected override bool FreezeCore(bool isChecking)
		{
			throw new NotImplementedException();
		}

		public object GetAnimationBaseValue(DependencyProperty dp)
		{
			throw new NotImplementedException();
		}

		protected override object GetValueCore(DependencyProperty dp, object baseValue, PropertyMetadata metadata)
		{
			return null;
		}

		#endregion Methods

		#region Properties

		public bool HasAnimatedProperties
		{
			get { throw new NotImplementedException(); }
		}

		#endregion Properties
	}
}
