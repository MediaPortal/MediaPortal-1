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
using System;
using System.Windows.Forms;

namespace MPLanguageTool
{
  public partial class frmEditMP : Form
  {
    public frmEditMP()
    {
      InitializeComponent();
    }
    public DialogResult ShowDialog(string id, string translation, string defaultTranslation, string prefix, string prefixTranslation)
    {
      lID.Text = id;
      edValue.Text = translation;
      edDefault.Text = defaultTranslation;
      textBox1.Text = prefix;
      textBox2.Text = prefixTranslation;

      return ShowDialog();
    }
    public string GetTranslation()
    {
      if (String.IsNullOrEmpty(edValue.Text))
      {
        return null;
      }
      return edValue.Text;
    }

    public string GetPrefixTranslation()
    {
      if (String.IsNullOrEmpty(edValue.Text))
      {
        return null;
      }
      return textBox1.Text;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void frmEdit_Shown(object sender, EventArgs e)
    {
      edValue.Focus();
    }
  }
}