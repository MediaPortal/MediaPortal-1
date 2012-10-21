#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#region

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

#endregion

[assembly: InternalsVisibleTo("TVServiceTests")]

namespace Mediaportal.TV.Server.TVLibrary
{
  /// <summary>
  ///   Class which holds the context for a specific card
  /// </summary>
  public class TvCardContext : ITvCardContext
  {
    #region variables
    //private readonly Timer _timer = new Timer();
    private readonly IDictionary<string,IUser> _users;
    private readonly IDictionary<string, IUser> _usersHistory;
    private readonly IDictionary<string, ParkedUser> _parkedUsers = new Dictionary<string, ParkedUser>();
    //holding a list of all the timeshifting users that have been stopped - mkaing it possible for the client to query the possible stop reason.

    private OwnerSubChannel _ownerSubChannel;
    

    #endregion

    #region ctor

    /// <summary>
    ///   Initializes a new instance of the <see cref = "TvCardContext" /> class.
    /// </summary>
    public TvCardContext()
    {      
      _users = new Dictionary<string, IUser>();
      _usersHistory = new Dictionary<string, IUser>();
      _ownerSubChannel = null;

      //_timer.Interval = 60000;
      //_timer.Enabled = true;
      //_timer.Elapsed += _timer_Elapsed;
    }

    #endregion

    #region public methods          

    /// <summary>
    ///   Sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    public OwnerSubChannel OwnerSubChannel
    {
      get { return _ownerSubChannel; }
      set { _ownerSubChannel = value; }      
    }

    /// <summary>
    ///   Gets the users.
    /// </summary>
    /// <value>The users.</value>
    public IDictionary<string, IUser> Users
    {
      get { return _users; }
    }

    public IDictionary<string, ParkedUser> ParkedUsers
    {
      get { return _parkedUsers; }
    }

    public IDictionary<string, IUser> UsersHistory
    {
      get { return _usersHistory; }
    }

    #endregion

    /*private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        foreach (KeyValuePair<string, IUser> existingUser in _users)
        {
          History history = existingUser.Value.History as History;
          if (history != null)
          {
            Channel channel = ChannelManagement.GetChannel(existingUser.Value.IdChannel);
            if (channel != null)
            {
              Program p = new ChannelBLL(channel).CurrentProgram;
              if (p != null && p.startTime != history.startTime)
              {                
                ChannelManagement.SaveChannelHistory(history);
                var history1 = new History
                                 {
                                   idChannel = channel.idChannel,
                                   startTime = p.startTime,
                                   endTime = p.endTime,
                                   title = p.title,
                                   description = p.description,
                                   ProgramCategory = p.ProgramCategory,
                                   recorded = false,
                                   watched = 0
                                 };

                existingUser.Value.History = history1;                
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }*/

    
  }

  
}