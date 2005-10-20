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

namespace MediaPortal.Drawing.Paths
{
	public class Triangulator
	{
		public class ITRIANGLE
		{
			internal int p1, p2, p3;
			internal ITRIANGLE()
			{
				;
			}
		}

		internal class IEDGE
		{
			internal int p1, p2;
			internal IEDGE()
			{
				p1 = - 1; p2 = - 1;
			}
		}

		public class XYZ
		{
			internal double x, y, z;
			internal XYZ()
			{
				;
			}
			internal XYZ(double _x, double _y)
			{
				this.x = _x; this.y = _y;
			}
			public XYZ(double _x, double _y, double _z)
			{
				this.x = _x; this.y = _y; this.z = _z;
			}
		}
	
		public static double EPSILON = 0.000001;
	
		/*
		Return TRUE if a point (xp,yp) is inside the circumcircle made up
		of the points (x1,y1), (x2,y2), (x3,y3)
		The circumcircle centre is returned in (xc,yc) and the radius r
		NOTE: A point on the edge is inside the circumcircle
		*/
		internal static bool CircumCircle(double xp, double yp, double x1, double y1, double x2, double y2, double x3, double y3, XYZ circle)
		{
			double m1, m2, mx1, mx2, my1, my2;
			double dx, dy, rsqr, drsqr;
			double xc, yc, r;
		
			/* Check for coincident points */
		
			if (System.Math.Abs(y1 - y2) < EPSILON && System.Math.Abs(y2 - y3) < EPSILON)
			{
				System.Console.Out.WriteLine("CircumCircle: Points are coincident.");
				return false;
			}
		
			if (System.Math.Abs(y2 - y1) < EPSILON)
			{
				m2 = (- (x3 - x2)) / (y3 - y2);
				mx2 = (x2 + x3) / 2.0;
				my2 = (y2 + y3) / 2.0;
				xc = (x2 + x1) / 2.0;
				yc = m2 * (xc - mx2) + my2;
			}
			else if (System.Math.Abs(y3 - y2) < EPSILON)
			{
				m1 = (- (x2 - x1)) / (y2 - y1);
				mx1 = (x1 + x2) / 2.0;
				my1 = (y1 + y2) / 2.0;
				xc = (x3 + x2) / 2.0;
				yc = m1 * (xc - mx1) + my1;
			}
			else
			{
				m1 = (- (x2 - x1)) / (y2 - y1);
				m2 = (- (x3 - x2)) / (y3 - y2);
				mx1 = (x1 + x2) / 2.0;
				mx2 = (x2 + x3) / 2.0;
				my1 = (y1 + y2) / 2.0;
				my2 = (y2 + y3) / 2.0;
				xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
				yc = m1 * (xc - mx1) + my1;
			}
		
			dx = x2 - xc;
			dy = y2 - yc;
			rsqr = dx * dx + dy * dy;
			r = System.Math.Sqrt(rsqr);
		
			dx = xp - xc;
			dy = yp - yc;
			drsqr = dx * dx + dy * dy;
		
			circle.x = xc;
			circle.y = yc;
			circle.z = r;
		
			return (drsqr <= rsqr?true:false);
		}
	
		/*
		Triangulation subroutine
		Takes as input NV vertices in array pxyz
		Returned is a list of ntri triangular faces in the array v
		These triangles are arranged in a consistent clockwise order.
		The triangle array 'v' should be malloced to 3 * nv
		The vertex array pxyz must be big enough to hold 3 more points
		The vertex array must be sorted in increasing x values say
	
		qsort(p,nv,sizeof(XYZ),XYZCompare);
	
		int XYZCompare(void *v1,void *v2)
		{
		XYZ *p1,*p2;
		p1 = v1;
		p2 = v2;
		if (p1->x < p2->x)
		return(-1);
		else if (p1->x > p2->x)
		return(1);
		else
		return(0);
		}
		*/
	
