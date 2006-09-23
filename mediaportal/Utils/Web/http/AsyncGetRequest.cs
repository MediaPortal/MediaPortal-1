using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace MediaPortal.Utils.Web
{
  public class AsyncGetRequest
  {
    public delegate void AsyncGetRequestCompleted(List<String> responseStrings, HttpStatusCode responseStatus, String requestedURLCommand);
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
      SendWorkerRequest((string)e.Argument, _requestDelay);
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
          request = (HttpWebRequest)WebRequest.Create(targetURL);
          //request.Timeout = 20000;
          request.Pipelined = false;

          request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
          //request.ContentType = "application/x-www-form-urlencoded";

          if (delayMSecs > 0)
            Thread.Sleep(delayMSecs);

          if (request == null)
            throw (new Exception());
        }
        catch (Exception ex1)
        {
          if (workerError != null)
            workerError(targetURL, ex1);
          return;
        }

        StreamReader reader = null;
        HttpStatusCode responseCode = new HttpStatusCode();

        // get the response
        try
        {
          response = (HttpWebResponse)request.GetResponse();
          // most likely timed out..
          if (response == null)
            throw (new Exception());

          reader = new StreamReader(response.GetResponseStream());
          responseCode = response.StatusCode;
          
          //request = null;
        }
        catch (Exception ex2)
        {
          if (workerError != null)
            workerError(targetURL, ex2);
          return;
        }        

        List<String> responseStrings = new List<string>();
        String tmp = String.Empty;
        while ((tmp = reader.ReadLine()) != null)
          responseStrings.Add(tmp);

        if (workerFinished != null)
          workerFinished(responseStrings, responseCode, targetURL);
      }
      finally
      {
        if (request != null)
          request = null;

        if (response != null)
        {
          response.Close();
          response = null;
        }
      }
    }
  }
}
