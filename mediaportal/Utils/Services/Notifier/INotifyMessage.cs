using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  public interface INotifyMessage
  {
    #region Mandatory items

    /// <summary>
    /// Unique ID assigned to the message. Assigned automatically upon registration of the message.
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Plugin ID associated to the message.
    /// </summary>
    int PluginId { get; }

    /// <summary>
    /// Origin(source) of the message.
    /// </summary>
    string Origin { get; }

    /// <summary>
    /// Title of the message.
    /// </summary>
    string Title { get; }

    #endregion

    #region Optional items        

    /// <summary>
    /// Optional description of the message.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Optional author of the message.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Optional dialog mode.
    /// </summary>
    NotifyMessageDialogModeEnum DialogMode { get; }

    /// <summary>
    /// If true, then upon clicking the message on the GUI, the associated window plugin is activated.
    /// </summary>
    bool ActivatePluginWindow { get; }

    /// <summary>
    /// Optional plugin arguments to be used for activation of the associated window plugin.
    /// </summary>
    string PluginArguments { get; }

    /// <summary>
    /// If true, the message is automatically deleted from the database after the presentation of the message.
    /// </summary>
    bool DeleteMessageAfterPresentation { get; }

    /// <summary>
    /// Optional logo(icon) of the message.
    /// </summary>
    string OriginLogo { get; }

    /// <summary>
    /// Optional thumb image of the message.
    /// </summary>
    string Thumb { get; }

    /// <summary>
    /// Optional publish date of the message.
    /// </summary>
    DateTime PublishDate { get; }

    /// <summary>
    /// Optional TimeToLive of the message in a seconds. If the value > 0, the message is removed from the presentation
    /// when the value decreases to zero. Otherwise the message will be presented endlessly.
    /// </summary>
    int MessageTTL { get; set; }

    /// <summary>
    /// Optional tag assigned to the message.
    /// </summary>
    string Tag { get; }

    /// <summary>
    /// Status of the message.
    /// </summary>
    NotifyMessageStatusEnum Status { get;}

    /// <summary>
    /// Creation timestamp of the message.
    /// </summary>
    DateTime TimeStamp { get; }

    /// <summary>
    /// Optional Message class
    /// </summary>
    NotifyMessageClassEnum Class { get;}

    /// <summary>
    /// Optional Message level
    /// </summary>
    NotifyMessageLevelEnum Level { get; }

    /// <summary>
    /// Text of the message (for internal purpose)
    /// </summary>
    string MessageText { get; }

    /// <summary>
    /// Link for direct media playback
    /// </summary>
    string MediaLink { get; }

    /// <summary>
    /// Internal presentation Time to Live used in video fullscreen mode
    /// </summary>
    int MessagePTTL { get; set; }

    #endregion;

  }
}
