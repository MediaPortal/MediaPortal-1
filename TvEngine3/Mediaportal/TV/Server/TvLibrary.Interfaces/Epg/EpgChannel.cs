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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Epg
{
  /// <summary>
  /// class which holds all epg information received for a specific DVB tv/radio channel
  /// </summary>
  [Serializable]
  public class EpgChannel
  {
    #region variables

    private List<EpgProgram> _programs;
    private IChannel _channel;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgChannel"/> class.
    /// </summary>
    public EpgChannel()
    {
      _programs = new List<EpgProgram>();
    }

    #region properties

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public IChannel Channel
    {
      get { return _channel; }
      set { _channel = value; }
    }

    /// <summary>
    /// Gets or sets the epg programs.
    /// </summary>
    /// <value>The programs.</value>
    public List<EpgProgram> Programs
    {
      get { return _programs; }
      set { _programs = value; }
    }

    #endregion
  }
}