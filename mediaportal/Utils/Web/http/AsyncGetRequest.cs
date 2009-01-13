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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace MediaPortal.Utils.Web
{
  public class AsyncGetRequest
  {
    public delegate void AsyncGetRequestCompleted(
      List<String> responseStrings, HttpStatusCode responseStatus, String requestedURLCommand);

    public event AsyncGetRequestCompleted workerFinished;

    public delegate void AsyncGetRequestError(String commandURL, Exception errorReason);

    public event AsyncGetRequestError workerError;

    // not really threadsafe but this is only used for the following param - no harm if overwritten
    private int _requestDelay = 0;

    public void SendAsyncGetRequest(String _url)
    {
      BackgroundWorker worker = new BackgroundWorker();
      _requestDelay = 0;
      worker.DoWork += new DoWorkEventHandler(RequestWorker_DoWork);
      worker.RunWorkerAsync(_url);
    }

    public void SendDelayedAsyncGetRequest(String _url, int _delayMSecs)
    {
      BackgroundWorker worker = new BackgroundWorker();
      _requestDelay = _delayMSecs;
      worker.DoWork += new DoWorkEventHandler(RequestWorker_DoWork);
      worker.RunWorkerAsync(_url);
    }


    private void RequestWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "HTTP Async request";
      SendWorkerRequest((string) e.Argument, _requestDelay);
    }

    private void SendWorkerRequest(String targetURL, int delayMSecs)
    {
      HttpWebRequest request = null;
      HttpWebResponse response = null;
      try
      {
        // send the command
        try
        {
          request = (HttpWebRequest) WebRequest.Create(targetURL);
          try
          {
            // Use the current user in case an NTLM Proxy or similar is used.
            // request.Proxy = WebProxy.GetDefaultProxy();
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          catch (Exception)
          {
          }

          //request.Timeout = 20000;
          request.Pipelined = false;

          request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
          //request.UserAgent = "User-Agent: Last.fm Client 1.5.1.30182 (Windows)";
          //request.ContentType = "application/x-www-form-urlencoded";

          if (delayMSecs > 0)
          {
            Thread.Sleep(delayMSecs);
          }

          if (request == null)
          {
            throw (new Exception());
          }
        }
        catch (Exception ex1)
        {
          if (workerError != null)
          {
            workerError(targetURL, ex1);
          }
          return;
        }

        StreamReader reader = null;
        HttpStatusCode responseCode = new HttpStatusCode();

        // get the response
        try
        {
          response = (HttpWebResponse) request.GetResponse();
          // most likely timed out..
          if (response == null)
          {
            throw (new Exception());
          }

          reader = new StreamReader(response.GetResponseStream());
          responseCode = response.StatusCode;

          //request = null;
        }
        catch (Exception ex2)
        {
          if (workerError != null)
          {
            workerError(targetURL, ex2);
          }
          return;
        }

        List<String> responseStrings = new List<string>();
        String tmp = string.Empty;
        while ((tmp = reader.ReadLine()) != null)
        {
          responseStrings.Add(tmp);
        }

        if (workerFinished != null)
        {
          workerFinished(responseStrings, responseCode, targetURL);
        }
      }
      finally
      {
        if (request != null)
        {
          request = null;
        }

        if (response != null)
        {
          response.Close();
          response = null;
        }
      }
    }
  }
}