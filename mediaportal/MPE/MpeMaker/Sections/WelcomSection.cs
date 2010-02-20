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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore;

namespace MpeMaker.Sections
{
  public partial class WelcomSection : UserControl, ISectionControl
  {
    public WelcomSection()
    {
      InitializeComponent();
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start(
          "http://forum.team-mediaportal.com/skins-plugins-installer-mpei-212/extension-installer-v2-71542/");
      }
      catch (Exception) {}
    }

    public void Set(PackageClass pak) {}

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("http://wiki.team-mediaportal.com/MpeMaker");
      }
      catch (Exception) {}
    }
  }
}