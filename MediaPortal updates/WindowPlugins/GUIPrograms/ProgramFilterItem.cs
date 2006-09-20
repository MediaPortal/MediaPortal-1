/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for ProgramFilterItem.
	/// </summary>
	public class ProgramFilterItem
	{
    string mTitle = "";
    string mTitle2 = "";
    string mGenre = "";
    string mCountry = "";
    string mManufacturer = "";
    int mYear = -1;
    int mRating = 5;

		public ProgramFilterItem()
		{
			//
			// TODO: Add constructor logic here
			//
		}
    public string Title
    {
      get
      {
        return mTitle;
      }
      set
      {
        mTitle = value;
      }
    }
    public string Title2
    {
      get
      {
        return mTitle2;
      }
      set
      {
        mTitle2 = value;
      }
    }

    public string Genre
    {
      get
      {
        return mGenre;
      }
      set
      {
        mGenre = value;
      }
    }
    public string Country
    {
      get
      {
        return mCountry;
      }
      set
      {
        mCountry = value;
      }
    }
    public string Manufacturer
    {
      get
      {
        return mManufacturer;
      }
      set
      {
        mManufacturer = value;
      }
    }
    public int Year
    {
      get
      {
        return mYear;
      }
      set
      {
        mYear = value;
      }
    }
    public int Rating
    {
      get
      {
        return mRating;
      }
      set
      {
        mRating = value;
      }
    }

	}
}
