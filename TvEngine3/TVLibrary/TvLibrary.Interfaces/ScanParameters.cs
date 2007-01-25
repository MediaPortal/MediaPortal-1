/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
  }
}
