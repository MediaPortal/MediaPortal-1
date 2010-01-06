using System;

namespace api
{
  // Referenced classes of package api:
  //            InterfaceStringMetric
  ///<summary>
  /// Abstradct String MEtric
  ///</summary>
  public abstract class AbstractStringMetric : InterfaceStringMetric
  {
    /// <summary>
    /// Short Descritpion
    /// </summary>
    public abstract String ShortDescriptionString { get; }

    /// <summary>
    /// Long Description
    /// </summary>
    public abstract String LongDescriptionString { get; }

    /// <summary>
    /// Return Similarity Timing Actual
    /// </summary>
    /// <param name="string1">Param1</param>
    /// <param name="string2">Param2</param>
    /// <returns>Similarity Timing</returns>
    public long getSimilarityTimingActual(String string1, String string2)
    {
      //UPGRADE_TODO: Method 'java.lang.System.currentTimeMillis' was converted to 'System.DateTime.Now' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javalangSystemcurrentTimeMillis"'
      long timeBefore = (DateTime.Now.Ticks - 621355968000000000) / 10000;
      getSimilarity(string1, string2);
      //UPGRADE_TODO: Method 'java.lang.System.currentTimeMillis' was converted to 'System.DateTime.Now' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javalangSystemcurrentTimeMillis"'
      long timeAfter = (DateTime.Now.Ticks - 621355968000000000) / 10000;
      return timeAfter - timeBefore;
    }

    /// <summary>
    /// Batch compare of string
    /// </summary>
    /// <param name="set_Renamed">Strings to compare</param>
    /// <param name="comparator">Comparator</param>
    /// <returns>Results</returns>
    public float[] batchCompareSet(String[] set_Renamed, String comparator)
    {
      float[] results = new float[set_Renamed.Length];
      for (int strNum = 0; strNum < set_Renamed.Length; strNum++)
      {
        results[strNum] = getSimilarity(set_Renamed[strNum], comparator);
      }

      return results;
    }

    /// <summary>
    /// Batch compare of two string sets
    /// </summary>
    /// <param name="firstSet">Fist set</param>
    /// <param name="secondSet">Second set</param>
    /// <returns>Results</returns>
    public float[] batchCompareSets(String[] firstSet, String[] secondSet)
    {
      float[] results = firstSet.Length <= secondSet.Length ? new float[firstSet.Length] : new float[secondSet.Length];
      for (int strNum = 0; strNum < results.Length; strNum++)
      {
        results[strNum] = getSimilarity(firstSet[strNum], secondSet[strNum]);
      }

      return results;
    }

    /// <summary>
    /// Return the similarty timing estimation
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>similarty timing estimation</returns>
    public abstract float getSimilarityTimingEstimated(String s, String s1);

    /// <summary>
    /// Return the similarity
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>Similarity</returns>
    public abstract float getSimilarity(String s, String s1);
  }
}