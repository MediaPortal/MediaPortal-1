using System;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvLibrary.Interfaces;

namespace TvEngine.Events
{
  public class TvServerEventArgs : EventArgs
  {
    #region variables
    User _user;
    VirtualCard _card;
    IChannel _channel;
    IController _controller;
    #endregion

    #region properties
    /// <summary>
    /// Gets the controller.
    /// </summary>
    /// <value>The controller.</value>
    public IController Controller
    {
      get
      {
        return _controller;
      }
    }
    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public User User
    {
      get
      {
        return _user;
      }
    }

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public VirtualCard Card
    {
      get
      {
        return _card;
      }
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public IChannel channel
    {
      get
      {
        return _channel;
      }
    }
    #endregion
  }
}
