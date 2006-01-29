using System;
using System.Net;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Player;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for YahooTrailers.
	/// </summary>
	public class YahooTrailers
	{
		public static string casturl;

        public static string TempXML = string.Empty;
		public static string TempHTML = string.Empty;
        public static string RSSTitle = string.Empty;
		public static string[] TrailersClipsMore = new string[4];

		public static string[] MovieName = new string[2000];
		public static string[] MovieURL = new string[2000];	// strings for all movies

		public static string[] LMovieUrl = new string[200]; // strings for letterbutton movies
		public static string[] LMovieName = new string[200];

		public static string[] RSSMovieUrl = new string[200];
        public static string[] RSSMovieName = new string[200];

		public static string[] Trailers = new string[25];
		public static string[] TrailersUrl = new string[25];

		public static string[] Clips = new string[25];
		public static string[] ClipsUrl = new string[25];

		public static string[] More = new string[25];
		public static string[] MoreUrl = new string[25];

		public static bool foundt = false;		// bools for if trailer, clips or more is found
		public static bool foundc = false;
		public static bool foundm = false;

        public static bool RSSView = false;
		public static bool allview = false; // bools for reminding which view the user is in
		public static bool tcmview = false;
		public static bool tview = false;
		public static bool cview = false;
		public static bool mview = false;
	
		public static string PosterUrl= string.Empty;

        // mediaportal.xml
        public static string server = string.Empty;

		public static void GetTrailers()
		{
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5910));

			TrailersUtility TU = new TrailersUtility();
			TU.GetWebPage(@"http://movies.yahoo.com/trailers/archive/", out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"<a\shref=.(?<trailerurl>/shop.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach (Match m in mc)
			{
				MovieURL[i] = m.Groups["trailerurl"].Value;
				MovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
			GUIPropertyManager.SetProperty("#title", "");
		}


		public static void GetMovieInfo(string url, string name)
		{
			GUIPropertyManager.SetProperty("#title", "Getting moviesinfo...");
			tcmview = true;

			TempHTML = "";
			foundt = false;
			foundc = false;
			foundm = false;
			Array.Clear(Trailers,0,25);
			Array.Clear(TrailersUrl,0,25);
			Array.Clear(Clips,0,25);
			Array.Clear(ClipsUrl,0,25);
			Array.Clear(More,0,25);
			Array.Clear(MoreUrl,0,25);
			Array.Clear(TrailersClipsMore,0,4);

			TrailersUtility TU = new TrailersUtility();
			//TU.GetWebPage(@"http://movies.yahoo.com/"+url, out TempHTML);
            if(url.StartsWith("http://") == false)
                url = "http://movies.yahoo.com/" + url;
            if (url.Contains("trailer") == false)
                url = url.Replace("info", "trailer");

            TU.GetWebPage(url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"(?sx-m).*?(?:</tr>)");

			int t = 0;
			int c = 0;
			int mo = 0;

			foreach (Match m in mc)
			{
				// get trailers & teasers can be more then 1
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/trailers/")!=-1)
				{
					// search for 700 kbit stream
					bool found_700=false;
					if(GUITrailers.bitrate.Equals("300")==false)
					{
						if(m.Value.IndexOf("700-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/trailers/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*).");
							TrailersUrl[t] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";

							Match m2 = Regex.Match(m.Value, @".(?<trailername>.*)</a>");
							Trailers[t] = m2.Groups["trailername"].Value;
							found_700=true;
							t++;
						}
					}
						// if there is no 700 kbit stream then get 300 kbit stream
					if(GUITrailers.bitrate.Equals("300")==true| found_700==false)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/trailers/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*).");
						TrailersUrl[t] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"].Value + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Log.Write(m1.Groups["id"].Value);
						Log.Write(m1.Groups["segment"].Value);
						Log.Write(m1.Groups["movienumber"].Value);

						Match m2 = Regex.Match(m.Value, @".(?<trailername>.*)</a>");
						Trailers[t] = m2.Groups["trailername"].Value;
						Log.Write(m2.Groups["trailername"].Value);
						t++;
					}
					foundt = true;
					TrailersClipsMore[0] = GUILocalizeStrings.Get(5906);
				}
				// search for clips
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/clips/") !=-1)
				{
					bool found_700=false;
					if(GUITrailers.bitrate.Equals("300")==false)
					{
						// search for 700 kbit stream
						if(m.Value.IndexOf("700-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							Clips[c] = m1.Groups["clipsname"].Value;
							found_700=true;
							c++;
						}
						else if(m.Value.IndexOf("700-s") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-700-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							Clips[c] = m1.Groups["clipsname"].Value;
							found_700=true;
							c++;
						}
					}

						// if there is no 700 kbit stream then get 300 kbit stream
					if(GUITrailers.bitrate.Equals("300")==true| found_700==false)
					{
						if(m.Value.IndexOf("300-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							Clips[c] = m1.Groups["clipsname"].Value;
							c++;
						}
						else if(m.Value.IndexOf("300-s") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-300-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							Clips[c] = m1.Groups["clipsname"].Value;
							c++;
						}
					}
					foundc = true;
					TrailersClipsMore[1] = GUILocalizeStrings.Get(5907);
				}
				// search for more "clips"
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/more/")!=-1)
				{
					bool found_700=false;
					if(GUITrailers.bitrate.Equals("300")==false)
					{
						// search for 700 kbit stream
						if(m.Value.IndexOf("700-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							More[mo] = m1.Groups["clipsname"].Value;
							found_700=true;
							mo++;
						}
						else if(m.Value.IndexOf("700-s") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-700-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							More[mo] = m1.Groups["clipsname"].Value;
							found_700=true;
							mo++;
						}
					}
						// if there is no 700 kbit stream then get 300 kbit stream
					if(GUITrailers.bitrate.Equals("300")==true| found_700==false)
					{

						if(m.Value.IndexOf("300-p") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							More[mo] = m1.Groups["clipsname"].Value;
							mo++;
						}
						else if(m.Value.IndexOf("300-s") !=-1)
						{
							Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-300-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
							MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
							More[mo] = m1.Groups["clipsname"].Value;
							mo++;
						}
					}

					foundm = true;
					TrailersClipsMore[2] = GUILocalizeStrings.Get(5908);
				}
			}
            if (TrailersClipsMore[0] == null & TrailersClipsMore[1] != null)
            {
                TrailersClipsMore[0] = TrailersClipsMore[1];
                TrailersClipsMore[1] = TrailersClipsMore[2];
                TrailersClipsMore[2] = null;
            }
			GUITrailers.ptitle = name;
			GUIPropertyManager.SetProperty("#title", GUITrailers.ptitle);
		}
		public static void GetMovieDetails(string url, string name)
		{
			TempHTML="";
			PosterUrl="";
			string Plot="";
			string Genre="";
			string Runtime="";
			string ReleaseDate="";
            if (url.StartsWith("http://") == false)
                url = "http://movies.yahoo.com/" + url;
            url = url.Replace("trailer", "info");

            TrailersUtility TU = new TrailersUtility();
            TU.GetWebPage(url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			// Get PosterUrl
			Match m = Regex.Match(TempHTML,@"<img\ssrc=(?<posterurl>http://us.movies1.yimg.com/movies.yahoo.com/images/.*.jpg)");
			PosterUrl = m.Groups["posterurl"].Value;
			// If Cover is TV-serie example Desperate Housewives
			if(PosterUrl.Equals("")==true)
			{
				Match p = Regex.Match(TempHTML,@"<img\ssrc=(?<posterurl>http://shopping.yahoo.com/video/images/muze/dvd.*.jpg)");
				PosterUrl = p.Groups["posterurl"].Value;
			}
			// Get MoviePlot
            Plot = Regex.Match(TempHTML, @"(.*\.\.\.)&nbsp").Groups[1].Value.ToString();
            if (Plot == string.Empty)
            {
                int EP = TempHTML.IndexOf("<br clear=\"all\"");
                int BP = TempHTML.LastIndexOf("size=-1>", EP) + 8;
                int TP = EP - BP;
                Plot = TempHTML.Substring(BP, TP);
                Plot = Plot.Replace("</font>","");
            }
            GUIPropertyManager.SetProperty("#plot", Plot);

			// Get Genre
            if (Regex.Match(TempHTML, @"<!--\sno\sresult\sfor\sGenres\s-->").Success == false)
                Genre = Regex.Match(TempHTML, @"(?isx-mn:genres:.*?size=-1>(.*?)</font)").Groups[1].Value.ToString();
            GUIPropertyManager.SetProperty("#genre", Genre);

            // Get Runtime
            if (Regex.Match(TempHTML, @"<!--\sno\sresult\sfor\sRunning\sTime\s-->").Success == false)
                Runtime = Regex.Match(TempHTML, @"(?isx-mn:Running\sTime:.*?size=-1>(.*?)</font)").Groups[1].Value.ToString();
			GUIPropertyManager.SetProperty("#runtime", Runtime);

			// Get ReleaseDate
            if (Regex.Match(TempHTML, @"<!--\sno\sresult\sfor\sRelease\sDate\s-->").Success == false)
                ReleaseDate = Regex.Match(TempHTML, @"(?isx-mn:Release\sDate:.*?size=-1>(.*?)</font)").Groups[1].Value.ToString();
			GUIPropertyManager.SetProperty("#year", ReleaseDate);

            casturl = Regex.Match(TempHTML, @"<A\sHRef=.(.*/cast)").Groups[1].Value.ToString();

			// Get Rating
			Match r = Regex.Match(TempHTML,@"Average\sGrade.*.grade.>(?<rating>.*)<");
			switch(r.Groups["rating"].Value)
			{
				case "A+":
					GUITrailers.prating = 10;
					return;
				case "A":
					GUITrailers.prating = 9;
					return;
				case "A-":
					GUITrailers.prating = 8.5;
					return;
				case "B+":
					GUITrailers.prating = 8;
					return;
				case "B":
					GUITrailers.prating = 7;
					return;
				case "B-":
					GUITrailers.prating = 6.5;
					return;
				case "C+":
					GUITrailers.prating = 6;
					return;
				case "C":
					GUITrailers.prating = 5;
					return;
				case "C-":
					GUITrailers.prating = 4.5;
					return;
				case "D+":
					GUITrailers.prating = 4;
					return;
				case "D":
					GUITrailers.prating = 3;
					return;
				case "D-":
					GUITrailers.prating = 2.5;
					return;
				case "F":
					GUITrailers.prating = 1;
					return;
			}
			GUIPropertyManager.SetProperty("#rating", GUITrailers.prating.ToString());
			
			//casturl = url;
		}
		public static void GetCastInfo(string url)
		{
            TempHTML="";
            //string casturl;
            string cast = "";

            //casturl = "http://movies.yahoo.com/"+url;
            //casturl = casturl.Replace("trailer","cast");

            //TrailersUtility TU = new TrailersUtility();
            //TU.GetWebPage(casturl, out TempHTML);
            TrailersUtility TU = new TrailersUtility();
            TU.GetWebPage(url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

            MatchCollection mc = Regex.Matches(TempHTML, @"./shop.*?>(?<cast>.*?)<");

			foreach(Match m in mc)
			{
				if(cast.Equals(""))
				{
					cast = m.Groups["cast"].Value;
				}
				else
				{
					cast = cast.Insert(cast.Length,", ");
					cast = cast.Insert(cast.Length, m.Groups["cast"].Value);
				}
			}
			GUIPropertyManager.SetProperty("#cast", cast);
		}

        public static void GetYahooTrailersRSS(string xmlurl)
        {
            Array.Clear(RSSMovieName, 0, 200);
            Array.Clear(RSSMovieUrl, 0, 200);

            TrailersUtility TU = new TrailersUtility();
            TU.GetWebPage(xmlurl, out TempXML);

            if (TempXML == null || TempXML == string.Empty)
                return;

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(TempXML);
            XmlNodeList MovieTitle = XmlDoc.SelectNodes("/rss/channel/item/title");
            XmlNodeList MovieLink = XmlDoc.SelectNodes("/rss/channel/item/link");
            if (MovieTitle == null)
                return;

            for (int i = 0; i < MovieTitle.Count; i++)
            {
                RSSMovieName[i] = MovieTitle[i].InnerText;
                Byte[] encodedBytes = Encoding.ASCII.GetBytes(MovieLink[i].InnerText);
                RSSMovieUrl[i] = Encoding.ASCII.GetString(encodedBytes);
            }
        }
        
        




		public YahooTrailers()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
