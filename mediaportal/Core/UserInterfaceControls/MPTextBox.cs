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
  /// Summary description for MPTextBox.
  /// </summary>
  public class MPTextBox : System.Windows.Forms.TextBox
  {
    private Color _borderColor = Color.Empty;

    public Color BorderColor
    {
      get { return _borderColor; }
      set { _borderColor = value; }
    }

    public MPTextBox() {}

    protected override void WndProc(ref Message msg)
    {
      if (msg.Msg == 0x000F && _borderColor != Color.Empty)
      {
        Graphics graphics = Graphics.FromHwnd(this.Parent.Handle);
        Rectangle rectangle = new Rectangle(
          this.Left - 1,
          this.Top - 1,
          this.Width + 1,
          this.Height + 1);
        graphics.DrawRectangle(new Pen(_borderColor), rectangle);
        graphics.Dispose();
      }
      base.WndProc(ref msg);
    }
  }
}