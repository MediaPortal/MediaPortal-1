using System;
using System.Collections.ObjectModel;

namespace WindowsApplication13
{
  internal class ServiceListDescriptor : Descriptor
  {
    #region Private Fields
    private Collection<ServiceListItem> _serviceList;
    private int _lastIndex = -1;
    #endregion
    #region Constructor
    /// <summary>
    /// Initialize a new instance of the DVBServiceListDescriptor class.
    /// </summary>
    internal ServiceListDescriptor() { }
    #endregion
    #region Properties
    /// <summary>
    /// Get the collection of services.
    /// </summary>
    public Collection<ServiceListItem> ServiceList { get { return (_serviceList); } }
    #endregion
    #region Overrides
    /// <summary>
    /// Get the index of the next byte in the section following this descriptor.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The descriptor has not been processed.
    /// </exception> 
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("ServiceList Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    /// <summary>
    /// Parse the descriptor.
    /// </summary>
    /// <param name="buffer">The MPEG2 section containing the descriptor.</param>
    /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        if (DescriptorLength != 0)
        {
          _serviceList = new Collection<ServiceListItem>();
          int length = DescriptorLength;
          while (length > 0)
          {
            int serviceID = Utils.Convert2BytesToInt(buffer, _lastIndex);
            _lastIndex += 2;
            int serviceType = (int)buffer[_lastIndex];
            _lastIndex++;
            _serviceList.Add(new ServiceListItem(serviceID, serviceType));
            length -= 3;
          }
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Service List Descriptor message is short"));
      }
    }
    #endregion    
  }
  internal class ServiceListItem
  {
    #region Private Fields
    private readonly int _serviceID;
    private readonly int _serviceType;
    #endregion
    #region Constructor
    public ServiceListItem() { }

    /// <summary>
    /// Initialize a new instance of the ServiceListEntry class.
    /// </summary>
    /// <param name="serviceID">The service identification.</param>
    /// <param name="serviceType">The type of service (EN 300 468 table 81).</param>
    public ServiceListItem(int serviceID, int serviceType)
    {
      _serviceID = serviceID;
      _serviceType = serviceType;
    }
    #endregion
    #region Properties
    /// <summary>
    /// Get the service identification.
    /// </summary>
    public int ServiceID { get { return (_serviceID); } }
    /// <summary>
    /// Get the service type.
    /// </summary>
    public int ServiceType { get { return (_serviceType); } }
    #endregion
  }
}