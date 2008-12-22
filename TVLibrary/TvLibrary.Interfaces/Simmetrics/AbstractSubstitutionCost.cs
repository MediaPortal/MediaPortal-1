
using System;
namespace api
{
	
	
	// Referenced classes of package api:
	//            InterfaceSubstitutionCost
	
	public abstract class AbstractSubstitutionCost : InterfaceSubstitutionCost
	{
		
		public AbstractSubstitutionCost()
		{
		}
		
		public abstract System.String getShortDescriptionString();
		
		public abstract float getCost(System.String s, int i, System.String s1, int j);
		
		public abstract float getMaxCost();
		
		public abstract float getMinCost();
	}
}