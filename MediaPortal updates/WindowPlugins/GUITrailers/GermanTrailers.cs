using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Player;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for GermanTrailers.
	/// </summary>
	public class GermanTrailers
	{
		public static string TempHTML = string.Empty;

		public static string[] GermanMovieName = new string[25];
		public static string[] GermanTrailerURL = new string[25];

		public static string[] Woche = {GUILocalizeStrings.Get(5912), GUILocalizeStrings.Get(5913),
										   GUILocalizeStrings.Get(5914), null};	
		public static string[] G_Genre = new string[25];
		public static string[] G_Runtime = new string[25];
		public static string[] G_Releasedate = new string[25];
		public static string[] G_Plot = new string[25];
		public static string[] G_Cast = new string[25];
		public static string[] G_PosterUrl = new string[25];
		public static Double[] G_Rating = new double[25];

		public static int GermanSelected;
		
		public static bool G_viewWoche = false; //Woche listview
		public static bool G_viewMovie = false; //Movie listview
		public static bool G_viewInfoAndTrailer = false; // Info and Trailerview
        

		public static void GetGermanTrailers(string url)
		{
			Array.Clear(GermanMovieName, 0,25);
			Array.Clear(GermanTrailerURL, 0,25);
			Array.Clear(G_Genre, 0,25);
			Array.Clear(G_Runtime, 0,25);
			Array.Clear(G_Releasedate, 0,25);
			Array.Clear(G_Plot, 0,25);
			Array.Clear(G_Cast, 0,25);
			Array.Clear(G_PosterUrl, 0,25);
			Array.Clear(G_Rating, 0,25);

			TrailersUtility TU = new TrailersUtility();

			TU.GetWebPage(url, out TempHTML);
			if(TempHTML == null || TempHTML == string.Empty)
				return;

			// check if there are any trailersreviews available for this week;
			Match a = Regex.Match(TempHTML, @"Leider noch keine Film-Besprechungen");
			if(a.Success==true)
				return;
            
			string[] TempBlok = new string[25];

			MatchCollection mc = Regex.Matches(TempHTML, @"(?s-imnx:&nbsp;<span\sclass=.promo.(?<tempblok>.*?).E9EEF2.>)");
			int i = 0;
			foreach(Match m in mc)
			{
				//split page in bloks to ease search
				TempBlok[i] = m.Groups["tempblok"].Value;

				//get moviename & releasedate
				Match ur = Regex.Match(TempBlok[i], @"<a\shref=(?<movieurl>.*.html)>(?<moviename>.*)?</a>.*(?<releasedate>ab.*).</span>");
				GermanMovieName[i] = ur.Groups["moviename"].Value;
				ConvertStr(GermanMovieName[i], out GermanMovieName[i]);
				G_Releasedate[i] = ur.Groups["releasedate"].Value;

				// get posterurl
				Match p = Regex.Match(TempBlok[i], @"<IMG\sSRC=.*reviews/(?<posterurl>.*)s.jpg");
				if(p.Groups["posterurl"].Success==true)
					G_PosterUrl[i] = "http://eur.i1.yimg.com/eur.yimg.com/emvreviews/" + p.Groups["posterurl"].Value + "m.jpg";

				// get genre
				Match g = Regex.Match(TempBlok[i], @"Genre:</b>(?<moviegenre>.*)<br>");
				G_Genre[i] = g.Groups["moviegenre"].Value;
				ConvertStr(G_Genre[i], out G_Genre[i]);

				// get rating
				MatchCollection rc = Regex.Matches(TempBlok[i], @"http://eur.news1.yimg.com/eur.yimg.com/i/de/mo/1s.gif");
				G_Rating[i] = rc.Count;

				// get cast
				G_Cast[i]="";
				MatchCollection cc = Regex.Matches(TempBlok[i], @"http://de.search.movies.yahoo.com/search/movies_de.*<nobr>(?<cast>.*)</nobr>");
				foreach(Match c in cc)
				{
					if(G_Cast[i].Equals(""))
					{
						G_Cast[i] = c.Groups["cast"].Value;
					}
					else
					{
						G_Cast[i] = G_Cast[i].Insert(G_Cast[i].Length,", ");
						G_Cast[i] = G_Cast[i].Insert(G_Cast[i].Length, c.Groups["cast"].Value);
					}
				}
				ConvertStr(G_Cast[i], out G_Cast[i]);

				// get plot
				Match pl = Regex.Match(TempBlok[i], @"(?s-imnx:<td\scolspan.*class=.pcontent.>(?<movieplot>.*)<p\sstyle)");
				G_Plot[i] = pl.Groups["movieplot"].Value;
				ConvertStr(G_Plot[i], out G_Plot[i]);

				// get trailerurl
				Match t = Regex.Match(TempBlok[i], @"href=.(?<trailerurl>.*).><b>Trailer</b></a>");
				if(t.Groups["trailerurl"].Success==true)
					GermanTrailerURL[i] = "http://de.movies.yahoo.com" + t.Groups["trailerurl"].Value;
                
				i++;
			}
            
		}
		public static void ConvertStr(string input, out string output)
		{
			input = input.Replace("&uml;" ,((char)168).ToString());
			input = input.Replace("&Auml;" ,((char)196).ToString());
			input = input.Replace("&Euml;" ,((char)203).ToString());
			input = input.Replace("&Iuml;" ,((char)207).ToString());
			input = input.Replace("&Ouml;" ,((char)214).ToString());
			input = input.Replace("&Uuml;" ,((char)220).ToString());
			input = input.Replace("&auml;" ,((char)228).ToString());
			input = input.Replace("&euml;" ,((char)235).ToString());
			input = input.Replace("&iuml;" ,((char)239).ToString());
			input = input.Replace("&ouml;" ,((char)246).ToString());
			input = input.Replace("&uuml;" ,((char)252).ToString());
			input = input.Replace("&yuml;" ,((char)255).ToString());
			input = input.Replace("&Yuml;" ,((char)168).ToString());
			input = input.Replace("&szlig;", "ß");
			output = input;
		}

		public static void SetProperties(string moviename, int i)
		{
			GUIPropertyManager.SetProperty("#title", moviename);
			GUIPropertyManager.SetProperty("#genre", G_Genre[i]);
			GUIPropertyManager.SetProperty("#runtime", "");
			GUIPropertyManager.SetProperty("#year", G_Releasedate[i]);
			GUIPropertyManager.SetProperty("#plot", G_Plot[i]);
			GUIPropertyManager.SetProperty("#cast", G_Cast[i]);
		}
		public static void PlayGermanTrailer(string url)
		{
			TrailersUtility TU = new TrailersUtility();
			TU.GetWebPage(url, out TempHTML);

			Match m = Regex.Match(TempHTML, @"wmv-300-.\.(?<trailernumber>\d*)??,");
			string Url = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m.Groups["trailernumber"].Value + "&ru=y&b=8kd6ji91f1vde434d527d";
			GUITrailers.PlayTrailer(Url);
//			Play(PlayUrl);
		}



		public GermanTrailers()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
