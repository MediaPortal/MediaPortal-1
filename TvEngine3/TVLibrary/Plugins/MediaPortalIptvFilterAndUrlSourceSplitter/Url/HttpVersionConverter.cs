using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for HTTP version converter.
    /// </summary>
    internal class HttpVersionConverter : ExpandableObjectConverter
    {
        #region Methods

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<String> versions = new List<String>();

            versions.Add(HttpVersionConverter.HttpVersionDefault);
            versions.Add(HttpVersionConverter.HttpVersionForce10);
            versions.Add(HttpVersionConverter.HttpVersionForce11);

            return new StandardValuesCollection(versions);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Version))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String))
            {
                if (value == null)
                {
                    return HttpVersionConverter.HttpVersionDefault;
                }

                if ((value != null) && (value is Version))
                {
                    if ((Version)value == HttpVersion.Version10)
                    {
                        return HttpVersionConverter.HttpVersionForce10;
                    }

                    if ((Version)value == HttpVersion.Version11)
                    {
                        return HttpVersionConverter.HttpVersionForce11;
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (String.CompareOrdinal((String)value, HttpVersionConverter.HttpVersionDefault) == 0)
            {
                return null;
            }

            if (String.CompareOrdinal((String)value, HttpVersionConverter.HttpVersionForce10) == 0)
            {
                return HttpVersion.Version10;
            }

            if (String.CompareOrdinal((String)value, HttpVersionConverter.HttpVersionForce11) == 0)
            {
                return HttpVersion.Version11;
            }

            return base.ConvertFrom(context, culture, value);
        }

        #endregion

        #region Constants

        public const String HttpVersionDefault = "Automatic HTTP version";
        public const String HttpVersionForce10 = "Force HTTP version 1.0";
        public const String HttpVersionForce11 = "Force HTTP version 1.1";

        #endregion
    }
}
