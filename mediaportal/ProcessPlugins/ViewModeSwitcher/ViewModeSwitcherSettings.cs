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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace ProcessPlugins.ViewModeSwitcher
{
  public class ViewModeswitcherSettings
  {
    public RuleSet ViewModeRules = new RuleSet();
    public bool verboseLog = false;
    public bool ShowSwitchMsg = false;
    public bool UseFallbackRule = true;
    public Geometry.Type FallBackViewMode = Geometry.Type.Normal;
    public bool DisableLBGlobaly = false;
    public decimal LBBlackLevel = 40;
    public int CropLeft = 0;
    public int CropRight = 0;
    public int CropTop = 0;
    public int CropBottom = 0;
    public int fboverScan = 0;
    public Geometry.Type PillarBoxViewMode = Geometry.Type.NonLinearStretch;

    // parameter names
    public static string ViewModeSwitcherSectionName = "ViewModeSwitcher";
    private const string ParmVerboselog = "parmverboselog";
    private const string ParmRuleCount = "parmrulecount";
    private const string ParmRulePrefix = "parmrule";
    private const string ParmShowSwitchMsg = "parmshowswitchmsg";
    private const string ParmUseFallbackRule = "parmusefallbackrule";
    private const string ParmFallbackViewMode = "parmfallbackviewmode";
    private const string ParmDisableLBGlobaly = "parmdisablelbglobaly";
    private const string ParmBlackLevel = "parmblacklevel";
    private const string FallBackOverScan = "parmfallbackoverscan";


    public static Geometry.Type StringToViewMode(string strViewmode)
    {
      foreach (Geometry.Type item in Enum.GetValues(typeof (Geometry.Type)))
      {
        if (strViewmode == item.ToString())
        {
          return item;
        }
      }
      return Geometry.Type.Normal;
    }

    public static List<String> LoadMediaPortalXml()
    {
      List<String> AvailableModes = new List<String>();
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (xmlreader.GetValueAsBool("mytv", "allowarnormal", true))
        {
          AvailableModes.Add(Geometry.Type.Normal.ToString());
        }
        if (xmlreader.GetValueAsBool("mytv", "allowaroriginal", true))
        {
          AvailableModes.Add(Geometry.Type.Original.ToString());
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarzoom", true))
        {
          AvailableModes.Add(Geometry.Type.Zoom.ToString());
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarzoom149", true))
        {
          AvailableModes.Add(Geometry.Type.Zoom14to9.ToString());
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarstretch", true))
        {
          AvailableModes.Add(Geometry.Type.Stretch.ToString());
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarnonlinear", true))
        {
          AvailableModes.Add(Geometry.Type.NonLinearStretch.ToString());
        }
        if (xmlreader.GetValueAsBool("mytv", "allowarletterbox", true))
        {
          AvailableModes.Add(Geometry.Type.LetterBox43.ToString());
        }
      }
      return AvailableModes;
    }

    public bool LoadSettings()
    {
      return LoadSettings(string.Empty);
    }

    /// <summary>
    /// load settings from configuration
    /// </summary>
    /// <returns></returns>
    public bool LoadSettings(string ImportFileName)
    {
      string tmpConfigFileName = Config.GetFile(Config.Dir.Config, "ViewModeSwitcher.xml");
      if (ImportFileName != string.Empty)
      {
        tmpConfigFileName = ImportFileName;
      }

      using (Settings reader = new Settings(tmpConfigFileName))
      {
        verboseLog = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmVerboselog, false);
        ShowSwitchMsg = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmShowSwitchMsg, false);
        UseFallbackRule = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmUseFallbackRule, true);
        String tmpFallbackViewMode = reader.GetValueAsString(ViewModeSwitcherSectionName, ParmFallbackViewMode, "Normal");
        FallBackViewMode = StringToViewMode(tmpFallbackViewMode);
        DisableLBGlobaly = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmDisableLBGlobaly, false);
        LBBlackLevel = reader.GetValueAsInt(ViewModeSwitcherSectionName, ParmBlackLevel, 40);
        fboverScan = reader.GetValueAsInt(ViewModeSwitcherSectionName, FallBackOverScan, 0);
        CropLeft = reader.GetValueAsInt("tv", "cropleft", 0);
        CropRight = reader.GetValueAsInt("tv", "cropright", 0);
        CropTop = reader.GetValueAsInt("tv", "croptop", 0);
        CropBottom = reader.GetValueAsInt("tv", "cropbottom", 0);        
        

        bool tmpReturn = false;
        ViewModeRules.Clear();
        int tmpRuleCount = reader.GetValueAsInt(ViewModeSwitcherSectionName, ParmRuleCount, 0);
        if (tmpRuleCount <= 0)
        {
          Rule tmpRule = new Rule();
          tmpRule.Enabled = true;
          tmpRule.Name = "4:3";
          tmpRule.ARFrom = 1.2f;
          tmpRule.ARTo = 1.46f;
          tmpRule.MinWidth = 200;
          tmpRule.MaxWidth = 2000;
          tmpRule.MinHeight = 200;
          tmpRule.MaxHeight = 2000;
          tmpRule.AutoCrop = false;
          tmpRule.ViewMode = Geometry.Type.Zoom14to9;
          tmpRule.MaxCrop = true;
          tmpRule.OverScan = 8;
          tmpRule.EnableLBDetection = true;
          ViewModeRules.Add(tmpRule);

          tmpRule = new Rule();
          tmpRule.Enabled = true;
          tmpRule.Name = "16:9";
          tmpRule.ARFrom = 1.70f;
          tmpRule.ARTo = 1.90f;
          tmpRule.MinWidth = 200;
          tmpRule.MaxWidth = 2000;
          tmpRule.MinHeight = 200;
          tmpRule.MaxHeight = 2000;
          tmpRule.AutoCrop = false;
          tmpRule.ViewMode = Geometry.Type.Normal;
          tmpRule.MaxCrop = true;
          tmpRule.OverScan = 8;
          tmpRule.EnableLBDetection = true;
          ViewModeRules.Add(tmpRule);

          tmpRule = new Rule();
          tmpRule.Enabled = true;
          tmpRule.Name = "4:3 inside 16:9";
          tmpRule.ARFrom = -1.20f;
          tmpRule.ARTo = -1.46f;
          tmpRule.MinWidth = 200;
          tmpRule.MaxWidth = 2000;
          tmpRule.MinHeight = 200;
          tmpRule.MaxHeight = 2000;
          tmpRule.AutoCrop = false;
          tmpRule.ViewMode = Geometry.Type.NonLinearStretch;
          tmpRule.MaxCrop = true;
          tmpRule.OverScan = 8;
          tmpRule.EnableLBDetection = false;
          ViewModeRules.Add(tmpRule);
          return true;
        }

        for (int i = 1; i <= tmpRuleCount; i++)
        {
          Rule tmpRule = new Rule();
          tmpRule.Enabled = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "Enabled", false);
          if (tmpRule.Enabled)
          {
            tmpReturn = true;
          }
          tmpRule.Name = reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "Name", "noname");
          tmpRule.ARFrom =
            (float)
            Convert.ToDouble(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "ARFrom", "0"));
          tmpRule.ARTo =
            (float)
            Convert.ToDouble(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "ARTo", "0"));
          tmpRule.MinWidth =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MinWidth", "0"));
          tmpRule.MaxWidth =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MaxWidth", "0"));
          tmpRule.MinHeight =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MinHeight", "0"));
          tmpRule.MaxHeight =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MaxHeight", "0"));
          tmpRule.AutoCrop = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "AutoCrop", true);
          String tmpViewMode = reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "ViewMode",
                                                       "Normal");
          tmpRule.ViewMode = StringToViewMode(tmpViewMode);
          tmpRule.MaxCrop = reader.GetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MaxCrop", true);
          tmpRule.OverScan =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName, ParmRulePrefix + i + "Overscan", "0"));
          tmpRule.EnableLBDetection = reader.GetValueAsBool(ViewModeSwitcherSectionName,
                                                            ParmRulePrefix + i + "EnableLBDetection", false);
          tmpRule.VerticalOffSetZoom =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName,
                                                    ParmRulePrefix + i + "VerticalOffSetZoom", "0"));
          tmpRule.VerticalOffSet14_9 =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName,
                                                    ParmRulePrefix + i + "VerticalOffSet14_9", "0"));
          tmpRule.VerticalOffSetWide14_9 =
            Convert.ToInt16(reader.GetValueAsString(ViewModeSwitcherSectionName,
                                                    ParmRulePrefix + i + "VerticalOffSetWide14_9", "0"));
          ViewModeRules.Add(tmpRule);

          if (verboseLog)
          {                   
            Log.Debug("ViewModeSwitcher: Rule " + i + ", Name:      " + tmpRule.Name             );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", Enabled:   " + tmpRule.Enabled          );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", ARFrom:    " + tmpRule.ARFrom           );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", ARTo:      " + tmpRule.ARTo             );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", MinWidth:  " + tmpRule.MinWidth         );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", MaxWidth:  " + tmpRule.MaxWidth         );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", MinHeight: " + tmpRule.MinHeight        );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", MaxHeight: " + tmpRule.MaxHeight        );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", AutoCrop:  " + tmpRule.AutoCrop         );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", ViewMode:  " + tmpRule.ViewMode         );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", MaxCrop:  " + tmpRule.MaxCrop         );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", OverScan:  " + tmpRule.OverScan         );
            Log.Debug("ViewModeSwitcher: Rule " + i + ", EnLBDet:   " + tmpRule.EnableLBDetection);
          }
        }
        return tmpReturn;
      }
    }

    public void SaveSettings()
    {
      SaveSettings(string.Empty);
    }

    public void SaveSettings(string ExportFileName)
    {
      string tmpConfigFileName = Config.GetFile(Config.Dir.Config, "ViewModeSwitcher.xml");
      if (ExportFileName != string.Empty)
      {
        tmpConfigFileName = ExportFileName;
      }

      using (Settings xmlwriter = new Settings(tmpConfigFileName))
      {
        xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmDisableLBGlobaly, DisableLBGlobaly);
        xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmVerboselog, verboseLog);
        xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmShowSwitchMsg, ShowSwitchMsg);
        xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmUseFallbackRule, UseFallbackRule);
        xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmFallbackViewMode, FallBackViewMode.ToString());
        xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRuleCount, ViewModeRules.Count.ToString());
        xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmBlackLevel, LBBlackLevel.ToString());
        xmlwriter.SetValue(ViewModeSwitcherSectionName, FallBackOverScan, fboverScan.ToString());

        for (int i = 1; i <= ViewModeRules.Count; i++)
        {
          Rule tmpRule = ViewModeRules[i - 1];
          xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "Enabled",
                                   tmpRule.Enabled);
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "Name", tmpRule.Name);
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "ARFrom",
                             tmpRule.ARFrom.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "ARTo",
                             tmpRule.ARTo.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MinWidth",
                             tmpRule.MinWidth.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MaxWidth",
                             tmpRule.MaxWidth.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MinHeight",
                             tmpRule.MinHeight.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MaxHeight",
                             tmpRule.MaxHeight.ToString());
          xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "AutoCrop",
                                   tmpRule.AutoCrop);
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "ViewMode",
                             tmpRule.ViewMode.ToString());
          xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "MaxCrop",
                                   tmpRule.MaxCrop);
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "Overscan",
                             tmpRule.OverScan.ToString());
          xmlwriter.SetValueAsBool(ViewModeSwitcherSectionName, ParmRulePrefix + i + "EnableLBDetection",
                                   tmpRule.EnableLBDetection);

          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "VerticalOffSetZoom",
                             tmpRule.VerticalOffSetZoom.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "VerticalOffSet14_9",
                             tmpRule.VerticalOffSet14_9.ToString());
          xmlwriter.SetValue(ViewModeSwitcherSectionName, ParmRulePrefix + i + "VerticalOffSetWide14_9",
                             tmpRule.VerticalOffSetWide14_9.ToString());
        }
        if (ExportFileName != string.Empty)
        {
          Settings.SaveCache();
        }
      }
    }
  }
}