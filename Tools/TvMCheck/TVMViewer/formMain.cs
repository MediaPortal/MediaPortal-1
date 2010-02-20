using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TvMCheck
{
    public partial class FormTvMCheck : Form
    {
        OleDbConnection _databaseConnection;
        ArrayList _tvmEpgChannels = new ArrayList();

        public FormTvMCheck()
        {
            InitializeComponent();

            LoadSettings();
        }

        private void LoadSettings()
        {
            textBoxDBPath.Text = TVMovieDatabasePath;
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            Stopwatch bench = new Stopwatch();
            bench.Start();
            Connect();
            bench.Stop();
            label1.Text = bench.ElapsedMilliseconds.ToString();
            lbChannels.Text = "Channels (" + _tvmEpgChannels.Count + ")";
        }

        private void Connect()
        {
            // http://www.microsoft.com/downloads/details.aspx?FamilyID=7554F536-8C28-4598-9B72-EF94E038C891&displaylang=en
            string dataProviderStringJet = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};", textBoxDBPath.Text);
            string dataProviderStringAce = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};", textBoxDBPath.Text);

            try
            {
                _databaseConnection = new OleDbConnection(dataProviderStringAce);
            }
            catch (Exception)
            {
                MessageBox.Show("Keinen ACE12 Provider gefunden (Office 2007). Probiere Jet4...");
            }

            try
            {
                _databaseConnection = new OleDbConnection(dataProviderStringJet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Keine Verbindung via JET OLEDB4 möglich: {0}", ex.Message));
                Clipboard.SetText(ex.Message + " \n " + ex.StackTrace);
                return;
            }

            string sqlSelect;

            if (cbFavorites.Checked)
                sqlSelect = "SELECT * FROM Sender WHERE (Favorit = true) AND (GueltigBis >=Now()) ORDER BY Bezeichnung ASC;";
            else
                sqlSelect = "SELECT * FROM Sender WHERE GueltigBis >=Now() ORDER BY Bezeichnung ASC;";

            DataSet tvMovieTable = new DataSet("Sender");
            try
            {
                if (_databaseConnection != null)
                {
                    if (_databaseConnection.State != ConnectionState.Open)
                        _databaseConnection.Open();
                    else
                    {
                        MessageBox.Show(string.Format("Verbindung war schon geöffnet! \nStatus: {0},\nProvider: {1},\nPfad: {2}", _databaseConnection.State.ToString(), _databaseConnection.Provider, _databaseConnection.DataSource));
                    }
                }
                else
                {
                    MessageBox.Show("Connection war NULL");
                    return;
                }

                using (OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection))
                {
                    using (OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand))
                    {
                        try
                        {
                            databaseAdapter.FillSchema(tvMovieTable, SchemaType.Source, "Sender");
                            databaseAdapter.Fill(tvMovieTable);
                        }
                        catch (Exception dsex)
                        {
                            string fehler = string.Format("Exception bei DataSet.Fill - {0}\nCmd.CommandText: {1}\nCmd.Connection: {2}\nConnection.State: {3}\nSource: {4}\nStack: {5}", dsex.Message, databaseCommand.CommandText, databaseCommand.Connection.ConnectionString, _databaseConnection.State.ToString(), dsex.Source, dsex.StackTrace);
                            MessageBox.Show(fehler);
                            Clipboard.SetText(fehler);
                            return;
                        }
                    }
                }
            }
            catch (System.Data.OleDb.OleDbException ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(ex.StackTrace);
                Clipboard.SetText(ex.Message + " \n " + ex.StackTrace);
                return;
            }
            finally
            {
                _databaseConnection.Close();
            }

            _tvmEpgChannels.Clear();
            treeViewStations.BeginUpdate();
            treeViewStations.Nodes.Clear();
            imageListSender.Images.Clear();
            treeViewStations.ImageList = imageListSender;

            int RowCount = tvMovieTable.Tables["Table"].Rows.Count;            
            for (int i = 0; i < RowCount; i++)
            {
                DataRow sender = tvMovieTable.Tables["Table"].Rows[i] as DataRow;

                _tvmEpgChannels.Add(sender["SenderKennung"]);
                string thumbFile = TVMovieProgramPath + @"Gifs\" + sender["Zeichen"].ToString();
                Bitmap bmp = new Bitmap(thumbFile);
                IntPtr HIcon = bmp.GetHicon();
                System.Drawing.Icon stationThumb = Icon.FromHandle(HIcon);

                imageListSender.Images.Add(new System.Drawing.Icon(stationThumb, new Size(32, 22)));


                //    TreeNode[] subitems = new TreeNode[]{new TreeNode("Sender " + (i +1).ToString())};
                // sender["SenderKennung"].ToString(),

                TreeNode stationNode = new TreeNode(sender["SenderKennung"].ToString(), i, i);
                treeViewStations.Nodes.Add(stationNode);
            }

            treeViewStations.EndUpdate();
        }

        private static string TVMovieDatabasePath
        {
            get
            {
                string path = @"C:\Program Files\TV Movie\TV Movie ClickFinder\tvdaten.mdb";
                try
                {
                    using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
                        if (rkey != null)
                            path = string.Format("{0}", rkey.GetValue("DBDatei"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("TVMovie: Error getting TV Movie DB dir (DBDatei) from registry {0}", ex.Message));
                }
                return path;
            }
        }

        private string TVMovieProgramPath
        {
            get
            {
                string path = string.Empty;
                try
                {
                    using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
                        if (rkey != null)
                            path = string.Format("{0}", rkey.GetValue("ProgrammPath"));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("TVMovie: Error getting TV Movie install dir (ProgrammPath) from registry {0}", ex.Message));
                }
                return path;
            }
        }

        private void treeViewStations_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeViewPrograms.Nodes.Clear();
            treeViewPrograms.BeginUpdate();
            DateTime startTime = DateTime.Now.Date; //Subtract(TimeSpan.FromHours(4));
            DateTime endTime = startTime + new TimeSpan(1, 0, 0, 0);
            string sqlSelectProgs = string.Format("SELECT * FROM TVDaten WHERE SenderKennung = \"{0}\" AND (TVDaten.Ende >= #{1}# AND TVDaten.Beginn <= #{2}#) ORDER BY Beginn ASC;", e.Node.Text, startTime.ToString("yyyy-MM-dd"), endTime.ToString("yyyy-MM-dd"));

            using (OleDbDataAdapter dbAdapter = new OleDbDataAdapter())
            {
                using (DataSet tvProgTable = new DataSet())
                {
                    try
                    {
                        _databaseConnection.Open();

                        using (OleDbCommand databaseCmd = new OleDbCommand(sqlSelectProgs, _databaseConnection))
                        {
                            dbAdapter.SelectCommand = databaseCmd;

                            dbAdapter.Fill(tvProgTable, "TVDaten");
                        }
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        MessageBox.Show(ex.Message);
                        MessageBox.Show(ex.StackTrace);
                        return;
                    }
                    finally
                    {
                        _databaseConnection.Close();
                    }

                    int RowCount = tvProgTable.Tables["TVDaten"].Rows.Count;
                    lbPrograms.Text = "Programs (" + RowCount.ToString() + ")";

                    for (int i = 0; i < RowCount; i++)
                    {
                        DataRow prog = tvProgTable.Tables["TVDaten"].Rows[i] as DataRow;

                        string genre = prog["Genre"].ToString();

                        //TreeNode[] subItems = new TreeNode[] { new TreeNode(prog["Beschreibung"].ToString()), new TreeNode(prog["KurzBeschreibung"].ToString()) };
                        TreeNode progNode = new TreeNode((Convert.ToDateTime(prog["Beginn"]).ToShortTimeString()) + " : " + prog["Sendung"].ToString() + " (" + genre + ")");// subItems);
                        treeViewPrograms.Nodes.Add(progNode);
                    }
                } // DataSet
            } // DbAdapter
            treeViewPrograms.EndUpdate();

        }

        private void cbFavorites_CheckedChanged(object sender, EventArgs e)
        {
            Connect();
        }
    }
}
