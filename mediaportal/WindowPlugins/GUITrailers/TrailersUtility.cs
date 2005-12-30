using System;
using System.Net;
using System.Text;
using System.Threading;
using System.ComponentModel;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;


namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for TrailersUtility.
	/// </summary>
	public class TrailersUtility
	{
		string _downloadedText		= string.Empty;

		// For Downloading trailers
		string DownloadFileName = string.Empty;
		string DownloadFileUrl = string.Empty;
//		bool downloadfileview = false;

		public void GetWebPage(string url, out string HTMLDownload) // Get url and put in string
		{
			if(_workerCompleted)
			{
				_workerCompleted = false;

				BackgroundWorker worker = new BackgroundWorker();

				worker.DoWork += new DoWorkEventHandler(DownloadWorker);
				worker.RunWorkerAsync(url);

				using(WaitCursor cursor = new WaitCursor())
				{
					while(_workerCompleted == false)
						GUIWindowManager.Process();
				}

				HTMLDownload = _downloadedText;

				_downloadedText = null;
			}
			else
			{
				HTMLDownload = string.Empty;
			}
		}

		public void DownloadWorker(object sender, DoWorkEventArgs e)
		{
			WebClient wc = new WebClient();

			try
			{
				byte[] HTMLBuffer;

				HTMLBuffer = wc.DownloadData((string)e.Argument);

                _downloadedText = Encoding.UTF8.GetString(HTMLBuffer);//wc.DownloadString((string)e.Argument);//
			}
			catch(Exception ex)
			{
				Log.Write("GUITrailers.DownloadWorker: {0}", ex.Message);
			}
			finally
			{
				wc.Dispose();
			}

			_workerCompleted = true;
		}

		public bool _workerCompleted = true;

		public void DownloadPoster(string downloadurl, string moviename)
		{
				using(WaitCursor cursor = new WaitCursor())
				{
					// Download Poster
					WebClient wc = new WebClient();
					moviename = moviename.Replace(":","-");
					wc.DownloadFile(downloadurl, @"thumbs\MPTemp -"+moviename + ".jpg");

					while(System.IO.File.Exists(@"thumbs\MPTemp -"+moviename + ".jpg")!=true)
						GUIWindowManager.Process();
				}
		}

        #region Download File in background and popup notify-window
        //void DownloadFile()
        //{
        //    if (_workerDownloadFileCompleted)
        //    {
        //        _workerDownloadFileCompleted = false;

        //        BackgroundWorker worker = new BackgroundWorker();

        //        worker.DoWork += new DoWorkEventHandler(DownloadFileWorker);
        //        worker.RunWorkerAsync(DownloadFileUrl);

        //        using (WaitCursor cursor = new WaitCursor())
        //        {
        //            while (_workerDownloadFileCompleted == false)
        //                GUIWindowManager.Process();
        //        }
        //    }
        //}

        //void DownloadFileWorker(object sender, DoWorkEventArgs e)
        //{
        //    //			string url = "http://movies.apple.com/movies/lionsgate/saw_2/saw_ii-tlr2_m480.mov";
        //    //			string filename = @"c:\saw.mov";

        //    string url = DownloadFileUrl;
        //    string filename = @"c:\" + DownloadFileName;

        //    WebClient wc = new WebClient();

        //    try
        //    {
        //        wc.DownloadFile(url, filename);

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write("Guitrailers.DownloadFile: {0}", ex.Message);
        //    }
        //    finally
        //    {
        //        wc.Dispose();
        //    }

        //    _workerDownloadFileCompleted = true;

        //    GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        //    dlgNotify.Reset();
        //    dlgNotify.ClearAll();
        //    dlgNotify.SetHeading("Download finished");
        //    if (System.IO.File.Exists(@"thumbs\MPTemp -" + DownloadFileName + ".jpg") == true)
        //        dlgNotify.SetImage(@"thumbs\MPTemp -" + DownloadFileName + ".jpg");
        //    dlgNotify.SetText("finished download of trailer: " + DownloadFileName);
        //    dlgNotify.TimeOut = 5;
        //    //dlgNotify.DoModal(GetID);

        //}

        //bool _workerDownloadFileCompleted = true;
        #endregion
		public TrailersUtility()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
