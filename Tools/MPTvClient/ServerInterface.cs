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
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using TvControl;
using TvLibrary.Interfaces;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using System.Windows.Forms;
using System.Xml;

namespace MPTvClient
{
  public class ServerInterface
  {
    IList groups;
    IList radioGroups;
    IList channels;
    IList mappings;
    IList cards;
    User me;
    bool _isTimeShifting = false;
    public Exception lastException = null;

    public bool GetDatabaseConnectionString(out string connStr,out string provider)
    {
      connStr = "";
      provider = "";
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", Application.StartupPath));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString"); ;
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name"); ;
        connStr = nodeConnection.InnerText;
        provider = nodeProvider.InnerText;
      }
      catch (Exception)
      {
        connStr = "";
        provider = "";
        return false;
      }
      return true;
    }

    #region Connection functions
    public bool Connect(string hostname)
    {
      try
      {
        string connStr;
        string provider; 
        if (!GetDatabaseConnectionString(out connStr,out provider))
          return false;
        RemoteControl.HostName = hostname;
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connStr);
        me = new User();
        me.IsAdmin = true;
        groups = ChannelGroup.ListAll();
        radioGroups = RadioChannelGroup.ListAll();
        channels = Channel.ListAll();
        mappings = GroupMap.ListAll();
        cards = Card.ListAll();
      }
      catch (Exception ex)
      {
        lastException = ex;
        RemoteControl.Clear();
        return false;
      }
      return true;
    }
    public void ResetConnection()
    {
      RemoteControl.Clear();
      _isTimeShifting = false;
    }
    #endregion

    #region Controll functions
    public TvResult StartTimeShifting(int idChannel, ref string rtspURL)
    {
      if (_isTimeShifting)
        StopTimeShifting();
      VirtualCard vcard;
      TvResult result;
      me = new User();
      try
      {
        result = RemoteControl.Instance.StartTimeShifting(ref me, idChannel, out vcard);
      }
      catch (Exception ex)
      {
        lastException = ex;
        return TvResult.UnknownError;
      }
      if (result == TvResult.Succeeded)
      {
        me.IdChannel = idChannel;
        me.CardId = vcard.Id;
        _isTimeShifting = true;
        rtspURL = vcard.RTSPUrl;
      }
      return result;
    }
    public bool StopTimeShifting()
    {
      if (!_isTimeShifting)
        return true;
      bool result;
      try
      {
        result = RemoteControl.Instance.StopTimeShifting(ref me);
      }
      catch (Exception ex)
      {
        lastException = ex;
        return false;
      }
      _isTimeShifting = false;
      return result;
    }
    public string GetRecordingURL(int idRecording)
    {
      TvServer server = new TvServer();
      string url = server.GetStreamUrlForFileName(idRecording);
      return url;
    }
    public void DeleteRecording(int idRecording)
    {
      RemoteControl.Instance.DeleteRecording(idRecording);
    }
    public void DeleteSchedule(int idSchedule)
    {
      Schedule sched = Schedule.Retrieve(idSchedule);
      sched.Remove();

    }
    #endregion

    #region Info functions
    public ReceptionDetails GetReceptionDetails()
    {
      VirtualCard vcard;
      try
      {
        vcard = new VirtualCard(me, RemoteControl.HostName);
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      ReceptionDetails details = new ReceptionDetails();
      details.signalLevel = vcard.SignalLevel;
      details.signalQuality = vcard.SignalQuality;
      return details;
    }
    public List<StreamingStatus> GetStreamingStatus()
    {
      List<StreamingStatus> states = new List<StreamingStatus>();
      VirtualCard vcard;
      try
      {
        foreach (Card card in cards)
        {
          User user = new User();
          User[] usersForCard=null;
          user.CardId = card.IdCard;
          try
          {
             usersForCard = RemoteControl.Instance.GetUsersForCard(card.IdCard);
          }
          catch (Exception)
          {
            continue;
          }
          if (usersForCard == null)
          {
            StreamingStatus state = new StreamingStatus();
            vcard = new VirtualCard(user, RemoteControl.HostName);
            string tmp = "idle";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            state.cardId = card.IdCard;
            state.cardName = vcard.Name;
            state.cardType = vcard.Type.ToString();
            state.status = tmp;
            state.channelName = "";
            state.userName = "";
            states.Add(state);
            continue;
          }
          if (usersForCard.Length == 0)
          {
            StreamingStatus state = new StreamingStatus();
            vcard = new VirtualCard(user, RemoteControl.HostName);
            string tmp = "idle";
            if (vcard.IsScanning) tmp = "Scanning";
            if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
            state.cardId = card.IdCard;
            state.cardName = vcard.Name;
            state.cardType = vcard.Type.ToString();
            state.status = tmp;
            state.channelName = "";
            state.userName = "";
            states.Add(state);
            continue;
          }
          for (int i = 0; i < usersForCard.Length; ++i)
          {
            StreamingStatus state = new StreamingStatus();
            string tmp = "idle";
            vcard = new VirtualCard(usersForCard[i], RemoteControl.HostName);
            if (vcard.IsTimeShifting)
              tmp = "Timeshifting";
            else
              if (vcard.IsRecording)
                tmp = "Recording";
              else
                if (vcard.IsScanning)
                  tmp = "Scanning";
                else
                  if (vcard.IsGrabbingEpg)
                    tmp = "Grabbing EPG";
            state.cardId = card.IdCard;
            state.cardName = vcard.Name;
            state.cardType = vcard.Type.ToString();
            state.status = tmp;
            state.channelName = vcard.ChannelName;
            state.userName = vcard.User.Name;
            states.Add(state);
          }
        }
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return states;
    }
    public List<string> GetGroupNames()
    {
      if (!RemoteControl.IsConnected)
        return null;
      List<string> lGroups = new List<string>();
      try
      {
        foreach (ChannelGroup group in groups)
          lGroups.Add(group.GroupName);
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return lGroups;
    }
    public List<string> GetRadioGroupNames()
    {
      if (!RemoteControl.IsConnected)
        return null;
      List<string> lGroups = new List<string>();
      try
      {
        foreach (RadioChannelGroup group in radioGroups)
          lGroups.Add(group.GroupName);
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return lGroups;
    }
    public List<ChannelInfo> GetChannelInfosForGroup(string groupName)
    {
      List<ChannelInfo> refChannelInfos = new List<ChannelInfo>();
      try
      {
        foreach (ChannelGroup group in groups)
        {
          if (group.GroupName == groupName)
          {
            IList maps = group.ReferringGroupMap();
            TvDatabase.Program epg;
            foreach (GroupMap map in maps)
            {
              ChannelInfo channelInfo = new ChannelInfo();
              channelInfo.channelID = map.ReferencedChannel().IdChannel.ToString();
              channelInfo.name = map.ReferencedChannel().DisplayName;
              epg = map.ReferencedChannel().CurrentProgram;
              channelInfo.epgNow = new ProgrammInfo();
              if (epg != null)
              {
                channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                channelInfo.epgNow.description = epg.Title;
              }
              epg = map.ReferencedChannel().NextProgram;
              channelInfo.epgNext = new ProgrammInfo();
              if (epg != null)
              {
                channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                channelInfo.epgNext.description = epg.Title;
              }
              refChannelInfos.Add(channelInfo);
            }
            break;
          }
        }
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return refChannelInfos;
    }
    public List<ChannelInfo> GetChannelInfosForRadioGroup(string groupName)
    {
      List<ChannelInfo> refChannelInfos = new List<ChannelInfo>();
      try
      {
        foreach (RadioChannelGroup group in radioGroups)
        {
          if (group.GroupName == groupName)
          {
            IList maps = group.ReferringRadioGroupMap();
            TvDatabase.Program epg;
            foreach (RadioGroupMap map in maps)
            {
              ChannelInfo channelInfo = new ChannelInfo();
              channelInfo.channelID = map.ReferencedChannel().IdChannel.ToString();
              channelInfo.name = map.ReferencedChannel().DisplayName;
              epg = map.ReferencedChannel().CurrentProgram;
              channelInfo.epgNow = new ProgrammInfo();
              if (epg != null)
              {
                channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                channelInfo.epgNow.description = epg.Title;
              }
              epg = map.ReferencedChannel().NextProgram;
              channelInfo.epgNext = new ProgrammInfo();
              if (epg != null)
              {
                channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                channelInfo.epgNext.description = epg.Title;
              }
              refChannelInfos.Add(channelInfo);
            }
            break;
          }
        }
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return refChannelInfos;
    }
    public List<ChannelInfo> GetRadioChannels()
    {
      List<ChannelInfo> radioChannels = new List<ChannelInfo>();
      try
      {
        TvDatabase.Program epg;
        foreach (Channel chan in channels)
        {
          if (!chan.IsRadio)
            continue;
          ChannelInfo channelInfo = new ChannelInfo();
          channelInfo.channelID = chan.IdChannel.ToString();
          channelInfo.name = chan.DisplayName;
          channelInfo.isWebStream = chan.IsWebstream();
          epg = chan.CurrentProgram;
          channelInfo.epgNow = new ProgrammInfo();
          if (epg != null)
          {
            channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
            channelInfo.epgNow.description = epg.Title;
          }
          epg = chan.NextProgram;
          channelInfo.epgNext = new ProgrammInfo();
          if (epg != null)
          {
            channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
            channelInfo.epgNext.description = epg.Title;
          }
          radioChannels.Add(channelInfo);
        }
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return radioChannels;
    }
    public string GetWebStreamURL(int idChannel)
    {
      string url = "";
      Channel chan = Channel.Retrieve(idChannel);
      IList details = chan.ReferringTuningDetail();
      foreach (TuningDetail detail in details)
      {
        if (detail.ChannelType == 5)
        {
          url = detail.Url;
          break;
        }
      }
      return url;
    }
    public List<RecordingInfo> GetRecordings()
    {
      List<RecordingInfo> recInfos = new List<RecordingInfo>();
      try
      {
        IList recordings = Recording.ListAll();
        foreach (Recording rec in recordings)
        {
          RecordingInfo recInfo = new RecordingInfo();
          recInfo.recordingID = rec.IdRecording.ToString();
          recInfo.title = rec.Title;
          recInfo.description = rec.Description;
          recInfo.genre = rec.Genre;
          recInfo.timeInfo = rec.StartTime.ToString() + "-" + rec.EndTime.ToString();
          recInfos.Add(recInfo);
        }
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return recInfos;
    }
    public List<ScheduleInfo> GetSchedules()
    {
      List<ScheduleInfo> schedInfos = new List<ScheduleInfo>();
      try
      {
        IList schedules = Schedule.ListAll();
        foreach (Schedule schedule in schedules)
        {
          ScheduleInfo sched = new ScheduleInfo();
          sched.scheduleID = schedule.IdSchedule.ToString();
          sched.startTime = schedule.StartTime;
          sched.endTime = schedule.EndTime;
          sched.channelName = schedule.ReferencedChannel().Name;
          sched.description = schedule.ProgramName;
          ScheduleRecordingType stype = (ScheduleRecordingType)schedule.ScheduleType;
          sched.type = stype.ToString();

          schedInfos.Add(sched);
        }
      }
      catch (Exception ex)
      {
        lastException = ex;
        return null;
      }
      return schedInfos;
    }
    private string GetDateTimeString()
    {
      string provider = Gentle.Framework.ProviderFactory.GetDefaultProvider().Name.ToLower();
      if (provider == "mysql") return "yyyy-MM-dd HH:mm:ss";
      return "yyyyMMdd HH:mm:ss";
    }
    public List<EPGInfo> GetEPGForChannel(string idChannel)
    {
      IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
      List<EPGInfo> infos = new List<EPGInfo>();
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
      sb.AddConstraint(Operator.Equals, "idChannel", Int32.Parse(idChannel));
      DateTime thisMorning = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
      sb.AddConstraint(String.Format("startTime>='{0}'", thisMorning.ToString(GetDateTimeString(), mmddFormat)));
      sb.AddOrderByField(true, "startTime");
      SqlStatement stmt = sb.GetStatement(true);
      IList programs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
      if (programs != null && programs.Count > 0)
      {
        foreach (Program prog in programs)
        {
          EPGInfo epg = new EPGInfo();
          epg.startTime = prog.StartTime;
          epg.endTime = prog.EndTime;
          epg.title = prog.Title;
          epg.description = prog.Description;
          infos.Add(epg);
        }
      }
      return infos;
    }

    #endregion
  }
}
