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
using System.IO;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects compatible DirectShow
  /// stream source filters.
  /// </summary>
  internal class TunerDetectorStream : ITunerDetectorSystem
  {
    #region variables

    private IDictionary<string, IList<ITuner>> _knownTuners = new Dictionary<string, IList<ITuner>>();    // key = name

    #endregion

    #region ITunerDetectorSystem members

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "stream";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a system device
    /// interface.
    /// </summary>
    /// <param name="classGuid">The identifier for the interface's class.</param>
    /// <param name="devicePath">The interface's device path.</param>
    /// <returns>the compatible tuners exposed by the interface</returns>
    public ICollection<ITuner> DetectTuners(Guid classGuid, string devicePath)
    {
      return new List<ITuner>(0);
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITuner> DetectTuners()
    {
      this.LogDebug("stream detector: detect tuners");
      int streamTunerCount = SettingsManagement.GetValue("streamTunerCount", 0);
      List<ITuner> tuners = new List<ITuner>(streamTunerCount * 2);
      IDictionary<string, IList<ITuner>> knownTuners = new Dictionary<string, IList<ITuner>>(streamTunerCount * 2);
      IList<ITuner> currentTuners;

      // Elecard stream source filter.
      string targetDeviceName = "Elecard NWSource-Plus";
      if (FilterGraphTools.IsThisComObjectInstalled(TunerStreamElecard.CLSID))
      {
        // Was the filter already installed? If so reuse the existing tuner
        // instances.
        IList<ITuner> newTuners = new List<ITuner>(streamTunerCount);
        if (!_knownTuners.TryGetValue(targetDeviceName, out currentTuners))
        {
          currentTuners = null;
        }

        for (int i = 1; i <= streamTunerCount; i++)
        {
          if (currentTuners != null && currentTuners.Count >= i)
          {
            newTuners.Add(currentTuners[i - 1]);
          }
          else
          {
            newTuners.Add(new TunerStreamElecard(i));
          }
        }

        tuners.AddRange(newTuners);
        knownTuners.Add(targetDeviceName, newTuners);
      }

      // MediaPortal stream source filter.
      if (File.Exists(PathManager.BuildAssemblyRelativePath("MPIPTVSource.ax")))
      {
        // Was the filter already installed? If so reuse the existing tuner
        // instances.
        targetDeviceName = "MediaPortal stream source";
        IList<ITuner> newTuners = new List<ITuner>(streamTunerCount);
        if (!_knownTuners.TryGetValue(targetDeviceName, out currentTuners))
        {
          currentTuners = null;
        }

        for (int i = 1; i <= streamTunerCount; i++)
        {
          if (currentTuners != null && currentTuners.Count >= i)
          {
            newTuners.Add(currentTuners[i - 1]);
          }
          else
          {
            newTuners.Add(new TunerStreamTve(i));
          }
        }

        tuners.AddRange(newTuners);
        knownTuners.Add(targetDeviceName, newTuners);
      }

      _knownTuners = knownTuners;
      return tuners;
    }

    #endregion
  }
}