using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  /// <summary>
  /// class holding all tuning details for DVBT
  /// </summary>
  [Serializable]
  public class DVBTChannel : DVBBaseChannel
  {
    #region variables
    int _bandWidth;
    #endregion

    public DVBTChannel()
    {
      BandWidth = 8;
    }

    /// <summary>
    /// gets/sets the bandwidth for this channel
    /// </summary>
    public int BandWidth
    {
      get
      {
        return _bandWidth;
      }
      set
      {
        _bandWidth = value;
      }
    }

    public override string ToString()
    {
      string line = String.Format("DVBT:{0} BandWidth:{1}", base.ToString(), BandWidth);
      return line;
    }
    public override bool Equals(object obj)
    {
      if ((obj as DVBTChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      DVBTChannel ch = obj as DVBTChannel;
      if (ch.BandWidth != BandWidth) return false;

      return true;
    }
  }
}

