using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Serialization;

namespace System.Windows
{
	public class TemplateBindingExtension : MarkupExtension
	{
		#region Constructors

		public TemplateBindingExtension()
		{
		}

		public TemplateBindingExtension(DependencyBinding binding)
		{
		}

		#endregion Constructors

		#region Methods

		public override object ProvideValue(object target, object value)
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Properties

		public object ConverterParameter
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public DependencyBinding Property
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IValueConverter ValueConverter
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		#endregion Properties
	}
}
