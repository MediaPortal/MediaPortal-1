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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicInfoScraper
  {
    private List<MusicAlbumInfo> _albumList = new List<MusicAlbumInfo>();

    public MusicInfoScraper()
    {
    }

    public int Count
    {
      get { return _albumList.Count; }
    }

    public MusicAlbumInfo this[int index]
    {
      get { return _albumList[index]; }
    }

    public bool FindAlbuminfo(string strAlbum, string artistName, int releaseYear)
    {
      _albumList.Clear();

//     strAlbum="1999";//escapolygy";

      // make request
      // type is 
      // http://www.allmusic.com/cg/amg.dll?P=amg&SQL=escapolygy&OPT1=2

      HTMLUtil util = new HTMLUtil();
      string postData = String.Format("P=amg&SQL={0}&OPT1=2", HttpUtility.UrlEncode(strAlbum));

      string html = PostHTTP("http://www.allmusic.com/cg/amg.dll", postData);
      if (html.Length == 0)
      {
        return false;
      }

      // check if this is an album
      MusicAlbumInfo newAlbum = new MusicAlbumInfo();
      newAlbum.AlbumURL = "http://www.allmusic.com/cg/amg.dll?" + postData;
      if (newAlbum.Parse(html))
      {
        _albumList.Add(newAlbum);
        return true;
      }

      string htmlLow = html;
      htmlLow = htmlLow.ToLower();
      int startOfTable = htmlLow.IndexOf("id=\"expansiontable1\"");
      if (startOfTable < 0)
      {
        return false;
      }
      startOfTable = htmlLow.LastIndexOf("<table", startOfTable);
      if (startOfTable < 0)
      {
        return false;
      }

      HTMLTable table = new HTMLTable();
      string strTable = html.Substring(startOfTable);
      table.Parse(strTable);

      for (int i = 1; i < table.Rows; ++i)
      {
        HTMLTable.HTMLRow row = table.GetRow(i);
        string albumName = "";
        string albumUrl = "";
        string nameOfAlbum = "";
        string nameOfArtist = "";
        for (int iCol = 0; iCol < row.Columns; ++iCol)
        {
          string column = row.GetColumValue(iCol);
          if (iCol == 1 && (column.Length != 0))
          {
            albumName = "(" + column + ")";
          }
          if (iCol == 2)
          {
            nameOfArtist = column;
            util.RemoveTags(ref nameOfArtist);
            if (!column.Equals("&nbsp;"))
            {
              albumName = String.Format("- {0} {1}", nameOfArtist, albumName);
            }
          }
          if (iCol == 4)
          {
            string tempAlbum = column;
            util.RemoveTags(ref tempAlbum);
            albumName = String.Format("{0} {1}", tempAlbum, albumName);
            nameOfAlbum = tempAlbum;
          }
          if (iCol == 4 && column.IndexOf("<a href=\"") >= 0)
          {
            int pos1 = column.IndexOf("<a href=\"");
            pos1 += +"<a href=\"".Length;
            int iPos2 = column.IndexOf("\">", pos1);
            if (iPos2 >= 0)
            {
              if (nameOfAlbum.Length == 0)
              {
                nameOfAlbum = albumName;
              }

              // full album url:
              // http://www.allmusic.com/cg/amg.dll?p=amg&token=&sql=10:66jieal64xs7
              string url = column.Substring(pos1, iPos2 - pos1);
              string albumNameStripped;
              albumUrl = String.Format("http://www.allmusic.com{0}", url);
              MusicAlbumInfo newAlbumInfo = new MusicAlbumInfo();
              util.ConvertHTMLToAnsi(albumName, out albumNameStripped);
              newAlbumInfo.Title2 = albumNameStripped;
              newAlbumInfo.AlbumURL = util.ConvertHTMLToAnsi(albumUrl);
              newAlbumInfo.Artist = util.ConvertHTMLToAnsi(nameOfArtist);
              newAlbumInfo.Title = util.ConvertHTMLToAnsi(nameOfAlbum);
              _albumList.Add(newAlbumInfo);
            }
          }
        }
      }

      // now sort
      _albumList.Sort(new AlbumSort(strAlbum, artistName, releaseYear));
      return true;
    }

    private string PostHTTP(string url, string strData)
    {
      try
      {
        string strBody;

        string strUri = String.Format("{0}?{1}", url, strData);
        HttpWebRequest req = (HttpWebRequest) WebRequest.Create(strUri);
        req.ProtocolVersion = HttpVersion.Version11;
        req.UserAgent =
          "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.04307.00";
        HttpWebResponse result = (HttpWebResponse) req.GetResponse();
        Stream ReceiveStream = result.GetResponseStream();

        // 1252 is encoding for Windows format
        Encoding encode = Encoding.GetEncoding("utf-8");
        StreamReader sr = new StreamReader(ReceiveStream, encode);
        strBody = sr.ReadToEnd();
        return strBody;
      }
      catch (Exception)
      {
      }
      return "";
    }
  }
}