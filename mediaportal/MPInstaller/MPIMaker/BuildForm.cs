#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.IO;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class Build_dialog : MPInstallerForm
  {
    public MPinstallerStruct _struct = new MPinstallerStruct();

    public Build_dialog()
    {
      InitializeComponent();
    }

    public Build_dialog(MPinstallerStruct st)
    {
      _struct = st;
      InitializeComponent();
      textBox1.Text = _struct.BuildFileName;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        textBox1.Text = saveFileDialog1.FileName;
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      _struct.BuildFileName = textBox1.Text;
      this.Hide();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      if (!String.IsNullOrEmpty(textBox1.Text))
      {
        _struct.BuildFileName = textBox1.Text;
        _struct.BuilFile(listBox1, progressBar1);
      }
      else
      {
        MessageBox.Show("File name is mandatory !", "Stop");
        textBox1.Focus();
      }
    }

    public void onbuild()
    {
    }

    private void Build_dialog_Load(object sender, EventArgs e)
    {
      listBox1.Items.Clear();
    }


    private void button4_Click(object sender, EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;
      if (!String.IsNullOrEmpty(textBox1.Text))
      {
        if (checkBox1.Checked)
        {
          _struct.BuildFileName = textBox1.Text;
          _struct.BuilFile(listBox1, progressBar1);
          listBox1.SelectedIndex = listBox1.Items.Count - 1;
          listBox1.Refresh();
          listBox1.Update();
          string tempfile = Path.GetDirectoryName(textBox1.Text) + @"\" +
                            Path.GetFileNameWithoutExtension(textBox1.Text) + ".xml";
          MPInstallHelper temp_mpih = new MPInstallHelper();
          MPpackageStruct pk = new MPpackageStruct();
          pk.LoadFromFile(textBox1.Text);
          temp_mpih.Add(pk);
          temp_mpih.FileName = tempfile;
          temp_mpih.SaveToFile();
          listBox1.Items.Add("Config file created !");
          listBox1.Refresh();
          listBox1.Update();
        }
      }
      else
      {
        MessageBox.Show("File name is mandatory !", "Stop");
        textBox1.Focus();
      }
      this.Cursor = Cursors.Arrow;
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBox1.Checked)
      {
        textBox2.Enabled = false;
        textBox3.Enabled = false;
        textBox4.Enabled = false;
      }
      else
      {
        textBox2.Enabled = true;
        textBox3.Enabled = true;
        textBox4.Enabled = true;
      }
    }

    private void button5_Click(object sender, EventArgs e)
    {
    }
  }
}