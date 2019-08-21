using System;

namespace TsPacketChecker
{
  internal class SatelliteDeliverySystemDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _symbolRate;
    private int _innerFec;
    private bool _eastFlag;
    private int _polarization;
    private int _rollOff;
    private int _modulationSystem;
    private int _modulationType;
    private int _orbitalPosition;
    private int _frequency;
    #endregion

    #region Constructor
    public SatelliteDeliverySystemDescriptor()
    {
    }
    #endregion

    #region Properties
    public int SymbolRate { get => _symbolRate; set => _symbolRate = value; }
    public int InnerFec { get => _innerFec; set => _innerFec = value; }
    public bool EastFlag { get => _eastFlag; set => _eastFlag = value; }
    public int Polarization { get => _polarization; set => _polarization = value; }
    public int RollOff { get => _rollOff; set => _rollOff = value; }
    public int ModulationSystem { get => _modulationSystem; set => _modulationSystem = value; }
    public int ModulationType { get => _modulationType; set => _modulationType = value; }
    public int OrbitalPosition { get => _orbitalPosition; set => _orbitalPosition = value; }
    public int Frequency { get => _frequency; set => _frequency = value; }
    #endregion

    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Satellite Delivery System Descriptor: Index requested before block processed"));
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

        _orbitalPosition = Utils.ConvertBCDToInt(buffer, _lastIndex, 4);
        _lastIndex += 2;

        _eastFlag = ((buffer[_lastIndex] & 0x80) != 0);
        _polarization = (buffer[_lastIndex] >> 5) & 0x03;
        _rollOff = (buffer[_lastIndex] >> 3) & 0x03;
        _modulationSystem = ((buffer[_lastIndex] & 0x04) >> 2);
        _modulationType = buffer[_lastIndex] & 0x03;

        _lastIndex++;

        _symbolRate = Utils.ConvertBCDToInt(buffer, _lastIndex, 7);
        _innerFec = buffer[_lastIndex + 3] & 0x17;
        _lastIndex += 4;        
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Satellite Delivery Descriptor message is short"));
      }
    }
    #endregion
  }
}