using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
namespace TvLibrary.Epg
{
  [Serializable]
  public class EpgChannel
  {
    #region variables
    List<EpgProgram> _programs;
    IChannel _channel;
    #endregion

    #region ctor
    public EpgChannel()
    {
      _programs = new List<EpgProgram>();
    }
    #endregion


    #region properties
    public IChannel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = value;
      }
    }

    public List<EpgProgram> Programs
    {
      get
      {
        return _programs;
      }
      set
      {
        _programs = value;
      }
    }
    #endregion

    public void Sort()
    {
      _programs.Sort();
    }
  }
}
