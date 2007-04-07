using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TvControl;
using TvDatabase;

namespace MyTv
{
  public interface ITvChannelNavigator:INotifyPropertyChanged
  {
    /// <summary>
    /// Gets the current group.
    /// </summary>
    /// <value>The current group.</value>
    ChannelGroup CurrentGroup { get;}
    
    /// <summary>
    /// Gets or sets the selected channel.
    /// </summary>
    /// <value>The selected channel.</value>
    Channel SelectedChannel { get;set;}
    
    /// <summary>
    /// Gets or sets the card.
    /// </summary>
    /// <value>The card.</value>
    VirtualCard Card { get;set;}

    /// <summary>
    /// Gets a value indicating whether tvserver is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is recording; otherwise, <c>false</c>.
    /// </value>
    bool IsRecording { get;}
  }
}
