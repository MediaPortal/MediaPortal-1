#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Summary description for ArtistInfoScraper.
  /// </summary>
  public class AllmusicSiteScraper
  {
    public enum SearchBy : int
    {
      Artists = 1,
      Albums
    } ;

    internal const string MAINURL = "http://www.allmusic.com";
    internal const string URLPROGRAM = "/cg/amg.dll";
    internal const string JAVASCRIPTZ = "p=amg&token=&sql=";
    protected ArrayList m_codes = new ArrayList(); // if multiple..
    protected ArrayList m_values = new ArrayList(); // if multiple..
    protected bool m_multiple = false;
    protected string m_htmlCode = null;
    protected string m_queryString = "";

    public AllmusicSiteScraper()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    public bool IsMultiple()
    {
      return m_multiple;
    }

    public string[] GetItemsFound()
    {
      return (string[]) m_values.ToArray(typeof (string));
    }

    public string GetHtmlContent()
    {
      return m_htmlCode;
    }

    public bool FindInfoByIndex(int index)
    {
      if (index < 0 || index > m_codes.Count - 1)
      {
        return false;
      }

      string strGetData = m_queryString + m_codes[index];

      string strHTML = GetHTTP(MAINURL + URLPROGRAM + "?" + strGetData);
      if (strHTML.Length == 0)
      {
        return false;
      }

      m_htmlCode = strHTML; // save the html content...
      return true;
    }

    public bool FindInfo(SearchBy searchBy, string searchStr)
    {
      HTMLUtil util = new HTMLUtil();
      searchStr = searchStr.Replace(",", ""); // Remove Comma, as it causes problems with Search
      string strPostData = String.Format("P=amg&opt1={0}&sql={1}&Image1.x=18&Image1.y=14", (int) searchBy,
                                         HttpUtility.UrlEncode(searchStr));

      string strHTML = PostHTTP(MAINURL + URLPROGRAM, strPostData);
      if (strHTML.Length == 0)
      {
        return false;
      }

      m_htmlCode = strHTML; // save the html content...

      Regex multiples = new Regex(
        @"\sSearch\sResults\sfor:",
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

      if (multiples.IsMatch(strHTML))
      {
        string pattern = "bogus";
        if (searchBy.ToString().Equals("Artists"))
        {
          pattern = @"<a\shref.*?sql=(?<code>(11:|41).*?)"">(?<name>.*?)</a>.*?<TD" +
                    @"\sclass.*?>(?<name2>.*?)</TD>.*?""cell"">(?<name3>.*?)</td>";
        }
        else if (searchBy.ToString().Equals("Albums")) // below patter needs to be checked
        {
          pattern = @"""cell"">(?<name2>.*?)</TD>.*?style.*?word;"">(?<name3>.*?)<" +
                    @"/TD>.*?onclick=""z\('(?<code>.*?)'\)"">(?<name>.*?)</a>";
        }


        Match m;
        Regex itemsFoundFromSite = new Regex(
          pattern,
          RegexOptions.IgnoreCase
          | RegexOptions.Multiline
          | RegexOptions.IgnorePatternWhitespace
          | RegexOptions.Compiled
          );


        for (m = itemsFoundFromSite.Match(strHTML); m.Success; m = m.NextMatch())
        {
          string code = m.Groups["code"].ToString();
          string name = m.Groups["name"].ToString();
          string detail = m.Groups["name2"].ToString();
          string detail2 = m.Groups["name3"].ToString();

          util.RemoveTags(ref name);
          util.ConvertHTMLToAnsi(name, out name);

          util.RemoveTags(ref detail);
          util.ConvertHTMLToAnsi(detail, out detail);

          util.RemoveTags(ref detail);
          util.ConvertHTMLToAnsi(detail, out detail);

          util.RemoveTags(ref detail2);
          util.ConvertHTMLToAnsi(detail2, out detail2);

          detail += " - " + detail2;
          Console.Out.WriteLine("code = {0}, name = {1}, detail = {2}", code, name, detail);
          if (detail.Length > 0)
          {
            m_codes.Add(code);
            m_values.Add(name + " - " + detail);
          }
          else
          {
            m_codes.Add(code);
            m_values.Add(name);
          }
        }
        m_queryString = JAVASCRIPTZ;
        Console.Out.WriteLine("url = {0}", m_queryString);
        m_multiple = true;
      }
      else // found the right one
      {
      }
      return true;
    }

    internal static string PostHTTP(string strURL, string strData)
    {
      try
      {
        string strBody;

        string strUri = String.Format("{0}?{1}", strURL, strData);
        HttpWebRequest req = (HttpWebRequest) WebRequest.Create(strUri);
        req.ProtocolVersion = HttpVersion.Version11;
        req.UserAgent =
          "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.04307.00";
        HttpWebResponse result = (HttpWebResponse) req.GetResponse();
        Stream ReceiveStream = result.GetResponseStream();

        // 1252 is encoding for Windows format
        Encoding encode = Encoding.GetEncoding(1252);
        StreamReader sr = new StreamReader(ReceiveStream, encode);
        strBody = sr.ReadToEnd();
        return strBody;
      }
      catch (Exception)
      {
      }
      return "";
    }

    internal static string GetHTTP(string strURL)
    {
      string retval = null;

      // Initialize the WebRequest.
      WebRequest myRequest = WebRequest.Create(strURL);

      // Return the response. 
      WebResponse myResponse = myRequest.GetResponse();

      Stream ReceiveStream = myResponse.GetResponseStream();

      // 1252 is encoding for Windows format
      Encoding encode = Encoding.GetEncoding(1252);
      StreamReader sr = new StreamReader(ReceiveStream, encode);
      retval = sr.ReadToEnd();

      // Close the response to free resources.
      myResponse.Close();

      return retval;
    }

    [STAThread]
    private static void Main(string[] args)
    {
      MusicArtistInfo artist = new MusicArtistInfo();
      MusicAlbumInfo album = new MusicAlbumInfo();
      AllmusicSiteScraper prog = new AllmusicSiteScraper();

      prog.FindInfo(SearchBy.Artists, "Disturbed");
      artist.Parse(prog.GetHtmlContent());
    }
  }
}