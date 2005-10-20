using System;
using System.Windows;

namespace MediaPortal
{
	public class App
	{
		#region Methods

		public object FindResource(object key)
		{
			if(_resources == null)
				return null;

			return _resources[key];
		}

		public void Run()
		{
			System.Windows.Forms.Application.Run();
		}

		#endregion Methods

		#region Properties

		public static App Current
		{
			get { if(_current == null) _current = new App(); return _current; }
		}

		public ResourceDictionary Resources
		{
			get { if(_resources == null) _resources = new ResourceDictionary(); return _resources; }
			set { _resources = value; }
		}

		#endregion Properties

		#region Fields

		static App					_current;
		ResourceDictionary			_resources;

		#endregion Fields
	}
}
