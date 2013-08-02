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

using System.Collections.Generic;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ViewModeSwitcher
{
  public class Rule
  {
    public bool Enabled = true;
    public string Name = "New rule";
    public double ARFrom = 1.2;
    public double ARTo = 1.46;
    public int MinWidth = 200;
    public int MaxWidth = 2000;
    public int MinHeight = 200;
    public int MaxHeight = 2000;
    public Geometry.Type ViewMode = Geometry.Type.Normal;
    public int OverScan = 8;
    public bool EnableLBDetection = false;
    public bool AutoCrop = false;
    public bool MaxCrop = true;
  }

  public class RuleSet : List<Rule> {}
}