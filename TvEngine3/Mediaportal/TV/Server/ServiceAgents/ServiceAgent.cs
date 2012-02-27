using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public abstract class ServiceAgent<T> : IDisposable
  {
    protected T _channel;

    public event EventHandler ServiceAgentFaulted;
    public event EventHandler ServiceAgentClosed;

    protected ServiceAgent (string hostname)
    {
      if (!String.IsNullOrEmpty(hostname))
      {
        var binding = ServiceHelper.GetHttpBinding();
        var endpoint = new EndpointAddress(ServiceHelper.GetEndpointURL(typeof (T), hostname));

        var channelFactory = new ChannelFactory<T>(binding, endpoint);

        foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
        {
          var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
          if (dataContractBehavior != null)
          {
            dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
          }
        }
        _channel = channelFactory.CreateChannel();

        ((IClientChannel)_channel).Faulted += new EventHandler(ServiceAgent_Faulted);
        ((IClientChannel)_channel).Closed += new EventHandler(ServiceAgent_Closed);
      }
    }    

    public void Dispose()
    {
      ((IClientChannel)_channel).Dispose();
      ((IClientChannel)_channel).Faulted -= new EventHandler(ServiceAgent_Faulted);
      ((IClientChannel)_channel).Closed -= new EventHandler(ServiceAgent_Closed);
      ((IClientChannel)_channel).Close();
    }

    private void ServiceAgent_Closed(object sender, EventArgs e)
    {
      if (ServiceAgentClosed != null)
      {
        ServiceAgentClosed(sender, e);
      }
    }

    private void ServiceAgent_Faulted(object sender, EventArgs e)
    {
      if (ServiceAgentFaulted != null)
      {
        ServiceAgentFaulted(sender, e);
      }
    }
  }
}
