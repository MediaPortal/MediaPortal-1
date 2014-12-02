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

#endregion

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    public partial class Editor : SetupTv.SectionSettings
    {
        #region Private fields

        Settings settings = null;
        private int urlColumnLength = 0;

        BackgroundWorker testWorker = null;
        DataGridViewRow testPlaylistRow = null;
        DataGridViewRow testDatabaseRow = null;

        IList<TuningDetail> tuningDetails = null;

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

            this.tabProtocols_SelectedIndexChanged(this, new EventArgs());

            try
            {
                this.tuningDetails = this.GetTuningDetails();

                foreach (var tuningDetail in this.tuningDetails)
                {
                    this.dataGridViewDatabase.Rows.Add(new Object[] { tuningDetail.Name, UrlFactory.CreateUrl(tuningDetail.Url), (String)Settings.SupportedProtocols[new Uri(tuningDetail.Url).Scheme.ToUpperInvariant()], tuningDetail.TransportId, tuningDetail.ServiceId, tuningDetail.PmtPid });
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error occurred while loading tuning details from database.\n{0}", ex.ToString()));

                this.dataGridViewDatabase.Rows.Clear();
            }
        }

        #endregion

        #region Methods

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

            if (!error)
            {
                this.ApplyDefaultUserSettings(this.settings, currentSettings);

                this.settings = currentSettings;
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

                    PlayList playlist = new PlayList();
                    PlayListM3uIO playlistFile = (PlayListM3uIO)PlayListFactory.CreateIO(openFileDialog.FileName);
                    playlistFile.Load(playlist, openFileDialog.FileName);

                    foreach (var playlistItem in playlist)
                    {
                        try
                        {
                            String url = playlistItem.FileName.Substring(playlistItem.FileName.LastIndexOf('\\') + 1);

                            this.dataGridViewPlaylist.Rows.Add(new Object[] { playlistItem.Description, url, (String)Settings.SupportedProtocols[new Uri(url).Scheme.ToUpperInvariant()], UrlFactory.CreateUrl(url) });
                        }
                        catch (Exception ex)
                        {
                            Log.Error(String.Format("Error occurred while loading playlist, playlist item: {1}.\n{0}", ex.ToString(), playlistItem.FileName));
                        }
                    }

                    if (this.dataGridViewPlaylist.RowCount != 0)
                    {
                        this.ApplyDefaultUserSettings(this.settings, this.settings);
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

                    foreach (DataGridViewRow row in this.dataGridViewPlaylist.Rows)
                    {
                        SimpleUrl simpleUrl = (SimpleUrl)row.Cells[3].Value;

                        playlist.Add(new PlayListItem((String)row.Cells[0].Value, simpleUrl.ToString()));
                    }

                    playlistFile.Save(playlist, saveFileDialog.FileName);
                }
            }
        }

        private void buttonUpdateDatabase_Click(object sender, EventArgs e)
        {
            this.UpdateLengthOfUrlColumn(this.GetProviderTypeFromConfig());

            this.urlColumnLength = this.GetLengthOfUrlColumn(this.GetProviderTypeFromConfig());
            this.propertyGridPlaylist_PropertyValueChanged(this, null);
            this.propertyGridDatabase_PropertyValueChanged(this, null);

            this.tuningDetails = this.GetTuningDetails();
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
            this.propertyGridPlaylist.SelectedObject = this.dataGridViewPlaylist.Rows[e.RowIndex].Cells[3].Value;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            Object[] objects = new Object[this.dataGridViewPlaylist.SelectedRows.Count];

            for (int i = 0; i < this.dataGridViewPlaylist.SelectedRows.Count; i++)
			{
                DataGridViewRow row = this.dataGridViewPlaylist.SelectedRows[i];

                objects[i] = row.Cells[3].Value;
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
                    this.testPlaylistRow = null;
                    this.testDatabaseRow = null;
                }

                if (this.testWorker == null)
                {
                    this.testPlaylistRow = this.dataGridViewPlaylist.Rows[e.RowIndex];
                    this.testPlaylistRow.Cells[4].Value = Properties.Resources.pending;
                    this.testPlaylistRow.Cells[4].ToolTipText = "Testing filter url ...";

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
                if (this.testPlaylistRow != null)
                {
                    this.testPlaylistRow.Cells[4].Value = Properties.Resources.failed;
                    this.testPlaylistRow.Cells[4].ToolTipText = e.Exception.Message;
                }

                if (this.testDatabaseRow != null)
                {
                    this.testDatabaseRow.Cells[6].Value = Properties.Resources.failed;
                    this.testDatabaseRow.Cells[6].ToolTipText = e.Exception.Message;
                }
            }
            else
            {
                if (this.testPlaylistRow != null)
                {
                    this.testPlaylistRow.Cells[4].Value = Properties.Resources.correct;
                    this.testPlaylistRow.Cells[4].ToolTipText = String.Empty;
                }

                if (this.testDatabaseRow != null)
                {
                    this.testDatabaseRow.Cells[6].Value = Properties.Resources.correct;
                    this.testDatabaseRow.Cells[6].ToolTipText = String.Empty;
                }
            }
        }

        void testWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IGraphBuilder graphBuilder = null;
            IBaseFilter sourceFilter = null;
            
            try
            {
                graphBuilder = (IGraphBuilder)new FilterGraph();
                // add the source filter
                sourceFilter = FilterGraphTools.AddFilterFromClsid(graphBuilder, typeof(TvLibrary.Implementations.DVB.TvCardDVBIPBuiltIn.MPIPTVSource).GUID, "MediaPortal IPTV Source Filter");

                IFileSourceFilter fileSource = sourceFilter as IFileSourceFilter;
                IFilterStateEx filterStateEx = sourceFilter as IFilterStateEx;

                int result = int.MinValue;
                if (this.testPlaylistRow != null)
                {
                    result = fileSource.Load((String)this.testPlaylistRow.Cells[3].Value.ToString(), null);
                }

                if (this.testDatabaseRow != null)
                {
                    result = fileSource.Load((String)this.testDatabaseRow.Cells[1].Value.ToString(), null);
                }

                if (result < 0)
                {
                    throw new FilterException(FilterError.ErrorDescription(filterStateEx, result));
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
            foreach (DataGridViewRow row in this.dataGridViewPlaylist.Rows)
            {
                SimpleUrl simpleUrl = (SimpleUrl)row.Cells[3].Value;

                if (simpleUrl is AfhsManifestUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (simpleUrl is HttpUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (simpleUrl is RtmpUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Rtmp, currentSettings.Rtmp);
                }

                if (simpleUrl is RtspUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Rtsp, currentSettings.Rtsp);
                }

                if (simpleUrl is UdpRtpUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.UdpRtp, currentSettings.UdpRtp);
                }
            }

            foreach (DataGridViewRow row in this.dataGridViewDatabase.Rows)
            {
                SimpleUrl simpleUrl = (SimpleUrl)row.Cells[1].Value;

                if (simpleUrl is AfhsManifestUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (simpleUrl is HttpUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Http, currentSettings.Http);
                }

                if (simpleUrl is RtmpUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Rtmp, currentSettings.Rtmp);
                }

                if (simpleUrl is RtspUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.Rtsp, currentSettings.Rtsp);
                }

                if (simpleUrl is UdpRtpUrl)
                {
                    simpleUrl.ApplyDefaultUserSettings(previousSettings.UdpRtp, currentSettings.UdpRtp);
                }
            }

            this.propertyGridPlaylist_PropertyValueChanged(this, null);
            this.propertyGridDatabase_PropertyValueChanged(this, null);
        }

        private void propertyGridPlaylist_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            bool urlLengthTooShort = false;

            foreach (DataGridViewRow row in this.dataGridViewPlaylist.Rows)
            {
                SimpleUrl simpleUrl = (SimpleUrl)row.Cells[3].Value;

                urlLengthTooShort |= (simpleUrl.ToString().Length > this.urlColumnLength);
            }

            this.buttonUpdateDatabase.Visible = urlLengthTooShort;
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

                objects[i] = row.Cells[1].Value;
            }

            this.propertyGridDatabase.SelectedObjects = objects;
        }

        private void dataGridViewDatabase_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.propertyGridDatabase.SelectedObject = this.dataGridViewDatabase.Rows[e.RowIndex].Cells[1].Value;
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
                    this.testPlaylistRow = null;
                    this.testDatabaseRow = null;
                }

                if (this.testWorker == null)
                {
                    this.testDatabaseRow = this.dataGridViewDatabase.Rows[e.RowIndex];
                    this.testDatabaseRow.Cells[6].Value = Properties.Resources.pending;
                    this.testDatabaseRow.Cells[6].ToolTipText = "Testing filter url ...";

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
                for (int i = 0; i < this.dataGridViewDatabase.Rows.Count; i++)
                {
                    DataGridViewRow row = this.dataGridViewDatabase.Rows[i];
                    TuningDetail detail = this.tuningDetails[i];

                    SimpleUrl simpleUrl = (SimpleUrl)row.Cells[1].Value;
                    detail.Url = simpleUrl.ToString();
                }

                foreach (var tuningDetail in this.tuningDetails)
                {
                    tuningDetail.Persist();
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

            foreach (DataGridViewRow row in this.dataGridViewDatabase.Rows)
            {
                SimpleUrl simpleUrl = (SimpleUrl)row.Cells[1].Value;

                urlLengthTooShort |= (simpleUrl.ToString().Length > this.urlColumnLength);
            }

            this.buttonUpdateDatabase.Visible = urlLengthTooShort;
            if (urlLengthTooShort)
            {
                this.buttonStoreChanges.Enabled = false;
            }
        }

        private void buttonSetMpeg2TSParser_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.dataGridViewDatabase.SelectedRows.Count; i++)
            {
                DataGridViewRow row = this.dataGridViewDatabase.Rows[i];
                TuningDetail detail = this.tuningDetails[i];
                SimpleUrl simpleUrl = (SimpleUrl)row.Cells[1].Value;

                simpleUrl.Mpeg2TsParser.AlignToMpeg2TSPacket = true;
                simpleUrl.Mpeg2TsParser.DetectDiscontinuity = true;
                simpleUrl.Mpeg2TsParser.TransportStreamID = detail.TransportId;
                simpleUrl.Mpeg2TsParser.ProgramNumber = detail.ServiceId;
                simpleUrl.Mpeg2TsParser.ProgramMapPID = detail.PmtPid;
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
