using System;
using System.ServiceModel;
using System.Xml;

namespace Mediaportal.TV.Server.TVControl
{
  public static class ServiceHelper
  {
    public const int PORT_HTTP_SERVICE = 8000;

    // TODO Is the fact that this is a constant going to prevent multiple
    // clients running simultaneously on a single system?
    public const int PORT_TCP_SERVICE = 8001;

    private const int MAX_BUFFER_SIZE = Int32.MaxValue;
    private const int MAX_RECEIVED_MESSAGE_SIZE = Int32.MaxValue;
    private const string SERVICE_NAME = "/TvService/";

    // If necessary we can have different time-out periods for HTTP, TCP,
    // send, receive, open, close and inactivity. For now use the same period
    // for all.
    private static readonly TimeSpan TIME_OUT = new TimeSpan(0, 60, 0);

    public static NetTcpBinding GetTcpBinding()
    {
      var netTcpBinding = new NetTcpBinding
      {
        Name = "netTcpBinding",
        MaxBufferSize = MAX_BUFFER_SIZE,
        MaxReceivedMessageSize = MAX_RECEIVED_MESSAGE_SIZE,
        ReceiveTimeout = TIME_OUT,
        SendTimeout = TIME_OUT,
        //TransactionFlow = false,
        CloseTimeout = TIME_OUT,
        OpenTimeout = TIME_OUT
      };

      /*netTcpBinding.ReliableSession.Enabled = true;
      netTcpBinding.ReliableSession.Ordered = true;
      netTcpBinding.ReliableSession.InactivityTimeout = TcpInactivityTimeout;*/

      SetReaderQuotas(netTcpBinding.ReaderQuotas);
      return netTcpBinding;
    }

    public static BasicHttpBinding GetHttpBinding()
    {
      var basicHttpBinding = new BasicHttpBinding
      {
        Name = "defaultBasicHttpBinding",
        MaxBufferSize = MAX_BUFFER_SIZE,
        MaxReceivedMessageSize = MAX_RECEIVED_MESSAGE_SIZE,
        ReceiveTimeout = TIME_OUT,
        SendTimeout = TIME_OUT,
        Security =
          {
            Mode = BasicHttpSecurityMode.None,
            Transport = { ClientCredentialType = HttpClientCredentialType.None }
          }
      };
      SetReaderQuotas(basicHttpBinding.ReaderQuotas);
      return basicHttpBinding;
    }

    private static void SetReaderQuotas(XmlDictionaryReaderQuotas readerQuotas)
    {
      readerQuotas.MaxDepth = Int32.MaxValue;
      readerQuotas.MaxStringContentLength = Int32.MaxValue;
      readerQuotas.MaxArrayLength = Int32.MaxValue;
      readerQuotas.MaxBytesPerRead = Int32.MaxValue;
      readerQuotas.MaxNameTableCharCount = Int32.MaxValue;
    }

    public static string GetEndPointRootUrl()
    {
      return @"http://+:" + PORT_HTTP_SERVICE + SERVICE_NAME;
    }

    public static string GetEndPointUrl(Type type, string hostName)
    {
      return @"http://" + hostName + ":" + PORT_HTTP_SERVICE + SERVICE_NAME + type.Name;
    }

    public static string GetTcpEndPointUrl(Type type, string hostName)
    {
      return @"net.tcp://" + hostName + ":" + PORT_TCP_SERVICE + SERVICE_NAME + type.Name;
    }
  }
}