using System;

namespace System.Windows.Serialization
{
	public abstract class MarkupExtension
	{
		#region Methods

		public abstract object ProvideValue(object target, object value);

		#endregion Methods
	}
}
