using System;
using System.ComponentModel;
using System.Windows.Serialization;

namespace System.Windows
{
	public sealed class Trigger : IAddChild
	{
		#region Constructors

		public Trigger()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is Setter == false)
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof(Trigger)));

			if(_setters == null)
				_setters = new SetterCollection();

			_setters.Add((Setter)child);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException();
		}

		#endregion Methods

		#region Properties
		
		public DependencyBinding Property
		{
			get { return _binding; }
			set { _binding = value; }
		}

		public SetterCollection Setters
		{
			get { if(_setters == null) _setters = new SetterCollection(); return _setters; }
		}

		public object Value
		{ 
			get { return _value; }
			set { _value = value; }
		}

/*		bool IScenegraphSwitch.Value
		{
			get { return _isTriggered; }
		}
*/
		#endregion Properties

		#region Fields

		DependencyBinding			_binding;
		bool						_isTriggered;
		SetterCollection			_setters;
		object						_value;

		#endregion Fields
	}
}
