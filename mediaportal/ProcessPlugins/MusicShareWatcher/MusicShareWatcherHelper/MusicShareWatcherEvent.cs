#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace MediaPortal.MusicShareWatcher
{
  internal class MusicShareWatcherEvent
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
      get { return m_Type; }
    }

    public string FileName
    {
      get { return m_strFilename; }
    }

    public string OldFileName
    {
      get { return m_strOldFilename; }
    }

    #endregion
  }
}