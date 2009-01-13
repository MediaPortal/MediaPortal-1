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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

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
    private int current = 0;
    private ArrayList linkArray = new ArrayList();
    private ArrayList imageArray = new ArrayList();
    private ArrayList imagedescArray = new ArrayList();

    #endregion

    #region constructors

    /// <summary>This constructor creates a new WikipediaArticle</summary>
    /// <summary>Searchterm and language need to be given</summary>
    /// <param name="title">The article's title</param>
    /// <param name="language">Language of the Wikipedia page</param>
    public WikipediaArticle(string title, string language)
    {
      SetLanguage(language);
      this.title = title;
      GetWikipediaXML();
      ParseWikipediaArticle();
      ParseLinksAndImages();
    }

    /// <summary>This constructor creates a new WikipediaArticle.</summary>
    /// <summary>Only called with a title string, language set to default.</summary>
    /// <param name="title">The article's title</param>
    public WikipediaArticle(string title)
      : this(title, "Default")
    {
    }

    /// <summary>This constructor creates a new WikipediaArticle if no parameter is given.</summary>
    /// <summary>Uses an empty searchterm and the default language.</summary>
    public WikipediaArticle()
      : this(string.Empty, "Default")
    {
    }

    #endregion

    /// <summary>Gets the current MP language from mediaportal.xml and sets the Wikipedia URL accordingly</summary>
    private void SetLanguage(string language)
    {
      if (language == "Default")
      {
        Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
        language = xmlreader.GetValueAsString("skin", "language", "English");
      }

      this.language = language;

      Settings detailxmlreader = new Settings(Config.GetFile(Config.Dir.Config, "wikipedia.xml"));
      this.WikipediaURL = detailxmlreader.GetValueAsString(language, "url",
                                                           "http://en.wikipedia.org/wiki/Special:Export/");
      this.imagePattern = detailxmlreader.GetValueAsString(language, "imagepattern", "Image");
      Log.Info("Wikipedia: Language set to " + language + ".");
    }

    /// <summary>Returns the parsed article text.</summary>
    /// <returns>String: parsed article</returns>
    public string GetArticleText()
    {
      return parsedArticle;
    }

    /// <summary>Returns the title of the article. Can differ from the passed parameter on redirects for example.</summary>
    /// <returns>String: title of the article</returns>
    public string GetTitle()
    {
      return title;
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
      Uri url = new Uri(WikipediaURL + this.title);
      Log.Info("Wikipedia: Trying to get following URL: {0}", url.ToString());

      // Here we get the content from the web and put it to a string
      try
      {
        WebClient client = new WebClient();
        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        Stream data = client.OpenRead(url);
        StreamReader reader = new StreamReader(data);
        wikipediaXML = reader.ReadToEnd();
        reader.Close();
        Log.Info("Wikipedia: Success! Downloaded all data.");
      }
      catch (Exception e)
      {
        Log.Info("Wikipedia: Exception during downloading:");
        Log.Info(e.ToString());
      }

      if (wikipediaXML.IndexOf("<text xml:space=\"preserve\">") > 0)
      {
        Log.Info("Wikipedia: Extracting unparsed string.");
        int iStart = 0;
        int iEnd = wikipediaXML.Length;
        // Start of the Entry
        iStart = wikipediaXML.IndexOf("<text xml:space=\"preserve\">") + 27;
        // End of the Entry
        iEnd = wikipediaXML.IndexOf("</text>");
        // Extract the Text and update the var
        this.unparsedArticle = wikipediaXML.Substring(iStart, iEnd - iStart);
        Log.Info("Wikipedia: Unparsed string extracted.");
      }
      else
      {
        if (this.title == this.title.ToLower())
        {
          this.unparsedArticle = string.Empty;
        }
        else
        {
          this.title = this.title.ToLower();
          GetWikipediaXML();
        }
      }
    }

    /// <summary>Cuts all special wiki syntax from the article to display plain text</summary>
    private void ParseWikipediaArticle()
    {
      string tempParsedArticle = this.unparsedArticle;

      // Check if the article is empty, if so do not parse.
      if (tempParsedArticle == string.Empty)
      {
        Log.Info("Wikipedia: Empty article found. Try another Searchterm.");
        this.unparsedArticle = string.Empty;
      }
        // Here we check if there is only a redirect as article to handle it as a special article type
      else if (tempParsedArticle.IndexOf("#REDIRECT") == 0)
      {
        Log.Info("Wikipedia: #REDIRECT found.");
        int iStart = tempParsedArticle.IndexOf("[[") + 2;
        int iEnd = tempParsedArticle.IndexOf("]]", iStart);
        // Extract the Text
        string keyword = tempParsedArticle.Substring(iStart, iEnd - iStart);
        this.unparsedArticle = string.Empty;
        this.title = keyword;
        GetWikipediaXML();
        ParseWikipediaArticle();
      }
        // Finally a well-formed article ;-)
      else
      {
        Log.Info("Wikipedia: Starting parsing.");
        StringBuilder builder = new StringBuilder(tempParsedArticle);
        int iStart = 0;
        int iEnd = 0;

        // Remove HTML comments
        Log.Debug("Wikipedia: Remove HTML comments.");
        while (tempParsedArticle.IndexOf("&lt;!--") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("&lt;!--");
          iEnd = tempParsedArticle.IndexOf("--&gt;", iStart) + 6;

          try
          {
            builder.Remove(iStart, iEnd - iStart);
          }
          catch (Exception e)
          {
            Log.Error(e.ToString());
            Log.Error(builder.ToString());
          }
          tempParsedArticle = builder.ToString();
        }

        // surrounded by {| and |} is (atm) unusable stuff.
        Log.Debug("Wikipedia: Remove unusable stuff 2.");
        while (current < tempParsedArticle.Length && tempParsedArticle.IndexOf("{|", current) >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("{|", current);
          iEnd = tempParsedArticle.IndexOf("|}", current) + 2;

          try
          {
            builder.Remove(iStart, iEnd - iStart);
          }
          catch (Exception e)
          {
            Log.Error(e.ToString());
            Log.Error(builder.ToString());
            current = iStart + 2;
            break;
          }

          tempParsedArticle = builder.ToString();
          current = iStart + 2;
        }
        current = 0;

        // surrounded by {{ and }} is (atm) unusable stuff.
        Log.Debug("Wikipedia: Remove unusable stuff 1.");
        while (current < tempParsedArticle.Length && tempParsedArticle.IndexOf("{{") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("{{");
          int tempposition = iStart;
          iEnd = tempParsedArticle.IndexOf("}}", iStart) + 2;

          // Find inner sets of "{{"
          while (tempParsedArticle.IndexOf("{{", tempposition + 2) >= 0 &&
                 tempParsedArticle.IndexOf("{{", tempposition + 2) < iEnd)
          {
            tempposition = tempParsedArticle.IndexOf("{{", tempposition + 2);
            iEnd = tempParsedArticle.IndexOf("}}", iEnd) + 2;
          }

          try
          {
            builder.Remove(iStart, iEnd - iStart);
            tempParsedArticle = builder.ToString();
          }
          catch (Exception e)
          {
            Log.Error(e.ToString());
            Log.Error(builder.ToString());
            break;
          }
        }

        // Remove audio links.
        Log.Debug("Wikipedia: Remove audio links.");
        while (tempParsedArticle.IndexOf("&lt;span") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("&lt;span");
          iEnd = tempParsedArticle.IndexOf("&lt;/span&gt;") + 13;

          try
          {
            builder.Remove(iStart, iEnd - iStart);
          }
          catch (Exception e)
          {
            Log.Error(e.ToString());
            Log.Error(builder.ToString());
          }

          tempParsedArticle = builder.ToString();
        }

        // Remove web references.
        Log.Debug("Wikipedia: Remove web references.");
        while (tempParsedArticle.IndexOf("&lt;ref&gt;") >= 0)
        {
          builder = new StringBuilder(tempParsedArticle);
          iStart = tempParsedArticle.IndexOf("&lt;ref");
          iEnd = tempParsedArticle.IndexOf("&lt;/ref&gt;") + 12;

          try
          {
            builder.Remove(iStart, iEnd - iStart);
          }
          catch (Exception e)
          {
            Log.Error(e.ToString());
            Log.Error(builder.ToString());
          }

          tempParsedArticle = builder.ToString();
        }

        // Remove <br />
        Log.Debug("Wikipedia: Remove <br />.");
        builder.Replace("&lt;br /&gt;", "\n");
        builder.Replace("&lt;br style=&quot;clear:both&quot;/&gt;", "\n");
        builder.Replace("&lt;br style=&quot;clear:left&quot;/&gt;", "\n");
        builder.Replace("&lt;br style=&quot;clear:right&quot;/&gt;", "\n");

        // Remove <sup>
        Log.Debug("Wikipedia: Remove <sup>.");
        builder.Replace("&lt;sup&gt;", "^");
        builder.Replace("&lt;/sup&gt;", "");

        // surrounded by ''' and ''' is bold text, atm also unusable.
        Log.Debug("Wikipedia: Remove \'\'\'.");
        builder.Replace("'''", "");

        // surrounded by '' and '' is italic text, atm also unusable.
        Log.Debug("Wikipedia: Remove \'\'.");
        builder.Replace("''", "");

        // surrounded by '' and '' is italic text, atm also unusable.
        Log.Debug("Wikipedia: Remove <small>-tags.");
        builder.Replace("&lt;small&gt;", "");
        builder.Replace("&lt;/small&gt;", "");

        // Display === as newlines (meaning new line for every ===).
        Log.Debug("Wikipedia: Display === as 1 newlines.");
        builder.Replace("===", "\n");

        // Display == as newlines (meaning new line for every ==).
        Log.Debug("Wikipedia: Display == as 1 newline.");
        builder.Replace("==", "\n");

        // Display * as list (meaning new line for every *).
        Log.Debug("Wikipedia: Display * as list.");
        builder.Replace("*", "\n +");

        // Remove HTML whitespace.
        Log.Debug("Wikipedia: Remove HTML whitespace.");
        builder.Replace("&amp;nbsp;", " ");

        // Display &quot; as ".
        Log.Debug("Wikipedia: Remove Quotations.");
        builder.Replace("&quot;", "\"");

        // Display &amp;mdash; as -.
        Log.Debug("Wikipedia: Remove &amp;mdash;.");
        builder.Replace("&amp;mdash;", "-");

        // Display &shy;.
        Log.Debug("Wikipedia: Remove &shy;.");
        builder.Replace("&amp;shy;", "");

        // Remove gallery tags.
        Log.Debug("Wikipedia: Remove gallery tags.");
        builder.Replace("&lt;gallery&gt;", "");
        builder.Replace("&lt;/gallery&gt;", "");

        // Remove gallery tags.
        Log.Debug("Wikipedia: Remove &amp;.");
        builder.Replace("&amp;", "&");

        // Remove (too many) newlines
        Log.Debug("Wikipedia: Remove (too many) newlines.");
        builder.Replace("\\n\\n\\n\\n", "\\n");
        builder.Replace("\\n\\n\\n", "\\n");
        builder.Replace("\\n\\n", "\\n");

        // Remove (too many) newlines
        Log.Debug("Wikipedia: Remove (too many) whitespaces.");
        builder.Replace("    ", " ");
        builder.Replace("   ", " ");
        builder.Replace("  ", " ");

        tempParsedArticle = builder.ToString();

        // The text shouldn't start with a newline.
        if (tempParsedArticle.IndexOf("\n") == 0)
        {
          tempParsedArticle.Remove(0, 2);
        }

        // For Debug purposes it is nice to see how the whole article text is parsed until here
        //Log.Debug(tempParsedArticle);

        Log.Info("Wikipedia: Finished parsing.");
        this.unparsedArticle = tempParsedArticle;
      }
    }

    /// <summary>Gets Links out of the article. External links are thrown away, links to other wikipedia articles get into the link array, images to the image array</summary>
    private void ParseLinksAndImages()
    {
      Log.Info("Wikipedia: Starting parsing of links and images.");
      string tempParsedArticle = this.unparsedArticle;
      int iStart = 0, iEnd = 0, iPipe = 0;

      // Surrounded by [[IMAGEPATTERN: and ]] are the links to IMAGES.
      // We need to check for the localized image keyword but also for the English as this is commonly used in some local sites.
      // Example: [[Bild:H_NeuesRathaus1.jpg|left|thumb|Das [[Neues Rathaus (Hannover)|Neue Rathaus]] mit Maschteich]]
      while ((iStart = tempParsedArticle.IndexOf("[[" + imagePattern + ":", iStart)) >= 0 ||
             (iStart = tempParsedArticle.IndexOf("[[Image:")) >= 0)
      {
        iEnd = tempParsedArticle.IndexOf("]]", (iStart + 2)) + 2;
        int disturbingLink = iStart;

        // Descriptions of images can contain links!
        // [[Bild:Hannover Merian.png|thumb|[[Matthäus Merian|Merian]]-Kupferstich um 1650, im Vordergrund Windmühle auf dem [[Lindener Berg]]]]
        while (tempParsedArticle.IndexOf("[[", disturbingLink + 2) >= 0 &&
               tempParsedArticle.IndexOf("[[", disturbingLink + 2) < iEnd)
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
          {
            linkkeyword = linkkeyword.Substring(linkkeyword.IndexOf("|") + 1,
                                                linkkeyword.IndexOf("]]") - linkkeyword.IndexOf("|") - 1);
          }
          else
          {
            linkkeyword = linkkeyword.Substring(linkkeyword.IndexOf("[[") + 2,
                                                linkkeyword.IndexOf("]]") - linkkeyword.IndexOf("[[") - 2);
          }

          keyword = keyword.Substring(0, iStartlink) + linkkeyword +
                    keyword.Substring(iEndlink, keyword.Length - iEndlink);
        }

        int iStartname = keyword.IndexOf(":") + 1;
        int iEndname = keyword.IndexOf("|");
        string imagename = keyword.Substring(iStartname, iEndname - iStartname);

        //Image names must not contain spaces!
        imagename = imagename.Replace(" ", "_");

        int iStartdesc = keyword.LastIndexOf("|") + 1;
        int iEnddesc = keyword.LastIndexOf("]]");
        string imagedesc = keyword.Substring(iStartdesc, iEnddesc - iStartdesc);
        ;

        this.imageArray.Add(imagename);
        this.imagedescArray.Add(imagedesc);
        Log.Debug("Wikipedia: Image added: {0}, {1}", imagedesc, imagename);

        tempParsedArticle = tempParsedArticle.Substring(0, iStart) +
                            tempParsedArticle.Substring(iEnd, tempParsedArticle.Length - iEnd);
      }

      // surrounded by [[ and ]] are the links to other articles.
      Log.Debug("Wikipedia: Starting Link parsing.");
      string parsedKeyword, parsedLink;
      iStart = iEnd = 0;
      try
      {
        while ((iStart = tempParsedArticle.IndexOf("[[", iStart)) >= 0)
        {
          iEnd = tempParsedArticle.IndexOf("]]") + 2;
          // Extract the Text
          string keyword = tempParsedArticle.Substring(iStart, iEnd - iStart);

          // Parse Links to other keywords.
          // 1st type of keywords is like [[article|displaytext]]	
          if ((iPipe = keyword.IndexOf("|")) > 0)
          {
            parsedKeyword = keyword.Substring(iPipe + 1, keyword.Length - iPipe - 3);
            parsedLink = keyword.Substring(2, iPipe - 2);
            if (!this.linkArray.Contains(parsedLink))
            {
              this.linkArray.Add(parsedLink);
              //Log.Debug("Wikipedia: Link added: {0}, {1}", parsedLink, parsedKeyword);
            }
          }
          else if (keyword.IndexOf(":") > 0)
          {
            // for the 2nd a ":" is a link to the article in another language.
            // TODO Add links to other languages!!!
            parsedKeyword = string.Empty;
          }
          else
          {
            // for the 3rd the article and displayed text are equal [[article]].
            parsedKeyword = keyword.Substring(2, keyword.Length - 4);
            if (!this.linkArray.Contains(parsedKeyword))
            {
              this.linkArray.Add(parsedKeyword);
              //Log.Debug("Wikipedia: Link added: {0}", parsedKeyword);
            }
          }

          StringBuilder builder = new StringBuilder(tempParsedArticle);
          builder.Remove(iStart, iEnd - iStart);
          builder.Insert(iStart, parsedKeyword);
          tempParsedArticle = builder.ToString();
        }
      }
      catch (Exception e)
      {
        Log.Error("Wikipedia: {0}", e.ToString());
        Log.Error("Wikipedia: tempArticle: {0}", tempParsedArticle);
      }
      Log.Debug("Wikipedia: Finished Link parsing: {0} Links added.", linkArray.Count);

      // surrounded by [ and ] are external Links. Need to be removed.
      Log.Debug("Wikipedia: Removing external links");
      iStart = -1;
      try
      {
        while ((iStart = tempParsedArticle.IndexOf("[")) >= 0)
        {
          iEnd = tempParsedArticle.IndexOf("]") + 1;

          StringBuilder builder = new StringBuilder(tempParsedArticle);
          builder.Remove(iStart, iEnd - iStart);
          tempParsedArticle = builder.ToString();
        }
      }
      catch (Exception e)
      {
        Log.Error("Wikipedia: {0}", e.ToString());
        Log.Error("Parsing Error: " + tempParsedArticle + "\nSTART: " + iStart + "\nEND: " + iEnd);
      }

      Log.Info("Wikipedia: Finished parsing of links and images.");
      this.parsedArticle = tempParsedArticle;
    }
  }
}