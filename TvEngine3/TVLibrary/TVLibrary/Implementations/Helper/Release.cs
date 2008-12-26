using System.Runtime.InteropServices;

namespace TvLibrary
{
  /// <summary>
  /// Helper class for releasing a com object
  /// </summary>
  public class Release
  {
    /// <summary>
    /// Releases a com object
    /// </summary>
    /// <param name="line">Log line input</param>
    /// <param name="o">Object to release</param>
    static public void ComObject(string line,object o)
    {
      if (o != null)
      {
        Marshal.ReleaseComObject(o);
        // Log.Log.WriteFile("  Release {0} returns {1}", line, hr);
      }
    }
  }
}
