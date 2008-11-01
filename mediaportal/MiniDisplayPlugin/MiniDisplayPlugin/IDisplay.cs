using System;
using System.Drawing;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public interface IDisplay : IDisposable
  {
    void CleanUp();
    void Configure();
    void DrawImage(Bitmap bitmap);
    void Initialize();
    void SetCustomCharacters(int[][] customCharacters);
    void SetLine(int line, string message);
    void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight, int backLightLevel, bool contrast, int contrastLevel, bool BlankOnExit);

    string Description { get; }

    string ErrorMessage { get; }

    bool IsDisabled { get; }

    string Name { get; }

    bool SupportsGraphics { get; }

    bool SupportsText { get; }
  }
}

