using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Configuration;
using MediaPortal.Services;
using MediaPortal.Player;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MediaPortal.GUI.Notifier
{
  [PluginIcons("MediaPortal.GUI.Notifier.Logo.png", "MediaPortal.GUI.Notifier.LogoDisabled.png")]
  public class GUINotifier : GUIWindow, ISetupForm
  {
    #region Constants
    internal const int PLUGIN_ID = (int)Window.WINDOW_NOTIFIER;
    internal const string PLUGIN_NAME = "myNotifier";

    internal const string PLUGIN_TITLE = "My Notifier";

    private const string _TAG_PREFFIX = "#" + PLUGIN_NAME;
    private const string _TAG_MESSAGE_CURRENT_SOURCE = _TAG_PREFFIX + ".Message.Current.Origin";
    private const string _TAG_MESSAGE_CURRENT_TITLE = _TAG_PREFFIX + ".Message.Current.Title";
    private const string _TAG_MESSAGE_CURRENT_DESCRIPTION = _TAG_PREFFIX + ".Message.Current.Description";
    private const string _TAG_MESSAGE_CURRENT_LOGO = _TAG_PREFFIX + ".Message.Current.Logo";
    private const string _TAG_MESSAGE_CURRENT_THUMB = _TAG_PREFFIX + ".Message.Current.Thumb";
    private const string _TAG_MESSAGE_CURRENT_DATE = _TAG_PREFFIX + ".Message.Current.Date";
    private const string _TAG_MESSAGE_CURRENT_PUBLISHED = _TAG_PREFFIX + ".Message.Current.Published";
    private const string _TAG_MESSAGE_CURRENT_AUTHOR = _TAG_PREFFIX + ".Message.Current.Author";
    private const string _TAG_MESSAGE_CURRENT_LEVEL = _TAG_PREFFIX + ".Message.Current.Level";
    private const string _TAG_MESSAGE_CURRENT_CLASS = _TAG_PREFFIX + ".Message.Current.Class";
    private const string _TAG_MESSAGES_COUNT = _TAG_PREFFIX + ".Messages.Count.Total";
    private const string _TAG_MESSAGES_COUNT_READ = _TAG_PREFFIX + ".Messages.Count.Read";
    private const string _TAG_MESSAGES_COUNT_UNREAD = _TAG_PREFFIX + ".Messages.Count.Unread";

    #endregion

    #region Private Fields

    private INotifyMessageService _Service;
    private NotifyMessageClassEnum _FilterClass = NotifyMessageClassEnum.All;
    private NotifyMessageLevelEnum _FilterLevel = NotifyMessageLevelEnum.Information;
    private string _MessageIdLast = null;
    private MediaPlaybackModeEnum _MediaPlaybackMode = MediaPlaybackModeEnum.Play;
    private int _LastItemIdxInList = -1;

    #region GUI

    [SkinControl(13)]
    protected GUIAnimation _GUIanimationWork = null;

    [SkinControl(14)]
    protected GUIButtonControl _GUIbuttonReadAll = null;

    [SkinControl(15)]
    protected GUIButtonControl _GUIbuttonClear = null;

    [SkinControl(16)]
    protected GUIButtonControl _GUIbuttonClearAll = null;

    [SkinControl(17)]
    protected GUIButtonControl _GUIbuttonExit = null;

    [SkinControl(18)]
    protected GUISelectButtonControl _GUIbuttonLevel = null;

    [SkinControl(19)]
    protected GUISelectButtonControl _GUIbuttonClass = null;

    [SkinControl(50)]
    protected GUIFacadeControl _GUIfacadeList = null;

    #endregion

    #endregion

    #region Types
    private class GUIItemMessage : GUIListItem
    {
      public INotifyMessage Message;

      public GUIItemMessage(string strLabel)
          : base(strLabel)
      {
      }

      public object Tag;
    }

    private enum MediaPlaybackModeEnum {Ask, Play, VisitSource };

    #endregion

    #region ctor
    public GUINotifier()
    {
    }
    #endregion

    #region Overrides

    public override bool Init()
    {
      //Load settings
      using (Settings mpSettings = new MPSettings())
      {
        try
        {
          this._MediaPlaybackMode = (MediaPlaybackModeEnum)Enum.Parse(typeof(MediaPlaybackModeEnum),
            mpSettings.GetValueAsString("guiNotifier", "mediaPlaybackMode", MediaPlaybackModeEnum.Play.ToString()));
        }
        catch (Exception ex)
        {
          Log.Error("[GUINotifier][Init] Error while loading settinhs: {0}", ex.Message);
        }
      }

      //Init Service
      this._Service = GlobalServiceProvider.Get<INotifyMessageService>();
      if (this._Service != null)
      {
        //Hook to the service
        this._Service.NotifyEvent += this.cbNotifyService;
      }

      //Init tags
      tagsInit();
      this.tagsRefreshCountFromService();

      return this.Load(GUIGraphicsContext.Skin + "\\" + PLUGIN_NAME + ".xml");
    }

    public override void DeInit()
    {
      if (this._Service != null)
      {
        //Unhook from the service
        this._Service.NotifyEvent -= this.cbNotifyService;
      }

      //Save settings
      using (Settings mpSettings = new MPSettings())
      {
        try
        {
          mpSettings.SetValue("guiNotifier", "mediaPlaybackMode", this._MediaPlaybackMode);
        }
        catch (Exception ex)
        {
          Log.Error("[GUINotifier][DeInit] Error while loading settinhs: {0}", ex.Message);
        }
      }

      base.DeInit();
    }

    public override int GetID
    {
      get { return (PLUGIN_ID); }
      set { }
    }

    protected override void OnPageLoad()
    {
      //string strLoadParam = this._loadParameter;

      if (this._GUIbuttonLevel != null)
      {
        this._GUIbuttonLevel.Clear();

        string[] names = Enum.GetNames(typeof(NotifyMessageLevelEnum));
        for (int i = 0; i < names.Length; i++)
        {
          this._GUIbuttonLevel.Add(names[i]);
        }

        this._GUIbuttonLevel.SelectedItem = 0;
        this._GUIbuttonLevel.Label = GUILocalizeStrings.Get(35004) + NotifyMessageLevelEnum.Information.ToString();
      }

      if (this._GUIbuttonClass != null)
      {
        this._GUIbuttonClass.Clear();
        this._GUIbuttonClass.Add("All");

        string[] names = Enum.GetNames(typeof(NotifyMessageClassEnum));
        for (int i = 0; i < names.Length; i++)
        {
          if (names[i] == "All")
            continue;

          this._GUIbuttonClass.Add(names[i]);
        }

        this._GUIbuttonClass.SelectedItem = 0;
        this._GUIbuttonClass.Label = GUILocalizeStrings.Get(35005) + NotifyMessageClassEnum.All.ToString();
      }

      tagsCurrentMessageClear();

      //this._Cover.Active = true;

      this.guiFacadeFill();

      this.guiSetButtons();
    }

    protected override void OnClicked(int controlId, GUIControl control, Library.Action.ActionType actionType)
    {
      if (control != null && (actionType == Library.Action.ActionType.ACTION_INVALID || actionType == Library.Action.ActionType.ACTION_SELECT_ITEM))
      {
        if (control == this._GUIbuttonClear)
        {
          if (this._GUIfacadeList.Count > 0 && this._GUIfacadeList.SelectedListItem != null)
          {
            tagsCurrentMessageClear();
            INotifyMessage msg = ((GUIItemMessage)this._GUIfacadeList.SelectedListItem).Message;
            if (msg.Status == NotifyMessageStatusEnum.Read)
              this._Service.MessageUnregister(msg.MessageId);
            this.guiSetButtons();
          }
        }
        else if (control == this._GUIbuttonClearAll)
        {
          if (this._GUIfacadeList.Count > 0)
          {
            for (int i = this._GUIfacadeList.Count - 1; i >= 0; i--)
            {
              INotifyMessage msg = ((GUIItemMessage)this._GUIfacadeList[0]).Message;
              if (msg.Status == NotifyMessageStatusEnum.Read)
                this._Service.MessageUnregister(msg.MessageId);
            }
            this.guiSetButtons();
            if (this._GUIfacadeList.Count == 0)
              tagsCurrentMessageClear();
          }
        }
        else if (control == this._GUIfacadeList)
        {
          GUIItemMessage item = (GUIItemMessage)this._GUIfacadeList.SelectedListItem;
          if (item != null)
          {
            this._MessageIdLast = item.Message.MessageId;

            bool bShowPlugin = item.Message.PluginId > 0 && (item.Message.ActivatePluginWindow || !string.IsNullOrWhiteSpace(item.Message.PluginArguments));

            if (!string.IsNullOrWhiteSpace(item.Message.MediaLink))
            {
              //Media playback is available

              switch (this._MediaPlaybackMode)
              {
                case MediaPlaybackModeEnum.VisitSource:
                  if (bShowPlugin)
                    goto show_plugin;

                  break;

                case MediaPlaybackModeEnum.Play:
                  break;

                case MediaPlaybackModeEnum.Ask:
                  if (bShowPlugin)
                  {
                    IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                    dlg.Reset();
                    dlg.SetHeading(300012); //Action

                    dlg.AddLocalizedString(208); //Play
                    dlg.AddLocalizedString(35006); //Visit source

                    dlg.DoModal(GUIWindowManager.ActiveWindow);

                    if (dlg.SelectedId == -1)
                      return; //exit

                    if (dlg.SelectedId == 35006 && bShowPlugin)
                      goto show_plugin;
                  }

                  break;
              }

              //Try play the media
              if (g_Player.Play(item.Message.MediaLink))
              {
                g_Player.ShowFullScreenWindow();
                return;
              }
            }

          //Activate source Plugin
          show_plugin:
            if (bShowPlugin)
              GUIWindowManager.ActivateWindow(item.Message.PluginId, new NotifyMessageServiceEventArgs()
              {
                Message = item.Message,
                EventType = NotifyMessageServiceEventTypeEnum.MessageClicked
              });
          }
        }
        else if (control == this._GUIbuttonExit)
          GUIWindowManager.ShowPreviousWindow();
        else if (control == this._GUIbuttonLevel)
        {
          this._FilterLevel = (NotifyMessageLevelEnum)Enum.Parse(typeof(NotifyMessageLevelEnum), this._GUIbuttonLevel.SelectedLabel);
          this._GUIbuttonLevel.Label = GUILocalizeStrings.Get(35004) + this._FilterLevel.ToString();
          this.guiFacadeFill();
          this.guiSetButtons();
        }
        else if (control == this._GUIbuttonClass)
        {
          this._FilterClass = (NotifyMessageClassEnum)Enum.Parse(typeof(NotifyMessageClassEnum), this._GUIbuttonClass.SelectedLabel);
          this._GUIbuttonClass.Label = GUILocalizeStrings.Get(35005) + this._FilterClass.ToString();
          this.guiFacadeFill();
          this.guiSetButtons();
        }
        else if (control == this._GUIbuttonReadAll)
        {
          if (this._LastItemIdxInList >= 0)
          {
            for (int i = 0; i < this._GUIfacadeList.Count; i++)
            {
              GUIItemMessage item = (GUIItemMessage)this._GUIfacadeList[i];
              if (item.Message.Status != NotifyMessageStatusEnum.Read)
              {
                this._Service.MessageSetStatus(NotifyMessageStatusEnum.Read, item.Message.MessageId);
                item.IsPlayed = true;
              }

              //Do not mark new messages as read(to avoid accidently remove new messages)
              if (i == this._LastItemIdxInList)
                break;
            }
          }
          this.guiSetButtons();
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          if (message.TargetWindowId == PLUGIN_ID)
          {
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          if (message.TargetWindowId == PLUGIN_ID)
          {
            if (message.TargetControlId == this._GUIfacadeList.GetID)
            {
              this._LastItemIdxInList = this._GUIfacadeList.Count - 1;
              this.guiSetButtons();
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          if (message.TargetWindowId == PLUGIN_ID)
          {
            if (message.SenderControlId == this._GUIfacadeList.GetID)
            {
              GUIItemMessage item = (GUIItemMessage)this._GUIfacadeList.SelectedListItem;
              if (item != null)
                this.tagsCurrentMessageSet(item);
              else
                tagsCurrentMessageClear();
            }
          }
          break;
      }

      return base.OnMessage(message);
    }

    public override void OnAction(Library.Action action)
    {
      switch ((action.wID))
      {
        case Library.Action.ActionType.ACTION_PREVIOUS_MENU:
          break;
      }

      base.OnAction(action);
    }

    protected override void OnShowContextMenu()
    {
      IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

      INotifyMessage msg;
      if (this._GUIfacadeList.Count > 0 && this._GUIfacadeList.SelectedListItem != null)
        msg = ((GUIItemMessage)this._GUIfacadeList.SelectedListItem).Message;
      else
        msg = null;

      while (true)
      {
        int iId = 0;
        int iIdMediaPlaybackMode;

        dlg.Reset();
        dlg.SetHeading(496); //Options

        if (msg != null)
        {
          if (!string.IsNullOrWhiteSpace(msg.MediaLink))
          {
            dlg.AddLocalizedString(208); //Play
            iId++;
          }

          if (msg.PluginId > 0 && (msg.ActivatePluginWindow || !string.IsNullOrWhiteSpace(msg.PluginArguments)))
          {
            dlg.AddLocalizedString(35006); //Visit source
            iId++;
          }
        }

        //"Media playback mode: "
        dlg.Add(GUILocalizeStrings.Get(35007) + translatePlaybackMode(this._MediaPlaybackMode));
        iIdMediaPlaybackMode = ++iId;

        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedId == -1)
          break;
        else if (dlg.SelectedId == iIdMediaPlaybackMode)
        {
          //Rotate the value
          if ((int)++this._MediaPlaybackMode >= Enum.GetNames(typeof(MediaPlaybackModeEnum)).Length)
            this._MediaPlaybackMode = 0;
        }
        else if (dlg.SelectedId == 208)
        {
          //Play media
          g_Player.Play(msg.MediaLink);
          break;
        }
        else if (dlg.SelectedId == 35006)
        {
          //Show plugin
          GUIWindowManager.ActivateWindow(msg.PluginId, new NotifyMessageServiceEventArgs()
          {
            Message = msg,
            EventType = NotifyMessageServiceEventTypeEnum.MessageClicked
          });
          break;
        }
      }

      base.OnShowContextMenu();
    }
    #endregion

    #region ISetupForm

    //Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return PLUGIN_TITLE;
    }

    //Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Notifier." + " (" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";
    }

    //Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "Team MediaPortal";
    }

    //Show the setup dialog
    public void ShowPlugin()
    {
    }

    //Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return (true);
    }

    //Get Windows-ID
    public int GetWindowId()
    {
      //WindowID of windowplugin belonging to this setup
      //enter your own unique code
      return (PLUGIN_ID);
    }

    //Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return (true);
    }

    //Indicates if a plugin has it's own setup screen
    public bool HasSetup()
    {
      return false;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = this.PluginName();
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return true;
    }


    #endregion

    #region Tags
    private static void tagsInit()
    {
      tagsCurrentMessageClear();
      GUIPropertyManager.SetProperty(_TAG_MESSAGES_COUNT, "0");
      GUIPropertyManager.SetProperty(_TAG_MESSAGES_COUNT_READ, "0");
      GUIPropertyManager.SetProperty(_TAG_MESSAGES_COUNT_UNREAD, "0");
    }

    private static void tagsCurrentMessageClear()
    {
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_SOURCE, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_TITLE, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_DESCRIPTION, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_LOGO, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_THUMB, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_DATE, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_PUBLISHED, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_AUTHOR, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_LEVEL, string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_CLASS, string.Empty);
    }

    private void tagsCurrentMessageSet(GUIItemMessage item)
    {
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_SOURCE, item.Message.Origin);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_TITLE, item.Message.Title);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_DESCRIPTION, item.Message.Description);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_LOGO, item.Message.OriginLogo);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_AUTHOR, item.Message.Author);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_LEVEL, item.Message.Level.ToString());
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_CLASS, item.Message.Class.ToString());
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_DATE, item.Message.TimeStamp.ToString("dd.MM.yyyy HH:mm:ss"));
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_PUBLISHED, item.Message.PublishDate.Year > 2000 ? item.Message.PublishDate.ToString("dd.MM.yyyy HH:mm:ss") : string.Empty);
      GUIPropertyManager.SetProperty(_TAG_MESSAGE_CURRENT_THUMB, item.Message.Thumb ?? string.Empty);

      if (item.Message.Status != NotifyMessageStatusEnum.Read)
      {
        this._Service.MessageSetStatus(NotifyMessageStatusEnum.Read, item.Message.MessageId);
        item.IsPlayed = true;
      }
    }

    private void tagsRefreshCountFromService()
    {
      if (this._Service != null)
      {
        int iAll = this._Service.CountAll;
        int iRead = this._Service.CountRead;

        GUIPropertyManager.SetProperty(_TAG_MESSAGES_COUNT, iAll.ToString());
        GUIPropertyManager.SetProperty(_TAG_MESSAGES_COUNT_READ, iRead.ToString());
        GUIPropertyManager.SetProperty(_TAG_MESSAGES_COUNT_UNREAD, (iAll - iRead).ToString());
      }
    }

    #endregion

    #region Private methods

    private static GUIItemMessage guiItemCreate(INotifyMessage msg)
    {
      return new GUIItemMessage(msg.Title)
      {
        Message = msg,
        Label3 = msg.Origin,
        Label2 = msg.TimeStamp.ToString("dd.MM.yyyy\r\n  HH:mm:ss"),
        ThumbnailImage = msg.Thumb ?? string.Empty,
        IconImage = msg.OriginLogo ?? string.Empty,
        IsPlayed = msg.Status == NotifyMessageStatusEnum.Read
      };
    }

    private void guiSetButtons()
    {
      if (this._GUIfacadeList != null && this._GUIbuttonClear != null && this._GUIbuttonClearAll != null && this._GUIbuttonExit != null && this._GUIbuttonReadAll != null)
      {
        if (this._Service == null || this._Service.CountAll == 0)
        {
          if (!this._GUIbuttonClear.Disabled || !this._GUIbuttonClearAll.Disabled || !this._GUIbuttonReadAll.Disabled)
          {
            if (this._GUIbuttonClear.IsFocused || this._GUIbuttonClearAll.IsFocused || this._GUIbuttonReadAll.IsFocused)
              GUIControl.FocusControl(PLUGIN_ID, this._GUIbuttonExit.GetID);

            this._GUIfacadeList.ListLayout.NavigateLeft = this._GUIbuttonExit.GetID;
            this._GUIbuttonClear.Disabled = true;
            this._GUIbuttonClearAll.Disabled = true;
            this._GUIbuttonReadAll.Disabled = true;
          }
        }
        else
        {
          bool bDisable = !this._GUIfacadeList.ListLayout.ListItems.Any(it => ((GUIItemMessage)it).Message.Status == NotifyMessageStatusEnum.Read);
          if (bDisable != this._GUIbuttonClearAll.Disabled)
            this._GUIbuttonClearAll.Disabled = bDisable;

          GUIItemMessage item = (GUIItemMessage)this._GUIfacadeList.SelectedListItem;
          bDisable = item == null || item.Message.Status != NotifyMessageStatusEnum.Read;
          if (bDisable != this._GUIbuttonClear.Disabled)
            this._GUIbuttonClear.Disabled = bDisable;

          bDisable = true;
          if (this._LastItemIdxInList >= 0 && this._GUIfacadeList.Count > 0)
          {
            for (int i = 0; i < this._GUIfacadeList.Count; i++)
            {
              GUIItemMessage it = (GUIItemMessage)this._GUIfacadeList[i];
              if (it.Message.Status != NotifyMessageStatusEnum.Read)
              {
                bDisable = false;
                break;
              }

              //do not check fresh new messages
              if (i == this._LastItemIdxInList)
                break;
            }
          }
          
          if (bDisable != this._GUIbuttonReadAll.Disabled)
            this._GUIbuttonReadAll.Disabled = bDisable;

          int iId;
          if (!this._GUIbuttonReadAll.Disabled)
            iId = this._GUIbuttonReadAll.GetID;
          else if (!this._GUIbuttonClear.Disabled)
            iId = this._GUIbuttonClear.GetID;
          else if (!this._GUIbuttonClearAll.Disabled)
            iId = this._GUIbuttonClearAll.GetID;
          else
            iId = this._GUIbuttonExit.GetID;

          this._GUIfacadeList.ListLayout.NavigateLeft = iId;

          iId = -1;
          if (this._GUIbuttonReadAll.Disabled && this._GUIbuttonReadAll.Focus)
            iId = this._GUIbuttonClear.GetID;

          if (this._GUIbuttonClear.Disabled && (this._GUIbuttonClear.Focus || this._GUIbuttonClear.GetID == iId))
            iId = this._GUIbuttonClearAll.GetID;

          if (this._GUIbuttonClearAll.Disabled && (this._GUIbuttonClearAll.Focus || this._GUIbuttonClearAll.GetID == iId))
            iId = this._GUIbuttonExit.GetID;

          if (iId > 0)
            GUIControl.FocusControl(PLUGIN_ID, iId);
        }
      }
    }

    private void guiFacadeFill()
    {
      this._GUIfacadeList.Clear();

      try
      {
        if (this._Service != null)
        {
          this._Service.MessageGetAll(cls: this._FilterClass, level: this._FilterLevel).ForEach(m =>
          {
            this._GUIfacadeList.Add(guiItemCreate(m));
            if (m.MessageId == this._MessageIdLast)
            {
              this._GUIfacadeList.SelectedListItemIndex = this._GUIfacadeList.Count - 1;
              this._MessageIdLast = null;
            }
          });
          this._LastItemIdxInList = this._GUIfacadeList.Count - 1;
        }
      }
      catch (Exception ex)
      {
        Log.Error("[GUINotifier][guiFacadeFill] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    private void guiFacadeMessageAdd(INotifyMessage msg)
    {
      if (this._GUIfacadeList != null)
      {
        if ((this._FilterClass == NotifyMessageClassEnum.All ||
         (this._FilterClass == NotifyMessageClassEnum.General && msg.Class == NotifyMessageClassEnum.General) ||
          (msg.Class & this._FilterClass) != 0) && msg.Level >= this._FilterLevel)
        {

          this._GUIfacadeList.Add(guiItemCreate(msg));

          if (this._GUIfacadeList.Focus)
            this._LastItemIdxInList = this._GUIfacadeList.Count - 1;
        }

        this.guiSetButtons();
      }
    }

    private void guiFacadeMessageRemove(INotifyMessage msg)
    {
      if (this._GUIfacadeList != null)
      {
        for (int i = 0; i < this._GUIfacadeList.ListLayout.ListItems.Count; i++)
        {
          GUIItemMessage gItem = (GUIItemMessage)this._GUIfacadeList.ListLayout.ListItems[i];
          if (gItem.Message == msg)
          {
            this._GUIfacadeList.ListLayout.RemoveItem(i);
            this.guiSetButtons();
            return;
          }
        }
      }
    }

    private void guiFacadeMessageChange(INotifyMessage msg)
    {
      if (this._GUIfacadeList != null)
      {
        for (int i = 0; i < this._GUIfacadeList.ListLayout.ListItems.Count; i++)
        {
          GUIItemMessage gItem = (GUIItemMessage)this._GUIfacadeList.ListLayout.ListItems[i];
          if (gItem.Message == msg)
          {
            if (!gItem.IsPlayed && msg.Status == NotifyMessageStatusEnum.Read)
            {
              gItem.IsPlayed = true;
              this.guiSetButtons();
            }
            return;
          }
        }
      }
    }

    private static string translatePlaybackMode(MediaPlaybackModeEnum mode)
    {
      switch (mode)
      {
        case MediaPlaybackModeEnum.Ask:
          return GUILocalizeStrings.Get(300008); //Ask what to do

        case MediaPlaybackModeEnum.Play:
          return GUILocalizeStrings.Get(208); //Play

        case MediaPlaybackModeEnum.VisitSource:
          return GUILocalizeStrings.Get(35006); //Visit source

        default:
          return string.Empty;
      }
    }

    #endregion

    #region Callbacks

    /// <summary>
    /// Callback from Notify Service
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cbNotifyService(object sender, NotifyMessageServiceEventArgs e)
    {
      switch (e.EventType)
      {
        case NotifyMessageServiceEventTypeEnum.MessageRegistered:
          if (e.Message.DialogMode != NotifyMessageDialogModeEnum.ShowDialogOnly)
          {
            if (Thread.CurrentThread.Name == "MPMain")
              this.guiFacadeMessageAdd(e.Message);
            else
              GUIWindowManager.SendThreadCallback((int param1, int param2, object data) =>
              {
                this.guiFacadeMessageAdd((INotifyMessage)data);
                return 0;
              }, 0, 0, e.Message);
          }

          this.tagsRefreshCountFromService();
          break;

        case NotifyMessageServiceEventTypeEnum.MessageUnregistered:
          if (Thread.CurrentThread.Name == "MPMain")
            this.guiFacadeMessageRemove(e.Message);
          else
            GUIWindowManager.SendThreadCallback((int param1, int param2, object data) =>
            {
              this.guiFacadeMessageRemove((INotifyMessage)data);
              return 0;
            }, 0, 0, e.Message);

          this.tagsRefreshCountFromService();
          break;

        case NotifyMessageServiceEventTypeEnum.MessageStatusChanged:
          if (Thread.CurrentThread.Name == "MPMain")
            this.guiFacadeMessageChange(e.Message);
          else
            GUIWindowManager.SendThreadCallback((int param1, int param2, object data) =>
            {
              this.guiFacadeMessageChange((INotifyMessage)data);
              return 0;
            }, 0, 0, e.Message);

          this.tagsRefreshCountFromService();
          break;
      }

    }

    #endregion
  }
}
