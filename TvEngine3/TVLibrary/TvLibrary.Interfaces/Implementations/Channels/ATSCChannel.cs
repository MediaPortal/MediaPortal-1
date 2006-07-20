using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib.BDA;

namespace TvLibrary.Channels
{
  /// <summary>
  /// class holding all tuning details for ATSC
  /// </summary>
  [Serializable]
  public class ATSCChannel : DVBBaseChannel
  {
    #region variables
    int _physicalChannel;
    int _majorChannel;
    int _minorChannel;
    int _symbolRate;
    ModulationType _modulation;
    #endregion

    public ATSCChannel()
    {
      _majorChannel = -1;
      _minorChannel = -1;
      _symbolRate = -1;
      _physicalChannel = -1;
    }

    #region properties
    /// <summary>
    /// gets/sets the modulation type
    /// </summary>
    public ModulationType ModulationType
    {
      get
      {
        return _modulation;
      }
      set
      {
        _modulation = value;
      }
    }
    /// <summary>
    /// gets/sets the physical channel
    /// </summary>
    public int PhysicalChannel
    {
      get
      {
        return _physicalChannel;
      }
      set
      {
        _physicalChannel = value;
      }
    }
    /// <summary>
    /// gets/sets the major channel
    /// </summary>
    public int MajorChannel
    {
      get
      {
        return _majorChannel;
      }
      set
      {
        _majorChannel = value;
      }
    }
    /// <summary>
    /// gets/sets the minor channel
    /// </summary>
    public int MinorChannel
    {
      get
      {
        return _minorChannel;
      }
      set
      {
        _minorChannel = value;
      }
    }
    /// <summary>
    /// gets/sets the symbolrate
    /// </summary>
    public int SymbolRate
    {
      get
      {
        return _symbolRate;
      }
      set
      {
        _symbolRate = value;
      }
    }
    #endregion

    public override string ToString()
    {
      return String.Format("{0} phys:{1} maj:{2} min:{3} SR:{4} mod:{5}",
        base.ToString(),_physicalChannel,_majorChannel,_minorChannel,_symbolRate,_modulation);
    }

    public override bool Equals(object obj)
    {
      if ((obj as ATSCChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      ATSCChannel ch = obj as ATSCChannel;
      if (ch.MajorChannel != MajorChannel) return false;
      if (ch.MinorChannel != MinorChannel) return false;
      if (ch.SymbolRate != SymbolRate) return false;

      return true;
    }
  }
}

