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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner
{
  /// <summary>
  /// Tuner in band electronic programme guide data grabber interface.
  /// </summary>
  public interface IEpgGrabber
  {
    /// <summary>
    /// Set the grabber's call-back.
    /// </summary>
    /// <param name="callBack">The delegate to notify about grabber events.</param>
    void SetCallBack(IEpgGrabberCallBack callBack);

    /// <summary>
    /// Enable or disable grabbing.
    /// </summary>
    /// <value><c>true</c> if grabbing is enabled, otherwise <c>false</c></value>
    bool IsEnabled
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the EPG data protocols supported by the tuner hardware and/or enabled for use.
    /// </summary>
    TunerEpgGrabberProtocol SupportedProtocols
    {
      get;
      set;
    }

    /// <summary>
    /// Get the EPG data protocols supported by the grabber code/class/type implementation.
    /// </summary>
    TunerEpgGrabberProtocol PossibleProtocols
    {
      get;
    }

    /// <summary>
    /// Get the grabber's current status.
    /// </summary>
    /// <value><c>true</c> if the grabber is grabbing, otherwise <c>false</c></value>
    bool IsGrabbing
    {
      get;
    }

    /// <summary>
    /// Get all available EPG data.
    /// </summary>
    /// <returns>the data, grouped by channel</returns>
    IList<Tuple<IChannel, IList<EpgProgram>>> GetData();
  }
}