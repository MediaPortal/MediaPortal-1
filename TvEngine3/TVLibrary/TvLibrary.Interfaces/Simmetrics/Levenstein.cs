
using System;
using AbstractStringMetric = api.AbstractStringMetric;
using AbstractSubstitutionCost = api.AbstractSubstitutionCost;
using MathFuncs = mathSimmetrics.MathFuncs;
using SubCost01 = similaritymetrics.costfunctions.SubCost01;

namespace similaritymetrics
{

  ///<summary>
  /// Levenstein metric
  ///</summary>
  [Serializable]
  public sealed class Levenstein : AbstractStringMetric//, System.Runtime.Serialization.ISerializable
  {
    private void InitBlock()
    {
      dCostFunc = new SubCost01();
    }
    /// <summary>
    /// Gets the short description
    /// </summary>
    override public String ShortDescriptionString
    {
      get
      {
        return "Levenstein";
      }

    }
    /// <summary>
    /// Gets the long description
    /// </summary>
    override public String LongDescriptionString
    {
      get
      {
        return "Implements the basic Levenstein algorithm providing a similarity measure between two strings";
      }

    }

    ///<summary>
    /// Constructor
    ///</summary>
    public Levenstein()
    {
      InitBlock();
    }

    /// <summary>
    /// Return the similarty timing estimation
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>similarty timing estimation</returns>
    public override float getSimilarityTimingEstimated(String s, String s1)
    {
      float str1Length = s.Length;
      float str2Length = s1.Length;
      return str1Length * str2Length * 0.00018F;
    }

    /// <summary>
    /// Return the similarity
    /// </summary>
    /// <param name="s">Param1</param>
    /// <param name="s1">Param2</param>
    /// <returns>Similarity</returns>
    public override float getSimilarity(String s, String s1)
    {
      if (s == null)
        return 0f;
      if (s1 == null)
        return 0f;
      float levensteinDistance = calcLevenDistance(s, s1);
      float maxLen = s.Length;
      //UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
      if (maxLen < s1.Length)
        maxLen = s1.Length;
      if (maxLen == 0.0F)
        return 1.0F;
      return 1.0F - levensteinDistance / maxLen;
    }

    private float calcLevenDistance(String s, String t)
    {
      int n = s.Length;
      int m = t.Length;
      if (n == 0)
      {
        return m;
      }
      if (m == 0)
      {
        return n;
      }
      float[][] d = new float[n + 1][];
      for (int i = 0; i < n + 1; i++)
      {
        d[i] = new float[m + 1];
      }
      for (int i = 0; i <= n; i++)
        d[i][0] = i;

      for (int j = 0; j <= m; j++)
        d[0][j] = j;

      for (int i = 1; i <= n; i++)
      {
        for (int j = 1; j <= m; j++)
        {
          float cost = dCostFunc.getCost(s, i - 1, t, j - 1);
          d[i][j] = MathFuncs.min3(d[i - 1][j] + 1.0F, d[i][j - 1] + 1.0F, d[i - 1][j - 1] + cost);
        }
      }

      return d[n][m];
    }

    private AbstractSubstitutionCost dCostFunc;
  }
}