using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using MediaPortal.Threading;
using MediaPortal.Services;
using MetadataExtractor;

namespace MediaPortal.ServiceImplementations
{
  public class ImageLoadService : IImageLoadService
  {
    #region Constants
    private const int _LIFETIME_IMAGES_DEFAULT = 60 * 24 * 7; //[min]

    private const int _WIDTH_THUMB_DEFAULT = 400; //for screen width 1920
    private const int _WIDTH_ICON_DEFAULT = 50; //for screen width 1920

    private const int _JOB_MAINTENANCE_CHECK_PERIOD = 30; //[secs]
    private const int _JOB_MAINTENANCE_JOB_LIFETIME = 60; //[secs]

    #endregion

    #region Types

    private class WorkClient : IAsyncResult
    {
      public Size SizeIconMax { get; } = Size.Empty;
      public Size SizeThumbMax { get; } = Size.Empty;

      private ImageLoadEventHandler _Callback;
      private bool _CompletedSynchronously = false;
      private bool _IsCompleted = false;
      private ManualResetEvent _AsyncWaitHandle = new ManualResetEvent(false);
      private object _AsyncState;

      public WorkClient(Size sizeIconMax, Size sizeThumbMax, ImageLoadEventHandler callback, object asyncState)
      {
        this.SizeIconMax = sizeIconMax;
        this.SizeThumbMax = sizeThumbMax;
        this._Callback = callback;
        this._AsyncState = asyncState;
      }

      #region IAsyncResult

      public bool IsCompleted => this._IsCompleted;

      public WaitHandle AsyncWaitHandle => this._AsyncWaitHandle;

      public object AsyncState => this._AsyncState;

      public bool CompletedSynchronously => this._CompletedSynchronously;

      #endregion

      internal void SetComplete(Job job, bool bCompletedSynchronously)
      {
        this._CompletedSynchronously = bCompletedSynchronously;
        this._IsCompleted = true;
        this._AsyncWaitHandle.Set();

        if (this._Callback != null)
        {
          try
          {
            //Callback to the client
            this._Callback(this, job.CreateImageLoadEventArgumnets(this));
          }
          catch (Exception ex)
          {
            _Logger.Error("[WorkClient][SetComplete] Error: {0}", ex.Message);
          }
        }
      }
    }

    private class Job : IWork
    {
      public Size ImageSize = Size.Empty;
      public DateTime TimeStampKeepAlive;
      public bool Done = false;

      /// <summary>
      /// Requested Url
      /// </summary>
      public string Url;

      /// <summary>
      /// Fullpath of the cached file
      /// </summary>
      public string FilePath;

      /// <summary>
      /// Fullpath of the cached thumb file
      /// </summary>
      public string FilePathThumb;

      /// <summary>
      /// Fullpath of the cached icon file
      /// </summary>
      public string FilePathIcon;

      public List<WorkClient> Clients = new List<WorkClient>();

      public int CacheLifeTime = -1;

      /// <summary>
      /// Caching service
      /// </summary>
      public IHttpCachingService Caching;

      /// <summary>
      /// Job done callback
      /// </summary>
      public EventHandler DoneCallback;

      /// <summary>
      /// True, if the requested file was truly downloaded - not loaded from the cache
      /// </summary>
      public bool Downloaded = false;

      public DateTime DownloadTimeStamp = DateTime.MinValue;

      public ImageLoadEventArgs CreateImageLoadEventArgumnets(WorkClient client)
      {
        //Determine final image path based on client's request
        string strFilepathResult = this.FilePath; //default = original
        if (!this.ImageSize.IsEmpty)
        {
          if (this.FilePathThumb != null && !client.SizeThumbMax.IsEmpty && this.ImageSize.Width > client.SizeThumbMax.Width)
            strFilepathResult = this.FilePathThumb;
          else if (this.FilePathIcon != null && !client.SizeIconMax.IsEmpty && this.ImageSize.Width > client.SizeIconMax.Width)
            strFilepathResult = this.FilePathIcon;
        }

        return new ImageLoadEventArgs()
        {
          Url = this.Url,
          FilePath = this.FilePath,
          FilePathThumb = this.FilePathThumb,
          FilePathIcon = this.FilePathIcon,
          FilePathResult = strFilepathResult,
          Status = this.State,
          ImageSize = this.ImageSize,
          DownloadTimeStamp = this.DownloadTimeStamp,
          AsyncResult = client,
          State = client.AsyncState
        };
      }

      #region IWork
      public WorkState State { get; set; }

      public string Description { get; set; }

      public Exception Exception { get; set; }

