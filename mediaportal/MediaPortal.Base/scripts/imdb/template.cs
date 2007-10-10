//css_reference "core.dll";
//css_reference "Databases.dll";
//css_reference "utils.dll";

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Collections;
using System.Web;
using System.Text.RegularExpressions;
using MediaPortal.Util;

// change to Grabber, this for avoid to load by mediaportal
class Grabber_Template : MediaPortal.Video.Database.IIMDBScriptGrabber
{
  public Grabber()
    {
    }

  void MediaPortal.Video.Database.IIMDBScriptGrabber.FindFilm(string strSearch, int iLimit, ArrayList elements)
  {
      // code for search for movie titles  
      // .............
      //.............
      // if movie found add to listing 
      MediaPortal.Video.Database.IMDB.IMDBUrl url = new MediaPortal.Video.Database.IMDB.IMDBUrl(strURL, strTitle + " (imdb_com)", "imdb_com");
      elements.Add(url);
  }


  bool MediaPortal.Video.Database.IIMDBScriptGrabber.GetDetails(MediaPortal.Video.Database.IMDB.IMDBUrl url, ref MediaPortal.Video.Database.IMDBMovie movieDetails)
  {
    // preces the web page defined by the url
    // ................
    // ................
    // then fill the database
    // movieDetails.Year
    // movieDetails.Genre 
    // movieDetails.Votes
    // movieDetails.Top250
    // movieDetails.TagLine
    // movieDetails.PlotOutline
    // movieDetails.ThumbURL
    // movieDetails.Plot
    // movieDetails.Cast
    // movieDetails.RunTime = ....
    // movieDetails.MPARating = ......

    // found some information 
    return true;
    // else
    return false;
  }

  string MediaPortal.Video.Database.IIMDBScriptGrabber.GetName()
  {
    return "IMDB grabber ";
  }
 
  string MediaPortal.Video.Database.IIMDBScriptGrabber.GetLanguage()
  {
    return "EN";
  }

  // a general procedure to get a web page 
  // use like :
  //    string absoluteUri;
  //    string strURL = "http://us.imdb.com/Tsearch?title=" + strSearch;
  //    string strBody = GetPage(strURL, "utf-8", out absoluteUri);

  private string GetPage(string strURL, string strEncode, out string absoluteUri)
  {
    string strBody = "";
    absoluteUri = String.Empty;
    Stream ReceiveStream = null;
    StreamReader sr = null;
    WebResponse result = null;
    try
    {
      // Make the Webrequest
      //Log.Info("IMDB: get page:{0}", strURL);
      WebRequest req = WebRequest.Create(strURL);

      result = req.GetResponse();
      ReceiveStream = result.GetResponseStream();

      // Encoding: depends on selected page
      Encoding encode = System.Text.Encoding.GetEncoding(strEncode);
      sr = new StreamReader(ReceiveStream, encode);
      strBody = sr.ReadToEnd();

      absoluteUri = result.ResponseUri.AbsoluteUri;
    }
    catch (Exception)
    {
      //Log.Error("Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message, ex.StackTrace);
    }
    finally
    {
      if (sr != null)
      {
        try
        {
          sr.Close();
        }
        catch (Exception)
        {
        }
      }
      if (ReceiveStream != null)
      {
        try
        {
          ReceiveStream.Close();
        }
        catch (Exception)
        {
        }
      }
      if (result != null)
      {
        try
        {
          result.Close();
        }
        catch (Exception)
        {
        }
      }
    }
    return strBody;
  } // END GetPage()

}