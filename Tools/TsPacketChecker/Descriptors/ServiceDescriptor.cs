using System;

namespace TsPacketChecker
{
  internal class ServiceDescriptor : Descriptor
  {
    #region Private Fields
    private int _serviceType = -1;
    private string _providerName;
    private string _serviceName;
    private int _lastIndex = -1;
    #endregion
    #region Constructor
    public ServiceDescriptor()
    {
    }
    #endregion
    #region Properties
    /// <summary>
    /// Get the service type.
    /// </summary>
    public int ServiceType { get { return (_serviceType); } }

    /// <summary>
    /// Get the provider name.
    /// </summary>
    public string ProviderName { get { return (_providerName); } }

    /// <summary>
    /// Get the service name.
    /// </summary>
    public string ServiceName { get { return (_serviceName); } }
    #endregion   
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("ServiceDescriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _serviceType = (int)buffer[_lastIndex];
        _lastIndex++;
        int providerNameLength = (int)buffer[_lastIndex];
        _lastIndex++;
        if (providerNameLength != 0)
        {
          _providerName = Utils.GetString(buffer, _lastIndex, providerNameLength);
          _lastIndex += providerNameLength;
        }
        int serviceNameLength = (int)buffer[_lastIndex];
        _lastIndex++;
        if (serviceNameLength != 0)
        {
          _serviceName = Utils.GetString(buffer, _lastIndex, serviceNameLength);
          _lastIndex += serviceNameLength;
        }        
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The ServiceDescriptor message is short"));
      }
    }

    

    #endregion
  }
}