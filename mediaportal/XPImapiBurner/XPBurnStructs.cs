using System;

namespace XPBurn
{
  /// <summary>
  /// This type is returned by the <see cref="XPBurnCD.MediaInfo" /> property and contains information about the 
  /// media (CD) which is currently inserted into the recorder.  
  /// </summary>
  public struct Media
  {
    /// <summary>
    /// Indicates the CD in the active recorder is blank.
    /// </summary>
    public bool isBlank;
    /// <summary>
    /// Indicates that the CD in the active recorder is both readable and writable.
    /// </summary>
    public bool isReadWrite;
    /// <summary>
    /// Indicates that the CD in the active recorder is writable.
    /// </summary>
    public bool isWritable;
    /// <summary>
    /// Indicates that the CD in the active recorder is usable.
    /// </summary>
    public bool isUsable;
  }
}
