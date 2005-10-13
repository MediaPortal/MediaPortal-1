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
				return RepeatBehavior.Parse((string)value);

			return base.ConvertFrom(context, culture, value);
		}

		#endregion Methods
	}
}
