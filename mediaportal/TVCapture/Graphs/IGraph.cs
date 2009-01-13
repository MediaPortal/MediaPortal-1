#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Drawing;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
  public enum NetworkType
  {
    Unknown,
    Analog, // Analog TV
    ATSC, // ATSC
    DVBC, // DVB-cable
    DVBS, // DVB-Sattelite
    DVBT // DVB-Terrestial
  }

  /// <summary>
  /// Interface definition for graphbuilder objects
  /// A graphbuilder object supports one or more TVCapture cards and
  /// contains all the code/logic necessary todo
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// 
  /// Currently there are 2 implementations of IGraph
  /// Sinkgraph.cs      : support for cards with an onboard MPEG2 encoder like the hauppauge PVR 250/350/USB2
  /// SWEncodingGraph.cs: support for cards without an onboard mpeg2 encoder like the pinnacle PCTV Pro, WinTV,...
  /// 
  /// NOTE!!! IGraph instances are created by the GraphFactory.cs  
  ///   
  /// Basicly the flow is
  /// 
  /// for viewing:
  ///   CreateGraph()
  ///   if SupportsTimeshifting() then 
  ///     StartTimeshifting()
  ///     ....
  ///     StopTimeshifting()
  ///     DeleteGraph()
  ///   else
  ///     StartViewing
  ///     ...
  ///     StopViewing()
  ///     DeleteGraph()
  ///     
  /// for recording:    
  ///   CreateGraph()
  ///   if SupportsTimeshifting() then 
  ///     StartTimeshifting()
  ///     StartRecording()
  ///     .....
  ///     StopRecording()
  ///     DeleteGraph()
  ///   else
  ///     StartRecording()
  ///     .....
  ///     StopRecording()
  ///     DeleteGraph()
  ///     
  ///     
  /// </summary>
  /// <seealso cref="MediaPortal.TV.Recording.GraphFactory"/>
  public interface IGraph
  {
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    bool CreateGraph(int Quality);

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    void DeleteGraph();

    /// <summary>
    /// Starts timeshifting the TV channel and stores the timeshifting 
    /// files in the specified filename
    /// </summary>
    /// <param name="channel">TV channel to which card should be tuned</param>
    /// <param name="strFileName">Filename for the timeshifting buffers</param>
    /// <returns>boolean indicating if timeshifting is running or not</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    bool StartTimeShifting(TVChannel channel, string strFileName);

    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    bool StopTimeShifting();


    /// <summary>
    /// Starts recording live TV to a file
    /// <param name="channel">TV channel to which card should be tuned</param>
    /// <param name="strFileName">filename for the new recording</param>
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// <param name="timeProgStart">Contains the starttime of the current tv program</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the timeProgStart till the moment recording is stopped again
    /// </remarks>
    bool StartRecording(Hashtable attribtutes, TVRecording recording, TVChannel channel, ref string strFileName,
                        bool bContentRecording, DateTime timeProgStart);


    /// <summary>
    /// Stops recording 
    /// </summary>
    /// <remarks>
    /// Graph should be recording. When Recording is stopped the graph is still 
    /// timeshifting
    /// </remarks>
    void StopRecording();


    /// <summary>
    /// Switches / tunes to another TV channel
    /// </summary>
    /// <param name="channel">TV channel to which card should be tuned</param>
    /// <remarks>
    /// Graph should be timeshifting. 
    /// </remarks>
    void TuneChannel(TVChannel channel);

    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
    int GetChannelNumber();

    /// <summary>
    /// Property indiciating if the graph supports timeshifting
    /// </summary>
    /// <returns>boolean indiciating if the graph supports timeshifting</returns>
    bool SupportsTimeshifting();


    /// <summary>
    /// Starts viewing the TV channel 
    /// </summary>
    /// <param name="channel">TV channel to which card should be tuned</param>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    bool StartViewing(TVChannel channel);


    /// <summary>
    /// Stops viewing the TV channel 
    /// </summary>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be viewing first with StartViewing()
    /// </remarks>
    bool StopViewing();

    /// <summary>
    /// This method can be used to ask the graph if it should be rebuild when
    /// we want to tune to the new channel:ichannel
    /// </summary>
    /// <param name="iChannel">new channel to tune to</param>
    /// <returns>true : graph needs to be rebuild for this channel
    ///          false: graph does not need to be rebuild for this channel
    /// </returns>
    bool ShouldRebuildGraph(TVChannel newChannel);

    /// <summary>
    /// This method returns whether a signal is present. Meaning that the
    /// TV tuner (or video input) is tuned to a channel
    /// </summary>
    /// <returns>true:  tvtuner is tuned to a channel (or video-in has a video signal)
    ///          false: tvtuner is not tuned to a channel (or video-in has no video signal)
    /// </returns>
    bool SignalPresent();

    /// <summary>
    /// returns the signal quality in 0%-100%
    /// </summary>
    /// <returns></returns>
    int SignalQuality();

    /// <summary>
    /// returns the signal strength in dB
    /// </summary>
    /// <returns></returns>
    int SignalStrength();

    /// <summary>
    /// This method returns the frequency to which the tv tuner is currently tuned
    /// </summary>
    /// <returns>frequency in Hertz
    /// </returns>
    long VideoFrequency();

    /// <summary>
    /// MP will call this function on a regular basis
    /// It allows the tv card todo any progressing like
    /// getting/updating the EPG, channel information etc (for DVB cards)
    /// </summary>
    void Process();

    /// <summary>
    /// returns a collection of property pages for tvcapture card
    /// </summary>
    /// <returns></returns>
    PropertyPageCollection PropertyPages();

    bool SupportsFrameSize(Size framesize);


    // returns card network type (analog, dvb-c, dvb-t, dvb-s, atsc)
    NetworkType Network();

    // tune to a specified frequency
    void Tune(object tuningObject, int disecqNo);

    // scan radio/tv channels and store them in the database
    void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels,
                       ref int newRadioChannels, ref int updatedRadioChannels);


    // tune Start listening to radio
    void StartRadio(RadioStation station);
    void StopRadio();


    // tune to the specified radio station
    void TuneRadioChannel(RadioStation station);

    // tune to the specified radio Frequency
    void TuneRadioFrequency(int frequency);

    //returns true if card can decode teletext and when current tv channel has teletext
    bool HasTeletext();

    // get the current audio language
    int GetAudioLanguage();

    //select an audio language
    void SetAudioLanguage(int audioPid);

    // get a list of the audio languages present in current tv show
    ArrayList GetAudioLanguageList();

    //return the timeshift filename for tv
    string TvTimeshiftFileName();

    //return the timeshift filename for radio
    string RadioTimeshiftFileName();

    //tell capture card to grab & decode teletext and give it to the TeletextGrabber class
    void GrabTeletext(bool yesNo);

    //Get the Maximum and Minimun channel for radio stations
    void RadioChannelMinMax(out int chanmin, out int chanmax);

    //Get the Maximum and Minimun channel for TV stations
    void TVChannelMinMax(out int chanmin, out int chanmax);

    IBaseFilter AudiodeviceFilter();
    bool IsTimeShifting();
    bool IsEpgGrabbing();
    bool IsEpgDone();
    bool GrabEpg(TVChannel chan);
    bool StopEpgGrabbing();
    bool SupportsHardwarePidFiltering();
    bool Supports5vAntennae();
    bool SupportsCamSelection();
    bool CanViewTimeShiftFile();
    bool IsRadio();
    bool IsRecording();
    string LastError();
  }
}