/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;

namespace MediaPortal.TV.Recording
{
  public class DummyGraph : IGraph
  {
    enum State
    {
      Idle,
      Created,
      Viewing,
      TimeShifting,
      Recording,
      Radio
    }
    State _state;
    TVCaptureDevice _card;
    int _audioPid = 123;
    public DummyGraph(TVCaptureDevice card)
    {
      _card = card;

    }
    public bool CreateGraph(int Quality)
    {
      if (_state != State.Idle)
        throw new ApplicationException("wrong state");
      _state = State.Created;
      return true;
    }
    public void DeleteGraph()
    {
      _state = State.Idle;
    }
    public bool StartTimeShifting(TVChannel channel, string strFileName)
    {
      if (_state != State.Created) 
        throw new ApplicationException("wrong state");
      _state = State.TimeShifting;
      return true;
    }
    public bool StopTimeShifting()
    {
      if (_state != State.TimeShifting)
        throw new ApplicationException("wrong state");
      _state = State.TimeShifting;
      return true;
    }
    public bool StartRecording(Hashtable attribtutes, TVRecording recording, TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (_state != State.TimeShifting)
        throw new ApplicationException("wrong state");
      _state = State.Recording;
      return true;
    }
    public void StopRecording()
    {
      if (_state != State.Recording)
        throw new ApplicationException("wrong state");
      _state = State.TimeShifting;
      return ;
    }

    public void TuneChannel(TVChannel channel)
    {
    }
    public int GetChannelNumber()
    {
      return 0;
    }
    public bool SupportsTimeshifting()
    {
      return true;
    }
    public bool StartViewing(TVChannel channel)
    {
      if (_state != State.Created)
        throw new ApplicationException("wrong state");
      _state = State.Viewing;
      return true;
    }
    public bool StopViewing()
    {
      if (_state != State.Viewing)
        throw new ApplicationException("wrong state");
      _state = State.Created;
      return true;
    }
    public bool ShouldRebuildGraph(TVChannel newChannel)
    {
      return false;
    }
    public bool SignalPresent()
    {
      return true;
    }
    public int SignalQuality()
    {
      return 100;
    }
    public int SignalStrength()
    {
      return 100;
    }
    public long VideoFrequency()
    {
      return -1;
    }
    public void Process()
    {
      return ;
    }
    public PropertyPageCollection PropertyPages()
    {
      return null;
    }
    public bool SupportsFrameSize(Size framesize)
    {
      return true;
    }
    public NetworkType Network()
    {
      return NetworkType.Analog;
    }
    public void Tune(object tuningObject, int disecqNo)
    {
    }
    public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels, ref int newRadioChannels, ref int updatedRadioChannels)
    {
    }
    public void StartRadio(RadioStation station)
    {
      if (_state != State.Created)
        throw new ApplicationException("wrong state");
      _state = State.Radio;
      return;
    }
    public void TuneRadioChannel(RadioStation station)
    {
      if (_state != State.Radio)
        throw new ApplicationException("wrong state");
    }
    public void TuneRadioFrequency(int frequency)
    {
      if (_state != State.Radio)
        throw new ApplicationException("wrong state");
    }
    public bool HasTeletext()
    {
      return false;
    }
    public int GetAudioLanguage()
    {
      return _audioPid;
    }
    public void SetAudioLanguage(int audioPid)
    {
      if (audioPid != 123 && audioPid != 456 && audioPid != 789) 
        throw new ArgumentException("invalid pid");

      if (_state != State.TimeShifting &&
          _state != State.Viewing) throw new ArgumentException("invalid state");
      _audioPid = audioPid;
    }
    public ArrayList GetAudioLanguageList()
    {
      ArrayList list = new ArrayList();
      list.Add(123);
      list.Add(456);
      list.Add(789);
      return list;
    }
    public string TvTimeshiftFileName()
    {
      return "live.tv";
    }
    public string RadioTimeshiftFileName()
    {
      return "radio.tv";
    }
    public void GrabTeletext(bool yesNo)
    {
    }
    public void RadioChannelMinMax(out int chanmin, out int chanmax)
    {
      chanmin = 0; chanmax = 255;
    }
    public void TVChannelMinMax(out int chanmin, out int chanmax)
    {
      chanmin = 0; chanmax = 255;
    }
    public IBaseFilter AudiodeviceFilter()
    {
      return null;
    }
    public bool IsTimeShifting()
    {
      return (_state == State.TimeShifting);
    }
    public bool IsEpgGrabbing()
    {
      return false;
    }
    public bool IsEpgDone()
    {
      return true;
    }
    public void GrabEpg(TVChannel chan)
    {
    }
    public void StopRadio()
    {
    }
    public void StopEpgGrabbing()
    {
    }
  }
}
