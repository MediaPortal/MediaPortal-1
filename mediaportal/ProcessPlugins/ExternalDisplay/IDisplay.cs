using System;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// The common interface for all displays that this plug-in supports
  /// </summary>
  /// <author>JoeDalton</author>
  public interface IDisplay : IDisposable
  {
    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="line">The line to thow the message on.</param>
    /// <param name="message">The message to show.</param>
    void SetLine(int line, string message);
    /// <summary>
    /// Gets the short name of the display
    /// </summary>
    string Name {get;}
    /// <summary>
    /// Gets the description of the display
    /// </summary>
    string Description {get;}
    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    bool SupportsText {get;}
    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    bool SupportsGraphics {get;}
    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    void Configure();
    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="port">The port the display is connected to</param>
    /// <param name="lines">The number of lines in text mode</param>
    /// <param name="cols">The number of columns in text mode</param>
    /// <param name="delay">Communication delay in text mode</param>
    /// <param name="linesG">The height in pixels in graphic mode</param>
    /// <param name="colsG">The width in pixels in graphic mode</param>
    /// <param name="timeG">Communication delay in graphic mode</param>
    /// <param name="backLight">Backlight on?</param>
    void Initialize(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight);
    /// <summary>
    /// Clears the display
    /// </summary>
    void Clear();

  }
}
