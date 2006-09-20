using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkinEditor.Design.Mask
{
	/// <summary>
	/// Summary description for MaskRight.
	/// </summary>
	public class MaskRight : MaskComponent {

		public MaskRight(ControlMask mask) : base(mask) {
			nodes = new Rectangle[1];
			nodes[0] = new Rectangle(0,0,mask.NodeSize,mask.NodeSize);
		}

		public override void Initialize() {
			if (mask != null && mask.SelectedControl != null) {
				Left = mask.SelectedControl.Left + mask.SelectedControl.Width;
				Top = mask.SelectedControl.Top;
				Width = mask.NodeSize;
				Height = mask.SelectedControl.Height;
				nodes[0].Location = new Point(0, Height/2 - mask.NodeSize/2);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			if (mask.MoveDrag == false && mask.ResizeDrag == false) {
				if (nodes[0].Contains(e.X,e.Y) && mask.SizeLocked == false) {
					if (Cursor != Cursors.SizeWE) {
						Cursor = Cursors.SizeWE;
						return;
					}
				} else if (mask.LocationLocked == false) {
					if (Cursor != Cursors.SizeAll) {
						Cursor = Cursors.SizeAll;
						return;
					}
				} else {
					if (Cursor != Cursors.Default) {
						Cursor = Cursors.Default;
						return;
					}
				}
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			if (Cursor == Cursors.Default) {
				return;
			} else if (Cursor == Cursors.SizeAll) {
				mask.MoveDrag = true;
			} else if (Cursor == Cursors.SizeWE) {
				mask.ResizeDrag = true;
				mask.ResizeNodeIndex = 4;
			}
			base.OnMouseDown(e);
		}

		protected override void OnPaint(PaintEventArgs e) {
			// Paint Bar
			e.Graphics.FillRectangle(mask.BarBrush, ClientRectangle);
			// Paint Nodes
			if (mask.SizeLocked) {
				e.Graphics.FillRectangle(mask.DisabledNodeBrush,nodes[0]);
			} else {
				e.Graphics.FillRectangle(mask.NodeBrush,nodes[0]);
			}
			e.Graphics.DrawRectangle(mask.NodePen,nodes[0].Left,nodes[0].Top,nodes[0].Width-1,nodes[0].Height-1);
		}

	}
}
