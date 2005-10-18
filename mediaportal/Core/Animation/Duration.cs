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
using System.ComponentModel;

namespace MediaPortal.Animation
{
	[TypeConverter(typeof(DurationConverter))]
	public class Duration
	{
		#region Constructors

		public Duration()
		{
		}

		public Duration(double duration)
		{
			_duration = duration;
		}

		#endregion Constructors

		#region Methods

		public static Duration Parse(string text)
		{
			if(string.Compare(text, "Automatic", true) == 0)
				return Duration.Automatic;

			return new Duration(TimeSpan.Parse(text).TotalMilliseconds);
		}

		#endregion Methods

		#region Operators
        
		public static implicit operator double(Duration duration) 
		{
			return duration._duration;
		}

		#endregion Operators

		#region Fields

		double _duration;

		public static readonly Duration Automatic = new Duration();

		#endregion Fields
	}
}
