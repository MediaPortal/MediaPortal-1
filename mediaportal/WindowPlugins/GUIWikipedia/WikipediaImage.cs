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
using System.IO;
using System.Net;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

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

    #endregion

    #region constructors

    /// <summary>This constructor creates a new WikipediaImage</summary>
    /// <summary>The name of the image and language need to be given</summary>
    /// <param name="imagename">The internal name of the image like "Bild_478.jpg" in "http://de.wikipedia.org/wiki/Bild:Bild_478.jpg"</param>
    /// <param name="language">Language of the Wikipedia page</param>
    public WikipediaImage(string imagename, string language)
    {
      SetLanguage(language);
      this.imagename = imagename;
      GetImageUrl();
      GetImageFile();
    }

    /// <summary>This constructor creates a new WikipediaArticle.</summary>
    /// <summary>Only called with a title string, language set to default.</summary>
    /// <param name="title">The article's title</param>
    public WikipediaImage(string imagename) : this(imagename, "Default") {}

    /// <summary>This constructor creates a new WikipediaArticle if no parameter is given.</summary>
    /// <summary>Uses an empty searchterm and the default language.</summary>
    public WikipediaImage() : this(string.Empty, "Default") {}

    #endregion

    #region class methods

    /// <summary>Gets the current MP language from mediaportal.xml and sets the Wikipedia URL accordingly</summary>
    private void SetLanguage(string language)
    {
      if (language == "Default")
      {
        Settings xmlreader = new MPSettings();
        language = xmlreader.GetValueAsString("skin", "language", "English");
      }

      Settings detailxmlreader = new Settings(Config.GetFile(Config.Dir.Config, "wikipedia.xml"));
      this.WikipediaURL = detailxmlreader.GetValueAsString(language, "imageurl", "http://en.wikipedia.org/wiki/Image:");
      Log.Info("Wikipedia: Image language set to " + language + ".");
    }

    /// <summary>Get the local filename of the downloaded image.</summary>
    /// <returns>String: filename of the downloaded image.</returns>
    public string GetImageFilename()
    {
      string imagelocal = Config.GetFile(Config.Dir.Thumbs, @"wikipedia\" + imagename);
      return imagelocal;
    }

    /// <summary>Getting the link to the full-size image.</summary>
    /// <returns>String: parsed article</returns>
    private void GetImageUrl()
    {
      string imagepage = string.Empty;

      // Build the URL to the Image page
      Uri url = new Uri(WikipediaURL + this.imagename);
      Log.Info("Wikipedia: Trying to get following Image page: {0}", url.ToString());

      // Here we get the content from the web and put it to a string
      try
      {
        WebClient client = new WebClient();
        client.Proxy.Credentials = CredentialCache.DefaultCredentials;
        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        Stream data = client.OpenRead(url);
        StreamReader reader = new StreamReader(data);
        imagepage = reader.ReadToEnd();
        reader.Close();
        Log.Info("Wikipedia: Success! Downloaded all data from the image page.");
      }
      catch (Exception e)
      {
        Log.Info("Wikipedia: Exception during downloading image page:");
        Log.Info(e.ToString());
      }

      //We're searching for something like this:
      //<div class="fullImageLink" id="file"><a href="http://upload.wikimedia.org/wikipedia/commons/7/7d/Bild_478.jpg">
      if (imagepage.IndexOf("class=\"fullImageLink\"") >= 0)
      {
        Log.Info("Wikipedia: Extracting link to full-size image.");
        int iStart = imagepage.IndexOf("class=\"fullImageLink\"");
        imagepage = imagepage.Substring(iStart, 1000);

        iStart = imagepage.IndexOf("href") + 6;
        int iEnd = imagepage.IndexOf("\"", iStart);

        this.imageurl = imagepage.Substring(iStart, iEnd - iStart);
        Log.Info("Wikipedia: URL of full-size image extracted.");
        Log.Info(imageurl);
      }
      else
      {
        this.imageurl = string.Empty;
      }
    }

    /// <summary>Downloads the full-size image from the wikipedia page</summary>
    private void GetImageFile()
    {
      if (imageurl != "")
      {
        //Check if we already have the file.
        string thumbspath = Config.GetSubFolder(Config.Dir.Thumbs, @"wikipedia\");

        //Create the wikipedia subdir in thumbs when it not exists.
        if (!Directory.Exists(thumbspath))
        {
          Directory.CreateDirectory(thumbspath);
        }

        if (!File.Exists(thumbspath + imagename))
        {
          Log.Info("Wikipedia: Trying to get following URL: {0}", imageurl);
          // Here we get the image from the web and save it to disk
          try
          {
            WebClient client = new WebClient();
            client.Proxy.Credentials = CredentialCache.DefaultCredentials;
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.DownloadFile(imageurl, thumbspath + imagename);
            Log.Info("Wikipedia: Success! Image downloaded.");
          }
          catch (Exception e)
          {
            Log.Info("Wikipedia: Exception during downloading:");
            Log.Info(e.ToString());
          }
        }
        else
        {
          Log.Info("Wikipedia: Image exists, no need to redownload!");
        }
      }
      else
      {
        Log.Info("Wikipedia: No imageurl. Can't download file.");
      }
    }

    #endregion
  }
}