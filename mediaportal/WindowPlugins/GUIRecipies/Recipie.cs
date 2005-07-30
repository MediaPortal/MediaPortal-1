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

using System;
using System.Collections;
using MediaPortal.GUI.Library;
namespace GUIRecipies
{
	/// <summary>
	/// Summary description for Recipie.
	/// </summary>
	public class Recipie {
		string mId;
		string mTitle;
		int mCYield;
		ArrayList mCategories;
		string mYield;
		ArrayList mIngredients = new ArrayList();
		ArrayList mLot = new ArrayList();
		ArrayList mUnit = new ArrayList();
		ArrayList mRemarks = new ArrayList();
		string mDirections;
		
		public Recipie(){
			//
			// TODO: Add constructor logic here
			//
		}

		public string Title	{
			get{ return mTitle; }
			set{ mTitle = value; }
		}

		public string Id {
			get { return mId; }
			set { mId = value; }
		}

		public ArrayList Categories	{
			get{ return mCategories; }
			set{ mCategories = value; }
		}

		public void SetCategories( string Categories) {
			mCategories = new ArrayList( Categories.Split( ',' ) );
		}

		public string Yield	{
			get{ return mYield; }
			set{ mYield = value; }
		}

		public int CYield {
			get{ return mCYield; }
			set{ mCYield = value; }
		}

		public string Directions {
			get{ return mDirections; }
			set{ mDirections = value; }
		}

		public ArrayList Ingredients {
			get{ return mIngredients; }
			set{ mIngredients = value; }
		}

		public ArrayList Lot {
			get{ return mLot; }
			set{ mLot = value; }
		}

		public ArrayList Unit {
			get{ return mUnit; }
			set{ mUnit = value; }
		}

		public ArrayList Remarks {
			get{ return mRemarks; }
			set{ mRemarks = value; }
		}

		public void AddIngredient( string ing )	{
			Ingredients.Add( ing );
		}

		public void AddLot( string lot ) {
			Lot.Add( lot );
		}
		
		public void AddUnit( string unit ) {
			Unit.Add( unit );
		}

		public void AddRemarks( string rem ) {
			Remarks.Add( rem );
		}

		public override string ToString()
		{
			int sunit=0;
			double dsunit=0.0;
			double dilot=0.0;
			string s1 = "";
			string stunit = "";
			try	{
				if (CYield <= 1) {
					stunit=Yield;
				} else {
					stunit=Yield.Trim();
					string[] s2 = stunit.Split( ' ' );
					dsunit=Convert.ToDouble(s2[0]);
					stunit=Convert.ToString(CYield);
				}
			}
			catch (Exception ) {
				stunit=Yield;
				dsunit=1.0;
			}
			string retval = String.Format("{0}:      {1}\n{2}:  {3}\n\n{4}:\n",
				GUILocalizeStrings.Get(2006),Title,
				GUILocalizeStrings.Get(2007),stunit,
				GUILocalizeStrings.Get(2008));

			for( int i=0; i < Ingredients.Count; i++ ) {
				sunit=Convert.ToInt16(Unit[i]);
				if (sunit > 10)	{
					stunit=GUILocalizeStrings.Get(sunit)+" ";
				} else {
					stunit="";
				}
				s1=(string)Lot[i];
				s1=s1.Trim();
				if (s1!="") {
					try	{
						if (CYield > 1) {
							string SLot=(string)Lot[i];
							SLot=SLot.Trim();
							if (SLot.IndexOf("/",0) > 0) {
								int l = SLot.IndexOf("/",0);
								string b = SLot.Substring(l-1,3);
								double x=0.0;
								double y=0.0;
								if (l>2) {
									int k=SLot.IndexOf(" ",0);
									y=Convert.ToDouble(SLot.Substring(0,k));
								}
								switch (b) {
									case "1/2" :     // 1/2
										x=(y+0.5)/dsunit;
										x=x*(double)CYield;
										break;
									case "1/4" :     // 1/4
										x=(y+0.25)/dsunit;
										x=x*(double)CYield;
										break;
									case "1/8" :     // 1/8
										x=(y+0.125)/dsunit;
										x=x*(double)CYield;
										break;
									case "3/8" :     // 3/8
										x=(y+0.375)/dsunit;
										x=x*(double)CYield;
										break;
									case "3/4" :     // 3/4
										x=(y+0.75)/dsunit;
										x=x*(double)CYield;
										break;
								}
								s1=String.Format("{0:0.##} ",x);
							} else {
								dilot=Convert.ToDouble(Lot[i]);
								dilot=dilot/dsunit;
								dilot=dilot*(double)CYield;
								s1=String.Format("{0:0.##} ",dilot);
							}
						} else s1=s1+" ";
					}
					catch (Exception ) {
						s1=(string)Lot[i];
						s1=s1+" ";
					}
				}
				if (i+1 < Ingredients.Count) {
					if (Convert.ToInt16(Unit[i+1]) == 1) { // Line Break with "-" in first char
						retval = retval + "             " + s1 + stunit + Ingredients[i]+ Ingredients[i+1]+"\n";
						i++;
					} else {
						if (sunit == 2)	{		// Subtitle
							retval = retval + "\n" + Ingredients[i]+ "\n";;
						} else {					// Normal Ingredients
							retval = retval + "             " + s1 + stunit + Ingredients[i]+ "\n";;
						}
					}
				} else {
					retval = retval + "             " + s1 + stunit + Ingredients[i]+ "\n";;
				}
			}
			retval = retval + String.Format("\n{0}:\n{1}",GUILocalizeStrings.Get(2009), Directions) ;
			return retval;
		}
	}
}
