#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;

namespace MpeCore.Classes.SectionPanel
{
  public partial class BaseVerticalLayout : BaseLayout
  {
    public BaseVerticalLayout()
    {
      InitializeComponent();

      button_back.FlatAppearance.MouseOverBackColor = button_back.BackColor;
      button_back.BackColorChanged += (s, e) => {button_back.FlatAppearance.MouseOverBackColor = button_back.BackColor;};

      button_next.FlatAppearance.MouseOverBackColor = button_next.BackColor;
      button_next.BackColorChanged += (s, e) => { button_next.FlatAppearance.MouseOverBackColor = button_next.BackColor; };

      button_cancel.FlatAppearance.MouseOverBackColor = button_cancel.BackColor;
      button_cancel.BackColorChanged += (s, e) => { button_cancel.FlatAppearance.MouseOverBackColor = button_cancel.BackColor; };
    }
  }
}