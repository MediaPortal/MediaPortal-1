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

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class ChannelScanning : IChannelScanning
  {
    private readonly ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisEqcManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public ChannelScanning(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning
    {
      get
      {
        try
        {
          if (_cardHandler.Card.IsEnabled == false)
          {
            return false;
          }

          IChannelScanner scanner = _cardHandler.Card.ChannelScanningInterface;
          if (scanner != null)
          {
            return scanner.IsScanning;
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex);
        }
        return false;
      }
    }

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="channel">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(IChannel channel)
    {
      try
      {
        if (_cardHandler.Card.IsEnabled == false)
        {
          return new List<IChannel>().ToArray();
        }
        
        IChannelScanner scanner = _cardHandler.Card.ChannelScanningInterface;
        if (scanner == null)
          return null;
        List<IChannel> channelsFound = scanner.Scan(channel);
        if (channelsFound == null)
          return null;
        return channelsFound.ToArray();
      }
      catch (TvExceptionNoSignal)
      {
        //ignore
        return null;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return null;
      }
    }

    public TuningDetail[] ScanNIT(IChannel channel)
    {
      try
      {
        if (_cardHandler.Card.IsEnabled == false)
        {
          return new List<TuningDetail>().ToArray();
        }
       
        IChannelScanner scanner = _cardHandler.Card.ChannelScanningInterface;
        if (scanner == null)
          return null;
        List<TuningDetail> channelsFound = scanner.ScanNIT(channel);
        if (channelsFound == null)
          return null;
        return channelsFound.ToArray();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return null;
      }
    }
  }
}