using System;

namespace api
{
  /// <summary>
  /// Interface for substitution cost
  /// </summary>
  public interface InterfaceSubstitutionCost
  {
    /// <summary>
    /// Gets the short description
    /// </summary>
    /// <returns>Short description</returns>
    String getShortDescriptionString();

    /// <summary>
    /// Returns the cost
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="i">Param2</param>
    /// <param name="s1">Param3</param>
    /// <param name="j">Param4</param>
    /// <returns>Cost</returns>
    float getCost(String s, int i, String s1, int j);

    /// <summary>
    /// Get the maximum cost
    /// </summary>
    /// <returns>Maximum cost</returns>
    float getMaxCost();

    /// <summary>
    /// Get the minimum cost
    /// </summary>
    /// <returns>Minimum cost</returns>
    float getMinCost();
  }
}