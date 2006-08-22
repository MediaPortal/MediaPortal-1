using System;
using System.Net;
using System.Text;
using System.Threading;
using System.ComponentModel;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for TrailersUtility.
	/// </summary>
	public class TrailersUtility
	{
		string _downloadedText		= string.Empty;
    static IConfig _config;

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

                _downloadedText = Encoding.UTF8.GetString(HTMLBuffer);
			}
			catch(Exception ex)
			{
        ServiceProvider services = GlobalServiceProvider.Instance;
        ILog log = services.Get<ILog>();

				log.Info("GUITrailers.DownloadWorker: {0}", ex.Message);
			}
			finally
			{
				wc.Dispose();
			}

			_workerCompleted = true;
		}

		public static bool _workerCompleted = true;
        public static bool interupted = false;

		public void DownloadPoster(string downloadurl, string moviename)
		{
				using(WaitCursor cursor = new WaitCursor())
				{
					// Download Poster
					WebClient wc = new WebClient();
					moviename = moviename.Replace(":","-");
					wc.DownloadFile(downloadurl, _config.Get(Config.Options.ThumbsPath) + "MPTemp -"+moviename + ".jpg");

          while (System.IO.File.Exists(_config.Get(Config.Options.ThumbsPath) + "MPTemp -" + moviename + ".jpg") != true)
						GUIWindowManager.Process();
				}
		}

        public TrailersUtility()
		{
      ServiceProvider services = GlobalServiceProvider.Instance;
      _config = services.Get<IConfig>();
		}
	}
}
