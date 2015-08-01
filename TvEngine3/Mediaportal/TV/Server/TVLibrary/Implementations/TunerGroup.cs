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
using System.Collections.ObjectModel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using DbTunerGroup = Mediaportal.TV.Server.TVDatabase.Entities.TunerGroup;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// An implementation of <see cref="ITunerGroup"/>, used to support
  /// multi-mode (hybrid and combo) tuners.
  /// </summary>
  internal class TunerGroup : ITunerGroup
  {
    #region variables

    private DbTunerGroup _databaseTunerGroup = null;
    private string _productInstanceId = null;
    private string _tunerInstanceId = null;
    private IList<ITuner> _tuners = new List<ITuner>();

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerGroup"/> class.
    /// </summary>
    /// <param name="dbGroup">The database settings for the group.</param>
    public TunerGroup(DbTunerGroup dbGroup)
    {
      _databaseTunerGroup = dbGroup;
    }

    /// <summary>
    /// Add a tuner to the group.
    /// </summary>
    /// <param name="tuner">The tuner to add.</param>
    public void Add(ITuner tuner)
    {
      _tuners.Add(tuner);
    }

    /// <summary>
    /// Remove a tuner from the group.
    /// </summary>
    /// <param name="tuner">The tuner to remove.</param>
    /// <returns><c>true</c> if the tuner was in the group and was removed, otherwise <c>false</c></returns>
    public bool Remove(ITuner tuner)
    {
      bool toReturn = false;
      for (int i = _tuners.Count - 1; i >= 0; i--)
      {
        if (_tuners[i].ExternalId.Equals(tuner.ExternalId))
        {
          _tuners.RemoveAt(i);
          toReturn = true;
        }
      }
      return toReturn;
    }

    #region ITunerGroup members

    /// <summary>
    /// Get the tuner group's identifier.
    /// </summary>
    public int TunerGroupId
    {
      get
      {
        return _databaseTunerGroup.IdTunerGroup;
      }
    }

    /// <summary>
    /// Get the tuner group's name.
    /// </summary>
    public string Name
    {
      get
      {
        return _databaseTunerGroup.Name;
      }
    }

    /// <summary>
    /// Get or set the tuner group's product instance identifier.
    /// </summary>
    public string ProductInstanceId
    {
      get
      {
        return _productInstanceId;
      }
      set
      {
        _productInstanceId = value;
      }
    }

    /// <summary>
    /// Get or set the tuner group's tuner instance identifier.
    /// </summary>
    public string TunerInstanceId
    {
      get
      {
        return _tunerInstanceId;
      }
      set
      {
        _tunerInstanceId = value;
      }
    }

    /// <summary>
    /// Get the tuner group's members.
    /// </summary>
    public ReadOnlyCollection<ITuner> Tuners
    {
      get
      {
        return new ReadOnlyCollection<ITuner>(_tuners);
      }
    }

    #endregion
  }
}