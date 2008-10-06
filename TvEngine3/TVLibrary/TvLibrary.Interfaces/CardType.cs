using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Types of cards
  /// </summary>
  public enum CardType
  {
    /// <summary>
    /// analog card
    /// </summary>
    Analog,
    /// <summary>
    /// DVB-S card
    /// </summary>
    DvbS,
    /// <summary>
    /// DVB-T card
    /// </summary>
    DvbT,
    /// <summary>
    /// DVB-C card
    /// </summary>
    DvbC,
    /// <summary>
    /// ATSC card
    /// </summary>
    Atsc,
    /// <summary>
    /// RadioWebStream card
    /// </summary>
    RadioWebStream,
    /// <summary>
    /// Unknown card
    /// </summary>
    Unknown
  }

}
