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

		// constructor
		public IMDB()
		{
			// load the settings
			LoadSettings();
		} // END constructor

		// load settings from mediaportal.xml
		private void LoadSettings()
		{
			// getting available databases and limits
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
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
		private string GetPage(string strURL, string strEncode)
		{
			string strBody = "";
			try
			{
				// Make the Webrequest
				WebRequest req = WebRequest.Create(strURL);
				WebResponse result = req.GetResponse();
				Stream ReceiveStream = result.GetResponseStream();

				// Encoding: depends on selected page
				Encoding encode = System.Text.Encoding.GetEncoding(strEncode);
				StreamReader sr = new StreamReader( ReceiveStream, encode );
				strBody = sr.ReadToEnd();
			}
			catch(Exception ex) 
			{
				Log.Write("Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message,ex.StackTrace);
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
			string strURL = System.IO.Path.GetFileNameWithoutExtension(strMovie);
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
			string strURL;
			// getting searchstring
			string strSearch = HttpUtility.UrlEncode(GetSearchString(strMovie));

			// be aware of german special chars äöüß Ã¤Ã¶Ã¼ÃŸ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
			strSearch = strSearch.Replace("%c3%a4","%E4");
			strSearch = strSearch.Replace("%c3%b6","%F6");
			strSearch = strSearch.Replace("%c3%bc","%FC");
			strSearch = strSearch.Replace("%c3%9f","%DF");

			elements.Clear();			

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
							strURL = "http://us.imdb.com/Tsearch?title="+strSearch;
							FindIMDB(strURL,aLimits[i]);
							// END IMDB support
							break;
						case "OFDB":
							// OFDB support
							strURL = "http://www.ofdb.de/view.php?page=suchergebnis&Kat=All&SText="+strSearch;
							FindOFDB(strURL,aLimits[i]);
							// END OFDB support
							break;
						default:
							// unsupported database?
							Log.Write("Movie database lookup - database not supported: {0}",aDatabases[i].ToUpper());
							break;
					}
				}
			}

		} // END Find()

		// this method switches between the different databases to fetche the search result into movieDetails
		public bool GetDetails(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
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
				default:
					// Not supported Database / Host
					Log.Write("Movie DB lookup GetDetails(): Unknown Database {0}",url.Database);
					return	false;
			}
		} // END GetDetails()

		// --------------------------------------------------------------------------------
		// Beginning of IMDB support
		// --------------------------------------------------------------------------------

		private void FindIMDB(string strURL,int iLimit)
		{
			int		iCount = 0;
			string	strTitle;
			try
			{
				string strBody = GetPage(strURL,"utf-8");

				// Mars Warrior @ 03-sep-2004.
				// First try to find an Exact Match. If no exact match found, just look
				// for any match and add all those to the list. This narrows it down more easily...
				int iStartOfMovieList = strBody.IndexOf("Exact Matches</b>");
				if (iStartOfMovieList<0) iStartOfMovieList=strBody.IndexOf(" Matches</b>");

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
								
								strURL=String.Format("http://us.imdb.com{0}", strURL);
								HTMLUtil htmlUtil= new HTMLUtil();
								htmlUtil.ConvertHTMLToAnsi(strTitle,out strTitle);

								IMDBUrl url =new IMDBUrl(strURL,strTitle+" (imdb)","IMDB");
								elements.Add(url);
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
				Log.Write("exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message,ex.StackTrace);
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

				string strBody;
				WebRequest req = WebRequest.Create(url.URL);
				WebResponse result = req.GetResponse();
				Stream ReceiveStream = result.GetResponseStream();
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
				StreamReader sr = new StreamReader( ReceiveStream, encode );
				strBody=sr.ReadToEnd();

				string strAbsURL=result.ResponseUri.AbsoluteUri;
				int iPos=strAbsURL.IndexOf("/title/");
				if (iPos>0)
				{
					iPos+="/title/".Length;
					movieDetails.IMDBNumber=strAbsURL.Substring(iPos);
					if (movieDetails.IMDBNumber[movieDetails.IMDBNumber.Length-1]=='/')
						movieDetails.IMDBNumber=movieDetails.IMDBNumber.Substring(0,movieDetails.IMDBNumber.Length-1);
				}

				url.Title = url.Title.Trim();
				// cut of " (imdb)"
				iEnd = url.Title.IndexOf(" (imdb)");
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
						string strPlotHTML =  GetPage(strPlotURL,"utf-8");

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
						Log.Write("exception for imdb lookup of {0} err:{1} stack:{2}", strPlotURL, ex.Message,ex.StackTrace);
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


				return true;
			}
			catch(Exception ex) 
			{
				Log.Write("exception for imdb lookup of {0} err:{1} stack:{2}", url.URL, ex.Message,ex.StackTrace);
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

		// --------------------------------------------------------------------------------
		// END of IMDB support
		// --------------------------------------------------------------------------------

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
				string strBody = GetPage(strURL,"iso-8859-1");
			
				// Get start of Movielist, so search for <b>Titel:</b><br><br>

				int iStartOfMovieList = strBody.IndexOf("<b>Titel:</b><br><br>");

				// Nothing found? What to do....?
				if (iStartOfMovieList<0)
				{
					Log.Write("OFDB: Keine Titel gefunden. Layout verändert?");
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
				Log.Write("Error getting Movielist: exception for db lookup of {0} err:{1} stack:{2}", strURL, ex.Message,ex.StackTrace);
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
				Log.Write("OFDB: error getting list, no start or end found.");
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
					Log.Write("OFDB: error getting end of entry");
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

        string strBody = GetPage(url.URL,"iso-8859-1");

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
              string strPlotHTML = GetPage("http://www.ofdb.de/"+strPlotURL,"iso-8859-1");

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
                  Log.Write("OFDB: could extract the plot description from {0}","http://www.ofdb.de/"+strPlotURL);
                }
              }
            }
            catch (Exception ex)
            {
              Log.Write("Error getting plot: exception for db lookup of {0} err:{1} stack:{2}", strPlotURL, ex.Message,ex.StackTrace);
            }
          }
        }
        else
        {
          // Could not get link to plot description
          Log.Write("OFDB: could not find link to plot description");
        }
        return true;
      }
      catch(Exception ex) 
      {
        Log.Write("Error getting detailed movie information: exception for db lookup of {0} err:{1} stack:{2}", url.URL, ex.Message,ex.StackTrace);
      }
      return false;
    } // END GetDetailsOFDB()

		// --------------------------------------------------------------------------------
		// END of OFDB support
		// --------------------------------------------------------------------------------
	
	} // END class IMDB

} // END namespace
