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
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

//using MediaPortal.GUI.Library;
//using MediaPortal.Dialogs;
//using MediaPortal.TV.Database;
using System.Runtime.InteropServices;
using TvEngine;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using Gentle.Common;
using Gentle.Framework;

namespace ProcessPlugins.EpgGrabber
{
    public class Zap2itPlugin : PluginSetup, ITvServerPlugin, IDisposable //, IPlugin, ISetupForm, IDisposable
    {
        #region Messages
        protected const string DIALOG_TITLE      = "Zap2it Electronic Program Guide Download";
        protected const string MSG_EPG_UPDATED   = "Electronic Program Guide now updated through {0}";
        protected const string MSG_EXPIRES       = "Your Zap2it EPG subscription must be renewed within {0} days";
        protected const string MSG_ACCESS_DENIED = "Access denied from Zap2it when trying to update.\nPlease check your username and password are correct.";
        protected const string MSG_BAD_USERNAME  = "Unable to download program guide.\nPlease configure your username and password.";
        protected const string MSG_NO_LINEUP     = "No lineups defined or your subscription has expired\nPlease check your lineups and subscription.";
        protected const string MSG_NO_CHANNELS   = "Unable to process any channels, have you done an autotune in setup?";
        protected const string MSG_NO_INTERNET   = "Unable to update Electronic Program Guide, no Internet connection detected";
        #endregion

        [DllImport("wininet.dll")]
        private extern static bool InternetCheckConnection(string lpszUrl, int dwFlags, int dwReserved);

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        private const int FLAG_ICC_FORCE_CONNECTION = 0x01; // &H1;
        
        private TvBusinessLayer tvLayer = new TvBusinessLayer();

        /// <summary>
        /// System.Threading.Timer is a simple, lightweight timer that is served by threadpool threads,
        /// it is used to call the import EPG callback on a scheduled basis.
        /// </summary>
        protected System.Threading.Timer timerThread;


