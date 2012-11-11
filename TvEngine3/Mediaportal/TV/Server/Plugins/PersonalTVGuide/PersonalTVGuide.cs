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
using System.Threading;
using Castle.Core;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces;

namespace Mediaportal.TV.Server.Plugins.PersonalTVGuide
{
  [Interceptor("PluginExceptionInterceptor")]
  public class PersonalTVGuide : ITvServerPlugin, IStandbyHandler
  {
    #region variables

    //private TvBusinessLayer cmLayer = new TvBusinessLayer();
    private bool _stopService = false;
    private bool _isUpdating = false;
    private bool _debugMode = true;

    #endregion

    #region public members

    /// <summary>
    /// Handles the OnTvServerEvent event fired by the server.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The <see cref="System.EventArgs"/> the event data.</param>
    private void events_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;
      if (tvEvent.EventType == TvServerEventType.ProgramUpdated)
      {
        UpdatePersonalTVGuide();
      }
    }


    /// <summary>
    /// Parses the sheduled recordings
    /// and updates the PersonalTVGuide
    /// </summary>
    public void UpdatePersonalTVGuide()
    {
      if (_debugMode) this.LogInfo("PersonalTVGuide: Updating list");
      if (!_isUpdating)
      {
        try
        {
          Thread updateThread = new Thread(new ThreadStart(UpdateThread));
          updateThread.Name = "PersonalTVGuide";
          updateThread.IsBackground = true;
          updateThread.Priority = ThreadPriority.Lowest;
          updateThread.Start();
        }
        catch (Exception ex)
        {
          this.LogError("PersonalTVGuide: Error spawing update thread - {0},{1}", ex.Message, ex.StackTrace);
        }
      }
      else
      {
        this.LogError("PersonalTVGuide: Update already started");
      }
    }

    #endregion

    #region private members

    /// <summary>
    /// Parses the keyword table
    /// and updates the PersonalTVGuideMap
    /// </summary>
    private void UpdateThread()
    {
      _isUpdating = true;

      if (_debugMode) this.LogInfo("PersonalTVGuide: UpdateThread - Start: " + DateTime.Now.ToLongTimeString());
      ClearPersonalTVGuideMap();
      IList<Keyword> list = Keyword.ListAll();
      foreach (Keyword key in list)
      {
        if (_stopService)
        {
          this.LogInfo("PersonalTVGuide: Stop Update loop");
          break;
        }
        UpdateKeyword(key);      
      }

      SettingsManagement.SaveSetting("PTVGLastUpdateTime", DateTime.Now.ToString());      
      if (_debugMode) this.LogInfo("PersonalTVGuide: UpdateThread - Stop : " + DateTime.Now.ToLongTimeString());
      _isUpdating = false;
    }

    /// <summary>
    /// Removes all records in Table : PersonalTVGuideMap
    /// </summary>
    private static void ClearPersonalTVGuideMap()
    {
      // clears all PersonalTVGuideMap in db
      IList<PersonalTVGuideMap> list = PersonalTVGuideMap.ListAll();
      foreach (PersonalTVGuideMap map in list) map.Remove();
    }

    private void UpdateKeyword(Keyword key)
    {      
      if (_debugMode) this.LogDebug("PersonalTVGuide: Updating Keyword: " + key.KeywordName);
      if (key.SearchInTitle)
      {
        SaveList(key.IdKeyword, ContainsInTitle(key.KeywordName));
        Thread.Sleep(100);
      }
      
      if (key.SearchInDescription)
      {
        SaveList(key.IdKeyword, ContainsInDescription(key.KeywordName));
        Thread.Sleep(100);
      }

      if (key.SearchInGenre)
      {
        SaveList(key.IdKeyword, ContainsInDescription(key.KeywordName));
        Thread.Sleep(100);
      }
    }

    private void SaveList(int IdKeyword, IList<Program> list)
    {
      if ((list == null) || (list.Count < 1)) return;
      foreach (Program prog in list)
      {
        PersonalTVGuideMap map = new PersonalTVGuideMap(IdKeyword, prog.IdProgram);
        map.Persist();
      }
    }

    /// <summary>
    /// Get a list which contains token in the title string
    /// </summary>
    private IList<Program> ContainsInTitle(string Token)
    {
      var stringComparisonEnum = StringComparisonEnum.StartsWith;
      stringComparisonEnum |= StringComparisonEnum.EndsWith;
      return ProgramManagement.GetProgramsByTitle(Token, MediaTypeEnum.TV, stringComparisonEnum);
    }

    /// <summary>
    /// Get a list which contains token in the description
    /// </summary>
    private IList<Program> ContainsInDescription(string token)
    {
      var stringComparisonEnum = StringComparisonEnum.StartsWith;
      stringComparisonEnum |= StringComparisonEnum.EndsWith;
      return ProgramManagement.GetProgramsByDescription(token, MediaTypeEnum.TV, stringComparisonEnum);      
    }
    

    private void LoadSettings()
    {
      //_debugMode = (cmLayer.GetSetting("PTVGDebugMode", "true").Value == "true");
      if (_debugMode) this.LogDebug("PersonalTVGuide: Extensive Logging switched on");
    }

    #endregion

    #region ITvServerPlugin Members

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get { return "PersonalTVGuide"; }
    }

    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get { return "0.0.0.1"; }
    }

    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get { return "Bavarian"; }
    }

    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get { return false; }
    }

    /// <summary>
    /// Plugin setup form
    /// </summary>
    public SectionSettings Setup
    {
      get { return new PTVGSetup(); }
    }

    /// <summary>
    /// Starts the plugin
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      LoadSettings();
      this.LogDebug("plugin: PersonalTVGuide started");
      _stopService = false;
      ITvServerEvent events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
      events.OnTvServerEvent += new TvServerEventHandler(events_OnTvServerEvent);
      if (_debugMode) UpdatePersonalTVGuide(); // Only for testing !!!!
    }

    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
      _stopService = true;
      this.LogDebug("plugin: PersonalTVGuide stopped");

      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= events_OnTvServerEvent;
      }
    }

    #endregion

    #region IStandbyHandler

    public void UserShutdownNow() {}

    public bool DisAllowShutdown
    {
      get { return _isUpdating; }
    }

    public string HandlerName
    {
      get { return Name; }
    }

    #endregion
  }
}