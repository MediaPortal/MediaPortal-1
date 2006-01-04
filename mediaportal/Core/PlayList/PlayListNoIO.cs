using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Playlists {
  public class PlayListNoIO : PlayList {
    public override bool Load( string filename ) {
      throw new Exception( "The method or operation is not implemented." );
    }

    public override void Save( string filename ) {
      throw new Exception( "The method or operation is not implemented." );
    }
  }
}
