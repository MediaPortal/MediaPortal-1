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
using Color=System.Drawing.Color;

namespace MediaPortal.Drawing
{
	public class LinearGradientBrush : GradientBrush
	{
		#region Constructors

		public LinearGradientBrush()
		{
		}

		public LinearGradientBrush(GradientStopCollection gradientStops) : base(gradientStops)
		{
		}

		public LinearGradientBrush(GradientStopCollection gradientStops, double angle) : base(gradientStops)
		{
			throw new NotImplementedException();
		}

		public LinearGradientBrush(Color startColor, Color endColor, double angle)
		{
			_startColor = startColor;
			_endColor = endColor;

			throw new NotImplementedException();
		}

		public LinearGradientBrush(GradientStopCollection gradientStops, Point startPoint, Point endPoint) : base(gradientStops)
		{
			_startPoint = startPoint;
			_endPoint = endPoint;
		}

		public LinearGradientBrush(Color startColor, Color endColor, Point startPoint, Point endPoint)
		{
			_startColor = startColor;
			_endColor = endColor;

			_startPoint = startPoint;
			_endPoint = endPoint;
		}

		#endregion Constructors

		#region Properties

		public Point EndPoint
		{
			get { return _endPoint; }
			set
			{
				value.X = Math.Max(0, Math.Min(1, value.X));
				value.Y = Math.Max(0, Math.Min(1, value.Y));
				
				if(Point.Equals(_endPoint, value))
				{
					_endPoint = value;
					RaiseChanged();
				}
			}
		}

		public Point StartPoint
		{ 
			get { return _startPoint; }
			set
			{
				value.X = Math.Max(0, Math.Min(1, value.X));
				value.Y = Math.Max(0, Math.Min(1, value.Y));
				
				if(Point.Equals(_startPoint, value))
				{
					_startPoint = value;
					RaiseChanged();
				}
			}
		}

		#endregion Properties

		#region Fields

		Color						_endColor = Color.White;
		Point						_endPoint = Point.Empty;
		Color						_startColor = Color.Black;
		Point						_startPoint = Point.Empty;
			
		#endregion Fields
	}
}
