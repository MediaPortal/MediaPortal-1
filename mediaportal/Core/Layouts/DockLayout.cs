using System;
using System.Collections;
using System.Drawing;

namespace MediaPortal.Layouts
{
	public class DockLayout : ILayout
	{
		#region Constructors

		public DockLayout()
		{
			_spacing = new Size(0, 0);
		}

		public DockLayout(int horizontalSpacing, int verticalSpacing)
		{
			_spacing = new Size(Math.Max(0, horizontalSpacing), Math.Max(0, verticalSpacing));
		}

		#endregion Constructors

		#region Methods

		public void Arrange(ILayoutComposite composite)
		{
			ILayoutComponent l = null;
			ILayoutComponent t = null;
			ILayoutComponent r = null;
			ILayoutComponent b = null;
			ILayoutComponent f = null;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.Dock == Dock.Left)
					l = child;

				if(child.Dock == Dock.Top)
					t = child;

				if(child.Dock == Dock.Right)
					r = child;

				if(child.Dock == Dock.Bottom)
					b = child;

				if(child.Dock == Dock.Fill)
					f = child;
			}

			Margins m = composite.Margins;
			Size size = composite.Size;
			Point location = composite.Location;

			int top = location.Y + m.Top;
			int bottom = location.Y + size.Height - m.Bottom;
			int left = location.X + m.Left;
			int right = location.X + size.Width - m.Right;

			if(t != null)
			{
				Size s = t.Size;

				t.Arrange(new Rectangle(left, top, right - left, s.Height));

				top = top + s.Height + _spacing.Height;
			}

			if(b != null)
			{
				Size s = b.Size;

				b.Arrange(new Rectangle(left, bottom - s.Height, right - left, s.Height));

				bottom = bottom - (s.Height + _spacing.Height);
			}

			if(r != null)
			{
				Size s = r.Size;

				r.Arrange(new Rectangle(right - s.Width, top, s.Width, bottom - top));

				right = right - (s.Width + _spacing.Width);
			}

			if(l != null)
			{
				Size s = l.Size;

				l.Arrange(new Rectangle(left, top, s.Width, bottom - top));

				left = left + s.Width + _spacing.Width;
			}

			if(f != null)
				f.Arrange(new Rectangle(left, top, right - left, bottom - top));
		}
	
		public Size Measure(ILayoutComposite composite, Size availableSize)
		{
			ILayoutComponent l = null;
			ILayoutComponent t = null;
			ILayoutComponent r = null;
			ILayoutComponent b = null;
			ILayoutComponent f = null;

			foreach(ILayoutComponent child in composite.Children)
			{
				if(child.Dock == Dock.Left)
					l = child;

				if(child.Dock == Dock.Top)
					t = child;

				if(child.Dock == Dock.Right)
					r = child;

				if(child.Dock == Dock.Bottom)
					b = child;

				if(child.Dock == Dock.Fill)
					f = child;
			}

			int w = 0;
			int h = 0;

			Size s = Size.Empty;

			if(r != null)
			{
				s = r.Measure();
				w = s.Width + _spacing.Width;
				h = Math.Max(h, s.Height);
			}

			if(l != null)
			{
				s = l.Measure();
				w = s.Width + _spacing.Width;
				h = Math.Max(h, s.Height);
			}

			if(f != null)
			{
				s = f.Measure();
				w = w + s.Width;
				h = Math.Max(h, s.Height);
			}

			if(t != null)
			{
				s = t.Measure();
				w = Math.Max(w, s.Width);
				h = h + s.Height + _spacing.Height;
			}

			if(b != null)
			{
				s = b.Measure();
				w = Math.Max(w, s.Width);
				h = h + s.Height + _spacing.Height;
			}

			Margins m = composite.Margins;

			_size.Width = w + m.Width;
			_size.Height = h + m.Height;

			return _size;
		}

		#endregion Methods

		#region Properties

		public Size Size
		{
			get { return _size; }
		}

		public Size Spacing
		{
			get { return _spacing; }
			set { if(Size.Equals(_spacing, value) == false) { _spacing = value; } }
		}

		#endregion Properties

		#region Fields

		Size						_spacing = Size.Empty;
		Size						_size = Size.Empty;

		#endregion Fields
	}
}
