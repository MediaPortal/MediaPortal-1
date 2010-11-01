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

using System.Windows.Forms;

namespace MediaPortal.Mixer
{
  internal class MixerEventListener : NativeWindow
  {
    #region Events

    public event MixerEventHandler LineChanged;
    public event MixerEventHandler ControlChanged;

    #endregion Events

    #region Methods

    public void Start()
    {
      CreateParams createParams = new CreateParams();

      createParams.ExStyle = 0x08000000;
      createParams.Style = unchecked((int)0x80000000);

      CreateHandle(createParams);
    }

    protected override void WndProc(ref Message m)
    {
      if (m.Msg == (int)MixerMessages.LineChanged && LineChanged != null)
      {
        LineChanged(this, new MixerEventArgs(m.WParam, m.LParam.ToInt32()));
      }

      if (m.Msg == (int)MixerMessages.ControlChanged && ControlChanged != null)
      {
        ControlChanged(this, new MixerEventArgs(m.WParam, m.LParam.ToInt32()));
      }

      base.WndProc(ref m);
    }

    #endregion Methods
  }
}