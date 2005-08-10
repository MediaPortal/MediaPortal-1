using System;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Animations
{
	public class DurationConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(String);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value is string)
				return new Duration(TimeSpan.Parse((string)value).TotalMilliseconds);

			return base.ConvertFrom(context, culture, value);
		}

		#endregion Methods
	}
}
