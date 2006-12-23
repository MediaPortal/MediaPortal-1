#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.InputDevices
{
  public partial class NewButtonForm : Form
  {
    private bool _accepted = false;

    public bool Accepted
    {
      get { return _accepted; }
    }

    public string ButtonCode
    {
      get { return textBoxButton.Text; }
      set
      {
        textBoxButton.Text = value;
        this.Text = "Edit button";
      }
    }

    public string ButtonName
    {
      get
      {
        if (textBoxButton.Text != string.Empty)
          return textBoxName.Text;
        else
          return string.Empty;
      }
      set
      {
        textBoxName.Text = value;
        this.Text = "Edit button";
      }
    }

    public NewButtonForm()
    {
      InitializeComponent();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      _accepted = true;
      this.Close();
    }
  }
}