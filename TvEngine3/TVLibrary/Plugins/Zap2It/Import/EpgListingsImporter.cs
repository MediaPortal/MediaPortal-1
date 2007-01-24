#region Copyright (C) 2005-2006 Team MediaPortal
/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
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
using System.Text;
using System.Collections.Generic;
//using MediaPortal.Database;
//using MediaPortal.TV.Database;
//using MediaPortal.GUI.Library;
using TvEngine;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Channels;
using Gentle.Common;
using Gentle.Framework;

namespace ProcessPlugins.EpgGrabber
{
    public class EpgListingsImporter : IDisposable
    {
        private const string XMLTVID = ".labs.zap2it.com";
        const int NOTFOUND = -1;

        /// <summary>
        /// Delegate for hooking into status update during import operations.
        /// </summary>
        /// <param name="stats"></param>
        public delegate void ShowProgressHandler(object sender, ImportStats stats);
        public event ShowProgressHandler ShowProgress;

        private int    _delay;
        private bool   _createChannels            = true;
        private bool   _renameChannels;
        private bool   _allowChannelNumberMapping = false;
        private bool   _sortChannels              = false;
        private string _channelNameTemplate       = "{channel} {callsign}";
        private TvLibrary.Country _ExternalInputCountry = null;
        private TvLibrary.Implementations.AnalogChannel.VideoInputType _ExternalInput = TvLibrary.Implementations.AnalogChannel.VideoInputType.SvhsInput1;

        private Dictionary<string, object> _mpEpgMappingCache = new Dictionary<string, object>();
        private Dictionary<int, ATSCChannel> _mpAtscChannelCache = new Dictionary<int, ATSCChannel>();

        private Zap2it.SoapEntities.DownloadResults _results;

        private TvBusinessLayer tvLayer = new TvBusinessLayer();
        //private IList<Channel> _mpChannelCache;
        private System.Collections.IList _mpChannelCache;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EpgListingsImporter"/> class.
        /// </summary>
        /// <param name="results">The results.</param>
        public EpgListingsImporter(Zap2it.SoapEntities.DownloadResults results)
        {
            this._results = results;
        }

