using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MediaPortal.Services
{
  public class ImageLoadEventArgs : EventArgs
  {
    public string Url = null;
    public string FilePath = null;
    public string FilePathThumb = null;
    public string FilePathIcon = null;
    public Size ImageSize = Size.Empty;
    public DateTime DownloadTimeStamp = DateTime.MinValue;
    public Threading.WorkState Status;
    public IAsyncResult AsyncResult;
    public object State = null;
  }
}
