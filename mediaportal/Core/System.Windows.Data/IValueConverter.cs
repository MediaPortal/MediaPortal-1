using System;
using System.Globalization;

namespace System.Windows.Data
{
	public interface IValueConverter
	{
		#region Methods

		object Convert(Object value, Type targetType, object parameter, CultureInfo culture);
		object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

		#endregion Methods
	}
}
