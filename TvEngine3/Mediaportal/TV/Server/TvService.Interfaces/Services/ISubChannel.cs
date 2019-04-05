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

namespace Mediaportal.TV.Server.TVService.Interfaces.Services
{
  public interface ISubChannel
  {
    [DataMember]
    int Id { get; }

    [DataMember]
    string UserName { get; }

    [DataMember]
    UserType UserType { get; }

    [DataMember]
    int? PriorityOverride { get; }

    [DataMember]
    int IdTuningDetail { get; }

    [DataMember]
    int IdTuner { get; }

    [DataMember]
    bool IsTuning { get; }

    [DataMember]
    bool IsParked { get; }

    [DataMember]
    double ParkPosition { get; }

    [DataMember]
    DateTime ParkTime { get; }
  }
}