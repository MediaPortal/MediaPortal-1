#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace MediaPortal.GUI.MusicVideos
{
    public partial class SetupFavoriteForm : Form
    {
       
        //private int miFavoriteId;

        public SetupFavoriteForm()
        {
            InitializeComponent();
            //msFavoriteName = "";
        }

        private void SetupFavoriteForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //msFavoriteName = '';
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

      private void textBoxFavoriteName_KeyUp(object sender, KeyEventArgs e)
      {
        if (e.KeyCode == Keys.Return)
        {
          this.DialogResult = DialogResult.OK;
          this.Close();
        }
      }
    }
}