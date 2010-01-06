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

namespace SetupTv
{
  public partial class NotifyForm : SetupControls.MPForm
  {
    public NotifyForm(string caption, string message)
    {
      InitializeComponent();
      Init(caption, message);
    }

    private void Init(string caption, string message)
    {
      Text = caption;
      label1.Text = message;
    }

    public void SetMessage(string message)
    {
      label1.Text = message;
      label1.Invalidate();
      label1.Refresh();
    }

    public void WaitForDisplay()
    {
      long ticks = DateTime.Now.Ticks;
      do
      {
        Application.DoEvents();
        System.Threading.Thread.Sleep(10);
      } while (DateTime.Now.Ticks < (ticks + 1500));
    }
  }
}