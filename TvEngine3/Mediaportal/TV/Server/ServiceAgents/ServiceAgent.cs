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

    public event EventHandler ServiceAgentRemoved;
    public event EventHandler ServiceAgentFaulted;
    public event EventHandler ServiceAgentClosed;

    protected ServiceAgent ()
    {
      
    }

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

    public virtual void Dispose()
    {
      var clientChannel = _channel as IClientChannel;            
      if (clientChannel == null)
      {
        return;
      }

      clientChannel.Faulted -= new EventHandler(ServiceAgent_Faulted);
      clientChannel.Closed -= new EventHandler(ServiceAgent_Closed);

      try
      {
        if (clientChannel.State != CommunicationState.Faulted)
        {
          // we need this timeout, otherwise the call to 'Close' will block until any ongoing parallel WCF calls are still active          
          // so instead of having to wait for the default timeout of 1min, we instead wait 1 sec, before giving up and instead calls 'Abort'
          var timeout = new TimeSpan(0,0,0,1); 
          clientChannel.Close(timeout);
        }
        else
        {
          clientChannel.Abort();
        }
      }
      catch (CommunicationException ce)
      {
        clientChannel.Abort();
      }
      catch (TimeoutException toe)
      {
        clientChannel.Abort();
      }
      catch (Exception e)
      {
        clientChannel.Abort();        
      }
      finally
      {
        clientChannel.Dispose();
        clientChannel = null;
      }
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
