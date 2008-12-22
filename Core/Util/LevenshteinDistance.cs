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

namespace MediaPortal.Util
{
	// http://www.merriampark.com/ld.htm
	// http://www.merriampark.com/ldcsharp.htm
	public class Levenshtein
	{
		/// <summary>
		/// Compute Levenshtein distance
		/// </summary>
		/// <param name="s">String 1</param>
		/// <param name="t">String 2</param>
		/// <returns>Distance between the two strings. The larger the number, the bigger the difference.</returns>

		public static int Match(string s, string t) 
		{
			// Step 1
			if(s.Length == 0)
				return t.Length;

			if(t.Length == 0)
				return s.Length;

			int n = s.Length; //length of s
			int m = t.Length; //length of t
			int[,] d = new int[n + 1, m + 1]; // matrix
			int cost; // cost

			// Step 2
			for(int i = 0; i <= n; d[i, 0] = i++);
			for(int j = 0; j <= m; d[0, j] = j++);

			// Step 3
			for(int i = 1; i <= n;i++) 
			{
				//Step 4
				for(int j = 1; j <= m;j++) 
				{
					// Step 5
					cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);

					// Step 6
					d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
				}
			}

			// Step 7
			return d[n, m];
		}
	}
}
