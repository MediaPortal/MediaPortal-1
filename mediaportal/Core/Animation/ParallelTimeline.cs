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
	public class ParallelTimeline : TimelineGroup
	{
		#region Constructors

		public ParallelTimeline()
		{
		}

		public ParallelTimeline(TimeSpan beginTime)
		{
		}

		public ParallelTimeline(TimeSpan beginTime, Duration duration)
		{
		}

		protected internal ParallelTimeline(ParallelTimeline timeline, CloneType cloneType)
		{
		}

		public ParallelTimeline(TimeSpan beginTime, Duration duration, RepeatBehavior repeatBehavior)
		{
		}

		#endregion Constructors

		#region Methods

		protected override Freezable CreateInstanceCore()
		{
			return new ParallelTimeline();
		}

		public new ParallelTimeline Copy()
		{
			return (ParallelTimeline)base.Copy();
		}

		protected override void CopyCore(Freezable sourceFreezable)
		{
		}

		#endregion Methods
	}
}
