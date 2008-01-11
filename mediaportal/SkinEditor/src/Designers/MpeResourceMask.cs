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

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Mpe.Controls;

namespace Mpe.Designers
{
  public class MpeResourceMask : Control
  {
    #region Variables

    private MpeControl control;

    #endregion

    [DllImport("user32")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    public MpeResourceMask()
    {
      //
    }

    public MpeControl SelectedControl
    {
      get { return control; }
      set { control = value; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (control != null)
      {
        IntPtr hDC = GetWindowDC(IntPtr.Zero);
        Graphics g = Graphics.FromHdc(hDC);
        g.FillRectangle(new SolidBrush(Color.Black), control.ClientRectangle);
        ReleaseDC(IntPtr.Zero, hDC);
      }
      e.Graphics.FillRectangle(new SolidBrush(Color.White), ClientRectangle);
    }
  }
}