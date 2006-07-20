using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  /// <summary>
  /// class holding all tuning details for DVBC
  /// </summary>
  [Serializable]
  public class DVBCChannel : DVBBaseChannel
  {
    #region variables
    ModulationType _modulation;
    int _symbolRate;
    #endregion

    public DVBCChannel()
    {
      ModulationType = ModulationType.Mod64Qam;
      SymbolRate = 6875;
    }

    #region properties
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
    /// gets/sets the ModulationType for this channel
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
    #endregion

    public override string ToString()
    {
      string line = String.Format("DVBC:{0} SymbolRate:{1} Modulation:{2}",
          base.ToString(), SymbolRate, ModulationType);
      return line;
    }


    public override bool Equals(object obj)
    {
      if ((obj as DVBCChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      DVBCChannel ch = obj as DVBCChannel;
      if (ch.ModulationType != ModulationType) return false;
      if (ch.SymbolRate != SymbolRate) return false;

      return true;
    }
  }
}

