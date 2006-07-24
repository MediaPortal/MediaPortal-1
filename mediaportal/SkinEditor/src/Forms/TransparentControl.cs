using System;
using System.Drawing;

namespace SkinEditor.Forms
{
	/// <summary>
	/// Summary description for TransparentControl.
	/// </summary>
	public class TransparentControl : TranspControl.TranspControl
	{
		private Bitmap bitmap;
		public TransparentControl() {
			bitmap = new Bitmap("d:\\Tools\\MediaPortal\\skin\\mce\\Media\\alarm_clock.png");
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			e.Graphics.DrawImage(bitmap,0,0,bitmap.Width,bitmap.Height);
			//e.Graphics.DrawRectangle(new Pen(Color.Black,1.0f),0,0,Width-1,Height-1);
		}

	}
}
