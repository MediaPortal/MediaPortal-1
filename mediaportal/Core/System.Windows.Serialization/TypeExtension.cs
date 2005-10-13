using System;

namespace System.Windows.Serialization
{
	public class TypeExtension : MarkupExtension
	{
		#region Methods

		public override object ProvideValue(object target, object value)
		{
			Type t = null;

			foreach(string ns in _namespaces)
			{
				t = Type.GetType(ns + "." + (string)value);

				if(t != null)
					return t;
			}

			return null;
		}

		#endregion Methods

		#region Fields

		static string[]				_namespaces = new string[] { "MediaPortal", "MediaPortal.Controls", "MediaPortal.Drawing", "MediaPortal.Drawing.Shapes", "MediaPortal.Drawing.Transforms", "MediaPortal.Animation", "System.Windows", "System.Windows.Serialization", "MediaPortal.Drawing.Paths" };

		#endregion Fields
	}
}
