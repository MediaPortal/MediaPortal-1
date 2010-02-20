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

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Properties
{
  [CompilerGenerated, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"),
   DebuggerNonUserCode]
  internal class Resources
  {
    private static CultureInfo resourceCulture;
    private static ResourceManager resourceMan;

    internal Resources() {}

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get { return resourceCulture; }
      set { resourceCulture = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (ReferenceEquals(resourceMan, null))
        {
          ResourceManager manager = new ResourceManager("MiniDisplayPlugin.Properties.Resources",
                                                        typeof (Resources).Assembly);
          resourceMan = manager;
        }
        return resourceMan;
      }
    }
  }
}