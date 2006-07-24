#region Copyright (C) 2005-2006 Team MediaPortal - Author: CoolHammer, mPod
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: CoolHammer, mPod
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.InputDevices
{
  /// <summary>
  ///  
  /// </summary>
  class X10RemoteForm : System.Windows.Forms.Form
  {
    private AxX10.AxX10Interface Ix10;
    /// <summary>
    /// 
    /// </summary>
    private System.ComponentModel.Container components = null;

    public X10RemoteForm(AxX10._DIX10InterfaceEvents_X10CommandEventHandler IxCmd)
    {
      InitializeComponent();
      
      Ix10.X10Command += IxCmd;
    }

    /// <summary>
    /// 
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

    #region Vom Windows Form-Designer generierter Code
    /// <summary>
    /// 
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(X10RemoteForm));
      this.Ix10 = new AxX10.AxX10Interface();
      ((System.ComponentModel.ISupportInitialize)(this.Ix10)).BeginInit();
      this.SuspendLayout();
      // 
      // Ix10
      // 
      this.Ix10.Enabled = true;
      //         this.Ix10.Location = new System.Drawing.Point(42, 24);
      this.Ix10.Name = "Ix10";
      this.Ix10.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("Ix10.OcxState")));
      //        this.Ix10.Size = new System.Drawing.Size(192, 192);
      this.Ix10.TabIndex = 0;
      this.Ix10.TabStop = false;
      this.Ix10.Visible = false;
      // 
      // X10RemoteForm
      // 
      //           this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      //           this.ClientSize = new System.Drawing.Size(104, 74);
      this.Controls.Add(this.Ix10);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "X10RemoteForm";
      this.Opacity = 0;
      this.ShowInTaskbar = false;
      this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
      ((System.ComponentModel.ISupportInitialize)(this.Ix10)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

  }
}
