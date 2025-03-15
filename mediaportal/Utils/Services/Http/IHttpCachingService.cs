using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Utils.Web;

namespace MediaPortal.Services
{
  public interface IHttpCachingService
  {
    /// <summary>
    /// Event is raised when cached file has been deleted
    /// </summary>
    event HttpCachingEventHandler DeleteEvent;

    /// <summary>
    /// Current cache full path
    /// </summary>
    string CachePath { get; }

    /// <summary>
    /// Clean http cache folder
    /// </summary>
    /// <param name="bAll">True: delete all files include non expired</param>
    void CleanUp(bool bAll);

    /// <summary>
    /// Download file based on url
    /// </summary>
    /// <param name="strUrl">URL</param>
    /// <param name="iLifeTime">Liftime of the cached file in minutes; 0 for no caching; -1 for default lifetime</param>
    /// <param name="postDownload">Post download callback</param>
    /// <param name="state">Post download user object</param>
    /// <returns>Cached fullpath filename</returns>
    string DownloadFile(string strUrl,
      int iLifeTime = -1,
      HttpCachingEventHandler postDownload = null,
      object state = null
      );


    /// <summary>
    /// Download file based on HTTPRequest.
    /// </summary>
    /// <param name="rq">URL request</param>
    /// <returns>Cached fullpath filename</returns>
    string DownloadFile(HTTPRequest rq);

    /// <summary>
    /// Download file based on HTTPRequest.
    /// </summary>
    /// <param name="rq">URL request</param>
    /// <param name="iLifeTime">Liftime of the cached file in minutes; 0 for no caching; -1 for default lifetime</param>
    /// <param name="postDownload">Post download callback</param>
    /// <param name="state">Post download user object</param>
    /// <returns>Cached fullpath filename</returns>
    string DownloadFile(HTTPRequest rq,
         int iLifeTime = -1,
         HttpCachingEventHandler postDownload = null,
         object state = null
         );
  }
}
