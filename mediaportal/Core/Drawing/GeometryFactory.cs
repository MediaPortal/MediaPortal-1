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
using System.Text.RegularExpressions;

using MediaPortal.Drawing;
using MediaPortal.Drawing.Paths;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using Color=System.Drawing.Color;
using PositionColored				= Microsoft.DirectX.Direct3D.CustomVertex.PositionColored;
using PositionTextured				= Microsoft.DirectX.Direct3D.CustomVertex.PositionTextured;
using PositionColoredTextured		= Microsoft.DirectX.Direct3D.CustomVertex.PositionColoredTextured;

namespace MediaPortal.Drawing.Shapes
{
	// Based on the sources provided by:
	//
	// Paul Bourke:			http://astronomy.swin.edu.au/~pbourke/curves/bezier/
	// Fernando Cacciola:	http://groups.google.co.uk/group/comp.graphics.algorithms/msg/1334c1e83745fc85?hl=en&

	public sealed class GeometryFactory
	{
		#region Constructors

		private GeometryFactory()
		{
		}

		#endregion Constructors

		#region Methods

		public static Point[] CreateArc(ref Point sp, ArcSegment s)
		{
			return CreateArc(ref sp, s.Size.Width, s.Size.Height, s.Rotation, s.LargeArc, s.SweepDirection, s.Point.X, s.Point.Y);
		}

		// rsvg_path_arc: Add an RSVG arc to the path context.
		// @ctx: Path context.
		// @rx: Radius in x direction (before rotation).
		// @ry: Radius in y direction (before rotation).
		// @rotationAngle: Rotation angle for axes.
		// @large_arc_flag: 0 for arc length <= 180, 1 for arc >= 180.
		// @sweep: 0 for "negative angle", 1 for "positive angle".
		// @x: New x coordinate.
		// @y: New y coordinate.
		private static Point[] CreateArc(ref Point sp, double rx, double ry, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, double x, double y)
		{
			// Check that neither radius is zero, since its isn't either geometrically or mathematically 
			// meaningful and will cause divide by zero and subsequent NaNs.  We should really do some
			// ranged check ie -0.001 < x < 000.1 rather can just a straight check again zero.
			if((rx == 0.0) || (ry == 0.0))
				return new Point[0];

			double sin_th = Math.Sin(rotationAngle * (Math.PI / 180.0));
			double cos_th = Math.Cos(rotationAngle * (Math.PI / 180.0));
			double a00 = cos_th / rx;
			double a01 = sin_th / rx;
			double a10 = -sin_th / ry;
			double a11 = cos_th / ry;
			double x0 = a00 * sp.X + a01 * sp.Y;
			double y0 = a10 * sp.X + a11 * sp.Y;
			double x1 = a00 * x + a01 * y;
			double y1 = a10 * x + a11 * y;

			// (x0, y0) is current point in transformed coordinate space.
			// (x1, y1) is new point in transformed coordinate space.
			// The arc fits a unit-radius circle in this space.
			double d = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);
			double sfactor_sq = 1.0 / d - 0.25;
			if (sfactor_sq < 0) sfactor_sq = 0;
			double sfactor = Math.Sqrt(sfactor_sq);
			if (sweepDirection == SweepDirection.Clockwise && isLargeArc) sfactor = -sfactor;
			double xc = 0.5 * (x0 + x1) - sfactor * (y1 - y0);
			double yc = 0.5 * (y0 + y1) + sfactor * (x1 - x0);

			// (xc, yc) is center of the circle.
			double th0 = Math.Atan2(y0 - yc, x0 - xc);
			double th1 = Math.Atan2(y1 - yc, x1 - xc);
			double th_arc = th1 - th0;

			if (th_arc < 0 && sweepDirection == SweepDirection.Clockwise)
				th_arc += 2 * Math.PI;
			else if (th_arc > 0 && sweepDirection == SweepDirection.CounterClockwise)
				th_arc -= 2 * Math.PI;

