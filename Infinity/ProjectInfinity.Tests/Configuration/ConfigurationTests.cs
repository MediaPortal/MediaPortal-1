







// This file contains an example of how Joe would like to use settings in PI











//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Text;

//namespace ProjectInfinity.Tests.Configuration
//{
//  class ConfigurationTests
//  {
//    ISettingsManager mgr = ServiceScope<ISettingsManager>.Get();
//    PluginSettings settings = mgr.Load<PluginSettings>("MyPlugin");

//    //is this plugin activated?
//    private bool isActivated = settings.Activated;
//    //to what folder does this user want to save?
//    private string folder = settings.Folder;
//  }

//  public class PluginSettings
//  {
//    private bool _activated;
//    private string _folder;

//    [Setting(SettingScope.Global)]
//    public bool Activated
//    {
//      get { return _activated; }
//      set { _activated = value; }
//    }

//    [Setting(SettingScope.User)]
//    public string Folder
//    {
//      get { return _folder; }
//      set { _folder = value; }
//    }
//  }
//}
