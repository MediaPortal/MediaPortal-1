using System;
using System.Reflection;
using System.Windows.Serialization;

namespace System.Windows
{
	public abstract class ResourceKey : MarkupExtension
	{
		public ResourceKey()
		{
		}

		#region Methods

		public override object ProvideValue(object targetObject, object targetProperty)
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Properties

		public abstract Assembly Assembly
		{
			get;
		}

		#endregion Properties
	}
}
