using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace SetupTv.Sections
{
  /// <summary>
  /// Type of scan action
  /// </summary>
  public enum ScanTypes
  {
    Predefined = 0,
    SingleTransponder = 1,
    NIT = 2
  }

  /// <summary>
  /// Counter class
  /// </summary>
  internal class suminfo
  {
    private List<IChannel> _allNewChannels = new List<IChannel>();

    public List<IChannel> newChannels
    {
      get { return _allNewChannels; }
    }

    public int newChannel = 0;
    public int updChannel = 0;

    public int newChannelSum
    {
      get { return _allNewChannels.Count; }
    }

    public int updChannelSum = 0;
  }

  /// <summary>
  /// State of scanning
  /// </summary>
  public enum ScanState
  {
    Initialized,
    Scanning,
    Cancel,
    Done,
    Updating
  }
}