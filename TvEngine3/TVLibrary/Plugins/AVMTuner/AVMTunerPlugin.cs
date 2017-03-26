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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using TvControl;

namespace TvEngine
{
  public class AVMTunerPlugin : ITvServerPlugin, ITvServerPluginStartedAll
  {
    #region ITvServerPlugin Members

    public string Name
    {
      get { return "AVM Tuner Helper"; }
    }

    public string Version
    {
      get { return "0.1"; }
    }

    public string Author
    {
      get { return "morpheus_xx"; }
    }

    public bool MasterOnly
    {
      get { return false; }
    }

    public void Start(IController controller)
    {
    }

    public void StartedAll()
    {
    }

    public void Stop()
    {
  
    }

    public SetupTv.SectionSettings Setup
    {
      get { return new AVMTuner.AVMTuner(); }
    }

    #endregion
  }
}