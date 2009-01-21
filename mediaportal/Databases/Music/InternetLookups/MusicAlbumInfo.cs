#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicAlbumInfo
  {
    private string m_artist = "";
    private string m_strTitle = "";
    private string m_strTitle2 = "";
    private string m_strDateOfRelease = "";
    private string m_strGenre = "";
    private string m_strTones = "";
    private string m_strStyles = "";
    private string m_strReview = "";
    private string m_strImageURL = "";
    private string m_albumUrl = "";
    private string m_strAlbumPath = "";
    private int m_iRating = 0;
    private ArrayList m_songs = new ArrayList();
    private bool m_bLoaded = false;

    public MusicAlbumInfo()
    {
    }


    public string Artist
    {
      get { return m_artist; }
      set { m_artist = value.Trim(); }
    }

    public string Title
    {
      get { return m_strTitle; }
      set { m_strTitle = value.Trim(); }
    }

    public string Title2
    {
      get { return m_strTitle2; }
      set { m_strTitle2 = value.Trim(); }
    }

    public string DateOfRelease
    {
      get { return m_strDateOfRelease; }
      set
      {
        m_strDateOfRelease = value.Trim();
        try
        {
          int iYear = Int32.Parse(m_strDateOfRelease);
        }
        catch (Exception)
        {
          m_strDateOfRelease = "0";
        }
      }
    }

    public string Genre
    {
      get { return m_strGenre; }
      set { m_strGenre = value.Trim(); }
    }

    public string Tones
    {
      get { return m_strTones; }
      set { m_strTones = value.Trim(); }
    }

    public string Styles
    {
      get { return m_strStyles; }
      set { m_strStyles = value; }
    }

    public string Review
    {
      get { return m_strReview; }
      set { m_strReview = value.Trim(); }
    }

    public string ImageURL
    {
      get { return m_strImageURL; }
      set { m_strImageURL = value.Trim(); }
    }

    public string AlbumURL
    {
      get { return m_albumUrl; }
      set { m_albumUrl = value.Trim(); }
    }

    public string AlbumPath
    {
      get { return m_strAlbumPath; }
      set { m_strAlbumPath = value.Trim(); }
    }

    public int Rating
    {
      get { return m_iRating; }
      set { m_iRating = value; }
    }

    public int NumberOfSongs
    {
      get { return m_songs.Count; }
    }

    public MusicSong GetSong(int iSong)
    {
      return (MusicSong) m_songs[iSong];
    }

    public bool Load()
    {
      try
      {
        string body;
        WebRequest req = WebRequest.Create(m_albumUrl);
        WebResponse result = req.GetResponse();
        Stream ReceiveStream = result.GetResponseStream();
        Encoding encode = Encoding.GetEncoding("utf-8");
        StreamReader sr = new StreamReader(ReceiveStream, encode);
        body = sr.ReadToEnd();
        return Parse(body);
      }
      catch (Exception)
      {
      }
      return false;
    }

    public bool Parse(string html)
    {
      m_songs.Clear();
      HTMLTable table;
      string strTable;
      HTMLUtil util = new HTMLUtil();
      string htmlLow = html.ToLower();
      string htmlOrg = html;

      //	Extract Cover URL
      int iStartOfCover = htmlLow.IndexOf("image.allmusic.com");
      if (iStartOfCover >= 0)
      {
        iStartOfCover = htmlLow.LastIndexOf("<img", iStartOfCover);
        int iEndOfCover = htmlLow.IndexOf(">", iStartOfCover);
        string strCover = htmlLow.Substring(iStartOfCover, iEndOfCover - iStartOfCover);
        util.getAttributeOfTag(strCover, "src=", ref m_strImageURL);
        if (m_strImageURL.Length > 0)
        {
          if (m_strImageURL[0] == '\"')
          {
            m_strImageURL = m_strImageURL.Substring(1);
          }
          if (m_strImageURL[m_strImageURL.Length - 1] == '\"')
          {
            m_strImageURL = m_strImageURL.Substring(0, m_strImageURL.Length - 1);
          }
        }
      }

      //	Extract Review
      int iStartOfReview = htmlLow.IndexOf("id=\"bio\"");
      if (iStartOfReview >= 0)
      {
        iStartOfReview = htmlLow.IndexOf("<table", iStartOfReview);
        if (iStartOfReview >= 0)
        {
          table = new HTMLTable();
          strTable = html.Substring(iStartOfReview);
          table.Parse(strTable);

          if (table.Rows > 0)
          {
            HTMLTable.HTMLRow row = table.GetRow(1);
            string strReview = row.GetColumValue(0);
            util.RemoveTags(ref strReview);
            util.ConvertHTMLToAnsi(strReview, out m_strReview);
            TrimRight(ref m_strReview, "Read More...");
          }
        }
      }

      if (m_strReview.Length == 0)
      {
        m_strReview = GUILocalizeStrings.Get(414);
      }

      //	Extract album, artist...
      int startOfTable = htmlLow.IndexOf("<table cellpadding=\"0\" cellspacing=\"0\">");
      if (startOfTable < 0)
      {
        return false;
      }

      table = new HTMLTable();
      strTable = html.Substring(startOfTable);
      table.Parse(strTable);

      //	Check if page has the album browser
      int iStartRow = 2;
      if (htmlLow.IndexOf("class=\"album-browser\"") == -1)
      {
        iStartRow = 1;
      }

      for (int iRow = iStartRow; iRow < table.Rows; iRow++)
      {
        HTMLTable.HTMLRow row = table.GetRow(iRow);

        string columnn = row.GetColumValue(0);
        HTMLTable valueTable = new HTMLTable();
        valueTable.Parse(columnn);
        columnn = valueTable.GetRow(0).GetColumValue(0);
        util.RemoveTags(ref columnn);

        if (columnn.IndexOf("Artist") >= 0 && valueTable.Rows >= 2)
        {
          string strValue = valueTable.GetRow(2).GetColumValue(0);
          m_artist = strValue;
          util.RemoveTags(ref m_artist);
        }
        if (columnn.IndexOf("Album") >= 0 && valueTable.Rows >= 2)
        {
          string strValue = valueTable.GetRow(2).GetColumValue(0);
          m_strTitle = strValue;
          util.RemoveTags(ref m_strTitle);
        }
        if (columnn.IndexOf("Release Date") >= 0 && valueTable.Rows >= 2)
        {
          string strValue = valueTable.GetRow(2).GetColumValue(0);
          m_strDateOfRelease = strValue;
          util.RemoveTags(ref m_strDateOfRelease);

          //	extract the year out of something like "1998 (release)" or "12 feb 2003"
          int nPos = m_strDateOfRelease.IndexOf("19");
          if (nPos > -1)
          {
            if ((int) m_strDateOfRelease.Length >= nPos + 3 && Char.IsDigit(m_strDateOfRelease[nPos + 2]) &&
                Char.IsDigit(m_strDateOfRelease[nPos + 3]))
            {
              string strYear = m_strDateOfRelease.Substring(nPos, 4);
              m_strDateOfRelease = strYear;
            }
            else
            {
              nPos = m_strDateOfRelease.IndexOf("19", nPos + 2);
              if (nPos > -1)
              {
                if ((int) m_strDateOfRelease.Length >= nPos + 3 && Char.IsDigit(m_strDateOfRelease[nPos + 2]) &&
                    Char.IsDigit(m_strDateOfRelease[nPos + 3]))
                {
                  string strYear = m_strDateOfRelease.Substring(nPos, 4);
                  m_strDateOfRelease = strYear;
                }
              }
            }
          }

          nPos = m_strDateOfRelease.IndexOf("20");
          if (nPos > -1)
          {
            if ((int) m_strDateOfRelease.Length > nPos + 3 && Char.IsDigit(m_strDateOfRelease[nPos + 2]) &&
                Char.IsDigit(m_strDateOfRelease[nPos + 3]))
            {
              string strYear = m_strDateOfRelease.Substring(nPos, 4);
              m_strDateOfRelease = strYear;
            }
            else
            {
              nPos = m_strDateOfRelease.IndexOf("20", nPos + 1);
              if (nPos > -1)
              {
                if ((int) m_strDateOfRelease.Length > nPos + 3 && Char.IsDigit(m_strDateOfRelease[nPos + 2]) &&
                    Char.IsDigit(m_strDateOfRelease[nPos + 3]))
                {
                  string strYear = m_strDateOfRelease.Substring(nPos, 4);
                  m_strDateOfRelease = strYear;
                }
              }
            }
          }
        }
        if (columnn.IndexOf("Genre") >= 0 && valueTable.Rows >= 1)
        {
          html = valueTable.GetRow(1).GetColumValue(0);
          string strTag = "";
          int iStartOfGenre = util.FindTag(html, "<li", ref strTag, 0);
          if (iStartOfGenre >= 0)
          {
            iStartOfGenre += (int) strTag.Length;
            int iEndOfGenre = util.FindClosingTag(html, "li", ref strTag, iStartOfGenre) - 1;
            if (iEndOfGenre < 0)
            {
              iEndOfGenre = (int) html.Length;
            }

            string strValue = html.Substring(iStartOfGenre, 1 + iEndOfGenre - iStartOfGenre);
            m_strGenre = strValue;
            util.RemoveTags(ref m_strGenre);
          }

          if (valueTable.GetRow(0).Columns >= 2)
          {
            columnn = valueTable.GetRow(0).GetColumValue(2);
            util.RemoveTags(ref columnn);

            if (columnn.IndexOf("Styles") >= 0)
            {
              html = valueTable.GetRow(1).GetColumValue(1);

              int iStartOfStyle = 0;
              while (iStartOfStyle >= 0)
              {
                iStartOfStyle = util.FindTag(html, "<li", ref strTag, iStartOfStyle);
                if (iStartOfStyle < 0)
                {
                  break;
                }
                iStartOfStyle += (int) strTag.Length;
                int iEndOfStyle = util.FindClosingTag(html, "li", ref strTag, iStartOfStyle) - 1;
                if (iEndOfStyle < 0)
                {
                  break;
                }

                string strValue = html.Substring(iStartOfStyle, 1 + iEndOfStyle - iStartOfStyle);
                util.RemoveTags(ref strValue);
                m_strStyles += strValue + ", ";
              }

              TrimRight(ref m_strStyles, ", ");
            }
          }
        }
        if (columnn.IndexOf("Moods") >= 0)
        {
          html = valueTable.GetRow(1).GetColumValue(0);
          string strTag = "";
          int iStartOfMoods = 0;
          while (iStartOfMoods >= 0)
          {
            iStartOfMoods = util.FindTag(html, "<li", ref strTag, iStartOfMoods);
            if (iStartOfMoods < 0)
            {
              break;
            }
            iStartOfMoods += (int) strTag.Length;
            int iEndOfMoods = util.FindClosingTag(html, "li", ref strTag, iStartOfMoods) - 1;
            if (iEndOfMoods < 0)
            {
              break;
            }

            string strValue = html.Substring(iStartOfMoods, 1 + iEndOfMoods - iStartOfMoods);
            util.RemoveTags(ref strValue);
            m_strTones += strValue + ", ";
          }

          TrimRight(ref m_strTones, ", ");
        }
        if (columnn.IndexOf("Rating") >= 0)
        {
          string strValue = valueTable.GetRow(1).GetColumValue(0);
          string strRating = "";
          util.getAttributeOfTag(strValue, "src=", ref strRating);
          strRating = strRating.Remove(0, 25);
          strRating = strRating.Remove(1, 4);
          try
          {
            m_iRating = Int32.Parse(strRating);
          }
          catch (Exception)
          {
          }
        }
      }

      //	Set to "Not available" if no value from web
      if (m_artist.Length == 0)
      {
        m_artist = GUILocalizeStrings.Get(416);
      }
      if (m_strDateOfRelease.Length == 0)
      {
        m_strDateOfRelease = GUILocalizeStrings.Get(416);
      }
      if (m_strGenre.Length == 0)
      {
        m_strGenre = GUILocalizeStrings.Get(416);
      }
      if (m_strTones.Length == 0)
      {
        m_strTones = GUILocalizeStrings.Get(416);
      }
      if (m_strStyles.Length == 0)
      {
        m_strStyles = GUILocalizeStrings.Get(416);
      }
      if (m_strTitle.Length == 0)
      {
        m_strTitle = GUILocalizeStrings.Get(416);
      }


      // parse songs...
      html = htmlOrg;
      startOfTable = htmlLow.IndexOf("id=\"expansiontable1\"", 0);
      if (startOfTable >= 0)
      {
        startOfTable = htmlLow.LastIndexOf("<table", startOfTable);
        if (startOfTable >= 0)
        {
          strTable = html.Substring(startOfTable);
          table.Parse(strTable);
          for (int iRow = 1; iRow < table.Rows; iRow++)
          {
            HTMLTable.HTMLRow row = table.GetRow(iRow);
            int iCols = row.Columns;
            if (iCols >= 7)
            {
              //	Tracknumber
              int iTrack = 0;
              try
              {
                iTrack = Int32.Parse(row.GetColumValue(2));
              }
              catch (Exception)
              {
              }

              //	Songname
              string strValue, strName;
              strValue = row.GetColumValue(4);
              util.RemoveTags(ref strValue);
              strValue = strValue.Trim();
              if (strValue.IndexOf("[*]") > -1)
              {
                TrimRight(ref strValue, "[*]");
              }
              util.ConvertHTMLToAnsi(strValue, out strName);

              //	Duration
              int iDuration = 0;
              string strDuration = row.GetColumValue(6);
              int iPos = strDuration.IndexOf(":");
              if (iPos >= 0)
              {
                string strMin, strSec;
                strMin = strDuration.Substring(0, iPos);
                iPos++;
                strSec = strDuration.Substring(iPos);
                int iMin = 0, iSec = 0;
                try
                {
                  iMin = Int32.Parse(strMin);
                  iSec = Int32.Parse(strSec);
                }
                catch (Exception)
                {
                }
                iDuration = iMin*60 + iSec;
              }

              //	Create new song object
              MusicSong newSong = new MusicSong();
              newSong.Track = iTrack;
              newSong.SongName = strName;
              newSong.Duration = iDuration;
              m_songs.Add(newSong);
            }
          }
        }
      }
      if (m_strTitle2.Length == 0)
      {
        m_strTitle2 = m_strTitle;
      }

      Loaded = true;
      return true;
    }

    public void Set(AlbumInfo album)
    {
      Artist = album.Artist;
      Title = album.Album;
      m_strDateOfRelease = String.Format("{0}", album.Year);
      Genre = album.Genre;
      Tones = album.Tones;
      Styles = album.Styles;
      Review = album.Review;
      ImageURL = album.Image;
      Rating = album.Rating;
      Tracks = album.Tracks;
      Title2 = "";
      Loaded = true;
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
      get { return m_bLoaded; }
      set { m_bLoaded = value; }
    }

    private void TrimRight(ref string strTxt, string strTag)
    {
      int pos = strTxt.LastIndexOf(strTag);
      if (pos < 0)
      {
        return;
      }
      if (pos + strTag.Length == strTxt.Length)
      {
        strTxt = strTxt.Remove(pos, strTag.Length);
      }
    }

    public string Tracks
    {
      get
      {
        string strTracks = "";
        foreach (MusicSong song in m_songs)
        {
          string strTmp = String.Format("{0}@{1}@{2}|", song.Track, song.SongName, song.Duration);
          strTracks = strTracks + strTmp;
        }
        return strTracks;
      }
      set
      {
        m_songs.Clear();
        Tokens token = new Tokens(value, new char[] {'|'});
        foreach (string strToken in token)
        {
          Tokens token2 = new Tokens(strToken, new char[] {'@'});
          MusicSong song = new MusicSong();
          int iTok = 0;
          foreach (string strCol in token2)
          {
            switch (iTok)
            {
              case 0:
                try
                {
                  song.Track = Int32.Parse(strCol);
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
                break;
            }
            iTok++;
          }
          if (song.Track > 0)
          {
            m_songs.Add(song);
          }
        }
      }
    }
  }
}