        #region ITvServerPlugin Start/Stop Members
        /// <summary>
        /// Starts this instance.
        /// </summary>
        void ITvServerPlugin.Start(IController controller)
        {
            timerThread = new System.Threading.Timer(new TimerCallback(RefreshEPG));

            Log.WriteFile("{0}: version {1} starting", Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            Log.WriteFile("Date of last program entry in database currently {0}", GetLastProgramEntry());
            Log.WriteFile("Configured to grab {0} days of EPG data", PluginSettings.GuideDays);

            // If our next poll is after the last program entry, update the data now so we don't run too short
            if (PluginSettings.NextPoll > GetLastProgramEntry() 
               || PluginSettings.NextPoll < DateTime.Now)
            {
                PluginSettings.NextPoll = DateTime.Now.AddSeconds(15);
            }

            Log.WriteFile("Scheduling first poll for Zap2it listings at {0}", PluginSettings.NextPoll);
            SetThreadTimer(PluginSettings.NextPoll);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void ITvServerPlugin.Stop()
        {
            Log.WriteFile("{0}: stopping", Name);
            if (timerThread != null)
            {
              timerThread.Dispose();
              timerThread = null;
            }
        }
        #endregion



        #region Plugin Implementation Methods
        /// <summary>
        /// Refreshes the EPG.
        /// </summary>
        /// <param name="state">The state.</param>
        protected void RefreshEPG(object state)
        {
            EpgImportResult results = new EpgImportResult();

            Thread.CurrentThread.Name = Name;
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            Thread.CurrentThread.IsBackground = true;
            Log.WriteFile("Starting EPG refresh job on thread #{0}", Thread.CurrentThread.ManagedThreadId);
            Log.WriteFile("UTC time is {0}, local time is {1} (DST is {2})", DateTime.Now.ToUniversalTime(), DateTime.Now.ToLocalTime(), DateTime.Now.IsDaylightSavingTime());
            Log.WriteFile("Rename Existing Channels is set to: {0}, with Template: {1}.", PluginSettings.RenameExistingChannels.ToString(), PluginSettings.ChannelNameTemplate);
            Log.WriteFile("Add New Digital Channels is set to: {0}, with External Input: {1}.", PluginSettings.AddNewChannels.ToString(), PluginSettings.ExternalInput.ToString());
            Log.WriteFile("Add New Digital Channels Country is set to: {0}.", PluginSettings.ExternalInputCountry.ToString());
            Log.WriteFile("Channel Sorting is set to: {0}.", PluginSettings.SortChannelsByNumber.ToString());
            Log.WriteFile("Allow Channel Matching without Frequency is set to: {0}.", PluginSettings.AllowChannelNumberOnlyMapping.ToString());
            Log.WriteFile("Delete Channels with No EPG Mapping is set to: {0}.", PluginSettings.DeleteChannelsWithNoEPGMapping.ToString());

            if (string.IsNullOrEmpty(PluginSettings.Username) || string.IsNullOrEmpty(PluginSettings.Password))
            {
                Log.WriteFile(MSG_BAD_USERNAME);
                Log.WriteFile("-Username: " + PluginSettings.Username + " -Password: " + PluginSettings.Password);
                Notify(MSG_BAD_USERNAME);
                return; // Don't reschedule, just quit
            }

            //if (!MediaPortal.Util.Win32API.IsConnectedToInternet())
            if (!CheckInternetConnection("http://www.team-mediaportal.com"))
            {
                // Should probably use the Zap2It location but not sure if ICMP needed
                Log.WriteFile("Not connected to Internet at this time, skipping update");
                ScheduleNextRetrievalTime();
                NotifyPopUp(MSG_NO_INTERNET);
                return;
            }

            // Import initiator
            try
            {
                PrepareForEpgImport();
                results = RunEpgImport();
                FinalizeEpgImport(results);
            }
            catch (System.Net.WebException ex)
            {
                Log.WriteFile(ex.ToString());
                RollbackEpgImport();
                // Check to see if we failed because our login information was incorrect
                System.Net.HttpWebResponse httpWebResponse = ex.Response as System.Net.HttpWebResponse;
                if (httpWebResponse != null && httpWebResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Notify(MSG_ACCESS_DENIED);
                }
            }
            catch (InvalidOperationException ex)
            {
                // This means the user has no lineup or their subscription is expired
                Log.WriteFile(ex.ToString());
                RollbackEpgImport();
                Notify(MSG_NO_LINEUP);
            }
            catch (Exception ex)
            {
                Log.WriteFile(ex.ToString());
                RollbackEpgImport();
            }
            finally
            {
                ScheduleNextRetrievalTime();
            }

            // Check if we managed to process any channels, if not, this probably means they didn't do an autotune
            if (results.ChannelsProcessed == 0)
            {
                Notify(MSG_NO_CHANNELS);
            }

            // Check for subscription expiration and notify user. You cannot renew until 7 days before expiration.
            // Once it has expired, we'll not be able to connect to download listings
            TimeSpan expires = (results.SubscriptionExpiration - DateTime.Now);
            if (expires.Days >= 0 && expires.Days < 7)
            {
                Notify(MSG_EXPIRES, expires.Days);
            }
            else if (PluginSettings.NotifyOnCompletion && results.ChannelsProcessed > 0 && GetLastProgramEntry() > DateTime.Now)
            {
                NotifyPopUp(MSG_EPG_UPDATED, GetLastProgramEntry());
            }
        }

        /// <summary>
        /// Prepares MediaPortal database for EPG import by supressing event generation, clearing the cache,
        /// starting a transaction and removing old programs.
        /// </summary>
        protected void PrepareForEpgImport()
        {
            tvLayer.RemoveOldPrograms();
            //TVDatabase.SupressEvents = true;
            //TVDatabase.RemoveOldPrograms();
            //TVDatabase.ClearCache();
            //TVDatabase.BeginTransaction();
        }

        /// <summary>
        /// Runs the epg import.
        /// </summary>
        /// <returns>DateTime of the subscription expiration</returns>
        protected EpgImportResult RunEpgImport()
        {
            const int UPDATE_HOURS = 24;
            Zap2it.SoapEntities.DownloadResults listingData;
            EpgImportResult result = new EpgImportResult();
            DateTime lastDbEntry;
            DateTime startDate;
            DateTime endDate;
            string channelHash;
            int programCount;

            // Get the end time of the latest program entry in the Epg before we start any imports
            lastDbEntry = GetLastProgramEntry();

            using (Zap2it.Zap2itWebService webService = new Zap2it.Zap2itWebService(PluginSettings.Username, PluginSettings.Password))
            {
                #region Update Epg with next 24 hours worth of data for last minute changes

                startDate = DateTime.Now;
                endDate   = startDate.AddHours(UPDATE_HOURS);

                Log.WriteFile("Requesting {0} hours of program listings from [{1}] to [{2}]", UPDATE_HOURS, startDate, endDate);
                
                listingData                   = webService.Download(startDate, endDate);
                result.SubscriptionExpiration = listingData.SubscriptionExpiration;
                result.Messages               = listingData.Messages;
                result.UpdateRequired         = false;

                // Log any messages sent by Zap2it to us
                foreach (string msg in listingData.Messages) 
                   Log.WriteFile("zap2it msg: {0}", msg);

                // If debug mode is enabled, write the data to disk for analysis
                if (PluginSettings.DebugMode)
                   WriteXTVD(listingData.Data, String.Format(@"{0}\MediaPortal TV Server\zap2itupdate.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
                   //WriteXTVD(listingData.Data, @"log\zap2itupdate.xml");

                // Instantiate an EPG listing importer to do the actual import work
                using (EpgListingsImporter epgListingsImporter = new EpgListingsImporter(listingData))
                {
                    epgListingsImporter.ChannelNameTemplate = PluginSettings.ChannelNameTemplate;
                    epgListingsImporter.CreateChannels = PluginSettings.AddNewChannels;
                    epgListingsImporter.RenameChannels = PluginSettings.RenameExistingChannels;
                    epgListingsImporter.Delay = 10;
                    epgListingsImporter.AllowChannelNumberOnlyMapping = PluginSettings.AllowChannelNumberOnlyMapping;
                    epgListingsImporter.ExternalInputCountry = PluginSettings.ExternalInputCountry;
                    epgListingsImporter.ExternalInput = PluginSettings.ExternalInput;
                    epgListingsImporter.AllowChannelSorting = PluginSettings.SortChannelsByNumber;

                    result.ChannelsProcessed = epgListingsImporter.ImportChannels();

                    Log.WriteFile("Sucessfully processed {0} of {1} channels", result.ChannelsProcessed, listingData.Data.Stations.List.Count);

                    if (result.ChannelsProcessed == 0)
                    {
                        Log.WriteFile("WARNING: Skipping schedule import (no channels sucessfully processed)");
                        return result;
                    }
                    else
                    {
                        programCount = epgListingsImporter.ImportPrograms();
                        Log.WriteFile("Sucessfully Imported {0} of {1} schedule entries", programCount, listingData.Data.Schedules.List.Count);
                    }
                }
                #endregion

                #region Full Epg update
                // Determine if there have been any changes in the Zap2it channel lineup
                channelHash = GetStationsHash(listingData.Data.Stations);
                if (PluginSettings.ChannelFingerprint.Equals(channelHash))
                {
                    Log.WriteFile("No changes detected in Zap2it channel lineup");
                    // Set start of full grab to the end of the first grab or the midnight of the last epg program in db if later
                    startDate = (lastDbEntry.Date > endDate ? lastDbEntry.Date : endDate);
                }
                else
                {
                    Log.WriteFile("Detected a change in Zap2it channel lineup fingerprint, resetting start date");
                    PluginSettings.ChannelFingerprint = channelHash;
                    startDate = endDate; // Set the start date to the previous end update end date (e.g. 24 hours from now)
                }
                
                // Add user selected number of GuideDays to midnight today to get the desired end date
                endDate = DateTime.Now.Date.AddDays(PluginSettings.GuideDays);

                // endDate could have been set to < startDate if getting only 1 day of data
                if (endDate < startDate)
                {
                   endDate = startDate.Date.AddDays((double)1);
                }

                // Check to see if we already have enough data in the EPG without asking for more. This could happen if
                // the user changes the config to ask for less days or if other import mechanisms have been used.
                if (endDate < lastDbEntry)
                {
                    Log.WriteFile("Already have enough guide data, skipping data request");
                    return result;
                }

                // Download program listings
                Log.WriteFile("Requesting program listings from [{0}] to [{1}]", startDate, endDate);
                listingData = webService.Download(startDate, endDate);

                // Log any messages sent by Zap2it
                foreach (string msg in listingData.Messages) 
                   Log.WriteFile("zap2it message: {0}", msg);

                // If debug mode is enabled, write the data to disk for analysis
                if (PluginSettings.DebugMode)
                   WriteXTVD(listingData.Data, String.Format(@"{0}\MediaPortal TV Server\zap2it.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
                   //WriteXTVD(listingData.Data, @"log\zap2it.xml");

                using (EpgListingsImporter epgListingsImporter = new EpgListingsImporter(listingData))
                {
                    epgListingsImporter.Delay = 10;
                    epgListingsImporter.CreateChannels = PluginSettings.AddNewChannels;

                    programCount = epgListingsImporter.ImportPrograms();
                    Log.WriteFile("Imported {0} of {1} schedule entries", programCount, listingData.Data.Schedules.List.Count);
                    Log.WriteFile("Last program entry in database was {0}, now {1}", lastDbEntry, GetLastProgramEntry());
                }
                #endregion

                result.StartOfUpdateRange = startDate;
                result.EndOfUpdateRange   = endDate;
                result.UpdateRequired     = true;
                return result;
            }
        }

        /// <summary>
        /// Rollback the EPG import.
        /// </summary>
        protected void RollbackEpgImport()
        {
            //Log.WriteFile("Rolling back the transaction");
            //TVDatabase.RollbackTransaction();
            //TVDatabase.SupressEvents = false;
        }

        /// <summary>
        /// Finalizes the EPG import by 
        ///     commiting the database transaction
        ///     removing overlapping programs
        ///     enabling event generation
        ///     notifyng the user of sucess or subscription expiration
        /// </summary>
        protected void FinalizeEpgImport(EpgImportResult results)
        {
            //Log.WriteFile("Removing any overlapping programs");
            //RemoveOverLappingPrograms();
            //TVDatabase.RemoveOverlappingPrograms();

            // Remove Channels with no EPG Mapping?
            if (PluginSettings.DeleteChannelsWithNoEPGMapping)
            {
               Log.WriteFile("Checking for channels with no EPG mapping to delete");
               int rnbr = RemoveChannelsWithNoEPGMapping();
               Log.WriteFile("Channels with no EPG Mapping Removed: {0}", rnbr);
            }

            Log.WriteFile("Removing any Overlapping programs");
            RemoveOverLappingPrograms();
            Log.WriteFile("Check for Overlapping programs completed");

            // TODO: This does not work as expected, but is much faster
            //       Try to determine another way.
            //const int UPDATE_HOURS = 24;
            //Log.WriteFile("Checking for Overlapping programs in the next {0} hours of program listings", UPDATE_HOURS);
            //// Always check the next 24 hours for overlaps 
            //RemoveOverLappingPrograms(DateTime.Now, DateTime.Now.AddHours(UPDATE_HOURS));
            
            //if (results.UpdateRequired)
            //{
            //   Log.WriteFile("Checking for overlapping programs from [{0}] to [{1}]", results.StartOfUpdateRange, results.EndOfUpdateRange);
            //   RemoveOverLappingPrograms(results.StartOfUpdateRange, results.EndOfUpdateRange);
            //}


            //Log.WriteFile("Committing the transaction");
            //TVDatabase.CommitTransaction();
            //TVDatabase.RemoveOverlappingPrograms();
            //TVDatabase.SupressEvents = false;
            // Reload the Television EPG
            //MediaPortal.GUI.TV.GUITVHome.Navigator.ReLoad();
        }

        /// <summary>
        /// Gets the next poll time from Zap2it or returns an hour from now if it failed or we are offline
        /// </summary>
        protected void ScheduleNextRetrievalTime()
        {
            DateTime nextPoll = DateTime.Now.AddHours(1);

            if (!CheckInternetConnection("http://www.team-mediaportal.com"))
            {
               Log.WriteFile("Scheduling next poll for {0}", nextPoll);
               PluginSettings.NextPoll = nextPoll;
               SetThreadTimer(nextPoll);
               return;
            }

            try
            {
                using (Zap2it.Zap2itWebService webService = new Zap2it.Zap2itWebService(PluginSettings.Username, PluginSettings.Password))
                {
                    nextPoll = webService.GetSuggestedTime();
                }
            }
            finally
            {
                Log.WriteFile("Scheduling next poll for {0}", nextPoll);
                PluginSettings.NextPoll = nextPoll;
                SetThreadTimer(nextPoll);
            }
        }

        /// <summary>
        /// Notifies the user through a GUIDialogNotify window. Window self-removes in 20 seconds.
        /// </summary>
        /// <param name="notifyText">The notify text.</param>
        static protected void NotifyWindowsPopUp(string notifyText, params object[] args)
        {
           const int TIMEOUT = 20;

           NotifyWindowForm dialogNotify = new NotifyWindowForm(DIALOG_TITLE, String.Format(System.Globalization.CultureInfo.CurrentCulture, notifyText, args), TIMEOUT);
           dialogNotify.ShowDialog();
        }

        /// <summary>
        /// Notifies the user through a GUIDialogNotify window. Window self-removes in 20 seconds.
        /// </summary>
        /// <param name="notifyText">The notify text.</param>
        static protected void NotifyPopUp(string notifyText, params object[] args)
        {
           //const int TIMEOUT = 20;
           //NotifyWindowForm dialogNotify = new NotifyWindowForm(DIALOG_TITLE, String.Format(System.Globalization.CultureInfo.CurrentCulture, notifyText, args), TIMEOUT);
           //dialogNotify.ShowDialog();
           Log.WriteFile(notifyText, args); 
        }

        /// <summary>
        /// Notifies the user through a GUIDialogNotify window. Window self-removes in 20 seconds.
        /// </summary>
        /// <param name="notifyText">The notify text.</param>
        static protected void Notify(string notifyText, params object[] args)
        {
           //const int TIMEOUT = 20;
           //NotifyWindowForm dialogNotify = new NotifyWindowForm(DIALOG_TITLE, String.Format(System.Globalization.CultureInfo.CurrentCulture, notifyText, args), TIMEOUT);
           //dialogNotify.ShowDialog();
           Log.WriteFile(notifyText, args); 
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
                timerThread.Dispose();
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Sets the timer.
        /// </summary>
        /// <param name="target">The target DateTime.</param>
        protected void SetThreadTimer(DateTime target)
        {
            TimeSpan ts = target.Subtract(DateTime.Now);
            int milliseconds = (int)ts.TotalMilliseconds;
            milliseconds = Math.Max(milliseconds, 15000); // 15 second minimum wait time
            timerThread.Change(milliseconds, Timeout.Infinite);
        }
        
        /// <summary>
        /// Gets the DateTime of the last program entry or now if MinValue if there are no program entries in the database.
        /// </summary>
        /// <returns></returns>
        static protected DateTime GetLastProgramEntry()
        {
            //long lastEntry;
            //// FIXME: Why is the GetLastProgramEntry call returning a string? Should probably natively return a long.
            //Int64.TryParse(TVDatabase.GetLastProgramEntry(), out lastEntry);
            //DateTime lastDate = (lastEntry == 0) ? DateTime.MinValue : MediaPortal.Util.Utils.longtodate(Convert.ToInt64(lastEntry));
            //return lastDate;

            SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
            IFormatProvider mmddFormat = new System.Globalization.CultureInfo(String.Empty, false);

            sb.SetRowLimit(1);
            sb.AddOrderByField(false, "EndTime");

            SqlStatement stmt = sb.GetStatement(true);
            System.Collections.IList progs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
            
            if (progs.Count >= 1)
            {
               Program lProgram = (Program)progs[0];
               return lProgram.EndTime;
            }
            
            return DateTime.MinValue;
        }

        /// <summary>
        /// Removes Old Programs from the TVDatabase.
        /// </summary>
        /// <returns></returns>
        static protected void RemoveOldPrograms()
        {
            SqlBuilder sb = new SqlBuilder(StatementType.Delete, typeof(Program));
            DateTime dtYesterday = DateTime.Now.AddDays(-1);
            IFormatProvider mmddFormat = new System.Globalization.CultureInfo(String.Empty, false);
            sb.AddConstraint(String.Format("endTime < '{0}'", dtYesterday.ToString("yyyyMMdd HH:mm:ss", mmddFormat)));
            SqlStatement stmt = sb.GetStatement(true);
            ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
        }

        /// <summary>
        /// This function will check all tv programs in the database and
        /// will remove any overlapping programs
        /// An overlapping program is a tv program which overlaps with another tv program in time
        /// for example 
        ///   program A on MTV runs from 20.00-21.00 on 1 november 2004
        ///   program B on MTV runs from 20.55-22.00 on 1 november 2004
        ///   this case, program B will be removed
        /// </summary>
        private void RemoveOverLappingPrograms()
        {
           //System.Collections.IList<Channel> chList = Channel.ListAll();
           System.Collections.IList chList = Channel.ListAll();
           
           int MaxDays = PluginSettings.GuideDays;

           foreach (Channel chan in chList)
           {
              //System.Collections.IList<Program> pgList = tvLayer.GetPrograms(chan, DateTime.MinValue, DateTime.MaxValue);
              System.Collections.IList pgList = tvLayer.GetPrograms(chan, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(MaxDays));
              DateTime pEnd   = DateTime.MinValue;
              DateTime pStart = DateTime.MinValue;

              foreach (Program prog in pgList)
              {
                 bool overLap = false;
                 if (pEnd > prog.StartTime)
                    overLap = true;
                 if (overLap)
                 {
                    prog.Delete();
                 }
                 else
                 {
                    pEnd   = prog.EndTime;
                    pStart = prog.StartTime;
                 }
              }
           }
        }

        /// <summary>
        /// This function will check tv programs in the database and
        /// will remove any overlapping programs within the range specified
        /// An overlapping program is a tv program which overlaps with another tv program in time
        /// for example 
        ///   program A on MTV runs from 20.00-21.00 on 1 november 2004
        ///   program B on MTV runs from 20.55-22.00 on 1 november 2004
        ///   this case, program B will be removed
        /// </summary>
        /// <param name="startTime">The start of the range to check for overlaps.</param>
        /// <param name="endTime">The end of the range to check for overlaps.</param>
        private void RemoveOverLappingPrograms(DateTime startTime, DateTime endTime)
        {
           //System.Collections.IList<Channel> chList = Channel.ListAll();
           System.Collections.IList chList = Channel.ListAll();

           int MaxDays = PluginSettings.GuideDays;

           foreach (Channel chan in chList)
           {
              System.Collections.IList pgList = tvLayer.GetPrograms(chan, startTime, endTime);

              DateTime pEnd   = DateTime.MinValue;
              DateTime pStart = DateTime.MinValue;

              foreach (Program prog in pgList)
              {
                 bool overLap = false;
                 if (pEnd > prog.StartTime)
                    overLap = true;
                 if (overLap)
                 {
                    prog.Delete();
                 }
                 else
                 {
                    pEnd   = prog.EndTime;
                    pStart = prog.StartTime;
                 }
              }
           }
        }

        /// <summary>
        /// Removes Channels With No External EPG Mapping from the TVDatabase.
        /// </summary>
        /// <returns>Number of Channels Removed</returns>
        private int RemoveChannelsWithNoEPGMapping()
        {
           if (!PluginSettings.DeleteChannelsWithNoEPGMapping)
              return 0;

           int chcount = 0;
           System.Collections.IList mpChannelCache = Channel.ListAll();
           foreach (Channel mpChannel in mpChannelCache)
           {
              if (String.IsNullOrEmpty(mpChannel.ExternalId))
              {
                 chcount ++;
                 mpChannel.Delete();
              }
           }

           return chcount;
        }

        /// <summary>
        /// Writes the Zap2it XML data structure to a file.
        /// </summary>
        /// <param name="listingData">The XTVD entity.</param>
        /// <param name="filename">The filename.</param>
        private static void WriteXTVD(Zap2it.SoapEntities.XTVD listingData, string filename)
        {
            Log.WriteFile("Debug mode enabled - dumping data to zap2it.xml");
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;
            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(filename, settings))
            {
                listingData.WriteTo(writer);
            }
        }

        /// <summary>
        /// Gets a MD5 hash for the collection of stations and returns as a Base64 string
        /// </summary>
        /// <param name="tvStations">The tv stations.</param>
        /// <returns>Base64 string</returns>
        static protected string GetStationsHash(Zap2it.SoapEntities.TVStations tvStations)
        {
            // We create a string with all the ID values from the channels and return an base64 MD5 hash
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            StringBuilder sb = new StringBuilder();
            tvStations.List.Sort(); // Sort the list so order changes will not effect the hash
            foreach (Zap2it.SoapEntities.TVStation tvStation in tvStations.List)
            {
                sb.Append(tvStation.ID);
            }
            return Convert.ToBase64String(md5.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString())));
        }

        /// <summary>
        /// Gets a bool Determining If Connected to the Internet
        /// </summary>
        /// <param name="sURL">Fully Qualified URL to use in determining Internet Connectivity.</param>
        /// <returns>bool</returns>
        public static bool CheckInternetConnection(string sURL)
        {
           // InternetGetConnectedState - Only Checks State of Connection Device (i.e. NIC)

           //int flags = 0; //DWORD flags = 0;

          int Desc;
          if (InternetGetConnectedState(out Desc, 0))  //if( InternetGetConnectedState( &flags, 0 ) )
          {
             if (InternetCheckConnection(sURL, FLAG_ICC_FORCE_CONNECTION, 0))
                return true;
             else
                return false;
          }
          else
             return false;
        }

        #endregion

        protected struct EpgImportResult
        {
            public DateTime SubscriptionExpiration;
            public int ChannelsProcessed;
            public List<string> Messages;
            public bool UpdateRequired;
            public DateTime StartOfUpdateRange;
            public DateTime EndOfUpdateRange;
        }
    }
}
