
using System;
namespace api
{
	
	
	// Referenced classes of package api:
	//            InterfaceStringMetric
	public abstract class AbstractStringMetric : InterfaceStringMetric
	{
		public abstract System.String ShortDescriptionString{get;}
		public abstract System.String LongDescriptionString{get;}
		
		public AbstractStringMetric()
		{
		}
		
		public long getSimilarityTimingActual(System.String string1, System.String string2)
		{
			//UPGRADE_TODO: Method 'java.lang.System.currentTimeMillis' was converted to 'System.DateTime.Now' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javalangSystemcurrentTimeMillis"'
			long timeBefore = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			getSimilarity(string1, string2);
			//UPGRADE_TODO: Method 'java.lang.System.currentTimeMillis' was converted to 'System.DateTime.Now' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javalangSystemcurrentTimeMillis"'
			long timeAfter = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			return timeAfter - timeBefore;
		}
		
		public float[] batchCompareSet(System.String[] set_Renamed, System.String comparator)
		{
			float[] results = new float[set_Renamed.Length];
			for (int strNum = 0; strNum < set_Renamed.Length; strNum++)
				results[strNum] = getSimilarity(set_Renamed[strNum], comparator);
			
			return results;
		}
		
		public float[] batchCompareSets(System.String[] firstSet, System.String[] secondSet)
		{
			float[] results;
			if (firstSet.Length <= secondSet.Length)
				results = new float[firstSet.Length];
			else
				results = new float[secondSet.Length];
			for (int strNum = 0; strNum < results.Length; strNum++)
				results[strNum] = getSimilarity(firstSet[strNum], secondSet[strNum]);
			
			return results;
		}
		
		public abstract float getSimilarityTimingEstimated(System.String s, System.String s1);
		
		public abstract float getSimilarity(System.String s, System.String s1);
	}
}