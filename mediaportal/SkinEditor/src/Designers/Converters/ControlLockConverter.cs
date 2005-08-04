using System;
using System.ComponentModel;
using System.Drawing.Design;

using SkinEditor.Controls;
using SkinEditor.Controls.Properties;

namespace SkinEditor.Design.Converters
{
	internal class ControlLockConverter : ExpandableObjectConverter {
		public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType) {
			if (destinationType == typeof(MpeControlLock))
				return true;
			return base.CanConvertTo(context, destinationType);
		}
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType) {
			if (destinationType == typeof(System.String) && value is MpeControlLock){
				MpeControlLock clp = (MpeControlLock)value;
				return clp.Location + ", " + clp.Size;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
		public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType) {
			if (sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if (value is string) {
				try {
					string s = (string)value;
					int i = s.IndexOf(',');

					if (i > 0) {
						string szLocation = s.Substring(0,i).Trim();
						string szSize = s.Substring(i+1).Trim();
						MpeControlLock clp = new MpeControlLock();
						clp.Location = bool.Parse(szLocation);
						clp.Size = bool.Parse(szSize);
						return clp;
					}
				}
				catch {
					throw new ArgumentException("Can not convert '" + (string)value + "' to type ControlLock");
				}
			}  
			return base.ConvertFrom(context, culture, value);
		}
	}

}
