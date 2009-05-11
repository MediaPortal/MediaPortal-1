#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.MusicShareWatcher
{
  class MusicShareWatcherEvent
  {
    #region Enums
    public enum EventType
    {
      Create,
      Change,
      Delete,
      Rename,
      DeleteDirectory
    }
    #endregion

    #region Variables
    private EventType m_Type;
    private string m_strFilename;
    private string m_strOldFilename;
    #endregion

    #region Constructors/Destructors
    public MusicShareWatcherEvent(EventType type, string strFilename)
    {
      m_Type = type;
      m_strFilename = strFilename;
      m_strOldFilename = null;
    }

    public MusicShareWatcherEvent(EventType type, string strFilename, string strOldFilename)
    {
      m_Type = type;
      m_strFilename = strFilename;
      m_strOldFilename = strOldFilename;
    }
    #endregion

    #region Properties
    public EventType Type
    {
      get
      {
        return m_Type;
      }
    }

    public string FileName
    {
      get
      {
        return m_strFilename;
      }
    }

    public string OldFileName
    {
      get
      {
        return m_strOldFilename;
      }
    }
    #endregion
  }
}