        #region Fields
        /// <summary>
        /// Gets or sets a value indicating whether the import should create channels it can't find.
        /// </summary>
        /// <value><c>true</c> if [create channels]; otherwise, <c>false</c>.</value>
        public bool CreateChannels
        {
            get { return _createChannels; }
            set { _createChannels = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rename channels.
        /// </summary>
        /// <value><c>true</c> if [rename channels]; otherwise, <c>false</c>.</value>
        public bool RenameChannels
        {
            get { return _renameChannels; }
            set { _renameChannels = value; }
        }

        /// <summary>
        /// Gets or sets the channel name template using the following variables:
        ///     {callsign}
        ///     {name}
        ///     {affiliate}
        ///     {number}
        /// </summary>
        /// <value>The channel name template.</value>
        public string ChannelNameTemplate
        {
            get { return _channelNameTemplate; }
            set { _channelNameTemplate = value; }
        }

        /// <summary>
        /// Gets or sets the delay (in ms) during import between processing each item.
        /// </summary>
        /// <value>The delay.</value>
        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether mapping by channel number only is allowed.
        /// </summary>
        /// <value><c>true</c> if [allow mapping by channel number only]; otherwise, <c>false</c>.</value>
        public bool AllowChannelNumberOnlyMapping
        {
           get { return _allowChannelNumberMapping; }
           set { _allowChannelNumberMapping = value; }
        }

        /// <summary>
        /// Gets or sets a value of the External Input Country.
        /// </summary>
        /// <value>Country for created Channels</value>
        public TvLibrary.Country ExternalInputCountry
        {
           get { return _ExternalInputCountry; }
           set { _ExternalInputCountry = value; }
        }

        /// <summary>
        /// Gets or sets a value of the External Input.
        /// </summary>
        /// <value>External Input for created Channels</value>
        public TvLibrary.Implementations.AnalogChannel.VideoInputType ExternalInput
        {
           get { return _ExternalInput; }
           set { _ExternalInput = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to sort the channels after import.
        /// </summary>
        /// <value><c>true</c> if [allow channel sorting]; otherwise, <c>false</c>.</value>
        public bool AllowChannelSorting
        {
           get { return _sortChannels; }
           set { _sortChannels = value; }
        }

        #endregion

        /// <summary>
        /// Imports the channels.
        /// </summary>
        /// <returns></returns>
        public int ImportChannels()
        {
            return ImportChannels(_delay);
        }

        /// <summary>
        /// Imports the channels.
        /// </summary>
        /// <param name="delay">The delay in ms between each record.</param>
        /// <returns></returns>
        public int ImportChannels(int delay)
        {
            ImportStats stats = new ImportStats();

            foreach (Zap2it.SoapEntities.TVLineup lineup in _results.Data.Lineups.List)
            {
                Log.WriteFile("Processing lineup {0} [id={1} type={2} postcode={3}]", lineup.Name, lineup.ID, lineup.Type, lineup.PostalCode);
                foreach (Zap2it.SoapEntities.TVStationMap tvStationMap in lineup.StationMap)
                {
                    if (ShowProgress != null) 
                       ShowProgress(this, stats);

                    Zap2it.SoapEntities.TVStation tvStation = _results.Data.Stations.StationById(tvStationMap.StationId);
                    if (tvStation == null)
                    {
                        Log.WriteFile("Unable to find stationId #{0} specified in lineup", tvStationMap.StationId);
                        continue;
                    }

                    if (tvStationMap.ChannelMajor < 0)
                    {
                       Log.WriteFile("TVStationMap ChannelMajor Not Valid. StationID: {3} ChannelMajor: {0} ChannelMinor: {1} Zap2ItChannel: {2}", tvStationMap.ChannelMajor, tvStationMap.ChannelMinor, tvStationMap.Zap2ItChannel, tvStationMap.StationId);
                       continue;
                    }

                    Channel mpChannel = FindTVChannel(tvStation, tvStationMap, lineup.IsLocalBroadcast());
                    // Update the channel and map it
                    if (mpChannel != null)
                    {
                        mpChannel.GrabEpg      = false;
                        mpChannel.LastGrabTime = DateTime.Now;
                        mpChannel.ExternalId   = tvStation.ID + XMLTVID;
                        if (_renameChannels)
                        {
                            string oldName = mpChannel.Name;
                            mpChannel.Name = BuildChannelName(tvStation, tvStationMap);
                            RenameLogo(oldName, mpChannel.Name);
                        }
                        stats._iChannels++;

                        mpChannel.Persist();

                        Log.WriteFile("Updated channel {1} [id={0} xmlid={2}]", mpChannel.IdChannel, mpChannel.Name, mpChannel.ExternalId);
                    }
                    else if (_createChannels == true && lineup.IsAnalogue() == false)
                    {
                       // Create the channel 
                        string cname = BuildChannelName(tvStation, tvStationMap);

                        mpChannel = new Channel(cname, false, true, 0, Schedule.MinSchedule, false, Schedule.MinSchedule, 10000, true, tvStation.ID + XMLTVID, true);
                        mpChannel.Persist();

                        TvLibrary.Implementations.AnalogChannel tuningDetail = new TvLibrary.Implementations.AnalogChannel();

                        tuningDetail.IsRadio       = false;
                        tuningDetail.IsTv          = true;
                        tuningDetail.Name          = cname;
                        tuningDetail.Frequency     = 0;
                        tuningDetail.ChannelNumber = tvStationMap.ChannelMajor;

                        //(int)PluginSettings.ExternalInput;
                        //tuningDetail.VideoSource = PluginSettings.ExternalInput;
                        tuningDetail.VideoSource = _ExternalInput; // PluginSettings.ExternalInput;

                        // Too much overhead using settings directly for country
                        if (_ExternalInputCountry != null)
                           tuningDetail.Country = _ExternalInputCountry; // PluginSettings.ExternalInputCountry;

                        if (lineup.IsLocalBroadcast())
                           tuningDetail.TunerSource = DirectShowLib.TunerInputType.Antenna;
                        else
                           tuningDetail.TunerSource = DirectShowLib.TunerInputType.Cable;

                        //mpChannel.XMLId                  = tvStation.ID + XMLTVID;
                        //mpChannel.Name                   = BuildChannelName(tvStation, tvStationMap);
                        //mpChannel.AutoGrabEpg            = false;
                        //mpChannel.LastDateTimeEpgGrabbed = DateTime.Now;

                        //mpChannel.External             = true; // This may change with cablecard support one day
                        //mpChannel.ExternalTunerChannel = tvStationMap.ChannelMajor.ToString();
                        //mpChannel.Frequency            = 0;
                        //mpChannel.Number               = (int)PluginSettings.ExternalInput;

                        tvLayer.AddTuningDetails(mpChannel, tuningDetail);

                        Log.WriteFile("Added channel {1} [id={0} xmlid={2}]", mpChannel.IdChannel, mpChannel.Name, mpChannel.ExternalId);
                        stats._iChannels++;
                    }
                    else
                    {
                        Log.WriteFile("Could not find a match for {0}/{1}", tvStation.CallSign, tvStationMap.Channel);
                    }
                    System.Threading.Thread.Sleep(delay);
                }
            }

            if (_sortChannels)
               SortTVChannels();

            return stats._iChannels;
        }

        /// <summary>
        /// Imports the programs.
        /// </summary>
        /// <returns></returns>
        public int ImportPrograms()
        {
            return ImportPrograms(_delay);
        }

        /// <summary>
        /// Imports the programs.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns></returns>
        public int ImportPrograms(int delay)
        {
            const string EMPTY = "-";
            StringBuilder description = new StringBuilder();
            string strTvChannel;
            int idTvChannel;

            ImportStats stats = new ImportStats();
            ClearCache();
            Log.WriteFile("Starting processing of {0} schedule entries", _results.Data.Schedules.List.Count);
            foreach (Zap2it.SoapEntities.TVSchedule tvSchedule in _results.Data.Schedules.List)
            {
                if (ShowProgress != null) 
                   ShowProgress(this, stats);

                GetEPGMapping(tvSchedule.Station.Trim() + XMLTVID, out idTvChannel, out strTvChannel);
                if (idTvChannel <= 0)
                {
                   Log.WriteFile("Unable to find Program Channel: StationID: #{0} XMLTVID {1} ", tvSchedule.Station, XMLTVID);
                   continue;
                }
                Zap2it.SoapEntities.TVProgram tvProgram = _results.Data.Programs.ProgramById(tvSchedule.ProgramId);
                if (tvProgram == null)
                {
                    Log.WriteFile("Unable to find programId #{0} specified in schedule at time {1)", tvSchedule.ProgramId, tvSchedule.StartTimeStr);
                    continue;
                }

                description.Length = 0; // Clears the description string builder

                DateTime localStartTime = tvSchedule.StartTimeUtc.ToLocalTime();
                DateTime localEndTime   = localStartTime + tvSchedule.Duration;

                //Program mpProgram = new Program(idTvChannel, localStartTime, localEndTime, tvProgram.Title, tvProgram.
                //MediaPortal.TV.Database.TVProgram mpProgram = new TVProgram(strTvChannel, localStartTime, localEndTime, tvProgram.Title);

                string zTitle           = tvProgram.Title;
                string zEpisode         = string.IsNullOrEmpty(tvProgram.Subtitle) ? EMPTY : tvProgram.Subtitle;
                string zDate            = string.IsNullOrEmpty(tvProgram.OriginalAirDateStr) ? EMPTY : tvProgram.OriginalAirDate.Date.ToString();
                string zSeriesNum       = string.IsNullOrEmpty(tvProgram.Series) ? EMPTY : tvProgram.Series;
                string zEpisodeNum      = string.IsNullOrEmpty(tvProgram.SyndicatedEpisodeNumber) ? EMPTY : tvProgram.SyndicatedEpisodeNumber;
                string zStarRating      = string.IsNullOrEmpty(tvProgram.StarRating) ? EMPTY : tvProgram.StarRatingNum.ToString() + "/8";
                string zClassification  = string.IsNullOrEmpty(tvProgram.MPAARating) ? tvSchedule.TVRating : tvProgram.MPAARating;
                string zRepeat          = (tvProgram.IsRepeat(tvSchedule)) ? "Repeat" : string.Empty;

                string zEpisodePart     = EMPTY;
                string zGenre           = EMPTY;

                if (tvSchedule.Part.Number > 0)
                {
                    zEpisodePart = String.Format(
                        System.Globalization.CultureInfo.InvariantCulture, "..{0}/{1}", tvSchedule.Part.Number, tvSchedule.Part.Total);
                }
                else
                {
                    zEpisodePart = EMPTY;
                }

                // MediaPortal only supports a single Genre so we use the (first) primary one
                Zap2it.SoapEntities.ProgramGenre tvProgramGenres = _results.Data.Genres.ProgramGenreById(tvProgram.ID);
                if (tvProgramGenres != null && tvProgramGenres.List.Count > 0)
                {
                    zGenre = tvProgramGenres.List[0].GenreClass;
                }
                else
                {
                    zGenre = EMPTY;
                }

                // Add tags to description (temporary workaround until MediaPortal supports and displays more tags)
                if (!string.IsNullOrEmpty(tvProgram.Subtitle)) 
                   description.Append(tvProgram.Subtitle).Append(": ");

                description.Append(tvProgram.Description);

                if (tvProgram.Year > 0) 
                   description.AppendFormat(" ({0} {1})", tvProgram.Year, tvProgram.StarRating);

                if (tvProgram.IsRepeat(tvSchedule))
                {
                    if (string.IsNullOrEmpty(tvProgram.OriginalAirDateStr))
                    {
                        description.Append(" (Repeat)");
                    }
                    else
                    {
                        description.Append(" (First aired ").Append(tvProgram.OriginalAirDate.ToShortDateString()).Append(")");
                    }
                }

                if (tvSchedule.HDTV) 
                   description.Append(" (HDTV)");

                if (!string.IsNullOrEmpty(tvSchedule.Dolby)) 
                   description.AppendFormat(" ({0})", tvSchedule.Dolby);

                if (tvProgram.Advisories != null && tvProgram.Advisories.Advisory.Count > 0)
                {
                    description.AppendFormat(" ({0})", string.Join(", ", tvProgram.Advisories.Advisory.ToArray()));
                }

                //mpProgram.Description = description.ToString();

                Program mpProgram = new Program(idTvChannel, localStartTime, localEndTime, zTitle, description.ToString(), zGenre, false);
                mpProgram.Persist();
                stats._iPrograms++;
                //if (TVDatabase.UpdateProgram(mpProgram) != NOTFOUND)
                //{
                //    stats._iPrograms++;
                //}
                //else
                //{
                //    Log.WriteFile(MediaPortal.Services.LogType.EPG, "Zap2it schedule #{0} @{1} not imported", tvSchedule.ProgramId, tvSchedule.StartTimeStr);
                //}
                System.Threading.Thread.Sleep(delay);
            }
            return stats._iPrograms;
        }

        /// <summary>
        /// Clears the local channel and epg mapping cache.
        /// </summary>
        public void ClearCache()
        {
            if (_mpChannelCache != null)
               _mpChannelCache.Clear();
            if (_mpEpgMappingCache != null)
               _mpEpgMappingCache.Clear();
            if (_mpAtscChannelCache != null)
               _mpAtscChannelCache.Clear();
        }

        #region Protected Support Methods
        /// <summary>
        /// Builds the name of the channel based on the template.
        /// </summary>
        /// <param name="tvStation">The tv station.</param>
        /// <param name="tvStationMap">The tv station map.</param>
        /// <returns></returns>
        protected string BuildChannelName(Zap2it.SoapEntities.TVStation tvStation, Zap2it.SoapEntities.TVStationMap tvStationMap)
        {
            string channelName = string.Empty;
            if (tvStation != null && tvStationMap != null)
            {
                channelName = this._channelNameTemplate.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                channelName = channelName.Replace("{callsign}", tvStation.CallSign);
                channelName = channelName.Replace("{name}", tvStation.Name);
                channelName = channelName.Replace("{affiliate}", tvStation.Affiliate);
                channelName = channelName.Replace("{number}", tvStationMap.Channel);
                // debug
                //channelName = channelName + " BCNbr: " + tvStation.BroadcastChannelNumber.ToString();
                //channelName = channelName + " CMajr: " + tvStationMap.ChannelMajor.ToString();
             }
            return channelName;
        }

        /// <summary>
        /// Renames the logo.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        static protected void RenameLogo(string oldName, string newName)
        {
            //string strOldLogo = MediaPortal.Util.Utils.GetCoverArtName(MediaPortal.Util.Thumbs.TVChannel, oldName);
            //string strNewLogo = MediaPortal.Util.Utils.GetCoverArtName(MediaPortal.Util.Thumbs.TVChannel, newName);
            //if (System.IO.File.Exists(strOldLogo))
            //{
            //    try
            //    {
            //        System.IO.File.Move(strOldLogo, strNewLogo);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Write(ex);
            //    }
            //}
        }

        /// <summary>
        /// Provides a cached wrapper for getting the channel EPG mapping.
        /// </summary>
        /// <param name="xmlTvId">The XML tv id.</param>
        /// <param name="idTvChannel">The id tv channel.</param>
        /// <param name="strTvChannel">The STR tv channel.</param>
        /// <returns>true if the mapping was found</returns>
        protected bool GetEPGMapping(string xmlTvId, out int idTvChannel, out string strTvChannel)
        {
            if (_mpEpgMappingCache.ContainsKey(xmlTvId))
            {
                object[] obj = _mpEpgMappingCache[xmlTvId] as object[];
                idTvChannel = (int)obj[0];
                strTvChannel = (string)obj[1];
                return true;
            }
            else if (LookupEPGMapping(xmlTvId, out idTvChannel, out strTvChannel))
            {
                object[] obj = new object[] { idTvChannel, strTvChannel };
                _mpEpgMappingCache.Add(xmlTvId, obj);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Provides a wrapper for getting the channel by xmlTvId from the database.
        /// </summary>
        /// <param name="xmlTvId">xmlTvId aka ExternalID</param>
        /// <param name="idTvChannel">The id tv channel.</param>
        /// <param name="strTvChannel">The name tv channel.</param>
        /// <returns>true if the channel name was found</returns>
        protected bool LookupEPGMapping(string xmlTvId, out int idTvChannel, out string strTvChannel)
        {
           //Channel fch = Channel.Retrieve(
           idTvChannel  = -1;
           strTvChannel = String.Empty;

           SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));

           sb.AddConstraint(Operator.Equals, "externalId", xmlTvId.ToString());

           SqlStatement stmt = sb.GetStatement(true);

           System.Collections.IList chList = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

           if (chList.Count > 0)
           {
              idTvChannel  = ((Channel)chList[0]).IdChannel;
              strTvChannel = ((Channel)chList[0]).Name;
              return true;
           }

           return false;
        }


        /// <summary>
        /// Provides a wrapper for getting the channel by Name from the database.
        /// </summary>
        /// <param name="channelName">Channel Name.</param>
        /// <param name="idTvChannel">The id tv channel.</param>
        /// <returns>true if the channel name was found</returns>
        protected bool GetChannelByName(string channelName, out int idTvChannel)
        {
           //Channel fch = Channel.Retrieve(
           idTvChannel = -1;
           SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));

           sb.AddConstraint(Operator.Equals, "name", channelName.ToString());

           SqlStatement stmt = sb.GetStatement(true);

           System.Collections.IList chList = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());

           if (chList.Count > 0)
           {
              idTvChannel = ((Channel)chList[0]).IdChannel;
              return true;
           }

           return false;
        }


        /// <summary>
        /// Provides a cached wrapped for getting the ATSC channel.
        /// </summary>
        /// <param name="mpChannel">The TvDatabase.Channel</param>
        /// <param name="retChannel">The TvLibrary.Channels.ATSCChannel</param>
        /// <returns>true if the ATSC channel was found</returns>
        protected bool GetATSCChannel(Channel mpChannel, ref ATSCChannel retChannel)
        {
            if (_mpAtscChannelCache.ContainsKey(mpChannel.IdChannel))
            {
                retChannel = _mpAtscChannelCache[mpChannel.IdChannel] as ATSCChannel;
                return true;
            }

            System.Collections.IList tuneDetails = mpChannel.ReferringTuningDetail();
            if (tuneDetails.Count > 0)
            {
                if (TransFormTuningDetailToATSCChannel((TuningDetail)tuneDetails[0], ref retChannel))
                {
                   _mpAtscChannelCache.Add(mpChannel.IdChannel, retChannel);
                   return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fills in the ATSCChannel detail from the provided TuningDetail.
        /// </summary>
        /// <param name="tuneDetail">The TvDatabase.TuningDetail to use</param>
        /// <param name="atscChannel">The TvLibrary.Channels.ATSCChannel to fill in</param>
        /// <returns>true if successfully filled in the ATSCChannel Detail</returns>
        protected bool TransFormTuningDetailToATSCChannel(TuningDetail tuneDetail, ref ATSCChannel atscChannel)
        {
           if (tuneDetail.ChannelType != 2)
              return false;

           atscChannel.MajorChannel    = tuneDetail.MajorChannel;
           atscChannel.MinorChannel    = tuneDetail.MinorChannel;
           atscChannel.PhysicalChannel = tuneDetail.ChannelNumber;
           atscChannel.FreeToAir       = tuneDetail.FreeToAir;
           atscChannel.Frequency       = tuneDetail.Frequency;
           atscChannel.IsRadio         = tuneDetail.IsRadio;
           atscChannel.IsTv            = tuneDetail.IsTv;
           atscChannel.Name            = tuneDetail.Name;
           atscChannel.NetworkId       = tuneDetail.NetworkId;
           atscChannel.PcrPid          = tuneDetail.PcrPid;
           atscChannel.PmtPid          = tuneDetail.PmtPid;
           atscChannel.Provider        = tuneDetail.Provider;
           atscChannel.ServiceId       = tuneDetail.ServiceId;
           atscChannel.SymbolRate      = tuneDetail.Symbolrate;
           atscChannel.TransportId     = tuneDetail.TransportId;
           atscChannel.AudioPid        = tuneDetail.AudioPid;
           atscChannel.VideoPid        = tuneDetail.VideoPid;

           return true;
        }

        /// <summary>
        /// Fills in the ATSCChannel detail from the provided TuningDetail.
        /// </summary>
        /// <param name="idTvChannel">The Channel Database ID</param>
        /// <param name="station">The station</param>
        /// <param name="map">The map</param>
        /// <returns>true if a fix was completed on the map.ChannelMajor and map.ChannelMinor else false</returns>
        protected bool FixDigitalTerrestrialChannelMap(int idTvChannel, Zap2it.SoapEntities.TVStation station, ref Zap2it.SoapEntities.TVStationMap map)
        {
           if (station.IsDigitalTerrestrial && map.ChannelMinor <= 0)
           {
              ATSCChannel atscEPGChannel = new ATSCChannel();
              Channel mpChannel = Channel.Retrieve(idTvChannel);
              if (GetATSCChannel(mpChannel, ref atscEPGChannel))
              {
                 map.ChannelMajor = atscEPGChannel.MajorChannel;
                 map.ChannelMinor = atscEPGChannel.MinorChannel;

                 return true;
              }
           }

           return false;
        }

        /// <summary>
        /// Finds the TV channel.
        /// </summary>
        /// <param name="station">The station.</param>
        /// <param name="map">The map.</param>
        /// <returns>TVChannel found or null</returns>
        protected Channel FindTVChannel(Zap2it.SoapEntities.TVStation station, Zap2it.SoapEntities.TVStationMap map, bool LineupIsLocalBroadcast)
        {
            string strTvChannel;
            int idTvChannel;
            int dtMinorChannel = 0;
            /*
            Log.WriteFile("Attempting Channel Find/Match for: Callsign: {0}, Map Channel: {1}, Map Major {2}, Map Minor: {3}, Map StationID: {4} ", 
               station.CallSign, map.Channel, map.ChannelMajor, map.ChannelMinor, map.StationId);
            */

            // Check if the channel is already in EPG mapping database
            //Log.WriteFile("GetEPGMapping: {0} for {1}", station.Name, station.ID + XMLTVID);
            if (GetEPGMapping(station.ID + XMLTVID, out idTvChannel, out strTvChannel))
            {
                Log.WriteFile("Channel {0} was found as {1} in EPG mapping database", station.Name, strTvChannel);
                FixDigitalTerrestrialChannelMap(idTvChannel, station, ref map);

                return Channel.Retrieve(idTvChannel);
            }

            // Try locating the channel by callsign
            //Log.WriteFile("GetChannelByName: {0}", station.CallSign);
            if (GetChannelByName(station.CallSign, out idTvChannel) == true)
            {
                Channel mpChannel = Channel.Retrieve(idTvChannel);
                Log.WriteFile("Matched channel {0} to {1} using CallSign", station.CallSign, mpChannel.Name);
                FixDigitalTerrestrialChannelMap(idTvChannel, station, ref map);

                return mpChannel;
            }

            // Load the full list of channels into memory if we don't have them already
            //Log.WriteFile("CheckChannelCache");
            if (_mpChannelCache == null || _mpChannelCache.Count < 1)
            {
                _mpChannelCache = Channel.ListAll();
            }
            
            // Iterate through each channel looking for a match
            //Log.WriteFile("Channel Cache Count {0} Callsign: {1}, Map Channel: {2}, Map StationID: {3} ", _mpChannelCache.Count, station.CallSign, map.Channel, map.StationId);
            foreach (Channel mpChannel in _mpChannelCache)
            {
                //Log.WriteFile("GettingTuningDetail: {0}", mpChannel.Name);
                System.Collections.IList chDetailList = mpChannel.ReferringTuningDetail();
                TuningDetail chDetail = (TuningDetail)chDetailList[0];
                
                if (String.IsNullOrEmpty(mpChannel.ExternalId)) // Only look at non-mapped channels
                {
                    // Check for an ATSC major/minor channel number match
                    ATSCChannel atscChannel = new ATSCChannel();
                    if (GetATSCChannel(mpChannel, ref atscChannel))
                    {
                        if (map.ChannelMajor == atscChannel.MajorChannel && map.ChannelMinor == atscChannel.MinorChannel)
                        {
                            Log.WriteFile("Matched channel {0} to {1} by ATSC channel ({2}-{3})", 
                                station.CallSign, mpChannel.Name, atscChannel.MajorChannel, atscChannel.MinorChannel);

                            return mpChannel;
                        }

                        // check that the DT subChannel is properly formated
                        // if not we do an additional check by broadcast number and subChannel
                        dtMinorChannel = 0;
                        if (station.IsDigitalTerrestrial && map.ChannelMinor <= 0)
                        {
                           station.DigitalTerrestrialSubChannel(out dtMinorChannel);
                           if (atscChannel.PhysicalChannel == map.ChannelMajor
                              && atscChannel.MinorChannel == dtMinorChannel)
                           {
                              Log.WriteFile("Matched channel {0} to {1} by ATSC channel ({2}-{3}), using Physical Channel: {4} and SubChannel: {5}",
                                  station.CallSign, mpChannel.Name, atscChannel.MajorChannel, atscChannel.MinorChannel, atscChannel.PhysicalChannel, dtMinorChannel);

                              // Fix the Map Entries for Major and Minor Channels 
                              map.ChannelMajor = atscChannel.MajorChannel;
                              map.ChannelMinor = atscChannel.MinorChannel;

                              return mpChannel;
                           }
                        }
                    }
                    // If the Lineup is a LocalBroadcast we want to give preference to 
                    // searching by Broadcast Number
                    // else give preference to the Major Channel Number
                    else if (LineupIsLocalBroadcast && !station.IsDigitalTerrestrial)
                    {
                       // Not an ATSC channel so check for an over-the-air (by checking it has a frequency) broadcast channel number match
                       if (chDetail.Frequency != 0 && chDetail.ChannelNumber == station.BroadcastChannelNumber)
                       {
                          Log.WriteFile("Matched channel {0} to {1} by OTA broadcast channel ({2})",
                             station.CallSign, chDetail.Name, chDetail.ChannelNumber);

                          return mpChannel;
                       }
                    }
                    else if (!station.IsDigitalTerrestrial)
                    {
                       // Check for an over-the-air (by checking it has a frequency) major channel number match
                       if (chDetail.Frequency != 0 && chDetail.ChannelNumber == map.ChannelMajor)
                       {
                          Log.WriteFile("Matched channel {0} to {1} by lineup channel ({2})",
                             station.CallSign, chDetail.Name, chDetail.ChannelNumber);

                          return mpChannel;
                       }
                    }

                }
            }

            if (_allowChannelNumberMapping && !station.IsDigitalTerrestrial)
            {
               foreach (Channel mpChannel in _mpChannelCache)
               {
                  if (!String.IsNullOrEmpty(mpChannel.ExternalId)) // Only Non-Mapped Channels
                     continue;

                  //Log.WriteFile("GettingTuningDetail: {0}", mpChannel.Name);
                  System.Collections.IList chDetailList = mpChannel.ReferringTuningDetail();
                  TuningDetail chDetail = (TuningDetail)chDetailList[0];

                  // If the Lineup is a LocalBroadcast search by Broadcast Number
                  // else give preference to the Major Channel Number
                  if (LineupIsLocalBroadcast)
                  {
                     // Not ATSC channel so check for an over-the-air broadcast channel number match
                     if (chDetail.ChannelNumber == station.BroadcastChannelNumber)
                     {
                        Log.WriteFile("Matched channel {0} to {1} by OTA broadcast channel ({2}) Using Channel Number ONLY",
                           station.CallSign, chDetail.Name, chDetail.ChannelNumber);

                        return mpChannel;
                     }
                  }
                  else
                  {
                     // Check for an channel number  major channel number match
                     if (chDetail.ChannelNumber == map.ChannelMajor)
                     {
                        Log.WriteFile("Matched channel {0} to {1} by lineup channel ({2}) Using Channel Number ONLY",
                           station.CallSign, chDetail.Name, chDetail.ChannelNumber);

                        return mpChannel;
                     }
                  }
               }
            }

            // Iterate through each channel looking for a match
            // One more time looking @ MajorChannel Number even if it is a local broadcast
            foreach (Channel mpChannel in _mpChannelCache)
            {
               if (!String.IsNullOrEmpty(mpChannel.ExternalId))
                  continue;

               System.Collections.IList chDetailList = mpChannel.ReferringTuningDetail();
               TuningDetail chDetail = (TuningDetail)chDetailList[0];

               if (!station.IsDigitalTerrestrial && String.IsNullOrEmpty(mpChannel.ExternalId)) // Only look at non-mapped channels
               {
                  if (LineupIsLocalBroadcast)
                  {
                     // Check for an over-the-air (by checking it has a frequency) major channel number match
                     if (chDetail.Frequency != 0 && chDetail.ChannelNumber == map.ChannelMajor)
                     {
                        Log.WriteFile("Matched channel {0} to {1} by lineup channel ({2})",
                           station.CallSign, chDetail.Name, chDetail.ChannelNumber);

                        return mpChannel;
                     }
                  }
                  else
                  {
                     // Not an ATSC channel so check for an over-the-air (by checking it has a frequency) broadcast channel number match
                     if (chDetail.Frequency != 0 && chDetail.ChannelNumber == station.BroadcastChannelNumber)
                     {
                        Log.WriteFile("Matched channel {0} to {1} by OTA broadcast channel ({2})",
                           station.CallSign, chDetail.Name, chDetail.ChannelNumber);

                        return mpChannel;
                     }
                  }
               }
            }

            return null;
        }

        /// <summary>
        /// Sorts the TV channels.
        /// </summary>
        protected void SortTVChannels()
        {
           // Get a fresh list of channels
           _mpChannelCache = Channel.ListAll();

           List<ChannelInfo> listChannels = new List<ChannelInfo>();
           foreach(Channel mpChannel in _mpChannelCache)
           {
              ChannelInfo chi;
              ATSCChannel atscCh = new ATSCChannel();
              if (GetATSCChannel(mpChannel, ref atscCh))
              {
                 chi = new ChannelInfo(mpChannel.IdChannel, atscCh.MajorChannel, atscCh.MinorChannel, mpChannel.Name);
              }
              else
              {
                 System.Collections.IList chDetailList = mpChannel.ReferringTuningDetail();
                 TuningDetail chDetail = (TuningDetail)chDetailList[0];
                 chi = new ChannelInfo(mpChannel.IdChannel, chDetail.ChannelNumber, 0, mpChannel.Name);
              }

              listChannels.Add(chi);
           }

           ChannelSorter sorter = new ChannelSorter(listChannels, new ChannelNumberComparer());

           for(int i = 0; i < listChannels.Count; i++)
           {
              ChannelInfo sChi = listChannels[i];
              foreach(Channel mpChannel in _mpChannelCache)
              {
                 if (sChi.ID != mpChannel.IdChannel)
                    continue;

                 if (mpChannel.SortOrder != i)
                 {
                    mpChannel.SortOrder = i;
                    mpChannel.Persist();
                 }
              }
           }

        }
        
        #endregion

        #region IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearCache();
                if (this._results != null)
                   this._results = null;
            }
        }
        #endregion
    }

    public class ImportStats
    {
        internal string _status = string.Empty;
        internal int _iPrograms;
        internal int _iChannels;
        internal DateTime _startTime = DateTime.Now;
        internal DateTime _endTime = DateTime.MinValue;

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public string Status
        {
            get { return _status; }
        }
        /// <summary>
        /// Gets the programs.
        /// </summary>
        /// <value>The programs.</value>
        public int Programs
        {
            get { return _iPrograms; }
        }
        /// <summary>
        /// Gets the channels.
        /// </summary>
        /// <value>The channels.</value>
        public int Channels
        {
            get { return _iChannels; }
        }
        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime StartTime
        {
            get { return _startTime; }
        }
        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        public DateTime EndTime
        {
            get { return _endTime; }
        }
    };




}
