using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  public class HttpCachingEventArgs : EventArgs
  {
    public string FileFullPath;
    public object Tag;
  }
}
