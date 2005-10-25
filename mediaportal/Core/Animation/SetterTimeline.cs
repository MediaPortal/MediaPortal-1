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
	public class SetterTimeline : ParallelTimeline
	{
		#region Constructors

		public SetterTimeline()
		{
			_targetName = string.Empty;
		}

		protected SetterTimeline(SetterTimeline timeline, CloneType cloneType) : base(timeline, cloneType)
		{
		}

		public SetterTimeline(string targetName, PropertyPath path, object value)
		{
			_targetName = targetName;
			_value = value;
		}

		#endregion Constructors

		#region Methods

		protected internal override Clock AllocateClock()
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Properties

		public string TargetName
		{
			get { return _targetName; }
			set { if(string.Compare(_targetName, value) != 0) { _targetName = value; } }
		}

		public object Value
		{
			get { return _value; }
			set { _value = value; }
		}

		#endregion Properties

		#region Fields

		string						_targetName = string.Empty;
		object						_value;

		#endregion Fields
	}
}
