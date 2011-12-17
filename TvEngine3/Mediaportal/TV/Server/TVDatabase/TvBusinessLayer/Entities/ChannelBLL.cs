using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

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

    private void UpdateNowAndNext()
    {
      if (_currentProgram != null)
      {
        if (DateTime.Now >= _currentProgram.startTime && DateTime.Now <= _currentProgram.endTime)
        {
          return;
        }
      }

      _currentProgram = null;
      _nextProgram = null;
      
      DateTime date = DateTime.Now;

      IList<Program> programs = GlobalServiceProvider.Instance.Get<IProgramService>().GetNowAndNextProgramsForChannel(_entity.idChannel).ToList();      
      if (programs.Count == 0)
      {
        return;
      }
      _currentProgram = programs[0];
      if (_currentProgram.startTime >= date)
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


    
  }
}
