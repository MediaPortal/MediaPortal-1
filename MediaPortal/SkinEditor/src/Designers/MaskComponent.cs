using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkinEditor.Design.Mask
{
	/// <summary>
	/// MaskComponent
	/// </summary>
	public abstract class MaskComponent : Control {

		protected ControlMask mask;
		protected Rectangle[] nodes;

		public MaskComponent(ControlMask mask) {
			this.mask = mask;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			BackColor = Color.Transparent;
		}

		public abstract void Initialize();

	}
}
