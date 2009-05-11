
using System;
namespace mathSimmetrics
{
	
	
	public sealed class MathFuncs
	{
		
		public MathFuncs()
		{
		}
		
		public static float max3(float x, float y, float z)
		{
			return System.Math.Max(x, System.Math.Max(y, z));
		}
		
		public static float max4(float w, float x, float y, float z)
		{
			return System.Math.Max(System.Math.Max(w, x), System.Math.Max(y, z));
		}
		
		public static float min3(float x, float y, float z)
		{
			return System.Math.Min(x, System.Math.Min(y, z));
		}
		
		public static int min3(int x, int y, int z)
		{
			return System.Math.Min(x, System.Math.Min(y, z));
		}
		
		public static int max3(int x, int y, int z)
		{
			return System.Math.Max(x, System.Math.Max(y, z));
		}
	}
}