			int n_segs = (int)Math.Ceiling(Math.Abs(th_arc / (Math.PI * 0.5 + 0.001)));

			ArrayList array = new ArrayList();

			for(int index = 0; index < n_segs; ++index)
			{
				foreach(Point p in CreateArcSegment(ref sp, xc, yc, th0 + index * th_arc / n_segs, th0 + (index + 1) * th_arc / n_segs, rx, ry, rotationAngle))
					array.Add(p);
			}

			sp.X = x;
			sp.Y = y;

			return (Point[])array.ToArray(typeof(Point));
		}

		private static Point[] CreateArcSegment(ref Point sp, double xc, double yc, double th0, double th1, double rx, double ry, double rotationAngle)
		{
			double sin_th = Math.Sin(rotationAngle * (Math.PI / 180.0));
			double cos_th = Math.Cos(rotationAngle * (Math.PI / 180.0)); 

			// inverse transform compared with rsvg_path_arc
			double a00 = cos_th * rx;
			double a01 = -sin_th * ry;
			double a10 = sin_th * rx;
			double a11 = cos_th * ry;

			double th_half = 0.5 * (th1 - th0);
			double t = (8.0 / 3.0) * Math.Sin(th_half * 0.5) * Math.Sin(th_half * 0.5) / Math.Sin(th_half);
			double x1 = xc + Math.Cos(th0) - t * Math.Sin(th0);
			double y1 = yc + Math.Sin(th0) + t * Math.Cos(th0);
			double x3 = xc + Math.Cos(th1);
			double y3 = yc + Math.Sin(th1);
			double x2 = x3 + t * Math.Sin(th1);
			double y2 = y3 - t * Math.Cos(th1);

			return CreateBezier(ref sp, a00 * x1 + a01 * y1, a10 * x1 + a11 * y1, a00 * x2 + a01 * y2, a10 * x2 + a11 * y2, a00 * x3 + a01 * y3, a10 * x3 + a11 * y3);
		}

		public static Point[] CreateBezier(ref Point sp, double x1, double y1, double x2, double y2)
		{
			double x3 = x2;
			double y3 = y2;

			x2 = (x3 + 2 * x1) * (1.0 / 3.0);
			y2 = (y3 + 2 * y2) * (1.0 / 3.0);
			x1 = (sp.X + 2 * x1) * (1.0 / 3.0);
			y1 = (sp.Y + 2 * y1) * (1.0 / 3.0);

			Point[] points = CreateBezier(ref sp, x1, y1, x2, y2, x3, y3);

			sp.X = x2;
			sp.Y = y2;

			// smoothing
			//			rp.X = x1;
			//			rp.Y = x2;

			return points;			
		}

		public static Point[] CreateBezier(ref Point sp, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			Point[] points = new Point[28];

			for(int index = 0; index < points.Length; ++index)
			{
				double mu = (double)index / points.Length;
				double mum1 = 1 - mu;
				double mum13 = mum1 * mum1 * mum1;
				double mu3 = mu * mu * mu;
				double x = mum13* sp.X + 3*mu*mum1*mum1* x1 + 3*mu*mu*mum1* x2 + mu3* x3;
				double y = mum13* sp.Y + 3*mu*mum1*mum1* y1 + 3*mu*mu*mum1* y2 + mu3* y3;

				points[index] = new Point(x, y);
			}

			sp.X = x3;
			sp.Y = y3;

			return points;
		}

		public static Point[] CreateBezier(ref Point sp, BezierSegment s)
		{
			ArrayList array = new ArrayList();

			foreach(Point point in CreateBezier(ref sp, s.Point1.X, s.Point1.Y, s.Point2.X, s.Point2.Y, s.Point3.X, s.Point3.Y))
				array.Add(point);

			return (Point[])array.ToArray(typeof(Point));
		}

		public static Point[] CreateBezier(ref Point sp, QuadraticBezierSegment s)
		{
			return CreateBezier(ref sp, s.Point1.X, s.Point1.Y, s.Point2.X, s.Point2.Y);
		}

