using System;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Animation
{
	public class DurationConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
		{
			if(t == typeof(string))
				return true;

			return base.CanConvertFrom(context, t);
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
