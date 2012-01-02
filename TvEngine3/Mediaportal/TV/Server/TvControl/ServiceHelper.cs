using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Mediaportal.TV.Server.TVControl
{
  public static class ServiceHelper
  {
    public static int PortHttpService = 8000;
    public static int PortTcpService = 8001;
    private const int MAX_BUFFER_SIZE = Int32.MaxValue;
    private const string TVSERVICE = "/TVService/";
    private const BasicHttpSecurityMode HTTP_SECURITY_MODE = BasicHttpSecurityMode.None;

    public static NetTcpBinding GetTcpBinding()
    {
      var defaultBinding = new NetTcpBinding
      {
        Name = "netTcpBinding",
        MaxBufferSize = MAX_BUFFER_SIZE,
        MaxReceivedMessageSize = MAX_BUFFER_SIZE,
        ReaderQuotas =
        {
          MaxDepth = MAX_BUFFER_SIZE,
          MaxStringContentLength = MAX_BUFFER_SIZE,
          MaxArrayLength = MAX_BUFFER_SIZE,
          MaxBytesPerRead = MAX_BUFFER_SIZE,
          MaxNameTableCharCount = MAX_BUFFER_SIZE
        },
      };
      return defaultBinding;
    }

    public static BasicHttpBinding GetHttpBinding()
    {
      var defaultBinding = new BasicHttpBinding
                             {
                               Name = "defaultBasicHttpBinding",
                               MaxBufferSize = MAX_BUFFER_SIZE,
                               MaxReceivedMessageSize = MAX_BUFFER_SIZE,
                               ReaderQuotas =
                                 {
                                   MaxDepth = MAX_BUFFER_SIZE,
                                   MaxStringContentLength = MAX_BUFFER_SIZE,
                                   MaxArrayLength = MAX_BUFFER_SIZE,
                                   MaxBytesPerRead = MAX_BUFFER_SIZE,
                                   MaxNameTableCharCount = MAX_BUFFER_SIZE
                                 },
                               Security =
                                 {
                                   Mode = HTTP_SECURITY_MODE,
                                   Transport = { ClientCredentialType = HttpClientCredentialType.None }
                                 }
                             };
      return defaultBinding;
    }

    public static string GetEndpointURL(Type type, string hostName)
    {
      return @"http://" + hostName + ":" + PortHttpService + TVSERVICE + type.Name;
    }

    public static string GetTcpEndpointURL(Type type, string hostName)
    {
      return @"net.tcp://" + hostName + ":" + PortTcpService + TVSERVICE + type.Name;
    }
  }
}
