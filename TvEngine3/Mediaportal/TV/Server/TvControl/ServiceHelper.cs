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

    public static NetTcpBinding GetTcpBinding()
    {
      var defaultBinding = new NetTcpBinding
      {
        Name = "netTcpBinding",
        MaxBufferSize = Int32.MaxValue,
        MaxReceivedMessageSize = Int32.MaxValue,
        ReaderQuotas =
        {
          MaxDepth = 32,
          MaxStringContentLength = Int32.MaxValue,
          MaxArrayLength = Int32.MaxValue,
          MaxBytesPerRead = Int32.MaxValue,
          MaxNameTableCharCount = Int32.MaxValue
        },
      };
      return defaultBinding;
    }

    public static BasicHttpBinding GetHttpBinding()
    {
      var defaultBinding = new BasicHttpBinding
                             {                               
                               Name = "defaultBasicHttpBinding",
                               MaxBufferSize = Int32.MaxValue,
                               MaxReceivedMessageSize = Int32.MaxValue,
                               ReaderQuotas =
                                 {
                                   MaxDepth = 32,
                                   MaxStringContentLength = Int32.MaxValue,
                                   MaxArrayLength = Int32.MaxValue,
                                   MaxBytesPerRead = Int32.MaxValue,
                                   MaxNameTableCharCount = Int32.MaxValue
                                 },
                               Security =
                                 {
                                   Mode = BasicHttpSecurityMode.None,
                                   Transport = { ClientCredentialType = HttpClientCredentialType.None }
                                 }
                             };
      return defaultBinding;
    }

    public static string GetEndpointURL(Type type, string hostName)
    {
      return @"http://" + hostName + ":" + PortHttpService + "/TVService/" + type.Name;
    }

    public static string GetTcpEndpointURL(Type type, string hostName)
    {
      return @"net.tcp://" + hostName + ":" + PortTcpService + "/TVService/" + type.Name;
    }
  }
}
