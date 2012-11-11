using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities
{
  public class ChannelBLL
  {
    private Program _currentProgram;
    private Program _nextProgram;    
    private Channel _entity;
    private ChannelGroup _currentGroup;

    public ChannelBLL (Channel entity)
    {      
      _entity = entity;      
    }

    /// <summary>
    /// Property describing the current group that was used to view the channel from
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      get { return _currentGroup; }
      set { _currentGroup = value; }
    }

    public Program CurrentProgram
    {
      get
      {
        UpdateNowAndNext();
        return _currentProgram;
      }
    }

    public Program NextProgram
    {
      get
      {
        UpdateNowAndNext();
        return _nextProgram;
      }
    }

    public Channel Entity
    {
      get { return _entity; }
      set { _entity = value; }
    }

    private bool _updateNowAndNextRun = false;
    private DateTime _updateNowAndNextLastRun = DateTime.MinValue;
    private void UpdateNowAndNext()
    {
      try
      {
        if (_currentProgram != null)
        {
          if (DateTime.Now >= _currentProgram.StartTime && DateTime.Now <= _currentProgram.EndTime)
          {
            return;
          }
        }
        else if (_updateNowAndNextRun) //non EPG channels will cause excessive server calls, lets limit the calls to once a minute.         
        {
          TimeSpan ts = DateTime.Now - _updateNowAndNextLastRun;
          if (ts.TotalSeconds < 60)
          {
            return;
          }          
        }

        _currentProgram = null;
        _nextProgram = null;

        DateTime date = DateTime.Now;

        IList<Program> programs = GlobalServiceProvider.Instance.Get<IProgramService>().GetNowAndNextProgramsForChannel(_entity.IdChannel).ToList();
        if (programs.Count == 0)
        {
          return;
        }
        _currentProgram = programs[0];
        if (_currentProgram.StartTime >= date)
        {
          _nextProgram = _currentProgram;
          _currentProgram = null;
        }
        else
        {
          if (programs.Count == 2)
          {
            _nextProgram = programs[1];
          }
        }
      }
      finally
      {
        _updateNowAndNextRun = true;
        _updateNowAndNextLastRun = DateTime.Now;
      }
    }
  }
}
