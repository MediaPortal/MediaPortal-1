using System;

namespace TsPacketChecker
{
  internal class TerrestrialDeliverySystemDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private int _frequency;
    private int _bandWidth;
    private bool _priorityFlag;
    private bool _timeSliceIndicator;
    private bool _fecIndicator;
    private int _constellation;
    private int _hierarchyInformation;
    private int _hpCodeRate;
    private int _lpCodeRate;
    private int _guardInterval;
    private int _transmissionMode;
    private bool _otherFrequencyFlag;
    #endregion

    #region Constructor
    public TerrestrialDeliverySystemDescriptor()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Get the Frequency.
    /// </summary>
    public int Frequency { get { return (_frequency); } }
    /// <summary>
    /// Get the bandwidth.
    /// </summary>
    public int Bandwidth { get { return (_bandWidth); } }

    /// <summary>
    /// Get the priority.
    /// </summary>
    public bool PriorityFlag { get { return (_priorityFlag); } }

    /// <summary>
    /// Get the time slice indicator.
    /// </summary>
    public bool TimeSliceIndicator { get { return (_timeSliceIndicator); } }

    /// <summary>
    /// Get the MPE-FEC indicator.
    /// </summary>
    public bool FECIndicator { get { return (_fecIndicator); } }

    /// <summary>
    /// Get the constellation.
    /// </summary>
    public int Constellation { get { return (_constellation); } }

    /// <summary>
    /// Get the hierarchy information.
    /// </summary>
    public int HierarchyInformation { get { return (_hierarchyInformation); } }

    /// <summary>
    /// Get the HP stream code rate.
    /// </summary>
    public int HPCodeRate { get { return (_hpCodeRate); } }

    /// <summary>
    /// Get the LP stream code rate.
    /// </summary>
    public int LPCodeRate { get { return (_lpCodeRate); } }

    /// <summary>
    /// Get the guard interval.
    /// </summary>
    public int GuardInterval { get { return (_guardInterval); } }

    /// <summary>
    /// Get the transmission mode.
    /// </summary>
    public int TransmissionMode { get { return (_transmissionMode); } }

    /// <summary>
    /// Get the other frequency flag.
    /// </summary>
    public bool OtherFrequencyFlag { get { return (_otherFrequencyFlag); } }
    #endregion

    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Terrestrial Delivery System Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        _frequency = Utils.Convert4BytesToInt(buffer, _lastIndex);
        _lastIndex += 4;

        _bandWidth = buffer[_lastIndex] >> 5;
        _priorityFlag = ((buffer[_lastIndex] & 0x10) != 0);
        _timeSliceIndicator = ((buffer[_lastIndex] & 0x08) != 0);
        _fecIndicator = ((buffer[_lastIndex] & 0x04) != 0);
        _lastIndex++;

        _constellation = buffer[_lastIndex] >> 6;
        _hierarchyInformation = (buffer[_lastIndex] >> 3) & 0x07;
        _hpCodeRate = buffer[_lastIndex] & 0x07;
        _lastIndex++;

        _lpCodeRate = buffer[_lastIndex] & 0xd0;
        _guardInterval = (buffer[_lastIndex] >> 3) & 0x03;
        _transmissionMode = (buffer[_lastIndex] >> 1) & 0x03;
        _otherFrequencyFlag = ((buffer[_lastIndex] & 0x01) != 0);
        _lastIndex++;

        _lastIndex += 4;

      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Terrestrial Delivery System Descriptor message is short"));
      }
    }
    #endregion
  }
}