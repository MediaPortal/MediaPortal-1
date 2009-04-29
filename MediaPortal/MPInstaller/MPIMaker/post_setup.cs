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

namespace MediaPortal.MPInstaller
{
  public partial class post_setup : MPInstallerForm
  {
    public MPinstallerStruct _struct;

    public post_setup()
    {
      InitializeComponent();
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBox1.SelectedIndex)
      {
        case 0:
          comboBox2.Items.Clear();
          comboBox2.Items.Add("MediaPortal.exe");
          comboBox2.Items.Add("Configuration.exe");
          foreach (MPIFileList fl in _struct.FileList)
          {
            if ((fl.Type == MPinstallerStruct.OTHER_TYPE) &&
                ((Path.GetExtension(fl.FileName).ToUpper() == ".BAT") ||
                 (Path.GetExtension(fl.FileName).ToUpper() == ".EXE")))
            {
              comboBox2.Items.Add(fl.FileNameShort);
            }
          }
          break;
        case 1:
          comboBox2.Items.Clear();
          foreach (MPIFileList fl in _struct.FileList)
          {
            if (fl.Type == MPinstallerStruct.PLUGIN_TYPE)
            {
              comboBox2.Items.Add(fl.FileNameShort);
            }
          }
          break;
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      _struct.AddAction(new ActionInfo("POSTSETUP", comboBox1.SelectedIndex, comboBox2.Text));
      this.Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void post_setup_Load(object sender, EventArgs e)
    {
      ActionInfo a = _struct.FindAction("POSTSETUP");
      if (a != null)
      {
        comboBox1.SelectedIndex = a.Id;
        comboBox2.Text = a.Command;
      }
    }
  }
}