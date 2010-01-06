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
using System.IO;
using System.Net;
using System.Threading;

namespace PostSetup
{
  public delegate void DownloadProgressHandler(int bytesRead, int totalBytes);


  // ClientGetAsync issues the async request.
  public class WebDownload
  {
    public ManualResetEvent allDone = new ManualResetEvent(false);
    private const int BUFFER_SIZE = 1024;
    // just so we can cancel this.
    private bool cancel = false;


    public byte[] Download(string url,
                           DownloadProgressHandler progressCB)
    {
      // Ensure flag set correctly.			
      allDone.Reset();

      // Get the URI from the command line.
      Uri httpSite = new Uri(url);

      // Create the request object.
      WebRequest req = WebRequest.Create(httpSite);
      try
      {
        // Use the current user in case an NTLM Proxy or similar is used.
        // wr.Proxy = WebProxy.GetDefaultProxy();
        req.Proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      catch (Exception) {}

      // Create the state object.
      DownloadInfo info = new DownloadInfo();

      // Put the request into the state object so it can be passed around.
      info.Request = req;

      // Assign the callbacks
      info.ProgressCallback += progressCB;

      // Issue the async request.
      IAsyncResult r = (IAsyncResult)req.BeginGetResponse(new AsyncCallback(ResponseCallback), info);

      // Wait until the ManualResetEvent is set so that the application
      // does not exit until after the callback is called.
      allDone.WaitOne();

      // Pass back the downloaded information.

      if (info.useFastBuffers)
      {
        return info.dataBufferFast;
      }
      else
      {
        byte[] data = new byte[info.dataBufferSlow.Count];
        for (int b = 0; b < info.dataBufferSlow.Count; b++)
        {
          data[b] = (byte)info.dataBufferSlow[b];
        }
        return data;
      }
    }

    private void ResponseCallback(IAsyncResult ar)
    {
      // Get the DownloadInfo object from the async result were
      // we're storing all of the temporary data and the download
      // buffer.
      DownloadInfo info = (DownloadInfo)ar.AsyncState;

      // Get the WebRequest from RequestState.
      WebRequest req = info.Request;
      try
      {
        // Use the current user in case an NTLM Proxy or similar is used.
        // wr.Proxy = WebProxy.GetDefaultProxy();
        req.Proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      catch (Exception) {}

      // Call EndGetResponse, which produces the WebResponse object
      // that came from the request issued above.
      WebResponse resp = req.EndGetResponse(ar);

      // Find the data size from the headers.
      string strContentLength = resp.Headers["Content-Length"];
      if (strContentLength != null)
      {
        info.dataLength = Convert.ToInt32(strContentLength);
        info.dataBufferFast = new byte[info.dataLength];
      }
      else
      {
        info.useFastBuffers = false;
        info.dataBufferSlow = new ArrayList(BUFFER_SIZE);
      }

      //  Start reading data from the response stream.
      Stream ResponseStream = resp.GetResponseStream();

      // Store the response stream in RequestState to read
      // the stream asynchronously.
      info.ResponseStream = ResponseStream;

      //  Pass do.BufferRead to BeginRead.
      IAsyncResult iarRead = ResponseStream.BeginRead(info.BufferRead,
                                                      0,
                                                      BUFFER_SIZE,
                                                      new AsyncCallback(ReadCallBack),
                                                      info);
    }

    private void ReadCallBack(IAsyncResult asyncResult)
    {
      // Get the DownloadInfo object from AsyncResult.
      DownloadInfo info = (DownloadInfo)asyncResult.AsyncState;

      // Retrieve the ResponseStream that was set in RespCallback.
      Stream responseStream = info.ResponseStream;

      // Read info.BufferRead to verify that it contains data.
      int bytesRead = responseStream.EndRead(asyncResult);
      if (bytesRead > 0)
      {
        if (this.cancel)
        {
          info.ResponseStream = null;
        }

        if (info.useFastBuffers)
        {
          Array.Copy(info.BufferRead, 0,
                     info.dataBufferFast, info.bytesProcessed,
                     bytesRead);
        }
        else
        {
          for (int b = 0; b < bytesRead; b++)
          {
            info.dataBufferSlow.Add(info.BufferRead[b]);
          }
        }
        info.bytesProcessed += bytesRead;

        // If a registered progress-callback, inform it of our
        // download progress so far.
        if (info.ProgressCallback != null)
        {
          info.ProgressCallback(info.bytesProcessed, info.dataLength);
        }

        // Continue reading data until responseStream.EndRead returns –1.
        IAsyncResult ar = responseStream.BeginRead(
          info.BufferRead, 0, BUFFER_SIZE,
          new AsyncCallback(ReadCallBack), info);
      }
      else
      {
        responseStream.Close();
        allDone.Set();
      }
      return;
    }

    public void Cancel()
    {
      this.cancel = true;
    }
  }
}