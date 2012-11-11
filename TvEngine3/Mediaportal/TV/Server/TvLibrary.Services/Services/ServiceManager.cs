using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ServiceManager : Singleton<ServiceManager>, IDisposable
  {


    private readonly object _lock = new object();    
    private readonly IDictionary<Type, ServiceHost> _serviceHosts = new Dictionary<Type, ServiceHost>();

    ~ServiceManager()
    {
      Dispose();      
    }

    private ServiceManager ()
    {
      Init();
    }   

    private void Init()
    {
      try
      {
        AddService<IProgramCategoryService, ProgramCategoryService>();
        AddService<IConflictService, ConflictService>();
        AddService<ICardService, CardService>();
        AddService<ICanceledScheduleService, CanceledScheduleService>();
        AddService<IChannelService, ChannelService>();
        AddService<IProgramService, ProgramService>();
        AddService<ISettingService, SettingService>();
        AddService<IRecordingService, RecordingService>();
        AddService<IScheduleService, ScheduleService>();
        AddService<IChannelGroupService, ChannelGroupService>();
        AddService<IControllerService, TvControllerService>();

        GlobalServiceProvider.Add<IInternalControllerService>(new TvController());

        AddEventService<IEventService, EventService>();
        AddService<IDiscoverService, DiscoverService>();
      }
      catch (Exception ex)
      {
        this.LogError("ServiceManager: exception - {0}", ex);
      }      
    }  

    private void AddEventService<I, T>() where T : class, I, new()
    {
      Type implType = typeof(T);
      Type interfaceType = typeof(I);
      ThrowExceptionIfServiceAlreadyAdded(implType);
      ServiceHost serviceHost = GetEventServiceHost(interfaceType, implType);
      lock (_lock)
      {
        var service = new T();
        GlobalServiceProvider.Add<I>(service);
        _serviceHosts.Add(implType, serviceHost);
      }
    }

    private ServiceHost GetServiceHost(Type interfaceType, Type implType)
    {
      var serviceHost = new ServiceHost(implType);
      BasicHttpBinding defaultBinding = ServiceHelper.GetHttpBinding();
      string endpointUrl = ServiceHelper.GetEndpointURL(interfaceType, "localhost");
      var httpUri = new Uri(endpointUrl);

      var endpointAddress = new EndpointAddress(httpUri, EndpointIdentity.CreateSpnIdentity("localhost"));
      ContractDescription contract = ContractDescription.GetContract(implType);
      var endpoint = new ServiceEndpoint(contract, defaultBinding, endpointAddress);
      serviceHost.AddServiceEndpoint(endpoint);

      var serviceDebugBehaviour = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
      serviceDebugBehaviour.IncludeExceptionDetailInFaults = true;

      ServiceMetadataBehavior serviceMetaDataBehaviour = SetHttpsGetEnabled(serviceHost);
      serviceMetaDataBehaviour.HttpGetUrl = httpUri;
      serviceHost.Open();
      SetMaxItemsInObjectGraph(serviceHost);
      this.LogDebug("Service '{0}' succesfully started.", endpointUrl);
      return serviceHost;
    }

    private ServiceHost GetEventServiceHost(Type interfaceType, Type implType)
    {
      var serviceHost = new ServiceHost(implType);
      NetTcpBinding defaultBinding = ServiceHelper.GetTcpBinding();
      string endpointUrl = ServiceHelper.GetTcpEndpointURL(interfaceType, "localhost");
      var tcpUri = new Uri(endpointUrl);

      var endpointAddress = new EndpointAddress(tcpUri, EndpointIdentity.CreateSpnIdentity("localhost"));
      ContractDescription contract = ContractDescription.GetContract(implType);
      var endpoint = new ServiceEndpoint(contract, defaultBinding, endpointAddress);
      serviceHost.AddServiceEndpoint(endpoint);

      var serviceDebugBehaviour = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
      serviceDebugBehaviour.IncludeExceptionDetailInFaults = true;

      ServiceMetadataBehavior serviceMetaDataBehaviour = SetHttpsGetEnabled(serviceHost);

      string endpointHTTPUrl = ServiceHelper.GetEndpointURL(interfaceType, "localhost");
      var httpUri = new Uri(endpointHTTPUrl);
      serviceMetaDataBehaviour.HttpGetUrl = httpUri;

      var throttle = new ServiceThrottlingBehavior { MaxConcurrentCalls = 3, MaxConcurrentInstances = 20 };
      throttle.MaxConcurrentInstances = 20;
      serviceHost.Description.Behaviors.Add(throttle);

      serviceHost.Open();
      SetMaxItemsInObjectGraph(serviceHost);
      this.LogDebug("Service '{0}' succesfully started.", endpointUrl);
      return serviceHost;
    }

    private static ServiceMetadataBehavior SetHttpsGetEnabled(ServiceHost serviceHost)
    {
      var serviceMetaDataBehaviour = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
      if (serviceMetaDataBehaviour == null)
      {
        serviceMetaDataBehaviour = new ServiceMetadataBehavior {HttpGetEnabled = true};
        serviceHost.Description.Behaviors.Add(serviceMetaDataBehaviour);
      }
      else
      {
        serviceMetaDataBehaviour.HttpsGetEnabled = true;
      }
      return serviceMetaDataBehaviour;
    }

  

    private static void SetMaxItemsInObjectGraph(ServiceHost serviceHost)
    {
      foreach (OperationDescription operation in serviceHost.Description.Endpoints[0].Contract.Operations)
      {
        var dataContractBehavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
        if (dataContractBehavior != null)
        {
          dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
        }
      }
    }

    public IEventService EventService
    {
      get { return GlobalServiceProvider.Get<IEventService>(); }
    }

    public ICardService CardService
    {
      get { return GlobalServiceProvider.Get<ICardService>(); }
    }

    public IProgramService ProgramService
    {
      get { return GlobalServiceProvider.Get<IProgramService>(); }
    }

    public IRecordingService RecordingService
    {
      get { return GlobalServiceProvider.Get<IRecordingService>(); }
    }

    public IChannelGroupService ChannelGroupService
    {
      get { return GlobalServiceProvider.Get<IChannelGroupService>(); }
    }

    public ISettingService SettingService
    {
      get { return GlobalServiceProvider.Get<ISettingService>(); }
    }

    public IChannelService ChannelService
    {
      get { return GlobalServiceProvider.Get<IChannelService>(); }
    }

    public IScheduleService ScheduleService
    {
      get { return GlobalServiceProvider.Get<IScheduleService>(); }
    }

    public ICanceledScheduleService CanceledScheduleService
    {
      get { return GlobalServiceProvider.Get<ICanceledScheduleService>(); }
    }

    public IConflictService ConflictService
    {
      get { return GlobalServiceProvider.Get<IConflictService>(); }
    }

    public IProgramCategoryService ProgramCategoryService
    {
      get { return GlobalServiceProvider.Get<IProgramCategoryService>(); }
    }    

    public IInternalControllerService InternalControllerService
    {
      get { return GlobalServiceProvider.Get<IInternalControllerService>(); }
    }

    public IControllerService ControllerService
    {
      get { return GlobalServiceProvider.Get<IControllerService>(); }
    }

    public void AddService(Type interfaceType, object instance)
    {
      Type implType = instance.GetType();      
      ThrowExceptionIfServiceAlreadyAdded(implType);
      ServiceHost serviceHost = GetServiceHost(interfaceType, implType);
      lock (_lock)
      {        
        GlobalServiceProvider.Add(interfaceType, instance);
        _serviceHosts.Add(implType, serviceHost);
      }
    }

    public void AddService<I, T>() where T : class, I, new()
    {
      Type implType = typeof (T);
      Type interfaceType = typeof (I);
      ThrowExceptionIfServiceAlreadyAdded(implType);
      ServiceHost serviceHost = GetServiceHost(interfaceType, implType);
      lock (_lock)
      {
        var service = new T();
        GlobalServiceProvider.Add<I>(service);
        _serviceHosts.Add(implType, serviceHost);
      }
    }

    

    private void ThrowExceptionIfServiceAlreadyAdded(Type contractType)
    {
      lock (_lock)
      {
        if (_serviceHosts.ContainsKey(contractType))
        {
          throw new InvalidOperationException(contractType.Name + " already added to service.");
        }
      }
    }
   

   
    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      this.LogDebug("closing WCF service.");
      try
      {
        Services.EventService.CleanUp();
        foreach (ServiceHost host in _serviceHosts.Values)
        {          
          if (host != null)
          {
            host.Close();            
          }
        }
        _serviceHosts.Clear();
        this.LogDebug("WCF service(s) closed.");
      }
      catch (Exception ex)
      {
        this.LogError("error closing WCF service", ex);
        throw;
      }      
    }

    #endregion
  }
}
