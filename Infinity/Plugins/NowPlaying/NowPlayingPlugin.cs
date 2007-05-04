using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Messaging;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Players;

namespace NowPlaying
{
  [Plugin("NowPlaying", "NowPlaying", AutoStart = true, ListInMenu = false, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Video.png")]
  public class NowPlayingPlugin : IPlugin, IAutoStart
  {
    #region IPlugin Members

    public void Initialize(string id)
    {
    }

    #endregion

    #region IMenuCommand Members

    public void Run()
    {
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
    }

    #endregion

    [MessageSubscription(typeof(PlayerEndedMessage))]
    protected void OnPlaybackEnded(PlayerEndedMessage e)
    {
      ServiceScope.Get<INavigationService>().Navigate(new PlaybackEnded());
    }

    [MessageSubscription(typeof(PlayerStartMessage))]
    protected void OnPlaybackStarted(PlayerStartMessage e)
    {
    }

    [MessageSubscription(typeof(PlayerStartFailedMessage))]
    protected void OnPlaybackFailed(PlayerStartFailedMessage e)
    {
    }

    #region IAutoStart Members

    public void Startup()
    {
      ServiceScope.Get<IMessageBroker>().Register(this);
    }

    #endregion
  }
}
