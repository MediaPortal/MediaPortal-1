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
	/// Summary description for AllGameInfoScraper.
	/// Heavily inspired by Frodo's MusicInfoScraper..... :-)
	/// </summary>
	public class AllGameInfoScraper
	{
		ArrayList m_games = new ArrayList();
		public AllGameInfoScraper()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public int Count
		{
			get { return m_games.Count;}
		}
		public FileInfo this[int index]
		{
			get { return (FileInfo)m_games[index];}
		}

		public ArrayList FileInfos
		{
			get { return m_games; }
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

			// make request
			// type is 
			// http://www.allgame.com/cg/agg.dll?p=agg&type=1&SRCH=SuperMario64

			string strPostData=String.Format("P=agg&TYPE=1&SRCH={0}", strGameTitle ); 
		
			string strHTML=PostHTTP("http://www.allgame.com/cg/agg.dll", strPostData);
			if (strHTML.Length==0) return false;

			string strHTMLLow=strHTML;
			strHTMLLow=strHTMLLow.ToLower();
			int iStartOfTable=strHTMLLow.IndexOf(">games with titles matching");
			if (iStartOfTable< 0) return false;
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
				FileInfo newGame = new FileInfo();

				//							FileItem newGameInfo = new FileItem(null);  // todo: sqldb necessary????
				//							util.ConvertHTMLToAnsi(strAlbumName, out strAlbumNameStripped);
				//							newGameInfo.Title2=strAlbumNameStripped;
				//							newGameInfo.URL=strAlbumURL;
				//							m_games.Add(newGameInfo);


				HTMLTable.HTMLRow row=table.GetRow(i);
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
						newGame.Relevance = strRelevance;
					}
					else if (iCol == 1)
					{
						string strYear = "";
						util.RemoveTags(ref strColumn);
						strYear = strColumn.Replace("&nbsp;", "");
						newGame.Year = strYear;
					}
					else if (iCol == 2)
					{
						// NOTHING TO DO, skip the bloody "buy-it" link ;-)
					}
					else if (iCol == 3)
					{
						// ex:
						// "<FONT SIZE=-1><A HREF=/cg/agg.dll?p=agg&SQL=GIH|||||1002>Super Mario 64</A></FONT>"
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

						string strTitle = "";
						util.RemoveTags(ref strColumn);
						strTitle = strColumn.Replace("&nbsp;", "");
						newGame.Title = strTitle;
						newGame.GameURL = strGameURL;

					}
					else if (iCol == 4)
					{
						string strGenre = "";
						util.RemoveTags(ref strColumn);
						strGenre = strColumn.Replace("&nbsp;", "");
						newGame.Genre = strGenre;
					}
					else if (iCol == 5)
					{
						string strStyle = "";
						util.RemoveTags(ref strColumn);
						strStyle = strColumn.Replace("&nbsp;", "");
						newGame.Style = strStyle;
					}
					else if (iCol == 6)
					{
						string strPlatform = "";
						util.RemoveTags(ref strColumn);
						strPlatform = strColumn.Replace("&nbsp;", "");
						newGame.Platform = strPlatform;
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
						newGame.Rating = strRating;
						m_games.Add(newGame);
					}
				}
			}
			return true;
		}



		public bool FindGameinfoDetail(AppItem curApp, FileItem curItem, FileInfo curGame)
		{
			if (curItem == null) return false;
			if (curGame == null) return false;
			if (curGame.GameURL == "") return false;

			HTMLUtil  util=new HTMLUtil();

			// query string is as in the following example:
			// ALTERED BEAST for sega genesis
			// http://www.allgame.com/cg/agg.dll?p=agg&SQL=GIH|||||||66
			// To use PostHTTP, we have to split the parameters from the full url

			//string strPostData="p=agg&SQL=GIH|||||||66";
			string strPostData = curGame.GetGameURLPostParams();
			if (strPostData == "") return false;
			string strHTML=PostHTTP("http://www.allgame.com/cg/agg.dll", strPostData);

			if (strHTML.Length==0) return false;

			string strHTMLLow=strHTML;
			strHTMLLow=strHTMLLow.ToLower();


			// 1) get MANUFACTURER
			string strManufacturer = "";
			int iStartOf = -1;
			int iEndOf = -1;
			// ex:
			// <TR><TD ALIGN=RIGHT BGCOLOR="#FF9933" WIDTH=122><FONT COLOR="#000000" SIZE=-1>Developer</FONT></TD>
			// <TD WIDTH=482 BGCOLOR="#D8D8D8" VALIGN=bottom><TABLE WIDTH=484 BGCOLOR="#D8D8D8" BORDER=0 CELLSPACING=0 CELLPADDING=0><TR>
			// <TD WIDTH=4><IMG SRC=/im/agg/1.gif WIDTH=4 HEIGHT=1></TD><TD WIDTH=478><A HREF=/cg/agg.dll?p=agg&SQL=CIB||||||970>Mythos Games, Ltd.</A></TD></TR>

			// a) FIND the "DEVELOPER" text
			// b) FIND the next table row
			// c) remove tags, trim "developer" away
			iStartOf = strHTMLLow.IndexOf(">developer<");
			if (iStartOf != -1)
			{
				iStartOf = strHTMLLow.IndexOf("<tr>", iStartOf);
				iEndOf = strHTMLLow.IndexOf("</tr>", iStartOf);
				if ((iEndOf != -1) && (iEndOf > iStartOf))
				{
					strManufacturer = strHTML.Substring(iStartOf, iEndOf - iStartOf);
					util.RemoveTags(ref strManufacturer);

					if (strManufacturer != "")
					{
						curGame.Manufacturer = strManufacturer;
					}

				}
			}


			// 2) get OVERVIEW / COVERSHOT
			string strOverview = "";
			string strCovershot = "";
			int iStartOfOV = -1;
			int iEndOfOV = -1;
			iStartOfOV = strHTMLLow.IndexOf("<img src=\"http://image.allmusic.com/00/agg/cov200");
			if (iStartOfOV != -1)
			{
				iEndOfOV = strHTMLLow.IndexOf("</tr>", iStartOfOV);
				if ((iEndOfOV != -1) && (iEndOfOV > iStartOfOV))
				{
					strOverview = strHTML.Substring(iStartOfOV, iEndOfOV - iStartOfOV);
					util.RemoveTags(ref strOverview);

					if (strOverview != "")
					{
						strOverview = strOverview.Replace("\r", "\r\n");
						strOverview = strOverview.Replace("\n", "\r\n");
						strOverview = strOverview.Replace("&#151;", "\r\n");
						curGame.Overview = strOverview;
					}

				}
				int iStartOfCovershot = iStartOfOV;
				int iEndOfCovershot = -1;
				if (iStartOfCovershot != -1)
				{
					iStartOfCovershot = strHTMLLow.IndexOf("\"", iStartOfCovershot);
					if (iStartOfCovershot != -1)
					{
						iEndOfCovershot = strHTMLLow.IndexOf("\"", iStartOfCovershot+1);
						if ((iEndOfCovershot != -1) && (iEndOfCovershot > iStartOfCovershot))
						{
							strCovershot = strHTML.Substring(iStartOfCovershot + 1, iEndOfCovershot - iStartOfCovershot - 1);
							curGame.AddImageURL(strCovershot);
						}
					}
				}
			}


			// 3) get SCREENSHOTS
			string strCurScreen = "";
			int iStartOfImg = -1;
			int iStartOfLink = -1;
			int iEndOfLink = -1;
			bool bGoOn = true;
			iStartOfImg = strHTMLLow.IndexOf("<a href=http://image.allmusic.com/00/agg/screen250");
			bGoOn = (iStartOfImg != -1);
			while (bGoOn)
			{
				iStartOfLink = strHTMLLow.IndexOf("=", iStartOfImg);
				if (iStartOfLink != -1)
				{
					iStartOfLink++;
					iEndOfLink = strHTMLLow.IndexOf(">", iStartOfLink);
					if ((iEndOfLink != -1) && (iEndOfLink > iStartOfLink))
					{
						strCurScreen = strHTML.Substring(iStartOfLink, iEndOfLink - iStartOfLink);
						if (strCurScreen != "")
						{
							curGame.AddImageURL(strCurScreen);
						}
					}
				}

				iStartOfImg = strHTMLLow.IndexOf("<a href=http://image.allmusic.com/00/agg/screen250", iStartOfImg + 1);
				bGoOn = (iStartOfImg != -1);
			}

			
			curGame.DownloadImages(curApp, curItem);
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

				outputStream.Close();
				ReceiveStream.Close();
				sr.Close();
				result.Close();

				
				req = null;
				outputStream = null;
				result = null;
				ReceiveStream = null;
				sr = null;

				return strBody;
			}
//			catch(Exception ex)
			catch(Exception)
			{
//				Log.Write("AllGameInfoScraper exception: {0}", ex.ToString());
			}
			return "";
		}
	}

}
