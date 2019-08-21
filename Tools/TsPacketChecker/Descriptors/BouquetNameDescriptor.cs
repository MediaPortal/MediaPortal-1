using System;

namespace TsPacketChecker
{
  internal class BouquetNameDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private string _name;


    #endregion
    #region Constructor
    public BouquetNameDescriptor()
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
          throw (new InvalidOperationException("Bouquet Name Descriptor: Index requested before block processed"));
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
          _name = Utils.GetString(buffer, _lastIndex, DescriptorLength);
          _lastIndex += DescriptorLength;
        }
        
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Bouquet Name Descriptor message is short"));
      }
    }
    #endregion
  }
}