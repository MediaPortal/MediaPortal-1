using System;

namespace mathSimmetrics
{
  /// <summary>
  /// Math helper functions
  /// </summary>
  public sealed class MathFuncs
  {
    /// <summary>
    /// Returns the maximum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Maximum of the given three values</returns>
    public static float max3(float x, float y, float z)
    {
      return Math.Max(x, Math.Max(y, z));
    }

    /// <summary>
    /// REturns the maximum of four values
    /// </summary>
    /// <param name="w">First value</param>
    /// <param name="x">Second value</param>
    /// <param name="y">Third value</param>
    /// <param name="z">Fourth value</param>
    /// <returns>Maximum of the given fourvalues</returns>
    public static float max4(float w, float x, float y, float z)
    {
      return Math.Max(Math.Max(w, x), Math.Max(y, z));
    }

    /// <summary>
    /// Returns the minimum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Minimum of the given three values</returns>
    public static float min3(float x, float y, float z)
    {
      return Math.Min(x, Math.Min(y, z));
    }

    /// <summary>
    /// Returns the minimum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Minimum of the given three values</returns>
    public static int min3(int x, int y, int z)
    {
      return Math.Min(x, Math.Min(y, z));
    }

    /// <summary>
    /// Returns the maximum of three values
    /// </summary>
    /// <param name="x">First value</param>
    /// <param name="y">Second value</param>
    /// <param name="z">Third value</param>
    /// <returns>Maximum of the given three values</returns>
    public static int max3(int x, int y, int z)
    {
      return Math.Max(x, Math.Max(y, z));
    }
  }
}