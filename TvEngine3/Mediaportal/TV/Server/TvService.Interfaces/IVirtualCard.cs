using System.Runtime.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
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
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="forceUpdate"><c>True</c> to force the signal status to be updated, and not use cached information.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    void GetSignalStatus(bool forceUpdate, out bool isLocked, out bool isPresent, out int strength, out int quality);

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>    
    void GetStreamQualityCounters(out int totalTSpackets, out int discontinuityCounter);

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

    #region quality control

    /// <summary>
    /// Determine which (if any) quality control features are supported by a tuner.
    /// </summary>
    /// <param name="supportedEncodeModes">The encoding modes supported by the tuner.</param>
    /// <param name="canSetBitRate"><c>True</c> if the tuner's average and/or peak encoding bit-rate can be set.</param>
    void GetSupportedQualityControlFeatures(out EncodeMode supportedEncodeModes, out bool canSetBitRate);

    /// <summary>
    /// Get and/or set the tuner's video and/or audio encoding mode.
    /// </summary>
    EncodeMode EncodeMode { get; set; }

    /// <summary>
    /// Get and/or set the tuner's average video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    int AverageBitRate { get; set; }

    /// <summary>
    /// Get and/or set the tuner's peak video and/or audio bit-rate, encoded as a percentage over the supported range.
    /// </summary>
    int PeakBitRate { get; set; }

    #endregion

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
  }
}