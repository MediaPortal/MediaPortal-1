using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Player;
using System.Drawing;

namespace MediaPortal.Dialogs
{
  public class GUIDialogOldSkin : GUIDialogWindow
  {
    [SkinControl(2)]
    protected GUICheckMarkControl  chkIgnore = null;
    [SkinControl(3)]
    protected GUIButtonControl btnContinue = null;

    private int _timeOutInSeconds = 15;
    private int _timeLeft;
    private DateTime timeStart;

    private string _userSkin;
    private bool _revertToOldSkin;

    public GUIDialogOldSkin()
    {
      GetID = (int)Window.WINDOW_DIALOG_OLD_SKIN;
    }

    public string UserSkin
    {
      get { return _userSkin; }
      set { _userSkin = value; }
    }

    public bool RevertToUserSkin
    {
      get { return _revertToOldSkin; } 
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\dialogOldSkin.xml");

      return result;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void DoModal(int dwParentId)
    {
      int nagCount;
      using (Settings xmlreader = new MPSettings())
      {
        nagCount = xmlreader.GetValueAsInt("general", "skinobsoletecount", 0);
      }

      //if (chkIgnore != null)
      //{
        chkIgnore.Visible = nagCount > 4;
      //}
      
      GUIPropertyManager.SetProperty("#userskin", _userSkin);
      _timeLeft = 0;
      timeStart = DateTime.Now;
      UpdateCountDown(0);
      base.DoModal(dwParentId);
      GUIPropertyManager.SetProperty("#userskin", "");
      GUIPropertyManager.SetProperty("#countdownseconds", "");

      if (RevertToUserSkin)
      {
        nagCount++; 
      }

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "dontshowskinversion", chkIgnore.Selected);
        xmlwriter.SetValue("general", "skinobsoletecount", nagCount);
      }
    }

    private void UpdateCountDown(double timeElapsed)
    {
      int timeLeft = _timeOutInSeconds - (int)Math.Truncate(timeElapsed);
      if (timeLeft != _timeLeft)
      {
        _timeLeft = timeLeft;
        GUIPropertyManager.SetProperty("#countdownseconds", _timeLeft.ToString());
      }
    }

    public override bool ProcessDoModal()
    {
      bool result = base.ProcessDoModal();
      TimeSpan timeElapsed = DateTime.Now - timeStart;
      UpdateCountDown(timeElapsed.TotalSeconds);
      if (_timeOutInSeconds > 0)
      {
        if (timeElapsed.TotalSeconds >= _timeOutInSeconds)
        {
          // handle timeout: close dialog and continue
          PageDestroy();
          _revertToOldSkin = false;
        }
      }
      return result;
    }

    public override bool OnMessage(GUIMessage message)
    {
      //needRefresh = true;
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            _revertToOldSkin = false;
            base.OnMessage(message);
            GUIControl.FocusControl(GetID, btnContinue.GetID);
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (btnContinue == null)
            {
              _revertToOldSkin = false;
              PageDestroy();
              return true;
            }
            if (iControl == btnContinue.GetID)
            {
              _revertToOldSkin = true;
              RevertSkin();
              PageDestroy();
              return true;
            }
          }
          break;
      }

      return base.OnMessage(message);
    }

    public void RevertSkin()
    {
      //int ActiveWindowID = GUIWindowManager.ActiveWindow;
      // Change skin back to OutdatedSkinName
      GUIGraphicsContext.Skin = _userSkin;
      GUITextureManager.Clear();
      GUITextureManager.Init();
      SkinSettings.Load();
      GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
      GUIFontManager.InitializeDeviceObjects();
      GUIExpressionManager.ClearExpressionCache();
      GUIControlFactory.ClearReferences();
      GUIControlFactory.LoadReferences(GUIGraphicsContext.GetThemedSkinFile(@"\references.xml"));
      GUIWindowManager.OnResize();
      //GUIWindowManager.ActivateWindow(ActiveWindowID);

      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("general", "skinobsoletecount", 0);
        if (!GUIGraphicsContext.Fullscreen)
        {
          try
          {
            GUIGraphicsContext.form.ClientSize = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
          }
          catch (Exception ex)
          {
            Log.Error("OnSkinChanged exception:{0}", ex.ToString());
            Log.Error(ex);
          }
        }
      }

      if (BassMusicPlayer.Player != null && BassMusicPlayer.Player.VisualizationWindow != null)
      {
        BassMusicPlayer.Player.VisualizationWindow.Reinit();
      }
    }

  }
}
