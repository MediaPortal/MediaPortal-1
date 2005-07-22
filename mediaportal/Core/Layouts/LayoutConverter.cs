using System;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Layouts
{
	public class LayoutConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
//			return new StackLayout();
			return null;
		}

		#endregion Methods
	}
}
