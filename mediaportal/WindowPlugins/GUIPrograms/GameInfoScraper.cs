using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using MediaPortal.Util;

using MediaPortal.GUI.Library;		

namespace ProgramsDatabase
{
	/// <summary>
	/// Summary description for GameInfoScraper.
	/// Heavily inspired by Frodo's MusicInfoScraper..... :-)
	/// </summary>
	public class GameInfoScraper
	{
		ArrayList m_games = new ArrayList();
		public GameInfoScraper()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public int Count
		{
			get { return m_games.Count;}
		}
		public FileItem this[int index]
		{
			get { return (FileItem)m_games[index];}
		}

		string AddMissingRowTags(string strTable)
		{
			// poor man's replace.... let's try:
			string strOrig = "</TD>\r\n<TR";
			string strReplace = "</TD>\r\n</TR>\r\n<TR";
			strTable = strTable.Replace(strOrig, strReplace);
			return strTable;
		}

		public bool FindGameinfo(string strGameTitle)
		{
			m_games.Clear();

			Log.Write("=> dw allgamescraper: FindGameInfo: '{0}'", strGameTitle);

			// make request
			// type is 
			// http://www.allgame.com/cg/agg.dll?p=agg&type=1&SRCH=SuperMario64



			string strPostData=String.Format("P=agg&TYPE=1&SRCH={0}", strGameTitle ); // todo: strip spaces!
		
			string strHTML=PostHTTP("http://www.allgame.com/cg/agg.dll", strPostData);
			if (strHTML.Length==0) return false;

			// check if this is an album
			FileItem newGame = new FileItem(null); // todo: sqldb necessary???
			newGame.URL="http://www.allgame.com/cg/agg.dll?"+strPostData;
			if ( newGame.ParseAllGame(strHTML) )
			{
				m_games.Add(newGame);
				return true;
			}

			string strHTMLLow=strHTML;
			strHTMLLow=strHTMLLow.ToLower();
//			int iStartOfTable=strHTMLLow.IndexOf(">relevance</b>");
			int iStartOfTable=strHTMLLow.IndexOf(">games with titles matching");
			if (iStartOfTable< 0) return false;
			//			iStartOfTable=strHTMLLow.LastIndexOf("<table",iStartOfTable);
			iStartOfTable=strHTMLLow.IndexOf("<table",iStartOfTable);
			if (iStartOfTable < 0) return false;
      
			HTMLUtil  util=new HTMLUtil();
			HTMLTable table=new HTMLTable();
			string strTable=strHTML.Substring(iStartOfTable);

			// now the allgame thing is that <tr> tags are not closed
			// for the decisive rows.... so I add them manually
			// otherwise the parser doesn't split up the string correctly
			strTable = AddMissingRowTags(strTable);
			table.Parse(strTable); // call frodo's html parser
			for (int i=1; i < table.Rows; ++i)  // skip first row (contains table header)
			{
				HTMLTable.HTMLRow row=table.GetRow(i);
//				string strAlbumName="";
//				string strAlbumURL="";
//				string strColumnStripped = "";
				for (int iCol=0; iCol < row.Columns; ++iCol)
				{
					string strColumn=row.GetColumValue(iCol);

					// ok here we cycle throuh the 8 columns of one table row:
					// col 0: "Relevance" => see width of the picture to measure this
					// col 1: "Year" 
					// col 2: "buy it"-link
					// col 3: "Title" => includes the detail URL
					// col 4: "Genre"
					// col 5: "Style"
					// col 6: "Platform"
					// col 7: "Rating" => use imagename to get rating: "st_gt1.gif" to "st_gt9.gif" 

					if (iCol == 0) 
					{
						string strRelevance = "";
						int iStartOfWidth = -1;
						int iEndOfWidth = -1;
						// ex:
						// "<img src="/im/agg/red_dot.jpg" valign=center width="56" height=5 border=0>&nbsp;"
						// the WIDTH attribute is the relevance: maximum value is 56, negative values are possible
						iStartOfWidth = strColumn.IndexOf("width=\"");
						if (iStartOfWidth != -1)
						{
							iStartOfWidth = strColumn.IndexOf("\"", iStartOfWidth);
							if (iStartOfWidth != -1)
							{
								iEndOfWidth = strColumn.IndexOf("\"", iStartOfWidth + 1);
								if ((iEndOfWidth != -1) && (iEndOfWidth > iStartOfWidth))
								{
									strRelevance = strColumn.Substring(iStartOfWidth + 1, iEndOfWidth - iStartOfWidth - 1);
								}
							}
						}
						Log.Write("dw allgamescraper: Relevance: {0}", strRelevance);
					}
					else if (iCol == 1)
					{
						string strYear = "";
						util.RemoveTags(ref strColumn);
						strYear = strColumn.Replace("&nbsp;", "");
						Log.Write("dw allgamescraper: Year: {0}", strYear);
					}
					else if (iCol == 2)
					{
						// NOTHING TO DO, skip the bloody "buy-it" link ;-)
					}
					else if (iCol == 3)
					{
						// ex:
						// "<FONT SIZE=-1><A HREF=/cg/agg.dll?p=agg&SQL=GIH|||||1002>Super Mario 64</A></FONT>"
						//
						// two things to do in this column:
						// => extract URL to the detail page
						// => extract gametitle
						string strGameURL = "";
						int iStartOfURL = -1;
						int iEndOfURL = -1;
						iStartOfURL = strColumn.ToLower().IndexOf("<a href");
						if (iStartOfURL != -1)
						{
							iStartOfURL = strColumn.IndexOf("/", iStartOfURL);
							if (iStartOfURL != -1)
							{
								iEndOfURL = strColumn.IndexOf(">", iStartOfURL + 1);
								if ((iEndOfURL != -1) && (iEndOfURL > iStartOfURL))
								{
									strGameURL = strColumn.Substring(iStartOfURL, iEndOfURL - iStartOfURL);
									// and add the prefix!
									strGameURL = "http://www.allgame.com" + strGameURL;
								}
							}
						}
						Log.Write("dw allgamescraper: GameURL: {0}", strGameURL);

						string strTitle = "";
						util.RemoveTags(ref strColumn);
						strTitle = strColumn.Replace("&nbsp;", "");
						Log.Write("dw allgamescraper: Title: {0}", strTitle);

					}
					else if (iCol == 4)
					{
						string strGenre = "";
						util.RemoveTags(ref strColumn);
						strGenre = strColumn.Replace("&nbsp;", "");
						Log.Write("dw allgamescraper: Genre: {0}", strGenre);
					}
					else if (iCol == 5)
					{
						string strStyle = "";
						util.RemoveTags(ref strColumn);
						strStyle = strColumn.Replace("&nbsp;", "");
						Log.Write("dw allgamescraper: Style: {0}", strStyle);
					}
					else if (iCol == 6)
					{
						string strPlatform = "";
						util.RemoveTags(ref strColumn);
						strPlatform = strColumn.Replace("&nbsp;", "");
						Log.Write("dw allgamescraper: Platform: {0}", strPlatform);
					}
					else if (iCol == 7)
					{
						string strRating = "";
						// ex.
						// <A HREF=/cg/agg.dll?p=agg&SQL=GRH|||||1002><IMG SRC=/im/agg/st_gt9.gif BORDER=0 WIDTH=75 HEIGHT=15 VSPACE=2></A>
						// the rating is coded in the gif - filename
						// st_gt1.gif is the worst rating
						// st_gt9.gif is the best rating
						strRating = "";
						int iStartOfRating = -1;
						int iEndOfRating = -1;
						iStartOfRating = strColumn.ToLower().IndexOf("<img src=");
						if (iStartOfRating != -1)
						{
							iStartOfRating = strColumn.IndexOf("/st_gt", iStartOfRating);
							if (iStartOfRating != -1)
							{
								iEndOfRating = strColumn.IndexOf(".gif", iStartOfRating);
								if ((iEndOfRating != -1) && (iEndOfRating > iStartOfRating))
								{
									strRating = strColumn.Substring(iStartOfRating + 6, 1); // 6 is the length of the IndexOf searchstring...
								}
							}
						}
						Log.Write("dw allgamescraper: Rating: {0}\n", strRating);
					}

					// todo: ADD FOUND GAME to arraylist!!!

//					if (iCol==1 && (strColum.Length!=0)) strAlbumName="("+strColum+")";
//					if (iCol==2)
//					{
//						string strArtist=strColum;
//						util.RemoveTags(ref strArtist);
//						if (!strColum.Equals("&nbsp;"))
//							strAlbumName=String.Format("- {0} {1}",strArtist,strAlbumName);
//					}
//					if (iCol==4)
//					{
//						string strAlbumTemp=strColum;
//						util.RemoveTags(ref strAlbumTemp);
//						strAlbumName=String.Format("{0} {1}",strAlbumTemp,strAlbumName);
//					}
//					if (iCol==4 && strColum.IndexOf("<a href=\"") >= 0)
//					{
//						int pos1=strColum.IndexOf("<a href=\"") ;
//						pos1+=+"<a href=\"".Length;
//						int iPos2=strColum.IndexOf("\">",pos1);
//						if (iPos2 >= 0)
//						{
//							// full album url:
//							// http://www.allmusic.com/cg/amg.dll?p=amg&token=&sql=10:66jieal64xs7
//							string strurl=strColum.Substring(pos1, iPos2-pos1);                
//							string strAlbumNameStripped;
//							strAlbumURL=String.Format("http://www.allmusic.com{0}", strurl);
//							FileItem newGameInfo = new FileItem(null);  // todo: sqldb necessary????
//							util.ConvertHTMLToAnsi(strAlbumName, out strAlbumNameStripped);
//							newGameInfo.Title2=strAlbumNameStripped;
//							newGameInfo.URL=strAlbumURL;
//							m_games.Add(newGameInfo);
//            
//						}
//					}


				}
			}
	
			return true;
		}
    
    
		string PostHTTP(string strURL, string strData)
		{
			try
			{
				string strBody;
				WebRequest req = WebRequest.Create(strURL);
				req.Method="POST";
				req.ContentType = "application/x-www-form-urlencoded";

				byte [] bytes = null;
				// Get the data that is being posted (or sent) to the server
				bytes = System.Text.Encoding.ASCII.GetBytes (strData);
				req.ContentLength = bytes.Length;
				// 1. Get an output stream from the request object
				Stream outputStream = req.GetRequestStream ();

				// 2. Post the data out to the stream
				outputStream.Write (bytes, 0, bytes.Length);

				// 3. Close the output stream and send the data out to the web server
				outputStream.Close ();


				WebResponse result = req.GetResponse();
				Stream ReceiveStream = result.GetResponseStream();
				Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
				StreamReader sr = new StreamReader( ReceiveStream, encode );
				strBody=sr.ReadToEnd();
				return strBody;
			}
			catch(Exception)
			{
			}
			return "";
		}
	}

}
