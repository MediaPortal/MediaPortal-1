using System;
using System.Collections.Generic;
using System.Text;

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
  class suminfo
  {
    public int newChannel = 0;
    public int updChannel = 0;
    public int newChannelSum = 0;
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
    Done
  }
}
