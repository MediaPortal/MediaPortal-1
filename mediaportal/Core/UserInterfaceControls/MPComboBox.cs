#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Windows.Forms;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Summary description for MPComboBox.
  /// </summary>
  public class MPComboBox : System.Windows.Forms.ComboBox
  {
    private Color _borderColor = Color.Empty;

    public Color BorderColor
    {
      get { return _borderColor; }
      set { _borderColor = value; }
    }

    public MPComboBox() {}

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