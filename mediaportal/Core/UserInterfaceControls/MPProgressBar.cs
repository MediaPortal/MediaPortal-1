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
using System.Windows.Forms;
using System.Drawing;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Summary description for MPProgressBar.
  /// </summary>
  public class MPProgressBar : System.Windows.Forms.ProgressBar
  {
    public MPProgressBar()
    {
       this.SetStyle(ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer, true);
	}
	
	protected override void OnPaint(PaintEventArgs e)
    {
        ProgressBarRenderer.DrawHorizontalBar(e.Graphics, this.ClientRectangle);
        Rectangle bounds = new Rectangle
        {
            X = this.ClientRectangle.X,
            Y = this.ClientRectangle.Y,
            Width = (Int32)Math.Floor(((double)this.Value / this.Maximum) * this.ClientRectangle.Width),
            Height = this.ClientRectangle.Height
        };
        bounds.Inflate(-1, -1);
        ProgressBarRenderer.DrawHorizontalChunks(e.Graphics, bounds);
    }
  }
}
