using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Services;

namespace MediaPortal.NotifyMessageService.Database
{
  internal class NotifyMessage : INotifyMessage
  {
    public NotifyMessage(int iDbId, string strMessageId, int iPluginID, string strOrigin, string strTitle, string strDescription, string strAuthor,
      NotifyMessageDialogModeEnum dialogMode, bool bActivatePluginWindow, string strPluginArgs, bool bDeleteMessage,
      string strLogo, string strThumb, DateTime dtPublish, int iTtl, string strTag, NotifyMessageStatusEnum status, DateTime ts, NotifyMessageClassEnum cls, NotifyMessageLevelEnum level,
      string strMediaLink)
    {
      this.DatabaseID = iDbId;
      this.MessageId = strMessageId;
      this.PluginId = iPluginID;
      this.Origin = strOrigin;
      this.Title = strTitle;
      this.Description = strDescription;
      this.Author = strAuthor;
      this.DialogMode = dialogMode;
      this.ActivatePluginWindow = bActivatePluginWindow || !string.IsNullOrWhiteSpace(strPluginArgs);
      this.PluginArguments = strPluginArgs;
      this.DeleteMessageAfterPresentation = bDeleteMessage;
      this.OriginLogo = strLogo;
      this.Thumb = strThumb;
      this.PublishDate = dtPublish;
      this.MessageTTL = iTtl;
      this.Tag = strTag;
      this.Status = status;
      this.TimeStamp = ts;
      this.Class = cls;
      this.Level = level;
      this.MediaLink = strMediaLink;
    }

    public NotifyMessage(string strMessageId, INotifyMessage msg)
    {
      this.MessageId = strMessageId;
      this.PluginId = msg.PluginId;
      this.Origin = msg.Origin;
      this.Title = msg.Title;
      this.Description = msg.Description;
      this.Author = msg.Author;
      this.DialogMode = msg.DialogMode;
      this.ActivatePluginWindow = msg.ActivatePluginWindow;
      this.PluginArguments = msg.PluginArguments;
      this.DeleteMessageAfterPresentation = msg.DeleteMessageAfterPresentation;
      this.OriginLogo = msg.OriginLogo;
      this.Thumb = msg.Thumb;
      this.PublishDate = msg.PublishDate;
      this.MessageTTL = msg.MessageTTL;
      this.Tag = msg.Tag;
      this.Status = msg.Status;
      this.TimeStamp = msg.TimeStamp;
      this.Class = msg.Class;
      this.Level = msg.Level;
      this.MediaLink = msg.MediaLink;
    }

    public int DatabaseID { get; }

    public string MessageId { get; set; }

    public int PluginId { get; }

    public string Origin { get; }

    public string Title { get; }

    public string Description { get; }

    public string Author { get; }

    public NotifyMessageDialogModeEnum DialogMode { get; set; }

    public bool ActivatePluginWindow { get; }

    public string PluginArguments { get; }

    public bool DeleteMessageAfterPresentation { get; }

    public string OriginLogo { get; }

    public string Thumb { get; }

    public DateTime PublishDate { get; }

    public int MessageTTL { get; set; }

    public string Tag { get; }

    public NotifyMessageStatusEnum Status { get; set; }

    public DateTime TimeStamp { get; set; }

    public NotifyMessageClassEnum Class { get; }

    public NotifyMessageLevelEnum Level { get; }

    public string MessageText
    {
      get
      {
        if (this._MessageText == null)
        {
          this._MessageText = this.Origin + ": " + this.Title;
        }

        return this._MessageText;
      }
    }private string _MessageText = null;

    public string MediaLink { get; }

    public int MessagePTTL { get; set; } = -1;
  }
}
