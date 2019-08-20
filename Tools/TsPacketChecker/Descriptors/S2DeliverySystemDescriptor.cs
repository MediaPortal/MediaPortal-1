using System;

namespace WindowsApplication13
{
  internal class S2DeliverySystemDescriptor : SatelliteDeliverySystemDescriptor
  {
    #region Private Fields
    private int _lastIndex = -1;


    #endregion

    #region Constructor
    public S2DeliverySystemDescriptor()
    {
    }
    #endregion

    #region Properties
    #endregion

    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("S2 Delivery System Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {

        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The S2 Delivery System Descriptor message is short"));
      }
    }
    #endregion
  }
}