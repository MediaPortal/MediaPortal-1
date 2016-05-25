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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
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
          if (_cardHandler.Card.IsEnabled)
          {
            IChannelScanner scanner = _cardHandler.Card.ChannelScanningInterface;
            if (scanner != null)
            {
              return scanner.IsScanning;
            }
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex);
        }
        return false;
      }
    }

    public void Scan(IChannel channel, bool isFastNetworkScan, out IList<ScannedChannel> channels, out IDictionary<ChannelGroupType, IDictionary<ulong, string>> groupNames)
    {
      channels = null;
      groupNames = null;
      try
      {
        if (!_cardHandler.Card.IsEnabled)
        {
          return;
        }
        
        IChannelScanner scanner = _cardHandler.Card.ChannelScanningInterface;
        if (scanner != null)
        {
          scanner.Scan(channel, isFastNetworkScan, out channels, out groupNames);
        }
      }
      catch (TvExceptionNoSignal)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    public TuningDetail[] ScanNIT(IChannel channel)
    {
      try
      {
        if (!_cardHandler.Card.IsEnabled)
        {
          return null;
        }
       
        IChannelScanner scanner = _cardHandler.Card.ChannelScanningInterface;
        if (scanner != null)
        {
          IList<TuningDetail> tuningDetails = scanner.ScanNetworkInformation(channel);
          if (tuningDetails != null)
          {
            TuningDetail[] returnArray = new TuningDetail[tuningDetails.Count];
            tuningDetails.CopyTo(returnArray, 0);
            return returnArray;
          }
        }
      }
      catch (TvExceptionNoSignal)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      return null;
    }
  }
}