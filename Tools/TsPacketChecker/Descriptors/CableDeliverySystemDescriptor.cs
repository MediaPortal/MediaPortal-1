using System;

namespace TsPacketChecker
{
  internal class CableDeliverySystemDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _outerFec;
    private int _frequency;
    private int _modulation;
    private int _symbolRate;
    private int _innerFec;

    #endregion

    #region Constructor
    public CableDeliverySystemDescriptor()
    {
    }
    #endregion

    #region Properties
    public int OuterFec { get => _outerFec; set => _outerFec = value; }
    public int Frequency { get => _frequency; set => _frequency = value; }
    public int Modulation { get => _modulation; set => _modulation = value; }
    public int SymbolRate { get => _symbolRate; set => _symbolRate = value; }
    public int InnerFec { get => _innerFec; set => _innerFec = value; }
    #endregion

    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Cable Delivery System Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _frequency = Utils.ConvertBCDToInt(buffer, _lastIndex, 8);
        _lastIndex += 4;

        _outerFec = buffer[_lastIndex + 1] & 0x17;
        _lastIndex += 2;

        _modulation = (int)buffer[_lastIndex];
        _lastIndex++;

        _symbolRate = Utils.ConvertBCDToInt(buffer, _lastIndex, 7);
        _innerFec = buffer[_lastIndex + 3] & 0x17;
        _lastIndex += 4;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Cable Delivery System Descriptor message is short"));
      }
    }
    #endregion
  }
}