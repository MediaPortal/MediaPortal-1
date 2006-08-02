#region Copyright (C) 2006 Team MediaPortal

/* 
 *      Copyright (C) 2006 Team MediaPortal
 *      http://www.team-mediaportal.com
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
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace Wikipedia
{
  /// <summary>
  /// This class holds all the logic to get info from Wikipedia and parse it for further using in MP.
  /// </summary>
  public class WikipediaArticle
  {
    #region vars
    private string WikipediaURL = "http://en.wikipedia.org/wiki/Special:Export/";
    private string imagePattern = "Image";
    private string title = string.Empty;
    private string unparsedArticle = string.Empty;
    private string parsedArticle = string.Empty;
    private string language = "Default";
    private ArrayList linkArray = new ArrayList();
    private ArrayList imageArray = new ArrayList();
    private ArrayList imagedescArray = new ArrayList();
    private ILog _log;
    #endregion

    #region constructors
    /// <summary>This constructor creates a new WikipediaArticle</summary>
    /// <summary>Searchterm and language need to be given</summary>
    /// <param name="title">The article's title</param>
    /// <param name="language">Language of the Wikipedia page</param>
    public WikipediaArticle(string title, string language)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      SetLanguage(language);
      this.title = title;
      GetWikipediaXML();
      ParseWikipediaArticle();
      parseLinksAndImages();
    }

    /// <summary>This constructor creates a new WikipediaArticle.</summary>
    /// <summary>Only called with a title string, language set to default.</summary>
    /// <param name="title">The article's title</param>
    public WikipediaArticle(string title) : this(title, "Default")
    {
    }

    /// <summary>This constructor creates a new WikipediaArticle if no parameter is given.</summary>
    /// <summary>Uses an empty searchterm and the default language.</summary>
    public WikipediaArticle() : this(string.Empty, "Default")
    {
    }
    #endregion

    /// <summary>Gets the current MP language from mediaportal.xml and sets the Wikipedia URL accordingly</summary>
    private void SetLanguage(string language)
    {
      if (language == "Default")
      {
        MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml");
        language = xmlreader.GetValueAsString("skin", "language", "English");
      }
      this.language = language;
      switch (language)
      {
        case "English":
          this.WikipediaURL = "http://en.wikipedia.org/wiki/Special:Export/";
          this.imagePattern = "Image";
          _log.Info("Wikipedia: Language set to English");
          break;
        case "German":
          this.WikipediaURL = "http://de.wikipedia.org/wiki/Spezial:Export/";
          this.imagePattern = "Bild";
          _log.Info("Wikipedia: Language set to German");
          break;
        case "French":
          this.WikipediaURL = "http://fr.wikipedia.org/wiki/Special:Export/";
          this.imagePattern = "Image";
          _log.Info("Wikipedia: Language set to French");
          break;
        case "Dutch":
          this.WikipediaURL = "http://nl.wikipedia.org/wiki/Speciaal:Export/";
          this.imagePattern = "Afbeelding";
          _log.Info("Wikipedia: Language set to Dutch");
          break;
        case "Norwegian":
          this.WikipediaURL = "http://no.wikipedia.org/wiki/Spesial:Export";
          this.imagePattern = "Bilde";
          _log.Info("Wikipedia: Language set to Norwegian");
          break;
        default:
          this.WikipediaURL = "http://en.wikipedia.org/wiki/Special:Export/";
          this.imagePattern = "Image";
          _log.Info("Wikipedia: Language set to Default (English)");
          break;
      }
    }

    /// <summary>Returns the parsed article text.</summary>
    /// <returns>String: parsed article</returns>
    public string GetArticleText()
    {
        return parsedArticle;
    }

    /// <summary>Returns all names of images.</summary>
    /// <returns>StringArray: images used in this article</returns>
    public ArrayList GetImageArray()
    {
      return imageArray;
    }

    /// <summary>Returns all descriptions of images.</summary>
    /// <returns>StringArray: images used in this article</returns>
    public ArrayList GetImagedescArray()
    {
      return imagedescArray;
    }

    /// <summary>Returns the titles of all linked articles.</summary>
    /// <returns>StringArray: titles of linked (internal) Wikipedia articles</returns>
    public ArrayList GetLinkArray()
    {
      return linkArray;
    }

    /// <summary>Returns the currently active language.</summary>
    /// <returns>String: language</returns>
    public string GetLanguage()
    {
      return language;
    }

    /// <summary>Downloads the xml content from Wikipedia and cuts metadata like version info.</summary>
    private void GetWikipediaXML()
    {
      string wikipediaXML = string.Empty;
      // Build the URL to the Wikipedia page
      System.Uri url = new System.Uri(WikipediaURL + this.title);
      _log.Info("Wikipedia: Trying to get following URL: {0}", url.ToString());

      // Here we get the content from the web and put it to a string
      try
      {
        WebClient client = new WebClient();
        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        Stream data = client.OpenRead(url);
        StreamReader reader = new StreamReader(data);
        wikipediaXML = reader.ReadToEnd();
        reader.Close();
        _log.Info("Wikipedia: Success! Downloaded all data.");
      }
      catch (Exception e)
      {
        _log.Info("Wikipedia: Exception during downloading:");
        _log.Info(e.ToString());
      }

      if (wikipediaXML.IndexOf("<text xml:space=\"preserve\">") > 0)
      {
        _log.Info("Wikipedia: Extracting unparsed string.");
        int iStart = 0;
        int iEnd = wikipediaXML.Length;
        // Start of the Entry
        iStart = wikipediaXML.IndexOf("<text xml:space=\"preserve\">") + 27;
        // End of the Entry
        iEnd = wikipediaXML.IndexOf("</text>");
        // Extract the Text and update the var
        this.unparsedArticle = wikipediaXML.Substring(iStart, iEnd - iStart);
        _log.Info("Wikipedia: Unparsed string extracted.");
      }
      else
        this.unparsedArticle = string.Empty;
    }

    /// <summary>Cuts all special wiki syntax from the article to display plain text</summary>
    private void ParseWikipediaArticle()
    {
      string tempParsedArticle = this.unparsedArticle;

      // Check if the article is empty, if so do not parse.
      if (tempParsedArticle == string.Empty)
      {
        _log.Info("Wikipedia: Empty article found. Try another Searchterm.");
        this.unparsedArticle = string.Empty;
      }
      // Here we check if there is only a redirect as article to handle it as a special article type
      else if (tempParsedArticle.IndexOf("#REDIRECT") >= 0)
      {
        _log.Info("Wikipedia: #REDIRECT found.");
        int iStart = tempParsedArticle.IndexOf("[[") + 2;
        int iEnd = tempParsedArticle.IndexOf("]]", iStart);
        // Extract the Text
        string keyword = tempParsedArticle.Substring(iStart, iEnd - iStart);
        linkArray.Add(keyword);
        this.unparsedArticle = "REDIRECT";
      }
      // Finally a well-formed article ;-)
      else
      {
        _log.Info("Wikipedia: Starting parsing.");
        StringBuilder builder = new StringBuilder(tempParsedArticle);
        int iStart = 0;
        int iEnd = 0;

        // Remove HTML comments
        _log.Debug("Wikipedia: Remove HTML comments.");
        while (tempParsedArticle.IndexOf("&lt;!--") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("&lt;!--");
          iEnd = tempParsedArticle.IndexOf("--&gt;", iStart) + 6;

          builder.Remove(iStart, iEnd - iStart);
          tempParsedArticle = builder.ToString();
        }

        // surrounded by {{ and }} is (atm) unusable stuff.
        //_log.Debug("Wikipedia: Remove stuff between {{ and }}.");
        while (tempParsedArticle.IndexOf("{{") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("{{");
          iEnd = tempParsedArticle.IndexOf("}}") + 2;

          builder.Remove(iStart, iEnd - iStart);
          tempParsedArticle = builder.ToString();
          //tempParsedArticle = tempParsedArticle.Substring(0, iStart) + tempParsedArticle.Substring(iEnd, tempParsedArticle.Length - iEnd);
        }

        // surrounded by {| and |} is (atm) unusable stuff.
        //_log.Debug("Wikipedia: Remove stuff between {| and |}.");
        while (tempParsedArticle.IndexOf("{|") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("{|");
          iEnd = tempParsedArticle.IndexOf("|}") + 2;

          builder.Remove(iStart, iEnd - iStart);
          tempParsedArticle = builder.ToString();
          //tempParsedArticle = tempParsedArticle.Substring(0, iStart) + tempParsedArticle.Substring(iEnd, tempParsedArticle.Length - iEnd);
        }

        // Remove web references.
        _log.Debug("Wikipedia: Remove web references.");
        while (tempParsedArticle.IndexOf("&lt;ref&gt;") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("&lt;ref&gt;");
          iEnd = tempParsedArticle.IndexOf("&lt;/ref&gt;") + 12;

          builder.Remove(iStart, iEnd - iStart);
          tempParsedArticle = builder.ToString();
        }

        // Remove <br />
        _log.Debug("Wikipedia: Remove <br />.");
        builder.Replace("&lt;br /&gt;", "\n");
        builder.Replace("&lt;br style=&quot;clear:both&quot;/&gt;", "\n");
        builder.Replace("&lt;br style=&quot;clear:left&quot;/&gt;", "\n");
        builder.Replace("&lt;br style=&quot;clear:right&quot;/&gt;", "\n");

        // surrounded by ''' and ''' is bold text, atm also unusable.
        _log.Debug("Wikipedia: Remove \'\'\'.");
        builder.Replace("'''", "");

        // surrounded by '' and '' is italic text, atm also unusable.
        _log.Debug("Wikipedia: Remove \'\'.");
        builder.Replace("''", "");

        // Display === as newlines (meaning new line for every ===).
        _log.Debug("Wikipedia: Display === as 2 newlines.");
        builder.Replace("===", "\n");

        // Display == as newlines (meaning new line for every ==).
        _log.Debug("Wikipedia: Display == as 1 newline.");
        builder.Replace("==", "\n");

        // Display * as list (meaning new line for every *).
        _log.Debug("Wikipedia: Display * as list.");
        builder.Replace("*", "\n +");

        // Remove HTML whitespace.
        _log.Debug("Wikipedia: Remove HTML whitespace.");
        builder.Replace("&amp;nbsp;", " ");

        // Display &quot; as ".
        _log.Debug("Wikipedia: Remove Quotations.");
        builder.Replace("&quot;", "\"");

        // Display &amp;mdash; as -.
        _log.Debug("Wikipedia: Remove &amp;mdash;.");
        builder.Replace("&amp;mdash;", "-");
        

        // Remove gallery tags.
        _log.Debug("Wikipedia: Remove gallery tags.");
        builder.Replace("&lt;gallery&gt;", "");
        builder.Replace("&lt;/gallery&gt;", "");

        // Remove gallery tags.
        _log.Debug("Wikipedia: Remove &amp;.");
        builder.Replace("&amp;", "&");

        tempParsedArticle = builder.ToString();
        this.unparsedArticle = tempParsedArticle;
      }
    }

    /// <summary>Gets Links out of the article. External links are thrown away, links to other wikipedia articles get into the link array, images to the image array</summary>
    private void parseLinksAndImages()
    {
      string tempParsedArticle = this.unparsedArticle;

      // surrounded by [[IMAGEPATTERN: and ]] are the links to IMAGES.
      // Example: [[Bild:H_NeuesRathaus1.jpg|left|thumb|Das [[Neues Rathaus (Hannover)|Neue Rathaus]] mit Maschteich]]
      while (tempParsedArticle.IndexOf("[[" + imagePattern + ":") >= 0)
      {
        int iStart = tempParsedArticle.IndexOf("[[" + imagePattern + ":");
        int iEnd = tempParsedArticle.IndexOf("]]", (iStart + 2)) + 2;
        int disturbingLink = iStart;

        // Descriptions of images can contain links!
        // [[Bild:Hannover Merian.png|thumb|[[Matthäus Merian|Merian]]-Kupferstich um 1650, im Vordergrund Windmühle auf dem [[Lindener Berg]]]]
        while (tempParsedArticle.IndexOf("[[", disturbingLink + 2) < iEnd)
        {
          disturbingLink = tempParsedArticle.IndexOf("[[", disturbingLink + 2);
          iEnd = tempParsedArticle.IndexOf("]]", disturbingLink) + 2;
          iEnd = tempParsedArticle.IndexOf("]]", iEnd) + 2;
        }        
        // Extract the Text
        string keyword = tempParsedArticle.Substring(iStart, iEnd - iStart);
        
        //Remove all links from the image description.
        while (keyword.IndexOf("[[", 2) >= 0)
        {
          int iStartlink = keyword.IndexOf("[[", 2);
          int iEndlink = keyword.IndexOf("]]", iStartlink) + 2;
          // Extract the Text
          string linkkeyword = keyword.Substring(iStartlink, iEndlink - iStartlink);

          // Parse Links to other keywords.
          // 1st type of keywords is like [[article|displaytext]]	
          // for the 2nd the article and displayed text are equal [[article]].
          if (linkkeyword.IndexOf("|") > 0)
            linkkeyword = linkkeyword.Substring(linkkeyword.IndexOf("|") + 1, linkkeyword.IndexOf("]]") - linkkeyword.IndexOf("|") - 1);
          else
            linkkeyword = linkkeyword.Substring(linkkeyword.IndexOf("[[") + 2, linkkeyword.IndexOf("]]") - linkkeyword.IndexOf("[[") - 2);
          
          keyword = keyword.Substring(0, iStartlink) + linkkeyword + keyword.Substring(iEndlink, keyword.Length - iEndlink);
        }

        int iStartname = keyword.IndexOf(":") + 1;
        int iEndname = keyword.IndexOf("|");
        string imagename = keyword.Substring(iStartname, iEndname - iStartname);

        //Image names must not contain spaces!
        imagename = imagename.Replace(" ", "_");

        int iStartdesc = keyword.LastIndexOf("|") + 1;
        int iEnddesc = keyword.LastIndexOf("]]");
        string imagedesc = keyword.Substring(iStartdesc, iEnddesc - iStartdesc); ;

        this.imageArray.Add(imagename);
        this.imagedescArray.Add(imagedesc);
        _log.Debug("Wikipedia: Image added: {0}, {1}", imagedesc, imagename);

        tempParsedArticle = tempParsedArticle.Substring(0, iStart) + tempParsedArticle.Substring(iEnd, tempParsedArticle.Length - iEnd);
      }

      // surrounded by [[ and ]] are the links to other articles.
      string parsedKeyword;
      try
      {
        while (tempParsedArticle.IndexOf("[[") >= 0)
        {
          int iStart = tempParsedArticle.IndexOf("[[");
          int iEnd = tempParsedArticle.IndexOf("]]") + 2;
          // Extract the Text
          string keyword = tempParsedArticle.Substring(iStart, iEnd - iStart);

          // Parse Links to other keywords.
          // 1st type of keywords is like [[article|displaytext]]	
          // for the 2nd the article and displayed text are equal [[article]].
          if (keyword.IndexOf("|") > 0)
          {
            parsedKeyword = keyword.Substring(keyword.IndexOf("|") + 1, keyword.IndexOf("]]") - keyword.IndexOf("|") - 1);
            if (!this.linkArray.Contains(parsedKeyword))
            {
              this.linkArray.Add(parsedKeyword);
              _log.Debug("Wikipedia: Link added: {0}", parsedKeyword);
            }
          }
          else if (keyword.IndexOf(":") > 0)
          {
            //TODO Add links to other languages!!!
            parsedKeyword = String.Empty;
          }
          else
          {
            parsedKeyword = keyword.Substring(keyword.IndexOf("[[") + 2, keyword.IndexOf("]]") - keyword.IndexOf("[[") - 2);
            if (!this.linkArray.Contains(parsedKeyword))
            {
              this.linkArray.Add(parsedKeyword);
              _log.Debug("Wikipedia: Link added: {0}", parsedKeyword);
            }
          }

          tempParsedArticle = tempParsedArticle.Substring(0, iStart) + parsedKeyword + tempParsedArticle.Substring(iEnd, tempParsedArticle.Length - iEnd);
        }
      }
      catch (Exception e)
      {
        _log.Error("Wikipedia: {0}", e.ToString());
        _log.Error("Wikipedia: tempArticle: {0}", tempParsedArticle);
      }

      // surrounded by [ and ] are external Links. Need to be removed.
      _log.Debug("Wikipedia: Removing external links");
      while (tempParsedArticle.IndexOf("[") >= 0)
      {
        int iStart = tempParsedArticle.IndexOf("[");
        int iEnd = tempParsedArticle.IndexOf("]") + 1;

        StringBuilder builder = new StringBuilder(tempParsedArticle);
        builder.Remove(iStart, iEnd - iStart);
        tempParsedArticle = builder.ToString();
      }

      this.parsedArticle = tempParsedArticle;
    }
  }
}
