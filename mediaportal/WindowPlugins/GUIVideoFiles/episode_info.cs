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
using System.Text;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Holds information for episode information downloaded by the tv.com Parser
	/// </summary>
	public class episode_info
	{
		public string title = "";
		public int globalNumber;
		public int seasonNumber;
		public int episodeNumber;
		public string prodNumber = "";
		public double rating;
		public int numberOfRatings;
		public DateTime firstAired;
		public string director = "";
		public string writer = "";
		public string description = "";

		public System.Collections.ArrayList stars = new System.Collections.ArrayList();
		public System.Collections.ArrayList starsCharacters = new System.Collections.ArrayList();
		public System.Collections.ArrayList guestStars = new System.Collections.ArrayList();
		public System.Collections.ArrayList guestStarsCharacters = new System.Collections.ArrayList();

		// new additions
		public string network = "";
		public string airtime = "";
		public int runtime = 0;
		public string status = "";
		public DateTime seriesPremiere;
		public string genre = "";
		public string seriesDescription = "";


	}
}
