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
  [Plugin("NowPlaying", "NowPlaying", ListInMenu = false, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/Video.png")]
  public class NowPlaying
  {
    #region IPlugin Members

    public void Initialize(string id)
    {
      ServiceScope.Get<IMessageBroker>().Register(this);
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
    }

    [MessageSubscription(typeof(PlayerStartMessage))]
    protected void OnPlaybackStarted(PlayerStartMessage e)
    {
    }

    [MessageSubscription(typeof(PlayerStartFailedMessage))]
    protected void OnPlaybackFailed(PlayerStartFailedMessage e)
    {
    }
  }
}
