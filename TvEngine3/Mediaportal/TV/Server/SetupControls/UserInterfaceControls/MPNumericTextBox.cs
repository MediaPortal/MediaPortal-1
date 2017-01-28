#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls.UserInterfaceControls
{
  /// <summary>
  /// Define a TextBox that allow only integer numbers.
  /// </summary>
  public class MPNumericTextBox : MPTextBox
  {
    private int _minimumValue = int.MinValue;
    private int _maximumValue = int.MaxValue;
    private string _previousVal = string.Empty;

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      base.OnKeyPress(e);
      if (!e.Handled)
      {
        if (
          "1234567890\b".IndexOf(e.KeyChar) < 0 &&
          (_minimumValue >= 0 || !string.Equals(e.KeyChar.ToString(), CultureInfo.CurrentCulture.NumberFormat.NegativeSign))
        )
        {
          e.Handled = true;
        }
      }
    }

    public event EventHandler FormatError;
    public event EventHandler FormatValid;

    protected virtual void OnFormatError(EventArgs e)
    {
      EventHandler handlers = FormatError;
      if (handlers != null)
      {
        handlers(this, e);
      }
    }

    protected virtual void OnFormatValid(EventArgs e)
    {
      EventHandler handlers = FormatValid;
      if (handlers != null)
      {
        handlers(this, e);
      }
    }

    protected override void OnGotFocus(EventArgs e)
    {
      _previousVal = Text;
      base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
      try
      {
        int value;
        if (
          !int.TryParse(Text, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out value) ||
          value < _minimumValue ||
          value > _maximumValue
        )
        {
          OnFormatError(e);
          Text = _previousVal;
        }
        else
        {
          OnFormatValid(e);
          Text = value.ToString();
        }
      }
      catch
      {
        OnFormatError(e);
      }
      base.OnLostFocus(e);
    }

    [DefaultValue(int.MinValue)]
    public int MinimumValue
    {
      get
      {
        return _minimumValue;
      }
      set
      {
        _minimumValue = value;
      }
    }

    [DefaultValue(int.MaxValue)]
    public int MaximumValue
    {
      get
      {
        return _maximumValue;
      }
      set
      {
        _maximumValue = value;
      }
    }

    public int Value
    {
      get
      {
        int val;
        if (!int.TryParse(Text, out val))
        {
          val = _minimumValue;
        }
        return val;
      }
      set
      {
        Text = value.ToString();
      }
    }
  }
}