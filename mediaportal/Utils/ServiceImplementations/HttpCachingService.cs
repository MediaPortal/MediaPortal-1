using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;
using MediaPortal.Services;
using MediaPortal.Utils.Web;

namespace MediaPortal.ServiceImplementations
{
  public class HttpCachingSevice : IHttpCachingService
  {
    public const string CACHE_FILENAME_EXT = "webcache";

    private const int _CLEAN_UP_PERIOD = 60 * 24; //[minutes]
    private const int _CACHE_FILES_LIFETIME = 60 * 24 * 7; //[minutes]
    private const int _CACHE_FILES_REFRESH = 60 * 24; //[minutes]
    
    private string _CachePath = null;
    private readonly List<string> _CacheRequests = new List<string>();
    private DateTime _LastCleanUp = DateTime.MinValue;

    private static readonly StringBuilder _SbHash = new StringBuilder(128);
    private static readonly System.Security.Cryptography.MD5 _Md5Hash = System.Security.Cryptography.MD5.Create();

    private readonly int _Instance_Id = -1;
    private static int _Instance_IdCounter = -1;

    private static readonly ILog _Logger = GlobalServiceProvider.Get<ILog>();

    public string CachePath
    {
      get
      {
        if (_CachePath == null)
        {
          this._CachePath = Path.Combine(Configuration.Config.GetFolder(Configuration.Config.Dir.Config), @".cache\http\");

          try
          {
            if (!Directory.Exists(this._CachePath))
              Directory.CreateDirectory(this._CachePath);
          }
          catch (Exception ex)
          {
            _Logger.Error("[{3}][CachePath] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace, this._Instance_Id);
          }

        }
        return this._CachePath;
      }
      set
      {
        if (!string.IsNullOrWhiteSpace(value))
        {
          if (!value.EndsWith("\\"))
            value += "\\";

          this._CachePath = value;

          try
          {
            if (!Directory.Exists(this._CachePath))
              Directory.CreateDirectory(_CachePath);

            _Logger.Debug("[HttpCaching][{0}][CachePath] {1}", this._Instance_Id, value);
          }
          catch (Exception ex)
          {
            _Logger.Error("[{3}][CachePath] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace, this._Instance_Id);
          }
        }
      }
    }

    public event HttpCachingEventHandler DeleteEvent;

    public HttpCachingSevice(string strDirectorPath)
    {
      this._Instance_Id = Interlocked.Increment(ref _Instance_IdCounter);
      this.CachePath = strDirectorPath;
    }

    /// <summary>
    /// Delete expired cached files
    /// </summary>
    /// <param name="bAll">True: delete all files include non expired</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void CleanUp(bool bAll)
    {
      if (bAll || (DateTime.Now - this._LastCleanUp).TotalMinutes >= _CLEAN_UP_PERIOD)
      {
        _Logger.Debug("[HttpCaching][{0}][CleanUp] CleanUp is in due.", this._Instance_Id);

        while (true)
        {
          lock (this._CacheRequests)
          {
            if (this._CacheRequests.Count == 0)
              break;
          }

          System.Threading.Thread.Sleep(100);
        }

        _Logger.Debug("[HttpCaching][{0}][CleanUp] Cleaning...", this._Instance_Id);

        DirectoryInfo di = new DirectoryInfo(this.CachePath);
        FileInfo[] fiList = di.GetFiles("*." + CACHE_FILENAME_EXT);
        foreach (FileInfo fi in fiList)
        {
          if (bAll || (DateTime.Now - fi.LastAccessTime).TotalMinutes >= _CACHE_FILES_LIFETIME)
          {
            try
            {
              _Logger.Debug("[HttpCaching][{0}][CleanUp] Deleting file: {1}", this._Instance_Id, fi.FullName);

              try
              {
                //Event
                if (this.DeleteEvent != null)
                  this.DeleteEvent(this, new HttpCachingEventArgs() { FileFullPath = fi.FullName });
              }
              catch (Exception ex)
              {
                _Logger.Error("[{3}][CleanUp] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace, this._Instance_Id);
              }

              File.Delete(fi.FullName);
            }
            catch (Exception ex)
            {
              _Logger.Error("[{3}][CleanUp] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace, this._Instance_Id);
            }
          }
        }

        this._LastCleanUp = DateTime.Now;
      }
    }

    /// <summary>
    /// Download file based on url
    /// </summary>
    /// <param name="strUrl"></param>
    /// <param name="iLifeTime">Liftime of the cached file in minutes; 0 for no caching; -1 for default lifetime</param>
    /// <param name="postDownload">Post download callback</param>
    /// <param name="state">Post download user object</param>
    /// <returns>Cached fullpath filename</returns>
    public string DownloadFile(string strUrl, int iLifeTime = -1,
      HttpCachingEventHandler postDownload = null, object state = null)
    {
      return this.DownloadFile(new HTTPRequest(strUrl), iLifeTime, postDownload, state);
    }

    /// <summary>
    /// Download file based on HTTPRequest.
    /// </summary>
    /// <param name="rq">URL request</param>
    /// <returns>Cached fullpath filename</returns>
    public string DownloadFile(HTTPRequest rq)
    {
      return this.DownloadFile(rq, -1, null, null);
    }

    /// <summary>
    /// Download file based on HTTPRequest.
    /// </summary>
    /// <param name="rq">URL request</param>
    /// <param name="iLifeTime">Liftime of the cached file in minutes; 0 for no caching; -1 for default lifetime</param>
    /// <param name="postDownload">Post download callback</param>
    /// <param name="state">Post download user object</param>
    /// <returns>Cached fullpath filename</returns>
    public string DownloadFile(HTTPRequest rq, int iLifeTime = -1,
          HttpCachingEventHandler postDownload = null,  object state = null)
    {
      if (rq == null || string.IsNullOrWhiteSpace(rq.Url))
        throw new ArgumentException(string.Format("[HttpCaching][{0}][DownloadFile] Both url and webrequest is null", this._Instance_Id));

      //if (string.IsNullOrWhiteSpace(strFilename))
      string strFilename = GetFileNameHash(rq.Url, !string.IsNullOrWhiteSpace(rq.PostQuery) ? Encoding.UTF8.GetBytes(rq.PostQuery) : null);

      FileInfo fi = null;

      this.CleanUp(false);

      //Sync of the same filenames

      string strCacheFullPath = this.CachePath + strFilename;

      lock (this._CacheRequests)
      {
        bool bInProgress = false;

        while (this._CacheRequests.Exists(p => p.Equals(strFilename)))
        {
          _Logger.Debug("[HttpCaching][{0}][DownloadFile] Wait: Url in the progress. '{1}'", this._Instance_Id, rq.Url);

          bInProgress = true;

          //Wait, url is in the progress now
          Monitor.Wait(this._CacheRequests);

          //Now we can check the existing request again
        }

        if (bInProgress)
        {
          int iAttempts = 5;
          while (iAttempts-- > 0)
          {
            //Another task has finished the download. No need to download the file again.
            if (File.Exists(strCacheFullPath))
            {
              _Logger.Debug("[HttpCaching][{0}][DownloadFile] Abort: Url already processed. '{1}'", this._Instance_Id, rq.Url);
              return strCacheFullPath;
            }

            Thread.Sleep(200);
          }
          _Logger.Error("[{0}][DownloadFile] Abort: Url already processed but does not exist. '{1}'", this._Instance_Id, rq.Url);
          return null;

        }
        else
          this._CacheRequests.Add(strFilename); //Add url to the progress list
      }

      try
      {
        //Load
        if (iLifeTime != 0 && File.Exists(strCacheFullPath))
        {
          fi = new FileInfo(strCacheFullPath);
          if (fi.Length > 0)
          {
            int iAge = (int)(DateTime.Now - fi.LastWriteTime).TotalMinutes;
            if (fi.CreationTime > DateTime.Now || iAge < (iLifeTime > 0 ? iLifeTime : _CACHE_FILES_REFRESH))
            {
              int iAttempts = 3;
              while (iAttempts-- > 0)
              {
                try
                {
                  fi.LastAccessTime = DateTime.Now;
                  _Logger.Debug("[HttpCaching][{0}][DownloadFile] Url request: '{1}' - Using cached file: '{2}'", this._Instance_Id, rq.Url, strCacheFullPath);
                  return strCacheFullPath;
                }
                catch (Exception ex) { }
                Thread.Sleep(200);
              }
              _Logger.Error("[{0}][DownloadFile] File Access: '{1}'", this._Instance_Id, strCacheFullPath);
              return strCacheFullPath;
            }
          }
        }

        //Download
        HTTPRequest wr = new HTTPRequest(rq.Url);
        if (fi != null)
          wr.ModifiedSince = fi.CreationTime;
          //wr.Headers = "If-Modified-Since=" + fi.CreationTime.ToUniversalTime().ToString("R");

        HTTPTransaction http = new HTTPTransaction();
        
        if (http.HTTPGet(wr, strCacheFullPath))
        {
          switch (http.StatusCode)
          {
            case HttpStatusCode.OK:
            case HttpStatusCode.NotModified:

              if (http.StatusCode == HttpStatusCode.OK && postDownload != null)
              {
                try
                {
                  postDownload(this, new HttpCachingEventArgs() { FileFullPath = strCacheFullPath, Tag = state });
                }
                catch { }
              }

              DateTime dtNow = DateTime.Now;
              DateTime dtLastModified = dtNow;
              if (DateTime.TryParse(http.ResponseHeaders["Last-Modified"], out DateTime dt) && dt > dtLastModified)
                dtLastModified = dt;

              if (DateTime.TryParse(http.ResponseHeaders["Expires"], out dt) && dt > dtNow)
                dtLastModified = dtNow.AddMinutes(Math.Min(_CACHE_FILES_LIFETIME, (dt - dtNow).TotalMinutes));

              //Try to set file last access time
              int iAttempts = 3;
              while (iAttempts-- > 0)
              {
                try
                {
                  fi = new FileInfo(strCacheFullPath);

                  if (dtLastModified > fi.CreationTime)
                    fi.CreationTime = dtLastModified;

                  fi.LastWriteTime = dtNow;
                  fi.LastAccessTime = dtNow;
                  return strCacheFullPath;
                }
                catch { }
                Thread.Sleep(200);
              }
              _Logger.Error("[{0}][DownloadFile] File Access: '{1}'", this._Instance_Id, strCacheFullPath);
              return strCacheFullPath;

            default:
              return null;
          }
        }
      }
      catch (Exception ex)
      {
        _Logger.Error("[{3}][DownloadFile] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace, this._Instance_Id);
      }
      finally
      {
        lock (this._CacheRequests)
        {
          //Remove url from the progress list
          this._CacheRequests.Remove(strFilename);

          //Awake waiting threads
          Monitor.PulseAll(this._CacheRequests);
        }
      }

      return null;
    }

    /// <summary>
    /// Get filename hash based on url
    /// </summary>
    /// <param name="strUrl"></param>
    /// <returns>Filename hash</returns>
    public static string GetFileNameHash(string strUrl)
    {
      if (string.IsNullOrWhiteSpace(strUrl))
        return string.Empty;
      return GetFileNameHash(new Uri(strUrl), null);
    }

    /// <summary>
    /// Get filename hash based on url
    /// </summary>
    /// <param name="strUrl"></param>
    /// <param name="data">Additional data to final hash. Can be null.</param>
    /// <returns>Filename hash</returns>
    public static string GetFileNameHash(string strUrl, byte[] data)
    {
      if (string.IsNullOrWhiteSpace(strUrl))
        return string.Empty;
      return GetFileNameHash(new Uri(strUrl), data);
    }

    /// <summary>
    /// Get filename hash based on uri
    /// </summary>
    /// <param name="uri"></param>
    /// <returns>Filename hash</returns>
    public static string GetFileNameHash(Uri uri)
    {
      return GetFileNameHash(uri, null);
    }

    /// <summary>
    /// Get filename hash based on uri
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="data">Additional data to final hash. Can be null.</param>
    /// <returns>Filename hash</returns>
    public static string GetFileNameHash(Uri uri, byte[] data)
    {
      byte[] toHash = Encoding.ASCII.GetBytes(uri.AbsoluteUri);
      if (data != null && data.Length > 0)
      {
        //Concat uri & data
        byte[] tmp = new byte[toHash.Length + data.Length];
        Buffer.BlockCopy(toHash, 0, tmp, 0, toHash.Length);
        Buffer.BlockCopy(data, 0, tmp, toHash.Length, data.Length);
        toHash = tmp;
      }

      //Create hash string
      lock (_Md5Hash)
      {
        _SbHash.Clear();
        _SbHash.Append(uri.Host);
        _SbHash.Append('_');

        PrintHash(_SbHash, _Md5Hash.ComputeHash(toHash));

        //Append file extension
        _SbHash.Append('.');
        _SbHash.Append(CACHE_FILENAME_EXT);

        return _SbHash.ToString();
      }
    }

    /// <summary>
    /// Print hash data in hex format
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="hash"></param>
    public static void PrintHash(StringBuilder sb, byte[] hash)
    {
      //Print hash
      byte b;
      int iVal;
      for (int i = 0; i < hash.Length; i++)
      {
        b = hash[i];

        iVal = b >> 4;
        iVal += iVal >= 10 ? 87 : 48;
        sb.Append((char)iVal);

        iVal = b & 0x0F;
        iVal += iVal >= 10 ? 87 : 48;
        sb.Append((char)iVal);
      }
    }
  }
}

