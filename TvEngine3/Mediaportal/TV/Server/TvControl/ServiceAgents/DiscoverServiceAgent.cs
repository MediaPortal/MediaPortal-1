using System;
using System.ServiceModel;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public interface IDiscoverServiceAgent : IDiscoverService
  {
    IDiscoverService Channel { get; }
  }

  public class DiscoverServiceAgent : IDiscoverServiceAgent
  {
    private readonly IDiscoverService _channel;


    /// <summary>
    /// EventServiceAgent
    /// </summary>
    /// <param name="hostname"></param>
    public DiscoverServiceAgent(string hostname)
    {
      BasicHttpBinding binding = ServiceHelper.GetHttpBinding();
      var timeout = new TimeSpan(0,0,0,2);
      binding.SendTimeout = timeout;
      binding.OpenTimeout = timeout;
      if (!String.IsNullOrEmpty(hostname))
      {
        var endpoint = new EndpointAddress(ServiceHelper.GetEndPointUrl(typeof(IDiscoverService), hostname));
        var channelFactory = new ChannelFactory<IDiscoverService>(binding, endpoint);
        _channel = channelFactory.CreateChannel();
      }
    }

    public IDiscoverService Channel
    {
      get { return _channel; }
    }

    #region Implementation of IDiscoverService

    public DateTime Ping()
    {
      return Channel.Ping();
    }

    #endregion
  }

  
}