      public ThreadPriority ThreadPriority { get; set; }

      public void Process()
      {
        // don't perform canceled work
        if (this.State == WorkState.CANCELED)
        {
          return;
        }
        // don't perform work which is in an invalid state
        if (this.State != WorkState.INQUEUE)
        {
          throw new InvalidOperationException(String.Format("[ImageLoadService][ImageWork][Process] WorkState for work {0} not INQUEUE, but {1}",
            this.Description, this.State));
        }

        this.State = WorkState.INPROGRESS;
        this.FilePath = null;
        try
        {
          this.FilePath = this.Caching.DownloadFile(this.Url,
              iLifeTime: this.CacheLifeTime > 0 ? this.CacheLifeTime : _LIFETIME_IMAGES_DEFAULT,
              postDownload: this.cbDownload,
              state: null);

          this.State = this.FilePath != null ? WorkState.FINISHED : WorkState.ERROR;
        }
        catch (Exception ex)
        {
          _Logger.Error("[ImageLoadService][ImageWork][Process] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
          this.State = WorkState.ERROR;
        }

        //Callback
        this.DoneCallback(this, null);
      }

      #endregion

      /// <summary>
      /// Callback from the caching service(when download is complete)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbDownload(object sender, HttpCachingEventArgs e)
      {
        this.Downloaded = true;
        this.DownloadTimeStamp = DateTime.Now;
      }
    }


    #endregion

    #region Private fileds
    //Services
    private static ILog _Logger = GlobalServiceProvider.Get<ILog>();
    private IThreadPool _ThreadPool = GlobalServiceProvider.Get<IThreadPool>();
    private IHttpCachingService _Caching = GlobalServiceProvider.Get<IHttpCachingService>();

    private List<Job> _JobList = new List<Job>();

    private int _IconWidth = _WIDTH_ICON_DEFAULT;
    private int _ThumbWidth = _WIDTH_THUMB_DEFAULT;

    private DateTime _MaintenanceTs = DateTime.Now;

    #endregion

    #region Events

    #endregion

    #region Public fields

    public int IconWidth
    {
      get { return this._IconWidth; }
      set
      {
        if (value < 10)
          this._IconWidth = 10;
        else
          this._IconWidth = value;
      }
    }

    public int ThumbWidth
    {
      get { return this._ThumbWidth; }
      set
      {
        if (value < 10)
          this._ThumbWidth = 10;
        else
          this._ThumbWidth = value;
      }
    }

    #endregion

