using System;
using System.Collections.ObjectModel;

namespace TsPacketChecker
{
  internal class ServiceAvailabilityDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private bool _vailabilityFlag;
    private Collection<int> _cells;
    #endregion
    #region Constructor
    public ServiceAvailabilityDescriptor()
    {
    }
    #endregion
    #region Properties
    public Collection<int> Cells { get => _cells; set => _cells = value; }
    public bool VailabilityFlag { get => _vailabilityFlag; set => _vailabilityFlag = value; }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Service Availability Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }



    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;

      try
      {
        if (DescriptorLength != 0)
        {
          _vailabilityFlag = (buffer[_lastIndex] & 0x80) != 0;
          _lastIndex++;

          while (_lastIndex < index + DescriptorLength)
          {
            if (_cells == null)
              _cells = new Collection<int>();

            _cells.Add(Utils.Convert2BytesToInt(buffer, _lastIndex));

            _lastIndex += 2;
          }
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Service Availability Descriptor message is short"));
      }
    } 
    #endregion
  }
}