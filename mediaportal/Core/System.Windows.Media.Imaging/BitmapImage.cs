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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace System.Windows.Media.Imaging
{
  public sealed class BitmapImage : BitmapSource, ISupportInitialize
  {
    #region Constructors

    public BitmapImage() {}

    public BitmapImage(Uri uriSource)
    {
      _uriSource = uriSource;
    }

    #endregion Constructors

    #region Events

    public override event EventHandler DownloadCompleted;
    public override event DownloadProgressEventHandler DownloadProgress;

    #endregion Events

    #region Methods

    public void BeginInit() {}

    public new BitmapImage Copy()
    {
      return (BitmapImage)base.Copy();
    }

    protected override Freezable CreateInstanceCore()
    {
      return new BitmapImage();
    }

    private void DownloadWorker(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "BitmapImage-Downloader";
      using (WebClient client = new WebClient())
      {
        client.Proxy.Credentials = CredentialCache.DefaultCredentials;
        client.DownloadData((string)e.Argument);
      }
    }

    private void DownloadWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      if (DownloadProgress != null)
        DownloadProgress(this, new DownloadProgressEventArgs(e.ProgressPercentage));
    }

    private void DownloadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (DownloadCompleted != null)
        DownloadCompleted(this, EventArgs.Empty);
    }

    public void EndInit()
    {
      if (_uriSource.IsFile == false)
      {
        BackgroundWorker worker = new BackgroundWorker();

        worker.DoWork += new DoWorkEventHandler(DownloadWorker);
        worker.ProgressChanged += new ProgressChangedEventHandler(DownloadWorkerProgressChanged);
        worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownloadWorkerCompleted);
        worker.WorkerReportsProgress = true;
        worker.WorkerSupportsCancellation = true;
        worker.RunWorkerAsync(_uriSource.ToString());
      }
    }

    public new BitmapImage GetCurrentValue()
    {
      return this;
    }

    #endregion Methods

    #region Properties

    public BitmapCacheOption CacheOption
    {
      get { return _cacheOption; }
      set { _cacheOption = value; }
    }

    public BitmapCreateOptions CreateOptions
    {
      get { return _createOptions; }
      set { _createOptions = value; }
    }

    public int DecodePixelHeight
    {
      get { return _decodePixelHeight; }
      set { _decodePixelHeight = value; }
    }

    public int DecodePixelWidth
    {
      get { return _decodePixelWidth; }
      set { _decodePixelWidth = value; }
    }

    public override bool IsDownloading
    {
      get { return _isDownloading; }
    }

    public Rotation Rotation
    {
      get { return _rotation; }
      set { _rotation = value; }
    }

//		public Int32Rect SourceRect
//		{
//			get { throw new NotImplementedException(); }
//			set { throw new NotImplementedException(); }
//		}

    public Stream StreamSource
    {
      get { return _streamSource; }
      set { _streamSource = value; }
    }

    public Uri UriSource
    {
      get { return _uriSource; }
      set { _uriSource = value; }
    }

    #endregion Properties

    #region Fields

    private BitmapCacheOption _cacheOption;
    private BitmapCreateOptions _createOptions;
    private int _decodePixelHeight;
    private int _decodePixelWidth;
    private bool _isDownloading = false;
    private Rotation _rotation;
    private Stream _streamSource;
    private Uri _uriSource;

    #endregion Fields
  }
}