using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;


namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class ServiceAgents : Singleton<ServiceAgents>, IDisposable
  {
    private static string _hostname = Dns.GetHostName();

    private ServiceAgents()
    {
      AddServices();
    }

    ~ServiceAgents()
    {
      Dispose();
    }      

    private void AddServices()
    {     
      AddService<ICardService>();
      AddService<IProgramService>();
      AddService<IRecordingService>();
      AddService<IChannelGroupService>();
      AddService<ISettingService>();
      AddService<IChannelService>();
      AddService<IScheduleService>();
      AddService<ICanceledScheduleService>();
      AddService<IConflictService>();
      AddService<IProgramCategoryService>();      
      AddService<IControllerService>();

      AddEventService();
      AddDiscoveryService();
    }


    private T GetOrCreateServiceAgent<T>() where T : class
    {
      var discoverServiceAgent = GlobalServiceProvider.Get<T>();
      if (discoverServiceAgent == null)
      {
        AddService<T>();
        discoverServiceAgent = GlobalServiceProvider.Get<T>();
      }
      return discoverServiceAgent;
    }

    public IEventServiceAgent EventServiceAgent
    {
      get
      {
        var eventServiceAgent = GlobalServiceProvider.Get<IEventServiceAgent>();
        if (eventServiceAgent == null)
        {
          AddEventService();
          eventServiceAgent = GlobalServiceProvider.Get<IEventServiceAgent>();
        }
        return eventServiceAgent;
      }
    } 

    public IDiscoverServiceAgent DiscoverServiceAgent
    {
      get
      {
        var discoverServiceAgent = GlobalServiceProvider.Get<IDiscoverServiceAgent>();
        if (discoverServiceAgent == null)
        {
          AddDiscoveryService();
          discoverServiceAgent = GlobalServiceProvider.Get<IDiscoverServiceAgent>();
        }

        return discoverServiceAgent;
      }
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
        return GetOrCreateServiceAgent<IProgramService>();
      }
    }

    public IRecordingService RecordingServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<IRecordingService>();
      }
    }

    public IChannelGroupService ChannelGroupServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<IChannelGroupService>();
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
        return GetOrCreateServiceAgent<IChannelService>();
      }
    }

    public IScheduleService ScheduleServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<IScheduleService>();
      }
    }

    public ICardService CardServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<ICardService>();
      }
    }

    public ICanceledScheduleService CanceledScheduleServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<ICanceledScheduleService>();
      }
    }

    public IConflictService ConflictServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<IConflictService>();
      }
    }

    public IProgramCategoryService ProgramCategoryServiceAgent
    {
      get
      {
        return GetOrCreateServiceAgent<IProgramCategoryService>();
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
        IEventServiceAgent eventserviceagent = new EventServiceAgent(_hostname);

        ((IClientChannel)eventserviceagent.Channel).Faulted += new EventHandler(ServiceAgents_Faulted);
        ((IClientChannel)eventserviceagent.Channel).Closed += new EventHandler(ServiceAgents_Closed);

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

    public void AddService<I>() where I : class
    {
      bool found = GlobalServiceProvider.IsRegistered<I>();

      if (!String.IsNullOrEmpty(_hostname))
      {
        var binding = ServiceHelper.GetHttpBinding();
        var endpoint = new EndpointAddress(ServiceHelper.GetEndpointURL(typeof(I), _hostname));
        var channelFactory = new ChannelFactory<I>(binding, endpoint);
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
      RemoveService(sender);
    }

    private void RemoveService(object sender)
    {
      ((ICommunicationObject) sender).Abort();
      ((ICommunicationObject) sender).Close();

      ((ICommunicationObject) sender).Faulted -= new EventHandler(ServiceAgents_Faulted);
      ((ICommunicationObject) sender).Closed -= new EventHandler(ServiceAgents_Closed);

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
    }

    private void ServiceAgents_Faulted(object sender, EventArgs e)
    {
      RemoveService(sender);
    }    

    public I PluginService<I> ()
    {
      return GlobalServiceProvider.Get<I>();      
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {      
      DisposeServiceProxy<ICardService>();
      DisposeServiceProxy<ICardService>();
      DisposeServiceProxy<IProgramService>();
      DisposeServiceProxy<IRecordingService>();
      DisposeServiceProxy<IChannelGroupService>();
      DisposeServiceProxy<ISettingService>();
      DisposeServiceProxy<IChannelService>();
      DisposeServiceProxy<IScheduleService>();
      DisposeServiceProxy<ICanceledScheduleService>();
      DisposeServiceProxy<IConflictService>();
      DisposeServiceProxy<IProgramCategoryService>();
      DisposeServiceProxy<IControllerService>();
      DisposeServiceProxy<IEventService>();
      DisposeServiceProxy<IDiscoverService>();
    }

    private void DisposeServiceProxy<T>() where T : class
    {
      T service = GlobalServiceProvider.Get<T>();
      if (service != null)
      {
        ((IClientChannel) service).Faulted -= new EventHandler(ServiceAgents_Faulted);
        ((IClientChannel) service).Closed -= new EventHandler(ServiceAgents_Closed);
        ((IClientChannel) service).Close();
        ((IDisposable) service).Dispose();
        GlobalServiceProvider.Remove<T>();
      }
    }

    #endregion
  }
}
