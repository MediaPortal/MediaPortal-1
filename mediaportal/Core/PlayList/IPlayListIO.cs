using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Playlists
{
  public interface IPlayListIO
  {
    bool Load(PlayList playlist, string fileName);
    void Save(PlayList playlist, string fileName);
  }
}
