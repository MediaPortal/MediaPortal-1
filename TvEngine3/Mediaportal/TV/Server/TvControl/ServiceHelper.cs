using System;
using System.ServiceModel;
using System.Xml;

namespace Mediaportal.TV.Server.TVControl
{
  public static class ServiceHelper
  {
    public const int PortHttpService = 8000;
    public const int PortTcpService = 8001;
    private const int ReaderQuotasMaxDepth = Int32.MaxValue;
    private const int ReaderQuotasMaxStringContentLength = Int32.MaxValue;
    private const int ReaderQuotasMaxArrayLength = Int32.MaxValue;
    private const int ReaderQuotasMaxBytesPerRead = Int32.MaxValue;
    private const int ReaderQuotasMaxNameTableCharCount = Int32.MaxValue;
    private const int MaxBufferSize = Int32.MaxValue;
    private const int MaxReceivedMessageSize = Int32.MaxValue;
    private const string Tvservice = "/TVService/";
    private const BasicHttpSecurityMode HttpSecurityMode = BasicHttpSecurityMode.None;
    private const string NetTcpBindingName = "netTcpBinding";
    private const string DefaultBasicHttpBindingName = "defaultBasicHttpBinding";

    private static readonly TimeSpan TcpReceiveTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan TcpSendTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan TcpCloseTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan TcpOpenTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan TcpInactivityTimeout = new TimeSpan(0, 0, 0, 60);

    private static readonly TimeSpan HttpReceiveTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan HttpSendTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan HttpCloseTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan HttpOpenTimeout = new TimeSpan(0, 0, 0, 60);
    private static readonly TimeSpan HttpInactivityTimeout = new TimeSpan(0, 0, 0, 60);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static NetTcpBinding GetTcpBinding()
    {      
      var netTcpBinding = new NetTcpBinding
      {
        Name = NetTcpBindingName,
        MaxBufferSize = MaxBufferSize,
        MaxReceivedMessageSize = MaxReceivedMessageSize,
        ReceiveTimeout = TcpReceiveTimeout,
        SendTimeout = TcpSendTimeout,   
        //TransactionFlow = false,
        CloseTimeout = TcpCloseTimeout,
        OpenTimeout = TcpOpenTimeout
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
        Name = DefaultBasicHttpBindingName,
        MaxBufferSize = MaxBufferSize,
        MaxReceivedMessageSize = MaxReceivedMessageSize,
        ReceiveTimeout = HttpReceiveTimeout,
        SendTimeout = HttpSendTimeout,   
        Security =
          {
            Mode = HttpSecurityMode,
            Transport = { ClientCredentialType = HttpClientCredentialType.None }
          }
      };
      SetReaderQuotas(basicHttpBinding.ReaderQuotas);
      return basicHttpBinding;
    }

    private static void SetReaderQuotas (XmlDictionaryReaderQuotas readerQuotas)
    {      
      readerQuotas.MaxDepth = ReaderQuotasMaxDepth;
      readerQuotas.MaxStringContentLength = ReaderQuotasMaxStringContentLength;
      readerQuotas.MaxArrayLength = ReaderQuotasMaxArrayLength;
      readerQuotas.MaxBytesPerRead = ReaderQuotasMaxBytesPerRead;
      readerQuotas.MaxNameTableCharCount = ReaderQuotasMaxNameTableCharCount;
    }


    public static string GetEndpointURL(Type type, string hostName)
    {
      return @"http://" + hostName + ":" + PortHttpService + Tvservice + type.Name;
    }

    public static string GetTcpEndpointURL(Type type, string hostName)
    {
      return @"net.tcp://" + hostName + ":" + PortTcpService + Tvservice + type.Name;
    }
  }
}
