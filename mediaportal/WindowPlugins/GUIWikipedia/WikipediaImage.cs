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
  /// This class holds all the logic to get an image from Wikipedia for further using in MP.
  /// </summary>
  public class WikipediaImage
  {
    #region vars
    private string WikipediaURL = "http://en.wikipedia.org/wiki/Special:Export/";
    private string imagename = string.Empty;
    private string imagedesc = string.Empty;
    private string imageurl = string.Empty;
    private string imagelocal = string.Empty;

    private ILog _log;
    #endregion

    #region constructors
    /// <summary>This constructor creates a new WikipediaImage</summary>
    /// <summary>The name of the image and language need to be given</summary>
    /// <param name="imagename">The internal name of the image like "Bild_478.jpg" in "http://de.wikipedia.org/wiki/Bild:Bild_478.jpg"</param>
    /// <param name="language">Language of the Wikipedia page</param>
    public WikipediaImage(string imagename, string language)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      SetLanguage(language);
      this.imagename = imagename;
      GetImageUrl();
      GetImageFile();
    }

    /// <summary>This constructor creates a new WikipediaArticle.</summary>
    /// <summary>Only called with a title string, language set to default.</summary>
    /// <param name="title">The article's title</param>
    public WikipediaImage(string imagename) : this(imagename, "Default")
    {
    }

    /// <summary>This constructor creates a new WikipediaArticle if no parameter is given.</summary>
    /// <summary>Uses an empty searchterm and the default language.</summary>
    public WikipediaImage() : this(string.Empty, "Default")
    {
    }
    #endregion

    #region class methods
    /// <summary>Gets the current MP language from mediaportal.xml and sets the Wikipedia URL accordingly</summary>
    private void SetLanguage(string language)
    {
      if (language == "Default")
      {
        MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml");
        language = xmlreader.GetValueAsString("skin", "language", "English");
      }
      switch (language)
      {
        case "English":
          this.WikipediaURL = "http://en.wikipedia.org/wiki/Image:";
          _log.Info("Wikipedia: Language set to English");
          break;
        case "German":
          this.WikipediaURL = "http://de.wikipedia.org/wiki/Bild:";
          _log.Info("Wikipedia: Language set to German");
          break;
        case "French":
          this.WikipediaURL = "http://fr.wikipedia.org/wiki/Image:";
          _log.Info("Wikipedia: Language set to French");
          break;
        case "Dutch":
          this.WikipediaURL = "http://nl.wikipedia.org/wiki/Afbeelding:";
          _log.Info("Wikipedia: Language set to Dutch");
          break;
        case "Norwegian":
          this.WikipediaURL = "http://no.wikipedia.org/wiki/Bilde:";
          _log.Info("Wikipedia: Language set to Norwegian");
          break;
        default:
          this.WikipediaURL = "http://en.wikipedia.org/wiki/Image:";
          _log.Info("Wikipedia: Language set to Default (English)");
          break;
      }
    }

    /// <summary>Get the local filename of the downloaded image.</summary>
    /// <returns>String: filename of the downloaded image.</returns>
    public string GetImageFilename()
    {
      string imagelocal = Application.StartupPath + @"\thumbs\wikipedia\" + imagename;
      return imagelocal;
    }

    /// <summary>Getting the link to the full-size image.</summary>
    /// <returns>String: parsed article</returns>
    private void GetImageUrl()
    {
      string imagepage = string.Empty;
      
      // Build the URL to the Image page
      System.Uri url = new System.Uri(WikipediaURL + this.imagename);
      _log.Info("Wikipedia: Trying to get following Image page: {0}", url.ToString());

      // Here we get the content from the web and put it to a string
      try
      {
        WebClient client = new WebClient();
        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        Stream data = client.OpenRead(url);
        StreamReader reader = new StreamReader(data);
        imagepage = reader.ReadToEnd();
        reader.Close();
        _log.Info("Wikipedia: Success! Downloaded all data from the image page.");
      }
      catch (Exception e)
      {
        _log.Info("Wikipedia: Exception during downloading image page:");
        _log.Info(e.ToString());
      }

      //We're searching for something like this:
      //<div class="fullImageLink" id="file"><a href="http://upload.wikimedia.org/wikipedia/commons/7/7d/Bild_478.jpg">
      if (imagepage.IndexOf("class=\"fullImageLink\"") >= 0)
      {
        _log.Info("Wikipedia: Extracting link to full-size image.");
        int iStart = imagepage.IndexOf("class=\"fullImageLink\"");
        imagepage = imagepage.Substring(iStart, 1000);

        iStart = imagepage.IndexOf("href") + 6;
        int iEnd = imagepage.IndexOf("\"", iStart);

        this.imageurl = imagepage.Substring(iStart, iEnd - iStart);
        _log.Info("Wikipedia: URL of full-size image extracted.");
        _log.Info(imageurl);
      }
      else
        this.imageurl = string.Empty;
    }

    /// <summary>Downloads the full-size image from the wikipedia page</summary>
    private void GetImageFile()
    {
      if (imageurl != "")
      {
        //Check if we already have the file.
        string thumbspath = "thumbs/wikipedia/";

        //Create the wikipedia subdir in thumbs when it not exists.
        if (!System.IO.Directory.Exists(thumbspath))
          System.IO.Directory.CreateDirectory(thumbspath);

        if (!System.IO.File.Exists(thumbspath + imagename))
        {

          _log.Info("Wikipedia: Trying to get following URL: {0}", imageurl);
          // Here we get the image from the web and save it to disk
          try
          {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.DownloadFile(imageurl, thumbspath + imagename);
            _log.Info("Wikipedia: Success! Image downloaded.");
          }
          catch (Exception e)
          {
            _log.Info("Wikipedia: Exception during downloading:");
            _log.Info(e.ToString());
          }
        }
        else
        {
          _log.Info("Wikipedia: Image exists, no need to redownload!");
        }
      }
      else
      {
        _log.Info("Wikipedia: No imageurl. Can't download file.");
      }
    }

    #endregion
  }

}
