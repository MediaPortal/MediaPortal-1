using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  public interface ITVScanning
  {
    void Dispose();

    /// <summary>
    /// resets the scanner
    /// </summary>
    void Reset();

    /// <summary>
    /// Tunes to the channel specified and will start scanning for any channel
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <returns>list of channels found</returns>
    List<IChannel> Scan(IChannel channel);

    /// <summary>
    /// returns the tv card used 
    /// </summary>
    ITVCard TvCard { get;}
  }
}
