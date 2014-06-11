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
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.IO.Pipes;

using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp
{
  class RtspClients
  {
    private static Int32 counter = 0;
    private Int32 id;
    private string _ip;
    private int _rtpClientPort;
    private int _rtcpClientPort;
    private int _rtpServerPort;
    private int _rtcpServerPort;
    private int _streamId;
    private TuningDetail _tuningDetail;
    private IVirtualCard _card;
    private int _cardId;
    private int _slot;
    private IUser _user;
    private NamedPipeClientStream _namedPipeClientStream;
    private StreamWriter _namedPipeWriter;

    //?src=1&fe=1&freq=12402&pol=v&msys=dvbs&sr=27500&fec=34&pids=0,16
    private int _src;
    private int _freq;
    private string _pol;
    private string _msys;
    private int _sr;
    private int _fec;
    private int _sessionId;
    private ArrayList _pids = new ArrayList();
    private bool _isTunedToFrequency = false;
    private int _tunedToFrequency;

    #region Properties

    /// <summary>
    /// Get the client id.
    /// </summary>
    public int clientId
    {
      get
      {
        return id;
      }
    }

    /// <summary>
    /// Get the session id.
    /// </summary>
    public int sessionId
    {
      get
      {
        return _sessionId;
      }
    }

    /// <summary>
    /// Get/Set the stream id.
    /// </summary>
    public int streamId
    {
      get
      {
        return _streamId;
      }
      set
      {
        _streamId = value;
      }
    }

    /// <summary>
    /// Get/Set the tuningDetail.
    /// </summary>
    public TuningDetail tuningDetail
    {
        get
        {
          return _tuningDetail;
        }
        set
        {
          _tuningDetail = value;
        }
    }

    /// <summary>
    /// Get/Set the card.
    /// </summary>
    public IVirtualCard card
    {
        get
        {
            return _card;
        }
        set
        {
            _card = value;
        }
    }

    /// <summary>
    /// Get/Set the card id.
    /// </summary>
    public int cardId
    {
      get
      {
        return _cardId;
      }
      set
      {
        _cardId = value;
      }
    }

    /// <summary>
    /// Get/Set the slot id.
    /// </summary>
    public int slot
    {
      get
      {
        return _slot;
      }
      set
      {
        _slot = value;
      }
    }

    /// <summary>
    /// Get/Set the user.
    /// </summary>
    public IUser user
    {
        get
        {
            return _user;
        }
        set
        {
            _user = value;
        }
    }

    /// <summary>
    /// Get/Set if we are tuned to a frequency.
    /// </summary>
    public int tunedToFrequency
    {
      get
      {
        return _tunedToFrequency;
      }
      set
      {
        _tunedToFrequency = value;
      }
    }

    /// <summary>
    /// Get/Set the frequency we are tuned to.
    /// </summary>
    public bool isTunedToFrequency
    {
      get
      {
        return _isTunedToFrequency;
      }
      set
      {
        _isTunedToFrequency = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public int src
    {
      get
      {
        return _src;
      }
      set
      {
        _src = value;
      }
    }

    /// <summary>
    /// Get the client frequency.
    /// </summary>
    public int freq
    {
      get
      {
        return _freq;
      }
      set
      {
        _freq = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public string pol
    {
      get
      {
        return _pol;
      }
      set
      {
        _pol = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public string msys
    {
      get
      {
        return _msys;
      }
      set
      {
        _msys = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public Int32 sr
    {
      get
      {
        return _sr;
      }
      set
      {
        _sr = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public Int32 fec
    {
      get
      {
        return _fec;
      }
      set
      {
        _fec = value;
      }
    }

    /// <summary>
    /// Get the pids.
    /// </summary>
    public ArrayList pids
    {
      get
      {
        return _pids;
      }
    }

    /// <summary>
    /// Get the client rtp port.
    /// </summary>
    public int rtpClientPort
    {
      get
      {
        return _rtpClientPort;
      }
      set
      {
        _rtpClientPort = value;
      }
    }

    /// <summary>
    /// Get the client rtcp port.
    /// </summary>
    public int rtcpClientPort
    {
      get
      {
        return _rtcpClientPort;
      }
      set
      {
        _rtcpClientPort = value;
      }
    }

    /// <summary>
    /// Get the server rtp port.
    /// </summary>
    public int rtpServerPort
    {
      get
      {
        return _rtpServerPort;
      }
      set
      {
        _rtpServerPort = value;
      }
    }

    /// <summary>
    /// Get the server rtcp port.
    /// </summary>
    public int rtcpServerPort
    {
      get
      {
        return _rtcpServerPort;
      }
      set
      {
        _rtcpServerPort = value;
      }
    }

    /// <summary>
    /// Get the client ip.
    /// </summary>
    public string ip
    {
      get
      {
        return _ip;
      }
      set
      {
        _ip = value;
      }
    }

    /// <summary>
    /// Get/set the _namedPipeClientStream.
    /// </summary>
    public NamedPipeClientStream namedPipeClientStream
    {
      get
      {
        return _namedPipeClientStream;
      }
      set
      {
        _namedPipeClientStream = value;
      }
    }

    /// <summary>
    /// Get/set the _namedPipeWriter.
    /// </summary>
    public StreamWriter namedPipeWriter
    {
      get
      {
        return _namedPipeWriter;
      }
      set
      {
        _namedPipeWriter = value;
      }
    }

    #endregion Properties

    public RtspClients()
    {
      lock (this)
      {
        counter++;
        id = counter;

        string sessionId = string.Empty;
        Random rand = new Random();
        for (int ctr = 0; ctr <= 7; ctr++)
          sessionId += rand.Next(0,10).ToString();
        _sessionId = int.Parse(sessionId);
      }
    }

    public void addPid(int value)
    {
      _pids.Add(value); 
    }

    public void delPid(int value)
    {
      _pids.Remove(value);
    }
  }
}
