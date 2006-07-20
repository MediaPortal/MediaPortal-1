using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// interface which describes a tv/radio channel
  /// </summary>
  public interface IChannel
  {
    /// <summary>
    /// gets/sets the channel name
    /// </summary>
    string Name { get;set;}

    /// <summary>
    /// boolean indication if this is a radio channel
    /// </summary>
    bool IsRadio { get;set;}

    /// <summary>
    /// boolean indication if this is a tv channel
    /// </summary>
    bool IsTv { get;set;}
  }
}
