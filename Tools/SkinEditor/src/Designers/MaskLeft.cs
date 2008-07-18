#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkinEditor.Design.Mask
{
	/// <summary>
	/// Summary description for MaskLeft.
	/// </summary>
	public class MaskLeft : MaskComponent {

		public MaskLeft(ControlMask mask) : base(mask) {
			nodes = new Rectangle[1];
			nodes[0] = new Rectangle(0,0,mask.NodeSize,mask.NodeSize);
		}

		public override void Initialize() {
			if (mask != null && mask.SelectedControl != null) {
				Left = mask.SelectedControl.Left - mask.NodeSize;
				Top = mask.SelectedControl.Top;
				Width = mask.NodeSize;
				Height = mask.SelectedControl.Height;
				nodes[0].Location = new Point(0, Height/2 - mask.NodeSize/2);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			if (mask.MoveDrag == false && mask.ResizeDrag == false) {
				if (nodes[0].Contains(e.X,e.Y) && mask.LocationLocked == false && mask.SizeLocked == false) {
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
				mask.ResizeNodeIndex = 3;
			}
			base.OnMouseDown(e);
		}

		protected override void OnPaint(PaintEventArgs e) {
			// Paint Bar
			e.Graphics.FillRectangle(mask.BarBrush, ClientRectangle);
			// Paint Nodes
			if (mask.SizeLocked || mask.LocationLocked) {
				e.Graphics.FillRectangle(mask.DisabledNodeBrush,nodes[0]);
			} else {
				e.Graphics.FillRectangle(mask.NodeBrush,nodes[0]);
			}
			e.Graphics.DrawRectangle(mask.NodePen,nodes[0].Left,nodes[0].Top,nodes[0].Width-1,nodes[0].Height-1);
		}
	}
}
