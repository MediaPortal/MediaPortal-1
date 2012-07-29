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

namespace MpeInstaller.Controls
{
  public partial class ExtensionControlHost : UserControl
  {
    public PackageClass Package { get; protected set; }
    public PackageClass UpdatePackage { get; protected set; }

    bool isInstalled;
    bool meetsAllDependencies;

    public ExtensionControlHost()
    {
      InitializeComponent();
    }

    private void CollapsedItemClicked(ExtensionControlCollapsed sender)
    {
      var listCtrl = Parent.Parent as ExtensionListControl;
      if (listCtrl.SelectedItem != null) listCtrl.SelectedItem.Collapse();
      extensionControlCollapsed.Visible = false;
      if (!extensionControlExpanded.IsInitialized)
        extensionControlExpanded.Initialize(isInstalled, meetsAllDependencies, Package, UpdatePackage);
      extensionControlExpanded.Visible = true;
      Height = extensionControlExpanded.PreferredSize.Height + Margin.Top + Margin.Bottom;

      listCtrl.SelectedItem = this;
    }

    void Collapse()
    {
      extensionControlExpanded.Visible = false;
      extensionControlCollapsed.Visible = true;
      Height = extensionControlCollapsed.PreferredSize.Height + Margin.Top + Margin.Bottom;
    }

    public void Initialize(PackageClass package, bool isInstalled)
    {
      this.Package = package;
      this.isInstalled = isInstalled;
      this.meetsAllDependencies = !package.CheckDependency(true);

      if (isInstalled) UpdatePackage = MpeCore.MpeInstaller.KnownExtensions.GetUpdate(package);

      extensionControlCollapsed.Initialize(
        package.GeneralInfo.Name, 
        package.GeneralInfo.Author, 
        package.GeneralInfo.Version.ToString(), 
        meetsAllDependencies, 
        UpdatePackage != null ? UpdatePackage.GeneralInfo.Version.ToString() : null, 
        CollapsedItemClicked);

      Height = extensionControlCollapsed.PreferredSize.Height + Margin.Top + Margin.Bottom;
    }

    public bool Filter(string str, string tag)
    {
      if (Package == null)
        return true;
      tag = tag.Trim();
      if (tag.ToUpper() == "ALL")
        tag = string.Empty;

      bool strResult = string.IsNullOrEmpty(str);
      bool tagResult = string.IsNullOrEmpty(tag);

      if (string.IsNullOrEmpty(str))
        strResult = true;
      else
      {
        if (Package.GeneralInfo.Name.ToUpper().Contains(str.ToUpper()))
          strResult = true;
        if (Package.GeneralInfo.ExtensionDescription.ToUpper().Contains(str.ToUpper()))
          strResult = true;
        if (Package.GeneralInfo.TagList.Tags.Contains(str.ToLower()))
          strResult = true;
      }
      if (!string.IsNullOrEmpty(tag))
      {
        if (Package.GeneralInfo.TagList.Tags.Contains(tag.ToLower()))
          tagResult = true;
      }
      return strResult && tagResult;
    }
  }
}
