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

namespace MediaPortal.Animation
{
	public abstract class AnimationTimeline : Timeline
	{	
		#region Constructors

		protected AnimationTimeline()
		{
		}

		protected AnimationTimeline(AnimationTimeline timeline, CloneType cloneType) : base(timeline, cloneType)
		{
		}

		#endregion Constructors

		#region Methods

		public new AnimationTimeline Copy()
		{
			return (AnimationTimeline)base.Copy();
		}

		protected internal override Clock AllocateClock()
		{
			return new AnimationClock(this);
		}

		public new AnimationClock CreateClock()
		{
			return (AnimationClock)base.CreateClock();
		}

		protected override void CopyCurrentValueCore(Animatable sourceAnimatable)
		{
		}
			
		protected override Duration GetNaturalDurationCore(Clock clock)
		{
			return base.GetNaturalDurationCore(clock);
		}
			
		#endregion Methods

		#region Properties

		public abstract Type BaseValueType
		{
			get;
		}

		public bool UsesBaseValue
		{
			get { return UsesBaseValueCore; }
		}

		protected abstract bool UsesBaseValueCore
		{
			get;
		}

		#endregion Properties
	}
}
