using System;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace WindowPlugins.VideoEditor
{
  public class CompressionSettings
  {
    public int resolutionX = 720;
    public int resolutionY = 576;
    public int videoQuality = 2200;
    public int audioQuality = 128;
  }

  public class CompressSettings : GUIDialogWindow
  {
    private bool result;

    [SkinControl(302)] protected GUISpinControl profilSelect = null;
    [SkinControl(402)] protected GUISpinControl videoQualitySelect = null;
    [SkinControl(403)] protected GUISpinControl audioQualitySelect = null;
    [SkinControl(100)] protected GUISliderControl resolution = null;
    [SkinControl(102)] protected GUILabelControl resolutionLbl = null;
    [SkinControl(104)] protected GUIButtonControl okBtn = null;
    [SkinControl(24)] protected GUIButtonControl abbortBtn = null;

    private CompressionSettings settings;

    public CompressSettings()
    {
      settings = new CompressionSettings();
      settings.resolutionX = 720;
      settings.resolutionY = 576;
      settings.videoQuality = 2200;
      settings.audioQuality = 192;
      GetID = (int) Window.WINDOW_VIDEO_EDITOR_COMPRESSSETTINGS;
    }

    public CompressionSettings Settings
    {
      get { return settings; }
      set
      {
        settings = value;
        //settings.resolutionX = 720;
        //settings.resolutionY = 576;
        //settings.videoQuality = 2200;
        //settings.audioQuality = 192;
      }
    }

    public override bool Init()
    {
      try
      {
        bool result = Load(GUIGraphicsContext.Skin + @"\VideoEditorCompressSettings.xml");
        return result;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      resolution.SetRange(1, 3);
      //resolution.SetRange(1, 4);
      resolution.SpinType = GUISpinControl.SpinType.Int;
      resolution.IntValue = 1;
      LoadSettings();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == okBtn)
      {
        result = true;
        SaveSettings();
        this.PageDestroy();
        return;
      }
      if (control == abbortBtn)
      {
        result = false;
        this.PageDestroy();
        return;
      }
      if (control == videoQualitySelect)
      {
        if (videoQualitySelect.GetLabel() == GUILocalizeStrings.Get(2062))
        {
          settings.videoQuality = 500;
        }
        else if (videoQualitySelect.GetLabel() == GUILocalizeStrings.Get(2063))
        {
          settings.videoQuality = 1000;
        }
        else if (videoQualitySelect.GetLabel() == GUILocalizeStrings.Get(2064))
        {
          settings.videoQuality = 2200;
        }
      }
      else if (control == audioQualitySelect)
      {
        if (audioQualitySelect.GetLabel() == GUILocalizeStrings.Get(2062))
        {
          settings.audioQuality = 128;
        }
        else if (audioQualitySelect.GetLabel() == GUILocalizeStrings.Get(2063))
        {
          settings.audioQuality = 192;
        }
        else if (audioQualitySelect.GetLabel() == GUILocalizeStrings.Get(2064))
        {
          settings.audioQuality = 256;
        }
      }
      else if (control == resolution)
      {
        if (resolution.IntValue == 1)
        {
          settings.resolutionX = 320;
          settings.resolutionY = 240;
          resolutionLbl.Label = "320 x 240";
        }
        else if (resolution.IntValue == 2)
        {
          settings.resolutionX = 640;
          settings.resolutionY = 480;
          resolutionLbl.Label = "640 x 480";
        }
        else if (resolution.IntValue == 3)
        {
          settings.resolutionX = 720;
          settings.resolutionY = 576;
          resolutionLbl.Label = "720 x 576";
        }
        else if (resolution.IntValue == 4)
        {
          settings.resolutionX = 1280;
          settings.resolutionY = 720;
          resolutionLbl.Label = "1280 x 720";
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        settings.audioQuality = xmlreader.GetValueAsInt("VideoEditor", "audioQuality", 192);
        settings.videoQuality = xmlreader.GetValueAsInt("VideoEditor", "videoQuality", 2200);
        settings.resolutionX = xmlreader.GetValueAsInt("VideoEditor", "resolutionX", 720);
        //settings.resolutionY = xmlreader.GetValueAsInt("VideoEditor", "resolutionY", 576);
      }

      switch (settings.audioQuality)
      {
        case 128:
          SelectItem(audioQualitySelect, GUILocalizeStrings.Get(2062));
          break;
        case 192:
          SelectItem(audioQualitySelect, GUILocalizeStrings.Get(2063));
          break;
        case 256:
          SelectItem(audioQualitySelect, GUILocalizeStrings.Get(2064));
          break;
      }

      switch (settings.videoQuality)
      {
        case 500:
          SelectItem(videoQualitySelect, GUILocalizeStrings.Get(2062));
          break;
        case 1000:
          SelectItem(videoQualitySelect, GUILocalizeStrings.Get(2063));
          break;
        case 2200:
          SelectItem(videoQualitySelect, GUILocalizeStrings.Get(2064));
          break;
      }

      switch (settings.resolutionX)
      {
        case 320:
          resolution.IntValue = 1;
          resolutionLbl.Label = "320 x 240";
          settings.resolutionY = 240;
          break;
        case 640:
          resolution.IntValue = 2;
          resolutionLbl.Label = "640 x 480";
          settings.resolutionY = 480;
          break;
        case 720:
          resolution.IntValue = 3;
          resolutionLbl.Label = "720 x 576";
          settings.resolutionY = 576;
          break;
      }
      resolution.SetRange(1, 3);
    }

    private void SelectItem(GUISpinControl spinControl, string text)
    {
      if (spinControl == null || text == string.Empty)
      {
        return;
      }
      for (int i = 0; i < spinControl.SubItemCount; i++)
      {
        string subItem = (string) spinControl.GetSubItem(i);
        if (subItem == text)
        {
          int j = 0;
          while (spinControl.Value != i)
          {
            //spinControl.Value = i;
            spinControl.MoveDown();
            // prevent for endless loop
            if (j > (spinControl.SubItemCount*2))
            {
              break;
            }
            j++;
          }
          break;
        }
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("VideoEditor", "audioQuality", settings.audioQuality);
        xmlwriter.SetValue("VideoEditor", "videoQuality", settings.videoQuality);
        xmlwriter.SetValue("VideoEditor", "resolutionX", settings.resolutionX);
        xmlwriter.SetValue("VideoEditor", "resolutionY", settings.resolutionY);
      }
    }

    public bool Result
    {
      get { return result; }
    }
  }
}