		public static Point[] Triangulate(Point[] points, ref Triangle[] triangles)
		{
			XYZ[] xyz = new XYZ[points.Length + 3];

			for(int index = 0; index < points.Length; ++index)
				xyz[index] = new XYZ(points[index].X, points[index].Y, 0);

			xyz[points.Length + 0] = new XYZ();
			xyz[points.Length + 1] = new XYZ();
			xyz[points.Length + 2] = new XYZ();

			ITRIANGLE[] tris = new ITRIANGLE[points.Length * 3];

			for(int index = 0; index < tris.Length; ++index)
				tris[index] = new ITRIANGLE();
			
			int triangleCount = Triangulate(xyz.Length - 3, xyz, tris);

			triangles = new Triangle[triangleCount];

			for(int index = 0; index < triangles.Length; ++index)
				triangles[index] = new Triangle(tris[index].p1, tris[index].p2, tris[index].p3);

			points = new Point[points.Length + 3];

			for(int index = 0; index < xyz.Length; ++index)
				points[index] = new Point(xyz[index].x, xyz[index].y);

			return points;
		}

		private static int Triangulate(int nv, XYZ[] pxyz, ITRIANGLE[] v)
		{
			bool[] complete = null;
			IEDGE[] edges = null;
			int nedge = 0;
			int trimax, emax = 200;
		
			bool inside;
			//int 	i, j, k;
			double xp, yp, x1, y1, x2, y2, x3, y3, xc, yc, r;
			double xmin, xmax, ymin, ymax, xmid, ymid;
			double dx, dy, dmax;
		
			int ntri = 0;
		
			/* Allocate memory for the completeness list, flag for each triangle */
			trimax = 4 * nv;
			complete = new bool[trimax];
			for (int ic = 0; ic < trimax; ic++)
				complete[ic] = false;
		
			/* Allocate memory for the edge list */
			edges = new IEDGE[emax];
			for (int ie = 0; ie < emax; ie++)
				edges[ie] = new IEDGE();
		
			/*
			Find the maximum and minimum vertex bounds.
			This is to allow calculation of the bounding triangle
			*/
			xmin = pxyz[0].x;
			ymin = pxyz[0].y;
			xmax = xmin;
			ymax = ymin;
			for (int i = 1; i < nv; i++)
			{
				if (pxyz[i].x < xmin)
					xmin = pxyz[i].x;
				if (pxyz[i].x > xmax)
					xmax = pxyz[i].x;
				if (pxyz[i].y < ymin)
					ymin = pxyz[i].y;
				if (pxyz[i].y > ymax)
					ymax = pxyz[i].y;
			}
			dx = xmax - xmin;
			dy = ymax - ymin;
			dmax = (dx > dy)?dx:dy;
			xmid = (xmax + xmin) / 2.0;
			ymid = (ymax + ymin) / 2.0;
		
			/*
			Set up the supertriangle
			This is a triangle which encompasses all the sample points.
			The supertriangle coordinates are added to the end of the
			vertex list. The supertriangle is the first triangle in
			the triangle list.
			*/
			pxyz[nv + 0].x = xmid - 2.0 * dmax;
			pxyz[nv + 0].y = ymid - dmax;
			pxyz[nv + 0].z = 0.0;
			pxyz[nv + 1].x = xmid;
			pxyz[nv + 1].y = ymid + 2.0 * dmax;
			pxyz[nv + 1].z = 0.0;
			pxyz[nv + 2].x = xmid + 2.0 * dmax;
			pxyz[nv + 2].y = ymid - dmax;
			pxyz[nv + 2].z = 0.0;
			v[0].p1 = nv;
			v[0].p2 = nv + 1;
			v[0].p3 = nv + 2;
			complete[0] = false;
			ntri = 1;
		
		
			/*
			Include each point one at a time into the existing mesh
			*/
			for (int i = 0; i < nv; i++)
			{
			
				xp = pxyz[i].x;
				yp = pxyz[i].y;
				nedge = 0;
			
			
				/*
				Set up the edge buffer.
				If the point (xp,yp) lies inside the circumcircle then the
				three edges of that triangle are added to the edge buffer
				and that triangle is removed.
				*/
				XYZ circle = new XYZ();
				for (int j = 0; j < ntri; j++)
				{
					if (complete[j])
						continue;
					x1 = pxyz[v[j].p1].x;
					y1 = pxyz[v[j].p1].y;
					x2 = pxyz[v[j].p2].x;
					y2 = pxyz[v[j].p2].y;
					x3 = pxyz[v[j].p3].x;
					y3 = pxyz[v[j].p3].y;
					inside = CircumCircle(xp, yp, x1, y1, x2, y2, x3, y3, circle);
					xc = circle.x; yc = circle.y; r = circle.z;
					if (xc + r < xp)
						complete[j] = true;
					if (inside)
					{
						/* Check that we haven't exceeded the edge list size */
						if (nedge + 3 >= emax)
						{
							emax += 100;
							IEDGE[] edges_n = new IEDGE[emax];
							for (int ie = 0; ie < emax; ie++)
								edges_n[ie] = new IEDGE();
							Array.Copy((System.Array) edges, 0, (System.Array) edges_n, 0, edges.Length);
							edges = edges_n;
						}
						edges[nedge + 0].p1 = v[j].p1;
						edges[nedge + 0].p2 = v[j].p2;
						edges[nedge + 1].p1 = v[j].p2;
						edges[nedge + 1].p2 = v[j].p3;
						edges[nedge + 2].p1 = v[j].p3;
						edges[nedge + 2].p2 = v[j].p1;
						nedge += 3;
						v[j].p1 = v[ntri - 1].p1;
						v[j].p2 = v[ntri - 1].p2;
						v[j].p3 = v[ntri - 1].p3;
						complete[j] = complete[ntri - 1];
						ntri--;
						j--;
					}
				}
			
				/*
				Tag multiple edges
				Note: if all triangles are specified anticlockwise then all
				interior edges are opposite pointing in direction.
				*/
				for (int j = 0; j < nedge - 1; j++)
				{
					//if ( !(edges[j].p1 < 0 && edges[j].p2 < 0) )
					for (int k = j + 1; k < nedge; k++)
					{
						if ((edges[j].p1 == edges[k].p2) && (edges[j].p2 == edges[k].p1))
						{
							edges[j].p1 = - 1;
							edges[j].p2 = - 1;
							edges[k].p1 = - 1;
							edges[k].p2 = - 1;
						}
						/* Shouldn't need the following, see note above */
						if ((edges[j].p1 == edges[k].p1) && (edges[j].p2 == edges[k].p2))
						{
							edges[j].p1 = - 1;
							edges[j].p2 = - 1;
							edges[k].p1 = - 1;
							edges[k].p2 = - 1;
						}
					}
				}
			
				/*
				Form new triangles for the current point
				Skipping over any tagged edges.
				All edges are arranged in clockwise order.
				*/
				for (int j = 0; j < nedge; j++)
				{
					if (edges[j].p1 == - 1 || edges[j].p2 == - 1)
						continue;
					if (ntri >= trimax)
						return - 1;
					v[ntri].p1 = edges[j].p1;
					v[ntri].p2 = edges[j].p2;
					v[ntri].p3 = i;
					complete[ntri] = false;
					ntri++;
				}
			}
		
		
			/*
			Remove triangles with supertriangle vertices
			These are triangles which have a vertex number greater than nv
			*/
			for (int i = 0; i < ntri; i++)
			{
				if (v[i].p1 >= nv || v[i].p2 >= nv || v[i].p3 >= nv)
				{
					v[i] = v[ntri - 1];
					ntri--;
					i--;
				}
			}
		
			return ntri;
		}
	}
}
