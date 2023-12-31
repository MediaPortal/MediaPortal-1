using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MPx86Proxy.Controls.Renderer
{
    public class ToolStripRendererCustom : ToolStripProfessionalRenderer
    {
        //private static Pen _PenBlack = new Pen(Color.Black);
        //private static SolidBrush _BrushSilver = new SolidBrush(Color.Silver);

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripButton btn = e.Item as ToolStripButton;
            if (btn != null && btn.Selected)
            {
                //Rectangle bounds = new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1);

                //if (btn.Pressed)
                //    e.Graphics.FillRectangle(_BrushSilver, bounds);

                //e.Graphics.DrawRectangle(_PenBlack, bounds);

                base.OnRenderButtonBackground(e);
            }
            //else
            //    base.OnRenderButtonBackground(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle imageRectangle = e.ImageRectangle;
            Image image = e.Image;

            if (imageRectangle != Rectangle.Empty && image != null)
            {
                if (e.Item.Pressed)
                    imageRectangle.Offset(1, 1);

                if (!e.Item.Enabled || (e.Item is ToolStripButton && ((ToolStripButton)e.Item).CheckOnClick && !((ToolStripButton)e.Item).Checked))
                {
                    //base.OnRenderItemImage(e);

                    image = CreateDisabledImage(image);
                    e.Graphics.DrawImage(image, imageRectangle);
                    image.Dispose();

                }
                else if (e.Item.ImageScaling == ToolStripItemImageScaling.None)
                {
                    e.Graphics.DrawImage(image, imageRectangle, new Rectangle(Point.Empty, imageRectangle.Size), GraphicsUnit.Pixel);
                }
                else
                {
                    e.Graphics.DrawImage(image, imageRectangle);
                }
            }
        }
    }
}
