using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace MPx86Proxy.Controls.Renderer
{
    public class Renderer
    {
        private static System.Collections.Hashtable _Pens = new System.Collections.Hashtable(8);
        private static System.Collections.Hashtable _Pens2 = new System.Collections.Hashtable(8);
        private static System.Collections.Hashtable _Brushes = new System.Collections.Hashtable(8);
        private static System.Collections.Hashtable _Bitmaps = new System.Collections.Hashtable(8);

        public static void RenderProgressBar(Graphics g, Rectangle bounds, Font font,
            float fBarPercentage, string strText, Color colorFont, Color colorBar, bool bSelected, int iSelColorDimm = -32,
            bool bOutter = false, bool bLeftEdge = true, bool bDrawDefaultBackground = true)
        {
            RenderProgressBar(g, bounds, font, fBarPercentage, strText, colorFont, colorBar,
             bSelected ? SystemColors.HighlightText : SystemColors.ControlText,
             bSelected ? SystemColors.Highlight : SystemColors.Window, 
             bSelected, iSelColorDimm, bOutter, bLeftEdge, bDrawDefaultBackground);
        }
        public static void RenderProgressBar(Graphics g, Rectangle bounds, Font font,
            float fBarPercentage, string strText, Color colorFont, Color colorBar, Color colorDefFore, Color colorDefBack,  bool bSelected, int iSelColorDimm = -32,
            bool bOutter = false, bool bLeftEdge = true, bool bDrawDefaultBackground = true)
        {
            //Dimming if selected
            if (bSelected)
                colorBar = ColorAdjustBrightness(colorBar, iSelColorDimm);

            //Bar width
            int iBarWidth = (int)(fBarPercentage * bounds.Width);

            if (iBarWidth <= 0)
            {
                //No bar
                if (bDrawDefaultBackground)
                    g.FillRectangle(GetCachedBrush(colorDefBack), bounds);

                if (!string.IsNullOrWhiteSpace(strText))
                {
                    bounds.Offset(0, 1);
                    TextRenderer.DrawText(g, strText, font, bounds, colorDefFore, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            }
            else if (iBarWidth >= bounds.Width)
            {
                //Full bar
                drawBar(g, colorBar, bounds, bOutter, bLeftEdge);

                if (!string.IsNullOrWhiteSpace(strText))
                {
                    bounds.Offset(0, 1);
                    TextRenderer.DrawText(g, strText, font, bounds, colorFont, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            }
            else
            {
                //Size of the text
                Size size = TextRenderer.MeasureText(g, strText, font, Size.Empty, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                //Individual bounds
                Rectangle rectText = new Rectangle(bounds.X + (bounds.Width - size.Width) / 2, bounds.Y + 1, size.Width, bounds.Height - 1);
                Rectangle rectBar = new Rectangle(bounds.X, bounds.Y, iBarWidth, bounds.Height);
                Rectangle rectRight = new Rectangle(bounds.X + iBarWidth, bounds.Y, bounds.Width - iBarWidth, bounds.Height);

                //Fill right part
                if (bDrawDefaultBackground)
                    g.FillRectangle(GetCachedBrush(colorDefBack), rectRight);

                //Draw text
                if (rectRight.X < rectText.Right && !string.IsNullOrWhiteSpace(strText))
                    TextRenderer.DrawText(g, strText, font, rectText, colorDefFore, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                //Fill bar
                drawBar(g, colorBar, rectBar, bOutter, bLeftEdge);

                //Draw text on the bar
                rectText = new Rectangle(rectText.X, rectText.Y, rectRight.X - rectText.X, rectText.Height);
                if (rectRight.X > rectText.X && !string.IsNullOrWhiteSpace(strText))
                    TextRenderer.DrawText(g, strText, font, rectText, colorFont, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }

        }
        public static void RenderProgressBar(Graphics g, Rectangle bounds, Font font, float[] fBarPercentages, string strText,
            Color[] colorsFont, Color[] colorsBar, bool bSelected, int iSelColorDimm = -32, bool bOutter = false, bool bLeftEdge = true, bool bDrawDefaultBackground = true)
        {
            RenderProgressBar(g, bounds, font, fBarPercentages, strText, colorsFont, colorsBar,
                bSelected ? SystemColors.HighlightText : SystemColors.ControlText,
                bSelected ? SystemColors.Highlight : SystemColors.Window,
                bSelected, iSelColorDimm, bOutter, bLeftEdge, bDrawDefaultBackground);
        }
        public static void RenderProgressBar(Graphics g, Rectangle bounds, Font font, float[] fBarPercentages, string strText,
            Color[] colorsFont, Color[] colorsBar, Color colorDefFore, Color colorDefBack, bool bSelected, int iSelColorDimm = -32, bool bOutter = false, bool bLeftEdge = true, bool bDrawDefaultBackground = true)
        {
            if (fBarPercentages != null && fBarPercentages.Length == 1 && colorsFont != null && colorsFont.Length >= 1 && colorsBar != null && colorsBar.Length >= 1)
            {
                RenderProgressBar(g, bounds, font, fBarPercentages[0], strText, colorsFont[0], colorsBar[0], colorDefFore, colorDefBack, bSelected, iSelColorDimm, bOutter, bLeftEdge, bDrawDefaultBackground);
                return;
            }

            bool bPrintText = !string.IsNullOrWhiteSpace(strText);

            //Bar width
            bool bNoBar = fBarPercentages == null || fBarPercentages.Length == 0 || fBarPercentages.All(f => f <= 0);
            bool bFullBar = fBarPercentages != null && fBarPercentages.Length > 0 && fBarPercentages.All(f => f >= 1.0F);

            //Default background
            if (bDrawDefaultBackground)
                g.FillRectangle(GetCachedBrush(colorDefBack), bounds);

            if (bNoBar)
            {
                //No bar
                if (!string.IsNullOrWhiteSpace(strText))
                {
                    bounds.Offset(0, 1);
                    TextRenderer.DrawText(g, strText, font, bounds, colorDefFore, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            }
            else if (bFullBar)
            {
                //Full bar
                drawBar(g, bSelected ? ColorAdjustBrightness(colorsBar[0], iSelColorDimm) : colorsBar[0], bounds, bOutter, bLeftEdge);

                if (bPrintText)
                {
                    bounds.Offset(0, 1);
                    TextRenderer.DrawText(g, strText, font, bounds, colorsFont[0], TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            }
            else
            {
                //Print default full text
                if (bPrintText)
                    TextRenderer.DrawText(g, strText, font, new Rectangle(bounds.X, bounds.Y + 1, bounds.Width, bounds.Height), 
                        bSelected ? SystemColors.HighlightText : SystemColors.ControlText, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                if (colorsBar == null || colorsBar.Length < fBarPercentages.Length || colorsFont == null || colorsFont.Length < fBarPercentages.Length)
                    return;


                //Size of the text
                Size size = TextRenderer.MeasureText(g, strText, font, Size.Empty, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                Rectangle rectText = new Rectangle(bounds.X + (bounds.Width - size.Width) / 2, bounds.Y + 1, size.Width, bounds.Height - 1);

                Dictionary<int, Bitmap> bmps = new Dictionary<int, Bitmap>();

                int iWidth;
                Rectangle rectBar;

                for (int iIdx = 0; iIdx < fBarPercentages.Length; iIdx++)
                {
                    float f = fBarPercentages[iIdx];

                    if (f <= 0)
                        continue;
                    else if (f > 1.0F)
                        f = 1.0F;

                    //Calculate offset & width of the piece
                    int iOffset = (int)(((double)iIdx / fBarPercentages.Length) * bounds.Width);


                    if (fBarPercentages.Length == 1)
                        iWidth = bounds.Width;
                    else
                    {
                        int iOffsetNext;
                        iOffsetNext = (int)(((double)(iIdx + 1) / fBarPercentages.Length) * bounds.Width);
                        iWidth = iOffsetNext - iOffset;

                        if (iOffsetNext > bounds.Width)
                            iWidth -= iOffsetNext - bounds.Width;
                    }

                    //Fill piece
                    rectBar = new Rectangle(bounds.X + iOffset, bounds.Y, (int)(f * iWidth), bounds.Height);
                    if (rectBar.Width > 0)
                    {
                        //Draw the piece
                        drawBar(g, bSelected ? ColorAdjustBrightness(colorsBar[iIdx], iSelColorDimm) : colorsBar[iIdx], rectBar, bOutter, bLeftEdge && iIdx == 0);

                        //Draw text on the piece
                        if (bPrintText && rectBar.X <= rectText.Right && rectBar.Right >= rectText.X)
                        {
                            //Get prerendered text of fontColor
                            Bitmap bmp;
                            int iArgb = colorsFont[iIdx].ToArgb();
                            if (!bmps.TryGetValue(iArgb, out bmp))
                            {
                                bmp = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                Graphics gBmp = Graphics.FromImage(bmp);

                                TextRenderer.DrawText(gBmp, strText, font, new Rectangle(new Point(0, 1), bmp.Size), colorsFont[iIdx],
                                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                                gBmp.Dispose();

                                bmps.Add(iArgb, bmp);
                            }

                            //Print part of the text over the piece
                            g.DrawImage(bmp, rectBar, rectBar.X - bounds.X, 0, rectBar.Width, rectBar.Height, GraphicsUnit.Pixel);
                        }
                    }
                }

                bmps = null;
            }
        }

        public static Pen GetCachedPen(Color color)
        {
            Pen pen = (Pen)_Pens[color];
            if (pen == null)
            {
                pen = new Pen(color);
                _Pens.Add(color, pen);
            }
            return pen;
        }
        public static Pen GetCachedPen2(Color color)
        {
            Pen pen = (Pen)_Pens2[color];
            if (pen == null)
            {
                pen = new Pen(color, 2F);
                _Pens2.Add(color, pen);
            }
            return pen;
        }

        public static SolidBrush GetCachedBrush(Color color)
        {
            SolidBrush brush = (SolidBrush)_Brushes[color];
            if (brush == null)
            {
                brush = new SolidBrush(color);
                _Brushes.Add(color, brush);
            }
            return brush;
        }

        public static Bitmap GetCachedBitmap(Icon icon)
        {
            if (icon == null)
                return null;

            Bitmap bmp = (Bitmap)_Bitmaps[icon];
            if (bmp == null)
            {
                bmp = icon.ToBitmap();
                _Bitmaps.Add(icon, bmp);
            }

            return bmp;
        }

        public static Color ColorAdjustBrightness(Color color, int iValue)
        {
            int iR = Math.Max(0, Math.Min(255, color.R + iValue));
            int iG = Math.Max(0, Math.Min(255, color.G + iValue));
            int iB = Math.Max(0, Math.Min(255, color.B + iValue));

            return Color.FromArgb(iR, iG, iB);
        }

        public static void DrawRowEdges2(DataGridView table, DataGridViewCellPaintingEventArgs e)
        {
            DrawRowEdges2(table, e.Graphics, e.CellBounds, table.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected ? e.CellStyle.SelectionBackColor : e.CellStyle.BackColor);
        }
        public static void DrawRowEdges2(DataGridView table, Graphics g, Rectangle bounds, Color c)
        {
            //Bottom line(separation)
            //Pen pen = getCachedPen(table.DefaultCellStyle.BackColor);
            Pen pen = GetCachedPen(Color.Black);
            g.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

            //Dark edge
            pen = GetCachedPen(ColorAdjustBrightness(c, -32));
            g.DrawLine(pen, bounds.Left, bounds.Bottom - 2, bounds.Right, bounds.Bottom - 2);

            pen = GetCachedPen(ColorAdjustBrightness(c, -16));
            g.DrawLine(pen, bounds.Left, bounds.Bottom - 3, bounds.Right, bounds.Bottom - 3);

            //Light edge
            pen = GetCachedPen(ColorAdjustBrightness(c, 64));
            g.DrawLine(pen, bounds.Left, bounds.Top + 0, bounds.Right, bounds.Top + 0);

            pen = GetCachedPen(ColorAdjustBrightness(c, 32));
            g.DrawLine(pen, bounds.Left, bounds.Top + 1, bounds.Right, bounds.Top + 1);
        }

        public static void DrawRowEdges(DataGridView table, DataGridViewCellPaintingEventArgs e)
        {
            DrawRowEdges(table, e.Graphics, e.CellBounds, table.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected ? e.CellStyle.SelectionBackColor : e.CellStyle.BackColor);
        }
        public static void DrawRowEdges(DataGridView table, Graphics g, Rectangle bounds, Color c)
        {
            //Bottom line(separation)
            //Pen pen = getCachedPen(table.DefaultCellStyle.BackColor);
            Pen pen = GetCachedPen(Color.Black);
            g.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

            //Dark edge
            pen = GetCachedPen(ColorAdjustBrightness(c, -32));
            g.DrawLine(pen, bounds.Left, bounds.Bottom - 2, bounds.Right, bounds.Bottom - 2);

            //Light edge
            pen = GetCachedPen(ColorAdjustBrightness(c, 32));
            g.DrawLine(pen, bounds.Left, bounds.Top + 0, bounds.Right, bounds.Top + 0);
        }



        public static void PaintSortGlyphDirection(DataGridView grid, Graphics g, Point point, SortOrder sortOrder)
        {
            DataGridViewAdvancedBorderStyle advancedBorderStyle = grid.AdvancedColumnHeadersBorderStyle;

            Pen penDark = null;
            Pen penLight = null;
            GetContrastedPens(grid.ColumnHeadersDefaultCellStyle.BackColor, ref penDark, ref penLight);
            if (sortOrder == SortOrder.Ascending)
            {
                switch (advancedBorderStyle.Right)
                {
                    case DataGridViewAdvancedCellBorderStyle.Inset:
                        g.DrawLine(penLight, point.X, point.Y + 7 - 2, point.X + 4 - 1, point.Y);
                        g.DrawLine(penLight, point.X + 1, point.Y + 7 - 2, point.X + 4 - 1, point.Y);
                        g.DrawLine(penDark, point.X + 4, point.Y, point.X + 9 - 2, point.Y + 7 - 2);
                        g.DrawLine(penDark, point.X + 4, point.Y, point.X + 9 - 3, point.Y + 7 - 2);
                        g.DrawLine(penDark, point.X, point.Y + 7 - 1, point.X + 9 - 2, point.Y + 7 - 1);
                        return;

                    case DataGridViewAdvancedCellBorderStyle.Outset:
                    case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                        g.DrawLine(penDark, point.X, point.Y + 7 - 2, point.X + 4 - 1, point.Y);
                        g.DrawLine(penDark, point.X + 1, point.Y + 7 - 2, point.X + 4 - 1, point.Y);
                        g.DrawLine(penLight, point.X + 4, point.Y, point.X + 9 - 2, point.Y + 7 - 2);
                        g.DrawLine(penLight, point.X + 4, point.Y, point.X + 9 - 3, point.Y + 7 - 2);
                        g.DrawLine(penLight, point.X, point.Y + 7 - 1, point.X + 9 - 2, point.Y + 7 - 1);
                        return;
                }
                for (int i = 0; i < 4; i++)
                {
                    g.DrawLine(penDark, point.X + i, point.Y + 7 - i - 1, point.X + 9 - i - 1, point.Y + 7 - i - 1);
                }

                g.DrawLine(penDark, point.X + 4, point.Y + 7 - 4 - 1, point.X + 4, point.Y + 7 - 4);
            }
            else
            {
                switch (advancedBorderStyle.Right)
                {
                    case DataGridViewAdvancedCellBorderStyle.Inset:
                        g.DrawLine(penLight, point.X, point.Y + 1, point.X + 4 - 1, point.Y + 7 - 1);
                        g.DrawLine(penLight, point.X + 1, point.Y + 1, point.X + 4 - 1, point.Y + 7 - 1);
                        g.DrawLine(penDark, point.X + 4, point.Y + 7 - 1, point.X + 9 - 2, point.Y + 1);
                        g.DrawLine(penDark, point.X + 4, point.Y + 7 - 1, point.X + 9 - 3, point.Y + 1);
                        g.DrawLine(penDark, point.X, point.Y, point.X + 9 - 2, point.Y);
                        return;

                    case DataGridViewAdvancedCellBorderStyle.Outset:
                    case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
                    case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
                        g.DrawLine(penDark, point.X, point.Y + 1, point.X + 4 - 1, point.Y + 7 - 1);
                        g.DrawLine(penDark, point.X + 1, point.Y + 1, point.X + 4 - 1, point.Y + 7 - 1);
                        g.DrawLine(penLight, point.X + 4, point.Y + 7 - 1, point.X + 9 - 2, point.Y + 1);
                        g.DrawLine(penLight, point.X + 4, point.Y + 7 - 1, point.X + 9 - 3, point.Y + 1);
                        g.DrawLine(penLight, point.X, point.Y, point.X + 9 - 2, point.Y);
                        return;
                }
                for (int j = 0; j < 4; j++)
                {
                    g.DrawLine(penDark, point.X + j, point.Y + j + 2, point.X + 9 - j - 1, point.Y + j + 2);
                }

                g.DrawLine(penDark, point.X + 4, point.Y + 4 + 1, point.X + 4, point.Y + 4 + 2);
            }
        }

        public static int GetColorDistance(Color color1, Color color2)
        {
            int iRed = (int)(color1.R - color2.R);
            int iGreen = (int)(color1.G - color2.G);
            int iBlue = (int)(color1.B - color2.B);
            return iRed * iRed + iGreen * iGreen + iBlue * iBlue;
        }

        public static void GetContrastedPens(Color colorBaseLine, ref Pen penDark, ref Pen penLight)
        {
            int iColorDistDark = GetColorDistance(colorBaseLine, SystemColors.ControlDark);
            int iColorDistLight = GetColorDistance(colorBaseLine, SystemColors.ControlLightLight);
            if (SystemInformation.HighContrast)
            {
                if (iColorDistDark < 2000)
                    penDark = GetCachedPen(ControlPaint.DarkDark(colorBaseLine));
                else
                    penDark = GetCachedPen(SystemColors.ControlDark);

                if (iColorDistLight < 2000)
                {
                    penLight = GetCachedPen(ControlPaint.LightLight(colorBaseLine));
                    return;
                }

                penLight = GetCachedPen(SystemColors.ControlLightLight);
                return;
            }
            else
            {
                if (iColorDistDark < 1000)
                    penDark = Renderer.GetCachedPen(ControlPaint.Dark(colorBaseLine));
                else
                    penDark = Renderer.GetCachedPen(SystemColors.ControlDark);

                if (iColorDistLight < 1000)
                {
                    penLight = Renderer.GetCachedPen(ControlPaint.Light(colorBaseLine));
                    return;
                }

                penLight = Renderer.GetCachedPen(SystemColors.ControlLightLight);
                return;
            }
        }


        private static void drawBar(Graphics g, Color colorBar, Rectangle bounds, bool bOutter, bool bLeftEdge)
        {
            if (bounds.Width < 1)
                return;

            int iDimm;

            //Fill rectangle
            int iOff = (bLeftEdge ? 3 : 0);
            g.FillRectangle(GetCachedBrush(colorBar), bounds.X + iOff, bounds.Y + 3, bounds.Width - iOff, bounds.Height - 6);

            //Draw lower edge
            if (!bOutter)
                iDimm = 0x0C;
            else
                iDimm = -0x14;

            int iX = bounds.X + (bLeftEdge ? 1 : 0);

            if (iX <= bounds.Right - 1)
            {
                for (int i = bOutter ? 4 : 3; i >= 1; i--)
                {
                    Color c = ColorAdjustBrightness(colorBar, iDimm);

                    if (iX == bounds.Right - 1)
                        g.FillRectangle(GetCachedBrush(c), iX, bounds.Bottom - i, 1, 1);
                    else
                        g.DrawLine(GetCachedPen(c), iX, bounds.Bottom - i, bounds.Right - 1, bounds.Bottom - i);

                    iDimm <<= 1;
                }
            }


            //Draw upper & left edge
            if (!bOutter)
                iDimm = -0xA0;
            else
                iDimm = bOutter ? 0x60 : 0x40;

            for (int i = 0; i <= (bOutter ? 3 : 2); i++)
            {
                iX = bounds.X + (bLeftEdge ? i : 0);

                if (iX >= bounds.Right)
                    break;

                if (bOutter && iDimm > 0x40)
                    iDimm = 0x40;

                Color c = ColorAdjustBrightness(colorBar, iDimm);

                if (bLeftEdge)
                    g.DrawLine(GetCachedPen(c), bounds.X + i, bounds.Y + i, bounds.X + i, bounds.Bottom - 1 - i);

                if (iX == bounds.Right - 1)
                    g.FillRectangle(GetCachedBrush(c), iX, bounds.Y + i, 1, 1);
                else
                    g.DrawLine(GetCachedPen(c), iX, bounds.Y + i, bounds.Right - 1, bounds.Y + i);

                if (!bOutter)
                    iDimm += 0x40;
                else
                    iDimm >>= 1;
            }
        }
    }
}
