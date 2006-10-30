
using System;
namespace api
{
	
	
	public interface InterfaceSubstitutionCost
		{
			
			System.String getShortDescriptionString();
			
			float getCost(System.String s, int i, System.String s1, int j);
			
			float getMaxCost();
			
			float getMinCost();
		}
}