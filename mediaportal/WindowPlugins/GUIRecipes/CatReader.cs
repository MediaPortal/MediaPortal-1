/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Collections;
using System.IO;
using MediaPortal.GUI.Library;

namespace GUIRecipes {
	/// <summary>
	/// Summary description for CatReader.
	/// </summary>
	public class CatReader {

		protected string cFileName;

		public int CatCount {
			get { return catCount; }
		}

		protected int catCount = 0;

		public CatReader( string fileName ) {
			cFileName = fileName;
		}

		public void GetCategories() {
			StreamReader reader = new StreamReader( cFileName, System.Text.Encoding.Default );
			string line = reader.ReadLine();
			bool maincat=false;
			bool sorts=false;
			string content="";
			int num=0;

			while( line != null ) {
				if (line.Trim().StartsWith( "#Main" )) {
					maincat=true;
					sorts=false;
					line = reader.ReadLine();
				}
				if (line.Trim().StartsWith( "#Sorts" )) {
					sorts=true;
					maincat=false;
					line = reader.ReadLine();
				}

				string[] lines = line.Split( ';' );
				int l=0;
				foreach( string lin in lines ) {
					if (l==1) {
						content=lin.Trim();
						l++;
					}
					if (l==0) {
						num=Convert.ToInt16(lin);
						l++;
					}
				}
				if (maincat==true) {
					catCount++;
					RecipeDatabase.GetInstance().AddMainCat( content.TrimEnd(), num,0);
				}
				if (sorts==true) {
					catCount++;
					RecipeDatabase.GetInstance().AddMainCat( content.TrimEnd(), 0, num);
				}
				line = reader.ReadLine();
			}
			reader.Close();
		}
	}
}
