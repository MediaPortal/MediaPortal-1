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
    List<MusicAlbumInfo> _albumList = new List<MusicAlbumInfo>();
    public MusicInfoScraper()
    {
    }
    public int Count
    {
      get { return _albumList.Count;}
    }
    public MusicAlbumInfo this[int index]
    {
      get { return _albumList[index];}
    }
    public bool FindAlbuminfo(string strAlbum, string artistName, int releaseYear)
    {
      _albumList.Clear();

//     strAlbum="1999";//escapolygy";

      // make request
      // type is 
      // http://www.allmusic.com/cg/amg.dll?P=amg&SQL=escapolygy&OPT1=2

      HTMLUtil  util=new HTMLUtil();
      string postData=String.Format("P=amg&SQL={0}&OPT1=2", HttpUtility.UrlEncode(strAlbum) );
		
      string html=PostHTTP("http://www.allmusic.com/cg/amg.dll", postData);
      if (html.Length==0) return false;

      // check if this is an album
      MusicAlbumInfo newAlbum = new MusicAlbumInfo();
      newAlbum.AlbumURL="http://www.allmusic.com/cg/amg.dll?"+postData;
      if ( newAlbum.Parse(html) )
      {
        _albumList.Add(newAlbum);
        return true;
      }

      string htmlLow=html;
      htmlLow=htmlLow.ToLower();
      int startOfTable=htmlLow.IndexOf("id=\"expansiontable1\"");
      if (startOfTable< 0) return false;
      startOfTable=htmlLow.LastIndexOf("<table",startOfTable);
      if (startOfTable < 0) return false;
      
      HTMLTable table=new HTMLTable();
      string strTable=html.Substring(startOfTable);
      table.Parse(strTable);

      for (int i=1; i < table.Rows; ++i)
      {
        HTMLTable.HTMLRow row=table.GetRow(i);
        string albumName="";
        string albumUrl="";
        string nameOfAlbum = "";
        string nameOfArtist = "";
        for (int iCol=0; iCol < row.Columns; ++iCol)
        {
          string column=row.GetColumValue(iCol);
          if (iCol==1 && (column.Length!=0)) albumName="("+column+")";
          if (iCol==2)
          {
            nameOfArtist = column;
            util.RemoveTags(ref nameOfArtist);
            if (!column.Equals("&nbsp;"))
              albumName = String.Format("- {0} {1}", nameOfArtist, albumName);
          }
          if (iCol==4)
          {
            string tempAlbum=column;
            util.RemoveTags(ref tempAlbum);
            albumName=String.Format("{0} {1}",tempAlbum,albumName);
            nameOfAlbum = tempAlbum;
          }
          if (iCol==4 && column.IndexOf("<a href=\"") >= 0)
          {
            int pos1=column.IndexOf("<a href=\"") ;
            pos1+=+"<a href=\"".Length;
            int iPos2=column.IndexOf("\">",pos1);
            if (iPos2 >= 0)
            {
                if (nameOfAlbum.Length == 0)
                  nameOfAlbum = albumName;

                // full album url:
                // http://www.allmusic.com/cg/amg.dll?p=amg&token=&sql=10:66jieal64xs7
                string url=column.Substring(pos1, iPos2-pos1);                
                string albumNameStripped;
                albumUrl=String.Format("http://www.allmusic.com{0}", url);
                MusicAlbumInfo newAlbumInfo = new MusicAlbumInfo();
                util.ConvertHTMLToAnsi(albumName, out albumNameStripped);
                newAlbumInfo.Title2=albumNameStripped;
                newAlbumInfo.AlbumURL=albumUrl;
                newAlbumInfo.Artist = util.ConvertHTMLToAnsi(nameOfArtist);
                newAlbumInfo.Title = util.ConvertHTMLToAnsi(nameOfAlbum);
                _albumList.Add(newAlbumInfo);
            
            }
          }
        }
      }
	
      // now sort
      _albumList.Sort(new AlbumSort(strAlbum,  artistName,  releaseYear));
      return true;
    }
    
    
    string PostHTTP(string url, string strData)
    {
      try
      {
        string body;
        WebRequest req = WebRequest.Create(url);
        req.Method="POST";
        req.ContentType = "application/x-www-form-urlencoded";

        byte [] bytes = null;
        // Get the data that is being posted (or sent) to the server
        bytes = System.Text.Encoding.ASCII.GetBytes (strData);
        req.ContentLength = bytes.Length;
        // 1. Get an output stream from the request object
        using (Stream outputStream = req.GetRequestStream())
        {

          // 2. Post the data out to the stream
          outputStream.Write(bytes, 0, bytes.Length);

          // 3. Close the output stream and send the data out to the web server
          outputStream.Close();
        }


        WebResponse result = req.GetResponse();
        using (Stream ReceiveStream = result.GetResponseStream())
        {
          Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
          StreamReader sr = new StreamReader(ReceiveStream, encode);
          body = sr.ReadToEnd();
          return body;
        }
      }
      catch(Exception)
      {
      }
      return "";
    }
  }
}
