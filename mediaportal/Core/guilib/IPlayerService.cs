using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public interface IPlayerService
  {
    /// <summary>
    /// Executes call to get final playback url from given strLink argument.
    /// </summary>
    /// <param name="strLink">Original playback url link.</param>
    /// <param name="result">Final playback link. Can be either url string or object passed as 'loadParametr' when the window is being activated.</param>
    /// <returns>Result of the operation.</returns>
    PlayerServiceResultEnum GetPlaybackLink(string strLink, out object result);
  }
}
