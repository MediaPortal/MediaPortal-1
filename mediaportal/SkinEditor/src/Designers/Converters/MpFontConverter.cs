using System;
using System.ComponentModel;
using System.Drawing.Design;

using SkinEditor.Controls;

namespace SkinEditor.Design.Converters
{
	/// <summary>
	/// Used to display MpeFont resources in the Property Grid
	/// </summary>
	public class MpeFontConverter : TypeConverter {
		
		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
			if (context.Instance is MpeControl) {
				MpeControl mpc = (MpeControl)context.Instance;
				return new StandardValuesCollection(mpc.Parser.FontNames);
			} else {
				return base.GetStandardValues(context);
			}
		}
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
			return true;
		}
		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType) {
			if( sourceType == typeof(string) )
				return true;
			else 
				return base.CanConvertFrom(context, sourceType);
		}
		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if( value.GetType() == typeof(string) && context.Instance is MpeControl ) {
				MpeControl mpc = (MpeControl)context.Instance;
				return mpc.Parser.GetFont((string)value);
			} else {
				return base.ConvertFrom(context, culture, value);
			}
		}
	}
}
