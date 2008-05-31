/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary
{
  /// <summary>
  /// class which holds all parameters needed during scanning for channels
  /// </summary>
  public class ScanParameters
  {
    int _timeoutTune = 2;
    int _timeoutPAT = 5;
    int _timeoutCAT = 5;
    int _timeoutPMT = 10;
    int _timeoutSDT = 20;
    int _timeoutAnalog = 20;

    int _lnbLowFrequency = -1;
    int _lnbHighFrequency = -1;
    int _lnbSwitchFrequency = -1;
    bool _useDefaultLnbFrequencies=true;
    int _minFiles = 6;
    int _maxFiles = 20;
    UInt32 _maxFileSize = (256 * 1000 * 1000);

    /// <summary>
    /// Gets or sets the minimium number of timeshifting files.
    /// </summary>
    /// <value>The minimium files.</value>
    public int MinimumFiles
    {
      get
      {
        return _minFiles;
      }
      set
      {
        _minFiles = value;
      }
    }
    /// <summary>
    /// Gets or sets the maximum number of timeshifting files.
    /// </summary>
    /// <value>The maximum files.</value>
    public int MaximumFiles
    {
      get
      {
        return _maxFiles;
      }
      set
      {
        _maxFiles = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximum filesize for each timeshifting file.
    /// </summary>
    /// <value>The maximum filesize.</value>
    public UInt32 MaximumFileSize
    {
      get
      {
        return _maxFileSize;
      }
      set
      {
        _maxFileSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the use default LNB frequencies.
    /// </summary>
    /// <value>The use default LNB frequencies.</value>
    public bool UseDefaultLnbFrequencies
    {
      get
      {
        return _useDefaultLnbFrequencies;
      }
      set
      {
        _useDefaultLnbFrequencies = value;
      }
    }
    /// <summary>
    /// Gets or sets the LNB low frequency.
    /// </summary>
    /// <value>The LNB low frequency.</value>
    public int LnbLowFrequency
    {
      get
      {
        return _lnbLowFrequency;
      }
      set
      {
        _lnbLowFrequency = value;
      }
    }

    /// <summary>
    /// Gets or sets the LNB switch frequency.
    /// </summary>
    /// <value>The LNB switch frequency.</value>
    public int LnbSwitchFrequency
    {
      get
      {
        return _lnbSwitchFrequency;
      }
      set
      {
        _lnbSwitchFrequency = value;
      }
    }

    /// <summary>
    /// Gets or sets the LNB high frequency.
    /// </summary>
    /// <value>The LNB high frequency.</value>
    public int LnbHighFrequency
    {
      get
      {
        return _lnbHighFrequency;
      }
      set
      {
        _lnbHighFrequency = value;
      }
    }

    /// <summary>
    /// Gets or sets the time out PAT.
    /// </summary>
    /// <value>The time out PAT.</value>
    public int TimeOutPAT
    {
      get
      {
        return _timeoutPAT;
      }
      set
      {
        _timeoutPAT = value;
      }
    }
    /// <summary>
    /// Gets or sets the time out CAT.
    /// </summary>
    /// <value>The time out CAT.</value>
    public int TimeOutCAT
    {
      get
      {
        return _timeoutCAT;
      }
      set
      {
        _timeoutCAT = value;
      }
    }
    /// <summary>
    /// Gets or sets the time out PMT.
    /// </summary>
    /// <value>The time out PMT.</value>
    public int TimeOutPMT
    {
      get
      {
        return _timeoutPMT;
      }
      set
      {
        _timeoutPMT = value;
      }
    }
    /// <summary>
    /// Gets or sets the time out tune.
    /// </summary>
    /// <value>The time out tune.</value>
    public int TimeOutTune
    {
      get
      {
        return _timeoutTune;
      }
      set
      {
        _timeoutTune = value;
      }
    }
    /// <summary>
    /// Gets or sets the time out SDT.
    /// </summary>
    /// <value>The time out SDT.</value>
    public int TimeOutSDT
    {
      get
      {
        return _timeoutSDT;
      }
      set
      {
        _timeoutSDT = value;
      }
    }
    /// <summary>
    /// Gets or sets the time out Analog scanning.
    /// </summary>
    /// <value>The time out Analog scanning.</value>
    public int TimeOutAnalog
    {
      get { 
        return _timeoutAnalog;
      }
      set { 
        _timeoutAnalog = value; 
      }
    }

  }
}
