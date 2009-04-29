#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MediaPortal.Drawing
{
  public class GeometryConverter : TypeConverter
  {
    #region Methods

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
    {
      if (t == typeof (string))
      {
        return true;
      }

      if (t == typeof (Geometry))
      {
        return true;
      }

      return base.CanConvertFrom(context, t);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is string)
      {
        return ConvertFromString(context, culture, (string) value);
      }

      if (value is Geometry)
      {
        return value;
      }

      return base.ConvertFrom(context, culture, value);
    }

//		private object ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text)
//		{
//			return null;
/*			PathSegmentCollection segments = new PathSegmentCollection();
			Point point = Point.Empty;
			ArrayList array = new ArrayList();
			Point start = Point.Empty;

			Polygon polygon = new Polygon();

			foreach(string segment in _pathRegex.Split(data))
			{
				if(segment == string.Empty)
					continue;

				double[] args = null;
				Point[] points = null;
				bool isClosed = false;

				switch(segment[0])
				{
					case 'A':
					case 'a':
						args = ParseParameters(segment, 7, "expecting 'size, rotationAngle isLargeArc sweepDirection point'");
						points = CreateArc(ref point, args[0], args[1], args[2], args[3] != 0, (SweepDirection)args[4], args[5], args[6]);
						break;
					case 'C':
					case 'c':
						args = ParseParameters(segment, 6, "expecting 'controlPoint1, controlPoint2, endPoint'");
						points = CreateBezier(ref point, args[0], args[1], args[2], args[3], args[4], args[5]);
						break;
					case 'L':
					case 'l':
						args = ParseParameters(segment, 2, "expecting 'x, y'");
						//						polygon.LineTo(args[0], args[1]);
						break;
					case 'H':
					case 'h':
						args = ParseParameters(segment, 1, "expecting 'x'");
						point = (points = new Point[1] { new Point(args[0], point.Y) })[0];
						break;
					case 'M':
					case 'm':
						args = ParseParameters(segment, 2, "expecting 'x, y'");
						start = point = (points = new Point[1] { new Point(args[0], args[1]) })[0];
						//						polygon.MoveTo(args[0], args[1]);
						break;
					case 'Q':
					case 'q':
						args = ParseParameters(segment, 4, "expecting 'controlPoint, endPoint'");
						points = CreateBezier(ref point, args[0], args[1], args[2], args[3]);
						break;
					case 'V':
					case 'v':
						args = ParseParameters(segment, 1, "expecting 'y'");
						point = (points = new Point[1] { new Point(point.X, args[0]) })[0];
						break;
					case 'Z':
					case 'z':
						//						polygon.ClosePath();
						break;
				}

				if(points != null)
				{
					foreach(Point p in points)
						array.Add(p);
				}

				if(isClosed)
					array.Add(start == Point.Empty ? array.Count > 0 ? array[0] : start : start);
			}

			return (Point[])array.ToArray(typeof(Point));
		}
*/

    #endregion Methods

    #region Fields

    // These regexs are courtesy of SharpVectorGraphics
    private static Regex _pathRegex = new Regex(@"(?=[A-Za-z])");
    private static Regex _argsRegex = new Regex(@"(\s*,\s*)|(\s+)|((?<=[0-9])(?=-))", RegexOptions.ExplicitCapture);

    #endregion Fields
  }
}