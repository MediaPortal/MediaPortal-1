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
using System.IO;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects compatible DirectShow
  /// stream source filters.
  /// </summary>
  internal class TunerDetectorStream : ITunerDetectorSystem
  {
    #region variables

    // key = name
    private IDictionary<string, IList<ITVCard>> _knownTuners = new Dictionary<string, IList<ITVCard>>();

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
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITVCard> DetectTuners()
    {
      this.LogDebug("stream detector: detect tuners");
      List<ITVCard> tuners = new List<ITVCard>();
      IDictionary<string, IList<ITVCard>> knownTuners = new Dictionary<string, IList<ITVCard>>();

      int streamTunerCount = SettingsManagement.GetValue("iptvCardCount", 1);

      // Elecard stream source filter.
      // TODO could we do this better/faster if we had access to the CLSID (FilterGraphTools.IsThisComObjectInstalled())?
      string targetDeviceName = "Elecard NWSource-Plus";
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      foreach (DsDevice device in devices)
      {
        try
        {
          string name = device.Name;
          if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(device.DevicePath))
          {
            continue;
          }

          if (name.Equals(targetDeviceName))
          {
            // Was the filter already installed? If so reuse the existing tuner
            // instances.
            IList<ITVCard> currentTuners;
            IList<ITVCard> newTuners = new List<ITVCard>(streamTunerCount);
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
        }
        finally
        {
          device.Dispose();
        }
      }

      // MediaPortal stream source filter.
      if (File.Exists(PathManager.BuildAssemblyRelativePath("MPIPTVSource.ax")))
      {
        // Was the filter already installed? If so reuse the existing tuner
        // instances.
        targetDeviceName = "MediaPortal stream source";
        IList<ITVCard> currentTuners;
        IList<ITVCard> newTuners = new List<ITVCard>(streamTunerCount);
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
            newTuners.Add(new TunerStream(i));
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