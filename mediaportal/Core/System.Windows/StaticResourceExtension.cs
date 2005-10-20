using System;
using System.Windows.Serialization;

namespace System.Windows
{
	public class StaticResourceExtension : MarkupExtension
	{
		#region Methods

		public override object ProvideValue(object target, object value)
		{
			return MediaPortal.App.Current.FindResource(value);
		}

		#endregion Methods
	}
}
