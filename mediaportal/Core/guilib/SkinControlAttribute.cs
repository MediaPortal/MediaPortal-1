using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for SkinControlAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SkinControlAttribute : Attribute
	{
		int _id;
		public SkinControlAttribute(int id)
		{
			_id = id;
		}

		public int ID
		{
			get { return _id; }
		}
	}
}
