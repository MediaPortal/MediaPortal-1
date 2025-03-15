using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  public interface INotifyMessageService
  {
    /// <summary>
    /// Notification event
    /// </summary>
    event NotifyMessageServiceEventHandler NotifyEvent;

    /// <summary>
    /// Register a new message.
    /// </summary>
    /// <param name="message">Message to be registerd. A new message ID is assigned when the operation is complete.</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageRegister(INotifyMessage message, out string strMessageID);

    /// <summary>
    /// Register a new message.
    /// </summary>
    /// <param name="strTitle">Title of the message</param>
    /// <param name="strOrigin">Origin(source) of the message</param>
    /// <param name="iPluginId">Plugin ID associated to the message</param>
    /// <param name="dtPublishDate">Publish date of the message</param>
    /// <param name="strMessageID">Unique ID assigned to the message</param>
    /// <param name="strOriginLogo">Optional logo(icon) of the message</param>
    /// <param name="strThumb">Optional thumb image of the message</param>
    /// <param name="strDescription">Optional description of the message</param>
    /// <param name="strAuthor">Optional author of the message</param>
    /// <param name="level">Optional Message level</param>
    /// <param name="cls">Optional Message class</param>
    /// <param name="dialog">Optional dialog mode</param>
    /// <param name="iTtl">Optional TimeToLive of the message in a seconds. If the value > 0, the message is removed from the presentation</param>
    /// <param name="bActivatePlugin">Optional: if true, then upon clicking the message on the GUI, the associated window plugin is activated</param>
    /// <param name="strPluginArgs">Optional plugin arguments to be used for activation of the associated window plugin</param>
    /// <param name="strTag">Optional tag assigned to the message</param>
    /// <param name="strMediaLink">Optional link for direct media playback</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageRegister(string strTitle, string strOrigin, int iPluginId, DateTime dtPublishDate, out string strMessageID,
              string strOriginLogo = null, string strThumb = null, string strDescription = null, string strAuthor = null,
              NotifyMessageLevelEnum level = NotifyMessageLevelEnum.Information, NotifyMessageClassEnum cls = NotifyMessageClassEnum.General,
              NotifyMessageDialogModeEnum dialog = NotifyMessageDialogModeEnum.None, int iTtl = -1,
              bool bActivatePlugin = false, string strPluginArgs = null, string strTag = null, string strMediaLink = null);


    /// <summary>
    /// Unregister existing message.
    /// </summary>
    /// <param name="strMessageID">ID of the message to be unregistered.</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageUnregister(string strMessageID);


    /// <summary>
    /// Unregister all messages.
    /// </summary>
    /// <param name="pluginIds">Optional Plugin ID filter</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageUnregisterAll(IEnumerable<int> pluginIds = null);





    /// <summary>
    /// Clear dialog mode
    /// </summary>
    /// <param name="strMessageID">ID of the message to be processed</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageClearDialogMode(string strMessageID);

    /// <summary>
    /// Set status of the existing message.
    /// </summary>
    /// /// <param name="status">Status of the message.</param>
    /// <param name="strMessageID">ID of the message to be marked.</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageSetStatus(NotifyMessageStatusEnum status, string strMessageID);

    /// <summary>
    /// Mark all messages as read.
    /// </summary>
    /// /// <param name="status">Status of the message.</param>
    /// <param name="pluginIds">Optional Plugin ID filter</param>
    /// <returns>Result of the operation.</returns>
    NotifyMessageResult MessageSetStatusAll(NotifyMessageStatusEnum status, IEnumerable<int> pluginIds = null);


    /// <summary>
    /// Get all messages based on given arguments.
    /// </summary>
    /// <param name="cls">Optional Message class filter</param>
    /// <param name="level">Optional Message level filter</param>
    /// <param name="pluginIds">Optional Plugin ID filter</param>
    /// <returns>List of messages.</returns>
    List<INotifyMessage> MessageGetAll(NotifyMessageClassEnum cls = NotifyMessageClassEnum.All, NotifyMessageLevelEnum level = NotifyMessageLevelEnum.Information, IEnumerable<int> pluginIds = null);

    /// <summary>
    /// Get number of all messages
    /// </summary>
    int CountAll
    { get; }

    /// <summary>
    /// Get number of all read messages
    /// </summary>
    int CountRead
    { get; }

    /// <summary>
    /// Get number of all unread messages
    /// </summary>
    int CountUnread
    { get; }
  }
}
