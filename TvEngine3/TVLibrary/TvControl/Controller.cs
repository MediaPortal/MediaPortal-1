using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
namespace TvControl
{
  /// <summary>
  /// Types of cards
  /// </summary>
  public enum CardType
  {
    Analog,
    DvbS,
    DvbT,
    DvbC,
    Atsc
  }
  public enum TvResult
  {
    Succeeded,
    AllCardsBusy,
    ChannelIsScrambled,
    NoVideoAudioDetected,
    UnknownError,
    UnableToStartGraph
  }
  /// <summary>
  /// interface class describing all methods available
  /// to remote-control the TVService
  /// </summary>
  public interface IController
  {
    #region internal interface
    ///<summary>
    ///Gets the total number of tv-cards installed.
    ///</summary>
    ///<value>Number which indicates the cards installed</value>
    int Cards { get;}

    /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <value>id of card</value>
    int CardId(int cardIndex);

    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <value>cardtype</value>
    CardType Type(int cardId);

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>name of card</returns>
    string CardName(int cardId);

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    bool CanTune(int cardId, IChannel channel);

    /// <summary>
    /// Gets the device path for a card.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>devicePath of card</returns>
    string CardDevice(int cardId);

    /// <summary>
    /// Gets the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>IChannel</returns>
    IChannel CurrentChannel(int cardId);

    /// <summary>
    /// Gets the name of the tv/radio channel on which the card is currently tuned
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>channel name</returns>
    string CurrentChannelName(int cardId);

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    bool IsScrambled(int cardId);

    /// <summary>
    /// Tune the the specified card to the channel and run the graph
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded</returns>
    bool TuneScan(int cardId, IChannel channel);

    /// <summary>
    /// Tune the the specified card to the channel.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded</returns>
    bool Tune(int cardId, IChannel channel);

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    bool TunerLocked(int cardId);

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    int SignalQuality(int cardId);

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>signal level (0-100)</returns>
    int SignalLevel(int cardId);


    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>filename of the recording or null when not recording</returns>
    string FileName(int cardId);

    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>timeshifting filename null when not timeshifting</returns>
    string TimeShiftFileName(int cardId);

    /// <summary>
    /// Returns if the card is currently timeshifting or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    bool IsTimeShifting(int cardId);

    /// <summary>
    /// Returns if the card is currently recording or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true when card is recording otherwise false</returns>
    bool IsRecording(int cardId);


    /// <summary>
    /// Returns if the card is currently scanning for channels or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    bool IsScanning(int cardId);

    /// <summary>
    /// Returns if the card is currently grabbing the epg or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    bool IsGrabbingEpg(int cardId);

    /// <summary>
    /// Returns if the card is currently grabbing teletext or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    bool IsGrabbingTeletext(int cardId);

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    TimeSpan TeletextRotation(int cardId, int pageNumber);

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    bool HasTeletext(int cardId);

    /// <summary>
    /// turn on/off teletext grabbing for a card
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns">onOff when true start grabbing teletext otherwise stop grabbing teletext</returns>
    void GrabTeletext(int cardId, bool onOff);

    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>byte[] array containing the raw teletext page or null if page is not found</returns>
    byte[] GetTeletextPage(int cardId, int pageNumber, int subPageNumber);

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>number of teletext subpages for the pagenumber</returns>
    int SubPageCount(int cardId, int pageNumber);

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>true if success otherwise false</returns>
    TvResult StartTimeShifting(int cardId, string fileName);

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true if success otherwise false</returns>
    bool StopTimeShifting(int cardId);

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <returns>true if success otherwise false</returns>
    bool StartRecording(int cardId, string fileName, bool contentRecording, long startTime);

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>true if success otherwise false</returns>
    bool StopRecording(int cardId);

    /// <summary>
    /// scans current transponder for channels.
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>list of all channels found</returns>
    IChannel[] Scan(int cardId, IChannel channel);


    /// <summary>
    /// Grabs the epg for the card specified
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns></returns>
    void GrabEpg(int cardId);

    /// <summary>
    /// returns the minium channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>minium channel number</returns>
    int MinChannel(int cardId);

    /// <summary>
    /// returns the maximum channel numbers for analog cards
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>maximum channel number</returns>
    int MaxChannel(int cardId);

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    DateTime TimeShiftStarted(int cardId);

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="cardId">Index of the card.</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    DateTime RecordingStarted(int cardId);

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    int GetRecordingSchedule(int cardId);
    #region audio stream selection
    /// <summary>
    /// returns the list of available audio streams for the card specified
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <returns>List containing all audio streams</returns>
    IAudioStream[] AvailableAudioStreams(int cardId);

    /// <summary>
    /// returns the current selected audio stream for the card specified
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <returns>current audio stream</returns>
    IAudioStream GetCurrentAudioStream(int cardId);


    /// <summary>
    /// Sets the current audio stream for the card specified
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="stream">audio stream</param>
    void SetCurrentAudioStream(int cardId, IAudioStream stream);
    #endregion

    #region streams
    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for the selected card
    /// </summary>
    /// <param name="cardId">card id</param
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>
    string GetStreamingUrl(int cardId);

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream for recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param
    /// <returns>URL containing the RTSP adress on which the recording can be found</returns>
    string GetRecordingUrl(int idRecording);
    #endregion

    #endregion

    #region public interface
    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="cardId">returns on which card timeshifting is started</param>
    /// <returns>true if timeshifting has started, otherwise false</returns>
    TvResult StartTimeShifting(string channelName, out VirtualCard card);

    /// <summary>
    /// Checks if the channel specified is being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="cardId">returns card is recording the channel</param>
    /// <returns>true if a card is recording the channel, otherwise false</returns>
    bool IsRecording(string channelName, out VirtualCard card);

    /// <summary>
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="cardId">returns card is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    bool IsRecordingSchedule(int idSchedule, out VirtualCard card);

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    void StopRecordingSchedule(int idSchedule);

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    void OnNewSchedule();

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    bool EpgGrabberEnabled { get;set;}

    /// <summary>
    /// Returns the SQl connection string to the database
    /// </summary>
    string DatabaseConnectionString { get;set;}
    #endregion




  }
}
