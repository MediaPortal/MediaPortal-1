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

using System.ComponentModel;
using System.Drawing;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Configuration.Sections
{
  public class EncoderFiltersSection : SectionSettings
  {
    private MPLabel mpLabel1;
    //private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    //private MediaPortal.UserInterface.Controls.MPLabel label4;
    //private System.ComponentModel.IContainer components = null;

    public EncoderFiltersSection() : this("Encoder Filters")
    {
    }

    private void InitializeComponent()
    {
      ComponentResourceManager resources = new ComponentResourceManager(typeof (EncoderFiltersSection));
      this.mpLabel1 = new MPLabel();
      this.SuspendLayout();
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new Point(42, 51);
      this.mpLabel1.MaximumSize = new Size(400, 0);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new Size(385, 52);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = resources.GetString("mpLabel1.Text");
      // 
      // EncoderFiltersSection
      // 
      this.Controls.Add(this.mpLabel1);
      this.Name = "EncoderFiltersSection";
      this.Size = new Size(472, 408);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public EncoderFiltersSection(string name) : base(name)
    {
    }
  }
}