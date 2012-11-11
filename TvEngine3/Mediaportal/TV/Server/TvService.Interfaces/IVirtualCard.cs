using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces
{
  public interface IVirtualCard
  {
    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    [DataMember]
    IUser User { get; }

    /// <summary>
    /// returns the card id of this virtual card
    /// </summary>
    [DataMember]
    int Id { get; }

    /// <summary>
    /// gets the ip adress of the tvservice
    /// </summary>
    [DataMember]
    string RemoteServer { get; set; }

    ///<summary>
    /// Gets/Set the recording format
    ///</summary>
    [DataMember]
    int RecordingFormat { get; set; }

    /// <summary>
    /// gets/sets the recording folder for the card
    /// </summary>
    [DataMember]
    string RecordingFolder { get; set; }

    /// <summary>
    /// gets/sets the timeshifting folder for the card
    /// </summary>
    [DataMember]
    string TimeshiftFolder { get; set; }

    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <value>cardtype</value>
    [DataMember]
    CardType Type { get; }

    /// <summary>
    /// Gets the name 
    /// </summary>
    /// <returns>name of card</returns>
    [DataMember]
    string Name { get; }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <returns>filename of the recording or null when not recording</returns>
    [DataMember]
    string RecordingFileName { get; }

    

    /// <summary>
    /// returns which schedule is currently being recorded
    /// </summary>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    [DataMember]
    int RecordingScheduleId { get; }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>
    [DataMember]
    string RTSPUrl { get; }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    [XmlIgnore]
    bool GrabTeletext { set; }

    /// <summary>
    /// Returns if the current channel has teletext or not
    /// </summary>
    /// <returns>yes if channel has teletext otherwise false</returns>
    [DataMember]
    bool HasTeletext { get; }

    /// <summary>
    /// Returns if we arecurrently grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg otherwise false</returns>
    [DataMember]
    bool IsGrabbingEpg { get; set; }

    /// <summary>
    /// Returns if card is currently recording or not
    /// </summary>
    /// <returns>true when card is recording otherwise false</returns>
    [DataMember]
    bool IsRecording { get; set; }

    /// <summary>
    /// Returns if card is currently scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>
    [DataMember]
    bool IsScanning { get; set; }

    /// <summary>
    /// Returns whether the current channel is scrambled or not.
    /// </summary>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    [DataMember]
    bool IsScrambled { get; }

    /// <summary>
    /// Returns if card is currently timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    [DataMember]
    bool IsTimeShifting { get; }

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <returns>timeshifting filename null when not timeshifting</returns>
    [DataMember]
    string TimeShiftFileName { get; }

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <returns>true if tuner is locked otherwise false</returns>
    [DataMember]
    bool IsTunerLocked { get; }

    /// <summary>
    /// Gets the name of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>
    [DataMember]
    string ChannelName { get; }


    /// <summary>
    /// returns the database channel
    /// </summary>
    /// <returns>int</returns>
    [DataMember]
    int IdChannel { get; }

    /// <summary>
    /// Returns the signal level 
    /// </summary>
    /// <returns>signal level (0-100)</returns>
    [XmlIgnore]
    int SignalLevel { get; }

    /// <summary>
    /// Returns the signal quality 
    /// </summary>
    /// <returns>signal quality (0-100)</returns>
    [XmlIgnore]
    int SignalQuality { get; }

    /// <summary>
    /// Gets/Sts the quality type
    /// </summary>
    QualityType QualityType { get; set; }

    /// <summary>
    /// Gets/Sts the bitrate mode
    /// </summary>
    VIDEOENCODER_BITRATE_MODE BitRateMode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    int NrOfOtherUsersTimeshiftingOnCard { get; set; }

    MediaTypeEnum? MediaType { get; }


    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>    
    void GetStreamQualityCounters(out int totalTSpackets, out int discontinuityCounter);

    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>byte[] array containing the raw teletext page or null if page is not found</returns>
    byte[] GetTeletextPage(int pageNumber, int subPageNumber);

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    void StopTimeShifting();

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    void StopRecording();

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns>true if success otherwise false</returns>
    TvResult StartRecording(ref string fileName);

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>number of teletext subpages for the pagenumber</returns>
    int SubPageCount(int pageNumber);

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <returns>Teletext pagenumber for the red button</returns>
    int GetTeletextRedPageNumber();

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <returns>Teletext pagenumber for the green button</returns>
    int GetTeletextGreenPageNumber();

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    int GetTeletextYellowPageNumber();

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <returns>Teletext pagenumber for the blue button</returns>
    int GetTeletextBluePageNumber();

    /// <summary>f
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    TimeSpan TeletextRotation(int pageNumber);

    /// <summary>
    /// Indicates, if the user is the owner of the card
    /// </summary>
    /// <returns>true/false</returns>
    bool IsOwner();

    /// <summary>
    /// Indicates, if the card supports quality control
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsQualityControl();

    /// <summary>
    /// Indicates, if the card supports bit rates
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsBitRate();

    /// <summary>
    /// Indicates, if the card supports bit rate modes 
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsBitRateModes();

    /// <summary>
    /// Indicates, if the card supports bit rate peak mode
    /// </summary>
    /// <returns>true/false</returns>
    bool SupportsPeakBitRateMode();

    /// <summary>
    /// Indicates, if the card supports CI Menu
    /// </summary>
    /// <returns>true/false</returns>
    bool CiMenuSupported();

    /// <summary>
    /// Enters the CI Menu for current card
    /// </summary>
    /// <returns>true if successful</returns>
    bool EnterCiMenu();

    /// <summary>
    /// Selects a ci menu entry
    /// </summary>
    /// <param name="choice">Choice (1 based), 0 for "back"</param>
    /// <returns>true if successful</returns>
    bool SelectCiMenu(byte choice);

    /// <summary>
    /// Closes the CI Menu for current card
    /// </summary>
    /// <returns>true if successful</returns>
    bool CloseMenu();

    /// <summary>
    /// Sends an answer to CAM after a request
    /// </summary>
    /// <param name="cancel">cancel request</param>
    /// <param name="answer">answer string</param>
    /// <returns>true if successful</returns>
    bool SendMenuAnswer(bool cancel, string answer);

    /// <summary>
    /// Sets a callback handler
    /// </summary>
    /// <param name="callbackHandler"></param>
    /// <returns></returns>
    bool SetCiMenuHandler(ICiMenuCallbacks callbackHandler);
  }
}