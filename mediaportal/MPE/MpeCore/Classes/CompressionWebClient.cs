using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MpeCore.Classes
{
  public class CompressionWebClient : WebClient
  {
    protected override WebRequest GetWebRequest(Uri address)
    {
      Headers["Accept-Encoding"] = "gzip,deflate";
      HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
      request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
      return request;
    }
  }
}
