using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public enum PlayerServiceResultEnum
  {
    /// <summary>
    /// Plugin can't handle the link.
    /// </summary>
    Unsupported,

    /// <summary>
    /// Plugin returned new url link.
    /// </summary>
    UrlLink,

    /// <summary>
    /// Plugin can handle the url by itself. Playback will be redirected to the plugin along with 'loadParameter'.
    /// </summary>
    PluginPlayback,

    /// <summary>
    /// Plugin can handle the link but error has occured.
    /// </summary>
    Error
  }
}
