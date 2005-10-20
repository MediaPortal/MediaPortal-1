using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows
{
	public sealed class PropertyPathConverter : TypeConverter
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
				return PropertyPath.Parse((string)value);

			return base.ConvertFrom(context, culture, value);
		}

		#endregion Methods
	}
}