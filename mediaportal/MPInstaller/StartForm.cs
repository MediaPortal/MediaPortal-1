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
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class start_form : MPInstallerForm
  {
    public start_form()
    {
      InitializeComponent();
    }

    //private void button1_Click(object sender, EventArgs e)
    //{
    //  EditForm create_dlg = new EditForm();
    //  this.Hide();
    //  create_dlg.ShowDialog();
    //  this.Show();
    //}

    private void button2_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        this.Hide();
        install_Package(openFileDialog1.FileName);
        this.Show();
      }
    }

    private void install_Package(string fil)
    {
      wizard_1 wiz = new wizard_1();
      wiz.package.LoadFromFile(fil);
      if (wiz.package.isValid)
      {
        wiz.starStep();
      }
      else
      {
        MessageBox.Show("Invalid package !");
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      controlp cnt = new controlp();
      this.Hide();
      cnt.ShowDialog();
      this.Show();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void button4_Click_1(object sender, EventArgs e)
    {
      this.Hide();
      UpdateChecker checker = new UpdateChecker();
      checker.Check();
      this.Show();
    }

    private void label1_MouseEnter(object sender, EventArgs e)
    {
      ((Label) sender).Font = new Font("Verdana", 9.75F, ((FontStyle) ((FontStyle.Bold | FontStyle.Underline))),
                                       GraphicsUnit.Point, ((byte) (0)));
      ((Label) sender).ForeColor = Color.FromArgb(0x879996);
    }

    private void label1_MouseLeave(object sender, EventArgs e)
    {
      ((Label) sender).Font = new Font("Verdana", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte) (0)));
      ((Label) sender).ForeColor = Color.White;
    }
  }
}