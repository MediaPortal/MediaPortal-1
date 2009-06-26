using System;

namespace api
{
  // Referenced classes of package api:
  //            InterfaceSubstitutionCost

  /// <summary>
  /// Abstract SubstitutionCost
  /// </summary>
  public abstract class AbstractSubstitutionCost : InterfaceSubstitutionCost
  {
    /// <summary>
    /// Short Descritption
    /// </summary>
    /// <returns>Short Description</returns>
    public abstract String getShortDescriptionString();

    /// <summary>
    /// Returns the cost
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="i">Param2</param>
    /// <param name="s1">Param3</param>
    /// <param name="j">Param4</param>
    /// <returns>Cost</returns>
    public abstract float getCost(String s, int i, String s1, int j);

    /// <summary>
    /// Get the maximum cost
    /// </summary>
    /// <returns>Maximum cost</returns>
    public abstract float getMaxCost();

    /// <summary>
    /// Get the minimum cost
    /// </summary>
    /// <returns>Minimum cost</returns>
    public abstract float getMinCost();
  }
}