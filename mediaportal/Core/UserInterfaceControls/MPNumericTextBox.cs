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
using System.Windows.Forms;
using System.Globalization;

namespace MediaPortal.UserInterface.Controls
{
  /// <summary>
  /// Define a TextBox that allow only integer numbers.
  /// </summary>
  public class MPNumericTextBox : System.Windows.Forms.TextBox
  {
    public MPNumericTextBox()
      : base() {}

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      base.OnKeyPress(e);
      if ((!e.Handled) && ("1234567890\b".IndexOf(e.KeyChar) < 0))
      {
        Yeti.Sys.Win32.MessageBeep(Yeti.Sys.BeepType.SimpleBeep);
        e.Handled = true;
      }
    }

    public event EventHandler FormatError;
    public event EventHandler FormatValid;

    protected virtual void OnFormatError(EventArgs e)
    {
      if (FormatError != null)
      {
        FormatError(this, e);
      }
    }

    protected virtual void OnFormatValid(EventArgs e)
    {
      if (FormatValid != null)
      {
        FormatValid(this, e);
      }
    }

    protected override void OnTextChanged(EventArgs e)
    {
      try
      {
        int.Parse(this.Text, NumberStyles.Integer);
        OnFormatValid(e);
      }
      catch
      {
        OnFormatError(e);
      }
      base.OnTextChanged(e);
    }

    public int Value
    {
      get { return int.Parse(this.Text, NumberStyles.Integer); }
      set { this.Text = value.ToString(); }
    }
  }
}