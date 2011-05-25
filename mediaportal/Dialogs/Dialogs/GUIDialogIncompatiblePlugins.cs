using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  public class GUIDialogIncompatiblePlugins : GUIDialogWindow
  {
    private const int DefaultTimeout = 30;


    [SkinControl(2)] protected GUIListControl listView = null;
    [SkinControl(3)] protected GUIButtonControl btnContinue = null;

    private int _timeOutInSeconds = DefaultTimeout;
    private int _timeLeft;
    private DateTime timeStart;

    public GUIDialogIncompatiblePlugins()
    {
      GetID = (int)Window.WINDOW_DIALOG_INCOMPATIBLE_PLUGINS;
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\dialogIncompatiblePlugins.xml");

      return result;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void DoModal(int dwParentId)
    {
      _timeOutInSeconds = DefaultTimeout;
      _timeLeft = 0;
      timeStart = DateTime.Now;
      UpdateCountDown(0);
      base.DoModal(dwParentId);
      GUIPropertyManager.SetProperty("#countdownseconds", "");
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
      if (_timeOutInSeconds > 0)
      {
        TimeSpan timeElapsed = DateTime.Now - timeStart;
        UpdateCountDown(timeElapsed.TotalSeconds);
        if (timeElapsed.TotalSeconds >= _timeOutInSeconds)
        {
          // handle timeout: close dialog and continue
          PageDestroy();
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
            base.OnMessage(message);
            FillList();
            // Set focus to "continue" button
            GUIControl.FocusControl(GetID, btnContinue.GetID);
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (btnContinue == null || iControl == btnContinue.GetID)
            {
              PageDestroy();
              return true;
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          {
            if (message.TargetControlId == listView.GetID)
            {
              _timeOutInSeconds = 0;
              GUIPropertyManager.SetProperty("#countdownseconds", "");
            }
          }
          break;
      }

      return base.OnMessage(message);
    }

    private void FillList()
    {
      listView.Clear();
      foreach (var plugin in PluginManager.IncompatiblePluginAssemblies)
      {
        listView.Add(CreateListItem(plugin));
      }
      foreach (var plugin in PluginManager.IncompatiblePlugins)
      {
        listView.Add(CreateListItem(plugin));
      }

    }

    private GUIListItem CreateListItem(Type plugin)
    {
      GUIListItem pItem = new GUIListItem();
      pItem.Label = plugin.Name;
      pItem.MusicTag = plugin;
      return pItem;
    }

    private GUIListItem CreateListItem(System.Reflection.Assembly plugin)
    {
      GUIListItem pItem = new GUIListItem();
      pItem.Label = plugin.GetName().Name;
      pItem.MusicTag = plugin;
      return pItem;
    }

   
  }
}
