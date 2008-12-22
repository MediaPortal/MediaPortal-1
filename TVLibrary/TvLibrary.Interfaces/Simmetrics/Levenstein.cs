
using System;
using AbstractStringMetric = api.AbstractStringMetric;
using AbstractSubstitutionCost = api.AbstractSubstitutionCost;
using MathFuncs = mathSimmetrics.MathFuncs;
using SubCost01 = similaritymetrics.costfunctions.SubCost01;
using System.Xml.Serialization;
namespace similaritymetrics
{
  
	[Serializable]
  public sealed class Levenstein:AbstractStringMetric//, System.Runtime.Serialization.ISerializable
	{
		private void  InitBlock()
		{
			dCostFunc = new SubCost01();
		}
		override public System.String ShortDescriptionString
		{
			get
			{
				return "Levenstein";
			}
			
		}
		override public System.String LongDescriptionString
		{
			get
			{
				return "Implements the basic Levenstein algorithm providing a similarity measure between two strings";
			}
			
		}
		
		public Levenstein()
		{
			InitBlock();
		}
		
		public override float getSimilarityTimingEstimated(System.String string1, System.String string2)
		{
			float str1Length = string1.Length;
			float str2Length = string2.Length;
			return str1Length * str2Length * 0.00018F;
		}
		
		public override float getSimilarity(System.String string1, System.String string2)
		{
      if (string1 == null) return 0f;
      if (string2 == null) return 0f;
			float levensteinDistance = calcLevenDistance(string1, string2);
			float maxLen = string1.Length;
			//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
			if (maxLen < (float) string2.Length)
				maxLen = string2.Length;
			if (maxLen == 0.0F)
				return 1.0F;
			else
				return 1.0F - levensteinDistance / maxLen;
		}
		
		private float calcLevenDistance(System.String s, System.String t)
		{
			int n = s.Length;
			int m = t.Length;
			if (n == 0)
			{
				//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
				return (float) m;
			}
			if (m == 0)
			{
				//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
				return (float) n;
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
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'ESTIMATEDTIMINGCONST '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		//private float ESTIMATEDTIMINGCONST = 0.00018F;
		//UPGRADE_NOTE: Final was removed from the declaration of 'dCostFunc '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		//UPGRADE_NOTE: The initialization of  'dCostFunc' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private AbstractSubstitutionCost dCostFunc;
	}
}