using System;
using System.Text;

namespace MediaPortal.GUI.Video
{
	public class episode_info
	{
		public string title = "";
		public int globalNumber;
		public int seasonNumber;
		public int episodeNumber;
		public string prodNumber = "";
		public double rating;
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
