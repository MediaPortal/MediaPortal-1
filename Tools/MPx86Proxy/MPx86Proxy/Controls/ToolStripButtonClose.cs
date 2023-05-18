using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MPx86Proxy.Controls
{
    public class ToolStripButtonClose : ToolStripButton
    {
        public ToolStripButtonClose()
            : base()
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.Owner != null)
            {
                ToolStripRenderer renderer = base.Owner.Renderer;
                renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));

                Pen pen = Renderer.Renderer.GetCachedPen2(this.Enabled ? this.ForeColor : SystemColors.GrayText);

                //int iX = e.ClipRectangle.X;
                //int iY = e.ClipRectangle.Y;
                //int iW = e.ClipRectangle.Width;
                //int iH = e.ClipRectangle.Height;

                int iX = 0;
                int iY = 0;
                int iW = this.Width;
                int iH = this.Height;

                int iOffsetBorder = (int)(0.28 * iW);
                int iOffset = this.Pressed ? 1 : 0;

                e.Graphics.DrawLine(pen,
                    iX + iOffsetBorder + iOffset,
                    iY + iOffsetBorder + iOffset,
                    iX + iW - iOffsetBorder + iOffset,
                    iY + iH - iOffsetBorder + iOffset
                    );

                e.Graphics.DrawLine(pen,
                    iX + iW - iOffsetBorder + iOffset,
                    iY + iOffsetBorder + iOffset,
                    iX + iOffsetBorder + iOffset,
                    iY + iH - iOffsetBorder + iOffset
                    );
            }

            //if (base.Owner != null)
            //{
            //    ToolStripRenderer renderer = base.Renderer;
            //    renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
            //    if ((DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
            //    {
            //        ToolStripItemImageRenderEventArgs toolStripItemImageRenderEventArgs = new ToolStripItemImageRenderEventArgs(e.Graphics, this, base.InternalLayout.ImageRectangle);
            //        toolStripItemImageRenderEventArgs.ShiftOnPress = true;
            //        renderer.DrawItemImage(toolStripItemImageRenderEventArgs);
            //    }
            //    if ((DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
            //    {
            //        renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, Text, base.InternalLayout.TextRectangle, ForeColor, Font, base.InternalLayout.TextFormat));
            //    }
            //}
        }
    }
}
