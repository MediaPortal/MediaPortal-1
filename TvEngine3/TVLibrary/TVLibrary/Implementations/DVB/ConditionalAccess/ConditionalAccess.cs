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
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using DirectShowLib.BDA;
using TvDatabase;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which handles the conditional access modules for a tv card
  /// (CI and CAM)
  /// </summary>
  public class ConditionalAccess
  {
    #region variables

    private readonly bool _useCam;

    /// <summary>
    /// CA decryption limit, 0 for disable CA
    /// </summary>
    private readonly int _decryptLimit;

    private readonly CamType _camType = CamType.Default;
    private readonly Dictionary<int, ConditionalAccessContext> _mapSubChannels;

    private readonly ICiMenuActions _ciMenu;


    /// <summary>
    /// Accessor for CI Menu handler
    /// </summary>
    public ICiMenuActions CiMenu
    {
      get { return _ciMenu; }
    }

    #endregion

    //ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalAccess"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The capture filter.</param>
    /// <param name="card">Determines the type of TV card</param>
    public ConditionalAccess(IBaseFilter tunerFilter, IBaseFilter analyzerFilter, TvCardBase card)
    {
      //System.Diagnostics.Debugger.Launch();        
      if (card != null && card.DevicePath != null)
      {
        //fetch decrypt limit from DB and apply it.
        TvBusinessLayer layer = new TvBusinessLayer();
        Card c = layer.GetCardByDevicePath(card.DevicePath);
        _decryptLimit = c.DecryptLimit;
        _useCam = c.CAM;
        _camType = (CamType)c.CamType;
        Log.Log.WriteFile("CAM is {0} model", _camType);
      }
    }

    /// <summary>
    /// Adds the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void AddSubChannel(int id)
    {
      /*if (!_mapSubChannels.ContainsKey(id))
      {
        _mapSubChannels[id] = new ConditionalAccessContext();
      }*/
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void FreeSubChannel(int id)
    {
      /*if (_mapSubChannels.ContainsKey(id))
      {
        Log.Log.WriteFile("FreeSubChannel CA: freeing sub channel : {0}", id);
        _mapSubChannels.Remove(id);
      }
      else
      {
        Log.Log.WriteFile("FreeSubChannel CA: tried to free non existing sub channel : {0}", id);
        return;
      }*/
    }

    /// <summary>
    /// returns if cam is ready or not
    /// </summary>
    public bool IsCamReady()
    {
      return true;
    }

    /// <summary>
    /// CA enabled or disabled ?
    /// </summary>
    /// <value>Is CA enabled or disabled</value>
    public bool UseCA
    {
      get { return _useCam; }
    }

    /// <summary>
    /// CA decryption limit, 0 for unlimited
    /// </summary>
    /// <value>The number of channels decrypting that are able to decrypt.</value>
    public int DecryptLimit
    {
      get { return _decryptLimit; }
    }

    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        if (_mapSubChannels == null)
          return 0;
        if (_mapSubChannels.Count == 0)
          return 0;
        if (_decryptLimit == 0)
          return 0; //CA disabled, so no channels are decrypting.

        List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();

        Dictionary<int, ConditionalAccessContext>.Enumerator en = _mapSubChannels.GetEnumerator();

        while (en.MoveNext())
        {
          bool exists = false;
          ConditionalAccessContext context = en.Current.Value;
          if (context != null)
          {
            foreach (ConditionalAccessContext c in filteredChannels)
            {
              if (c.Channel != null && context.Channel != null)
              {
                if (c.Channel.Equals(context.Channel))
                {
                  exists = true;
                  break;
                }
              }
            }
            if (!exists)
            {
              if (context.Channel != null && !context.Channel.FreeToAir)
              {
                filteredChannels.Add(context);
              }
            }
          }
        }
        return filteredChannels.Count;
      }
    }

    /// <summary>
    /// Patches the PMT to force standard AC3 header.
    /// </summary>
    /// <param name="pmt">byte array containing the PMT</param>
    /// <returns></returns>
    private static byte[] PatchPmtForAstonCrypt2(byte[] pmt)
    {
      if (pmt == null || pmt.Length < 12)
      {
        return pmt;
      }

      // This function doesn't change the length of the PMT data.
      byte[] outputPmt = new byte[pmt.Length];

      // Directly copy the input PMT data into the output PMT.
      Buffer.BlockCopy(pmt, 0, outputPmt, 0, pmt.Length);

      // Skip to the elementary streams data.
      int offset = 12 + ((pmt[10] & 0x0f) << 8) + pmt[11];
      if (offset > pmt.Length - 4)
      {
        return outputPmt;
      }

      // Patch the stream type on AC3 streams.
      while (offset + 4 < pmt.Length - 4)
      {
        byte streamType = pmt[offset];
        int esInfoLength = ((pmt[offset + 3] & 0x0f) << 8) + pmt[offset + 4];
        if (streamType == (byte)StreamType.Mpeg2Part1PrivateData)
        {
          int streamTypeOffset = offset;
          offset += 5;
          int length = esInfoLength;
          while (length > 0)
          {
            byte descriptorTag = pmt[offset];
            byte descriptorLength = pmt[offset + 1];
            if (descriptorTag == (byte)DescriptorType.Ac3)
            {
              outputPmt[streamTypeOffset] = (byte)StreamType.Ac3Audio;
              offset = streamTypeOffset + 5 + esInfoLength;
              break;
            }
            length -= (descriptorLength + 2);
            offset += descriptorLength + 2;
          }
        }
        else
        {
          offset += 5 + esInfoLength;
        }
      }

      return outputPmt;
    }

    /// <summary>
    /// Ask the conditional access interface to start decrypting a channel by
    /// sending PMT to the interface.
    /// </summary>
    /// <param name="subChannel">The sub channel.</param>
    /// <param name="channel">The channel to decrypt.</param>
    /// <param name="pmt">A byte array containing the PMT.</param>
    /// <param name="pmtLength">The number of data bytes in the PMT array.</param>
    /// <param name="audioPid">The PID of the current audio stream.</param>
    /// <returns><c>true</c> if the conditional access interface successfully starts decrypting the channel, otherwise <c>false</c></returns>
    public bool SendPmt(int subChannel, DVBBaseChannel channel, byte[] pmt, int audioPid)
    {
      return true;
    }

    /// <summary>
    /// Instructs the cam/ci module to use hardware filter and only send the pids listed in pids to the pc
    /// </summary>
    /// <param name="subChannel">The sub channel id</param>
    /// <param name="channel">The current tv/radio channel.</param>
    /// <param name="pids">The pids.</param>
    /// <remarks>when the pids array is empty, pid filtering is disabled and all pids are received</remarks>
    public bool SendPids(int subChannel, DVBBaseChannel channel, List<ushort> pids)
    {
      /*try
      {
        List<ushort> HwPids = new List<ushort>();

        _mapSubChannels[subChannel].Pids = pids;

        Dictionary<int, ConditionalAccessContext>.Enumerator enSubch = _mapSubChannels.GetEnumerator();
        while (enSubch.MoveNext())
        {
          List<ushort> enPid = enSubch.Current.Value.Pids;
          if (enPid != null)
          {
            for (int i = 0; i < enPid.Count; ++i)
            {
              if (!HwPids.Contains(enPid[i]))
              {
                HwPids.Add(enPid[i]);
              }
            }
          }
        }

        ModulationType modulation = ModulationType.ModNotSet;
        if (channel is DVBSChannel)
        {
          modulation = (channel as DVBSChannel).ModulationType;
        }
        else if (channel is DVBCChannel)
        {
          modulation = (channel as DVBCChannel).ModulationType;
        }

        /*if (_digitalEveryWhere != null)
        {
          return _digitalEveryWhere.SetHardwareFilterPids(modulation, HwPids);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }*/
      return true;
    }

    /// <summary>
    /// Property to set CI Menu Handler 
    /// </summary>
    public ICiMenuCallbacks CiMenuHandler
    {
      set
      {
        if (_ciMenu != null)
        {
          _ciMenu.SetCiMenuHandler(value);
        }
      }
    }
  }
}