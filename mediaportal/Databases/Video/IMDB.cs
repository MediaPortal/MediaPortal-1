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
using System.Net;
using System.Web;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.Video.Database
{
	/// <summary>
	/// supporting classes to fetch movie information out of different databases
	/// currently supported: IMDB http://us.imdb.com and OFDB http://www.ofdb.de
	/// 
	/// @ 21.09.2004 FlipGer
	/// - renamend Find() to FindIMDB()
	/// - renamend GetDetails to GetDetailsIMDB()
	/// - minor changes to FindIMDB() and GetDetailsIMDB() to support mulitple databases
	/// - renamend ParseAHREF() to ParseAHREFIMDB() 
	/// - ParseGenres() to ParseGenresIMDB()
	/// - rewritten Find() and GetDetails() to support mulitple databases
	/// - new method GetPage() to load a webpage
	/// - new method LoadSettings() called in constructor to fetch database settings
	/// - new attributes aLimits and aDatabases to store the settings
	/// - new methods FindOFDB(), GetDetailsOFDB() and ParseListOFDB() to support OFDB
	/// - renamend and minor changes from GetMovie() to GetSearchString(), i think this name suits better
	/// 
	/// @ 27.09.2004 FlipGer
	/// - GetSearchString()
	/// * major changes 
	/// * bug in lookup for "2001 a space odyssey" solved
	/// * changed bracket skipping
	/// - only strTest.Trim(); does not work! changed every occurence to strTest = strTest.Trim();
	/// 
	/// </summary>
	public class IMDB: IEnumerable
	{
		public interface IProgress
		{
			void OnProgress(string line1, string line2, string line3, int percent);
		}
		// class that represents URL and Title of a search result
		public class IMDBUrl
		{
			string m_strURL="";
			string m_strTitle="";
			string m_strDatabase="";

			public IMDBUrl(string strURL, string strTitle, string strDB)
			{
				m_strURL = strURL;
				m_strTitle = strTitle;
				m_strDatabase = strDB;
			}

			public string URL
			{
				get { return m_strURL;}
				set { m_strURL=value;}
			}

			public string Title
			{
				get { return m_strTitle;}
				set { m_strTitle=value;}
			}

			public string Database
			{
				get { return m_strDatabase; }
				set { m_strDatabase = value; }
			}
		}; // END class IMDBUrl


		// do not know, what this class does ;-)
		public class IMDBEnumerator: IEnumerator
		{
			private int position = -1;
			private IMDB t;

			public IMDBEnumerator(IMDB t)
			{
				this.t = t;
			}

			public bool MoveNext()
			{
				if (position < t.elements.Count - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				position = -1;
			}

			public IMDB.IMDBUrl Current // non-IEnumerator version: type-safe
			{
				get
				{
					if (t.elements.Count==0) return null;
					return (IMDB.IMDBUrl)t.elements[position];
				}
			}

			object IEnumerator.Current // IEnumerator version: returns object
			{
				get
				{
					if (t.elements.Count==0) return null;
					return t.elements[position];
				}
			}
		} // END class IMDBEnumerator

		// internal vars
		// list of the search results, containts objects of IMDBUrl
		ArrayList elements = new ArrayList();

		// Arrays for multiple database support
		int[]		aLimits;		// contains the limit for searchresults
		string[]	aDatabases;		// contains the name of the database, e.g. IMDB

		IProgress m_progress;
		// constructor
		public IMDB() : this(null)
		{
		}

		public IMDB(IProgress progress)
		{
			m_progress=progress;
			// load the settings
			LoadSettings();
		} // END constructor

		// load settings from mediaportal.xml
		private void LoadSettings()
		{
			// getting available databases and limits
			using(MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				int iNumber = xmlreader.GetValueAsInt("moviedatabase","number",0);
				if (iNumber<=0)
				{
					// no given databases in XML - setting to IMDB
					aLimits = new int[1];
					aDatabases = new string[1];
					aLimits[0] = 25;
					aDatabases[0] = "IMDB";
				}
				else
				{
					// initialise arrays
					aLimits = new int[iNumber];
					aDatabases = new string[iNumber];
					string	strDatabase;
					int		iLimit;
					bool	bDouble = false;
					// get the databases
					for(int i = 0;i < iNumber;i++)
					{
						bDouble = false;
						iLimit = xmlreader.GetValueAsInt("moviedatabase","limit"+i.ToString(),25);
						strDatabase = xmlreader.GetValueAsString("moviedatabase","database"+i.ToString(),"IMDB");
						// be aware of double entries!
						for(int j = 0;j < i; j++)
						{
							if (aDatabases[j] == strDatabase)
							{
								// double entry found, exit search
								bDouble = true;
								j = i;
							}
						}
						// valid entry?
						if (!bDouble)
						{
							// entry does not exist yet
							aLimits[i] = iLimit;
							aDatabases[i] = strDatabase;
						}
						else
						{
							// skip this entry
							aLimits[i] = 0;
							aDatabases[i] = "";
						}
					}
				}
			}
		} // END LoadSettings()

		// count the elements
		public int Count
		{
			get {return elements.Count;}
		} // END Count

		//??
		public IMDB.IMDBUrl this[int index]
		{
			get {  return (IMDB.IMDBUrl)elements[index];}
		} // END IMDB.IMDBUrl this[int index]

		//??
		public IMDBEnumerator GetEnumerator() // non-IEnumerable version
		{
			return new IMDBEnumerator(this);
		} // END IMDBEnumerator GetEnumerator()

		//??
		IEnumerator IEnumerable.GetEnumerator() // IEnumerable version
		{
			return (IEnumerator) new IMDBEnumerator(this);
		} // END IEnumerable.GetEnumerator()

		// trys to get a webpage from the specified url and returns the content as string
		private string GetPage(string strURL, string strEncode, out string absoluteUri)
		{
			string strBody = "";
			absoluteUri=String.Empty;
			try
			{
				// Make the Webrequest
				Log.Write("IMDB: get page:{0}", strURL);
				WebRequest req = WebRequest.Create(strURL);
				WebResponse result = req.GetResponse();
				Stream ReceiveStream = result.GetResponseStream();

				// Encoding: depends on selected page
				Encoding encode = System.Text.Encoding.GetEncoding(strEncode);
				StreamReader sr = new StreamReader( ReceiveStream, encode );
				strBody = sr.ReadToEnd();

				absoluteUri=result.ResponseUri.AbsoluteUri;
				sr.Close();
				ReceiveStream.Close();
				result.Close();
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message,ex.StackTrace);
			}
			return strBody;
		} // END GetPage()

		// cuts end of sting after strWord
		void RemoveAllAfter(ref string strLine, string strWord)
		{
			int iPos=strLine.IndexOf(strWord);
			if (iPos>0)
			{
				strLine=strLine.Substring(0,iPos);
			}
		} // END RemoveAllAfter()

		// make a searchstring out of the filename
		string GetSearchString(string strMovie)
		{
			string strURL = strMovie;
			strURL = strURL.ToLower();
			strURL = strURL.Trim();

			// @ 23.09.2004 by FlipGer
			if (strURL.Length<=7)
			{
				return strURL;
			}
			// END @ | i think it does not make much sense to parse such a short string
			// and i have no problems with x-men on OFDB ;-) and a better result on IMDB with x-men (try out "x men" :-)

			string	strTmp="";
			int		ipos=0;
			int		iBracket = 0;
			//bool	bSkip = false;
			for (int i=0; i < strURL.Length;++i)
			{
				/* Why are numbers bigger than 999 skipped?
				for (int c=0;i+c < strURL.Length&&Char.IsDigit(strURL[i+c]);c++)
				{
					Log.Write("c: {0}",c);
					if (c==3)
					{
						i+=4;
						break;
					}
				}*/
				//if (i >=strURL.Length) break;
				char kar=strURL[i];
				if (kar == '[' || kar=='(' ) iBracket++;			//skip everthing between () and []
				else if (kar == ']' || kar==')' ) iBracket--;
				else if (iBracket<=0)
				{
					// change all non cahrs or digits into ' '
					if (!Char.IsLetterOrDigit(kar))
					{
						kar = ' ';

					}
					// skip whitespace at the beginning, only necessary if the "number skipping" is used
					//if ((kar==' ') && (ipos==0)) continue;

					// Mars Warrior @ 03-sep-2004.
					// Check for ' ' and '+' to avoid double or more ' ' and '+' which
					// mess up the search to the IMDB...
					if (strTmp.Length==0)
					{
						strTmp += kar;
						ipos++;
					}
					else
					{
						if	(
							Char.IsLetterOrDigit(kar)						||
							(kar == ' ' && strTmp[strTmp.Length -1] != ' ')
							//|| (kar == '+' && strTmp[strTmp.Length -1] != '+')
							)
						{
							strTmp += kar;
							ipos++;
						}
					}
				}
			}

			strTmp = strTmp.Trim();

			// Mars Warrior @ 03-sep-2004.
			// The simple line "strTmp.ToLower()" does NOT work. As a result the wrong string
			// (still includes the " dvd" etc. strings) is send to the IMDB causing wrong lookups
			// By changing the line, everything is working MUCH better now ;-)

			RemoveAllAfter(ref strTmp," divx");
			RemoveAllAfter(ref strTmp," xvid");
			RemoveAllAfter(ref strTmp," dvd");
			//RemoveAllAfter(ref strTmp," dvdrip"); already done by " dvd" i think
			RemoveAllAfter(ref strTmp," svcd");
			RemoveAllAfter(ref strTmp," mvcd");
			RemoveAllAfter(ref strTmp," vcd");
			RemoveAllAfter(ref strTmp," cd");
			RemoveAllAfter(ref strTmp," ac3");
			RemoveAllAfter(ref strTmp," ogg");
			RemoveAllAfter(ref strTmp," ogm");
			RemoveAllAfter(ref strTmp," internal");
			RemoveAllAfter(ref strTmp," fragment");
			RemoveAllAfter(ref strTmp," proper");
			RemoveAllAfter(ref strTmp," limited");
			RemoveAllAfter(ref strTmp," rerip");

			RemoveAllAfter(ref strTmp,"+divx");
			RemoveAllAfter(ref strTmp,"+xvid");
			RemoveAllAfter(ref strTmp,"+dvd");
			//RemoveAllAfter(ref strTmp,"+dvdrip"); already done by " dvd" i think
			RemoveAllAfter(ref strTmp,"+svcd");
			RemoveAllAfter(ref strTmp,"+mvcd");
			RemoveAllAfter(ref strTmp,"+vcd");
			RemoveAllAfter(ref strTmp,"+cd");
			RemoveAllAfter(ref strTmp,"+ac3");
			RemoveAllAfter(ref strTmp,"+ogg");
			RemoveAllAfter(ref strTmp,"+ogm");
			RemoveAllAfter(ref strTmp,"+internal");
			RemoveAllAfter(ref strTmp,"+fragment");
			RemoveAllAfter(ref strTmp,"+proper");
			RemoveAllAfter(ref strTmp,"+limited");
			RemoveAllAfter(ref strTmp,"+rerip");

			// return the new formatted string
			return strTmp;
		} // END GetSearchString()

		// this method switches between the different databases to get the search results
		public void Find(string strMovie)
		{
			try
			{
				string strURL;
				// getting searchstring
				string strSearch = HttpUtility.UrlEncode(GetSearchString(strMovie));

				// be aware of german special chars äöüß Ã¤Ã¶Ã¼ÃŸ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
				strSearch = strSearch.Replace("%c3%a4","%E4");
				strSearch = strSearch.Replace("%c3%b6","%F6");
				strSearch = strSearch.Replace("%c3%bc","%FC");
				strSearch = strSearch.Replace("%c3%9f","%DF");

				elements.Clear();			

				string line1,line2,line3;
				line1=GUILocalizeStrings.Get(984);
				line2=GetSearchString(strMovie).Replace("+"," ");
				line3="";
				int percent=0;

				if(m_progress != null)
					m_progress.OnProgress(line1,line2,line3,percent);
				// search the desired databases
				for (int i = 0; i < aDatabases.Length; i++)
				{
					// only do a search if requested
					if (aLimits[i]>0)
					{
						switch (aDatabases[i].ToUpper())
						{

							case "IMDB":
								// IMDB support
							
								line1=GUILocalizeStrings.Get(984) +":IMDB";
								percent=0;
								if(m_progress != null)
									m_progress.OnProgress(line1,line2,line3,percent);
								strURL = "http://us.imdb.com/Tsearch?title="+strSearch;
								FindIMDB(strURL,aLimits[i]);
								percent=33;
								if(m_progress != null)
									m_progress.OnProgress(line1,line2,line3,percent);
								// END IMDB support
								break;
							case "OFDB":
								// OFDB support
								line1=GUILocalizeStrings.Get(984) +":OFDB";
								percent=33;
								if(m_progress != null)
									m_progress.OnProgress(line1,line2,line3,percent);
								strURL = "http://www.ofdb.de/view.php?page=suchergebnis&Kat=All&SText="+strSearch;
								FindOFDB(strURL,aLimits[i]);
								percent=66;
								if(m_progress != null)
									m_progress.OnProgress(line1,line2,line3,percent);
								// END OFDB support
								break;

							case "FRDB":
								// FRDB support
								percent=66;
								if(m_progress != null)
									m_progress.OnProgress(line1,line2,line3,percent);
								line1=GUILocalizeStrings.Get(984) +":FRDB";
								strURL = "http://www.dvdfr.com/search/search.php?multiname="+strSearch;
								FindFRDB(strURL,aLimits[i]);
								percent=100;
								if(m_progress != null)
									m_progress.OnProgress(line1,line2,line3,percent);
								// END FRDB support
								break;

							default:
								// unsupported database?
								Log.WriteFile(Log.LogType.Log,true,"Movie database lookup - database not supported: {0}",aDatabases[i].ToUpper());
								break;
						}
					}
				}
			}
			catch(Exception)
			{
			}
		} // END Find()

		#region actors
		public void FindActor(string strActor)
		{
			string strURL;
			// getting searchstring
			string strSearch = HttpUtility.UrlEncode(GetSearchString(strActor));

			// be aware of german special chars äöüß Ã¤Ã¶Ã¼ÃŸ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
			strSearch = strSearch.Replace("%c3%a4","%E4");
			strSearch = strSearch.Replace("%c3%b6","%F6");
			strSearch = strSearch.Replace("%c3%bc","%FC");
			strSearch = strSearch.Replace("%c3%9f","%DF");

			elements.Clear();			

			string line1,line2,line3;
			line1=GUILocalizeStrings.Get(986);
			line2=strActor;
			line3="";
			int percent=0;
			if (m_progress!=null) m_progress.OnProgress(line1,line2,line3,percent);
			percent=0;
			if (m_progress!=null) m_progress.OnProgress(line1,line2,line3,percent);
			strURL=String.Format( "http://us.imdb.com/find?q={0};nm=on;mx=20",strSearch);
			FindIMDBActor(strURL,strActor);

		} // END FindActor()

		private void FindIMDBActor(string strURL, string strActor)
		{
			try
			{
				HTMLUtil htmlUtil=new HTMLUtil();
				string absoluteUri;
				string strBody = GetPage(strURL,"utf-8", out absoluteUri);
				int posStart=0,posEnd;
				posStart=strBody.IndexOf("<a name=\"headshot");
				if (posStart>0)
				{
					IMDBUrl oneUrl =new IMDBUrl(strURL,strActor,"IMDB");
					elements.Add(oneUrl);
					return;
				}
				posStart=0;
				do
				{

					//<a href="/name/nm0000246/" onclick="set_args('nm0000246', 1)">Bruce Willis</a>
					posStart=strBody.IndexOf("<a href=\"/name/",posStart);
					if (posStart < 0)  break;
					
					posEnd=strBody.IndexOf(@"</a>",posStart);
					if (posEnd < 0)  break;
				
					string ahref=strBody.Substring(posStart, (posEnd+4)-posStart);
					string title,url;
					htmlUtil.ParseAHREF(ahref, out title, out url);
					title=Utils.stripHTMLtags(title);
					htmlUtil.ConvertHTMLToAnsi(title,out title);
					IMDBUrl newUrl =new IMDBUrl("http://us.imdb.com"+url,title,"IMDB");
					elements.Add(newUrl);
					posStart=posEnd;
				}while(true);
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message,ex.StackTrace);
			}
		}
		public bool GetActorDetails(IMDB.IMDBUrl url, out IMDBActor actor)
		{
			actor = new IMDBActor();
			try
			{
				//<a name="headshot" href="photogallery"><img border="0" src="http://ia.imdb.com/media/imdb/01/I/84/36/12m.jpg" width="100" height="140" alt="Bruce Willis (I)"></a>
				string absoluteUri;
				string strBody=GetPage(url.URL,"utf-8", out absoluteUri);
				if (strBody==null) return false;
				if (strBody.Length==0) return false;


				//get picture
				int posStart=strBody.IndexOf("<a name=\"headshot");
				if (posStart<0) return false;
				posStart=strBody.IndexOf("<img",posStart);

				int posEnd=strBody.IndexOf(">",posStart);
				if (posEnd<0) return false;
				string imgTag=strBody.Substring(posStart,posEnd-posStart);
				string strURL="";
				HTMLUtil util= new HTMLUtil();
				util.getAttributeOfTag(imgTag, "src=\"", ref strURL);
				if (strURL==null)
					util.getAttributeOfTag(imgTag, "SRC=\"", ref strURL);

				actor.Name=url.Title;
				actor.ThumbnailUrl=strURL;

				//find date of birth
				//<a href="/OnThisDay?day=11&month=November">11 November</a> <a href="/BornInYear?1962">1962</a><br>
				posStart=strBody.IndexOf("/OnThisDay?");
				if (posStart>0)
				{
					posStart=strBody.IndexOf(">",posStart);
					posStart++;
					posEnd=strBody.IndexOf("<",posStart);
					string birthDay=strBody.Substring(posStart,posEnd-posStart);
					posStart=strBody.IndexOf("/BornInYear?");
					if (posStart>0)
					{
						posStart=strBody.IndexOf(">",posStart);
						posStart++;
						posEnd=strBody.IndexOf("<",posStart);
						birthDay+=" "+strBody.Substring(posStart,posEnd-posStart);
					}
					actor.DateOfBirth=birthDay;
				}
				//find place of birth
				//<a href="/BornWhere?Roswell,%20New%20Mexico,%20USA">Roswell, New Mexico, USA</a>
				posStart=strBody.IndexOf("/BornWhere?");
				if (posStart>0)
				{
					posStart=strBody.IndexOf(">",posStart);
					posStart++;
					posEnd=strBody.IndexOf("<",posStart);
					actor.PlaceOfBirth=util.ConvertHTMLToAnsi(strBody.Substring(posStart,posEnd-posStart));
				}
				
				//find Mini Biography
				//<dt><div class="ch">Mini biography</div></dt>
				//<dd><a href="/name/nm0000193/">Demi Moore</a> was born 1962 in Roswell, New Mexico. Her father left her mother... <a href="bio">(show more)</a></dd>
				//</dl>

				posStart=strBody.IndexOf("Mini biography");
				if (posStart>0)
				{
					posStart=strBody.IndexOf("<a href",posStart);
					posEnd=strBody.IndexOf("<a ",posStart+3);
					string href=strBody.Substring(posStart,posEnd-posStart);
					actor.MiniBiography=Utils.stripHTMLtags(href).Trim();

					//get complete biography
					string bioURL=absoluteUri;
					int pos=bioURL.IndexOf("?");
					if (pos > 0) 
						bioURL=bioURL.Substring(0,pos);

					string strBioBody=GetPage(bioURL+"bio","utf-8",out absoluteUri);
					if (strBioBody!=null && strBioBody.Length>0)
					{

						posStart=strBioBody.IndexOf("biopar");
						if (posStart>0)
						{
							posStart=strBioBody.IndexOf(">",posStart);
							posStart++;
							posEnd=strBioBody.IndexOf("</p>",posStart);
							href=strBioBody.Substring(posStart,posEnd-posStart);
							href=Utils.stripHTMLtags(href).Trim();
							actor.Biography=util.ConvertHTMLToAnsi(href);
						}
					}
				}

				// get movie list
				posStart=strBody.IndexOf("<ol>");
				posEnd=strBody.IndexOf("</ol",posStart);
				string movieList=strBody.Substring(posStart,posEnd-posStart);
				posStart=0;
				while (true)
				{
					int movieStart=movieList.IndexOf("<li>",posStart);
					if (movieStart<0) break;
					int movieEnd=movieList.IndexOf("</li>",movieStart);
					//<li><a href="/title/tt0257778/">The Hunchback of Notre Dame II</a> (2002) (V)  (voice)   .... Esmeralda</li>
					string movie=movieList.Substring(movieStart,movieEnd-movieStart);
					int titleStart=movie.IndexOf("<a");
					int titleEnd=movie.IndexOf("</a>");
					string titleHREF=movie.Substring(titleStart,titleEnd+3-titleStart);
					string title,titleUrl;
					util.ParseAHREF(titleHREF,out title, out titleUrl);
					title=util.ConvertHTMLToAnsi(title);
					movie=Utils.stripHTMLtags(movie);
					movie=util.ConvertHTMLToAnsi(movie);
					int year=0;
					int yearStart=movie.IndexOf("(19");
					if (yearStart<0)
						yearStart=movie.IndexOf("(20");
					if (yearStart>0)
					{
						year=Int32.Parse(movie.Substring(yearStart+1,4));
					}

					IMDBActor.IMDBActorMovie actorMovie = new IMDBActor.IMDBActorMovie();
					actorMovie.MovieTitle=title;
					actorMovie.Role=movie;
					actorMovie.Year=year;
					actor.Add(actorMovie);
					posStart=movieEnd;
				}
				return true;
			}
			catch(Exception ex)
			{
				Log.Write("IMDB.GetActorDetails({0} exception:{1} {2} {3}", url.URL,ex.Message,ex.Source,ex.StackTrace);
			}
			return false;
		}


		#endregion
		// this method switches between the different databases to fetche the search result into movieDetails
		public bool GetDetails(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
		{
			try
			{
				/*
				// extract host from url, to find out which mezhod should be called
				int		iStart = url.URL.IndexOf(".")+1;
				int		iEnd = url.URL.IndexOf(".",iStart);
				if ((iStart<0) || (iEnd<0))
				{
					// could not extract hostname!
					Log.Write("Movie DB lookup GetDetails(): could not extract hostname from {0}",url.URL);
					return false;
				}
				string	strHost = url.URL.Substring(iStart,iEnd-iStart).ToUpper();*/
				switch (url.Database)
				{
					case "IMDB":
						return	GetDetailsIMDB(url, ref movieDetails);
					case "OFDB":
						return	GetDetailsOFDB(url, ref movieDetails);

					case "FRDB":
						return	GetDetailsFRDB(url, ref movieDetails);

					default:
						// Not supported Database / Host
						Log.WriteFile(Log.LogType.Log,true,"Movie DB lookup GetDetails(): Unknown Database {0}",url.Database);
						return	false;
				}
			}
			catch (Exception)
			{
			}
			return false;
		} // END GetDetails()

		
		#region IMDB
		private void FindIMDB(string strURL,int iLimit)
		{
			int		iCount = 0;
			string	strTitle;
			try
			{
				string absoluteUri;
				string strBody = GetPage(strURL,"utf-8",out absoluteUri);

				// Mars Warrior @ 03-sep-2004.
				// First try to find an Exact Match. If no exact match found, just look
				// for any match and add all those to the list. This narrows it down more easily...
				int iStartOfMovieList = strBody.IndexOf("Popular Titles");
				if (iStartOfMovieList<0) iStartOfMovieList=strBody.IndexOf("Exact Matches");
				if (iStartOfMovieList<0) iStartOfMovieList=strBody.IndexOf("Partial Matches");
				if (iStartOfMovieList<0) iStartOfMovieList=strBody.IndexOf("Approx Matches");

				int endOfTitleList = strBody.IndexOf("Suggestions For Improving Your Results");
				if (iStartOfMovieList<0)
				{
					int iMovieTitle	 = strBody.IndexOf("\"title\">");
					int iMovieDirector = strBody.IndexOf("Directed");
					int iMovieGenre	 = strBody.IndexOf("Genre:");
					int iMoviePlot	 = strBody.IndexOf("Plot");

					if (iMovieTitle >=0 && iMovieDirector>=0 && iMovieGenre>=0 && iMoviePlot>=0)
					{
						int iEnd=strBody.IndexOf("<",iMovieTitle+8);
						if (iEnd>0)
						{
							iMovieTitle+="\"title\">".Length;
							strTitle=strBody.Substring(iMovieTitle,iEnd-iMovieTitle);
							strTitle=Utils.stripHTMLtags(strTitle);
							HTMLUtil htmlUtil= new HTMLUtil();
							htmlUtil.ConvertHTMLToAnsi(strTitle,out strTitle);
							IMDBUrl url =new IMDBUrl(strURL,strTitle+" (imdb)","IMDB");
							elements.Add(url);
						}
					}
					return;
				}

				iStartOfMovieList+="<table>".Length;
				int iEndOfMovieList=strBody.IndexOf("</table>",iStartOfMovieList);

				if (iEndOfMovieList<0)
				{
					iEndOfMovieList=strBody.Length;
				}
				if (endOfTitleList < iEndOfMovieList && endOfTitleList>iStartOfMovieList)
				{
					iEndOfMovieList=endOfTitleList;
				}
				strBody=strBody.Substring(iStartOfMovieList,iEndOfMovieList-iStartOfMovieList);
				while ((true) && (iCount<iLimit))
				{
					////<A HREF="/Title?0167261">Lord of the Rings: The Two Towers, The (2002)</A>
					int iAHREF=strBody.IndexOf("<a href=");
					if (iAHREF>=0)
					{
						int iEndAHREF=strBody.IndexOf("</a>");
						if (iEndAHREF>=0)
						{
							iAHREF+="<a href=.".Length;
							string strAHRef=strBody.Substring(iAHREF, iEndAHREF-iAHREF);
							int iURL=strAHRef.IndexOf(">");
							if (iURL>0)
							{
								strTitle="";
								strURL=strAHRef.Substring(0,iURL);
								if (strURL[strURL.Length-1]=='\"') 
									strURL=strURL.Substring(0,strURL.Length-1);
								iURL++;
								int iURLEnd=strAHRef.IndexOf("<",iURL);
								if (iURLEnd>0)
								{
									strTitle=strAHRef.Substring(iURL,iURLEnd-iURL);
								}
								else strTitle=strAHRef.Substring(iURL);
								
								int onclick=strURL.IndexOf(" onclick");
								if (onclick>=0) strURL=strURL.Substring(0,onclick-1);
								strURL=String.Format("http://us.imdb.com{0}", strURL);
								HTMLUtil htmlUtil= new HTMLUtil();
								htmlUtil.ConvertHTMLToAnsi(strTitle,out strTitle);


								int endTagLength="</a>".Length;
								int posNextTag=strBody.IndexOf("<",iEndAHREF+endTagLength);
								if (posNextTag>0)
								{
									string strSub=strBody.Substring(iEndAHREF+endTagLength, posNextTag- (iEndAHREF+endTagLength));
									strTitle+=strSub;
								}

								if (strTitle.IndexOf("\n")<0)
								{
									IMDBUrl url =new IMDBUrl(strURL,strTitle+" (imdb)","IMDB");
									elements.Add(url);
								}
								iCount++;
							}
							if (iEndAHREF+1>=strBody.Length) break;
							iStartOfMovieList = iEndAHREF+1;
							strBody=strBody.Substring(iEndAHREF+1);
						}
						else
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message,ex.StackTrace);
			}
		}

		private bool GetDetailsIMDB(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
		{
			try
			{

				int iStart=0;
				int iEnd=0;
				movieDetails.Reset();
				// add databaseinfo
				movieDetails.Database = "IMDB";

				string strAbsURL;
				string strBody=GetPage(url.URL,"utf-8",out strAbsURL);
				if (strBody==null) return false;
				if (strBody.Length==0) return false;

				int iPos=strAbsURL.IndexOf("/title/");
				if (iPos>0)
				{
					iPos+="/title/".Length;
					movieDetails.IMDBNumber=strAbsURL.Substring(iPos);
					int pos=movieDetails.IMDBNumber.IndexOf("/");
					if (pos>0)
						movieDetails.IMDBNumber=movieDetails.IMDBNumber.Substring(0,pos);
				}

				url.Title = url.Title.Trim();
				// cut of " (imdb)"
				iEnd = url.Title.IndexOf("(");
				if (iEnd >= 0)
					movieDetails.Title = url.Title.Substring(0,iEnd);
				else
					movieDetails.Title = url.Title;
				int iDirectedBy=strBody.IndexOf("Directed by");
				int iCredits=strBody.IndexOf("Writing credits");
				int iGenre=strBody.IndexOf("Genre:");
				int iTagLine=strBody.IndexOf("Tagline:</b>");	
				int iPlotOutline=strBody.IndexOf("Plot Outline:</b>");	
				int iPlotSummary=strBody.IndexOf("Plot Summary:</b>");	
				int iPlot=strBody.IndexOf("<a href=\"plotsummary");
				int iImage=strBody.IndexOf("<img alt=\"cover\" align=\"left\" src=\"");
				if (iImage>=0)
				{
					iImage+="<img alt=\"cover\" align=\"left\" src=\"".Length;
				}
				else
				{
					iImage=strBody.IndexOf("<img border=\"0\" alt=\"cover\" src=\"");
					if (iImage>=0) iImage+="<img border=\"0\" alt=\"cover\" src=\"".Length;
					else 
					{
						int ipos=strBody.IndexOf("alt=\"cover\"");
						if (ipos>0)
						{
							ipos=strBody.IndexOf("src=\"",ipos);
							if (ipos>0) iImage=ipos+"src=\"".Length;
						}
					}
				}
				int iRating=strBody.IndexOf("User Rating:</b>");
				int iCast=strBody.IndexOf("first billed only: </b></td></tr>");
				int iCred=strBody.IndexOf("redited cast:"); // Complete credited cast or Credited cast
				int iTop=strBody.IndexOf( "top 250:");
				int iYear=strBody.IndexOf("/Sections/Years/");
				if (iYear>=0)
				{	
					iYear += "/Sections/Years/".Length;
					string strYear=strBody.Substring(iYear,4);
					movieDetails.Year=System.Int32.Parse(strYear);
				}

				if (iDirectedBy>=0) 
					movieDetails.Director=ParseAHREFIMDB(strBody,iDirectedBy, url.URL ).Trim();

				if (iCredits>=0) 
					movieDetails.WritingCredits=ParseAHREFIMDB(strBody,iCredits, url.URL ).Trim();

				if (iGenre>=0)  
					movieDetails.Genre=ParseGenresIMDB(strBody, iGenre, url.URL ).Trim();

				if (iRating>=0) // and votes
				{
					iRating+="User Rating:</b>".Length;
					iStart = strBody.IndexOf("<b>",iRating); 
					if(iStart >=0) 
					{
						iStart+="<b>".Length;
						iEnd = strBody.IndexOf("/",iStart);

						// set rating
						string strRating=strBody.Substring(iStart,iEnd-iStart);
            if (strRating !=String.Empty) strRating = strRating.Replace('.',','); 
						try
						{
							movieDetails.Rating = (float)System.Double.Parse(strRating);
							if (movieDetails.Rating>10.0f) movieDetails.Rating/=10.0f;
						}
						catch(Exception)
						{
						}

						if(movieDetails.Rating != 0.0f) 
						{
							// now, votes
							movieDetails.Votes = "0";
							iStart = strBody.IndexOf("(",iEnd+2);
							if(iStart>0)
							{
								iEnd = strBody.IndexOf(" votes)",iStart);
								if(iEnd>0) 
								{
									iStart++; // skip the parantese before votes
									movieDetails.Votes = strBody.Substring(iStart,iEnd-iStart).Trim();
								}
							} 
						}
					}
				}

				if(iTop>=0) // top rated movie :)
				{
					iTop += "top 250:".Length + 2; // jump space and #
					iEnd=strBody.IndexOf("</a>",iTop);
					string strTop=strBody.Substring(iTop,iEnd-iTop);
					movieDetails.Top250 = System.Int32.Parse( strTop );
				}
				if (iTagLine>=0)
				{
					iTagLine += "Tagline:</b>".Length;
					iEnd = strBody.IndexOf("<",iTagLine);
					movieDetails.TagLine=strBody.Substring(iTagLine, iEnd-iTagLine).Trim();
					movieDetails.TagLine=Utils.stripHTMLtags(movieDetails.TagLine);
					movieDetails.TagLine=HttpUtility.HtmlDecode(movieDetails.TagLine);  // Remove HTML entities like &#189;
				}

				if (iPlotOutline<0)
				{
					if (iPlotSummary>0)
					{
						iPlotSummary += "Plot Summary:</b>".Length;
						iEnd = strBody.IndexOf("<",iPlotSummary);
						movieDetails.PlotOutline=strBody.Substring(iPlotSummary, iEnd-iPlotSummary).Trim();
						movieDetails.PlotOutline=Utils.stripHTMLtags(movieDetails.PlotOutline);
						movieDetails.PlotOutline=HttpUtility.HtmlDecode(movieDetails.PlotOutline);  // remove HTML entities
					}
				}
				else
				{
					iPlotOutline += "Plot Outline:</b>".Length;
					iEnd = strBody.IndexOf("<",iPlotOutline);
					movieDetails.PlotOutline=strBody.Substring(iPlotOutline, iEnd-iPlotOutline).Trim();
					movieDetails.PlotOutline=Utils.stripHTMLtags(movieDetails.PlotOutline);
					movieDetails.PlotOutline=HttpUtility.HtmlDecode(movieDetails.PlotOutline);  // remove HTML entities
					movieDetails.Plot=movieDetails.PlotOutline.Trim();
					movieDetails.Plot=HttpUtility.HtmlDecode(movieDetails.Plot);  // remove HTML entities
				}

				if (iImage>=0)
				{
					iEnd = strBody.IndexOf("\"",iImage);
					movieDetails.ThumbURL=strBody.Substring(iImage,iEnd-iImage).Trim();
				}

				//plot
				if (iPlot>=0)
				{
					string strPlotURL= url.URL + "plotsummary";
					try
					{
						string absoluteUri;
						string strPlotHTML =  GetPage(strPlotURL,"utf-8",out absoluteUri);

						if (0!=strPlotHTML.Length)
						{
							
							int iPlotStart=strPlotHTML.IndexOf("<p class=\"plotpar\">");
							if (iPlotStart>=0)
							{
								iPlotStart += "<p class=\"plotpar\">".Length;
								int iPlotEnd=strPlotHTML.IndexOf("</p>",iPlotStart);
								if (iPlotEnd>=0)
								{
									movieDetails.Plot=strPlotHTML.Substring(iPlotStart, iPlotEnd-iPlotStart);
									movieDetails.Plot=Utils.stripHTMLtags(movieDetails.Plot);
									movieDetails.Plot=HttpUtility.HtmlDecode(movieDetails.Plot);  // remove HTML entities
								}
							}
						}
					}
					catch (Exception ex)
					{
						Log.WriteFile(Log.LogType.Log,true,"exception for imdb lookup of {0} err:{1} stack:{2}", strPlotURL, ex.Message,ex.StackTrace);
					}
				}

				//cast
				if(iCast<0) 
				{
					iCast=iCred;
				}
				if(iCast>=0) 
				{
					int iRealEnd = strBody.IndexOf("&nbsp;",iCast);
					iStart = strBody.IndexOf("<a href",iCast);
					iEnd = iCast;

					if(iRealEnd>=0 &&  iStart>=0 && iEnd>=0)
					{
						movieDetails.Cast = "Cast overview:\n";
						while(iRealEnd > iStart) 
						{
							//string strurl = "";
							string actor = "";
							string role = "";
		
							// actor
							iEnd = strBody.IndexOf("</a>",iStart);
							iEnd += 4;
							string strTmp=strBody.Substring(iStart,iEnd-iStart);

							iEnd += 1;

							actor=ParseAHREFIMDB(strTmp,0,"");

							// role

							iStart = strBody.IndexOf("<td valign=\"top\">",iEnd);
							iStart += "<td valign=\"top\">".Length;
			
							iEnd = strBody.IndexOf("</td>",iStart);
							role=strTmp=strBody.Substring(iStart,iEnd-iStart);
							iEnd += 1;
							role=Utils.stripHTMLtags(role).Trim();
							role=HttpUtility.HtmlDecode(role);  // remove HTML entities

							// add to cast
							movieDetails.Cast += actor.Trim();
							if(role.Length!=0) // Role not always listed
								movieDetails.Cast += " as " + role;
				
							movieDetails.Cast += "\n";
				
							// next actor
							iStart = strBody.IndexOf("<a href",iEnd);
						}
					}
				}
				int iRunTime=strBody.IndexOf("Runtime:");
				if (iRunTime>0)
				{
					string runtime="";
					while ( !Char.IsDigit(strBody[iRunTime]) && iRunTime+1 < strBody.Length) iRunTime++;
					if (iRunTime < strBody.Length)
					{
						while ( Char.IsDigit(strBody[iRunTime]) && iRunTime+1 < strBody.Length) 
						{
							runtime += strBody[iRunTime];
							iRunTime++;
						}
						try
						{
							movieDetails.RunTime=Int32.Parse(runtime);
						}
						catch (Exception){}
					}
				}
				
				int mpaa=strBody.IndexOf("MPAA</a>:</b>");
				if (mpaa>0)
				{
					mpaa+="MPAA</a>:</b>".Length;
					int mpaaEnd=strBody.IndexOf("<br>", mpaa);
					if (mpaaEnd>0)
					{
						movieDetails.MPARating=strBody.Substring(mpaa,mpaaEnd-mpaa);
					}
				}


				return true;
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"exception for imdb lookup of {0} err:{1} stack:{2}", url.URL, ex.Message,ex.StackTrace);
			}
			return false;
		}

		string ParseAHREFIMDB(string strBody,int iahref,  string strURL)
		{
			int iStart=strBody.IndexOf("<a href=\"",iahref);
			if (iStart<0) iStart=strBody.IndexOf("<A HREF=\"",iahref);
			if (iStart<0) return "";

			int iEnd=strBody.IndexOf("</a>",iStart);
			if (iEnd<0) iEnd=strBody.IndexOf("</A>",iStart);
			if (iEnd<0) return "";
			
			iStart+="<a href=\"".Length;
			int iSep=strBody.IndexOf(">",iStart);
			string strurl=strBody.Substring(iStart,(iSep-iStart)-1);
			iSep++;
			string strTitle=strBody.Substring(iSep,iEnd-iSep);
			strTitle=Utils.stripHTMLtags(strTitle);
			HTMLUtil htmlUtil= new HTMLUtil();
			htmlUtil.ConvertHTMLToAnsi(strTitle,out strTitle);
			strTitle = strTitle.Trim();
			return strTitle.Trim();
	
		}
		string ParseGenresIMDB(string strBody, int iGenre, string url )
		{
			string strTmp;
			string strTitle="";
			string strHRef=strBody.Substring(iGenre);
			int iSlash=strHRef.IndexOf(" / ");
			int iEnd=0;
			int iStart=0;
			if (iSlash>=0)
			{
				int iRealEnd=strHRef.IndexOf("(more)");
				if (iRealEnd<0) iRealEnd=strHRef.IndexOf("<br><br>");					
				while (iSlash < iRealEnd)
				{
					iStart=iEnd+2;
					iEnd=iSlash;
					int iLen=iEnd-iStart;
					if (iLen<0) break;
					strTmp=strHRef.Substring(iStart,iLen);
					strTitle = strTitle + ParseAHREFIMDB(strTmp,0,"") + " / ";

					iSlash=strHRef.IndexOf(" / ",iEnd+2);
					if(iSlash<0) iSlash = iRealEnd;
				}				
			}
			// last genre
			iEnd+=2;
			strTmp=strHRef.Substring(iEnd);
			strTitle = strTitle + ParseAHREFIMDB(strTmp,0,"");
			HTMLUtil htmlUtil= new HTMLUtil();
			htmlUtil.ConvertHTMLToAnsi(strTitle,out strTitle);

			return strTitle;
		}

		#endregion
		
		#region OFDB
		// --------------------------------------------------------------------------------
		// Beginning of OFDB support
		// --------------------------------------------------------------------------------
		// this method fetches all possible matches in array elements
		private void FindOFDB(string strURL, int iLimit)
		{
			// No results to return!
			if (iLimit<=0)
				return;
			// resultcounter
			int		iCount = 0;
			string	strTitle;
			try
			{
				// Body of the page with the searchresults
				string absoluteUri;
				string strBody = GetPage(strURL,"iso-8859-1", out absoluteUri);
			
				// Get start of Movielist, so search for <b>Titel:</b><br><br>

				int iStartOfMovieList = strBody.IndexOf("<b>Titel:</b><br><br>");

				// Nothing found? What to do....?
				if (iStartOfMovieList<0)
				{
					Log.WriteFile(Log.LogType.Log,true,"OFDB: Keine Titel gefunden. Layout verändert?");
					return;
				}
				// No matches....
				//if (strBody.IndexOf("<i>Keine Ergebnisse</i>")>=0)
				//	return;

				// Find end of list
				int iEndOfMovieList=strBody.IndexOf("<br><br><br>",iStartOfMovieList);
				if (iEndOfMovieList<0)
				{
					iEndOfMovieList=strBody.Length;
				}
				strBody = strBody.Substring(iStartOfMovieList,iEndOfMovieList-iStartOfMovieList);
				while ((true) && (iCount<iLimit))
				{
					// 1. <a href='view.php?page=film&fid=5209'>Spider-Man schlägt zurück<font size='1'> / Spider-Man strikes back</font> (1978)</a><br>

					int iAHREF=strBody.IndexOf("<a href=");
					if (iAHREF>=0)
					{
						int iEndAHREF=strBody.IndexOf("</a>");
						if (iEndAHREF>=0)
						{
							iAHREF+="<a href=.".Length;
							string strAHRef=strBody.Substring(iAHREF, iEndAHREF-iAHREF);
							// remove everything between the font Tags, it is only the english title ;-)
							int iFontStart=strAHRef.IndexOf("<font size='1'>");
							int iFontEnd=strAHRef.IndexOf("</font>");
							// be sure you found something
							if ((iFontStart>=0) && (iFontEnd)>=0)
								strAHRef = strAHRef.Substring(0,iFontStart)+strAHRef.Substring(iFontEnd+"</font>".Length,strAHRef.Length-iFontEnd-"</font>".Length);
							// Find beginning of the title
							int iURL=strAHRef.IndexOf(">");
							if (iURL>0)
							{
								// read the link
								strURL=strAHRef.Substring(0,iURL);
								if (strURL[strURL.Length-1]=='\'') 
									strURL=strURL.Substring(0,strURL.Length-1);
								// extract the title
								strTitle="";
								iURL++;
								int iURLEnd=strAHRef.IndexOf("<",iURL);
								if (iURLEnd>0)
								{
									strTitle=strAHRef.Substring(iURL,iURLEnd-iURL);
								}
								else strTitle=strAHRef.Substring(iURL);
							
								strURL=String.Format("http://www.ofdb.de/{0}", strURL);
								
								HTMLUtil htmlUtil= new HTMLUtil();

								htmlUtil.ConvertHTMLToAnsi(strTitle,out strTitle);

								IMDBUrl url = new IMDBUrl(strURL,strTitle+" (ofdb)","OFDB");
								elements.Add(url);

								// count the new element
								iCount++;
							}
							if (iEndAHREF+1>=strBody.Length) break;
							iStartOfMovieList = iEndAHREF+1;
							strBody = strBody.Substring(iEndAHREF+1);
						}
						else
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"Error getting Movielist: exception for db lookup of {0} err:{1} stack:{2}", strURL, ex.Message,ex.StackTrace);
			}
		} // END FindOFDB()

		// new private method to get List of items out of the result page
		private string ParseListOFDB(string strIn, string strSep)
		{
			// replace ... with </b> for cast list
			strIn = strIn.Replace("...","</b>");
			// some helpers
			string	strOut = "";
			int		iStart = strIn.IndexOf("<b>")+3;
			int		iEnd =  strIn.IndexOf("</b>")+4;
			// bold Tags not found!
			if ((iStart==2) || (iEnd==3))
			{
				// possible change of sitelayout
				Log.WriteFile(Log.LogType.Log,true,"OFDB: error getting list, no start or end found.");
				return "";
			}
			// strip the infos, they are in an bold Tag
			strIn = strIn.Substring(iStart,iEnd-iStart);
			// remove </a>
			strIn = strIn.Replace("</a>","");
			// Is there any information, left?
			if (strIn.Length==0)
				return "";
			while(true)
			{
				// is the information part of a link?
				iStart = strIn.IndexOf("<a");
				if (iStart>=0)
					iStart = strIn.IndexOf(">")+1;
				else
					iStart = 0;
				strIn = strIn.Substring(iStart,strIn.Length-iStart);
				// find the end of the information
				iEnd = strIn.IndexOf("<");
				if (iEnd>=0)
				{
					// strip the information and add the separator
					strOut += strIn.Substring(0,iEnd)+strSep;
					// get the rest
					strIn = strIn.Substring(iEnd,strIn.Length-iEnd);
					// remove possible list of <br>
					while(strIn.Substring(0,4)=="<br>")
					{
						strIn = strIn.Substring(4,strIn.Length-4);
						strIn = strIn.Trim();
					}
				}
				else
				{
					// End not found, possible error in OFDB
					Log.WriteFile(Log.LogType.Log,true,"OFDB: error getting end of entry");
					return "";
				}
				// Ende erreicht?
				if ((strIn=="</b>") || (strIn.Length<4))
				{
					break;
				}
			}
			// remove last separator, if nedded
			if (strOut.Length>0)
				strOut = strOut.Substring(0,strOut.Length-strSep.Length);
			strOut = strOut.Trim();
			return strOut.Trim();
		} // END ParseListOFDB()


		// this method fetches the search result into movieDetails
		private bool GetDetailsOFDB(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
		{
			try
			{

				// Initialise some helpers
				string	strTemp = "";
				int		iStart=0;
				int		iEnd=0;
				movieDetails.Reset();

				// add databaseinfo
				movieDetails.Database = "OFDB";

				string absoluteUri;
				string strBody = GetPage(url.URL,"iso-8859-1",out absoluteUri);

				// Read Starting Points of the details
				//int iTitle = strBody.IndexOf("Originaltitel:");
				int iDirectedBy = strBody.IndexOf("Regie:");
				int iCast = strBody.IndexOf("Darsteller:");
				int iGenre = strBody.IndexOf("Genre(s):");
				int iYear = strBody.IndexOf("Erscheinungsjahr:");
				int iPlotOutline = strBody.IndexOf("<b>Inhalt:</b>");
				int iImage = strBody.IndexOf("<img src=\"images/film/");
				int iRating = strBody.IndexOf("Note:");
				int iPlot=strBody.IndexOf("view.php?page=inhalt&");

				// to much information :-)
				//int iATitle = strBody.IndexOf("Alternativtitel:");

				// Not available in OFDB?
				//int iCredits=strBody.IndexOf("Writing credits");				
				//int iCred=strBody.IndexOf("redited cast:"); // Complete credited cast or Credited cast
				//int iPlotSummary=strBody.IndexOf("Plot Summary:</b>");
				//int iTagLine=strBody.IndexOf("Tagline:</b>");
                
				// Go get the title
				//iStart = iTitle;
				//if (iStart >= 0)
				//	movieDetails.Title = ParseListOFDB(strBody.Substring(iStart,strBody.Length-iStart)," / ");
				movieDetails.Title = url.Title.Substring(0,url.Title.Length-7);

				// Add alternative titles - could be to much :-)
				// it is to much, so comment out
				/*
				iStart = iATitle;
				if (iStart >= 0)
				  movieDetails.Title += " ("+ParseListOFDB(strBody.Substring(iStart,strBody.Length-iStart)," / ")+")";
				*/

				// Go get the director
				iStart = iDirectedBy;
				if (iStart >= 0)
					movieDetails.Director = ParseListOFDB(strBody.Substring(iStart,strBody.Length-iStart)," / ");

				// Go get the cast
				iStart = iCast;
				if (iStart >= 0)
					movieDetails.Cast = ParseListOFDB(strBody.Substring(iStart,strBody.Length-iStart),"\n");

				// Go get the genre
				iStart = iGenre;
				if (iStart >= 0)
					movieDetails.Genre = ParseListOFDB(strBody.Substring(iStart,strBody.Length-iStart)," / ");

				// Go get the year
				iStart = iYear;
				if (iStart >= 0)
					movieDetails.Year = System.Int32.Parse(ParseListOFDB(strBody.Substring(iStart,strBody.Length-iStart)," "));

				// Go get the PlotOutline
				iStart = iPlotOutline;
				if (iStart >= 0)
				{
					iStart += "<b>Inhalt:</b>".Length;
					strTemp = strBody.Substring(iStart,strBody.Length-iStart);
					iEnd = strTemp.IndexOf("<a");
					if (iEnd>=0)
						movieDetails.PlotOutline = strTemp.Substring(0,iEnd);
				}

				// Go get the picture
				iStart = iImage;
				if(iStart>=0)
				{
					iStart += 10;
					// found one
					strTemp = strBody.Substring(iStart,strBody.Length-iStart);
					iEnd = strTemp.IndexOf("\"");
					if (iEnd>=0)
						movieDetails.ThumbURL = "http://www.ofdb.de/"+strTemp.Substring(0,iEnd);
				}

				// Go get the rating, votes and position
				iStart = iRating;
				if (iStart>=0)
				{
					iStart += "Note:".Length;
					strTemp = strBody.Substring(iStart,strBody.Length-iStart);
					iEnd = strTemp.IndexOf("&");

					if (iEnd>=0)
					{
						// set rating
						string strRating = strTemp.Substring(0,iEnd);
						strRating = strRating.Trim();
						try
						{
							movieDetails.Rating = (float)System.Double.Parse(strRating);
							if (movieDetails.Rating>10.0f) movieDetails.Rating/=100.0f;
						}
						catch(Exception)
						{
						}
					}
					if(movieDetails.Rating != 0.0f) 
					{
						// now, votes
						movieDetails.Votes = "0";
						iStart = strTemp.IndexOf("Stimmen:");
						if(iStart>0)
						{
							iEnd = strTemp.IndexOf("&",iStart);
							if(iEnd>0)
							{
								iStart += "Stimmen:".Length;
								movieDetails.Votes = strTemp.Substring(iStart,iEnd-iStart).Trim();
							}
						}
						// now the postion
						iStart = strTemp.IndexOf("Platz:");
						if(iStart>0)
						{
							iEnd = strTemp.IndexOf("&",iStart);
							if(iEnd>0)
							{
								iStart += "Platz:".Length;
								string strTop = strTemp.Substring(iStart,iEnd-iStart).Trim();
								int	iTop250 = 251;
								try
								{
									iTop250 = System.Int32.Parse(strTop);
								}
								catch(Exception)
								{
								}
								// we have more postions, but only add thos up to 250
								if (iTop250 <= 250)
									movieDetails.Top250 = iTop250;
							}
						}
					}
				}
				

				// Go get the plot
				iStart = iPlot;
				if (iStart >= 0)
				{
					// extract the path to the detailed description
					iEnd = strBody.IndexOf("\"",iStart);
					if(iEnd>=0)
					{
						string strPlotURL = strBody.Substring(iStart,iEnd-iStart);
						strPlotURL = strPlotURL.Trim();

						try
						{
							// Open the new page with detailed description
							string strPlotHTML = GetPage("http://www.ofdb.de/"+strPlotURL,"iso-8859-1", out absoluteUri);

							if (0!=strPlotHTML.Length)
							{
								int iPlotStart = strPlotHTML.IndexOf("Eine Inhaltsangabe");
								// Verfasser auslassen? Wahrscheinlich besser, wegen der Rechte
								if (iPlotStart>=0)
									iPlotStart = strPlotHTML.IndexOf("<br><br>",iPlotStart);
								if (iPlotStart>=0)
								{
									// Verfasser auslassen?
									iPlotStart += "<br><br>".Length;
									// Ende suchen
									int iPlotEnd = strPlotHTML.IndexOf("</font>",iPlotStart);
									if (iPlotEnd>=0)
									{
										movieDetails.Plot = strPlotHTML.Substring(iPlotStart, iPlotEnd-iPlotStart);
										// Zeilenumbrüche umwandeln
										movieDetails.Plot.Replace("<br>","\n");
										movieDetails.Plot = Utils.stripHTMLtags(movieDetails.Plot);
										movieDetails.Plot = HttpUtility.HtmlDecode(movieDetails.Plot);  // remove HTML entities
									}
								}
								if (movieDetails.Plot.Length==0)
								{
									// Could not get link to plot description
									Log.WriteFile(Log.LogType.Log,true,"OFDB: could extract the plot description from {0}","http://www.ofdb.de/"+strPlotURL);
								}
							}
						}
						catch (Exception ex)
						{
							Log.WriteFile(Log.LogType.Log,true,"Error getting plot: exception for db lookup of {0} err:{1} stack:{2}", strPlotURL, ex.Message,ex.StackTrace);
						}
					}
				}
				else
				{
					// Could not get link to plot description
					Log.WriteFile(Log.LogType.Log,true,"OFDB: could not find link to plot description");
				}
				return true;
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"Error getting detailed movie information: exception for db lookup of {0} err:{1} stack:{2}", url.URL, ex.Message,ex.StackTrace);
			}
			return false;
		} // END GetDetailsOFDB()

		// --------------------------------------------------------------------------------
		// END of OFDB support
		// --------------------------------------------------------------------------------
		#endregion

		#region FRDB
		// --------------------------------------------------------------------------------
		// Beginning of FRDB support
		// --------------------------------------------------------------------------------
		// this method fetches all possible matches in array elements
		private void FindFRDB(string strURL, int iLimit)
		{
			// No results to return!
			if (iLimit<=0)
				return;

			int		iStart = 0;
			int		iStop = -1;
			int		iCount = 0;
			string	strTitle = null;
			string	strMovieId = null;

			try
			{
				// Body of the page with the searchresults
				string absoluteUri;
				string strBody = GetPage(strURL,"iso-8859-1",out absoluteUri);
			
				// Get start of Movielist, so search for <b>Titel:</b><br><br>
				int iStartOfMovieList = strBody.IndexOf("<table class=\"tableSearchResult\"");

				// Nothing found? What to do....?
				if (iStartOfMovieList<0)
				{
					Log.Write("FRDB: Aucun film trouvé");
					return;
				}
				// No matches....
				//if (strBody.IndexOf("<i>Keine Ergebnisse</i>")>=0)
				//	return;

				// Find end of list
				int iEndOfMovieList=strBody.IndexOf("<B>[tout]</B>",iStartOfMovieList);
				if (iEndOfMovieList<0)
				{
					iEndOfMovieList=strBody.Length;
				}

				strBody = strBody.Substring(iStartOfMovieList,iEndOfMovieList-iStartOfMovieList);
				while ((true) && (iCount<iLimit))
				{
					// 1. <A CLASS="searchText" HREF="../dvd/dvd.php?id=11248">Matrix Revolutions (Edition Double)</A></TD>

					// read movie url
					iStart = strBody.IndexOf("<A CLASS=\"searchText\" HREF=\"", iStart) + "<A CLASS=\"searchText\" HREF=\"".Length + 3;
					if( iStart == -1 || iStart == 0 ) throw new ApplicationException("Parsing failed for FRDB !");
					iStop = strBody.IndexOf("\">", iStart);
					if( iStop == -1 ) throw new ApplicationException("Parsing failed for FRDB !");
					strMovieId = strBody.Substring(iStart, iStop-iStart);

					// read movie title
					iStart = strBody.IndexOf("\">", iStart) + 2;
					if( iStart == -1 ) throw new ApplicationException("Parsing failed for FRDB !");
					iStop = strBody.IndexOf("<", iStart);
					if( iStop == -1 ) throw new ApplicationException("Parsing failed for FRDB !");
					strTitle = strBody.Substring(iStart, iStop-iStart);

					// build url
					strURL = String.Format( "http://www.dvd-fr.com/{0}", strMovieId );
					HTMLUtil htmlUtil= new HTMLUtil();
					htmlUtil.ConvertHTMLToAnsi(strTitle, out strTitle);

					IMDBUrl url = new IMDBUrl( strURL, strTitle+" (frdb)","FRDB");
					elements.Add(url);

					// count the new element
					iCount++;
					// position indexes
					iStart = iStop + 2;
				}
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"Error getting Movielist: exception for db lookup of {0} err:{1} stack:{2}", strURL, ex.Message,ex.StackTrace);
			}

			return;
		} // END FindFRDB()


		// this method fetches the search result into movieDetails
		private bool GetDetailsFRDB(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
		{
			string strBody = string.Empty;
			int iStart = 0, iStop = 0;
			int actorStart, actorEnd;
			bool castEnd = false;

			try
			{
				// Initialise some helpers
				movieDetails.Reset();
				// add databaseinfo
				movieDetails.Database = "FRDB";
				// get page content
				string absoluteUri;
				strBody = GetPage(url.URL,"iso-8859-1",out absoluteUri);
				
				iStart = strBody.IndexOf("<br>", strBody.IndexOf("<div class=\"dvd_titleinfo\">"));
				movieDetails.Year = int.Parse( strBody.Substring( iStart-4, 4) );

				iStart = strBody.IndexOf( ">", strBody.IndexOf("../images/vote_dvdfr.gif")) + 1;
				iStop = strBody.IndexOf( "<", iStart );
				try
				{
					movieDetails.Rating = float.Parse(strBody.Substring( iStart, iStop-iStart));
				}
				catch
				{
					//movieDetails.Rating = "?";
				}

				iStart = strBody.IndexOf( ">", strBody.IndexOf("../images/vote_public.gif")) + 1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.Votes = strBody.Substring( iStart, iStop-iStart);
				if( movieDetails.Votes == "&nbsp;" ) movieDetails.Votes = "?";

				iStart = strBody.IndexOf( ">", strBody.IndexOf("dvd_title"))+1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.Title = strBody.Substring( iStart, iStop-iStart);

				iStart = strBody.IndexOf( ">", strBody.IndexOf("Synopsis</div>")+18)+1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.PlotOutline = strBody.Substring( iStart, iStop-iStart).Replace("\n"," ");

				iStart = strBody.IndexOf( ">", strBody.IndexOf( ">", strBody.IndexOf("Réalisation</div>")+20)+1)+1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.Director = strBody.Substring( iStart, iStop-iStart);

				iStart = strBody.IndexOf( ">", strBody.IndexOf( ">", strBody.IndexOf("Scénario</div>")+18)+1)+1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.WritingCredits = strBody.Substring( iStart, iStop-iStart);

				iStart = strBody.IndexOf( ">", strBody.IndexOf("Avec...</div>")+1)+1;
				iStop = strBody.IndexOf( "</div>", iStart );
				actorStart = iStart;
				actorEnd = iStop;
				while( ! castEnd )
				{
					actorStart = strBody.IndexOf( ">", strBody.IndexOf("<a class", actorStart) )+1;
					actorEnd = strBody.IndexOf( "</a>", actorStart );

					movieDetails.Cast += strBody.Substring( actorStart, actorEnd - actorStart );
					if( iStop - actorEnd > 10 )
					{
						movieDetails.Cast += ", ";
					}
					else castEnd = true;

					actorStart = actorEnd;
				}

				iStart = strBody.IndexOf( ">", strBody.IndexOf("Référence</div>")+20)+1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.IMDBNumber = strBody.Substring( iStart, iStop-iStart);

				iStart = strBody.IndexOf( ">", strBody.IndexOf("../search/search.php?categorie"))+1;
				iStop = strBody.IndexOf( "<", iStart );
				movieDetails.Genre = strBody.Substring(iStart,1) + strBody.Substring( iStart+1, iStop-iStart-1).ToLower();

				iStart = strBody.IndexOf( "/", strBody.IndexOf("../images/dvd"))+1;
				iStop = strBody.IndexOf( "\"", iStart );
				movieDetails.ThumbURL = "http://www.dvd-fr.com/" + strBody.Substring( iStart, iStop-iStart);
			}
			catch(Exception ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"Error getting detailed movie information: exception for db lookup of {0} err:{1} stack:{2}", url.URL, ex.Message,ex.StackTrace);
				return false;
			}
			return true;
		} // END GetDetailsFRDB()

		// --------------------------------------------------------------------------------
		// END of FRDB support
		// --------------------------------------------------------------------------------
		#endregion
	
	} // END class IMDB

} // END namespace
