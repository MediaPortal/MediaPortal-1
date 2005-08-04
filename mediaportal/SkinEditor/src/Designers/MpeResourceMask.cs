using System;
using System.Drawing;
using System.Windows.Forms;

using Mpe.Controls;

namespace Mpe.Designers
{
	public class MpeResourceMask : Control {

		#region Variables
		private MpeControl control;
		#endregion

		[System.Runtime.InteropServices.DllImport("user32")]
		public static extern IntPtr GetWindowDC(IntPtr hWnd);

		[System.Runtime.InteropServices.DllImport("user32")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		public MpeResourceMask() {
			//
		}

		public MpeControl SelectedControl {
			get {
				return control;
			}
			set {
				control = value;
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (control != null) {
				IntPtr hDC = GetWindowDC(IntPtr.Zero);
				Graphics g = Graphics.FromHdc(hDC);
				g.FillRectangle(new SolidBrush(Color.Black),control.ClientRectangle);
				ReleaseDC(IntPtr.Zero,hDC);
			}
			e.Graphics.FillRectangle(new SolidBrush(Color.White),this.ClientRectangle);

		}

	}
}
