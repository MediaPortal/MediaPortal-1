using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Music.Database
{
  public class ScrobblerUtilsResponse
  {
    public readonly ScrobblerUtilsRequest Request;
    public object Response;

    public ScrobblerUtilsResponse(ScrobblerUtilsRequest request, object response)
    {
      Request = request;
      Response = response;
    }
  }
}
