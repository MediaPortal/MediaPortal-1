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
using Color=System.Drawing.Color;

namespace MediaPortal.Drawing
{
	public sealed class GradientStop
	{
		#region Constructors

		public GradientStop()
		{
		}

		public GradientStop(Color color, double offset)
		{
			_color = color;
			_offset = offset;
		}

		#endregion Constructors

		#region Properties

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}

		public double Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		public byte[] color
		{
			get { return _xolor; }
		}

		byte[] _xolor = new byte[4];
		
		#endregion Properties

		#region Fields

		Color						_color;
		double						_offset;

		#endregion Fields
	}
}
