using System;
using System.Collections;
using System.IO;
using MediaPortal.GUI.Library;

namespace GUIRecipies {
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
					RecipieDatabase.GetInstance().AddMainCat( content.TrimEnd(), num,0);
				}
				if (sorts==true) {
					catCount++;
					RecipieDatabase.GetInstance().AddMainCat( content.TrimEnd(), 0, num);
				}
				line = reader.ReadLine();
			}
			reader.Close();
		}
	}
}
