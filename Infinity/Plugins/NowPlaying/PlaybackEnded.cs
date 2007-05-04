using System;
using System.Collections.Generic;
using System.Text;

using ProjectInfinity;
using ProjectInfinity.Menu;
using ProjectInfinity.Messaging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Players;
using ProjectInfinity.Controls;
namespace NowPlaying
{
  public class PlaybackEnded : View
  {
    public PlaybackEnded()
    {

      DataContext = new BaseViewModel() ;
    }
  }
}
