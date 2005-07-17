using System;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.GUI.Layouts
{
	public class LayoutConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(String);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return new StackLayout();
		}

		#endregion Methods
	}
}
