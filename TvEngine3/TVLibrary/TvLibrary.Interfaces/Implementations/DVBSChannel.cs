using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  public enum DisEqcType
  {
    None,
    SimpleA,
    SimpleB,
    Level1AA,
    Level1BA,
    Level1AB,
    Level1BB,
  };

  /// <summary>
  /// class holding all tuning details for DVBS
  /// </summary>
  [Serializable]
  public class DVBSChannel : DVBBaseChannel
  {
    #region variables
    Polarisation _polarisation;
    int _symbolRate;
    int _switchingFrequency;
    DisEqcType _disEqc;
    #endregion

    public DVBSChannel()
    {
      SwitchingFrequency = 0;
      DisEqc = DisEqcType.SimpleA;
    }

    #region properties
    /// <summary>
    /// gets/sets the Polarisation for this channel
    /// </summary>
    public Polarisation Polarisation
    {
      get
      {
        return _polarisation;
      }
      set
      {
        _polarisation = value;
      }
    }
    /// <summary>
    /// gets/sets the symbolrate for this channel
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
    /// <summary>
    /// gets/sets the lnb switch frequency for this channel
    /// </summary>
    public int SwitchingFrequency
    {
      get
      {
        return _switchingFrequency;
      }
      set
      {
        _switchingFrequency = value;
      }
    }
    /// <summary>
    /// gets/sets the diseqc setting for this channel
    /// </summary>
    public DisEqcType DisEqc
    {
      get
      {
        return _disEqc;
      }
      set
      {
        _disEqc = value;
      }
    }
    #endregion

    public override string ToString()
    {
      string line = String.Format("DVBS:{0} SymbolRate:{1} Polarisation:{2} DisEqc:{3} Switch Freq:{4}",
          base.ToString(), SymbolRate, Polarisation, DisEqc,SwitchingFrequency);
      return line;
    }

    public override bool Equals(object obj)
    {
      if ((obj as DVBSChannel)==null) return false;
      if (!base.Equals(obj)) return false;
      DVBSChannel ch = obj as DVBSChannel;
      if (ch.Polarisation != Polarisation) return false;
      if (ch.SymbolRate != SymbolRate) return false;
      if (ch.SwitchingFrequency != SwitchingFrequency) return false;
      if (ch.DisEqc != DisEqc) return false;

      return true;
    }
  }
}

