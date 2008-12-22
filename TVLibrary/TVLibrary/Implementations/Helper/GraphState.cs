using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.Helper
{
  /// <summary>
  /// Different states of the card
  /// </summary>
  public enum GraphState
  {
    /// <summary>
    /// Card is idle
    /// </summary>
    Idle,
    /// <summary>
    /// Card is idle, but graph is created
    /// </summary>
    Created,
    /// <summary>
    /// Card is timeshifting
    /// </summary>
    TimeShifting,
    /// <summary>
    /// Card is recording
    /// </summary>
    Recording
  }
}
