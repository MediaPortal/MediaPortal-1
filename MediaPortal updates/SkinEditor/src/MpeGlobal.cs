using Mpe.Forms;

namespace Mpe
{
  /// <summary>
  /// Global Static References
  /// </summary>
  public interface MpeGlobal
  {
    MpeStatusBar StatusBar { get; }

    MpeParser Parser { get; set; }

    MpePreferences Preferences { get; }

    MpePropertyManager PropertyManager { get; }

    MpeExplorer Explorer { get; }

    object Clipboard { get; set; }
  }
}