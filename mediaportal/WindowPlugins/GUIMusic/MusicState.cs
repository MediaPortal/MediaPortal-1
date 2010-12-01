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

using MediaPortal.GUI.Library;
using WindowPlugins;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicState : StateBase
  {
    
    private static PlayMode m_playMode = PlayMode.PLAY_MODE;
    
    static MusicState()
    {
      m_iStartWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
    }

    public enum PlayMode
    {
      PLAY_MODE,
      PLAYLIST_MODE,
    }

    /// <summary>
    /// This property controls how pressing enter/ok are handled 
    /// when viewing music tracks
    /// in PLAY_MODE enter/ok will do the same as play (clear playlist
    /// then start selected track)
    /// in PLAYLIST_MODE enter/ok will add the selected track to the 
    /// current playlist
    /// </summary>    
    public static PlayMode CurrentPlayMode
    {
      get { return m_playMode; }
      set { m_playMode = value; }
    }
    
  }
}