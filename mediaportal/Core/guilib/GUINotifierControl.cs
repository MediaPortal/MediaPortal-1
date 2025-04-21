using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Timers;
using System.Threading;
using System.Runtime.CompilerServices;
using MediaPortal.Services;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  public class GUINotifierControl : GUIGroup
  {
    public enum VisualStyleEnum
    {
      HorizontalScroll,
      VerticalFlip
    }

    public enum FullscreenVideoBehaviorEnum
    {
      Default = 0,
      RunAlways,
      RunWhenTvPlayback,
      Sleep
    }

    #region GUI properties
    [XMLSkinElement("messageFilterClass")] protected NotifyMessageClassEnum _MessageFilterClass = NotifyMessageClassEnum.All;
    [XMLSkinElement("messageFilterLevel")] protected NotifyMessageLevelEnum _MessageFilterLevel = NotifyMessageLevelEnum.Information;
    [XMLSkinElement("messageFilterPluginId")] protected string _MessageFilterPluginID = string.Empty;

    [XMLSkinElement("visualStyle")] protected VisualStyleEnum _VisualStyle = VisualStyleEnum.VerticalFlip;
    [XMLSkinElement("rotationSpeed")] protected int _RotationSpeed = 400;

    [XMLSkinElement("fullscreenVideoBehavior")] protected FullscreenVideoBehaviorEnum _FullscreenVideoBehavior = FullscreenVideoBehaviorEnum.Default;

    [XMLSkinElement("messageTimeActive")] protected int _TimeMessageActive = -1; //automatic

    [XMLSkinElement("backgroundTexture")] protected string _BackgroundTexture = string.Empty;

    [XMLSkinElement("messageFont")] protected string _MessageFont = "font12";
    [XMLSkinElement("messageOffsetX")] protected int _MessageOffsetX = 110;
    [XMLSkinElement("messageOffsetY")] protected int _MessageOffsetY = 6;
    [XMLSkinElement("messageWidth")] protected int _MessageWidth = 1720;
    [XMLSkinElement("messageHeight")] protected int _MessageHeight = 53;
    [XMLSkinElement("messageColorInformation")] protected long _MessageColorInformation = 0xFFFFFFFF;
    [XMLSkinElement("messageColorWarning")] protected long _MessageColorWarning = 0xFFFFFF00;
    [XMLSkinElement("messageColorError")] protected long _MessageColorError = 0xFFFF0000;

    [XMLSkinElement("messageIconOffsetX")] protected int _MessageIconOffsetX = 50;
    [XMLSkinElement("messageIconOffsetY")] protected int _MessageIconOffsetY = 7;
    [XMLSkinElement("messageIconWidth")] protected int _MessageIconWidth = 50;
    [XMLSkinElement("messageIconHeight")] protected int _MessageIconHeight = 50;

    [XMLSkinElement("camera")] private bool _HasCamera = false;

    [XMLSkinElement("active")] private string _Active = string.Empty;

    #endregion

    protected GUIImage _GuiImageBackground = null;
    protected GUIImage _GuiImageMessageIcon = null;
    protected GUIImage _GuiImageMessageIconOut = null;
    protected GUIFadeLabel _GuiLabelMessage = null;
    protected GUIFadeLabel _GuiLabelMessageOut = null;



    private const int _TIME_MIN_WAIT = 500;
    private const int _TIME_MIN_IDDLE = 8000;
    private const int _TIME_ACTIVE_MESSAGE_MIN = 3000;
    private const int _TIME_ACTIVE_CHAR_BASE = 200;

    private enum Status { Off, MessageActive, MessageClosing, MessageOffPeriod }

    private ManualResetEvent _FlagWakeUp = new ManualResetEvent(false);
    private Thread _ThreadMessenger;
    private Status _MessageStatus = Status.Off;
    private List<INotifyMessage> _MessageList = new List<INotifyMessage>();
    private bool _MessengerActive = false;
    private int _MessageIdx = 0;
    private bool _Terminate = false;
    private bool _Paused = false;
    private bool _IsSleeping = false;
    private int _TimePeriod = 0;
    private INotifyMessageService _Service;
    private List<int> _FilterPluginIds = null;
    private int _AutoSetMessageTtl = -1; //for video fullscreen mode
    private int _ActiveCondition = 0;
    private bool _IsActive = true;

    private int _Id = -1;
    private static int _IdCnt = -1;

    public override bool Dimmed { get { return false; } set { } }

    public bool IsActive
    {
      get
      {
        return this._IsActive;
      }

      set
      {
        if (value != this._IsActive)
        {
          Log.Debug("[GUINotifierControl][{0}] IsActive: {1}", this._Id, value);
          this._IsActive = value;

          if (!value)
            this.rotatorPause();
          else
            this.rotatotrUpdateMode();
        }
      }
    }

    #region ctor
    /// <summary>
    /// The constructor of the GUINotifierControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    public GUINotifierControl(int dwParentID)
        : base(dwParentID)
    {
      this._Id = Interlocked.Increment(ref _IdCnt);
    }
    #endregion

    #region Overrides

    public override void Render(float timePassed)
    {
      if (this._ActiveCondition != 0)
        this.IsActive = GUIInfoManager.GetBool(this._ActiveCondition, this.ParentID);

      if (this._IsActive)
        base.Render(timePassed);
    }

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      if (!string.IsNullOrWhiteSpace(this._Active))
        this._ActiveCondition = GUIInfoManager.TranslateString(this._Active);

      this.HasCamera = this._HasCamera;

      using (Settings xmlreader = new MPSettings())
      {
        if (this._FullscreenVideoBehavior == FullscreenVideoBehaviorEnum.Default &&
          !Enum.TryParse(xmlreader.GetValueAsString("guiNotifier", "fullscreenVideoBehavior", "RunWhenTvPlayback"), true,
          out this._FullscreenVideoBehavior))
          this._FullscreenVideoBehavior = FullscreenVideoBehaviorEnum.RunWhenTvPlayback;
      }

      #region GUIControls
        this._GuiLabelMessage = new GUIFadeLabel(this._parentControlId, 0,
        this._positionX + this._MessageOffsetX, this._positionY + this._MessageOffsetY, this._MessageWidth, this._MessageHeight,
        this._MessageFont, this._MessageColorInformation, Alignment.Left, VAlignment.Middle, 0, 0, 0, " *** ")
      { Visible = false };

      this._GuiLabelMessageOut = new GUIFadeLabel(this._parentControlId, 0,
        this._positionX + this._MessageOffsetX, this._positionY + this._MessageOffsetY, this._MessageWidth, this._MessageHeight,
        this._MessageFont, this._MessageColorInformation, Alignment.Left, VAlignment.Middle, 0, 0, 0, " *** ")
      { Visible = false };

      this._GuiImageMessageIcon = new GUIImage(this._parentControlId, 0, this._positionX + this._MessageIconOffsetX, this._positionY + this._MessageIconOffsetY, this._MessageIconWidth, this._MessageIconHeight, "InfoService\\defaultFeedRSS.png", 1)
      { Visible = false };

      this._GuiImageMessageIconOut = new GUIImage(this._parentControlId, 0, this._positionX + this._MessageIconOffsetX, this._positionY + this._MessageIconOffsetY, this._MessageIconWidth, this._MessageIconHeight, "InfoService\\defaultFeedRSS.png", 1)
      { Visible = false };

      this._GuiImageBackground = new GUIImage(this._parentControlId, 0, this._positionX, this._positionY, this._width, this._height, this._BackgroundTexture, 1)
      { Visible = false };
      #endregion

      #region Animations
      if (this._RotationSpeed < 1)
        this._RotationSpeed = 400;

      VisualEffect ve;
      XmlDocument xml = new XmlDocument();
      List<VisualEffect> anims = new List<VisualEffect>();
      if (this._VisualStyle == VisualStyleEnum.HorizontalScroll)
      {
        xml.LoadXml(
          string.Format("<root>" +
          "<animation effect=\"slide\" start=\"{1},0\" tween=\"quadratic\" easing=\"in\" delay=\"100\" time=\"{0}\" reversible=\"false\">Visible</animation>" +
          "<animation effect=\"slide\" end=\"-{2},0\" tween=\"quadratic\" easing=\"in\" time=\"{0}\" reversible=\"false\">Visible</animation>" +
          "</root>",
          this._RotationSpeed,
          this._MessageWidth + this._MessageOffsetX,
          this._width)
          );

        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[0]);
        anims.Add(ve);
        this._GuiLabelMessage.AddAnimations(anims);
        this._GuiImageMessageIcon.AddAnimations(anims);

        anims.Clear();
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[1]);
        anims.Add(ve);
        this._GuiLabelMessageOut.AddAnimations(anims);
        this._GuiImageMessageIconOut.AddAnimations(anims);
      }
      else
      {
        const string _ANIM_FLIP = "<root>" +
          "<animation effect=\"rotatex\" start=\"-90\" centerRelative=\"{1},{2}\" time=\"{0}\" reversible=\"false\">Visible</animation>" +
          "<animation effect=\"fade\" start=\"0\" time=\"{0}\" reversible=\"false\">Visible</animation>" +
          "<animation effect=\"rotatex\" end=\"90\" centerRelative=\"{1},{3}\" time=\"{0}\" reversible=\"false\">Visible</animation>" +
          "<animation effect=\"fade\" start=\"100\" end=\"0\" time=\"{0}\" reversible=\"false\">Visible</animation>" +
          "</root>";

        xml.LoadXml(
          string.Format(_ANIM_FLIP,
          this._RotationSpeed,
          (int)(this._MessageHeight * -0.085F),
          (int)(this._MessageHeight * 0.33F),
          (int)(this._MessageHeight * 0.33F))
          );

        anims.Clear();
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[0]);
        anims.Add(ve);
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[1]);
        anims.Add(ve);
        this._GuiLabelMessage.AddAnimations(anims);

        anims.Clear();
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[2]);
        anims.Add(ve);
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[3]);
        anims.Add(ve);
        this._GuiLabelMessageOut.AddAnimations(anims);


        xml = new XmlDocument();
        xml.LoadXml(
          string.Format(_ANIM_FLIP,
          this._RotationSpeed,
          (int)(this._GuiImageMessageIcon.Height * -0.085F),
          (int)(this._GuiImageMessageIcon.Height * 0.33F),
          (int)(this._GuiImageMessageIcon.Height * 0.33F))
          );

        anims.Clear();
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[0]);
        anims.Add(ve);
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[1]);
        anims.Add(ve);
        this._GuiImageMessageIcon.AddAnimations(anims);

        anims.Clear();
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[2]);
        anims.Add(ve);
        ve = new VisualEffect();
        ve.Create(xml.DocumentElement.ChildNodes[3]);
        anims.Add(ve);
        this._GuiImageMessageIconOut.AddAnimations(anims);
      }
      #endregion

      this._GuiLabelMessage.ParentControl = this;
      this._GuiLabelMessageOut.ParentControl = this;
      this._GuiImageMessageIcon.ParentControl = this;
      this._GuiImageMessageIconOut.ParentControl = this;
      this._GuiImageBackground.ParentControl = this;

      //Add controls to the GUIGroup
      this.addControlToBase(this._GuiImageBackground);
      this.addControlToBase(this._GuiLabelMessage);
      this.addControlToBase(this._GuiImageMessageIcon);
      this.addControlToBase(this._GuiLabelMessageOut);
      this.addControlToBase(this._GuiImageMessageIconOut);

      this.UpdateVisibility();
      this.Visible = false;
      this.rotatorInit();

      #region Service
      this._Service = GlobalServiceProvider.Get<INotifyMessageService>();
      if (this._Service != null)
      {
        //Hook to the service
        this._Service.NotifyEvent += this.cbNotifyService;

        //Create pluginId filter list
        if (!string.IsNullOrWhiteSpace(this._MessageFilterPluginID))
        {
          this._FilterPluginIds = new List<int>();
          string[] parts = this._MessageFilterPluginID.Split(',');
          for(int i = 0; i< parts.Length; i++)
          {
            int iId;
            if (int.TryParse(parts[i], out iId))
              this._FilterPluginIds.Add(iId);
          }
        }

        //Get all messages related to this control(based on filter)
        this._Service.MessageGetAll(this._MessageFilterClass, this._MessageFilterLevel, this._FilterPluginIds).ForEach(m => this.rotatorAddNewMessage(m));

        //Start rotator
        this.rotatorStart();

        //Hook to the window mode change
        GUIGraphicsContext.OnVideoWindowChanged += this.cbVideoWindowChanged;
        this.cbVideoWindowChanged();

        //Test
        //if (this._MessageList.Count == 0)
        //{
        //  string strId;
        //  this._Service.MessageRegister("Message test 1", "GUINotifierControl", 123456, DateTime.Now, out strId,
        //    strDescription: "This is testing message 1",
        //    strAuthor: "GUINotifierControl",
        //    strThumb: "https://storage.animetosho.org/sframes/00115a74_14020.jpg?w=128&h=96",
        //    strOriginLogo: "defaultExtension.png",
        //    level: NotifyMessageLevelEnum.Error );

        //  this._Service.MessageRegister("Message test 2", "GUINotifierControl", 123456, DateTime.Now, out strId,
        //    strDescription: "This is testing message 2",
        //    strAuthor: "GUINotifierControl",
        //    strThumb: "https://cdn-eu.anidb.net/images/65/89945.jpg-thumb.jpg",
        //    strOriginLogo: "defaultExtension.png",
        //    level: NotifyMessageLevelEnum.Warning,
        //    dlg: NotifyMessageDialogModeEnum.ShowDialog);
        //}
      }
      #endregion
    }

    /// <summary>
    /// Checks if the control can focus.
    /// </summary>
    /// <returns>false</returns>
    public override bool CanFocus()
    {
      return false;
    }

    /// <summary>
    /// Allocate any direct3d sources
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
    }

    /// <summary>
    /// Free any direct3d resources
    /// </summary>
    public override void Dispose()
    {
      try
      {
        base.Dispose();

        if (this._Service != null)
        {
          //Unhook from the service
          this._Service.NotifyEvent -= this.cbNotifyService;
          this.rotatorStop();
        }

        //Unhook from the window mode change
        GUIGraphicsContext.OnVideoWindowChanged -= this.cbVideoWindowChanged;
      }
      catch { }
    }
    #endregion

    #region Private methods

    private void addControlToBase(GUIControl control)
    {
      //Inherited animations of the GUINotifer
      int iCnt = base.Animations.Count;

      this.AddControl(control);

      //We need to place GUINotifer's animations to the top of the list
      while (iCnt-- > 0)
      {
        int iIdxLast = control.Animations.Count - 1;
        VisualEffect ve = control.Animations[iIdxLast];
        control.Animations.RemoveAt(iIdxLast);
        control.Animations.Insert(0, ve);
      }
    }

    private void cbNotifyService(object sender, NotifyMessageServiceEventArgs e)
    {
      if ((this._MessageFilterClass == NotifyMessageClassEnum.All ||
          (this._MessageFilterClass == NotifyMessageClassEnum.General && e.Message.Class == NotifyMessageClassEnum.General) ||
          (e.Message.Class & this._MessageFilterClass) != 0) &&
            (this._FilterPluginIds == null || this._FilterPluginIds.Any(id => id == e.Message.PluginId)) &&
            e.Message.Level >= this._MessageFilterLevel)
      {
        switch (e.EventType)
        {
          case NotifyMessageServiceEventTypeEnum.MessageRegistered:
            this.rotatorAddNewMessage(e.Message);
            break;

          case NotifyMessageServiceEventTypeEnum.MessageUnregistered:
          case NotifyMessageServiceEventTypeEnum.MessageStatusChanged:
            this.rotatorRemoveMessage(e.Message.MessageId);
            break;
        }
      }
    }

    /// <summary>
    /// Callback from GUIGraphicsContext.OnVideoWindowChanged
    /// </summary>
    private void cbVideoWindowChanged()
    {
      //_Logger.Debug("[cbVideoWindowChanged] FullScreen:{0} TV:{1}", GUIGraphicsContext.IsFullScreenVideo, g_Player.IsTV);

      this.rotatotrUpdateMode();
    }

    private void activateNewMessage(INotifyMessage msg)
    {
      if (this._GuiLabelMessage != null)
      {
        lock (GUIGraphicsContext.RenderLock)
        {
          if (!this.Visible)
          {
            this._GuiLabelMessage.Label = string.Empty;
            this._GuiImageMessageIcon.FileName = string.Empty;
            this.Visible = true;
          }

          //Pass current message props to outgoing message
          this._GuiLabelMessageOut.Label = this._GuiLabelMessage.Label;
          this._GuiLabelMessageOut.TextColor = this._GuiLabelMessage.TextColor;
          this._GuiImageMessageIconOut.FileName = this._GuiImageMessageIcon.FileName;

          //New message
          this._GuiLabelMessage.Label = msg.MessageText;
          this._GuiImageMessageIcon.FileName = msg.OriginLogo;

          switch (msg.Level)
          {
            case NotifyMessageLevelEnum.Warning:
              this._GuiLabelMessage.TextColor = this._MessageColorWarning;
              break;

            case NotifyMessageLevelEnum.Error:
              this._GuiLabelMessage.TextColor = this._MessageColorError;
              break;

            default:
              this._GuiLabelMessage.TextColor = this._MessageColorInformation;
              break;
          }


          //Trigger the messages's animation
          int iIdx = this._GuiLabelMessage.Animations.Count - 1;

          if (!this._GuiLabelMessage.Visible)
          {
            this._GuiImageBackground.Visible = true;
            this._GuiLabelMessage.Visible = true;
            this._GuiLabelMessageOut.Visible = true;
            this._GuiImageMessageIcon.Visible = true;
            this._GuiImageMessageIconOut.Visible = true;
          }
          else if (this._VisualStyle == VisualStyleEnum.HorizontalScroll)
          {
            this._GuiLabelMessage.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;
            this._GuiLabelMessageOut.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;
            this._GuiImageMessageIcon.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;
            this._GuiImageMessageIconOut.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;
          }
          else
          {
            this._GuiLabelMessage.Animations[iIdx - 1].QueuedProcess = AnimationProcess.Normal;
            this._GuiLabelMessage.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;

            this._GuiLabelMessageOut.Animations[iIdx - 1].QueuedProcess = AnimationProcess.Normal;
            this._GuiLabelMessageOut.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;

            this._GuiImageMessageIcon.Animations[iIdx - 1].QueuedProcess = AnimationProcess.Normal;
            this._GuiImageMessageIcon.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;

            this._GuiImageMessageIconOut.Animations[iIdx - 1].QueuedProcess = AnimationProcess.Normal;
            this._GuiImageMessageIconOut.Animations[iIdx].QueuedProcess = AnimationProcess.Normal;
          }
        }
      }
    }

    private void deactivate()
    {
      this._GuiLabelMessageOut.Label = string.Empty;
      this._GuiImageMessageIconOut.FileName = string.Empty;
      this.Visible = false;
    }

    private bool _IsMessageToBeShown(INotifyMessage msg)
    {
      return msg.MessageTTL != 0 && (msg.Status == NotifyMessageStatusEnum.Unread || (msg.Status == NotifyMessageStatusEnum.Shown && this._AutoSetMessageTtl < 1));
    }

    private int _MessagesToShowCount
    {
      get
      {
        lock (this._MessageList)
        {
          return this._MessageList.Count > 0 ? this._MessageList.Count(p => this._IsMessageToBeShown(p)) : 0;
        }
      }
    }

    private bool _IsAnyMessageToShow
    {
      get
      {
        return this._MessageList.Count > 0 && this._MessageList.Any(p => this._IsMessageToBeShown(p));
      }
    }

    private bool isMessageToShowSingle(INotifyMessage msg)
    {
      if (this._MessageList.Count > 0)
      {
        int iCnt = 0;
        foreach (INotifyMessage m in this._MessageList.Where(p => this._IsMessageToBeShown(p)))
        {
          if (iCnt++ != 0 || msg != m)
            return false;
        }

        return iCnt == 1;
      }
      else
        return false;
    }

    private int getMessagePeriod(INotifyMessage msg)
    {
      //Presentation period
      if (this._TimeMessageActive <= 0)
        return Math.Max(_TIME_ACTIVE_MESSAGE_MIN, msg.MessageText.Length * _TIME_ACTIVE_CHAR_BASE);
      else
        return this._TimeMessageActive;
    }

    /// <summary>
    /// Automacitally set message TTL if not specified [ms]; >0: multiples of default time otherwise disabled
    /// </summary>
    private void setAutoSetMessageTtl(int value)
    {
      if (value != this._AutoSetMessageTtl)
      {
        this._AutoSetMessageTtl = value;

        Log.Debug("[GUINotifierControl][{0}][setAutoSetMessageTtl] Value: {1}", this._Id, value);

        if (value > 0 && this._IsSleeping)
          this._FlagWakeUp.Set();
      }
    }

    private bool rotatorProcessMessage(INotifyMessage msg)
    {
      bool bResult;

      //Delete message if needed
      if (msg.DialogMode == NotifyMessageDialogModeEnum.ShowDialogOnly || (msg.DeleteMessageAfterPresentation && msg.MessageTTL == 0))
      {
        //Remove from the list
        this._MessageList.Remove(msg);

        //Unregister the message
        this._Service.MessageUnregister(msg.MessageId);

        return false;  //expired
      }
      else if (!this._IsMessageToBeShown(msg))
        bResult = false; // expired
      else
      {
        //Presentation Time To Live preselection
        if (msg.MessageTTL < 0 && msg.MessagePTTL < 0 && this._AutoSetMessageTtl > 0)
        {
          //Presentation period
          msg.MessagePTTL = this._TimePeriod * this._AutoSetMessageTtl;
          Log.Debug("[GUINotifierControl][{0}][rotatorProcessMessage] Message PTTL[{1}] init: {2}", this._Id, msg.MessagePTTL, msg.MessageText);
        }

        //Time To Live couner
        if (msg.MessageTTL > 0)
        {
          if ((msg.MessageTTL -= this._TimePeriod) < 0)
            msg.MessageTTL = 0;
        }
        //Presentation Time to Live counter
        else if (msg.MessagePTTL >= 0)
        {
          if ((msg.MessagePTTL -= this._TimePeriod) < 0)
            msg.MessagePTTL = 0;
        }

        //Mark message as read if TTL elapsed 
        if (msg.MessageTTL == 0)
        {
          //Mark the message as read
          this._Service.MessageSetStatus(NotifyMessageStatusEnum.Read, msg.MessageId);

          Log.Debug("[GUINotifierControl][{0}][rotatorProcessMessage] Message marked as Read: {1}", this._Id, msg.MessageText);
        }
        //Mark message as shown if PTTL elapsed 
        else if (msg.MessagePTTL == 0)
        {
          //Mark the message as shown
          this._Service.MessageSetStatus(NotifyMessageStatusEnum.Shown, msg.MessageId);

          Log.Debug("[GUINotifierControl][{0}][rotatorProcessMessage] Message marked as Shown: {1}", this._Id, msg.MessageText);
        }

        bResult = true; //show the message
      }


      //Increase idx to the next message
      this._MessageIdx++;

      return bResult;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void rotatotrUpdateMode()
    {
      if (this._IsActive)
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          //Going to fullscreen mode

          switch (this._FullscreenVideoBehavior)
          {
            case FullscreenVideoBehaviorEnum.Sleep:
              this.rotatorPause();
              return;

            default:
            case FullscreenVideoBehaviorEnum.Default:
            case FullscreenVideoBehaviorEnum.RunWhenTvPlayback:
              if (!Player.g_Player.IsTV)
              {
                this.rotatorPause();
                return;
              }
              break;

            case FullscreenVideoBehaviorEnum.RunAlways:
              break;
          }

          //To avoid endless message scrolling on the screen, preselect message time to live (TTL)
          this.setAutoSetMessageTtl(2); //show twice; should be enough
        }
        else
        {
          //Escaping fullscreen mode
          this.setAutoSetMessageTtl(0); //disable
          this.rotatorResume();
        }
      }
    }

    private void rotatorProcess()
    {
      INotifyMessage msg = null;
      this._TimePeriod = _TIME_MIN_WAIT;

      while (!this._Terminate)
      {
        this._FlagWakeUp.Reset();

        lock (this._MessageList)
        {
          if (!this._MessengerActive || this._Paused || this._TimePeriod == 0)
            this._IsSleeping = true;

          if (this._Terminate)
            break; //terminaton
        }

        if (this._IsSleeping)
        {
          Log.Debug("[GUINotifierControl][{0}][rotatorProcess] Sleeping...", this._Id);
          this._FlagWakeUp.WaitOne();

          //If new message has been added while showing single message infinitely then move to the new message
          if (this._MessageStatus == Status.MessageActive && this._MessagesToShowCount > 1 && this._MessageIdx == 0)
            this._MessageIdx++;
        }
        else
        {
          this._TimePeriod = Math.Max(_TIME_MIN_WAIT, this._TimePeriod);
          this._FlagWakeUp.WaitOne(this._TimePeriod);
        }

        this._IsSleeping = false;

        if (this._Terminate)
          break; //terminaton

        if (this._Paused)
          continue;

        //Keep GUI animation
        GUIGraphicsContext.ResetLastActivity();

        lock (this._MessageList)
        {
          switch (this._MessageStatus)
          {
            case Status.Off:
              if (this._IsAnyMessageToShow)
                break; //activate new msg
              else
              {
                this._MessengerActive = false; //we are going to sleep
                this._MessageIdx = 0;
                continue;
              }

            case Status.MessageActive:
              //Message presentation elapsed
              if (this.isMessageToShowSingle(msg))
              {
                //Single message; do not rotate; just stay on
                if (msg.MessageTTL < 0 && msg.MessagePTTL < 0 && this._AutoSetMessageTtl < 1)
                  this._TimePeriod = 0; //infinite time; go to sleep
                else
                {
                  this._TimePeriod = this.getMessagePeriod(msg);

                  //Decrease TTL; delete the message if needed
                  if (!this.rotatorProcessMessage(msg))
                    goto nxt; //expired
                }

                this._MessageIdx = 0;

                continue; //continue to show the current message
              }

            nxt:
              if (this._IsAnyMessageToShow)
              {
                //Close current message and wait a little
                this._TimePeriod = _TIME_MIN_WAIT;
                this._MessageStatus = Status.MessageClosing;
                //this.deactivateMessage();
                continue;
              }
              else
                goto deactivate; //no more messages


            case Status.MessageClosing:
              //Safe period elapsed
              msg = null;

              if (!this._IsAnyMessageToShow)
                goto deactivate; //no more messages
              else
                break; //activate new msg

            default:
              throw new ArgumentOutOfRangeException("MessageStatus", this._MessageStatus.ToString());
          }
        }

        //Activate new message
        lock (this._MessageList)
        {
          do
          {
            if (!this._IsAnyMessageToShow)
              goto deactivate;

            if (this._MessageIdx >= this._MessageList.Count)
              this._MessageIdx = 0;

            //Current message
            msg = this._MessageList[this._MessageIdx];

            //Dialog
            if (msg.DialogMode != NotifyMessageDialogModeEnum.None)
            {
              //Show notify dialog
              GUIMessage guiMsg = new GUIMessage()
              {
                Message = GUIMessage.MessageType.GUI_MSG_NOTIFY,
                Label = msg.Origin,
                Label2 = msg.Title,
                Label3 = String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin, msg.OriginLogo)
              };

              GUIWindowManager.SendThreadMessage(guiMsg);

              //Remove dialog mode
              if (msg.DialogMode == NotifyMessageDialogModeEnum.ShowDialog)
                this._Service.MessageClearDialogMode(msg.MessageId);
            }

            //Presentation period
            this._TimePeriod = this.getMessagePeriod(msg) + this._RotationSpeed;

          } while (!this.rotatorProcessMessage(msg)); //Decrease TTL; delete the message if needed
        }

        this.activateNewMessage(msg);

        this._MessageStatus = Status.MessageActive;

        continue;

      deactivate:
        Log.Debug("[GUINotifierControl][{0}][rotatorProcess] Deactivated.", this._Id);
        this._TimePeriod = _TIME_MIN_IDDLE;
        this._MessageStatus = Status.Off;
        this.deactivate();
        this._MessageIdx = 0;
        msg = null;

      }

      Log.Debug("[GUINotifierControl][{0}][rotatorProcess] Terminated.", this._Id);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void rotatorInit()
    {
      this._GuiLabelMessage.Label = string.Empty;
      this._GuiLabelMessageOut.Label = string.Empty;
      this._MessageStatus = Status.Off;
      this._MessengerActive = false;
      this._MessageIdx = 0;
      this._Terminate = false;

      if (this._TimeMessageActive > 0)
      {
        if (this._TimeMessageActive < 1000)
          this._TimeMessageActive = 1000;
        else if (this._TimeMessageActive > 60000)
          this._TimeMessageActive = 60000;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void rotatorStart()
    {
      if (this._ThreadMessenger == null)
      {
        Log.Debug("[GUINotifierControl][{0}][rotatorStart]", this._Id);

        this.rotatorInit();

        //Activate the messenger if we have some messages
        this._MessengerActive = this._IsAnyMessageToShow;

        //Start main proccess
        this._ThreadMessenger = new Thread(new ThreadStart(() =>
        {
          this.rotatorProcess();
          //this._ThreadMessenger.Priority = ThreadPriority.AboveNormal;
        }));
        this._ThreadMessenger.Start();
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void rotatorStop()
    {
      if (this._ThreadMessenger != null)
      {
        Log.Debug("[GUINotifierControl][{0}][rotatorStop]", this._Id);

        lock (this._MessageList)
        {
          this._Terminate = true;
        }
        this._FlagWakeUp.Set();
        this._ThreadMessenger.Join();
        this._ThreadMessenger = null;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void rotatorPause()
    {
      if (!this._Paused)
      {
        Log.Debug("[GUINotifierControl][{0}][rotatorPause]", this._Id);
        this._Paused = true;
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void rotatorResume()
    {
      if (this._Paused)
      {
        Log.Debug("[GUINotifierControl][{0}][rotatorResume]", this._Id);
        this._Paused = false;
        this._FlagWakeUp.Set();
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void rotatorAddNewMessage(INotifyMessage message)
    {
      if (message == null)
      {
        Log.Warn("[GUINotifierControl][{0}][rotatorAddNewMessage] Invalid Message.", this._Id);
        return;
      }

      if (!this._IsMessageToBeShown(message))
        return;

      lock (this._MessageList)
      {
        if (!this._MessageList.Exists(m => m.MessageId.Equals(message.MessageId)))
        {
          this._MessageList.Add(message);
          if (this._ThreadMessenger != null && !this._Paused && (!this._MessengerActive || this._IsSleeping))
          {
            Log.Debug("[GUINotifierControl][{0}][AddNewMessage] Wake-Up", this._Id);
            this._MessengerActive = true;
            this._FlagWakeUp.Set();
          }
          Log.Debug("[GUINotifierControl][rotatorAddNewMessage] Message:{1}", this._Id, message.MessageId);
        }
        else
          Log.Warn("[GUINotifierControl][rotatorAddNewMessage] Message already exist:{1}", this._Id, message.MessageId);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private INotifyMessage rotatorRemoveMessage(string strMessageId)
    {
      if (string.IsNullOrWhiteSpace(strMessageId))
        return null;

      lock (this._MessageList)
      {
        INotifyMessage msg = this._MessageList.Find(p => p.MessageId.Equals(strMessageId));

        if (msg == null)
          return null;

        this._MessageList.Remove(msg);

        //Wake up the threed if sleeping, because the current message can be the message we are deleting
        if (this._IsSleeping)
          this._FlagWakeUp.Set();

        return msg;
      }
    }

    #endregion
  }
}
