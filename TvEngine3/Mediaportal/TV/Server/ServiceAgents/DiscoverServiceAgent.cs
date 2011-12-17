using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
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
      var endpoint = new EndpointAddress(ServiceHelper.GetEndpointURL(typeof(IDiscoverService), hostname));      
      var channelFactory = new ChannelFactory<IDiscoverService>(binding, endpoint);
      _channel = channelFactory.CreateChannel();
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
