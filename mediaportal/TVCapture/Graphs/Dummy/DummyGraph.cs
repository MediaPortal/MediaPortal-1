#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
  public class DummyGraph : IGraph
  {
    private string _timeShiftingFileName = null;

    private enum State
    {
      Idle,
      Created,
      Viewing,
      TimeShifting,
      Recording,
      Radio,
      Epg
    }

    private State _state;
    private TVCaptureDevice _card;
    private int _audioPid = 123;

    public DummyGraph(TVCaptureDevice card)
    {
      _card = card;
    }

    public bool CreateGraph(int Quality)
    {
      if (_state != State.Idle)
      {
        throw new ApplicationException("wrong state");
      }
      _state = State.Created;
      //Trace.WriteLine(String.Format("card:{0} CreateGraph", _card.FriendlyName));
      return true;
    }

    public void DeleteGraph()
    {
      //Trace.WriteLine(String.Format("card:{0} DeleteGraph", _card.FriendlyName));
      _state = State.Idle;
    }

    public bool StartTimeShifting(TVChannel channel, string strFileName)
    {
      if (_state != State.Created)
      {
        throw new ApplicationException("wrong state");
      }
      //We create the timeshifting file, because elsewhere it's existance is tested
      _timeShiftingFileName = strFileName;
      FileInfo fi = new FileInfo(_timeShiftingFileName);
      DirectoryInfo di = fi.Directory;
      if (!di.Exists)
      {
        di.Create();
      }
      fi.Create().Close();
      //Trace.WriteLine(String.Format("card:{0} StartTimeShifting:{1}", _card.FriendlyName, channel.Name));
      _state = State.TimeShifting;
      return true;
    }

    public bool StopTimeShifting()
    {
      if (_state != State.TimeShifting)
      {
        throw new ApplicationException("wrong state");
      }
      //Delete the timeshifting file again (if it exists)
      FileInfo fi = new FileInfo(_timeShiftingFileName);
      DirectoryInfo di = fi.Directory;
      if (di.Exists && fi.Exists)
      {
        fi.Delete();
      }
      //Trace.WriteLine(String.Format("card:{0} StopTimeShifting", _card.FriendlyName));
      _state = State.Created;
      return true;
    }

    public bool StartRecording(Hashtable attribtutes, TVRecording recording, TVChannel channel,
                               ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (_state != State.TimeShifting)
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} StartRecording", _card.FriendlyName));
      _state = State.Recording;
      return true;
    }

    public void StopRecording()
    {
      if (_state != State.Recording)
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} StopRecording", _card.FriendlyName));
      _state = State.TimeShifting;
      return;
    }

    public void TuneChannel(TVChannel channel)
    {
      if (_state != State.TimeShifting && _state != State.Viewing)
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} TuneChannel:{1}", _card.FriendlyName, channel.Name));
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
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} StartViewing:{1}", _card.FriendlyName, channel.Name));
      _state = State.Viewing;
      return true;
    }

    public bool StopViewing()
    {
      if (_state != State.Viewing)
      {
        throw new ApplicationException("wrong state");
      }
      _state = State.Created;
      //Trace.WriteLine(String.Format("card:{0} StopViewing", _card.FriendlyName));
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
      return;
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

    public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels,
                              ref int newRadioChannels, ref int updatedRadioChannels)
    {
    }

    public void StartRadio(RadioStation station)
    {
      if (_state != State.Created && _state != State.Radio)
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} StartRadio:{1}", _card.FriendlyName,station.Name));
      _state = State.Radio;
      return;
    }

    public void TuneRadioChannel(RadioStation station)
    {
      if (_state != State.Radio)
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} TuneRadioChannel:{1}", _card.FriendlyName, station.Name));
    }

    public void TuneRadioFrequency(int frequency)
    {
      if (_state != State.Radio)
      {
        throw new ApplicationException("wrong state");
      }
      //Trace.WriteLine(String.Format("card:{0} TuneRadioFrequency:{1}", _card.FriendlyName, frequency));
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
      {
        throw new ArgumentException("invalid pid");
      }

      if (_state != State.TimeShifting &&
          _state != State.Viewing)
      {
        throw new ArgumentException("invalid state");
      }
      _audioPid = audioPid;
      //Trace.WriteLine(String.Format("card:{0} SetAudioLanguage:{1}", _card.FriendlyName, audioPid));
    }

    public ArrayList GetAudioLanguageList()
    {
      ArrayList list = new ArrayList();
      list.Add(123);
      list.Add(456);
      list.Add(789);
      //Trace.WriteLine(String.Format("card:{0} GetAudioLanguageList", _card.FriendlyName));
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
      chanmin = 0;
      chanmax = 255;
    }

    public void TVChannelMinMax(out int chanmin, out int chanmax)
    {
      chanmin = 0;
      chanmax = 255;
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
      return (_state == State.Epg);
    }

    public bool IsEpgDone()
    {
      return true;
    }

    public bool GrabEpg(TVChannel chan)
    {
      if (_state != State.Created)
      {
        throw new ApplicationException("wrong state");
      }
      _state = State.Epg;
      return true;
      //Trace.WriteLine(String.Format("card:{0} GrabEpg", _card.FriendlyName));
    }

    public void StopRadio()
    {
      if (_state != State.Radio)
      {
        throw new ApplicationException("wrong state");
      }
      _state = State.Created;
      //Trace.WriteLine(String.Format("card:{0} StopRadio", _card.FriendlyName));
    }

    public bool StopEpgGrabbing()
    {
      if (_state != State.Epg)
      {
        throw new ApplicationException("wrong state");
      }
      _state = State.Created;
      return true;
      //Trace.WriteLine(String.Format("card:{0} StopEpgGrabbing", _card.FriendlyName));
    }

    public bool SupportsHardwarePidFiltering()
    {
      return false;
    }

    public bool Supports5vAntennae()
    {
      return false;
    }

    public bool SupportsCamSelection()
    {
      return false;
    }

    public bool CanViewTimeShiftFile()
    {
      if (_state != State.TimeShifting && _state != State.Recording)
      {
        return false;
      }
      return true;
    }

    public bool IsRadio()
    {
      return (_state == State.Radio);
    }

    public bool IsRecording()
    {
      return (_state == State.Recording);
    }

    public string LastError()
    {
      return string.Empty;
    }
  }
}