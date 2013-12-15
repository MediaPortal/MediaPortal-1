using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces.ServiceAgents;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{

  public class ServiceAgents : Singleton<ServiceAgents>, IDisposable
  {

    public delegate void ServiceAgentRemovedDelegate(Type service);
    private static string _hostname = Dns.GetHostName();

    private ServiceAgents()
    {
      AddServices();
    }

    ~ServiceAgents()
    {
      Dispose();
    }

    private void ReconnectServices()
    {
      GetOrCreateServiceAgent<ISettingService>();
      GetOrCreateServiceAgent<IControllerService>();


      // most WCF agents have a specific agent that does additional stuff like stripping away unneeded data before sending it over the wire.
      // this is often done on save related methods.
      GetOrCreateCustomServiceAgent<IProgramCategoryService, ProgramCategoryAgent>();
      GetOrCreateCustomServiceAgent<ICardService, CardServiceAgent>();
      GetOrCreateCustomServiceAgent<IProgramService, ProgramServiceAgent>();
      GetOrCreateCustomServiceAgent<IRecordingService, RecordingServiceAgent>();
      GetOrCreateCustomServiceAgent<IChannelGroupService, ChannelGroupServiceAgent>();
      GetOrCreateCustomServiceAgent<IChannelService, ChannelServiceAgent>();
      GetOrCreateCustomServiceAgent<IScheduleService, ScheduleServiceAgent>();
      GetOrCreateCustomServiceAgent<ICanceledScheduleService, CanceledScheduleServiceAgent>();
      GetOrCreateCustomServiceAgent<IConflictService, ConflictServiceAgent>();

      GetOrCreateEventServiceAgent();
      GetOrCreateDiscovererServiceAgent();

    }

    private void AddServices()
    {
      AddGenericService<ISettingService>();
      AddGenericService<IControllerService>();
      AddCustomService<IProgramCategoryService, ProgramCategoryAgent>();

      // most WCF agents have a specific agent that does additional stuff like stripping away unneeded data before sending it over the wire.
      // this is often done on save related methods.

      AddCustomService<ICardService, CardServiceAgent>();
      AddCustomService<IProgramService, ProgramServiceAgent>();
      AddCustomService<IRecordingService, RecordingServiceAgent>();
      AddCustomService<IChannelGroupService, ChannelGroupServiceAgent>();
      AddCustomService<IChannelService, ChannelServiceAgent>();
      AddCustomService<IScheduleService, ScheduleServiceAgent>();
      AddCustomService<ICanceledScheduleService, CanceledScheduleServiceAgent>();
      AddCustomService<IConflictService, ConflictServiceAgent>();

      AddEventService();
      AddDiscoveryService();
    }

    private void AddCustomService<TServiceInterface, TServiceImpl>()
      where TServiceImpl : ServiceAgent<TServiceInterface>, TServiceInterface
      where TServiceInterface : class
    {
      bool found = GlobalServiceProvider.IsRegistered<TServiceInterface>();

      TServiceInterface service = Activator.CreateInstance(typeof(TServiceImpl), new object[] { _hostname }) as TServiceImpl;

      if (service != null)
      {
        var serviceAgent = service as ServiceAgent<TServiceInterface>;
        if (serviceAgent != null)
        {
          serviceAgent.ServiceAgentFaulted += new EventHandler(ServiceAgents_Faulted);
          serviceAgent.ServiceAgentClosed += new EventHandler(ServiceAgents_Closed);

          if (found)
          {
            GlobalServiceProvider.Replace(service);
          }
          else
          {
            GlobalServiceProvider.Add(service);
          }
        }
      }
    }


    private TServiceInterface GetOrCreateCustomServiceAgent<TServiceInterface, TServiceImpl>()
      where TServiceImpl : ServiceAgent<TServiceInterface>, TServiceInterface
      where TServiceInterface : class
    {
      var customServiceAgent = GlobalServiceProvider.Get<TServiceInterface>();
      if (customServiceAgent == null)
      {
        AddCustomService<TServiceInterface, TServiceImpl>();
        customServiceAgent = GlobalServiceProvider.Get<TServiceInterface>();
      }
      return customServiceAgent;
    }



    private T GetOrCreateServiceAgent<T>() where T : class
    {
      var serviceAgent = GlobalServiceProvider.Get<T>();
      if (serviceAgent == null)
      {
        AddGenericService<T>();
        serviceAgent = GlobalServiceProvider.Get<T>();
      }
      return serviceAgent;
    }

    public IEventServiceAgent EventServiceAgent
    {
      get
      {
        return GetOrCreateEventServiceAgent();
      }
    }

    private IEventServiceAgent GetOrCreateEventServiceAgent()
    {
      var eventServiceAgent = GlobalServiceProvider.Get<IEventServiceAgent>();
      if (eventServiceAgent == null)
      {
        AddEventService();
        eventServiceAgent = GlobalServiceProvider.Get<IEventServiceAgent>();
      }
      return eventServiceAgent;
    }

    public IDiscoverServiceAgent DiscoverServiceAgent
    {
      get
      {
        return GetOrCreateDiscovererServiceAgent();
      }
    }

    private IDiscoverServiceAgent GetOrCreateDiscovererServiceAgent()
    {
      var discoverServiceAgent = GlobalServiceProvider.Get<IDiscoverServiceAgent>();
      if (discoverServiceAgent == null)
      {
        AddDiscoveryService();
        discoverServiceAgent = GlobalServiceProvider.Get<IDiscoverServiceAgent>();
      }

      return discoverServiceAgent;
    }

    public IControllerService ControllerServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<IControllerService>();
      }
    }

    public IProgramService ProgramServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IProgramService, ProgramServiceAgent>();
      }
    }

    public IRecordingService RecordingServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IRecordingService, RecordingServiceAgent>();
      }
    }

    public IChannelGroupService ChannelGroupServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IChannelGroupService, ChannelGroupServiceAgent>();
      }
    }

    public ISettingService SettingServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<ISettingService>();
      }
    }

    public IChannelService ChannelServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IChannelService, ChannelServiceAgent>();
      }
    }

    public IScheduleService ScheduleServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IScheduleService, ScheduleServiceAgent>();
      }
    }

    public ICardService CardServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<ICardService, CardServiceAgent>();
      }
    }

    public ICanceledScheduleService CanceledScheduleServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<ICanceledScheduleService, CanceledScheduleServiceAgent>();
      }
    }

    public IConflictService ConflictServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IConflictService, ConflictServiceAgent>();
      }
    }

    public IProgramCategoryService ProgramCategoryServiceAgent
    {
      get
      {
        return GetOrCreateCustomServiceAgent<IProgramCategoryService, ProgramCategoryAgent>();
      }
    }


    public string Hostname
    {
      get
      {
        return _hostname;
      }
      set
      {
        bool changed = value != _hostname;
        _hostname = value;

        if (changed)
        {
          Dispose();
          AddServices();
        }
      }
    }


    private void AddDiscoveryService()
    {
      if (!String.IsNullOrEmpty(_hostname))
      {
        bool found = GlobalServiceProvider.IsRegistered<IDiscoverServiceAgent>();
        IDiscoverServiceAgent discoverServiceAgent = new DiscoverServiceAgent(_hostname);

        ((IClientChannel)discoverServiceAgent.Channel).Faulted += new EventHandler(ServiceAgents_Faulted);
        ((IClientChannel)discoverServiceAgent.Channel).Closed += new EventHandler(ServiceAgents_Closed);

        if (found)
        {
          GlobalServiceProvider.Replace(discoverServiceAgent);
        }
        else
        {
          GlobalServiceProvider.Add(discoverServiceAgent);
        }
      }
    }

    private void AddEventService()
    {
      bool found = GlobalServiceProvider.IsRegistered<IEventServiceAgent>();
      if (!String.IsNullOrEmpty(_hostname))
      {
        IEventServiceAgent eventserviceagent = new EventServiceClient(_hostname);

        //((IClientChannel)eventserviceagent.Channel).Faulted += new EventHandler(ServiceAgents_Faulted);
        //((IClientChannel)eventserviceagent.Channel).Closed += new EventHandler(ServiceAgents_Closed);

        if (found)
        {
          GlobalServiceProvider.Replace(eventserviceagent);
        }
        else
        {
          GlobalServiceProvider.Add(eventserviceagent);
        }
      }
    }

    public void AddGenericService<I>() where I : class
    {
      bool found = GlobalServiceProvider.IsRegistered<I>();

      if (!String.IsNullOrEmpty(_hostname))
      {
        var binding = ServiceHelper.GetHttpBinding();
        var endpoint = new EndpointAddress(ServiceHelper.GetEndpointURL(typeof(I), _hostname));

        var channelFactory = new ChannelFactory<I>(binding, endpoint);

        foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
        {
          var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
          if (dataContractBehavior != null)
          {
            dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
          }
        }

        I channel = channelFactory.CreateChannel();

        ((IClientChannel)channel).Faulted += new EventHandler(ServiceAgents_Faulted);
        ((IClientChannel)channel).Closed += new EventHandler(ServiceAgents_Closed);

        if (found)
        {
          GlobalServiceProvider.Replace(channel);
        }
        else
        {
          GlobalServiceProvider.Add(channel);
        }
      }
    }

    private void ServiceAgents_Closed(object sender, EventArgs e)
    {
      this.LogDebug("ServiceAgents.ServiceAgents_Closed: {0}", sender.GetType());
      RemoveService(sender);
    }

    private void RemoveService(object sender)
    {
      var communicationObject = ((ICommunicationObject)sender);
      communicationObject.Abort();
      communicationObject.Close();

      communicationObject.Faulted -= new EventHandler(ServiceAgents_Faulted);
      communicationObject.Closed -= new EventHandler(ServiceAgents_Closed);

      Type type = sender.GetType();
      if (type == typeof(IEventService))
      {
        type = typeof(IEventServiceAgent);
      }
      else if (type == typeof(IDiscoverService))
      {
        type = typeof(IDiscoverServiceAgent);
      }

      bool found = GlobalServiceProvider.IsRegistered(type);

      if (found)
      {
        GlobalServiceProvider.Remove(type);
      }

      this.LogDebug("ServiceAgents.RemoveService: removed service:{0}", type);
    }

    private void ServiceAgents_Faulted(object sender, EventArgs e)
    {
      this.LogDebug("ServiceAgents.ServiceAgents_Faulted: {0}", sender.GetType());
      RemoveService(sender);
    }

    public I PluginService<I>()
    {
      return GlobalServiceProvider.Get<I>();
    }

    public void ReConnect()
    {
      ReconnectServices();
    }

    public void Disconnect()
    {
      DisposeCustomServiceProxy<ICardService>();
      DisposeCustomServiceProxy<ICardService>();
      DisposeCustomServiceProxy<IProgramService>();
      DisposeCustomServiceProxy<IRecordingService>();
      DisposeCustomServiceProxy<IChannelGroupService>();
      DisposeCustomServiceProxy<IChannelService>();
      DisposeCustomServiceProxy<IScheduleService>();
      DisposeCustomServiceProxy<ICanceledScheduleService>();
      DisposeCustomServiceProxy<IConflictService>();
      DisposeCustomServiceProxy<IProgramCategoryService>();

      DisposeGenericServiceProxy<ISettingService>();
      DisposeGenericServiceProxy<IControllerService>();

    }

    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      Disconnect();
      DisposeGenericServiceProxy<IEventService>();
      DisposeGenericServiceProxy<IDiscoverService>(); // we cant get rid of IDiscoverService, when calling disconnect as we depend on this to re-discover a server connection-
    }

    private void DisposeCustomServiceProxy<TServiceInterface>() where TServiceInterface : class
    {
      var service = GlobalServiceProvider.Get<TServiceInterface>();
      if (service != null)
      {
        var serviceAgent = service as ServiceAgent<TServiceInterface>;
        if (serviceAgent != null)
        {
          serviceAgent.ServiceAgentFaulted -= ServiceAgents_Faulted;
          serviceAgent.ServiceAgentClosed -= ServiceAgents_Closed;
        }
        IDisposable disposable = service as IDisposable;
        if (disposable != null)
          disposable.Dispose();
        GlobalServiceProvider.Remove<TServiceInterface>();
      }
    }

    private void DisposeGenericServiceProxy<T>() where T : class
    {
      T service = GlobalServiceProvider.Get<T>();
      if (service != null)
      {
        IClientChannel channel = service as IClientChannel;
        if (channel != null)
        {
          channel.Faulted -= ServiceAgents_Faulted;
          channel.Closed -= ServiceAgents_Closed;
          channel.Close();
        }
        IDisposable disposable = service as IDisposable;
        if (disposable != null)
          disposable.Dispose();
        GlobalServiceProvider.Remove<T>();
      }
    }

    #endregion
  }
}