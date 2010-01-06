#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Summary description for BeveledLine.
  /// </summary>
  public class MPBeveledLine : System.Windows.Forms.UserControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public MPBeveledLine()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      DrawForeground(this.CreateGraphics());
      base.OnPaint(e);
    }

    private void DrawForeground(Graphics graphics)
    {
      graphics.Clear(System.Drawing.SystemColors.Control);

      System.Drawing.Pen grayPen = new Pen(Color.FromArgb(200, 200, 200));
      graphics.DrawLine(grayPen, 0, 0, this.Width - 1, 0);
      graphics.DrawLine(System.Drawing.Pens.WhiteSmoke, 0, this.Height - 1, this.Width - 1, this.Height - 1);
      grayPen.Dispose();
    }

    /// <summary> 
    /// Clean up any resources being used.
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
    }

    #endregion
  }
}