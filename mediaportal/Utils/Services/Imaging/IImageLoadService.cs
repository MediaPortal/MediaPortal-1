using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MediaPortal.Services
{
  public interface IImageLoadService
  {
    /// <summary>
    /// Add new download task.
    /// </summary>
    /// <param name="strUrl">Url of the file to be downloaded</param>
    /// <param name="sizeIconMax">Create icon if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <param name="sizeThumbMax">Create thumb if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <returns>The reference to the pending asynchronous request.</returns>
    IAsyncResult BeginDownload(string strUrl, Size sizeIconMax, Size sizeThumbMax);


    /// <summary>
    /// Add new download task.
    /// </summary>
    /// <param name="strUrl">Url of the file to be downloaded</param>
    /// <param name="sizeIconMax">Create icon if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <param name="sizeThumbMax">Create thumb if url image excedes given size. Use Size.Empty if not specified.</param>
    /// <param name="iLifeTime">Lifetime of the cached file in minutes. <1 for default</param>
    /// <param name="userCallback">Optional callback to be executed upon task completation</param>
    /// <param name="stateObject">Optional user object passed to the callback</param>
    /// <returns>The reference to the pending asynchronous request.</returns>
    IAsyncResult BeginDownload(string strUrl, Size sizeIconMax, Size sizeThumbMax, int iLifeTime, ImageLoadEventHandler userCallback, object stateObject);

    /// <summary>
    /// Get result of the operation
    /// </summary>
    /// <param name="ar">The reference to the pending asynchronous request.</param>
    /// <returns>Result of the operation.</returns>
    bool EndDownload(IAsyncResult ar);
  }
}
