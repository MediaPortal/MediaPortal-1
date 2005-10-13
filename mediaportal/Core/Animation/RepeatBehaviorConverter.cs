using System;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Animation
{
	public class RepeatBehaviorConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(String);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value is string)
			{
				if(string.Compare((string)value, "Forever", true) == 0)
					return RepeatBehavior.Forever;

				return new RepeatBehavior(double.Parse((string)value));
			}

			return base.ConvertFrom(context, culture, value);
		}

		#endregion Methods
	}
}
