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

namespace TvLibrary
{
  /// <summary>
  /// class which holds all parameters needed during scanning for channels
  /// </summary>
  public class ScanParameters
  {
    // Tuning and scanning timeouts.
    private int _timeoutTune = 2;
    private int _timeoutCAT = 5;
    private int _timeoutPMT = 10;
    private int _timeoutSDT = 20;
    private int _timeoutAnalog = 20;

    // Timeshifting file parameters.
    private int _minFiles = 6;
    private int _maxFiles = 20;
    private UInt32 _maxFileSize = (256 * 1000 * 1000);

    /// <summary>
    /// Gets or sets the minimium number of timeshifting files.
    /// </summary>
    /// <value>The minimium files.</value>
    public int MinimumFiles
    {
      get { return _minFiles; }
      set { _minFiles = value; }
    }

    /// <summary>
    /// Gets or sets the maximum number of timeshifting files.
    /// </summary>
    /// <value>The maximum files.</value>
    public int MaximumFiles
    {
      get { return _maxFiles; }
      set { _maxFiles = value; }
    }

    /// <summary>
    /// Gets or sets the maximum filesize for each timeshifting file.
    /// </summary>
    /// <value>The maximum filesize.</value>
    public UInt32 MaximumFileSize
    {
      get { return _maxFileSize; }
      set { _maxFileSize = value; }
    }

    /// <summary>
    /// Gets or sets the time out CAT.
    /// </summary>
    /// <value>The time out CAT.</value>
    public int TimeOutCAT
    {
      get { return _timeoutCAT; }
      set { _timeoutCAT = value; }
    }

    /// <summary>
    /// Gets or sets the time out PMT.
    /// </summary>
    /// <value>The time out PMT.</value>
    public int TimeOutPMT
    {
      get { return _timeoutPMT; }
      set { _timeoutPMT = value; }
    }

    /// <summary>
    /// Gets or sets the time out tune.
    /// </summary>
    /// <value>The time out tune.</value>
    public int TimeOutTune
    {
      get { return _timeoutTune; }
      set { _timeoutTune = value; }
    }

    /// <summary>
    /// Gets or sets the time out SDT.
    /// </summary>
    /// <value>The time out SDT.</value>
    public int TimeOutSDT
    {
      get { return _timeoutSDT; }
      set { _timeoutSDT = value; }
    }

    /// <summary>
    /// Gets or sets the time out Analog scanning.
    /// </summary>
    /// <value>The time out Analog scanning.</value>
    public int TimeOutAnalog
    {
      get { return _timeoutAnalog; }
      set { _timeoutAnalog = value; }
    }
  }
}