    #region ctor
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sizeScreen">Screen size to calculate icon & thumb width</param>
    public ImageLoadService(Size sizeScreen)
    {
      //Hook to caching service to delete icon/thumb files
      this._Caching.DeleteEvent += this.cbDeleteCachingFile;

      if (!sizeScreen.IsEmpty)
      {
        float f = sizeScreen.Width / 1920F;
        this.IconWidth = (int)(_WIDTH_ICON_DEFAULT * f);
        this.ThumbWidth = (int)(_WIDTH_THUMB_DEFAULT * f);
      }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Add new download task.
    /// </summary>
    /// <param name="strUrl">Url of the file to be downloaded</param>
    /// <param name="sizeIconMax">Create icon if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <param name="sizeThumbMax">Create thumb if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <returns></returns>
    public IAsyncResult BeginDownload(string strUrl, Size sizeIconMax, Size sizeThumbMax)
    {
      return this.BeginDownload(strUrl, sizeIconMax, sizeThumbMax, -1, null, null);
    }

    /// <summary>
    /// Add new download task.
    /// </summary>
    /// <param name="strUrl">Url of the file to be downloaded</param>
    /// <param name="sizeIconMax">Create icon if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <param name="sizeThumbMax">Create thumb if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <param name="iLifeTime">Lifetime of the cached file in minutes. <1 for default</param>
    /// <param name="userCallback">Optional callback to be executed upon task completation</param>
    /// <param name="stateObject">Optional user object passed to the callback</param>
    /// <returns></returns>
    public IAsyncResult BeginDownload(string strUrl, Size sizeIconMax, Size sizeThumbMax, int iLifeTime, ImageLoadEventHandler userCallback, object stateObject)
    {
      if (!Uri.IsWellFormedUriString(strUrl, UriKind.Absolute))
      {
        _Logger.Warn("[ImageLoadService][BeginDownload] Invalid url.");
        return null;
      }

      //Final cached filename
      string strFilename = HttpCachingSevice.GetFileNameHash(strUrl);

      WorkClient ar = new WorkClient(sizeIconMax, sizeThumbMax, userCallback, stateObject);

      lock (this._JobList)
      {
        //Do some job cleaning
        this.jobsMaintenance();

        Job job = this._JobList.Find(j => j.Url.Equals(strUrl)); //check for existing job
        if (job == null)
        {
          //New request
          job = new Job()
          {
            Url = strUrl,
            Caching = this._Caching,
            DoneCallback = this.cbJobDone,
            CacheLifeTime = iLifeTime,
            ThreadPriority = ThreadPriority.BelowNormal
          };

          job.Clients.Add(ar);

          //Add the new job to the list
          this._JobList.Add(job);

          //Add the new task to the thread pool for execution
          this._ThreadPool.Add(job);
        }
        else
        {
          //Existing
          if (job.Done)
          {
            //Job is already done

            //_Logger.Debug("[ImageLoadService][BeginDownload] Already Done: {1}", this._Id, strUrl);

            job.TimeStampKeepAlive = DateTime.Now;

            this.buildThumb(job, sizeIconMax, sizeThumbMax);

            ar.SetComplete(job, true);
          }
          else
          {
            //Job is currently being processed

            //_Logger.Debug("[ImageLoadService][BeginDownload] Url already exists: {1}", this._Id, strUrl);
            job.Clients.Add(ar);
          }
        }
      }
      return ar;
    }

    public bool EndDownload(IAsyncResult ar)
    {
      if (ar == null)
        return false;

      return ar.IsCompleted;
    }

    #endregion

    #region Private methods

    private void jobsMaintenance()
    {
      if ((DateTime.Now - this._MaintenanceTs).TotalSeconds >= _JOB_MAINTENANCE_CHECK_PERIOD)
      {
        for (int i = this._JobList.Count - 1; i >= 0; i--)
        {
          Job j = this._JobList[i];
          if (j.Done && (DateTime.Now - j.TimeStampKeepAlive).TotalSeconds >= _JOB_MAINTENANCE_JOB_LIFETIME)
          {
            this._JobList.RemoveAt(i);
            //_Logger.Debug("[ImageLoadService][jobsMaintenance] Removed: {0}", j.Url);
          }
        }

        this._MaintenanceTs = DateTime.Now;
      }
    }

    private void buildThumb(Job job, Size sizeIconMax, Size sizeThumbMax)
    {
      if (job.FilePathIcon != null && job.FilePathThumb != null)
        return; //already created

      string strFilePathThumb = job.FilePath + ".thumb";
      string strFilePathIcon = job.FilePath + ".icon";

      try
      {
        if (job.Downloaded)
        {
          //New content
          //Delete existing thum & icon

          _Logger.Debug("[ImageLoadService][buildThumb] New content: cleaning icon & thumb... {0}", job.FilePath);

          if (System.IO.File.Exists(strFilePathIcon))
            System.IO.File.Delete(strFilePathIcon);

          if (System.IO.File.Exists(strFilePathThumb))
            System.IO.File.Delete(strFilePathThumb);

          job.FilePathIcon = null;
          job.FilePathThumb = null;

          job.Downloaded = false;
        }

        if (!System.IO.File.Exists(job.FilePath))
          return;

        int iAttempts = 3;
        while (iAttempts-- > 0)
        {
          Image im = null;
          Image imThumb = null;
          Image imIcon = null;
          string strFormat = null;

          try
          {
            //Extract width
            if (job.ImageSize == Size.Empty)
            {
              IList<Directory> dirs = ImageMetadataReader.ReadMetadata(job.FilePath);
              for (int i = 0; i < dirs.Count; i++)
              {
                Directory directory = dirs[i];
                if (directory is MetadataExtractor.Formats.Jpeg.JpegDirectory
                    || directory is MetadataExtractor.Formats.Png.PngDirectory
                    || directory is MetadataExtractor.Formats.Bmp.BmpHeaderDirectory
                    || directory is MetadataExtractor.Formats.Gif.GifHeaderDirectory
                    || directory is MetadataExtractor.Formats.WebP.WebPDirectory
                    )
                {
                  strFormat = directory.Name;
                  int iWidth = -1, iHeigth = -1;
                  Tag mTag = directory.Tags.Where((x) => x.Name == "Image Width").FirstOrDefault();
                  if (mTag != null)
                  {
                    iWidth = directory.GetInt32(mTag.Type);
                    mTag = directory.Tags.Where((x) => x.Name == "Image Height").FirstOrDefault();
                    if (mTag != null)
                      iHeigth = directory.GetInt32(mTag.Type);
                  }
                  job.ImageSize = new Size(iWidth, iHeigth);
                  break;
                }
              }
            }

            //Icon
            if (job.FilePathIcon == null && !sizeIconMax.IsEmpty && job.ImageSize.Width > sizeIconMax.Width && job.ImageSize.Width > this._IconWidth && !System.IO.File.Exists(strFilePathIcon))
            {
              if (im == null)
              {
                //Create image from downloaded file
                if (strFormat == "WebP")
                {
                  im = Imaging.WebP.Load(job.FilePath);
                  if (im == null)
                    throw new Exception("Failed to load WebP image.");
                }
                else
                  im = Image.FromFile(job.FilePath);
              }

              //Create icon
              _Logger.Debug("[ImageLoadService][buildThumb] Creating icon... {0}", strFilePathIcon);
              imIcon = new Bitmap(im, new Size(this._IconWidth, (int)((float)im.Height / im.Width * this._IconWidth)));
              imIcon.Save(strFilePathIcon, System.Drawing.Imaging.ImageFormat.Png);
              imIcon.Dispose();
              imIcon = null;
              job.FilePathIcon = strFilePathIcon;
            }

            //Thumb
            if (job.FilePathThumb == null && !sizeThumbMax.IsEmpty && job.ImageSize.Width > sizeThumbMax.Width && job.ImageSize.Width > this._ThumbWidth && !System.IO.File.Exists(strFilePathThumb))
            {
              //Create image from downloaded file
              if (strFormat == "WebP")
              {
                im = Imaging.WebP.Load(job.FilePath);
                if (im == null)
                  throw new Exception("Failed to load WebP image.");
              }
              else
                im = Image.FromFile(job.FilePath);

              //Create thumb
              _Logger.Debug("[ImageLoadService][buildThumb] Creating thumb... {0}", strFilePathThumb);
              imThumb = new Bitmap(im, new Size(this._ThumbWidth, (int)((float)im.Height / im.Width * this._ThumbWidth)));
              im.Dispose();
              im = null;
              imThumb.Save(strFilePathThumb, System.Drawing.Imaging.ImageFormat.Png);
              imThumb.Dispose();
              imThumb = null;
              job.FilePathThumb = strFilePathThumb;
            }

            return;
          }
          catch (Exception ex)
          {
            _Logger.Error("[ImageLoadService][buildThumb] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
          }
          finally
          {
            if (im != null)
            {
              im.Dispose();
              im = null;
            }

            if (imThumb != null)
            {
              imThumb.Dispose();
              imThumb = null;
            }

            if (imIcon != null)
            {
              imIcon.Dispose();
              imIcon = null;
            }
          }

          //Try again later
          Thread.Sleep(1000);
        }
      }
      finally
      {
        if (job.FilePathIcon == null)
          job.FilePathIcon = System.IO.File.Exists(strFilePathIcon) ? strFilePathIcon : null;

        if (job.FilePathThumb == null)
          job.FilePathThumb = System.IO.File.Exists(strFilePathThumb) ? strFilePathThumb : null;
      }
    }

    #endregion

    #region Callbacks
    private void cbDeleteCachingFile(object sender, HttpCachingEventArgs e)
    {
      string strFileToDelete = e.FileFullPath + ".icon";
      if (System.IO.File.Exists(strFileToDelete))
      {
        _Logger.Debug("[ImageLoadService][cbDeleteCachingFile] Deleting file: {0}", strFileToDelete);
        try { System.IO.File.Delete(strFileToDelete); }
        catch (Exception ex)
        { _Logger.Error("[ImageLoadService][cbDeleteCachingFile] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace); }
      }

      strFileToDelete = e.FileFullPath + ".thumb";
      if (System.IO.File.Exists(strFileToDelete))
      {
        _Logger.Debug("[ImageLoadService][cbDeleteCachingFile] Deleting file: {0}", strFileToDelete);
        try { System.IO.File.Delete(strFileToDelete); }
        catch (Exception ex)
        { _Logger.Error("[ImageLoadService][cbDeleteCachingFile] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace); }
      }
    }

    private void cbJobDone(object sender, EventArgs e)
    {
      Job job = (Job)sender;
      lock (this._JobList)
      {
        //Build the thumb & icon
        job.Clients.ForEach(client => this.buildThumb(job, client.SizeIconMax, client.SizeThumbMax));
        job.Done = true; //no more clients to add
        job.TimeStampKeepAlive = DateTime.Now;
      }

      //Execute callback to each client
      job.Clients.ForEach(client => client.SetComplete(job, false));
      job.Clients.Clear();
    }
    #endregion
  }  
}
