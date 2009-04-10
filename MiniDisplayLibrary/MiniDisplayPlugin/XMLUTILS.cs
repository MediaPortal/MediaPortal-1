using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class XMLUTILS
  {
    public static MatrixGX.MOGX_Control LoadBackLightSettings()
    {
      MatrixGX.MOGX_Control control = new MatrixGX.MOGX_Control();
      if (Settings.Instance.Type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings = MatrixGX.AdvancedSettings.Load();
        control.BackLightRed = settings.BacklightRED;
        control.BackLightGreen = settings.BacklightGREEN;
        control.BackLightBlue = settings.BacklightBLUE;
        control.InvertDisplay = settings.UseInvertedDisplay;
      }
      return control;
    }

    public static void SaveBackLightSettings(MatrixGX.MOGX_Control BLSettings)
    {
      if (Settings.Instance.Type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings toSave = MatrixGX.AdvancedSettings.Load();
        toSave.BacklightRED = BLSettings.BackLightRed;
        toSave.BacklightGREEN = BLSettings.BackLightGreen;
        toSave.BacklightBLUE = BLSettings.BackLightBlue;
        toSave.UseInvertedDisplay = BLSettings.InvertDisplay;
        MatrixGX.AdvancedSettings.Instance = toSave;
        MatrixGX.AdvancedSettings.Save(toSave);
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
    }

    public static DisplayControl LoadDisplayControlSettings()
    {
      DisplayControl control = new DisplayControl();
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings.EnableDisplayAction;
        control.DisplayActionTime = settings.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings.BlankIdleTime;
        return control;
      }
      if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings2 = MatrixGX.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings2.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings2.EnableDisplayAction;
        control.DisplayActionTime = settings2.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings2.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings2.BlankIdleTime;
        return control;
      }
      if (type.Equals("MD8800"))
      {
        MD8800.AdvancedSettings settings3 = MD8800.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings3.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings3.EnableDisplayAction;
        control.DisplayActionTime = settings3.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings3.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings3.BlankIdleTime;
        return control;
      }
      if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings4 = CFontz.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings4.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings4.EnableDisplayAction;
        control.DisplayActionTime = settings4.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings4.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings4.BlankIdleTime;
        return control;
      }
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings5 = MatrixMX.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings5.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings5.EnableDisplayAction;
        control.DisplayActionTime = settings5.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings5.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings5.BlankIdleTime;
        return control;
      }
      if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings6 = VLSYS_Mplay.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings6.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings6.EnableDisplayAction;
        control.DisplayActionTime = settings6.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings6.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings6.BlankIdleTime;
      }
      if (type.Equals("DebugForm"))
      {
        DebugForm.AdvancedSettings settings7 = DebugForm.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings7.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings7.EnableDisplayAction;
        control.DisplayActionTime = settings7.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings7.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings7.BlankIdleTime;
      }
      return control;
    }

    public static void SaveDisplayControlSettings(DisplayControl DisplayControl)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        toSave.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        toSave.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        toSave.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        toSave.BlankIdleTime = DisplayControl.BlankIdleDelay;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        settings2.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings2.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings2.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings2.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings2.BlankIdleTime = DisplayControl.BlankIdleDelay;
        CFontz.AdvancedSettings.Instance = settings2;
        CFontz.AdvancedSettings.Save(settings2);
        CFontz.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MD8800"))
      {
        MD8800.AdvancedSettings settings3 = MD8800.AdvancedSettings.Load();
        settings3.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings3.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings3.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings3.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings3.BlankIdleTime = DisplayControl.BlankIdleDelay;
        MD8800.AdvancedSettings.Instance = settings3;
        MD8800.AdvancedSettings.Save(settings3);
        MD8800.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings4 = MatrixMX.AdvancedSettings.Load();
        settings4.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings4.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings4.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings4.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings4.BlankIdleTime = DisplayControl.BlankIdleDelay;
        MatrixMX.AdvancedSettings.Instance = settings4;
        MatrixMX.AdvancedSettings.Save(settings4);
        MatrixMX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings5 = MatrixGX.AdvancedSettings.Load();
        settings5.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings5.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings5.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings5.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings5.BlankIdleTime = DisplayControl.BlankIdleDelay;
        MatrixGX.AdvancedSettings.Instance = settings5;
        MatrixGX.AdvancedSettings.Save(settings5);
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings6 = VLSYS_Mplay.AdvancedSettings.Load();
        settings6.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings6.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings6.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings6.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings6.BlankIdleTime = DisplayControl.BlankIdleDelay;
        VLSYS_Mplay.AdvancedSettings.Instance = settings6;
        VLSYS_Mplay.AdvancedSettings.Save(settings6);
        VLSYS_Mplay.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("DebugForm"))
      {
        DebugForm.AdvancedSettings settings7 = DebugForm.AdvancedSettings.Load();
        settings7.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings7.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings7.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings7.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings7.BlankIdleTime = DisplayControl.BlankIdleDelay;
        DebugForm.AdvancedSettings.Instance = settings7;
        DebugForm.AdvancedSettings.Save(settings7);
        DebugForm.AdvancedSettings.NotifyDriver();
      }
    }

    public static DisplayOptions LoadDisplayOptionsSettings()
    {
      DisplayOptions options = new DisplayOptions();
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        options.VolumeDisplay = settings.VolumeDisplay;
        options.ProgressDisplay = settings.ProgressDisplay;
        options.DiskIcon = settings.DiskIcon;
        options.DiskMediaStatus = settings.DiskMediaStatus;
        options.DiskMonitor = settings.DeviceMonitor;
        options.UseCustomFont = settings.UseCustomFont;
        options.UseLargeIcons = settings.UseLargeIcons;
        options.UseCustomIcons = settings.UseCustomIcons;
        options.UseInvertedIcons = settings.UseInvertedIcons;
        return options;
      }
      if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings2 = MatrixGX.AdvancedSettings.Load();
        options.VolumeDisplay = settings2.VolumeDisplay;
        options.ProgressDisplay = settings2.ProgressDisplay;
        options.DiskIcon = settings2.UseIcons;
      }
      if (type.Equals("DebugForm"))
      {
        iMONLCDg.AdvancedSettings settings3 = iMONLCDg.AdvancedSettings.Load();
        options.VolumeDisplay = settings3.VolumeDisplay;
        options.ProgressDisplay = settings3.ProgressDisplay;
        options.DiskIcon = settings3.DiskIcon;
        options.DiskMediaStatus = settings3.DiskMediaStatus;
        options.DiskMonitor = settings3.DeviceMonitor;
        options.UseCustomFont = settings3.UseCustomFont;
        options.UseLargeIcons = settings3.UseLargeIcons;
        options.UseCustomIcons = settings3.UseCustomIcons;
        options.UseInvertedIcons = settings3.UseInvertedIcons;
        return options;
      }

      return options;
    }

    public static void SaveDisplayOptionsSettings(DisplayOptions DisplayOptions)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.VolumeDisplay = DisplayOptions.VolumeDisplay;
        toSave.ProgressDisplay = DisplayOptions.ProgressDisplay;
        toSave.DiskIcon = DisplayOptions.DiskIcon;
        toSave.DiskMediaStatus = DisplayOptions.DiskMediaStatus;
        toSave.DeviceMonitor = DisplayOptions.DiskMonitor;
        toSave.UseCustomFont = DisplayOptions.UseCustomFont;
        toSave.UseLargeIcons = DisplayOptions.UseLargeIcons;
        toSave.UseCustomIcons = DisplayOptions.UseCustomIcons;
        toSave.UseInvertedIcons = DisplayOptions.UseInvertedIcons;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings2 = MatrixGX.AdvancedSettings.Load();
        settings2.VolumeDisplay = DisplayOptions.VolumeDisplay;
        settings2.ProgressDisplay = DisplayOptions.ProgressDisplay;
        settings2.UseIcons = DisplayOptions.DiskIcon;
        settings2.UseDiskIconForAllMedia = DisplayOptions.DiskMediaStatus;
        MatrixGX.AdvancedSettings.Instance = settings2;
        MatrixGX.AdvancedSettings.Save(settings2);
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("DebugForm"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.VolumeDisplay = DisplayOptions.VolumeDisplay;
        toSave.ProgressDisplay = DisplayOptions.ProgressDisplay;
        toSave.DiskIcon = DisplayOptions.DiskIcon;
        toSave.DiskMediaStatus = DisplayOptions.DiskMediaStatus;
        toSave.DeviceMonitor = DisplayOptions.DiskMonitor;
        toSave.UseCustomFont = DisplayOptions.UseCustomFont;
        toSave.UseLargeIcons = DisplayOptions.UseLargeIcons;
        toSave.UseCustomIcons = DisplayOptions.UseCustomIcons;
        toSave.UseInvertedIcons = DisplayOptions.UseInvertedIcons;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
      }
      else if (!type.Equals("MatrixMX"))
      {
        type.Equals("VLSYS_Mplay");
      }
    }

    public static DisplayOptionsLayout GetDisplayOptionsLayout(DisplayOptions DisplayOptions)
    {
      DisplayOptionsLayout layout = new DisplayOptionsLayout();
      layout.Volume = false;
      layout.Progress = false;
      layout.DiskIcon = false;
      layout.MediaStatus = false;
      layout.DiskStatus = false;
      layout.CustomFont = false;
      layout.LargeIcons = false;
      layout.CustomIcons = false;
      layout.InvertIcons = false;
      layout.FontEditor = false;
      layout.IconEditor = false;

      string type = Settings.Instance.Type;

      if (type != null)
      {
        if (!(type == "MatrixGX"))
        {
          if (type == "iMONLCDg" | type == "DebugForm")
          {
            layout.Volume = true;
            layout.Progress = true;
            layout.DiskIcon = true;
            if (DisplayOptions.DiskIcon)
            {
              layout.MediaStatus = true;
              layout.DiskStatus = true;
            }
            layout.CustomFont = true;
            layout.LargeIcons = true;
            if (DisplayOptions.UseLargeIcons)
            {
              layout.CustomIcons = true;
              layout.InvertIcons = true;
            }
            if (DisplayOptions.UseCustomFont)
            {
              layout.FontEditor = true;
            }
            if (DisplayOptions.UseLargeIcons)
            {
              layout.IconEditor = true;
            }
          }
          else if ((type == "VLSYS_Mplay") || (type == "MatrixMX"))
          {
          }
        }
        else
        {
          layout.Volume = true;
          layout.Progress = true;
          layout.DiskIcon = true;
          if (DisplayOptions.DiskIcon)
          {
            layout.MediaStatus = true;
          }
        }
      }
      return layout;
    }

    public static EQControl LoadEqualizerSettings()
    {
      EQControl control = new EQControl();
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        control.UseEqDisplay = settings.EqDisplay;
        control.UseNormalEq = settings.NormalEQ;
        control.UseStereoEq = settings.StereoEQ;
        control.UseVUmeter = settings.VUmeter;
        control.UseVUmeter2 = settings.VUmeter2;
        control.SmoothEQ = settings.SmoothEQ;
        control.DelayEQ = settings.DelayEQ;
        control._DelayEQTime = settings.DelayEqTime;
        control.EQTitleDisplay = settings.EQTitleDisplay;
        control._EQTitleShowTime = settings.EQTitleShowTime;
        control._EQTitleDisplayTime = settings.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        control.UseEqDisplay = settings2.EqDisplay;
        control.UseNormalEq = settings2.NormalEQ;
        control.UseStereoEq = settings2.StereoEQ;
        control.UseVUmeter = settings2.VUmeter;
        control.UseVUmeter2 = settings2.VUmeter2;
        control.SmoothEQ = settings2.SmoothEQ;
        control.DelayEQ = settings2.DelayEQ;
        control._DelayEQTime = settings2.DelayEqTime;
        control.EQTitleDisplay = settings2.EQTitleDisplay;
        control._EQTitleShowTime = settings2.EQTitleShowTime;
        control._EQTitleDisplayTime = settings2.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings3 = MatrixMX.AdvancedSettings.Load();
        control.UseEqDisplay = settings3.EqDisplay;
        control.UseNormalEq = settings3.NormalEQ;
        control.UseStereoEq = settings3.StereoEQ;
        control.UseVUmeter = settings3.VUmeter;
        control.UseVUmeter2 = settings3.VUmeter2;
        control.SmoothEQ = settings3.SmoothEQ;
        control.DelayEQ = settings3.DelayEQ;
        control._DelayEQTime = settings3.DelayEqTime;
        control.EQTitleDisplay = settings3.EQTitleDisplay;
        control._EQTitleShowTime = settings3.EQTitleShowTime;
        control._EQTitleDisplayTime = settings3.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings4 = MatrixGX.AdvancedSettings.Load();
        control.UseEqDisplay = settings4.EqDisplay;
        control.UseNormalEq = settings4.NormalEQ;
        control.UseStereoEq = settings4.StereoEQ;
        control.UseVUmeter = settings4.VUmeter;
        control.UseVUmeter2 = settings4.VUmeter2;
        control.SmoothEQ = settings4.SmoothEQ;
        control.DelayEQ = settings4.DelayEQ;
        control._DelayEQTime = settings4.DelayEqTime;
        control.EQTitleDisplay = settings4.EQTitleDisplay;
        control._EQTitleShowTime = settings4.EQTitleShowTime;
        control._EQTitleDisplayTime = settings4.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings5 = VLSYS_Mplay.AdvancedSettings.Load();
        control.UseEqDisplay = settings5.EqDisplay;
        control.UseNormalEq = settings5.NormalEQ;
        control.UseStereoEq = settings5.StereoEQ;
        control.UseVUmeter = settings5.VUmeter;
        control.UseVUmeter2 = settings5.VUmeter2;
        control.SmoothEQ = settings5.SmoothEQ;
        control.DelayEQ = settings5.DelayEQ;
        control._DelayEQTime = settings5.DelayEqTime;
        control.EQTitleDisplay = settings5.EQTitleDisplay;
        control._EQTitleShowTime = settings5.EQTitleShowTime;
        control._EQTitleDisplayTime = settings5.EQTitleDisplayTime;
      }
      if (type.Equals("DebugForm"))
      {
        DebugForm.AdvancedSettings settings6 = DebugForm.AdvancedSettings.Load();
        control.UseEqDisplay = settings6.EqDisplay;
        control.UseNormalEq = settings6.NormalEQ;
        control.UseStereoEq = settings6.StereoEQ;
        control.UseVUmeter = settings6.VUmeter;
        control.UseVUmeter2 = settings6.VUmeter2;
        control.SmoothEQ = settings6.SmoothEQ;
        control.DelayEQ = settings6.DelayEQ;
        control._DelayEQTime = settings6.DelayEqTime;
        control.EQTitleDisplay = settings6.EQTitleDisplay;
        control._EQTitleShowTime = settings6.EQTitleShowTime;
        control._EQTitleDisplayTime = settings6.EQTitleDisplayTime;
      }
      return control;
    }

    public static void SaveEqualizerSettings(EQControl EQSettings)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.EqDisplay = EQSettings.UseEqDisplay;
        toSave.NormalEQ = EQSettings.UseNormalEq;
        toSave.StereoEQ = EQSettings.UseStereoEq;
        toSave.VUmeter = EQSettings.UseVUmeter;
        toSave.VUmeter2 = EQSettings.UseVUmeter2;
        toSave.SmoothEQ = EQSettings.SmoothEQ;
        toSave.DelayEQ = EQSettings.DelayEQ;
        toSave.DelayEqTime = EQSettings._DelayEQTime;
        toSave.EQTitleDisplay = EQSettings.EQTitleDisplay;
        toSave.EQTitleShowTime = EQSettings._EQTitleShowTime;
        toSave.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        toSave.RestrictEQ = true;
        toSave.EqRate = 30;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        settings2.EqDisplay = EQSettings.UseEqDisplay;
        settings2.NormalEQ = EQSettings.UseNormalEq;
        settings2.StereoEQ = EQSettings.UseStereoEq;
        settings2.VUmeter = EQSettings.UseVUmeter;
        settings2.VUmeter2 = EQSettings.UseVUmeter2;
        settings2.SmoothEQ = EQSettings.SmoothEQ;
        settings2.DelayEQ = EQSettings.DelayEQ;
        settings2.DelayEqTime = EQSettings._DelayEQTime;
        settings2.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings2.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings2.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        CFontz.AdvancedSettings.Instance = settings2;
        CFontz.AdvancedSettings.Save(settings2);
        CFontz.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings3 = MatrixMX.AdvancedSettings.Load();
        settings3.EqDisplay = EQSettings.UseEqDisplay;
        settings3.NormalEQ = EQSettings.UseNormalEq;
        settings3.StereoEQ = EQSettings.UseStereoEq;
        settings3.VUmeter = EQSettings.UseVUmeter;
        settings3.VUmeter2 = EQSettings.UseVUmeter2;
        settings3.SmoothEQ = EQSettings.SmoothEQ;
        settings3.DelayEQ = EQSettings.DelayEQ;
        settings3.DelayEqTime = EQSettings._DelayEQTime;
        settings3.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings3.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings3.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        MatrixMX.AdvancedSettings.Instance = settings3;
        MatrixMX.AdvancedSettings.Save(settings3);
        MatrixMX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings4 = MatrixGX.AdvancedSettings.Load();
        settings4.EqDisplay = EQSettings.UseEqDisplay;
        settings4.NormalEQ = EQSettings.UseNormalEq;
        settings4.StereoEQ = EQSettings.UseStereoEq;
        settings4.VUmeter = EQSettings.UseVUmeter;
        settings4.VUmeter2 = EQSettings.UseVUmeter2;
        settings4.SmoothEQ = EQSettings.SmoothEQ;
        settings4.DelayEQ = EQSettings.DelayEQ;
        settings4.DelayEqTime = EQSettings._DelayEQTime;
        settings4.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings4.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings4.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        MatrixGX.AdvancedSettings.Instance = settings4;
        MatrixGX.AdvancedSettings.Save();
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings5 = VLSYS_Mplay.AdvancedSettings.Load();
        settings5.EqDisplay = EQSettings.UseEqDisplay;
        settings5.NormalEQ = EQSettings.UseNormalEq;
        settings5.StereoEQ = EQSettings.UseStereoEq;
        settings5.VUmeter = EQSettings.UseVUmeter;
        settings5.VUmeter2 = EQSettings.UseVUmeter2;
        settings5.SmoothEQ = EQSettings.SmoothEQ;
        settings5.DelayEQ = EQSettings.DelayEQ;
        settings5.DelayEqTime = EQSettings._DelayEQTime;
        settings5.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings5.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings5.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        VLSYS_Mplay.AdvancedSettings.Instance = settings5;
        VLSYS_Mplay.AdvancedSettings.Save(settings5);
        VLSYS_Mplay.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("DebugForm"))
      {
        DebugForm.AdvancedSettings settings6 = DebugForm.AdvancedSettings.Load();
        settings6.EqDisplay = EQSettings.UseEqDisplay;
        settings6.NormalEQ = EQSettings.UseNormalEq;
        settings6.StereoEQ = EQSettings.UseStereoEq;
        settings6.VUmeter = EQSettings.UseVUmeter;
        settings6.VUmeter2 = EQSettings.UseVUmeter2;
        settings6.SmoothEQ = EQSettings.SmoothEQ;
        settings6.DelayEQ = EQSettings.DelayEQ;
        settings6.DelayEqTime = EQSettings._DelayEQTime;
        settings6.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings6.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings6.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        DebugForm.AdvancedSettings.Instance = settings6;
        DebugForm.AdvancedSettings.Save(settings6);
        DebugForm.AdvancedSettings.NotifyDriver();
      }
    }

    public static MatrixMX.KeyPadControl LoadKeyPadSettings()
    {
      MatrixMX.KeyPadControl control = new MatrixMX.KeyPadControl();
      string type = Settings.Instance.Type;
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings = MatrixMX.AdvancedSettings.Load();
        control.EnableKeyPad = settings.EnableKeypad;
        control.EnableCustom = settings.UseCustomKeypadMap;
        return control;
      }
      if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        control.EnableKeyPad = settings2.EnableKeypad;
        control.EnableCustom = settings2.UseCustomKeypadMap;
      }
      return control;
    }

    public static void SaveKeyPadSettings(MatrixMX.KeyPadControl KeyPadOptions)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings toSave = MatrixMX.AdvancedSettings.Load();
        toSave.EnableKeypad = KeyPadOptions.EnableKeyPad;
        toSave.UseCustomKeypadMap = KeyPadOptions.EnableCustom;
        MatrixMX.AdvancedSettings.Instance = toSave;
        MatrixMX.AdvancedSettings.Save(toSave);
        MatrixMX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        settings2.EnableKeypad = KeyPadOptions.EnableKeyPad;
        settings2.UseCustomKeypadMap = KeyPadOptions.EnableCustom;
        CFontz.AdvancedSettings.Instance = settings2;
        CFontz.AdvancedSettings.Save(settings2);
        CFontz.AdvancedSettings.NotifyDriver();
      }
    }

    public static KeyPadLayout GetKeyPadLayout(MatrixMX.KeyPadControl KPSettings)
    {
      KeyPadLayout layout = new KeyPadLayout();
      layout.EnableKeyPad = false;
      layout.EnableCustom = false;
      layout.KeyPadMapping = false;
      layout.Label1 = false;
      string type = Settings.Instance.Type;
      if (type != null)
      {
        if (!(type == "CFontz") && !(type == "MatrixMX"))
        {
          if (((type == "VLSYS_Mplay") || (type == "MatrixGX")) || (type == "iMONLCDg"))
          {
          }
        }
        else
        {
          layout.EnableKeyPad = true;
          if (KPSettings.EnableKeyPad)
          {
            layout.EnableCustom = true;
            if (KPSettings.EnableCustom)
            {
              layout.KeyPadMapping = true;
            }
          }
          return layout;
        }
      }
      layout.Label1 = true;
      return layout;
    }

    public static VLSYS_Mplay.RemoteControl LoadRemoteSettings()
    {
      VLSYS_Mplay.RemoteControl control = new VLSYS_Mplay.RemoteControl();
      if (Settings.Instance.Type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings = VLSYS_Mplay.AdvancedSettings.Load();
        control.DisableRemote = settings.DisableRemote;
        control.DisableRepeat = settings.DisableRepeat;
        control.RepeatDelay = settings.RepeatDelay;
      }
      return control;
    }

    public static void SaveRemoteSettings(VLSYS_Mplay.RemoteControl RemoteOptions)
    {
      if (Settings.Instance.Type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings toSave = VLSYS_Mplay.AdvancedSettings.Load();
        toSave.DisableRemote = RemoteOptions.DisableRemote;
        toSave.DisableRepeat = RemoteOptions.DisableRepeat;
        toSave.RepeatDelay = RemoteOptions.RepeatDelay;
        VLSYS_Mplay.AdvancedSettings.Instance = toSave;
        VLSYS_Mplay.AdvancedSettings.Save(toSave);
        VLSYS_Mplay.AdvancedSettings.NotifyDriver();
      }
    }

    public static RemoteLayout GetRemoteLayout(VLSYS_Mplay.RemoteControl RCSettings)
    {
      RemoteLayout layout = new RemoteLayout();
      layout.DisableRemote = false;
      layout.DisableRepeat = false;
      layout.RepeatDelay = false;
      layout.RemoteMapping = false;
      layout.Label1 = false;
      string type = Settings.Instance.Type;
      if (type != null)
      {
        if (!(type == "VLSYS_Mplay"))
        {
          if (((type == "MatrixMX") || (type == "MatrixGX")) || (type == "iMONLCDg"))
          {
          }
        }
        else
        {
          layout.DisableRemote = true;
          if (!RCSettings.DisableRemote)
          {
            layout.DisableRepeat = true;
            if (!RCSettings.DisableRepeat)
            {
              layout.RepeatDelay = true;
            }
            layout.RemoteMapping = true;
          }
          return layout;
        }
      }
      layout.Label1 = true;
      return layout;
    }

    public static MainMenuLayout GetMainMenuLayout()
    {
      MainMenuLayout layout = new MainMenuLayout();
      switch (Settings.Instance.Type)
      {
        case "iMONLCDg":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = false;
            layout.DisplayControl = true;
            layout.DisplayOptions = true;
            layout.Equalizer = true;
            layout.KeyPad = false;
            layout.Remote = false;
            layout.Contrast = true;
            layout.MonitorPower = true;
            layout.Brightness = false;
            break;
          }
        case "VLSYS_Mplay":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = false;
            layout.DisplayControl = true;
            layout.DisplayOptions = false;
            layout.Equalizer = true;
            layout.KeyPad = false;
            layout.Remote = true;
            layout.Contrast = true;
            layout.MonitorPower = false;
            layout.Brightness = false;
            break;
          }
        case "MD8800":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = false;
            layout.DisplayControl = true;
            layout.DisplayOptions = false;
            layout.Equalizer = false;
            layout.KeyPad = false;
            layout.Remote = false;
            layout.Contrast = true;
            layout.MonitorPower = false;
            layout.Brightness = false;
            break;
          }
        case "CFontz":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = false;
            layout.DisplayControl = true;
            layout.DisplayOptions = false;
            layout.Equalizer = true;
            layout.KeyPad = true;
            layout.Remote = false;
            layout.Contrast = true;
            layout.MonitorPower = false;
            layout.Brightness = false;
            break;
          }
        case "MatrixMX":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = false;
            layout.DisplayControl = true;
            layout.DisplayOptions = false;
            layout.Equalizer = true;
            layout.KeyPad = true;
            layout.Remote = false;
            layout.Contrast = true;
            layout.MonitorPower = false;
            layout.Brightness = false;
            break;
          }
        case "MatrixGX":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = true;
            layout.DisplayControl = true;
            layout.DisplayOptions = true;
            layout.Equalizer = true;
            layout.KeyPad = false;
            layout.Remote = false;
            layout.Contrast = true;
            layout.MonitorPower = false;
            layout.Brightness = false;
            break;
          }
        case "DebugForm":
          {
            layout.LabelInfo1 = false;
            layout.LabelInfo2 = false;
            layout.Backlight = true;
            layout.DisplayControl = true;
            layout.DisplayOptions = true;
            layout.Equalizer = true;
            layout.KeyPad = false;
            layout.Remote = false;
            layout.Contrast = false;
            layout.MonitorPower = false;
            layout.Brightness = false;
            break;
          }
        default:
          layout.LabelInfo1 = true;
          layout.LabelInfo2 = true;
          layout.Backlight = false;
          layout.DisplayControl = false;
          layout.DisplayOptions = false;
          layout.Equalizer = false;
          layout.KeyPad = false;
          layout.Remote = false;
          layout.Contrast = false;
          layout.MonitorPower = false;
          layout.Brightness = false;
          break;
      }
      return layout;
    }

    public static void SaveMonitorPowerState(bool value)
    {
      iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
      toSave.MonitorPowerState = value;
      iMONLCDg.AdvancedSettings.Save(toSave);
      iMONLCDg.AdvancedSettings.NotifyDriver();
    }

    public static void SaveContrast(int value)
    {
      Settings.Instance.Contrast = value;
      Settings.Save();
      string type = Settings.Instance.Type;
      if (type != null)
      {
        if (!(type == "iMONLCDg"))
        {
          if (type == "VLSYS_Mplay")
          {
            VLSYS_Mplay.AdvancedSettings.NotifyDriver();
          }
          else if (type == "MatrixMX")
          {
            MatrixMX.AdvancedSettings.NotifyDriver();
          }
          else if (type == "MatrixGX")
          {
            MatrixGX.AdvancedSettings.NotifyDriver();
          }
          else if (type == "CFontz")
          {
            CFontz.AdvancedSettings.NotifyDriver();
          }
          else if (type == "MD8800")
          {
            MD8800.AdvancedSettings.NotifyDriver();
          }
        }
        else
        {
          iMONLCDg.AdvancedSettings.NotifyDriver();
        }
      }
    }

    public static void SaveBrightness(int value)
    {
      Settings.Instance.Backlight = value;
      Settings.Save();
      string str2 = Settings.Instance.Type;
      if (str2 != null)
      {
        if (!(str2 == "iMONLCDg"))
        {
          if (str2 == "VLSYS_Mplay")
          {
            VLSYS_Mplay.AdvancedSettings.NotifyDriver();
          }
          else if (str2 == "MatrixMX")
          {
            MatrixMX.AdvancedSettings.NotifyDriver();
          }
          else if (str2 == "MatrixGX")
          {
            MatrixGX.AdvancedSettings.NotifyDriver();
          }
          else if (str2 == "CFontz")
          {
            CFontz.AdvancedSettings.NotifyDriver();
          }
          else if (str2 == "MD8800")
          {
            MD8800.AdvancedSettings.NotifyDriver();
          }
        }
        else
        {
          iMONLCDg.AdvancedSettings.NotifyDriver();
        }
      }
    }

    public static bool LoadMonitorPowerSate()
    {
      if (Settings.Instance.Type == "iMONLCDg")
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        return settings.MonitorPowerState;
      }
      return false;
    }
  }
}