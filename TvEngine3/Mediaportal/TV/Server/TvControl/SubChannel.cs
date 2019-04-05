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
using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl
{
  [DataContract]
  public class SubChannel : ISubChannel
  {
    [DataMember]
    private int _id;

    [DataMember]
    private string _userName;

    [DataMember]
    private UserType _userType;

    [DataMember]
    private int? _priorityOverride;

    [DataMember]
    private int _idTuningDetail;

    [DataMember]
    private int _idTuner;

    [DataMember]
    private bool _isTuning;

    [DataMember]
    private bool _isParked;

    [DataMember]
    private double _parkPosition;

    [DataMember]
    private DateTime _parkTime;

    public SubChannel(int id, string userName, UserType userType, int? priorityOverride = null)
    {
      _id = id;
      _userName = userName;
      _userType = userType;
      _priorityOverride = priorityOverride;
      _idTuningDetail = -1;
      _idTuner = -1;
      _isTuning = false;
      _isParked = false;
      _parkPosition = ulong.MaxValue;
      _parkTime = DateTime.MinValue;
    }

    public int Id
    {
      get { return _id; }
    }

    public string UserName
    {
      get { return _userName; }
      set { _userName = value; }
    }

    public UserType UserType
    {
      get { return _userType; }
      set { _userType = value; }
    }

    public int? PriorityOverride
    {
      get { return _priorityOverride; }
    }

    public int IdTuningDetail
    {
      get { return _idTuningDetail; }
      set { _idTuningDetail = value; }
    }

    public int IdTuner
    {
      get { return _idTuner; }
      set { _idTuner = value; }
    }

    public bool IsTuning
    {
      get { return _isTuning; }
      set { _isTuning = value; }
    }

    public bool IsParked
    {
      get { return _isParked; }
      set { _isParked = value; }
    }

    public double ParkPosition
    {
      get { return _parkPosition; }
      set { _parkPosition = value; }
    }

    public DateTime ParkTime
    {
      get { return _parkTime; }
      set { _parkTime = value; }
    }
  }
}