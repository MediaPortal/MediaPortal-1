using System;

namespace PostSetup
{
	public delegate void DownloadCompleteHandler(byte[] dataDownloaded);
	public delegate void DownloadErrorHandler(Exception e);

	/// <summary>
	/// Summary description for DownloadThread.
	/// </summary>
	public class DownloadThread
	{
		public event DownloadCompleteHandler CompleteCallback;
		public event DownloadProgressHandler ProgressCallback;
		public event DownloadErrorHandler ErrorCallback;
		

		public string _downloadUrl = "";

		public string DownloadUrl
		{
			get { return _downloadUrl; }
			set { _downloadUrl = value; }
		}

		public void Download()
		{
			WebDownload webDL = null;
			try
			{
				if (CompleteCallback != null &&
					DownloadUrl != "")
				{
					webDL = new WebDownload();
					byte[] downloadedData = webDL.Download(DownloadUrl,ProgressCallback);
					CompleteCallback(downloadedData);					
				}
			}
			catch (Exception e)
			{			
				webDL.Cancel();
				ErrorCallback(e);								
				
			}
		}
	}
}