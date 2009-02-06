using System;

namespace api
{
  /// <summary>
  /// String metric interface
  /// </summary>
  public interface InterfaceStringMetric
  {
    /// <summary>
    /// Gets the short description
    /// </summary>
    String ShortDescriptionString { get; }

    /// <summary>
    /// Gets the long description
    /// </summary>
    String LongDescriptionString { get; }

    /// <summary>
    /// Return Similarity Timing Actual
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>Similarity Timing</returns>
    long getSimilarityTimingActual(String s, String s1);

    /// <summary>
    /// Return the similarty timing estimation
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>similarty timing estimation</returns>
    float getSimilarityTimingEstimated(String s, String s1);

    /// <summary>
    /// Return the similarity
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>Similarity</returns>
    float getSimilarity(String s, String s1);
  }
}