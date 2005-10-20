using System;
using System.ComponentModel;
using System.Xml;

namespace System.Windows
{
	public sealed class Setter
	{
		#region Constructors

		public Setter()
		{
		}

		public Setter(DependencyBinding binding, object value)
		{
			_binding = binding;
			_value = value;
		}

		public Setter(DependencyBinding binding, object value, string targetName)
		{
			_binding = binding;
			_value = value;
			_targetName = targetName;
		}

		#endregion Constructors

		#region Methods

		void SetValue(object value)
		{
			if(_binding == null)
			{
				_value = value;
				return;
			}

			if(_binding.PropertyType == typeof(object))
			{
				_value = value;
				return;
			}

			if(_binding.PropertyType == value.GetType())
			{
				_value = value;
				return;
			}

			TypeConverter converter = TypeDescriptor.GetConverter(_binding.PropertyType);

			try
			{
				_value = converter.ConvertFromString((string)value);
			}
			catch(Exception e)
			{
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", value, _binding.PropertyType), e);
			}
		}

		#endregion Methods

		#region Properties

		public DependencyBinding Property
		{
			get { return _binding; }
			set { _binding = value; }
		}

		public string TargetName
		{
			get { return _targetName; }
			set { _targetName = value; }
		}

		public object Value
		{
			get { return _value; }
			set { SetValue(value); }
		}

		#endregion Properties

		#region Fields

		DependencyBinding			_binding;
		string						_targetName = string.Empty;
		object						_value;

		#endregion Fields
	}
}
