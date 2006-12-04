using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// interface definition for diseqc devices
  /// </summary>
  public interface IDiSEqCController
  {
    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    bool SendDiSEqCCommand(byte[] diSEqC);

    /// <summary>
    /// Sends a diseqc command and reads a reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    bool ReadDiSEqCCommand(out byte[] reply);
  }
}
