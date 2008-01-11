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
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
  /// <summary>
  /// Search IMPaward.com for movie-posters
  /// </summary>
  public class IMPawardsSearch
  {
    ArrayList imageList = new ArrayList();

    public IMPawardsSearch()
    {
    }

    public int Count
    {
      get { return imageList.Count; }
    }

    public string this[int index]
    {
      get
      {
        if (index < 0 || index >= imageList.Count) return string.Empty;
        return (string)imageList[index];
      }
    }

    public void Search(string searchtag)
    {
      if (searchtag == null) return;
      if (searchtag == string.Empty) return;
      imageList.Clear();
      searchtag = searchtag.Replace(" ", "+");
      string result = string.Empty;

      string url = "http://www.google.com/custom?domains=www.impawards.com&q=" + searchtag + "&sa=Google+Search&sitesearch=www.impawards.com";
      WebClient wc = new WebClient();
      try
      {
        byte[] buffer;
        buffer = wc.DownloadData(url);
        result = Encoding.UTF8.GetString(buffer);
      }
      catch (Exception)
      {
        return;
      }
      finally
      {
        wc.Dispose();
      }

      Match m = Regex.Match(result, @"http://www.impawards.com/(?<year>\d{4})/.*?.html");
      if (m.Success)
      {
        string year = m.Groups["year"].Value;
        string url2 = m.Value;
        try
        {
          byte[] buffer;
          buffer = wc.DownloadData(url2);
          result = Encoding.UTF8.GetString(buffer);
        }
        catch (Exception)
        {
          return;
        }
        finally
        {
          wc.Dispose();
        }

        //get main poster displayed on html-page
        m = Regex.Match(result, @"posters/.*?.jpg");
        if (m.Success)
        {
          imageList.Add("http://www.impawards.com/" + year + "/" + m.Value);

          //get other posters displayed on this html-page as thumbs
          MatchCollection mc = Regex.Matches(result, @"thumbs/imp_(?<poster>.*?.jpg)");
          foreach (Match m1 in mc)
          {
            imageList.Add("http://www.impawards.com/" + year + "/posters/" + m1.Groups["poster"].Value);
          }
        }
        else
          return;
      }
      else
        return;

    }
  }
}

