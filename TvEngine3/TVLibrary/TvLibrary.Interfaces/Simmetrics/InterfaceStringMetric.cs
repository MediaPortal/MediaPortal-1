
using System;
namespace api
{

	public interface InterfaceStringMetric
		{
			System.String ShortDescriptionString
			{
				get;
				
			}
			System.String LongDescriptionString
			{
				get;
				
			}
			
			long getSimilarityTimingActual(System.String s, System.String s1);
			
			float getSimilarityTimingEstimated(System.String s, System.String s1);
			
			float getSimilarity(System.String s, System.String s1);
		}
}