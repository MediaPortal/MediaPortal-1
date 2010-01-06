#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicArtistInfo
  {
    private string m_strArtistName = "";
    private string m_strArtistPictureURL = "";
    private string m_strAKA = "";
    private string m_strBorn = "";
    private string m_strYearsActive = "";
    private string m_strGenres = "";
    private string m_strTones = "";
    private string m_strStyles = "";
    private string m_strInstruments = "";
    //string		m_strLabels="";
    //string		m_strSeeAlso="";
    //string    m_strGroupMembers="";
    private string m_strAMGBiography = "";
    private Hashtable m_relatedArtists = new Hashtable();
    private ArrayList m_discographyAlbum = new ArrayList();
    private ArrayList m_discographyCompilations = new ArrayList();
    private ArrayList m_discographySingles = new ArrayList();
    private ArrayList m_discographyMisc = new ArrayList();
    private Hashtable m_appearsOn = new Hashtable();
    private Hashtable m_songsAppearOn = new Hashtable();
    private ArrayList m_songHighlights = new ArrayList();
    private bool m_bLoaded = false;

    private string m_albums = "";
    private string m_compilations = "";
    private string m_singles = "";
    private string m_misc = "";

    public MusicArtistInfo() {}

    public bool isLoaded()
    {
      return m_bLoaded;
    }

    public string Artist
    {
      get { return m_strArtistName; }
      set { m_strArtistName = value.Trim(); }
    }

    public string ImageURL
    {
      get { return m_strArtistPictureURL; }
      set { m_strArtistPictureURL = value.Trim(); }
    }

    public string Aka
    {
      get { return m_strAKA; }
      set { m_strAKA = value.Trim(); }
    }

    public string Born
    {
      get { return m_strBorn; }
      set { m_strBorn = value.Trim(); }
    }

    public string YearsActive
    {
      get { return m_strYearsActive; }
      set { m_strYearsActive = value.Trim(); }
    }

    public string Genres
    {
      get { return m_strGenres; }
      set { m_strGenres = value.Trim(); }
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

    public string Instruments
    {
      get { return m_strInstruments; }
      set { m_strInstruments = value.Trim(); }
    }

    public string AMGBiography
    {
      get { return m_strAMGBiography; }
      set { m_strAMGBiography = value.Trim(); }
    }

    public ArrayList DiscographyAlbums
    {
      get { return m_discographyAlbum; }
      set { m_discographyAlbum = value; }
    }

    public ArrayList DiscographyCompilations
    {
      get { return m_discographyCompilations; }
      set { m_discographyCompilations = value; }
    }

    public ArrayList DiscographySingles
    {
      get { return m_discographySingles; }
      set { m_discographySingles = value; }
    }

    public ArrayList DiscographyMisc
    {
      get { return m_discographyMisc; }
      set { m_discographyMisc = value; }
    }

    public string Albums
    {
      get
      {
        if (m_albums != null && m_albums.Length > 0)
        {
          return m_albums;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographyAlbums;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        m_albums = strLine.ToString();
        return m_albums;
      }
      set { m_albums = value; }
    }

    public string Compilations
    {
      get
      {
        if (m_compilations != null && m_compilations.Length > 0)
        {
          return m_compilations;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographyCompilations;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        m_compilations = strLine.ToString();
        return m_compilations;
      }
      set { m_compilations = value; }
    }

    public string Singles
    {
      get
      {
        if (m_singles != null && m_singles.Length > 0)
        {
          return m_singles;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographySingles;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        m_singles = strLine.ToString();
        return m_singles;
      }
      set { m_singles = value; }
    }

    public string Misc
    {
      get
      {
        if (m_misc != null && m_misc.Length > 0)
        {
          return m_misc;
        }

        StringBuilder strLine = new StringBuilder(2048);
        string strTmp = null;
        ArrayList list = null;
        list = DiscographyMisc;
        for (int i = 0; i < list.Count; ++i)
        {
          string[] listInfo = (string[])list[i];
          strTmp = String.Format("{0} - {1} ({2})\n",
                                 listInfo[0], // year 
                                 listInfo[1], // title
                                 listInfo[2]); // label
          strLine.Append(strTmp);
        }
        ;
        strLine.Append('\n');
        m_misc = strLine.ToString();
        return m_misc;
      }
      set { m_misc = value; }
    }

    public bool Parse(string strHTML)
    {
      Match match = null;
      Regex regex = null;

      // get the artist name
      string begStr = "<span class=\"title\">";
      string endStr = "</span>";
      int begIndex = strHTML.IndexOf(begStr);

      if (begIndex == -1)
      {
        return false;
      }

      int endIndex = strHTML.IndexOf(endStr, begIndex);

      m_strArtistName = strHTML.Substring(begIndex + begStr.Length, endIndex - (begIndex + begStr.Length));

      string strHTMLLow = strHTML;
      strHTMLLow = strHTMLLow.ToLower();

      HTMLUtil util = new HTMLUtil();
      HTMLTable table = new HTMLTable();
      int iStartOfTable = 0;
      string strTable = null;

      // Born
      begIndex = strHTMLLow.IndexOf("<span>born</span>");
      if (begIndex != -1)
      {
        iStartOfTable = strHTMLLow.LastIndexOf("<table", begIndex);
        // look for the table that is holding the artist name to get more data
        if (iStartOfTable != -1)
        {
          string data;
          strTable = strHTML.Substring(iStartOfTable);
          table.Parse(strTable);
          HTMLTable.HTMLRow row = table.GetRow(2);

          string strValue = row.GetColumValue(0);
          util.RemoveTags(ref strValue);
          util.ConvertHTMLToAnsi(strValue, out data);
          m_strBorn = data.Trim().Replace("  ", ", ");
        }
      }

      // Years Active
      {
        StringBuilder buff = new StringBuilder();
        match = null;
        regex = new Regex(@"<div\sclass=""timeline-sub-active"">(?<year>[\w:]*)</div>",
                          RegexOptions.IgnoreCase
                          | RegexOptions.Multiline
                          | RegexOptions.IgnorePatternWhitespace
                          | RegexOptions.Compiled
          );
        for (match = regex.Match(strHTML); match.Success; match = match.NextMatch())
        {
          string year = match.Groups["year"].ToString();
          buff.Append(", ");
          buff.Append(year);
          buff.Append('s');
        }
        if (buff.Length > 0)
        {
          m_strYearsActive = buff.ToString(1, buff.Length - 1);
        }
      }

      // Genre
      begIndex = strHTMLLow.IndexOf("<!--begin genre listing-->");
      endIndex = strHTMLLow.IndexOf("<!--genre listing-->", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
        StringBuilder buff = new StringBuilder();
        match = null;
        regex = new Regex(@"<li>(?<data>.*?)</li>",
                          RegexOptions.IgnoreCase
                          | RegexOptions.Multiline
                          | RegexOptions.IgnorePatternWhitespace
                          | RegexOptions.Compiled
          );
        for (match = regex.Match(contentInfo); match.Success; match = match.NextMatch())
        {
          string data = match.Groups["data"].ToString();
          buff.Append(", ");
          buff.Append(data);
        }
        if (buff.Length > 0)
        {
          string data;
          string strValue = buff.ToString(1, buff.Length - 1);
          util.RemoveTags(ref strValue);
          util.ConvertHTMLToAnsi(strValue, out data);
          m_strGenres = data.Trim();
        }
      }

      // Style
      begIndex = strHTMLLow.IndexOf("<!--style listing-->");
      endIndex = strHTMLLow.IndexOf("<!--style listing-->", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
        StringBuilder buff = new StringBuilder();
        match = null;
        regex = new Regex(@"<li>(?<data>.*?)</li>",
                          RegexOptions.IgnoreCase
                          | RegexOptions.Multiline
                          | RegexOptions.IgnorePatternWhitespace
                          | RegexOptions.Compiled
          );
        for (match = regex.Match(contentInfo); match.Success; match = match.NextMatch())
        {
          string data = match.Groups["data"].ToString();
          buff.Append(", ");
          buff.Append(data);
        }
        if (buff.Length > 0)
        {
          string data;
          string strValue = buff.ToString(1, buff.Length - 1);
          util.RemoveTags(ref strValue);
          util.ConvertHTMLToAnsi(strValue, out data);
          m_strStyles = data.Trim();
        }
      }

      // Mood
      begIndex = strHTMLLow.IndexOf("<!--begin moods listing-->");
      endIndex = strHTMLLow.IndexOf("<!--moods listing-->", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
        StringBuilder buff = new StringBuilder();
        match = null;
        regex = new Regex(@"<li>(?<data>.*?)</li>",
                          RegexOptions.IgnoreCase
                          | RegexOptions.Multiline
                          | RegexOptions.IgnorePatternWhitespace
                          | RegexOptions.Compiled
          );
        for (match = regex.Match(contentInfo); match.Success; match = match.NextMatch())
        {
          string data = match.Groups["data"].ToString();
          buff.Append(", ");
          buff.Append(data);
        }
        if (buff.Length > 0)
        {
          string data;
          string strValue = buff.ToString(1, buff.Length - 1);
          util.RemoveTags(ref strValue);
          util.ConvertHTMLToAnsi(strValue, out data);
          m_strTones = data.Trim();
        }
      }

      // Instruments
      begIndex = strHTMLLow.IndexOf("<!--instruments listing-->");
      endIndex = strHTMLLow.IndexOf("<!--instruments listing-->", begIndex + 2);
      if (begIndex != -1 && endIndex != -1)
      {
        string contentInfo = strHTML.Substring(begIndex, endIndex - begIndex);
        StringBuilder buff = new StringBuilder();
        match = null;
        regex = new Regex(@"<li>(?<data>.*?)</li>",
                          RegexOptions.IgnoreCase
                          | RegexOptions.Multiline
                          | RegexOptions.IgnorePatternWhitespace
                          | RegexOptions.Compiled
          );
        for (match = regex.Match(contentInfo); match.Success; match = match.NextMatch())
        {
          string data = match.Groups["data"].ToString();
          buff.Append(", ");
          buff.Append(data);
        }
        if (buff.Length > 0)
        {
          string data;
          string strValue = buff.ToString(1, buff.Length - 1);
          util.RemoveTags(ref strValue);
          util.ConvertHTMLToAnsi(strValue, out data);
          m_strInstruments = data.Trim();
        }
      }


      // parse AMG BIOGRAPHY
      begIndex = -1;
      endIndex = strHTMLLow.IndexOf("read more...");
      if (endIndex != -1)
      {
        begIndex = strHTMLLow.LastIndexOf("<a href=", endIndex);
        if (begIndex != -1)
        {
          begIndex += 9;
        }
        endIndex = strHTMLLow.LastIndexOf("\">", endIndex);
      }
      if (begIndex != -1 && endIndex != -1)
      {
        try
        {
          string url = AllmusicSiteScraper.MAINURL + strHTML.Substring(begIndex, endIndex - begIndex);
          string httpcontent = AllmusicSiteScraper.GetHTTP(url);

          match = null;
          regex = new Regex(
            @"class=""title.*?>Biography.*?<p>(?<bio>.*?)</p>",
            RegexOptions.IgnoreCase
            | RegexOptions.Multiline
            | RegexOptions.Singleline
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
          match = regex.Match(httpcontent);
          if (match.Success)
          {
            string data = match.Groups["bio"].ToString();
            util.RemoveTags(ref data);
            util.ConvertHTMLToAnsi(data, out data);
            m_strAMGBiography = data.Trim();
          }
        }
        catch {}
      }

      // picture URL
      begIndex = -1;
      endIndex = strHTMLLow.IndexOf("<!--begin page photo-->");
      if (endIndex != -1)
      {
        begIndex = strHTMLLow.IndexOf("<img src=", endIndex);
        if (begIndex != -1)
        {
          begIndex += 10;
        }
        endIndex = strHTMLLow.IndexOf("\"", begIndex + 2);
      }
      if (begIndex != -1 && endIndex != -1)
      {
        m_strArtistPictureURL = strHTML.Substring(begIndex, endIndex - begIndex);
      }

      // Related Artists
      /*
      iStartOfTable=strHTMLLow.IndexOf("hrelart1",0);
      if (iStartOfTable > 0)
      {
          iStartOfTable=strHTMLLow.LastIndexOf("<table", iStartOfTable);
          strTable=strHTML.Substring(iStartOfTable);
          table.Parse(strTable);
          for (int iRow=0; iRow < table.Rows; iRow++)
          {
              HTMLTable.HTMLRow row=table.GetRow(iRow);
              string strColum1=row.GetColumValue(0);
              if(strColum1.IndexOf("Similar Artists:") != -1)
              {
                  strColum1 = strColum1.Replace("Similar Artists:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Similar Artist", strColum1.Substring(0, strColum1.Length - 1));
              }
              else if(strColum1.IndexOf("Roots and Influences:") != -1)
              {
                  strColum1 = strColum1.Replace("Roots and Influences:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Roots and Influences", strColum1.Substring(0, strColum1.Length - 1));
              }
              else if(strColum1.IndexOf("Followers:") != -1)
              {
                  strColum1 = strColum1.Replace("Followers:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Followers", strColum1.Substring(0, strColum1.Length - 1));
              }
              else if(strColum1.IndexOf("Formal Connections:") != -1)
              {
                  strColum1 = strColum1.Replace("Formal Connections:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Formal Connections", strColum1.Substring(0, strColum1.Length - 1));
              }                    
              else if(strColum1.IndexOf("Performed Songs By:") != -1)
              {
                  strColum1 = strColum1.Replace("Performed Songs By:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Performed Songs By", strColum1.Substring(0, strColum1.Length - 1));
              }
              else if(strColum1.IndexOf("Worked With:") != -1)
              {
                  strColum1 = strColum1.Replace("Worked With:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Worked With", strColum1.Substring(0, strColum1.Length - 1));
              }
              else if(strColum1.IndexOf("Member Of:") != -1)
              {
                  strColum1 = strColum1.Replace("Member Of:","");
                  strColum1 = strColum1.Replace("</a>",",");
                  util.RemoveTags(ref strColum1);
                  util.ConvertHTMLToAnsi(strColum1, out strColum1);
                  //System.Console.Out.WriteLine("strColumn = {0}", strColum1);
                  strColum1 = strColum1.Trim();
                  m_relatedArtists.Add("Member Of", strColum1.Substring(0, strColum1.Length - 1));
              }
          }
      }
      */

      // discography (albums)
      string discographyPageContent = null;
      string discographyPageContentLower = null;
      begIndex = -1;
      endIndex = strHTMLLow.IndexOf(">discography</a>");
      if (endIndex != -1)
      {
        begIndex = strHTMLLow.LastIndexOf("<a href=", endIndex);
        if (begIndex != -1)
        {
          begIndex += 9;
        }
        endIndex = strHTMLLow.LastIndexOf("\">", endIndex);
      }
      if (begIndex != -1 && endIndex != -1)
      {
        try
        {
          string url = AllmusicSiteScraper.MAINURL + strHTML.Substring(begIndex, endIndex - begIndex);
          string httpcontent = AllmusicSiteScraper.GetHTTP(url);
          discographyPageContent = httpcontent;
          discographyPageContentLower = httpcontent.ToLower();

          begIndex = httpcontent.IndexOf("<!--Begin Album List-->");
          if (begIndex != -1)
          {
            string contentInfo = httpcontent.Substring(begIndex);
            StringBuilder buff = new StringBuilder();
            match = null;
            regex = new Regex(
              @"""sorted-cell"">(?<year>.*?)</td>.*?sql=10:.*?"">(?<title>.*" +
              @"?)</a>.*?class=""cell"".*?>(?<label>.*?)</td>",
              RegexOptions.IgnoreCase
              | RegexOptions.Multiline
              | RegexOptions.IgnorePatternWhitespace
              | RegexOptions.Compiled
              );
            for (match = regex.Match(contentInfo); match.Success; match = match.NextMatch())
            {
              string year = match.Groups["year"].ToString();
              string albumTitle = match.Groups["title"].ToString();
              string label = match.Groups["label"].ToString();

              util.RemoveTags(ref year);
              util.ConvertHTMLToAnsi(year, out year);
              util.RemoveTags(ref albumTitle);
              util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
              util.RemoveTags(ref label);
              util.ConvertHTMLToAnsi(label, out label);
              try
              {
                string[] dAlbumInfo = {year.Trim(), albumTitle.Trim(), label.Trim()};
                m_discographyAlbum.Add(dAlbumInfo);
              }
              catch {}
            }
          }
        }
        catch {}
      }

      // discography (compilations, boxes[x])
      if (discographyPageContent != null && discographyPageContent.Length > 0)
      {
        string[] albumTypes = new string[]
                                {
                                  ">compilations</a>", ">singles & eps</a>", ">dvds & videos</a>",
                                  ">other</a>"
                                };
        foreach (string albumtype in albumTypes)
        {
          begIndex = -1;
          endIndex = discographyPageContentLower.IndexOf(albumtype);
          if (endIndex != -1)
          {
            begIndex = discographyPageContentLower.LastIndexOf("<a href=", endIndex);
            if (begIndex != -1)
            {
              begIndex += 9;
            }
            endIndex = discographyPageContentLower.LastIndexOf("\">", endIndex);
          }
          if (begIndex != -1 && endIndex != -1)
          {
            try
            {
              string href = discographyPageContent.Substring(begIndex, endIndex - begIndex);
              if (href.IndexOf(">") == -1) // just in case there is no compilations
              {
                string url = AllmusicSiteScraper.MAINURL + href;
                string httpcontent = AllmusicSiteScraper.GetHTTP(url);

                begIndex = httpcontent.IndexOf("<!--Begin Album List-->");
                if (begIndex != -1)
                {
                  string contentInfo = httpcontent.Substring(begIndex);
                  match = null;
                  regex = new Regex(
                    @"""sorted-cell"">(?<year>.*?)</td>.*?sql=10:.*?"">(?<title>.*" +
                    @"?)</a>.*?class=""cell"".*?>(?<label>.*?)</td>",
                    RegexOptions.IgnoreCase
                    | RegexOptions.Multiline
                    | RegexOptions.IgnorePatternWhitespace
                    | RegexOptions.Compiled
                    );
                  for (match = regex.Match(contentInfo); match.Success; match = match.NextMatch())
                  {
                    string year = match.Groups["year"].ToString();
                    string albumTitle = match.Groups["title"].ToString();
                    string label = match.Groups["label"].ToString();

                    util.RemoveTags(ref year);
                    util.ConvertHTMLToAnsi(year, out year);
                    util.RemoveTags(ref albumTitle);
                    util.ConvertHTMLToAnsi(albumTitle, out albumTitle);
                    util.RemoveTags(ref label);
                    util.ConvertHTMLToAnsi(label, out label);
                    try
                    {
                      string[] dAlbumInfo = {year.Trim(), albumTitle.Trim(), label.Trim()};
                      if (albumtype.StartsWith(">compilations"))
                      {
                        m_discographyCompilations.Add(dAlbumInfo);
                      }
                      else if (albumtype.StartsWith(">singles"))
                      {
                        m_discographySingles.Add(dAlbumInfo);
                      }
                      else if (albumtype.StartsWith(">dvds") || albumtype.StartsWith(">other"))
                      {
                        m_discographyMisc.Add(dAlbumInfo);
                      }
                    }
                    catch {}
                  } // end of for loop 
                } // end of if(begIndex != -1)
              } // end of if(href.IndexOf(">") == -1)
            } // end of try
            catch {}
          } // end of if(begIndex != -1 && endIndex != -1)         
        } // end foreach
      } // end of if (discographyPageContent != null && discographyPageContent.Length > 0)

      /*
      // Appears On
      iStartOfTable=strHTMLLow.IndexOf("/i/hguest",0);
      if (iStartOfTable > 0)
      {
          iStartOfTable=strHTMLLow.LastIndexOf("<table",iStartOfTable);
          strTable=strHTML.Substring(iStartOfTable);
          table.Parse(strTable);
          for (int iRow=0; iRow < table.Rows; iRow++)
          {
              HTMLTable.HTMLRow row=table.GetRow(iRow);
              int iColums=row.Columns;
              if (iColums>1)
              {
                  string artist=row.GetColumValue(0);
                  string title=row.GetColumValue(1);
                  string type=row.GetColumValue(2);
                  util.RemoveTags(ref artist);
                  util.ConvertHTMLToAnsi(artist, out artist);
                  util.RemoveTags(ref title);
                  util.ConvertHTMLToAnsi(title, out title);
                  util.RemoveTags(ref type);
                  util.ConvertHTMLToAnsi(type, out type);
                  try
                  {
                      m_songsAppearOn.Add(title.Trim(), artist.Trim());
                  }
                  catch{}
                  //System.Console.Out.WriteLine("artist = {0}, title = {1}", artist, title);
              }
          }
      }

      // Songs Appear On
      iStartOfTable=strHTMLLow.IndexOf("/i/hsongs",0);
      if (iStartOfTable > 0)
      {
          iStartOfTable=strHTMLLow.LastIndexOf("<table",iStartOfTable);
          strTable=strHTML.Substring(iStartOfTable);
          table.Parse(strTable);
          for (int iRow=0; iRow < table.Rows; iRow++)
          {
              HTMLTable.HTMLRow row=table.GetRow(iRow);
              int iColums=row.Columns;
              if (iColums>1)
              {
                  string artist=row.GetColumValue(0);
                  string title=row.GetColumValue(1);
                  util.RemoveTags(ref artist);
                  util.ConvertHTMLToAnsi(artist, out artist);
                  util.RemoveTags(ref title);
                  util.ConvertHTMLToAnsi(title, out title);
                  try
                  {
                      m_songsAppearOn.Add(title.Trim(), artist.Trim());
                  }
                  catch{}
                  //System.Console.Out.WriteLine("artist = {0}, title = {1}", artist, title);
              }
          }
      }

      // Song Hightligts
      iStartOfTable=strHTMLLow.IndexOf("/i/htopsong",0);
      if (iStartOfTable > 0)
      {
          iStartOfTable=strHTMLLow.LastIndexOf("<table",iStartOfTable);
          strTable=strHTML.Substring(iStartOfTable);
          table.Parse(strTable);
          for (int iRow=0; iRow < table.Rows; iRow++)
          {
              HTMLTable.HTMLRow row=table.GetRow(iRow);
              int iColums=row.Columns;
              if (iColums>1)
              {
                  string artist=row.GetColumValue(0);
                  string title=row.GetColumValue(1);
                  util.RemoveTags(ref artist);
                  util.ConvertHTMLToAnsi(artist, out artist);
                  util.RemoveTags(ref title);
                  util.ConvertHTMLToAnsi(title, out title);
                  try
                  {
                      m_songHighlights.Add(title.Trim());
                  }
                  catch{}
                  //System.Console.Out.WriteLine("artist = {0}, title = {1}", artist, title);
              }
          }
      }
      */

      m_bLoaded = true;
      return m_bLoaded;
    }

    public void Set(ArtistInfo artist)
    {
      Artist = artist.Artist;
      Born = artist.Born;
      YearsActive = artist.YearsActive;
      Genres = artist.Genres;
      Tones = artist.Tones;
      Styles = artist.Styles;
      Instruments = artist.Instruments;
      AMGBiography = artist.AMGBio;
      ImageURL = artist.Image;
      Albums = artist.Albums;
      Compilations = artist.Compilations;
      Singles = artist.Singles;
      Misc = artist.Misc;

      m_bLoaded = true;
    }

    public ArtistInfo Get()
    {
      ArtistInfo artist = new ArtistInfo();
      if (m_bLoaded)
      {
        artist.Artist = Artist;
        artist.Born = Born;
        artist.YearsActive = YearsActive;
        artist.Genres = Genres;
        artist.Tones = Tones;
        artist.Styles = Styles;
        artist.Instruments = Instruments;
        artist.AMGBio = AMGBiography;
        artist.Image = ImageURL;
        artist.Albums = Albums;
        artist.Compilations = Compilations;
        artist.Singles = Singles;
        artist.Misc = Misc;
      }
      return artist;
    }
  }
}