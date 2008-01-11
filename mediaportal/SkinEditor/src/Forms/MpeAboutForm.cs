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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mpe.Forms
{
  /// <summary>
  /// Summary description for AboutForm.
  /// </summary>
  public class MpeAboutForm : Form
  {
    private PictureBox pictureBox1;
    private Button button1;
    private Panel panel2;
    private Label label1;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    [DllImport("user32")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    /// <summary>
    /// Creates a new AboutForm
    /// </summary>
    public MpeAboutForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MpeAboutForm));
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.button1 = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image) (resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(8, 8);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(361, 96);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // button1
      // 
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button1.Location = new System.Drawing.Point(280, 144);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(80, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "OK";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // panel2
      // 
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 112);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(360, 24);
      this.label1.TabIndex = 2;
      this.label1.Text = "Powered By DotNetMagic (www.dotnetmagic.com)";
      // 
      // MpeAboutForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(376, 176);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.pictureBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MpeAboutForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MediaPortal - Skin Editor";
      this.Load += new System.EventHandler(this.AboutForm_Load);
      this.ResumeLayout(false);
    }

    #endregion

    private void button1_Click(object sender, System.EventArgs e)
    {
      IntPtr hDC = GetWindowDC(IntPtr.Zero);
      Graphics gdc = Graphics.FromHdc(hDC);
      Color c = Color.FromArgb(128, 0, 0, 0);
      gdc.DrawRectangle(new Pen(c, 2.0f), 10, 10, 100, 100);
      ReleaseDC(IntPtr.Zero, hDC);
      Close();
    }

    private void AboutForm_Load(object sender, System.EventArgs e)
    {
      CenterToParent();
    }

    private void OnClick(object sender, EventArgs e)
    {
      MpeLog.Info("Testing");
    }
  }
}