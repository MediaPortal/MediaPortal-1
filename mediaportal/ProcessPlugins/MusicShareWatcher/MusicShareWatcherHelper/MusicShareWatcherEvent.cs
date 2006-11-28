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