		public static Point[] CreateLine(ref Point sp, LineSegment s)
		{
			Point[] points = new Point[] { sp, s.Point };

			sp = points[1];

			return points;
		}
					
		public static Point[] CreateLine(ref Point sp, double x, double y)
		{
			Point[] points = new Point[] { sp, new Point(x, y) };

			sp = points[1];

			return points;
		}

/*		public static Point[] CreateGeometry(string data)
		{
			PathSegmentCollection segments = new PathSegmentCollection();
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
				
		public static Geometry CreateGeometry(Type vertexType, int vertexCount, int primitiveCount)
		{
			return new Geometry(vertexType, vertexCount, primitiveCount);
		}

		public static Geometry CreateGeometry(Type vertexType, Array array, int primitiveCount)
		{
			return new Geometry(vertexType, array, primitiveCount);
		}

		public static Geometry CreateGradient(Color sc, Color ec, GradientStopCollection stops)
		{
			return CreateGradient(sc, ec, new Point(0, 0), new Point(1, 1), stops);
		}
			
		public static Geometry CreateGradient(Color sc, Color ec, Point sp, Point ep, GradientStopCollection stops)
		{
			if(sp.X < 0 || sp.X > 1 || sp.Y < 0 || sp.Y > 1)
				throw new ArgumentOutOfRangeException("sp");

			if(ep.X < 0 || ep.X > 1 || ep.Y < 0 || ep.Y > 1)
				throw new ArgumentOutOfRangeException("ep");

			double dx = ep.X - sp.X;
			double dy = ep.Y - sp.Y;

			if(dx == 0 && dy == 0)
				throw new ArgumentOutOfRangeException("sp or ep");

			int hash = PositionColored.Format.GetHashCode() ^ sp.GetHashCode() ^ ep.GetHashCode() ^ sc.GetHashCode() ^ ec.GetHashCode();

			Geometry geometry = null;

			lock(_rectangleCache.SyncRoot)
				geometry = _rectangleCache[hash] as Geometry;

			if(geometry != null)
				return geometry;

			double w = 100;
			double h = 100;
			double l = Math.Sqrt(dx * dx + dy * dy);
			double a = Math.Atan(dx / dy);
			double b = 90 - a;
			Point vert = Point.Empty;
			Point horz = Point.Empty;

			ArrayList vertexArray = new ArrayList();

			vertexArray.Add(new PositionColored((float)sp.X, (float)sp.Y, 0, sc.ToArgb()));
			vertexArray.Add(new PositionColored((float)sp.X, (float)sp.Y, 0, sc.ToArgb()));
	
			foreach(GradientStop stop in stops)
			{
				vert.X = sp.X;
				vert.Y = sp.Y + (stop.Offset * dy) * Math.Cos(a);

				if(vert.Y > dy)
				{
					// clip to bottom edge
					vert.X = sp.X + ((vert.Y - dy) * Math.Tan(b));
					vert.Y = sp.Y + dx;
				}

				horz.X = sp.X + (stop.Offset * dx) * Math.Cos(b);
				horz.Y = sp.Y;
			
				if(horz.X > dx)
				{
					// clip to right edge
					horz.X = sp.X + dx;  
					horz.Y = sp.Y + ((horz.X - dx) * Math.Tan(a));
				}

				vertexArray.Add(new PositionColored((float)(horz.X * w), (float)(horz.Y * h), 0, stop.Color.ToArgb()));
				vertexArray.Add(new PositionColored((float)(vert.X * w), (float)(vert.Y * h), 0, stop.Color.ToArgb()));
			}

			vertexArray.Add(new PositionColored((float)(ep.X * w), (float)(ep.Y * h), 0, ec.ToArgb()));
			vertexArray.Add(new PositionColored((float)(ep.X * w), (float)(ep.Y * h), 0, ec.ToArgb()));

			geometry = CreateGeometry(typeof(PositionColored), vertexArray.ToArray(), vertexArray.Count);
			geometry.VertexFormat = PositionColored.Format;
			geometry.PrimitiveType = PrimitiveType.LineStrip;

			_rectangleCache[hash] = geometry;

			return geometry;
		}

		public static Geometry CreateRectangle(Size size, Color color)
		{
			int hash = PositionColored.Format.GetHashCode() ^ size.GetHashCode() ^ color.GetHashCode();

			Geometry geometry = null;

			lock(_rectangleCache.SyncRoot)
				geometry = _rectangleCache[hash] as Geometry;

			if(geometry != null)
				return geometry;

			// v0         v1
			// +----------+
			// |		  |
			// |		  |		
			// |		  |
			// |		  |
			// +----------+
			// v2         v3

			// the .5 adjustments are needed for correct texel mapping
			float x1 = -.5f;
			float x2 = -.5f + (float)size.Width;
			float y1 = -.5f;
			float y2 = -.5f + (float)size.Height;

			geometry = CreateGeometry(typeof(PositionColored), 4, 2);
			geometry.VertexFormat = PositionColored.Format;
			geometry.PrimitiveType = PrimitiveType.TriangleStrip;
			geometry[0] = new PositionColored(x1, y1, 0, color.ToArgb());
			geometry[1] = new PositionColored(x2, y1, 0, color.ToArgb());
			geometry[2] = new PositionColored(x1, y2, 0, color.ToArgb());
			geometry[3] = new PositionColored(x2, y2, 0, color.ToArgb());

			lock(_rectangleCache.SyncRoot)
				_rectangleCache[hash] = geometry;

			return geometry;
		}

		public static Geometry CreateTextured(Size size)
		{
			int hash = PositionTextured.Format.GetHashCode() ^ size.GetHashCode();

			Geometry geometry = null;

			lock(_rectangleCache.SyncRoot)
				geometry = _rectangleCache[hash] as Geometry;

			if(geometry != null)
				return geometry;

			// v0         v1
			// +----------+
			// |		  |
			// |		  |		
			// |		  |
			// |		  |
			// +----------+
			// v2         v3

			// the .5 adjustments are needed for correct texel mapping
			float x1 = -.5f;
			float x2 = -.5f + (float)size.Width;
			float y1 = -.5f;
			float y2 = -.5f + (float)size.Height;

			geometry = CreateGeometry(typeof(PositionTextured), 4, 2);
			geometry.VertexFormat = PositionTextured.Format;
			geometry.PrimitiveType = PrimitiveType.TriangleStrip;
			geometry[0] = new PositionTextured(x1, y1, 0, 0, 0);
			geometry[1] = new PositionTextured(x2, y1, 0, 1, 0);
			geometry[2] = new PositionTextured(x1, y2, 0, 0, 1);
			geometry[3] = new PositionTextured(x2, y2, 0, 1, 1);

			lock(_rectangleCache.SyncRoot)
				_rectangleCache[hash] = geometry;

			return geometry;
		}

		private static double[] ParseParameters(string segment, int count, string message)
		{
			double[] args = null;
					
			segment = segment.Substring(1);
			segment = segment.Trim();
			segment = segment.Trim(new char[]{','});

			if(segment.Length > 0)
			{
				string[] sCoords = _argsRegex.Split(segment);

				args = new double[sCoords.Length];
							
				for(int index = 0; index < args.Length; ++index)
					args[index] = double.Parse(sCoords[index]);
			}

			if(args == null || args.Length != count)
				throw new ArgumentException(message);

			return args;
		}
*/
		#endregion Methods
		
		#region Fields

		// These regexs are courtesy of SharpVectorGraphics
		static Regex				_pathRegex = new Regex(@"(?=[A-Za-z])");
		static Regex				_argsRegex = new Regex(@"(\s*,\s*)|(\s+)|((?<=[0-9])(?=-))", RegexOptions.ExplicitCapture);

		static Hashtable			_rectangleCache = new Hashtable();

		#endregion Fields
	}
}