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
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicAlbumInfo
  {
    
    string		m_strArtist="";
    string		m_strTitle="";
    string		m_strTitle2="";
    string		m_strDateOfRelease="";
    string		m_strGenre="";
    string		m_strTones="";
    string		m_strStyles="";
    string    m_strReview="";
    string	  m_strImageURL="";
    string    m_strAlbumURL="";
    string		m_strAlbumPath="";
    int				m_iRating=0;
    ArrayList m_songs=new ArrayList();
    bool      m_bLoaded=false;

    public MusicAlbumInfo()
    {
    }
   
    
    public string Artist
    {
      get { return m_strArtist;}
      set {m_strArtist=value.Trim();}
    }
    public string Title
    {
      get { return m_strTitle;}
      set {m_strTitle=value.Trim();}
    }
    public string Title2
    {
      get { return m_strTitle2;}
      set {m_strTitle2=value.Trim();}
    }
    public string DateOfRelease
    {
      get { 
        return m_strDateOfRelease;
      }
      set {
        m_strDateOfRelease=value.Trim();
        try
        {
          int iYear=Int32.Parse(m_strDateOfRelease);
        }
        catch(Exception)
        {
          m_strDateOfRelease="0";
        }
      }
    }
    public string Genre
    {
      get { return m_strGenre;}
      set {m_strGenre=value.Trim();}
    }
    public string Tones
    {
      get { return m_strTones;}
      set {m_strTones=value.Trim();}
    }
    public string Styles
    {
      get { return m_strStyles;}
      set {m_strStyles=value;}
    }
    public string Review
    {
      get { return m_strReview;}
      set {m_strReview=value.Trim();}
    }
    public string ImageURL
    {
      get { return m_strImageURL;}
      set { m_strImageURL=value.Trim();}
    }
    public string AlbumURL
    {
      get { return m_strAlbumURL;}
      set {m_strAlbumURL=value.Trim();}
    }
    public string AlbumPath
    {
      get { return m_strAlbumPath;}
      set {m_strAlbumPath=value.Trim();}
    }
    public int Rating
    {
      get { return m_iRating;}
      set {m_iRating=value;}
    }
    
    public int	NumberOfSongs
    {
      get {return m_songs.Count;}
    }

    public MusicSong GetSong(int iSong)
    {
      return (MusicSong)m_songs[iSong];
    }

    public bool Load()
    {
      try
      {
        string strBody;
        WebRequest req = WebRequest.Create(m_strAlbumURL);
        WebResponse result = req.GetResponse();
        Stream ReceiveStream = result.GetResponseStream();
        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
        StreamReader sr = new StreamReader( ReceiveStream, encode );
        strBody=sr.ReadToEnd();
        return Parse(strBody);
      }
      catch(Exception)
      {
      }
      return false;
    }

    public bool Parse(string strHTML)
    {
      m_songs.Clear();
      HTMLTable table ;
      string strTable;
      HTMLUtil  util = new HTMLUtil();
      string strHTMLLow=strHTML.ToLower();
      string strHTMLOrg=strHTML;
	
      //	Extract Cover URL
      int iStartOfCover=strHTMLLow.IndexOf("image.allmusic.com");
      if (iStartOfCover>=0)
      {
        iStartOfCover=strHTMLLow.LastIndexOf("<img", iStartOfCover);
        int iEndOfCover=strHTMLLow.IndexOf(">", iStartOfCover);
        string strCover=strHTMLLow.Substring(iStartOfCover, iEndOfCover-iStartOfCover);
        util.getAttributeOfTag(strCover, "src=", ref m_strImageURL);
        if (m_strImageURL.Length>0)
        {
          if (m_strImageURL[0]=='\"') m_strImageURL=m_strImageURL.Substring(1);
          if (m_strImageURL[m_strImageURL.Length-1]=='\"') m_strImageURL=m_strImageURL.Substring(0,m_strImageURL.Length-1);
        }
      }

      //	Extract Review
      int iStartOfReview=strHTMLLow.IndexOf("id=\"bio\"");
      if (iStartOfReview>=0)
      {
        iStartOfReview=strHTMLLow.IndexOf("<table", iStartOfReview);
        if (iStartOfReview>=0)
        {
          table = new HTMLTable();
          strTable=strHTML.Substring(iStartOfReview);
          table.Parse(strTable);

          if (table.Rows>0)
          {
            HTMLTable.HTMLRow row=table.GetRow(1);
            string strReview=row.GetColumValue(0);
            util.RemoveTags(ref strReview);
            util.ConvertHTMLToAnsi(strReview, out m_strReview);
            TrimRight(ref m_strReview,"Read More...");
          }
        }
      }

      if (m_strReview.Length==0)
        m_strReview=GUILocalizeStrings.Get(414);
	
      //	Extract album, artist...
      int iStartOfTable=strHTMLLow.IndexOf("<table cellpadding=\"0\" cellspacing=\"0\">");
      if (iStartOfTable< 0) return false;

      table=new HTMLTable();
      strTable=strHTML.Substring(iStartOfTable);
      table.Parse(strTable);

      //	Check if page has the album browser
      int iStartRow=2;
      if (strHTMLLow.IndexOf("class=\"album-browser\"")==-1)
        iStartRow=1;

      for (int iRow=iStartRow; iRow < table.Rows; iRow++)
      {
        HTMLTable.HTMLRow row=table.GetRow(iRow);

        string strColumn=row.GetColumValue(0);
        HTMLTable valueTable =new HTMLTable();
        valueTable.Parse(strColumn);
        strColumn=valueTable.GetRow(0).GetColumValue(0);
        util.RemoveTags(ref strColumn);

        if (strColumn.IndexOf("Artist") >=0 && valueTable.Rows>=2)
        {
          string strValue=valueTable.GetRow(2).GetColumValue(0);
          m_strArtist=strValue;
          util.RemoveTags(ref m_strArtist);
        }
        if (strColumn.IndexOf("Album") >=0 && valueTable.Rows>=2)
        {
          string strValue=valueTable.GetRow(2).GetColumValue(0);
          m_strTitle=strValue;
          util.RemoveTags(ref m_strTitle);
        }
        if (strColumn.IndexOf("Release Date") >=0 && valueTable.Rows>=2)
        {
          string strValue=valueTable.GetRow(2).GetColumValue(0);
          m_strDateOfRelease=strValue;
          util.RemoveTags(ref m_strDateOfRelease);

          //	extract the year out of something like "1998 (release)" or "12 feb 2003"
          int nPos=m_strDateOfRelease.IndexOf("19");
          if (nPos>-1)
          {
            if ((int)m_strDateOfRelease.Length >= nPos+3 && Char.IsDigit(m_strDateOfRelease[nPos+2])&&Char.IsDigit(m_strDateOfRelease[nPos+3]))
            {
              string strYear=m_strDateOfRelease.Substring(nPos, 4);
              m_strDateOfRelease=strYear;
            }
          else
          {
            nPos=m_strDateOfRelease.IndexOf("19", nPos+2);
            if (nPos>-1)
            {
              if ((int)m_strDateOfRelease.Length >= nPos+3 && Char.IsDigit(m_strDateOfRelease[nPos+2])&&Char.IsDigit(m_strDateOfRelease[nPos+3]))
              {
                string strYear=m_strDateOfRelease.Substring(nPos, 4);
                m_strDateOfRelease=strYear;
              }
            }
          }
          }

          nPos=m_strDateOfRelease.IndexOf("20");
          if (nPos>-1)
          {
            if ((int)m_strDateOfRelease.Length > nPos+3 && Char.IsDigit(m_strDateOfRelease[nPos+2])&&Char.IsDigit(m_strDateOfRelease[nPos+3]))
            {
              string strYear=m_strDateOfRelease.Substring(nPos, 4);
              m_strDateOfRelease=strYear;
            }
          else
          {
            nPos=m_strDateOfRelease.IndexOf("20", nPos+1);
            if (nPos>-1)
            {
              if ((int)m_strDateOfRelease.Length > nPos+3 && Char.IsDigit(m_strDateOfRelease[nPos+2])&&Char.IsDigit(m_strDateOfRelease[nPos+3]))
              {
                string strYear=m_strDateOfRelease.Substring(nPos, 4);
                m_strDateOfRelease=strYear;
              }
            }
          }
          }
        }
        if (strColumn.IndexOf("Genre") >=0 && valueTable.Rows>=1)
        {
          strHTML=valueTable.GetRow(1).GetColumValue(0);
          string strTag="";
          int iStartOfGenre=util.FindTag(strHTML,"<li",ref strTag,0);
          if (iStartOfGenre>=0)
          {
            iStartOfGenre+=(int)strTag.Length;
            int iEndOfGenre=util.FindClosingTag(strHTML,"li",ref  strTag,iStartOfGenre)-1;
            if (iEndOfGenre < 0)
            {
              iEndOfGenre=(int)strHTML.Length;
            }
				
            string strValue=strHTML.Substring(iStartOfGenre,1+iEndOfGenre-iStartOfGenre);
            m_strGenre=strValue;
            util.RemoveTags(ref m_strGenre);
          }

          if (valueTable.GetRow(0).Columns>=2)
          {
            strColumn=valueTable.GetRow(0).GetColumValue(2);
            util.RemoveTags(ref strColumn);

            if (strColumn.IndexOf("Styles") >=0)
            {
              strHTML=valueTable.GetRow(1).GetColumValue(1);
              
              int iStartOfStyle=0;
              while (iStartOfStyle>=0)
              {
                iStartOfStyle=util.FindTag(strHTML, "<li", ref strTag, iStartOfStyle);
                if (iStartOfStyle < 0) break;
                iStartOfStyle+=(int)strTag.Length;
                int iEndOfStyle=util.FindClosingTag(strHTML, "li",ref  strTag, iStartOfStyle)-1;
                if (iEndOfStyle < 0)
                  break;
						
                string strValue=strHTML.Substring(iStartOfStyle, 1+iEndOfStyle-iStartOfStyle);
                util.RemoveTags(ref strValue);
                m_strStyles+=strValue + ", ";
              }

              TrimRight(ref m_strStyles,", ");
            }
          }
        }
        if (strColumn.IndexOf("Moods") >=0)
        {
          strHTML=valueTable.GetRow(1).GetColumValue(0);
          string strTag="";
          int iStartOfMoods=0;
          while (iStartOfMoods>=0)
          {
            iStartOfMoods=util.FindTag(strHTML, "<li", ref strTag, iStartOfMoods);
            if (iStartOfMoods<0) break;
            iStartOfMoods+=(int)strTag.Length;
            int iEndOfMoods=util.FindClosingTag(strHTML, "li", ref strTag, iStartOfMoods)-1;
            if (iEndOfMoods < 0)
              break;
					
            string strValue=strHTML.Substring(iStartOfMoods, 1+iEndOfMoods-iStartOfMoods);
            util.RemoveTags(ref strValue);
            m_strTones+=strValue + ", ";
          }

          TrimRight(ref m_strTones,", ");
        }
        if (strColumn.IndexOf("Rating") >=0)
        {
          string strValue=valueTable.GetRow(1).GetColumValue(0);
          string strRating="";
          util.getAttributeOfTag(strValue, "src=", ref strRating);
          strRating=strRating.Remove(0, 25);
          strRating=strRating.Remove(1, 4);
          try 
          {
            m_iRating=Int32.Parse(strRating);
          } catch(Exception){}
        }
      }

      //	Set to "Not available" if no value from web
      if (m_strArtist.Length==0)
        m_strArtist=GUILocalizeStrings.Get(416);
      if (m_strDateOfRelease.Length==0)
        m_strDateOfRelease=GUILocalizeStrings.Get(416);
      if (m_strGenre.Length==0)
        m_strGenre=GUILocalizeStrings.Get(416);
      if (m_strTones.Length==0)
        m_strTones=GUILocalizeStrings.Get(416);
      if (m_strStyles.Length==0)
        m_strStyles=GUILocalizeStrings.Get(416);
      if (m_strTitle.Length==0)
        m_strTitle=GUILocalizeStrings.Get(416);


      // parse songs...
      strHTML=strHTMLOrg;
      iStartOfTable=strHTMLLow.IndexOf("id=\"expansiontable1\"",0);
      if (iStartOfTable >= 0)
      {
        iStartOfTable=strHTMLLow.LastIndexOf("<table",iStartOfTable);
        if (iStartOfTable >= 0)
        {
          strTable=strHTML.Substring(iStartOfTable);
          table.Parse(strTable);
          for (int iRow=1; iRow < table.Rows; iRow++)
          {
            HTMLTable.HTMLRow row=table.GetRow(iRow);
            int iCols=row.Columns;
            if (iCols >=7)
            {

              //	Tracknumber
              int iTrack=0;
              try{
                iTrack=Int32.Parse(row.GetColumValue(2));
              } catch(Exception){}

              //	Songname
              string strValue, strName;
              strValue=row.GetColumValue(4);
              util.RemoveTags(ref strValue);
              strValue=strValue.Trim();
              if (strValue.IndexOf("[*]")>-1)
                TrimRight(ref strValue,"[*]");
              util.ConvertHTMLToAnsi(strValue, out strName);

              //	Duration
              int iDuration=0;
              string strDuration=row.GetColumValue(6);
              int iPos=strDuration.IndexOf(":");
              if (iPos>=0)
              {
                string strMin, strSec;
                strMin=strDuration.Substring(0,iPos);
                iPos++;
                strSec=strDuration.Substring(iPos);
                int iMin=0,iSec=0;
                try
                {
                  iMin=Int32.Parse(strMin);
                  iSec=Int32.Parse(strSec);
                }
                catch(Exception){}
                iDuration=iMin*60+iSec;
              }

              //	Create new song object
              MusicSong newSong = new MusicSong();
              newSong.Track=iTrack;
              newSong.SongName=strName;
              newSong.Duration= iDuration;
              m_songs.Add(newSong);
            }
          }
        }
      }
      if (m_strTitle2.Length==0) m_strTitle2=m_strTitle;

      Loaded=true;
      return true;
    }

    public void Set(AlbumInfo album)
    {
      Artist=album.Artist;
      Title=album.Album;
      m_strDateOfRelease=String.Format("{0}",album.Year);
      Genre=album.Genre;
      Tones=album.Tones;
      Styles=album.Styles;
      Review=album.Review;
      ImageURL=album.Image;
      Rating=album.Rating;
      Tracks=album.Tracks;
      Title2="";
      Loaded=true;
    }
    public void SetSongs(ArrayList list)
    {
      m_songs.Clear();
      foreach (MusicSong song in list)
      {
        m_songs.Add(song);
      }
    }
    public bool Loaded
    {
      get { return m_bLoaded;}
      set { m_bLoaded=value;}
    }
    void TrimRight(ref string strTxt, string strTag)
    {
      int pos=strTxt.LastIndexOf(strTag);
      if (pos <0) return;
      if (pos+strTag.Length==strTxt.Length)
      {
        strTxt=strTxt.Remove(pos,strTag.Length);
      }
    }

    public string Tracks
    {
      get
      {
        string strTracks="";
        foreach (MusicSong song in m_songs)
        {
          string strTmp=String.Format("{0}@{1}@{2}|", song.Track, song.SongName, song.Duration);
          strTracks=strTracks+strTmp;
        }
        return strTracks;
      }
      set
      {
        m_songs.Clear();
        Tokens token = new Tokens(value, new char[] { '|'} );
        foreach (string strToken in token)
        {
          Tokens token2 = new Tokens(strToken, new char[] { '@'} );
          MusicSong song = new  MusicSong();
          int iTok=0;
          foreach (string strCol in token2)
          {
            switch (iTok)
            {
              case 0:
                try
                {
                  song.Track = Int32.Parse(strCol);
                }
                catch(Exception)
                {
                }
                break;
              case 1:
                song.SongName = strCol;
                break;

              case 2:
                try
                {
                  song.Duration = Int32.Parse(strCol);
                }
                catch(Exception)
                {
                }
                break;
            }
            iTok++;
          }
          if (song.Track>0)
            m_songs.Add(song);
        }
      }
    }
  }
}
