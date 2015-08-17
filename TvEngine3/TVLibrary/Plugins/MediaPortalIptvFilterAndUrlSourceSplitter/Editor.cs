#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TvDatabase;
using TvLibrary.Log;
using MediaPortal.Playlists;
using System.IO;
using System.Xml;
using TvLibrary.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using DirectShowLib;
using TvLibrary.Implementations.DVB;
using TvLibrary;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Filter;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url;
using Gentle.Framework;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections;

#endregion

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    public partial class Editor : SetupTv.SectionSettings
    {
        #region Private fields

        Settings settings = null;
        private int urlColumnLength = 0;

        BackgroundWorker testWorker = null;
        int testPlaylistIndex = -1;
        int testDatabaseIndex = -1;

        FilterUrlCollection playlistUrls = new FilterUrlCollection();
        FilterUrlCollection databaseUrls = new FilterUrlCollection();

        #endregion

        #region Constructors

        public Editor()
        {
            InitializeComponent();

            try
            {
                this.urlColumnLength = this.GetLengthOfUrlColumn(this.GetProviderTypeFromConfig());

                Log.Info(String.Format("URL column length: {0}", this.urlColumnLength));

                this.settings = Settings.Load();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error occurred while loading settings.\n{0}", ex.ToString()));

                this.settings = new Settings();
            }

            this.tabProtocols.TabPages.Clear();

            this.tabProtocols.TabPages.Add(tabPagePlaylistEditor);
            this.tabProtocols.TabPages.Add(tabPageDatabaseEditor);

            this.tabProtocols.TabPages.Add(tabPageHttp);
            this.tabProtocols.TabPages.Add(tabPageRtmp);
            this.tabProtocols.TabPages.Add(tabPageRtsp);
            this.tabProtocols.TabPages.Add(tabPageUdpRtp);

            this.tabProtocols.TabPages.Add(tabPageSettings);

            System.Net.NetworkInformation.NetworkInterface[] networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            this.comboBoxHttpPreferredNetworkInterface.Items.Add(ProtocolSettings.NetworkInterfaceSystemDefault);
            this.comboBoxRtmpPreferredNetworkInterface.Items.Add(ProtocolSettings.NetworkInterfaceSystemDefault);
            this.comboBoxRtspPreferredNetworkInterface.Items.Add(ProtocolSettings.NetworkInterfaceSystemDefault);
            this.comboBoxUdpRtpPreferredNetworkInterface.Items.Add(ProtocolSettings.NetworkInterfaceSystemDefault);

            foreach (var networkInterface in networkInterfaces)
            {
                this.comboBoxHttpPreferredNetworkInterface.Items.Add(networkInterface.Name);
                this.comboBoxRtmpPreferredNetworkInterface.Items.Add(networkInterface.Name);
                this.comboBoxRtspPreferredNetworkInterface.Items.Add(networkInterface.Name);
                this.comboBoxUdpRtpPreferredNetworkInterface.Items.Add(networkInterface.Name);
            }

            this.comboBoxHttpPreferredNetworkInterface.SelectedIndex = 0;
            this.comboBoxRtmpPreferredNetworkInterface.SelectedIndex = 0;
            this.comboBoxRtspPreferredNetworkInterface.SelectedIndex = 0;
            this.comboBoxUdpRtpPreferredNetworkInterface.SelectedIndex = 0;

            for (int i = 0; i < comboBoxHttpPreferredNetworkInterface.Items.Count; i++)
            {
                String nic = (String)comboBoxHttpPreferredNetworkInterface.Items[i];

                if (nic == this.settings.Http.NetworkInterface)
                {
                    this.comboBoxHttpPreferredNetworkInterface.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < comboBoxRtmpPreferredNetworkInterface.Items.Count; i++)
            {
                String nic = (String)comboBoxRtmpPreferredNetworkInterface.Items[i];

                if (nic == this.settings.Rtmp.NetworkInterface)
                {
                    comboBoxRtmpPreferredNetworkInterface.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < comboBoxRtspPreferredNetworkInterface.Items.Count; i++)
            {
                String nic = (String)comboBoxRtspPreferredNetworkInterface.Items[i];

                if (nic == this.settings.Rtsp.NetworkInterface)
                {
                    comboBoxRtspPreferredNetworkInterface.SelectedIndex = i;
                    break;
                }
            }

            for (int i = 0; i < comboBoxUdpRtpPreferredNetworkInterface.Items.Count; i++)
            {
                String nic = (String)comboBoxUdpRtpPreferredNetworkInterface.Items[i];

                if (nic == this.settings.UdpRtp.NetworkInterface)
                {
                    comboBoxUdpRtpPreferredNetworkInterface.SelectedIndex = i;
                    break;
                }
            }

            this.textBoxHttpOpenConnectionTimeout.Text = this.settings.Http.OpenConnectionTimeout.ToString();
            this.textBoxHttpOpenConnectionSleepTime.Text = this.settings.Http.OpenConnectionSleepTime.ToString();
            this.textBoxHttpTotalReopenConnectionTimeout.Text = this.settings.Http.TotalReopenConnectionTimeout.ToString();

            this.textBoxRtmpOpenConnectionTimeout.Text = this.settings.Rtmp.OpenConnectionTimeout.ToString();
            this.textBoxRtmpOpenConnectionSleepTime.Text = this.settings.Rtmp.OpenConnectionSleepTime.ToString();
            this.textBoxRtmpTotalReopenConnectionTimeout.Text = this.settings.Rtmp.TotalReopenConnectionTimeout.ToString();

            this.textBoxRtspOpenConnectionTimeout.Text = this.settings.Rtsp.OpenConnectionTimeout.ToString();
            this.textBoxRtspOpenConnectionSleepTime.Text = this.settings.Rtsp.OpenConnectionSleepTime.ToString();
            this.textBoxRtspTotalReopenConnectionTimeout.Text = this.settings.Rtsp.TotalReopenConnectionTimeout.ToString();
            this.textBoxRtspClientPortMin.Text = this.settings.Rtsp.ClientPortMin.ToString();
            this.textBoxRtspClientPortMax.Text = this.settings.Rtsp.ClientPortMax.ToString();

            this.rtspConnectionPreference.MulticastPreference = this.settings.Rtsp.MulticastPreference;
            this.rtspConnectionPreference.UdpPreference = this.settings.Rtsp.UdpPreference;
            this.rtspConnectionPreference.SameConnectionPreference = this.settings.Rtsp.SameConnectionPreference;
            this.checkBoxRtspIgnoreRtpPayloadType.Checked = settings.Rtsp.IgnoreRtpPayloadType;
            
            this.textBoxUdpRtpOpenConnectionTimeout.Text = this.settings.UdpRtp.OpenConnectionTimeout.ToString();
            this.textBoxUdpRtpOpenConnectionSleepTime.Text = this.settings.UdpRtp.OpenConnectionSleepTime.ToString();
            this.textBoxUdpRtpTotalReopenConnectionTimeout.Text = this.settings.UdpRtp.TotalReopenConnectionTimeout.ToString();
            this.textBoxUdpRtpReceiveDataCheckInterval.Text = this.settings.UdpRtp.ReceiveDataCheckInterval.ToString();

            this.checkBoxStreamAnalysis.Checked = this.settings.StreamAnalysis;
            this.textBoxStreamAnalysisTimeout.Text = this.settings.StreamAnalysisTimeout.ToString();

            this.tabProtocols_SelectedIndexChanged(this, new EventArgs());

            try
            {
                foreach (var tuningDetail in this.GetTuningDetails())
                {
                    this.databaseUrls.Add(new FilterUrl()
                    {
                        ChannelName = tuningDetail.Name,
                        Url = UrlFactory.CreateUrl(tuningDetail.Url),
                        PlaylistUrl = String.Empty,
                        TransportStreamId = tuningDetail.TransportId,
                        ProgramNumber = tuningDetail.ServiceId,
                        ProgramMapPID = tuningDetail.PmtPid,
                        Detail = tuningDetail
                    });
                }

                this.ApplyDefaultUserSettings(this.settings, this.settings);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error occurred while loading tuning details from database.\n{0}", ex.ToString()));

                this.databaseUrls.Clear();
            }

            this.UpdateGridViewDatabase();

            this.propertyGridPlaylist_PropertyValueChanged(this, null);
            this.propertyGridDatabase_PropertyValueChanged(this, null);
        }

        #endregion

        #region Methods

        private void UpdateGridViewDatabase()
        {
            for (int i = 0; i < this.databaseUrls.Count; i++)
			{
                FilterUrl filterUrl = this.databaseUrls[i];

                if (i < this.dataGridViewDatabase.Rows.Count)
                {
                    this.UpdateGridViewDatabase(i);
                }
                else
                {
                    this.dataGridViewDatabase.Rows.Add(new Object[] { filterUrl.ChannelName, filterUrl.Url, (String)Settings.SupportedProtocols[filterUrl.Url.Uri.Scheme.ToUpperInvariant()], filterUrl.TransportStreamId, filterUrl.ProgramNumber, filterUrl.ProgramMapPID });
                }
			}
        }

        private void UpdateGridViewDatabase(int row)
        {
            FilterUrl filterUrl = this.databaseUrls[row];
            DataGridViewRow dataRow = this.dataGridViewDatabase.Rows[row];

            dataRow.Cells[0].Value = filterUrl.ChannelName;
            dataRow.Cells[1].Value = filterUrl.Url;
            dataRow.Cells[2].Value = (String)Settings.SupportedProtocols[filterUrl.Url.Uri.Scheme.ToUpperInvariant()];
            dataRow.Cells[3].Value = filterUrl.TransportStreamId;
            dataRow.Cells[4].Value = filterUrl.ProgramNumber;
            dataRow.Cells[5].Value = filterUrl.ProgramMapPID;

            switch (filterUrl.State)
            {
                case FilterUrlState.NotTested:
                    dataRow.Cells[6].Value = Properties.Resources.not_tested;
                    dataRow.Cells[6].ToolTipText = String.Empty;
                    break;
                case FilterUrlState.Pending:
                    dataRow.Cells[6].Value = Properties.Resources.pending;
                    dataRow.Cells[6].ToolTipText = "Testing filter url ...";
                    break;
                case FilterUrlState.Failed:
                    dataRow.Cells[6].Value = Properties.Resources.failed;
                    dataRow.Cells[6].ToolTipText = filterUrl.Error;
                    break;
                case FilterUrlState.Correct:
                    dataRow.Cells[6].Value = Properties.Resources.correct;
                    dataRow.Cells[6].ToolTipText = String.Empty;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void UpdateGridViewPlaylist()
        {
            for (int i = 0; i < this.playlistUrls.Count; i++)
            {
                FilterUrl filterUrl = this.playlistUrls[i];

                if (i < this.dataGridViewPlaylist.Rows.Count)
                {
                    this.UpdateGridViewPlaylist(i);
                }
                else
                {
                    this.dataGridViewPlaylist.Rows.Add(new Object[] { filterUrl.ChannelName, filterUrl.PlaylistUrl, (String)Settings.SupportedProtocols[new Uri(filterUrl.PlaylistUrl).Scheme.ToUpperInvariant()], filterUrl.Url });
                }
            }
        }

        private void UpdateGridViewPlaylist(int row)
        {
            FilterUrl filterUrl = this.playlistUrls[row];
            DataGridViewRow dataRow = this.dataGridViewPlaylist.Rows[row];

            dataRow.Cells[0].Value = filterUrl.ChannelName;
            dataRow.Cells[1].Value = filterUrl.PlaylistUrl;
            dataRow.Cells[2].Value = (String)Settings.SupportedProtocols[new Uri(filterUrl.PlaylistUrl).Scheme.ToUpperInvariant()];
            dataRow.Cells[3].Value = filterUrl.Url;

            switch (filterUrl.State)
            {
                case FilterUrlState.NotTested:
                    dataRow.Cells[4].Value = Properties.Resources.not_tested;
                    dataRow.Cells[4].ToolTipText = String.Empty;
                    break;
                case FilterUrlState.Pending:
                    dataRow.Cells[4].Value = Properties.Resources.pending;
                    dataRow.Cells[4].ToolTipText = "Testing filter url ...";
                    break;
                case FilterUrlState.Failed:
                    dataRow.Cells[4].Value = Properties.Resources.failed;
                    dataRow.Cells[4].ToolTipText = filterUrl.Error;
                    break;
                case FilterUrlState.Correct:
                    dataRow.Cells[4].Value = Properties.Resources.correct;
                    dataRow.Cells[4].ToolTipText = String.Empty;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void SaveSettings()
        {
            try
            {
                Settings.Save(this.settings);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error occurred while saving settings.\n{0}", ex.ToString()));
            }

            base.SaveSettings();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            bool error = false;
            Settings currentSettings = new Settings();

            // HTTP
            currentSettings.Http.NetworkInterface = (String)this.comboBoxHttpPreferredNetworkInterface.SelectedItem;

            try
            {
                currentSettings.Http.OpenConnectionTimeout = int.Parse(this.textBoxHttpOpenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxHttpOpenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxHttpOpenConnectionTimeout, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Http.OpenConnectionSleepTime = int.Parse(this.textBoxHttpOpenConnectionSleepTime.Text);
                this.errorProvider.SetError(this.textBoxHttpOpenConnectionSleepTime, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxHttpOpenConnectionSleepTime, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Http.TotalReopenConnectionTimeout = int.Parse(this.textBoxHttpTotalReopenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxHttpTotalReopenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxHttpTotalReopenConnectionTimeout, ex.Message);
                error = true;
            }

            // RTMP
            currentSettings.Rtmp.NetworkInterface = (String)this.comboBoxRtmpPreferredNetworkInterface.SelectedItem;

            try
            {
                currentSettings.Rtmp.OpenConnectionTimeout = int.Parse(this.textBoxRtmpOpenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxRtmpOpenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtmpOpenConnectionTimeout, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Rtmp.OpenConnectionSleepTime = int.Parse(this.textBoxRtmpOpenConnectionSleepTime.Text);
                this.errorProvider.SetError(this.textBoxRtmpOpenConnectionSleepTime, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtmpOpenConnectionSleepTime, ex.Message);
                error = true;
            }


            try
            {
                currentSettings.Rtmp.TotalReopenConnectionTimeout = int.Parse(this.textBoxRtmpTotalReopenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxRtmpTotalReopenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtmpTotalReopenConnectionTimeout, ex.Message);
                error = true;
            }

            // RTSP
            currentSettings.Rtsp.NetworkInterface = (String)this.comboBoxRtspPreferredNetworkInterface.SelectedItem;

            try
            {
                currentSettings.Rtsp.OpenConnectionTimeout = int.Parse(this.textBoxRtspOpenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxRtspOpenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtspOpenConnectionTimeout, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Rtsp.OpenConnectionSleepTime = int.Parse(this.textBoxRtspOpenConnectionSleepTime.Text);
                this.errorProvider.SetError(this.textBoxRtspOpenConnectionSleepTime, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtspOpenConnectionSleepTime, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Rtsp.TotalReopenConnectionTimeout = int.Parse(this.textBoxRtspTotalReopenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxRtspTotalReopenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtspTotalReopenConnectionTimeout, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Rtsp.ClientPortMin = int.Parse(this.textBoxRtspClientPortMin.Text);
                this.errorProvider.SetError(this.textBoxRtspClientPortMin, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtspClientPortMin, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.Rtsp.ClientPortMax = int.Parse(this.textBoxRtspClientPortMax.Text);
                this.errorProvider.SetError(this.textBoxRtspClientPortMax, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxRtspClientPortMax, ex.Message);
                error = true;
            }


            currentSettings.Rtsp.UdpPreference = this.rtspConnectionPreference.UdpPreference;
            currentSettings.Rtsp.MulticastPreference = this.rtspConnectionPreference.MulticastPreference;
            currentSettings.Rtsp.SameConnectionPreference = this.rtspConnectionPreference.SameConnectionPreference;
            currentSettings.Rtsp.IgnoreRtpPayloadType = this.checkBoxRtspIgnoreRtpPayloadType.Checked;

            // UDP / RTP
            currentSettings.UdpRtp.NetworkInterface = (String)this.comboBoxUdpRtpPreferredNetworkInterface.SelectedItem;

            try
            {
                currentSettings.UdpRtp.OpenConnectionTimeout = int.Parse(this.textBoxUdpRtpOpenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxUdpRtpOpenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxUdpRtpOpenConnectionTimeout, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.UdpRtp.OpenConnectionSleepTime = int.Parse(this.textBoxUdpRtpOpenConnectionSleepTime.Text);
                this.errorProvider.SetError(this.textBoxUdpRtpOpenConnectionSleepTime, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxUdpRtpOpenConnectionSleepTime, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.UdpRtp.TotalReopenConnectionTimeout = int.Parse(this.textBoxUdpRtpTotalReopenConnectionTimeout.Text);
                this.errorProvider.SetError(this.textBoxUdpRtpTotalReopenConnectionTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxUdpRtpTotalReopenConnectionTimeout, ex.Message);
                error = true;
            }

            try
            {
                currentSettings.UdpRtp.ReceiveDataCheckInterval = int.Parse(this.textBoxUdpRtpReceiveDataCheckInterval.Text);
                this.errorProvider.SetError(this.textBoxUdpRtpReceiveDataCheckInterval, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxUdpRtpReceiveDataCheckInterval, ex.Message);
                error = true;
            }

            currentSettings.StreamAnalysis = this.checkBoxStreamAnalysis.Checked;

            try
            {
                currentSettings.StreamAnalysisTimeout = int.Parse(this.textBoxStreamAnalysisTimeout.Text);
                this.errorProvider.SetError(this.textBoxStreamAnalysisTimeout, "");
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxStreamAnalysisTimeout, ex.Message);
                error = true;
            }

            if (!error)
            {
                this.ApplyDefaultUserSettings(this.settings, currentSettings);
                this.settings = currentSettings;

                this.UpdateGridViewDatabase();
                this.UpdateGridViewPlaylist();

                this.propertyGridPlaylist_PropertyValueChanged(this, null);
                this.propertyGridDatabase_PropertyValueChanged(this, null);

                this.propertyGridPlaylist.Refresh();
                this.propertyGridDatabase.Refresh();
            }
        }

        private void buttonLoadPlaylist_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Playlist files|*.m3u|All files|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.InitialDirectory = Path.Combine(TvLibrary.Interfaces.PathManager.GetDataPath, "TuningParameters\\dvbip");

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.dataGridViewPlaylist.Rows.Clear();
                    this.playlistUrls.Clear();

                    PlayList playlist = new PlayList();
                    PlayListM3uIO playlistFile = (PlayListM3uIO)PlayListFactory.CreateIO(openFileDialog.FileName);
                    playlistFile.Load(playlist, openFileDialog.FileName);

                    foreach (var playlistItem in playlist)
                    {
                        try
                        {
                            String url = playlistItem.FileName.Substring(playlistItem.FileName.LastIndexOf('\\') + 1);

                            this.playlistUrls.Add(new FilterUrl()
                            {
                                ChannelName = playlistItem.Description,
                                PlaylistUrl = url,
                                Url = UrlFactory.CreateUrl(url)
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Error(String.Format("Error occurred while loading playlist, playlist item: {1}.\n{0}", ex.ToString(), playlistItem.FileName));
                        }
                    }

                    this.ApplyDefaultUserSettings(this.settings, this.settings);
                    this.UpdateGridViewPlaylist();

                    this.propertyGridPlaylist_PropertyValueChanged(this, null);
                    this.propertyGridDatabase_PropertyValueChanged(this, null);

                    if (this.dataGridViewPlaylist.RowCount != 0)
                    {
                        this.dataGridView_CellClick(this, new DataGridViewCellEventArgs(0, 0));
                    }
                }
            }
        }

        private void buttonSavePlaylist_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Playlist files|*.m3u|All files|*.*";
                saveFileDialog.FilterIndex = 0;
                saveFileDialog.InitialDirectory = Path.Combine(TvLibrary.Interfaces.PathManager.GetDataPath, "TuningParameters\\dvbip");
                saveFileDialog.AddExtension = true;
                saveFileDialog.OverwritePrompt = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PlayList playlist = new PlayList();
                    PlayListM3uIO playlistFile = new PlayListM3uIO();

                    foreach (var filterUrl in this.playlistUrls)
                    {
                        playlist.Add(new PlayListItem(filterUrl.ChannelName, filterUrl.Url.ToString()));
                    }

                    playlistFile.Save(playlist, saveFileDialog.FileName);
                }
            }
        }

        private void buttonUpdateDatabase_Click(object sender, EventArgs e)
        {
            this.UpdateLengthOfUrlColumn(this.GetProviderTypeFromConfig());

            this.urlColumnLength = this.GetLengthOfUrlColumn(this.GetProviderTypeFromConfig());

            IList<TuningDetail> tuningDetails = this.GetTuningDetails();
            for (int i = 0; i < this.databaseUrls.Count; i++)
            {
                FilterUrl filterUrl = this.databaseUrls[i];

                filterUrl.Detail = tuningDetails[i];
            }

            this.propertyGridPlaylist_PropertyValueChanged(this, null);
            this.propertyGridDatabase_PropertyValueChanged(this, null);
        }

        private void tabProtocols_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabProtocols.SelectedIndex == 0)
            {
                this.buttonLoadPlaylist.Visible = true;
                this.buttonSavePlaylist.Visible = true;
            }
            else
            {
                this.buttonLoadPlaylist.Visible = false;
                this.buttonSavePlaylist.Visible = false;
            }

            if (this.tabProtocols.SelectedIndex == 1)
            {
                this.buttonStoreChanges.Visible = true;
                this.buttonSetMpeg2TSParser.Visible = true;
            }
            else
            {
                this.buttonStoreChanges.Visible = false;
                this.buttonSetMpeg2TSParser.Visible = false;
            }
        }

        private void dataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.propertyGridPlaylist.SelectedObject = this.playlistUrls[e.RowIndex].Url;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            Object[] objects = new Object[this.dataGridViewPlaylist.SelectedRows.Count];

            for (int i = 0; i < this.dataGridViewPlaylist.SelectedRows.Count; i++)
            {
                DataGridViewRow row = this.dataGridViewPlaylist.SelectedRows[i];

                objects[i] = this.playlistUrls[row.Index].Url;
            }

            this.propertyGridPlaylist.SelectedObjects = objects;
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                // test

                if ((this.testWorker != null) && (this.testWorker.IsFinished))
                {
                    this.testWorker.Dispose();
                    this.testWorker = null;
                    this.testPlaylistIndex = -1;
                    this.testDatabaseIndex = -1;
                }

                if (this.testWorker == null)
                {
                    this.testPlaylistIndex = e.RowIndex;
                    this.playlistUrls[this.testPlaylistIndex].State =  FilterUrlState.Pending;

                    this.UpdateGridViewPlaylist(this.testPlaylistIndex);
                    this.dataGridViewPlaylist.Refresh();

                    this.testWorker = new BackgroundWorker();

                    this.testWorker.DoWork += new EventHandler<DoWorkEventArgs>(testWorker_DoWork);
                    this.testWorker.FinishedWork += new EventHandler<FinishedWorkEventArgs>(testWorker_FinishedWork);
                    this.testWorker.StartOperation();
                }
            }
        }

        void testWorker_FinishedWork(object sender, FinishedWorkEventArgs e)
        {
            if (e.IsError)
            {
                if (this.testPlaylistIndex != (-1))
                {
                    this.playlistUrls[this.testPlaylistIndex].State = FilterUrlState.Failed;
                    this.playlistUrls[this.testPlaylistIndex].Error = e.Exception.Message;

                    this.UpdateGridViewPlaylist(this.testPlaylistIndex);
                    this.dataGridViewPlaylist.Refresh();
                }

                if (this.testDatabaseIndex != (-1))
                {
                    this.databaseUrls[this.testDatabaseIndex].State = FilterUrlState.Failed;
                    this.databaseUrls[this.testDatabaseIndex].Error = e.Exception.Message;

                    this.UpdateGridViewDatabase(this.testDatabaseIndex);
                    this.dataGridViewDatabase.Refresh();
                }
            }
            else
            {
                if (this.testPlaylistIndex != (-1))
                {
                    this.playlistUrls[this.testPlaylistIndex].State = FilterUrlState.Correct;
                    this.playlistUrls[this.testPlaylistIndex].Error = String.Empty;

                    this.UpdateGridViewPlaylist(this.testPlaylistIndex);
                    this.dataGridViewPlaylist.Refresh();
                }

                if (this.testDatabaseIndex != (-1))
                {
                    this.databaseUrls[this.testDatabaseIndex].State = FilterUrlState.Correct;
                    this.databaseUrls[this.testDatabaseIndex].Error = String.Empty;

                    this.UpdateGridViewDatabase(this.testDatabaseIndex);
                    this.dataGridViewDatabase.Refresh();
                }
            }

            this.testPlaylistIndex = -1;
            this.testDatabaseIndex = -1;
        }

        void testWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IGraphBuilder graphBuilder = null;
            IBaseFilter sourceFilter = null;
            
            try
            {
                //System.Diagnostics.Debugger.Launch();
                graphBuilder = (IGraphBuilder)new FilterGraph();
                // add the source filter
                sourceFilter = FilterGraphTools.AddFilterFromClsid(graphBuilder, typeof(TvLibrary.Implementations.DVB.TvCardDVBIPBuiltIn.MPIPTVSource).GUID, "MediaPortal IPTV Source Filter");

                if (sourceFilter == null)
                {
                    throw new InvalidOperationException("No MediaPortal IPTV source filter is loaded.");
                }

                IFileSourceFilter fileSource = sourceFilter as IFileSourceFilter;
                IFilterStateEx filterStateEx = sourceFilter as IFilterStateEx;

                if (filterStateEx == null)
                {
                    throw new InvalidOperationException("No MediaPortal IPTV filter and url source splitter is loaded.");
                }

                int result = int.MinValue;
                FilterUrl filterUrl = null;

                if (this.testPlaylistIndex != (-1))
                {
                    filterUrl = this.playlistUrls[this.testPlaylistIndex];
                }

                if (this.testDatabaseIndex != (-1))
                {
                    filterUrl = this.databaseUrls[this.testDatabaseIndex];
                }

                filterUrl.Url.Mpeg2TsParser.Sections.Clear();

                filterUrl.Url.Mpeg2TsParser.StreamAnalysis = this.settings.StreamAnalysis;
                result = fileSource.Load(filterUrl.Url.ToString(), null);
                filterUrl.Url.Mpeg2TsParser.StreamAnalysis = false;

                if (result < 0)
                {
                    throw new FilterException(FilterError.ErrorDescription(filterStateEx, result));
                }
                
                result = sourceFilter.Run(0);

                if (result < 0)
                {
                    throw new FilterException(FilterError.ErrorDescription(filterStateEx, result));
                }

                Boolean compatible = false;
                result = filterStateEx.IsStreamIptvCompatible(out compatible);

                if (result < 0)
                {
                    throw new FilterException(FilterError.ErrorDescription(filterStateEx, result));
            }

                if (!compatible)
                {
                    throw new FilterException("The received stream is not IPTV compatible.");
                }

                if (this.settings.StreamAnalysis)
                {
                    DateTime start = DateTime.Now;
                    DateTime stop = start.AddMilliseconds(this.settings.StreamAnalysisTimeout);

                    while (DateTime.Now < stop)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    
                    uint iptvSectionCount = 0;
                    result = filterStateEx.GetIptvSectionCount(out iptvSectionCount);

                    if (result < 0)
                    {
                        throw new FilterException(FilterError.ErrorDescription(filterStateEx, result));
                    }

                    for (uint i = 0; i < iptvSectionCount; i++)
                    {
                        String sectionDataEncoded = null;

                        result = filterStateEx.GetIptvSection(i, out sectionDataEncoded);

                        if (result < 0)
                        {
                            throw new FilterException(FilterError.ErrorDescription(filterStateEx, result));
                        }

                        Byte[] sectionData = Convert.FromBase64String(sectionDataEncoded);

                        Section section = SectionFactory.CreateSection(sectionData);

                        if (section != null)
                        {
                            if (section is ProgramAssociationSection)
                            {
                                ProgramAssociationSection streamSection = section as ProgramAssociationSection;

                                filterUrl.Url.Mpeg2TsParser.Sections.Add(new StreamSection(streamSection));
                            }
                            else if (section is TransportStreamProgramMapSection)
                            {
                                TransportStreamProgramMapSection streamSection = section as TransportStreamProgramMapSection;

                                bool continueWithNextSection = false;
                                for (int j = 0; ((!continueWithNextSection) && (j < filterUrl.Url.Mpeg2TsParser.Sections.Count)); j++)
                                {
                                    ProgramAssociationSection filterSection = filterUrl.Url.Mpeg2TsParser.Sections[j].Section as ProgramAssociationSection;

                                    if (filterSection != null)
                                    {
                                        for (int k = 0; k < filterSection.Programs.Count; k++)
                                        {
                                            ProgramAssociationSectionProgram program = filterSection.Programs[k];

                                            if (program.ProgramNumber == streamSection.ProgramNumber)
                                            {
                                                continueWithNextSection = true;

                                                streamSection.ProgramMapPID = program.ProgramMapPID;
                                                filterUrl.Url.Mpeg2TsParser.Sections[j].StreamSections.Add(new StreamSection(streamSection));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                filterUrl.Url.Mpeg2TsParser.Sections.Add(new StreamSection(section));
                            }
                        }
                    }
                }
            }
            finally
            {
                e.IsFinished = true;

                if ((graphBuilder != null) && (sourceFilter != null))
                {
                    graphBuilder.RemoveFilter(sourceFilter);
                    Release.ComObject("MediaPortal IPTV Source Filter", sourceFilter);
                    sourceFilter = null;
                }
            }
        }

        private ProviderType GetProviderTypeFromConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(PathManager.GetDataPath, "gentle.config"));
            XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
            XmlNode serverName = nodeKey.Attributes.GetNamedItem("name");
            String serverType = serverName.InnerText.ToLowerInvariant();

            switch (serverType)
            {
                case "mysql":
                    return ProviderType.MySql;
                case "sqlserver":
                    return ProviderType.SqlServer;
                default:
                    throw new NotImplementedException();
            }
        }

        private String GetConnectionStringFromConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(PathManager.GetDataPath, "gentle.config"));
            XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");

            return nodeKey.Attributes.GetNamedItem("connectionString").InnerText;
        }

        private int GetLengthOfUrlColumn(ProviderType provider)
        {
            switch (provider)
            {
                case ProviderType.SqlServer:
                    {
                        using (SqlConnection connection = new SqlConnection(this.GetConnectionStringFromConfig()))
                        {
                            connection.Open();

                            using (SqlCommand cmd = new SqlCommand("SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE (TABLE_NAME = 'TuningDetail') AND (COLUMN_NAME = 'url')", connection))
                            {
                                using (IDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        return (int)reader["CHARACTER_MAXIMUM_LENGTH"];
                                    }

                                    return 0;
                                }
                            }
                        }
                    }
                case ProviderType.MySql:
                    {
                        using (MySqlConnection connection = new MySqlConnection(this.GetConnectionStringFromConfig()))
                        {
                            connection.Open();

                            using (MySqlCommand cmd = new MySqlCommand("SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE (TABLE_NAME = 'TuningDetail') AND (COLUMN_NAME = 'url')", connection))
                            {
                                using (IDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        return (int)(UInt64)reader["CHARACTER_MAXIMUM_LENGTH"];
                                    }

                                    return 0;
                                }
                            }
                        }
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private void UpdateLengthOfUrlColumn(ProviderType provider)
        {
            switch (provider)
            {
                case ProviderType.SqlServer:
                    {
                        using (SqlConnection connection = new SqlConnection(this.GetConnectionStringFromConfig()))
                        {
                            connection.Open();

                            String urlColumnAlter = String.Format("ALTER TABLE [TuningDetail] ALTER COLUMN [url] VARCHAR({0}) NOT NULL", Editor.DefaultUrlColumnLength);

                            using (SqlCommand cmd = new SqlCommand(urlColumnAlter, connection))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    break;
                case ProviderType.MySql:
                    {
                        using (MySqlConnection connection = new MySqlConnection(this.GetConnectionStringFromConfig()))
                        {
                            connection.Open();

                            String urlColumnAlter = String.Format("ALTER TABLE TuningDetail MODIFY COLUMN url VARCHAR({0}) NOT NULL", Editor.DefaultUrlColumnLength);
                            
                            using (MySqlCommand cmd = new MySqlCommand(urlColumnAlter, connection))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ApplyDefaultUserSettings(Settings previousSettings, Settings currentSettings)
        {
            foreach (var filterUrl in this.playlistUrls)
            {
                if (filterUrl.Url is AfhsManifestUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (filterUrl.Url is HttpUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (filterUrl.Url is RtmpUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Rtmp, currentSettings.Rtmp);
                }

                if (filterUrl.Url is RtspUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Rtsp, currentSettings.Rtsp);
                }

                if (filterUrl.Url is UdpRtpUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.UdpRtp, currentSettings.UdpRtp);
                }
            }

            foreach (var filterUrl in this.databaseUrls)
            {
                if (filterUrl.Url is AfhsManifestUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (filterUrl.Url is HttpUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (filterUrl.Url is RtmpUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Rtmp, currentSettings.Rtmp);
                }

                if (filterUrl.Url is RtspUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.Rtsp, currentSettings.Rtsp);
                }

                if (filterUrl.Url is UdpRtpUrl)
                {
                    filterUrl.Url.ApplyDefaultUserSettings(previousSettings.UdpRtp, currentSettings.UdpRtp);
                }
            }
        }

        private void propertyGridPlaylist_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            bool urlLengthTooShort = false;

            foreach (var filterUrl in this.playlistUrls)
            {
                urlLengthTooShort |= (filterUrl.Url.ToString().Length > this.urlColumnLength);
            }

            this.buttonUpdateDatabase.Visible = urlLengthTooShort;

            this.UpdateGridViewPlaylist();
            this.dataGridViewPlaylist.Refresh();
        }

        private IList<TuningDetail> GetTuningDetails()
        {
            SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(TuningDetail));
            sb.AddConstraint(Operator.Equals, "channelType", 7);

            SqlStatement stmt = sb.GetStatement(true);
            IList<TuningDetail> details = ObjectFactory.GetCollection<TuningDetail>(stmt.Execute());
            return details;
        }

        private void dataGridViewDatabase_SelectionChanged(object sender, EventArgs e)
        {
            Object[] objects = new Object[this.dataGridViewDatabase.SelectedRows.Count];

            for (int i = 0; i < this.dataGridViewDatabase.SelectedRows.Count; i++)
            {
                DataGridViewRow row = this.dataGridViewDatabase.SelectedRows[i];

                objects[i] = this.databaseUrls[row.Index].Url;
            }

            this.propertyGridDatabase.SelectedObjects = objects;
        }

        private void dataGridViewDatabase_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.propertyGridDatabase.SelectedObject = this.databaseUrls[e.RowIndex].Url;
        }

        private void dataGridViewDatabase_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
            {
                // test

                if ((this.testWorker != null) && (this.testWorker.IsFinished))
                {
                    this.testWorker.Dispose();
                    this.testWorker = null;
                    this.testPlaylistIndex = -1;
                    this.testDatabaseIndex = -1;
                }

                if (this.testWorker == null)
                {
                    this.testDatabaseIndex = e.RowIndex;
                    this.databaseUrls[this.testDatabaseIndex].State = FilterUrlState.Pending;

                    this.UpdateGridViewDatabase(this.testDatabaseIndex);
                    this.dataGridViewDatabase.Refresh();

                    this.testWorker = new BackgroundWorker();

                    this.testWorker.DoWork += new EventHandler<DoWorkEventArgs>(testWorker_DoWork);
                    this.testWorker.FinishedWork += new EventHandler<FinishedWorkEventArgs>(testWorker_FinishedWork);
                    this.testWorker.StartOperation();
                }
            }
        }

        private void buttonStoreChanges_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < this.databaseUrls.Count; i++)
                {
                    FilterUrl filterUrl = this.databaseUrls[i];

                    filterUrl.Detail.Url = filterUrl.Url.ToString();
                }

                foreach (var filterUrl in this.databaseUrls)
                {
                    filterUrl.Detail.Persist();
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error occurred while storing changes to database.\n{0}", ex.ToString()));
            }
        }

        private void propertyGridDatabase_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            bool urlLengthTooShort = false;

            foreach (var filterUrl in this.databaseUrls)
            {
                urlLengthTooShort |= (filterUrl.Url.ToString().Length > this.urlColumnLength);
            }

            this.buttonUpdateDatabase.Visible = urlLengthTooShort;
            this.buttonStoreChanges.Enabled = !urlLengthTooShort;

            this.UpdateGridViewDatabase();
            this.dataGridViewDatabase.Refresh();
        }

        private void buttonSetMpeg2TSParser_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.dataGridViewDatabase.SelectedRows.Count; i++)
            {
                int j = this.dataGridViewDatabase.SelectedRows[i].Index;

                DataGridViewRow row = this.dataGridViewDatabase.Rows[j];
                FilterUrl filterUrl = this.databaseUrls[j];

                filterUrl.Url.Mpeg2TsParser.AlignToMpeg2TSPacket = true;
                filterUrl.Url.Mpeg2TsParser.DetectDiscontinuity = true;
                filterUrl.Url.Mpeg2TsParser.TransportStreamID = filterUrl.Detail.TransportId;
                filterUrl.Url.Mpeg2TsParser.ProgramNumber = filterUrl.Detail.ServiceId;
                filterUrl.Url.Mpeg2TsParser.ProgramMapPID = filterUrl.Detail.PmtPid;

                this.UpdateGridViewDatabase(j);
            }

            this.propertyGridDatabase_PropertyValueChanged(this, null);
        }

        // MySQL
        // bigint, SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE (TABLE_NAME = 'TuningDetail') AND (COLUMN_NAME = 'url')
        // ALTER TABLE TuningDetail MODIFY COLUMN url VARCHAR(200) NOT NULL

        // MSSQL
        // int, SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE (TABLE_NAME = 'TuningDetail') AND (COLUMN_NAME = 'url')
        // ALTER TABLE [TuningDetail] ALTER COLUMN [url] VARCHAR(200) NOT NULL

        #endregion

        #region Constants

        private static readonly int DefaultUrlColumnLength = 4096;

        #endregion
    }
}
