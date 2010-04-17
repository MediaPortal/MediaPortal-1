using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public class DownloadItem
  {
    public string SourceUrl { get; set; }
    public string Destination { get; set; }
    public string TempDestination { get; set; }
  }
}
