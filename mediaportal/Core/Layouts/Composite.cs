using System;
using System.Collections;
using System.Drawing;

using MediaPortal.GUI.Library;

namespace MediaPortal.Layouts
{
	public class Composite : Scrollable, ILayoutComposite
	{
		#region Methods

		public void Add(ILayoutComponent component)
		{
			_children.Add(component);
		}

		public void Add(GUIControl control)
		{
			_children.Add(control);
		}
		
		public void Arrange()
		{
			if(_beginInitCount != 0)
				return;

			if(_layout == null)
				return;

			_layout.Arrange(this);
		}

		protected override void ArrangeCore(Rectangle rect)
		{
			_location = rect.Location;
			_size = rect.Size;

			if(_layout == null)
				return;

			_layout.Arrange(this);
		}

		protected override void MeasureCore()
		{
			if(_layout == null)
				return;

			_layout.Measure(this, _size);
			_size = _layout.Size;
		}

		public void BeginInit()
		{
			_beginInitCount++;
		}

		public void EndInit()
		{
			if(_beginInitCount-- == 0)
				Arrange();
		}

		public void Measure(Size availableSize)
		{
			if(_layout == null)
				return;

			_layout.Measure(this, availableSize);
			_size = _layout.Size;
		}

		#endregion Methods

		#region Properties

		public ICollection Children
		{
			get { return _children; }
		}

		public ILayout Layout
		{
			get { return _layout; }
			set { _layout = value; }
		}

		public Margins Margins
		{
			get { return _margins; }
			set { _margins = value; }
		}

		#endregion Properties

		#region Fields

		int							_beginInitCount = 0;
		ILayoutComponentCollection	_children = new ILayoutComponentCollection();
		ILayout						_layout = null;
		Margins						_margins = Margins.Empty;

		#endregion Fields
	}
}
