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

using System.ComponentModel;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls.UserInterfaceControls
{
  public class MPNumericUpDown : NumericUpDown
  {
    private bool _truncateDecimalPlaces = false;

    public MPNumericUpDown()
    {
      base.TextAlign = HorizontalAlignment.Center;
    }

    [DefaultValue(HorizontalAlignment.Center)]
    public new HorizontalAlignment TextAlign
    {
      get
      {
        return base.TextAlign;
      }
      set
      {
        base.TextAlign = value;
        Invalidate();
      }
    }

    [DefaultValue(false)]
    public bool TruncateDecimalPlaces
    {
      get
      {
        return _truncateDecimalPlaces;
      }
      set
      {
        _truncateDecimalPlaces = value;
      }
    }

    protected override void UpdateEditText()
    {
      if (!_truncateDecimalPlaces)
      {
        base.UpdateEditText();
        return;
      }
      Text = Value.ToString("0." + new string('#', DecimalPlaces));
    }
  }
}