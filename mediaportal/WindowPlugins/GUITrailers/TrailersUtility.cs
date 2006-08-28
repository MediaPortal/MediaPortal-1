#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Net;
using System.Text;
using System.Threading;
using System.ComponentModel;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for TrailersUtility.
	/// </summary>
	public class TrailersUtility
	{
		string _downloadedText		= string.Empty;

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

				Log.Info("GUITrailers.DownloadWorker: {0}", ex.Message);
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
					wc.DownloadFile(downloadurl, Config.Get(Config.Dir.Thumbs) + "MPTemp -"+moviename + ".jpg");

          while (System.IO.File.Exists(Config.Get(Config.Dir.Thumbs) + "MPTemp -" + moviename + ".jpg") != true)
						GUIWindowManager.Process();
				}
		}

        public TrailersUtility()
		{
		}
	}
}
