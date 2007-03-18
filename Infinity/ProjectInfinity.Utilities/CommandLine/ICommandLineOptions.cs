using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Utilities.CommandLine
{
  /// <summary>
  /// Sets an option, with arguments.
  /// </summary>
  public interface ICommandLineOptions
  {
    /// <summary>
    /// Sets an option, with arguments.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <param name="argument">The argument (can be null).</param>
    void SetOption(string option, string argument);

    /// <summary>
    /// Displays the options to console
    /// </summary>
    void DisplayOptions();
  }
}
