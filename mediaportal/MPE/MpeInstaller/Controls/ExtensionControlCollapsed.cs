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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeInstaller.Dialogs;

namespace MpeInstaller.Controls
{
  public partial class ExtensionControlCollapsed : UserControl
  {
    Action<ExtensionControlCollapsed> OnSelectedDelegate;

    public ExtensionControlCollapsed()
    {
      InitializeComponent();
    }

    public void Initialize(string name, string authors, string version, bool meetsAllDependencies, string updateVersion, Action<ExtensionControlCollapsed> onClick)
    {
      lbl_name.Text = name;
      lblAuthors.Text = string.Format("[{0}]", authors);
      lbl_version.Text = version;
      img_dep.Visible = meetsAllDependencies;
      if (meetsAllDependencies)
      {
        (Parent.Parent.Parent as ExtensionListControl).toolTip1.SetToolTip(img_dep, 
          "Some dependencies are not met.\r\nThe extension may not work properly.\r\nClick here for more information.");
      }
      if (!string.IsNullOrEmpty(updateVersion))
      {
        img_update.Visible = true;
        (Parent.Parent.Parent as ExtensionListControl).toolTip1.SetToolTip(img_update, string.Format("New update available. Version: {0}", updateVersion));
      }
      else
      {
        img_update.Visible = false;
      }

      OnSelectedDelegate = onClick;
    }

    private void img_dep_Click(object sender, EventArgs e)
    {
      DependencyForm depForm = new DependencyForm((Parent as ExtensionControlHost).Package);
      depForm.ShowDialog();
    }

    private void ExtensionControlCollapsed_Click(object sender, EventArgs e)
    {
      if (OnSelectedDelegate != null) OnSelectedDelegate(this);
    }
  }
}
