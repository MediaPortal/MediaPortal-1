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
using DShowNET;
using DirectShowLib;

namespace WindowPlugins.VideoEditor
{
	public partial class VideoEditorConfiguration : Form
	{
		public VideoEditorConfiguration()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog diag = new OpenFileDialog();
			diag.Filter = "exe-File (*.exe)|*.exe";
			if(diag.ShowDialog() == DialogResult.OK)
			{
				if(System.IO.File.Exists(diag.FileName))
				{
					mencoderPath.Text = diag.FileName;
				}
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
			{
				xmlwriter.SetValue("VideoEditor", "mencoder", mencoderPath.Text);
			}
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}