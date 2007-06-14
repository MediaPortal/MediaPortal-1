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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;


using Gentle.Common;
using Gentle.Framework;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class FormEditCard : Form
  {
    Card _card;
    public FormEditCard()
    {
      InitializeComponent();
    }

    public Card Card
    {
      get
      {
        return _card;
      }
      set
      {
        _card = value;
      }
    }

    private void FormEditCard_Load(object sender, EventArgs e)
    {
      textBoxDecryptLimit.Text = _card.DecryptLimit.ToString();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
      try
      {
        _card.DecryptLimit = Int32.Parse(textBoxDecryptLimit.Text);
      }
      catch (Exception)
      {
        _card.DecryptLimit = 1;
      }
      this.Close();
    }
  }
}