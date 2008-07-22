/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Implementations.RadioWebStream;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.ChannelLinkage;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  public interface ITvCardHandler
  {
    UserManagement Users { get;}
    DisEqcManagement DisEqC { get;}
    TeletextManagement Teletext { get;}
    ChannelScanning Scanner { get;}
    EpgGrabbing Epg { get;}
    AudioStreams Audio { get;}
    Recorder Recorder { get;}
    TimeShifter TimeShifter { get;}
    CardTuner Tuner { get;}

    ITVCard Card { get;}
    bool IsLocal { get;set;}
    bool IsIdle { get;}
    Card DataBaseCard { get;set;}
    CardType Type { get;}
    string CardName { get;}
    string CardDevice();
    int NumberOfChannelsDecrypting { get;}
    bool HasCA { get;}

    bool SupportsSubChannels { get;}

    void UpdateSignalSate();
    bool TunerLocked { get;}
    int SignalQuality { get;}
    int SignalLevel { get;}
    int MinChannel { get;}
    int MaxChannel { get;}

    IChannel CurrentChannel(ref User user);
    int CurrentDbChannel(ref User user);
    string CurrentChannelName(ref User user);
    int GetCurrentVideoStream(User user);
    bool IsScrambled(ref User user);


    void StopCard(User user);
    void SetParameters();
    void Dispose();
  }
}
