#region Copyright (C) 2005-2012 Team MediaPortal
// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class FutabaMDM166A_AdvancedSetupForm : Form
    {
        public class Matrix
        {
            public bool led { get; set; }                   // keeps on/off state
            public Rectangle rect { get; set; }             // keeps segment rectangle
        }

        private List<Matrix> matrix = new List<Matrix>();   // temporary character matrix

        LedDisplay ld;                                      // stores the calling object
        ulong bits;

        public FutabaMDM166A_AdvancedSetupForm(LedDisplay ld)
        {
            InitializeComponent();

            this.ld = ld;       // get the current leddisplay for color and other settings
            bits = 0;           // empty display
        }

        private void DesignForm_Load(object sender, EventArgs e)
        {
            // get horizontal and vertical number of segments (6x8 now)
            this.pbCharDesign.Size = new System.Drawing.Size((int) ld.HorizontalSegments * 25,
                                                            (int) ld.VerticalSegments *25);
            this.pbCharPreview.Size = new System.Drawing.Size((int) ld.HorizontalSegments * 6,
                                                            (int) ld.VerticalSegments * 6);
            // get control backcolor
            pbCharDesign.BackColor = ld.BackColor;
            pbCharPreview.BackColor = ld.BackColor;

            // get space between segments
            nudSpace.Value = ld.SegmentSpace; 

            // get the current list af characters in dictionary and show them in combobox
            cbSelectCharacter.Items.AddRange(ld.GetKeys().ToArray());

            // select the first character, if any
            if (cbSelectCharacter.Items.Count > 0)
                cbSelectCharacter.SelectedIndex = 0;

            txtNoOfChars.Text=cbSelectCharacter.Items.Count.ToString(); // no. of chars in dictiobary
        }

        // paintbox uses the same colors as the control,
        // the grid will not show if backcolor is grey, g.DrawRectangle(Pens.Gray, m.rect);
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int width = pbCharDesign.Width / (int) ld.HorizontalSegments;
            int height = pbCharDesign.Height / (int) ld.VerticalSegments;

            ulong bitsCopy = bits;              // make a copy to work with

            matrix.Clear();                     // empty the matrix and make a new one

            using (SolidBrush segmentBrush = new SolidBrush(ld.ForeColor))
            {
                for (int y = 0; y < ld.VerticalSegments; y++)           // vertical row
                {
                    for (int x = 0; x < ld.HorizontalSegments; x++)     // horizontal row
                    {
                        Matrix m = new Matrix();
                        m.rect = new Rectangle(x * width, y * height, width, height);

                        if ((bitsCopy & 1) != 0)            // if bit 1 is set, fill segment
                        {
                            m.led = true;                   // lights on
                            Rectangle segment = Rectangle.Inflate(m.rect, -4, -4);
                            g.FillEllipse(segmentBrush, segment);
                        }
                        else
                            m.led = false;                  // lights off

                        bitsCopy = bitsCopy >> 1;           // roll bits to the right to get next in row

                        matrix.Add(m);                      // add this segment to the matrix list

                        g.DrawRectangle(Pens.Gray, m.rect);
                    }
                }
            }

            matrix.Reverse();                               // reverse to get matrix in right order
        }

        // show character in a smaller size
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            ld.DrawChar(bits, e.Graphics, pbCharPreview.ClientRectangle);
        }

        // this draws and hides segments as user clicks in the paintbox
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtNewCharacter.Text == string.Empty)
            {
                MessageBox.Show("Please enter character to map first!");
                return;
            }

            Graphics g = pbCharDesign.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            bits = 0;                                       // build new bit definition

            // start with the highest bit in this matrix, x*y-1
            ulong highBit = (ulong) Math.Pow(2, (ld.HorizontalSegments * ld.VerticalSegments) - 1);

            foreach (Matrix m in matrix)                    // run trough the matrix
            {
                if (m.rect.Contains(e.X, e.Y))              // mouse clicked inside a segment rectangle?
                {
                    m.led = m.led == true ? false : true;   // switch state of led
                    Rectangle segment = m.rect;             // make copy of rectangle

                    if (m.led == true)                      // draw segment
                    {
                        segment.Inflate(-4, -4);            // make a little bit smaller
                        g.FillEllipse(new SolidBrush(ld.ForeColor), segment);
                    }
                    else                                    // erase segment
                    {
                        segment.Inflate(-2, -2);  // make it bigger to cover edges of colored circle
                        g.FillEllipse(new SolidBrush(ld.BackColor), segment);
                    }
                }

                if (m.led == true)                          
                    bits = bits | highBit;                  // set current bit

                highBit >>= 1;                              // rotate bit to the right, to set next bit
            }

            g.Dispose();

            pbCharPreview.Invalidate();                       // clear small picturebox
        }

        // save character layout to dictionary
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (ld.FindChar(txtNewCharacter.Text[0]))  // check if it already exists
                if (MessageBox.Show("Replace existing char?", "Add character", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    return;

            // add to dictionary
            ld.AddChar(txtNewCharacter.Text[0], bits);

            // write definitions to file
            ld.WriteCharDef();

            // renew combobox list
            cbSelectCharacter.Items.Clear();
            cbSelectCharacter.Items.AddRange(ld.GetKeys().ToArray());
            cbSelectCharacter.SelectedIndex = cbSelectCharacter.Items.Count - 1; // jump to the last inserted
            txtNewCharacter.Text = cbSelectCharacter.SelectedItem.ToString();

            // update number of characters in dictionary
            lblNoOfChars.Text = "no. of chars " + cbSelectCharacter.Items.Count.ToString();
        }

        // could add a warning if not saved
        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // just erases pictureboxes and clear bits
        private void clearButton_Click(object sender, EventArgs e)
        {
            bits = 0;
            pbCharDesign.Invalidate();
            pbCharPreview.Invalidate();
        }

        // change space between segments in character
        // this is updated in the smaller picturebox
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ld.SegmentSpace = (int) nudSpace.Value;
            pbCharPreview.Invalidate();
        }

        // if character changed, try to find it in the dictionary and redraw
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (txtNewCharacter.Text != String.Empty)
            {
                bits = ld.GetValue(txtNewCharacter.Text[0]);

                pbCharDesign.Invalidate();
                pbCharPreview.Invalidate();
            }
        }

        // update textbox with character from index
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtNewCharacter.Text = cbSelectCharacter.SelectedItem.ToString();
        }


    }
}