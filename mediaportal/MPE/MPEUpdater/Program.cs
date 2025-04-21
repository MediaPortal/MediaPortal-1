#region Copyright (C) 2024 Team MediaPortal

// Copyright (C) 2024 Team MediaPortal
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
using System.Linq;
using System.Reflection;
using System.Text;

using MpeCore.Classes;

namespace MPEUpdater
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("MPE Updater version: {0}{1}", Assembly.GetExecutingAssembly().GetName().Version, (IntPtr.Size == 8) ? " | x64" : string.Empty);
      Console.WriteLine("Update started at {0}", DateTime.Now.ToLongTimeString());

      MpeCore.MpeInstaller.Init();
      ExtensionUpdateDownloader.UpdateList(true, false, null, null);

      Console.WriteLine("Update complete at {0}", DateTime.Now.ToLongTimeString());
    }
  }
}
