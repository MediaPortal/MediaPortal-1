
using System;
using AbstractSubstitutionCost = api.AbstractSubstitutionCost;
namespace similaritymetrics.costfunctions
{
	
	[Serializable]
	public sealed class SubCost01:AbstractSubstitutionCost//, System.Runtime.Serialization.ISerializable
	{
		
		public SubCost01()
		{
		}
		
		public override System.String getShortDescriptionString()
		{
			return "SubCost01";
		}
		
		public override float getCost(System.String str1, int string1Index, System.String str2, int string2Index)
		{
			return str1[string1Index] != str2[string2Index]?1.0F:0.0F;
		}
		
		public override float getMaxCost()
		{
			return 1.0F;
		}
		
		public override float getMinCost()
		{
			return 0.0F;
		}
	}
}