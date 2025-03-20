using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using MediaPortal.Services;

namespace MediaPortal.NotifyMessageService.Database
{
  public class NotifyMessageService : INotifyMessageService
  {
    private NotifyMessageServiceSqLite _SqLite;
    private List<NotifyMessage> _Messages;

    public event NotifyMessageServiceEventHandler NotifyEvent;


    public NotifyMessageService()
    {
      this._SqLite = new NotifyMessageServiceSqLite();
      this._Messages = this._SqLite.GetMessages();
    }


    [MethodImpl(MethodImplOptions.Synchronized)]
    public List<INotifyMessage> MessageGetAll(NotifyMessageClassEnum cls = NotifyMessageClassEnum.All, NotifyMessageLevelEnum level = NotifyMessageLevelEnum.Information, IEnumerable<int> pluginIds = null)
    {
      return this._Messages.Where(m =>
       (pluginIds == null || pluginIds.Any(id => id == m.PluginId)) &&
       (cls == NotifyMessageClassEnum.All ||
       (cls == NotifyMessageClassEnum.General && m.Class == NotifyMessageClassEnum.General) ||
       (m.Class & cls) != 0) && m.Level >= level).Cast<INotifyMessage>().ToList();
    }



    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageSetStatus(NotifyMessageStatusEnum status, string strMessageID)
    {
      if (!string.IsNullOrWhiteSpace(strMessageID))
      {
        NotifyMessage msg = this._Messages.Find(m => m.MessageId.Equals(strMessageID));
        if (msg != null)
        {
          if (msg.Status != status)
          {
            if (this._SqLite.MessageSetStatus(strMessageID, status))
            {
              msg.Status = status;
              if (this.NotifyEvent != null)
              {
                try
                {
                  this.NotifyEvent(this, new NotifyMessageServiceEventArgs()
                  {
                    EventType = NotifyMessageServiceEventTypeEnum.MessageStatusChanged,
                    Message = msg
                  });
                }
                catch { }
              }

              return NotifyMessageResult.OK;
            }
            else
              return NotifyMessageResult.Failed;
          }
          else
            return NotifyMessageResult.NoAction;
        }
      }

      return NotifyMessageResult.InvalidMessage;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageSetStatusAll(NotifyMessageStatusEnum status, IEnumerable<int> pluginIds = null)
    {
      for (int i = 0; i < this._Messages.Count; i++)
      {
        NotifyMessage msg = this._Messages[i];

        if (msg.Status != status && (pluginIds == null || pluginIds.Any(iId => msg.PluginId == iId)))
        {
          if (this._SqLite.MessageSetStatus(msg.MessageId, status))
          {
            msg.Status = NotifyMessageStatusEnum.Read;
            if (this.NotifyEvent != null)
            {
              try
              {
                this.NotifyEvent(this, new NotifyMessageServiceEventArgs()
                {
                  EventType = NotifyMessageServiceEventTypeEnum.MessageStatusChanged,
                  Message = msg
                });
              }
              catch { }
            }
          }
          else
            return NotifyMessageResult.Failed;
        }
      }

      return NotifyMessageResult.OK;
    }


    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageRegister(INotifyMessage message, out string strMessageID)
    {
      strMessageID = null;
      if (message != null)
      {
        strMessageID = message.PluginId.ToString() + ':' + Guid.NewGuid().ToString();

        //Check for existing message
        if (this._Messages.Exists(m => m.PluginId == message.PluginId && m.PublishDate == message.PublishDate && m.Title == message.Title))
          return NotifyMessageResult.AlreadyExists;

        NotifyMessage msg;
        if (message is NotifyMessage)
        {
          msg = (NotifyMessage)message;
          msg.MessageId = strMessageID;
        }
        else
          msg = new NotifyMessage(strMessageID, message);

        if (msg.MessageTTL <= 0)
          msg.MessageTTL = -1;
        else if (msg.MessageTTL < 5000)
          msg.MessageTTL = 5000;

        msg.TimeStamp = DateTime.Now;

        if (this._SqLite.MessageCreate(strMessageID, msg) > 0)
        {
          
          this._Messages.Add(msg);

          if (this.NotifyEvent != null)
          {
            try
            {
              this.NotifyEvent(this, new NotifyMessageServiceEventArgs()
              {
                EventType = NotifyMessageServiceEventTypeEnum.MessageRegistered,
                Message = msg,
              });
            }
            catch { }
          }

          return NotifyMessageResult.OK;
        }
        else
          return NotifyMessageResult.Failed;
      }

      return NotifyMessageResult.InvalidMessage;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageRegister(string strTitle, string strOrigin, int iPluginId, DateTime dtPublishDate, out string strMessageID,
          string strOriginLogo = null, string strThumb = null, string strDescription = null, string strAuthor = null,
          NotifyMessageLevelEnum level = NotifyMessageLevelEnum.Information, NotifyMessageClassEnum cls = NotifyMessageClassEnum.General,
          NotifyMessageDialogModeEnum dialog = NotifyMessageDialogModeEnum.None, int iTtl = -1,
          bool bActivatePlugin = false, string strPluginArgs = null, string strTag = null, string strMediaLink = null)
    {
      return this.MessageRegister(new NotifyMessage(-1, null, iPluginId, strOrigin, strTitle, strDescription, strAuthor, dialog,
        bActivatePlugin, strPluginArgs, false, strOriginLogo, strThumb, dtPublishDate, iTtl, strTag, NotifyMessageStatusEnum.Unread, DateTime.Now, cls, level, strMediaLink), out strMessageID);
    }


    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageUnregister(string strMessageID)
    {
      if (!string.IsNullOrWhiteSpace(strMessageID))
      {
        for (int i = 0; i < this._Messages.Count; i++)
        {
          NotifyMessage msg = this._Messages[i];

          if (msg.MessageId.Equals(strMessageID) && this._SqLite.MessageDelete(strMessageID))
          {
            this._Messages.RemoveAt(i);

            if (this.NotifyEvent != null)
            {
              try
              {
                this.NotifyEvent(this, new NotifyMessageServiceEventArgs()
                {
                  EventType = NotifyMessageServiceEventTypeEnum.MessageUnregistered,
                  Message = msg
                });
              }
              catch { }
            }

            return NotifyMessageResult.OK;
          }
        }
      }

      return NotifyMessageResult.InvalidMessage;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageUnregisterAll(IEnumerable<int> pluginIds = null)
    {
      for (int i = this._Messages.Count - 1; i >= 0; i--)
      {
        NotifyMessage msg = this._Messages[i];

        if (pluginIds == null || pluginIds.Any(iId => msg.PluginId == iId))
        {
          this._SqLite.MessageDelete(msg.MessageId);
          {
            this._Messages.RemoveAt(i);

            if (this.NotifyEvent != null)
            {
              try
              {
                this.NotifyEvent(this, new NotifyMessageServiceEventArgs()
                {
                  EventType = NotifyMessageServiceEventTypeEnum.MessageUnregistered,
                  Message = msg
                });
              }
              catch { }
            }
          }
        }
      }

      return NotifyMessageResult.OK;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public NotifyMessageResult MessageClearDialogMode(string strMessageID)
    {
      if (!string.IsNullOrWhiteSpace(strMessageID))
      {
        NotifyMessage msg = this._Messages.Find(m => m.MessageId.Equals(strMessageID));
        if (msg != null)
        {
          if (msg.DialogMode != NotifyMessageDialogModeEnum.None)
          {
            if (this._SqLite.MessageClearDialogMode(strMessageID))
            {
              msg.DialogMode = NotifyMessageDialogModeEnum.None;
              if (this.NotifyEvent != null)
              {
                try
                {
                  this.NotifyEvent(this, new NotifyMessageServiceEventArgs()
                  {
                    EventType = NotifyMessageServiceEventTypeEnum.MessageDialogModeCleared,
                    Message = msg
                  });
                }
                catch { }
              }

              return NotifyMessageResult.OK;
            }
            else
              return NotifyMessageResult.Failed;
          }
          else
            return NotifyMessageResult.NoAction;
        }
      }

      return NotifyMessageResult.InvalidMessage;
    }



    public int CountAll
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return this._Messages.Count(); }
    }

    public int CountRead
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return this._Messages.Count(m => m.Status == NotifyMessageStatusEnum.Read); }
    }

    public int CountUnread
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return this._Messages.Count(m => m.Status != NotifyMessageStatusEnum.Read); }
    }
  }
}
