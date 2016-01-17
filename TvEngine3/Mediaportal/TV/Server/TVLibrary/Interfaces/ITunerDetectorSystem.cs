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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// An interface for detecting tuners that are directly connected to the system.
  /// </summary>
  internal interface ITunerDetectorSystem
  {
    /// <summary>
    /// Get the detector's name.
    /// </summary>
    string Name
    {
      get;
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a system device
    /// interface.
    /// </summary>
    /// <param name="classGuid">The identifier for the interface's class.</param>
    /// <param name="devicePath">The interface's device path.</param>
    /// <returns>the compatible tuners exposed by the interface</returns>
    ICollection<ITuner> DetectTuners(Guid classGuid, string devicePath);

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    ICollection<ITuner> DetectTuners();
  }
}